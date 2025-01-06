import Airport from "../airport"


export default class Flightplan {

	public file_name: string;
	public id: number;
	public hash: string;
	public name: string;
	public file: string;
	public last_modified: Date;
	public airports: Airport[];
	public distance: number;
	public waypoints: Waypoint[];

	constructor(init?:Partial<Flightplan>) {
        Object.assign(this, init);
	}
}

export class Waypoint {
	type: string;
	airway: string;
	location :number[];
	code: string;
	airport: Airport;
	airway_first: boolean;
	airway_last: boolean;
	airway_index: number;
	dist_to_next: number;

	constructor(init?:Partial<Waypoint>) {
        Object.assign(this, init);
	}
}