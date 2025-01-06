<template>
	<div></div>
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
	},
	data() {
		return {
		}
	},
	methods: {
	},
	mounted() {
	},
	activated() {
	},
	deactivated() {
	},
	beforeDestroy() {
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

</style>