import { Route } from 'vue-router'

export enum AppType {
	GENERAL, // 0
	HOME, // 1
	SLEEP, // 2
	LOCKED, // 3
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
	open() {
	}
	close() {
	}
	constructor(init?:Partial<AppEvents>) {
        Object.assign(this, init);
    }
}

export class AppInfo {
	app_name: string;
	app_url: string;
	app_vendor: string;
	app_ident: string;
	app_nav_class: string;
	app_type: AppType = AppType.GENERAL;
	app_windowed = false;
	app_visible = true;
	app_isexternal = false;
	app_needs_dev = false;
	app_tier = [];
	app_loaded = false;
	app_sleeping = true;
	app_can_sleep = true;
	app_cache = false;
	app_preload_images = null as string[]
	app_serviced = false;
	app_service = null as any;
	app_icon_html = [] as any[];
	app_icon_method = null as Function;
	app_icon_color = '#CCCCCC';
	app_events: AppEvents = new AppEvents();
	app_child_routes: string[] = [];
	app_theme_mode = null as string;
	app_theme: {
		bright: {
			status: StatusType,
			nav: NavType,
			shaded: boolean,
		},
		dark: {
			status: StatusType,
			nav: NavType,
			shaded: boolean,
		}
	};
	app_theme_option: {
		bright: {
			status: StatusType,
			nav: NavType,
			shaded: boolean,
		},
		dark: {
			status: StatusType,
			nav: NavType,
			shaded: boolean,
		}
	} = null;
	app_theme_override: {
		bright: {
			status: StatusType,
			nav: NavType,
			shaded: boolean,
		},
		dark: {
			status: StatusType,
			nav: NavType,
			shaded: boolean,
		}
	} = null;
	loaded_state = null as any;
	constructor(init?:Partial<AppInfo>) {

		this.app_theme = {
			bright: {
				status: StatusType.DARK,
				nav: NavType.DARK,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		};

        Object.assign(this, init);
	}

	private cipher(salt :string) {
		const textToChars = (text :string) => text.split('').map(c => c.charCodeAt(0));
		const byteHex = (n :any) => ("0" + Number(n).toString(16)).substr(-2);
		const applySaltToChar = (code :any) => textToChars(salt).reduce((a,b) => a ^ b, code);

		return (text :string) => text.split('')
			.map(textToChars)
			.map(applySaltToChar)
			.map(byteHex)
			.join('');
	}

	private decipher(salt :string) {
		const textToChars = (text :string) => text.split('').map(c => c.charCodeAt(0));
		const applySaltToChar = (code :any) => textToChars(salt).reduce((a,b) => a ^ b, code);
		return (encoded :any) => encoded.match(/.{1,2}/g)
			.map((hex :any) => parseInt(hex, 16))
			.map(applySaltToChar)
			.map((charCode :any) => String.fromCharCode(charCode))
			.join('');
	}

	StateLoad() {
		const decipher = this.decipher('f7e9b278-f982-4c40-8a32-22f28eb6f8dc');
		const loadedStore = localStorage.getItem('store_' + this.app_vendor + '_' + this.app_ident);
		if(loadedStore){
			try{
				//this.loaded_state = JSON.parse(decipher(loadedStore));
				this.loaded_state = JSON.parse(loadedStore);
			} catch {
				console.log("Inavlid store for " + this.app_vendor + '_' + this.app_ident);
			}
		}
	}

	StateSave(structure :any) {
		const cipher = this.cipher('f7e9b278-f982-4c40-8a32-22f28eb6f8dc');
		//localStorage.setItem('store_' + this.app_vendor + '_' + this.app_ident, cipher(JSON.stringify(structure)));
		localStorage.setItem('store_' + this.app_vendor + '_' + this.app_ident, JSON.stringify(structure));
	}

	SetThemeOverride(comp :Vue, structure :any) {
		this.app_theme_override = structure;
		comp.$emit('themechange', structure);
	}

	SetThemeOption(comp :Vue, structure :any) {
		this.app_theme_option = structure;
		comp.$emit('themechange', structure);
	}

	Navigate(comp :Vue, to :string) {
		let App = null as AppInfo;
		let RouteIdent = '';
		const AppIdent = this.app_vendor + '_' + this.app_ident;
		if(!App) {
			if(to.startsWith(AppIdent)) {
				App = this;
				RouteIdent = AppIdent;
				this.app_child_routes.forEach(cr => {
					if(to == AppIdent + '_' + cr){
						App = this;
						RouteIdent = AppIdent + '_' + cr;
					}
				});
				if(AppIdent == to) {
					if(this.app_child_routes.length) {
						App = this;
						RouteIdent = AppIdent + '_' + this.app_child_routes[0];
					}
				}
				this.app_nav_class = RouteIdent;
				return RouteIdent;
			}
		}
		return null;
	}
}

const Apps: Array<AppInfo> = [

	new AppInfo({
		app_vendor: "p42",
		app_ident: "onboarding",
		app_name: "",
		app_url: "/onboarding",
		app_type: AppType.HOME,
		app_visible: false,
		app_cache: true,
		app_theme: {
			bright: {
				status: StatusType.NONE,
				nav: NavType.NONE,
				shaded: false,
			},
			dark: {
				status: StatusType.NONE,
				nav: NavType.NONE,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "home",
		app_name: "Home",
		app_url: "/",
		app_cache: true,
		app_type: AppType.HOME,
		app_theme: {
			bright: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "sleep",
		app_name: "Sleep",
		app_url: "/z",
		app_cache: true,
		app_type: AppType.SLEEP,
		app_theme: {
			bright: {
				status: StatusType.NONE,
				nav: NavType.NONE,
				shaded: false,
			},
			dark: {
				status: StatusType.NONE,
				nav: NavType.NONE,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "locked",
		app_name: "Locked",
		app_url: "/l",
		app_cache: true,
		app_type: AppType.LOCKED,
		app_theme: {
			bright: {
				status: StatusType.NONE,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.NONE,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "contrax",
		app_name: "Contrax",
		app_url: "/contrax",
		app_icon_color: '#4D4D4F',
		app_cache: true,
		app_type: AppType.GENERAL,
		app_theme: {
			bright: {
				status: StatusType.DARK,
				nav: NavType.DARK,
				shaded: true,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: true,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "flyr",
		app_name: "Strip",
		app_url: "/flyr",
		app_icon_color: '#ED1949',
		app_cache: true,
		app_visible: false,
		//app_needs_dev: true,
		app_type: AppType.GENERAL,
		app_theme: {
			bright: {
				status: StatusType.BRIGHT,
				nav: NavType.DARK,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.DARK,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "conduit",
		app_name: "Conduit",
		app_url: "/conduit",
		app_icon_color: '#FFFFFF',
		app_cache: true,
		app_can_sleep: false,
		app_type: AppType.GENERAL,
		app_theme: {
			bright: {
				status: StatusType.DARK,
				nav: NavType.BRIGHT,
				shaded: true,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: true,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "yoflight",
		app_name: "yoFlight",
		app_url: "/yoflight",
		app_icon_color: '#000073',
		app_cache: true,
		app_can_sleep: false,
		app_type: AppType.GENERAL,
		app_icon_method: (comp :Vue, app :AppInfo) => {
			const content = [
				{
					class: 'full',
					styles: [
						"background-image:url(" + require('../../apps/p42/yoflight/icon_1.svg') + ")",
						"transform:rotate(" + comp.$root.$data.state.services.simulator.location.Hdg + "deg)"
					]
				}
			]
			app.app_icon_html = content;
		},
		app_theme: {
			bright: {
				status: StatusType.DARK,
				nav: NavType.DARK,
				shaded: true,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: true,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "aeroservice",
		app_name: "Aero Service",
		app_url: "/aeroservice",
		app_tier: [
			'endeavour'
		],
		app_icon_color: '#EADCE4',
		app_can_sleep: false,
		app_visible: false,
		app_needs_dev: true,
		app_type: AppType.GENERAL,
		app_child_routes: [
			'overview',
		],
		app_theme: {
			bright: {
				status: StatusType.DARK,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "holdings",
		app_name: "Holdings",
		app_url: "/holdings",
		app_tier: [
			'prospect',
			'endeavour'
		],
		app_icon_color: '#669A49',
		app_type: AppType.GENERAL,
		app_child_routes: [
			'activity',
			'statements',
		],
		app_theme: {
			bright: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "progress",
		app_name: "Progress",
		app_url: "/progress",
		app_tier: [
			'prospect',
			'endeavour'
		],
		app_icon_color: '#F9B618',
		app_type: AppType.GENERAL,
		app_serviced: true,
		app_child_routes: [
			'overview',
		],
		app_theme: {
			bright: {
				status: StatusType.DARK,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "settings",
		app_name: "Settings",
		app_url: "/settings",
		app_icon_color: '#2B2B2D',
		app_type: AppType.GENERAL,
		app_child_routes: [
			'updates',
			'display',
			'region',
			'config',
			'tier',
			'tier_change',
			'legal',
		],
		app_theme: {
			bright: {
				status: StatusType.DARK,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "feedback",
		app_name: "Feedback",
		app_url: "/feedback",
		app_icon_color: '#EDE7DF',
		app_type: AppType.GENERAL,
		app_can_sleep: false,
		app_visible: false,
		app_theme: {
			bright: {
				status: StatusType.DARK,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.DARK,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "discord",
		app_name: "Discord",
		app_url: "https://tfdidesign.com/chat",
		app_icon_color: '#7289da',
		app_type: AppType.GENERAL,
		app_isexternal: true,
		app_needs_dev: true,
	}),
	new AppInfo({
		app_vendor: "p42",
		app_ident: "scenr",
		app_name: "Scenr",
		app_url: "/scenr",
		app_icon_color: '#080808',
		app_visible: false,
		app_needs_dev: true,
		app_windowed: true,
		app_cache: true,
		app_can_sleep: false,
		app_type: AppType.GENERAL,
		app_theme: {
			bright: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false,
			}
		}
	}),
];


export default Apps;