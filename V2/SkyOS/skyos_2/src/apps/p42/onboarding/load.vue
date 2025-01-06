<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props"/>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_onboarding",
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