<template>
	<div class="progress_karma" :class="[type]">

		<div class="progress-bar">
			<div class="node bad"></div>
			<div class="track">
				<div class="label">Current Karma: <strong>{{ progressed >= 0 ? '+' : '' }}{{ (Math.round(progressed * 10) / 10).toLocaleString('en-gb') }}</strong></div>
				<div class="progress">
					<div class="current" :class="{ 'positive': progressed >= 0 }" :style="'left:' + progressLeft + '%;width:' + progressWidth + '%;'"></div>
					<div class="offset" :class="{ 'push': progressed < -15, 'positive': gain >= 0 }" :style="'left:' + gainLeft + '%;width:' + gainWidth + '%;'">
						<div>{{ (gain > 0 ? '+' : '') + gain.toLocaleString('en-gb') }}</div>
					</div>
				</div>
			</div>
			<div class="node good"></div>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import { writeHeapSnapshot } from 'v8';

export default Vue.extend({
	name: "progress_karma",
	props: ['type', 'gain', 'progressed'],
	data() {
		return {
			progressLeft: 50,
			progressWidth: 0,
			gainLeft: 50,
			gainWidth: 0,
		}
	},
	methods: {
		getRank() {
			const r = 42;

			const p = this.progressed;
			const c = p + this.gain;
			let g = this.gain;

			if(Math.abs(c) > r) {
				if(c > r) {
					g = r - p;
				} else {
					g = r + p;
				}
			}

			const processRatioed = (50 / r * p);
			const gainRatioed = (50 / r * g);

			if(p >= 0) {
				this.progressWidth = processRatioed;
				this.progressLeft = 50;
				if(g >= 0) {
					this.gainLeft = this.progressLeft + this.progressWidth;
					this.gainWidth = gainRatioed;
				} else {
					this.gainLeft = this.progressLeft + this.progressWidth + gainRatioed;
					this.gainWidth = -gainRatioed;
				}
			} else {
				this.progressLeft = 50 + processRatioed;
				this.progressWidth = 50 - this.progressLeft;
				if(g >= 0) {
					this.gainLeft = this.progressLeft;
					this.gainWidth = gainRatioed;
				} else {
					this.gainLeft = this.progressLeft + gainRatioed;
					this.gainWidth = -gainRatioed;
				}
			}
		},
	},
	mounted() {
		this.getRank();
	},
	watch: {
		gain() {
			this.getRank();
		},
		progressed() {
			this.getRank();
		},
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';

.progress_karma {
	width: 100%;

	.theme--bright &,
	&.theme--bright {
		.progress-bar {
			.node {
				&.bad {
					background-color: rgba($ui_colors_bright_shade_4, 1);
					background-image: url(../../../sys/assets/icons/bad.svg);
				}
				&.good {
					background-color: rgba($ui_colors_bright_button_gold, 1);
					background-image: url(../../../sys/assets/icons/bright/good.svg);
				}
			}
			.track {
				.progress {
					background: linear-gradient(to right, rgba($ui_colors_bright_shade_4, 0.8), rgba($ui_colors_bright_shade_5, 0.1), rgba($ui_colors_bright_button_gold, 0.8));
					div {
						color: $ui_colors_bright_shade_5;
						&.current {
							background-color: $ui_colors_bright_shade_4;
							&.positive {
								background-color: $ui_colors_bright_button_gold;
							}
						}
						&.offset {
							background-color: $ui_colors_bright_shade_5;
							&.positive {
								background-color: $ui_colors_bright_shade_5;
							}
						}
					}
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		.progress-bar {
			.node {
				&.bad {
					background-color: rgba($ui_colors_dark_shade_1, 1);
					background-image: url(../../../sys/assets/icons/bad.svg);
				}
				&.good {
					background-color: rgba($ui_colors_dark_button_gold, 1);
					background-image: url(../../../sys/assets/icons/bright/good.svg);
				}
			}
			.track {
				.progress {
					background: linear-gradient(to right, rgba($ui_colors_dark_shade_1, 0.8), rgba($ui_colors_dark_shade_5, 0.1), rgba($ui_colors_dark_button_gold, 0.8));
					div {
						color: $ui_colors_dark_shade_5;
						&.current {
							background-color: $ui_colors_dark_shade_1;
							&.positive {
								background-color: $ui_colors_dark_button_gold;
							}
						}
						&.offset {
							background-color: $ui_colors_dark_shade_5;
							box-shadow: 0 0 20px $ui_colors_dark_shade_5;
							&.positive {
								background-color: $ui_colors_dark_shade_5;
								box-shadow: 0 0 20px $ui_colors_dark_shade_5;
							}
						}
					}
				}
			}
		}
	}

	.progress-bar {
		display: flex;
		align-items: center;
		.node {
			display: flex;
			height: 2em;
			width: 2em;
			border-radius: 1em;
			font-family: "SkyOS-SemiBold";
			align-items: center;
			justify-content: center;
			background-position: center;
			background-repeat: no-repeat;
			&.bad {
				background-size: 125%;
				background-position: center 170%;
			}
			&.good {
				background-size: 1.25em;
			}
		}
		.track {
			flex-grow: 1;
			margin: 4px;
			.label {
				font-size: 0.8em;
				white-space: nowrap;
				position: absolute;
				margin-top: -16px;
			}
			.progress {
				display: flex;
				position: relative;
				height: 4px;
				> div {
					position: absolute;
					top: 0;
					bottom: 0;
					left: 50%;
					right: 50%;
					&.current {
						background-color: $ui_colors_bright_button_info;
					}
					&.offset {
						&.positive {
							div {
								right: 0;
								left: auto;
							}
						}
						&.push {
							& > div {
								right: auto;
								left: 0;
							}
						}
						div {
							font-family: "SkyOS-SemiBold";
							font-size: 0.8em;
							position: absolute;
							left: 0;
							margin-top: 5px;
							transition: left 0.8s cubic-bezier(.08,0,.1,1), width 0.8s cubic-bezier(.08,0,.1,1);
						}
					}
				}
			}
		}
	}
}
</style>