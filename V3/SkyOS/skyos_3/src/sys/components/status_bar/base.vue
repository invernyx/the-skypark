<template>
	<section class="os-status" :class="[theme.status, { 'is-audio': audio.playing, 'is-shaded': theme.statusShaded }]">
		<div class="os-status_background">
			<div class="os-status_background_bright"></div>
			<div class="os-status_background_dark"></div>
			<div class="os-status_background_bright_audio"></div>
			<div class="os-status_background_dark_audio"></div>
		</div>
		<div class="os-status_stack">
			<div>
				<StatusSignal />
				<StatusNotice />
				<StatusAudio />
			</div>
			<div>
				<StatusClock />
				<StatusNotifications @toggle="toggle_notifications" />
			</div>
		</div>
	</section>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		theme: Object
	},
	components: {
		StatusAudio: () => import("@/sys/components/status_bar/audio.vue"),
		StatusSignal: () => import("@/sys/components/status_bar/signal.vue"),
		StatusNotice: () => import("@/sys/components/status_bar/notice.vue"),
		StatusClock: () => import("@/sys/components/status_bar/clock.vue"),
		StatusNotifications: () => import("@/sys/components/status_bar/notifications.vue"),
	},
	data() {
		return {
			audio: {
				source: null,
				time: '00:00',
				playing: false,
			}
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('audio', this.listener_audio);
	},
	methods: {

		toggle_notifications() {
			this.$emit('toggle');
		},

		update_time() {
			if(this.audio.source.audio.duration) {
				this.audio.time = Eljs.secondsToTime(this.audio.source.audio.duration - this.audio.source.audio.currentTime);
			} else {
				this.audio.time = "loading...";
			}
		},

		listener_audio(wsmsg: any) {
			switch(wsmsg.name){
				case 'playing': {
					this.audio.source = wsmsg.payload;
					this.audio.source.audio.addEventListener('play', this.update_time);
					this.audio.source.audio.addEventListener('timeupdate', this.update_time);
					this.audio.playing = true;
					break;
				}
				case 'ended':
				case 'stopped': {
					this.audio.playing = false;
					break;
				}
			}
		},
	},
});
</script>

<style lang="scss">
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';


