import _Vue from 'vue';

class ContractMutator {

	private vue: _Vue = null;

	constructor(Vue: _Vue) {
		this.vue = Vue;

	}

	public EventInList(wsmsg: any, list: any) {

		list.Contracts.forEach(targetContract => {
			const targetTemplate = list.Templates.find((x :any) => x.FileName == targetContract.FileName);
			this.Event(wsmsg, targetContract, targetTemplate);
		});

		switch(wsmsg.name[1]) {
			case 'delete': {
				list.Contracts.forEach((element, index) => {
					if(element.ID == wsmsg.payload.ID) {
						list.Contracts.splice(index, 1);
					}
				});
				break;
			}
		}
	}

	public Event(wsmsg: any, targetContract: any, targetTemplate :any) {

		switch(wsmsg.name[0]) {
			case 'flightplans': {
				const foundFlpIndex = targetContract.Flightplans.findIndex(x => x.Hash == wsmsg.payload.Hash);

				switch(wsmsg.name[1]) {
					case 'change':
					case 'rename': {
						if(foundFlpIndex > -1) {
							targetContract.Flightplans[foundFlpIndex] = wsmsg.payload.Updated;
						} else {
							targetContract.Flightplans.push(wsmsg.payload.Updated);
						}
						break;
					}
					case 'delete': {
						if(foundFlpIndex > -1) {
							targetContract.Flightplans.splice(foundFlpIndex, 1);
						}
						break;
					}
				}
				break;
			}
		}

		if(targetContract && targetContract.ID == wsmsg.payload.ID){
			switch(wsmsg.name[1]) {
				case 'request':
				case 'state': {
					if(wsmsg.payload.State !== undefined){

						if(targetContract.State != wsmsg.payload.State) {
							switch(wsmsg.payload.State) {
								case 'Active':
								case 'Saved': {
									if(!targetTemplate.RunningClock) {
										const n = new Date();
										n.setTime(n.getTime() + (targetTemplate.TimeToComplete*60*60*1000));
										targetContract.ExpireAt = n.toString();
									}
									break;
								}
							}
						}

						targetContract.State = wsmsg.payload.State;
					}

					if(wsmsg.payload.AircraftCompatible !== undefined) {
						targetContract.AircraftCompatible = wsmsg.payload.AircraftCompatible;
					}

					if(wsmsg.payload.LastLocationGeo !== undefined) {
						targetContract.LastLocationGeo = wsmsg.payload.LastLocationGeo;
					}

					if(wsmsg.payload.EndSummary !== undefined) {
						targetContract.EndSummary = wsmsg.payload.EndSummary;
					}

					if(wsmsg.payload.CompletedAt !== undefined) {
						targetContract.CompletedAt = wsmsg.payload.CompletedAt;
					}

					if(wsmsg.payload.StartedAt !== undefined) {
						targetContract.StartedAt = wsmsg.payload.StartedAt;
					}

					if(wsmsg.payload.IsMonitored !== undefined) {
						targetContract.IsMonitored = wsmsg.payload.IsMonitored;
					}

					if(wsmsg.payload.RangeAirports !== undefined) {
						targetContract.RangeAirports = wsmsg.payload.RangeAirports;
					}

					break;
				}
				case 'manifests': {
					targetContract.Manifests = wsmsg.payload.Manifests;
					break;
				}
				case 'invoices': {
					targetContract.Invoices = wsmsg.payload.Invoices;
					break;
				}
				case 'interactions': {
					targetContract.Interactions = wsmsg.payload.Interactions;
					break;
				}
				case 'memos': {
					targetContract.Memos = wsmsg.payload.Memos;
					break;
				}
				case 'path': {
					targetContract.Path = wsmsg.payload.Path;
					break;
				}
				case 'limits': {
					targetContract.Limits = wsmsg.payload.Limits;
					break;
				}
				case 'flightplan': {
					targetContract.Flightplans.push(wsmsg.payload.Plan);
					break;
				}
			}
		}

	}

	public Interact(targetContract: any, type: string, payload :any, callback :Function) {
		switch(type) {
			case 'manage': {
				this.vue.$router.push({ name: 'p42_conduit', params: { contract: targetContract.ID }});
				break;
			}
			case 'plan': {
				this.vue.$router.push({ name: 'p42_yoflight', params: { contract: targetContract.ID }});
				break;
			}
			case 'fly': {
				this.vue.$router.push({ name: 'p42_conduit', params: { contract: targetContract.ID }});
				break;
			}
			case 'interaction': {
				targetContract.RequestStatus = 'loading';
				this.vue.$data.services.api.SendWS('adventure:interaction', payload, (ret :any) => {
					targetContract.RequestStatus = 'ready';
					if(callback) {
						callback(ret.payload);
					}
				});
				break;
			}
			default: {
				targetContract.RequestStatus = 'loading';
				this.vue.$data.services.api.SendWS('adventure:' + type, { ID: targetContract.ID }, (ret :any) => {
					targetContract.RequestStatus = 'ready';
					if(callback) {
						callback(ret.payload);
					}
				});
				break;
			}
		}
	}

}

export default {
	install: (Vue: typeof _Vue) => { //, options?: any
	  let installed = false;
	  Vue.mixin({
		beforeCreate() {
		  if (!installed) {
			installed = true;
			Vue.prototype.$ContractMutator = new ContractMutator(this);
		  }
		}
	  });
	}
  };

  declare module 'vue/types/vue' {
	interface Vue {
	  $ContractMutator: ContractMutator;
	}
  }