<template>
	<div class="splash app-frame" :class="{ 'loading': !ready }" :data-appname="this.appName" v-if="app">
		<div class="app-box" :style="'background-color:' + app.icon_color">
			<div class="icon" :style="'background-image: url(' + getIcon() + ');'"></div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Splash from "@/sys/components/splash.vue"

export default Vue.extend({
	name: "p42_app_launch",
	props: {
		root: Object,
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
				return require('../../apps/' + this.app.vendor + '/' + this.app.ident + '/icon.svg');
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