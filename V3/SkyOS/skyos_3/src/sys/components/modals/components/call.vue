<template>
	<div class="call_overlay">
		<audio ref="ringer"></audio>
		<audio ref="audio"></audio>
		<div v-if="ui.isReady">
			<div class="highlight" :style="{ 'left': ui.highlight.location[0] + 'px', 'top': ui.highlight.location[1] + 'px', 'width': ui.highlight.location[2] + 'px', 'height': ui.highlight.location[3] + 'px' }" :class="[{'visible': ui.highlight }, (ui.highlight ? ui.highlight.type : '')]"></div>
			<div class="limiter" :class="{ 'allow': ui.highlight.allow, 'visible': ui.highlight.type }">
				<div :style="{ 'left': ui.highlight.sizes[0][0], 'top': ui.highlight.sizes[0][1], 'width': ui.highlight.sizes[0][2], 'height': ui.highlight.sizes[0][3] }"></div>
				<div :style="{ 'left': ui.highlight.sizes[1][0], 'top': ui.highlight.sizes[1][1], 'width': ui.highlight.sizes[1][2], 'height': ui.highlight.sizes[1][3] }"></div>
				<div :style="{ 'left': ui.highlight.sizes[2][0], 'top': ui.highlight.sizes[2][1], 'width': ui.highlight.sizes[2][2], 'height': ui.highlight.sizes[2][3] }"></div>
				<div :style="{ 'left': ui.highlight.sizes[3][0], 'top': ui.highlight.sizes[3][1], 'width': ui.highlight.sizes[3][2], 'height': ui.highlight.sizes[3][3] }"></div>
			</div>
			<div class="call-info ringer" v-if="!ui.answered">
				<div class="columns columns_margined_half">
					<div class="column_narrow call-info-box">
						<div class="call-info-name">Incoming call from {{ ui.name }}</div>
						<div class="call-info-company" v-if="md.data.Title.length">{{ md.data.Title }}</div>
						<ChatAvatar class="call-info-avatar" :style="ui.media.vuStyle" :member="ui.alias" :member_name="ui.name" />
						<button_action class="pickup" @click.native="start()"></button_action>
						<slider class="theme--bright" @input="volumeAdjust" :value="ui.media.volume"/>
					</div>
				</div>
			</div>


			<div v-else class="call-info call" :class="{ 'pip':  ui.media.pip != '', 'pip-left': ui.media.pip == 'left', 'pip-right': ui.media.pip == 'right', 'out': ui.media.out }">
				<div class="columns columns_margined_half">
					<div class="column_narrow call-info-box">
						<div class="call-info-time">{{ ui.media.time }}</div>
						<ChatAvatar class="call-info-avatar" :style="ui.media.vuStyle" :member="ui.alias" :member_name="ui.name" />
						<div class="call-info-name">{{ ui.name }}</div>
						<div class="call-info-company" v-if="md.data.Title.length">{{ md.data.Title }}</div>
						<div class="state-control">
							<slider class="theme--bright" @input="volumeAdjust" :value="ui.media.volume"/>
							<div>
								<button_action class="hangup" @click.native="close()"></button_action>
								<button_action v-if="ui.media.el" class="pause" :class="{ 'paused': ui.media.el.paused }" @click.native="toggle()"></button_action>
							</div>
						</div>
					</div>
					<div class="column call-info-chat">
						<ChatFrame v-if="ui.isReady" class="theme--bright" :noAvatar="true" :thread="ui.memos" :hasScroll="false" />
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';
import moment from 'moment';
import ChatAvatar from '@/sys/components/messaging/chat_avatar.vue';
import ChatFrame from "@/sys/components/messaging/chat.vue"

