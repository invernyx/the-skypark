<template>
	<div class="button_listed" :class="[theme, { 'arrow' : arrow}]" v-if="!simple">
		<span v-if="icon" class="icon" :style="'background-image: url(' + getIcon() + ')'"></span>
		<slot v-if="$slots.icon" class="icon" name="icon"></slot>
		<span>
			<strong v-if="$slots.default"><slot></slot></strong>
			<strong v-else><slot name="label"></slot></strong>
			<span v-if="$slots.right" class="right_label"><slot name="right"></slot></span>
		</span>
	</div>
	<div class="button_listed" :class="[theme, { 'arrow' : arrow}]" v-else>
		<slot></slot>
		<span v-if="$slots.right" class="right_label"><slot name="right"></slot></span>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "button_listed",
	props: ['theme', 'arrow', 'icon', 'right_text', 'simple'],
	methods: {
		getIcon() {
			let fn = this.icon;
			if((this.icon as string).startsWith('theme/')) {
				if(this.theme ? this.theme.includes('theme--') : false) {
					if(this.$el.classList.contains('theme--dark')) {
						fn = (this.icon as string).replace('theme/', 'dark/');
					} else {
						fn = (this.icon as string).replace('theme/', 'bright/');
					}
				} else {
					fn = (this.icon as string).replace('theme/', this.$os.userConfig.get(['ui','theme']) == 'theme--bright' ? 'dark/' : 'bright/');
				}
			}

			try {
				return require('@/sys/assets/icons/' + fn + '.svg');
			} catch (e) {
				return require('@/sys/assets/icons/state_mask_alert.svg');
			}
		}
	}
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.button_listed {
	display: flex;
	align-items: center;
	position: relative;
	padding: 8px 11px 8px 8px;
	cursor: pointer;
	margin-bottom: 1px;
	overflow: hidden;
	text-overflow: ellipsis;
	border-left: 3px solid transparent;
	transition: border-left 0.1s ease-out, background 0.1s ease-out;

	&.arrow {
		padding: 8px 36px 8px 11px;
		&::after {
			content: "";
			position: absolute;
			right: 14px;
			top: 50%;
			margin-top: -5px;
			width: 10px;
			height: 10px;
			transition: transform 0.8s $transition;
		}
	}

	&:hover {
		background-color: rgba($ui_colors_bright_shade_1, 1);
	}
	&:first-of-type {
		border-top-left-radius: 8px;
		border-top-right-radius: 8px;
	}
	&:last-of-type {
		border-bottom-left-radius: 8px;
		border-bottom-right-radius: 8px;
		margin-bottom: 0;
	}
	&.sharp {
		border-radius: 0;
	}

	&.naved {
		transition: border-left 0.1s ease-out, height 0.5s $transition, padding 0.5s $transition, border-radius 0.5s $transition, opacity 0.2s $transition;
		height: 1.3em;
		border-radius: 0;
		&::before {
			content: "";
			position: absolute;
			left: 14px;
			top: 50%;
			margin-top: -5px;
			width: 10px;
			height: 10px;
			transform: translateX(-40px);
			transition: transform 0.8s $transition;
		}
		&-out {
			height: 0;
			padding-top: 0;
			padding-bottom: 0;
			margin-bottom: 0;
			opacity: 0;
		}
		&-in {
			padding-left: 30px;
			&::after {
				transform: translateX(40px);
			}
			&::before {
				transform: translateX(0px);
			}
		}
	}

	&.disabled {
		pointer-events: none !important;
		filter: saturate(0);
		opacity: 0.5;
		transform: scale(1);
		cursor: default;
	}

	&.listed_h {
		border-radius: 0;
		margin-right: 1px;
	}

	& > span {
		display: flex;
		flex-grow: 1;
		justify-content: space-between;
	}

	.column:first-of-type > & {
		&.listed_h {
			border-top-left-radius: 8px;
			border-bottom-left-radius: 8px;
		}
	}
	.column:last-of-type > & {
		&.listed_h {
			border-top-right-radius: 8px;
			border-bottom-right-radius: 8px;
			margin-right: 0;
		}
	}

	.icon {
		width: 1.5em;
		max-width: 1.5em;
		height: 1.5em;
		margin-top: -0.5em;
		margin-bottom: -0.5em;
		margin-right: 0.5em;
		flex-shrink: 1;
		background-repeat: no-repeat;
		background-size: contain;
	}

	&.theme--bright,
	.theme--bright &,
	#app .theme--bright & {
		background-color: $ui_colors_bright_shade_0;
		color: $ui_colors_bright_shade_5;
		border-left-color: rgba($ui_colors_bright_shade_0, 0);
		&::after {
			background-image: url(../../sys/assets/framework/dark/arrow_right.svg);
		}
		&::before {
			background-image: url(../../sys/assets/framework/dark/arrow_left.svg);
		}
		&:hover {
			background-color: $ui_colors_bright_shade_0;
			border-left-color: $ui_colors_bright_button_info;
		}
		&.selected,
		&.naved-in {
			background-color: rgba($ui_colors_bright_button_info, 1);
			border-left-color: $ui_colors_bright_button_info;
			color: $ui_colors_bright_shade_0;
			&::after {
				background-image: url(../../sys/assets/framework/bright/arrow_right.svg);
			}
			&::before {
				background-image: url(../../sys/assets/framework/bright/arrow_left.svg);
			}
			&:hover {
				background-color: darken($ui_colors_bright_button_info, 10%);
			}
		}
		&.cancel {
			background-color: $ui_colors_bright_shade_0;
			&:hover {
				border-left-color: $ui_colors_bright_button_cancel;
			}
		}
	}

	&.theme--dark,
	.theme--dark &,
	#app .theme--dark & {
		background-color: $ui_colors_dark_shade_2;
		color: $ui_colors_dark_shade_5;
		border-left-color: rgba($ui_colors_dark_shade_2, 0);
		&::after {
			background-image: url(../../sys/assets/framework/bright/arrow_right.svg);
		}
		&::before {
			background-image: url(../../sys/assets/framework/bright/arrow_left.svg);
		}
		&:hover {
			background-color: $ui_colors_dark_shade_2;
			border-left-color: $ui_colors_dark_button_info;
		}
		&.selected,
		&.naved-in {
			background-color: rgba($ui_colors_dark_button_info, 1);
			border-left-color: $ui_colors_dark_button_info;
			color: $ui_colors_dark_shade_5;
			&::after {
				background-image: url(../../sys/assets/framework/bright/arrow_right.svg);
			}
			&::before {
				background-image: url(../../sys/assets/framework/bright/arrow_left.svg);
			}
			&:hover {
				background-color: darken($ui_colors_dark_button_info, 5%);
			}

		}
		&.cancel {
			background-color: $ui_colors_bright_button_cancel;
			color: $ui_colors_bright_shade_0;
			&:hover {
				border-left-color: $ui_colors_bright_shade_0;
			}
		}
	}
}
</style>