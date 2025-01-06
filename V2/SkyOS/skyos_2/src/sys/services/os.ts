import _Vue from 'vue';
import Apps, { AppInfo, AppType } from "@/sys/foundation/app_bundle"
import { Route } from 'vue-router'
import Notification from '@/sys/models/notification'

class OS {

	private SunCalc = require('suncalc');

	private vue: _Vue = null;
	public analytics: any = null;
	public analyticsPingTimeout: any = null;
	public awaitsTransponderReboot: Date = null;
	public isDev = false;
	public isBeta = false;
	public UntrackedMap = {};
	public transponderSettings = {
		tier: 'endeavour'
	} as any;

	public activeApp: AppInfo = null;
	public activeAppRouter: Route = null;
	public activeAppRouterRestoreURL: string = null;
	public themeAccentOverride = null;

	public appHistory: AppInfo[] = [];
	public appAllHistory: AppInfo[] = [];

	constructor(Vue: _Vue) {
		this.vue = Vue;

		this.vue.$on('ws-in', (wsmsg: any) => {
			switch(wsmsg.name[0]){
				case 'disconnect': {
					this.isDev = false;
					break;
				}
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.setConfig(['services','ga','tag'], wsmsg.payload.ga);
							if(wsmsg.payload.dev) {
								this.isDev = true;
							}
							if(wsmsg.payload.beta) {
								this.isBeta = true;
							}
							this.transponderSettings = wsmsg.payload.set;
							break;
						}
					}
					break;
				}
			}
		});
	}

	//#region Set App
	public setCurrentApp(app :AppInfo) {
		this.activeApp = app;

		this.vue.$emit('navigate', app);

		const historyPush = () => {
			if(this.appHistory.includes(app)) {
				const index = this.appHistory.indexOf(app);
				this.appHistory.splice(index, 1);
			}
			if(this.appAllHistory.includes(app)) {
				const index = this.appAllHistory.indexOf(app);
				this.appAllHistory.splice(index, 1);
			}
			if(app.app_visible) {
				if(app.app_type == AppType.GENERAL) {
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
	//#endregion


	//#region Notifications
	public addNotification(notificationInstance :Notification, Broadcast = true, Emit = true) {
		const notifObj = this.vue.$data.state.services.notifications.list.find(x => x.UID == notificationInstance.UID);
		if(notifObj) {
			const index = this.vue.$data.state.services.notifications.list.indexOf(notifObj);
			this.removeNotification(notifObj, false);
			this.vue.$data.state.services.notifications.list.splice(index, 0, notificationInstance);
		} else {
			this.vue.$data.state.services.notifications.list.push(notificationInstance);
		}
		if(Emit && !notifObj) {
			this.vue.$emit('notif-svc', notificationInstance);
		}
		if(Broadcast) {
			this.vue.$data.services.api.SendWS('notification:add', notificationInstance);
		}
	}
	public removeNotification(notificationInstance :Notification, Broadcast = true, Trigger = true) {
		const notifObj = this.vue.$data.state.services.notifications.list.find(x => x.UID == notificationInstance.UID);
		const indexOf = this.vue.$data.state.services.notifications.list.indexOf(notifObj);
		if(indexOf > -1) {
			//console.log('Removed notification: ', this.vue.$data.state.services.notifications.list[indexOf]);
			this.vue.$data.state.services.notifications.list.splice(indexOf, 1);
		}
		if(Broadcast) {
			this.vue.$data.services.api.SendWS('notification:remove', { UID: notificationInstance.UID, Trigger: true });
		}
	}
	public removeNotificationFromID(UID :number, Broadcast = true, Trigger = true) {
		const notifObj = this.vue.$data.state.services.notifications.list.find(x => x.UID == UID);
		const indexOf = this.vue.$data.state.services.notifications.list.indexOf(notifObj);
		if(indexOf > -1) {
			//console.log('Removed notification: ', this.vue.$data.state.services.notifications.list[indexOf]);
			this.vue.$data.state.services.notifications.list.splice(indexOf, 1);
		}
		if(Broadcast) {
			this.vue.$data.services.api.SendWS('notification:remove', { UID: UID, Trigger: true });
		}
	}
	public clearAllNotifications() {
		while(this.vue.$data.state.services.notifications.list.filter(x => x.CanDismiss !== false).length > 0) {
			this.removeNotification(this.vue.$data.state.services.notifications.list[0], true, false);
		}
	}
	//#endregion


	//#region Set sun position
	public setSunPosition(d :Date, lon :number, lat :number) {
		const pos = this.SunCalc.getPosition(d, lat, lon);

		this.vue.$data.state.ui.sunPosition[0] = pos.azimuth * 180 / Math.PI;
		this.vue.$data.state.ui.sunPosition[1] = pos.altitude * 180 / Math.PI;

		if(this.vue.$data.config.ui.themeAuto) {
			if(this.getConfig(['ui', 'theme']) == 'theme--dark') {
				if(pos.altitude > 0) {
					this.setConfig(['ui', 'theme'], 'theme--bright');
				}
			} else {
				if(pos.altitude < 0) {
					this.setConfig(['ui', 'theme'], 'theme--dark');
				}
			}
		}
	}
	//#endregion


	//#region Set Theme Accent
	public setThemeAccent(accent :any) {
		this.themeAccentOverride = accent;
		this.vue.$emit('themechange', accent);
	}
	//#endregion


	//#region App Switching / History
	public initAppHistory() {
		Apps.forEach(app => {
			if(app.app_visible) {
				if(app.app_type == AppType.GENERAL && !app.app_isexternal && app != this.activeApp){
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
			return require('../../apps/' + app_vendor + '/' + app_ident + '/icon.svg');
		} catch (e) {
			return require('../../sys/assets/framework/icon_default.svg');
		}
	}
	//#endregion


	//#region State and Config
	public setState(path :string[], value :any) {
		const deepen = (obj :any,  depth :number) :boolean => {
			const keys = Object.keys(obj);
			if(keys.includes(path[depth])) {
				if(depth == path.length - 1) {
					if(obj[path[depth]] != value) {
						obj[path[depth]] = value;
						this.vue.$emit("statechange", path, value);
					}
					return true;
				} else {
					return deepen(obj[path[depth]], depth + 1);
				}
			} else {
				return false;
			}
		}
		if(deepen(this.vue.$data.state, 0)) {
			(this.vue as any).stateSave();
		}
	}
	public getState(path :string[]) {
		const deepen = (obj :any,  depth :number) => {
			const keys = Object.keys(obj);
			if(keys.includes(path[depth])) {
				if(depth == path.length - 1) {
					return obj[path[depth]];
				} else {
					return deepen(obj[path[depth]], depth + 1);
				}
			} else {
				return null;
			}
		}
		return deepen(this.vue.$data.state, 0);
	}
	//#endregion


	//#region State and Config
	public getCDN(type :string, url :string) {
		switch(type){
			case "images":
				return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v1/common/images/" + url;
			case "avatars":
				return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v1/common/images/avatars/" + url;
			case "tutorials":
				return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v1/common/tutorials/" + url;
			default:
				return "";
		}
	}
	public getCDNBase(url :string) {
		return url.replace("%imagecdn%", "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v1/common/images");
	}
	//#endregion


	//#region Config
	public setConfig(path :string[], param :any, force = false) {
		let HasChanged = false;
		const deepen = (obj :any,  depth :number) :boolean => {
			const keys = Object.keys(obj);
			if(keys.includes(path[depth])) {
				if(depth == path.length - 1) {
					if(obj[path[depth]] != param) {
						obj[path[depth]] = param;
						this.vue.$emit("configchange", path, param);
						HasChanged = true;
					}
					return true;
				} else {
					return deepen(obj[path[depth]], depth + 1);
				}
			} else {
				return false;
			}
		}
		if(deepen(this.vue.$data.config, 0)) {
			if(HasChanged || force) {
				(this.vue as any).stateSave();
				switch(path[0]) {
					case 'ui': {
						switch(path[1]) {
							case 'theme': {
								this.TrackEvent("UI", "Theme", param);
								break;
							}
						}
						break;
					}
				}
			}
		}
	}
	public getConfig(path :string[]) {
		const deepen = (obj :any,  depth :number) :any => {
			const keys = Object.keys(obj);
			if(keys.includes(path[depth])) {
				if(depth == path.length - 1) {
					return obj[path[depth]];
				} else {
					return deepen(obj[path[depth]], depth + 1);
				}
			} else {
				return null;
			}
		}
		return deepen(this.vue.$data.config, 0);
	}
	//#endregion


	//#region Google Analytics
	public SetGA() {
		//const tag = this.getConfig(['services','ga','tag']);
		//this.analytics = ua('ANALYTICS_KEY', tag.length ? tag : null);

		//this.analytics.pageview("/", "http://theskypark.com");
		this.ResetGAPing();
	}
	public ResetGAPing() {
		clearTimeout(this.analyticsPingTimeout);
		this.analyticsPingTimeout = setTimeout(() => {
			this.TrackEvent("Ping", "Ping", "Ping");
			this.ResetGAPing();
		}, 240000);
	}
	public TrackEvent(Category :string, Action :string, Label :string, Value = 1) {
		if(this.vue.$data.state.device.isElectron) {
			this.vue.$data.ipcR.send('a-msg', 'analytics:event', {
				Category: Category,
				Action: Action,
				Label: Label,
				Value: Value,
				CID: this.getConfig(['services','ga','tag'])
			});
		}
	}
	public TrackPage(URL :string, Title :string) {
		if(this.vue.$data.state.device.isElectron) {
			this.vue.$data.ipcR.send('a-msg', 'analytics:page', {
				URL: URL,
				Title: Title,
				CID: this.getConfig(['services','ga','tag'])
			});
		}
	}
	public TrackMapLoad(app :string) {
		this.TrackEvent("UI", "MapLoad", app);
	}
	//#endregion


	//#region Modals / Ask
	public modalClose() {
		this.vue.$data.state.ui.modals.queue.splice(0, 1);
	}
	public modalPush(dt :any) {
		this.vue.$data.state.ui.modals.queue.unshift(dt);
	}
	//#endregion

}

export default {
  install: (Vue: typeof _Vue) => { //, options?: any
    let installed = false;
    Vue.mixin({
      beforeCreate() {
        if (!installed) {
          installed = true;
          Vue.prototype.$os = new OS(this);
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
