<template>
	<div class="progress" v-if="progress" :data-thook="'progress_section_reliability'">
		<div class="progress-box">
			<div class="progress-box--header">
				<div>
					<div class="progress-box--title">Reliability</div>
					<div class="progress-box--balance">{{ Math.round(progress.balance).toLocaleString('en-gb') + '%' }}</div>
				</div>
			</div>
			<div class="progress-box--icons">
				<div></div>
				<div></div>
			</div>
			<div class="progress-box--mask">
			</div>
			<div class="progress-box--trend">
				<div :style="'width:' + w + '%;'"></div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		progress :Object
	},
	data() {
		return {
			w: 0,
		}
	},
	methods: {
		makeTrend() {
			this.w = this.progress.balance;
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
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

	.progress {

		.theme--bright &,
		&.theme--bright {
			&-box {
				background: $ui_colors_bright_shade_0;
				@include shadowed_shallow($ui_colors_bright_shade_4);
				&--icons {
					& > div {
						&:first-child {
							background-image: url(../../../sys/assets/icons/dark/no.svg);
						}
						&:last-child {
							background-image: url(../../../sys/assets/icons/dark/yes.svg);
						}
					}
				}
				&--mask {
					background: $ui_colors_bright_shade_0;
					box-shadow: 0 0 20px rgba($ui_colors_bright_shade_5, 0.2);
				}
				&--trend {
					& > div {
						background: linear-gradient(to right, rgba($ui_colors_bright_button_go, 0.5), rgba($ui_colors_bright_button_go, 0.8));
						border: none;
						border-right: 2px solid $ui_colors_bright_button_go;
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
							background-image: url(../../../sys/assets/icons/bright/no.svg);
						}
						&:last-child {
							background-image: url(../../../sys/assets/icons/bright/yes.svg);
						}
					}
				}
				&--mask {
					background: $ui_colors_dark_shade_0;
					box-shadow: 0 0 20px rgba($ui_colors_bright_shade_0, 0.2);
				}
				&--trend {
					& > div {
						background: linear-gradient(to right, rgba($ui_colors_dark_button_go, 0.5), rgba($ui_colors_dark_button_go, 0.8));
						border-right: 2px solid $ui_colors_dark_button_go;
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
					margin-top: 10px;
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
				left: 0%;
				width: 300%;
				height: 400px;
				margin-top: 95px;
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
			&--notice {
				height: 100px;
				position: relative;
			}
			&--footer {
				margin-bottom: 2em;
			}
		}
	}

</style>