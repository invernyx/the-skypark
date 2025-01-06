import _Vue from 'vue';
import { OS } from '@/sys/services/os'
import Notification from '@/sys/models/notification';

export default class Messaging {
	private $os: OS;

	constructor(os :any, vue :_Vue) {
		this.$os = os;
	}

	//public startup() {
	//	this.$os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	//}

	//public listener_ws(wsmsg: any, self: Messaging){
	//	switch(wsmsg.name[0]){
	//		case 'messaging': {
	//			switch(wsmsg.name[1]){
	//				case 'new': {
	//					break;
	//				}
	//			}
	//			break;
	//		}
	//	}
	//}

}