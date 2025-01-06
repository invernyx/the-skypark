<template>
	<div class="splash loading app-frame">
		tgfughjkfgh
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_app_start",
	beforeCreate() {
		console.log("rtrdtgfh");
	},
	methods: {

	},
});
</script>

<style lang="scss" scoped>
	.splash {
		display: flex;
		opacity: 0;
		z-index: 100;
		align-items: stretch;
		justify-content: stretch;
		pointer-events: none;
		&.loading {
			opacity: 1;
			pointer-events: all;
			.app-box {
				opacity: 1;
			}
		}
		.app-box {
			display: flex;
			align-items: center;
			justify-content: center;
			opacity: 0;
			transition: opacity 0.2s ease-out;
		}
		.icon {
			width: 100px;
			height: 100px;
			mask-image: url(../../sys/assets/framework/icon_mask.svg);
			mask-repeat: no-repeat;
			mask-position: center;
			mask-composite: exclude;
			border-radius: 1vh;
		}
	}
</style>