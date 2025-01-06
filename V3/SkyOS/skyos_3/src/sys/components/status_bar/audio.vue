<template>
	<div class="os-status_audio">
		<div class="audio-player" :class="{ 'playing': playing }">
			<div class="content">
				<div class="waves">
					<div></div>
					<div></div>
					<div></div>
					<div></div>
					<div></div>
					<div></div>
					<div></div>
					<div></div>
					<div></div>
				</div>
				<button_nav class="cancel compact" shape="forward" @click.native="stop_speech()">{{ time }}</button_nav>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		source1 :Object,
	},
	methods: {
		stop_speech() {
			this.$os.audioPlayer.stop();
		},
		update_time() {
			if(this.source.audio.duration) {
				this.time = Eljs.secondsToTime(this.source.audio.duration - this.source.audio.currentTime);
			} else {
				this.time = "loading...";
			}
		},
		listener_audio(wsmsg: any) {
			switch(wsmsg.name){
				case 'playing': {
					if(wsmsg.payload.type == "Audio") {
						this.source = wsmsg.payload;
						this.source.audio.addEventListener('play', this.update_time);
						this.source.audio.addEventListener('timeupdate', this.update_time);
						this.playing = true;
					}
					break;
				}
				case 'ended':
				case 'stopped': {
					this.playing = false;
					break;
				}
			}
		},
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('audio', this.listener_audio);
	},
	data() {
		return {
			source: null,
			time: '00:00',
			playing: false,
		}
	},
});
</script>


<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.audio-player {

	.theme--bright & {
		.progress {
			background-color: rgba($ui_colors_bright_shade_0, 0.1);
		}
		.waves {
			& > div {
				background-color: $ui_colors_bright_shade_0;
			}
		}
		.button_nav {
			color: $ui_colors_bright_shade_0;
		}
	}

	.theme--dark & {
		.progress {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		.waves {
			& > div {
				background-color: $ui_colors_dark_shade_5;
			}
		}
		.button_nav {
			color: $ui_colors_dark_shade_5;
		}
	}
	$borderRadius: 0.75em;
	margin-left: 8px;
	position: relative;
	overflow: hidden;
	display: none;

	&.playing {
		display: flex;
	}

	.content {
		display: flex;
		flex-direction: row;
		align-items: center;
	}

	.waves {
		display: flex;
		height: 14px;
		& > div {
			@keyframes chat_message_type_audio_wave {
				0%  { transform: scaleY(0.2); }
				25% { transform: scaleY(0.6); }
				50% { transform: scaleY(0.4); }
				75% { transform: scaleY(0.8); }
				100% { transform: scaleY(1); }
			}
			animation-name: chat_message_type_audio_wave;
			animation-iteration-count: infinite;
			animation-direction: alternate;
			align-self: center;
			width: 4px;
			height: 14px;
			margin-right: 2px;
			border-radius: 2px;
			transform: scaleY(1);
			transition: transform 0.4s ease-out;
			&:nth-child(1) {
				transform: scaleY(0.2);
				animation-delay: 0.3s;
				animation-duration: 1.2s;
			}
			&:nth-child(2) {
				transform: scaleY(0.8);
				animation-delay: 0.9s;
				animation-duration: 1.7s;
			}
			&:nth-child(3) {
				transform: scaleY(0.4);
				animation-delay: 0.1s;
				animation-duration: 1.3s;
			}
			&:nth-child(4) {
				transform: scaleY(0.2);
				animation-delay: 0.36s;
				animation-duration: 1.8s;
			}
			&:nth-child(5) {
				transform: scaleY(0.6);
				animation-delay: 0.2s;
				animation-duration: 1.4s;
			}
			&:nth-child(6) {
				transform: scaleY(1);
				animation-delay: 0.0s;
				animation-duration: 1.7s;
			}
			&:nth-child(7) {
				transform: scaleY(0.7);
				animation-delay: 0.6s;
				animation-duration: 1.9s;
			}
			&:nth-child(8) {
				transform: scaleY(0.2);
				animation-delay: 0.1s;
				animation-duration: 1.3s;
			}
			&:nth-child(9) {
				transform: scaleY(0.4);
				animation-delay: 0.6s;
				animation-duration: 1.8s;
			}
		}
	}

	.button_nav {
		padding-top: 0;
		padding-bottom: 0;
		margin-left: 8px;
	}

}
</style>