import Airport from "../airport";

export default class Destination {

	public location :number[];
	public airport :Airport;

	constructor(init?:Partial<Destination>) {
        Object.assign(this, init);
	}
}