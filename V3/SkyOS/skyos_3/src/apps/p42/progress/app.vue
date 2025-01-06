<template>
	<div :class="[appName, app.nav_class]">

		<div class="app-frame" :class="{ 'has-subcontent': ui.subframe !== null }">
			<scroll_stack :sid="'p42_progress_contracts_list'" class="app-box shadowed-deep">
				<!--
				<template v-slot:top>
					<div class="controls-top h_edge_padding">
					</div>
				</template>
				-->
				<template v-slot:content class="confine">
					<div class="searching" v-if="state.status == 1">
						<span class="title">Searching...</span>
						<p>This might take a few seconds.</p>
					</div>
					<div class="no-transponder" v-else-if="state.status == 2">
						<span class="title">No Transponder</span>
						<p>The Skypark Transponder is required to browse contracts.</p>
						<p>Please start your Transponder.</p>
					</div>
					<div class="no-results" v-else-if="state.status == 3">
						<span class="title">No results</span>
						<p>Try moving the map to another location or adjust your filters.</p>
					</div>
					<div v-else>

						<div class="h_edge_padding">
							<div class="columns columns_margined">
								<div class="column">
									<data_stack class="center" label="Contracts">
										<number :amount="state.contracts.length" :decimals="0"/>
									</data_stack>
								</div>
								<div class="column">
									<data_stack class="center" label="Completed">
										<distance :amount="state_sections[0].contracts.reduce((acc, c) => acc + c.distance, 0)" :decimals="0"/>
									</data_stack>
								</div>
							</div>
						</div>

						<collapser :withArrow="true" :default="section.code == 'succeeded'"  v-for="(section, index) in state_sections.filter(x => x.contracts.length)" v-bind:key="index">
							<template v-slot:title>
								<div class="section_header">
									<h2><span class="notice">{{section.contracts.length }}</span> {{ section.name }}</h2>
									<div class="collapser_arrow"></div>
								</div>
							</template>
							<template v-slot:content>
								<ContractBox
									v-for="(contract, index) in section.contracts.slice(section.offset, section.offset + section.limit)"
									v-bind:key="index"
									:index="index"
									:contract="contract"
									:selected="state.selected.contract"
									@details="contract_select(contract)"/>
								<pagination
									class="h_edge_padding_vertical_half"
									:qty="section.contracts.length"
									:limit="section.limit"
									:offset="section.offset"
									@set_offset="section.offset = $event;"/>
							</template>
						</collapser>

					</div>
				</template>
			</scroll_stack>
		</div>

		<app_panel :has_content="true" :has_subcontent="ui.subframe !== null" :scroll_top="ui.panel_scroll_top">
			<transition :duration="800">
				<router-view @scroll="ui.panel_scroll_top = $event.scrollTop" v-if="ui.panel"></router-view>
				<StatisticsView :root="root" :app="app" :appName="appName" @scroll="ui.panel_scroll_top = $event.scrollTop" v-else></StatisticsView>
			</transition>
		</app_panel>

  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import SearchStates from "@/sys/enums/search_states";
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import Airport from "@/sys/classes/airport";
import { AppInfo } from "@/sys/foundation/app_model"
import Flightplan from '@/sys/classes/flight_plans/plan';

