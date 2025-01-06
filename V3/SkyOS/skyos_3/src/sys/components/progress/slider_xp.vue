<template>
	<div class="slider_xp">
		<div class="level level_current" :class="{ 'up': level_up }">
			<div>
				{{ level }}
			</div>
		</div>
		<div class="slider">
			<div class="marker" :style="{ 'margin-left': marker_location.left + '%', 'transform': 'translateX(' + marker_location.offset + '%)' }">
				<div class="label" :class="{ 'right': progress_location > 50 }">XP</div>
				<div class="balance">
					<number :amount="Math.round(new_gain + balance)" :decimals="0"/>
				</div>
				<div class="gain-label positive" v-if="new_gain >= 0">
					{{ '+' + Math.round(new_gain) }}
				</div>
			</div>
			<div class="track">
				<div class="range" :style="{ 'margin-right': range_location + '%' }"></div>
				<div class="margin">
					<div class="fill" :style="{ 'margin-right': progress_location + '%' }"></div>
					<div class="gain positive" :class="{ 'start': level_up }" :style="{ 'margin-left': gain_location.left + '%', 'margin-right': gain_location.right + '%' }"></div>
				</div>
			</div>
		</div>
		<div class="level level_up">
			<div>
				{{ level + 1 }}
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		gain :Number
	},
	components: {

	},
	data() {
		return {
			balance: this.$os.progress.xp.balance,
			new_balance: this.$os.progress.xp.balance,
			deflection_pct: 0,
			new_gain: 0,
			level: 1,
			level_up: false,
			marker_location: {
				left: 0,
				offset: 0
			},
			gain_location: {
				left: 0,
				right: 0,
			},
			progress_location: 0,
			range_location: 0
		}
	},
	methods: {
		init() {

			const deflection_factor = 1 / 100;
			this.deflection_pct = deflection_factor * 100;

			const rank = Eljs.CalculateRank(this.balance, this.gain);
			const rank_shown = rank.current.level_up ? rank.next : rank.current;
			this.level_up = rank.current.level_up;
			this.level = rank_shown.level;

			this.new_gain = this.gain;

			const progress_pct = !this.level_up ? (rank_shown.progress + rank_shown.gain_progress) * 100 : rank_shown.progress * 100;

			this.marker_location.left = progress_pct;
			this.marker_location.offset = -progress_pct;

			this.progress_location = 100 - progress_pct - this.deflection_pct;
			this.gain_location.left = this.level_up ? 0 :(rank_shown.progress * 100);
			this.gain_location.right = 100 - progress_pct;

		}
	},
	mounted() {
		this.init();
	},
	beforeDestroy() {
	},
	watch: {
		gain() {
			this.init();
		},
		range() {
			this.init();
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.slider_xp {
	position: relative;
	display: flex;

	.slider {
		position: relative;
		flex-grow: 1;
		z-index: 2;
	}

	.level {
		font-family: "SkyOS-Bold";
		position: relative;
		width: 40px;
		border-radius: 50%;
		& > div {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			margin: 2px;
			display: flex;
			border-radius: 50%;
			justify-content: center;
			align-items: center;
			transition: none;
		}
		&_up {
			border-bottom-left-radius: 0;
			margin-left: -2px;
			& > div {
				border-bottom-left-radius: 0;
			}
		}
		&_current {
			border-bottom-right-radius: 0;
			margin-right: -2px;
			& > div {
				border-bottom-right-radius: 0;
			}
		}
		&.up {
			& > div {
				transition: background 0.4s 0.3s ease-out, box-shadow 0.4s 0.3s ease-out;
			}
		}
	}

	.label {
		position: absolute;
		font-size: 14px;
		transform: translateX(-100%);
		margin-left: -10px;
		font-family: "SkyOS-Bold";
		&.right {
			right: 0;
			margin-left: 0px;
			margin-right: -10px;
			transform: translateX(100%);
		}
	}
	.marker {
		display: inline-flex;
		align-items: center;
		border-radius: 8px;
		font-size: 12px;
		margin-bottom: 4px;
		will-change: transform;
		font-family: "SkyOS-Bold";
		.balance {
			padding: 2px 6px;
		}
		.gain-label {
			padding: 0px 6px;
			border-radius: 6px;
			margin: 2px;
		}
	}
	.track {
		$size: 10px;
		//border: 1px solid transparent;
		position: relative;
		height: $size - 1;
		box-sizing: border-box;
		transition: margin 0.3s ease-out;
		.margin {
			position: absolute;
			right: 0;
			top: 0;
			bottom: 0;
			left: 0;
		}
		.range {
			position: absolute;
			top: 0;
			right: 2px;
			bottom: 0;
			left: 2px;
			transition: margin 0.3s ease-out;
		}
		.fill {
			position: absolute;
			top: 0;
			right: 2px;
			bottom: 0;
			left: 2px;
			border-radius: $size / 2;
			border-top-left-radius: 0;
			border-bottom-left-radius: 0;
			transition: margin 0.3s ease-out;
		}
		.gain {
			position: absolute;
			top: 2px;
			right: 2px;
			bottom: 2px;
			left: 2px;
			border-radius: $size / 2;
			background: transparent;
			transition: margin 0.3s ease-out, background 0.3s ease-out;
			&.start {
				left: 0;
				border-top-left-radius: 0;
				border-bottom-left-radius: 0;
			}
		}
	}

	.theme--bright & {
		.marker {
			background: rgba($ui_colors_bright_button_info, 0.5);
			.gain {
				&.positive {
					background: $ui_colors_bright_button_go;
				}
				&.negative {
					background: $ui_colors_bright_button_cancel;
				}
			}
		}
		.level {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
			&_current {
				background-color: $ui_colors_bright_button_info;
			}
			&.up {
				& > div {
					background: $ui_colors_bright_button_go;
					box-shadow: 0 0 10px $ui_colors_bright_button_go;
				}
			}
		}
		.range {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		.track {
			border-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		.fill {
			background-color: $ui_colors_bright_button_info;
		}
		.gain {
			&.positive {
				background: $ui_colors_bright_button_go;
				box-shadow: 0 0 10px $ui_colors_bright_button_go;
			}
		}
		.gain-label {
			&.positive {
				background: $ui_colors_bright_button_go;
			}
		}
	}
	.theme--dark & {
		.marker {
			background: rgba($ui_colors_dark_button_info, 0.5);
			.gain {
				&.positive {
					background: $ui_colors_dark_button_go;
				}
				&.negative {
					background: $ui_colors_dark_button_cancel;
				}
			}
		}
		.level {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			&_current {
				background-color: $ui_colors_dark_button_info;
			}
			&.up {
				& > div {
					background: $ui_colors_dark_button_go;
					box-shadow: 0 0 10px $ui_colors_dark_button_go;
				}
			}
		}
		.range {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		.track {
			border-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		.fill {
			background-color: $ui_colors_dark_button_info;
		}
		.gain {
			&.positive {
				background: $ui_colors_dark_button_go;
				box-shadow: 0 0 10px $ui_colors_dark_button_go;
			}
		}
		.gain-label {
			&.positive {
				background: $ui_colors_dark_button_go;
			}
		}
	}
}
</style>