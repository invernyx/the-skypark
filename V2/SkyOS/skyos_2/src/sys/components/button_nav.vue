<template>
	<div class="button_nav" :class="[theme, { 'heldnotice': holdNotice, 'hold': hold, 'held': hold && heldTimeout, 'grow': grow, 'button_nav-back': shape == 'back', 'button_nav-forward': shape == 'forward', 'is-icon': !Object.keys($slots).length }]" @mouseleave="holdEvEnd" @mousedown="holdEvStart" @mouseup="holdEvEnd">
		<span v-if="icon" class="icon" :style="'background-image: url(' + getIcon() + ')'"></span>
		<span v-if="Object.keys($slots).length"><slot></slot></span>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "button_nav",
	props: ['theme', 'shape', 'grow', 'hold', 'icon'],
	data() {
		return {
			holdNoticeTimeout: null as any,
			holdNotice: false,
			heldTimeout: null as any,
			heldTime: null as Date
		}
	},
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
					fn = (this.icon as string).replace('theme/', this.$os.getConfig(['ui','theme']) == 'theme--bright' ? 'dark/' : 'bright/');
				}
			}

			try {
				return require('@/sys/assets/icons/' + fn + '.svg');
			} catch (e) {
				return require('@/sys/assets/icons/state_mask_alert.svg');
			}
		},
		holdEvStart() {
			if(this.hold) {
				this.heldTime = new Date();
				this.heldTimeout = setTimeout(() => {
					this.$emit('hold');
					this.heldTimeout = null;
				}, 2000);
			}
		},
		holdEvEnd() {
			if(this.heldTimeout){
				if(new Date().getTime() - this.heldTime.getTime() < 300) {
					this.holdNotice = true;
					clearTimeout(this.holdNoticeTimeout);
					this.holdNoticeTimeout = setTimeout(() => {
						this.holdNotice = false;
						this.holdNoticeTimeout = null;
					}, 1500);
				}
				clearTimeout(this.heldTimeout);
				this.heldTime = null;
				this.heldTimeout = null;
			}
		}
	},
	beforeDestroy() {
		clearTimeout(this.heldTimeout);
		clearTimeout(this.holdNoticeTimeout);
	}
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
@import '../scss/mixins.scss';
.button_nav {
	display: flex;
	position: relative;
	align-items: center;
	padding: 4px 10px 5px 10px;
	border-radius: 8px;
	box-sizing: border-box;
	font-family: "SkyOS-SemiBold";
	overflow: hidden;
	cursor: pointer;
	transition: transform 1s cubic-bezier(0, 1, 0.15, 1);
	* {
		pointer-events: none;
	}

	.theme--bright &,
	&.theme--bright {
		background-color: darken($ui_colors_bright_shade_2, 5%);
		color: $ui_colors_bright_shade_4;
		&:hover {
			background-color: darken($ui_colors_bright_shade_2, 10%);
		}
		&:active {
			background-color: darken($ui_colors_bright_shade_2, 20%);
		}
		&:before {
			background-color: rgba($ui_colors_bright_shade_2, 0.9);
		}
		&.outlined {
			box-shadow: inset 0px 0px 0px 1px rgba($ui_colors_bright_shade_5, 0.2);
		}
		&.bright {
			background: $ui_colors_bright_shade_0;
			color: $ui_colors_bright_shade_5;
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_shade_5);
			}
			&:hover {
				background: darken($ui_colors_bright_shade_0, 3%);
			}
			&:active {
				background: darken($ui_colors_bright_shade_0, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.cancel {
			background: $ui_colors_bright_button_cancel;
			color: $ui_colors_bright_shade_0;
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_button_cancel);
			}
			&:hover {
				background: darken($ui_colors_bright_button_cancel, 3%);
			}
			&:active {
				background: darken($ui_colors_bright_button_cancel, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.go {
			background: $ui_colors_bright_button_go;
			color: $ui_colors_bright_shade_0;
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_button_go);
			}
			&:hover {
				background: darken($ui_colors_bright_button_go, 3%);
			}
			&:active {
				background: darken($ui_colors_bright_button_go, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.info {
			background: $ui_colors_bright_button_info;
			color: $ui_colors_bright_shade_0;
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_button_info);
			}
			&:hover {
				background: darken($ui_colors_bright_button_info, 3%);
			}
			&:active {
				background: darken($ui_colors_bright_button_info, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.transparent {
			background-color: transparent;
			&:hover {
				background-color: rgba($ui_colors_dark_shade_1, 0.1);
			}
			&:active {
				background-color: rgba($ui_colors_dark_shade_1, 0.2);
			}
		}
		&.translucent {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
			color: $ui_colors_bright_shade_5;
			&:hover {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
			&:active {
				background-color: rgba($ui_colors_bright_shade_5, 0.3);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.loading span:after {
			border-color: rgba($ui_colors_bright_shade_5,0.2);
			border-top-color: rgba($ui_colors_bright_shade_5,1);
		}
	}
	.theme--dark &,
	&.theme--dark {
		background-color: lighten($ui_colors_dark_shade_2, 5%);
		color: $ui_colors_dark_shade_5;
		&:hover {
			background-color: lighten($ui_colors_dark_shade_2, 10%);
		}
		&:active {
			background-color: lighten($ui_colors_dark_shade_2, 20%);
		}
		&:before {
			background-color: rgba($ui_colors_dark_shade_2, 0.9);
		}
		&.outlined {
			box-shadow: inset 0px 0px 0px 1px rgba($ui_colors_dark_shade_5, 0.2);
		}
		&.bright {
			background: $ui_colors_dark_shade_2;
			color: $ui_colors_dark_shade_5;
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_shade_0);
			}
			&:hover {
				background: lighten($ui_colors_dark_shade_2, 3%);
			}
			&:active {
				background: lighten($ui_colors_dark_shade_2, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_0,0.2);
				border-top-color: rgba($ui_colors_dark_shade_0,1);
			}
		}
		&.cancel {
			background: $ui_colors_dark_button_cancel;
			color: $ui_colors_dark_shade_5;
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_button_cancel);
			}
			&:hover {
				background: darken($ui_colors_dark_button_cancel, 3%);
			}
			&:active {
				background: darken($ui_colors_dark_button_cancel, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_0,0.2);
				border-top-color: rgba($ui_colors_dark_shade_0,1);
			}
		}
		&.go {
			background: $ui_colors_dark_button_go;
			color: $ui_colors_dark_shade_5;
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_button_go);
			}
			&:hover {
				background: darken($ui_colors_dark_button_go, 3%);
			}
			&:active {
				background: darken($ui_colors_dark_button_go, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_5,0.2);
				border-top-color: rgba($ui_colors_dark_shade_5,1);
			}
		}
		&.info {
			background: $ui_colors_dark_button_info;
			color: $ui_colors_dark_shade_5;
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_button_info);
			}
			&:hover {
				background: darken($ui_colors_dark_button_info, 3%);
			}
			&:active {
				background: darken($ui_colors_dark_button_info, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_5,0.2);
				border-top-color: rgba($ui_colors_dark_shade_5,1);
			}
		}
		&.transparent {
			background-color: transparent;
			&:hover {
				background-color: rgba($ui_colors_dark_shade_1, 0.1);
			}
			&:active {
				background-color: rgba($ui_colors_dark_shade_1, 0.2);
			}
		}
		&.translucent {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			color: $ui_colors_dark_shade_5;
			&:hover {
				background-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
			&:active {
				background-color: rgba($ui_colors_dark_shade_5, 0.3);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_0,0.2);
				border-top-color: rgba($ui_colors_dark_shade_0,1);
			}
		}
		&.loading span:after {
			border-color: rgba($ui_colors_dark_shade_5,0.2);
			border-top-color: rgba($ui_colors_dark_shade_5,1);
		}
	}

	&:hover {
		background-color: rgba($ui_colors_bright_shade_5, 0.2);
		&.grow {
			transform: scale(1.05);
			z-index: 2;
		}
	}
	&:active {
		background-color: rgba($ui_colors_bright_shade_5, 0.3);
	}
	&:before {
		content: "";
		position: absolute;
		top: 0;
		bottom: 0;
		left: 0;
		width: 0;
		background: rgba(#000000, 0.2);
		transition: width 0.5s 0.1s ease-in;
	}

	&.disabled {
		pointer-events: none;
		filter: saturate(0);
		opacity: 0.8;
		transform: scale(1);
	}
	&.hold {
		& > span {
			transition: opacity 0.5s 0.2s ease-out;
		}
		&::after {
			display: flex;
			position: absolute;
			align-items: center;
			top: 0;
			left: 0;
			bottom: 0;
			content: 'Hold...';
			padding: 0.3em 0.6em 0.4em 0.6em;
			transform: translateX(-20px);
			opacity: 0;
			transition: opacity 0.2s ease-out, transform 1s cubic-bezier(0,.5,.5,1);
		}
	}
	&.heldnotice {
		& > span {
			opacity: 0.1;
			transition: opacity 0.1s ease-out;
		}
		&::after {
			opacity: 1;
			transform: translateX(0px);
		}
	}
	&.held {
		&:before {
			width: 100%;
			transition: width 2s ease-out;
		}
	}
	&.compact {
		font-size: 12px;
		padding: 3px 8px 3px 8px;
		.icon {
			margin-left: -0.25em;
			margin-right: 0.25em;
		}
	}
	&.is-icon {
		.icon {
			min-height: 2em;
			margin-right: 0em;
		}
	}

	&-back {
		padding-left: 1.2em;
		mask-image: url(../assets/framework/button_back_mask.svg);
		mask-position: center left;
		mask-repeat: no-repeat;
		mask-size: cover;
		&.compact {
			padding-left: 0.8em;
		}
	}
	&-forward {
		padding-right: 1.2em;
		mask-image: url(../assets/framework/button_forward_mask.svg);
		mask-position: center right;
		mask-repeat: no-repeat;
		mask-size: cover;
		&.compact {
			padding-right: 1em;
		}
	}

	& > span {
		position: relative;
		z-index: 1;
	}

	.icon {
		width: 1.5em;
		height: 1.5em;
		margin-top: -0.5em;
		margin-bottom: -0.5em;
		margin-right: 0.5em;
		background-repeat: no-repeat;
		background-size: contain;
		background-position: center;
	}

	&.loading {
		& > span {
			&:after {
				display: inline-block;
				margin-top: -0.4em;
				margin-bottom: -0.4em;
				border: 0.2em solid;
				border-top: 0.2em solid;
				border-radius: 50%;
				width: 1em;
				height: 1em;
				animation: spin 2s linear infinite;
				content: "";
				@keyframes spin {
					0% { transform: rotate(0deg); }
					100% { transform: rotate(360deg); }
				}
			}
		}
	}
}
</style>