import _Vue from 'vue';
import { OS } from '@/sys/services/os'
import Notification from '@/sys/models/notification';
import Flightplan, { Waypoint } from '@/sys/classes/flight_plans/plan';
import Contract from '@/sys/classes/contracts/contract';
import SearchStates from '@/sys/enums/search_states';
import Eljs from '@/sys/libraries/elem';

export default class Navigation {
	private $os: OS;
	public legs: NavLeg[];
	public contract: Contract;
	public plan: Flightplan;
	public total_distance: number;

	constructor(os :any, vue :_Vue) {
		this.$os = os;

		this.legs = null;
		this.contract = null;
		this.plan = null;
	}

	public startup() {
		this.$os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
		this.$os.eventsBus.Bus.on('nav_set', (e) => this.nav_set(e, this));
	}

	private build() {
		if(this.plan) {
			this.legs = [];
			this.plan.waypoints.forEach((waypoint :Waypoint, index :number) => {

				const node = new NavLeg({
					index: index,
					start: waypoint,
				});

				if(index > 0){
					const node_prev = this.legs[node.index - 1];
					node.dist_to_next = Eljs.GetDistance(waypoint.location[1], waypoint.location[0], node_prev.start.location[1], node_prev.start.location[0], "km");
					node.dist_total_at_start = node_prev.dist_total_at_start + node_prev.dist_to_next;
				}

				this.legs.push(node);
			});
			this.load_topo();
		} else {
			this.legs = null;
		}
		this.refresh();
		this.broadcast();
	}

	private refresh() {

		if(this.legs) {

			this.legs.forEach((leg, index) => {
				leg.is_current = false;
				let leg_next = null;
				let leg_prev = null;

				if(index < this.legs.length) {
					leg_next = this.legs[index + 1];
				}

				if(index > 0) {

					leg_prev = this.legs[index - 1];

					// Current location
					const closest_to_leg = Eljs.ClosestToPoly(
						leg_prev.start.location[0],
						leg_prev.start.location[1],
						leg.start.location[0],
						leg.start.location[1],
						this.$os.simulator.location.Lon,
						this.$os.simulator.location.Lat,
					);
					const dist_to_leg = Eljs.GetDistance(closest_to_leg[2], closest_to_leg[1], this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, "km");

					// Seek ahead
					const seconds_ahead = 15;
					const location_offset_ahead = Eljs.MapOffsetPosition(this.$os.simulator.location.Lon, this.$os.simulator.location.Lat, this.$os.simulator.location.GS * 0.514444 * seconds_ahead, this.$os.simulator.location.CRS);
					const location_ahead_closest_to_leg = Eljs.ClosestToPoly(
						leg_prev.start.location[0],
						leg_prev.start.location[1],
						leg.start.location[0],
						leg.start.location[1],
						location_offset_ahead[0],
						location_offset_ahead[1],
					);
					const location_ahead_dist_to_leg = Eljs.GetDistance(location_ahead_closest_to_leg[2], location_ahead_closest_to_leg[1], location_offset_ahead[1], location_offset_ahead[0], "km");

					// Calculate
					//leg.dist_total_at_start = leg.dist_total_at_start + Eljs.GetDistance(leg.start.location[1], leg.start.location[0], leg_prev.start.location[1], leg_prev.start.location[0], "km");
					leg.dist_to_leg = dist_to_leg; //d0
					leg.dist_ahead_to_leg = location_ahead_dist_to_leg; //d1
					leg.closest_on_leg = closest_to_leg[3]; // c
					leg.track_offset = Math.abs(Eljs.MapCompareBearings(leg.bearing_to, this.$os.simulator.location.CRS)); //b
					leg.distance = Eljs.GetDistance(leg.start.location[1], leg.start.location[0], this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, "km"); //ir

					leg.next = leg_next; //n1
					leg.prev = leg_prev;
				} else {
					//leg.dist_total_at_start = Eljs.GetDistance(leg.start.location[1], leg.start.location[0], this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, "km");
					leg.dist_to_leg = leg.dist_total_at_start; //d0
					leg.dist_ahead_to_leg = leg.dist_total_at_start; //d1
					leg.closest_on_leg = 0; // c
					leg.track_offset = Math.abs(Eljs.MapCompareBearings(leg.bearing_to, this.$os.simulator.location.CRS)); //b
					leg.distance = leg.dist_total_at_start; //ir
					leg.next = leg_next; //n1
					leg.prev = leg_prev;
				}


				leg.bearing_to = Eljs.GetBearing(this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, leg.start.location[1], leg.start.location[0]);
				leg.bearing_from = Eljs.GetBearing(leg.start.location[1], leg.start.location[0], this.$os.simulator.location.Lat, this.$os.simulator.location.Lon);
				leg.dist_direct = Eljs.GetDistance(leg.start.location[1], leg.start.location[0], this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, "km");
				leg.bearing_dif = Eljs.MapCompareBearings(this.$os.simulator.location.CRS, leg.bearing_to);
			});

			const legs_sorted = Object.assign([], this.legs) as NavLeg[];
			legs_sorted.sort((x1, x2) => {
				if(x2.index > 0) {
					const dists = ((x1.dist_to_leg + x1.dist_ahead_to_leg) - (x2.dist_to_leg + x2.dist_ahead_to_leg));
					const apt = (x2.start.type == 'airport' && x2.distance < 1 ? -3000 : 0);
					const hdg = this.$os.simulator.location.GAlt > 10 ? (Math.abs(x2.track_offset) / 180) - (Math.abs(x1.track_offset) / 180) : 0;
					return dists + apt - hdg;
				} else {
					return -9999999
				}
			});

			const leg_current = legs_sorted[0];
			if(leg_current) {
				leg_current.is_current = true;
			}

			let dist_cumul = 0;
			this.legs.forEach(leg => {

				// Figure out progress percentages
				if(this.$os.simulator.live) {
					if(leg_current.index < leg.index) {
						leg.progress = 0;
					} else if(leg_current.index > leg.index) {
						leg.progress = 100;
					} else {
						leg.progress = Eljs.limiter(0, 100, leg.closest_on_leg * 100);
					}
				} else {
					leg.progress = -1;
				}

				// Figure out distance to go
				if(leg.index > 0) {
					if(leg.index > leg_current.index) {
						dist_cumul += leg.dist_to_next;
						leg.dist_to_go = dist_cumul;
					} else if(leg.index == leg_current.index) {
						leg.dist_to_go = Eljs.GetDistance(this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, leg.start.location[1], leg.start.location[0], "km");
						dist_cumul += leg.dist_to_go;
					} else {
						leg.dist_to_go = -1;
					}
				}

				// Calculate ETA and DTG
				if(leg.index < leg_current.index || !this.$os.simulator.live || this.$os.simulator.location.GS < 20){
					leg.ete = -1;
				} else {
					leg.ete = (leg.dist_to_go / (this.$os.simulator.location.GS * 1.852));
				}

			});


			//console.log(legs_sorted.map(x => {
			//	return {
			//		n: x.start.code,
			//		i: x.is_current,
			//		d: x
			//	}
			//}));

		}
	}

