<template>
	<div class="contract_detailed" :class="[theme, { 'contained': contained }]" @click="select"  :style="!nobg ? state.ui.color ? 'background-color:' + (this.$root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : '' : ''">

		<div class="map" :class="{ 'expanded': state.ui.map.expanded }" v-if="map_id" @mouseleave="mapCollapse()">
			<MapboxFrame
				ref="map"
				mstyle="small"
				:id="map_id"
				:app="app"
				:mapTheme="$os.getConfig(['ui', 'theme'])"
				@load="mapLoaded"
				@resetTheme="resetTheme"
				>
				<template>
					<MglGeojsonLayer
						layerId="range"
						sourceId="range"
						:source="state.ui.map.sources.range"
						:layer="state.ui.map.layers.range" />

					<MglGeojsonLayer
						layerId="outRange"
						sourceId="outRange"
						:source="state.ui.map.sources.outRange"
						:layer="state.ui.map.layers.outRange" />

					<MglGeojsonLayer
						layerId="contractPaths"
						sourceId="contractPaths"
						:clearSource="false"
						:source="state.ui.map.sources.contractPaths"
						:layer="state.ui.map.layers.contractPaths" />

					<MglGeojsonLayer
						layerId="situationAirports"
						sourceId="situationAirports"
						:source="state.ui.map.sources.situationAirports"
						:layer="state.ui.map.layers.situationAirports"
					/>
					<MglGeojsonLayer
						layerId="situationAirportsLabel"
						sourceId="situationAirports"
						:source="state.ui.map.sources.situationAirports"
						:layer="state.ui.map.layers.situationAirportsLabel"
					/>

					<MglMarker v-if="$root.$data.state.services.simulator.live" :coordinates="[$root.$data.state.services.simulator.location.Lon, $root.$data.state.services.simulator.location.Lat]">
						<div class="map_marker map_marker_position" slot="marker">
							<div>
							</div>
						</div>
					</MglMarker>

				</template>
			</MapboxFrame>
		</div>

		<div class="background" v-if="contract.ImageURL.trim().length">
			<div class="blur" :style="'background-image: url(' + contract.ImageURL + ')'"></div>
			<div class="image" :style="'background-image: url(' + contract.ImageURL + ')'"></div>
			<div class="shade-bottom">
				<div>
					<h1 class="name">{{ template.Name.length ?  template.Name : contract.Route }}</h1>
					<div class="route" v-if="template.Name.length">{{ contract.Route }}</div>
				</div>
				<div class="companies" v-if="template.Company.length">
					<span class="value">
						<div class="company" :class="'company-' + company" v-for="(company, index) in template.Company" v-bind:key="index"></div>
					</span>
				</div>
			</div>
			<div class="flags">
				<div v-for="(country, index) in state.ui.countries" v-bind:key="index">
					<flags :code="country.toLowerCase()" />
				</div>
			</div>
			<div class="routecode" v-if="contract.RouteCode.length && $os.getConfig(['ui','tier']) != 'prospect'"><span>{{ contract.RouteCode }}</span></div>
		</div>
		<div class="header" v-else>
			<div class="flags">
				<div v-for="(country, index) in state.ui.countries" v-bind:key="index">
					<flags :code="country.toLowerCase()" />
				</div>
			</div>
			<div class="titles">
				<div>
					<h1 class="name">{{ template.Name.length ?  template.Name : contract.Route }}</h1>
					<div class="route" v-if="template.Name.length">{{ contract.Route }}</div>
				</div>
				<div>
					<div class="companies" v-if="template.Company.length">
						<span class="value">
							<div class="company" :class="'company-' + company" v-for="(company, index) in template.Company" v-bind:key="index"></div>
						</span>
					</div>
				</div>
			</div>
			<div class="routecode" v-if="contract.RouteCode.length && $os.getConfig(['ui','tier']) != 'prospect'"><span>{{ contract.RouteCode }}</span></div>
		</div>

		<ContractState v-if="showstate" type="full" :contract="contract" :template="template" @interactState="interactState"/>

		<div class="elevation-graph">
			<svg width="100%" height="100%" viewBox="0 0 100 100" preserveAspectRatio="none">
				<linearGradient :id="'gradient_d_' + contract.ID" x2="0" y2="1">
					<stop offset="0%" />
					<stop offset="100%" />
				</linearGradient>
				<polygon :fill="'url(#gradient_d_' + contract.ID + ')'" class="poly" :points="state.ui.elevationPoly"></polygon>
			</svg>
		</div>

		<div class="navigation">
			<div :class="{ 'active': state.ui.page == 'info' }" @click="state.ui.page = 'info'">
				<label_collapser :state="state.ui.page == 'info'">
					<template v-slot:icon>
						<div class="icon icon--info"></div>
					</template>
					<template v-slot:label>
						<div class="label">Info</div>
					</template>
				</label_collapser>
			</div>
			<div :class="{ 'active': state.ui.page == 'todos' }" @click="state.ui.page = 'todos'">
				<label_collapser :state="state.ui.page == 'todos'">
					<template v-slot:icon>
						<div class="icon icon--todos"></div>
					</template>
					<template v-slot:label>
						<div class="label">To-Do's</div>
					</template>
				</label_collapser>
			</div>
			<div :class="{ 'active': state.ui.page == 'memos' }" @click="state.ui.page = 'memos'">
				<label_collapser :state="state.ui.page == 'memos'">
					<template v-slot:icon>
						<div class="icon icon--memos"></div>
					</template>
					<template v-slot:label>
						<div class="label">Memos</div>
					</template>
				</label_collapser>
				<span class="badge positive" v-if="contract.Memos.Messages.length">{{ contract.Memos.Messages.length }}</span></div>
			<div :class="{ 'active': state.ui.page == 'docs' }" @click="state.ui.page = 'docs'" v-if="$root.$data.config.ui.tier != 'discovery'">
				<label_collapser :state="state.ui.page == 'docs'">
					<template v-slot:icon>
						<div class="icon icon--docs"></div>
					</template>
					<template v-slot:label>
						<div class="label">Fees</div>
					</template>
				</label_collapser>
				<span class="badge" :class="{ 'positive': contract.Invoices.TotalProfits > 0 }" v-if="invoiceUnpaid.length">{{ invoiceUnpaid.length }}</span></div>
			<!--<div :class="{ 'active': state.ui.page == 'requirements' }" @click.native="state.ui.page = 'requirements'">Requirements</div>-->
		</div>

		<div v-if="state.ui.page == 'info'" class="elevation-shaded">
			<div class="data-stack data-stack--vertical" v-if="template.AircraftRestrictionLabel.length">
				<div>
					<span class="label">Required Aircraft</span>
					<span class="value">{{ template.AircraftRestrictionLabel }}</span>
				</div>
			</div>
			<div class="data-stack data-stack--vertical" v-else>
				<div>
					<!--<span class="label text-center">Recommended aircraft</span>-->
					<RecomAircraft :contract="contract" />
				</div>
			</div>

			<div class="data-stack data-stack--vertical">
				<div>
					<span class="label">{{ contract.Situations.length > 2 ? 'Total distance' : 'Distance'}}</span>
					<span class="value">{{ contract.Distance.toLocaleString('en-gb') }} nm</span>
				</div>
				<div>
					<span class="label">Highest obstacle</span>
					<span class="value">{{ Math.round(Math.ceil(Math.max.apply(Math, (contract.Topo)) * 3.28084 / 100) * 100).toLocaleString('en-gb') }}′</span>
				</div>
			</div>

			<div class="description" v-if="contract.DescriptionLong || contract.Description">{{ contract.DescriptionLong ? contract.DescriptionLong : contract.Description }}</div>

			<ContractMedia v-if="contract.MediaLink.length" :contract="contract" :template="template"/>

			<div v-if="(contract.State == 'Listed' || contract.State == 'Active' || contract.State == 'Saved')" class="helper_edge_padding_top">
				<div class="data-stack data-stack--vertical">
					<div v-if="template.RunningClock">
						<span class="label" v-if="state.ui.expireAt > new Date()">Expires <countdown :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt" :only_hours="true"></countdown></span>
						<span class="label" v-else>Expired <countdown :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt" :only_hours="true"></countdown></span>
						<span class="value">{{ moment(state.ui.expireAt).utc().format('MMM Do, HH:mm [GMT]') }}</span>
					</div>
					<div v-else-if="contract.State == 'Listed'">
						<span class="label">Time</span>
						<span class="value"><strong>{{ template.TimeToComplete > 0 ? template.TimeToComplete.toLocaleString('en-gb') + 'h' : 'Infinite' }}</strong></span>
					</div>
					<div v-else-if="template.TimeToComplete > 0">
						<span class="label"><countdown :no_fix="true" :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt" :full="false" :capitalize="true" :only_hours="true"></countdown> remaining</span>
						<span class="value">{{ moment(state.ui.expireAt).utc().format('MMM Do, HH:mm [GMT]') }}</span>
					</div>
					<div v-else>
						<span class="label">Time remaining</span>
						<span class="value">Infinite</span>
					</div>
				</div>
			</div>

			<div v-if="$root.$data.config.ui.tier != 'discovery'">
				<ContractProfits :invoiceData="contract.Invoices" :contract="contract"/>

				<div class="data-stack data-stack--vertical" v-if="$root.$data.config.ui.tier != 'prospect'">
					<div class="progress">
						<div>
							<ProgressXPComp :gain="contract.RewardXP" :progressed="$root.$data.state.services.userProgress.XP.Balance"/>
						</div>
						<div>
							<ProgressMoralComp :gain="contract.RewardKarma" :progressed="$root.$data.state.services.userProgress.Karma.Balance"/>
						</div>
					</div>
				</div>
			</div>

		</div>
		<div v-else-if="state.ui.page == 'todos'" class="elevation-shaded">
			<div class="data-stack data-stack--vertical separator" v-if="contract.Limits.length">
				<div>
					<span class="label">Restrictions</span>
					<ContractLimits :contract="contract" :template="template"/>
				</div>
			</div>
			<ContractTodo @interactAction="interactAction" :contract="contract" :template="template"/>
		</div>

		<DocumentsSection v-else-if="state.ui.page == 'docs'" class="elevation-shaded" :contract="contract"/>

		<MemosSection v-else-if="state.ui.page == 'memos'" class="elevation-shaded" :contract="contract" :template="template"/>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import * as turf from '@turf/turf';
