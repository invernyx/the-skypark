<template>
	<div class="sidebar" :class="[theme, { 'spaced-nav': navspaced, 'spaced-status': statusspaced }]">
		<div>
			<slot></slot>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "sidebar",
	props: ['theme', 'navspaced', 'statusspaced'],
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.sidebar {
	margin-right: 0;
	overflow: hidden;

	.theme--bright & {
		background: $ui_colors_bright_shade_0;
		color: $ui_colors_bright_shade_5;
		.sidebar-caption {
			background: $ui_colors_bright_shade_2;
		}
	}
	.theme--dark & {
		background: $ui_colors_dark_shade_0;
		color: $ui_colors_dark_shade_5;
		.sidebar-caption {
			background: $ui_colors_dark_shade_2;
		}
	}

	&.spaced {
		&-nav {
			& > div {
				bottom: $nav-size;
			}
		}
		&-status {
			& > div {
				top: $status-size;
			}
		}
	}

	.sidebar-caption {
		padding: 10px 14px;
		border-radius: 14px;
		margin: 12px;
		white-space: pre-line;
		h2 {
			margin: 0;
			margin-bottom: 4px;
			font-size: 16px;
		}
		p {
			margin: 0;
			font-size: 13px;
		}
	}

	& > div {
		position: absolute;
		top: 0;
		right: 0;
		left: 0;
		bottom: 0;
	}

	h2 {
		margin-top: 0.4em;
		margin-bottom: 0.4em;
	}
}
</style>