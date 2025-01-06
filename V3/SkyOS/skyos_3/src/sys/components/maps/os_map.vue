<template>
	<div class="map-container" :class="{ 'covered': covered }">

		<div class="map-container-background" :class="theme">
			<div class="map-container-background-image"></div>
		</div>

		<MapboxFrame
			ref="map"
			id="os_map"
			class="os_map"
			mapStyle="big"
			:mapTheme="theme"
			:hasFog="true"
			@load="mapLoaded"
			@theme_update="theme_update"
			>

			<MglGeojsonLayer
				sourceId="airportRunwaysSolid"
				layerId="airportRunwaysSolid"
				:source="map.sources.airportRunwaysSolid"
				:layer="map.layers.airportRunwaysSolid"
			/>
			<MglGeojsonLayer
				sourceId="airportRunwayMarkings"
				layerId="airportRunwayMarkings"
				:source="map.sources.airportRunwayMarkings"
				:layer="map.layers.airportRunwayMarkings"
			/>
			<MglGeojsonLayer
				sourceId="airportRunwayNumbers"
				layerId="airportRunwayNumbers"
				:source="map.sources.airportRunwayNumbers"
				:layer="map.layers.airportRunwayNumbers"
			/>
			<MglGeojsonLayer
				sourceId="airportParkings"
				layerId="airportParkings"
				:source="map.sources.airportParkings"
				:layer="map.layers.airportParkings"
			/>
			<MglGeojsonLayer
				sourceId="airportParkingNodes"
				layerId="airportParkingNumbers"
				:source="map.sources.airportParkingNodes"
				:layer="map.layers.airportParkingNumbers"
			/>


			<MglGeojsonLayer
				sourceId="airportMarkers"
				layerId="airportMarkers"
				:source="map.sources.airportMarkers"
				:layer="map.layers.airportMarkers"
			/>

			<MglGeojsonLayer
				sourceId="airportMarkers"
				layerId="airportMarkersLabel"
				:source="map.sources.airportMarkers"
				:layer="map.layers.airportMarkersLabel"
			/>

			<MglGeojsonLayer
				sourceId="airportMarkers"
				layerId="airportMarkersLabelDetailed"
				:source="map.sources.airportMarkers"
				:layer="map.layers.airportMarkersLabelDetailed"
			/>


			<MglGeojsonLayer
				layerId="airportRunwayExt"
				sourceId="airportRunwayExt"
				:source="map.sources.airportRunwayExt"
				:layer="map.layers.airportRunwayExt" />

			<MglGeojsonLayer
				layerId="airportRunwayExtNames"
				sourceId="airportRunwayExtNames"
				:source="map.sources.airportRunwayExtNames"
				:layer="map.layers.airportRunwayExtNames" />

			<MglGeojsonLayer
				layerId="terrainAvoidanceFill"
				sourceId="terrainAvoidanceFill"
				:source="map.sources.terrainAvoidanceFill"
				:layer="map.layers.terrainAvoidanceFill" />

			<MglGeojsonLayer
				layerId="terrainAvoidance"
				sourceId="terrainAvoidance"
				:source="map.sources.terrainAvoidance"
				:layer="map.layers.terrainAvoidance" />



			<!--<keep-alive>   :max="3" :include="app_cache_list" -->
				<transition :duration="1000">
					<router-view :root="root" name="map"/>
				</transition>
			<!--</keep-alive> -->

			<CommonPilot v-if="$os.routing.activeApp" :name="'p42_pilot'" :root="root" :app="$os.routing.activeApp" :appName="$os.routing.activeApp.name" />
			<CommonContract v-if="$os.routing.activeApp" :name="'p42_contracts'" :root="root" :app="$os.routing.activeApp" :appName="$os.routing.activeApp.name" />
			<CommonManifest v-if="$os.routing.activeApp" :name="'p42_manifest'" :root="root" :app="$os.routing.activeApp" :appName="$os.routing.activeApp.name" />
			<CommonFleet v-if="$os.routing.activeApp" :name="'p42_fleet'" :root="root" :app="$os.routing.activeApp" :appName="$os.routing.activeApp.name" />
			<CommonPlan v-if="$os.routing.activeApp" :name="'p42_plan'" :root="root" :app="$os.routing.activeApp" :appName="$os.routing.activeApp.name" />

		</MapboxFrame>

		<div class="map-container-controls">
			<div class="app-box nooverflow transparent small shadowed-deep">
				<div class="confine">
					<button_action class="listed map-control map-control-north" :class="{ 'info': display_layer.north }" @click.native="mapNorth()">
						<div class="map-control-north-compass" :style="{ 'transform': 'rotateZ(' + -map.heading + 'deg)'}">
							<div>
								<span>N</span>
								<span>000</span>
							</div>
							<div>
								<span>NE</span>
								<span>045</span>
							</div>
							<div>
								<span>E</span>
								<span>090</span>
							</div>
							<div>
								<span>SE</span>
								<span>135</span>
							</div>
							<div>
								<span>S</span>
								<span>180</span>
							</div>
							<div>
								<span>SW</span>
								<span>225</span>
							</div>
							<div>
								<span>W</span>
								<span>270</span>
							</div>
							<div>
								<span>NW</span>
								<span>315</span>
							</div>
						</div>
					</button_action>
					<button_action class="listed map-control map-control-3d" :class="{ 'angle': Math.round(map.pitch) != 0 }" @click.native="mapTiltToggle()">
						<span class="map-control-3d-pane">
							<span :style="{ 'transform': 'rotateX(' + (map.pitch * 0.6) + 'deg)'}">{{ Math.round(map.pitch) != 0 ? "3D" : "2D" }}</span>
						</span>
					</button_action>
					<button_action class="listed map-control map-control-track" :class="{ 'info': tracking.enabled }" @click.native="mapTrackToggle()" v-if="sim_live"></button_action>
					<button_action class="listed map-control map-control-autozoom" :class="{ 'info': tracking.autozoom }" @click.native="mapAutoZoomToggle()" v-if="tracking.enabled && sim_live"></button_action>
				</div>
			</div>

			<div class="app-box nooverflow transparent small shadowed-deep">
				<div class="confine">
					<button_action class="listed map-control map-control-hill" :class="{ 'info': display_layer.hill }" @click.native="mapToggleHill()"></button_action>
					<button_action class="listed map-control map-control-sat" :class="{ 'info': display_layer.sat }" @click.native="mapToggleSat()"></button_action>
					<button_action class="listed map-control map-control-sec" :class="{ 'info':  display_layer.sectional.us.enabled }" @click.native="mapToggleSectionalUS()">
						<span>US</span>
					</button_action>
				</div>
			</div>

			<div class="app-box nooverflow transparent small shadowed-deep">
				<div class="confine">
					<button_action class="listed map-control map-control-apt" :class="{ 'info':display_layer.airports.icaos.enabled }" @click.native="mapToggleApt()" v-if="has_transponder">
						<span>ICAO</span>
					</button_action>
					<button_action class="listed map-control map-control-wx" :class="{ 'info': display_layer.wx.radar.enabled }" @click.native="mapToggleWx()"></button_action>
					<button_action class="listed map-control map-control-fleet" :class="{ 'info': display_layer.fleet.enabled }" @click.native="mapToggleFleet()"></button_action>
				</div>
			</div>

		</div>

		<div class="map-container-cover" @click="mapUncover">
		</div>

	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import * as turf from '@turf/turf';
