<template>
	<div>

		<MglGeojsonLayer
			:sourceId="name + '_plan_paths'"
			:layerId="name + '_plan_paths_shadow'"
			:source="sources.plan_paths"
			:layer="layers.plan_paths_shadow"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_plan_paths'"
			:layerId="name + '_plan_paths'"
			:source="sources.plan_paths"
			:layer="layers.plan_paths"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_contract_paths'"
			:layerId="name + '_contract_paths'"
			:source="sources.contract_paths"
			:layer="layers.contract_paths"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_poi'"
			:layerId="name + '_poi'"
			:source="sources.poi"
			:layer="layers.poi"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_poi'"
			:layerId="name + '_poi_labels'"
			:source="sources.poi"
			:layer="layers.poi_labels"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_situation_range'"
			:layerId="name + '_situation_range'"
			:source="sources.situation_range"
			:layer="layers.situation_range"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_range'"
			:layerId="name + '_range'"
			:source="sources.range"
			:layer="layers.range"
		/>

		<MglGeojsonLayer
			:sourceId="name + '_range_out'"
			:layerId="name + '_range_out'"
			:source="sources.range_out"
			:layer="layers.range_out"
		/>

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
import Flightplan, { Waypoint } from '@/sys/classes/flight_plans/plan';
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
			anim: null,
			active: null as Flightplan,
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
				plan_paths: {
					type: 'geojson',
					lineMetrics: true,
					data: {
						type: 'FeatureCollection',
						features: []
					}
				},
				contract_paths: {
					type: 'geojson',
					lineMetrics: true,
					data: {
						type: 'FeatureCollection',
						features: []
					}
				},
				poi: {
					type: 'geojson',
					lineMetrics: true,
					data: {
						type: 'FeatureCollection',
						features: []
					}
				},
				situation_range: {
					type: 'geojson',
					lineMetrics: true,
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
				plan_paths: {
					id: this.name + '_plan_paths',
					type: 'line',
					source: this.name + '_plan_paths',
					layout: {
						"line-join": "round",
						"line-cap": "round"
					},
					paint: {
						'line-color': 'rgba(255,255,255,0.4)',
						'line-width': [
							"case",
							["boolean", ["feature-state", "selected"], false], 4,
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
				plan_paths_shadow: {
					id: this.name + '_plan_paths_shadow',
					type: 'line',
					source: this.name + '_plan_paths',
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
							["boolean", ["feature-state", "selected"], false], 0.2,
							["boolean", ["feature-state", "ghost"], false], 0.1,
							["boolean", ["feature-state", "hover"], false], 0.5,
							0.1
						]
					}
				},
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
				situation_range: {
					type: "fill",
					source: "situation_range",
					paint: {
						'fill-color': '#FFFFFF',
						'fill-opacity': 0.2
					}
				},
				poi: {
					id: "poi",
					type: 'circle',
					source: 'poi',
					paint: {
						'circle-color': '#FFFFFF',
						'circle-pitch-alignment': 'map',
						'circle-stroke-color': '#FFFFFF',
						'circle-stroke-opacity': 0.2,
						'circle-stroke-width': [
							"interpolate",
							["linear"],
							["zoom"],
							0, 1,
							8, 3,
							15, 50
						],
						'circle-radius': [
							"interpolate",
							["linear"],
							["zoom"],
							0, 1,
							8, 2,
							15, 8
						]

					}
				},
				poi_labels: {
					id: "poi_labels",
					type: 'symbol',
					source: 'poi',
					layout: {
						"text-field": ['get', 'title'],
						"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
						'text-variable-anchor': ['top'], // 'top', 'bottom', 'left', 'right'
						'text-justify': 'center',
						'text-radial-offset': [
							"interpolate",
							["linear"],
							["zoom"],
							0, 0,
							15, 1
						],
						"text-size": 12,
					},
					paint: {
						'text-color': "#FFF",
						'text-halo-color': '#FFFFFF',
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
			state: new MapFeaturesConfig({
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
			}),
		}
	},
	activated() {
		this.init();
	},
	deactivated() {

	},
	methods: {

		contracts_update() {

			// Clear ghosting
			this.sources.contract_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_contract_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing ones
			this.sources.contract_paths.data.features = [];
			this.markers = [];

			if(this.app.show.plans && !this.state.selected.plan) {
				const contracts = this.state.contracts;
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
					this.markers.push({
						location: contract.situations[0].location,
						id: contract.id,
						guid: '',
						type: contract.template.company[0]
					});

					// Push new contract feature to map
					this.sources.contract_paths.data.features.push(feature);

					// If currently selected, reapply selection
					if(this.state.selected.contract)
						this.contract_select_apply(this.state.selected.contract.id);

				});

			}

			this.contract_update_resume_circle();
		},

		contract_select(id :number) {
			let contract = this.state.contracts.find(x => x.id == id);
			this.$os.eventsBus.Bus.emit('map_selected', { name: 'plans_contracts', payload: contract } );
		},

		contract_select_apply(id :number) {

			this.state.selected.plan = null;

			// Clear ghosting
			this.sources.contract_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_contract_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing selection
			if(this.selected.source) {
				this.$os.maps.main.map.setFeatureState({ source: this.selected.source, id: this.selected.id }, { selected: false });
				this.selected.source = null;
				this.selected.id = null;
			}

			// If we have an ID, select it on the map
			if(id != null) {

				let contract = this.state.contracts.find(x => x.id == id);

				if(!contract) {
					return;
				}

				if(contract.expired) {
					return;
				}

				this.state.selected.contract = contract;
				this.state.plans = contract.flight_plans;

				// Set Ghost
				this.sources.contract_paths.data.features.forEach(feature => {
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

				});

				// Set Selected
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_contract_paths', id: id }, { selected: true });
				this.selected.source = this.name + '_contract_paths';
				this.selected.id = id;

			} else {
				this.state.selected.contract = null;
				this.state.plans = [];
			}

		},

		contract_update_resume_circle() {

			this.sources.range.data.features = [];
			this.sources.range_out.data.features = [];

			if(this.app.show.plans) {
				const contract = this.state.selected.contract;
				if(contract) {
					// Show resume circles
					if(contract.last_location_geo && contract.state == 'Active' && !contract.is_monitored) {
						this.sources.range.data.features.push(turf.circle(contract.last_location_geo, 14.816, { steps: 90, units: 'kilometers' }));
						for (let i = 6; i < 20; i++) {
							const circle = turf.circle(contract.last_location_geo, (1 + Math.pow((i * 0.5), 3)), {
								steps: 90,
								units: 'kilometers',
								properties: {
									opacity: Eljs.round(Math.pow(0.8, i), 2) * 1
								}
							});
							this.sources.range_out.data.features.push(circle);
						}
					}
				}
			}
		},


		plans_update() {

			// Clear ghosting
			this.sources.plan_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_plan_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing ones
			this.sources.plan_paths.data.features = [];

			// Renderer
			const renderer = (plan :Flightplan) => {
				// Initial feature for the current contract
				const feature = {
					type: 'Feature',
					id: plan.id,
					properties: {
						color: "#FFFFFF",
					},
					geometry: {
						type: 'MultiLineString',
						coordinates: [[]] as any[]
					},
				}

				// Loop all situations
				let previous_node = null as Array<number>;
				plan.waypoints.forEach((waypoint: Waypoint, index: number) => {
					if(index > 0){
						const greatCircle = turf.greatCircle(turf.point(previous_node), turf.point(waypoint.location));
						if(greatCircle.geometry.type == 'LineString'){
							feature.geometry.coordinates.push(greatCircle.geometry.coordinates);
						} else {
							greatCircle.geometry.coordinates.forEach((pos: any) => {
								feature.geometry.coordinates.push(pos);
							});
						}
					}
					previous_node = waypoint.location;
				});

				// Push new contract feature to map
				this.sources.plan_paths.data.features.push(feature);

				// If currently selected, reapply selection
				if(this.active)
					this.plan_select(this.active.id);
			}

			if(this.app.show.plans) {
				if(!this.state.selected.plan) {
					this.state.plans.forEach(plan => {
						renderer(plan);
					});
				} else {
					renderer(this.state.selected.plan);
				}
			}
		},

		plan_select(id :number) {
			let plan = this.state.plans.find(x => x.id == id);
			this.$os.eventsBus.Bus.emit('map_selected', { name: 'plans', payload: plan } );
		},

		plan_select_apply(id :number) {

			// Clear ghosting
			this.sources.plan_paths.data.features.forEach(feature => {
				this.$os.maps.main.map.setFeatureState({ source: this.name + '_plan_paths', id: feature.id }, { ghost: false });
			})

			// Clear existing selection
			if(this.selected.source) {
				this.$os.maps.main.map.setFeatureState({ source: this.selected.source, id: this.selected.id }, { selected: false });
				this.selected.source = null;
				this.selected.id = null;
			}

			// If we have an ID, select it on the map
			if(id != null) {

				const plan = this.state.plans.find(x => x.id == id);
				this.state.selected.plan = plan;

				if(!plan) {
					return;
				}

				// Set Ghost
				this.sources.plan_paths.data.features.forEach(feature => {
					if(feature.id != id)
						this.$os.maps.main.map.setFeatureState({ source: this.name + '_plan_paths', id: feature.id }, { ghost: true });
				})

				// Set Selected
				this.selected.source = this.name + '_plan_paths';
				this.selected.id = id;

				// Frame view on contract
				window.requestAnimationFrame(() => {
					this.plan_frame(this.$os.system.map_covered);
				});

			}

		},

		plan_leg_select() {
			if(this.state.selected.plan) {
				//this.$os.maps.main.map.setFeatureState({ source: this.name + '_plan_paths', id: id }, { selected: true });
			}
		},

		plan_frame(instant :boolean) {
			if(this.active) {

				const nodes = [] as any[];
				this.active.waypoints.forEach((waypoint :Waypoint) => {
					nodes.push(waypoint.location);
				});

				this.$os.eventsBus.Bus.emit('map', {
					name: 'goto',
					type: 'area',
					payload: {
						nodes: nodes,
					}
				});
			}
		},


		init() {
			['mouseup', 'touchend'].forEach((ev_type) => { this.$os.maps.main.map.on(ev_type, this.map_feature_click); }); // Check/Touch events
			['mousemove'].forEach((ev_type) => { this.$os.maps.main.map.on(ev_type, this.map_feature_move); }); // Move event
		},

		map_feature_click(e: any) {

		},

		map_feature_move(e: any) {
			// Get features under the cursor
			let features = this.$os.maps.main.map.queryRenderedFeatures(e.point);
			const parents = Eljs.getDOMParents(e.originalEvent.target).filter(x => x.nodeName == "DIV");
			const marker = parents.find(x => (x as HTMLElement).classList.contains('map_marker'));

			// Make sure we're not hovering a marker
			if(!marker){
				// Go through all of them
				features.forEach(feature => {
					switch(feature.layer.id) {
						case this.name + '_plan_paths_shadow': {

							// Check if hovered or hover
							const is_hovered = this.hovered.find(x => x.layer == feature.layer.id);
							if(!is_hovered) {
								this.hovered.push({ source: feature.source, layer: feature.layer.id, id: feature.id });
								this.$os.maps.main.map.setFeatureState({ source: feature.source, id: feature.id }, { hover: true });
							}
							break;
						}
					}
				});
				// Unhover what was hovered
				this.hovered.forEach((hovered) => {
					const still_hovered = features.find(x => x.source == hovered.source && x.layer.id == hovered.layer && x.id == hovered.id);
					if(!still_hovered) {
						this.hovered.splice(this.hovered.indexOf(hovered), 1);
						this.$os.maps.main.map.setFeatureState({ source: hovered.source, id: hovered.id }, { hover: false });
					}
				});
			} else {
				// Unhover everything
				this.hovered.forEach((hovered) => {
					this.hovered.splice(this.hovered.indexOf(hovered), 1);
					this.$os.maps.main.map.setFeatureState({ source: hovered.source, id: hovered.id }, { hover: false });
				});
			}

		},

		map_layers_reorder() {
			const order = {}
			order[this.name + '_plan_paths_shadow'] = 'split_1_5';
			order[this.name + '_plan_paths'] = 'split_1_5';
			order[this.name + '_contract_paths'] = 'split_1_1';

			order[this.name + '_range'] = 'split_0_1';
			order[this.name + '_range_out'] = 'split_0_1';

			Object.keys(order).forEach(element => {
				const entry = order[element];
				this.$os.maps.main.map.moveLayer(element, entry);
			});
			//console.log(this.$os.maps.main.map.getStyle().layers);
		},


		theme_update() {
			const layers = this.layers;

			if((this.$os.userConfig.get(['ui', 'theme']) == 'theme--bright' && !this.$os.maps.display_layer.sat) || this.$os.maps.display_layer.sectional.us.enabled) {

				const rgb = Eljs.HEXToRBG(this.$os.theme.colors.$ui_colors_bright_shade_3);
				let npp = [
					'interpolate',
					['linear'],
					['line-progress'],
					0, this.$os.theme.colors.$ui_colors_bright_button_go,
					0.1, 'rgba(' + rgb.r + ',' + rgb.g + ',' + rgb.b + ',0.8)',
					0.9, 'rgba(' + rgb.r + ',' + rgb.g + ',' + rgb.b + ',0.8)',
					1, this.$os.theme.colors.$ui_colors_bright_button_cancel,
				]
				layers.contract_paths.paint['line-gradient'] = npp;
				this.$os.maps.main.map.setPaintProperty(this.name + "_contract_paths", 'line-gradient', npp);
				layers.contract_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_info;

				layers.plan_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_magenta;
				layers.plan_paths_shadow.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_magenta;

				layers.range.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_cancel;
				layers.range_out.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_cancel;

			} else {

				const rgb = Eljs.HEXToRBG(this.$os.theme.colors.$ui_colors_dark_shade_3);
				let npp = [
					'interpolate',
					['linear'],
					['line-progress'],
					0, this.$os.theme.colors.$ui_colors_dark_button_go,
					0.1, 'rgba(' + rgb.r + ',' + rgb.g + ',' + rgb.b + ',0.2)',
					0.9, 'rgba(' + rgb.r + ',' + rgb.g + ',' + rgb.b + ',0.2)',
					1, this.$os.theme.colors.$ui_colors_dark_button_cancel,
				]
				layers.contract_paths.paint['line-gradient'] = npp;
				this.$os.maps.main.map.setPaintProperty(this.name + "_contract_paths", 'line-gradient', npp);
				layers.contract_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_button_info;

				layers.plan_paths.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_magenta;
				layers.plan_paths_shadow.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_magenta;

				layers.range.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_shade_5;
				layers.range_out.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_shade_5;
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
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					this.app = wsmsg.payload;
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
					});
					break;
				}
				case 'layer': {
					window.requestAnimationFrame(() => {
						this.theme_update();
					});
					break;
				}
			}
		},
		listener_map_select(wsmsg :any) {
			switch(wsmsg.name) {
				case 'plans_contracts': {
					this.contract_select_apply(wsmsg.payload ? wsmsg.payload.id : null);
					this.contracts_update();
					this.plans_update();
					break;
				}
				case 'plans': {
					this.plan_select_apply(wsmsg.payload ? wsmsg.payload.id : null);
					this.contracts_update();
					this.plans_update();
					break;
				}
			}
		},
		listener_map_state(wsmsg :any) {
			switch(wsmsg.name) {
				case 'plans': {
					this.state = wsmsg.payload;
					this.contracts_update();
					this.plans_update();
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
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('map_select', this.listener_map_select);
		this.$os.eventsBus.Bus.on('map_state', this.listener_map_state);
		this.app = this.$os.routing.activeApp;
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

</style>