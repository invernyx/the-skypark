import Vue from 'vue'
import { AppInfo } from "@/sys/foundation/app_bundle"

let t = null as c;
class c {

	private root = null as Vue;
	private app = null as AppInfo;

	constructor(root :Vue, app :AppInfo){
		this.root = root;
		this.app = app;
		root.$on('notif-svc', this.listenerNotif);
		t = this;
	}

	listenerNotif(notif: any) {
		switch(notif.Type){
			case 'Success':
			case 'Fail': {
				t.root.$os.modalPush({
					type: 'endcard',
					data: notif
				});
				break;
			}
		}
	}
}

export default c;