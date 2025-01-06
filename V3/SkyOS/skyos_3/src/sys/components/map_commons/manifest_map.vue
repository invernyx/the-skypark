<template>
	<div>

		<MglGeojsonLayer
			:sourceId="name + '_manifest_paths'"
			:layerId="name + '_manifest_paths_shadow'"
			:source="ui.map.sources.manifest_paths"
			:layer="ui.map.layers.manifest_paths_shadow"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_manifest_paths'"
			:layerId="name + '_manifest_paths'"
			:source="ui.map.sources.manifest_paths"
			:layer="ui.map.layers.manifest_paths"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_manifest_traveled'"
			:layerId="name + '_manifest_traveled'"
			:source="ui.map.sources.manifest_traveled"
			:layer="ui.map.layers.manifest_traveled"
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

		<div v-for="(marker, index) in ui.map.markers" v-bind:key="index">
			<MglMarker :coordinates="marker.location">
				<div class="map_payload_marker" :class="['map_marker', 'type-' + marker.type, { 'selected': state.selected.contract ? (state.selected.contract.id == marker.id) : false, 'ghost': state.selected.contract ? (state.selected.contract.id != marker.id) : false }]" slot="marker" @click="map_marker_click($event, state.contracts.find(x => x.id == marker.id))">
					<div></div>
				</div>
			</MglMarker>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import * as turf from '@turf/turf';
