import Vue from 'vue'
import Hardware from './sys/foundation/hw_frame.vue'
import { RouteConfig, Route } from 'vue-router'
import router from './sys/router'
import Apps, { AppInfo, AppType, StatusType, NavType } from "./sys/foundation/app_bundle"
import RegisterComponent from "./sys/foundation/os_ui_components"
import Notification from '@/sys/models/notification'
import ChangeLog from './sys/components/changelog/changes.json';
import VueCompositionAPI from '@vue/composition-api'

Vue.use(VueCompositionAPI);

Vue.config.productionTip = false;

// Moment
import moment from 'moment'
Vue.prototype.moment = moment
moment.locale('en-gb');

// Vue Axios
import axios from 'axios'
import VueAxios from 'vue-axios'
Vue.use(VueAxios, axios)

// Auth Plugin
import Auth_Plugin from './sys/services/auth';
Vue.use(Auth_Plugin);

// API Plugin
import API_Plugin from './sys/services/api';
Vue.use(API_Plugin);

// OS Plugin
import OS_Plugin from './sys/services/os';
Vue.use(OS_Plugin);

// Local WS Plugin
import LWS_Plugin from './sys/services/ws-local';
Vue.use(LWS_Plugin);

// Remove WS Plugin
import RWS_Plugin from './sys/services/ws-remote';
Vue.use(RWS_Plugin);


// Contract Mutator Plugin
import Contract_Mutator from './sys/mutators/contract-mutator';
Vue.use(Contract_Mutator);


// Color Seek Plugin
import ColorSeek_Plugin from './sys/services/color-seek';
import Eljs from './sys/libraries/elem'
Vue.use(ColorSeek_Plugin);

RegisterComponent();

// Dynamically load routes
Apps.forEach((app: AppInfo) => {

	if(!app.app_isexternal) {
		const childs: RouteConfig[] = [];

		// Load child routes
		app.app_child_routes.forEach((route, index) => {
			childs.push({
				path: route,
				name: app.app_vendor + '_' + app.app_ident + "_" + route,
				//component: () => ({
				//	// The component to load (should be a Promise)
				//	component: import('./MyComponent.vue'),
				//	// A component to use while the async component is loading
				//	loading: LoadingComponent,
				//	// A component to use if the load fails
				//	error: ErrorComponent,
				//	// Delay before showing the loading component. Default: 200ms.
				//	delay: 200,
				//	// The error component will be displayed if a timeout is
				//	// provided and exceeded. Default: Infinity.
				//	timeout: 3000
				//  }),
				component: () => import('./apps/' + app.app_vendor + '/' + app.app_ident + '/views/' + route + '.vue'),
				props: {
					app: app,
					appName: app.app_vendor + '_' + app.app_ident,
				},
				meta: {
					app: app,
					appName: app.app_vendor + '_' + app.app_ident,
				},
			});
			//console.log("Added child route: " + app.app_vendor + '_' + app.app_ident + "_" + route);
		});

		// Load routes
		router.addRoute({
			path: app.app_url,
			name: app.app_vendor + '_' + app.app_ident,
			component: () => import('./apps/' + app.app_vendor + '/' + app.app_ident + '/load.vue'),
			props: {
				app: app,
				appName: app.app_vendor + '_' + app.app_ident,
			},
			meta: {
				app: app,
				appName: app.app_vendor + '_' + app.app_ident,
			},
			children: childs,
		});
	}

});

