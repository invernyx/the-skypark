import _Vue from 'vue';
import { OS } from '@/sys/services/os'
import Contract, { RequestStatus } from "@/sys/classes/contracts/contract"
import Template from "@/sys/classes/contracts/template"
import Eljs from '@/sys/libraries/elem';
import Notification from '@/sys/models/notification';
import Flightplan from '@/sys/classes/flight_plans/plan';

export default class ContractService {
	private $os: OS;

	private workers_cumulative = 0;

	public startup() {
		this.$os.eventsBus.Bus.on('ws-in', (e) => this.broadcast_internal(e, this));
	}

	private report_cumulative() {
		//console.log('Cumulative contracts: ' + this.workers_cumulative);
	}

	private broadcast_internal(wsmsg: any, self: ContractService) {
		switch(wsmsg.name[0]){
			case 'adventure': {
				switch(wsmsg.name[1]){
					case 'remove': {
						self.$os.eventsBus.Bus.emit('contracts', { name: 'remove', payload: {
							id: wsmsg.payload.id
						}});
						break;
					}
					default: {
						self.$os.eventsBus.Bus.emit('contracts', { name: 'mutate', payload: {
							id: wsmsg.payload.id,
							contract: wsmsg.payload as Partial<Contract>
						}});
						break;
					}
				}
				break;
			}
		}
	}

	public event_list(actions :string[], target_id: number, mutation: Partial<Contract>, list: Contract[]) {

		switch(actions[0]) {
			case 'remove': {
				list.forEach((contract, index) => {
					if(contract.id == target_id) {
						list.splice(index, 1);
					}
				});
				break;
			}
			default: {
				list.forEach(contract => {
					this.event(actions, target_id, mutation, contract);
				});
				break;
			}
		}
	}

	public event(actions :string[], target_id: number, mutation: Partial<Contract>, contract: Contract) {

		// Replace flight plans
		switch(actions[0]) {
			case 'flightplans': {
				mutation.flight_plans.forEach((plan) => {
					const existing_plan_index = contract.flight_plans.findIndex(x => x.hash == plan.hash);

					switch(actions[1]) {
						case 'change':
						case 'rename': {
							if(existing_plan_index > -1) {
								contract.flight_plans[existing_plan_index] = plan;
							} else {
								contract.flight_plans.push(plan);
							}
							break;
						}
						case 'delete': {
							if(existing_plan_index > -1) {
								contract.flight_plans.splice(existing_plan_index, 1);
							}
							break;
						}
					}

				});
				break;
			}
		}

		if(contract && contract.id == target_id){
			switch(actions[0]) {
				default: {

					// If state changed, let's trigger a few things
					if(mutation.state !== undefined ? contract.state != mutation.state : false) {
						switch(mutation.state) {
							case 'Active':
							case 'Saved': {
								if(!contract.template.running_clock) {
									const n = new Date();
									n.setTime(n.getTime() + (contract.template.time_to_complete * 60 * 60 * 1000));
									contract.expire_at = n;
								}
								break;
							}
						}
					}

					// Merge the new data with existing datax
					Eljs.merge_deep(contract, mutation);
					break;
				}
				//case 'flightplan': {
				//	// Add flight plans
				//	contract.flightplans.push(wsmsg.payload.plan);
				//	break;
				//}
			}
		}

	}

	public set_status(target: Contract, status :RequestStatus) {
		this.$os.eventsBus.Bus.emit('contracts', { name: 'mutate', payload: {
			id: target.id,
			contract: {
				request_status: status
			} as Partial<Contract>
		}});
	}

