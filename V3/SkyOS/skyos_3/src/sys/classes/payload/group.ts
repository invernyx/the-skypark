import Aircraft from "../aircraft";
import Airport from "../airport";

export default class Group {

	public guid :string;
	public loaded_on :Aircraft;
	public destination_index :number;
	public loadable :boolean;
	public unloadable :boolean;
	public transferable :boolean;
	public deliverable :boolean;
	public location :number[];
	public health :number;
	public nearest_airport :Airport;
	public origin :object

	constructor(init?:Partial<Group>) {
        Object.assign(this, init);
	}
}