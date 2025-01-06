<template>
	<div class="progress-box">
		<div class="progress-box--steps">
			<div>
				<div class="progress-box--steps-progress" :class="type">
					<div :style="'right:' + (100-percent) + '%'"></div>
				</div>
				<div class="progress-box--steps-label-bottom">
					<div>{{ label }}</div>
					<div><span>{{ percent }}</span>%</div>
				</div>
			</div>
			<!--
			<div>
				<div class="progress-box--steps-level">1</div>
			</div>
			-->
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "aircraft",
	props: ['meta', 'label', 'percent','type'],
	components: {
	},
	beforeDestroy() {

	},
	mounted() {
	},
	methods: {

	},
	data() {
		return {

		}
	},
});
</script>


<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';

.progress-box {

	position: relative;
	margin-top: 4px;
	margin-bottom: 4px;

	.theme--bright &,
	&.theme--bright {
		&--steps {
			&-progress {
				background: $ui_colors_bright_shade_2;
				& > div {
					background: $ui_colors_bright_button_info;
					box-shadow: 0 0px 10px $ui_colors_bright_button_info;
				}
				&.cancel {
					& > div {
						background: $ui_colors_bright_button_cancel;
						box-shadow: 0 0px 10px $ui_colors_bright_button_cancel;
					}
				}
			}
			&-level {
				border-color: $ui_colors_bright_shade_2;
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		&--steps {
			&-progress {
				background: $ui_colors_dark_shade_1;
				& > div {
					background: $ui_colors_dark_button_info;
					box-shadow: 0 0px 10px $ui_colors_dark_button_info;
				}
				&.cancel {
					& > div {
						background: $ui_colors_dark_button_cancel;
						box-shadow: 0 0px 10px $ui_colors_dark_button_cancel;
					}
				}
			}
			&-level {
				border-color: $ui_colors_dark_shade_1;
			}
		}
	}

	&--steps {
		display: flex;
		justify-content: stretch;
		align-items: center;
		font-size: 12px;
		& > div {
			&:first-child {
				flex-grow: 1;
			}
		}
		&-progress {
			position: relative;
			height: 2px;
			margin: 0;
			border-radius: 1px;
			flex-grow: 1;
			& > div {
				position: absolute;
				left: 0;
				top: 0;
				bottom: 0;
				border-radius: 3px;
			}
		}
		&-level {
			display: flex;
			min-width: 1.75em;
			height: 1.75em;
			padding: 0 0.4em;
			margin-left: 4px;
			font-family: "SkyOS-SemiBold";
			font-size: 1.5em;
			border-radius: 1em;
			align-items: center;
			box-sizing: border-box;
			justify-content: center;
			border: 2px solid transparent;
		}
		&-label {
			&-top {
				margin-bottom: 2px;
			}
			&-bottom {
				display: flex;
				margin-top: 2px;
				justify-content: space-between;
				& > div {
				}
				& span {
					font-family: "SkyOS-SemiBold";
					//display: inline-block;
					//&:first-child {
					//	margin-right: 0.2em;
					//}
					//&:last-child {
					//	margin-left: 0.2em;
					//}
				}
			}
		}
	}
}
</style>