<template>
	<div class="app-panel-wrap">
		<div class="app-panel-content" ref="app_panel_content">
			<div class="app-spacer"></div>
			<div class="app-panel-hit" v-if="selected_plan">

				<NavBar :plan="selected_plan" :contract="selected_contract" />

			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import ContractPanel from "@/sys/components/contracts/contract_panel.vue"
import Flightplan from '@/sys/classes/flight_plans/plan';
import NavBar from './../components/navigation_bar.vue'
import Contract from '@/sys/classes/contracts/contract';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ContractPanel,
		NavBar
	},
	data() {
		return {
			open: false,
			loading_plan: true,
			loading_contract: true,
			selected_plan: null as Flightplan,
			selected_contract: null as Contract
		}
	},
	methods: {

		navigated() {
			switch(this.$route.name) {
				case 'p42_yoflight_plan': {
					const ids = this.$route.params.id.toString().split('_');
					const plan_id = parseInt(ids[0]);
					const contract_id = ids.length > 1 ? parseInt(ids[1]) : -1;

					this.$os.api.send_ws('flightplans:query-from-id',
						{
							id: plan_id,
							fields: null
						},
						(planData: any) => {
							this.selected_plan = new Flightplan(planData.payload);
							this.loading_plan = false;
							//this.$os.eventsBus.Bus.emit('map_select', { name: 'plans', payload: this.selected_plan } );
						}
					);

					this.$os.api.send_ws('adventures:query-from-id',
						{
							id: contract_id,
							fields: {
								contract: {
									id: true,
									situation_at: true,
								},
								template: {
									file_name: true,
								}
							}
						},
						(contractsData: any) => {
							this.selected_contract = this.$os.contract_service.ingest(contractsData.payload.contract, contractsData.payload.template);
							this.loading_contract = false;
						}
					);

					break;
				}
			}
		},

		listener_map(wsmsg: any) {
			switch(wsmsg.name){
				case 'interact': {
					this.$os.scrollView.scroll_to('p42_yoflight_plan', 0, 0, 300);
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					this.navigated();
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'uncover': {
					this.$os.scrollView.scroll_to('p42_yoflight_plan', 0, 0, 100);
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
		this.navigated();
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('map', this.listener_map);
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.app-panel-wrap {
	flex-grow: 1;
	align-self: stretch;
	justify-content: stretch;
	.app-panel-content {
		display: flex;
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: $nav-size;
		padding: 0;
		flex-direction: column;
		flex-grow: 1;
		margin-bottom: 0;
		.app-spacer {
			flex-grow: 1;
		}
		.app-panel-hit {
			flex-shrink: 1;
			.theme--bright & {
				filter:
					drop-shadow(0px 1px 2px rgba($ui_colors_bright_shade_5, 0.3))
					drop-shadow(0px 2px 4px rgba($ui_colors_bright_shade_5, 0.15))
					drop-shadow(0px 5px 10px rgba($ui_colors_bright_shade_5, 0.2));
			}
			.theme--dark & {
				filter:
					drop-shadow(0px 1px 2px rgba($ui_colors_dark_shade_2, 0.3))
					drop-shadow(0px 2px 4px rgba($ui_colors_dark_shade_2, 0.15))
					drop-shadow(0px 5px 10px rgba($ui_colors_dark_shade_2, 0.2));
			}
			.theme--sat & {
				filter:
					drop-shadow(0px 1px 2px rgba($ui_colors_bright_shade_5, 0.3))
					drop-shadow(0px 2px 4px rgba($ui_colors_bright_shade_5, 0.15))
					drop-shadow(0px 5px 10px rgba($ui_colors_bright_shade_5, 0.2));
			}
		}
	}
}
</style>