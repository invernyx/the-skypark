<template>
	<div class="button_action" :class="[theme, { 'held': hold && heldTimeout, 'shadowed': shadowed, 'grow': grow }]" @mousedown="holdEvStart" @mouseup="holdEvEnd">
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
			heldTimeout: null as any,
		}
	},
	methods: {
		holdEvStart() {
			if(this.hold) {
				this.heldTimeout = setTimeout(() => {
					this.$emit('hold');
				}, 2000);
			}
		},
		holdEvEnd() {
			if(this.heldTimeout){
				clearTimeout(this.heldTimeout);
				this.heldTimeout = null;
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
@import '../scss/mixins.scss';
.button_action {
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 0.5em 0.8em;
	border-radius: 8px;
	text-align: center;
	font-family: "SkyOS-SemiBold";
	cursor: pointer;
	transition: transform 1s cubic-bezier(0, 1, 0.15, 1), opacity 0.5s cubic-bezier(0, 1, 0.15, 1);
	* {
		pointer-events: none;
	}
	&:hover {
		&.grow {
			transform: scale(1.05);
			z-index: 2;
		}
	}

	.theme--bright &,
	&.theme--bright {
		background-color: $ui_colors_bright_shade_2;
		color: $ui_colors_bright_shade_5;
		&.shadowed {
			@include shadowed_shallow($ui_colors_bright_shade_2);
		}
		&:hover {
			background-color: darken($ui_colors_bright_shade_2, 2%);
		}
		&:active {
			background-color: darken($ui_colors_bright_shade_2, 5%);
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_shade_5);
			}
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_button_cancel);
			}
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_button_go);
			}
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_bright_button_info);
			}
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
		&.shadowed {
			@include shadowed_shallow($ui_colors_dark_shade_2);
		}
		&:hover {
			background-color: lighten($ui_colors_dark_shade_2, 5%);
		}
		&:active {
			background-color: lighten($ui_colors_dark_shade_2, 8%);
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_shade_0);
			}
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_button_cancel);
			}
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_button_go);
			}
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
			&.shadowed {
				@include shadowed_shallow($ui_colors_dark_button_info);
			}
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
		font-size: 0.8em;
	}
	&.inline {
		padding: 0 0.5em;
	}
	&.grow {
		will-change: transform;
	}
	&.listed {
		margin-bottom: 1px;
		border-radius: 0;
		&:last-child {
			margin-bottom: 0;
		}
	}

	&.invisible {
		pointer-events: none !important;
		opacity: 0;
	}
	&.disabled {
		pointer-events: none !important;
		filter: saturate(0);
		opacity: 0.8;
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
		white-space: nowrap;
	}
}
</style>