import Aircraft from "../aircraft";
import Airport from "../airport";

export default class GroupState {

	public guid :string;
	public loaded_on :Aircraft;
	public transition_to :Aircraft;
	public loadable :boolean;
	public unloadable :boolean;
	public transferable :boolean;
	public deliverable :boolean;
	public boarded :number;
	public location :number[];
	public nearest_airport :Airport;

	constructor(init?:Partial<GroupState>) {
        Object.assign(this, init);
	}
}