import AircraftCabin from "./cabin/aircraft_cabin";
import Airport from "./airport";

export default class Aircraft {

	public id :number;
	public creator :string;
	public manufacturer :string;
	public model :string;
	public name :string;
	public is_duty :boolean;
	public size :number;
	public image :string;
	public image_blob :string
	public last_livery :string;
	public wingspan :number;
	public empty_weight :number;
	public max_weight :number;
	public engine_count :number;
	public insurance_billed :number;
	public damage_pcnt :number;
	public duty_flight_hours_month :number;
	public duty_flight_hours :number;
	public cabin :AircraftCabin;
	public cabins :AircraftCabin[];
	public directory_name :string;
	public directory_full :string;
	public duty_first_flown :Date;
	public duty_last_flown :Date;
	public insurance_last :Date;
	public insurance_contract_expire :Date;
	public totaled_instances :Date;
	public location :number[]
	public nearest_airport :Airport

	constructor(init?:Partial<Aircraft>) {
        Object.assign(this, init);

		if(init.cabin) {
			this.cabin = new AircraftCabin(init.cabin);
		}

		if(init.cabins) {
			this.cabins = [];
			init.cabins.forEach(cabin => {
				this.cabins.push(new AircraftCabin(cabin));
			});
		}

		if(this.image) {
			const contentType = 'image/jpg';
			const b64Data = this.image;

			const blob = this.b64toBlob(b64Data, contentType);
			this.image_blob = URL.createObjectURL(blob);
		}
	}

	private b64toBlob (b64Data, contentType='', sliceSize=512) {
		const byteCharacters = atob(b64Data);
		const byteArrays = [];

		for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
		  const slice = byteCharacters.slice(offset, offset + sliceSize);

		  const byteNumbers = new Array(slice.length);
		  for (let i = 0; i < slice.length; i++) {
			byteNumbers[i] = slice.charCodeAt(i);
		  }

		  const byteArray = new Uint8Array(byteNumbers);
		  byteArrays.push(byteArray);
		}

		const blob = new Blob(byteArrays, {type: contentType});
		return blob;
	  }

	public dispose() {

	}
}