	public interact(target: Contract, type: string, interaction_payload :any = null, callback :Function = null) {
		switch(type) {
			case 'manage': {
				this.$os.routing.goTo({ name: 'p42_conduit', params: { contract: target.id }});
				break;
			}
			case 'plan': {
				this.$os.routing.goTo({ name: 'p42_yoflight', params: { contract: target.id }});
				break;
			}
			case 'fly': {
				this.$os.routing.goTo({ name: 'p42_conduit', params: { contract: target.id }});
				break;
			}
			case 'interaction': {
				target.request_status = RequestStatus.Requesting;
				this.set_status(target, RequestStatus.Requesting);
				this.$os.api.send_ws('adventure:interaction', interaction_payload, (ret :any) => {
					if(callback) {
						callback(ret.payload);
					}
					target.request_status = RequestStatus.Idle;
					this.set_status(target, RequestStatus.Idle);
				});
				break;
			}
			default: {

				target.request_status = RequestStatus.Requesting;
				this.set_status(target, RequestStatus.Requesting);
				const exec = (finalType) => {
					this.$os.api.send_ws('adventure:' + finalType, { id: target.id }, (ret :any) => {

						if(callback) {
							callback(ret.payload);
						}

						target.request_status = RequestStatus.Idle;
						this.set_status(target, RequestStatus.Idle);

						if(ret.payload.status ? ret.payload.status == "FAILED" : true) {
							this.$os.notifications.addNotification(new Notification({
								UID: Eljs.getNumGUID(),
								Type: 'Status',
								Title: 'Failed',
								Message: 'Failed to save contract: ' + ret.payload.reason,
							}));

						}

					});
				};

				// Guards
				switch(type){
					case 'cancel_confirm': {
						exec('cancel');
						break;
					}
					case 'cancel': {

						let title = "";
						switch(target.state) {
							case "Saved": {
								title = 'Are you sure you want to unsave this ' + target.template.type_label + '?';
								break;
							}
							default: {
								title = 'Are you sure you want to cancel this ' + target.template.type_label + '?';
								break;
							}
						}

						this.$os.modals.add({
							type: 'ask',
							title: title,
							text: [
								'This cannot be undone and this ' + target.template.type_label + ' will not be recoverable. '
							],
							actions: {
								yes: 'Yes',
								no: 'Cancel'
							},
							data: {
							},
							func: (state :boolean) => {
								if(state) {
									exec(type);
								} else {
									target.request_status = RequestStatus.Idle;
								}
							}
						});

						break;
					}
					default: {
						exec(type);
						break;
					}
				}

				break;
			}
		}
	}

	public dispose_list(targets: Contract[]) {
		targets.forEach(contract => {
			if(!contract.is_disposed()) {
				contract.dispose();
				this.workers_cumulative--;
			}
		});
		this.report_cumulative();
		return [];
	}

	public dispose_single(target :Contract) {
		if(target) {
			if(!target.is_disposed()) {
				target.dispose();
				this.workers_cumulative--;
				this.report_cumulative();
			}
		}
	}

	public ingest(input_contract :any, input_template :any) {
		const contract = new Contract(this.$os, input_contract);
		const template = new Template(input_template);
		contract.template = template;
		contract.watch_expire();

		/*
		const t = new Date();
		t.setSeconds(t.getSeconds() + 10);
		contract.pull_at = t;
		contract.watch_expire();
		*/

		this.workers_cumulative++;
		this.report_cumulative();
		return contract;
	}

	public ingest_list(input_contracts :any[], input_templates :any[]) {

		const contracts = [] as Contract[];
		const templates = [] as Template[];

		// Create Templates
		input_templates.forEach(input_template => {
			const template = new Template(input_template);
			templates.push(template);
		});

		// Create Contracts with Templates as reference
		input_contracts.forEach(input_contract => {
			const contract = new Contract(this.$os, input_contract);
			contract.template = templates.find(x => x.file_name == contract.file_name && x.modified_on.getTime() == contract.modified_on.getTime());
			contract.watch_expire();

			if(contract.flight_plans) {
				const new_plans = [];
				contract.flight_plans.forEach((plan) => {
					new_plans.push(new Flightplan(plan));
				});
				contract.flight_plans = new_plans;
			}

			/*
			const t = new Date();
			t.setSeconds(t.getSeconds() + 10);
			contract.pull_at = t;
			contract.watch_expire();
			*/

			this.workers_cumulative++;
			contracts.push(contract);
		});

		this.report_cumulative();
		return contracts;
	}



	constructor(os :any, vue :_Vue) {
		this.$os = os;
	}

}