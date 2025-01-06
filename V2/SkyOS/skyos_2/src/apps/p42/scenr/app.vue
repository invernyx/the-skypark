<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame" ref="app-frame">

			<split_view>
				<sidebar class="large" :statusspaced="true">
					<StartupBar :app="this" v-if="!state.project.adventure && !state.project.scene"/>
					<AdventureBar :app="this" :adventure="state.project.adventure" v-if="state.project.adventure"/>
				</sidebar>
				<!-- https://soal.github.io/vue-mapbox/guide/ -->
				<MapboxFrame
					v-if="state.ui.map.load"
					:class="[{ 'map-hidden': !state.ui.map.loaded }]"
					ref="map"
					id="p42_scenr_map_main"
					:mstyle="state.ui.map.mapStyle"
					:app="app"
					:mapTheme="state.ui.map.theme"
					:hasFog="true"
					@load="mapLoaded"
					>
					<template>

						<MglMarker v-if="$root.$data.state.services.simulator.live" :coordinates="[$root.$data.state.services.simulator.location.Lon, $root.$data.state.services.simulator.location.Lat]">
							<div class="map_marker map_marker_position" slot="marker" @click="copyToClipboard($root.$data.state.services.simulator.location.Lat + ', ' + $root.$data.state.services.simulator.location.Lon)">
								<div>
								</div>
							</div>
						</MglMarker>

						<div v-if="state.project.adventure">
							<MglMarker
								v-for="(sit, index) in state.project.adventure.Situations.filter(x => x.SituationType == 'Geo')"
								v-bind:key="index"
								:coordinates="[sit.Lon, sit.Lat]">
								<div class="map_marker map_marker_location" slot="marker" data-type="situation" :data-index="index">
									<div>
									</div>
								</div>
							</MglMarker>
						</div>


						<MglGeojsonLayer
							layerId="situationBoundary"
							sourceId="situationBoundary"
							:source="state.ui.map.sources.situationBoundary"
							:layer="state.ui.map.layers.situationBoundary" />

						<div v-if="state.ui.mapControlMode.type == 'situation_bounds'">
							<MglMarker
								v-for="(boundaryNode, index) in state.ui.map.sources.situationBoundary.data.features[state.ui.mapControlMode.reference.Index].geometry.coordinates[0].slice(0, -1)"
								v-bind:key="index"
								:coordinates="boundaryNode"
								@click="onMarkerClick"
								@dragstart="onMarkerDragStart"
								@drag="onMarkerDrag"
								:draggable="true">
								<div class="map_marker map_marker_grab" slot="marker" data-type="bounds" :data-index="index" @mousedown="onMarkerMouseDown">
									<div>
									</div>
								</div>
							</MglMarker>
						</div>

						<MglGeojsonLayer
							layerId="situationAirports"
							sourceId="situationAirports"
							:source="state.ui.map.sources.situationAirports"
							:layer="state.ui.map.layers.situationAirports"
						/>

						<MglGeojsonLayer
							layerId="situationRange"
							sourceId="situationRange"
							:source="state.ui.map.sources.situationRange"
							:layer="state.ui.map.layers.situationRange" />

						<MglGeojsonLayer
							layerId="routes"
							sourceId="routes"
							:source="state.ui.map.sources.routes"
							:layer="state.ui.map.layers.routes"
						/>

						<MglGeojsonLayer
							layerId="situationAirportsLabel"
							sourceId="situationAirports"
							:source="state.ui.map.sources.situationAirports"
							:layer="state.ui.map.layers.situationAirportsLabel"
						/>

						<MglGeojsonLayer
							layerId="poisNode"
							sourceId="poisNode"
							:clearSource="false"
							:source="state.ui.map.sources.poisNode"
							:layer="state.ui.map.layers.poisNode" />

						<MglGeojsonLayer
							layerId="poisNodeLabel"
							sourceId="poisNode"
							:source="state.ui.map.sources.poisNode"
							:layer="state.ui.map.layers.poisNodeLabel" />

						<!--
						<div v-for="(situation, index) in state.ui.map.markers.situationAirport" v-bind:key="index">
							<MglMarker
								v-for="(airport, index) in situation.list"
								v-bind:key="index"
								:coordinates="airport.Location">
								<div class="map_marker map_marker_grab" slot="marker" :data-index="index">
									<div>
									</div>
								</div>
							</MglMarker>
						</div>
						-->




					</template>
				</MapboxFrame>
			</split_view>

			<MapImageFrame :app="this" :d="state.project.mapimage" v-if="state.project.mapimage"/>

		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "./../../../sys/foundation/app_bundle"
