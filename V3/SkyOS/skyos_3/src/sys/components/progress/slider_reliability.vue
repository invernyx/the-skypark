<template>
	<div class="slider_reliability">
		<div class="marker" :style="{ 'margin-left': marker_location.left + '%', 'transform': 'translateX(' + marker_location.offset + '%)' }">
			<div class="label" :class="{ 'right': progress_location > 50 }">Reliability</div>
			<div class="balance"><number :amount="Math.round(new_gain + balance)" :decimals="0"/>%</div>
			<div class="gain-label" :class="{ 'positive': new_gain > 0 }" v-if="new_gain >= 0">{{ '+' + Math.round(new_gain) }}</div>
		</div>
		<div class="track">
			<div class="range" :style="{ 'margin-right': range_location + '%' }"></div>
			<div class="margin">
				<div class="fill" :style="{ 'margin-right': progress_location + '%' }"></div>
				<div class="gain positive" :style="{ 'margin-left': gain_location.left + '%', 'margin-right': gain_location.right + '%' }"></div>
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
			balance: this.$os.progress.reliability.balance,
			new_balance: this.$os.progress.reliability.balance,
			deflection_pct: 0,
			deflection_sum: 0,
			new_gain: 0,
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

			this.new_balance = this.balance + this.gain;
			this.new_gain = this.gain;

			if(this.new_balance > 100) {
				this.new_balance = 100;
				this.new_gain = this.new_balance - this.balance;
			} else if(this.new_balance < 0) {
				this.new_balance = 0;
				this.new_gain = this.new_balance - this.balance;
			}

			const progress_pct = this.new_balance;

			const deflection_factor = 1 / 100;
			this.deflection_pct = deflection_factor * 100;

			this.marker_location.left = progress_pct;
			this.marker_location.offset = -progress_pct;

			this.progress_location = 100 - progress_pct - this.deflection_pct;
			this.gain_location.left = this.balance + (this.deflection_pct / 2);
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

.slider_reliability {
	position: relative;

	.slider {
		position: relative;
		flex-grow: 1;
		z-index: 2;
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
			border-radius: $size / 2;
			transition: margin 0.3s ease-out;
		}
		.fill {
			position: absolute;
			top: 0;
			right: 2px;
			bottom: 0;
			left: 2px;
			border-radius: $size / 2;
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
			background-color: rgba($ui_colors_bright_shade_0, 0.3);
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
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			&.positive {
				background: $ui_colors_dark_button_go;
			}
		}
	}
}
</style>