import _Vue from 'vue';
import { AppInfo } from '../foundation/app_model';

import System from './extensions/system';
import EventsBus from './extensions/events-bus';
import Modals from './extensions/modals';
import Analytics from './extensions/analytics';
import UserConfig from './extensions/user-config';
import Routing from './extensions/routing';
import ScrollView from './extensions/scroll_views';
import Notifications from './extensions/notifications';
import Messaging from './extensions/messaging';
import Time from './extensions/time';
import Theme from './extensions/theme';
import ColorSeek from './extensions/color-seek';
import Auth from './extensions/auth';
import AudioPlayer from './extensions/audio';
import API from './extensions/api';
import Companies from './extensions/companies';
import Units from './extensions/units';
import Maps from './extensions/maps';
import WSLocal from './extensions/ws-local';
import WSRemote from './extensions/ws-remote';

import Simulator from './extensions/simulator';
import Progress from './extensions/progress';
import contract_service from './extensions/contract-service';
import PayloadMutator from './extensions/payload-mutator';
import FleetService from './extensions/fleet-service';
import Navigation from './extensions/navigation';
import AircraftTypes from './extensions/aircraft-types';

export class OS {
	private vue: _Vue;

	public apps: AppInfo[]
	public system: System;
	public eventsBus: EventsBus;
	public modals: Modals;
	public analytics: Analytics;
	public userConfig: UserConfig;
	public routing: Routing;
	public scrollView: ScrollView;
	public notifications: Notifications;
	public messaging: Messaging;
	public navigation: Navigation;
	public time: Time;
	public theme: Theme;
	public colorSeek: ColorSeek;
	public auth: Auth;
	public audioPlayer: AudioPlayer;
	public api: API;
	public companies: Companies;
	public aircraftTypes: AircraftTypes;
	public units: Units;
	public maps: Maps;
	public wsLocal: WSLocal;
	public wsRemote: WSRemote;

	public simulator: Simulator;
	public fleetService: FleetService;
	public progress: Progress;
	public contract_service: contract_service;
	public payloadMutator: PayloadMutator;

	public mounted() {
		this.system.startup();
		this.notifications.startup();
		this.api.startup();
		this.simulator.startup();
		this.fleetService.startup();
		this.audioPlayer.startup();
		this.progress.startup();
		this.scrollView.startup();
		this.contract_service.startup();
		this.navigation.startup();

		this.wsLocal.startup();
		this.wsRemote.startup();
	}

	public beforeCreate() {
	}

	public created() {
	}

	constructor(Vue: _Vue) {
		this.vue = Vue;

		this.system = new System(this, this.vue);

		this.eventsBus = new EventsBus(this, this.vue);
		this.wsLocal = new WSLocal(this, this.vue);
		this.wsRemote = new WSRemote(this, this.vue);
		this.auth = new Auth(this, this.vue);
		this.audioPlayer = new AudioPlayer(this, this.vue);
		this.api = new API(this, this.vue);
		this.companies = new Companies(this, this.vue);
		this.aircraftTypes = new AircraftTypes(this, this.vue);
		this.units = new Units(this, this.vue);

		this.modals = new Modals(this, this.vue);
		this.analytics = new Analytics(this, this.vue);
		this.userConfig = new UserConfig(this, this.vue);
		this.routing = new Routing(this, this.vue);
		this.scrollView = new ScrollView(this, this.vue);
		this.notifications = new Notifications(this, this.vue);
		this.messaging = new Messaging(this, this.vue);
		this.time = new Time(this, this.vue);
		this.theme = new Theme(this, this.vue);
		this.colorSeek = new ColorSeek(this, this.vue);
		this.maps = new Maps(this, this.vue);
		this.navigation = new Navigation(this, this.vue);

		this.simulator = new Simulator(this, this.vue);
		this.fleetService = new FleetService(this, this.vue);
		this.progress = new Progress(this, this.vue);
		this.contract_service = new contract_service(this, this.vue);
		this.payloadMutator = new PayloadMutator(this, this.vue);

	}

}

export default {
	install: (Vue: typeof _Vue) => { //, options?: any
		let created = false;
		let mounted = false;
		let installed = false;
		let os = null as OS;
		Vue.mixin({
			beforeCreate() {
				if (!installed) {
					installed = true;
					os = new OS(this);
					Vue.prototype.$os = os;
					os.beforeCreate();

					console.log(this);
				}
			},
			created() {
				if(!created) {
					created = true;
					os.created();
				}
			},
			mounted() {
				if(!mounted) {
					mounted = true;
					os.mounted();
				}
			}
		});
	}
};

declare module 'vue/types/vue' {
  interface Vue {
    $os: OS;
  }
}
