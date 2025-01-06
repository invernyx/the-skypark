<template>
  <div class="navigation_bar" :class="[theme, { 'navigation_bar-status_padding': status_padding, 'navigation_bar-translucent': translucent, 'navigation_bar-transparent': transparent, 'navigation_bar-shadowed': shadowed }]">
	<slot></slot>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "navigation_bar",
	props: ['shadowed','translucent','transparent','theme','status_padding'],
});
</script>

<style lang="scss">
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
.navigation_bar {
	position: absolute;
	top: 0;
	left: 0;
	right: 0;
	display: flex;
	align-items: center;
	min-height: 34px;
	flex-wrap: wrap;
	justify-content: space-between;
	padding-top: ($edge-margin / 2);
	padding-left: $edge-margin / 2;
	padding-right: $edge-margin / 2;
	padding-bottom: $edge-margin / 2;
	z-index: 20;

	.theme--bright & {
		background: $ui_colors_bright_shade_1;
		color: $ui_colors_bright_shade_5;
		&.is-sidebar {
			background: $ui_colors_bright_shade_0;
		}
	}
	.theme--dark & {
		background: $ui_colors_dark_shade_1;
		color: $ui_colors_dark_shade_5;
		&.is-sidebar {
			background: $ui_colors_dark_shade_0;
		}
	}

	&-status_padding {
		padding-top: $status-size + ($edge-margin / 2);
	}
	&-transparent {
		background: transparent !important;
	}
	&-translucent {
		backdrop-filter: blur(5px);
		.theme--bright & {
			background: rgba($ui_colors_bright_shade_1, 0.8);
			&.is-sidebar {
				background: rgba($ui_colors_bright_shade_0, 0.8);
			}
		}
		.theme--dark & {
			background: rgba($ui_colors_dark_shade_1, 0.8);
			&.is-sidebar {
				background: rgba($ui_colors_dark_shade_0, 0.8);
			}
		}
	}
	&-shadowed {
		.theme--bright & {
			border-bottom: 1px solid rgba($ui_colors_bright_shade_2, var(--context-scroll-opacity-in));
		}
		.theme--dark & {
			border-bottom: 1px solid rgba($ui_colors_dark_shade_2, var(--context-scroll-opacity-in));
		}
	}

	& > .rows {
		display: flex;
		flex-direction: column;
		flex-grow: 1;
		& .row {
			display: flex;
			justify-self: stretch;
			justify-content: space-between;
		}
	}

	& .h-stack {
		display: flex;
		& > .button_nav {
			margin-right: 4px;
			&:last-of-type {
				margin-right: 0;
			}
		}
	}

	& > div {
		z-index: 2;
	}

	.abs-center {
		display: flex;
		align-items: center;
		justify-content: center;
		position: absolute;
		left: 0;
		right: 0;
		top: 0;
		bottom: 0;
		z-index: 0;
	}

	h1 {
		display: block;
		margin-top: 0.4em;
		margin-bottom: 0.4em;
		font-size: 1.6em;
		flex-grow: 1;
		flex-basis: 100%;
		&:first-child {
			margin-top: 0;
		}
		&:last-child {
			margin-bottom: 0;
		}
	}

	h2 {
		font-size: 1em;
		flex-grow: 1;
		text-align: center;
		margin: 0;
	}

	.scroll-fade-out {
		transform: translateY(calc((var(--context-scroll-opacity-out) - 1) * 20px));
		//filter: blur(calc((1 - var(--context-scroll-opacity-out)) * 3px));
	}
	.scroll-fade-in {
		transform: translateY(calc(var(--context-scroll-opacity-in) * -10px + 10px));
		//filter: blur(calc((1 - var(--context-scroll-opacity-in)) * 3px));
	}
}
</style>