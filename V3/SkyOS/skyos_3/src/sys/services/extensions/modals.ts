import _Vue from 'vue';

export default class Modals {
	public os: any;
	public vue: _Vue;

	public queue = [] as any[];

	public close() {
		this.os.eventsBus.Bus.emit("modals", {
			name: 'close'
		});
	}
	public add(dt :any) {
		this.os.eventsBus.Bus.emit("modals", {
			name: 'open',
			payload: dt
		});
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}