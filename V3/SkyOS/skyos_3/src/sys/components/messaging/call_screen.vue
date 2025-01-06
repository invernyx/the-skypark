<template>
	<transition :duration="{ enter: 800, leave: 200 }">
		<div class="call-screen" ref="frame_range" :class="{ 'playing': playing, 'collapsed': collapsed }" v-if="shown">
			<div ref="frame">
				<div class="app-box shadowed-deep nooverflow">
					<div class="columns">
						<div class="column">
							<div :class="{ 'h_edge_padding': !collapsed, 'h_edge_padding_half': collapsed }">
								<div class="content">
									<div class="title">{{ chat.handles[0].first_name }}</div>
									<div class="timer" v-if="!collapsed">{{ answered ? time : "Incoming call" }}</div>
									<ChatAvatar :handle="chat.handles[0]" />
									<div class="state">
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
									</div>
								</div>
								<div class="actions">

									<div class="columns columns_margined_half" v-if="!done">
										<div class="column column_narrow">
											<button_action class="cancel icon icon-hang" @click.native="stop_speech()"></button_action>
										</div>
										<div class="column">
											<button_action class="go icon icon-answer" @click.native="answer_speech()" v-if="!playing && !answered"></button_action>
											<button_action class="go icon icon-play" @click.native="play_speech()" v-if="!playing && answered"></button_action>
											<button_action class="info icon icon-pause" @click.native="pause_speech()" v-if="playing && answered"></button_action>
										</div>
										<div class="column column_narrow">
											<button_action class="icon icon-collapse" @click.native="set_collapse(!collapsed)"></button_action>
										</div>
									</div>

									<div class="columns columns_margined_half" v-else>
										<div class="column column_narrow">
											<button_action class="cancel icon icon-hang" @click.native="close()"></button_action>
										</div>
										<div class="column">
											<button_action class="go icon icon-play" @click.native="answer_speech()"></button_action>
										</div>
										<div class="column column_narrow">
											<button_action class="icon icon-collapse" @click.native="set_collapse(!collapsed)"></button_action>
										</div>
									</div>

								</div>
							</div>
						</div>
						<div class="column chat_box" v-if="chat && !collapsed">
							<scroll_view :scroller_offset="{ top: 5, bottom: 5 }">
								<div class="h_edge_padding">
									<ChatUI class="full_width" :data="chat" :no_avatar="true" :no_audio="true" :no_title="true"/>
								</div>
							</scroll_view>
						</div>
					</div>

				</div>
			</div>
		</div>
	</transition>

</template>

<script lang="ts">
import Vue from 'vue';
import ChatAvatar from './chat_avatar.vue';
import Eljs from '@/sys/libraries/elem';
import Message from '@/sys/classes/messaging/message';
import Chat from '@/sys/classes/messaging/chat';
import ChatUI from '@/sys/components/messaging/chat.vue';

