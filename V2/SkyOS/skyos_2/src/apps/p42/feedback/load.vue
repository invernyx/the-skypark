<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props" @loaded="ready = true" v-if="load"/>
		<Splash :ready="ready" :inst="inst" :app="app" :appName="appName" @done="splash = false" v-if="splash"/>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"
import Splash from "@/sys/components/splash.vue"

export default Vue.extend({
	name: "p42_feedback",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		Splash,
		ap: () => import('./app.vue'),
	},
	data() {
		return {
			load: false,
			ready: false,
			splash: true,
		}
	},
	mounted() {
		setTimeout(() => {
			this.load = true;
		}, 300);
	},
});
</script>