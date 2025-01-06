import _Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default class WsLocal {
	public os: any;
	public vue: _Vue;

	private GlobalCallbackID = 2;
	public endpoint = "ws://localhost:8163";
	public WS?: WebSocket = null;
	public WSReady = false;
	private Callbacks: any = {};

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

				console.debug("--> Send WS msg: " + this.endpoint, msgStruct, message);
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

		const self = this;
		this.WS = new WebSocket(this.endpoint);
		this.WS.onmessage = (event) => {

			const data = JSON.parse(event.data, Eljs.json_parser);
			const n = data.name;
			data.name = data.name.split(':');
			data.payload = JSON.parse(data.payload, Eljs.json_parser);

			if(n != 'eventbus:meta') {
				console.debug("--> Recv WS msg: " + self.endpoint + " - " + data.name, data);
			}

			self.os.eventsBus.Bus.emit('ws-in', data);

			if (data.meta.callback) {
				if (self.Callbacks[data.meta.callback]) {
					self.Callbacks[data.meta.callback](data);
				}
			}
			if (data.meta.callbackType === 0) {
				delete self.Callbacks[data.meta.callback];
			}
		};
		this.WS.onopen = () => {
			console.debug("WS Connection to " + self.endpoint + " Open");
			self.WSReady = true;
			self.os.wsRemote.Disconnect();
			self.Send(
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
			(self.vue as any).setAPIState('local', self.WSReady);
			self.os.eventsBus.Bus.emit('ws-in', { name: ['connect'], payload: 'local' });
		};
		this.WS.onclose = (code) => {
			console.debug("WS Connection to " + self.endpoint + " Closed");
			if(self.WSReady){
				self.WSReady = false;
				(self.vue as any).setAPIState('local', self.WSReady);
				self.os.eventsBus.Bus.emit('ws-in', { name: ['disconnect'], payload: 'local' });
			}
			self.WS = null;
		};
		this.WS.onerror = () => {
			self.WSReady = false;
			self.WS = null;
			self.os.eventsBus.Bus.emit('ws-in', { name: ['failsconnect'], payload: 'local' });
			console.debug("WS Connection to " + self.endpoint + " had an Error");
			if(callback != undefined){
				callback();
			}
		};
	}


	public startup() {

		// Connect check
		setInterval(() => {
			if (this.WS === null) {
				this.Connect();
			}
		}, 6000);

		this.Connect();
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;

	}

}