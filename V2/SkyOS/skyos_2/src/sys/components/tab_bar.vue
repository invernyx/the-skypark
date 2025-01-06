<template>
	<div class="tab_bar" :class="[theme, { 'tab_bar-nav_padding': nav_padding, 'tab_bar-translucent': translucent, 'tab_bar-shadowed': shadowed }]">
		<slot></slot>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "tab_bar",
	props: ['shadowed','translucent','theme','nav_padding']
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
.tab_bar {
	position: absolute;
	bottom: 0;
	left: 0;
	right: 0;
	display: flex;
	align-items: center;
	min-height: 34px;
	justify-content: space-between;
	z-index: 2;

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

	& > div {
		flex-grow: 1;
	}

	&-nav_padding {
		padding-bottom: $nav-size + $edge-margin;
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
			border-top: 1px solid $ui_colors_bright_shade_2;
		}
		.theme--dark & {
			border-top: 1px solid $ui_colors_dark_shade_2;
		}
	}

	& > .h-stack {
		display: flex;
		& > .button_nav {
			margin-right: 4px;
			&:last-of-type {
				margin-right: 0;
			}
		}
	}
}
</style>