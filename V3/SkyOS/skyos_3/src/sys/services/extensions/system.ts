import _Vue from 'vue';
import Notification from '@/sys/models/notification'

export default class System {
	public os: any;
	public vue: _Vue;

	public isDev = false;
	public isBeta = false;

	public map_covered = false;
	public sidebarOpen = true;

	public awaitsTransponderReboot: Date = null;

	public transponderSettings = {
		tier: 'endeavour'
	} as any;

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;

		window.addEventListener('resize', this.resize);
		this.resize();
	}

	public startup() {
		this.os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	}

	public resize() {
		if(document.body.clientWidth > 700)
			this.sidebarOpen = true;
	}

	public setSidebar(state :boolean) {
		//if(document.body.clientWidth > 700) {
		//	state = true;
		//}

		if(this.sidebarOpen != state) {
			this.sidebarOpen = state;
			this.os.eventsBus.Bus.emit('os', { name: 'sidebar', payload: this.sidebarOpen });
		}
	}

	public set_cover(state :boolean) {
		if(this.map_covered != state) {
			this.map_covered = state;
			this.os.eventsBus.Bus.emit('os', { name: 'covered', payload: this.map_covered });
			this.os.eventsBus.Bus.emit('os', { name: 'themechange', payload: this.os.theme.getTheme() });
		}
	}

	public listener_ws(wsmsg: any, self: System){
		switch(wsmsg.name[0]){
			case 'disconnect': {
				self.isDev = false;
				break;
			}
			case 'failsconnect': {
				if(self.awaitsTransponderReboot) {
					if(self.awaitsTransponderReboot.getTime() + 30000 < new Date().getTime()){ //300000
						self.awaitsTransponderReboot = null;
					}
				}

				if(!self.awaitsTransponderReboot) {
					self.os.notifications.clearAllNotifications(true);
					self.os.notifications.addNotification(new Notification({
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
						self.awaitsTransponderReboot = null;
						self.transponderSettings = wsmsg.payload.set;
						if(wsmsg.payload.dev) {
							self.isDev = true;
						}
						if(wsmsg.payload.beta) {
							self.isBeta = true;
						}
						break;
					}
					case 'restart': {
						self.awaitsTransponderReboot = new Date();
						self.os.notifications.addNotification(new Notification({
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
		}
	}

}