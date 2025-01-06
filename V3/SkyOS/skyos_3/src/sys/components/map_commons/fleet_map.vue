<template>
	<div>

		<MglGeojsonLayer
			:sourceId="name + '_location_label'"
			:layerId="name + '_location_label'"
			:source="ui.map.sources.location_label"
			:layer="ui.map.layers.location_label"
		/>

		<MglGeojsonLayer
			:layerId="name + '_aircraft_poly_glow'"
			:sourceId="name + '_aircraft_poly_glow'"
			:source="ui.map.sources.aircraft_poly_glow"
			:layer="ui.map.layers.aircraft_poly_glow" />

		<MglGeojsonLayer
			:layerId="name + '_aircraft_poly'"
			:sourceId="name + '_aircraft_poly'"
			:source="ui.map.sources.aircraft_poly"
			:layer="ui.map.layers.aircraft_poly" />

		<div v-for="(acf, index) in ui.map.markers" v-bind:key="index">
			<MglMarker :coordinates="acf.coordinates">
				<div class="map_aircraft_marker" :class="[
						'map_marker',
						'type-' + acf.type,
						{
							'duty': acf.duty,
							'selected': aircraft ? (aircraft.id == acf.id) : false,
							'ghost': aircraft ? (aircraft.id != acf.id) : false
						}]" slot="marker" @click="map_marker_click($event, acf.id)">
					<div></div>
				</div>
			</MglMarker>
		</div>

	</div>
</template>

<script lang="ts">

// 'current': aircraft_loaded ? (acf.id == aircraft_loaded.id) : false