import MapboxExt from '@/sys/libraries/mapboxExt';
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import { AppInfo, StatusType, NavType } from '@/sys/foundation/app_model';
import Airport, { Runway } from "@/sys/classes/airport";
import { MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';

import CommonContract from "@/sys/components/map_commons/contracts_map.vue"
import CommonManifest from "@/sys/components/map_commons/manifest_map.vue"
import CommonFleet from "@/sys/components/map_commons/fleet_map.vue"
import CommonPlan from "@/sys/components/map_commons/plans_map.vue"
import CommonPilot from "@/sys/components/map_commons/pilot_map.vue"

// Future shading: https://github.com/wwwtyro/map-tile-lighting-demo

export default Vue.extend({
	props: ["root"],
	components: {
		MapboxFrame,
		MglMarker,
		MglNavigationControl,
		MglGeojsonLayer,

		CommonContract,
		CommonManifest,
		CommonFleet,
		CommonPlan,
		CommonPilot
	},
	data() {
		return {
			mapbox: null,
			loaded: false,
			theme: 'theme--bright',
			has_transponder: this.$os.api.connected,
			sim_live: this.$os.simulator.live,
			covered: false,
			app_cache_list: [] as string[],
			map: {
				loaded: false,
				location: [0, 0],
				heading: 0,
				pitch: 0,
				zoom: 0,
				boundsMatrix: '',
				padding_animation: null,
				sources: {
					airportMarkers: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					airportRunwayExt: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					airportRunwayExtNames:{
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					airportRunwaysSolid: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					airportRunwayMarkings: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					airportRunwayNumbers: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					airportParkings: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					airportParkingNodes: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					terrainAvoidanceFill: {
						type: 'geojson',
						data: {
							type: 'FeatureCollection',
							features: []
						}
					},
					terrainAvoidance: {
						type: 'geojson',
						lineMetrics: true,
						data: {
							type: 'FeatureCollection',
							features: []
						}
					}
				},
				layers: {
					airportMarkersLabelDetailed: {
						id: "airportMarkersLabelDetailed",
						type: 'symbol',
						source: 'airportMarkers',
						minzoom: 12,
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
					airportMarkersLabel: {
						id: "airportMarkersLabel",
						type: 'symbol',
						source: 'airportMarkers',
						maxzoom: 12,
						minzoom: 5,
						layout: {
							"text-field": ['get', 'icao'],
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
					airportMarkers: {
						id: "airportMarkers",
						type: 'circle',
						source: 'airportMarkers',
						paint: {
							'circle-color': '#000000',
							'circle-pitch-alignment': 'map',
							'circle-stroke-color': '#000000',
							'circle-stroke-opacity': 0.5,
							'circle-opacity': 0,
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

						}
					},
					airportRunwaysSolid: {
						id: "airportRunwaysSolid",
						type: 'fill',
						source: 'airportRunwaysSolid',
						paint: {
							'fill-color': ["get", "type"],
						},
					},
					airportRunwayMarkings: {
						id: "airportRunwayMarkings",
						type: 'line',
						source: 'airportRunwayMarkings',
						layout: {
							//"line-cap": "square"
						},
						paint: {
							"line-dasharray": ["literal", [20, 15]],
							"line-color": "hsl(0, 38%, 100%)",
							"line-width": [
								"interpolate",
								["exponential", 1.91],
								["zoom"],
								13,
								1,
								19,
								10
							],
							"line-opacity": [
								"interpolate",
								["linear"],
								["zoom"],
								13,
								0,
								14,
								1
							]
						},
					},
					airportRunwayNumbers: {
						id: "airportRunwayNumbers",
						type: 'symbol',
						source: 'airportRunwayNumbers',
						minzoom: 12,
						layout: {
							"text-field": ['get', 'rwy'],
							"text-size": 12,
							"text-max-angle": 0,
							"text-font": ["DIN Pro Bold", "Arial Unicode MS Regular"],
							"text-padding": 5,
							"text-offset": [0, 0],
        					"text-allow-overlap": true,
						},
						paint: {
							"text-halo-color": "#454545",
							"text-halo-width": 8,
							"text-color": "hsl(60, 2%, 88%)",
							"text-translate": [0, 0]
						}
					},
					airportParkings: {
						id: 'airportParkings',
            			type: "line",
						source: 'airportParkings',
						layout: {},
						paint: {
							"line-color": "#e2e0d9",
							"line-width": 2,
							"line-opacity": 0.3,
						}
					},
					airportParkingNumbers: {
						id: "airportParkingNumbers",
						type: 'symbol',
						source: 'airportParkingNodes',
						minzoom: 13,
						layout: {
							"text-field": ['get', 'number'],
							"text-size": 12,
							"text-font": ["DIN Pro Bold", "Arial Unicode MS Regular"],
							"text-padding": 5,
							"text-offset": [0, 0],
        					"text-allow-overlap": true,
						},
						paint: {
							"text-halo-color": "#121212",
							"text-halo-width": 8,
							"text-color": "#FFFFFF",
							"text-translate": [0, 0]
						}
					},
					airportRunwayExt: {
						id: 'airportRunwayExt',
						type: 'line',
						source: 'airportRunwayExt',
						minzoom: 9,
						maxzoom: 13,
						layout: {
							'line-cap': 'round',
							'line-join': 'round'
						},
						paint: {
							'line-width': ['get', 'width'],
							'line-opacity': ['get', 'opacity'],
							'line-color': ['get', 'color'],
						},
						filter: ['in', '$type', 'LineString']
					},
					airportRunwayExtNames: {
						id: "airportRunwayExtNames",
						type: 'symbol',
						source: 'airportRunwayExtNames',
						minzoom: 9,
						maxzoom: 13,
						layout: {
							"text-field": ['get', 'title'],
							"text-font": ["DIN Offc Pro Bold", "Arial Unicode MS Bold"],
							"text-size": 14,
						},
						paint: {
							'text-color': "#FFF",
						}
					},

					terrainAvoidanceFill: {
						id: 'terrainAvoidanceFill',
						type: 'heatmap',
						source: 'terrainAvoidanceFill',
						paint: {
							// Increase the heatmap weight based on frequency and property magnitude
							'heatmap-weight': ['get', 'delta'],
							// Increase the heatmap color weight weight by zoom level
							// heatmap-intensity is a multiplier on top of heatmap-weight
							'heatmap-intensity': [
								'interpolate',
								['linear'],
								['zoom'],
								0, 0.8,
								9, 0.8
							],
							// Color ramp for heatmap.  Domain is 0 (low) to 1 (high).
							// Begin color ramp at 0-stop with a 0-transparancy color
							// to create a blur-like effect.
							'heatmap-color': [
								'interpolate',
								['linear'],
								['heatmap-density'],
								0,
								'rgba(255,255,0,0)',
								0.2,
								'rgba(255,255,0,0.1)',
								0.4,
								'rgba(255,255,0,0.3)',
								0.6,
								'rgba(255,0,0,0.7)',
								0.8,
								'rgba(255,0,0,0.8)',
								1,
								'rgba(255,0,0,1)'
							],
							// Adjust the heatmap radius by zoom level
							'heatmap-radius': [
								'interpolate',
								["exponential", 1.91],
								['zoom'],
								0, 0,
								15, 100
							],
							// Transition from heatmap to circle layer by zoom level
							'heatmap-opacity': 1
						}
					},
					terrainAvoidance: {
						id: 'terrainAvoidance',
						type: 'line',
						source: 'terrainAvoidance',
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
								0, 'rgba(255,0,0,0)',
								0.1, 'rgba(255,0,0,1)',
								0.9, 'rgba(255,0,0,1)',
								1, 'rgba(255,0,0,0)'
							],
							'line-width': 5,
							'line-opacity': 1
						}
					},
				}
			},
			airports: {
				list: []
			},
			tracking: {
				enabled: true,
				tilted: true,
				north: false,
				autozoom: false,
			},
			display_layer: {
				pitch: 0,
				north: false,
				autozoom: false,
				sat: false,
				hill: true,
				fleet: {
					enabled: true,
				},
				wx: {
					radar: {
						enabled: true,
					}
				},
				sectional: {
					us: {
						enabled: false,
					}
				},
				airports: {
					icaos: {
						enabled: true,
					},
					layout: {
						enabled: true,
					}
				}
			},
			weatherRadar: {
				radarAnimated: true,
				radarFetchTimer: null,
				radarTimer: null,
				data: null,
			}
		}
	},
	methods: {

		mapLoaded(map :any) {
			this.$os.maps.main = map;
			this.mapbox = Object.freeze(map);

			//this.mapbox.map.setProjection('globe');

			// Restore map state
			if(this.$os.maps.display_layer.sat)
				this.mapToggleSat(this.$os.maps.display_layer.sat, false);

			if(this.$os.maps.display_layer.wx.radar.enabled)
				this.mapToggleWx(this.$os.maps.display_layer.wx.radar.enabled, false);

			if(this.$os.maps.display_layer.sectional.us.enabled)
				this.mapToggleSectionalUS(this.$os.maps.display_layer.sectional.us.enabled, false);

			if(this.$os.maps.display_layer.fleet.enabled)
				this.mapToggleFleet(this.$os.maps.display_layer.fleet.enabled, false);

			if(this.$os.maps.display_layer.airports.icaos.enabled)
				this.mapToggleApt(this.$os.maps.display_layer.airports.icaos.enabled, false);

			if(this.$os.simulator.live){
				this.mapbox.map.setCenter([this.$os.simulator.location.Lon, this.$os.simulator.location.Lat]);
			} else {
				this.mapbox.map.setCenter([this.map.location[0], this.map.location[1]]);
			}

			this.mapbox.map.setBearing(this.map.heading);
			this.mapbox.map.setPitch(this.map.pitch);
			this.mapbox.map.setZoom(this.map.zoom);


			['mousedown', 'touchstart'].forEach((ev) => { //, 'zoom', 'rotate', 'pitch'
				this.mapbox.map.on(ev, () => {
					//this.$os.system.setSidebar(false);
					this.$os.eventsBus.Bus.emit('map', { name: 'interact', payload: null });
				});
			});

			let move_end_timer = null;
			['moveend'].forEach((ev) => { //, 'zoom', 'rotate', 'pitch'
				this.mapbox.map.on(ev, () => {
					clearTimeout(move_end_timer);
					move_end_timer = setTimeout(() => {
						clearTimeout(move_end_timer);
						this.mapMoveEnd();
					}, 10);
				});
			});

			['wheel'].forEach((ev) => {
				this.mapbox.map.on(ev, (ev1 :any) => {
					if(this.tracking.enabled) {
						if(this.tracking.autozoom) {
							this.tracking.autozoom = false;
						}

						ev1.preventDefault();

						const cz = this.mapbox.map.getZoom();
						this.map.zoom = Eljs.limiter(1, 24, cz - (ev1.originalEvent.deltaY / 1000));

						this.mapTrackUpdate();
					}
				});
			});

			['move', 'zoom', 'rotate', 'pitch'].forEach((ev) => {
				this.mapbox.map.on(ev, (ev1) => {
					if(ev1.originalEvent) {
						switch(ev) {
							case 'move':
							case 'pitch':
							case 'rotate': {
								if(this.tracking.enabled) {
									this.mapTrackToggle(true);
								}
								break;
							}
						}
					}
				});
			});


			//this.mapbox.map.on('dataloading', (ev) => { // dataloading // sourcedata

				//console.log(ev.sourceId);

				//if(ev.dataType != 'source') {
				//	console.log(ev.dataType);
				//}

				//console.log(this.$os.maps.main.map.getSource('composite'));

				//if(ev.dataType == 'source' && ev.sourceId == 'composite') {

					//console.log(ev, ev.tile.state, ev.tile.uses)

					//console.log(ev, ev.tile.uses, MapboxExt.globeTileLatLngCorners(ev.coord.canonical))
					//console.log(ev);
					//console.log(ev.sourceId, MapboxExt.globeTileLatLngCorners(ev.coord.canonical));
					//globeTileLatLngCorners

        			//const center = tr.unproject(ev.coord.projMatrix);

					// actor
        			//this.target.addEventListener('message', this.receive, false);

				//}
			//});

			this.mapbox.map.on('rotate', () => {
				this.map.heading = this.mapbox.map.getBearing();
			});

			this.mapbox.map.on('pitch', () => {
				this.map.pitch = this.mapbox.map.getPitch();
			});

			// Reorder Layers
			this.map_layers_reorder();
			this.mapUpdateRadar();
			this.update_airports();
			this.update_padding(this.$os.routing.activeApp.padding);

			this.map.loaded = true;

			// Set Event handlers
			this.$os.eventsBus.Bus.emit('map', { name: 'loaded', payload: null });
			this.$os.eventsBus.Bus.emit('map', { name: 'interact', payload: null });
		},

		mapMoveEnd() {
			if(this.map.loaded) {
				const centerLocation = this.mapbox.map.getCenter();
				this.map.location = [centerLocation.lng, centerLocation.lat];
				this.map.heading = this.mapbox.map.getBearing();
				this.map.pitch = this.mapbox.map.getPitch();
				this.map.zoom = this.mapbox.map.getZoom();

				this.update_bounds();
				this.update_airports();
				this.state_save();
			}
		},

		mapTrackUpdate() {
			if(this.$os.simulator.live) {

				if(this.tracking.enabled) {
					const simData = this.$os.simulator.location
					const appFrameRef = ((this.$refs['map'] as any).$el as HTMLElement);
					let zoom = 10;
					let heading = 0;
					let speed = 0;
					let pitch = 0;
					let xOffset = 0;
					let yOffset = 0;

					if ((this.tracking.tilted && !this.tracking.north)) {
						pitch = 68;
					} else {
						pitch = 0;
						this.tracking.tilted = false;
					}

					//speed = 1000;
					if(!this.tracking.north) {
						const rawZoom = (22 - simData.GS / 200);
						//const turnRateEff = simData.TR * (simData.GS / 200);
						zoom = Eljs.limiter(8, 20, -6 + Eljs.Easings.easeInExpo(0, rawZoom, 0, 22, 22));

						if(simData.GS > 20) {
							heading = simData.CRS;
						//	xOffset -= Eljs.limiter(-appFrameRef.offsetWidth / 4, appFrameRef.offsetWidth / 4, (appFrameRef.offsetWidth) * turnRateEff / 25);
						//} else if(simData.GS > 5) {
						//	heading = Math.abs(turnRateEff) > 5 ? simData.Hdg + Eljs.limiter(-35, 35, (turnRateEff * 5)) : simData.Hdg;
						} else {
							heading = simData.Hdg;
						}

						yOffset = -(170 - ((appFrameRef.offsetHeight) / 2));
					} else {
						const rawZoom = (22 - simData.GS / 150);
						zoom = Eljs.limiter(8, 20, -6 + Eljs.Easings.easeInExpo(0, rawZoom, 0, 22, 22));

						yOffset = 30;
					}


					const opt = {
						center: [simData.Lon, simData.Lat],
						duration: speed,
						zoom: 0,
						offset: [xOffset, yOffset],
						easing: (t :number) => {
							return 1 - Math.pow(1 - t, 5);
						},
					};

					if(this.tracking.autozoom) {
						opt.zoom = Eljs.limiter(4, 24, zoom);// * this.state.ui.map.zoomOffset;
					} else {
						opt.zoom = this.map.zoom;
					}

					if(!this.tracking.north) {
						opt['bearing'] = heading;
					} else {
						opt['bearing'] = 0;
					}

					if(this.tracking.tilted) {
						opt['pitch'] = pitch;
					} else {
						opt['pitch'] = 0;
					}

					if(this.$os.maps.main)
						this.$os.maps.main.map.flyTo(opt);
				}
			}
		},

		mapUpdateRadar() {
			// https://www.rainviewer.com/api.html
			//"https://tilecache.rainviewer.com/v2/radar/{ts}/{size}/{z}/{x}/{y}/{color}/{options}.png"
			clearInterval(this.weatherRadar.radarFetchTimer);

			if(this.display_layer.wx.radar.enabled) {
				let TimeIndex = 0;
				const fetchData = () => {
					fetch('https://api.rainviewer.com/public/maps.json', { method: 'get' })
					.then(response => response.json())
					.then((data) => {
						if(this.mapbox) {
							this.weatherRadar.data = data;
							data.forEach((radarTime :number, index :number) => {
								let existing = this.mapbox.map.getSource('radar-tiles-' + index);
								if(!existing) {
									const size = this.$root.$data.state.device.is_apple_webkit ? 256 : 512;
									this.mapbox.map.addSource('radar-tiles-' + index, {
										"type": "raster",
										"tiles": [
											"https://tilecache.rainviewer.com/v2/radar/" + data[index] + "/" + size + "/{z}/{x}/{y}/8/1_1.png"
										],
										"tileSize": size
									});
									this.mapbox.map.addLayer({
										"id": 'radar-tiles-' + index,
										"type": "raster",
										"source": 'radar-tiles-' + index,
										"minzoom": 0,
										"maxzoom": 22
									});
									this.mapbox.map.moveLayer('radar-tiles-' + index, this.mapbox.map.getStyle().layers[10].id);

									this.mapbox.map.setPaintProperty('radar-tiles-' + index, 'raster-opacity', 1);
									this.mapbox.map.setPaintProperty('radar-tiles-' + index, 'raster-fade-duration', 1000);
								} else {
									existing.tiles[0] = "https://tilecache.rainviewer.com/v2/radar/" + data[index] + "/512/{z}/{x}/{y}/8/1_1.png";
									this.mapbox.map.style._sourceCaches['radar-tiles-' + index].clearTiles();
									this.mapbox.map.style._sourceCaches['radar-tiles-' + index].update(this.mapbox.map.transform);
									this.mapbox.map.triggerRepaint();
								}

							});

							clearInterval(this.weatherRadar.radarTimer);
							this.weatherRadar.radarTimer = setInterval(() => {
								//if(!this.$os.routing.activeApp.sleeping) {
									if(!this.weatherRadar.radarAnimated) {
										TimeIndex = data.length - 1;
									}
									if(TimeIndex < data.length) {
										for (let i = 0; i < data.length; i++) {
											if(i == TimeIndex) {
												this.mapbox.map.setLayoutProperty('radar-tiles-' + i, 'visibility', 'visible');
												//this.mapbox.map.setPaintProperty('radar-tiles-' + i, 'raster-opacity', 0.5);
											} else {
												this.mapbox.map.setLayoutProperty('radar-tiles-' + i, 'visibility', 'none');
												//setTimeout(() => {
												//	this.mapbox.map.setPaintProperty('radar-tiles-' + i, 'raster-opacity', 0);
												//}, 30);
											}
										}
									}

									if(TimeIndex < data.length + 2) {
										TimeIndex++;
									} else {
										TimeIndex = 0;
									}
								//}
							}, 250);

						}
					}).catch((err) => {

					});
				}
				fetchData();
				this.weatherRadar.radarFetchTimer = setInterval(() => fetchData, 600000);
			} else {
				if(this.weatherRadar.data) {
					clearInterval(this.weatherRadar.radarTimer);
					clearInterval(this.weatherRadar.radarFetchTimer);
					this.weatherRadar.data.forEach((radarTime :number, index :number) => {
						this.mapbox.map.removeLayer('radar-tiles-' + index);
						this.mapbox.map.removeSource('radar-tiles-' + index);
					});
				}
			}
		},

		mapGoTo(type: string, data :any) {

			if(this.tracking.enabled) {
				this.$os.eventsBus.Bus.emit('map', {
					name: 'untrack',
					payload: null
				});
			}

			switch(type) {
				case 'airport': {
					const nodes = [data.airport.location];

					nodes.push(Eljs.MapOffsetPosition(data.airport.location[0], data.airport.location[1], data.airport.radius * 1000, 0));
					nodes.push(Eljs.MapOffsetPosition(data.airport.location[0], data.airport.location[1], data.airport.radius * 1000, 90));
					nodes.push(Eljs.MapOffsetPosition(data.airport.location[0], data.airport.location[1], data.airport.radius * 1000, 180));
					nodes.push(Eljs.MapOffsetPosition(data.airport.location[0], data.airport.location[1], data.airport.radius * 1000, 270));

					MapboxExt.fitBoundsExt(this.mapbox.map, turf.bbox(turf.lineString(nodes)), {
						//padding: this.$os.maps.padding,
						offset: [0,0],
						pitch: 0,
						bearing: 0,
						duration: 500,
						vanish: this.$os.maps.padding,
					}, null);
					break;
				}
				case 'location': {
					const nodes = [data.location];

					for (let i = 0; i < 360; i+=90) {
						nodes.push(Eljs.MapOffsetPosition(data.location[0], data.location[1], data.radius * 1000, i));
						console.log(nodes, data);
					}


					MapboxExt.fitBoundsExt(this.mapbox.map, turf.bbox(turf.lineString(nodes)), {
						//padding: this.$os.maps.padding,
						offset: [0,0],
						pitch: 0,
						bearing: 0,
						duration: 500,
						vanish: this.$os.maps.padding,
					}, null);
					break;
				}
				case 'area': {
					const nodes = data.nodes;

					MapboxExt.fitBoundsExt(this.mapbox.map, turf.bbox(turf.lineString(nodes)), {
						offset: [0,0],
						pitch: 0,
						bearing: 0,
						duration: 500,
						vanish: this.$os.maps.padding,
					}, null);
					break;
				}
			}
		},

		update_airports(onlyRender = false) {

			const zoom = this.mapbox.map.getZoom();
			const detailed = zoom;
			const clear = () => {
				this.mapbox.map.setPaintProperty('aeroway-runway', 'line-opacity', 1);
				this.mapbox.map.setLayoutProperty('aeroway-runway-label', 'visibility', 'visible');
				this.mapbox.map.setLayoutProperty('aeroway', 'visibility', 'visible');
				this.map.sources.airportMarkers.data.features = [];
				this.map.sources.airportRunwaysSolid.data.features = [];
				this.map.sources.airportRunwayMarkings.data.features = [];
				this.map.sources.airportRunwayNumbers.data.features = [];
				this.map.sources.airportRunwayExt.data.features = [];
				this.map.sources.airportRunwayExtNames.data.features = [];
				this.map.sources.airportParkings.data.features = [];
				this.map.sources.airportParkingNodes.data.features = [];
			}

			if(!this.has_transponder) {
				clear();
				return;
			}

			if(onlyRender && this.display_layer.airports.icaos.enabled) {
				clear();
				this.airports.list.forEach((airport :Airport) => {
					if(detailed > 10) {
						this.mapRenderAirport(airport);
					}
					if(detailed > 7) {
						this.mapRenderAirportRunwayExt(airport);
					}
					this.mapbox.map.setPaintProperty('aeroway-runway', 'line-opacity', 0.2);
					this.mapbox.map.setLayoutProperty('aeroway-runway-label', 'visibility', 'none');
					this.mapbox.map.setLayoutProperty('aeroway', 'visibility', 'none');
				});
				this.mapbox.map.setPaintProperty('aeroway-runway', 'line-opacity', 0.2);
				return;
			}

			if(this.display_layer.airports.icaos.enabled) {
				const bounds = this.$os.maps.bounds;
				const matrix = this.mapGetBoundMatrix(bounds);

				if(matrix != this.map.boundsMatrix) {
					this.map.boundsMatrix = matrix;

					this.$os.api.send_ws('airports:from-coords', {
					coords: bounds,
					detail: zoom,
					}, (ret :any) => {
						clear();
						this.airports.list = [];
						ret.payload.forEach(tile => {
							tile.list.forEach((airport :Airport) => {

								this.airports.list.push(airport);

								// Add details
								if(detailed > 10) {
									this.mapRenderAirport(airport);
								}

								if(detailed > 7) {
									this.mapRenderAirportRunwayExt(airport);
								}

								// Add airport basic
								this.map.sources.airportMarkers.data.features.push({
									type: "Feature",
									id: Eljs.getNumGUID(),
									properties: {
										title: airport.icao + " - " + airport.name,
										icao: airport.icao,
										airport: airport,
									},
									geometry: {
										type: "Point",
										coordinates: airport.location,
									}
								})
							});

						});

						this.mapbox.map.setPaintProperty('aeroway-runway', 'line-opacity', 0.2);
						this.mapbox.map.setLayoutProperty('aeroway-runway-label', 'visibility', 'none');
						this.mapbox.map.setLayoutProperty('aeroway', 'visibility', 'none');
					});

				}


			} else {
				this.map.boundsMatrix = '';
				clear();
			}
		},

		update_bounds() {
			if(this.$os.maps.main) {

				const bounds = this.mapbox.map.getBounds();
				//console.log(bounds);

				//const mapPadding = this.$os.maps.padding;
				//const mapRef = (this.$refs.map as any).$el;
				//const mapRange = {
				//	top: 0,
				//	right: 0,
				//	bottom: 0,
				//	left: 0,
				//}

				const NW = bounds.getNorthWest()//this.mapbox.map.unproject([mapRange.left, mapRange.top]);
				const SE = bounds.getSouthEast()//this.mapbox.map.unproject([mapRange.right, mapRange.bottom]);

				var line = turf.lineString([[NW.lng, NW.lat], [SE.lng, SE.lat]]);
				var bbox = turf.bbox(line) as any;

				this.$os.maps.bounds = {
					nw: [bbox[0],bbox[3]],
					se: [bbox[2],bbox[1]],
				}
			}

			return this.$os.maps.bounds;
		},

		update_padding(padding :{
			top: number,
			left: number,
			right: number,
			bottom: number,
			sidebar?: boolean
		}) {

			if(this.mapbox) {

				let padding_in = {
					top: 100,
					left: 60,
					right: 120,
					bottom: 60,
				}

				if(padding) {
					padding_in.left += padding.left;
					padding_in.right += padding.right;
					padding_in.top += padding.top;
					padding_in.bottom += padding.bottom;
				}

				if(this.$os.system.sidebarOpen) {
					if(padding) {
						padding_in.left += padding.sidebar != undefined ? padding.sidebar ? 370 : 0 : 370;
					} else {
						padding_in.left += 370;
					}
				}

				this.$os.maps.padding = padding_in;


				//if(this.covered) {
				//	new_padding.bottom = 50;
				//} else {
				//	if(this.$route.matched.length > 1) {
				//		new_padding.bottom = 300;
				//	} else {
				//		new_padding.bottom = 20;
				//	}
				//}

				//console.log('reset map padding')

				//this.mapbox.map.setPadding(new_padding);

				/*
				const duration = 1000;
				const osp = Object.assign({}, this.$os.maps.padding);

				if(new_padding.left != osp.left || new_padding.top != osp.top || new_padding.right != osp.right || new_padding.bottom != osp.bottom) {

					clearInterval(this.map.padding_animation);
					const diffs = {
						top: osp.top - new_padding.top,
						left: osp.left - new_padding.left,
						right: osp.right - new_padding.right,
						bottom: osp.bottom - new_padding.bottom,
					}
					const time_start = new Date();
					this.map.padding_animation = setInterval(() => {

						const time_now = new Date();
						const time_diff = time_now.getTime() - time_start.getTime();
						const factor = Eljs.Easings.easeOutQuad(0, time_diff / duration, 0, 1, 1);

						const interpolated = {
							top: osp.top - (factor * diffs.top),
							left: osp.left - (factor * diffs.left),
							right: osp.right - (factor * diffs.right),
							bottom: osp.bottom - (factor * diffs.bottom),
						}

						if(time_diff >= duration) {
							clearInterval(this.map.padding_animation);
							this.$os.maps.padding = new_padding;
							if(this.mapbox) {
								//this.mapbox.map.transform.padding = this.$os.maps.padding;
								//this.mapbox.map.triggerRepaint();
								console.log(this.mapbox.map);
								//this.mapbox.map.setPadding(this.$os.maps.padding);
							}
						} else {
							if(this.mapbox) {
								this.mapbox.map.transform.interpolatePadding(osp, new_padding, factor);
								//this.mapbox.map.transform.padding = interpolated;
								//this.mapbox.map.triggerRepaint();
								//this.mapbox.map.setPadding(interpolated);
							}
						}

					}, 10);
				}
				*/

			}
		},

		mapApplyPadding() {

			if(this.mapbox) {
				MapboxExt.ensurePadding(this.mapbox.map, this.$os.maps.padding);

				this.mapbox.map.easeTo({
					padding: this.$os.maps.padding,
					duration: 300
				});
			}
		},

		mapRenderTerrain() {

			this.map.sources.terrainAvoidance.data.features = [];
			this.map.sources.terrainAvoidanceFill.data.features = [];

			const offset = 100;
			let barriers = [];
			let barrier = [];
			let previous_ray = null;
			this.$os.simulator.location.Terrain.forEach((ray) => {

				let found = false;

				ray.forEach((node, index) => {

					let difference = node[2] - this.$os.simulator.location.Alt * 0.3048;

					if(difference > -30) {

						// Add heatmap
						this.map.sources.terrainAvoidanceFill.data.features.push({
							type: "Feature",
							properties: {
								delta: ((difference + offset) / 1000),
							},
							geometry: {
								type: 'Point',
								coordinates: [node[0], node[1]]
							}
						});

						if(previous_ray) {

							// Interpolate
							const split = 4;
							let splitAt = 1;
							const lon_intr = (node[0] - previous_ray[index][0]) / split;
							const lat_intr = (node[1] - previous_ray[index][1]) / split;
							const alt_intr = (node[2] - previous_ray[index][2]) / split;

							while(split > splitAt) {

								const lon = previous_ray[index][0] + (lon_intr * splitAt);
								const lat = previous_ray[index][1] + (lat_intr * splitAt);
								const dif = previous_ray[index][2] + (alt_intr * splitAt);

								let difference_int = dif - this.$os.simulator.location.Alt * 0.3048;

								if(difference > 0) {
									if(!found && difference_int + offset > 0) {
										if(index > 4) {
											barrier.push([lon, lat, dif]);
											found = true;
										}
									}
								}

								this.map.sources.terrainAvoidanceFill.data.features.push({
									type: "Feature",
									properties: {
										delta: ((difference_int + offset) / 1000),
									},
									geometry: {
										type: 'Point',
										coordinates: [lon, lat]
									}
								});

								splitAt++;
							}
						}

					}

				});

				if(!found) {
					barriers.push(barrier);
					if(barrier.length > 1) {
						barrier = [];
					}
				}

				previous_ray = ray;
			});


			if(barrier.length > 1) {
				barriers.push(barrier);
			}

			const feature = {
				type: "Feature",
				properties: {	},
				geometry: {
					type: 'MultiLineString',
					coordinates: [] as any[]
				}
			};

			barriers.forEach(barrier => {
				let barrier_n = [];

				barrier.forEach(node => {
					barrier_n.push([node[0], node[1]]);
				});

				if(barrier_n.length)
					feature.geometry.coordinates.push(barrier_n)
			});


			this.map.sources.terrainAvoidance.data.features.push(feature);
		},

		mapRenderAirport(airport :Airport) {

			// Parkings
			airport.parkings.forEach(parking => {

				const center = parking.location;
				const radius = parking.diameter / 2;
				const options = { steps: 20, properties: {}};

				const circle = Object.assign({}, turf.circle(center, radius / 1000, options));

				const offset1 = Eljs.MapOffsetPosition(center[0], center[1], radius * 0.6, parking.heading + 30);
				const offset2 = Eljs.MapOffsetPosition(center[0], center[1], radius * 0.85, parking.heading + 0);
				const offset3 = Eljs.MapOffsetPosition(center[0], center[1], radius * 0.6, parking.heading + -30);
				this.map.sources.airportParkings.data.features.push({
					type: "Feature",
					id: Eljs.getNumGUID(),
					properties: {
						number: parking.number
					},
					geometry: {
						type: "MultiLineString",
						coordinates: [
							circle.geometry.coordinates[0],
							[
								offset1,
								offset2,
								offset3
							]
						]
					}
				});

				this.map.sources.airportParkingNodes.data.features.push({
					type: "Feature",
					id: Eljs.getNumGUID(),
					properties: {
						number: parking.number
					},
					geometry: {
						type: "Point",
						coordinates: center
					}
				});

			});

			// Runways
			airport.runways.forEach((runway :Runway) => {

				const width = runway.width;
				const length = runway.length;
				const offset1 = Eljs.MapOffsetPosition(runway.location[0], runway.location[1], width / 2, runway.heading + 90);
				const offset2 = Eljs.MapOffsetPosition(runway.location[0], runway.location[1], width / 2, runway.heading - 90);

				const rw_start1 = Eljs.MapOffsetPosition(offset1[0], offset1[1], length / 2, runway.heading);
				const rw_end1 = Eljs.MapOffsetPosition(offset1[0], offset1[1], length / 2, runway.heading + 180);

				const rw_start2 = Eljs.MapOffsetPosition(offset2[0], offset2[1], length / 2, runway.heading);
				const rw_end2 = Eljs.MapOffsetPosition(offset2[0], offset2[1], length / 2, runway.heading + 180);

				let findColor = () => {
					const th = this.theme == 'theme--bright';
					switch(runway.surface) {
						case 'Water':
							return th ? '#c6c5c3' : '#00000f';
						case 'Grass':
						case 'Snow':
						case 'Gravel':
						case 'Sand':
						case 'Shale':
							return th ? '#2aa314' : '#335234';
						default: return th ? '#454545' : '#454545';
					}
				};

				// Surface
				let feature = {
					type: "Feature",
					id: Eljs.getNumGUID(),
					properties: {
						airport: airport,
						type: findColor(),
					},
					geometry: {
						type: "Polygon",
						coordinates: [
							[
								[rw_start1[0], rw_start1[1]],
								[rw_end1[0], rw_end1[1]],
								[rw_end2[0], rw_end2[1]],
								[rw_start2[0], rw_start2[1]],
								[rw_start1[0], rw_start1[1]]
							]
						]
					}
				};
				this.map.sources.airportRunwaysSolid.data.features.push(feature);

				// Marking
				const markingLength = (length / 2) - 50;
				let feature2 = {
					type: "Feature",
					id: Eljs.getNumGUID(),
					properties: {
						airport: airport,
					},
					geometry: {
						type: "LineString",
						coordinates: [Eljs.MapOffsetPosition(runway.location[0], runway.location[1], markingLength, runway.heading), Eljs.MapOffsetPosition(runway.location[0], runway.location[1], markingLength, runway.heading - 180)]
					}
				};
				this.map.sources.airportRunwayMarkings.data.features.push(feature2);


				// Number
				const numberLength = (length / 2) - 25;
				this.map.sources.airportRunwayNumbers.data.features.push({
					type: "Feature",
					id: Eljs.getNumGUID(),
					properties: {
						rwy: runway.secondary_name
					},
					geometry: {
						type: "Point",
						coordinates: Eljs.MapOffsetPosition(runway.location[0], runway.location[1], numberLength, runway.heading)
					}
				});
				this.map.sources.airportRunwayNumbers.data.features.push({
					type: "Feature",
					id: Eljs.getNumGUID(),
					properties: {
						rwy: runway.primary_name
					},
					geometry: {
						type: "Point",
						coordinates: Eljs.MapOffsetPosition(runway.location[0], runway.location[1], numberLength, runway.heading - 180)
					}
				});

			});
		},

		mapRenderAirportRunwayExt(airport :Airport) {

			const MakeRwy = (wx :any, runway :Runway, heading :number, name :string) => {

				const IsDark = this.$os.userConfig.get(['ui', 'theme']) == 'theme--dark';
				const DistanceLimit = Eljs.limiter(2, 10, Math.round(runway.length / 300));
				let Opacity = 1;
				let Color = IsDark ? 'rgba(80,80,80,1)' : 'rgba(125,125,125,1)';
				if(wx != null) {
					if(wx.wind_speed < 5 || Math.abs(Eljs.MapCompareBearings(heading, wx.wind_heading)) > 90) {
						Color = IsDark ? 'rgba(30,125,30,1)' : 'rgba(30,200,30,1)';
						Opacity = 1;
					} else {
						Color = IsDark ? 'rgba(125,30,30,1)' : 'rgba(200,30,30,1)';
						Opacity = 0.5;
					}
				}

				const centerLineFeature = {
					type: 'Feature',
					properties: {
						opacity: 1 * Opacity,
						width: 3,
						color: Color
					},
					geometry: {
						type: 'MultiLineString',
						coordinates: [] as any
					},
				}

				const add = (feature :any, feat :any) => {
					if(feat.geometry.type == 'LineString'){
						feature.geometry.coordinates.push(feat.geometry.coordinates);
					} else {
						feat.geometry.coordinates.forEach((pos: any) => {
							feature.geometry.coordinates.push(pos);
						});
					}
				}

				const rwyThreshold = Eljs.MapOffsetPosition(runway.location[0], runway.location[1], (runway.length / 2), heading);
				const rwyExtensionOffsetLocation = Eljs.MapOffsetPosition(rwyThreshold[0], rwyThreshold[1], 1852 * DistanceLimit, heading); // runway.LengthFT * 10

				add(centerLineFeature, turf.greatCircle(turf.point(rwyThreshold), turf.point(rwyExtensionOffsetLocation)));

				const marksLineFeature = {
					type: 'Feature',
					properties: {
						opacity: 1 * Opacity,
						width: 3,
						color: Color
					},
					geometry: {
						type: 'MultiLineString',
						coordinates: [] as any
					},
				}

				const marksLineMinorFeature = {
					type: 'Feature',
					properties: {
						opacity: 0.5 * Opacity,
						width: 2,
						color: Color
					},
					geometry: {
						type: 'MultiLineString',
						coordinates: [] as any
					},
				}

				const mark = (feature :any, nm :number, size :number) => {
					const offsetNM = Eljs.MapOffsetPosition(rwyThreshold[0], rwyThreshold[1], 1852 * nm, heading);
					const offsetNMLeft = turf.point(Eljs.MapOffsetPosition(offsetNM[0], offsetNM[1], 100 * size, heading - 90));
					const offsetNMRight = turf.point(Eljs.MapOffsetPosition(offsetNM[0], offsetNM[1], 100 * size, heading + 90));
					add(feature, turf.greatCircle(offsetNMLeft, offsetNMRight));
				}

				if(DistanceLimit >= 1) { mark(marksLineMinorFeature, 1, 1); }
				if(DistanceLimit >= 2) { mark(marksLineMinorFeature, 2, 2); }
				if(DistanceLimit >= 3) { mark(marksLineMinorFeature, 3, 3); }
				if(DistanceLimit >= 4) { mark(marksLineMinorFeature, 4, 4); }
				if(DistanceLimit >= 5) { mark(marksLineFeature, 5, 5); }
				if(DistanceLimit >= 6) { mark(marksLineMinorFeature, 6, 6); }
				if(DistanceLimit >= 7) { mark(marksLineMinorFeature, 7, 7); }
				if(DistanceLimit >= 8) { mark(marksLineMinorFeature, 8, 8); }
				if(DistanceLimit >= 9) { mark(marksLineMinorFeature, 9, 9); }
				if(DistanceLimit >= 10) { mark(marksLineFeature, 10, 10); }

				this.map.sources.airportRunwayExt.data.features.push(centerLineFeature);
				this.map.sources.airportRunwayExt.data.features.push(marksLineFeature);
				this.map.sources.airportRunwayExt.data.features.push(marksLineMinorFeature);

				// Runway Names
				const airportFeature = {
					type: "Feature",
					properties: {
						title: name,
					},
					geometry: {
						type: "Point",
						coordinates: Eljs.MapOffsetPosition(rwyThreshold[0], rwyThreshold[1], (1852 * (DistanceLimit + 1)), heading),
					}
				};
				this.map.sources.airportRunwayExtNames.data.features.push(airportFeature);

			}

			airport.runways.forEach((runway :Runway) => {
				MakeRwy(airport.wx, runway, runway.heading - 180, runway.primary_name);
				MakeRwy(airport.wx, runway, runway.heading, runway.secondary_name);
			});
		},

		mapUncover() {
			this.$os.eventsBus.Bus.emit('os', { name: 'uncover', payload: true });
		},

		map_layers_reorder() {
			const order = {
				'airportRunwayExt': 'split_1_0',
				'airportRunwayExtNames': 'split_1_0',
				'airportRunwaysSolid': 'split_1_0',
				'airportRunwayMarkings': 'split_1_0',
				'airportRunwayNumbers': 'split_1_0',

				'airportMarkers': 'split_1_0',
				'airportMarkersLabel': 'split_2_1',
				'airportMarkersLabelDetailed': 'split_2_1',

				'airportParkings': 'split_1_0',
				'airportParkingNumbers': 'split_1_0',

				'terrainAvoidance': 'split_2_2',
				'terrainAvoidanceFill': 'split_0_0'
			}
			Object.keys(order).forEach(element => {
				const entry = order[element];
				this.$os.maps.main.map.moveLayer(element, entry);
			});

			//setTimeout(() => {
			//	console.log(this.$os.maps.main.map.getStyle().layers);
			//}, 5000);

		},

		mapTiltToggle(save = true) {
			if(!this.tracking.enabled || !this.sim_live) {
				this.mapbox.map.easeTo({
					pitch: Math.round(this.map.pitch) > 5 ? 0 : 60,
				});
			} else {
				this.tracking.tilted = !this.tracking.tilted;
				this.mapTrackUpdate();
			}

			if(save)
				this.state_save();
		},

		mapNorth(save = true) {
			this.tracking.north = !this.tracking.north;
			if(!this.tracking.enabled || !this.sim_live) {
				this.mapbox.map.easeTo({
					bearing: 0,
				});
			} else {
				this.mapTrackUpdate();
			}

			if(save)
				this.state_save();
		},

		mapTrackToggle(save = true) {
			this.tracking.enabled = !this.tracking.enabled;
			if(this.tracking.enabled) {
				this.mapTrackUpdate();
			}

			if(save)
				this.state_save();
		},

		mapAutoZoomToggle(save = true) {
			this.tracking.autozoom = !this.tracking.autozoom;
			this.mapTrackUpdate();

			if(save)
				this.state_save();
		},


		mapToggleApt(state? :boolean, save = true) {
			if(state === undefined)
				state = !this.display_layer.airports.icaos.enabled;

			this.display_layer.airports.icaos.enabled = state;
			this.$os.maps.set_layer(['airports', 'icaos', 'enabled'], state);

			this.update_airports();
			if(save)
				this.state_save();
		},

		mapToggleHill(state? :boolean, save = true) {
			if(state === undefined)
				state = !this.display_layer.hill;

			this.$os.routing.activeApp.theme_mode = state ? 'theme--dark' : null;
			this.mapbox.map.setLayoutProperty('hillshade-raster', 'visibility', state ? 'visible' : 'none');
			this.mapbox.map.setLayoutProperty('hillshade-shadow', 'visibility', state ? 'visible' : 'none');
			this.mapbox.map.setLayoutProperty('hillshade-highlight', 'visibility', state ? 'visible' : 'none');

			this.display_layer.hill = state;
			this.$os.maps.set_layer(['hill'], state);

			this.theme_update();
			if(save)
				this.state_save();
		},

		mapToggleSat(state? :boolean, save = true) {
			if(state === undefined)
				state = !this.display_layer.sat;

			this.$os.routing.activeApp.theme_mode = state ? 'theme--dark' : null;
			this.mapbox.map.setLayoutProperty('mapbox-satellite', 'visibility', state ? 'visible' : 'none');
			//this.mapbox.map.setLayoutProperty('mapbox-satellite', 'visibility', 'visible');
			//this.mapbox.map.setPaintProperty('mapbox-satellite', 'raster-opacity', 0.8);
			//this.mapbox.map.setPaintProperty('mapbox-satellite', 'raster-fade-duration', 200);
			//this.mapbox.map.setPaintProperty('mapbox-satellite', 'raster-opacity', 0.1);

			this.display_layer.sat = state;
			this.$os.maps.set_layer(['sat'], state);

			this.theme_update();
			if(save)
				this.state_save();
		},

		mapToggleWx(state? :boolean, save = true) {
			if(state === undefined)
				state = !this.display_layer.wx.radar.enabled;

			this.display_layer.wx.radar.enabled = state;
			this.$os.maps.set_layer( ['wx', 'radar', 'enabled'], state );

			this.mapUpdateRadar();
			if(save)
				this.state_save();
		},

		mapToggleSectionalUS(state? :boolean, save = true) {
			if(state === undefined)
				state = !this.display_layer.sectional.us.enabled;

			if(state){
				this.mapbox.map.addSource('sectional-us-tiles', {
					"type": "raster",
					"tiles": [
						"http://wms.chartbundle.com/tms/1.0.0/sec/{z}/{x}/{y}.png?origin=nw"
					],
					"tileSize": 256
				});
				this.mapbox.map.addLayer({
					"id": 'sectional-us-tiles',
					"type": "raster",
					"source": 'sectional-us-tiles',
					"minzoom": 0,
					"maxzoom": 22
				});
				this.mapbox.map.moveLayer('sectional-us-tiles', this.mapbox.map.getStyle().layers[9].id);
			} else {
				try{
					this.mapbox.map.removeLayer('sectional-us-tiles');
					this.mapbox.map.removeSource('sectional-us-tiles');
				} catch {}
			}

			this.display_layer.sectional.us.enabled = state;
			this.$os.maps.set_layer(['sectional', 'us', 'enabled'], state);

			this.theme_update();
			if(save)
				this.state_save();
		},

		mapToggleFleet(state? :boolean, save = true) {
			if(state === undefined)
				state = !this.display_layer.fleet.enabled;

			this.display_layer.fleet.enabled = state;
			this.$os.maps.set_layer(['fleet','enabled'], state);

			if(save)
				this.state_save();
		},

		mapGetBoundMatrix(coords: {
			nw: number[],
			se: number[],
		}) {

			const n = Eljs.ceil(coords.nw[1], 3);
			const s = Eljs.floor(coords.se[1], 3);
			const e = Eljs.ceil(coords.se[0], 3);
			const w = Eljs.floor(coords.nw[0], 3);

			let lonAt = w;
			let lonCount = 0;
			while(lonAt < e) {
				lonAt++;
				lonCount++;
			}

			let latAt = s;
			let latCount = 0;
			while(latAt < n) {
				latAt++;
				latCount++;
			}

			const result = w + '+' + lonCount + "|" + s + '+' + latCount;
			return result;
		},


		theme_update() {
			const layers = this.map.layers;

			const setLayersSat = () => {
				const layers = this.mapbox.map.getStyle().layers;
				const satDisable = [
					'aeroway-apron',
					'aeroway-taxiway',
					'aeroway-taxilines',
					'building',
					'tunnel-.*',
					'road-minor.*',
					'road-street.*',
					'road-secondary.*',
					'road-primary.*',
					'road-major.*',
					'road-motorway.*',
					'road-construction.*',
					'road-rail.*',
					'bridge-.*',
				];

				layers.forEach(layer => {
					satDisable.forEach(regex => {
						const result = new RegExp(regex, 'g').exec(layer.id);
						if(result) {
							this.mapbox.map.setLayoutProperty(layer.id, 'visibility', this.$os.maps.display_layer.sat || this.$os.maps.display_layer.sectional.us.enabled ? 'none' : 'visible');
						}
					});
				});

			};


			if(this.$os.maps.display_layer.sat && !this.$os.maps.display_layer.sectional.us.enabled) {
				this.theme = 'theme--dark';

				this.$os.theme.setThemeLayer({
					name: 'map_sat',
					bright: {
						status: StatusType.BRIGHT,
						nav: NavType.BRIGHT,
						shaded: true
					},
					dark: {
						status: StatusType.BRIGHT,
						nav: NavType.BRIGHT,
						shaded: true
					}
				})

				//this.mapbox.map.setPaintProperty('sky-day', 'sky-opacity', 0);
				//this.mapbox.map.setPaintProperty('sky-night', 'sky-opacity', 1);

				//this.map.setFog({ 'color': 'rgba(31, 31, 31, 1)' });

				layers.airportMarkersLabel.paint['text-color'] = this.$os.theme.colors.$ui_colors_dark_shade_5;
				layers.airportMarkersLabelDetailed.paint['text-color'] = this.$os.theme.colors.$ui_colors_dark_shade_5;
				layers.airportMarkersLabel.paint['text-halo-color'] = this.$os.theme.colors.$ui_colors_dark_shade_1;
				layers.airportMarkersLabelDetailed.paint['text-halo-color'] = this.$os.theme.colors.$ui_colors_dark_shade_1;

				layers.airportRunwaysSolid.paint["fill-color"][3] = "#00000f";

				layers.airportParkingNumbers.paint["text-halo-color"] = "#121212";
				layers.airportParkingNumbers.paint["text-color"] = this.$os.theme.colors.$ui_colors_dark_shade_5;
				layers.airportParkings.paint["line-color"]= this.$os.theme.colors.$ui_colors_dark_shade_5;

				layers.airportMarkers.paint['circle-stroke-color'] = this.$os.theme.colors.$ui_colors_dark_shade_4;
				layers.airportMarkers.paint['circle-stroke-opacity'] = 0.5;

			} else {
				this.$os.theme.setThemeLayer(null, 'map_sat');

				if(this.$os.userConfig.get(['ui', 'theme']) == 'theme--bright' || this.$os.maps.display_layer.sectional.us.enabled) {
					this.theme = 'theme--bright';

					layers.airportMarkersLabel.paint['text-color'] = this.$os.theme.colors.$ui_colors_bright_shade_5;
					layers.airportMarkersLabelDetailed.paint['text-color'] = this.$os.theme.colors.$ui_colors_bright_shade_5;
					layers.airportMarkersLabel.paint['text-halo-color'] = this.$os.theme.colors.$ui_colors_bright_shade_1;
					layers.airportMarkersLabelDetailed.paint['text-halo-color'] = this.$os.theme.colors.$ui_colors_bright_shade_1;

					layers.airportRunwaysSolid.paint["fill-color"][3] = "#c6c5c3";

					layers.airportParkingNumbers.paint["text-halo-color"] = "#e2e0d9";
					layers.airportParkingNumbers.paint["text-color"] = this.$os.theme.colors.$ui_colors_bright_shade_5;
					layers.airportParkings.paint["line-color"]= this.$os.theme.colors.$ui_colors_bright_shade_5;

					layers.airportMarkers.paint['circle-stroke-color'] = this.$os.theme.colors.$ui_colors_bright_shade_3;
					layers.airportMarkers.paint['circle-stroke-opacity'] = 0.8;

				} else {
					this.theme = 'theme--dark';

					layers.airportMarkersLabel.paint['text-color'] = this.$os.theme.colors.$ui_colors_dark_shade_4;
					layers.airportMarkersLabelDetailed.paint['text-color'] = this.$os.theme.colors.$ui_colors_dark_shade_4;
					layers.airportMarkersLabel.paint['text-halo-color'] = this.$os.theme.colors.$ui_colors_dark_shade_2;
					layers.airportMarkersLabelDetailed.paint['text-halo-color'] = this.$os.theme.colors.$ui_colors_dark_shade_2;

					layers.airportRunwaysSolid.paint["fill-color"][3] = "#00000f";

					layers.airportParkingNumbers.paint["text-halo-color"] = "#121212";
					layers.airportParkingNumbers.paint["text-color"] = this.$os.theme.colors.$ui_colors_dark_shade_5;
					layers.airportParkings.paint["line-color"]= this.$os.theme.colors.$ui_colors_dark_shade_5;

					layers.airportMarkers.paint['circle-stroke-color'] = this.$os.theme.colors.$ui_colors_dark_shade_3;
					layers.airportMarkers.paint['circle-stroke-opacity'] = 0.2;
				}
			}

			setLayersSat();
			this.update_airports(true);
		},

		state_load() {
			const loadedStore = localStorage.getItem('store_p42_map');
			if(loadedStore){
				try{
					const loaded_state = JSON.parse(loadedStore, Eljs.json_parser);

					try{ this.map.location = loaded_state.map.location; } catch { }
					try{ this.map.heading = loaded_state.map.heading; } catch { }
					try{ this.map.pitch = loaded_state.map.pitch; } catch { }
					try{ this.map.zoom = loaded_state.map.zoom; } catch { }

					try{ this.tracking.enabled = loaded_state.tracking.enabled; } catch { }
					try{ this.tracking.tilted = loaded_state.tracking.tilted; } catch { }
					try{ this.tracking.north = loaded_state.tracking.north; } catch { }
					try{ this.tracking.autozoom = loaded_state.tracking.autozoom; } catch { }

					try{ this.display_layer.pitch = loaded_state.display_layer.pitch; } catch { }
					try{ this.display_layer.north = loaded_state.display_layer.north; } catch { }
					try{ this.display_layer.autozoom = loaded_state.display_layer.autozoom; } catch { }

					Eljs.merge_deep(this.display_layer, loaded_state.display_layer);
					Eljs.merge_deep(this.$os.maps.display_layer, loaded_state.display_layer);

					//try{ this.$os.maps.display_layer.sat = loaded_state.layers.sat } catch { }
					//try{ this.$os.maps.display_layer.wx = loaded_state.layers.wx } catch { }
					//try{ this.$os.maps.display_layer.sectional = loaded_state.layers.sectional } catch { }
					//try{ this.$os.maps.display_layer.airports = loaded_state.layers.airports } catch { }

				} catch {
					console.log("Inavlid store for map");
				}
			}
		},

		state_save() {
			localStorage.setItem('store_p42_map', JSON.stringify({
				map: {
					location: this.map.location,
					heading: this.map.heading,
					pitch: this.map.pitch,
					zoom: this.map.zoom,
				},
				tracking: this.tracking,
				display_layer: this.display_layer,
			}));
		},


		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					if(this.mapbox) {
						this.update_airports();
					}
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					if(this.mapbox) {
						this.update_airports();
					}
					break;
				}
			}
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'app': {
					const app_in = wsmsg.payload.in as AppInfo;
					this.update_padding(app_in.padding as {
						top: number,
						left: number,
						right: number,
						bottom: number,
					});
					this.mapApplyPadding();
					//this.update_bounds();
					break;
				}
			}
		},

		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'covered': {
					this.covered = wsmsg.payload;
					//this.update_padding();
					//this.update_bounds();
					break;
				}
				case 'sidebar': {
					//this.update_padding();
					//this.mapApplyPadding();
					this.update_bounds();
					const app_in = this.$os.routing.activeApp as AppInfo;

					if(this.$os.system.sidebarOpen || !app_in.padding_full ) {
						this.update_padding(app_in.padding as {
							top: number,
							left: number,
							right: number,
							bottom: number,
						});
					} else {
						this.update_padding(app_in.padding_full as {
							top: number,
							left: number,
							right: number,
							bottom: number,
						});
					}

					this.mapApplyPadding();
					break;
				}
				//case 'reframe': {
				//	this.update_padding();
				//	this.mapApplyPadding();
				//	break;
				//}
			}
		},

		listener_map(wsmsg: any) {
			switch(wsmsg.name){
				case 'untrack': {
					if(this.tracking.enabled) {
						this.mapTrackToggle();
					}
					break;
				}
				case 'goto': {
					this.mapGoTo(wsmsg.type, wsmsg.payload);
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
					this.mapTrackUpdate();
					this.mapRenderTerrain();
					break;
				}
			}
		}

	},
	beforeMount() {
		this.state_load();
	},
	mounted() {

		const cache_apps = this.$os.apps.filter((x :AppInfo) => x.cache);
		cache_apps.forEach((app :AppInfo) => {
			this.app_cache_list.push(app.vendor + '_' + app.ident);
		});

		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('map', this.listener_map);
	},
	beforeDestroy() {
		this.state_save();
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.off('map', this.listener_map);
	}
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';

	.os_map {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
	}

	.map {
		&-container {
			&.covered {
				.map-container-controls {
					transform: translateX(200px);
					right: 0;
				}
			}

			&-cover {
				content: '';
				position: absolute;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				opacity: 0;
				//backdrop-filter: blur(0.001px);
				cursor: pointer;
				z-index: 2;
				transition: opacity 0.5s ease-out;//, backdrop-filter 0.4s ease-out;
				pointer-events: none;
				.theme--sat &,
				.theme--bright & {
					background-color: rgba($ui_colors_bright_shade_0, 0.8);
				}
				.theme--dark & {
					background-color: rgba($ui_colors_dark_shade_0, 0.8);
				}
				.covered > & {
					opacity: 1;
					//backdrop-filter: blur(3px);
					pointer-events: all;
					transition: opacity 0.3s ease-out;//, backdrop-filter 0.4s ease-out;
				}
			}

			/*
			.app-cover {
				content: '';
				position: absolute;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				pointer-events: none;
				opacity: 0;
				transition: opacity 0.4s ease-out, backdrop-filter 0.4s ease-out;
				.theme--bright & {
					background-color: rgba($ui_colors_bright_shade_2, 0.3);
				}
				.theme--dark & {
					background-color: rgba($ui_colors_dark_shade_0, 0.3);
				}
				&.is-visible {
					pointer-events: all;
					opacity: 1;
					backdrop-filter: blur(2px);
				}
			};
			*/

			&-controls {
				position: absolute;
				top: $status-size;
				right: $edge-margin;
				transition: right 0.4s ease-out, transform 0.4s ease-out;
			}

			&-background {
				position: absolute;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				transition: background 0.3s ease-out;

				&-image {
					position: absolute;
					top: 0;
					right: 0;
					bottom: 0;
					left: 0;
					opacity: 1;

					//background-image: url('/assets/wallpapers/1.jpg');
					background-size: cover;
					filter: blur(30px);
					transform: scale(1.1);
				}

				&.theme--bright {
					background-color: $ui_colors_bright_shade_2;
				}
				&.theme--dark {
					background-color: $ui_colors_dark_shade_1;
				}
			}
		}

		&-control {
			display: flex;
			align-items: center;
			background-position: center;
			background-repeat: no-repeat;
			box-sizing: border-box;
			height: 34px;
			width: 38px;

			.theme--bright &,
			&.theme--bright {
				&-apt {
					span {
						&::before {
							background-color: $ui_colors_bright_button_info;
							border-color: $ui_colors_bright_shade_0;
						}
					}
				}
				&-3d {
					&-pane {
						span {
							border-color: rgba($ui_colors_bright_shade_5, 0.3);
						}
					}
				}
				&-sec {
					background-image: url(../../../sys/assets/icons/dark/sectionals.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/sectionals.svg);
					}
				}
				&-wx {
					background-image: url(../../../sys/assets/icons/dark/wx.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/wx.svg);
					}
				}
				&-sat {
					background-image: url(../../../sys/assets/icons/dark/sat.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/sat.svg);
					}
				}
				&-autozoom {
					background-image: url(../../../sys/assets/icons/dark/autozoom.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/autozoom.svg);
					}
				}
				&-north {
					//background-image: url(../../../sys/assets/icons/dark/north.svg);
					//&.info {
					//	background-image: url(../../../sys/assets/icons/bright/north.svg);
					//}
					&-compass {
						& > div {
							&:after {
								background-image: linear-gradient(rgba($ui_colors_bright_shade_5, 0) 0%, rgba($ui_colors_bright_shade_5, 0.2) 20%, rgba($ui_colors_bright_shade_5, 0) 50%);
							}
						}
					}
				}
				&-crs {
					background-image: url(../../../sys/assets/icons/dark/north.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/north.svg);
					}
				}
				&-track {
					background-image: url(../../../sys/assets/icons/dark/track.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/track.svg);
					}
				}
				&-elev {
					background-image: url(../../../sys/assets/icons/dark/terrain_avoidance.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/terrain_avoidance.svg);
					}
				}
				&-hill {
					background-image: url(../../../sys/assets/icons/dark/hillshade.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/hillshade.svg);
					}
				}
				&-fleet {
					background-image: url(../../../sys/assets/icons/dark/fleet.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/fleet.svg);
					}
				}
				&-poi {
					.icon {
						background-image: radial-gradient(closest-side, rgba($ui_colors_bright_button_gold, 1) 0%, rgba($ui_colors_bright_button_gold, 0) 100%);
					}
				}
			}

			.theme--dark &,
			&.theme--dark {
				&-apt {
					span {
						&::before {
							background-color: $ui_colors_dark_button_info;
							border-color: $ui_colors_dark_shade_5;
						}
					}
				}
				&-3d {
					&-pane {
						span {
							border-color: rgba($ui_colors_dark_shade_5, 0.3);
						}
					}
				}
				&-sec {
					background-image: url(../../../sys/assets/icons/bright/sectionals.svg);
				}
				&-wx {
					background-image: url(../../../sys/assets/icons/bright/wx.svg);
				}
				&-sat {
					background-image: url(../../../sys/assets/icons/bright/sat.svg);
				}
				&-autozoom {
					background-image: url(../../../sys/assets/icons/bright/autozoom.svg);
				}
				&-north {
					//background-image: url(../../../sys/assets/icons/bright/north.svg);
					&-compass {
						& > div {
							&:after {
								background-image: linear-gradient(rgba($ui_colors_dark_shade_5, 0) 0%, rgba($ui_colors_dark_shade_5, 0.2) 20%, rgba($ui_colors_dark_shade_5, 0) 50%);
							}
						}
					}
				}
				&-crs {
					background-image: url(../../../sys/assets/icons/bright/north.svg);
				}
				&-track {
					background-image: url(../../../sys/assets/icons/bright/track.svg);
				}
				&-elev {
					background-image: url(../../../sys/assets/icons/bright/terrain_avoidance.svg);
				}
				&-hill {
					background-image: url(../../../sys/assets/icons/bright/hillshade.svg);
				}
				&-fleet {
					background-image: url(../../../sys/assets/icons/bright/fleet.svg);
				}
			}

			&-apt {
				position: relative;
				span {
					display: flex;
					flex-direction: column;
					align-items: center;
					font-size: 10px;
					&::before {
						display: block;
						content: '';
						width: 6px;
						height: 6px;
						margin-top: 4px;
						margin-bottom: -2px;
						border-radius: 50%;
						background-color: $ui_colors_bright_button_info;
						border: 2px solid transparent;
						border-color: $ui_colors_bright_shade_0;
					}
				}
			}
			&-3d {
				&-pane {
					perspective: 100px;
					span {
						display: block;
						will-change: transform;
						border: 1px solid #000;
						border-radius: 3px;
						padding: 0 3px;
					}
				}
			}
			&-sec {
				position: relative;
				background-size: 1.8em;
				span {
					font-size: 10px;
					position: absolute;
					bottom: 4px;
					left: 6px;
				}
			}
			&-wx {
				background-size: 1.4em;
			}
			&-sat {
				background-size: 1.4em;
			}
			&-north {
				//background-size: 1.4em;
				overflow: hidden;
				&-compass {
					position: relative;
					transform: rotateZ(0deg);
					margin-top: 84px;
					& > div {
						position: absolute;
						top: 50%;
						left: 50%;
						height: 110px;
						& > span {
							display: block;
							line-height: 1;
							&:last-child {
								opacity: 0.5;
								font-size: 0.75em;
							}
						}
						&:after {
							content: '';
							position: absolute;
							top: 0;
							bottom: 50%;
							left: 50%;
							width: 1px;
							transform: translateX(-50%) rotateZ(22.5deg);
							transform-origin: bottom;
						}
						&:nth-child(1) {
							transform: translate(-50%, -50%) rotateZ(0deg);
						}
						&:nth-child(2) {
							transform: translate(-50%, -50%) rotateZ(45deg);
						}
						&:nth-child(3) {
							transform: translate(-50%, -50%) rotateZ(90deg);
						}
						&:nth-child(4) {
							transform: translate(-50%, -50%) rotateZ(135deg);
						}
						&:nth-child(5) {
							transform: translate(-50%, -50%) rotateZ(180deg);
						}
						&:nth-child(6) {
							transform: translate(-50%, -50%) rotateZ(225deg);
						}
						&:nth-child(7) {
							transform: translate(-50%, -50%) rotateZ(270deg);
						}
						&:nth-child(8) {
							transform: translate(-50%, -50%) rotateZ(315deg);
						}
					}
				}
			}
			&-crs {
				background-size: 1.4em;
			}
			&-track {
				background-size: 1.4em;
			}
			&-elev {
				background-size: 1.6em;
			}
			&-hill {
				background-size: 1.6em;
			}
			&-fleet {
				background-size: 2em;
			}
		}
	}
