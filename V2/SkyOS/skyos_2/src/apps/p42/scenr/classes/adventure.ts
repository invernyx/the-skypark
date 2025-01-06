import _Vue from 'vue';
import AdventureSituation from "../interfaces/adventure/situation";
import AdventureActionInterface from "../interfaces/adventure/action";
const moment = require("moment");
import Eljs from '@/sys/libraries/elem';

let vue: _Vue = null;

export default class AdventureProject {

	public Loaded = false;
	public Version = 3;
	public State = "Listed";
	public Published = false;
	public Name = "";
	public File = "";
	public TemplateCode = "";
	public ModifiedOn = new Date();
	public TypeLabel = "contract";
	public Type = ["Cargo"];
	public Company = ["clearsky"];
	public Tiers = ["discovery","endeavour"];
	public POIs = [];
	public DirectStart = false;
	public RunningClock = true;
	public TimeToComplete = 48;
	public ExpireMin = 0;
	public ExpireMax = 240;
	public Dates: Date[] = [
		new Date(),
		moment().add(5, 'years').toDate()
	];


	public GalleryURL = [] as string[];
	public ImageURL = [] as string[];
	public Description = [] as string[];
	public DescriptionLong = [] as string[];
	public MediaLink = [] as string[];
	public AircraftRestriction = "";
	public AircraftRestrictionLabel = "";

	public IncludeICAO = "";
	public ExcludeICAO = "";
	public IncludeAPTName = "";
	public ExcludeAPTName = "";

	public RewardPerItem = 1;
	public RewardBase = 200;
	public RewardPerNM = 1;

	public Concurent = 1;
	public Instances = 10;
	public RouteLimit = 100;
	public MaxPerDay = 1000;
	public XPBase = 0;
	public LvlMin = 0;
	public LvlMax = 999999;
	public KarmaGain = 1;
	public KarmaMin = -42;
	public KarmaMax = 42;
	public RatingGainSucceed = 5;
	public RatingGainFail = -8;
	public RatingMin = 0;
	public RatingMax = 100;
	public Unlisted = false;
	public StrictOrder = true;
	public DiscountFees = [];

	public Actions: AdventureActionInterface[] = [];
	public Situations: AdventureSituation[] = [];

	public SavedActions = [] as any[];
	public StartedActions = [] as any[];
	public SuccessActions = [] as any[];
	public FailedActions = [] as any[];


	constructor(Vue: _Vue, structure?: any) {
		vue = Vue;
		if (structure !== undefined) {
			Object.assign(this, structure);

			Vue.$data.state.ui.map.sources.situationBoundary.data.features = [];

			this.Situations.forEach((Sit, Index) => {
				Sit = new AdventureSituation(Sit);
				Sit.Index = Index;
				this.Situations[Index] = Sit;
				this.InitSitFeatures(Sit);
			});
		}
		this.ValidateGhosts();
		this.Loaded = true;
	}

	public InitSitFeatures(Sit :AdventureSituation) {
		const feature = {
			type: 'Feature',
			geometry: {
				type: 'Polygon',
				coordinates: [[]] as any
			}
		}

		if(Sit.Boundaries.length){
			feature.geometry.coordinates[0] = Sit.Boundaries;
		}

		vue.$data.state.ui.map.sources.situationBoundary.data.features.splice(Sit.Index, 0, feature);
	}

	public ValidateGhosts() {
		const accountedFor = [];
		const unAccountedFor = [];

		const proc = (act) => {
			Object.keys(act['Params']).forEach(key => {
				if(key.endsWith('Actions')) {
					act['Params'][key].forEach(act1 => {
						proc(this.Actions.find(x => x.UID == act1));
						accountedFor.push(act1);
					});
				}
			});
		}

		// Template base
		Object.keys(this).forEach(key => {
			if(key.endsWith('Actions') && key != 'Actions') {
				this[key].forEach(act => {
					proc(this.Actions.find(x => x.UID == act));
					accountedFor.push(act);
				});
			}
		})

		// Situations
		this.Situations.forEach((Sit, Index) => {
			Object.keys(Sit).forEach(key => {
				if(key.endsWith('Actions')) {
					Sit[key].forEach(act => {
						proc(this.Actions.find(x => x.UID == act));
						accountedFor.push(act);
					});
				}
			})
		});

		// Confirm ghosts
		this.Actions.forEach(act => {
			if(!accountedFor.includes(act.UID)) {
				console.log("Ghost found: ", act);
				unAccountedFor.push(act);
			}
		});

		unAccountedFor.forEach(act => {
			const index = this.Actions.indexOf(act);
			this.Actions.splice(index, 1);
		});

	}

