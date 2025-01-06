<template>
	<div class="splash" :class="{ 'loading': !ready }" :data-appname="this.appName" :style="'background-color:' + app.app_icon_color" v-if="app">
		<div class="icon" :style="'background-image: url(' + getIcon() + ');'"></div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"
import Splash from "@/sys/components/splash.vue"

export default Vue.extend({
	name: "p42_app_launch",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String,
		ready: Boolean,
	},
	components: {
		Splash,
	},
	methods: {
		getIcon() {
			try {
				return require('../../apps/' + this.app.app_vendor + '/' + this.app.app_ident + '/icon.svg');
			} catch (e) {
				return require('../../sys/assets/framework/icon_default.svg');
			}
		}
	},
	watch: {
		ready() {
			setTimeout(() => {
				this.$emit('done');
			}, 300);
		}
	}
});
</script>

<style lang="scss" scoped>
	.is-device .hw_frame .splash {
		clip-path: inset(0 0 0 0 round 25px);
	}
	.splash {
		display: flex;
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		opacity: 0;
		z-index: 100;
		align-items: center;
		justify-content: center;
		transition: opacity 0.2s ease-out;
		pointer-events: none;
		&.loading {
			opacity: 1;
			pointer-events: all;
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