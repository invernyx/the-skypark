import mitt from 'mitt' //, { Emitter, EventType }
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export enum AppType {
	GENERAL, // 0
	HOME, // 1
	SLEEP, // 2
	LOCKED, // 3
	SYSTEM, // 4
}

export enum NavType {
	NONE, // 0
	BRIGHT, // 1
	DARK, // 2
}

export enum StatusType {
	NONE, // 0
	BRIGHT, // 1
	DARK, // 2
}

export class AppEvents {
	emitter = mitt();//:Emitter<Record<EventType, unknown>>
	open() {
	}
	close() {
	}
	constructor(init?:Partial<AppEvents>) {
        Object.assign(this, init);

		this.emitter.on('*', (ev, msg) => {
			if(msg) {
				console.debug("App Event |", ev, msg);
			} else {
				console.debug("App Event |", ev);
			}
		});
    }
}

export class Theme {
	public name: string;
	public bright: {
		status: StatusType,
		nav: NavType,
		shaded: boolean,
	}
	public dark: {
		status: StatusType,
		nav: NavType,
		shaded: boolean,
	}
}

export class ThemeAccent {
	public name: string;
	public bright: string;
	public dark: string;
	public image: string;
}



export class AppInfo {
	name: string;
	url: string;
	vendor: string;
	ident: string;
	uid: string;
	nav_class: string;
	type: AppType = AppType.GENERAL;
	windowed = false;
	visible = true;
	isexternal = false;
	needs_dev = false;
	tier = [];
	loaded = false;
	sleeping = true;
	can_sleep = true;
	cache = false;
	preload_images = null as string[]
	serviced = false;
	service = null as any;
	icon_html = [] as any[];
	icon_method = null as Function;
	icon_color = '#CCCCCC';
	padding : {
		left: number,
		right: number,
		top: number,
		bottom: number,
	};
	padding_full : {
		left: number,
		right: number,
		top: number,
		bottom: number,
	};
	show = {
		contracts: true,
		manifests: false,
		fleet: true,
		plans: false,
	}
	events: AppEvents = new AppEvents();
	child_routes = [] as {
		name: string,
		path: string
	}[]
	theme_mode = null as string;
	loaded_state = {} as any;
	open_path = null as string;

	constructor(init?:Partial<AppInfo>) {
        Object.assign(this, init);
		this.uid = this.vendor + '_' + this.ident;
	}

	state_set(path :string[], value :any) {
		const deepen = (obj :any,  depth :number) :boolean => {
			if(!Object.keys(obj).includes(path[depth])) {
				obj[path[depth]] = {};
			}
			if(depth == path.length - 1) {
				if(obj[path[depth]] != value)
					obj[path[depth]] = value;
				return true;
			} else {
				return deepen(obj[path[depth]], depth + 1);
			}
		}
		if(deepen(this.loaded_state, 0)) {
			localStorage.setItem('store_' + this.vendor + '_' + this.ident, JSON.stringify(this.loaded_state));
			console.debug("Saved state for " + this.name, path, this.loaded_state);
		}
	}

	state_get(path :string[], def: any = null) {
		if(path != null) {
			const deepen = (obj :any,  depth :number) => {
				const keys = Object.keys(obj);
				if(!keys.includes(path[depth])) {
					if(depth == path.length - 1) {
						obj[path[depth]] = def;
					} else {
						obj[path[depth]] = {};
					}
				}
				if(depth == path.length - 1) {
					return obj[path[depth]];
				} else {
					return deepen(obj[path[depth]], depth + 1);
				}
			}
			return deepen(this.loaded_state, 0);
		} else {
			if(def) {
				this.loaded_state = Eljs.merge_deep(def, this.loaded_state);
			}
			return this.loaded_state;
		}
	}

	state_load(os :any) {
		//console.debug("Loading state for " + this.vendor + '_' + this.ident)
		const loadedStoreJSON = localStorage.getItem('store_' + this.vendor + '_' + this.ident);
		if(loadedStoreJSON){
			try{
				const loadedStore = JSON.parse(loadedStoreJSON, os.json_parser);
				Eljs.merge_deep(this.loaded_state, loadedStore);
				console.debug("Loaded state for " + this.vendor + '_' + this.ident, this.loaded_state, loadedStore)

				//if(this.loaded_state.filters && loadedStore.filters) {
				//	console.debug("Extract state for " + this.vendor + '_' + this.ident, this.loaded_state.filters.companies, loadedStore.filters.companies)
				//}

			} catch (e) {
				console.debug("Invalid state for " + this.vendor + '_' + this.ident, e);
			}
		}// else {
			//console.debug("Empty state for " + this.vendor + '_' + this.ident);
		//}
	}

	state_reset() {
		this.loaded_state = {};
		localStorage.setItem('store_' + this.vendor + '_' + this.ident, JSON.stringify(this.loaded_state));
		console.debug("Reset state for " + AppEvents.name, this.loaded_state);
	}

	navigate(comp :Vue, to :string) :string {
		let App = null as AppInfo;
		let RouteIdent = '';
		const AppIdent = this.vendor + '_' + this.ident;
		if(!App) {
			if(to.startsWith(AppIdent)) {
				App = this;
				RouteIdent = AppIdent;
				this.child_routes.forEach(cr => {
					if(to == AppIdent + '_' + cr){
						App = this;
						RouteIdent = AppIdent + '_' + cr;
					}
				});
				//if(AppIdent == to) {
				//	if(this.child_routes.length) {
				//		App = this;
				//		RouteIdent = AppIdent + '_' + this.child_routes[0];
				//	}
				//}
				this.nav_class = RouteIdent;
				return RouteIdent;
			}
		}
		return null as string;
	}
}