import Vue from 'vue';
import * as turf from '@turf/turf';
import { AppInfo } from "@/sys/foundation/app_model"
import { MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import MapboxExt from '@/sys/libraries/mapboxExt';
import Eljs from '@/sys/libraries/elem';
import Aircraft from '@/sys/classes/aircraft';
import ActionPreviewData from '@/sys/classes/action_preview';

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
			has_transponder: this.$os.api.connected,
			ui: {
				map: {
					sources: {
						location_label: {
							type: 'geojson',
							data: {
								type: 'FeatureCollection',
								features: []
							}
						},
						aircraft_poly_glow: {
							type: 'geojson',
							data: {
								type: 'FeatureCollection',
								features: []
							}
						},
						aircraft_poly: {
							type: 'geojson',
							data: {
								type: 'FeatureCollection',
								features: []
							}
						}
					},
					layers: {
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
						aircraft_poly_glow: {
							id: this.name + '_aircraft_poly_glow',
							type: 'circle',
							source: this.name + '_aircraft_poly_glow',
							paint: {
								'circle-pitch-alignment': "map",
								'circle-blur': 2,
								'circle-opacity': 0.5,
								'circle-radius': {
									'base': 1.75,
									'stops': [
										[1, 50],
										[14, 100],
										[22, 180]
									]
								},
								'circle-color': "#CCC"
							},
						},
						aircraft_poly: {
							id: this.name + '_aircraft_poly',
							type: 'fill-extrusion',
							source: this.name + '_aircraft_poly',
							paint: {
								'fill-extrusion-color': '#FFFFFF',
								'fill-extrusion-height': ['get', 'thick'],
								'fill-extrusion-base': ['get', 'base'],
								'fill-extrusion-opacity': 0.8,
								'fill-extrusion-translate': [0,0]
							},
							filter: ['==', '$type', 'Polygon']
						}
					},
					markers: [] as {
						id: number,
						coordinates: number[],
						type: number,
						duty: boolean,
					}[]
				}
			},
			sim_location: null as number[],
			aircraft: null as Aircraft,
			aircraft_loaded: this.$os.fleetService.aircraft_current,
			fleet: [] as Aircraft[],
		}
	},
	activated() {
		this.init();
	},
	deactivated() {

	},
	methods: {

		init() {

			['zoom'].forEach((ev) => {
				this.$os.maps.main.map.on(ev, (ev1 :any) => {
					this.map_render_aircraft();
				});
			});

			['move'].forEach((ev) => {
				this.$os.maps.main.map.on(ev, (ev1 :any) => {
					this.map_render_aircraft();
				});
			});

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

			this.fleet_search();
		},


		fleet_update() {

			// Clear existing ones
			this.ui.map.sources.location_label.data.features = [];
			this.ui.map.markers = [];

			if(this.app.show.fleet && this.$os.maps.display_layer.fleet.enabled) {
				//const processed_airports = [] as string[];
				// Go through all contracts
				this.fleet.forEach(aircraft => {

					if(aircraft.location) {

						const loc_feature = {
							id: aircraft.id,
							type: "Feature",
							geometry: null as any
						};

						loc_feature.geometry = {
							type: "Point",
							coordinates: aircraft.location,
						}

						this.ui.map.sources.location_label.data.features.push(loc_feature);

						this.ui.map.markers.push({
							id: aircraft.id,
							coordinates: aircraft.location,
							type: aircraft.size,
							duty: aircraft.is_duty
						})
					}

				});
			}
		},

		fleet_duty_update() {
			const aircraft_current = this.$os.fleetService.aircraft_current;
			if(aircraft_current) {
				if(aircraft_current.is_duty) {
					const aircraft_marker = this.ui.map.markers.find(x => x.id == aircraft_current.id);
					if(aircraft_marker) {
						aircraft_marker.duty = aircraft_current.is_duty;
						aircraft_marker.coordinates = this.sim_location;
						const existing = this.ui.map.sources.location_label.data.features.find(x => x.id == aircraft_current.id);
						existing.geometry.coordinates = aircraft_marker.coordinates;
						this.$os.maps.main.map.getSource(this.name + '_location_label').setData(this.ui.map.sources.location_label.data);
					}
				}
			}
		},

		fleet_search() {
			if(this.has_transponder) {
				this.fleet = this.$os.fleetService.dispose_list(this.fleet);

				const queryoptions = {
					fields: null
				}

				this.$os.api.send_ws(
					"fleet:get_all",
					queryoptions,
					(fleetData) => {

						if(fleetData.payload.length) {
							this.fleet = this.$os.fleetService.ingest_list(fleetData.payload);
						}

						this.fleet_update();
					}
				);
			}
		},


		aircraft_select(id :number) {

			const aircraft = this.fleet.find(x => x.id == id);
			this.$os.eventsBus.Bus.emit('map_select', { name: 'fleet', payload: aircraft } );

		},

		aircraft_select_apply(id :number){
			this.aircraft = this.fleet.find(x => x.id == id);
		},

		aircraft_frame() {
			if(this.aircraft) {
				this.$os.eventsBus.Bus.emit('map', {
					name: 'goto',
					type: 'location',
					payload: {
						airport: this.aircraft.nearest_airport,
						location: this.aircraft.location,
						radius: 10,
					}
				});
			}
		},


		map_feature_click(e: any) {
			if(this.app.show.fleet) {
				var parents = Eljs.getDOMParents(e.originalEvent.target).filter(x => (x as HTMLElement).classList ? (x as HTMLElement).classList.contains('map_aircraft_marker') : false);
				if(!parents.length && this.aircraft) {
					this.aircraft_select(null);
				}
			}
		},

		map_marker_click(ev :PointerEvent, id :number) {
			ev.stopPropagation();
			this.aircraft_select(id);
		},

		map_render_aircraft() {
			const simData = this.$os.simulator.location;
			let sourceData = this.ui.map.sources.aircraft_poly.data;
			const Existing = sourceData.features.filter(x => x.properties.id == 0);

			if(this.$os.simulator.live) {

				// Poly
				let feature = null;
				if(Existing.length) {
					feature = (Existing as any)[0];
				} else {
					feature = {
						type: 'Feature',
						id: 56154,
						properties: {
							id: 0,
							thick: 0,
							base: 0,
						},
						geometry: {
							type: 'Polygon',
							coordinates: [[]] as any
						},
					}
					sourceData.features.push(feature);
				}

				const tsize = Math.pow(2, (22 - (this.$os.maps.main.map.getZoom() * 0.9)) - 6);
				feature.properties.base = 0; //simData.location.GAlt * 0.3048;
				feature.properties.thick = feature.properties.base + (tsize * 2); //simData.location.GAlt * 0.3048 + (tsize * 2); //

				const curLon = simData.Lon;
				const curLat = simData.Lat;
				const curHdg = simData.GS > 20 ? simData.CRS : simData.Hdg;
				feature.geometry.coordinates[0] = [];

				const startNode = Eljs.MapOffsetPosition(curLon, curLat, tsize * 6, curHdg + 0);
				feature.geometry.coordinates[0].push(startNode); // Top tip
				feature.geometry.coordinates[0].push(Eljs.MapOffsetPosition(curLon, curLat, tsize * 12, curHdg + 143)); // Right tip
				feature.geometry.coordinates[0].push(Eljs.MapOffsetPosition(curLon, curLat, tsize * 6, curHdg + 180)); // Back tip
				feature.geometry.coordinates[0].push(Eljs.MapOffsetPosition(curLon, curLat, tsize * 12, curHdg - 143)); // Left tip
				feature.geometry.coordinates[0].push(startNode); // Closing

				this.$os.maps.main.map.getSource(this.name + '_aircraft_poly').setData(sourceData);


				// Glow
				sourceData = this.ui.map.sources.aircraft_poly_glow.data;
				sourceData.features = [];
				sourceData.features.push({
					type: 'Feature',
					id: 56155,
					geometry: {
						type: 'Point',
						coordinates: [simData.Lon, simData.Lat]
					},
				});

				this.$os.maps.main.map.getSource(this.name + '_aircraft_poly_glow').setData(sourceData);

			} else {
				Existing.forEach(element => {
					const Index = sourceData.features.indexOf(element);
					sourceData.features.splice(Index, 1);
				});
			}
		},

		map_layers_reorder() {
			const order = {}
			order[this.name + '_location_label_shadow'] = 'split_1_2';
			order[this.name + '_location_label'] = 'split_1_2';
			order[this.name + '_aircraft_poly'] = 'split_2_0';
			order[this.name + '_aircraft_poly_glow'] = 'split_2_0';

			Object.keys(order).forEach(element => {
				const entry = order[element];
				this.$os.maps.main.map.moveLayer(element, entry);
			});
			//console.log(this.$os.maps.main.map.getStyle().layers);
		},


		theme_update() {
			const layers = this.ui.map.layers;

			if((this.$os.userConfig.get(['ui', 'theme']) == 'theme--bright' && !this.$os.maps.display_layer.sat) || this.$os.maps.display_layer.sectional.us.enabled) {
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-color', this.$os.theme.colors.$ui_colors_bright_shade_5);
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-halo-color', this.$os.theme.colors.$ui_colors_bright_shade_1);

				layers.aircraft_poly.paint['fill-extrusion-color'] = '#3333FF';
				layers.aircraft_poly_glow.paint['circle-color'] = '#3333FF';
			} else {
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-color', this.$os.theme.colors.$ui_colors_dark_shade_5);
				this.$os.maps.main.map.setPaintProperty(this.name + "_location_label", 'text-halo-color', this.$os.theme.colors.$ui_colors_dark_shade_1);

				layers.aircraft_poly.paint['fill-extrusion-color'] = '#00F5FF';
				layers.aircraft_poly_glow.paint['circle-color'] = '#00F5FF';
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
					console.log(wsmsg.payload.path);
					if(wsmsg.payload.path[0] == 'fleet') {
						window.requestAnimationFrame(() => {
							this.fleet_update();
						});
					}
					break;
				}
				case 'aircraft_frame': {
					this.aircraft_frame();
					break;
				}
			}
		},
		listener_map_select(wsmsg :any) {
			switch(wsmsg.name) {
				case 'fleet': {
					this.aircraft_select_apply(wsmsg.payload ? wsmsg.payload.id : null);
					break;
				}
			}
		},
		listener_map_state(wsmsg :any) {
			switch(wsmsg.name) {
				case 'fleet': {
					if(wsmsg.payload) {
						this.fleet = wsmsg.payload;
					} else {
						this.aircraft_select(null);
						this.fleet = []
					}
					this.fleet_update();
					this.fleet_duty_update();
					break;
				}
			}
		},
		listener_os_fleet(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.fleetService.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.aircraft, this.fleet);
					this.fleet_update();
					break;
				}
				case 'current_aircraft': {
					this.aircraft_loaded = this.$os.fleetService.aircraft_current;
					this.fleet_search();
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					this.app = wsmsg.payload;
					this.fleet_update();
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
					this.has_transponder = true;
					this.sim_location = null;
					this.fleet_search();
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					this.sim_location = null;
					this.fleet = [];
					this.fleet_update();
					break;
				}
			}
		},
		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					if(!wsmsg.payload) {
						this.sim_location = null;
					}
					break;
				}
				case 'meta': {
					this.sim_location = [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat];
					this.fleet_duty_update();
					this.map_render_aircraft();
					break;
				}
			}
		}
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('fleet', this.listener_os_fleet);
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

