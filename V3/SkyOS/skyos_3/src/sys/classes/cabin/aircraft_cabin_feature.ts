export default class AircraftCabinFeature {

	public guid :string;
	public type :string;
	public sub_type :string;
	public layer :string;
	public orientation :string;
	public is_attribute: Boolean;
	public x :number;
	public y :number;
	public z :number;

	constructor(init?:Partial<AircraftCabinFeature>) {
        Object.assign(this, init);

		this.guid = this.x + ':' + this.y + ':' + this.z + ':' + this.type + ':' + this.orientation;
	}

	public dispose() {

	}
}