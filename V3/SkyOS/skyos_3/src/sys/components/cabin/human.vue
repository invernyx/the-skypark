<template>
	<div
		class="human-icon"
		:class="[
			'human-icon-' + human.state.icon,
			'human-icon-type-' + human.type,
			'human-icon-action-' + (show_actions !== false ? human.state.action : 'none'),
			'human-icon-sub-' + (show_actions !== false ? human.state.sub_action : 'none'),
			{
				'human-icon-window-right': cabin && state ? (human.state.x == (cabin.levels[state.level][2]) && human.state.action == 'seated') : false ,
				'human-icon-window-left': cabin && state ? ( human.state.x == (cabin.levels[state.level][0] + cabin.levels[state.level][2]) - 1 && human.state.action == 'seated') : false,
			}
		]">
		<div>
			<div class="option"></div>
			<div class="seatbelt"></div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
export default Vue.extend({
	props:{
		human :Object,
		cabin :Object,
		state :Object,
		show_actions :Boolean,
	},
	data() {
		return {

		}
	},
	methods: {
	},
	mounted() {
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

	.human-icon {
		position: relative;
		width: var(--human-icon-size);
		height: var(--human-icon-size);
		margin: 0 auto;
		margin-bottom: calc(var(--human-icon-size) * 0.4);
		$offset_x: 100 / 7;
		$offset_y: 100 / 7;
		transition: transform 1s ease-out;

		.theme--bright & {
			&::after {
				background-image: url(../../../sys/assets/pax/bright/cabin_window_right_day.svg);
			}
			&::before {
				background-image: url(../../../sys/assets/pax/bright/cabin_window_left_day.svg);
			}
		}

		.theme--dark & {
			&::after {
				background-image: url(../../../sys/assets/pax/dark/cabin_window_right_day.svg);
			}
			&::before {
				background-image: url(../../../sys/assets/pax/dark/cabin_window_left_day.svg);
			}
		}

		&-neutral {
			& > div:before {
				background-position: $offset_x*0% calc(#{$offset_y}*0%);
			}
		}
		&-happy {
			& > div:before {
				background-position: $offset_x*1% calc(#{$offset_y}*0%);
			}
		}
		&-annoyed {
			& > div:before {
				background-position: $offset_x*2% calc(#{$offset_y}*0%);
			}
		}
		&-angry {
			& > div:before {
				background-position: $offset_x*3% calc(#{$offset_y}*0%);
			}
		}
		&-sleeping {
			& > div:before {
				background-position: $offset_x*4% calc(#{$offset_y}*0%);
			}
		}
		&-fear {
			& > div:before {
				background-position: $offset_x*5% calc(#{$offset_y}*0%);
			}
		}
		&-dead {
			& > div:before {
				background-position: $offset_x*6% calc(#{$offset_y}*0%);
			}
		}
		&-sick {
			& > div:before {
				background-position: $offset_x*7% calc(#{$offset_y}*0%);
			}
		}
		&-window {
			&-left {
				transform: translateX(calc(var(--human-icon-size) * 0.2));
				&::before {
					transform: translateX(calc(var(--human-icon-size) * -0.5)) !important;
					opacity: 1 !important;
				}
			}
			&-right {
				transform: translateX(calc(var(--human-icon-size) * -0.2));
				&::after {
					transform: translateX(calc(var(--human-icon-size) * 0.5)) !important;
					opacity: 1 !important;
				}
			}
		}
		&-type {
			&-flight_attendant {
				& > div {
					& > div {
						background-image: url(../../../sys/assets/pax/icon-attendant.svg);
						height: 200%;
						margin-top: -50%;
					}
				}
			}
		}
		&-action {
			&-walking {
				@keyframes progress-bar-blink {
					0%   { transform: translate(0,0) rotate(0deg) scale(1,1); }
					25%  { transform: translate(2px,1px) rotate(1deg) scale(1.03,1.03); }
					50%   { transform: translate(0,0) rotate(0deg) scale(1,1); }
					75%  { transform: translate(-2px,1px) rotate(-1deg) scale(1.03,1.03); }
					100% { transform: translate(0,0) rotate(0deg) scale(1,1); }
				}
				animation-name: progress-bar-blink;
				animation-duration: 1s;
				animation-iteration-count: infinite;
			}
		}
		&-sub {
			&-working,
			&-movie {
				& > div {
					&:before {
						filter: brightness(90%);
					}
					&:after {
						margin-bottom: calc(var(--human-icon-size) * -0.15);
						background-image: url(../../../sys/assets/pax/icon-movie.svg);
						@keyframes human-movie-flicker {
							0%   { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							10%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							20%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							30%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							40%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							50%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							60%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							70%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							80%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							90%  { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
							100% { filter: blur(5 + random(5) + px); opacity: random(10)/10; }
						}
						animation-name: human-movie-flicker;
						animation-duration: 5s;
						animation-direction: alternate;
						animation-iteration-count: infinite;
					}
				}
			}
			&-music,
			&-movie {
				& > div {
					.option {
						background-image: url(../../../sys/assets/pax/icon-music.svg);
					}
				}
			}
			&-music {
				& > div {
					@keyframes human-music-beat {
						0%   { transform: scale(1,1); }
						20%  { transform: scale(1.02,1.02); }
						100% { transform: scale(1,1); }
					}
					animation-name: human-music-beat;
					animation-duration: 0.5s;
					animation-iteration-count: infinite;
				}
			}
			&-sleeping {
				& > div {
					@keyframes human-sleeping-beat {
						0%   { transform: translateY(0); }
						40%  { transform: translateY(5px); }
						100% { transform: translateY(0); }
					}
					animation-name: human-sleeping-beat;
					animation-duration: 5s;
					animation-iteration-count: infinite;
				}
			}
		}
		& > div {
			width: var(--human-icon-size);
			height: var(--human-icon-size);
			.option {
				position: absolute;
				display: block;
				top: 0;
				bottom: 0;
				right: 0;
				left: 0;
				background-size: contain;
				background-repeat: no-repeat;
				background-position: center;
			}
			.seatbelt {
				position: absolute;
				display: none;
				bottom: 0;
				right: 0;
				left: 0;
				height: 30px;
				margin-bottom: -30px;
				background-image: url(../../../sys/assets/pax/seatbelt.svg);
				background-size: contain;
				background-position: center;
				background-repeat: no-repeat;
			}
			&:before {
				content: '';
				position: absolute;
				display: block;
				top: 0;
				bottom: 0;
				right: 0;
				left: 0;
				background-image: url(../../../sys/assets/pax/icons.svg);
				background-size: 800%;
			}
			&:after {
				content: '';
				position: absolute;
				display: block;
				top: 0;
				bottom: 0;
				right: 0;
				left: 0;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
			}
		}
		&::after,
		&::before {
			content: '';
			position: absolute;
			display: block;
			top: 0;
			bottom: 0;
			opacity: 0;
			width: calc(var(--human-icon-size) * 0.6);
			transition: transform 1s ease-out, opacity 1s ease-out;
			background-size: 90%;
			background-repeat: no-repeat;
			background-position: center;
			z-index: -2;
		}
		&::after {
			transform: translateX(calc(var(--human-icon-size) * 0.5));
			right: 0;
		}
		&::before {
			transform: translateX(calc(var(--human-icon-size) * -0.5));
			left: 0;
		}
	}

</style>