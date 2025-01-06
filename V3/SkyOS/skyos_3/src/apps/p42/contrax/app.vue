<template>
	<div :class="[appName, app.nav_class]">

		<div class="app-frame" :class="[{ 'has-subcontent': ui.subframe !== null }]">
			<scroll_stack :sid="'p42_contrax_contracts_list'" class="app-box shadowed-deep">
				<template v-slot:top v-if="has_transponder">
					<div class="controls-top">

						<collapser :withArrow="true" :state="ui.filter_panel" class="filters shadowed-shallow" @toggle="ui.filter_panel = $event; filters_updated()">
							<template v-slot:title>
								<div class="filters_header" :class="{ 'has': filters_active.length }">
									<div class="columns">
										<div class="column column_narrow">
											<h2>Filters</h2>
										</div>
										<div class="column"></div>
										<!-- {{ filters_active == 0 ? 'No Filters' : filters_active + ' filter' + (filters_active == 1 ? '' : 's') }} -->
										<div class="column column_narrow column_justify_center column_align_end nowrap">{{ filters_active.length ? filters_get_cat_string(filters_active, 25) : 'No Filters' }}</div>
									</div>
									<div class="collapser_arrow"></div>
								</div>
							</template>
							<template v-slot:content>

								<div class="h_edge_padding">

									<div class="buttons_list shadowed-shallow">
										<textbox type="text" placeholder="Smart Search" placeholder_focused="(ID, ICAOs, whatever else)" v-model="filters.query"></textbox>
									</div>

									<div class="buttons_list shadowed-shallow">
										<button_listed class="arrow" @click.native="ui.subframe == 'filters_aircraft' ? ui.subframe = null : ui.subframe = 'filters_aircraft'">
											<template>Aircraft</template>
											<template v-slot:right>{{ filters_get_cat_string(['types'], 20) }}</template>
										</button_listed>
										<button_listed class="arrow" @click.native="ui.subframe == 'filters_type' ? ui.subframe = null : ui.subframe = 'filters_type'">
											<template>Type</template>
											<template v-slot:right>{{ filters_get_cat_string(['companies', 'type'], 20) }}</template>
										</button_listed>
										<button_listed class="arrow" @click.native="ui.subframe == 'filters_distance' ? ui.subframe = null : ui.subframe = 'filters_distance'">
											<template>Distance</template>
											<template v-slot:right>{{ filters_get_cat_string(['range', 'legsCount'], 20) }}</template>
										</button_listed>
										<button_listed class="arrow" @click.native="ui.subframe == 'filters_airport' ? ui.subframe = null : ui.subframe = 'filters_airport'">
											<template>Airports</template>
											<template v-slot:right>{{ filters_get_cat_string(['runways', 'rwyCount', 'rwySurface', 'requiresILS', 'requiresLight'], 20) }}</template>
										</button_listed>
									</div>
									<div class="buttons_list shadowed-shallow">
										<button_listed class="arrow" @click.native="ui.subframe == 'filters_sort' ? ui.subframe = null : ui.subframe = 'filters_sort'">
											<template>Sorting</template>
											<template v-slot:right>{{ filters_get_cat_string(['sort', 'sortAsc'], 20) }}</template>
										</button_listed>
									</div>

									<div class="columns columns_margined_half">
										<div class="column column_narrow column_h-stretch">
											<button_action class="cancel shadowed-shallow" @click.native="filters_reset()">Clear All</button_action>
										</div>
										<div class="column column_3 column_h-stretch">
											<button_action class="go shadowed-shallow" @click.native="contracts_search_region()">Search</button_action>
										</div>
									</div>
								</div>


							</template>
						</collapser>

					</div>
				</template>
				<template v-slot:content class="confine">

					<collapser :withArrow="true" :state="section.state" v-for="(section, index) in state_sections" v-bind:key="index" @toggle="section.state = $event">
						<template v-slot:title>
							<div class="section_header">
								<div class="columns">
									<div class="column column_narrow">
										<h2>{{ section.name }}</h2>
									</div>
									<div class="column"></div>
									<div class="column column_narrow column_justify_center column_align_end nowrap">
										<span class="notice" v-if="section.contracts.length">
											<number :amount="section.offset" :decimals="0"/>
											<span>-</span>
											<number :amount="section.offset + section.contracts.length" :decimals="0"/>
											<span> of </span>
											<number :amount="section.count" :decimals="0"/>
										</span>
										<span class="notice" v-else>
											<number :amount="0" :decimals="0"/>
										</span>
									</div>
								</div>
								<div class="collapser_arrow"></div>
							</div>
						</template>
						<template v-slot:content>

							<pagination
								class="h_edge_padding_vertical_half"
								:qty="section.count"
								:limit="section.limit"
								:offset="section.offset"
								@set_offset="section.offset = $event; contracts_search(section)"/>

							<div class="search_results" :class="['search_results_' + section.code ]">
								<div class="app-box no-transponder" v-if="section.status == 2">
									<span class="title">No Transponder</span>
									<p>The Skypark Transponder is required to browse contracts.</p>
									<p>Please start your Transponder.</p>
								</div>
								<div class="app-box no-results" v-else-if="section.status == 3">
									<span class="title">{{ section.no_results[0] }}</span>
									<p>{{ section.no_results[1] }}</p>
								</div>
								<div v-else :class="{ 'loading': section.status == 1 }">
									<ContractBox
										v-for="(contract, index) in section.contracts"
										v-bind:key="contract.id + '_' + section.search_index"
										:data-anchor="contract.id.toString()"
										:index="index"
										:contract="contract"
										:selected="state.selected.contract"
										@details="contract_select(contract)"/>
								</div>
							</div>

						</template>
					</collapser>

				</template>
			</scroll_stack>
		</div>

		<div class="app-subframes">
			<div class="app-subframe" :class="{ 'has-content': ui.subframe !== null }">
				<transition :duration="1000">
					<PanelFiltersType
						v-if="ui.subframe == 'filters_type'"
						:filters="filters"
						@close="ui.subframe = null"
						@changed="filters_updated()"
						@reset="filters_reset()"
						@search="contracts_search()"/>
					<PanelFiltersAircraft
						v-if="ui.subframe == 'filters_aircraft'"
						:filters="filters"
						@close="ui.subframe = null"
						@changed="filters_updated()"
						@reset="filters_reset()"
						@search="contracts_search()"/>
					<PanelFiltersDistance
						v-if="ui.subframe == 'filters_distance'"
						:filters="filters"
						@close="ui.subframe = null"
						@changed="filters_updated()"
						@reset="filters_reset()"
						@search="contracts_search()"/>
					<PanelFiltersAirport
						v-if="ui.subframe == 'filters_airport'"
						:filters="filters"
						@close="ui.subframe = null"
						@changed="filters_updated()"
						@reset="filters_reset()"
						@search="contracts_search()"/>
					<PanelFiltersSort
						v-if="ui.subframe == 'filters_sort'"
						:filters="filters"
						@close="ui.subframe = null"
						@changed="filters_updated()"
						@reset="filters_reset()"
						@search="contracts_search()"/>

				</transition>
			</div>
		</div>
		<div class="map-controls">
			<button_action class="info" @click.native="contracts_search_region()">Search this region</button_action>
		</div>
		<app_panel :has_content="ui.panel" :has_subcontent="ui.subframe !== null" :scroll_top="ui.panel_scroll_top">
			<transition :duration="800">
				<router-view
					@scroll="ui.panel_scroll_top = $event.scrollTop"
					@route_code="contract_route_code($event)"
					@template_code="contract_template_code($event)"
				></router-view>
			</transition>
		</app_panel>

  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import Filter from "./classes/filter";
