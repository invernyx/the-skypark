<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props" @loaded="ready = true" v-if="load"/>
		<Splash :ready="ready" :root="root" :app="app" :appName="appName" @done="splash = false" v-if="splash"/>
		<app_header :app="app" />
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Splash from "@/sys/components/splash.vue"

export default Vue.extend({
	props: {
		root: Object,
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