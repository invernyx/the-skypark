<template>
	<div>
		<h1>test</h1>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"

export default Vue.extend({
	name: "p42_home_content",
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
});
</script>

<style lang="scss" scoped>

</style>