	private load_topo(){

		const paths = [];
		const nodes = [];
		this.legs.forEach(node => {
			if(node.index > 0){
				const prev = this.legs[node.index - 1];
				if(prev.topo == null) {
					prev.topo = [];
					nodes.push([prev, node]);
					paths.push([prev.start.location, node.start.location]);
				}
			}
		});

		this.$os.api.send_ws(
			'topography:getForPath', {
				paths: paths,
				resolution: 3,
			}, (topoData :any) => {

				this.legs.forEach((node, index) => {
					if(index > 0) {
						node.topo = topoData.payload[index - 1];
					}
				});

				this.refresh();
				this.broadcast();
			}
		);
	}

	private broadcast() {

		this.$os.eventsBus.Bus.emit("navigation", {
			name: 'data',
			payload: {
				nodes: this.legs,
				contract: this.contract,
				plan: this.plan,
			}
		});
	}


	public listener_ws(wsmsg: any, self: Navigation){
		switch(wsmsg.name[0]){
			case 'eventbus': {
				switch(wsmsg.name[1]){
					case 'meta': {
						self.refresh();
						self.broadcast();
						break;
					}
				}
				break;
			}
		}
	}

	public nav_set(wsmsg :any, self: Navigation) {
		switch(wsmsg.name) {
			case 'plan_contract': {

				if(self.contract && self.plan && wsmsg.payload.contract && wsmsg.payload.plan) {
					if(self.contract.id == wsmsg.payload.contract.id && self.plan.id == wsmsg.payload.plan.id) {
						return;
					}
				}

				self.contract = wsmsg.payload.contract;
				self.plan = wsmsg.payload.plan;

				self.build();
				break;
			}
		}
	}

}

export class NavLeg {
	public index: number;
	public is_next: boolean;
	public is_current: boolean;
	public bearing_to: number;
	public bearing_from: number;
	public bearing_dif: number;
	public dist_direct: number;
	public dist_total_at_start: number;
	public dist_to_next: number;
	public dist_to_go: number;
	public ete: number;
	public topo: number[];
	public max_alt: number;
	public max_alt_ft: number;
	public max_alt_offset: number;
	public is_last: boolean;
	public is_range: boolean;
	public progress: number;
	public start: Waypoint;

	public dist_to_leg: number; //d0
	public dist_ahead_to_leg: number; //d1
	public closest_on_leg: number; // c
	public track_offset: number; //b
	public distance: number; //ir
	public next: NavLeg; //n1
	public prev: NavLeg; //n1

	constructor(init?:Partial<NavLeg>) {

		this.is_next = false;
		this.is_current = false;
		this.is_last = false;
		this.is_range = false;
		this.progress = 0;
		this.bearing_to = 0;
		this.bearing_from = 0;
		this.bearing_dif = 0;
		this.dist_direct = 0;
		this.dist_total_at_start = 0;
		this.dist_to_next = 0;
		this.dist_to_go = 0;
		this.ete = 0;
		this.topo = null;
		this.max_alt = 0;
		this.max_alt_ft = 0;
		this.max_alt_offset = 0;
		this.start = null;

		this.dist_to_leg = 0; //d0
		this.dist_ahead_to_leg = 0; //d1
		this.closest_on_leg = 0; // c
		this.track_offset = 0; //b
		this.distance = 0; //ir
		this.next = null; //n1

        Object.assign(this, init);
	}
}