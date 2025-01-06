<template>
	<div class="button_tile" :class="[theme, { 'arrow' : arrow}]" v-if="!simple">
		<span v-if="icon" class="icon" :style="'background-image: url(' + getIcon() + ')'"></span>
		<slot v-if="$slots.icon" class="icon" name="icon"></slot>
		<span>
			<strong v-if="$slots.default"><slot></slot></strong>
			<strong v-else><slot name="label"></slot></strong>
			<span v-if="$slots.badge" class="badge_label"><slot name="badge"></slot></span>
		</span>
	</div>
	<div class="button_tile" :class="[theme, { 'arrow' : arrow}]" v-else>
		<slot></slot>
		<span v-if="$slots.badge" class="badge_label"><slot name="badge"></slot></span>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "button_tile",
	props: ['theme', 'arrow', 'icon', 'badge_text', 'simple'],
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
.button_tile {
	display: flex;
	align-items: center;
	position: relative;
	padding: 8px 11px 8px 8px;
	height: 40px;
	width: 40px;
	cursor: pointer;
	margin-bottom: 1px;
	border-radius: 8px;
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
	&.sharp {
		border-radius: 0;
	}

	&.disabled {
		pointer-events: none !important;
		filter: saturate(0);
		opacity: 0.5;
		transform: scale(1);
		cursor: default;
	}

	& > span {
		display: flex;
		flex-grow: 1;
		justify-content: space-between;
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
		&.selected {
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
		&.selected {
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