<template>
 	<div class="hw" v-if="ready" :class="
	[
	 	!window_device ? 'is-device' : 'is-window',
		{
			'frameless': window_frameless,
			'device-apple': this.$root.$data.state.device.is_apple_webkit
		}
	]">
		<div v-if="$root.$data.state.device.is_electron && $os.routing.activeAppRouter">
			<div class="hw_frame">
				<div class="hw_frame_bezel" :style="'background-color:' + deviceColor" @dblclick="dClickBezel" @mousedown="onMouseDown" ref="hw_frame_bezel">
					<!--
					<svg viewBox="0 0 586 770" v-if="!window_device">
						<g>
							<path d="M574.7,11.3C563.5,0.1,551.6,0,532.9,0H53.1C34.4,0,22.5,0.1,11.3,11.3C0.1,22.5,0,34.4,0,53.1v663.8
								c0,18.7,0.1,30.6,11.3,41.8C22.5,769.9,34.4,770,53.1,770h479.8c18.7,0,30.6-0.1,41.8-11.3c11.2-11.2,11.3-23.2,11.3-41.8V53.1
								C586,34.4,585.9,22.5,574.7,11.3z" :style="'fill:' + deviceColor"/>
						</g>
					</svg>
					-->
				</div>
				<div class="hw_frame_screen">
					<App class="hw_frame_pixels" :class="theme"/>
				</div>
				<div class="hw_frame_chin">
					<svg viewBox="0 0 154 41" :style="'display:block'">
						<path :style="'fill:none;stroke:#000000;stroke-width:1'" d="M0.8,2c10.7,0,20.8,4.2,28.4,11.8C36.4,21,46.1,25,56.3,25H136c8.8,0,16,7.2,16,16" />
						<path :style="'fill:' + deviceColor" d="M0.8,0v2c10.7,0,20.8,4.2,28.4,11.8C36.4,21,46.1,25,56.3,25H136c8.8,0,16,7.2,16,16h2V0H0.8z" />
					</svg>
				</div>
				<div class="hw_frame_outline"></div>
				<div class="hw_frame_logo"></div>
			</div>
			<div class="w_controls">
				<div class="w_control w_control_p42" @mousedown="holdP42" @mouseup="releaseP42" @mouseleave="releaseP42"></div>
				<div class="w_control w_control_pin" @click="handlePin"></div>
				<div class="w_control w_control_min" @click="handleMin"></div>
				<div class="w_control w_control_cls" @click="handleCls"></div>
			</div>
			<div class="hw_wake" :style="wakeColor ? 'background-color:' + wakeColor : ''" @mousedown="holdWake" @mouseup="releaseWake" @mouseleave="releaseWake"></div>
			<!--
			<div class="hw_sizes">
				<div class="hw_resize" :style="'height:' + (size_2 * 80) + 'px'" @click="clickResize(2)">
					<div></div>
				</div>
				<div class="hw_resize" :style="'height:' + (size_1 * 80) + 'px'" @click="clickResize(1)">
					<div></div>
				</div>
			</div>
			-->
		</div>
		<App class="hw_blank" :class="theme" v-else/>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from "../libraries/elem";
import App from './os_base.vue'

