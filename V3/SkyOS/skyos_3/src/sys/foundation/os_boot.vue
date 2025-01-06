<template>
	<div class="os-boot" :class="{ 'is-ready': ready, 'is-loading': !loaded }">
		<div class="gooey">
			<span class="dot"></span>
			<span class="dot"></span>
			<span class="dot"></span>
			<span class="dot"></span>
			<span class="dot"></span>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
	props: {
		ready: Boolean
	},
	data() {
		return {
			loaded: false
		};
	},
	methods: {

	},
	mounted() {
		window.requestAnimationFrame(() => {
			this.loaded = true;
		});
	},
	watch: {

	}
});
</script>

<style lang="scss" scoped>
	.os-boot {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		background-color: #020202;
		z-index: 1000;
		transition: opacity 0.3s 0.7s ease-out;
		&.is-ready {
			opacity: 0;
			pointer-events: none;
			.gooey {
				.dot {
					opacity: 0;
				}
			}
		}
		&.is-loading {
			.gooey {
				.dot {
					opacity: 0;
				}
			}
		}
		.gooey {
			position: absolute;
			top: 50%;
			left: 50%;
			width: 160px;
			height: 80px;
			transform: translate(-50%, -50%);
			background: #020202;
			filter: contrast(10);
			mix-blend-mode: lighten;
			.dot {
				$animDuration: 3s;
				position: absolute;
				width: 16px;
				height: 16px;
				top: 26px;
				left: 25px;
				filter: blur(4px);
				border: 2px solid #000;
				border-radius: 50%;
				transform: translateX(0);
				animation: dot 2s infinite;
				animation-direction: alternate;
				animation-timing-function: cubic-bezier(.54,0,.54,1);
				transition: opacity 0.5s ease-out;
				&:nth-child(1) {
					animation-delay: 0s;
					animation-duration: $animDuration;
					background-color: #FFFFFF;
					transition-delay: 0s;
				}
				&:nth-child(2) {
					animation-delay: -$animDuration - 1;
					animation-duration: $animDuration - 0.17;
					background-color: #F00;
					transition-delay: 0.1s;
				}
				&:nth-child(3) {
					animation-delay: -$animDuration - 3;
					animation-duration: $animDuration - 0.83;
					background-color: #F0F;
					transition-delay: 0.2s;
				}
				&:nth-child(4) {
					animation-delay: -$animDuration - 5;
					animation-duration: $animDuration - 1.10;
					background-color: #FF0;
					transition-delay: 0.3s;
				}
				&:nth-child(5) {
					animation-delay: -$animDuration - 7;
					animation-duration: $animDuration - 0.42;
					background-color: #00F;
					transition-delay: 0.4s;
				}
			}
		}

		@keyframes dot {
			0% {
				transform: translateX(0px)
			}
			100% {
				transform: translateX(75px)
			}
		}
	}
</style>