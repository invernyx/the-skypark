import _Vue from 'vue';

export default class UserConfig {
	public os: any;
	public vue: _Vue;

	//#region Config
	public set(path :string[], value :any) {
		(this.vue as any).sys.state_set(path, value);
		this.os.eventsBus.Bus.emit("configchange", {
			name: path,
			payload: value
		});
	}
	public get(path :string[], def :any = null) {
		return (this.vue as any).sys.state_get(path, def);
	}
	//#endregion

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

	public startup() {
		this.os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	}

	public listener_ws(wsmsg: any, self: UserConfig){
		switch(wsmsg.name[0]){
			case 'transponder': {
				switch(wsmsg.name[1]){
					case 'state': {
						self.set(['services','ga','tag'], wsmsg.payload.ga);
						break;
					}
				}
				break;
			}
		}
	}

}