export default Vue.extend({
	name: "call_overlay",
	props: ['md'],
	components: {
		ChatAvatar,
		ChatFrame,
	},
	data() {
		return {
			ui: {
				isReady: false,
				alias: '',
				name: '',
				answered: false,
				highlight: {
					type: null,
					hook: '',
					allow: false,
					location: [0,0,0,0],
					sizes: [
						['0','0','0','0'],
						['0','0','0','0'],
						['0','0','0','0'],
						['0','0','0','0'],
					],
				},
				media: {
					ringer_el: null as HTMLMediaElement,
					el: null as HTMLMediaElement,
					volume: 30,
					time: '00:00',
					pip: '',
					out: false,
					donePlaying: false,
					autoClose: false,
					ready: false,
					covered: false,
					coverLabel: '',
					vuStyle: '',
					vuInterval: null,
					nextClickHook: '',
					nextClickHookFn: () => {}
				},
				memos: {
					Messages: [],
					Members: {}
				}
			},
			timeline: []
		}
	},
	mounted() {
		this.timeline = [];

		fetch(this.md.data.Meta, { method: 'get' })
		.then(response => response.json())
		.then((data) => {

			this.ui.alias = data.Cast;
			this.ui.name = data.Cast.charAt(0).toUpperCase() + data.Cast.slice(1);
			this.ui.memos.Members[data.Cast] = this.ui.name;
			this.ui.memos.Messages.push({
				Type: 'text',
				Member: data.Cast,
				Sent: new Date(),
				Content: data.Caption.EN
			});

			this.ui.isReady = true;
			document.addEventListener('click', this.clickev);
			this.init();

		}).catch((err) => {

		});

	},
	beforeDestroy() {
		clearInterval(this.ui.media.vuInterval);
		document.removeEventListener('click', this.clickev);
	},
	methods: {
		init() {

			// Call
			let timelineIndex = 0;
			this.ui.media.ringer_el = this.$refs.ringer as HTMLMediaElement;
			this.ui.media.el = this.$refs.audio as HTMLMediaElement;
			this.ui.media.el.ontimeupdate = (ev) => {

				var a = moment().add(this.ui.media.el.currentTime, 'seconds');
				var b = moment();

				const min = a.diff(b, 'minutes');
				const sec = (a.diff(b, 'seconds') - (min * 60));

				this.ui.media.time = min.toLocaleString('en', {minimumIntegerDigits:2, useGrouping:false}) + ':' + sec.toLocaleString('en', {minimumIntegerDigits:2, useGrouping:false});

				if(this.timeline[timelineIndex]) {
					if(this.ui.media.el.currentTime > this.timeline[timelineIndex].timecode) {
						this.timeline[timelineIndex].fn();
						timelineIndex++;
					}
				}
			}
			this.ui.media.el.onended = (ev) => {
				this.ui.media.out = true;
				this.ui.media.donePlaying = true;
				if(this.ui.media.autoClose) {
					setTimeout(() => {
						this.$emit('close', true);
					}, 400);
				}
			}
			this.ui.media.el.oncanplay = (ev) => {
				this.ui.media.ready = true;
				this.ui.media.el.volume = this.ui.media.volume / 100;
				//this.ui.media.el.currentTime = 20;
				//this.ui.media.el.playbackRate = 10;

				var context = new AudioContext();
				var analyser = context.createAnalyser();
				var source = context.createMediaElementSource(this.ui.media.el);
				source.connect(analyser);
				source.connect(context.destination); // connect the source to the destination
				analyser.connect(context.destination); // chrome needs the analyser to be connected too...

				clearInterval(this.ui.media.vuInterval);
				this.ui.media.vuInterval = setInterval(() => {
					const fftSize = 1024;
					const timeData = new Uint8Array(fftSize);
					let i = 0;
					var total = i = 0, percentage, float, rms, db;
					analyser.getByteTimeDomainData(timeData);
					while ( i < fftSize ) {
						float = ( timeData[i++] / 0x80 ) - 1;
						total += ( float * float );
					}
					rms = Math.sqrt(total / fftSize);
					db  = 20 * ( Math.log(rms) / Math.log(10) );
					// sanity check
					db = Math.max(-48, Math.min(db, 0));
					percentage = 100 + ( db * 2.083 );

					if(!this.ui.media.el.paused) {
						this.ui.media.vuStyle = 'box-shadow: 0 0 ' + percentage + 'px #FFFFFF';
					} else {
						this.ui.media.vuStyle = '';
					}
				}, 60);

			}
			this.ui.media.el.src = this.md.data.URL;

			// Ringtone
			this.ui.media.ringer_el.volume = this.ui.media.volume / 100;
			this.ui.media.ringer_el.src = this.$os.api.getCDN('sounds', 'effects/ringtone.mp3');
			this.ui.media.ringer_el.loop = true;
			this.ui.media.ringer_el.play();
		},
		start() {
			this.ui.media.ringer_el.pause();
			this.ui.answered = true;
			this.ui.media.el.play();
		},
		close() {
			this.ui.media.out = true;
			this.ui.highlight.type = null;
			this.ui.highlight.allow = true;
			let interval = setInterval(() => {
				this.ui.media.el.volume *= 0.9;
				if(this.ui.media.el.volume < 0.01) {
					this.ui.media.el.volume = 0;
					clearInterval(interval);
				}
			}, 10);
			setTimeout(() => {
				this.$emit('close', true);
			}, 500);
		},
		toggle() {
			this.ui.answered = true;
			if(this.ui.media.el.paused) {
				this.ui.media.el.play();
			} else {
				this.ui.media.el.pause();
			}
		},
		volumeAdjust(val :number) {
			if(this.ui.media.el) {
				this.ui.media.volume = val
				this.ui.media.el.volume = this.ui.media.volume / 100;
				this.ui.media.ringer_el.volume = this.ui.media.volume / 100;
			}
		},
		setShadeHook(hook :string, padding :number[], allow :boolean) {
			this.ui.highlight.type = 'icon';
			this.ui.highlight.allow = allow;
			this.ui.highlight.hook = hook;
			const hookEl = document.querySelectorAll("[data-thook='" + hook + "']");
			if(hookEl.length) {
				const pRect = document.getElementById('app').getBoundingClientRect();
				const rect = hookEl[0].getBoundingClientRect();
				const paddedRect = {
					left: Math.round(rect.left - pRect.left) - padding[0],
					top: Math.round(rect.top - pRect.top) - padding[1],
					width: Math.round(rect.width) + (padding[0] + padding[2]),
					height: Math.round(rect.height) + (padding[1] + padding[3]),
				}
				this.ui.highlight.location = [paddedRect.left, paddedRect.top, paddedRect.width, paddedRect.height];
				this.ui.highlight.sizes[0] = ['0', '0', '100%', (paddedRect.top) + 'px'];
				this.ui.highlight.sizes[1] = ['0', (paddedRect.top) + 'px', (paddedRect.left) + 'px', '100%'];
				this.ui.highlight.sizes[2] = [(paddedRect.left + paddedRect.width) + 'px', (paddedRect.top) + 'px', '100%', '100%'];
				this.ui.highlight.sizes[3] = [(paddedRect.left) + 'px', (paddedRect.top + paddedRect.height) + 'px', (paddedRect.width) + 'px', '100%'];
			}
		},
		clickev(ev :MouseEvent) {
			const nodes = Eljs.getDOMParents(ev.target as Node);
			const found = nodes.find(x => {
				const dataset = ((x as HTMLElement).dataset);
				if(dataset) {
					if(dataset.thook) {
						if(dataset.thook == this.ui.media.nextClickHook) {
							this.ui.media.nextClickHook = '';
							this.ui.media.nextClickHookFn();
						}
					}
				}
			})
		}
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
.call_overlay {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	z-index: 98;
	pointer-events: none;
	$transition: 1s cubic-bezier(.22,0,0,1);

	.theme--bright &,
	&.theme--bright {
		.video {
			background: $ui_colors_bright_shade_1;
		}
	}
	.theme--dark &,
	&.theme--dark {
		.video {
			background: $ui_colors_dark_shade_1;
		}
	}


	.limiter {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		z-index: 1;
		opacity: 0;
		transition: opacity 0.2 cubic-bezier(.22,0,0,1);
		pointer-events: all;
		&.allow {
			pointer-events: none;
		}
		&.visible {
			opacity: 1;
			transition: opacity 1s $transition;
			pointer-events: all;
			&.allow {
				pointer-events: none;
				& > div {
					pointer-events: all;
				}
			}
			& > div {
				pointer-events: all;
			}
		}
		& > div {
			position: absolute;
			background: rgba(0,0,0,0.4);
			//backdrop-filter: blur(1px);
			pointer-events: none;
			transition: background $transition, left $transition, top $transition, width $transition, height $transition;
		}
	}
	.highlight {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		z-index: 3;
		opacity: 0;
		transition: opacity 0.2s cubic-bezier(.22,0,0,1);
		&:after {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			margin: -3px;
			border: 3px solid transparent;
			box-shadow: 0 0 0 transparent;
			border-radius: 10px;
			content: '';
			transition: border $transition, box-shadow $transition;
		}
		&.icon {
			width: 100px;
			height: 100px;
			box-sizing: border-box;
			opacity: 1;
			transition: opacity 0.5s $transition, left $transition, top $transition, width $transition, height $transition;
			&::after {
				@keyframes tutorial_overlay_icon_pulse {
					from {
						box-shadow: 0 0 80px rgba(orange, 1);
					}
					to {
						box-shadow: 0 0 80px rgba(orange, 0.6);
					}
				}
				border: 3px solid orange;
				animation-name: tutorial_overlay_icon_pulse;
				animation-duration: 2s;
 				animation-iteration-count: infinite;
  				animation-direction: alternate;
			}
		}
	}

	.ringer {
		.call-info {
			&-name {
				font-size: 1.6em;
				margin-top: 0;
				font-family: "SkyOS-SemiBold";
			}
			&-avatar {
				width: 180px;
				height: 180px;
				margin-top: 4em;
				margin-bottom: 4em;
				animation-name: call-pulse;
				animation-duration: 2.3s;
				animation-iteration-count: infinite;
				transition: box-shadow 0.01s ease-out;
				@keyframes call-pulse {
					0%   { box-shadow: 0 0 10px rgba(#FFFFFF, 0.2); }
					6%   { box-shadow: 0 0 20px $ui_colors_bright_button_info; }
					12%  { box-shadow: 0 0 10px rgba(#FFFFFF, 0.2); }
					18%  { box-shadow: 0 0 40px $ui_colors_bright_button_info; }
					24%  { box-shadow: 0 0 10px rgba(#FFFFFF, 0.2); }
					30%  { box-shadow: 0 0 60px $ui_colors_bright_button_info; }
					100% { box-shadow: 0 0 10px rgba(#FFFFFF, 0.2); }
				}
			}
		}
		.slider {
			width: 120px;
			height: 30px;
			margin-top: 30px;
			margin-bottom: 0;
		}
		.button_action {
			&.pickup {
				width: 120px;
				height: 60px;
				border-radius: 16px;
				padding: 0;
				background-color: $ui_colors_bright_button_go;
				background-image: url(../../../../sys/assets/icons/bright/answer.svg);
				background-position: center;
				background-repeat: no-repeat;
				&:hover {
					background-color: darken($ui_colors_bright_button_go, 10%);
					&:active {
						background-color: darken($ui_colors_bright_button_go, 15%);
					}
				}
			}
		}
	}

	.call {
		&:hover {
			.state-control {
				opacity: 1;
				transition: margin $transition, opacity 0.3s cubic-bezier(.22,0,0,1);
			}
		}
		&.pip {
			&-left {
				bottom: 64px;
				left: 4px;
				width: 185px;
				height: 260px;
				&.out {
					transform: translateX(-150%);
				}
			}
			&-right {
				bottom: 64px;
				right: 4px;
				width: 185px;
				height: 260px;
				&.out {
					transform: translateX(150%);
				}
			}
			.state-control {
				margin: 0;
			}
			.call-info {
				&-avatar {
					width: 80px;
					height: 80px;
				}
			}
		}
		.call-info {
			&-avatar {
				cursor: pointer;
			}
		}
		.call-cover {
			display: flex;
			align-items: center;
			justify-content: center;
			position: absolute;
			text-align: center;
			font-family: "SkyOS-SemiBold";
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			padding: 5px;
			color: #FFF;
			opacity: 0;
			pointer-events: none;
			background: rgba(0,0,0,0);
			transition: opacity 0.4s ease-out, background 0.4s ease-out;
			&.visible {
				opacity: 1;
				background: rgba(0,0,0,0.8);
			}
		}
	}

	.call-info {
		display: flex;
		align-items: center;
		justify-content: center;
		position: absolute;
		right: 0;
		bottom: 0;
		width: 100%;
		height: 100%;
		pointer-events: all;
		background: rgba(10,10,10,0.9);
		backdrop-filter: blur(5px);
		border-radius: 16px;
		box-shadow: 0 3px 6px rgba(0,0,0,0.7), 0 7px 30px rgba(0,0,0,0.5);
		transition: transform $transition, bottom $transition, left $transition, right $transition, width $transition, height $transition;
		z-index: 5;
		&.loading {
			opacity: 0;
		}
		&-box {
			display: flex;
			flex-direction: column;
			align-items: center;
		}
		&-avatar {
			width: 120px;
			height: 120px;
			border-radius: 50%;
			background: #CCC;
			background-size: cover;
			background-repeat: no-repeat;
			transition: box-shadow 0.3s ease-out, width 0.3s ease-out, height 0.3s ease-out;
		}
		&-time {
			text-align: center;
			color: #FFF;
			margin-bottom: 0.75em;
			font-size: 1.2em;
			font-family: "SkyOS-SemiBold";
		}
		&-name {
			text-align: center;
			color: #FFF;
			margin-top: 0.75em;
			font-size: 1.2em;
			font-family: "SkyOS-SemiBold";
		}
		&-company {
			text-align: center;
			color: #FFF;
			font-size: 1em;
		}
		&-chat {
			max-width: 400px;
		}

		.state-control {
			bottom: 6px;
			right: 6px;
			left: 6px;
			margin: 16px;
			opacity: 0.2;
			z-index: 6;
			transition: margin $transition, opacity 2s 3s cubic-bezier(.22,0,0,1);
			& > div {
				display: flex;
				.slider {
					width: 70px;
					//margin-right: 10px;
				}
			}
			.slider {
				width: 120px;
				height: 20px;
				margin: 0 auto;
				margin-top: 10px;
				margin-bottom: 0;
			}
			.button_action {
				margin-right: 8px;
				background-position: center;
				background-repeat: no-repeat;
				&:last-child {
					margin-right: 0;
				}
				&.pause {
					width: 60px;
					height: 30px;
					border-radius: 8px;
					padding: 0;
					background-color: $ui_colors_bright_button_go;
					background-image: url(../../../../sys/assets/icons/bright/pause.svg);
					&:hover {
						background-color: darken($ui_colors_bright_button_go, 5%);
						&:active {
							background-color: darken($ui_colors_bright_button_go, 15%);
						}
					}
					&.paused {
						background-image: url(../../../../sys/assets/icons/bright/play.svg);
					}
				}
				&.hangup {
					width: 60px;
					height: 30px;
					border-radius: 8px;
					padding: 0;
					background-color: $ui_colors_bright_button_cancel;
					background-image: url(../../../../sys/assets/icons/bright/hangup.svg);
					&:hover {
						background-color: darken($ui_colors_bright_button_cancel, 10%);
						&:active {
							background-color: darken($ui_colors_bright_button_cancel, 15%);
						}
					}
				}
			}
		}
	}

}
</style>