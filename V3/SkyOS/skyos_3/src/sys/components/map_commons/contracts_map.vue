<template>
	<div>

		<MglGeojsonLayer
			:sourceId="name + '_contract_paths'"
			:layerId="name + '_contract_paths_shadow'"
			:source="ui.map.sources.contract_paths"
			:layer="ui.map.layers.contract_paths_shadow"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_contract_paths'"
			:layerId="name + '_contract_paths'"
			:source="ui.map.sources.contract_paths"
			:layer="ui.map.layers.contract_paths"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_location'"
			:layerId="name + '_location'"
			:source="ui.map.sources.location"
			:layer="ui.map.layers.location"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_location'"
			:layerId="name + '_location_poly'"
			:source="ui.map.sources.location"
			:layer="ui.map.layers.location_poly"
		/>

		<MglGeojsonLayer
			:sourceId="name + 'location'"
			:layerId="name + 'location_label'"
			:source="ui.map.sources.location"
			:layer="ui.map.layers.location_label"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_range'"
			:layerId="name + '_range'"
			:source="ui.map.sources.range"
			:layer="ui.map.layers.range"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_range_out'"
			:layerId="name + '_range_out'"
			:source="ui.map.sources.range_out"
			:layer="ui.map.layers.range_out"
		/>

		<div v-for="(marker, index) in ui.map.markers" v-bind:key="index">
			<MglMarker :coordinates="marker.location">
				<div class="map_start_marker" :class="['map_marker', 'type-' + marker.type, { 'selected': state.selected.contract ? (state.selected.contract.id == marker.id) : false, 'ghost': state.selected.contract ? (state.selected.contract.id != marker.id) : false }]" slot="marker">
					<div></div>
				</div>
			</MglMarker>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import * as turf from '@turf/turf';
import { MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import SearchStates from "@/sys/enums/search_states";
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import Eljs from '@/sys/libraries/elem';
import Flightplan from '@/sys/classes/flight_plans/plan';
import MapFeaturesConfig from '@/sys/classes/maps/map_features_config';

export default Vue.extend({
	props: {
		name: String,
	},
	components: {
		MglMarker,
		MglNavigationControl,
		MglGeojsonLayer,
	},
	data() {
		return {
			app: this.$os.routing.activeApp,
			ui: {
				map: {
					anim: null,
					selected: {
						source: null as string,
						id: null as number,
					},
					hovered: [] as {
						source: string,
						layer: string,
						id: number,
					}[],
					markers: [] as {
						guid: string,
						id: number,
						location: number[],
						type: string,
					}[],
					sources: {
						contract_paths: {
							type: 'geojson',
							lineMetrics: true,
							data: {
								type: 'FeatureCollection',
								features: []
							}
						},
						location: {
							type: 'geojson',
							data: {
								type: 'FeatureCollection',
								features: []
							}
						},
						range: {
							type: 'geojson',
							data: {
								type: 'FeatureCollection',
								features: []
							}
						},
						range_out: {
							type: 'geojson',
							data: {
								type: 'FeatureCollection',
								features: []
							}
						}
					},
					layers: {
						contract_paths: {
							id: this.name + '_contract_paths',
							type: 'line',
							source: this.name + '_contract_paths',
							layout: {
								"line-join": "round",
								"line-cap": "round"
							},
							paint: {
								//'line-color': 'transparent',
								'line-gradient': [
									'interpolate',
									['linear'],
									['line-progress'],
									0, 'transparent',
									1, 'transparent'
								],
								'line-width': [
									"case",
									["boolean", ["feature-state", "selected"], false], 6,
									["boolean", ["feature-state", "hover"], false], 4,
									2
								],
								'line-opacity': [
									"case",
									["boolean", ["feature-state", "selected"], false], 1,
									["boolean", ["feature-state", "ghost"], false], 0.3,
									["boolean", ["feature-state", "hover"], false], 0.9,
									1
								]
							}
						},
						contract_paths_shadow: {
							id: this.name + '_contract_paths_shadow',
							type: 'line',
							source: this.name + '_contract_paths',
							layout: {
								"line-join": "round",
								"line-cap": "round"
							},
							paint: {
								'line-color': 'transparent',
								'line-width': [
									"case",
									["boolean", ["feature-state", "selected"], false], 12,
									["boolean", ["feature-state", "hover"], false], 12,
									8
								],
								'line-opacity': [
									"case",
									["boolean", ["feature-state", "selected"], false], 1,
									["boolean", ["feature-state", "ghost"], false], 0.1,
									["boolean", ["feature-state", "hover"], false], 0.9,
									0.8
								]
							}
						},
						location: {
							id: this.name + "_location",
							type: 'circle',
							source: this.name + '_location',
							paint: {
								'circle-pitch-alignment': 'map',
								'circle-color': null as any,
								'circle-stroke-opacity': 1,
								'circle-stroke-color': '#000000',
								'circle-stroke-width': [
									"interpolate",
									["linear"],
									["zoom"],
									0, 1,
									3, 1,
									5, 2,
									14, 4
								],
								'circle-radius': [
									"interpolate",
									["linear"],
									["zoom"],
									0, ["case",["boolean", ["feature-state", "hover"], false], 8, 2],
									14, ["case",["boolean", ["feature-state", "hover"], false], 9, 8]
								]

							},
							filter: ['in', '$type', 'Point']
						},
						location_poly: {
							id: this.name + '_location_poly',
							type: 'fill',
							source: this.name + '_location',
							layout: {},
							paint: {
								'fill-color': "#FFFFFF",
								'fill-opacity': 0.5
								//"line-color": ,
								//"line-width": 5,
								//"line-opacity": 1,
							},
							filter: ['in', '$type', 'Polygon']
						},
						location_label: {
							id: this.name + '_location_label',
							type: 'symbol',
							source: this.name + '_location',
							layout: {
								"text-field": ['get', 'title'],
								"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
								'text-variable-anchor': ['top'], // 'top', 'bottom', 'left', 'right'
								'text-justify': 'left',
								'text-radial-offset': [
									"interpolate",
									["linear"],
									["zoom"],
									0, 0.5,
									14, 1.5
								],
								"text-size": 12,
							},
							paint: {
								'text-color': "#FFF",
								'text-halo-color': "#FFF",
								'text-halo-width': 2,
							}
						},
						range: {
							type: "line",
							source: this.name + '_range',
							paint: {
								'line-color': '#FF0000',
								'line-width': 3,
								'line-opacity': 0.5
							}
						},
						range_out: {
							type: "line",
							source: this.name + '_range_out',
							paint: {
								'line-color': '#FF0000',
								'line-width': 1,
								'line-opacity': ['get', 'opacity'],
							}
						},
					},
				}
			},
			state: new MapFeaturesConfig({
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
			}),
			blanket: [] as Contract[]
		}
	},
	methods: {

		init() {
			['mousemove'].forEach((ev_type) => { this.$os.maps.main.map.on(ev_type, this.map_feature_move); }); // Move event

			let touch_cancel = false;
			['dragstart'].forEach((ev) => {
				this.$os.maps.main.map.on(ev, (ev1 :any) => {
					touch_cancel = true;
				});
			});

			['mousedown', 'touchstart'].forEach((ev_type) => {
				this.$os.maps.main.map.on(ev_type, (ev) => {
					touch_cancel = false;
				});
			});

			['mouseup', 'touchend'].forEach((ev_type) => {
				this.$os.maps.main.map.on(ev_type, (ev) => {
					if(!touch_cancel) {
						this.map_feature_click(ev);
					}
					touch_cancel = false;
				});
			});
		},

		blanket_search() {

			this.blanket_reset();

			const queryoptions = {
				state: 'Saved,Active',
				fields: {
					contract: {
						id: true,
						title: true,
						description: true,
						recommended_aircraft: true,
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
						this.blanket = this.$os.contract_service.ingest_list(contractsData.payload.contracts, contractsData.payload.templates);
					}

					this.blanket_update();
				}
			);

		},

		blanket_update() {

			// Clear ghosting
			this.ui.map.sources.contract_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_contract_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing ones
			this.ui.map.sources.contract_paths.data.features = [];

			this.ui.map.markers = [];

			if(this.app.show.contracts) {
				const contracts = this.state.contracts.length ? this.state.contracts : this.blanket;
				//const processed_airports = [] as string[];
				// Go through all contracts
				contracts.forEach(contract => {

					if(contract.expired) {
						return;
					}

					// Initial feature for the current contract
					const feature = {
						type: 'Feature',
						id: contract.id,
						properties: {
							color: "#CCCCCC",
						},
						geometry: {
							type: 'MultiLineString',
							coordinates: [[]] as any[]
						},
					}

					// Loop all situations
					let previous_node = null as number[];
					contract.situations.forEach((situation :Situation, index :number) => {
						if(index > 0){
							var start = turf.point(previous_node);
							var end = turf.point(situation.location);

							// Let's create a great circle between the 3 last coordinates
							var greatCircle = turf.greatCircle(start, end);
							if(greatCircle.geometry.type == 'LineString'){
								greatCircle.geometry.coordinates.forEach(coordinate => {
									feature.geometry.coordinates[0].push(coordinate);
								});
							} else {
								greatCircle.geometry.coordinates.forEach((pos: any) => {
									feature.geometry.coordinates.push(pos);
								});
							}
						}
						previous_node = situation.location;
					});

					// Add company marker
					if(this.state.settings.contract_show_badges) {
						this.ui.map.markers.push({
							location: contract.situations[0].location,
							id: contract.id,
							guid: '',
							type: contract.template.company[0]
						});
					}

					// Push new contract feature to map
					this.ui.map.sources.contract_paths.data.features.push(feature);

					// If currently selected, reapply selection
					//if(this.state.selected.contract)
					//	this.contract_select_apply(this.state.selected.contract.id);

				});

			}
			this.contract_update_resume_circle();
		},

		blanket_reset() {
			this.contract_select_apply(null);
			this.blanket = this.$os.contract_service.dispose_list(this.blanket);
			this.state = new MapFeaturesConfig();
		},

		contract_expire(id :number) {

			const index_feature = this.ui.map.sources.contract_paths.data.features.findIndex(x => x.id == id);
			if(index_feature > -1) {
				this.ui.map.sources.contract_paths.data.features.splice(index_feature, 1);
			}

			const index_feature_labels = this.ui.map.sources.location.data.features.filter(x => x.properties.id == id);
			index_feature_labels.forEach(element => {
				const index_feature_label = this.ui.map.sources.location.data.features.findIndex(x => x == element)
				if(index_feature_label > -1) {
					this.ui.map.sources.location.data.features.splice(index_feature_label, 1);
				}
			});

			//const index_contract = this.state.contracts.findIndex(x => x.id == id);
			//if(index_contract > -1) {
			//	this.state.contracts.splice(index_contract, 1);
			//}

		},

		contract_select(id :number) {

			let contract = this.state.contracts.find(x => x.id == id);
			if(!contract) { contract = this.blanket.find(x => x.id == id); }

			this.$os.eventsBus.Bus.emit('map_selected', { name: 'contracts', payload: contract } );
		},

		contract_select_apply(id :number) {

			// Clear features
			this.ui.map.sources.location.data.features = [];

			// Clear ghosting
			this.ui.map.sources.contract_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_contract_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing selection
			if(this.ui.map.selected.source) {
				this.$os.maps.main.map.setFeatureState({ source: this.ui.map.selected.source, id: this.ui.map.selected.id }, { selected: false });
				this.ui.map.selected.source = null;
				this.ui.map.selected.id = null;
			}

			// If we have an ID, select it on the map
			if(id != null) {

				let contract = this.state.contracts.find(x => x.id == id);

				if(!contract) {
					contract = this.blanket.find(x => x.id == id);
				}

				if(!contract) {
					return;
				}

				if(contract.expired) {
					return;
				}

				this.state.selected.contract = contract;

				// Set Ghost
				this.ui.map.sources.contract_paths.data.features.forEach(feature => {
					if(feature.id != id)
						this.$os.maps.main.map.setFeatureState({ source: this.name + '_contract_paths', id: feature.id }, { ghost: true });
				})

				// Add Locations
				contract.situations.forEach((situation, index) => {

					const loc_feature = {
						type: "Feature",
						properties: {
							id: id,
							type: index == 0 ? 'start' : index < contract.situations.length - 1 ? 'mid' : 'end',
							display: 'unknown',
							title: 'Unknown',
						},
						geometry: null as any
					};

					if(situation.airport) {
						loc_feature.properties.title = situation.airport.icao;
						loc_feature.properties.display = 'airport';
						loc_feature.geometry = {
							type: "Point",
							coordinates: situation.location,
						}
					} else {
						loc_feature.properties.title = situation.label;
						loc_feature.properties.display = 'range';
						loc_feature.geometry = {
							type: "Polygon",
							coordinates: [
								Object.assign({}, turf.circle(situation.location, situation.trigger_range, { steps: 20, properties: {} })).geometry.coordinates[0]
							]
						}
					}

					this.ui.map.sources.location.data.features.push(loc_feature);
				});

				// Set Selected
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_contract_paths', id: id }, { selected: true });
				this.ui.map.selected.source = this.name + '_contract_paths';
				this.ui.map.selected.id = id;

				// Frame view on contract
				window.requestAnimationFrame(() => {
					this.contract_frame(this.$os.system.map_covered);
				});

			} else {
				this.state.selected.contract = null;
			}

			this.contract_update_resume_circle();
		},


		contract_frame(instant :boolean) {
			if(this.ui.map.selected) {
				let contract = this.state.contracts.find(x => x.id == this.ui.map.selected.id);

				if(!contract) {
					contract = this.blanket.find(x => x.id == this.ui.map.selected.id);
				}

				if(contract) {
					const nodes = [] as any[];
					contract.situations.forEach((situation :Situation) => {
						nodes.push(situation.location);
					});

					this.$os.eventsBus.Bus.emit('map', {
						name: 'goto',
						type: 'area',
						payload: {
							nodes: nodes,
						}
					});
				}

			}
		},


		contract_update_resume_circle() {
			this.ui.map.sources.range.data.features = [];
			this.ui.map.sources.range_out.data.features = [];

			if(this.app.show.contracts) {
				this.state.contracts.forEach(contract => {

					if(this.state.selected.contract ? contract.id == this.state.selected.contract.id : false) {
						// Show resume circles
						if(contract.last_location_geo && contract.state == 'Active' && !contract.is_monitored) {
							this.ui.map.sources.range.data.features.push(turf.circle(contract.last_location_geo, 14.816, { steps: 90, units: 'kilometers' }));
							for (let i = 6; i < 20; i++) {
								const circle = turf.circle(contract.last_location_geo, (1 + Math.pow((i * 0.5), 3)), {
									steps: 90,
									units: 'kilometers',
									properties: {
										opacity: Eljs.round(Math.pow(0.8, i), 2) * 1
									}
								});
								this.ui.map.sources.range_out.data.features.push(circle);
							}
						}
					}
				});
			}
		},

		map_feature_click(e: any) {
			if(this.app.show.contracts) {

				// Get features under the cursor
				const features = this.$os.maps.main.map.queryRenderedFeatures(e.point).filter(x => x.layer.id == this.name + '_contract_paths_shadow');
				const parents = Eljs.getDOMParents(e.originalEvent.target).filter(x => x.nodeName == "DIV");
				const marker = parents.find(x => (x as HTMLElement).classList.contains('map_marker'));

				// Make sure we're not hovering a marker
				if(!marker){
					if(features.length) {
						const feature = features[0];
						let contract = this.state.contracts.find(x => x.id == feature.id);
						if(!contract) {
							contract = this.blanket.find(x => x.id == feature.id);
						}
						if(contract){
							this.contract_select(contract.id);
						} else {
							this.contract_select(null);
						}
					} else {
						this.contract_select(null);
					}
				}
			}
		},

		map_feature_move(e: any) {
			if(this.app.show.contracts) {
				// Get features under the cursor
				const features = this.$os.maps.main.map.queryRenderedFeatures(e.point);
				const parents = Eljs.getDOMParents(e.originalEvent.target).filter(x => x.nodeName == "DIV");
				const marker = parents.find(x => (x as HTMLElement).classList.contains('map_marker'));

				// Make sure we're not hovering a marker
				if(!marker){
					// Go through all of them
					features.forEach(feature => {
						switch(feature.layer.id) {
							case this.name + '_contract_paths_shadow': {
								// Check if hovered or hover
								const is_hovered = this.ui.map.hovered.find(x => x.layer == feature.layer.id);
								if(!is_hovered) {
									this.ui.map.hovered.push({ source: feature.source, layer: feature.layer.id, id: feature.id });
									this.$os.maps.main.map.setFeatureState({ source: feature.source, id: feature.id }, { hover: true });
								}
								break;
							}
						}
					});
					// Unhover what was hovered
					this.ui.map.hovered.forEach((hovered) => {
						const still_hovered = features.find(x => x.source == hovered.source && x.layer.id == hovered.layer && x.id == hovered.id);
						if(!still_hovered) {
							this.ui.map.hovered.splice(this.ui.map.hovered.indexOf(hovered), 1);
							this.$os.maps.main.map.setFeatureState({ source: hovered.source, id: hovered.id }, { hover: false });
						}
					});
				} else {
					// Unhover everything
					this.ui.map.hovered.forEach((hovered) => {
						this.ui.map.hovered.splice(this.ui.map.hovered.indexOf(hovered), 1);
						this.$os.maps.main.map.setFeatureState({ source: hovered.source, id: hovered.id }, { hover: false });
					});
				}
			}
		},

		map_layers_reorder() {
			const order = {}

			order[this.name + '_contract_paths_shadow'] = 'split_1_1';
			order[this.name + '_contract_paths'] = 'split_1_1';

			order[this.name + '_range'] = 'split_0_1';
			order[this.name + '_range_out'] = 'split_0_1';

			order[this.name + '_location_poly'] = 'split_1_2';
			order[this.name + '_location_label'] = 'split_1_2';

			Object.keys(order).forEach(element => {
				const entry = order[element];
				this.$os.maps.main.map.moveLayer(element, entry);
			});
		},


		theme_update() {
			const layers = this.ui.map.layers;

			if((this.$os.userConfig.get(['ui', 'theme']) == 'theme--bright' && !this.$os.maps.display_layer.sat) || this.$os.maps.display_layer.sectional.us.enabled) {

				layers.contract_paths_shadow.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_shade_1;

				let npp = [
					'interpolate',
					['linear'],
					['line-progress'],
					0, this.$os.theme.colors.$ui_colors_bright_button_go,
					0.1, this.$os.theme.colors.$ui_colors_bright_button_info,
					0.9, this.$os.theme.colors.$ui_colors_bright_button_info,
					1, this.$os.theme.colors.$ui_colors_bright_button_cancel,
				]
				layers.contract_paths.paint['line-gradient'] = npp;
				this.$os.maps.main.map.setPaintProperty(this.name + "_contract_paths", 'line-gradient', npp);
				layers.contract_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_info;


				npp = [
					'match',
						['get', 'type'],
						'start', this.$os.theme.colors.$ui_colors_bright_button_go,
						'end', this.$os.theme.colors.$ui_colors_bright_button_cancel,
					this.$os.theme.colors.$ui_colors_bright_button_info
				];
				layers.location.paint['circle-color'] = npp;
				layers.location.paint['circle-stroke-color'] = this.$os.theme.colors.$ui_colors_bright_shade_1;
				this.$os.maps.main.map.setPaintProperty(this.name + "_location", 'circle-color', npp);

				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-color', this.$os.theme.colors.$ui_colors_bright_shade_5);
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-halo-color', this.$os.theme.colors.$ui_colors_bright_shade_1);

				layers.location_poly.paint['fill-color'] = this.$os.theme.colors.$ui_colors_bright_button_info;

				layers.range.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_cancel;
				layers.range_out.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_cancel;

			} else {

				layers.contract_paths_shadow.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_shade_1;

				let npp = [
					'interpolate',
					['linear'],
					['line-progress'],
					0, this.$os.theme.colors.$ui_colors_dark_button_go,
					0.1, this.$os.theme.colors.$ui_colors_dark_button_info,
					0.9, this.$os.theme.colors.$ui_colors_dark_button_info,
					1, this.$os.theme.colors.$ui_colors_dark_button_cancel,
				]
				layers.contract_paths.paint['line-gradient'] = npp;
				this.$os.maps.main.map.setPaintProperty(this.name + "_contract_paths", 'line-gradient', npp);
				layers.contract_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_button_info;

				npp = [
					'match',
						['get', 'type'],
						'start', this.$os.theme.colors.$ui_colors_dark_button_go,
						'end', this.$os.theme.colors.$ui_colors_dark_button_cancel,
					this.$os.theme.colors.$ui_colors_dark_button_info
				];
				layers.location.paint['circle-color'] = npp;
				layers.location.paint['circle-stroke-color'] = this.$os.theme.colors.$ui_colors_dark_shade_1;
				this.$os.maps.main.map.setPaintProperty(this.name + "_location", 'circle-color', npp);

				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-color', this.$os.theme.colors.$ui_colors_dark_shade_5);
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-halo-color', this.$os.theme.colors.$ui_colors_dark_shade_1);

				layers.location_poly.paint['fill-color'] = this.$os.theme.colors.$ui_colors_dark_button_info;

				layers.range.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_shade_5;
				layers.range_out.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_shade_5;
			}
		},


		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'app': {

					const exceptions = [
						'p42_sleep',
						'p42_locked',
						'p42_home'
					];

					if(exceptions.includes(wsmsg.payload.in.uid) && !exceptions.includes(wsmsg.payload.out ? wsmsg.payload.out.uid : '')) {
						this.blanket_search();
					}

					break;
				}
				case 'to': {
					this.app = wsmsg.payload;
					this.blanket_update();
					break;
				}
			}
		},
		listener_map(wsmsg :any) {
			switch(wsmsg.name) {
				case 'loaded': {
					window.requestAnimationFrame(() => {
						this.init();
						this.theme_update();
						this.map_layers_reorder();
						this.blanket_search();
					});
					break;
				}
				case 'layer': {
					window.requestAnimationFrame(() => {
						this.theme_update();
					});
					break;
				}
				case 'contract_frame': {
					this.contract_frame(this.$os.system.map_covered);
					break;
				}
			}
		},
		listener_map_select(wsmsg :any) {
			switch(wsmsg.name) {
				case 'contracts': {
					this.contract_select_apply(wsmsg.payload ? wsmsg.payload.id : null);
					break;
				}
			}
		},
		listener_map_state(wsmsg :any) {
			switch(wsmsg.name) {
				case 'contracts': {
					if(wsmsg.payload) {
						this.state = wsmsg.payload;
					}
					this.blanket_update();
					break;
				}
			}
		},
		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.state.contracts);
					this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.blanket);
					this.blanket_update();
					break;
				}
				case 'expire': {
					this.contract_expire(wsmsg.payload);
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'themechange': {
					this.theme_update();
					break;
				}
			}
		},
		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.blanket_search();
					break;
				}
				case 'disconnect': {
					//this.blanket_update();
					this.blanket_reset();
					break;
				}
			}
		}
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('map_select', this.listener_map_select);
		this.$os.eventsBus.Bus.on('map_state', this.listener_map_state);

		this.app = this.$os.routing.activeApp;
	},


});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.map_start_marker {
	pointer-events: none;
	position: absolute;
	width: 20px;
	height: 20px;
	background-size: cover;
	background-repeat: no-repeat;
	border-radius: 50%;

	.theme--bright & {
		background-color: $ui_colors_bright_shade_1;
		@include shadowed_shallow($ui_colors_bright_shade_5);
		&.type {
			&-clearsky {
				background-image: url(../../../sys/assets/icons/companies/logo_clearsky.svg);
			}
			&-coyote {
				background-image: url(../../../sys/assets/icons/companies/logo_coyote.svg);
				background-color: #111;
			}
			&-skyparktravel {
				background-image: url(../../../sys/assets/icons/companies/logo_skyparktravel.svg);
			}
			&-oceanicair {
				background-image: url(../../../sys/assets/icons/companies/logo_oceanicair.svg);
			}
			&-lastflight {
				background-image: url(../../../sys/assets/icons/companies/logo_lastflight.svg);
			}
		}
	}
	.theme--sat &,
	.theme--dark & {
		background-color: $ui_colors_dark_shade_1;
		@include shadowed_shallow($ui_colors_dark_shade_0);
		&.type {
			&-clearsky {
				background-image: url(../../../sys/assets/icons/companies/logo_clearsky.svg);
			}
			&-coyote {
				background-image: url(../../../sys/assets/icons/companies/logo_coyote.svg);
				background-color: #111;
			}
			&-skyparktravel {
				background-image: url(../../../sys/assets/icons/companies/logo_skyparktravel.svg);
			}
			&-oceanicair {
				background-image: url(../../../sys/assets/icons/companies/logo_oceanicair.svg);
				background-color: #FFF;
			}
			&-lastflight {
				background-image: url(../../../sys/assets/icons/companies/logo_lastflight.svg);
				background-color: #FFF;
			}
		}
	}


}
</style>