<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<router-link class="app-frame" :to="{name: 'p42_locked'}" tag="div">
			<div class="clock">
				<div class="clock-relation">Device local</div>
				<time class="clock-time">{{ clockTime }}<span>{{ clockAMPM }}</span></time>
				<time class="clock-date">{{ clockDate }}</time>
			</div>
		</router-link>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import moment from 'moment';
import { AppInfo, AppEvents } from "./../../../sys/foundation/app_bundle"
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "p42_sleep",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
		data() {
			return {
				ready: false,
				clockTimer: null,
				clockAMPM: "",
				clockTime: "",
				clockDate: "",
			}
	},
	beforeMount() {
		this.app.app_events = new AppEvents({
			open: () => {

			},
			close: () => {

			}
		})
	},
	mounted() {

		let runClock = () => {
			if(!this.$os.getState(['ui','is1142'])) {
				const d = new Date();
				let timeStr = Eljs.getTime(d, { hour: '2-digit', minute: '2-digit' });
				let timeStrAMPMMatch = timeStr.match(/am|pm/i);
				if (timeStrAMPMMatch) {
					this.clockTime = timeStr.split(/am|pm/i)[0].trim();
					this.clockAMPM = timeStrAMPMMatch[0];
				} else {
					this.clockTime = timeStr;
					this.clockAMPM = '';
				}
				this.clockDate = Eljs.getDate(d);
			} else {
				this.clockTime = "11:42";
				this.clockAMPM = "AM";
				this.clockDate = "11 November";
			}
		};
		this.clockTimer = setInterval(runClock, 1000);
		runClock();
		this.$emit('loaded');

	},
	beforeDestroy() {
		clearInterval(this.clockTimer);
	},
});
</script>

<style lang="scss">
@import '../../../sys/scss/colors.scss';
.p42_sleep {

	.app-frame {
		background: transparent !important;
		align-items: center;
		justify-content: center;
	}

	.clock {
		text-align: center;
		opacity: 0.8;
		font-size: 12px;
		transition: opacity 5s ease-out;
		color: $ui_colors_bright_shade_0;
		&-relation {
			font-size: 12px;
			text-transform: uppercase;
			opacity: 0.3;
		}
		&-time {
			font-size: 42px;
			line-height: 42px;
			span {
				display: inline-block;
				font-family: "SkyOS-SemiBold";
				font-size: 12px;
				vertical-align: 20px;
				margin-left: 2px;
			}
		}
		&-date {
			font-size: 16px;
			line-height: 28px;
			text-transform: uppercase;
		}
		time {
			display: block;
		}
	}
}
</style>