new Vue({
  router,
  render: h => h(Hardware),
  data() {
	  	return {
			apps: Apps,
			ipcR: null,
			services: {
				api: this.$apiService,
				auth: this.$authService,
				colorSeek: this.$colorSeek,
				wsLocal: this.$wsLocalService,
				wsRemote: this.$wsRemoteService
			},
			state: {
				device: {
					windowed: false,
					frameless: false,
					stateLoaded: false,
					isElectron: false,
					isLandscape: false,
					isAppleWebkit: false,
					hardware: null,
				},
				services: {
					api: {
						connected: false,
						connectedLocal: false,
						connectedRemote: false,
					},
					weather: { },
					simulator: {
						connected: false,
						live: false,
						name: '',
						version: '',
						aircraft: {
							name: '',
							manufacturer: '',
							model: '',
						},
						locationHistory: {
							name: '',
							costPerNM: 0,
							location: [0,0],
						},
						location: {
							Lon: 0,
							Lat: 0,
							Alt: 0,
							GAlt: 0,
							MagVar: 0,
							GS: 0,
							TR: 0,
							Hdg: 0,
							CRS: 0,
							FPM: 0,
							SimTimeZulu: null,
							SimTimeOffset: 0,
						},
						autopilot: {
							On: false,
							HdgOn: false,
							Hdg: 0,
						}
					},
					userProgress: {
						ReliabilityUnlock: 0,
						Reliability: {
							Balance: null,
							StartDate: null,
							Trend: null,
						},
						Bank: {
							Balance: null,
							StartDate: null,
							Trend: null,
						},
						Karma: {
							Balance: null,
							StartDate: null,
							Trend: null,
						},
						XP: {
							Balance: null,
							StartDate: null,
							Trend: null,
						},
					},
					imagesHue: {
						cache: []
					},
					notifications: {
						lastFetch: 0,
						list: []
					}
				},
				ui: {
					is1142: false,
					sleepTimer: null,
					status: StatusType.DARK,
					nav: NavType.DARK,
					tutorials: false,
					sunPosition: [0,0],
					modals: {
						queue: [] as any[],
					}
				}
			},
			config: {
				services: {
					wsRemote: {
						room: '',
					},
					ga: {
						tag: '',
					},
					userProgress: {
						ReliabilityExpire: 0,
					}
				},
				ui: {
					tier: 'endeavour',
					topmost: true,
					framed: false,
					stowCan: true,
					stowAuto: true,
					device_resize: {
						selected: 1,
						size_1: 0.9,
						size_2: 1,
					},
					changelogLast: null,
					changelogShow: true,
					theme: 'theme--dark',
					themeAuto: true,
					sleepTimer: 300000,
					sleepTimerLocked: 15000,
					wallpaper: "/assets/wallpapers/%theme%/0.jpg",
					wallpaperLocked: "/assets/wallpapers/1.jpg",
				},
				onboarding: {
					hasSetup: false,
					hasRequested: false,
				},
			},
			defaultConfig: null,
	  	};
  	},
 	methods: {

		statePrep() {
			this.defaultConfig = JSON.parse(JSON.stringify(this.config))
		},
		stateLoad() {
			if(!this.defaultConfig) {
				this.statePrep();
			}

			const loadedStore = localStorage.getItem('store_system');
			if(loadedStore){
				try {

					const loaded_state = JSON.parse(loadedStore);

					if(loaded_state.config.services) {
						Object.assign(this.config.services, loaded_state.config.services);
					}

					if(loaded_state.config.ui) {
						Object.assign(this.config.ui, loaded_state.config.ui);
					}

					if(loaded_state.config.onboarding) {
						Object.assign(this.config.onboarding, loaded_state.config.onboarding);
					}

					if(this.config.ui.changelogShow) {
						const changeLogLast = new Date(ChangeLog.entries[0].date);
						if(changeLogLast.getTime() != new Date(this.config.ui.changelogLast).getTime()) {
							this.config.ui.changelogLast = changeLogLast.getTime();
							this.$os.modalPush({
								type: 'changelog'
							});
							setTimeout(() => {
								this.stateSave();
							}, 200);
						}
					}

				} catch (error) {
					console.error(error);
					console.log("Inavlid store for store_system");
				}
			}

			// Notify Electron

			if(this.ipcR) {
				this.ipcR.send('a-msg', 'stow-auto', this.config.ui.stowAuto);
				this.ipcR.send('a-msg', 'stow-can', this.config.ui.stowCan);
				this.ipcR.send('a-msg', 'pin', this.config.ui.topmost);
			}

			this.preloadWallpapers();
			this.state.device.stateLoaded = true;
			this.$os.SetGA();
		},
		stateSave() {
			if(this.state.device.stateLoaded) {
				localStorage.setItem('store_system', JSON.stringify({
					config: {
						ui: this.config.ui,
						services: this.config.services,
						onboarding: this.config.onboarding
					}
				}));
			} else {
				console.log("Failed to save store_system. Trying to save before restore.")
			}
		},

		routeStateLoad() {
			const loadedStore = localStorage.getItem('store_system_route');
			if(loadedStore){
				try {
					const loaded_state = JSON.parse(loadedStore);
					if(new Date().getTime() - loaded_state.last < 7.2e+6) {
						this.$os.activeAppRouterRestoreURL = loaded_state.route;
					} else {
						this.$os.activeAppRouterRestoreURL = null;
					}
				} catch {
					console.log("Inavlid store for store_system_route");
				}
			} else {
				this.$os.setConfig(['ui', 'changelogLast'], new Date());
			}
		},
		routeStateSave(route :Route) {
			localStorage.setItem('store_system_route', JSON.stringify({
				last: new Date().getTime(),
				route: route.path,
			}));
		},

		changeSize(change :string) {
			if(this.state.device.isElectron) {
				this.state.device.hardware.changeSize(change);
			}
		},
		setTopmost(state :boolean) {
			if(this.state.device.isElectron && this.ipcR) {
				this.ipcR.send('a-msg', 'pin', state);
			}
		},

		setAPIState(type: string, newValue :boolean) {
			const services = this.state.services;
			if(type == 'local') {
				services.api.connectedLocal = newValue;
				if(services.api.connectedLocal || services.api.connectedRemote) {
					services.api.connected = true;
				} else {
					services.api.connected = false;
				}
			} else if(type == 'remote') {
				services.api.connectedRemote = newValue;
				if(services.api.connectedLocal || services.api.connectedRemote) {
					services.api.connected = true;
				} else {
					services.api.connected = false;
				}
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.services.api.SendWS('progress:get', {});
					this.$os.removeNotificationFromID(662);
					break;
				}
				case 'disconnect': {
					this.state.services.simulator.connected = false;
					this.state.services.simulator.live = false;
					this.state.services.simulator.name = '';
					this.state.services.simulator.version = '';
					this.$os.isDev = false;
					this.$os.clearAllNotifications();
					break;
				}
				case 'failsconnect': {
					if(this.$os.awaitsTransponderReboot) {
						if(this.$os.awaitsTransponderReboot.getTime() + 30000 < new Date().getTime()){ //300000
							this.$os.awaitsTransponderReboot = null;
						}
					}

					if(!this.$os.awaitsTransponderReboot) {
						this.$os.addNotification(new Notification({
							UID: 662,
							Type: 'Status',
							Time: new Date(),
							CanDismiss: false,
							Title: 'Transponder is not running',
							Message: "Make sure the Transponder is running to access your contracts, progress data and other functions of the Skypad.",
						}), false);
					}
					break;
				}
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.$os.awaitsTransponderReboot = null;
							this.config.ui.tier = wsmsg.payload.set.tier;
							break;
						}
						case 'restart': {
							this.$os.awaitsTransponderReboot = new Date();
							this.$os.addNotification(new Notification({
								UID: 662,
								Type: 'Status',
								Time: new Date(),
								CanDismiss: false,
								Title: 'Transponder is restarting',
								Message: "Access to your contracts, progress data and other functions of the Skypad will resume shortly.",
							}), false);
							break;
						}
					}
					break;
				}
				case 'progress': {
					switch(wsmsg.name[1]){
						case 'get': {
							this.state.services.userProgress = {
								ReliabilityUnlock: new Date(wsmsg.payload.ReliabilityUnlock).getTime(),
								Reliability: wsmsg.payload.Reliability,
								Bank: wsmsg.payload.Bank,
								Karma: wsmsg.payload.Karma,
								XP: wsmsg.payload.XP,
							};
							break;
						}
					}
					break;
				}
				case 'notifications': {
					switch(wsmsg.name[1]){
						case 'get': {
							wsmsg.payload.List.forEach(notif => {
								this.$os.addNotification(new Notification({
									UID: notif.UID,
									Type:  notif.Type,
									Time: new Date(notif.Time),
									Expire: notif.Expire ? new Date(notif.Expire) : null,
									CanOpen: notif.CanOpen,
									CanDismiss: notif.CanDismiss,
									LaunchArgument: notif.LaunchArgument,
									App: notif.App,
									Title: notif.Title,
									Message: notif.Message,
									Group: notif.Group,
									Data: notif.Data,
								}), false, false);
							});
							break;
						}
					}
					break;
				}
				case 'notification': {
					switch(wsmsg.name[1]){
						case 'add': {
							this.$os.addNotification(new Notification({
								UID: wsmsg.payload.UID,
								Type:  wsmsg.payload.Type,
								Time: new Date(wsmsg.payload.Time),
								Expire: wsmsg.payload.Expire ? new Date(wsmsg.payload.Expire) : null,
								CanOpen: wsmsg.payload.CanOpen,
								CanDismiss: wsmsg.payload.CanDismiss,
								LaunchArgument: wsmsg.payload.LaunchArgument,
								App: wsmsg.payload.App,
								Title: wsmsg.payload.Title,
								Message: wsmsg.payload.Message,
								Group: wsmsg.payload.Group,
								Data: wsmsg.payload.Data,
							}), false);
							break;
						}
						case 'remove': {
							this.$os.removeNotificationFromID(wsmsg.payload.UID, false);
							break;
						}
					}
					break;
				}
				case 'weather': {
					switch(wsmsg.name[1]){
						case 'get': {
							switch(wsmsg.name[2]){
								case 'icao': {
									wsmsg.payload.forEach((station: any) => {
										this.state.services.weather[station.Station] = {
											Time: new Date(),
											Data: station
										}
									});
									break;
								}
							}
							break;
						}
					}
					break;
				}
				case 'sim': {
					switch(wsmsg.name[1]){
						case 'status': {
							this.state.services.simulator.connected = wsmsg.payload.State;
							if(wsmsg.payload.State){
								this.state.services.simulator.name = wsmsg.payload.Name;
								this.state.services.simulator.version = wsmsg.payload.Version;
							} else {
								this.state.services.simulator.live = false;
								this.state.services.simulator.name = '';
								this.state.services.simulator.version = '';
							}
							break;
						}
					}
					break;
				}
				case 'locationhistory': {
					switch(wsmsg.name[1]){
						case 'latest': {
							const pl = wsmsg.payload;

							const svc_loc = this.state.services.simulator.locationHistory;
							svc_loc.name = pl.Name;
							svc_loc.costPerNM = pl.CostPerNM;
							svc_loc.location[0] = pl.Location[0];
							svc_loc.location[1] = pl.Location[1];

							break;
						}
					}
					break;
				}
				case 'eventbus': {
					switch(wsmsg.name[1]){
						case 'meta': {
							const pl = wsmsg.payload;

							const svc_loc = this.state.services.simulator.location;
							svc_loc.Lon = pl.Lon;
							svc_loc.Lat = pl.Lat;
							svc_loc.Hdg = pl.HDG;
							svc_loc.Alt = pl.Alt;
							svc_loc.GAlt = pl.GAlt;
							svc_loc.MagVar = pl.MagVar;
							svc_loc.TR = pl.TurnRate;
							svc_loc.GS = pl.GS;
							svc_loc.CRS = pl.CRS;
							svc_loc.FPM = pl.FPM;
							svc_loc.SimTimeZulu = new Date(pl.SimTimeZulu);
							svc_loc.SimTimeOffset = pl.SimTimeOffset;

							const svc_acf = this.state.services.simulator.aircraft;
							svc_acf.manufacturer = pl.Aircraft.Manufacturer;
							svc_acf.model = pl.Aircraft.Model;
							svc_acf.name = pl.Aircraft.Name;

							this.$os.setSunPosition(svc_loc.SimTimeZulu, pl.Lon, pl.Lat);
							this.state.services.simulator.live = (Eljs.round(svc_loc.Lat, 3) != 0 && Eljs.round(svc_loc.Lon, 3) != 0) || Math.round(svc_loc.GS) > 70;

							break;
						}
						case 'event': {
							wsmsg.payload.forEach((pl: any) => {
								switch(pl.Type) {
									case 'Autopilot': {
										this.state.services.simulator.autopilot.On = pl.Params.On;
										this.state.services.simulator.autopilot.HdgOn = pl.Params.HdgOn;
										this.state.services.simulator.autopilot.Hdg = pl.Params.Hdg;
										break;
									}
								}
							});
							break;
						}
					}
					break;
				}
			}
		},

		setWindowFormat(state :string) {
			this.$os.setState(['device','frameless'], true);
			if(this.ipcR) {
				this.ipcR.send('a-msg', 'format-set', state);
			}
		},

		setWindowResize(state :boolean) {
			if(this.ipcR) {
				this.ipcR.send('a-msg', 'resize', state);
			}
			this.$os.setState(['device','windowed'], state);
			this.$os.setState(['device','frameless'], false);
		},

		setWindowFramed(state :boolean) {
			if(this.ipcR) {
				this.ipcR.send('a-msg', 'framed', state);
			}
		},

		setWindowStowCan(state :boolean) {
			this.$os.setConfig(['ui','stowCan'], state);
			if(this.ipcR) {
				this.ipcR.send('a-msg', 'stow-can', state);
			}
		},

		setWindowStowAuto(state :boolean) {
			this.$os.setConfig(['ui','stowAuto'], state);
			if(this.ipcR) {
				this.ipcR.send('a-msg', 'stow-auto', state);
			}
		},

		wake(){
			if(!this.state.ui.tutorials) {
				if(this.$route.name != 'p42_sleep') {
					this.$router.push({name: 'p42_sleep'});
				} else {
					this.$router.push({name: 'p42_locked'});
				}
			}
		},

		unlock() {
			const history = this.$os.getAppAllHistory().filter(x => x.app_type != AppType.SLEEP && x.app_type != AppType.LOCKED);
			if(history.length > 1 && history[history.length - 1] != this.$os.activeApp) {
				const lastApp = history[history.length - 1];
				this.$router.push({ name: lastApp.app_vendor + "_" + lastApp.app_ident });
			} else {
				this.$router.push({ name: 'p42_home' });
			}
		},

		getIcon(app: AppInfo) {
			try {
				return require('@/apps/' + app.app_vendor + '/' + app.app_ident + '/icon.svg');
			} catch (e) {
				return require('@/sys/assets/framework/icon_default.svg');
			}
		},

		preloadWallpapers() {
			const images = [];
			const urls = [
				this.config.ui.wallpaper.replace('%theme%', 'theme--bright'),
				this.config.ui.wallpaperLocked.replace('%theme%', 'theme--bright'),
				this.config.ui.wallpaper.replace('%theme%', 'theme--dark'),
				this.config.ui.wallpaperLocked.replace('%theme%', 'theme--dark'),
			];

			urls.forEach((url, index) => {
				images[index] = new Image();
				images[index].src = url;
			});
		},

		preloadIcons(app: AppInfo) {

			// Preload icon
			const icon = new Image();
			icon.src = this.getIcon(app);

			// Preload images
			if(app.app_preload_images) {
				const images = [];
				app.app_preload_images.forEach((url, index) => {
					images[index] = new Image();
					images[index].src = url;
				});
			}
		}
	},
	beforeMount() {

		if (navigator.userAgent.match(/(AppleWebKit)/i)) { this.state.device.isAppleWebkit = true; }

		if (navigator.userAgent.indexOf('Electron') >= 0) {
			const { ipcRenderer } = window.require('electron')
			this.ipcR = ipcRenderer;
		}

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
					this.$os.setState(['ui','is1142'], true);
					break;
				}
			}
		};

		document.onclick = (e) => {
			if(this.ipcR) {
				this.ipcR.send('a-msg', 'tap');
			}
		}

		this.routeStateLoad();
		this.stateLoad();

		// Load Services
		Apps.forEach((app: AppInfo) => {
			this.preloadIcons(app);
			if(app.app_serviced){
				app.app_service = new (require('@/apps/' + app.app_vendor + '/' + app.app_ident + '/service.ts')).default(this, app);
			}
		});

	},
	mounted() {
		this.$router.beforeEach((to :Route, from :Route, next :any) => {

			if(this.config.onboarding.hasSetup) {
				if(from.meta.app) {
					from.meta.app.app_sleeping = true;
					const RouteIdent = to.meta.app.Navigate(this, to.name);
					if(to.name != RouteIdent) {
						next({ name: RouteIdent });
						return;
					} else {
						to.meta.app.app_sleeping = false;
						to.meta.app.app_events.open();
						next();
					}
				} else {
					next();
				}
			} else {
				switch(to.name) {
					case 'p42_locked':
					case 'p42_sleep':
					case 'p42_onboarding': {
						next();
						break
					}
					default: {
						next({ name: 'p42_onboarding' });
						break;
					}
				}
			}

			if (navigator.userAgent.indexOf('Electron') >= 0 && to.meta.app) {
				this.$os.setState(['device','windowed'], to.meta.app.app_windowed);
				if(to.meta.app.app_windowed){
					this.setWindowResize(true);
				} else {
					this.setWindowResize(false);
				}
			}

			this.$os.TrackPage(to.path, to.name);
		})

		if(this.config.onboarding.hasSetup) {
			if(this.state.device.isElectron){
				if(this.$os.activeAppRouterRestoreURL){
					this.$router.push(this.$os.activeAppRouterRestoreURL);
				} else {
					this.$router.push({name: 'p42_locked'});
				}
			}
		} else {
			this.$router.push({name: 'p42_locked'});
		}

		this.$os.activeAppRouter = this.$route;

		this.$router.afterEach((to :Route, from :Route) => {
			to.meta.app.app_sleeping = false;
			this.routeStateSave(to);
		});

		this.$on('ws-in', this.listenerWs);
		this.$on('wake', this.wake);
		this.$on('unlock', this.unlock);
	},
}).$mount('#app')