</style>

<style lang="scss">
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
	.os_map {
		.theme--sat &
		.theme--bright & {
			.mapboxgl-ctrl-attrib {
				background-color: rgba($ui_colors_bright_shade_0,0.5);
				& * {
					color: rgba($ui_colors_bright_shade_4,0.5);
				}
			}
		}

		.theme--dark & {
			.mapboxgl-ctrl-attrib {
				background-color: rgba($ui_colors_dark_shade_0,0.5);
				& * {
					color: rgba($ui_colors_dark_shade_4,0.5);
				}
			}
		}

		.mapboxgl-canvas {
			cursor: default;
		}

		.mapboxgl-ctrl-bottom-left {
			margin-bottom: 2px;
			margin-right: 0;
			left: auto;
			right: 14px;
		}
		.mapboxgl-ctrl-bottom-right {
			margin-bottom: 12px;
			margin-right: 115px;
		}
		.mapboxgl-compact {
			margin: 0;
		}
		.mapboxgl-ctrl-logo {
			transition: opacity 0.3s ease-out;
			.map-container.covered & {
				opacity: 0;
				pointer-events: none;
			}
		}
		.mapboxgl-ctrl-attrib {
			border-radius: 8px;
			//backdrop-filter: blur(3px);
			font-size: 10px;
			transition: opacity 0.3s ease-out;
			.map-container.covered & {
				opacity: 0;
				pointer-events: none;
			}
		}
	}
</style>