import SearchStates from "@/sys/enums/search_states";
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import Airport from "@/sys/classes/airport";
import { AppInfo } from "@/sys/foundation/app_model"
import Flightplan from '@/sys/classes/flight_plans/plan';

import PanelFiltersAircraft from "./subframes/filters_aircraft.vue";
import PanelFiltersType from "./subframes/filters_type.vue";
import PanelFiltersDistance from "./subframes/filters_distance.vue";
import PanelFiltersAirport from "./subframes/filters_airport.vue";
import PanelFiltersSort from "./subframes/filters_sort.vue";
import MapFeaturesConfig from '@/sys/classes/maps/map_features_config';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		PanelFiltersType,
		PanelFiltersAircraft,
		PanelFiltersDistance,
		PanelFiltersAirport,
		PanelFiltersSort,
		ContractBox: () => import("./components/contract.vue"),
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			filters: new Filter(),
			filters_default: new Filter(),
			ui: {
				panel: false,
				subframe: null,
				filter_panel: false,
				panel_scroll_top: 0,
				search_index: 0,
				filter_keys: [],
				filter_strings: [],
				filter_keys_force: ['sort'],
			},
			state: new MapFeaturesConfig({
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
			}),
			state_sections: [
				{
					code: "active",
					name: "Active",
					filter: "Active",
					no_results: ['No active contracts', 'Get to the starting point to begin a saved contract.'],
					offset: 0,
					limit: 5,
					count: 0,
					bounds: false,
					search_index: 0,
					state: false,
					contracts: [] as Contract[],
					status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
				},
				{
					code: "saved",
					name: "Saved",
					filter: "Saved",
					no_results: ['No saved contracts', 'Browse below and save the ones you want.'],
					offset: 0,
					limit: 5,
					count: 0,
					bounds: false,
					search_index: 0,
					state: false,
					contracts: [] as Contract[],
					status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
				},
				{
					code: "listed",
					name: "Available",
					filter: "Listed",
					no_results: ['No results', 'Try moving the map to another location or adjust your filters.'],
					offset: 0,
					limit: 20,
					count: 0,
					bounds: true,
					search_index: 0,
					state: true,
					contracts: [] as Contract[],
					status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
				}
			]
		}
	},
	methods: {

		filters_reset() {
			this.filters = new Filter();
			this.filters_updated();
		},

		contracts_reset(section = null) {

			if(!section){
				this.state.count = 0;
				this.state.contracts = this.$os.contract_service.dispose_list(this.state.contracts);
				this.state.status = SearchStates.Idle;
				this.state.selected.contract = null;
				this.state.selected.plan = null;
				this.state.contracts = [];
				this.state_sections.forEach(section => {
					section.contracts = this.$os.contract_service.dispose_list(section.contracts);
					section.count = 0;
					section.offset = 0;
					section.status = SearchStates.Idle;
				});
			} else {
				this.state.count -= section.contracts.length;

				section.contracts.forEach(contract => {
					const index = this.state.contracts.indexOf(contract);
					this.state.contracts.splice(index, 1);
				});

				section.contracts = this.$os.contract_service.dispose_list(section.contracts);
			}

			this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });
		},

		contracts_search_region(){
			this.ui.filter_panel = false;

			const listed = this.state_sections.find(x => x.code == 'listed');
			listed.offset = 0;
			this.contracts_search(listed);
		},

		contracts_search(section = null) {

			this.filters_updated();
			this.ui.filter_panel = false;

			const filters = this.filters;
			this.app.state_set(['filters'], filters);

			if(this.has_transponder) {
				//this.state.status = SearchStates.Searching;

				const center = this.$os.maps.main.map.getCenter();
				const zoom = this.$os.maps.main.map.getZoom();

				const queryoptions = {
					type: filters.type,
					subType: filters.subType,
					types: filters.types,
					companies: filters.companies,
					//state: 'Listed,Saved,Active',
					//priorityState: 'Saved,Active',
					location: this.$os.simulator.live ? [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat] : null,
					range: filters.range,
					legsCount: filters.legsCount,
					runways: filters.runways,
					rwySurface: filters.rwySurface,
					rwyCount: filters.rwyCount,
					query: filters.query,
					//bounds: zoom > 0.8 ? this.$os.maps.bounds : null,
					weatherExcl: filters.weatherExcl,
					requiresLight: filters.requiresLight ? filters.requiresLight : null,
					onlyCustomContracts: filters.onlyCustomContracts ? filters.onlyCustomContracts : null,
					requiresILS: filters.requiresILS ? filters.requiresILS : null,
					center: [center.lng, center.lat],
					sort: filters.sort,
					sortAsc: filters.sortAsc,
					diced: false,
					detailed: false,
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
							limits: true,
							manifests: {
								total_weight: true
							},
							situations: {
								id: true,
								location: true,
								trigger_range: true,
								label: true,
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

				this.state_sections.filter(s => section ? s == section : true).forEach(section_l => {

					section_l.status = SearchStates.Searching;
					//section_l.contracts = [];
					//section_l.count = 0;

					this.$os.api.send_ws('adventures:query-from-filters',
						Object.assign(queryoptions, {
							state: section_l.filter,
							limit: section_l.limit,
							bounds: section_l.bounds ? (zoom > 0.8 ? this.$os.maps.bounds : null) : null,
							offset: section_l.offset
						}),
						(data: any) => {

							this.contracts_reset(section_l);

							if(data.payload.contracts.length) {

								// General State
								const new_contracts = this.$os.contract_service.ingest_list(data.payload.contracts, data.payload.templates);
								this.state.contracts = this.state.contracts.concat(new_contracts);
								this.state.count += data.payload.count,

								// Section
								section_l.contracts = new_contracts;
								section_l.count = data.payload.count;
								section_l.status = SearchStates.Idle;

								section_l.state = true;
							} else {
								//this.state.status = SearchStates.NoResults;
								//this.state.count = 0;

								section_l.contracts = [];
								section_l.count = 0;
								section_l.status = SearchStates.NoResults;

								section_l.state = section_l.code == 'listed' ? true : false;
							}

							section_l.search_index++;
							this.update_results();
						}
					);
				});

			}
		},

		filters_updated(){

			const filter_translate = (key) => {
				switch(key){
					case 'query': {
						return this.filters.query;
					}
					case 'types': {
						const cos = [];
						this.$os.aircraftTypes.library.forEach((co, i) => {
							if(!this.filters.types.includes(i))
								cos.push(co.name);
						});
						return cos.length < 3 ? cos.join(', ') : cos.length + ' types';
					}
					case 'companies': {
						const cos = [];
						this.$os.companies.library.forEach((co) => {
							if(!this.filters.companies.includes(co.code))
								cos.push(co.short);
						});
						return cos.length < 3 ? cos.join(', ') : cos.length + ' companies';
					}
					case 'requiresLight': {
						return 'Req. Rwy Lights';
					}
					case 'requiresILS': {
						return 'Req. ILS';
					}
					case 'runways': {
						return this.$os.units.ConvertDistance(this.filters.runways[0], 0, false) + ' - ' + this.$os.units.ConvertDistance(this.filters.runways[1], 0);
					}
					case 'rwyCount': {
						return this.filters.rwyCount[0] + ' - ' + this.filters.rwyCount[1] +' rwys';
					}
					case 'rwySurface': {
						return this.filters.rwySurface;
					}
					case 'type': {
						switch(this.filters.type){
							case 'Cargo': return 'Cargo';
							case 'Passengers': return 'Passengers';
							case 'Rescue': return 'Rescue';
							case 'Tour': return 'Tour';
							case 'Experience': return 'Experience';
							case 'Ferry': return 'Ferry';
							case 'Bush Trip': return 'Bush Trip';
							default: return 'Unknown';
						}
					}
					case 'range': {
						return this.$os.units.ConvertDistance(this.filters.range[0], 0, false) + ' - ' + this.$os.units.ConvertDistance(this.filters.range[1], 0);
					}
					case 'legsCount': {
						return this.filters.legsCount[0] + ' - ' + this.filters.legsCount[1] +' legs';
					}
					case 'sort': {
						switch(this.filters.sort){
							case 'relevance': return 'Relevance';
							case 'topography_var': return 'Topo. variations';
							case 'aircraft': return 'Aircraft location';
							case 'distance': return 'Distance';
							case 'ending': return 'Ending soon';
							case 'payload': return 'Payload';
							case 'reward': return 'Pay';
							case 'xp': return 'XP';
							default: return 'Unknown';
						}
					}
					default: return null;
				}
			};

			this.ui.filter_keys = [];
			this.ui.filter_strings = [];

			Object.keys(this.filters_default).forEach(k => {
				if(this.ui.filter_keys_force.includes(k)) {
					const str = filter_translate(k);
					this.ui.filter_keys.push(k);
					this.ui.filter_strings.push(str);
				} else {
					const c1 = JSON.stringify(this.filters_default[k]);
					const c2 = JSON.stringify(this.filters[k]);
					if(c1 != c2) {
						const str = filter_translate(k);
						if(str) {
							this.ui.filter_keys.push(k);
							this.ui.filter_strings.push(str);
						}
					}
				}
			});
		},

		filters_get_cat_string(keys :string[], limit :number) {

			const comb = [];
			keys.forEach(k => {
				const i = this.ui.filter_keys.indexOf(k);
				if(i > -1) {
					if(this.ui.filter_strings[i]){
						comb.push(this.ui.filter_strings[i]);
					}
				}
			});

			const str = comb.join(', ');
			return str.length ? (str.length <= limit ? str : comb.length + ' filters' ) : 'Any';
		},

		update_results() {

			// Switch the contracts to the appropriate section
			this.state_sections.forEach(section => {
				const changed = section.contracts.filter(c => c.state != section.filter);
				changed.forEach(contract => {
					const index = section.contracts.indexOf(contract);
					section.contracts.splice(index, 1);
					section.count -= 1;
					const new_section = this.state_sections.find(x => x.filter == contract.state);
					if(new_section) {
						new_section.contracts.unshift(contract);
						new_section.count += 1;
					}
				});
			});

			// Propagate the good news
			this.$os.eventsBus.Bus.emit('map_state', { name: 'contracts', payload: this.state });

			// Select the right contract based on the URL
			if(this.$route.params.id) {
				const found = this.state.contracts.find(x => x.id == this.$route.params.id);
				if(found) {
					this.contract_select(found);
				}
			}

		},

		contract_route_code(code :string) {
			this.filters.query = "#" + code;
			this.contracts_search();
		},

		contract_template_code(code :string) {
			this.filters.query = "#" + code;
			this.contracts_search();
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
				this.$os.routing.goTo({ name: 'p42_contrax_contract', params: { id: contract.id, contract: contract }});
				this.$os.scrollView.scroll_to_el('p42_contrax_contracts_list', contract.id.toString(), [-80, 0], 1000);
			} else {
				this.$os.eventsBus.Bus.emit('map_select', { name: 'contracts', payload: null } );
				this.$os.routing.goTo({ name: 'p42_contrax' });
			}
		},

		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {

					this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.state.contracts);

					this.state_sections.forEach(section => {
						const initial_count = section.contracts.length;
						this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, section.contracts);
						const delta = section.contracts.length - initial_count;
						section.count += delta;
						if(section.count > 0) {
							if(section.status == SearchStates.NoResults) {
								section.state = true;
								section.status = SearchStates.Idle;
							}
						} else {
							if(section.status == SearchStates.Idle)
								section.status = SearchStates.NoResults;
						}
					});

					this.update_results();

					window.requestAnimationFrame(() => {
						if(wsmsg.payload.contract) {
							if(wsmsg.payload.contract.state) {
								this.$os.scrollView.scroll_to_el('p42_contrax_contracts_list', wsmsg.payload.contract.id.toString(), [-80, 0], 1000);
							}
						}
					});
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
					this.state.status = SearchStates.Idle;
					this.contracts_search();
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					this.state.status = SearchStates.NoTransponder;
					this.contracts_reset();
					break;
				}
			}
		},
	},
	mounted() {
		this.$emit('loaded');
		this.ui.panel = this.$route.matched.length > 1;

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
		const nf = new Filter(this.app.state_get(['filters'], this.filters));
		this.filters = nf;

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

	},
	computed: {
		filters_active():any {
			return this.ui.filter_keys.filter(x => !this.ui.filter_keys_force.includes(x))
		}
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
		& > .columns {
			flex-grow: 1;
		}
		h2 {
			margin: 0;
		}
		.notice {
			opacity: 0.5;
		}
	}

	.filters {
		.theme--bright & {
			background-color: rgba($ui_colors_bright_shade_3, 0.3);
			color: $ui_colors_bright_shade_5;
		}
		.theme--dark & {
			background-color: rgba($ui_colors_dark_shade_3, 0.3);
			color: $ui_colors_dark_shade_5;
		}

		.filters_header {
			display: flex;
			justify-content: space-between;
			align-items: center;
			padding: 8px 16px;
			padding-top: 16px;
			transition: background 0.3s ease-out;
			.theme--bright & {
				&.has {
					background-color: rgba($ui_colors_bright_button_cancel, 1);
				}
			}
			.theme--dark & {
				&.has {
					background-color: rgba($ui_colors_dark_button_cancel, 1);
				}
			}

			& > .columns {
				flex-grow: 1;
			}
			h2 {
				margin: 0;
			}
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

	.search_results {
		margin: 6px;
		border-radius: 8px;
		&_saved,
		&_active {
			.no-results {
				.theme--bright & {
					&:before {
						background-image: url('../../../sys/assets/icons/dark/black_hole.svg');
					}
				}
				.theme--dark & {
					&:before {
						background-image: url('../../../sys/assets/icons/bright/black_hole.svg');
					}
				}
			}
		}
		.loading {
			pointer-events: none;
			opacity: 0.2;
		}
	}


</style>