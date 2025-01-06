<template>
	<scroll_view :sid="'p42_contrax_contract'" :scroller_offset="{ top: 36, bottom: 30 }" @scroll="$emit('scroll', $event)">
		<div class="app-panel-wrap">
			<div class="app-panel-content" ref="app_panel_content" :class="{ 'is-open': open }" :style="{ 'padding-top': 'var(--spacing-top)' }">
				<div class="app-spacer"></div>
				<div class="app-panel-hit" @mouseenter="$emit('can_scroll', true)" @mouseleave="$emit('can_scroll', false)">
					<ContractPanel
						v-if="selected_contract"
						:app="app"
						:loading="loading"
						:contract="selected_contract"
						:app_panel_content="$refs.app_panel_content"
						@route_code="$emit('route_code', $event)"
						@template_code="$emit('template_code', $event)"
						/>
				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import ContractPanel from "@/sys/components/contracts/contract_panel.vue"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ContractPanel
	},
	data() {
		return {
			open: false,
			loading: true,
			selected_contract: null as Contract,
		}
	},
	methods: {

		init() {
			this.loading = true;
			this.$os.contract_service.dispose_single(this.selected_contract);
			const adventure_id = parseInt(this.$route.params.id);
			this.$os.api.send_ws('adventures:query-from-id',
				{
					id: adventure_id,
					fields: {
						contract: {
							id: true,
							title: true,
							ready: true,
							is_monitored: true,
							aircraft_compatible: true,
							end_summary: true,
							description: true,
							description_long: true,
							recommended_aircraft: true,
							aircraft_used: true,
							operated_for: true,
							route: true,
							topo: true,
							file_name: true,
							state: true,
							request_status: true,
							last_location_geo: true,
							last_location_airport: {
								name: true,
								icao: true,
								city: true,
								state: true,
								country: true,
								country_name: true,
								location: true,
								elevation: true,
								runways: true,
							},
							modified_on: true,
							requested_at: true,
							distance: true,
							duration_range: true,
							route_code: true,
							media_link: true,
							expire_at: true,
							completed_at: true,
							started_at: true,
							pull_at: true,
							interactions: true,
							path: true,
							manifests: true,
							manifests_state: true,
							limits: true,
							invoices: true,
							memos: true,
							situations: {
								id: true,
								location: true,
								dist_to_next: true,
								trigger_range: true,
								height: true,
								icao: true,
								label: true,
								airport: {
									name: true,
									icao: true,
									city: true,
									state: true,
									country: true,
									country_name: true,
									location: true,
									elevation: true,
									runways: true,
								}
							},
							situation_at: true,
							image_url: true,
							reward_xp: true,
							reward_karma: true,
							reward_bux: true,
							reward_reliability: true,
						},
						template: null
					}
				},
				(contractsData: any) => {
					this.selected_contract = this.$os.contract_service.ingest(contractsData.payload.contract, contractsData.payload.template);
					this.loading = false;
				}
			);
		},

		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.contract_service.event([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.selected_contract)
					break;
				}
			}
		},
		listener_map(wsmsg: any) {
			switch(wsmsg.name){
				case 'interact': {
					this.$os.scrollView.scroll_to('p42_contrax_contract', 0, 0, 300);
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					if(this.$route.name == 'p42_contrax_contract') {
						this.init();
					} else {
						this.$os.contract_service.dispose_single(this.selected_contract);
					}
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'uncover': {
					this.$os.scrollView.scroll_to('p42_contrax_contract', 0, 0, 100);
					break;
				}
				case 'covered': {
					this.open = wsmsg.payload;
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
		this.init();
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('map', this.listener_map);
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.off('contracts', this.listener_os_contracts);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.app-panel-content {
	max-width: 700px;
	@media only screen and (max-width: $bp-1000) {
		max-width: 400px;
	}
}
</style>