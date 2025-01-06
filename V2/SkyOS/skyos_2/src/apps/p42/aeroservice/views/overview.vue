<template>
	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ top: 30, bottom: 10 }">
		<template v-slot:content>
			<div class="helper_edge_padding_lateral helper_status-margin">
				<h1>Fleet</h1>
				<div class="aircraft-frame" v-for="(aircraft, index) in fleet" v-bind:key="index">
					<Aircraft :meta="aircraft" />
				</div>
				<div class="acolumns acolumns-2 acolumns_margined">
				</div>
			</div>
		</template>
	</content_controls_stack>
</template>

<script lang="ts">
import { AppInfo } from '@/sys/foundation/app_bundle';
import Vue from 'vue';
import Aircraft from "./../components/aircraft.vue"

export default Vue.extend({
	name: "p42_aeroservice_overview",
	components: {
		Aircraft
	},
	props: {
		inst: Object,
		appvue: Vue,
		app: AppInfo,
		appName: String,
		fleet: Array
	},
	data() {
		return {
		}
	},
	methods: {
		listenerWs(wsmsg: any) {

		},
	},
	mounted() {

	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
});
</script>

<style lang="scss">
	@import '../../../../sys/scss/colors.scss';
	.p42_aeroservice {
		.aircraft-frame {
			display: block;
			position: relative;
			padding-bottom: 8px;
		}
	}
</style>