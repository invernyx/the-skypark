<template>
	<div class="audio-player" :class="{ 'playing': playing }">
		<div class="progress" :style="{ 'width': '' + (progress * 100) + '%'}" v-if="playing"></div>
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
			<button_nav class="info compact" shape="forward" @click.native="play_speech()" v-if="!playing">Play Message</button_nav>
			<button_nav class="cancel compact" shape="forward" @click.native="stop_speech()" v-else>{{ time }}</button_nav>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		chat :Object,
		message :Object,
	},
	data() {
		return {
			source: null,
			progress: 0,
			time: '00:00',
			playing: false,
			type: null,
			path: null,
			url: null
		}
	},
	methods: {
		init() {
			const path_spl = this.message.audio_path.split(':');

			this.type = path_spl[0];
			this.path = path_spl[1];
			this.url = this.$os.api.getCDN('sounds', this.type + '/' + this.path + ".mp3");

			const currently_playing = this.$os.audioPlayer.get_playing();
			if(currently_playing.audio) {
				this.playing = currently_playing.audio.src == this.url && !currently_playing.audio.ended;
				if(this.playing) {
					this.source = currently_playing;
					this.source.audio.addEventListener('play', this.update_time);
					this.source.audio.addEventListener('timeupdate', this.update_time);
				}
			}
		},
		play_speech() {
			this.time = "Loading...";
			this.$os.audioPlayer.ingest(this.url, this.message.content_type, this.chat, this.message, true);
		},
		stop_speech() {
			this.$os.audioPlayer.stop();
		},
		update_time() {
			if(this.source.audio.duration) {
				this.progress = (1 / this.source.audio.duration) * this.source.audio.currentTime;
				this.time = Eljs.secondsToTime(this.source.audio.duration - this.source.audio.currentTime);
			} else {
				this.progress = 0;
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
						this.playing = this.source.audio.src == this.url;
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
	mounted() {
		this.init();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('audio', this.listener_audio);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.on('audio', this.listener_audio);
		if(this.source) {
			this.source.audio.removeEventListener('play', this.update_time)
			this.source.audio.removeEventListener('timeupdate', this.update_time)
		}
		this.playing = false;
	},
	watch: {
		chat: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.audio-player {

	.theme--bright & {
		background: $ui_colors_bright_shade_0;
		@include shadowed_shallow($ui_colors_bright_shade_5);
		.progress {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		.waves {
			& > div {
				background-color: $ui_colors_bright_button_info;
			}
		}
		.button_nav {
			color: $ui_colors_bright_shade_0;
		}
	}

	.theme--dark & {
		color: $ui_colors_dark_shade_0;
		background: rgba($ui_colors_dark_shade_4, 0.9);
		@include shadowed_shallow($ui_colors_dark_shade_0);
		.progress {
			background-color: rgba($ui_colors_dark_shade_0, 0.3);
		}
		.waves {
			& > div {
				background-color: $ui_colors_dark_shade_2;
			}
		}
		.button_nav {
			color: $ui_colors_dark_shade_5;
		}
	}

	$borderRadius: 0.75em;
	position: relative;
	overflow: hidden;
	border-radius: $borderRadius;
	padding: 0.5em 0.75em;
	min-width: 170px;
	flex-grow: 1;
	margin-bottom: 8px;
	display: flex;

	.content {
		display: flex;
		flex-direction: row;
		align-items: center;
		flex-grow: 1;
		padding: 0;
		margin: 0;
		z-index: 2;
		justify-content: space-between;
	}

	.progress {
		position: absolute;
		top: 0;
		left: 0;
		bottom: 0;
		right: 0;
		transform-origin: left;
	}

	.waves {
		display: flex;
		height: 14px;
		margin-right: 8px;
		& > div {
			@keyframes chat_message_type_audio_wave {
				0%  { transform: scaleY(0.2); }
				25% { transform: scaleY(0.6); }
				50% { transform: scaleY(0.4); }
				75% { transform: scaleY(0.8); }
				100% { transform: scaleY(1); }
			}
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

	&.playing {
		.waves {
			& > div {
				animation-name: chat_message_type_audio_wave;
			}
		}
	}
}
</style>