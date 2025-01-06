<template>
	<div :class="{ 'loading': loading }">

		<AircraftActions
			:app="app"
			:aircraft="aircraft" />

		<AircraftHeader
			:app="app"
			:aircraft="aircraft"
			:app_panel_content="app_panel_content"
			class="h_edge_margin_bottom" />

		<AircraftContent
			:app="app"
			:aircraft="aircraft" />

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import AircraftActions from "@/sys/components/aircraft/panel/actions.vue"
import AircraftHeader from "@/sys/components/aircraft/panel/header.vue"
import AircraftContent from "@/sys/components/aircraft/panel/content.vue"
import { AppInfo } from '@/sys/foundation/app_model';
import Aircraft from '@/sys/classes/aircraft';

export default Vue.extend({
	props:{
		app: AppInfo,
		aircraft :Aircraft,
		loading :Boolean,
		app_panel_content :HTMLDivElement
	},
	components: {
		AircraftActions,
		AircraftHeader,
		AircraftContent
	},
	data() {
		return {
		}
	},
	mounted() {
	},
	methods: {
		init() {

		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
</style>