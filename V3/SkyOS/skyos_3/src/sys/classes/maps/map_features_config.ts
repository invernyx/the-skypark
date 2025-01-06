import SearchStates from "@/sys/enums/search_states";
import Contract from "../contracts/contract";
import Flightplan from "../flight_plans/plan";

export default class MapFeaturesConfig {

	public status = SearchStates.Idle;
	public contracts = [] as Contract[];
	public plans = [] as Flightplan[];
	public count = 0;

	public selected = {
		contract: null as Contract,
		plan: null as Flightplan,
	}

	public settings = null as {
		contract_show_badges? :boolean,
		contract_can_select? :boolean,
	}

	constructor(init?:Partial<MapFeaturesConfig>) {
        Object.assign(this, init);

		this.settings = {
			contract_show_badges: true,
			contract_can_select: true,
		}

		Object.assign(this.settings, init?.settings);
	}
}