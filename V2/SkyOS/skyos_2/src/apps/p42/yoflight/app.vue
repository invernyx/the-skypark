<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame" ref="app-frame">

			<content_controls_stack :translucent="true" :status_padding="true" :nav_padding="true" :content_fixed="true" :shadowed="true">
				<template v-slot:content>

					<div class="helper_absolute_full">
						<!-- https://soal.github.io/vue-mapbox/guide/ -->
						<MapboxFrame
							ref="map"
							mstyle="big"
							id="p42_yoFlight_map_main"
							:app="app"
							:mapTheme="$os.getConfig(['ui', 'theme'])"
							:hasFog="true"
							@load="mapLoaded"
							@mapMoveDone="mapMoveDone"
							@resetTheme="resetTheme"
							>
							<template>

								<!--
								<div v-if="state.ui.map.displayLayer.poi">
									<MglMarker
										v-for="(POI, index) in state.ui.map.markers.POIs"
										v-bind:key="index"
										:coordinates="[POI[0], POI[1]]">
										<div class="map_marker map_marker_poi" slot="marker" :data-index="index">
											<div class="symbol"></div>
											<div class="label">{{ POI[2] }}</div>
										</div>
									</MglMarker>
								</div>
								-->

								<MglGeojsonLayer
									layerId="poisNode"
									sourceId="poisNode"
									:clearSource="false"
									:source="state.ui.map.sources.poisNode"
									:layer="state.ui.map.layers.poisNode" />

								<MglGeojsonLayer
									layerId="range"
									sourceId="range"
									:source="state.ui.map.sources.range"
									:layer="state.ui.map.layers.range" />

								<MglGeojsonLayer
									layerId="rangeOut"
									sourceId="rangeOut"
									:source="state.ui.map.sources.rangeOut"
									:layer="state.ui.map.layers.rangeOut" />

								<MglGeojsonLayer
									layerId="runwayExtensions"
									sourceId="runwayExtensions"
									:source="state.ui.map.sources.runwayExtensions"
									:layer="state.ui.map.layers.runwayExtensions" />

								<MglGeojsonLayer
									layerId="contractPaths"
									sourceId="contractPaths"
									:clearSource="false"
									:source="state.ui.map.sources.contractPaths"
									:layer="state.ui.map.layers.contractPaths" />
								<MglGeojsonLayer
									layerId="contractPathsShadow"
									sourceId="contractPaths"
									:source="state.ui.map.sources.contractPaths"
									:layer="state.ui.map.layers.contractPathsShadow" />

								<MglGeojsonLayer
									layerId="situationRange"
									sourceId="situationRange"
									:source="state.ui.map.sources.situationRange"
									:layer="state.ui.map.layers.situationRange" />

								<MglGeojsonLayer
									layerId="planPaths"
									sourceId="planPaths"
									:clearSource="false"
									:source="state.ui.map.sources.planPaths"
									:layer="state.ui.map.layers.planPaths" />
								<MglGeojsonLayer
									layerId="planPathsShadow"
									sourceId="planPaths"
									:source="state.ui.map.sources.planPaths"
									:layer="state.ui.map.layers.planPathsShadow" />


								<MglGeojsonLayer
									layerId="poisNodeLabel"
									sourceId="poisNode"
									:source="state.ui.map.sources.poisNode"
									:layer="state.ui.map.layers.poisNodeLabel" />

								<MglGeojsonLayer
									layerId="runway"
									sourceId="runway"
									:source="state.ui.map.sources.runway"
									:layer="state.ui.map.layers.runway" />

								<MglGeojsonLayer
									layerId="runwayExtendedNames"
									sourceId="runwayExtendedNames"
									:source="state.ui.map.sources.runwayExtendedNames"
									:layer="state.ui.map.layers.runwayExtendedNames" />

								<MglGeojsonLayer
									layerId="aircraftPolyGlow"
									sourceId="aircraftPolyGlow"
									:source="state.ui.map.sources.aircraftPolyGlow"
									:layer="state.ui.map.layers.aircraftPolyGlow" />

								<MglGeojsonLayer
									layerId="aircraftPoly"
									sourceId="aircraftPoly"
									:source="state.ui.map.sources.aircraftPoly"
									:layer="state.ui.map.layers.aircraftPoly" />

								<MglGeojsonLayer
									layerId="aircraftPlot"
									sourceId="aircraftPlot"
									:source="state.ui.map.sources.aircraftPlot"
									:layer="state.ui.map.layers.aircraftPlot" />



								<div v-for="(actions, name, index) in state.ui.map.markers.actions" v-bind:key="index">
									<MglMarker v-for="(action, name1, index) in actions" v-bind:key="index" :coordinates="action.location">
										<div class="map_marker mapboxgl-marker mapboxgl-marker-anchor-center" :class="'map_marker_' + name" slot="marker" v-bind:key="index">
											<div>
											</div>
										</div>
									</MglMarker>
								</div>

								<MglMarker v-for="(navaid, name, index) in state.ui.map.markers.navaids" v-bind:key="index" :coordinates="navaid.location">
									<div class="map_marker map_marker_navaid mapboxgl-marker mapboxgl-marker-anchor-center" :class="'map_marker_navaid_' + navaid.type" slot="marker" v-bind:key="index">
										<div class="label">{{ navaid.label }}</div>
										<div class="wx" v-if="navaid.wx.length">{{ navaid.wx }}</div>
									</div>
								</MglMarker>

							</template>
						</MapboxFrame>
						<div class="map_controls">
							<div class="buttons_list shadowed">
								<button_action class="listed map_controls_3d" @click.native="mapTiltToggle" :class="{ 'info': Math.round(state.ui.map.pitch) != 0 }">3D</button_action>
								<button_action class="listed map_controls_north" @click.native="mapNorth" :class="{ 'info': state.ui.map.tracking.north }"></button_action>
								<button_action class="listed map_controls_track" @click.native="mapTrackToggle" :class="{ 'info': state.ui.map.tracking.enabled }" v-if="$root.$data.state.services.simulator.live"></button_action>
								<button_action class="listed map_controls_autozoom" @click.native="mapAutoZoomToggle" :class="{ 'info': state.ui.map.tracking.autozoom }" v-if="state.ui.map.tracking.enabled && $root.$data.state.services.simulator.live"></button_action>
							</div>
							<div class="buttons_list shadowed">
								<!--<button_action class="listed map_controls_elev" :class="{ 'info': state.ui.map.displayLayer.terrain_avoidance }" @click.native="mapSetLayerVis('terrain_avoidance', !state.ui.map.displayLayer.terrain_avoidance)"></button_action>-->
								<button_action class="listed map_controls_hill" :class="{ 'info': state.ui.map.displayLayer.hillshading }" @click.native="mapSetLayerVis('hillshading', !state.ui.map.displayLayer.hillshading)"></button_action>
								<button_action class="listed map_controls_poi" :class="{ 'info': state.ui.map.displayLayer.poi }" @click.native="mapSetLayerVis('poi', !state.ui.map.displayLayer.poi)" v-if="state.contracts.selected.Template ? state.contracts.selected.Template.POIs.length : false"><div class="icon"></div><div class="text">POI</div></button_action>
							</div>
						</div>
					</div>

					<div class="top-content" v-if="state.contracts.selected.Contract && state.contracts.selected.Template">
						<ContractTodo @interactAction="interactAction" @contractDetail="contractModalOpen" @close="flightCancel" :contract="state.contracts.selected.Contract" :template="state.contracts.selected.Template" :exclude="['done', 'future']"/>
					</div>

					<div class="bottom-content">
						<div class="bottom-blur"></div>
						<div class="results_section" :class="{ 'has-selected': state.contracts.selected.Plan && !state.ui.isFlying, 'is-flying': state.ui.isFlying }">

							<div class="results_contracts" :class="{ 'is-active': !state.contracts.selected.Plan && !state.ui.isFlying }">
								<div class="results_header">
									<div class="columns columns_2 helper_edge_margin_lateral">
										<div class="column column_2 column_left">
											<div class="results_nav">
												<button_action class="results_nav_back transparent" :class="{ 'disabled': state.contracts.actives.Contracts.length < 2 }" @click.native="contractsOffset('search_results', -1)"></button_action>
												<button_action class="results_nav_forward transparent" :class="{ 'disabled': state.contracts.actives.Contracts.length < 2 }" @click.native="contractsOffset('search_results', 1)"></button_action>
											</div>
											<div class="results_title">
												<h2>Select a Contract</h2>
											</div>
										</div>
									</div>
								</div>
								<div class="results_list">
									<ContractList @select="contractsSelect($event, false)" @expand="contractsSelect($event, true)" theme="helper_edge_padding_lateral" :selected="state.contracts.selected" :contracts="state.contracts.actives" ref="search_results"></ContractList>
								</div>
							</div>

							<div class="results_plans" :class="{ 'is-active': state.contracts.selected.Plan && !state.ui.isFlying }">
								<div class="results_header" v-if="state.contracts.selected.Plan && state.contracts.selected.Contract">
									<div class="columns helper_edge_margin_lateral">
										<div class="column column_left">
											<div class="results_nav">
												<button_nav shape="back" @click.native="planSelect(null, false)">Contracts</button_nav>

												<button_action class="results_nav_back transparent" :class="{ 'disabled': state.contracts.selected.Contract.Flightplans.length < 2 }" @click.native="plansOffset('plans_results', -1)"></button_action>
												<button_action class="results_nav_forward transparent" :class="{ 'disabled': state.contracts.selected.Contract.Flightplans.length < 2 }" @click.native="plansOffset('plans_results', 1)"></button_action>
											</div>
											<div class="results_title">
												<h2>Select a Flight Plan</h2>
											</div>
										</div>
									</div>
								</div>
								<div class="results_list" v-if="state.contracts.selected.Contract && state.contracts.selected.Plan">
									<PlansList @select="planSelect($event)" @fly="flightFly($event)" theme="helper_edge_padding_lateral" :selected="state.contracts.selected" ref="plans_results"></PlansList>
								</div>
							</div>

							<div class="results_enroute" :class="{ 'is-active': state.ui.isFlying }">
								<div class="results_header" v-if="state.contracts.selected.Plan">
									<div class="columns helper_edge_margin_lateral">
										<div class="column column_left">
											<div class="results_nav">
												<button_nav shape="back" @click.native="flightCancel">Close Flight Plan</button_nav>
												<!--<button_nav @click.native="flightSendToSim">Send to sim</button_nav>-->
											</div>
										</div>
									</div>
								</div>
								<div class="results_data">
									<div v-for="(value, index) in state.ui.dataBar" v-bind:key="index">
										<DataBlock :app="app" :type="value" @click.native="dateBlockOpen(index)" />
									</div>
								</div>
								<div class="results_list" v-if="state.contracts.selected.Plan && state.ui.isFlying">
									<EnrouteBar :selected="state.contracts.selected" />
								</div>
							</div>

						</div>
					</div>

				</template>
			</content_controls_stack>

			<modal type="grow" :app="app" v-if="state.ui.currentModal == 'plan' && state.contracts.selected.Contract && state.contracts.selected.Plan" width="narrow" @close="state.ui.currentModal = null">
				<content_controls_stack :translucent="true">
					<template v-slot:nav>
						<button_nav @click.native="state.ui.currentModal = null">Close</button_nav>
						<div class="h-stack">
							<button_nav shape="back" :class="{ 'disabled': state.contracts.selected.Contract.Flightplans.length < 2  }" @click.native="plansOffset('plans_results', -1)">Previous</button_nav>
							<button_nav shape="forward" :class="{ 'disabled': state.contracts.selected.Contract.Flightplans.length < 2  }" @click.native="plansOffset('plans_results', 1)">Next</button_nav>
						</div>
					</template>
					<template v-slot:content>
						<FlightplanDetailed :plan="state.contracts.selected.Plan" />
					</template>
					<template v-slot:tab>
						<div class="h-stack">
							<!--<button_nav class="go">Copy Route</button_nav>-->
						</div>
						<div class="h-stack">
							<button_nav shape="forward" class="go" @click.native="flightFly(state.contracts.selected.Plan)">Fly</button_nav>
						</div>
					</template>
				</content_controls_stack>
			</modal>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import ContractList from "./components/contract_list.vue"
