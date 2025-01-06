<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props"/>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_locked",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ap: () => import('./app.vue'),
	},
});
</script>

<style lang="scss" scoped>

	.transition {

		&-in-none-pre {
			/deep/.backsplash-image {
				filter: contrast(200%);
				transform: scale(1);
				opacity: 0;
				transition: opacity 0.3s ease-out, filter 0s ease-out 1s, transform 0.4s cubic-bezier(0.5, 0, 0.75, 0.45);
			}
			/deep/.clock {
				opacity: 0;
				transition: opacity 0s;
			}
		}

		&-out-none {
			pointer-events: none;
			/deep/.backsplash-image {
				opacity: 0;
				transition: opacity 0.3s ease-out;
			}
			/deep/.clock {
				opacity: 0;
				transition: opacity 0.1s ease-out;
			}
		}

		&-out-fade {
			pointer-events: none;
			/deep/.backsplash-image {
				filter: contrast(200%);
				opacity: 0;
				transform: scale(1);
				transition: opacity 0.3s ease-out, filter 0s ease-out 1s, transform 0.4s cubic-bezier(0.5, 0, 0.75, 0.45);
			}
		}

	}
</style>