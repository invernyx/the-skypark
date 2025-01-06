import _Vue from 'vue';
import Notification from '@/sys/models/notification'

export default class Notifications {
	public os: any;
	public vue: _Vue;

	public lastFetch: 0;
	public list: Notification[] = [];

	public addNotification(notificationInstance :Notification, Broadcast = true, Emit = true) {
		const notifObj = this.list.find(x => x.UID == notificationInstance.UID);
		if(notifObj) {
			const index = this.list.indexOf(notifObj);
			this.removeNotification(notifObj, false, false);
			this.list.splice(index, 0, notificationInstance);
		} else {
			this.list.push(notificationInstance);
		}
		if(Emit) {
			if(!notifObj) {
				this.os.eventsBus.Bus.emit('notif-svc', { name: 'add', payload: { new: notificationInstance, list: this.list }  });
			} else {
				this.os.eventsBus.Bus.emit('notif-svc', { name: 'update', payload: { id: notificationInstance.UID, new: notificationInstance, list: this.list }  });
			}
		}
		if(Broadcast) {
			this.os.api.send_ws('notification:add', notificationInstance);
		}
	}

	public removeNotification(notificationInstance :Notification, Broadcast = true, Emit = true) {
		const notifObj = this.list.find(x => x.UID == notificationInstance.UID);
		const indexOf = this.list.indexOf(notifObj);
		if(indexOf > -1) {
			this.list.splice(indexOf, 1);
			if(Emit) {
				this.os.eventsBus.Bus.emit('notif-svc', { name: 'remove', payload: { id: notificationInstance.UID, list: this.list } });
			}
		}
		if(Broadcast) {
			this.os.api.send_ws('notification:remove', { UID: notificationInstance.UID, Trigger: true });
		}
	}

	public removeNotificationFromID(UID :number, Broadcast = true) {
		const notifObj = this.list.find(x => x.UID == UID);
		const indexOf = this.list.indexOf(notifObj);
		if(indexOf > -1) {
			this.list.splice(indexOf, 1);
			this.os.eventsBus.Bus.emit('notif-svc', { name: 'remove', payload: { id: UID, list: this.list } });
		}
		if(Broadcast) {
			this.os.api.send_ws('notification:remove', { UID: UID, Trigger: true });
		}
	}

	public clearAllNotifications(force = false) {

		this.list.filter(x => x.CanDismiss !== false || force).forEach(notif => {
			this.removeNotification(notif, true);
		});

		this.os.eventsBus.Bus.emit('notif-svc', { name: 'clear', payload: { list: this.list } });
	}


	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

	public startup() {
		this.os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	}

	public listener_ws(wsmsg: any, self: Notifications){
		switch(wsmsg.name[0]){
			case 'connect': {
				self.removeNotificationFromID(662);
				break;
			}
			case 'disconnect': {
				self.clearAllNotifications();
				break;
			}
			case 'notifications': {
				switch(wsmsg.name[1]){
					case 'get': {
						wsmsg.payload.List.forEach(notif => {
							self.addNotification(new Notification({
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
							}), false, true);
						});
						break;
					}
				}
				break;
			}
			case 'notification': {
				switch(wsmsg.name[1]){
					case 'add': {
						self.addNotification(new Notification({
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
						}), false, true);
						break;
					}
					case 'remove': {
						self.removeNotificationFromID(wsmsg.payload.UID, false);
						break;
					}
				}
				break;
			}
		}
	}

}