import { AppInfo } from "@/sys/foundation/app_model"
import { MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import SearchStates from "@/sys/enums/search_states";
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import MapboxExt from '@/sys/libraries/mapboxExt';
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
			sim_live: this.$os.simulator.live,
			sim_location: [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat],
			ui: {
				map: {
					anim: null,
					selected: {
						source: null as string,
						id: null as number,
					},
					hovered: [] as {
						id: number,
						source: string,
						layer: string,
					}[],
					markers: [] as {
						guid: string,
						id: number,
						location: number[],
						type: string,
					}[],
					sources: {
						manifest_traveled: {
							type: 'geojson',
							lineMetrics: true,
							data: {
								type: 'FeatureCollection',
								features: []
							}
						},
						manifest_paths: {
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
					},
					layers: {
						manifest_traveled: {
							id: this.name + '_manifest_traveled',
							type: 'line',
							source: this.name + '_manifest_traveled',
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
						manifest_paths_shadow: {
							id: this.name + '_manifest_paths_shadow',
							type: 'line',
							source: this.name + '_manifest_paths',
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
						manifest_paths: {
							id: this.name + '_manifest_paths',
							type: 'line',
							source: this.name + '_manifest_paths',
							layout: {
								"line-join": "round",
								"line-cap": "round"
							},
							paint: {
								'line-color': 'rgba(0,0,0,0)',
      							'line-dasharray': [2, 2],
								'line-width': [
									"case",
									["boolean", ["feature-state", "selected"], false], 2,
									["boolean", ["feature-state", "hover"], false], 2,
									2
								],
								'line-opacity': [
									"case",
									["boolean", ["feature-state", "hover"], false], 0.9,
									["boolean", ["feature-state", "selected"], false], 1,
									["boolean", ["feature-state", "ghost"], false], 0.3,
									0.5
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
									0, 2,
									14, 2
								],
								"text-size": 12,
							},
							paint: {
								'text-color': "#FFF",
								'text-halo-color': "#FFF",
								'text-halo-width': 2,
							}
						},
					},
				}
			},
			state: new MapFeaturesConfig({
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
			}),
		}
	},
	methods: {

		init() {
			['mouseup', 'touchend'].forEach((ev_type) => { this.$os.maps.main.map.on(ev_type, this.map_feature_click); }); // Check/Touch events
			['mousemove'].forEach((ev_type) => { this.$os.maps.main.map.on(ev_type, this.map_feature_move); }); // Move event

			this.map_layers_reorder();
		},


		manifests_update() {

			// Clear ghosting
			this.ui.map.sources.manifest_traveled.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_manifest_traveled', id: feature.id }, { ghost: false });
			})
			this.ui.map.sources.manifest_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_manifest_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing ones
			this.ui.map.sources.manifest_traveled.data.features = [];
			this.ui.map.sources.manifest_paths.data.features = [];

			this.ui.map.markers = [];

			if(this.app.show.manifests) {
				// Go through all contracts
				this.state.contracts.forEach(contract => {

					if(contract.expired) {
						return;
					}

					// Initial feature for the current contract
					const featureTraveled = {
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

					const featureToDest = {
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


					// Push new contract feature to map
					this.ui.map.sources.manifest_traveled.data.features.push(featureTraveled);
					this.ui.map.sources.manifest_paths.data.features.push(featureToDest);

					// If currently selected, reapply selection
					if(this.state.selected.contract)
						this.manifest_select_apply(this.state.selected.contract.id);

				});

				this.manifests_refresh();

				if(this.$os.maps.main.map.isStyleLoaded()) {
					this.$os.maps.main.map.getSource(this.name + '_manifest_paths').setData(this.ui.map.sources.manifest_paths.data);
					this.$os.maps.main.map.getSource(this.name + '_manifest_traveled').setData(this.ui.map.sources.manifest_traveled.data);
				}

			}
		},

		manifests_refresh() {

			if(this.app.show.manifests) {

				// Go through all contracts
				const greatCircle = (target, start, end) => {
					var greatCircle = turf.greatCircle(start, end);
					if(greatCircle.geometry.type == 'LineString'){
						target[0] = target[0].concat(greatCircle.geometry.coordinates);
					} else {
						target = target.concat(greatCircle.geometry.coordinates);
					}
				}

				this.state.contracts.forEach(contract => {

					let featureTraveled = this.ui.map.sources.manifest_traveled.data.features.find(x => x.id == contract.id);
					let featureToDest = this.ui.map.sources.manifest_paths.data.features.find(x => x.id == contract.id);

					featureTraveled.geometry.coordinates = [[]] as any[];
					featureToDest.geometry.coordinates = [[]] as any[];

					// Loop all manifests
					contract.manifests.cargo.forEach((cargo :any, i1) => {
						const cargo_state = contract.manifests_state.cargo[i1];
						cargo.manifests.forEach((manifest :any, i2) => {
							const manifest_state = cargo_state.manifests[i2];
							if(manifest.groups.length) {
								manifest.groups.forEach((group, i3) => {
									const group_state = manifest_state.groups[i3];

									//#region Traveled
									let group_location = [group_state.location[0], group_state.location[1]];
									if(group_state.loaded_on) {
										if(this.$os.fleetService.aircraft_current.id == group_state.loaded_on.id) {
											group_location = this.sim_location;
										}
									}
									let group_coords = [[]];
									greatCircle(group_coords, turf.point([manifest.origin.location[0], manifest.origin.location[1]]), turf.point(group_location));
									featureTraveled.geometry.coordinates = featureTraveled.geometry.coordinates.concat(group_coords);
									//#endregion

									//#region To Dest
									group_location = [group_state.location[0], group_state.location[1]];
									if(group_state.loaded_on) {
										if(this.$os.fleetService.aircraft_current.id == group_state.loaded_on.id) {
											group_location = this.sim_location;
										}
									}
									group_coords = [[]];
									greatCircle(group_coords, turf.point(group_location), turf.point(manifest.destinations[group.destination_index].location));
									featureToDest.geometry.coordinates = featureToDest.geometry.coordinates.concat(group_coords);
									//#endregion

									//#region Marker
									const existing_marker = this.ui.map.markers.find(x => x.guid == group.guid);
									if(!existing_marker) {
										this.ui.map.markers.push({
											location: group_location,
											id: contract.id,
											guid: group.guid,
											type: 'cargo'
										});
									} else {
										existing_marker.location = group_location;
									}
									//#endregion

								});
							} else {
								manifest.destinations.forEach((destination, index) => {
									var start = turf.point(manifest.origin.location);
									var end = turf.point(destination.location);
									greatCircle(featureToDest.geometry.coordinates, start, end);

									this.ui.map.markers.push({
										location: manifest.origin.location,
										id: contract.id,
										guid: '',
										type: 'cargo'
									});
								});
							}

						});
					});
				});

				if(this.$os.maps.main.map.isStyleLoaded()) {
					this.$os.maps.main.map.getSource(this.name + '_manifest_paths').setData(this.ui.map.sources.manifest_paths.data);
					this.$os.maps.main.map.getSource(this.name + '_manifest_traveled').setData(this.ui.map.sources.manifest_traveled.data);
				}

			}
		},

		manifests_reset() {
			this.manifest_select_apply(null);
			this.state = new MapFeaturesConfig();
		},


		contract_expire(id :number) {

			const index_feature = this.ui.map.sources.manifest_traveled.data.features.findIndex(x => x.id == id);
			if(index_feature > -1) {
				this.ui.map.sources.manifest_traveled.data.features.splice(index_feature, 1);
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

		manifest_select(id :number) {
			let contract = this.state.contracts.find(x => x.id == id);
			this.$os.eventsBus.Bus.emit('map_select', { name: 'manifests', payload: contract } );
		},

		manifest_select_apply(id :number) {

			// Clear features
			this.ui.map.sources.location.data.features = [];

			// Clear ghosting
			this.ui.map.sources.manifest_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_manifest_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing selection
			if(this.ui.map.selected.source) {
				this.$os.maps.main.map.setFeatureState({ source: this.ui.map.selected.source, id: this.ui.map.selected.id }, { selected: false });
				this.ui.map.selected.source = null;
				this.ui.map.selected.id = null;
			}

			if(this.app.show.manifests) {

				// If we have an ID, select it on the map
				if(id != null) {
					const contract = this.state.contracts.find(x => x.id == id);

					if(!contract) {
						return;
					}

					if(contract.expired) {
						return;
					}

					if(contract.manifests) {

						// Set Ghost
						this.ui.map.sources.manifest_paths.data.features.forEach(feature => {
							if(feature.id != id)
								this.$os.maps.main.map.setFeatureState({ source: this.name + '_manifest_paths', id: feature.id }, { ghost: true });
						})


						// Loop all manifests
						contract.manifests.cargo.forEach((cargo :any) => {
							cargo.manifests.forEach((manifest :any) => {
								manifest.groups.forEach(group => {

									const loc_feature = {
										type: "Feature",
										properties: {
											id: id,
											type: 'end',
											display: 'unknown',
											title: 'Unknown',
										},
										geometry: null as any
									};

									const dest = manifest.destinations[group.destination_index];
									if(dest.airport) {
										loc_feature.properties.title = dest.airport.icao;
										loc_feature.properties.display = 'airport';
										loc_feature.geometry = {
											type: "Point",
											coordinates: [dest.location[0], dest.location[1]],
										}
									} else {
										loc_feature.properties.title = dest.location;
										loc_feature.properties.display = 'range';
										loc_feature.geometry = {
											type: "Polygon",
											coordinates: [
												Object.assign({}, turf.circle([dest.location[0], dest.location[1]], 0.5, { steps: 20, properties: {} })).geometry.coordinates[0]
											]
										}
									}

									this.ui.map.sources.location.data.features.push(loc_feature);
								});

							});
						});

						// Set Selected
						this.$os.maps.main.map.setFeatureState({ source: this.name + '_manifest_paths', id: id }, { selected: true });
						this.ui.map.selected.source = this.name + '_manifest_paths';
						this.ui.map.selected.id = id;

						this.state.selected.contract = contract;

						// Frame view on contract
						window.requestAnimationFrame(() => {
							this.manifest_frame(this.$os.system.map_covered);
						});
					}

				} else {
					this.state.selected.contract = null;
				}

			}

		},

		manifest_frame(instant :boolean) {
			if(this.state.selected.contract) {

				const nodes = [] as any[];
				this.state.selected.contract.manifests.cargo.forEach((cargo :any, i1) => {
					const cargo_state = this.state.selected.contract.manifests_state.cargo[i1];
					cargo.manifests.forEach((manifest :any, i2) => {
						const manifest_state = cargo_state.manifests[i2];
						if(manifest.groups.length) {
							manifest.groups.forEach((group, i3) => {
								const group_state = manifest_state.groups[i3];
								nodes.push([group_state.location[0], group_state.location[1]]);
								nodes.push(manifest.destinations[group.destination_index].location);
							});
						} else {
							manifest.destinations.forEach((destination, index) => {
								nodes.push([manifest.origin.location[0], manifest.origin.location[1]]);
								nodes.push(destination.location);
							});
						}
					});
				});

				if(nodes.length) {

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

		map_feature_click(e: any) {
			if(this.app.show.manifests) {

				// Get features under the cursor
				let features = this.$os.maps.main.map.queryRenderedFeatures(e.point);
				const parents = Eljs.getDOMParents(e.originalEvent.target).filter(x => x.nodeName == "DIV");
				const marker = parents.find(x => (x as HTMLElement).classList.contains('map_marker'));

				// Make sure we're not hovering a marker
				if(!marker){

					// Go through all of them
					let escape = false;
					features.forEach(feature => {

						if(escape) return;

						switch(feature.layer.id) {
							case this.name + '_manifest_paths_shadow': {
								const contract = this.state.contracts.find(x => x.id == feature.id);
								this.$os.eventsBus.Bus.emit('map_select', { name: 'manifests', payload: contract });
								escape = true;
								return;
							}
						}

					});
				}
			}
		},


		map_marker_click(ev :PointerEvent, contract :Contract) {
			ev.stopPropagation();
			this.$os.eventsBus.Bus.emit('map_select', { name: 'manifests', payload: contract });
		},

		map_feature_move(e: any) {
			if(this.app.show.manifests) {
				// Get features under the cursor
				let features = this.$os.maps.main.map.queryRenderedFeatures(e.point);
				const parents = Eljs.getDOMParents(e.originalEvent.target).filter(x => x.nodeName == "DIV");
				const marker = parents.find(x => (x as HTMLElement).classList.contains('map_marker'));

				// Make sure we're not hovering a marker
				if(!marker){
					// Go through all of them
					features.forEach(feature => {
						switch(feature.layer.id) {
							case this.name + '_manifest_paths_shadow': {
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

		theme_update() {
			const layers = this.ui.map.layers;

			if((this.$os.userConfig.get(['ui', 'theme']) == 'theme--bright' && !this.$os.maps.display_layer.sat) || this.$os.maps.display_layer.sectional.us.enabled) {

				// #region Traveled lines
				layers.manifest_paths_shadow.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_shade_1;

				const line_color =  Eljs.HEXToRBG(this.$os.theme.colors.$ui_colors_bright_button_go);
				const line_color_str =  line_color.r + ',' + line_color.g + ',' + line_color.b;

				let gradient_colors = [
					'interpolate',
					['linear'],
					['line-progress'],
					0, 'rgba(' + line_color_str + ', 1)',
					0.1, 'rgba(' + line_color_str + ', 0.2)',
					0.9, 'rgba(' + line_color_str + ', 0.2)',
					1, 'rgba(' + line_color_str + ', 1)',
				]
				layers.manifest_traveled.paint['line-gradient'] = gradient_colors;
				this.$os.maps.main.map.setPaintProperty(this.name + "_manifest_traveled", 'line-gradient', gradient_colors);
				layers.manifest_traveled.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_go;
				//#endregion

				// #region To Destination
				layers.manifest_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_shade_4;
				//#endregion

				// #region Current region
				gradient_colors = [
					'match',
						['get', 'type'],
						'start', this.$os.theme.colors.$ui_colors_bright_button_go,
						'end', this.$os.theme.colors.$ui_colors_bright_button_cancel,
					this.$os.theme.colors.$ui_colors_bright_shade_0
				];
				layers.location.paint['circle-color'] = gradient_colors;
				layers.location.paint['circle-stroke-color'] = this.$os.theme.colors.$ui_colors_bright_shade_1;
				this.$os.maps.main.map.setPaintProperty(this.name + "_location", 'circle-color', gradient_colors);

				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-color', this.$os.theme.colors.$ui_colors_bright_shade_5);
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-halo-color', this.$os.theme.colors.$ui_colors_bright_shade_1);

				layers.location_poly.paint['fill-color'] = this.$os.theme.colors.$ui_colors_bright_button_info;
				//#endregion

			} else {

				// #region Traveled lines
				layers.manifest_paths_shadow.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_shade_1;

				const line_color =  Eljs.HEXToRBG(this.$os.theme.colors.$ui_colors_dark_button_go);
				const line_color_str =  line_color.r + ',' + line_color.g + ',' + line_color.b;

				let gradient_colors = [
					'interpolate',
					['linear'],
					['line-progress'],
					0, 'rgba(' + line_color_str + ', 1)',
					0.1, 'rgba(' + line_color_str + ', 0.2)',
					0.9, 'rgba(' + line_color_str + ', 0.2)',
					1, 'rgba(' + line_color_str + ', 1)',
				]
				layers.manifest_traveled.paint['line-gradient'] = gradient_colors;
				this.$os.maps.main.map.setPaintProperty(this.name + "_manifest_traveled", 'line-gradient', gradient_colors);
				layers.manifest_traveled.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_button_go;
				// #endregion

				// #region To Destination
				layers.manifest_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_shade_4;
				//#endregion

				// #region Current region
				gradient_colors = [
					'match',
						['get', 'type'],
						'start', this.$os.theme.colors.$ui_colors_dark_button_go,
						'end', this.$os.theme.colors.$ui_colors_dark_button_cancel,
					this.$os.theme.colors.$ui_colors_dark_button_warn
				];
				layers.location.paint['circle-color'] = gradient_colors;
				layers.location.paint['circle-stroke-color'] = this.$os.theme.colors.$ui_colors_dark_shade_1;
				this.$os.maps.main.map.setPaintProperty(this.name + "_location", 'circle-color', gradient_colors);

				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-color', this.$os.theme.colors.$ui_colors_dark_shade_5);
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-halo-color', this.$os.theme.colors.$ui_colors_dark_shade_1);

				layers.location_poly.paint['fill-color'] = this.$os.theme.colors.$ui_colors_dark_button_info;
				//#endregion

			}
		},

		map_layers_reorder() {
			const order = {}
			order[this.name + '_manifest_paths_shadow'] = 'split_1_3';
			order[this.name + '_manifest_paths'] = 'split_1_3';
			order[this.name + '_manifest_traveled'] = 'split_1_3';
			order[this.name + '_location_poly'] = 'split_1_3';
			order[this.name + '_location_label'] = 'split_1_3';

			Object.keys(order).forEach(element => {
				const entry = order[element];
				this.$os.maps.main.map.moveLayer(element, entry);
			});
			//console.log(this.$os.maps.main.map.getStyle().layers);
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					this.app = wsmsg.payload;
					this.manifests_update();
					break;
				}
			}
		},
		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.sim_live = wsmsg.payload;
					break;
				}
				case 'meta': {
					this.sim_location = [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat];
					this.manifests_refresh()
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
					});
					break;
				}
				case 'layer': {
					window.requestAnimationFrame(() => {
						this.theme_update();
					});
					break;
				}
				case 'manifest_frame': {
					this.manifest_frame(this.$os.system.map_covered);
					break;
				}
			}
		},
		listener_map_select(wsmsg :any) {
			switch(wsmsg.name) {
				case 'manifests': {
					this.manifest_select_apply(wsmsg.payload ? wsmsg.payload.id : null);
					break;
				}
			}
		},
		listener_map_state(wsmsg :any) {
			switch(wsmsg.name) {
				case 'manifests': {
					if(wsmsg.payload) {
						this.state = wsmsg.payload;
					} else {
						this.manifests_reset();
					}
					this.manifests_update();
					break;
				}
			}
		},
		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.contract_service.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.contract, this.state.contracts);
					this.manifests_update();
					break;
				}
				case 'contract_expire': {
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
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
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

.map_payload_marker {
	position: absolute;

	& > div {
		background-size: 70%;
		background-position: center;
		background-repeat: no-repeat;
		border-radius: 50%;
		width: 30px;
		height: 30px;
		transition: transform 0.1s ease-out, opacity 0.1s ease-out;
	}

	&:hover {
		& > div {
			transform: scale(1.2);
		}
	}

	&.selected {
		z-index: 2;
		& > div {
			transform: scale(1.2);
		}
	}

	&.ghost {
		& > div {
			opacity: 0.3;
		}
	}

	.theme--bright & {
		& > div {
			background-color: $ui_colors_bright_shade_1;
			@include shadowed_shallow($ui_colors_bright_shade_5);
		}
		&.selected {
			& > div {
				background-color: $ui_colors_bright_button_info;
			}
		}
		&.type {
			&-cargo {
				& > div {
					background-image: url(../../../sys/assets/cargo/default.png);
				}
			}
		}
	}
	.theme--sat &,
	.theme--dark & {
		& > div {
			background-color: $ui_colors_dark_shade_1;
			@include shadowed_shallow($ui_colors_dark_shade_0);
		}
		&.selected {
			& > div {
				background-color: $ui_colors_dark_button_info;
			}
		}
	}
	&.selected,
	.theme--sat &,
	.theme--dark & {

		&.type {
			&-cargo {
				& > div {
					background-image: url(../../../sys/assets/cargo/default.png);
				}
			}
		}
	}
}
</style>