import * as turf from '@turf/turf';
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import { MglMap, MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import '@/sys/libraries/mapbox-vue/mapbox-gl.css';
import Eljs from '@/sys/libraries/elem';
import Notification from '@/sys/models/notification'

import AdventureProj from './classes/adventure';
import SceneProj from './classes/adventure';

import StartupBar from './components/startup.vue';
import AdventureBar from './components/adventure/adventure.vue';
import ContractDetailed from "@/sys/components/contracts/contract_detailed.vue"

import MapImageFrame from './components/mapimage.vue';

export default Vue.extend({
	name: "p42_scenr",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		MapboxFrame,
		MglMap,
		MglMarker,
		MglNavigationControl,
		MglGeojsonLayer,
		StartupBar,
		AdventureBar,
		ContractDetailed,
		MapImageFrame,
	},
	data() {
		return {
			ready: true,
			state: {
				project: {
					mapimage: null as any,
					adventure: null as AdventureProj,
					scene: null as SceneProj,
					contract: null as any,
					template: null as any,
					cargoTags: [],
				},
				ui: {
					loaded: false,
					mapControlMode: {
						type: null,
						reference: null,
					},
					map: {
						load: false,
						loaded: false,
						location: [0,0],
						zoom: 2,
						heading: 0,
						pitch: 0,
						padding: { left: 50, top: 50, right: 50, bottom: 50 },
						moveTimeout: null,
						lineAnimationInterval: null,
						mapStyle: 'mapbox://styles/biarzed/ckq9jto5t20ry17qm07q2de8s',
						interactions: {
							scheduleClearSelection: false,
							selectCancel: false
						},
						sources: {
							situationBoundary: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							situationRange: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							situationAirports: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							routes: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							poisNode: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
						},
						layers: {
							situationBoundary: {
								id: 'situationBoundary',
								type: 'line',
								source: 'situationBoundary',
								paint: {
									'line-color': '#F55',
									'line-width': 3,
									'line-opacity': 0.4
								},
								filter: ['==', '$type', 'Polygon']
							},
							situationAirportsLabel: {
								id: "situationAirportsLabel",
								type: 'symbol',
								source: 'situationAirports',
								minzoom: 8,
								layout: {
									"text-field": ['get', 'title'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									'text-variable-anchor': ['left'], // 'top', 'bottom', 'left', 'right'
									'text-radial-offset': 0.5,
									"text-size": 12,
								},
								paint: {
									'text-color': "#FFF",
									"text-opacity": 1
								}
							},
							situationAirports: {
								id: "situationAirports",
								type: 'circle',
								source: 'situationAirports',
								paint: {
									'circle-color': '#FFF',
									'circle-radius': 2,
									'circle-opacity': [
										"interpolate",
										["linear"],
										["zoom"],
										8, 0.5,
										10, 1
									]
								}
							},
							situationRange: {
								type: "fill",
								source: "situationRange",
								paint: {
									'fill-color': '#FFFFFF',
									'fill-opacity': 0.2
								}
							},
							routes: {
								id: 'routes',
								type: 'line',
								source: 'routes',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': '#FFF',
									'line-width': 3,
									'line-opacity': 0.2
								},
								filter: ['in', '$type', 'LineString']
							},
							poisNode: {
								id: "poisNode",
								type: 'circle',
								source: 'poisNode',
								paint: {
									'circle-color': '#cca34d',
									'circle-pitch-alignment': 'map',
									'circle-stroke-color': '#cca34d',
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
							poisNodeLabel: {
								id: "poisNodeLabel",
								type: 'symbol',
								source: 'poisNode',
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
									'text-color': "#cca34d",
									'text-halo-color': '#222222',
									'text-halo-width': 2,
								}
							},
						},
					}
				}
			}
		}
	},
	beforeMount() {

		this.$os.setConfig(['ui', 'theme'], 'theme--bright')

		this.getCargoTags();
		if(this.app.loaded_state != null){
			this.state.ui.map.location = this.app.loaded_state.ui.map.location;
			this.state.ui.map.zoom = this.app.loaded_state.ui.map.zoom;
			this.state.ui.map.heading = this.app.loaded_state.ui.map.heading;
			this.state.ui.map.pitch = this.app.loaded_state.ui.map.pitch;
		}
		this.$on('interaction', this.interaction);
		this.$on("adventuretemplate:reset", (e: any) => {
			this.$os.UntrackedMap['p42_scenr_map_main'].map.getSource('routes').setData(this.state.ui.map.sources.routes.data);
		});
		this.$on("adventure:situation:update", (e: any) => {
			this.mapRefresh();
		});
	},
	methods: {
		mapLoaded(map: any) {
			this.$os.TrackMapLoad(this.$route.path);

			window.requestAnimationFrame(() => {
				setTimeout(() => {
					if(this.$root.$data.state.services.simulator.live){
						this.$os.UntrackedMap['p42_scenr_map_main'].map.setCenter([this.$root.$data.state.services.simulator.location.Lon, this.$root.$data.state.services.simulator.location.Lat]);
					} else {
						this.$os.UntrackedMap['p42_scenr_map_main'].map.setCenter([this.state.ui.map.location[0], this.state.ui.map.location[1]]);
					}
					this.state.ui.map.loaded = true;
					this.mapSetLineAnimation();
					this.$emit('loaded');
				}, 100)

				this.$os.UntrackedMap['p42_scenr_map_main'].map.setZoom(this.state.ui.map.zoom);
				this.$os.UntrackedMap['p42_scenr_map_main'].map.setBearing(this.state.ui.map.heading);
				this.$os.UntrackedMap['p42_scenr_map_main'].map.setPitch(this.state.ui.map.pitch);
				this.mapMoveDone();

				this.$os.UntrackedMap['p42_scenr_map_main'].map.setLayoutProperty('satellite', 'visibility', 'visible');
				this.$os.UntrackedMap['p42_scenr_map_main'].map.setPaintProperty('satellite', 'raster-opacity', 0.3);
				this.$os.UntrackedMap['p42_scenr_map_main'].map.setPaintProperty('satellite', 'raster-fade-duration', 200);

				['moveend', 'zoomend', 'rotateend', 'pitchend'].forEach((ev) => {
					this.$os.UntrackedMap['p42_scenr_map_main'].map.on(ev, () => {
						clearTimeout(this.state.ui.map.moveTimeout);
						this.state.ui.map.moveTimeout = setTimeout(() => {
							this.mapMoveDone();
						}, 300);
					});
				});

				['mousedown', 'touchstart'].forEach((ev) => {
					this.$os.UntrackedMap['p42_scenr_map_main'].map.on(ev, () => {
						this.state.ui.map.interactions.selectCancel = false;
						this.state.ui.map.interactions.scheduleClearSelection = true;
					});
					this.$os.UntrackedMap['p42_scenr_map_main'].map.on(ev, 'situationBoundary', (e: any) => {
						const features = this.$os.UntrackedMap['p42_scenr_map_main'].map.queryRenderedFeatures(e.point);

					});
				});

				['move', 'zoom', 'rotate', 'pitch'].forEach((ev) => {
					this.$os.UntrackedMap['p42_scenr_map_main'].map.on(ev, () => {
						if(!this.state.ui.map.interactions.selectCancel){
							this.state.ui.map.interactions.selectCancel = true;
						}
						if(this.state.ui.map.interactions.scheduleClearSelection){
							this.state.ui.map.interactions.scheduleClearSelection = false;
						}
					});
				});

				['mouseup', 'touchend'].forEach((ev) => {
					this.$os.UntrackedMap['p42_scenr_map_main'].map.on(ev, (e: any) => {

						this.mapClick(e);

						if(e.originalEvent.button == 0){
							const features = this.$os.UntrackedMap['p42_scenr_map_main'].map.queryRenderedFeatures(e.point);
							if(!this.state.ui.map.interactions.selectCancel){
								if(this.state.ui.mapControlMode.type) {
									switch(this.state.ui.mapControlMode.type) {
										case "situation_bounds": {
											const path = Eljs.getDOMParents(e.originalEvent.target) as HTMLElement[];
											if(!path[0].className.includes('marker')) {
												const index = this.state.ui.mapControlMode.reference.Index;
												const coords = this.state.ui.map.sources.situationBoundary.data.features[index].geometry.coordinates[0];
												if(coords.length == 0){
													coords.push([e.lngLat.lng, e.lngLat.lat]);
													coords.push([e.lngLat.lng, e.lngLat.lat]);
												} else {
													if(coords.length > 2){
														let found = false;
														coords.forEach((coord :any, index :number) => {
															if(index > 0 && !found) {
																const bearingToCur = Eljs.GetBearing(e.lngLat.lat, e.lngLat.lng, coords[index][1], coords[index][0]);
																const bearingToPrev = Eljs.GetBearing(e.lngLat.lat, e.lngLat.lng, coords[index - 1][1], coords[index - 1][0]);
																const bearingDif = Math.abs(Eljs.MapCompareBearings(bearingToCur, bearingToPrev));
																if(bearingDif > 90) {
																	found = true;
																	coords.splice(index, 0, [e.lngLat.lng, e.lngLat.lat]);
																}
															}
														});
														if(!found){
															coords.splice(coords.length - 1, 0, [e.lngLat.lng, e.lngLat.lat]);
														}
													} else {
														coords.splice(coords.length - 1, 0, [e.lngLat.lng, e.lngLat.lat]);
													}

												}
											}
											break;
										}
									}
								}
							}
						}

						this.state.ui.map.interactions.scheduleClearSelection = false;
					});
				});

				if(!this.state.ui.loaded){
					this.state.ui.loaded = true;
				}
			});
		},
		mapMoveDone() {
			if(this.state.ui.map.loaded && this.state.ui.map.load){
				const centerLocation = this.$os.UntrackedMap['p42_scenr_map_main'].map.getCenter();
				this.state.ui.map.location = [centerLocation.lng, centerLocation.lat];
				this.state.ui.map.heading = this.$os.UntrackedMap['p42_scenr_map_main'].map.getBearing();
				this.state.ui.map.pitch = this.$os.UntrackedMap['p42_scenr_map_main'].map.getPitch();
				this.state.ui.map.zoom = this.$os.UntrackedMap['p42_scenr_map_main'].map.getZoom();
			}
		},
		mapSetLineAnimation() {
			const dashLength = 10;
			const gapLength = 3;
			const steps = 15;
			const stepOffset = 0.5;
			const dashSteps = steps * dashLength / (gapLength + dashLength);
			const gapSteps = steps - dashSteps;
			let step = steps;
			clearInterval(this.state.ui.map.lineAnimationInterval);
			this.state.ui.map.lineAnimationInterval = setInterval(() => {
				step -= stepOffset;
				if (step <= 0) step = steps - stepOffset;

				let t, a, b, c, d;
				if (step < dashSteps) {
					t = step / dashSteps;
					a = (1 - t) * dashLength;
					b = gapLength;
					c = t * dashLength;
					d = 0;
				} else {
					t = (step - dashSteps) / (gapSteps);
					a = 0;
					b = (1 - t) * gapLength;
					c = dashLength;
					d = t * gapLength;
				}

				this.$os.UntrackedMap['p42_scenr_map_main'].map.setPaintProperty("routes", "line-dasharray", [a, b, c, d]);
			}, 100);
		},
		mapClick(ev :any) {
			this.$emit('map:click', ev);
		},
		mapDrawGeoSitRange() {
			var sits = this.state.project.adventure.Situations.filter(x => x.SituationType == 'Geo');
			this.state.ui.map.sources.situationRange.data.features = [];
			sits.forEach((Sit) => {
				if(Sit.TriggerRange) {
					const c = turf.circle([Sit.Lon, Sit.Lat], Sit.TriggerRange * 1.852, { steps: 90, units: 'kilometers' });
					this.state.ui.map.sources.situationRange.data.features.push(c);
				}
			});
		},

		onMarkerMouseDown(e: any) {
			const type = Eljs.getData(e.target, 'type');
			switch(type) {
				case 'bounds': {
					if(e.button == 2){
						const index = parseInt(Eljs.getData(e.target, 'index'));
						const coords = this.state.ui.map.sources.situationBoundary.data.features[this.state.ui.mapControlMode.reference.Index].geometry.coordinates;
						if(index == 0){
							coords[0].splice(0, 1);
							if(coords[0].length == 1) {
								coords[0].splice(0, 1);
							} else {
								coords[0][coords[0].length-1] = coords[0][0];
							}
						} else {
							coords[0].splice(index, 1);
						}
						this.mapRefresh();
					}
					break;
				}
			}
		},
		onMarkerClick(e: any) {

		},
		onMarkerDragStart(e: any) {
			const type = Eljs.getData(e.marker.getElement(), 'type');
			switch(type) {
				case 'bounds': {
					this.state.ui.map.interactions.selectCancel = true;
					this.state.ui.map.interactions.scheduleClearSelection = false;
				}
			}
		},
		onMarkerDrag(e: any) {
			const type = Eljs.getData(e.marker.getElement(), 'type');
			switch(type) {
				case 'bounds': {
					const newLoc = e.mapboxEvent.target.getLngLat();
					const index = parseInt(Eljs.getData(e.marker.getElement(), 'index'));
					const coords = this.state.ui.map.sources.situationBoundary.data.features[this.state.ui.mapControlMode.reference.Index].geometry.coordinates;
					coords[0][index] = [newLoc.lng, newLoc.lat];
					if(index == 0) {
						coords[0][coords[0].length-1] = [newLoc.lng, newLoc.lat];
					}
					this.$os.UntrackedMap['p42_scenr_map_main'].map.getSource('situationBoundary').setData(this.state.ui.map.sources.situationBoundary.data);
					break;
				}
			}
		},
		copyToClipboard(text) {
			var dummy = document.createElement("textarea");
			document.body.appendChild(dummy);
			dummy.value = text;
			dummy.select();
			document.execCommand("copy");
			document.body.removeChild(dummy);

			this.$os.addNotification(new Notification({
				UID: Eljs.getNumGUID(),
				Type: 'Status',
				Title: 'Copied to clipboard',
				Message: text,
			}), false);
		},

		interaction(data :any) {
			const commandSpl = data.cmd.split(':');
			switch(commandSpl[0]) {
				case "adventure": {
					switch(commandSpl[1]) {
						case "create": {
							this.state.project.adventure = new AdventureProj(this);
							break;
						}
						case "load": {
							this.$root.$data.services.api.SendWS('scenr:adventuretemplate:load', { File: data.file, Type: data.type }, (proj: any) => {

								const dateFormat = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{0,3}Z$/;

								function reviver(key: any, value: any) {
									if (typeof value === 'string' && dateFormat.test(value)) {
										return new Date(value);
									}
									return value;
								}

								const obj = JSON.parse( JSON.stringify(proj.payload) as string, reviver);
								this.state.project.adventure = new AdventureProj(this, obj);
								this.mapDrawGeoSitRange();

							});
							break;
						}
						case "open": {
							this.loadAdventure();
							break;
						}
						case "rename": {
							this.$root.$data.services.api.SendWS('scenr:adventuretemplate:rename', { File: data.file, NewFile: data.newFile, Type: data.type }, (proj: any) => {});
							break;
						}
						case "delete": {
							this.$root.$data.services.api.SendWS('scenr:adventuretemplate:delete', { File: data.file, Type: data.type }, (proj: any) => {
								data.callback();
							});
							break;
						}
						case "close": {
							this.state.project.adventure = null;
							this.state.ui.mapControlMode.type = null;
							this.state.ui.mapControlMode.reference = null;
							this.state.ui.map.sources.situationBoundary.data.features = [];
							this.state.ui.map.sources.situationAirports.data.features = [];
							this.state.ui.map.sources.routes.data.features = [];
							this.state.ui.map.sources.poisNode.data.features = [];
							this.state.ui.map.sources.situationRange.data.features = [];
							break;
						}
						case "contract": {
							this.state.project.contract = data.contract;
							this.state.project.template = data.template;
							break;
						}
						case "modal": {

							// Return if already present
							if(this.$root.$data.state.ui.modals.queue.length) {
								if(this.$root.$data.state.ui.modals.queue[0].type != 'contract') {
									return;
								}
							}

							this.$os.modalPush({
								type: 'contract',
								data: {
									App :this.app,
									Selected: {
										Contract: this.state.project.contract,
										Template: this.state.project.template
									},
								},
								func: () => {

								}
							});
							break;
						}
					}
					break;
				}
				case "scene": {
					switch(commandSpl[1]) {
						case "create": {
							this.state.project.scene = new SceneProj(this);
							break;
						}
					}
					break;
				}
				case "map": {
					switch(commandSpl[1]) {
						case "situations": {
							this.mapDrawGeoSitRange();
							break;
						}
						case "pois": {
							this.mapRenderPOIs();
							break;
						}
						case "move": {
							const opt = {
								center: data.location,
								duration: 200,
								easing: (t :number) => {
									return 1 - Math.pow(1 - t, 5);
								},
							};
							this.$os.UntrackedMap['p42_scenr_map_main'].map.flyTo(opt);
							break;
						}
						case "airports": {
							switch(commandSpl[2]) {
								case "query": {

									this.$root.$data.services.api.SendWS('scenr:adventuretemplate:query:airports', { Adventure: this.state.project.adventure }, (returnedData: any) => {

										const sourceData = this.state.ui.map.sources.situationAirports.data;
										const Existing = sourceData.features.filter(x => x.properties.id == 'apt' + data.situation );
										Existing.forEach(element => {
											const Index = sourceData.features.indexOf(element);
											sourceData.features.splice(Index, 1);
										});

										returnedData.payload.list.forEach((airport :any) => {
											const feature = {
												type: "Feature",
												properties: {
													title: airport.ICAO,
													id: 'apt' + data.situation,
													//tag: airport.ICAO
												},
												geometry: {
													type: "Point",
													coordinates: airport.Location,
												}
											};
											sourceData.features.push(feature);
										});

									});

									break;
								}
							}
							break;
						}
						case "draw": {
							if(data.state){
								if(this.state.ui.mapControlMode.type != null){
									this.interaction({ cmd: 'map:draw', state: false});
								}
								switch(data.type) {
									case "situation_bounds": {
										this.state.ui.mapControlMode.type = "situation_bounds";
										this.state.ui.mapControlMode.reference = data.sit;
										break;
									}
								}
							} else {
								switch(this.state.ui.mapControlMode.type) {
									case "situation_bounds": {
										this.state.ui.mapControlMode.reference.Boundaries = this.state.ui.map.sources.situationBoundary.data.features[this.state.ui.mapControlMode.reference.Index].geometry.coordinates[0];
										break;
									}
								}
								this.state.ui.mapControlMode.type = null;
								this.state.ui.mapControlMode.reference = null;
							}
							break;
						}
						case "images": {
							switch(commandSpl[2]) {
								case "open": {
									this.state.project.mapimage = {

									}
									break;
								}
								case "close": {
									this.state.project.mapimage = null;
									break;
								}
							}
							break;
						}
					}
					break;
				}
			}
		},

		getCargoTags() {
			this.$root.$data.services.api.SendWS('scenr:cargotags', {}, (cargoTagsData: any) => {
				this.state.project.cargoTags = cargoTagsData.payload;
			});
		},

		mapRefresh() {
			if(this.$os.UntrackedMap['p42_scenr_map_main']) {
				this.$os.UntrackedMap['p42_scenr_map_main'].map.getSource('situationBoundary').setData(this.state.ui.map.sources.situationBoundary.data);
				this.$os.UntrackedMap['p42_scenr_map_main'].map.getSource('situationAirports').setData(this.state.ui.map.sources.situationAirports.data);
			}
		},
		mapRenderPOIs() {
			this.state.ui.map.sources.poisNode.data.features = [];
			this.state.project.adventure.POIs.forEach(poi => {
				const runwayFeature = {
					type: "Feature",
					properties: {
						title: poi[2],
					},
					geometry: {
						type: "Point",
						coordinates: [poi[0],poi[1]],
					}
				};
				this.state.ui.map.sources.poisNode.data.features.push(runwayFeature);
			});
		},

		loadAdventure() {
			const self = this;
			const dlAnchorElem = document.createElement('input');
			dlAnchorElem.style.display = 'none';
			dlAnchorElem.setAttribute('type', 'file');
			dlAnchorElem.setAttribute('accept', '.p42adv');
			dlAnchorElem.onchange = (evt: Event) => {
				try {
					const files = (evt.target as HTMLInputElement).files;
					if (!files.length) {
						return;
					}
					const file = files[0];
					const reader = new FileReader();
					reader.onload = (event: any) => {
						const dateFormat = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{0,3}Z$/;

						function reviver(key: any, value: any) {
							if (typeof value === 'string' && dateFormat.test(value)) {
								return new Date(value);
							}
							return value;
						}

						const obj = JSON.parse(event.target.result as string, reviver);

						this.state.project.adventure = new AdventureProj(this, obj);
						this.state.project.adventure.File = file.name.replace('.p42adv', '');
					};
					reader.readAsText(file);
				} catch (err) {
					console.error(err);
				}
				document.body.removeChild(dlAnchorElem);
			};
			document.body.appendChild(dlAnchorElem);
			dlAnchorElem.click();
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.getCargoTags();
					break;
				}
				case 'disconnect': {

					break;
				}
				case 'adventure': {
					this.$ContractMutator.Event(wsmsg, this.state.project.contract, this.state.project.template);
					break;
				}
				case 'scenr': {
					switch(wsmsg.name[1]){
						case 'mapimages': {
							switch(wsmsg.name[2]){
								case 'getnext': {

									break;
								}
							}
							break;
						}
						case 'adventuretemplate': {
							switch(wsmsg.name[2]){
								case 'save': {
									break;
								}
								case 'test': {
									const sourceData = this.state.ui.map.sources.routes.data;
									wsmsg.payload.forEach((route :any) => {

										const feature = {
											type: "Feature",
											geometry: {
												type: 'MultiLineString',
												coordinates: [] as any
											}
										};

										let previousNode = null as Array<number>;
										route.forEach((situation :any, index :number) => {
											if(index > 0){
												var start = turf.point(previousNode);
												var end = turf.point(situation['Location']);
												var greatCircle = turf.greatCircle(start, end);

												if(greatCircle.geometry.type == 'LineString'){
													feature.geometry.coordinates.push(greatCircle.geometry.coordinates);
												} else {
													greatCircle.geometry.coordinates.forEach((pos: any) => {
														feature.geometry.coordinates.push(pos);
													});
												}
											}
											previousNode = situation['Location'];
										});

										sourceData.features.push(feature);
									});

									break;
								}
							}
							break;
						}
					}
					break;
				}
				case 'eventbus': {

					break;
				}
			}
		},
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
		this.state.ui.map.load = true;
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
		clearInterval(this.state.ui.map.lineAnimationInterval);
	},
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.p42_scenr {

	.app-frame {
		background: #000;
		align-items: stretch;
		flex-direction: row;
	}

	.sidebar {
		background-color: $ui_colors_bright_shade_1;
		margin: 0;
	}

	.map {
		height: 100%;
		transition: opacity 2s ease-out;
		background: #000;
		&-hidden {
			opacity: 0;
			transition: opacity 0s;
		}
		.map_marker {
			&_location {
				width: 10px;
				height: 10px;
				border-radius: 50%;
				background-color: $ui_colors_dark_button_warn;
				border: 1px solid $ui_colors_dark_shade_5;
			}
			&_grab {
				width: 10px;
				height: 10px;
				border-radius: 50%;
				background-color: $ui_colors_dark_button_cancel;
				border: 1px solid $ui_colors_dark_shade_5;
			}
			&_position {
				width: 10px;
				height: 10px;
				border-radius: 50%;
				background-color: $ui_colors_bright_button_info;
				border: 3px solid $ui_colors_bright_shade_0;
				@include shadowed($ui_colors_bright_shade_5);
				cursor: crosshair;
			}
			&_poi {
				display: flex;
				color: #FFF;
				.symbol {
					position: relative;
					width: 45px;
					height: 45px;
					border-radius: 50%;
					background-image: radial-gradient(closest-side, rgba($ui_colors_dark_button_gold, 1) 0%, rgba($ui_colors_dark_button_gold, 0) 100%);
				}
				.label {
					position: absolute;
					left: 50%;
					top: 30px;
					transform: translateX(-50%);
					white-space: nowrap;
					font-family: "SkyOS-SemiBold";
					z-index: 2;
				}
			}
		}
	}
}
</style>