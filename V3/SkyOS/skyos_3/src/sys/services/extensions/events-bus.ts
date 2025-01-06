import _Vue from 'vue';
import mitt from 'mitt' //, { Emitter, EventType }

export default class EventsBus {
	public os: any;
	public vue: _Vue;

	public Bus = mitt<any>();

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;

		const ignore = [
			'eventbus',
			'meta',
			'sim',
			'navigation'
		] as any[]

		this.Bus.on('*', (ev, msg) => {
			if(!ignore.includes(msg.name[0]) && !ignore.includes(msg.name) && !ignore.includes(ev)) {
				console.warn("OS Event |", ev, msg);
				//console.debug(new Error().stack);
			}
		});
	}

}