import _Vue from 'vue';

export default class PayloadMutator {
	public os: any;
	public vue: _Vue;


	public Event(wsmsg: any, targetContract: any, targetTemplate :any) {

	}

	public Interact(targetContract: any, type: string, payload :any, callback :Function) {

	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;

	}

}