export default Vue.extend({
	components: {
		ChatAvatar,
		ChatUI
	},
	data() {
		return {
			collapsed: false,
			auto_answer: -1,
			shown: false,
			answered: false,
			done: false,
			ringer: null as HTMLAudioElement,
			ringer_timer: null,
			message: null as Message,
			chat: null as Chat,
			progress: 0,
			time: '00:00',
			playing: false,
			url_type: null,
			url_path: null,
			url: null,
			source: null,
			drag: {
				pos1: 0,
				pos2: 0,
				pos3: 0,
				pos4: 0
			}
		}
	},
	methods: {
		ring() {
			clearTimeout(this.ringer_timer);
			const url = this.$os.api.getCDN('sounds', "effects/ringtone.mp3");
			this.ring_kill();
			this.ringer = new Audio(url);
			this.ringer.loop = true;
			this.ringer.play();

			if(this.auto_answer > 0) {
				this.ringer_timer = setTimeout(() => {
					this.answer_speech();
				}, this.auto_answer);
			} else {
				this.ringer_timer = setTimeout(() => {
					this.ring_kill();
				}, 10000);
			}
		},
		ring_kill() {
			clearTimeout(this.ringer_timer);
			if(this.ringer){
				this.ringer.pause();
				this.ringer = null;
			}
		},
		answer_speech() {
			this.ring_kill();
			this.ringer = null;
			this.done = false;
			this.answered = true;
			this.time = "Loading...";

			this.$os.audioPlayer.play();
		},
		play_speech() {
			this.$os.audioPlayer.play();
		},
		pause_speech() {
			this.$os.audioPlayer.pause();
			this.playing = false;
		},
		stop_speech() {
			this.$os.audioPlayer.stop();
			this.shown = false;
			this.ring_kill();
		},
		update_time() {
			this.time = Eljs.secondsToTime(this.source.audio.currentTime);
			if(this.source.audio.duration) {
				this.progress = (1 / this.source.audio.duration) * this.source.audio.currentTime;
			} else {
				this.progress = 0;
			}
		},
		close() {
			this.shown = false;
		},
		ended() {
			this.done = true;
			this.playing = false;
		},
		set_collapse(state :boolean) {
			const frame_el = (this.$refs.frame as HTMLElement);
			this.collapsed = state;
			if(state) {
				window.addEventListener('resize', this.document_resize);
				frame_el.addEventListener('mousedown', this.drag_mouse_down);
			} else if(frame_el) {
				window.removeEventListener('resize', this.document_resize);
				frame_el.removeEventListener('mousedown', this.drag_mouse_down);
			}
		},

		document_resize(e) {
			this.drag.pos3 = 0;
			this.drag.pos4 = 0;
			this.element_move(0, 0)
		},

		drag_mouse_down(e) {
			e = e || window.event;
			e.preventDefault();
			this.drag.pos3 = e.clientX;
			this.drag.pos4 = e.clientY;
			document.onmouseup = this.close_drag_element;
			document.onmousemove = this.element_drag;
		},

		element_move(x_offset :number, y_offset :number) {

			const frame_el = (this.$refs.frame as HTMLElement);
			const frame_range_el = (this.$refs.frame_range as HTMLElement);

			this.drag.pos1 = this.drag.pos3 - x_offset;
			this.drag.pos2 = this.drag.pos4 - y_offset;
			this.drag.pos3 = x_offset;
			this.drag.pos4 = y_offset;

			let pos_new_x = (frame_el.offsetLeft - this.drag.pos1);
			let pos_new_y = (frame_el.offsetTop - this.drag.pos2);
			const pos_new_right = pos_new_x + frame_el.clientWidth;
			const pos_new_bottom = pos_new_y + frame_el.clientHeight;

			const parent_w = frame_range_el.clientWidth;
			const parent_h = frame_range_el.clientHeight;

			if(pos_new_right > parent_w) {
				pos_new_x = parent_w - frame_el.clientWidth;
			}

			if(pos_new_bottom > parent_h) {
				pos_new_y = parent_h - frame_el.clientHeight;
			}

			if(pos_new_x < 0) {
				pos_new_x = 0;
			}

			if(pos_new_y < 0) {
				pos_new_y = 0;
			}

			frame_el.style.top = pos_new_y + "px";
			frame_el.style.left = pos_new_x + "px";
		},

		element_drag(e) {
			e = e || window.event;
			e.preventDefault();

			this.element_move(e.clientX, e.clientY)
		},

		close_drag_element() {
			document.onmouseup = null;
			document.onmousemove = null;
		},


		listener_ws(wsmsg: any){
			switch(wsmsg.name[0]){
				case 'messaging': {
					switch(wsmsg.name[1]){
						case 'add': {
							switch(wsmsg.payload.message.content_type){
								case "Call": {
									this.chat = new Chat(Object.assign(wsmsg.payload.chat, {
										messages: [
											wsmsg.payload.message
										]
									}) );
									this.message = this.chat.messages[0];
									const path_spl = this.message.audio_path.split(':');
									this.url_type = path_spl[0];
									this.url_path = path_spl[1];
									this.url = this.$os.api.getCDN('sounds', this.url_type + '/' + this.url_path + ".mp3");
									this.$os.audioPlayer.ingest(this.url, this.message.content_type, this.chat, this.message, false);
									break;
								}
							}
							break;
						}
					}
					break;
				}
			}
		},
		listener_audio(wsmsg: any) {
			switch(wsmsg.name){
				case 'ring': {

					this.set_collapse(false);
					this.answered = false;
					this.playing = false;
					this.done = false;

					this.source = wsmsg.payload;
					this.source.audio.addEventListener('ended', this.ended);
					this.source.audio.addEventListener('play', this.update_time);
					this.source.audio.addEventListener('timeupdate', this.update_time);
					this.shown = true;

					this.ring();
					break;
				}
				case 'playing': {
					this.playing = true;
					break;
				}
				case 'paused': {
					this.playing = false;
					break;
				}
				case 'stopped': {
					this.shown = false;
					this.playing = false;
					break;
				}
			}
		},
	},
	mounted() {
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('audio', this.listener_audio);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('audio', this.listener_audio);
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
		if(this.source.audio) {
			this.source.audio.removeEventListener('play', this.update_time)
			this.source.audio.removeEventListener('timeupdate', this.update_time)
		}
		this.playing = false;
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.call-screen {
	position: absolute;
	display: flex;
	justify-content: center;
	align-items: center;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	z-index: 2;
	backdrop-filter: blur(5px);

	.theme--bright & {
		background: rgba($ui_colors_bright_shade_0, 0.9);
		.waves {
			& > div {
				background-color: $ui_colors_bright_shade_5;
			}
		}
	}

	.theme--dark & {
		background: rgba($ui_colors_dark_shade_0, 0.9);
		.waves {
			& > div {
				background-color: $ui_colors_dark_shade_5;
			}
		}
	}

	&.collapsed {
		top: $status-size;
		bottom: 12px;
		left: 12px;
		right: 12px;
		background: transparent;
		backdrop-filter: none;
		pointer-events: none;
		.collapser {
			display: none;
		}
		& > div {
			position: absolute;
			width: 160px;
			pointer-events: all;
			cursor: grab;
		}
		.chat_avatar {
			width: 42px;
			height: 42px;
			margin-bottom: 18px;
		}
		.waves {
			display: none;
		}
	}

	& > div {
		flex-grow: 1;
		max-width: 500px;
	}

	.title {
		font-size: 18px;
	}

	.chat_avatar {
		width: 100px;
		height: 100px;
		margin-top: 8px;
		margin-bottom: 8px;
	}

	.chat_box {
		width: 250px;
	}

	.content {
		display: flex;
		flex-direction: column;
		align-items: center;
	}

	.waves {
		display: flex;
		height: 24px;
		margin-bottom: 18px;
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
			height: 24px;
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


	&.v-enter {
		& > .app-box {
			transform: translateY(10px);
		}
		opacity: 0;
	}
	&.v-enter-active {
		& > .app-box {
			transition: transform 1s 0.1s cubic-bezier(.37,.87,.5,1);
		}
		transition: opacity 0.3s 0.1s ease-out;
	}
	&.v-leave,
	&.v-enter-to {
		& > .app-box {
			transform: translateY(0px);
		}
		opacity: 1;
	}
	&.v-leave {
		pointer-events: none;
	}
	&.v-leave-active {
		& > .app-box {
			transition: transform 0.1s cubic-bezier(.37,.87,.5,1);
		}
		transition: opacity 0.1s ease-out;
		.simplebar-track {
			display: none;
		}
	}
	&.v-leave-to {
		& > .app-box {
			transform: translateY(0px);
		}
		opacity: 0;
	}

}
</style>