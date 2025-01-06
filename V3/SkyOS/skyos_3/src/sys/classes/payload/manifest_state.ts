import Airport from "../airport";
import Destination from "./destination";
import GroupState from "./group_state";

export default class ManifestState {

	public id :number;
	public groups :GroupState[];

	constructor(init?:Partial<ManifestState>) {
        Object.assign(this, init);
	}
}