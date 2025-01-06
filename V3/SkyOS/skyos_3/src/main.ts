import Vue from 'vue'
import Hardware from './sys/foundation/hw_frame.vue'
import { RouteConfig, Route } from 'vue-router'
import router from './sys/router'
import Apps from "./sys/foundation/app_bundle"
import { AppInfo, AppType, StatusType, NavType } from "./sys/foundation/app_model"
import Eljs from '@/sys/libraries/elem';
import RegisterComponent from "./sys/foundation/os_ui_components"
import ChangeLog from './sys/components/changelog/changes.json';

Vue.config.productionTip = false;

// Moment
import moment from 'moment'
Vue.prototype.moment = moment
moment.locale('en-gb');


// Vue Axios
import axios from 'axios'
import VueAxios from 'vue-axios'
Vue.use(VueAxios, axios)

// OS Plugin
import OS_Plugin from './sys/services/os';
Vue.use(OS_Plugin);


RegisterComponent();

// Dynamically load routes
Apps.forEach((app: AppInfo) => {

	if(!app.isexternal) {
		const childs: RouteConfig[] = [];

		// Load child routes
		app.child_routes.forEach((route, index) => {
			childs.push({
				path: route.path,
				name: app.vendor + '_' + app.ident + "_" + route.name,
				component: () => import('./apps/' + app.vendor + '/' + app.ident + '/views/' + route.name + '.vue'),
				props: {
					app: app,
					appName: app.vendor + '_' + app.ident,
				},
				meta: {
					app: app,
					appName: app.vendor + '_' + app.ident,
				},
			});
			//console.log("Added child route: " + app.vendor + '_' + app.ident + "_" + route);
		});

		// Load routes
		router.addRoute({
			path: app.url,
			name: app.vendor + '_' + app.ident,
			components: {
				load: () => import('@/apps/' + app.vendor + '/' + app.ident + '/load.vue'),
				map: () => import('@/apps/' + app.vendor + '/' + app.ident + '/map.vue'),
			},
			props: {
				load: {
					app: app,
					appName: app.vendor + '_' + app.ident,
				},
				map: {
					app: app,
					appName: app.vendor + '_' + app.ident,
				}
			},
			meta: {
				app: app,
				appName: app.vendor + '_' + app.ident,
			},
			children: childs,
		});
	}

});

Vue.mixin({
	methods: {

	},
})

