import Airport from "../airport";
import Destination from "./destination";
import Group from "./group";

export default class Manifest {

	public id :number;
	public total_quantity :number;
	public total_weight :number;
	public cargo_guid :string;
	public name :string;
	public weight :string;
	public destinations :Destination[];
	public origin :{
		airport :Airport,
		location :number[]
	};
	public groups :Group[];

	constructor(init?:Partial<Manifest>) {
        Object.assign(this, init);
	}
}