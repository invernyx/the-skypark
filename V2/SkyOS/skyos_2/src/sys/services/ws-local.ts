import _Vue from 'vue';
import Eljs from './../../sys/libraries/elem';

class Service {

	private GlobalCallbackID = 2;
	public endpoint = "ws://localhost:8163";
	public WS?: WebSocket = null;
	public WSReady = false;

	private vue: _Vue;
	private Callbacks: any = {};

	constructor(Vue: _Vue) {
		this.vue = Vue;

		// Connect check
		setInterval(() => {
			if (this.WS === null) {
				this.Connect();
			}
		}, 6000);

		this.Connect();
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
					'origin': '0',
					'meta': metaStruct,
					'payload': JSON.stringify(message),
				}

				const msgStr = JSON.stringify(msgStruct);

				console.debug("--> Send WS msg: " + this.endpoint + " - " + msgStr);
				this.WS.send(msgStr);
			} else {
				console.error("--> WS message dropped");
			}
		});
	}

	public Connect(callback?: Function) {

		if (this.WS !== null) {
			if(callback != undefined){
				callback();
			}
			return;
		}

		this.WS = new WebSocket(this.endpoint);
		this.WS.onmessage = (event) => {

			const data = JSON.parse(event.data);
			const n = data.name;
			data.name = data.name.split(':');
			data.payload = JSON.parse(data.payload);

			if(n != 'eventbus:meta') {
				console.debug("--> Recv WS msg: " + this.endpoint + " - " + data.name, data);
			}

			this.vue.$emit('ws-in', data);

			if (data.meta.callback) {
				if (this.Callbacks[data.meta.callback]) {
					this.Callbacks[data.meta.callback](data);
				}
			}
			if (data.meta.callbackType === 0) {
				delete this.Callbacks[data.meta.callback];
			}

		};
		this.WS.onopen = () => {
			console.debug("WS Connection to " + this.endpoint + " Open");
			this.WSReady = true;
			this.vue.$wsRemoteService.Disconnect();
			this.Send(
				"connect:confirm",
				{
					id: "skypad_" + new Date().getTime(),
					account: "1",
					type: "skypad",
					name: "Skypad_" + new Date().getUTCMilliseconds()
				}
			);
			if(callback != undefined){
				callback();
			}
			(this.vue as any).setAPIState('local', this.WSReady);
			this.vue.$emit('ws-in', { name: ['connect'] });
		};
		this.WS.onclose = (code) => {
			console.debug("WS Connection to " + this.endpoint + " Closed");
			if(this.WSReady){
				this.WSReady = false;
				(this.vue as any).setAPIState('local', this.WSReady);
				this.vue.$emit('ws-in', { name: ['disconnect'] });
			}
			this.WS = null;
		};
		this.WS.onerror = () => {
			this.WSReady = false;
			this.WS = null;
			this.vue.$emit('ws-in', { name: ['failsconnect'] });
			console.debug("WS Connection to " + this.endpoint + " had an Error");
			if(callback != undefined){
				callback();
			}
		};
	}

}

export default {
	install: (Vue: typeof _Vue, options?: any) => {
		let installed = false;
		Vue.mixin({
			beforeCreate() {
				if (!installed) {
					installed = true;
					Vue.prototype.$wsLocalService = new Service(this);
				}
			}
		});
	}
};

declare module 'vue/types/vue' {
	interface Vue {
		$wsLocalService: Service;
	}
}
