import _Vue from 'vue';

export default class Time {
	public os: any;
	public vue: _Vue;

	private SunCalc = require('suncalc');

	public setSunPosition(d :Date, lon :number, lat :number) {
		const pos = this.SunCalc.getPosition(d, lat, lon);

		this.vue.$data.state.ui.sun_position[0] = pos.azimuth * 180 / Math.PI;
		this.vue.$data.state.ui.sun_position[1] = pos.altitude * 180 / Math.PI;

		if(this.os.userConfig.get(['ui','themeAuto'])) {
			if(this.os.userConfig.get(['ui', 'theme']) == 'theme--dark') {
				if(pos.altitude > 0) {
					this.os.userConfig.set(['ui', 'theme'], 'theme--bright');
				}
			} else {
				if(pos.altitude < 0) {
					this.os.userConfig.set(['ui', 'theme'], 'theme--dark');
				}
			}
		}
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}