<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props"/>
		<app_header :app="app" :isHidden="true" />
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ap: () => import('./app.vue'),
	},
});
</script>

<style lang="scss" scoped>

</style>