import MapboxExt from '@/sys/libraries/mapboxExt';
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import { MglMap, MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import ProgressXPComp from "@/sys/components/progress/xp_small.vue"
import ProgressMoralComp from "@/sys/components/progress/karma_small.vue"
import ContractMedia from "@/sys/components/contracts/contract_media.vue"
import ContractState from "@/sys/components/contracts/contract_state.vue"
import ContractLimits from "@/sys/components/contracts/contract_limits.vue"
import RecomAircraft from "@/sys/components/contracts/contract_recom_aircraft.vue"
import ContractTodo from "@/sys/components/contracts/contract_todo.vue"
import DocumentsSection from "@/sys/components/contracts/contract_detailed_docs.vue"
import MemosSection from "@/sys/components/contracts/contract_detailed_memos.vue"
import ContractProfits from "@/sys/components/contracts/contract_profits.vue"
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "contract_detailed",
	props: ['app', 'theme', 'defaultPage', 'nobg', 'map_id', 'showstate', 'contained', 'contextTop', 'contract', 'templates'],
	components: {
		DocumentsSection,
		ProgressXPComp,
		ProgressMoralComp,
		ContractMedia,
		ContractState,
		ContractLimits,
		ContractTodo,
		MemosSection,
		ContractProfits,
		RecomAircraft,
		MapboxFrame,
		MglMap,
		MglMarker,
		MglNavigationControl,
		MglGeojsonLayer,
	},
	data() {
		return {
			codeCumul: "",
			codeTO: null as any,
			template: null,
			state: {
				ui: {
					page: 'info',
					expireInterval: null,
					elevationPoly: "",
					elevationPolyTransition: null,
					isExpired: false,
					colorIsDark: false,
					color: null,
					colorDark: null,
					countries: [],
					expireAt: new Date(),
					pullAt: new Date(),
					map: {
						load: false,
						loaded: false,
						focused: false,
						expanded: false,
						location: [0,0],
						zoom: 2,
						heading: 0,
						pitch: 0,
						padding: { left: 200, top: 50, right: 50, bottom: 80 },
						moveTimeout: null,
						mapStyle: 'mapbox://styles/biarzed/cjie4rwi91za72rny0hlv9v7t',
						reloadOnActivate: false,
						//mapStyle: {
						//	version: 8,
						//	sources: {
						//	},
						//	glyphs: "mapbox://fonts/mapbox/{fontstack}/{range}.pbf",
						//	layers: [{
						//		"id": "map-tiles",
						//		"type": "raster",
						//		"source": "map-tiles",
						//		"minzoom": 0,
						//		"maxzoom": 22
						//	}]
						//},
						sources: {
							range: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							outRange: {
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
							situationAirports: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
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
							outRange: {
								type: "line",
								source: "outRange",
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
									'line-color': '#FFF',
									'line-width': 2,
									'line-opacity': 0.5
								},
								filter: ['in', '$type', 'LineString']
							},
							situationAirportsLabel: {
								id: "situationAirportsLabel",
								type: 'symbol',
								source: 'situationAirports',
								layout: {
									"text-field": ['get', 'icao'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									'text-variable-anchor': ['top', 'bottom', 'left', 'right'], // 'top', 'bottom', 'left', 'right'
									'text-justify': 'left',
									'text-radial-offset': [
										"interpolate",
										["linear"],
										["zoom"],
										0, ['get', 'o0'],
										14, ['get', 'o14']
									],
									"text-size": 12,
								},
								paint: {
									'text-color': "#FFF",
								}
							},
							situationAirports: {
								id: "situationAirports",
								type: 'circle',
								source: 'situationAirports',
								paint: {
									'circle-color': '#FFFFFF',
									'circle-pitch-alignment': 'map',
									'circle-stroke-color': '#FFFFFF',
									'circle-stroke-opacity': 0.2,
									'circle-stroke-width': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0,
										3, 0,
										5, ['get', 'w5'],
										14, ['get', 'w14']
									],
									'circle-radius': [
										"interpolate",
										["linear"],
										["zoom"],
										0, ['get', 'r0'],
										14, ['get', 'r14']
									]

								}
							},
						},
						markers: {
							clickLayer: null,
							airports: [],
						}
					},
				}
			}
		}
	},
	methods: {
		initCard() {
			this.state.ui.map.focused = false;

			if(this.contract.ImageURL.length) {
				this.$root.$data.services.colorSeek.find(this.contract.ImageURL, (color :any) => {
					this.state.ui.color = color.color;
					this.state.ui.colorDark = color.colorDark;
					this.state.ui.colorIsDark = color.colorIsDark;
				});
			} else {
				this.state.ui.colorIsDark = false;
				this.state.ui.color = null;
				this.state.ui.colorDark = null;
			}

			this.state.ui.countries = [];
			this.contract.Situations.forEach((sit :any) => {
				if(sit.Airport){
					if(!this.state.ui.countries.includes(sit.Airport.Country)) {
						this.state.ui.countries.push(sit.Airport.Country);
					}
				}
			});

			if(this.state.ui.countries.length > 5) {
				const fullList = this.state.ui.countries;
				const spacing = (fullList.length / 4);
				this.state.ui.countries = [];
				for (let i = 0; i < 4; i++) {
					const country = fullList[Math.floor(i * spacing)];
					this.state.ui.countries.push(country);
				}
				this.state.ui.countries.push("+" + (fullList.length - this.state.ui.countries.length - 1));
				this.state.ui.countries.push(fullList[fullList.length - 1]);
			}

			this.state.ui.isExpired = false;
			this.state.ui.expireInterval = setInterval(() => {
				this.checkExpired();
			}, 5000);
			this.checkExpired();
			this.createElevation();
		},
		createElevation() {

			// Create Points
			let pointList = [] as string[];
			pointList.push("0,100");
			const total = this.contract.Topo.length - 1;
			let maxVal = 2000;
			this.contract.Topo.forEach((topo :any) => {
				if(topo > maxVal) { maxVal = topo + 300; }
			});
			this.contract.Topo.forEach((topo :any, index :number) => {
				pointList.push(((index / total) * 100) + "," + (100 - ((topo / maxVal) * 100)));
			});

			pointList.push("100,0");
			pointList.push("100,100");

			// Setup Transition
			if(this.state.ui.elevationPoly) {
				clearInterval(this.state.ui.elevationPolyTransition);
				let animateAt = 0;
				const enterPoly = [] as number[];
				const exitPoly = [] as number[];
				const exitPolyRaw = this.state.ui.elevationPoly.split(' ');
				if(pointList.length == exitPolyRaw.length) {
					exitPolyRaw.forEach((node, index) => {
						enterPoly.push(parseFloat(pointList[index].split(',')[1]));
						exitPoly.push(parseFloat(node.split(',')[1]));
					});

					this.state.ui.elevationPolyTransition = setInterval(() => {
						if(animateAt >= 1) {
							this.state.ui.elevationPoly = pointList.join(' ');
							clearInterval(this.state.ui.elevationPolyTransition);
						} else {
							const pct = Eljs.Easings.easeInOutCubic(0, animateAt, 0, 1, 1);
							const transitionPoly = [];
							pointList.forEach((node, index) => {
								transitionPoly.push(node.split(',')[0] + ',' + (exitPoly[index] + ((enterPoly[index] - exitPoly[index]) * pct)));
							});

							this.state.ui.elevationPoly = transitionPoly.join(' ');
							animateAt += 0.05;
						}
					}, 10);
				} else {
					this.state.ui.elevationPoly = pointList.join(' ');
				}
			} else {
				this.state.ui.elevationPoly = pointList.join(' ');
			}

		},
		interactState(ev: Event, name: string) {
			this.$ContractMutator.Interact(this.contract, name, null, (response) => {
			});
		},
		interactAction(ev: Event, interaction: any) {
			this.$root.$data.services.api.SendWS(
				"adventure:interaction",
				{
					ID: this.contract.ID,
					Link: interaction.UID,
					Verb: interaction.Verb,
					Data: {},
				}
			);
		},
		select() {
			this.$emit('select', this.contract);
		},

		keyPressed(e: KeyboardEvent) {
			clearTimeout(this.codeTO);
			this.codeTO = setTimeout(() => {
				clearTimeout(this.codeTO);
				this.codeCumul = "";
				this.codeTO = null;
			}, 500);

			this.codeCumul += e.key;
			switch(this.codeCumul) {
				case 'share42': {
					if(this.$os.isDev) {
						this.$os.modalPush({
							type: 'sharecard',
							data: {
								contract: this.contract,
								template: this.template,
							}
						})
					}
					break;
				}
			}
		},

		mapLoaded(map: any) {
			this.$os.TrackMapLoad(this.$route.path);
			this.state.ui.map.loaded = true;
			this.mapMakeLines();

			['mousedown', 'touchstart', 'wheel'].forEach((ev) => {
				this.$os.UntrackedMap[this.map_id].map.on(ev, () => {
					this.state.ui.map.expanded = true;
				});
			});
			this.mapReframe(0);
		},
		mapMakeLines() {
			this.state.ui.map.sources.contractPaths.data.features = [];
			this.state.ui.map.sources.situationAirports.data.features = [];
			this.state.ui.map.sources.range.data.features = [];
			this.state.ui.map.sources.outRange.data.features = [];

			const feature = {
				type: 'Feature',
				id: this.contract.ID,
				geometry: {
					type: 'MultiLineString',
					coordinates: [] as any
				},
			}
			this.state.ui.map.sources.contractPaths.data.features.push(feature);

			const processedAirports = [] as string[];
			let previousNode = null as Array<number>;
			this.contract.Situations.forEach((situation: any, index: number) => {

				if(situation.Airport) {
					if(!processedAirports.includes(situation.Airport.ICAO)) {
						const airportFeature = {
							type: "Feature",
							id: Eljs.getNumGUID(),
							properties: {
								title: situation.Airport.ICAO + " - " + situation.Airport.Name,
								icao: situation.Airport.ICAO,
								r0: 2,
								r14: 5,
								w5: 5,
								w14: 8,
								o0: 0.3,
								o14: 1.3
							},
							geometry: {
								type: "Point",
								coordinates: situation.Location,
							}
						};
						this.state.ui.map.sources.situationAirports.data.features.push(airportFeature);
						processedAirports.push(situation.Airport.ICAO);
					}
				}

				if(index > 0){
					var start = turf.point(previousNode);
					var end = turf.point(situation['Location']);
					var greatCircle = turf.greatCircle(start, end, {
						properties: {
							name: "",
							id: ""
						}
					});

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

			if(!this.contract.IsMonitored && this.contract.RangeAirports) {
				this.contract.RangeAirports.forEach((apt :any) => {
					if(!processedAirports.includes(apt.ICAO)) {
						const airportFeature = {
							type: "Feature",
							id: Eljs.getNumGUID(),
							properties: {
								title: apt.ICAO + " - " + apt.Name,
								icao: apt.ICAO,
								r0: 1,
								r14: 2,
								w5: 2,
								w14: 3,
								o0: 0.3,
								o14: 0.9
							},
							geometry: {
								type: "Point",
								coordinates: apt.Location,
							}
						};
						this.state.ui.map.sources.situationAirports.data.features.push(airportFeature);
						processedAirports.push(apt.ICAO);
					}
				});
			}

			if(this.contract.LastLocationGeo && this.contract.State == 'Active' && !this.contract.IsMonitored && this.contract.LastLocationGeo) {

				//Main red
				this.state.ui.map.sources.range.data.features.push(turf.circle(this.contract.LastLocationGeo, 14.816, { steps: 90, units: 'kilometers' }));

				for (let i = 6; i < 35; i++) {
					const circle = turf.circle(this.contract.LastLocationGeo, (1 + Math.pow((i * 0.5), 3)), {
						steps: 90,
						units: 'kilometers',
						properties: {
							opacity: Eljs.round(Math.pow(0.9, i), 2) * 0.5
						}
					});
					this.state.ui.map.sources.outRange.data.features.push(circle);
				}
			}

		},
		mapCollapse() {
			this.state.ui.map.expanded = false;
			this.mapReframe(2000);
		},
		mapReframe(duration :number) {
			const nodes = [] as any[];
			if(this.contract.IsMonitored || this.contract.LastLocationGeo == null || this.contract.RangeAirports.length == 0 || this.contract.State != 'Active') {
				this.contract.Situations.forEach((sit :any, index :number) => {
					if(!this.contract.Path[index].Done || this.contract.State != 'Active') {
						nodes.push(sit.Location);
					}
				});
			} else {
				this.contract.RangeAirports.forEach((apt :any) => {
					nodes.push(apt.Location);
				});
			}

			if(this.contract.LastLocationGeo && this.contract.State == 'Active') {
				nodes.push(this.contract.LastLocationGeo);
			}

			if(this.$os.UntrackedMap[this.map_id] && nodes.length > 1) {
				MapboxExt.fitBoundsExt(this.$os.UntrackedMap[this.map_id].map, turf.bbox(turf.lineString(nodes)), {
					padding: { left: 60, top: 30, right: 60, bottom: 180 },
					pitch: 0,
					duration: duration,
				}, null);
			}
		},

		checkExpired() {
			if(this.contract.State == 'Listed') {
				if(this.state.ui.pullAt < new Date()){
					this.state.ui.isExpired = true;
					clearInterval(this.state.ui.expireInterval);
					this.$emit('expire', this.contract);
				}
			}
		},

		resetTheme() {
			const layers = this.state.ui.map.layers;
			if(this.$os.getConfig(['ui', 'theme']) == 'theme--dark'){
				layers.contractPaths.paint['line-color'] = "#FFFFFF";
				layers.situationAirportsLabel.paint['text-color'] = "#FFFFFF";
				layers.situationAirports.paint['circle-color'] = "#FFFFFF";
				layers.situationAirports.paint['circle-stroke-color'] = "#FFFFFF";
			} else {
				layers.contractPaths.paint['line-color'] = "#000000";
				layers.situationAirportsLabel.paint['text-color'] = "#000000";
				layers.situationAirports.paint['circle-color'] = "#000000";
				layers.situationAirports.paint['circle-stroke-color'] = "#000000";
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'adventure': {
					this.state.ui.expireAt = new Date(this.contract.ExpireAt);
					this.state.ui.pullAt = new Date(this.contract.PullAt);
					this.mapMakeLines();
					this.mapReframe(0);
					break;
				}
			}
		},
	},
	mounted() {
		if(this.defaultPage) {
			this.state.ui.page = this.defaultPage;
		}
		this.$emit('init');
		document.addEventListener('keypress', this.keyPressed);
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		clearInterval(this.state.ui.elevationPolyTransition);
		clearInterval(this.state.ui.expireInterval);
		this.$root.$off('ws-in', this.listenerWs);
		document.removeEventListener('keypress', this.keyPressed);
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.template = this.templates.find((x: any) => x.FileName == newValue.FileName);
					this.state.ui.expireAt = new Date(this.contract.ExpireAt);
					this.state.ui.pullAt = new Date(this.contract.PullAt);
					this.state.ui.isExpired = false;
					this.initCard();
					if(this.state.ui.map.loaded) {
						this.mapMakeLines();
						this.mapReframe(0);
					}
				}
			}
		}
	},
	computed: {
		toDosCount() :number {
			let count = 0;
			this.contract.Situations.forEach((sit, index) => {
				count++;
				if(this.contract.Path[index].Done) {
					count--;
				}
				this.contract.Path[index].Actions.forEach(act => {
					count++;
					if(act.Completed) {
						count--;
					}
				});
			});
			return count;
		},
		invoiceUnpaid() :any {
			return this.contract.Invoices.Invoices.filter(x => x.Status != 'PAID');
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.contract_detailed {
	display: flex;
	flex-grow: 1;
	flex-direction: column;
	background-color: transparent;
	will-change: transform;
	transition: background 1s $transition;

	.theme--bright &,
	&.theme--bright {
		color: $ui_colors_bright_shade_5;
		.map {
			&_marker {
				&_position {
					background-color: $ui_colors_bright_button_info;
					border: 3px solid $ui_colors_bright_shade_0;
					@include shadowed($ui_colors_bright_shade_5);
				}
			}
		}
		.elevation-graph {
			svg {
				stop:nth-child(1) {
					stop-color: rgba($ui_colors_bright_shade_5, 0.25);
				}
				stop:nth-child(2) {
					stop-color: rgba($ui_colors_bright_shade_5, 0.1);
				}
			}
		}
		.elevation-shaded {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		.navigation {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
			.icon {
				&--info {
					background-image: url(../../../sys/assets/icons/dark/info.svg);
				}
				&--todos {
					background-image: url(../../../sys/assets/icons/dark/todos.svg);
				}
				&--memos {
					background-image: url(../../../sys/assets/icons/dark/memos.svg);
				}
				&--docs {
					background-image: url(../../../sys/assets/icons/dark/docs.svg);
				}
			}
		}
		.background {
			@include shadowed($ui_colors_bright_shade_5);
		}
		.framed {
			background: rgba($ui_colors_bright_shade_5, 0.1);
		}
	}

	.theme--dark &,
	&.theme--dark {
		color: $ui_colors_dark_shade_5;
		.map {
			&_marker {
				&_position {
					background-color: $ui_colors_bright_button_info;
					border: 3px solid $ui_colors_bright_shade_0;
					@include shadowed($ui_colors_bright_shade_5);
				}
			}
		}
		.elevation-graph {
			svg {
				stop:nth-child(1) {
					stop-color: rgba($ui_colors_dark_shade_5, 0.25);
				}
				stop:nth-child(2) {
					stop-color: rgba($ui_colors_dark_shade_5, 0.1);
				}
			}
		}
		.elevation-shaded {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		.navigation {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			.icon {
				&--info {
					background-image: url(../../../sys/assets/icons/bright/info.svg);
				}
				&--todos {
					background-image: url(../../../sys/assets/icons/bright/todos.svg);
				}
				&--memos {
					background-image: url(../../../sys/assets/icons/bright/memos.svg);
				}
				&--docs {
					background-image: url(../../../sys/assets/icons/bright/docs.svg);
				}
			}
		}
		.background {
			@include shadowed($ui_colors_dark_shade_0);
		}
		.framed {
			background: rgba($ui_colors_dark_shade_3, 0.1);
		}
	}

	&.contained {
		border-radius: 14px;
		overflow: hidden;

		.background {
			margin: 8px;
		}
	}

	.map {
		position: relative;
		height: 150px;
		margin-bottom: 8px;
		border-radius: 8px;
		overflow-y: hidden;
		transition: height 0.4s cubic-bezier(.15,0,0,1);
		&.expanded {
			height: 300px;
		}
		& > div {
			position: absolute;
			top: 0;
			right: 0;
			height: 300px;
			left: 0;
		}
		&_marker {
			&_position {
				width: 10px;
				height: 10px;
				border-radius: 50%;
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
	}
	.background {
		position: relative;
		background-color: rgba(0,0,0,0.1);
		height: 180px;
		min-height: 180px;
		margin-bottom: 8px;
		border-radius: 8px;
		.flags {
			font-size: 1.5em;
			display: flex;
			margin-left: 6px;
			padding-top: 6px;
			align-items: stretch;
			& > div {
				font-size: 1em;
				margin-right: 5px;
				display: flex;
				align-items: stretch;
				color: #FFF;
				font-family: "SkyOS-SemiBold";
			}
			.flag {
				border-radius: 4px;
				display: flex;
				box-shadow: 0 1.5px 4px rgba(0,0,0,0.5);
				&.flag-texted {
					font-size: 0.5em;
					padding: 0 1em;
				}
				span {
					display: flex;
					font-size: 0.5em;
					line-height: 0.5em;
				}
			}
		}
		.routecode {
			position: absolute;
			top: 6px;
			right: 6px;
			font-size: 0.85em;
			line-height: 1em;
			background-color: #222;
			background-image: url(../../../sys/assets/icons/bright/share.svg);
			background-size: 1em;
			background-position: 3px center;
			background-repeat: no-repeat;
			color: #FFF;
			padding: 2px 6px 2px 6px;
			border-radius: 0.5em;
			span {
				font-family: "SkyOS-SemiBold";
				letter-spacing: 0.05em;
				margin-left: 1.2em;
			}
		}
		.image {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 8px;
			background-size: cover;
			background-position: center;
		}
		.blur {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 50%;
			transform: scale(1.8);
			opacity: 0.75;
			z-index: -1;
			background-size: cover;
			background-position: center;
			filter: blur(20px) brightness(1) contrast(1);
			will-change: transform, filter;
		}
		.shade-bottom {
			display: flex;
			position: absolute;
			justify-content: space-between;
			left: 0;
			right: 0;
			bottom: 0;
			padding: 8px;
			border-radius: 8px;
			background: linear-gradient(to top, rgba(0,0,0,0.5), cubic-bezier(.2,0,.4,1), rgba(0,0,0,0));
			.name {
				font-size: 1.4em;
				line-height: 1.2em;
				white-space: normal;
				font-family: "SkyOS-SemiBold";
				margin-top: 32px;
				margin-bottom: 4px;
				color: #FFF;
				text-shadow: 0px 2px 5px #000;
			}
			.route {
				font-size: 12px;
				color: #FFF;
			}
		}
	}
	.header {
		position: relative;
		margin-top: 14px;
		margin-bottom: 14px;
		z-index: 2;
		& > div {
			flex-grow: 1;
			&.titles {
				display: flex;
				justify-content: space-between;
				align-items: center;
				.companies {
					margin-top: 4px;
				}
			}
			&.flags {
				font-size: 1.5em;
				display: flex;
				margin-right: 0.2em;
				flex-grow: 0;
				& > div {
					margin-right: 5px;
				}
				.flag {
					border-radius: 4px;
					display: block;
					&.flag-texted {
						font-size: 0.5em;
						padding: 0 1em;
					}
				}
			}
			&.routecode {
				position: absolute;
				top: 0;
				right: 0;
				font-size: 0.85em;
				line-height: 1em;
				background-color: #222;
				background-image: url(../../../sys/assets/icons/bright/share.svg);
				background-size: 1em;
				background-position: 3px center;
				background-repeat: no-repeat;
				color: #FFF;
				padding: 2px 6px 2px 6px;
				border-radius: 0.5em;
				span {
					font-family: "SkyOS-SemiBold";
					letter-spacing: 0.05em;
					margin-left: 1.2em;
				}
			}
			.name {
				font-size: 1.4em;
				line-height: 1em;
				margin-top: 8px;
				white-space: normal;
				font-family: "SkyOS-SemiBold";
				margin-bottom: 0;
			}
			.route {
				font-size: 12px;
			}
		}
	}

	.companies {
		display: flex;
		align-items: flex-end;
		margin-left: 4px;
		.company {
			width: 70px;
			height: 40px;
			margin-left: 4px;
			border-radius: 4px;
			background-size: contain;
			background-repeat: no-repeat;
			background-position: center;
			&-clearsky {
				background-color: #FFF;
				background-image: url(../../../sys/assets/icons/companies/logo_clearsky_l.svg);
			}
			&-coyote {
				background-color: #111;
				background-image: url(../../../sys/assets/icons/companies/logo_coyote_l.svg);
			}
			&-skyparktravel {
				background-color: #FFF;
				background-image: url(../../../sys/assets/icons/companies/logo_skyparktravel_l.svg);
			}
		}
	}

	.elevation-graph {
		position: relative;
		display: flex;
		align-items: flex-end;
		justify-content: center;
		height: 100px;
		svg {
			position: absolute;
			left: 0;
			right: 0;
			top: 0;
			bottom: 0;
		}
	}
	.elevation-shaded {
		padding: 0 14px;
		padding-top: 16px;
		padding-bottom: 4px;
		border-bottom-right-radius: 14px;
		border-bottom-left-radius: 14px;
		.data-stack {
			padding-top: 8px;
			margin-bottom: 8px;
			&:first-child {
				padding-top: 0;
			}
		}
	}

	.recommended_aircraft {
		margin-top: 4px;
	}
	.navigation {
		padding-top: 4px;
	}
	.description {
		font-size: 1.2em;
		padding-top: 8px;
		margin-bottom: 8px;
		white-space: pre-wrap;
	}
	.media-embed {
		padding-top: 16px;
		margin-bottom: 8px;
	}
	.todolist {
		padding-bottom: 16px;
	}
	.contract-profits {
		margin-top: 16px;
		margin-bottom: 8px;
	}
	.progress {
		& > div {
			margin-bottom: 14px;
			&:last-child {
				margin-bottom: 0;
			}
		}
	}
	.limits {
		padding-top: 8px;
		padding-bottom: 4px;

	}

}
</style>