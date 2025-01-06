
export default class Filter {
	public query = '';
	public bounds = null;
	public types = [];
	public companies = [];
	public onlyCustomContracts = false;
	public requiresLight = false;
	public requiresILS = false;
	public weatherExcl = {
		precip: [],
		wind: [],
		vis: [],
	};
	public range = [10,37040];
	public legsCount = [1,50];
	public runways = [0.03048,6.096];
	public rwyCount = [1,20];
	public rwySurface = 'any';
	public type = 'any';
	public sort = 'relevance';
	public sortAsc =  false;
	public subType = '';


	constructor(init?:Partial<Filter>) {
        Object.assign(this, init);

	}
}