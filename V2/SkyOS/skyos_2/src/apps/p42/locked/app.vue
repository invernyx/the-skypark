<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame" @click="open">
			<div class="backsplash">
				<div class="backsplash-image" :style="'background-image: url(' + inst.config.ui.wallpaperLocked.replace('%theme%', $root.$data.config.ui.theme) + ')'"></div>
			</div>
			<div class="clock helper_status-margin">
				<div class="clock-relation">Device local</div>
				<time class="clock-time">{{ clockTime }}<span>{{ clockAMPM }}</span></time>
				<time class="clock-date">{{ clockDate }}</time>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import moment from 'moment';
import Eljs from '@/sys/libraries/elem';
import { AppInfo, AppEvents, AppType } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_locked",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
	data() {
		return {
			ready: true,
			clockTimer: null,
			clockAMPM: "",
			clockTime: "",
			clockDate: "",
		}
	},
	beforeMount() {

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
	methods: {
		open() {
			this.$root.$emit('unlock');
		}
	},
	beforeDestroy() {
		clearInterval(this.clockTimer);
	},
});
</script>

<style lang="scss">
	@import '../../../sys/scss/colors.scss';
	@import '../../../sys/scss/sizes.scss';
	.p42_locked {

		.app-frame {
			background: transparent !important;
			justify-content: flex-start;
		}

		.clock {
			text-align: center;
			opacity: 0.99;
			padding-top: $status-size;
			transition: opacity 0.3s ease-out;
			color: $ui_colors_bright_shade_0;
			text-shadow: 0px 3px 5px rgba(0,0,0,.5), 0px 3px 30px rgba(0,0,0,1);
			&-relation {
				font-size: 12px;
				text-transform: uppercase;
				opacity: 0.3;
			}
			&-time {
				font-size: 48px;
				line-height: 48px;
				span {
					display: inline-block;
					font-family: "SkyOS-SemiBold";
					font-size: 18px;
					vertical-align: 21px;
					margin-left: 5px;
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