<template>
	<span class="countdown" :class="{ 'warn': level_1_calc > diffWarnH && has_warn, 'alert': level_2_calc > diffWarnH && has_warn,  }">
		{{ timeStr }}
	</span>
</template>

<script lang="ts">
import Vue from 'vue';
import moment from 'moment';
import { tag } from '@turf/turf';

export default Vue.extend({
	name: "countdown",
	props: ['time', 'short', 'stop_zero', 'full', 'precise', 'capitalize', 'only_hours', 'no_fix', 'has_warn', 'warn_for', 'level_1', 'level_2'],
	data() {
		return {
			timeStr: "",
			diffS: 0,
			diffM: 0,
			diffH: 0,
			diffWarnH: 0,
			level_1_calc: 4,
			level_2_calc: 1,
			intervalRef: null,
		};
	},
	beforeMount() {
		if(this.level_1) { this.level_1_calc = this.level_1 }
		if(this.level_2) { this.level_2_calc = this.level_2 }
	},
	created() {
		this.intervalRef = setInterval(() => {
			this.recalculate()
		}, this.diffWarnH < 10000 ? 1000 : 10000)
	},
	beforeDestroy() {
		clearInterval(this.intervalRef);
	},
	methods: {
		recalculate() {
			const now = moment();
			const warn = moment(this.warn_for);
			let target = moment(this.time);

			let hoursStr = 'hour';
			let minutesStr = 'minute';

			this.diffS = Math.abs(target.diff(now, 'seconds'));
			this.diffM = Math.abs(target.diff(now, 'minutes'));
			this.diffH = Math.abs(target.diff(now, 'hours'));

			if(this.warn_for) {
				this.diffWarnH = Math.abs(warn.diff(now, 'hours'));
			}

			if(this.stop_zero && target.isBefore(now)) {
				target = now;
			}

			let pre = "";
			let pro = "";
			if(!this.no_fix) {
				if (target.isBefore(now)) {
					pro = ' ago'
				} else {
					pre = 'in ';
				}
			}

			if(!this.only_hours) {
				if(this.short){
					this.timeStr = target.utc().calendar({
						sameDay: ((now) => {
							if(this.diffM > 120) {
								return '[' + pre + this.diffH + 'h ' + pro + ']'
							} else if(this.diffM > 1) {
								return '[' + pre + this.diffM  + 'm ' + pro + ']'
							} else {
								if(!this.precise) {
									return '[now]'
								} else {
									return '[' + pre + this.diffS  + 's ' + pro + ']'
								}
							}
						})(),
						nextDay: (() => {
							return '[' + pre + this.diffH  + 'h ' + pro + ']'
						})(),
						nextWeek: (() => {
							return 'ddd[,] HH:mm [GMT]'
						})(),
						lastDay: (() => {
							return '[' + pre + this.diffH  + 'h ' + pro + ']'
						})(),
						lastWeek: '[Last] ddd[,] HH:mm [GMT]',
						sameElse: 'MMM DD[,] HH:mm [GMT]'
					});
				} else {
					this.timeStr = target.utc().calendar({
						sameDay: ((now) => {
							if(this.diffM > 120) {
								return '[' + pre + this.diffH + ' ' + hoursStr + (this.diffH != 1 ? 's' : '') + pro + ']'
							} else if(this.diffM > 1) {
								return '[' + pre + this.diffM  + ' ' + minutesStr + (this.diffM != 1 ? 's' : '') + pro + ']'
							} else {
								if(!this.precise) {
									return '[just now]'
								} else {
									return '[' + pre + this.diffS  + 's ' + pro + ']'
								}
							}
						})(),
						nextDay: (() => {
							return '[' + pre + this.diffH  + ' ' + hoursStr + (this.diffH != 1 ? 's' : '') + pro + ']'
						})(),
						nextWeek: (() => {
							return 'ddd[,] HH:mm [GMT]'
						})(),
						lastDay: (() => {
							return '[' + pre + this.diffH  + ' ' + hoursStr + (this.diffH != 1 ? 's' : '') + pro + ']'
						})(),
						lastWeek: '[Last] dddd[,] HH:mm [GMT]',
						sameElse: 'MMM DD[,] HH:mm [GMT]'
					});
				}
			} else {
				if(this.diffM > 120) {
					if(this.short){
						this.timeStr = pre + ' ' + this.diffH + 'h ' + pro;
					} else {
						this.timeStr = pre + ' ' + this.diffH + ' hour' + (this.diffH != 1 ? 's' : '') + ' ' + pro;
					}
				} else {
					if(this.short){
						this.timeStr = pre + ' ' + this.diffM + 'm ' + pro;
					} else {
						this.timeStr = pre + ' ' + this.diffM + ' minute' + (this.diffM != 1 ? 's' : '') + ' ' + pro;
					}
				}
			}
			if(this.capitalize) {
				this.timeStr = this.timeStr.charAt(0).toUpperCase() + this.timeStr.slice(1);
			}
		}
	},
	watch: {
		short: {
			immediate: true,
			handler(newValue, oldValue) {
				this.recalculate();
			}
		},
		time: {
			immediate: true,
			handler(newValue, oldValue) {
				this.recalculate();
			}
		}
	}
});
</script>


<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.countdown {

	.theme--bright &,
	&.theme--bright {
		&.warn {
			background-color: $ui_colors_bright_button_warn;
		}
		&.alert {
			background-color: $ui_colors_bright_button_cancel;
		}
	}

	.theme--dark &,
	&.theme--dark {
		&.warn {
			background-color: $ui_colors_dark_button_warn;
		}
		&.alert {
			background-color: $ui_colors_dark_button_cancel;
		}
	}

	&.warn,
	&.alert {
		padding: 0px 4px;
		border-radius: 4px;
	}
}

</style>