import StatisticsView from './views/statistics.vue';
import MapFeaturesConfig from '@/sys/classes/maps/map_features_config';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ContractBox: () => import("./components/contract.vue"),
		StatisticsView
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			ui: {
				panel: false,
				subframe: null,
				panel_scroll_top: 0,
			},
			state: new MapFeaturesConfig({
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
				settings: {
					contract_show_badges: false,
				}
			}),
			state_sections: [
				{
					code: "succeeded",
					name: "Completed",
					offset: 0,
					limit: 5,
					contracts: [] as Contract[],
				},
				{
					code: "failed",
					name: "Failed",
					offset: 0,
					limit: 5,
					contracts: [] as Contract[],
				}
			]
		}
	},
	methods: {

		contracts_reset() {
			this.state.contracts = this.$os.contract_service.dispose_list(this.state.contracts);
			this.state.status = SearchStates.Idle;
			this.state.selected.contract = null;
			this.state.selected.plan = null;
			this.state.contracts = [];
			this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });
		},

		contracts_update() {

			this.state_sections[0].offset = 0;
			this.state_sections[1].offset = 0;

			this.state_sections[0].contracts = this.state.contracts.filter(x => x.state == 'Succeeded');
			this.state_sections[1].contracts = this.state.contracts.filter(x => x.state == 'Failed');

		},

		contracts_search() {
			this.contracts_reset();
			if(this.has_transponder) {
				this.state.status = SearchStates.Searching;
				this.state.contracts = this.$os.contract_service.dispose_list(this.state.contracts);
				this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });

				const queryoptions = {
					state: 'Succeeded,Failed',
					priorityState: 'Succeeded,Failed',
					sort: 'requested',
					sortAsc: false,
					detailed: false,
					limit: 2000,
					fields: {
						contract: {
							id: true,
							route: true,
							file_name: true,
							state: true,
							modified_on: true,
							requested_at: true,
							distance: true,
							expire_at: true,
							completed_at: true,
							started_at: true,
							pull_at: true,
							situations: {
								id: true,
								location: true,
								icao: true,
								trigger_range: true,
								airport: {
									icao: true,
									location: true,
									radius: true
								}
							},
							reward_bux: true,
						},
						template: {
							time_to_complete: true,
							running_clock: true,
							file_name: true,
							company: true,
							modified_on: true,
						}
					}
				}

				this.$os.api.send_ws(
					'adventures:query-from-filters',
					queryoptions,
					(contractsData: any) => {

						if(contractsData.payload.contracts.length) {
							this.state.contracts = this.$os.contract_service.ingest_list(contractsData.payload.contracts, contractsData.payload.templates);
							//this.state.search.count = contractsData.payload.count;
							//this.state.search.limit = contractsData.payload.limit;
							this.state.status = SearchStates.Idle;
							this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });

							if(this.state.contracts.length == 1) {
								this.contract_select(this.state.contracts[0]);
							}
						} else {
							this.state.status = SearchStates.NoResults;
						}

						this.contracts_update();
					}
				);

			}
		},

		contract_stops_next() {

			const zoom = this.$os.maps.main.map.getZoom();
			const contract = this.state.selected.contract;
			const center = this.$os.maps.main.map.getCenter();

			if(zoom > 6) {
				if(contract) {
					const closest = {
						distance: null,
						index: 0,
						airport: null,
					};

					contract.situations.forEach((situation :Situation, index :number) => {
						const distance = Eljs.GetDistance(center.lat, center.lng, situation.location[1], situation.location[0], 'm') / 1000;
						if(closest.distance > distance || !closest.distance ) {
							closest.distance = distance;
							closest.index = index;
							closest.airport = situation.airport
						}
					});

					let airport = null as Airport;
					let location = null as number[];
					let radius = null as number;
					if(closest.index < contract.situations.length - 1) {
						const next_sit = contract.situations[closest.index + 1];
						airport = next_sit.airport;
						location = next_sit.location;
						radius = next_sit.trigger_range;
					} else {
						airport = contract.situations[0].airport;
						location = contract.situations[0].location;
						radius = contract.situations[0].trigger_range
					}

					this.$os.eventsBus.Bus.emit('map', {
						name: 'goto',
						type: airport ? 'airport' : 'location',
						payload: {
							airport: airport,
							location: location,
							radius: radius,
						}
					});

				}
			} else {

				this.$os.eventsBus.Bus.emit('map', {
					name: 'goto',
					type: 'airport',
					payload: {
						airport: contract.situations[0].airport,
					}
				});
			}

		},

		contract_select(contract :Contract) {
			if(contract) {
				this.$os.eventsBus.Bus.emit('map_select', { name: 'contracts', payload: contract } );
				this.$os.routing.goTo({ name: 'p42_progress_contract', params: { id: contract.id, contract: contract }});
			} else {
				this.$os.eventsBus.Bus.emit('map_select', { name: 'contracts', payload: null } );
				this.$os.routing.goTo({ name: 'p42_progress' });
			}
		},

		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.state.contracts);
					this.contracts_update();
					break;
				}
			}
		},
		listener_map_selected(wsmsg :any) {
			switch(wsmsg.name) {
				case 'contracts': {
					if(wsmsg.payload) {
						this.contract_select(this.state.contracts.find(x => x.id == wsmsg.payload.id));
					} else {
						this.contract_select(null);
					}
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to_pre': {
					this.ui.panel = wsmsg.route.matched.length > 1;
					this.state.selected.contract = this.state.contracts.find(x => x.id == wsmsg.route.params.id);
					break;
				}
			}
		},
		listener_map(wsmsg: any) {
			switch(wsmsg.name){
				case 'loaded': {
					this.contracts_search();
					break;
				}
				case 'contract_stops_next': {
					this.contract_stops_next();
					break;
				}
			}
		},
		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					this.contracts_search();
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					this.state.status = SearchStates.NoTransponder;
					this.state.contracts = [];
					this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });
					break;
				}
			}
		},
	},
	mounted() {
		this.$emit('loaded');

		this.app.events.emitter.on('contracts_close', () => {
			this.contract_select(null);
		});

		this.app.events.emitter.on('contracts_collapse', () => {
			this.$os.eventsBus.Bus.emit('os', { name: 'uncover', payload: true });
		});

		if(this.$os.maps.main)
			this.contracts_search();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
		this.$os.eventsBus.Bus.on('map_selected', this.listener_map_selected);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('map', this.listener_map);
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.off('contracts', this.listener_os_contracts);
		this.$os.eventsBus.Bus.off('map_selected', this.listener_map_selected);
		this.contracts_reset();
	}
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
	@import '@/sys/scss/helpers.scss';

	.searching {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		padding: 30px;
		text-align: center;
		&::before {
			content: '';
			opacity: 0.3;
			width: 130px;
			height: 140px;
			background-size: 130px;
			background-position: center;
			background-repeat: no-repeat;
			background-position: center top;
			.theme--bright & {
				background-image: url(../../../sys/assets/icons/dark/tickets.svg);
			}
			.theme--dark & {
				background-image: url(../../../sys/assets/icons/bright/tickets.svg);
			}
		}
		.title {
			font-family: "SkyOS-SemiBold";
			font-size: 18px;
			display: block;
		}
		p {
			margin: 0;
			margin-top: 8px;
		}
	}

	.section_header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 8px 16px;
		padding-top: 16px;
		h2 {
			margin: 0;
			.notice {
				opacity: 0.5;
			}
		}
	}

	.map-controls {
		pointer-events: all;
		position: absolute;
		right: $edge-margin + 60px;
		top: $status-size;
		.map-covered & {
			display: none;
		}
	}

</style>