new Vue({
  router,
  render: h => h(Hardware),
  data() {
	  	return {
			apps: Apps,
			sys: null as AppInfo,
			state: {
				device: {
					windowed: false,
					frameless: false,
					state_loaded: false,
					is_electron: false,
					is_landscape: false,
					is_apple_webkit: false,
					hardware: null,
				},
				ui: {
					is_1142: false,
					sleep_timer: null,
					status: StatusType.DARK,
					nav: NavType.DARK,
					tutorials: false,
					sun_position: [0,0],
				}
			},
	  	};
  	},
 	methods: {

		configure() {

			if(this.sys.state_get(['ui','changelog','show'])) {
				const changelog_last = new Date(ChangeLog.entries[0].date);
				if(changelog_last.getTime() != new Date(this.sys.state_get(['ui','changelog','last'])).getTime()) {
					this.sys.state_set(['ui','changelog','last'], changelog_last.getTime());
					this.$os.modals.add({
						type: 'changelog'
					});
				}
			}

			// Notify Electron
			if(this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'stow-auto', this.sys.state_get(['ui','stow','auto']));
				this.$os.api.ipcR.send('asynchronous-message', 'stow-can', this.sys.state_get(['ui','stow','can']));
				this.$os.api.ipcR.send('asynchronous-message', 'pin', this.sys.state_get(['ui','topmost']));
			}

			this.state.device.state_loaded = true;
			this.$os.analytics.SetGA();
		},

		routestate_load() {
			const loadedStore = localStorage.getItem('store_system_route');
			if(loadedStore){
				try {
					const loaded_state = JSON.parse(loadedStore, Eljs.json_parser);
					if(new Date().getTime() - loaded_state.last < 7.2e+6) {
						this.$os.routing.activeAppRouterRestoreURL = loaded_state.route;
					} else {
						this.$os.routing.activeAppRouterRestoreURL = null;
					}
				} catch {
					console.log("Inavlid store for store_system_route");
				}
			} else {
				this.$os.userConfig.set(['ui', 'changelog_last'], new Date());
			}
		},
		routestate_save(route :Route) {
			localStorage.setItem('store_system_route', JSON.stringify({
				last: new Date().getTime(),
				route: route.path,
			}));
		},

		changeSize(change :string) {
			if(this.state.device.is_electron) {
				this.state.device.hardware.changeSize(change);
			}
		},
		setTopmost(state :boolean) {
			if(this.state.device.is_electron && this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'pin', state);
			}
		},

		setAPIState(type: string, newValue :boolean) {
			if(type == 'local') {
				this.$os.api.connectedLocal = newValue;
				if(this.$os.api.connectedLocal || this.$os.api.connectedRemote) {
					this.$os.api.connected = true;
				} else {
					this.$os.api.connected = false;
				}
			} else if(type == 'remote') {
				this.$os.api.connectedRemote = newValue;
				if(this.$os.api.connectedLocal || this.$os.api.connectedRemote) {
					this.$os.api.connected = true;
				} else {
					this.$os.api.connected = false;
				}
			}
		},

		setWindowFormat(state :string) {
			this.$os.userConfig.set(['device','frameless'], true);
			if(this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'format-set', state);
			}
		},

		setWindowResize(state :boolean) {
			if(this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'resize', state);
			}
			this.$os.userConfig.set(['device','windowed'], state);
			this.$os.userConfig.set(['device','frameless'], false);
		},

		setWindowFramed(state :boolean) {
			if(this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'framed', state);
			}
		},

		setWindowStowCan(state :boolean) {
			this.$os.userConfig.set(['ui','stow','can'], state);
			if(this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'stow-can', state);
			}
		},

		setWindowStowAuto(state :boolean) {
			this.$os.userConfig.set(['ui','stow','auto'], state);
			if(this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'stow-auto', state);
			}
		},

		wake(){
			if(!this.state.ui.tutorials) {
				if(this.$route.name != 'p42_sleep') {
					this.$os.routing.goTo({ name: 'p42_sleep' });
				} else {
					this.unlock();
				}
			}
		},

		unlock() {
			const history = this.$os.routing.getAppAllHistory().filter(x => x.type != AppType.SLEEP && x.type != AppType.LOCKED);
			if(history.length > 1 && history[history.length - 1] != this.$os.routing.activeApp) {
				const lastApp = history[history.length - 1];
				this.$os.routing.goTo({ name: lastApp.vendor + "_" + lastApp.ident }, true);
			} else {
				this.$os.routing.goTo({ name: 'p42_home' });
			}
		},

		reset() {

		},


		getMap(app: AppInfo) {
			try {
				return require('@/apps/' + app.vendor + '/' + app.ident + '/map.vue');
			} catch (e) {
				return null;
			}
		},

		getIcon(app: AppInfo) {
			try {
				return require('@/apps/' + app.vendor + '/' + app.ident + '/icon.svg');
			} catch (e) {
				return require('@/sys/assets/framework/icon_default.svg');
			}
		},

		preloadIcons(app: AppInfo) {

			// Preload icon
			const icon = new Image();
			icon.src = this.getIcon(app);

			// Preload images
			if(app.preload_images) {
				const images = [];
				app.preload_images.forEach((url, index) => {
					images[index] = new Image();
					images[index].src = url;
				});
			}
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.$os.userConfig.set(['ui','tier'], wsmsg.payload.set.tier);
							break;
						}
					}
					break;
				}
				//case 'weather': {
				//	switch(wsmsg.name[1]){
				//		case 'get': {
				//			switch(wsmsg.name[2]){
				//				case 'icao': {
				//					wsmsg.payload.forEach((station: any) => {
				//						this.state.services.weather[station.Station] = {
				//							Time: new Date(),
				//							Data: station
				//						}
				//					});
				//					break;
				//				}
				//			}
				//			break;
				//		}
				//	}
				//	break;
				//}
			}
		},

		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'wake': {
					this.wake();
					break;
				}
				case 'unlock': {
					this.unlock();
					break;
				}
			}
		}
	},
	beforeMount() {

		this.$os.apps = this.apps;
		this.apps.forEach(app => {
			app.state_load(this.$os);
		});

		this.sys = this.$os.apps.find(x => x.vendor == 'p42' && x.ident == 'system');

		if (navigator.userAgent.match(/(AppleWebKit)/i)) { this.state.device.is_apple_webkit = true; }

		let codeCumul = "";
		let codeTO = null as any;
		document.onkeypress = (e) => {
			clearTimeout(codeTO);
			codeTO = setTimeout(() => {
				clearTimeout(codeTO);
				codeCumul = "";
				codeTO = null;
			}, 500);

			codeCumul += e.key;
			switch(codeCumul) {
				case '1142': {
					this.$os.userConfig.set(['ui','is_1142'], true);
					break;
				}
			}
		};

		document.onclick = (e) => {
			if(this.$os.api.ipcR) {
				this.$os.api.ipcR.send('asynchronous-message', 'tap');
			}
		}

		// Init Config
		this.configure();
		this.routestate_load();

		// Load Services
		Apps.forEach((app: AppInfo) => {
			this.preloadIcons(app);
			if(app.serviced){
				app.service = new (require('@/apps/' + app.vendor + '/' + app.ident + '/service.ts')).default(this, app);
			}
		});

	},
	mounted() {

		this.$os.routing.activeAppRouter = this.$route;
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('os', this.listener_os);

		// Run before each routing
		this.$router.beforeEach((to :Route, from :Route, next :any) => {

			if(from.meta.app)
				from.meta.app.sleeping = true;

			if(to.matched.length == 1)
				this.$os.system.set_cover(false);

			this.$os.eventsBus.Bus.emit('navigate', { name: 'to_pre', payload: to.meta.app, route: to });

			//if(this.config.onboarding.has_setup) {
			//	if(from.meta.app) {
			//		from.meta.app.sleeping = true;
			//		const routeIdent = to.meta.app.navigate(this, to.name);
			//		if(to.name != routeIdent) {
			//			next({ name: routeIdent });
			//			return;
			//		} else {
			//			to.meta.app.sleeping = false;
			//			to.meta.app.events.open();
			//		}
			//	}
			//} else {
				//switch(to.name) {
				//	case 'p42_sleep':
				//	case 'p42_onboarding': {
				//		next();
				//		break
				//	}
				//	default: {
				//		next({ name: 'p42_onboarding' });
				//		break;
				//	}
				//}
			//}

			this.$os.analytics.TrackPage(to.path, to.name);

			//if (navigator.userAgent.indexOf('Electron') >= 0 && to.meta.app) {
			//	this.$os.userConfig.set(['device','windowed'], to.meta.app.windowed);
			//	if(to.meta.app.windowed){
			//		this.setWindowResize(true);
			//	} else {
			//		this.setWindowResize(false);
			//	}
			//}

			next();
		})

		this.$router.beforeResolve((to, from, next) => {

			next();
		})

		// Run after each routing
		this.$router.afterEach((to :Route, from :Route) => {

			//if(to.matched.length > 1) {
				//this.$os.system.setSidebar(false);
			//} else {
			//	this.$os.system.setSidebar(true);
			//}

			console.log('New Location: ' + to.fullPath);
			to.meta.app.sleeping = false;
			this.routestate_save(to);
		});

		// Where do we go first (on load)
		if(this.$os.userConfig.get(['onboarding','has_setup'])) {
			if(this.state.device.is_electron){
				if(this.$os.routing.activeAppRouterRestoreURL){
					this.$os.routing.goTo({ path: this.$os.routing.activeAppRouterRestoreURL }, true);
				} else {
					this.$os.routing.goTo({ name: 'p42_home' });
				}
			}
		} else {
			this.$os.routing.goTo({ name: 'p42_home' });
		}


		//this.$os.modals.add({
		//	type: 'call',
		//	data: {
		//		Title: "Very Nice!",
		//		Param: 'adventure:introtrainingflight_jeff/jeff_manifest_training',
		//		URL: "https://storage.googleapis.com/gilfoyle/the-skypark/v1/common/sounds/characters/brigit/load_turnaround.mp3",
		//		Meta: "https://storage.googleapis.com/gilfoyle/the-skypark/v1/common/sounds/characters/brigit/load_turnaround.json"
		//	}
		//});

	},
}).$mount('#app')
