<template>
	<span>{{ output }}</span>
</template>

<script lang="ts">
import Vue from 'vue';
import moment from 'moment';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: ["time", "decimals", "as_number", "brackets"],
	beforeMount() {
		this.init();
	},
	methods: {
		init() {
			const format = this.$os.userConfig.get(['ui','units','numbers']);

			const moment_now = moment().locale(format);
			const moment_then = moment().add(this.time, 'hours').locale(format);
			const moment_diff = moment.duration(moment_then.diff(moment_now));
			const moment_hours = moment_diff.asHours();

			if(this.brackets) {
				if(this.brackets.to_seconds ? (moment_hours < this.brackets.to_seconds) : false) {
					this.output = Eljs.floor(moment_diff.asSeconds(), this.decimals) + ' s';
					return;
				} else if(this.brackets.to_minutes ? (moment_hours < this.brackets.to_minutes) : false) {
					if(!this.as_number) {
						const moment_minutes = moment_diff.asMinutes();
						const minute_round = Math.floor(moment_minutes);
						const minutes_in_hour = Math.floor(0.6 * ((moment_minutes - minute_round)) * 100);
						this.output = minute_round + "m" + Eljs.pad(minutes_in_hour, 2) + 's'; //Eljs.round(moment_hours, this.decimals) + ' h';
					} else {
						this.output = Eljs.floor(moment_diff.asMinutes(), this.decimals) + ' m';
					}
					return;
				}
			}

			if(!this.as_number) {
				const hour_round = Math.floor(moment_hours);
				const minutes_in_hour = Math.floor(0.6 * ((moment_hours - hour_round)) * 100);
				this.output = hour_round + "h" + Eljs.pad(minutes_in_hour, 2)  ; //Eljs.round(moment_hours, this.decimals) + ' h';
			} else {
				this.output = Eljs.floor(moment_hours, this.decimals) + ' h';
			}

		}
	},
	data() {
		return {
			output: this.time,
		}
	},
	watch: {
		time: {
			immediate: true,
			handler(newValue, oldValue) {
				this.init();
			}
		}
	}
});
</script>