	public AddSituation(Index :number) {
		const NewSit = new AdventureSituation();
		NewSit.UID = Eljs.getNumGUID();
		NewSit.Index = Index;
		this.Situations.splice(Index, 0, NewSit);
		this.InitSitFeatures(NewSit);

		this.Situations.forEach((Sit, Index) => {
			Sit.Index = Index;
		});
	}

	public RemoveSituation(UID :number) {
		const index = this.Situations.findIndex((x: AdventureSituation) => x.UID === UID);
		this.Situations.splice(index, 1);
		vue.$data.state.ui.map.sources.situationBoundary.data.features.splice(index, 1);

		// Remove airport heatmap
		const sourceData = vue.$data.state.ui.map.sources.situationAirports.data;
		const Existing = sourceData.features.filter((x :any) => x.properties.id == 'i' + UID);
		Existing.forEach((element :any) => {
			const Index = sourceData.features.indexOf(element);
			sourceData.features = sourceData.features.splice(Index, 1);
		});

		vue.$emit('adventure:situation:update', this);
		this.Situations.forEach((Sit, Index) => {
			Sit.Index = Index;
		});
	}

	public CopySituation(UID :number) {
		const index = this.Situations.findIndex((x: AdventureSituation) => x.UID === UID);
		const sit = this.Situations.find((x: AdventureSituation) => x.UID === UID);
		const newSit = JSON.parse(JSON.stringify(sit));
		newSit.UID = Eljs.getNumGUID();
		this.Situations.splice(index, 0, newSit);
		this.InitSitFeatures(this.Situations[index]);

		this.Situations.forEach((Sit, Index) => {
			Sit.Index = Index;
		});
	}

	public Save() {
		this.ValidateGhosts();
		const struct = JSON.stringify(this);
		return struct;
	}

	public GetActionSituationMaps() {

		const Map = [] as {
			Sitution: number,
			ActionID: number
		}[];

		const ProcAction = (Action :any, Index :number) => {
			Map.push({
				Sitution: Index,
				ActionID: Action.UID
			})
			Object.keys(Action.Params).forEach((k :string, i :number) => {
				if(k.toLocaleLowerCase().endsWith('actions')) {
					Action.Params[k].forEach((A3: any) => {
						ProcAction(this.Actions.find(x => x.UID == A3), Index);
					});
				}
			});
		}

		this.Situations.forEach((sit, index) => {
			sit.Actions.forEach(actID => {
				const act = this.Actions.find(x => x.UID == actID);
				if(act != null) {
					ProcAction(act, index);
				}
			});
		});

		return Map;
	}

	public GetAvailable(CurrentAction: any, StartCode: string, EndCode: string, Exclusive = true) {

		const Starts = [] as AdventureActionInterface[];

		const ProcAction = (Action: any) => {

			if (Action.UID === CurrentAction.UID) {
				return;
			}

			switch (Action.Action) {
				case StartCode: {
					Starts.push(Action);
					break;
				}
				case EndCode: {
					if(Exclusive) {
						const lnk = Starts.findIndex(x => x.UID === Action.Params.Link);
						if (lnk > -1) {
							Starts.splice(lnk, 1);
						}
					}
					break;
				}
			}

			Object.keys(Action.Params).forEach((k :string, i :number) => {
				if(k.toLocaleLowerCase().endsWith('actions')) {
					Action.Params[k].forEach((A3: any) => {
						ProcAction(this.Actions.find(x => x.UID == A3));
					});
				}
			});
		}

		this.Situations.forEach(sit => {
			sit.Actions.forEach(actID => {
				const act = this.Actions.find(x => x.UID == actID);
				if(act != null) {
					ProcAction(act);
				}
			});
		});

		return Starts;
	}
}