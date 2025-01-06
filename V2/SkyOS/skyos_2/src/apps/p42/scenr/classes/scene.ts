import _Vue from 'vue';
import Eljs from "@/sys/libraries/elem";

let vue: _Vue = null;

export default class ScenrProject {

	public Features: any = [];
	public Name = "Unnamed Scenr";
	public ID = Eljs.genGUID()

	constructor(Vue: _Vue, structure?: any) {
		vue = Vue;
		const self = this;
		if (structure !== undefined) {
			Object.assign(this, structure);
		}
	}

  }