import PlansList from "./components/plans_list.vue"
import EnrouteBar from "./components/enroute_bar.vue"
import DataBlock from "./components/data_block.vue"
import ContractTodo from "./components/contract_todo.vue"
import FlightplanDetailed from "@/sys/components/flightplans/flightplan_detailed.vue";
import * as turf from '@turf/turf';
import MapboxExt from '@/sys/libraries/mapboxExt';
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import { AppInfo } from "@/sys/foundation/app_bundle"
import { MglMap, MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "p42_yoflight",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ContractList,
		ContractTodo,
		PlansList,
		EnrouteBar,
		DataBlock,
		FlightplanDetailed,
		MapboxFrame,
		MglMap,
		MglMarker,
		MglNavigationControl,
		MglGeojsonLayer,
	},
	data() {
		return {
			ready: true,
			state: {
				contracts: {
					selected: {
						restored: false,
						Contract: null,
						Template: null,
						Plan: null,
						At: 0,
						AtPrev: 0,
						ExpiresAt: null,
						ExpiresAtInterval: null,
					},
					actives: {
						Limit: 0,
						State: 0,
						Count: 0,
						Contracts: [],
						Templates: []
					}
				},
				ui: {
					loaded: false,
					currentModal: null,
					isTracking: false,
					isFlying: false,
					dataBar: [
						'Alt',
						'GAlt',
						'FPM',
						'HDG',
						'CRS',
						'GS',
						//'AirTime',
					],
					map: {
						loaded: false,
						location: [0,0],
						zoom: 2,
						zoomOffset: 1,
						heading: 0,
						pitch: 0,
						padding: { left: 50, top: 220, right: 100, bottom: 270 },
						elevationBracket: -2000,
						reframeTimeout: null,
						radarFetchTimer: null,
						radarTimer: null,
						radarAnimated: false,
						dynamicLayers: {
							nightlights: false,
						},
						tracking: {
							enabled: false,
							north: false,
							tilted: true,
							soft: true,
							autozoom: true,
						},
						displayLayer: {
							sat: false,
							terrain_avoidance: false,
							weather_radar: false,
							hillshading: true,
							poi: true,
						},
						airports: {
							level: 'large',
							visibleTiles: [],
							data: {
								large: {

								},
								medium: {

								},
								small: {

								}
							}
						},
						mapStyle: 'mapbox://styles/biarzed/cjie4rwi91za72rny0hlv9v7t',
						sources: {
							range: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							rangeOut: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							contractPaths: {
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
							planPaths: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							aircraftPoly: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							aircraftPolyGlow: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							aircraftPlot: {
								type: 'geojson',
								lineMetrics: true,
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
							runway: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							runwayExtensions: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							runwayExtendedNames: {
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
								source: "range",
								paint: {
									'line-color': '#FF0000',
									'line-width': 8,
									'line-opacity': 0.5
								}
							},
							rangeOut: {
								type: "line",
								source: "rangeOut",
								paint: {
									'line-color': '#FF0000',
									'line-width': 1,
									'line-opacity': ['get', 'opacity'],
								}
							},
							contractPaths: {
								id: 'contractPaths',
								type: 'line',
								source: 'contractPaths',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': '#46b446',
									'line-width': [
										"case",
										["==", ["feature-state", "state"], 'active'], 1,
										//["boolean", ["feature-state", "hovered"], false], 6,
										2
									],
									'line-opacity': [
										"case",
										["==", ["feature-state", "state"], 'active'], 0.5,
										["boolean", ["feature-state", "hovered"], false], 0.9,
										0.8
									]
								},
								filter: ['in', '$type', 'LineString']
							},
							contractPathsShadow: {
								id: 'contractPathsShadow',
								type: 'line',
								source: 'contractPaths',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': [
										'match',
										['feature-state', 'state'],
										'Listed', '#4285f4',
										'Active', '#46b446',
										'Saved', '#46b446',
										'Failed', '#b44646',
										'#CCC'
									],
									'line-width': 20,
									'line-opacity': 0.05
								},
								filter: ['in', '$type', 'LineString']
							},
							situationRange: {
								type: "fill",
								source: "situationRange",
								paint: {
									'fill-color': '#FFFFFF',
									'fill-opacity': 0.2
								}
							},
							planPaths: {
								id: 'planPaths',
								type: 'line',
								source: 'planPaths',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': '#f500f5',
									'line-width': [
										"case",
										["==", ['get', 'state'], 'active'], 6,
										//["boolean", ["get", "hovered"], false], 1,
										1
									],
									'line-opacity': [
										"case",
										["==", ['get', 'state'], 'active'], 1,
										["boolean", ["get", "hovered"], false], 0.9,
										0.8
									]
								},
								filter: ['in', '$type', 'LineString']
							},
							planPathsShadow: {
								id: 'planPathsShadow',
								type: 'line',
								source: 'planPaths',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': '#f500f5',
									'line-width': 20,
									'line-opacity': 0.05
								},
								filter: ['in', '$type', 'LineString']
							},
							aircraftPlot: {
								id: 'aircraftPlot',
								type: 'line',
								source: 'aircraftPlot',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': '#FFFFFF',
									'line-width': 3,
									'line-opacity': ["get", 'rate'],
								},
								filter: ['in', '$type', 'LineString']
							},
							aircraftPoly: {
								id: 'aircraftPoly',
								type: 'fill-extrusion',
								source: 'aircraftPoly',
								paint: {
									'fill-extrusion-color': '#FFFFFF',
									'fill-extrusion-height': ['get', 'thick'],
									'fill-extrusion-base': ['get', 'base'],
									'fill-extrusion-opacity': 0.8,
									'fill-extrusion-translate': [0,0]
								},
								filter: ['==', '$type', 'Polygon']
							},
							aircraftPolyGlow: {
								id: 'aircraftPolyGlow',
								type: 'circle',
								source: 'aircraftPolyGlow',
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
							poisNode: {
								id: "poisNode",
								type: 'circle',
								source: 'poisNode',
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
									'text-color': "#FFF",
									'text-halo-color': '#FFFFFF',
									'text-halo-width': 2,
								}
							},
							runwayExtensions: {
								id: 'runwayExtensions',
								type: 'line',
								source: 'runwayExtensions',
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
							runway: {
								id: "runway",
        						type: "symbol",
								source: 'runway',
								minzoom: 14,
								layout: {
									"symbol-spacing": 500,
									"symbol-placement": "line",
									"text-font": ["DIN Offc Pro Bold", "Arial Unicode MS Bold"],
									"text-field": ['get', 'title'],
									"text-size": 12,
									"text-anchor": "top",
									//"text-offset": [0, -0.5],
									"text-allow-overlap": false
								},
								paint: {
									'text-color': "#FFF",
								}
							},
							runwayLength: {
								id: "runwayLength",
        						type: "line",
								source: 'runway',
								minzoom: 7,
								layout: {
									"text-field": ['get', 'title'],
									"text-font": ["DIN Offc Pro Bold", "Arial Unicode MS Bold"],
									"text-size": 14,
								},
								paint: {
									'text-color': "#FFF",
								}
							},
							runwayExtendedNames: {
								id: "runwayExtendedNames",
								type: 'symbol',
								source: 'runwayExtendedNames',
								minzoom: 7,
								layout: {
									"text-field": ['get', 'title'],
									"text-font": ["DIN Offc Pro Bold", "Arial Unicode MS Bold"],
									"text-size": 14,
								},
								paint: {
									'text-color': "#FFF",
								}
							},
						},
						markers: {
							airports: [],
							navaids: [],
							actions: {
								cargo_pickup: [],
								cargo_dropoff: [],
							}
						}
					}
				}
			}
		}
	},
	beforeMount() {
		if(this.app.loaded_state != null){
			if(this.app.loaded_state.ui.map) {

				this.state.ui.map.location = this.app.loaded_state.ui.map.location;
				this.state.ui.map.zoom = this.app.loaded_state.ui.map.zoom;
				this.state.ui.map.heading = this.app.loaded_state.ui.map.heading;
				this.state.ui.map.pitch = this.app.loaded_state.ui.map.pitch;

				if(this.app.loaded_state.ui.map.tracking) {
					this.state.ui.map.tracking.enabled = this.app.loaded_state.ui.map.tracking.enabled;
					this.state.ui.map.tracking.tilted = this.app.loaded_state.ui.map.tracking.tilted;
				}

				if(this.app.loaded_state.ui.map.displayLayer) {
					this.state.ui.map.displayLayer.terrain_avoidance = this.app.loaded_state.ui.map.displayLayer.terrain_avoidance;
					this.state.ui.map.displayLayer.weather_radar = this.app.loaded_state.ui.map.displayLayer.weather_radar;
					this.state.ui.map.displayLayer.hillshading = this.app.loaded_state.ui.map.displayLayer.hillshading;
				}
			}
		}
	},
	mounted() {
		this.$emit('loaded');
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
		this.state.contracts.selected.ExpiresAtInterval = setInterval(this.contractCheckExpiration, 1000);
	},
	beforeDestroy() {
		clearTimeout(this.state.ui.map.reframeTimeout);
		clearInterval(this.state.contracts.selected.ExpiresAtInterval);
		clearInterval(this.state.ui.map.radarFetchTimer);
		clearInterval(this.state.ui.map.radarTimer);
		this.state.ui.map.loaded = false;
		this.$root.$off('ws-in', this.listenerWs);
	},
	activated() {
		setTimeout(() => {
			this.contractsSearch();
		}, 800);
	},
	methods: {
		mapLoaded(map: any) {
			this.$os.TrackMapLoad(this.$route.path);
			window.requestAnimationFrame(() => {
				setTimeout(() => {
					if(this.$root.$data.state.services.simulator.live){
						this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setCenter([this.$root.$data.state.services.simulator.location.Lon, this.$root.$data.state.services.simulator.location.Lat]);
					} else {
						this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setCenter([this.state.ui.map.location[0], this.state.ui.map.location[1]]);
					}
					this.state.ui.map.loaded = true;
					this.mapRenderFeatures();
					//this.mapUpdateRadar();
					this.mapTrackUpdate();
				}, 100)

				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setZoom(this.state.ui.map.zoom);
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setBearing(this.state.ui.map.heading);
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setPitch(this.state.ui.map.pitch);

				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('elev-warn', 'visibility', this.state.ui.map.displayLayer.terrain_avoidance ? 'visible' : 'none');
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('hillshade-raster', 'visibility', this.state.ui.map.displayLayer.hillshading ? 'visible' : 'none');
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('Hillshade Highlight', 'visibility', this.state.ui.map.displayLayer.hillshading ? 'visible' : 'none');
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('Hillshade Shadow', 'visibility', this.state.ui.map.displayLayer.hillshading ? 'visible' : 'none');

				this.mapSetEvents();

				if(!this.state.ui.loaded){
					this.state.ui.loaded = true;
					setTimeout(() => {
						this.contractsSearch();
					}, 500);
				}

			});
		},
		mapSetEvents(){
			['moveend', 'zoomend', 'rotateend', 'pitchend'].forEach((ev) => {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, () => {
					SelectCancel = false;
					if(!this.state.ui.map.tracking.enabled) {
						if(Math.abs(this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getBearing()) < 5) {
							this.state.ui.map.tracking.north = true;
						} else {
							this.state.ui.map.tracking.north = false;
						}
						//if(Math.abs(this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getPitch()) > 5) {
						//	this.state.ui.map.tracking.tilted = true;
						//} else {
						//	this.state.ui.map.tracking.tilted = false;
						//}
					}

				});
			});

			let ScheduleInteractionClear = false;
			let SelectCancel = false;
			['dragstart'].forEach((ev) => {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, () => {
					SelectCancel = false;
					ScheduleInteractionClear = true;
					setTimeout(() => {
						this.$emit('state:ui:map:interac');
					}, 100);
				});
			});

			['wheel'].forEach((ev) => {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, (ev1 :any) => {
					if(this.state.ui.map.tracking.enabled) {
						if(this.state.ui.map.tracking.autozoom) {
							this.state.ui.map.tracking.autozoom = false;
						}

						ev1.preventDefault();

						//if(this.state.ui.map.tracking.autozoom) {
						//	this.state.ui.map.zoomOffset -= ev1.originalEvent.deltaY / 10000;
						//	if(this.state.ui.map.zoomOffset > 1.2) {
						//		this.state.ui.map.zoomOffset = 1.2;
						//	} else if(this.state.ui.map.zoomOffset < 0.3) {
						//		this.state.ui.map.zoomOffset = 0.3;
						//	}
						//} else {
							const cz = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getZoom();
							this.state.ui.map.zoom = Eljs.limiter(1, 24, cz - (ev1.originalEvent.deltaY / 1000));
						//}

						//this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getZoom()
						this.mapTrackUpdate();
					}
				});
			});

			//['rotateend'].forEach((ev) => {
			//	this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, () => {
			//		this.state.ui.map.tracking.north = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getBearing() == 0;
			//	});
			//});

			['move', 'zoom', 'rotate', 'pitch'].forEach((ev) => {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, (ev1) => {

					if(ev1.originalEvent) {
						switch(ev) {
							case 'pitch':
							case 'rotate': {
								this.state.ui.map.tracking.enabled = false;
								break;
							}
						}
					}

					if(!SelectCancel){
						SelectCancel = true;
					}
					if(ScheduleInteractionClear){
						this.state.ui.map.tracking.enabled = false;
						ScheduleInteractionClear = false;
					}
					this.mapMakeAircraftPoly();
				});
			});

			['mouseup', 'touchend'].forEach((ev) => {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, (e: any) => {
					const features = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.queryRenderedFeatures(e.point);

					const contractFeature = features.find((x :any) => x.layer.id == 'contractPathsShadow');
					const planFeature = features.find((x :any) => x.layer.id == 'planPathsShadow');

					if(!this.state.ui.isFlying){
						if(!planFeature) {
							if(contractFeature && !SelectCancel) {
								this.contractsSelect(this.state.contracts.actives.Contracts.find((x :any) => x.ID == contractFeature.id), false);
							} else {
								if(ScheduleInteractionClear){
									ScheduleInteractionClear = false;
									this.contractsSelect(null, false)
								}
							}
						} else {
							if(!SelectCancel && this.state.contracts.selected.Contract) {
								this.planSelect(this.state.contracts.selected.Contract.Flightplans.find((x :any) => x.UID == planFeature.id), false);
							}
						}

					}

					ScheduleInteractionClear = false;
				});
			});

			['mouseleave'].forEach((ev) => {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, 'contractPathsShadow', (e: any) => {
					this.mapSetHover('contractPaths', null, false);
				});
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, 'planPathsShadow', (e: any) => {
					this.mapSetHover('planPaths', null, false);
				});
			});

			['mousemove'].forEach((ev) => {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, 'contractPathsShadow', (e: any) => {
					const features = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.queryRenderedFeatures(e.point);
					features.forEach((feature: any, index: number) => {
						if(index == 0){
							this.mapSetHover('contractPaths', feature, true);
						}
					});
				});
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.on(ev, 'planPathsShadow', (e: any) => {
					const features = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.queryRenderedFeatures(e.point);
					features.forEach((feature: any, index: number) => {
						if(index == 0){
							this.mapSetHover('planPaths', feature, true);
						}
					});
				});
			});
		},
		mapSetLayerVis(layerName :string, state :boolean) {
			switch(layerName) {
				case 'terrain_avoidance': {
					this.state.ui.map.displayLayer.terrain_avoidance = state;
					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('elev-warn', 'visibility', state ? 'visible' : 'none');
					break;
				}
				case 'hillshading': {
					this.state.ui.map.displayLayer.hillshading = state;
					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('hillshade-raster', 'visibility', state ? 'visible' : 'none');
					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('Hillshade Highlight', 'visibility', state ? 'visible' : 'none');
					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('Hillshade Shadow', 'visibility', state ? 'visible' : 'none');
					break;
				}
				case 'poi': {
					this.state.ui.map.displayLayer.poi = state;
					this.mapRenderPOIs();
				}
			}
			this.stateSave();
		},

		mapSetState(source :any, id :any, state :string) {
			if(this.state.ui.map.loaded) {
				let features = this.state.ui.map.sources[source].data.features;
				if(id) {
					features = features.filter((x :any) => x.id == id);
				}
				features.forEach((feature: any, index: number) => {
					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setFeatureState({
						source: source,
						id: feature.id
					}, {
						state: state
					});
				});
			}
		},
		mapSetHover(source :any, id :any, hovered :boolean) {
			let features = this.state.ui.map.sources[source].data.features;
			if(id) {
				features = features.filter((x :any) => x.id == id);
			}
			features.forEach((feature: any, index: number) => {
				feature.properties.hover = hovered;
				//if(index == 0){
				//	this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setFeatureState({
				//		source: source,
				//		id: feature.id
				//	}, {
				//		hovered: hovered
				//	});
				//}
			});
			this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getSource('planPaths').setData(this.state.ui.map.sources[source].data);
		},

		mapRenderFeatures() {
			// Process Contracts / Plans
			if(!this.state.ui.isFlying){

				// Catalog existing Contracts
				const existingContracts = [] as any[];
				this.state.ui.map.sources.contractPaths.data.features.forEach((feature :any) => { existingContracts.push(feature.id); });

				// Process active contracts
				this.state.contracts.actives.Contracts.forEach((contract :any) => {

					let existing = this.state.ui.map.sources.contractPaths.data.features.find(x => x.id == contract.ID);
					let makeActive = false;
					if(this.state.contracts.selected.Contract) {
						if(this.state.contracts.selected.Contract.ID == contract.ID) {
							makeActive = true;
						}
					}

					if(existing) {
						existingContracts.splice(existingContracts.indexOf(existing.id), 1);
					} else {
						const feature = {
							type: 'Feature',
							id: contract.ID,
							properties: {
								state: 'passive',
								hovered: false,
							},
							geometry: {
								type: 'MultiLineString',
								coordinates: [] as any
							},
						}

						let previousNode = null as Array<number>;
						contract.Situations.forEach((situation: any, index: number) => {
							if(index > 0){
								const greatCircle = turf.greatCircle(turf.point(previousNode), turf.point(situation.Location));
								if(greatCircle.geometry.type == 'LineString'){
									feature.geometry.coordinates.push(greatCircle.geometry.coordinates);
								} else {
									greatCircle.geometry.coordinates.forEach((pos: any) => {
										feature.geometry.coordinates.push(pos);
									});
								}
							}
							previousNode = situation.Location;
						});

						this.state.ui.map.sources.contractPaths.data.features.push(feature);
					}

					if(makeActive) {
						this.mapSetState('contractPaths', contract.ID, 'active');
					} else {
						this.mapSetState('contractPaths', contract.ID, 'passive');
					}
				});

				// Remove trash
				existingContracts.forEach((existing :any) => {
					const Index = this.state.ui.map.sources.contractPaths.data.features.indexOf(existing);
					this.state.ui.map.sources.contractPaths.data.features.splice(Index, 1);
				});

				// Catalog existing flightplans
				this.state.ui.map.sources.planPaths.data.features = [];

				// Process each flightplan
				if(this.state.contracts.selected.Contract) {
					this.state.contracts.selected.Contract.Flightplans.forEach((plan :any) => {

						let makeActive = false;
						if(this.state.contracts.selected.Plan) {
							makeActive = this.state.contracts.selected.Plan.UID == plan.UID;
						}

						const feature = {
							type: 'Feature',
							id: plan.UID,
							properties: {
								state: makeActive ? 'active' : 'passive',
								hovered: false,
							},
							geometry: {
								type: 'MultiLineString',
								coordinates: [] as any
							},
						}

						let previousNode = null as Array<number>;
						plan.Waypoints.forEach((waypoint: any, index: number) => {
							if(index > 0){
								const greatCircle = turf.greatCircle(turf.point(previousNode), turf.point(waypoint.Location));
								if(greatCircle.geometry.type == 'LineString'){
									feature.geometry.coordinates.push(greatCircle.geometry.coordinates);
								} else {
									greatCircle.geometry.coordinates.forEach((pos: any) => {
										feature.geometry.coordinates.push(pos);
									});
								}
							}
							previousNode = waypoint.Location;
						});

						this.state.ui.map.sources.planPaths.data.features.push(feature);
					});
				}

			} else {
				this.state.ui.map.sources.contractPaths.data.features = [];
				this.state.ui.map.sources.runwayExtendedNames.data.features = [];
				this.state.ui.map.sources.runwayExtensions.data.features = [];

				// Catalog existing flightplans
				const existingPlans = [] as any[];
				this.state.ui.map.sources.planPaths.data.features = [];

				if(this.state.contracts.selected.Plan) {
					const plan = this.state.contracts.selected.Plan;

					let previousNode = null as Array<number>;
					plan.Waypoints.forEach((waypoint: any, index: number) => {
						if(index > 0){
							const feature = {
								type: 'Feature',
								id: plan.UID + index,
								properties: {
									state: 'active',
									hovered: false,
								},
								geometry: {
									type: 'MultiLineString',
									coordinates: [] as any
								},
							}

							const greatCircle = turf.greatCircle(turf.point(previousNode), turf.point(waypoint.Location));
							if(greatCircle.geometry.type == 'LineString'){
								feature.geometry.coordinates.push(greatCircle.geometry.coordinates);
							} else {
								greatCircle.geometry.coordinates.forEach((pos: any) => {
									feature.geometry.coordinates.push(pos);
								});
							}
							this.state.ui.map.sources.planPaths.data.features.push(feature);
						}
						previousNode = waypoint.Location;
					});

				}

				this.mapUpdateFlightPlan(true);

				// Remove trash
				existingPlans.forEach((existing :any) => {
					const Index = this.state.ui.map.sources.planPaths.data.features.indexOf(existing);
					this.state.ui.map.sources.planPaths.data.features.splice(Index, 1);
				});

			}
			this.state.contracts.selected.At = 0;
			this.state.contracts.selected.AtPrev = -1;
			this.mapRenderPOIs();
			this.mapRenderExtendedRunways();
			this.mapRenderMarkers();
			this.mapRenderPauseRange();
		},
		mapRenderMarkers() {

			// Clear all markers
			Object.keys(this.state.ui.map.markers.actions).forEach((actionKey: string) => {
				this.state.ui.map.markers.actions[actionKey] = [];
			});
			this.state.ui.map.markers.navaids = [];
			this.state.ui.map.sources.situationRange.data.features = [];

			// Add selected contract
			if(this.state.contracts.selected.Contract && !this.state.ui.isFlying) {
				const contract = this.state.contracts.selected.Contract;
				contract.Path.forEach((node: any, index: number) => {
					node.Actions.forEach((action: any) => {
						const Sit = contract.Situations[index];

						if(this.state.ui.map.markers.actions[action.ActionType] == undefined){
							this.state.ui.map.markers.actions[action.ActionType] = [];
						}

						const markerData = {
							location: Sit.Location
						};

						this.state.ui.map.markers.actions[action.ActionType].push(markerData);

					});
				});
			}

			// Add selected plan
			if(this.state.contracts.selected.Plan) {
				const plan = this.state.contracts.selected.Plan;
				plan.Waypoints.forEach((wp : any) => {
					const markerData = {
						location: wp.Location,
						type: wp.Type,
						label: '',
						wx: ''
					};
					//switch(wp.Type.toLowerCase()) {
					//	case 'airport': {
					//		const apt = plan.Airports.find((x :any) => x.ICAO ==  wp.Code);
					//		if(apt.Weather) {
					//			markerData.wx = apt.Weather.WindHeading + "@" + apt.Weather.WindSpeed + (apt.Weather.WindGust > 0 ? "G" + apt.Weather.WindGust : '') + "KT";
					//		}
					//		break;
					//	}
					//}
					switch(wp.Code.toLowerCase()){
						case 'timecruis': {
							markerData.label = 'T/C';
							break;
						}
						case 'timedscnt': {
							markerData.label = 'T/D';
							break;
						}
						default: {
							markerData.label = wp.Code;
							break;
						}
					}
					this.state.ui.map.markers.navaids.push(markerData);
				});
			} else {
				this.state.ui.map.markers.navaids = [];
			}

			// Add Situation Ranges
			if(this.state.contracts.selected.Contract && this.state.ui.isFlying) {
				const contract = this.state.contracts.selected.Contract;
				contract.Situations.forEach((Sit: any, index: number) => {
					if(Sit.TriggerRange) {
						const c = turf.circle([Sit.Location[0], Sit.Location[1]], Sit.TriggerRange * 1.852, { steps: 90, units: 'kilometers' });
						this.state.ui.map.sources.situationRange.data.features.push(c);
					}
				})
			}


		},
		mapRenderExtendedRunways() {
			this.state.ui.map.sources.runwayExtensions.data.features = [];
			this.state.ui.map.sources.runway.data.features = [];
			this.state.ui.map.sources.runwayExtendedNames.data.features = [];
			if(this.state.ui.isFlying){
				const plan = this.state.contracts.selected.Plan;
				if(plan) {

					const MakeRwy = (wx :any, runway :any, heading :number, name :string) => {

						const IsDark = this.$root.$data.config.ui.theme == 'theme--dark';
						const DistanceLimit = 10; //Eljs.limiter(5, 10, Math.round(runway.LengthFT / 800));
						let Color = IsDark ? 'rgba(125,125,125,1)' : 'rgba(125,125,125,1)';
						let Opacity = 1;
						//if(wx != null) {
						//	if(Math.abs(Eljs.MapCompareBearings(heading, wx.WindHeading)) > 90) {
						//		Color = IsDark ? 'rgba(30,125,30,1)' : 'rgba(30,200,30,1)';
						//		Opacity = 1;
						//	} else {
						//		Color = IsDark ? 'rgba(125,30,30,1)' : 'rgba(200,30,30,1)';
						//		Opacity = 0.5;
						//	}
						//}

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

						const rwyThreshold = Eljs.MapOffsetPosition(runway.Location[0], runway.Location[1], runway.LengthFT / 2 * 0.3048, heading);
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

						this.state.ui.map.sources.runwayExtensions.data.features.push(centerLineFeature);
						this.state.ui.map.sources.runwayExtensions.data.features.push(marksLineFeature);
						this.state.ui.map.sources.runwayExtensions.data.features.push(marksLineMinorFeature);

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
						this.state.ui.map.sources.runwayExtendedNames.data.features.push(airportFeature);

					}

					plan.Airports.forEach((airport :any) => {
						airport.Runways.forEach((runway :any) => {
							MakeRwy(airport.Weather, runway, runway.Heading, runway.SecondaryName);
							MakeRwy(airport.Weather, runway, runway.Heading + 180, runway.PrimaryName);

							// Runway Length
							const runwayFeature = {
								type: "Feature",
								properties: {
									title: runway.PrimaryName + '/' + runway.SecondaryName,
								},
								geometry: {
									type: "LineString",
									coordinates: [
										Eljs.MapOffsetPosition(runway.Location[0], runway.Location[1], runway.LengthFT / 2 * 0.3048, runway.Heading),
										Eljs.MapOffsetPosition(runway.Location[0], runway.Location[1], runway.LengthFT / 2 * 0.3048, runway.Heading + 180),
									],
								}
							};

							this.state.ui.map.sources.runway.data.features.push(runwayFeature);

						});
					});
				}
			}
		},
		mapRenderPauseRange() {
			this.state.ui.map.sources.range.data.features = [];
			this.state.ui.map.sources.rangeOut.data.features = [];
			if(this.state.contracts.selected.Contract) {
				if(this.state.contracts.selected.Contract.LastLocationGeo && this.state.contracts.selected.Contract.State == 'Active' && !this.state.contracts.selected.Contract.IsMonitored && this.state.contracts.selected.Contract.LastLocationGeo) {

					//Main red
					this.state.ui.map.sources.range.data.features.push(turf.circle(this.state.contracts.selected.Contract.LastLocationGeo, 14.816, { steps: 90, units: 'kilometers' }));

					for (let i = 6; i < 35; i++) {
						const circle = turf.circle(this.state.contracts.selected.Contract.LastLocationGeo, (1 + Math.pow((i * 0.5), 3)), {
							steps: 90,
							units: 'kilometers',
							properties: {
								opacity: Eljs.round(Math.pow(0.9, i), 2) * 0.5
							}
						});
						this.state.ui.map.sources.rangeOut.data.features.push(circle);
					}
				}
			}
		},
		mapRenderPlot() {
			/*
			this.state.ui.map.sources.aircraftPlot.data.features = [];
			const simData = this.$root.$data.state.services.simulator;
			const rateImportance = Math.abs(simData.location.TR * 60 / 180);

			if(this.$os.UntrackedMap['p42_yoFlight_map_main'] && simData.location.GS > 60 && rateImportance > 0.05) {
				const feature = {
					type: 'Feature',
					id: 0,
					properties: {
						rate: 1
					},
					geometry: {
						type: 'MultiLineString',
						coordinates: [] as any
					},
				}

				feature.properties.rate = rateImportance > 1 ? 1 : rateImportance;

				const duration = 60;
				let hitAP = false;
				let timeAt = 0;
				let courseToGo = 0;
				let previousLocation = [simData.location.Lon, simData.location.Lat];
				let prevCourse = 0;
				while(timeAt < duration) {
					let offsetPos = previousLocation;
					const newCourse = simData.location.CRS + ((timeAt + 1) * simData.location.TR);
					if(prevCourse == 0) {
						prevCourse = newCourse;
					}

					const targetCrs = simData.autopilot.Hdg + simData.location.MagVar - (simData.location.Hdg - simData.location.CRS);
					if(simData.autopilot.On && simData.autopilot.HdgOn && (hitAP ? true : Eljs.BearingThresholdHitTest(prevCourse, newCourse, targetCrs))) {
						hitAP = true;
						offsetPos = Eljs.MapOffsetPosition(previousLocation[0], previousLocation[1], simData.location.GS * 0.514444, targetCrs);
					} else {
						offsetPos = Eljs.MapOffsetPosition(previousLocation[0], previousLocation[1], simData.location.GS * 0.514444, newCourse);
					}

					const greatCircle1 = turf.greatCircle(turf.point(previousLocation), turf.point(offsetPos));
					if(greatCircle1.geometry.type == 'LineString'){
						feature.geometry.coordinates.push(greatCircle1.geometry.coordinates);
					} else {
						greatCircle1.geometry.coordinates.forEach((pos: any) => {
							feature.geometry.coordinates.push(pos);
						});
					}

					prevCourse = newCourse;
					previousLocation = offsetPos;
					timeAt += 1;
				}
				this.state.ui.map.sources.aircraftPlot.data.features.push(feature);

				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setFeatureState({
					source: 'aircraftPlot',
					id: feature.id
				}, {
					state: Math.abs(this.$root.$data.state.services.simulator.location.TR) > 0.5 ? 'active': 'passive'
				});
			}
			*/
		},
		mapRenderPOIs() {

			this.state.ui.map.sources.poisNode.data.features = [];
			if(this.state.contracts.selected.Template && this.state.ui.map.displayLayer.poi) {
				if(this.state.contracts.selected.Template.POIs) {
					this.state.contracts.selected.Template.POIs.forEach(poi => {
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
				}
			}
		},
		mapUpdateFlightPlan(force :boolean) {
			if(this.state.ui.isFlying){
				const plan = this.state.contracts.selected.Plan;
				if(this.state.contracts.selected.At != this.state.contracts.selected.AtPrev || force) {
					this.state.contracts.selected.AtPrev = this.state.contracts.selected.At;
					this.state.ui.map.sources.planPaths.data.features.forEach((feature, index) => {
						//if() {
							feature.properties.state = this.state.contracts.selected.At == index ? 'active' : 'passive';
							//feature.properties.state = index == 1 ? 'active' : 'passive';
							//this.mapSetState('planPaths', plan.UID + index, 'active');

						//} else {
							//this.mapSetState('planPaths', plan.UID + index, 'passive');
						//}
						//console.log(plan.UID + index);
						//console.log(this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getSource('planPaths')._data.features[index].id);
						//console.log(feature.properties.state);
					});
					//state
					//console.log(this.state.ui.map.sources.planPaths.data);
					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getSource('planPaths').setData(this.state.ui.map.sources.planPaths.data);
				}
			}
		},

		mapMoveDone() {
			if(this.state.ui.map.loaded){
				const centerLocation = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getCenter();
				this.state.ui.map.location = [centerLocation.lng, centerLocation.lat];
				this.state.ui.map.heading = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getBearing();
				this.state.ui.map.pitch = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getPitch();
				this.state.ui.map.zoom = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getZoom();
				this.mapGetAirports();
				this.stateSave();
			}
		},
		mapNightLightSet(state :boolean) {
			if(this.$os.UntrackedMap['p42_yoFlight_map_main']) {
				if(state != this.state.ui.map.dynamicLayers.nightlights) {
					if(this.$os.UntrackedMap['p42_yoFlight_map_main'].map) {
						this.state.ui.map.dynamicLayers.nightlights = state;
						this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('rl major dots', 'visibility', this.state.ui.map.dynamicLayers.nightlights ? 'visible' : 'none');
						this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setLayoutProperty('rl major blur', 'visibility', this.state.ui.map.dynamicLayers.nightlights ? 'visible' : 'none');
					}
				}
			}
		},
		mapTrackToggle() {
			this.state.ui.map.tracking.enabled = !this.state.ui.map.tracking.enabled;
			if(this.state.ui.map.tracking.enabled) {
				this.mapTrackUpdate();
			}
		},
		mapTrackUpdate() {
			if(this.$root.$data.state.services.simulator.live && this.$os.UntrackedMap['p42_yoFlight_map_main'] && !this.app.app_sleeping) {

				if(this.state.ui.map.tracking.enabled) {
					const simData = this.$root.$data.state.services.simulator;
					const appFrameRef = (this.$refs['app-frame'] as HTMLElement);
					let zoom = 10;
					let heading = 0;
					let speed = 0;
					let pitch = 0;
					let xOffset = 0;
					let yOffset = 0;

					if ((this.state.ui.map.tracking.tilted && !this.state.ui.map.tracking.north)) {
						pitch = 68;
					} else {
						pitch = 0;
						this.state.ui.map.tracking.tilted = false;
					}

					//speed = 1000;
					if(!this.state.ui.map.tracking.north) {
						const rawZoom = (22 - simData.location.GS / 200);
						const turnRateEff = simData.location.TR * (simData.location.GS / 200);
						zoom = Eljs.limiter(8, 20, -6 + Eljs.Easings.easeInExpo(0, rawZoom, 0, 22, 22));

						if(simData.location.GS > 20) {
							heading = Math.abs(turnRateEff) > 5 ? simData.location.CRS + Eljs.limiter(-35, 35, (turnRateEff * 5)) : simData.location.CRS;
							xOffset -= Eljs.limiter(-appFrameRef.offsetWidth / 4, appFrameRef.offsetWidth / 4, (appFrameRef.offsetWidth) * turnRateEff / 25);
						} else if(simData.location.GS > 5) {
							heading = Math.abs(turnRateEff) > 5 ? simData.location.Hdg + Eljs.limiter(-35, 35, (turnRateEff * 5)) : simData.location.Hdg;
						} else {
							heading = simData.location.Hdg;
						}

						yOffset = (230 - ((appFrameRef.offsetHeight) / 2)) * -1;
					} else {
						const rawZoom = (22 - simData.location.GS / 150);
						zoom = Eljs.limiter(8, 20, -6 + Eljs.Easings.easeInExpo(0, rawZoom, 0, 22, 22));

						yOffset = -20;
					}


					const opt = {
						center: [this.$root.$data.state.services.simulator.location.Lon, this.$root.$data.state.services.simulator.location.Lat],
						duration: speed,
						zoom: 0,
						offset: [xOffset, yOffset],
						easing: (t :number) => {
							return 1 - Math.pow(1 - t, 5);
						},
					};

					if(this.state.ui.map.tracking.autozoom) {
						opt.zoom = Eljs.limiter(4, 24, zoom);// * this.state.ui.map.zoomOffset;
					} else {
						opt.zoom = this.state.ui.map.zoom;
					}

					if(!this.state.ui.map.tracking.north) {
						opt['bearing'] = heading;
					} else {
						opt['bearing'] = 0;
					}

					if(this.state.ui.map.tracking.tilted) {
						opt['pitch'] = pitch;
					} else {
						opt['pitch'] = 0;
					}

					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.flyTo(opt);
				}

				if(this.state.ui.map.displayLayer.terrain_avoidance) {

					const zoom = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getZoom();
					if(zoom < 13) {

						let AltMeters = (this.$root.$data.state.services.simulator.location.Alt * 0.3048);
						let Spacing = 10;

						if(this.$root.$data.state.services.simulator.location.GS < 60) {
							AltMeters = 999999;
						}

						if(zoom < 9) {
							Spacing = 500;
						} else if(zoom < 10) {
							Spacing = 200;
						} else if(zoom < 11) {
							Spacing = 100;
						} else if(zoom < 12) {
							Spacing = 50;
						} else if(zoom < 13) {
							Spacing = 20;
						}

						//console.log(zoom);
						//console.log("Spacing: " + Spacing);
						AltMeters -= Spacing;

						const newBracket = Math.floor(AltMeters / 10);
						if(this.state.ui.map.elevationBracket != newBracket) {
							this.state.ui.map.elevationBracket = newBracket;

							const elevWarnCode = [
								"interpolate",
								["linear"],
								["get", "ele"],
								-1200,
								"rgba(255, 0, 0, 0)",
								AltMeters - 300,
								"rgba(255, 0, 0, 0)",
								AltMeters - 299,
								"rgba(255, 0, 0, 1)",
								999999,
								"rgba(255, 0, 0, 1)"
							];

							//console.log(JSON.stringify(elevWarnCode));
							this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setPaintProperty('elev-warn', 'fill-color', elevWarnCode);
						}
					}
				} else {
					this.state.ui.map.elevationBracket = -1000;
				}

			}
		},
		mapAutoZoomToggle() {
			this.state.ui.map.tracking.autozoom = !this.state.ui.map.tracking.autozoom;
			this.mapTrackUpdate();
		},
		mapNorth() {
			this.state.ui.map.tracking.north = !this.state.ui.map.tracking.north;
			if(!this.state.ui.map.tracking.enabled) {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.easeTo({
					bearing: 0,
				});
			} else {
				this.mapTrackUpdate();
			}
		},
		mapTiltToggle() {
			this.state.ui.map.tracking.tilted = !this.state.ui.map.tracking.tilted;
			if(!this.state.ui.map.tracking.enabled) {
				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.easeTo({
					pitch: this.state.ui.map.tracking.tilted ? 60 : 0,
				});
			} else {
				if(this.state.ui.map.tracking.tilted) {
					this.state.ui.map.tracking.north = false;
				}
				this.mapTrackUpdate();
			}
		},
		mapMakeAircraftPoly() {
			if(this.$os.UntrackedMap['p42_yoFlight_map_main']){
				const simData = this.$root.$data.state.services.simulator;
				let sourceData = this.state.ui.map.sources.aircraftPoly.data;
				const Existing = sourceData.features.filter(x => x.properties.id == 0);

				if(simData.connected && simData.live) {

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

					const tsize = Math.pow(2, (22 - (this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getZoom() * 0.9)) - 6);
					feature.properties.base = 0; //simData.location.GAlt * 0.3048;
					feature.properties.thick = feature.properties.base + (tsize * 2); //simData.location.GAlt * 0.3048 + (tsize * 2); //

					const curLon = simData.location.Lon;
					const curLat = simData.location.Lat;
					const curHdg = simData.location.GS > 20 ? simData.location.CRS : simData.location.Hdg;
					feature.geometry.coordinates[0] = [];

					const startNode = Eljs.MapOffsetPosition(curLon, curLat, tsize * 6, curHdg + 0);
					feature.geometry.coordinates[0].push(startNode); // Top tip
					feature.geometry.coordinates[0].push(Eljs.MapOffsetPosition(curLon, curLat, tsize * 12, curHdg + 143)); // Right tip
					feature.geometry.coordinates[0].push(Eljs.MapOffsetPosition(curLon, curLat, tsize * 6, curHdg + 180)); // Back tip
					feature.geometry.coordinates[0].push(Eljs.MapOffsetPosition(curLon, curLat, tsize * 12, curHdg - 143)); // Left tip
					feature.geometry.coordinates[0].push(startNode); // Closing

					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getSource('aircraftPoly').setData(sourceData);


					// Glow
					sourceData = this.state.ui.map.sources.aircraftPolyGlow.data;
					sourceData.features = [];
					sourceData.features.push({
						type: 'Feature',
						id: 56155,
						geometry: {
							type: 'Point',
							coordinates: [simData.location.Lon, simData.location.Lat]
						},
					});

					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getSource('aircraftPolyGlow').setData(sourceData);

				} else {
					Existing.forEach(element => {
						const Index = sourceData.features.indexOf(element);
						sourceData.features.splice(Index, 1);
					});
				}

			}
		},
		mapUpdateRadar() {
			// https://www.rainviewer.com/api.html
			//"https://tilecache.rainviewer.com/v2/radar/{ts}/{size}/{z}/{x}/{y}/{color}/{options}.png"
			clearInterval(this.state.ui.map.radarFetchTimer);
			let TimeIndex = 0;
			const fetchData = () => {
				fetch('https://api.rainviewer.com/public/maps.json', { method: 'get' })
				.then(response => response.json())
				.then((data) => {
					if(this.state.ui.map.loaded) {
						data.forEach((radarTime :number, index :number) => {
							let existing = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getSource('radar-tiles-' + index);
							if(!existing) {
								const size = this.$root.$data.state.device.isAppleWebkit ? 256 : 512;
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.addSource('radar-tiles-' + index, {
									"type": "raster",
									"tiles": [
										"https://tilecache.rainviewer.com/v2/radar/" + data[index] + "/" + size + "/{z}/{x}/{y}/4/1_0.png"
									],
									"tileSize": size
								});
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.addLayer({
									"id": 'radar-tiles-' + index,
									"type": "raster",
									"source": 'radar-tiles-' + index,
									"minzoom": 0,
									"maxzoom": 11.5
								});
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.moveLayer('radar-tiles-' + index, this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getStyle().layers[3].id);
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setPaintProperty('radar-tiles-' + index, 'raster-opacity', 0);
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setPaintProperty('radar-tiles-' + index, 'raster-fade-duration', 1000);
							} else {
								existing.tiles[0] = "https://tilecache.rainviewer.com/v2/radar/" + data[index] + "/512/{z}/{x}/{y}/4/1_0.png";
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.style._sourceCaches['radar-tiles-' + index].clearTiles();
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.style._sourceCaches['radar-tiles-' + index].update(this.$os.UntrackedMap['p42_yoFlight_map_main'].map.transform);
								this.$os.UntrackedMap['p42_yoFlight_map_main'].map.triggerRepaint();
							}

						});

						//clearInterval(this.state.ui.map.radarTimer);
						//this.state.ui.map.radarTimer = setInterval(() => {
						//	if(!this.state.ui.map.radarAnimated) {
						//		TimeIndex = data.length - 1;
						//	}
						//	if(TimeIndex < data.length) {
						//		for (let i = 0; i < data.length; i++) {
						//			if(i == TimeIndex) {
						//				this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setPaintProperty('radar-tiles-' + i, 'raster-opacity', 0.5);
						//			} else {
						//				setTimeout(() => {
						//					this.$os.UntrackedMap['p42_yoFlight_map_main'].map.setPaintProperty('radar-tiles-' + i, 'raster-opacity', 0);
						//				}, 30);
						//			}
						//		}
						//	}
						//	if(TimeIndex < data.length + 2) {
						//		TimeIndex++;
						//	} else {
						//		TimeIndex = 0;
						//	}
						//}, 250);
					}
				}).catch((err) => {

				});
			}
			fetchData();
			this.state.ui.map.radarFetchTimer = setInterval(() => fetchData, 600000);
		},
		mapGetAirports() {

			const bounds = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getBounds();
			const zoom = this.$os.UntrackedMap['p42_yoFlight_map_main'].map.getZoom();
			const tileScale = 0;

			const tilesMatrix = [];
			const tilesLon = [];
			const tilesLat = [];

			let NW = bounds.getNorthWest();
			let SE = bounds.getSouthEast();
			NW = [NW.lng, NW.lat];
			SE = [SE.lng, SE.lat];

			// Lon FLip
            let flipped = false;
            if (NW[0] - SE[0] > 180) { flipped = true; }
            if (NW[0] < -180) { NW = [NW[0] + 360, NW[1]]; }
            if (SE[0] > 180) { SE = [SE[0] - 360, SE[1]]; }
			if (NW[0] - SE[0] > 180) { flipped = true; }

			const MWF = [Math.floor(NW[0]), Math.max(Math.min(Math.floor(NW[1]), 70), -60)];
			const SEF = [Math.floor(SE[0]), Math.max(Math.min(Math.floor(SE[1]), 70), -60)];

			// Longitude
			let LonAt = MWF[0];
			if(flipped) {
				while(LonAt < 180) {
					tilesLon.push(LonAt);
					LonAt++;
				}
				LonAt = -180;
			}
			while(LonAt <= SEF[0]) {
				tilesLon.push(LonAt);
				LonAt++;
			}

			// Latitude
			let LatAt = SEF[1];
			while(LatAt <= MWF[1]) {
				tilesLat.push(LatAt);
				LatAt++;
			}

			// Matrix
			tilesLon.forEach(lon => {
				tilesLat.forEach(lat => {
					tilesMatrix.push(lon + '_' + lat);
				});
			});

			//console.log(AzureMaps.GetQuadkeysInBoundingBox([NW[0], SE[1], SE[0], NW[1]], zoom, 512));

			//this.$root.$data.services.api.SendWS(
			//	'adventures:query-from-filters', queryoptions, responseFn
			//);

		},

		flightSendToSim() {
			//this.state.contracts.selected.Plan
			const responseFn = (contractsData: any) => {

			};
			const queryoptions = {
				ID: this.state.contracts.selected.Contract.ID,
			}
			this.$root.$data.services.api.SendWS(
				'adventure:flightplan:send', queryoptions, responseFn
			);
		},
		flightFly(plan :any) {
			this.state.contracts.selected.Plan = plan;
			if(!this.state.ui.isFlying) {
				this.state.ui.isFlying = true;
				this.state.ui.map.tracking.enabled = true;
				this.state.ui.map.tracking.north = false;
				this.mapRenderFeatures();
				clearTimeout(this.state.ui.map.reframeTimeout);
				this.state.ui.map.reframeTimeout = setTimeout(() => {
					this.mapTrackUpdate();
					this.stateSave();
				}, 300);
			}
		},
		flightCancel() {
			this.state.ui.isFlying = false;
			this.state.contracts.selected.Plan = null;
			this.state.contracts.selected.Contract = null;
			this.state.contracts.selected.Template = null;
			this.contractsSearch();
			this.stateSave();
		},

		contractsSelect(contract: any, expand: boolean) {

			this.state.ui.map.tracking.enabled = false;
			//this.state.ui.map.tracking.tilted = false;

			if(contract) {
				let request = false;
				if(this.state.contracts.selected.Contract){
					if(this.state.contracts.selected.Contract.ID != contract.ID){
						request = true;
					}
				} else {
					request = true;
				}

				if(request) {
					this.state.contracts.selected.Contract = contract;
					this.state.contracts.selected.Template = this.state.contracts.actives.Templates.find(x => x.FileName == contract.FileName);
					this.state.ui.map.sources.planPaths.data.features = [];

					if(expand){
						this.planSelect(contract.Flightplans[0], false);
					}
				} else {
					if(contract.Flightplans.length){
						this.planSelect(contract.Flightplans[0], false);
					}
				}

				const nodes = [] as any[];
				contract.Situations.forEach((sit : any) => {
					nodes.push(sit.Location);
				});

				clearTimeout(this.state.ui.map.reframeTimeout);
				this.state.ui.map.reframeTimeout = setTimeout(() => {
					MapboxExt.fitBoundsExt(this.$os.UntrackedMap['p42_yoFlight_map_main'].map, turf.bbox(turf.lineString(nodes)), {
						padding: this.state.ui.map.padding,
						pitch: 0,
						bearing: 0,
						duration: 1000,
					}, null);
				}, 300);
			} else {
				this.state.contracts.selected.Contract = null;
				this.state.contracts.selected.Template = null;
				this.state.contracts.selected.Plan = null;
			}

			this.contractCheckExpiration();
			this.mapRenderFeatures();
			this.stateSave();
		},
		contractsOffset(ref: string, change: number) {
			if(this.state.contracts.actives.Contracts.length){
				let index = 0;
				if(this.state.contracts.selected.Contract) {
					index = this.state.contracts.actives.Contracts.findIndex((x :any) => x.ID == this.state.contracts.selected.Contract.ID);
					index += change;
				}
				if(index >= this.state.contracts.actives.Contracts.length){
					index = 0;
				} else if(index < 0){
					index = this.state.contracts.actives.Contracts.length - 1;
				}
				this.contractsSelect(this.state.contracts.actives.Contracts[index], false);
			}
		},
		contractsClear(state: number) {
			if(state != 1){
				this.state.ui.map.sources.contractPaths.data.features = [];
			}
			this.state.contracts.actives.State = state;
			this.state.contracts.actives.Count = 0;
			this.state.contracts.actives.Limit = 0;
			this.state.contracts.actives.Contracts = [];
			this.state.contracts.actives.Templates = [];
			this.stateSave();
		},
		contractsSearch() {

			if(this.$root.$data.state.services.api.connected) {
				this.contractsClear(1);

				// Response function
				const responseFn = (contractsData: any) => {

					// Chear Search list
					this.state.contracts.actives.Contracts = contractsData.payload.Contracts;
					this.state.contracts.actives.Templates = contractsData.payload.Templates;
					this.state.contracts.actives.Count = contractsData.payload.Count;
					this.state.contracts.actives.Limit = contractsData.payload.Limit;
					this.state.contracts.actives.State = 0;

					if(this.state.contracts.selected.Contract) {
						this.state.contracts.selected.Contract = this.state.contracts.actives.Contracts.find(x => x.ID == this.state.contracts.selected.Contract.ID);
						this.state.contracts.selected.Template = this.state.contracts.actives.Templates.find(x => x.FileName == this.state.contracts.selected.Contract.FileName);
					}

					if(this.app.loaded_state != null) {
						if(!this.state.contracts.selected.restored) {
							this.state.contracts.selected.restored = true;

							if(this.app.loaded_state.contracts.selected.Contract) {
								this.contractsSelect(this.state.contracts.actives.Contracts.find(x => x.ID == this.app.loaded_state.contracts.selected.Contract), false);
							}

							if(this.$route.params.contract) {
								this.state.ui.isFlying = false;
								this.contractsSelect(this.state.contracts.actives.Contracts.find(x => x.ID == this.$route.params.contract), true);
							} else {
								if(this.app.loaded_state.contracts.selected.Plan && this.state.contracts.selected.Contract) {
									this.flightFly(this.app.loaded_state.contracts.selected.Plan);
								} else {
									this.state.ui.isFlying = false;
								}
							}
						} else {
							if(this.$route.params.contract) {
								if(this.app.loaded_state.contracts.selected.Contract != this.$route.params.contract) {
									this.state.ui.isFlying = false;
									this.contractsSelect(this.state.contracts.actives.Contracts.find(x => x.ID == this.$route.params.contract), true);
								}
							}
						}
					} else {
						this.state.contracts.selected.restored = true;
					}

					this.mapRenderFeatures();
				}

				const queryoptions = {
					state: 'Active,Saved',
					detailed: true,
					flightplans: true,
					sort: 'requested',
					sortAsc: false,
					limit: 30,
				}
				this.$root.$data.services.api.SendWS(
					'adventures:query-from-filters', queryoptions, responseFn
				);

			} else {
				this.contractsClear(-1);
			}
		},
		contractModalOpen() {

			// Return if already present
			if(this.$root.$data.state.ui.modals.queue.length) {
				if(this.$root.$data.state.ui.modals.queue[0].type != 'contract') {
					return;
				}
			}

			this.$os.modalPush({
				type: 'contract',
				title: 'Start fees',
				data: {
					App :this.app,
					Selected: this.state.contracts.selected,
				},
				func: () => {

				}
			});

		},
		contractCheckExpiration() {
			this.state.contracts.selected.ExpiresAt = this.state.contracts.selected.Contract ? new Date(this.state.contracts.selected.Contract.ExpireAt) : null;

		},


		planSelect(plan: any, open: boolean) {
			if(plan){
				let render = false;
				if(this.state.contracts.selected.Plan){
					if(this.state.contracts.selected.Plan.Hash != plan.Hash){
						render = true;
					}
				} else {
					render = true;
				}

				if(render){
					plan.At = 0;
					this.state.contracts.selected.Plan = plan;

					const nodes = [] as any[];
					plan.Waypoints.forEach((wp : any) => {
						nodes.push(wp.Location);
					});

					clearTimeout(this.state.ui.map.reframeTimeout);
					this.state.ui.map.reframeTimeout = setTimeout(() => {
						MapboxExt.fitBoundsExt(this.$os.UntrackedMap['p42_yoFlight_map_main'].map, turf.bbox(turf.lineString(nodes)), {
							padding: this.state.ui.map.padding,
							pitch: 0,
							duration: 1000,
						}, null);
					});
				} else {
					this.state.ui.currentModal = 'plan';
				}
			} else {
				this.state.contracts.selected.Plan = null;
			}
			this.mapRenderFeatures();
		},
		plansOffset(ref: string, change: number) {
			const contract = this.state.contracts.selected.Contract;
			if(contract){
				let index = 0;
				if(this.state.contracts.selected.Plan) {
					index = contract.Flightplans.findIndex((x :any) => x.Hash == this.state.contracts.selected.Plan.Hash);
					index += change;
				}
				if(index >= contract.Flightplans.length){
					index = 0;
				} else if(index < 0){
					index = contract.Flightplans.length - 1;
				}
				this.planSelect(contract.Flightplans[index], false);
			}
		},

		checkTime() {
			this.mapNightLightSet(this.$root.$data.state.ui.sunPosition[1] < 0);
		},

		interactAction(ev: Event, interaction: any) {
			this.$root.$data.services.api.SendWS(
				"adventure:interaction",
				{
					ID: this.state.contracts.selected.Contract.ID,
					Link: interaction.UID,
					Verb: interaction.Verb,
					Data: {},
				}
			);
		},
		dateBlockOpen() {

		},

		stateSave() {
			this.app.StateSave({
				contracts: {
					selected: {
						Contract: this.state.contracts.selected.Contract ? this.state.contracts.selected.Contract.ID : null,
						Template: this.state.contracts.selected.Template ? this.state.contracts.selected.Template.FileName : null,
						Plan: this.state.contracts.selected.Plan ? this.state.contracts.selected.Plan : null
					}
				},
				ui: {
					currentModal: this.state.ui.currentModal,
					isTracking: this.state.ui.isTracking,
					isFlying: this.state.ui.isFlying,
					map: {
						location: this.state.ui.map.location,
						zoom: this.state.ui.map.zoom,
						zoomOffset: this.state.ui.map.zoomOffset,
						heading: this.state.ui.map.heading,
						pitch: this.state.ui.map.pitch,
						displayLayer: {
							terrain_avoidance: this.state.ui.map.displayLayer.terrain_avoidance,
							weather_radar: this.state.ui.map.displayLayer.weather_radar,
							hillshading: this.state.ui.map.displayLayer.hillshading,
						},
						tracking: {
							enabled: this.state.ui.map.tracking.enabled,
							tilted: this.state.ui.map.tracking.tilted,
						},
					}
				}
			});
		},

		resetTheme() {
			const layers = this.state.ui.map.layers;
			if(this.$os.getConfig(['ui', 'theme']) == 'theme--dark'){
				layers.aircraftPoly.paint['fill-extrusion-color'] = '#00F5FF';
				layers.aircraftPolyGlow.paint['circle-color'] = '#00F5FF';
				layers.aircraftPlot.paint['line-color'] = '#FFFFFF';
				layers.runwayExtendedNames.paint['text-color'] = '#FFFFFF';
				layers.poisNode.paint['circle-color'] = '#cca34d';
				layers.poisNode.paint['circle-stroke-color'] = '#cca34d';
				layers.poisNodeLabel.paint['text-color'] = '#cca34d';
				layers.poisNodeLabel.paint['text-halo-color'] = '#222222';
				layers.situationRange.paint['fill-color'] = '#FFFFFF';
			} else {
				layers.aircraftPoly.paint['fill-extrusion-color'] = '#3333FF';
				layers.aircraftPolyGlow.paint['circle-color'] = '#3333FF';
				layers.aircraftPlot.paint['line-color'] = '#000000';
				layers.runwayExtendedNames.paint['text-color'] = '#000000';
				layers.poisNode.paint['circle-color'] = '#b38a33';
				layers.poisNode.paint['circle-stroke-color'] = '#b38a33';
				layers.poisNodeLabel.paint['text-color'] = '#b38a33';
				layers.poisNodeLabel.paint['text-halo-color'] = '#FFFFFF';
				layers.situationRange.paint['fill-color'] = '#222222';
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					if(this.state.ui.map.loaded){
						this.contractsSearch();
					}
					break;
				}
				case 'disconnect': {
					this.contractsClear(-1);
					this.state.ui.map.tracking.enabled = false;
					this.mapMakeAircraftPoly();
					this.mapNightLightSet(false);
					break;
				}
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							if(wsmsg.payload.persistence) {
								this.contractsSearch();
							}
							break;
						}
					}
					break;
				}
				case 'adventure':
				case 'flightplans': {
					this.$ContractMutator.EventInList(wsmsg, this.state.contracts.actives);
					//this.$ContractMutator.Event(wsmsg, this.state.contracts.selected.Contract, this.state.contracts.selected.Template);

					if(this.state.contracts.selected.Plan) {
						if(this.state.contracts.selected.Plan.Hash == wsmsg.payload.Hash) {
							if(wsmsg.payload.Updated) {
								this.state.contracts.selected.Plan = wsmsg.payload.Updated;
							}
						}
					}

					this.mapRenderFeatures();
					break;
				}
				case 'eventbus': {
					if(!this.app.app_sleeping) {
						this.checkTime();
						this.mapTrackUpdate();
						this.mapRenderPlot();
						this.mapUpdateFlightPlan(false);
						if(!this.state.ui.map.tracking.enabled) {
							this.mapMakeAircraftPoly();
						}
					}
					break;
				}
			}
		},
	}

});
</script>

