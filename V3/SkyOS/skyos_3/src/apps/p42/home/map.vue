<template>
	<div>
		<MglGeojsonLayer
			:data-keep="true"
			v-if="state.ui.active"
			sourceId="locationHistory"
			layerId="locationHistory"
			:source="state.ui.map.sources.locationHistory"
			:layer="state.ui.map.layers.locationHistory"
		/>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import { MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		MglMarker,
		MglNavigationControl,
		MglGeojsonLayer,
	},
	data() {
		return {
			state: {
				ui: {
					active: false,
					interval: null,
					map: {
						sources: {
							locationHistory: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: [
										{
											type: "Feature",
											geometry: {
												type: "Point",
												coordinates: [-112.0372, 46.608058]
											}
										}
									]
								}
							}
						},
						layers: {
							locationHistory: {
								id: "locationHistory",
								type: 'circle',
								source: 'locationHistory',
								paint: {
									'circle-color': '#4285f4',
									'circle-opacity': 0.2,
									'circle-pitch-alignment': 'map',
									'circle-stroke-color': '#4285f4',
									'circle-stroke-opacity': 0.2,
									'circle-stroke-width': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0,
										3, 0,
										5, 5,
										14, 9
									],
									'circle-radius': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 15,
										14, 10
									]

								}
							},
						},
					}
				}
			}
		}
	},
	activated() {
		this.state.ui.active = true;
		this.startInterval();

	},
	deactivated() {
		this.state.ui.active = false;
		clearInterval(this.state.ui.interval);
		this.$children.forEach((el :any) => { if(el.$attrs['data-keep'] !== (undefined || false)) { el.remove(); } });
	},
	methods: {
		startInterval() {
			this.state.ui.interval = setInterval(() => {
				const data = this.state.ui.map.sources.locationHistory.data;
				const lnglat = data.features[0].geometry.coordinates;
				lnglat[0] = lnglat[0] += 0.2;
				this.$os.maps.main.map.getSource('locationHistory').setData(data);
			}, 100);
		}
	}
});
</script>

<style lang="scss" scoped>

</style>