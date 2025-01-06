<template>
	<div :class="[appName, app.nav_class]">

		<div class="app-frame" :class="{ 'has-subcontent': ui.subframe !== null }">

			<scroll_stack :sid="'p42_yoflight_contracts_list'" class="app-box shadowed-deep" v-if="!ui.navigating" :key="'p42_yoflight_contracts_list'">
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

						<collapser :withArrow="true" :default="true"  v-for="(section, index) in state_sections.filter(x => x.contracts.length)" v-bind:key="index">
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

			<NavigationBox :humans="humans" :contract="state.selected.contract" :plan="state.selected.plan" v-else @close="navigate_stop()"/>

		</div>

		<div class="app-subframes">
			<div class="app-subframe" :class="{ 'has-content': ui.subframe !== null }">
				<transition :duration="1000">
					<Flight_plans
						v-if="ui.subframe == 'contract'"
						:contract="state.selected.contract"
						@details="plan_select($event.contract, $event.plan)"
						@close="contract_select(null)"/>
				</transition>
			</div>
		</div>
		<app_panel :has_content="ui.panel" :has_subcontent="ui.subframe !== null" :scroll_top="ui.panel_scroll_top">
			<transition :duration="800">
				<router-view :state="state" @scroll="ui.panel_scroll_top = $event.scrollTop"></router-view>
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
import Flight_plans from './subframes/flight_plans.vue';
import Flightplan from '@/sys/classes/flight_plans/plan';
import Aircraft from '@/sys/classes/aircraft';
import MapFeaturesConfig from '@/sys/classes/maps/map_features_config';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		NavigationBox: () => import("./components/navigation.vue"),
		ContractBox: () => import("./components/contract.vue"),
		Flight_plans
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			ui: {
				panel: false,
				subframe: null,
				navigating: false,
				panel_scroll_top: 0,
			},
			state: new MapFeaturesConfig({
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
			}),
			state_sections: [
				{
					code: "active",
					name: "Active",
					offset: 0,
					limit: 5,
					contracts: [] as Contract[],
				},
				{
					code: "paused",
					name: "Paused",
					offset: 0,
					limit: 5,
					contracts: [] as Contract[],
				}
			],
            humans: [],
            selected_aircraft: null as Aircraft,
		}
	},
	methods: {

        get_cabin() {

			if (this.selected_aircraft) {
				this.state.status = SearchStates.Searching;
				this.$os.api.send_ws("cabin:get-all", {
					fields: null
				}, (wsmsg) => {
					this.state.status = SearchStates.Idle;
					if (wsmsg.payload.humans) {
						this.humans = wsmsg.payload.humans;
					}
					if (wsmsg.payload.humans_state) {
						wsmsg.payload.humans_state.forEach(human_state => {
							const host = this.humans.find(x => x.guid == human_state.guid);
							if (host) {
								this.$set(host, "state", human_state.state);
							}
						});
					}
				});
			}
			else {
				this.state.status = SearchStates.NoResults;
			}
		},

		contracts_reset() {
			this.state.contracts = this.$os.contract_service.dispose_list(this.state.contracts);
			this.state.status = SearchStates.Idle;
			this.state.selected.contract = null;
			this.state.selected.plan = null;
			this.state.contracts = [];
			this.state.plans = [];
			this.$os.eventsBus.Bus.emit('map_state', { name: 'plans', payload: new MapFeaturesConfig() });
		},

		contracts_update() {

			this.state_sections[0].offset = 0;
			this.state_sections[1].offset = 0;

			this.state_sections[0].contracts = this.state.contracts.filter(x => x.is_monitored);
			this.state_sections[1].contracts = this.state.contracts.filter(x => !x.is_monitored);

			if(!this.state.contracts.length) {
				this.state.status = SearchStates.NoResults;
			}

			// Select the right contract based on the URL
			if(this.$route.params.id) {
				const ids = this.$route.params.id.toString().split('_');
				if(ids.length > 1) {
					const plan_id = parseInt(ids[0]);
					const contract_id = parseInt(ids[1]);
					this.ui.navigating = false;

					if(plan_id) {
						if(contract_id > -1) {
							this.state.selected.contract = this.state.contracts.find(x => x.id == contract_id);
							this.state.plans = this.state.selected.contract.flight_plans;
						} else{
							this.state.selected.contract = null;
							this.state.plans = [];
						}
						this.state.selected.plan = this.state.plans.find(x => x.id == plan_id);
						this.ui.panel = true;
						this.ui.navigating = true;
						this.ui.subframe = null;
					}

					const found = this.state.contracts.find(x => x.id == this.$route.params.id);
					if(found) {
						this.state.selected.contract = found;
						this.ui.panel = true;
						this.ui.navigating = true;
						this.ui.subframe = 'contract';
					}
				} else {
					const contract_id = parseInt(this.$route.params.id);
					const found = this.state.contracts.find(x => x.id == contract_id);
					if(found) {
						this.state.selected.contract = found;
						this.ui.panel = true;
						this.ui.navigating = false;
						this.ui.subframe = 'contract';
					}
				}

				this.$os.eventsBus.Bus.emit('nav_set', { name: 'plan_contract', payload: this.state.selected } );
			}

		},

		contracts_search() {
			this.contracts_reset();
			if(this.has_transponder) {
				this.state.status = SearchStates.Searching;
				this.state.contracts = this.$os.contract_service.dispose_list(this.state.contracts);
				this.state.plans = this.state.selected.contract ? this.state.selected.contract.flight_plans : [];
				this.emit_state();

				const queryoptions = {
					state: 'Active,Saved',
					priorityState: 'Active,Saved',
					sort: 'requested',
					sortAsc: false,
					detailed: false,
					limit: 2000,
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
							flight_plans: true,
						},
						template: {
							time_to_complete: true,
							running_clock: true,
							aircraft_restriction_label: true,
							file_name: true,
							company: true,
							type_label: true,
							modified_on: true,
						}
					}
				}

				this.$os.api.send_ws(
					'adventures:query-from-filters',
					queryoptions,
					(contractsData: any) => {

						if(contractsData.payload.contracts.length) {
							this.state.status = SearchStates.Idle;

							this.state.contracts = this.$os.contract_service.ingest_list(contractsData.payload.contracts, contractsData.payload.templates);
							this.state.plans = this.state.selected.contract ? this.state.selected.contract.flight_plans : [];

							this.emit_state();

						} else {
							this.state.status = SearchStates.NoResults;
						}

						this.contracts_update();
					}
				);

			}
		},

		plan_select(contract :Contract, plan :Flightplan){
			if(contract && plan) {
				this.$os.eventsBus.Bus.emit('map_select', { name: 'plans_contracts', payload: contract } );
				this.$os.eventsBus.Bus.emit('map_select', { name: 'plans', payload: plan } );
				this.$os.routing.goTo({ name: 'p42_yoflight_plan', params: { id: plan.id + (contract ? ('_' + contract.id) : ''), plan: plan }});
			} else {
				this.contract_select(contract);
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
				this.ui.subframe = 'contract';
				this.$os.eventsBus.Bus.emit('map_select', { name: 'plans_contracts', payload: contract } );
				this.$os.routing.goTo({ name: 'p42_yoflight_contract', params: { id: contract.id, contract: contract }});
			} else {
				this.ui.subframe = null;
				this.$os.eventsBus.Bus.emit('map_select', { name: 'plans_contracts', payload: null } );
				this.$os.routing.goTo({ name: 'p42_yoflight' });
			}
		},

		navigate_stop() {
			if(this.state.selected.contract) {
				this.$os.routing.goTo({ name: 'p42_yoflight_contract', params: { id: this.state.selected.contract.id, contract: this.state.selected.contract }})
			} else {
				this.$os.routing.goTo({ name: 'p42_yoflight' });
			}
		},

		emit_state() {
			this.$os.eventsBus.Bus.emit('map_state', { name: 'plans', payload:
				{
					status: this.state.status,
					selected: {
						contract: this.state.selected.contract,
						plan: this.state.selected.plan,
					},
					contracts: this.state.contracts,
					plans: this.state.plans
				}
			});
		},

		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.state.contracts);
					this.contracts_update();

                    if (wsmsg.payload.contract.state || wsmsg.payload.contract.is_monitored) {
                        this.get_cabin();
                    }
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
				case 'plans': {
					if(wsmsg.payload) {
						this.plan_select(this.state.selected.contract, this.state.plans.find(x => x.id == wsmsg.payload.id));
					} else {
						this.plan_select(this.state.selected.contract, null);
					}
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					this.ui.panel = wsmsg.route.matched.length > 1;
					switch(wsmsg.route.name) {
						case 'p42_yoflight': {
							this.state.selected.contract = null;
							this.state.selected.plan = null;
							this.ui.panel = true;
							this.ui.navigating = false;
							this.ui.subframe = null;
							break;
						}
						case 'p42_yoflight_contract': {
							this.state.selected.contract = this.state.contracts.find(x => x.id == wsmsg.route.params.id);
							this.ui.panel = true;
							this.ui.navigating = false;
							this.ui.subframe = 'contract';
							this.$os.eventsBus.Bus.emit('nav_set', { name: 'plan_contract', payload: this.state.selected } );
							break;
						}
						case 'p42_yoflight_plan': {
							const ids = wsmsg.route.params.id.split('_');
							const plan_id = parseInt(ids[0]);
							const contract_id = ids.length > 1 ? parseInt(ids[1]) : -1;
							this.ui.navigating = false;

							if(plan_id) {
								if(contract_id > -1) {
									this.state.selected.contract = this.state.contracts.find(x => x.id == contract_id);
									this.state.plans = this.state.selected.contract.flight_plans;
								} else{
									this.state.selected.contract = null;
									this.state.plans = [];
								}
								this.state.selected.plan = this.state.plans.find(x => x.id == plan_id);
								this.ui.panel = true;
								this.ui.navigating = true;
								this.ui.subframe = null;
							}

							this.$os.eventsBus.Bus.emit('nav_set', { name: 'plan_contract', payload: this.state.selected } );
							break;
						}
					}
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
					this.state.plans = [];
					this.emit_state();
					break;
				}
                case "cabin": {

					if(wsmsg.payload.humans) {
						this.humans = wsmsg.payload.humans;
					}

					wsmsg.payload.humans_removed.forEach(human => {
						const found = this.humans.find(h => h.guid == human);
						const found_index = this.humans.indexOf(found);
						if(found_index > -1) {
							this.humans.splice(found_index, 1);
						}
					});

					wsmsg.payload.humans_state.forEach(human_state => {
						const host = this.humans.find(x => x.guid == human_state.guid);
						if(host) {
							this.$set(host, 'state', human_state.state);
						}
					});
                    break;
                }
			}
		},
        listenerFleet(wsmsg: any) {
            switch (wsmsg.name) {
                case "current_aircraft": {
                    this.selected_aircraft = this.$os.fleetService.aircraft_current;
                    break;
                }
            }
        },
	},
	mounted() {
		this.$emit('loaded');

        this.selected_aircraft = this.$os.fleetService.aircraft_current;
		this.get_cabin();

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
        this.$os.eventsBus.Bus.on("fleet", this.listenerFleet);
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
		this.$os.eventsBus.Bus.on('map_selected', this.listener_map_selected);
	},
	beforeDestroy() {
        this.$os.eventsBus.Bus.off("fleet", this.listenerFleet);
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