<template>
	<div class="button_action" :class="[theme, { 'heldnotice': holdNotice, 'hold': hold, 'held': hold && heldTimeout, 'shadowed': shadowed, 'grow': grow }]" @mousedown="holdEvStart" @mouseup="holdEvEnd">
		<span><slot></slot></span>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "button_action",
	props: ['theme', 'shadowed', 'grow', 'hold'],
	data() {
		return {
			holdNoticeTimeout: null as any,
			holdNotice: false,
			heldTimeout: null as any,
			heldTime: null as Date
		}
	},
	methods: {
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
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.button_action {
	position: relative;
	overflow: hidden;
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 5px 8px;
	border-radius: 8px;
	text-align: center;
	font-family: "SkyOS-SemiBold";
	cursor: pointer;
	transition: transform 1s cubic-bezier(0, 1, 0.15, 1), opacity 0.5s cubic-bezier(0, 1, 0.15, 1);
	* {
		pointer-events: none;
	}
	&:before {
		content: "";
		position: absolute;
		top: 0;
		bottom: 0;
		left: 0;
		width: 0;
		transition: width 0.5s 0.1s ease-in;
	}

	.theme--bright &,
	&.theme--bright {
		background-color: $ui_colors_bright_shade_2;
		color: $ui_colors_bright_shade_5;
		&:hover {
			background-color: darken($ui_colors_bright_shade_2, 2%);
		}
		&:active {
			background-color: darken($ui_colors_bright_shade_2, 5%);
		}
		&:before {
			background: rgba($ui_colors_bright_shade_5, 0.2);
		}
		&:after {
			background-image: url(../../sys/assets/framework/dark/arrow_right.svg);
		}
		&.outlined {
			background-color: transparent;
			color: $ui_colors_bright_shade_5;
			border: 2px solid rgba($ui_colors_bright_shade_5, 0.1);
			padding: 3px 6px;
			&:hover {
				background-color: rgba($ui_colors_bright_shade_5, 0.1);
			}
			&:active {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
		}
		&.transparent {
			background-color: transparent;
			color: $ui_colors_bright_shade_5;
			&:hover {
				background-color: transparent;
			}
			&:active {
				background-color: transparent;
			}
		}
		&.translucent {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
			color: $ui_colors_dark_shade_0;
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
		&.bright {
			background-color: $ui_colors_bright_shade_0;
			color: $ui_colors_bright_shade_5;
			&:hover {
				background-color: darken($ui_colors_bright_shade_0, 3%);
			}
			&:active {
				background-color: darken($ui_colors_bright_shade_0, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.cancel {
			background-color: $ui_colors_bright_button_cancel;
			color: $ui_colors_bright_shade_0;
			&:hover {
				background-color: darken($ui_colors_bright_button_cancel, 3%);
			}
			&:active {
				background-color: darken($ui_colors_bright_button_cancel, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.go {
			background-color: $ui_colors_bright_button_go;
			color: $ui_colors_bright_shade_0;
			&:hover {
				background-color: darken($ui_colors_bright_button_go, 3%);
			}
			&:active {
				background-color: darken($ui_colors_bright_button_go, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_bright_shade_0,0.2);
				border-top-color: rgba($ui_colors_bright_shade_0,1);
			}
		}
		&.info {
			background-color: $ui_colors_bright_button_info;
			color: $ui_colors_bright_shade_0;
			&:hover {
				background-color: darken($ui_colors_bright_button_info, 3%);
			}
			&:active {
				background-color: darken($ui_colors_bright_button_info, 5%);
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
		&.selected,
		&.naved-in {
			background-color: rgba($ui_colors_bright_button_info, 1);
			color: $ui_colors_bright_shade_0;
			&:hover {
				background-color: darken($ui_colors_bright_button_info, 10%);
			}
		}
	}
	.theme--dark &,
	&.theme--dark {
		background-color: $ui_colors_dark_shade_2;
		color: $ui_colors_dark_shade_5;
		&:hover {
			background-color: lighten($ui_colors_dark_shade_2, 5%);
		}
		&:active {
			background-color: lighten($ui_colors_dark_shade_2, 8%);
		}
		&:before {
			background: rgba($ui_colors_dark_shade_5, 0.2);
		}
		&:after {
			background-image: url(../../sys/assets/framework/bright/arrow_right.svg);
		}
		&.outlined {
			background-color: transparent;
			color: $ui_colors_dark_shade_5;
			border: 2px solid rgba($ui_colors_dark_shade_5, 0.1);
			padding: 3px 6px;
			&:hover {
				background-color: rgba($ui_colors_dark_shade_5, 0.1);
			}
			&:active {
				background-color: rgba($ui_colors_dark_shade_5, 0.1);
			}
		}
		&.transparent {
			background-color: transparent;
			color: $ui_colors_dark_shade_5;
			&:hover {
				background-color: transparent;
			}
			&:active {
				background-color: transparent;
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
		&.bright {
			background-color: $ui_colors_dark_shade_2;
			color: $ui_colors_dark_shade_5;
			&:hover {
				background-color: lighten($ui_colors_dark_shade_2, 3%);
			}
			&:active {
				background-color: lighten($ui_colors_dark_shade_2, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_0,0.2);
				border-top-color: rgba($ui_colors_dark_shade_0,1);
			}
		}
		&.cancel {
			background-color: $ui_colors_dark_button_cancel;
			color: $ui_colors_dark_shade_5;
			&:hover {
				background-color: darken($ui_colors_dark_button_cancel, 3%);
			}
			&:active {
				background-color: darken($ui_colors_dark_button_cancel, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_0,0.2);
				border-top-color: rgba($ui_colors_dark_shade_0,1);
			}
		}
		&.go {
			background-color: $ui_colors_dark_button_go;
			color: $ui_colors_dark_shade_5;
			&:hover {
				background-color: darken($ui_colors_dark_button_go, 3%);
			}
			&:active {
				background-color: darken($ui_colors_dark_button_go, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_5,0.2);
				border-top-color: rgba($ui_colors_dark_shade_5,1);
			}
		}
		&.info {
			background-color: $ui_colors_dark_button_info;
			color: $ui_colors_dark_shade_5;
			&:hover {
				background-color: darken($ui_colors_dark_button_info, 3%);
			}
			&:active {
				background-color: darken($ui_colors_dark_button_info, 5%);
			}
			&.loading span:after {
				border-color: rgba($ui_colors_dark_shade_5,0.2);
				border-top-color: rgba($ui_colors_dark_shade_5,1);
			}
		}
		&.loading span:after {
			border-color: rgba($ui_colors_dark_shade_5,0.2);
			border-top-color: rgba($ui_colors_dark_shade_5,1);
		}
		&.selected,
		&.naved-in {
			background-color: rgba($ui_colors_dark_button_info, 1);
			color: $ui_colors_dark_shade_5;
			&:hover {
				background-color: darken($ui_colors_dark_button_info, 10%);
			}
		}
	}

	&.small {
		font-size: 11px;
	}
	&.inline {
		padding: 0 0.5em;
	}
	&.grow {
		flex-grow: 1;
		will-change: transform;
	}
	&.listed {
		margin-bottom: 1px;
		border-radius: 0;
		&:last-child {
			margin-bottom: 0;
		}
	}
	&.justify {
		& > span {
			flex-grow: 1;
			justify-content: space-between;
		}
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

	&.invisible {
		pointer-events: none !important;
		opacity: 0;
	}
	&.disabled {
		pointer-events: none !important;
		filter: saturate(0);
		opacity: 0.5;
		transform: scale(1);
		cursor: default;
	}
	&.loading {
		& > span {
			&:after {
				display: inline-block;
				margin-left: 0.3em;
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

	& > span {
		display: flex;
		white-space: nowrap;
		min-height: 1.4em;
		justify-content: center;
    	align-items: center;
	}

	&.arrow {
		padding-right: 2em;
		&::after {
			content: "";
			position: absolute;
			right: 0.5em;
			top: 50%;
			margin-top: -5px;
			width: 10px;
			height: 10px;
			transition: transform 0.8s cubic-bezier(.25,0,.14,1);
		}
	}
	&.icon {
		padding: 0.5em 0.8em;
		& > span {
			width: 1.3em;
			background-position: center;
			background-size: contain;
			background-repeat: no-repeat;
		}

		.theme--bright &,
		&.theme--bright {
			&-up {
				& > span {
					background-image: url(../../sys/assets/icons/dark/arrow_up.svg);
				}
			}
			&-down {
				& > span {
					background-image: url(../../sys/assets/icons/dark/arrow_down.svg);
				}
			}
			&-close {
				& > span {
					background-image: url(../../sys/assets/icons/dark/close.svg);
				}
			}
			&-collapse {
				& > span {
					background-image: url(../../sys/assets/icons/dark/arrow_down.svg);
				}
			}
			&-next {
				& > span {
					background-image: url(../../sys/assets/icons/dark/next.svg);
				}
			}
			&-path {
				& > span {
					background-image: url(../../sys/assets/icons/dark/path.svg);
				}
			}
			&-answer {
				& > span {
					background-image: url(../../sys/assets/icons/dark/answer.svg);
				}
			}
			&-play {
				& > span {
					background-image: url(../../sys/assets/icons/dark/play.svg);
				}
			}
			&-pause {
				& > span {
					background-image: url(../../sys/assets/icons/dark/pause.svg);
				}
			}
			&-hang {
				& > span {
					background-image: url(../../sys/assets/icons/dark/hang.svg);
				}
			}
			&-collapse {
				& > span {
					background-image: url(../../sys/assets/icons/dark/collapse.svg);
				}
			}
		}

		.theme--dark &,
		&.theme--dark {
			&-up {
				& > span {
					background-image: url(../../sys/assets/icons/bright/arrow_up.svg);
				}
			}
			&-down {
				& > span {
					background-image: url(../../sys/assets/icons/bright/arrow_down.svg);
				}
			}
			&-close {
				& > span {
					background-image: url(../../sys/assets/icons/bright/close.svg);
				}
			}
			&-collapse {
				& > span {
					background-image: url(../../sys/assets/icons/bright/arrow_down.svg);
				}
			}
			&-next {
				& > span {
					background-image: url(../../sys/assets/icons/bright/next.svg);
				}
			}
			&-path {
				& > span {
					background-image: url(../../sys/assets/icons/bright/path.svg);
				}
			}
			&-answer {
				& > span {
					background-image: url(../../sys/assets/icons/bright/answer.svg);
				}
			}
			&-play {
				& > span {
					background-image: url(../../sys/assets/icons/bright/play.svg);
				}
			}
			&-pause {
				& > span {
					background-image: url(../../sys/assets/icons/bright/pause.svg);
				}
			}
			&-hang {
				& > span {
					background-image: url(../../sys/assets/icons/bright/hang.svg);
				}
			}
			&-collapse {
				& > span {
					background-image: url(../../sys/assets/icons/bright/collapse.svg);
				}
			}
		}

		&.info,
		&.cancel {
			&.icon {
				&-up {
					& > span {
						background-image: url(../../sys/assets/icons/bright/arrow_up.svg);
					}
				}
				&-down {
					& > span {
						background-image: url(../../sys/assets/icons/bright/arrow_down.svg);
					}
				}
				&-close {
					& > span {
						background-image: url(../../sys/assets/icons/bright/close.svg);
					}
				}
				&-collapse {
					& > span {
						background-image: url(../../sys/assets/icons/bright/arrow_down.svg);
					}
				}
				&-next {
					& > span {
						background-image: url(../../sys/assets/icons/bright/next.svg);
					}
				}
				&-path {
					& > span {
						background-image: url(../../sys/assets/icons/bright/path.svg);
					}
				}
				&-answer {
					& > span {
						background-image: url(../../sys/assets/icons/bright/answer.svg);
					}
				}
				&-play {
					& > span {
						background-image: url(../../sys/assets/icons/bright/play.svg);
					}
				}
				&-pause {
					& > span {
						background-image: url(../../sys/assets/icons/bright/pause.svg);
					}
				}
				&-hang {
					& > span {
						background-image: url(../../sys/assets/icons/bright/hang.svg);
					}
				}
				&-collapse {
					& > span {
						background-image: url(../../sys/assets/icons/bright/collapse.svg);
					}
				}
			}
		}
	}
}
</style>