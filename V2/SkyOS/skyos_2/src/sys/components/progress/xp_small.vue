<template>
	<div class="progress_xp" :class="[type]">

		<div class="progress-bar">
			<div class="node level">{{ this.level }}</div>
			<div class="track">
				<div class="label">Current XP: <strong>{{ Math.round(progressed).toLocaleString('en-gb') }}</strong></div>
				<div class="progress">
					<div class="current" :style="'width:' + (progressInRank) + '%;'"></div>
					<div class="offset" :class="{ 'push': progressInRank < 15 }" :style="'max-width:' + (gainInRank) + '%;'">
						<div>{{ '+' + Math.round(gain).toLocaleString('en-gb') }}</div>
					</div>
				</div>
			</div>
			<div class="node next" :class="{ 'up': levelUp }">{{ this.level + 1 }}</div>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "progress_xp",
	props: ['type', 'gain', 'progressed'],
	data() {
		return {
			level: 1,
			progressInRank: 0,
			gainInRank: 0,
			levelUp: false,
		}
	},
	methods: {
		getRank() {
			const rank = Eljs.CalculateRank(this.progressed, this.gain);
			this.levelUp = rank.levelUp;
			this.level = rank.level;
			this.progressInRank = rank.progress * 100;
			this.gainInRank = rank.gainProgress * 100;
		},
		//getColor() {
		//	const newColor = Eljs.HSLToRGB(Eljs.normalizeAngle360(((this.level + (this.progressInRank / 100)) * 15)), 50, 50);
		//	this.color = 'rgba(' + newColor.r + ',' + newColor.g + ',' + newColor.b + ', 1)';
		//}
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

.progress_xp {
	width: 100%;

	.theme--bright &,
	&.theme--bright {
		.progress-bar {
			.node {
				background-color: $ui_colors_bright_button_info;
				&.up {
					color: $ui_colors_bright_shade_0;
					background-color: $ui_colors_bright_shade_5;
				}
			}
			.track {
				.progress {
					background-color: rgba($ui_colors_bright_shade_5, 0.1);
					div {
						color: $ui_colors_bright_shade_5;
						&.current {
							background-color: $ui_colors_bright_button_info;
						}
						&.offset {
							background-color: $ui_colors_bright_shade_5;
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
				background-color: $ui_colors_dark_button_info;
				&.up {
					color: $ui_colors_dark_shade_0;
					background-color: $ui_colors_dark_shade_5;
				}
			}
			.track {
				.progress {
					background-color: rgba($ui_colors_dark_shade_5, 0.1);
					div {
						color: $ui_colors_dark_shade_5;
						&.current {
							background-color: $ui_colors_dark_button_info;
						}
						&.offset {
							background-color: $ui_colors_dark_shade_5;
							box-shadow: 0 0 20px $ui_colors_dark_shade_5;
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
			width: 2em;
			height: 2em;
			font-family: "SkyOS-SemiBold";
			border-radius: 1em;
			align-items: center;
			justify-content: center;
			color: #FFF;
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
				height: 4px;
				& > div {
					&.current {
						position: relative;
					}
					&.offset {
						position: relative;
						flex-grow: 1;
						&.push {
							& > div {
								right: auto;
								left: 0;
							}
						}
						& > div {
							font-family: "SkyOS-SemiBold";
							font-size: 0.8em;
							position: absolute;
							right: 0;
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