<style lang="scss">
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
@import './../../../sys/scss/mixins.scss';
.p42_yoflight {

	.theme--bright &,
	&.theme--bright {
		.results {
			&_nav {
				&_back {
					background-image: url(../../../sys/assets/framework/dark/arrow_left.svg);
				}
				&_forward {
					background-image: url(../../../sys/assets/framework/dark/arrow_right.svg);
				}
			}
		}
		.map {
			background-color: #c6c5c3;
			.map_marker {
				&_navaid {
					background-image: url(../../../sys/assets/icons/dark/navaid_unknown.svg);
					&_intersection {
						background-image: url(../../../sys/assets/icons/dark/intersection.svg);
					}
					&_vor {
						background-image: url(../../../sys/assets/icons/dark/vor.svg);
					}
					&_airport {
						background-image: url(../../../sys/assets/icons/dark/airport.svg);
					}
				}
				&_poi {
					.symbol {
						background-image: radial-gradient(closest-side, rgba($ui_colors_bright_button_gold, 1) 0%, rgba($ui_colors_bright_button_gold, 0) 100%);
					}
				}
			}
			&_controls {
				&_autozoom {
					background-image: url(../../../sys/assets/icons/dark/autozoom.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/autozoom.svg);
					}
				}
				&_north {
					background-image: url(../../../sys/assets/icons/dark/north.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/north.svg);
					}
				}
				&_crs {
					background-image: url(../../../sys/assets/icons/dark/north.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/north.svg);
					}
				}
				&_track {
					background-image: url(../../../sys/assets/icons/dark/track.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/track.svg);
					}
				}
				&_elev {
					background-image: url(../../../sys/assets/icons/dark/terrain_avoidance.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/terrain_avoidance.svg);
					}
				}
				&_hill {
					background-image: url(../../../sys/assets/icons/dark/hillshade.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/hillshade.svg);
					}
				}
				&_poi {
					.icon {
						background-image: radial-gradient(closest-side, rgba($ui_colors_bright_button_gold, 1) 0%, rgba($ui_colors_bright_button_gold, 0) 100%);
					}
				}
			}
		}
		.bottom-blur {
			background-image: linear-gradient(to bottom, rgba($ui_colors_bright_shade_1, 0), cubic-bezier(.2,0,.4,1), rgba($ui_colors_bright_shade_1, 0.7));
		}
	}

	.theme--dark &,
	&.theme--dark {
		.results {
			&_nav {
				&_back {
					background-image: url(../../../sys/assets/framework/bright/arrow_left.svg);
				}
				&_forward {
					background-image: url(../../../sys/assets/framework/bright/arrow_right.svg);
				}
			}
		}
		.map {
			background-color: #00000f;
			.map_marker {
				&_navaid {
					background-image: url(../../../sys/assets/icons/bright/navaid_unknown.svg);
					//filter: drop-shadow(0 2px 2px #000);
					&_intersection {
						background-image: url(../../../sys/assets/icons/bright/intersection.svg);
					}
					&_vor {
						background-image: url(../../../sys/assets/icons/bright/vor.svg);
					}
					&_airport {
						background-image: url(../../../sys/assets/icons/bright/airport.svg);
					}
				}
				&_poi {
					.symbol {
						background-image: radial-gradient(closest-side, rgba($ui_colors_dark_button_gold, 1) 0%, rgba($ui_colors_dark_button_gold, 0) 100%);
					}
				}
			}
			&_controls {
				&_autozoom {
					background-image: url(../../../sys/assets/icons/bright/autozoom.svg);
				}
				&_north {
					background-image: url(../../../sys/assets/icons/bright/north.svg);
				}
				&_crs {
					background-image: url(../../../sys/assets/icons/bright/north.svg);
				}
				&_track {
					background-image: url(../../../sys/assets/icons/bright/track.svg);
				}
				&_elev {
					background-image: url(../../../sys/assets/icons/bright/terrain_avoidance.svg);
				}
				&_hill {
					background-image: url(../../../sys/assets/icons/bright/hillshade.svg);
				}
			}
		}
		.bottom-blur {
			background-image: linear-gradient(to bottom, rgba($ui_colors_dark_shade_1, 0), cubic-bezier(.2,0,.4,1), rgba($ui_colors_dark_shade_1, 0.7));
		}
	}

	.app-frame {
		align-items: stretch;
		flex-direction: row;
	}
	.top-content {
		position: absolute;
		left: 0;
		top: $status-size;
		right: 100px;
		pointer-events: none;
		& > .todolist {
			pointer-events: all;
			margin-left: 16px;
			max-width: 400px;
			padding: 8px;
			padding-bottom: 0;
			border-radius: 14px;
			backdrop-filter: blur(8px);
		}
		& > .limits {
			margin-left: 16px;
			padding: 8px;
		}
		& > .expires {
			margin-left: 16px;
			padding: 8px;
		}
	}
	.bottom-content {
		z-index: 2;
		.bottom-blur {
			position: absolute;
			right: 0;
			bottom: 0;
			left: 0;
			height: 250px;
			//backdrop-filter: blur(10px);
			pointer-events: none;
			//mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 0) 0px, rgba(0, 0, 0, 1) 30px);
		}
	}
	.results {
		&_section {
			.results_contracts{
				opacity: 0;
				transform: translateY(100%);
				.button_action,
				.button_nav {
					pointer-events: none;
				}
			}
			.results_plans {
				opacity: 0;
				transform: translateY(-100%);
				.button_action,
				.button_nav {
					pointer-events: none;
				}
				&.is-active {
					.plans_list {
						pointer-events: all;
					}
				}
			}
			.results_enroute {
				opacity: 0;
				transform: translateY(100%);
				.button_action,
				.button_nav {
					pointer-events: none;
				}
			}
			.is-active {
				opacity: 1;
				transform: translateY(0%);
				.card {
					pointer-events: all;
				}
				.button_action,
				.button_nav {
					pointer-events: all;
				}
				.hitbox {
					pointer-events: all;
				}
			}

			&.is-flying {
				.results_enroute {
					opacity: 1;
					transform: translateY(0%);
					.button_action,
					.button_nav {
						pointer-events: all;
					}
				}
			}
			&.has-selected {
				.results_plans {
					opacity: 1;
					transform: translateY(0%);
					.button_action,
					.button_nav {
						pointer-events: all;
					}
				}
			}
		}
		&_header {
			margin-bottom: $edge-margin / 2;
			h2 {
				font-family: "SkyOS-SemiBold";
				margin: 0;
			}
			.data-block {
				font-size: 0.9em;
				line-height: 1.4em;
				text-align: left;
				margin-left: 14px;
				span {
					display: block;
					&.value {
						font-family: "SkyOS-SemiBold";
					}
				}
			}
		}
		&_nav {
			display: flex;
			align-items: center;
			&_back {
				background-size: 18px;
				background-position: center;
				background-repeat: no-repeat;
				align-self: stretch;
			}
			&_forward {
				background-size: 18px;
				background-position: center;
				background-repeat: no-repeat;
				align-self: stretch;
			}
			.button_nav {
				margin-right: 8px;
			}
		}
		&_title {
			display: flex;
			align-items: center;
			margin: 0 1em;
		}
		&_contracts {
			position: absolute;
			bottom: $nav-size;
			left: 0;
			right: 0;
			margin-top: 16px;
			pointer-events: none;
			transition: opacity 0.2s ease-out, transform 0.2s ease-out;
		}
		&_plans {
			position: absolute;
			bottom: $nav-size;
			left: 0;
			right: 0;
			opacity: 0;
			pointer-events: none;
			transform: translateY(-100%);
			transition: opacity 0.2s ease-out, transform 0.2s ease-out;
		}
		&_enroute {
			position: absolute;
			bottom: 0;
			left: 0;
			right: 0;
			opacity: 0;
			pointer-events: none;
			transform: translateY(-100%);
			transition: opacity 0.2s ease-out, transform 0.2s ease-out;
		}
		&_data {
			display: flex;
			justify-content: space-between;
			& > div {
				flex-grow: 1;
				flex-basis: 0;
			}
		}
	}
	.map {
		position: relative;
		height: 100%;
		transition: opacity 2s ease-out;
		&-hidden {
			opacity: 0;
			transition: opacity 0s;
		}
		.map_marker {
			&_cargo_pickup {
				width: 24px;
				height: 24px;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
				background-image: url(../../../sys/assets/icons/cargo_pickup.svg);
			}
			&_cargo_dropoff {
				width: 24px;
				height: 24px;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
				background-image: url(../../../sys/assets/icons/cargo_dropoff.svg);
			}
			&_navaid {
				display: flex;
				align-items: center;
				width: 24px;
				height: 24px;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
				.label {
					font-family: "SkyOS-SemiBold";
					position: absolute;
					left: 30px;
				}
				.wx {
					position: absolute;
					left: 30px;
					top: 1.4em;
				}
			}
		}
		.mapboxgl-ctrl-bottom-left {
			z-index: 10;
			margin-left: 8px;
			margin-bottom: 8px;
		}
		.mapboxgl-ctrl-bottom-right {
			z-index: 10;
			margin-right: 4px;
			margin-bottom: 4px;
			opacity: 0.3;
		}
		&_controls {
			position: absolute;
			right: $edge-margin;
			top: $status-size;
			& > div {
				& > div {
					display: flex;
					align-items: center;
					background-position: center;
					background-repeat: no-repeat;
					height: 1.4em;
					width: 1.4em;
				}
			}
			&_north {
				background-size: 1.4em;
			}
			&_crs {
				background-size: 1.4em;
			}
			&_track {
				background-size: 1.4em;
			}
			&_elev {
				background-size: 1.6em;
			}
			&_hill {
				background-size: 1.6em;
			}
		}
	}
}
</style>