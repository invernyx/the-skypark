import AircraftCabinFeature from "./aircraft_cabin_feature";

export default class AircraftCabin {

	public name :string;
	public livery :string;
	public levels: number[][];

	public seatbelts_behavior = 2;
	public seatbelts_state = false;

	public features :AircraftCabinFeature[];

	public static seat_x =[
		'A',
		'B',
		'C',
		'D',
		'E',
		'F',
		'G',
		'H',
		'J',
		'K',
		'L',
		'M',
		'N',
		'P',
		'R',
		'T',
		'U',
		'V',
		'W',
		'X',
		'Y',
	]

	public static feature_options = [
		{
			type: 'util',
			name: "Utilities",
			sub_types: [
				{
					sub_type: 'partition',
					name: 'Partition',
					layer: 'wall',
					include_in_count: false,
				},
				{
					sub_type: 'fill',
					name: 'Fill',
					layer: 'floor',
					include_in_count: false,
				},
				{
					sub_type: 'lavatories',
					name: 'Lavatories',
					layer: 'floor',
					include_in_count: false,
				},
				{
					sub_type: 'galley',
					name: 'Galley',
					layer: 'floor',
					include_in_count: false,
				}
			]
		},
		{
			type: 'door',
			name: "Doors",
			sub_types: [
				{
					sub_type: '',
					name: 'Door',
					layer: 'wall',
					include_in_count: false,
				},
			]
		},
		{
			type: 'stairs',
			name: "Stairs",
			sub_types: [
				{
					sub_type: '',
					name: 'Stairs',
					layer: 'floor',
					include_in_count: false,
				},
			]
		},
		{
			type: 'seat',
			name: "Seats",
			sub_types: [
				{
					sub_type: 'first_class',
					name: 'First Class seat',
					layer: 'floor',
					include_in_count: true,
				},
				{
					sub_type: 'premium_class',
					name: 'Premium Class seat',
					layer: 'floor',
					include_in_count: true,
				},
				{
					sub_type: 'premium_economy_class',
					name: 'Premium Eco seat',
					layer: 'floor',
					include_in_count: true,
				},
				{
					sub_type: 'economy_class',
					name: 'Economy seat',
					layer: 'floor',
					include_in_count: true,
				},
				{
					sub_type: 'pilot',
					name: 'Pilot seat',
					layer: 'floor',
					include_in_count: false,
				},
				{
					sub_type: 'copilot',
					name: 'Copilot seat',
					layer: 'floor',
					include_in_count: false,
				},
				{
					sub_type: 'jumpseat',
					name: 'Jump seat',
					layer: 'floor',
					include_in_count: false,
				},
			]
		},
		{
			type: 'cargo',
			name: "Cargo",
			sub_types: [
				{
					sub_type: 'small',
					name: 'Small Cargo area',
					layer: 'floor',
					include_in_count: false,
				},
				{
					sub_type: 'medium',
					name: 'Medium Cargo area',
					layer: 'floor',
					include_in_count: false,
				},
				{
					sub_type: 'large',
					name: 'Large Cargo area',
					layer: 'floor',
					include_in_count: false,
				},
			]
		}
	];

	public static human_state = {
		walking: "Walking",
		waiting_path: "Waiting",
		getting_seated: "Getting seated",
		seated: "Seated",
		bathroom: "Lavatories",
		galley: "Preparing"
	}

	public static human_sub_state = {
		none: "",
		music: "listening to music",
		movie: "watching a movie",
		working: "working",
		sleeping: "sleeping",
		taking_orders: "Taking Orders",
		serving: "Serving Orders",
		deceased: "Deceased"
	}

	public get_level_label(z :number) {

		const features_in_z = this.features.filter(x => x.z == z && x.type != 'util');

		const feature_groups = features_in_z.reduce((groups, item) => {
			const group = (groups[item.type] || []);
			group.push(item);
			groups[item.type] = group;
			return groups;
		}, {});

		const feature_sets = (Object.values(feature_groups) as Array<any>).sort((x1, x2) => x2.length - x1.length);

		let feature_set_dominant = '';

		if(feature_sets.length) {
			feature_set_dominant = feature_sets[0][0].type;
		}

		switch(feature_set_dominant){
			case 'seat': {
				return 'Cabin';
			}
			case 'cargo': {
				return 'Cargo';
			}
			default: {
				return 'Floor';
			}
		}

	}

	public get_feature_count(type :string) {
		const features_lib = AircraftCabin.feature_options.find(x => x.type == type);
		return this.features.filter(x => x.type == type && features_lib.sub_types.find(x1 => x1.sub_type == x.sub_type).include_in_count ).length;
	}

	public update(init?:Partial<AircraftCabin>){
		//Eljs.merge_deep(self.aircraft_current.cabin, wsmsg.payload.cabin);
        Object.assign(this, init);
	}

	constructor(init?:Partial<AircraftCabin>) {
        Object.assign(this, init);
	}

	public dispose() {

	}
}