.os-status {
	position: absolute;
	top: 0;
	left: 0;
	right: 0;
	z-index: 3;
	transform: rotate3d(0, 1, 0, 0deg);
	&.is-invisible {
		margin-top: -100px;
	}
	&.is-dark {
		//@include shadowed_text(#FFFFFF);
		&.is-shaded {
			.os-status_background_bright {
				opacity: 1;
			}
		}
		&.is-audio {
			.os-status_background_bright_audio {
				opacity: 1;
			}
		}
		.os-status_signal {
			&_wifi {
				&_1 {
				  background-image: url(../../assets/framework/dark/signal_indicators/wifi_1.svg);
				}
				&_2 {
				  background-image: url(../../assets/framework/dark/signal_indicators/wifi_2.svg);
				}
				&_3 {
				  background-image: url(../../assets/framework/dark/signal_indicators/wifi_3.svg);
				}
			}
			&_lte {
				&_1 {
				  background-image: url(../../assets/framework/dark/signal_indicators/lte_1.svg);
				}
				&_2 {
				  background-image: url(../../assets/framework/dark/signal_indicators/lte_2.svg);
				}
				&_3 {
				  background-image: url(../../assets/framework/dark/signal_indicators/lte_3.svg);
				}
				&_4 {
				  background-image: url(../../assets/framework/dark/signal_indicators/lte_4.svg);
				}
				&_5 {
				  background-image: url(../../assets/framework/dark/signal_indicators/lte_5.svg);
				}
			}
		}
		.os-status_notification {
			&_bell {
				background-image: url(../../assets/icons/notification-empty_dark.svg);
				&.is-full {
					background-image: url(../../assets/icons/notification-full_dark.svg);
				}
			}
			& span {
				color: $ui_colors_dark_shade_1;
			}
			&.active {
				animation-name: os_status_notification_bright;
				//background-color: lighten($ui_colors_bright_button_cancel, 10%);
				&:hover {
					background-color: lighten($ui_colors_bright_button_cancel, 20%);
				}
			}
		}
		.os-status_user {
			& span {
				color: $ui_colors_dark_shade_1;
			}
		}
		.os-status_notice {
			color: $ui_colors_dark_shade_1;
		}
	}
	&.is-bright {
		//@include shadowed_text(#000000);
		&.is-shaded {
			.os-status_background_dark {
				opacity: 1;
			}
		}
		&.is-audio {
			.os-status_background_dark_audio {
				opacity: 1;
			}
		}
		.os-status_signal {
			&_wifi {
				&_1 {
				  background-image: url(../../assets/framework/bright/signal_indicators/wifi_1.svg);
				}
				&_2 {
				  background-image: url(../../assets/framework/bright/signal_indicators/wifi_2.svg);
				}
				&_3 {
				  background-image: url(../../assets/framework/bright/signal_indicators/wifi_3.svg);
				}
			}
			&_lte {
				&_1 {
				  background-image: url(../../assets/framework/bright/signal_indicators/lte_1.svg);
				}
				&_2 {
				  background-image: url(../../assets/framework/bright/signal_indicators/lte_2.svg);
				}
				&_3 {
				  background-image: url(../../assets/framework/bright/signal_indicators/lte_3.svg);
				}
				&_4 {
				  background-image: url(../../assets/framework/bright/signal_indicators/lte_4.svg);
				}
				&_5 {
				  background-image: url(../../assets/framework/bright/signal_indicators/lte_5.svg);
				}
			}
		}
		.os-status_notification {
			&_bell {
				background-image: url(../../assets/icons/notification-empty_bright.svg);
				&.is-full {
					background-image: url(../../assets/icons/notification-full_bright.svg);
				}
			}
			& span {
				color: $ui_colors_bright_shade_1;
			}
			&.active {
				animation-name: os_status_notification_dark;
				//background-color: $ui_colors_dark_button_cancel;
				&:hover {
					background-color: $ui_colors_bright_button_cancel;
				}
			}
		}
		.os-status_user {
			& span {
				color: $ui_colors_bright_shade_1;
			}
		}
		.os-status_notice {
			color: $ui_colors_bright_shade_1;
		}
	}

	.is-device .hw_frame & {
		padding-right: 120px;
	}

	.os-status_stack {
		position: relative;
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0 14px;
		height: 30px;
		z-index: 2;
		& > div {
			display: flex;
			margin: 0 7px;
			& > div {
				display: flex;
				align-items: center;
			}
			&:first-child {
				& > div {
					margin-right: 10px;
				}
			}
			&:last-child {
				& > div {
					margin-left: 10px;
				}
			}
		}
	}
	.os-status_background {
		z-index: 0;
		& > div {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			opacity: 0;
			transition: opacity 0.3s ease-out;
		}
		&_bright {
			background: linear-gradient(to bottom, rgba($ui_colors_bright_shade_2, 0.6), cubic-bezier(.54,0,.65,1), rgba($ui_colors_bright_shade_2, 0));
		}
		&_dark {
			background: linear-gradient(to bottom, rgba($ui_colors_dark_shade_1, 0.6), cubic-bezier(.54,0,.65,1), rgba($ui_colors_dark_shade_1, 0));
		}
		&_bright_audio {
			background: linear-gradient(to bottom, rgba($ui_colors_bright_button_info, 1), cubic-bezier(.54,0,.65,1), rgba($ui_colors_bright_button_info, 0));
		}
		&_dark_audio {
			background: linear-gradient(to bottom, rgba($ui_colors_dark_button_info, 1), cubic-bezier(.54,0,.65,1), rgba($ui_colors_dark_button_info, 0));
		}
	}
	.os-status_signal {
		flex-grow: 1;
		display: flex;
		justify-content: center;
		width: 26px;
		height: 16px;
		&.is-remote {
			.os-status_signal_wifi {
				opacity: 0;
			}
			.os-status_signal_lte {
				opacity: 1;
			}
		}
		&_wifi {
			&.searching {
				@keyframes signal_pulse {
					0% {
						opacity: 0.3;
					}
					40% {
						opacity: 0.3;
					}
					50% {
						opacity: 1;
					}
					100% {
						opacity: 0.3;
					}
				}
				& .os-status_signal_wifi_1 {
					animation: signal_pulse 3s linear 0s forwards infinite;
				}
				& .os-status_signal_wifi_2 {
					animation: signal_pulse 3s linear 0.4s forwards infinite;
				}
				& .os-status_signal_wifi_3 {
					animation: signal_pulse 3s linear 0.8s forwards infinite;
				}
			}
			&.active {
				& > div {
					opacity: 1;
				}
			}
			width: 21px;
			height: 16px;
			& > div {
				position: absolute;
				background-repeat: no-repeat;
				background-size: 21px;
				width: 21px;
				height: 16px;
				opacity: 0.3;
				transition: opacity 0.3s 0.3s;
			}
		}
		&_lte {
			&.searching {
				@keyframes signal_pulse {
					0% {
						opacity: 0.3;
					}
					40% {
						opacity: 0.3;
					}
					50% {
						opacity: 1;
					}
					100% {
						opacity: 0.3;
					}
				}
				& .os-status_signal_lte_1 {
					animation: signal_pulse 3s linear 0s forwards infinite;
				}
				& .os-status_signal_lte_2 {
					animation: signal_pulse 3s linear 0.2s forwards infinite;
				}
				& .os-status_signal_lte_3 {
					animation: signal_pulse 3s linear 0.4s forwards infinite;
				}
				& .os-status_signal_lte_4 {
					animation: signal_pulse 3s linear 0.6s forwards infinite;
				}
				& .os-status_signal_lte_5 {
					animation: signal_pulse 3s linear 0.8s forwards infinite;
				}
			}
			&.active {
				& > div {
					opacity: 1;
				}
			}
			width: 26px;
			height: 16px;
			opacity: 0;
			& > div {
				position: absolute;
				background-repeat: no-repeat;
				background-size: 21px;
				width: 26px;
				height: 16px;
				opacity: 0.3;
				transition: opacity 0.3s 0.3s;
			}
		}
		& > div {
			position: absolute;
		}
		& span {
			display: block;
			margin-left: 0.5em;
			font-family: "SkyOS-SemiBold";
			transition: color 0.2s 0s ease-out;
		}
	}
	.os-status_notification {
		border-radius: 8px;
		padding: 0.2em 0.4em;
		margin: -0.2em -0.4em;
		cursor: pointer;
		animation-duration: 1s;
		animation-iteration-count: infinite;
		animation-direction: alternate;
		@keyframes os_status_notification_bright {
			from {
				background-color: darken($ui_colors_bright_button_cancel, 10%);
			}
			to {
				background-color: lighten($ui_colors_bright_button_cancel, 10%);
			}
		}
		@keyframes os_status_notification_dark {
			from {
				background-color: darken($ui_colors_dark_button_cancel, 10%);
			}
			to {
				background-color: lighten($ui_colors_dark_button_cancel, 10%);
			}
		}
		&_bell {
			width: 1.2em;
			height: 1.2em;
			background-repeat: no-repeat;
			background-position: center;
			transition: background 0.2s 0s ease-out;
		}
		& span {
			display: block;
			margin-left: 0.1em;
			font-family: "SkyOS-SemiBold";
			transition: color 0.2s 0s ease-out;
		}
	}
	.os-status_user {
		& span {
			transition: color 0.2s 0.1s ease-out;
		}
	}
	.os-status_notice {
		font-family: "SkyOS-SemiBold";
		@media only screen and (max-width: 600px) {
			display: none !important;
		}
	}
}
</style>