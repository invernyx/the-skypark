<template>
	<div>
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
	data() {
		return {
		}
	},
});
</script>

<style lang="scss" scoped>
</style>