export default Vue.extend({
	components: {
		App: App
	},
	data() {
		return {
			theme: this.$os.userConfig.get(['ui','theme']),
			window_frameless: false,
			window_device: false,
			ipcR: null,
			wakeHeld: null,
			p42Held: null,
			ready: false,
			deviceColor: '#000',
			wakeColor: '#BBB',
			animationId: null,
			moved: false,
			mouseX: 0,
			mouseY: 0,
			size1: 1,//this.$os.userConfig.get(['ui','device_resize','size_1']),
			size2: 1,//this.$os.userConfig.get(['ui','device_resize','size_2'])
		};
	},
	methods: {
		handlePin() {
			this.$os.api.ipcR.send('asynchronous-message', 'pin');
		},

		changeSize(change :any) {
			this.$os.api.ipcR.send('asynchronous-message', 'zoom', change);
		},

		handleMin() {
			this.$os.api.ipcR.send('asynchronous-message', 'minimize');
		},

		handleCls() {
			this.ready = false;
			this.$os.api.ipcR.send('asynchronous-message', 'quit');
		},

		holdP42() {
			clearTimeout(this.p42Held);
			this.p42Held = setTimeout(() => {
				this.p42Held = null;
				this.$os.api.ipcR.send('asynchronous-message', 'center');
			}, 2000);
		},

		releaseP42() {
			if(this.p42Held != null) {
				clearTimeout(this.p42Held);
				this.p42Held = null;
				window.open("https://parallel42.com/");
			}
		},

		holdWake() {
			clearTimeout(this.wakeHeld);
			this.wakeHeld = setTimeout(() => {
				(this.$root as any).reset();
				this.wakeHeld = null;
				this.$os.routing.goTo({ name: 'p42_onboarding' })
			}, 8000);
		},

		releaseWake() {
			if(this.wakeHeld != null) {
				clearTimeout(this.wakeHeld);
				this.wakeHeld = null;
				this.$os.eventsBus.Bus.emit('os', { name: 'wake', payload: null });
			}
		},

		dClickBezel(e) {
			const padding = 20;
			const bezel_el = this.$refs.hw_frame_bezel as HTMLElement;
			const bezel_size = [bezel_el.offsetWidth, bezel_el.offsetHeight];
			const offset_el = [bezel_el.offsetTop, bezel_el.offsetLeft];
			const click_loc = [e.offsetX, e.offsetY];
			let side = null as string;

			if(click_loc[1] < padding) {
				side = 'b';
			} else if(click_loc[1] > bezel_size[1] - padding) {
				side = 't';
			}

			if(click_loc[0] < padding) {
				side = 'r';
			} else if(click_loc[0] > bezel_size[0] - padding) {
				side = 'l';
			}

			if(side) {
				this.$os.api.ipcR.send('asynchronous-message', 'stow', side);
			}
		},

		onMouseDown(e) {
			this.mouseX = e.clientX;
			this.mouseY = e.clientY;
			this.moved = false;

			document.addEventListener('mousemove', this.onMouseMove);
			document.addEventListener('mouseup', this.onMouseUp);
			cancelAnimationFrame(this.animationId);
			requestAnimationFrame(this.moveWindow);
		},

		onMouseUp(e) {
			if(this.moved) {
				this.$os.api.ipcR.send('asynchronous-message', 'moved');
			}
			document.removeEventListener('mousemove', this.onMouseMove);
			document.removeEventListener('mouseup', this.onMouseUp);
			cancelAnimationFrame(this.animationId);
		},

		onMouseMove(e) {
			this.moved = true;
		},

		moveWindow(e) {
			if(this.moved) {
				this.$os.api.ipcR.send('asynchronous-message', 'moving', [this.mouseX, this.mouseY]);
			}
			this.animationId = requestAnimationFrame(this.moveWindow);
		},

		clickResize(sel :number) {
			this.$os.userConfig.set(['ui','device_resize','selected'], sel);
			switch(sel) {
				case 1: {
					this.changeSize(this.$os.userConfig.get(['ui','device_resize','size_1']));
					break;
				}
				case 2: {
					this.changeSize(this.$os.userConfig.get(['ui','device_resize','size_2']));
					break;
				}
			}
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'disconnect': {
					this.deviceColor = "#000";
					this.wakeColor = null;
					break;
				}
				case 'failsconnect': {
					this.deviceColor = "#000";
					this.wakeColor = null;
					break;
				}
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							if(wsmsg.payload.dev) {
								this.deviceColor = "#000"; //"#500";
								this.wakeColor = null;
							} else if(wsmsg.payload.beta) {
								this.deviceColor = "#000";
								this.wakeColor = "#E22";
							} else {
								this.deviceColor = "#000";
								this.wakeColor = null;
							}
							break;
						}
					}
					break;
				}
			}
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
	created() {
		if (navigator.userAgent.indexOf('Electron') >= 0) {
			const { ipcRenderer, ipcMain } = window.require('electron');

			this.$root.$data.state.device.is_electron = true;
			this.$root.$data.state.device.hardware = this;

			this.$os.api.ipcR = ipcRenderer;
			ipcRenderer.on('asynchronous-reply', (event, arg, arg2) => {

				let argSpl = arg.split(':');
				switch(argSpl[0]){
					case 'wake': {
						this.$os.eventsBus.Bus.emit('os', { name: 'wake', payload: null });
						break;
					}
					case 'unlock': {
						this.$os.eventsBus.Bus.emit('os', { name: 'unlock', payload: null });
						break;
					}
					case 'sleep': {
						this.$os.routing.goTo({ name: 'p42_sleep' });
						break;
					}
					case 'minimized': {
						break;
					}
					case 'restored': {
						break;
					}
					case 'framed': {
						this.$os.userConfig.set(['ui','framed'], arg2);
						break;
					}
					case 'zoom': {
						switch(this.$root.$data.config.ui.device_resize.selected) {
							case 1: {
								this.$os.userConfig.set(['ui','device_resize','size_1'], Eljs.round(arg2, 2));
								break;
							}
							case 2: {
								this.$os.userConfig.set(['ui','device_resize','size_2'], Eljs.round(arg2, 2));
								break;
							}
						}
						break;
					}
					case 'pin': {
						const state = arg2;
						const pinEl = document.getElementsByClassName('w_control_pin')[0];
						if(state){
							pinEl.classList.add('active');
						} else {
							pinEl.classList.remove('active');
						}
						this.$os.userConfig.set(['ui','topmost'], state)
						break;
					}
				}
			});

			setTimeout(() => {
				(this.$root as any).setTopmost(this.$os.userConfig.get(['ui','topmost']));
			}, 1000);
		}

		this.$os.eventsBus.Bus.on('statechange', (ev) => {
			switch(ev.name[0]) {
				case 'device': {
					switch(ev.name[1]) {
						case 'windowed': {
							this.window_device = ev.value;
							break;
						}
						case 'frameless': {
							this.window_frameless = ev.value;
							break;
						}
					}
					break;
				}
			}
		});

	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
	},
	mounted() {
		this.ready = true;
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.hw {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
}

.is-window {
	opacity: 1;
	-webkit-app-region: drag;
	.hw_frame {
		display: flex;
		position: absolute;
		justify-content: center;
		align-items: center;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		overflow: hidden;
		&_bezel {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			background: $ui_colors_bright_shade_4;
			border: 1px solid $ui_colors_bright_shade_3;
			padding: 20px;
		}
		&_screen {
			position: absolute;
			top: 24px;
			right: 2px;
			bottom: 2px;
			left: 2px;
			z-index: 1;
			pointer-events: all;
			-webkit-app-region: no-drag;
			border: 1px solid $ui_colors_bright_shade_5;
		}
		&_pixels {
			width: 556px;
			height: 740px;
		}
	}
	.w_controls {
		position: absolute;
		display: flex;
		z-index: 10;
		right: 4px;
		top: 0;
		padding-top: 3px;
		z-index: 20;
		-webkit-app-region: no-drag;
		pointer-events: all;
		.w_control {
			width: 19px;
			height: 19px;
			background-size: 12px;
			background-repeat: no-repeat;
			background-position: center;
			background-color: #222;
			margin-left: 3px;
			border-radius: 3px;
			transition: background 0.4s ease-out;
			z-index: 20;
			//-webkit-app-region: no-drag;
			&:hover {
				transition: background 0.1s ease-out;
			}
			&_p42 {
				display: none;
			}
			&_pin {
				background-image: url(../assets/hardware/pin.svg);
				&:hover {
					background-color: #444;
				}
				&.active {
					background-color: rgb(70, 70, 200);
				}
			}
			&_min {
				background-image: url(../assets/hardware/min.svg);
				&:hover {
					background-color: #444;
				}
			}
			&_cls {
				background-image: url(../assets/hardware/cls.svg);
				&:hover {
					background-color: rgb(182, 23, 23);
				}
			}
		}
	}
	.hw_blank {
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background: #000;
	}

	&.frameless  {
		.hw_frame {
			&_bezel {
				background: #000000;
				border: none;
				padding: 0;
			}
			&_screen {
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				border: none;
			}
		}
		.w_controls {
			justify-content: flex-end;
			right: 0;
			left: 0;
			top: 0;
			padding: 8px;
			opacity: 0;
			&::before {
				content: '';
				position: absolute;
				bottom: 0;
				right: 0;
				left: 0;
				top: 0;
				-webkit-app-region: drag;
				background: rgba(0,0,0,0.5);
			}
			&:hover {
				opacity: 1;
			}
			.w_control {
				-webkit-app-region: no-drag;
			}
		}
	}
}

.is-device {
	$bezelSize: 14px;
	$bezelRad: 25px;
	opacity: 1;
	transform: translateY(0) perspective(2000px) scale(1) rotate3d(0, 0, 0, 15deg);
	display: flex;
	.hw_frame {
		position: absolute;
		top: 10px;
		right: 4px;
		bottom: 4px;
		left: 8px;
		&_bezel {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			border-radius: $bezelRad + $bezelSize;
			-webkit-app-region: drag;
			path {
				transition: fill 4s ease-out;
			}
		}
		&_outline {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			border: 1px solid rgba(150,150,150,0.8);
			border-radius: $bezelRad + $bezelSize;
			z-index: 100;
			pointer-events: none;
		}
		&_screen {
			position: absolute;
			top: $bezelSize;
			right: $bezelSize;
			bottom: $bezelSize;
			left: $bezelSize;
			z-index: 1;
			pointer-events: all;
			background-color: rgb(2, 2, 2);
			border: 1px solid rgb(2, 2, 2);
			border-radius: $bezelRad;
			border-top-right-radius: 0;
			overflow: hidden;
			-webkit-app-region: no-drag;
		}
		&_pixels {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
		}
		&_chin {
			position: absolute;
			top: 12px;
    		right: 12px;
			width: 152px;
			height: 39px;
    		z-index: 19;
			overflow: hidden;
			pointer-events: none;
		}
	}
	.w_controls {
		position: absolute;
		top: 19px;
    	right: 26px;
		display: flex;
		z-index: 20;
		-webkit-app-region: no-drag;
		pointer-events: all;
		.w_control {
			width: 19px;
			height: 19px;
			background-size: 12px;
			background-repeat: no-repeat;
			background-position: center;
			background-color: rgba(#000, 0.2);
			margin-left: 3px;
			border-radius: 3px;
			transition: background 0.4s ease-out;
			z-index: 20;
			//-webkit-app-region: no-drag;
			&:hover {
				transition: background 0.1s ease-out;
				background-color: rgba(#FFF, 0.2);
			}
			&_p42 {
				background-size: 27px;
				background-image: url(../assets/hardware/p42_white.svg);
				width: 36px;
				background-color: transparent;
			}
			&_pin {
				background-image: url(../assets/hardware/pin.svg);
				&.active {
					background-color: rgb(70, 70, 200);
				}
			}
			&_min {
				background-image: url(../assets/hardware/min.svg);
			}
			&_cls {
				background-image: url(../assets/hardware/cls.svg);
				&:hover {
					background-color: rgb(182, 23, 23);
				}
			}
		}


	}
	.hw_wake {
		position: absolute;
		top: 5px;
		left: 55px;
		width: 35px;
		height: 4px;
		background-color: #AAA;
		border: 1px solid rgba(0,0,0,0.3);
		border-bottom: none;
		border-top-left-radius: 3px;
		border-top-right-radius: 3px;
		z-index: 20;
		overflow: hidden;
		-webkit-app-region: no-drag;
		cursor: pointer;
		&:hover {
			&::after {
				background: rgba(#FFF, 0.3);
				transition: 0.15s ease-out background;
			}
		}
		&:active {
			height: 4px;
			margin-top: 1px;
		}
		&::after {
			content: '';
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			transition: 0.4s ease-out background;
		}
	}

	.hw_sizes {
		position: absolute;
		top: 100px;
		left: 3px;
		z-index: 20;
		-webkit-app-region: no-drag;
		.hw_resize {
			position: relative;
			width: 18px;
			height: 50px;
			margin-bottom: 10px;
			cursor: pointer;
			overflow: hidden;
			&:hover {
				& > div {
					&::after {
						background: rgba(#FFF, 0.3);
						transition: 0.15s ease-out background;
					}
				}
			}
			&:active {
				& > div {
					left: 2px;
				}
			}
			& > div {
				position: absolute;
				top: 0;
				left: 0;
				right: 12px;
				bottom: 0;
				background-color: #AAA;
				border: 1px solid rgba(0,0,0,0.3);
				border-right: none;
				border-top-left-radius: 3px;
				border-bottom-left-radius: 3px;
				transition: 0.15s ease-out background;
				&::after {
					content: '';
					position: absolute;
					top: 0;
					left: 0;
					right: 0;
					bottom: 0;
					transition: 0.4s ease-out background;
				}
			}
		}
	}

	.hw_blank {
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		background: #000;
	}
}

</style>