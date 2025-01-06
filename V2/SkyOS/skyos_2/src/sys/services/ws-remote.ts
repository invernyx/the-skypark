import _Vue from 'vue';
import Eljs from '../libraries/elem';

class Service {

	private GlobalCallbackID = 2;
	public endpoint = "ws://192.168.2.32:8080";
	public WS?: WebSocket = null;
	public WSReady = false;
	private ClientID = "";

	private vue: _Vue;
	private Callbacks: any = {};

	constructor(Vue: _Vue) {
		this.vue = Vue;

		// WS Ping
		setInterval(() => {
			if (this.WS !== null) {
				this.Send("connect:ping", {});
			}
		}, 60000);

		// Connect check
		setInterval(() => {
			if (this.WS === null && !this.vue.$data.state.services.api.connectedLocal) {
				//this.Connect();
			}
		}, 6000);
	}

	public Send(name: string, message: object, callbackFn?: Function, callbackType?: number) {
		// callbackTypes
		// 0 = None
		// 1 = Once
		// 2 = Stream

		let callbackID = 0;
		const metaStruct = {} as any;

		if(callbackFn){
			if(!callbackType) {
				callbackType = 1;
			}
		}

		if (callbackType) {
			callbackID = this.GlobalCallbackID++; //Eljs.getNumGUID();
			metaStruct.callback = callbackID;
			metaStruct.callbackType = callbackType;
			this.Callbacks[callbackID] = callbackFn;
		}

		this.Connect(() => {
			if (this.WS != undefined && this.WS.readyState === 1) {

				const msgStruct = {
					'name': name,
					'origin': this.ClientID,
					'meta': metaStruct,
					'payload': JSON.stringify(message),
				}

				const msgStr = JSON.stringify(msgStruct);

				console.debug("--> Send WS Remote msg: " + this.endpoint + " - " + msgStr);
				this.WS.send(msgStr);
			} else {
				console.error("--> WS Remote message dropped");
			}
		});
	}

	public Connect(callback?: Function) {

		if (this.WS !== null || this.vue.$data.config.services.wsRemote.room == '') {
			if(callback != undefined){
				callback();
			}
			return;
		}

		this.WS = new WebSocket(this.endpoint + "?type=skypad&session_key=" + this.vue.$data.config.services.wsRemote.room);
		this.WS.onmessage = (event) => {

			const data = JSON.parse(event.data);
			if(data.session_key) {

				this.ClientID = data.client_id
				this.vue.$data.config.services.wsRemote.room = data.session_key;
				this.vue.$emit('sys:config:save');

				this.WSReady = true;
				this.Send(
					"connect:confirm",
					{
						id: this.ClientID,
						account: "1",
						type: "skypad",
						name: "Skypad_" + new Date().getUTCMilliseconds()
					}
				);
				if(callback != undefined){
					callback();
				}
				(this.vue as any).setAPIState('remove', this.WSReady);
				this.vue.$emit('ws-in', { name: ['connect'] });

			} else if(data.target == this.ClientID) {

				data.name = data.name.split(':');
				data.payload = JSON.parse(data.payload);

				console.debug("--> Recv WS Remote msg: " + this.endpoint + " - " + data.name, data);

				this.vue.$emit('ws-in', data);

				if (data.meta.callback) {
					if (this.Callbacks[data.meta.callback]) {
						this.Callbacks[data.meta.callback](data);
					}
				}
				if (data.meta.callbackType === 0) {
					delete this.Callbacks[data.meta.callback];
				}
			}
		};
		this.WS.onopen = () => {
			console.debug("WS Remote Connection to " + this.endpoint + " Open");
		};
		this.WS.onclose = (code) => {
			console.debug("WS Remote Connection to " + this.endpoint + " Closed");
			if(this.WSReady){
				this.WSReady = false;
				(this.vue as any).setAPIState('remove', this.WSReady);
				this.vue.$emit('ws-in', { name: ['disconnect'] });
			}
			this.WS = null;
		};
		this.WS.onerror = () => {
			this.WSReady = false;
			this.WS = null;
			if(!this.vue.$root.$wsLocalService.WSReady) {
				this.vue.$emit('ws-in', { name: ['failsconnect'] });
			}
			console.debug("WS Remote Connection to " + this.endpoint + " had an Error");
			if(callback != undefined){
				callback();
			}
		};
	}

	public Disconnect() {
		if(this.WS) {
			this.WS.close();
			this.WS = null;
		}
	}

}

export default {
	install: (Vue: typeof _Vue, options?: any) => {
		let installed = false;
		Vue.mixin({
			beforeCreate() {
				if (!installed) {
					installed = true;
					Vue.prototype.$wsRemoteService = new Service(this);
				}
			}
		});
	}
};

declare module 'vue/types/vue' {
	interface Vue {
		$wsRemoteService: Service;
	}
}
