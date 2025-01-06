import _Vue from 'vue';
import axios, { Method, AxiosInstance } from 'axios';

enum APIChannels {
	Production = "Production",
	Staging = "Staging",
	Development = "Development",
}

export default class API {
	public os: any;
	public vue: _Vue;

	public connected = false;
	public connectedLocal = false;
	public connectedRemote = false;

	public ipcR = null;
	public endpointPath = "http://api-dev.theskypark.com/";
	public endpoint = this.endpointPath;
	public channel = APIChannels.Production;
	public axios = axios.create({ baseURL: this.endpointPath });
	private forceRemoteWS = false;


	public getCDN(type :string, url :string) {
		switch(type){
			case "sounds":
				return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v3/common/sounds/" + url;
			case "images":
				if(url.startsWith("airports/")) {
					return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v1/common/images/" + url;
				} else {
					return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v3/common/images/" + url;
				}
			case "avatars":
				return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v3/common/images/avatars/" + url;
			case "tutorials":
				return "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v3/common/tutorials/" + url;
			default:
				return "";
		}
	}

	public getCDNBase(url :string) {
		return url.replace("%imagecdn%", "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v3/common/images");
	}

	public updateToken(token: string) {
		this.axios.defaults.headers.common = { 'Authorization': "Bearer " + token }
	}

	public send_ws(name: string, message: object, callbackFn?: Function, callbackType?: number) {
		if(this.os.wsLocal.WSReady && !this.forceRemoteWS) {
			this.os.wsLocal.Send(name, message, callbackFn, callbackType);
		} else if(this.os.wsRemote.WSReady) {
			this.os.wsRemote.Send(name, message, callbackFn, callbackType);
		} else {
			console.error("--> WS message dropped");
		}
	}


	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;

		if(this.forceRemoteWS) {
			console.warn("forceRemoteWS set to TRUE in api.ts");
		}
	}

	public startup() {
		if (navigator.userAgent.indexOf('Electron') >= 0) {
			const { ipcRenderer } = window.require('electron')
			this.ipcR = ipcRenderer;
		}

		this.os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	}

	public listener_ws(wsmsg: any, self: API){
		switch(wsmsg.name[0]){
			case 'connect': {
				this.send_ws('progress:get', {});
				break;
			}
		}
	}

}