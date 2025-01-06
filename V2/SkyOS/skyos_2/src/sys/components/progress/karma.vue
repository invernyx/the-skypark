<template>
	<div class="progress" v-if="progress" :data-thook="'progress_section_karma'">
		<div class="progress-box">
			<div class="progress-box--header">
				<div>
					<div class="progress-box--title">Karma</div>
					<div class="progress-box--balance">{{ progress.Balance != null ? (progress.Balance > 0 ? '+' : '') + (Math.round(progress.Balance * 100) / 100).toLocaleString('en-gb') : '?' }}</div>
				</div>
			</div>
			<div class="progress-box--icons">
				<div></div>
				<div></div>
			</div>
			<div class="progress-box--mask">
			</div>
			<div class="progress-box--trend">
				<div :class="{ 'p': progress.Balance > 0 }" :style="'left:' + progressLeft + '%; right:' + progressRight + '%;'"></div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "p42_progress_comp_Karma",
	props: ['progress'],
	data() {
		return {
			progressLeft: 0,
			progressRight: 0
		}
	},
	methods: {
		makeTrend() {
			const range = 42;
			if(this.progress.Balance >= 0) {
				this.progressLeft = 50;
				this.progressRight = 50 - (50 / range * this.progress.Balance);
			} else {
				this.progressRight = 50;
				this.progressLeft = 50 + (50 / range * this.progress.Balance);
			}
		},
	},
	mounted() {
		this.makeTrend();
	},
	watch: {
		progress() {
			this.makeTrend();
		}
	}
});
</script>

<style lang="scss" scoped>
	@import '../../../sys/scss/colors.scss';
	@import '../../../sys/scss/mixins.scss';

	.progress {

		.theme--bright &,
		&.theme--bright {
			&-box {
				background: $ui_colors_bright_shade_0;
				@include shadowed_shallow($ui_colors_bright_shade_4);
				&--icons {
					& > div {
						&:first-child {
							background-image: url(../../../sys/assets/icons/dark/bad.svg);
						}
						&:last-child {
							background-image: url(../../../sys/assets/icons/dark/good.svg);
						}
					}
				}
				&--mask {
					background: $ui_colors_bright_shade_0;
					box-shadow: 0 0 20px rgba($ui_colors_bright_shade_5, 0.2);
				}
				&--trend {
					& > div {
						background: linear-gradient(to left, rgba($ui_colors_bright_shade_5, 0.0), rgba($ui_colors_bright_shade_4, 0.8));
						border-left: 2px solid $ui_colors_bright_shade_4;
						&.p {
							background: linear-gradient(to right, rgba($ui_colors_bright_shade_5, 0.0), rgba($ui_colors_bright_button_gold, 0.8));
							border: none;
							border-right: 2px solid $ui_colors_bright_button_gold;
						}
					}
				}
			}
		}
		.theme--dark &,
		&.theme--dark {
			&-box {
				background: $ui_colors_dark_shade_0;
				@include shadowed_shallow($ui_colors_dark_shade_0);
				&--icons {
					& > div {
						&:first-child {
							background-image: url(../../../sys/assets/icons/bright/bad.svg);
						}
						&:last-child {
							background-image: url(../../../sys/assets/icons/bright/good.svg);
						}
					}
				}
				&--mask {
					background: $ui_colors_dark_shade_0;
					box-shadow: 0 0 20px rgba($ui_colors_bright_shade_0, 0.2);
				}
				&--trend {
					& > div {
						background: linear-gradient(to left, rgba($ui_colors_dark_shade_5, 0.0), rgba($ui_colors_dark_shade_2, 0.8));
						border-left: 2px solid $ui_colors_dark_shade_2;
						&.p {
							background: linear-gradient(to right, rgba($ui_colors_dark_shade_5, 0.0), rgba($ui_colors_dark_button_gold, 0.8));
							border: none;
							border-right: 2px solid $ui_colors_dark_button_gold;
						}
					}
				}
			}
		}

		&-box {
			position: relative;
			border-radius: 13px;
			overflow: hidden;
			margin-bottom: 8px;
			&--header {
				display: flex;
				position: relative;
				z-index: 3;
				& > div {
					position: absolute;
					left: 0;
					right: 0;
					text-align: center;
					margin: 12px;
				}
			}
			&--title {
				font-size: 1em;
			}
			&--balance {
				font-size: 2.4em;
				line-height: 1em;
				font-family: "SkyOS-SemiBold";
			}
			&--icons {
				& > div {
					position: absolute;
					top: 8px;
					width: 28px;
					height: 28px;
					z-index: 3;
					&:first-child {
						left: 8px;
					}
					&:last-child {
						right: 8px;
					}
				}
			}
			&--mask {
				position: absolute;
				left: 50%;
				width: 300%;
				height: 1200px;
				margin-top: 75px;
				border-radius: 50%;
				z-index: 2;
				transform: translate(-50%, -100%);
			}
			&--trend {
				height: 100px;
				position: relative;
				& > div {
					position: absolute;
					left: 0;
					top: 0;
					bottom: 0;
				}
			}
			&--footer {
				margin-bottom: 2em;
			}
		}
	}

</style>