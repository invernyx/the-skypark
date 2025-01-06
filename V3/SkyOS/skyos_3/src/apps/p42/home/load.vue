<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props"/>
		<app_header :app="app" :isHidden="true" />
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import ap from './app.vue';

export default Vue.extend({
	name: "p42_home",
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ap,
	},
});
</script>

<style lang="scss" scoped>

</style>