.map_aircraft_marker {
	position: absolute;

	& > div {
		background-size: 80%;
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

	&.current,
	&.duty {
		display: none;
	}

	&.ghost {
		& > div {
			opacity: 0.7;
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
			&-0 {
				& > div {
					background-image: url(../../../sys/assets/icons/dark/acf_heli.svg);
				}
			}
			&-1 {
				& > div {
					background-image: url(../../../sys/assets/icons/dark/acf_ga.svg);
				}
			}
			&-2 {
				& > div {
					background-image: url(../../../sys/assets/icons/dark/acf_turbo.svg);
				}
			}
			&-3 {
				& > div {
					background-image: url(../../../sys/assets/icons/dark/acf_jet.svg);
				}
			}
			&-4 {
				& > div {
					background-image: url(../../../sys/assets/icons/dark/acf_narrow.svg);
				}
			}
			&-5 {
				& > div {
					background-image: url(../../../sys/assets/icons/dark/acf_wide.svg);
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
			&-0 {
				& > div {
					background-image: url(../../../sys/assets/icons/bright/acf_heli.svg);
				}
			}
			&-1 {
				& > div {
					background-image: url(../../../sys/assets/icons/bright/acf_ga.svg);
				}
			}
			&-2 {
				& > div {
					background-image: url(../../../sys/assets/icons/bright/acf_turbo.svg);
				}
			}
			&-3 {
				& > div {
					background-image: url(../../../sys/assets/icons/bright/acf_jet.svg);
				}
			}
			&-4 {
				& > div {
					background-image: url(../../../sys/assets/icons/bright/acf_narrow.svg);
				}
			}
			&-5 {
				& > div {
					background-image: url(../../../sys/assets/icons/bright/acf_wide.svg);
				}
			}
		}
	}
}
</style>