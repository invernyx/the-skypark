<template>
	<div :class="[appName, app.nav_class]">
		<div class="app-frame app-frame-noheader" @click="open">
			<div class="app-box shadowed-deep backsplash">
				<div class="backsplash-image" :style="'background-image: url(' + wallpaper_locked.replace('%theme%', theme) + ')'"></div>
			</div>
			<div class="clock h_status-margin">
				<div class="clock-relation">Device local</div>
				<time class="clock-time">{{ clockTime }}<span>{{ clockAMPM }}</span></time>
				<time class="clock-date">{{ clockDate }}</time>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import { AppInfo, AppEvents, AppType } from "@/sys/foundation/app_model"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
	data() {
		return {
			ready: true,
			theme: this.$os.userConfig.get(['ui','theme']),
			clockTimer: null,
			clockAMPM: "",
			clockTime: "",
			clockDate: "",
			wallpaper: this.$os.userConfig.get(['ui','wallpaper']),
			wallpaper_locked: this.$os.userConfig.get(['ui','wallpaper_locked'])
		}
	},
	mounted() {

		let runClock = () => {
			if(!this.$os.userConfig.get(['ui','is_1142'])) {
				const d = new Date();
				const splitRegex = /\s/;
				let timeStr = Eljs.getTime(d, { hour: '2-digit', minute: '2-digit' }, this.$os.userConfig.get(['ui','units','numbers']));
				let timeStrAMPMMatch = timeStr.match(splitRegex);
				if (timeStrAMPMMatch) {
					const spl = timeStr.split(/\s/);
					this.clockTime = spl[0].trim();
					this.clockAMPM = spl[1].trim();
				} else {
					this.clockTime = timeStr;
					this.clockAMPM = '';
				}
				this.clockDate = Eljs.getDate(d, this.$os.userConfig.get(['ui','units','numbers']));
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
			this.$os.eventsBus.Bus.emit('os', { name: 'unlock', payload: null });
		},

		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'themechange': {
					this.theme = this.$os.userConfig.get(['ui','theme']);
					break;
				}
			}
		},
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		clearInterval(this.clockTimer);
	},
});
</script>

<style lang="scss">
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/sizes.scss';
	.p42_sleep {

		.app-frame {
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