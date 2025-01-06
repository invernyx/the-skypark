import _Vue from 'vue';
import { OS } from '@/sys/services/os'
import Eljs from '@/sys/libraries/elem';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';

export enum RequestStatus {
	Idle,
	Requesting
}

export default class FleetService {
	private $os: OS;

	public aircraft_current :Aircraft;

	public startup() {
		this.$os.eventsBus.Bus.on('ws-in', (e) => this.broadcast_internal(e, this));
		this.$os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
		this.get_current_aircraft();
	}

	private get_current_aircraft() {
		this.$os.api.send_ws(
			"fleet:get_current", {
				fields: null
			 },
			(result) => {
				if(result.payload != null) {
					const new_aircraft = this.ingest(result.payload);
					this.aircraft_current = new_aircraft;
				} else {
					this.aircraft_current = null;
				}
				this.$os.eventsBus.Bus.emit('fleet', { name: 'current_aircraft', payload: {
					aircraft: this.aircraft_current
				}});
			}
		);
	}

	private broadcast_internal(wsmsg: any, self: FleetService) {
		switch(wsmsg.name[0]){
			case 'fleet': {
				switch(wsmsg.name[1]){
					case 'remove_aircraft': {
						self.$os.eventsBus.Bus.emit('fleet', { name: 'remove_aircraft', payload: {
							id: wsmsg.payload.id
						}});
						break;
					}
					case 'cabin': {
						self.$os.eventsBus.Bus.emit('fleet', { name: 'cabin', payload: {
							livery: wsmsg.payload.livery,
							cabin: wsmsg.payload.cabin as Partial<AircraftCabin>
						}});
						break;
					}
					default: {
						self.$os.eventsBus.Bus.emit('fleet', { name: 'mutate', payload: {
							id: wsmsg.payload.id,
							aircraft: wsmsg.payload.aircraft as Partial<Aircraft>
						}});
						break;
					}
				}
				break;
			}
		}
	}

	public event_list(actions :string[], target_id: number, mutation: Partial<Aircraft>, list: Aircraft[]) {

		switch(actions[0]) {
			case 'remove': {
				list.forEach((aircraft, index) => {
					if(aircraft.id == target_id) {
						list.splice(index, 1);
					}
				});
				break;
			}
			default: {
				list.forEach(aircraft => {
					this.event(actions, target_id, mutation, aircraft);
				});
				break;
			}
		}
	}

	public event(actions :string[], target_id: number, mutation: Partial<Aircraft>, aircraft: Aircraft) {

		if(aircraft && aircraft.id == target_id){
			switch(actions[0]) {
				default: {
					// Merge the new data with existing data
					Eljs.merge_deep(aircraft, mutation);
					break;
				}
			}
		}

	}

	public set_status(target: Aircraft, status :RequestStatus) {
		this.$os.eventsBus.Bus.emit('fleet', { name: 'mutate', payload: {
			id: target.id,
			aircraft: {
				request_status: status
			} as Partial<Aircraft>
		}});
	}

	public interact(target: Aircraft, type: string, interaction_payload :any = null, callback :Function = null) {
		switch(type) {
			case 'interaction': {
				break;
			}
			default: {

				break;
			}
		}
	}

	public dispose_list(targets: Aircraft[]) {
		targets.forEach(aircraft => {
			aircraft.dispose();
		});
		return [];
	}

	public dispose_single(target :Aircraft) {
		if(target) {
			target.dispose();
		}
	}

	public ingest(input_aircraft :any) {
		const aircraft = new Aircraft(input_aircraft);

		/*
		const t = new Date();
		t.setSeconds(t.getSeconds() + 10);
		contract.pull_at = t;
		contract.watch_expire();
		*/

		return aircraft;
	}

	public ingest_list(input_fleet :any[]) {

		const fleet = [] as Aircraft[];

		// Create Contracts with Templates as reference
		input_fleet.forEach(input_aircraft => {
			const aircraft = new Aircraft(input_aircraft);

			fleet.push(aircraft);
		});

		return fleet;
	}


	public listener_ws(wsmsg: any, self: FleetService){
		switch(wsmsg.name[0]){
			case 'fleet': {
				switch(wsmsg.name[1]){
					case 'current_aircraft': {
						const new_aircraft = self.ingest(wsmsg.payload);
						if(self.aircraft_current ? (new_aircraft.id != self.aircraft_current.id || new_aircraft.last_livery != self.aircraft_current.last_livery) : true) {
							self.aircraft_current = new_aircraft;
							self.$os.eventsBus.Bus.emit('fleet', { name: 'current_aircraft', payload: {
								aircraft: self.aircraft_current
							}});
						}
						break;
					}
					case 'cabin': {
						if(self.aircraft_current ? wsmsg.payload.livery == self.aircraft_current.cabin.livery : false) {
							self.aircraft_current.cabin.update(wsmsg.payload.cabin as Partial<AircraftCabin>)
						}
						break;
					}
				}
				break;
			}
			case 'sim': {
				switch(wsmsg.name[1]){
					case 'status': {
						if(!wsmsg.payload.State){
							this.aircraft_current = null;
							self.$os.eventsBus.Bus.emit('fleet', { name: 'current_aircraft', payload: {
								aircraft: null
							}});
						}
						break;
					}
				}
				break;
			}
			case 'connect': {
				this.get_current_aircraft();
				break;
			}
			case 'disconnect': {
				this.aircraft_current = null;
				self.$os.eventsBus.Bus.emit('fleet', { name: 'current_aircraft', payload: {
					aircraft: null
				}});
				break;
			}
		}
	}


	constructor(os :any, vue :_Vue) {
		this.$os = os;
	}

}