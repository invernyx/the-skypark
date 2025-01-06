<template>
	<div :class="[appName, app.nav_class]">

		<div class="app-frame" :class="{ 'has-subcontent': ui.subframe !== null }">
			<scroll_stack :sid="'p42_manifest_contracts_list'" class="app-box shadowed-deep">
				<template v-slot:top>
					<div class="controls-top h_edge_padding_half">
						<div class="buttons_list shadowed-shallow ">
							<div class="columns columns_3">
								<div class="column column_3 column_h-stretch">
									<button_listed class="listed_h" :class="[{ 'selected': ui.mode == 'contracts' }]" @click.native="set_list('contracts')">Contracts</button_listed>
								</div>
								<div class="column column_3 column_h-stretch">
									<button_listed class="listed_h" :class="[{ 'selected': ui.mode == 'manifests' }]" @click.native="set_list('manifests')">Manifests</button_listed>
								</div>
							</div>
						</div>
					</div>
				</template>
				<template v-slot:content class="confine">

					<div v-if="state.status != 0">
						<div v-if="ui.mode == 'contracts'">
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
								<span class="title">Nothing to see here!</span>
								<p>First, open the Contrax app to request a contract.</p>
								<div class="buttons_list shadowed-shallow h_edge_margin_top">
									<app_icon :app="$os.apps.find(x => x.vendor == 'p42' && x.ident == 'contrax')" :is_button="true"></app_icon>
								</div>
							</div>
						</div>
						<div v-else-if="ui.mode == 'manifests'">
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
								<span class="title">No Manifests</span>
								<p>First, open the Contrax app to request a contract and come back here to manage the payload.</p>
								<div class="buttons_list shadowed-shallow h_edge_margin_top">
									<app_icon :app="$os.apps.find(x => x.vendor == 'p42' && x.ident == 'contrax')" :is_button="true"></app_icon>
								</div>
							</div>
						</div>
					</div>
					<div v-else>
						<div v-if="ui.mode == 'contracts'">

							<collapser :withArrow="true" :default="true" v-for="(section, index) in state_sections.filter(x => x.contracts.length)" v-bind:key="index">
								<template v-slot:title>
									<div class="section_header">
										<h2><span class="notice">{{section.contracts.length }}</span> {{ section.name }}</h2>
										<div class="collapser_arrow"></div>
									</div>
								</template>
								<template v-slot:content>
									<ContractBox
										v-for="(contract, index) in section.contracts.slice(section.offset, section.offset + section.limit)"
										v-bind:key="contract.id"
										:data-anchor="contract.id.toString()"
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
						<div v-else-if="ui.mode == 'manifests'">


							<div class="h_edge_margin_lateral_half">
								<button_action class="arrow" v-if="$route.name != 'p42_conduit_manifest'" @click.native="$os.routing.goTo({ name: 'p42_conduit_manifest' })">See all</button_action>
								<button_action class="info arrow" v-else @click.native="$os.routing.goTo({ name: 'p42_conduit' })">Map</button_action>
							</div>


							<collapser :withArrow="true" :default="true" v-for="(section, index) in state_sections.filter(x => x.contracts.length)" v-bind:key="index">
								<template v-slot:title>
									<div class="section_header">
										<h2><span class="notice">{{section.contracts.length }}</span> {{ section.name }}</h2>
										<div class="collapser_arrow"></div>
									</div>
								</template>
								<template v-slot:content>
									<ManifestBox
										v-for="(contract, index) in section.contracts.slice(section.offset, section.offset + section.limit)"
										v-bind:key="contract.id"
										:data-anchor="contract.id.toString()"
										:index="index"
										:contract="contract"
										:selected="state.selected.contract"
										@details="manifest_select(contract)"/>
									<pagination
										class="h_edge_padding_vertical_half"
										:qty="section.contracts.length"
										:limit="section.limit"
										:offset="section.offset"
										@set_offset="section.offset = $event;"/>
								</template>
							</collapser>

						</div>
					</div>
				</template>
			</scroll_stack>
		</div>

		<app_panel :has_content="ui.panel" :has_subcontent="ui.subframe !== null" :scroll_top="ui.panel_scroll_top">
			<transition :duration="800">
				<router-view @scroll="ui.panel_scroll_top = $event.scrollTop"></router-view>
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
import MapFeaturesConfig from '@/sys/classes/maps/map_features_config';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ManifestBox: () => import("./components/manifest.vue"),
		ContractBox: () => import("./components/contract.vue"),
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			ui: {
				mode: 'contracts',
				panel: false,
				subframe: null,
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
					code: "saved",
					name: "Saved",
					offset: 0,
					limit: 5,
					contracts: [] as Contract[],
				}
			]
		}
	},
	methods: {

		set_list(type :string) {
			if(this.ui.mode != type) {
				this.ui.mode = type;
				if(type == 'manifests') {
					this.manifests_search();
				} else {
					this.contracts_search();
				}
			}
		},

		update_results() {

			this.state_sections[0].offset = 0;
			this.state_sections[1].offset = 0;

			this.state_sections[0].contracts = this.state.contracts.filter(x => x.state == 'Active');
			this.state_sections[1].contracts = this.state.contracts.filter(x => x.state == 'Saved');

			if(!this.state.contracts.length) {
				this.state.status = SearchStates.NoResults;
			}

		},

		contracts_reset() {
			this.state.contracts = this.$os.contract_service.dispose_list(this.state.contracts);
			this.state.selected.contract = null;
			this.state.selected.plan = null;
			this.state.contracts = [];

			this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });
			this.$os.eventsBus.Bus.emit('map_state', { name: 'manifests', payload: this.state });
		},

		manifests_search() {
			this.contracts_reset();
			if(this.has_transponder) {
				this.state.status = SearchStates.Searching;

				const queryoptions = {
					state: 'Saved,Active',
					priorityState: 'Saved,Active',
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
							manifests: true,
							manifests_state: true,
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
							//this.contracts.search.count = contractsData.payload.count;
							//this.contracts.search.limit = contractsData.payload.limit;
							this.state.status = SearchStates.Idle;
							this.$os.eventsBus.Bus.emit('map_state', { name: 'manifests', payload:
								{
									status: this.state.status,
									selected: {
										contract: this.state.selected.contract,
										//group: this.state.selected.group,
										plan: this.state.selected.plan,
									},
									contracts: this.state.contracts,
								}
							});

							if(this.$route.params.id) {
								const found = this.state.contracts.find(x => x.id == this.$route.params.id);
								this.manifest_select(found);
							} else if(this.state.contracts.length == 1) {
								this.manifest_select(this.state.contracts[0]);
							}
						} else {
							this.state.status = SearchStates.NoResults;
						}

						this.update_results();
					}
				);

			}
		},

		contracts_search() {
			this.contracts_reset();
			if(this.has_transponder) {
				this.state.status = SearchStates.Searching;

				const center = this.$os.maps.main.map.getCenter();
				const zoom = this.$os.maps.main.map.getZoom();

				const queryoptions = {
					state: 'Saved,Active',
					priorityState: 'Saved,Active',
					limit: 20,
					fields: {
						contract: {
							id: true,
							title: true,
							description: true,
							recommended_aircraft: true,
							aircraft_used: true,
							operated_for: true,
							route: true,
							topo: true,
							file_name: true,
							state: true,
							is_monitored: true,
							last_location_geo: true,
							request_status: true,
							modified_on: true,
							requested_at: true,
							distance: true,
							expire_at: true,
							started_at: true,
							pull_at: true,
							manifests: {
								total_weight: true
							},
							situations: {
								id: true,
								location: true,
								trigger_range: true,
								airport: {
									icao: true,
									location: true,
									radius: true
								}
							},
							image_url: true,
							reward_xp: true,
							reward_karma: true,
							reward_bux: true,
							reward_reliability: true
						},
						template: {
							time_to_complete: true,
							running_clock: true,
							file_name: true,
							aircraft_restriction_label: true,
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
							this.state.contracts = this.$os.contract_service.ingest_list(contractsData.payload.contracts, contractsData.payload.templates);
							this.state.status = SearchStates.Idle;
							this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });

							if(this.$route.params.id) {
								const found = this.state.contracts.find(x => x.id == this.$route.params.id);
								this.contract_select(found);
							} else if(this.state.contracts.length == 1) {
								this.contract_select(this.state.contracts[0]);
							}
						} else {
							this.state.status = SearchStates.NoResults;
						}

						this.update_results();
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
						radius = contract.situations[0].trigger_range;
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
			this.$os.eventsBus.Bus.emit('map_select', { name: 'contracts', payload: contract } );
		},
		contract_select_apply(contract :Contract) {
			if(contract) {
				if(this.state.selected.contract ? (this.state.selected.contract.id != contract.id) : true) {
					this.set_list('contracts');
					this.state.selected.contract = contract;
					this.$os.routing.goTo({ name: 'p42_conduit_contract', params: { id: contract.id, contract: contract }});

					this.$os.scrollView.scroll_to_el('p42_manifest_contracts_list', contract.id.toString(), [-80, 0], 1000);
				}
			} else {
				if(this.state.selected.contract) {
					this.state.selected.contract = null;
					this.$os.routing.goTo({ name: 'p42_conduit' });
				}
			}

			this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });
		},

		manifest_select(contract :Contract) {
			this.$os.eventsBus.Bus.emit('map_select', { name: 'manifests', payload: contract } );

		},
		manifest_select_apply(contract :Contract) {
			if(contract) {
				if(this.state.selected.contract ? (this.state.selected.contract.id != contract.id) : true) {
					this.set_list('manifests');
					this.state.selected.contract = contract;
					this.$os.routing.goTo({ name: 'p42_conduit_manifest_contract', params: { id: contract.id, contract: contract }});

					this.$os.scrollView.scroll_to_el('p42_manifest_contracts_list', contract.id.toString(), [-80, 0], 1000);
				}
			} else {
				if(this.state.selected.contract) {
					this.state.selected.contract = null;
					this.$os.routing.goTo({ name: 'p42_conduit' });
				}
			}

			this.$os.eventsBus.Bus.emit('map_state', { name: 'manifests', payload:
				{
					status: this.state.status,
					selected: {
						contract: this.state.selected.contract,
						plan: this.state.selected.plan,
					},
					contracts: this.state.contracts,
				}
			});
		},

		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.state.contracts);
					this.update_results();

					window.requestAnimationFrame(() => {
						if(wsmsg.payload.contract) {
							if(wsmsg.payload.contract.state) {
								this.$os.scrollView.scroll_to_el('p42_manifest_contracts_list', wsmsg.payload.contract.id.toString(), [-80, 0], 1000);
							}
						}
					});
					break;
				}
			}
		},
		listener_map_select(wsmsg :any) {
			switch(wsmsg.name) {
				case 'contracts': {
					this.contract_select_apply(wsmsg.payload != null ? this.state.contracts.find(x => x.id == wsmsg.payload.id) : null);
					break;
				}
				case 'manifests': {
					this.manifest_select_apply(wsmsg.payload != null ? this.state.contracts.find(x => x.id == wsmsg.payload.id) : null);
					break;
				}
				default: {
					if(wsmsg.payload) {
						this.contract_select_apply(null);
						this.manifest_select_apply(null);
					}
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to_pre': {
					this.ui.panel = wsmsg.route.matched.length > 1;
					break;
				}
			}
		},
		listener_map(wsmsg: any) {
			switch(wsmsg.name){
				case 'loaded': {
					this.manifests_search();
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
					this.manifests_search();
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					this.state.status = SearchStates.NoTransponder;
					this.state.contracts = [];
					this.$os.eventsBus.Bus.emit('map_state', { name: 'manifests', payload:
						{
							status: this.state.status,
							selected: {
								contract: this.state.selected.contract,
								//group: this.state.selected.group,
								plan: this.state.selected.plan,
							},
							contracts: this.state.contracts,
						}
					});
					break;
				}
			}
		},
	},
	mounted() {
		this.$emit('loaded');
		this.ui.panel = this.$route.matched.length > 1;

		this.app.events.emitter.on('manifests_close', () => {
			this.manifest_select(null);
		});

		this.app.events.emitter.on('manifests_collapse', () => {
			this.$os.eventsBus.Bus.emit('os', { name: 'uncover', payload: true });
		});

		this.app.events.emitter.on('contracts_close', () => {
			this.contract_select(null);
		});

		this.app.events.emitter.on('contracts_collapse', () => {
			this.$os.eventsBus.Bus.emit('os', { name: 'uncover', payload: true });
		});

		const params = Object.keys(this.$route.params);

		if(this.$os.maps.main) {
			if(params.includes('contract')) {
				this.set_list('contracts');
			} else if(params.includes('manifest')) {
				this.set_list('manifests');
			} else {
				this.contracts_search();
			}
		}
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
		this.$os.eventsBus.Bus.on('map_select', this.listener_map_select);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('map', this.listener_map);
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.off('contracts', this.listener_os_contracts);
		this.$os.eventsBus.Bus.off('map_select', this.listener_map_select);
		this.manifest_select(null);
		this.contract_select(null);
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

	.no-results {
		.theme--bright & {
			&:before {
				background-image: url('../../../sys/assets/icons/dark/truck.svg');
			}
		}
		.theme--dark & {
			&:before {
				background-image: url('../../../sys/assets/icons/bright/truck.svg');
			}
		}
	}

</style>