import Eljs from '@/sys/libraries/elem';
import _Vue from 'vue';

export default class Units {
	public os: any;
	public vue: _Vue;

	public ConvertDistance(dist_m :number, decimals :number, show_post = true){

		const unit = this.os.userConfig.get(['ui','units','distances']);
		const locale = this.os.userConfig.get(['ui','units','numbers']);
		const format = new Intl.NumberFormat(locale);
		let amount = dist_m;
		let post = '';

		switch(unit) {
			case 'feet': {
				amount = dist_m * 3280.84;
				post = '\u00A0ft';
				break;
			}
			case 'nautical_miles': {
				amount = dist_m * 0.539957;
				post = '\u00A0nm';
				break;
			}
			case 'kilometers': {
				amount = dist_m;
				post = '\u00A0km';
				break;
			}
		}

		return format.format(Eljs.round(amount, decimals)) + (show_post ? post : '');
	}


	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}