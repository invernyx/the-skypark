<template>
  <div class="split_view">
	<slot></slot>
  </div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
.split_view {
	display: flex;
	flex-grow: 1;
	position: relative;
	& > div {
		display: flex;
		position: relative;
		flex-grow: 1;

		&.background {
			z-index: 0;
			position: absolute;
			left: 0;
			right: 0;
			top: 0;
			bottom: 0;
		}

		&.split_view_narrow {
			width: 192px;
			box-sizing: border-box;
			flex-grow: 0;
		}

		&.split_view_strip {
			position: absolute;
			top: 0;
			left: 0;
			bottom: 0;
			width: 56px;
			box-sizing: border-box;
			flex-grow: 0;
			transition: width 0.2s cubic-bezier(.1,0,0,1);
		}

		&.large {
			width: 350px;
			flex-grow: 0;
		}

		&.one-half {
			width: 50%;
			flex-grow: 0;
		}

		&.one-third {
			width: 33.33%;
			flex-grow: 0;
		}
		&.two-third {
			width: 66.66%;
			flex-grow: 0;
		}

		&.one-fourth {
			width: 25%;
			flex-grow: 0;
		}
		&.two-fourth {
			width: 50%;
			flex-grow: 0;
		}
		&.three-fourth {
			width: 75%;
			flex-grow: 0;
		}

		&:nth-child(1) {
			z-index: 10;
		}
		&:nth-child(2) {
			z-index: 9;
		}
		&:nth-child(3) {
			z-index: 8;
		}
		&:nth-child(4) {
			z-index: 7;
		}
		&:nth-child(5) {
			z-index: 6;
		}
		&:nth-child(6) {
			z-index: 5;
		}
		&:nth-child(7) {
			z-index: 4;
		}
		&:nth-child(8) {
			z-index: 3;
		}
		&:nth-child(9) {
			z-index: 2;
		}
		&:nth-child(10) {
			z-index: 1;
		}

		& .narrow-sidebar-margin {
			margin-left: 190px;
		}

		& .narrow-sidebar-padding {
			padding-left: 190px;
		}
	}

	.theme--bright &,
	&.theme--bright {
		& > div {
		}
	}

	.theme--dark &,
	&.theme--dark {
		& > div {
		}
	}
}
</style>