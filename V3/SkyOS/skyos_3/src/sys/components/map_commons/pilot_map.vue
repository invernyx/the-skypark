<template>
	<div>
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

		<MglMarker :coordinates="location" v-if="location && !duty && shown">
			<div class="map_pilot_marker" slot="marker">
				<div></div>
			</div>
		</MglMarker>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import * as turf from '@turf/turf';
import Eljs from '@/sys/libraries/elem';
import { MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';

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
			location: null,
			duty: false,
			shown: false,
			ui: {
				map: {
					anim: null,
					sources: {
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
						range: {
							type: "line",
							source: this.name + '_range',
							paint: {
								'line-color': '#000000',
								'line-width': 3,
								'line-opacity': 0.5
							}
						},
						range_out: {
							type: "line",
							source: this.name + '_range_out',
							paint: {
								'line-color': '#000000',
								'line-width': 1,
								'line-opacity': ['get', 'opacity'],
							}
						},
					}
				}
			}
		}
	},
	methods: {

		init() {
			this.map_layers_reorder();
			this.duty = this.$os.fleetService.aircraft_current ? this.$os.fleetService.aircraft_current.is_duty : false;
			this.location = this.$os.simulator.locationHistory.location;
			this.shown = this.$os.api.connected;
		},

		mapUpdateRelocation() {
			this.ui.map.sources.range.data.features = [];
			this.ui.map.sources.range_out.data.features = [];

			//if(this.$os.simulator.locationHistory && !this.duty) {
			//	this.location = this.$os.simulator.locationHistory.location;
			//	this.ui.map.sources.range.data.features.push(turf.circle(this.location, 14.816, { steps: 90, units: 'kilometers' }));
			//	for (let i = 6; i < 20; i++) {
			//		const circle = turf.circle(this.location, (1 + Math.pow((i * 0.5), 3)), {
			//			steps: 90,
			//			units: 'kilometers',
			//			properties: {
			//				opacity: Eljs.round(Math.pow(0.8, i), 2) * 1
			//			}
			//		});
			//		this.ui.map.sources.range_out.data.features.push(circle);
			//	}
			//}
		},

		theme_update() {
			const layers = this.ui.map.layers;

			if((this.$os.userConfig.get(['ui', 'theme']) == 'theme--bright' && !this.$os.maps.display_layer.sat) || this.$os.maps.display_layer.sectional.us.enabled) {

				layers.range.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_gold;
				layers.range_out.paint['line-color'] = this.$os.theme.colors.$ui_colors_bright_button_gold;

			} else {

				layers.range.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_button_gold;
				layers.range_out.paint['line-color'] = this.$os.theme.colors.$ui_colors_dark_button_gold;
			}
		},

		map_layers_reorder() {
			const order = {}

			order[this.name + '_range'] = 'split_0_1';
			order[this.name + '_range_out'] = 'split_0_1';

			Object.keys(order).forEach(element => {
				const entry = order[element];
				this.$os.maps.main.map.moveLayer(element, entry);
			});
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.shown = true;
					break;
				}
				case 'disconnect': {
					this.shown = false;
					break;
				}
				case 'fleet': {
					switch(wsmsg.name[1]){
						case 'update': {
							this.duty = this.$os.fleetService.aircraft_current ? this.$os.fleetService.aircraft_current.is_duty : false;
							this.mapUpdateRelocation()
							break;
						}
					}
					break;
				}
				case 'locationhistory': {
					switch(wsmsg.name[1]){
						case 'latest': {
							this.location = this.$os.simulator.locationHistory.location;
							this.mapUpdateRelocation()
							break;
						}
					}
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
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);

		this.app = this.$os.routing.activeApp;
	},

});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.map_pilot_marker {
	pointer-events: none;
	position: absolute;
	width: 30px;
	height: 30px;
	margin-top: -15px;
	background-size: contain;
	background-position: center;
	background-repeat: no-repeat;
	z-index: 2;
	.theme--bright & {
		filter: drop-shadow(0 3px 2px rgba($ui_colors_bright_shade_1, 1));
		background-image: url(../../../sys/assets/icons/dark/location_gold.svg);
	}
	.theme--sat &,
	.theme--dark & {
		filter: drop-shadow(0 3px 2px rgba($ui_colors_dark_shade_0, 0.3));
		background-image: url(../../../sys/assets/icons/bright/location_gold.svg);
	}


}
</style>