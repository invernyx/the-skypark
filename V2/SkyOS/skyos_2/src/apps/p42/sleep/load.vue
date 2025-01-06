<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props"/>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_sleep",
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
			/deep/.clock {
				opacity: 0;
				transition: opacity 0s;
			}
		}
		&-out-none {
			/deep/.clock {
				opacity: 0;
				transition: opacity 0.1s ease-out;
			}
		}
	}

</style>