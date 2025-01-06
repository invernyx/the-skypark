import _Vue from 'vue';
import { AppInfo, AppType } from "@/sys/foundation/app_model"
import Apps from "@/sys/foundation/app_bundle"
import { Route } from 'vue-router'
import { Dictionary } from 'vue-router/types/router';

export default class Routing {
	public os: any;
	public vue: _Vue;

	public activeApp: AppInfo = null;
	public activeAppRouter: Route = null;
	public activeAppRouterRestoreURL: string = null;

	public appHistory: AppInfo[] = [];
	public appAllHistory: AppInfo[] = [];

	public setCurrentRoute(app :AppInfo, route :Route) {

		app.open_path = route.path;
		this.activeAppRouter = route;

		if(this.activeApp != app) {
			const app_out = this.activeApp;
			this.activeApp = app;
			this.os.eventsBus.Bus.emit('navigate', { name: 'app', payload: {
				in: app,
				out: app_out
			}, route: route });
		}

		//console.log("Navigating to", route);
		this.os.eventsBus.Bus.emit('navigate', { name: 'to', payload: app, route: route });

		const historyPush = () => {
			if(this.appHistory.includes(app)) {
				const index = this.appHistory.indexOf(app);
				this.appHistory.splice(index, 1);
			}
			if(this.appAllHistory.includes(app)) {
				const index = this.appAllHistory.indexOf(app);
				this.appAllHistory.splice(index, 1);
			}
			if(app.visible) {
				if(app.type == AppType.GENERAL) {
					this.appHistory.push(app);
				}
				this.appAllHistory.push(app);
			}
		}

		setTimeout(() => {
			//if(this.appHistory.length > 0) {
			//	if(this.appHistory[this.appHistory.length - 1] != app) {
			//		historyPush();
			//	}
			//} else {
				historyPush();
			//}
		}, 100);

	}

	public initAppHistory() {
		Apps.forEach(app => {
			if(app.visible) {
				if(app.type == AppType.GENERAL && !app.isexternal && app != this.activeApp){
					if(!this.appHistory.includes(app)) {
						this.appHistory.push(app);
					}
				}
			}
		});
	}

	public getAppAllHistory() {
		return this.appAllHistory;
	}

	public getAppHistory() {
		return this.appHistory;
	}

	public getAppIcon(app_vendor :string, app_ident :string) {
		try {
			return require('../../../apps/' + app_vendor + '/' + app_ident + '/icon.svg');
		} catch (e) {
			return require('../../../sys/assets/framework/icon_default.svg');
		}
	}

	public goTo(options :{ name? :string, path? :string, id? :any, params? :object }, restore = false) {

		const pushOption = {
			name: null,
			path: null,
			params: (options.params as Dictionary<string>)
		}

		if(options.path) {
			if(this.vue.$route.path != options.path)
				pushOption.path = options.path;
		} else {
			if(this.vue.$route.name != options.name)
				pushOption.name = options.name;
		}

		const currentRoute = this.vue.$router.currentRoute;
		const resolvedRoute = this.vue.$router.resolve(pushOption);

		if(!this.activeApp) {
			this.activeApp = resolvedRoute.route.meta.app;
			this.activeAppRouter = resolvedRoute.route;
		}

		if(currentRoute.fullPath != resolvedRoute.route.fullPath) {
			if(resolvedRoute.route.matched.length > 1 || !restore) {
				this.vue.$router.push(pushOption);
			} else {
				const openPath = (resolvedRoute.route.meta.app as AppInfo).open_path;
				if(openPath) {
					pushOption.name = null;
					pushOption.path = openPath;
				}
				this.vue.$router.push(pushOption);
			}
		}

	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}