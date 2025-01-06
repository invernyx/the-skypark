import Template from "./template"
import Airport from "../airport"
import Invoices from "../invoices";
import Chat from "../messaging/chat";
import { OS } from "@/sys/services/os";
import Manifest from "../payload/manifest";
import Flightplan from "../flight_plans/plan";

export enum RequestStatus {
	Idle,
	Requesting
}

export class Situation {
	public id :number;
	public location :number[];
	public height :number;
	public visible :boolean;
	public dist_to_next :number;
	public icao :string;
	public airport :Airport;
	public trigger_range :number;
	public label :string;
}

export default class Contract {

	private $os = null as OS;

	public template :Template;
	public request_status = RequestStatus.Idle;
	public file_name = null;
	public title = null;
	public operated_for :string[];
	public modified_on :Date;
	public id = null;
	public is_monitored = null;
	public last_location_geo = null;
	public last_location_airport :Airport;
	public range_airports = null;
	public aircraft_compatible = null;
	public aircraft_used = null;
	public recommended_aircraft = null;
	public route = null;
	public flight_plans :Array<Flightplan>;
	public end_summary = null;
	public description = null;
	public description_long = null;
	public image_url = null;
	public distance = null;
	public duration_range :number[];
	public state = null;
	public route_code = null;
	public media_link = null;
	public expire_at :Date;
	public expired = false;
	public completed_at :Date;
	public started_at :Date;
	public pulled = false;
	public pull_at :Date;
	public requested_at :Date;
	public situation_at :number;
	public situations :Situation[];
	public interactions = null;
	public path = null;
	public manifests = null;
	public manifests_state = null;
	public limits = null;
	public memos :Chat[];
	public topo = null;
	public invoices :Invoices;

	public reward_xp :number;
	public reward_karma :number;
	public reward_bux :number;
	public reward_reliability :number;

	private expire_watch = null;
	private _is_disposed = false;
	public is_disposed() {
		return this.expire_watch == null && this._is_disposed;
	}

	constructor(os :OS, init?:Partial<Contract>) {
		this.$os = os;
        Object.assign(this, init);
	}

	public watch_expire() {
		clearTimeout(this.expire_watch);

		if(this.template) {
			if((this.state == "Listed" || this.state == "Saved") && (this.template.running_clock && this.template.time_to_complete > -1)) {
				const time_to_expire = this.pull_at.getTime() - new Date().getTime();
				if(time_to_expire > 0) {
					if(time_to_expire < 2147483647) {
						this.expire_watch = setTimeout(() => {
							if((this.state == "Listed" || this.state == "Saved")) {
								this.expired = true;
								this.$os.eventsBus.Bus.emit('contracts', {
									name: 'contract_expire',
									payload: this.id
								});
							}
						}, time_to_expire);
					}
				} else {
					this.expired = true;
				}
			}
		}
	}

	public dispose(){
		clearTimeout(this.expire_watch);
		this._is_disposed = true;
		this.expire_watch = null;
	}

}