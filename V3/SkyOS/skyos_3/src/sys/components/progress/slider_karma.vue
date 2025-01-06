<template>
	<div class="slider_karma">
		<div class="marker" :style="{ 'margin-left': marker_location.left + '%', 'transform': 'translateX(' + marker_location.offset + '%)' }">
			<div class="label" :class="{ 'right': new_balance < 0 }">Karma</div>
			<div class="gain-label negative" v-if="new_gain < 0">
				<number :amount="new_gain" :decimals="1"/>
			</div>
			<div class="balance">
				<number :amount="new_balance" :decimals="1"/>
			</div>
			<div class="gain-label" :class="{ 'positive': new_gain > 0 }" v-if="new_gain >= 0">
				+<number :amount="new_gain" :decimals="1"/>
			</div>
		</div>
		<div class="track">
			<div class="range" :style="{ 'margin-left': range_location.left + '%', 'margin-right': range_location.right + '%' }"></div>
			<div class="margin" :style="{ 'margin-left': (deflection_pct / 2) + '%', 'margin-right': (deflection_pct / 2) + '%' }">
				<div class="fill" :style="{ 'margin-left': progress_location.left + '%', 'margin-right': progress_location.right + '%' }"></div>
				<div class="gain" :class="{ 'positive': new_gain > 0, 'negative': new_gain < 0 }" :style="{ 'margin-left': gain_location.left + '%', 'margin-right': gain_location.right + '%' }"></div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		range :Array,
		gain :Number
	},
	components: {

	},
	data() {
		return {
			balance: this.$os.progress.karma.balance,
			new_balance: this.$os.progress.karma.balance,
			new_gain: 0,
			deflection_sum: 0,
			deflection_pct: 0,
			deflection: {
				min: -42,
				max: 42,
			},
			marker_location: {
				left: 0,
				offset: 0
			},
			gain_location: {
				left: 0,
				right: 0,
			},
			progress_location: {
				left: 20,
				right: 20,
			},
			range_location: {
				left: 0,
				right: 0,
			}
		}
	},
	methods: {
		init() {
			this.new_balance = this.balance + this.gain;
			this.new_gain = this.gain;

			if(this.new_balance > 42) {
				this.new_balance = 42;
				this.new_gain = this.new_balance - this.balance;
			} else if(this.new_balance < -42) {
				this.new_balance = -42;
				this.new_gain = this.new_balance - this.balance;
			}

			this.deflection_sum = this.deflection.max - this.deflection.min;
			const deflection_factor = 1 / this.deflection_sum;
			this.deflection_pct = deflection_factor * 100;

			const progress_abs = this.new_balance - this.deflection.min;
			const progress_pct = (deflection_factor * progress_abs) * 100;

			const balance_abs = this.balance - this.deflection.min;
			const balance_pct = (deflection_factor * balance_abs) * 100;

			this.marker_location.left = progress_pct;// - (deflection_pct / 2);
			this.marker_location.offset = -progress_pct;

			if(this.new_gain >= 0) {

				if(this.new_balance >= 0) {
					this.progress_location.left = 50 - (this.deflection_pct / 2);
					this.progress_location.right = 100 - progress_pct - (this.deflection_pct / 2);
				} else {
					this.progress_location.left = balance_pct - (this.deflection_pct / 2);
					this.progress_location.right = 50 - (this.deflection_pct / 2);
				}

				this.gain_location.left = balance_pct - (this.deflection_pct / 2);
				this.gain_location.right = 100 - progress_pct - (this.deflection_pct / 2);
			} else {

				if(this.new_balance >= 0) {
					this.progress_location.left = 50 - (this.deflection_pct / 2);
					this.progress_location.right = 100 - balance_pct - (this.deflection_pct / 2);
				} else {
					this.progress_location.left = progress_pct - (this.deflection_pct / 2);
					this.progress_location.right = 50 - (this.deflection_pct / 2);
				}

				this.gain_location.left = progress_pct - (this.deflection_pct / 2);
				this.gain_location.right = 100 - balance_pct - (this.deflection_pct / 2);
			}



			// Fill ranges
			const range_min_abs = (this.range[0] as number) - this.deflection.min;
			const range_min_pct = (deflection_factor * range_min_abs) * 100;
			this.range_location.left = range_min_pct;

			const range_max_abs = (this.range[1] as number) - this.deflection.min;
			const range_max_pct = (deflection_factor * range_max_abs) * 100;
			this.range_location.right = 100 - range_max_pct;

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

.slider_karma {
	position: relative;
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
		font-family: "SkyOS-Bold";
		will-change: transform;
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
		border-radius: $size / 2;
		position: relative;
		height: $size - 1;
		box-sizing: border-box;
		border: 1px solid transparent;
		transition: margin 0.3s ease-out;
		.margin {
			position: absolute;
			right: -1px;
			top: -1px;
			bottom: -1px;
			left: -1px;
		}
		.range {
			position: absolute;
			right: 0;
			top: 0;
			bottom: 0;
			left: 0;
			border-radius: $size / 2;
			transition: margin 0.3s ease-out;
		}
		.fill {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
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
			&.negative {
				background: $ui_colors_bright_button_cancel;
				box-shadow: 0 0 10px $ui_colors_bright_button_cancel;
			}
		}
		.gain-label {
			background-color: rgba($ui_colors_bright_shade_0, 0.3);
			&.positive {
				background: $ui_colors_bright_button_go;
			}
			&.negative {
				background: $ui_colors_bright_button_cancel;
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
			&.negative {
				background: $ui_colors_dark_button_cancel;
				box-shadow: 0 0 10px $ui_colors_dark_button_cancel;
			}
		}
		.gain-label {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			&.positive {
				background: $ui_colors_dark_button_go;
			}
			&.negative {
				background: $ui_colors_dark_button_cancel;
			}
		}
	}
}
</style>