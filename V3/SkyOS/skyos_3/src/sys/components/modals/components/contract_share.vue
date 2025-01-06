<template>
	<div class="contract_share theme--dark" :class="state.ui.mode">

		<div class="mood-color" v-if="md.data.contract ? md.data.contract.ImageURL.trim().length : false" :style="'background-image: url(' + md.data.contract.ImageURL + ')'"></div>

		<MapboxFrame
			ref="map"
			id="p42_sharecard_map_main"
			:mapStyle="state.ui.map.mapStyle"
			:demExaggeration="5"
			@load="mapLoaded"
			>
			<template>
				<MglGeojsonLayer
					layerId="contractPaths"
					sourceId="contractPaths"
					:source="state.ui.map.sources.contractPaths"
					:layer="state.ui.map.layers.contractPaths" />

				<MglGeojsonLayer
					layerId="poisNode"
					sourceId="poisNode"
					:source="state.ui.map.sources.poisNode"
					:layer="state.ui.map.layers.poisNode" />

				<MglGeojsonLayer
					layerId="poisNodeLabel"
					sourceId="poisNode"
					:source="state.ui.map.sources.poisNode"
					:layer="state.ui.map.layers.poisNodeLabel" />
			</template>
		</MapboxFrame>

		<div class="info-section-top" v-if="md.data.template && md.data.contract">
			<div class="centered">
				<div class="columns">
					<div class="column column_narrow">
						<div :class="'company-logo company-logo-' + md.data.template.Company[0]"></div>
					</div>
					<div class="column">
						<!--<textarea rows="1" class="pre-title" spellcheck="false" autocorrect="off" :value="state.ui.labels.na" @input="state.ui.labels.na = $event.target.value"/>-->
						<h1 class="title">{{ md.data.title }}</h1>
						<textarea rows="1" class="description" spellcheck="false" autocorrect="off" :value="state.ui.labels.description" @input="state.ui.labels.description = $event.target.value"/>
						<div class="post-title">
							<!--
							<div class="companies" v-if="md.data.template.Company.length">
								<span class="value">
									<div class="company" :class="'company-' + company" v-for="(company, index) in md.data.template.Company" v-bind:key="index"></div>
								</span>
							</div>
							-->
							<div>
								<!--<div>{{ state.ui.routeCount.toLocaleString('en-gb') }} {{ state.ui.routeCount == 1 ? 'Route' : 'Routes' }} </div>-->
								<div class="flags">
									<div v-for="(country, index) in state.ui.countries" v-bind:key="index">
										<flags :code="country.toLowerCase()" />
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>

		<div class="info-section-bottom" v-if="md.data.template && md.data.contract">
			<div class="memo-section" v-if="state.ui.name.length" :class="{ 'hidden': !state.ui.showChat }" @click="state.ui.showChat = !state.ui.showChat">
				<ChatFrame :thread="state.ui.memos" :hasScroll="false" :readOnly="true" :blured="true" />
			</div>
			<div class="centered">
				<div class="background" :style="'background-color: ' + (state.ui.color_bright ? state.ui.color_dark : '#000000') + ''"></div>
				<div class="columns columns_margined">
					<div class="column info-section-col">
						<div class="pilot-requires">Min. pilot requirements</div>
						<div>
							<div class="pilot-require">
								<div class="label">Level</div>
								<div class="value">{{ state.ui.requires.Level }}</div>
							</div>
							<div class="pilot-require">
								<div class="label">Karma</div>
								<div class="value">{{ state.ui.requires.Karma }}</div>
							</div>
							<div class="pilot-require">
								<div class="label">Reliability</div>
								<div class="value">{{ state.ui.requires.Reliability }}</div>
							</div>
						</div>
					</div>
					<div class="column info-section-col" v-if="md.data.template.TemplateCode">
						<textarea rows="1" class="pre-routecode" spellcheck="false" autocorrect="off" :value="state.ui.labels.preRoute" @input="state.ui.labels.preRoute = $event.target.value"/>
						<div class="v-centered">
							<div class="routecode"><span>#{{ md.data.template.TemplateCode.toUpperCase() }}</span></div>
						</div>
					</div>
					<div class="column info-section-col">
						<div class="tsp-logo"></div>
					</div>
				</div>
			</div>
		</div>

		<div class="controls">
			<button_action @click.native="close" class="cancel">Exit</button_action>
			<br>
			<button_action @click.native="setFormat('4_3')">4:3</button_action>
			<button_action @click.native="setFormat('16_9')">16:9</button_action>
			<button_action @click.native="setFormat('1_1')">1:1</button_action>
			<button_action @click.native="setFormat('9_16')">9:16</button_action>
			<button_action @click.native="setFormat('VID')">VID</button_action>
			<br>
			<textbox type="text" placeholder="Chat Name" v-model="state.ui.name" @modified="setName"></textbox>
			<br>
			<button_action @click.native="offsetOpacity('+')">Opacity +</button_action>
			<button_action @click.native="offsetOpacity('-')">Opacity -</button_action>
			<br>
			<button_action @click.native="toggleGetAll()">{{ this.state.ui.limit }}</button_action>
			<button_action @click.native="offsetLimit('+')">Limit +</button_action>
			<button_action @click.native="offsetLimit('-')">Limit -</button_action>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import * as turf from '@turf/turf';
import ChatFrame from "@/sys/components/messaging/chat.vue"
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import { MglMap, MglMarker, MglNavigationControl, MglGeojsonLayer, MglAttributionControl } from 'v-mapbox';
import { NavType, StatusType } from "@/sys/foundation/app_model"
import Textbox from '../../textbox.vue';

export default Vue.extend({
	name: "contract_share",
	props: ['md'],
	components: {
		MapboxFrame,
		MglMarker,
		MglAttributionControl,
		MglNavigationControl,
		MglGeojsonLayer,
		ChatFrame,
Textbox,
	},
	data() {
		return {
			codeCumul: "",
			codeTO: null as any,
			state: {
				ui: {
					mode: '4_3',
					color_is_dark: false,
					color_bright: null,
					color_dark: null,
					routeCount: 0,
					getAll: false,
					limit: 1500,
					opacity: 0.1,
					name: "Autumn",
					showChat: true,
					countries: [],
					labels: {
						na: '',
						preRoute: '',
						description: '',
					},
					requires: {
						Level: '',
						Karma: '',
						Reliability: '',
					},
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
						mapStyle: 'mapbox://styles/biarzed/ckhnvm48v0w7i19l2dv55e8p2',
						sources: {
							contractPaths: {
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
									'line-opacity': 0.1
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
									'circle-stroke-color': '#222222',
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
									'text-variable-anchor': ['top', 'bottom', 'left', 'right'], // 'top', 'bottom', 'left', 'right'
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
						}
					},
					memos: null,
				}
			}
		}
	},
	methods: {
		mapLoaded(map: any) {
			this.loadRoutes();
			this.setName();

			if(this.state.ui.labels.na == '') {
				this.state.ui.labels.na = 'New ' + this.md.data.template.TypeLabel + ' available!';
				this.state.ui.labels.preRoute = 'Find your ' + this.md.data.template.TypeLabel + ' by searching';
			}

			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('country-label-lg', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('country-label-md', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('country-label-sm', 'visibility', 'none');

			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('state-label-lg', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('state-label-md', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('state-label-sm', 'visibility', 'none');

			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('marine-label-lg-pt', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('marine-label-lg-ln', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('marine-label-md-pt', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('marine-label-md-ln', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('marine-label-sm-pt', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('marine-label-sm-ln', 'visibility', 'none');

			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-city-lg-n', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-city-lg-s', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-city-md-n', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-city-md-s', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-city-sm', 'visibility', 'none');


			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-islands-2', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-islands', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-islets-archipelago-aboriginal-2', 'visibility', 'none');
			this.$os.maps.untracked['p42_sharecard_map_main'].map.setLayoutProperty('place-islets-archipelago-aboriginal', 'visibility', 'none');

			this.$os.maps.untracked['p42_sharecard_map_main'].map.setPaintProperty('contractPaths','line-opacity', this.state.ui.opacity);

		},
		mapRenderPOIs() {

			this.state.ui.map.sources.poisNode.data.features = [];
			if(this.md.data.template) {
				if(this.md.data.template.POIs) {
					this.md.data.template.POIs.forEach(poi => {
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
		loadRoutes() {

			this.state.ui.map.sources.contractPaths.data.features = [];
			this.$root.$data.services.api.send_ws(
			'adventure:routes:get', {
				ID: this.md.data.contract.ID,
				GetAll: this.state.ui.getAll,
				limit: this.state.ui.limit,
			}, (routesData: any) => {

				this.state.ui.map.sources.contractPaths.data.features = [];
				this.state.ui.routeCount = routesData.payload.Count;

				const feature = {
					type: 'Feature',
					id: this.md.data.contract.ID,
					geometry: {
						type: 'MultiLineString',
						coordinates: [] as any
					},
				}

				routesData.payload.Routes.forEach((route: any, index1: number) => {
					let previousNode = null as Array<number>;
					route.forEach((situation: any, index: number) => {
						if(index > 0){
							var start = turf.point(previousNode);
							var end = turf.point(situation);
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
						previousNode = situation;
					});
				});

				this.state.ui.countries = [];
				routesData.payload.Countries.forEach((country :any) => {
					if(!this.state.ui.countries.includes(country)) {
						this.state.ui.countries.push(country);
					}
				});

				if(this.state.ui.countries.length > 8) {
					const fullList = this.state.ui.countries;
					this.state.ui.countries = [];
					Object.assign(this.state.ui.countries, fullList)
					while(this.state.ui.countries.length > 8) {
						this.state.ui.countries.pop();
					}
					this.state.ui.countries.push("+" + (fullList.length - this.state.ui.countries.length - 1));
				}

				this.state.ui.map.sources.contractPaths.data.features.push(feature);

				this.mapRenderPOIs();
			});
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
				case 'f': {
					break;
				}
			}
		},
		autosize(el :any) {
			if(!el.style) {
				el = el.target;
			}
			setTimeout(() => {
				el.style.cssText = 'height:auto; padding:0';
				el.style.cssText = 'height:' + el.scrollHeight + 'px';
			}, 0);
		},
		processRequires() {
			// Level
			if(this.md.data.template.RequiresLevel[0] >= 1) {
				if(this.md.data.template.RequiresLevel[1] > 9999) {
					this.state.ui.requires.Level = this.md.data.template.RequiresLevel[0] + ' or more';
				} else {
					this.state.ui.requires.Level = this.md.data.template.RequiresLevel[0] + ' to ' + this.md.data.template.RequiresLevel[1];
				}
			} else {
				if(this.md.data.template.RequiresLevel[1] > 9999) {
					this.state.ui.requires.Level = 'Any';
				} else {
					this.state.ui.requires.Level = this.md.data.template.RequiresLevel[1] + ' or below';
				}
			}

			// Karma
			if(this.md.data.template.RequiresKarma[0] > -42) {
				if(this.md.data.template.RequiresKarma[1] >= 42) {
					this.state.ui.requires.Karma = this.md.data.template.RequiresKarma[0] + ' or more';
				} else {
					this.state.ui.requires.Karma = this.md.data.template.RequiresKarma[0] + ' to ' + this.md.data.template.RequiresKarma[1];
				}
			} else {
				if(this.md.data.template.RequiresKarma[1] >= 42) {
					this.state.ui.requires.Karma = 'Any';
				} else {
					this.state.ui.requires.Karma = this.md.data.template.RequiresKarma[1] + ' or below';
				}
			}

			// Reliability
			if(this.md.data.template.RequiresReliability[0] > 0) {
				if(this.md.data.template.RequiresReliability[1] >= 100) {
					this.state.ui.requires.Reliability = this.md.data.template.RequiresReliability[0] + ' or more';
				} else {
					this.state.ui.requires.Reliability = this.md.data.template.RequiresReliability[0] + ' to ' + this.md.data.template.RequiresReliability[1];
				}
			} else {
				if(this.md.data.template.RequiresReliability[1] >= 100) {
					this.state.ui.requires.Reliability = 'Any';
				} else {
					this.state.ui.requires.Reliability = this.md.data.template.RequiresReliability[1] + ' or below';
				}
			}
		},
		close() {
			this.$os.modals.close();
			(this.$root as any).setWindowResize(false);
			this.$os.theme.setThemeLayer(null, 'sharecard');
		},
		offsetOpacity(c :string) {
			if(c == '+') {
				this.state.ui.opacity += 0.01;
			} else {
				this.state.ui.opacity -= 0.01;
			}
			if(this.state.ui.opacity < 0.01) { this.state.ui.opacity = 0.01; }
			if(this.state.ui.opacity > 1) { this.state.ui.opacity = 1; }

			this.$os.maps.untracked['p42_sharecard_map_main'].map.setPaintProperty('contractPaths','line-opacity', this.state.ui.opacity);
		},
		setName() {

			const newNameID = this.state.ui.name.replace(/[^A-Za-z0-9]/g, '_').toLowerCase();
			const newMemo = {
				"Members": {},
				"LastRead": new Date(),
				"Messages": [{
					"Member": newNameID,
					"Sent": new Date(),
					"Content": "🔊 New voice memo from " + this.state.ui.name + ".",
					"Type": "message"
				}]
			}
			newMemo.Members[newNameID] = this.state.ui.name;
			this.state.ui.memos = newMemo;
		},
		setFormat(type :string) {
			(this.$root as any).setWindowResize(true);
			(this.$root as any).setWindowFormat(type);
			this.state.ui.mode = type;

			setTimeout(() => {
				const tas = document.querySelectorAll("textarea");
				for (let i = 0; i < tas.length; ++i) {
					this.autosize(tas[i]);
				}
			}, 200);
		},
		offsetLimit(c :string) {
			if(c == '+') {
				this.state.ui.limit += 200;
			} else {
				this.state.ui.limit -= 200;
			}
			if(this.state.ui.limit < 100) { this.state.ui.limit = 100; }
			this.loadRoutes();
		},
		toggleGetAll() {
			this.state.ui.getAll = !this.state.ui.getAll;
			this.loadRoutes();
		},
	},
	beforeDestroy() {
		document.removeEventListener('keypress', this.keyPressed);
	},
	mounted() {
		document.addEventListener('keypress', this.keyPressed);

		this.setFormat('4_3');

		this.state.ui.labels.description = this.md.data.contract.Description;

		this.$os.theme.setThemeLayer({
			name: 'sharecard',
			bright: {
				status: StatusType.NONE,
				nav: NavType.NONE,
				shaded: false
			},
			dark: {
				status: StatusType.NONE,
				nav: NavType.NONE,
				shaded: false
			}
		})

		if(this.md.data.contract.ImageURL.length) {
			this.$os.colorSeek.find(this.md.data.contract.ImageURL, 160, (color :any) => {
				this.state.ui.color_bright = color.color_bright.h;
				this.state.ui.color_dark = color.color_dark.h;
				this.state.ui.color_is_dark = color.color_is_dark;
			});
		} else {
			this.state.ui.color_is_dark = false;
			this.state.ui.color_bright = null;
			this.state.ui.color_dark = null;
		}

		const tas = document.querySelectorAll("textarea");
		for (let i = 0; i < tas.length; ++i) {
			tas[i].addEventListener('keydown', this.autosize);
		}

		this.processRequires();
	},
	watch: {
		contract() {

		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';

.contract_share {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	background: #000;
	font-size: 2em;
	color: $ui_colors_dark_shade_5;

	.mood-color {
		filter: blur(100px) brightness(1) contrast(1) saturate(2);
	}

	.background {
		.blur {
			filter: blur(100px) brightness(1) contrast(3);
		}
	}

	& > .columns {
		margin: 100px;
	}

	.map {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		/deep/ .mapboxgl-control-container {
			position: absolute;
			bottom: 10px;
			left: 0;
			right: 0;
			display: flex;
			align-items: center;
			justify-content: center;
			opacity: 0.5;
			z-index: 100;
			& > div {
				display: block;
				position: relative;
			}
			.mapboxgl-ctrl-bottom-left {
				margin-right: 0.2em;
				//position: absolute;
				//left: 50%;
				//transform: translateX(-50%);
				.mapboxgl-ctrl {
					margin: 0;
				}
				.mapboxgl-ctrl-logo {
					display: none;
				}
				&::before {
					content: "Maps provided by";
				}
			}
			.mapboxgl-ctrl-attrib {
				background: transparent;
				a {
					color: #FFF;
				}
				.mapbox-improve-map {
					display: none;
				}
			}
		}
	}

	.flags {
		font-size: 1em;
		display: flex;
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
			border-radius: 5px;
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

	.controls {
		position: absolute;
		padding: 50px 20px 20px 20px;
		top: 0;
		left: 0;
		opacity: 0;
		font-size: 1.2rem;
		&:hover {
			opacity: 1;
		}
	}

	.info-section {
		//&-col {
		//	justify-content: center;
		//}
		&-top {
			position: absolute;
			top: 2em;
			right: 2em;
			left: 2em;
			@media only screen and (max-width: 720px) {
				top: 5.5em;
				right: 1em;
				left: 1em;
			}
			.centered {
				max-width: 900px;
				margin: 0 auto;
			}
			.column {
				&:last-child {
					display: flex;
					justify-content: center;
				}
			}
			.description {
				font-size: 1em;
				margin: 0;
				min-width: 1px;
				max-width: 900px;
				flex-grow: 1;
			}
			.company-logo {
				width: 7em;
				height: 7em;
				margin-right: 1em;
				margin-top: 0.2em;
				background-position: center;
				background-size: contain;
				background-repeat: no-repeat;
				@media only screen and (max-width: $bp-900) {
					width: 6em;
					height: 6em;
				}
				&-clearsky {
					background-image: url(../../../../sys/assets/icons/companies/bright/logo_clearsky.svg);
				}
				&-coyote {
					background-image: url(../../../../sys/assets/icons/companies/bright/logo_coyote.svg);
				}
				&-skyparktravel {
					background-image: url(../../../../sys/assets/icons/companies/bright/logo_skyparktravel.svg);
				}
			}
		}
		&-bottom {
			position: absolute;
			right: 2em;
			bottom: 0.75em;
			left: 2em;
			padding: 1em;
			font-size: 0.8em;
			.background {
				position: absolute;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				opacity: 0.3;
			}
			.centered {
				max-width: 850px;
				margin: 0 auto;
				padding: 1em;
				padding-top: 0.8em;
				text-align: center;
				border-radius: 1em;
				overflow: hidden;
				backdrop-filter: blur(1em);
			}
			.column {
				flex-basis: auto;
				.v-centered {
					display: flex;
					align-items: center;
					flex-grow: 1;
				}
				&:last-child {
					flex-grow: 0;
				}
			}
			.tsp-logo {
				width: 7em;
				height: 7em;
				margin-top: 0.2em;
				background-image: url(../../../../sys/assets/icons/bright/tsp_logo.svg);
				background-position: center;
				background-size: contain;
				background-repeat: no-repeat;
				@media only screen and (max-width: $bp-900) {
					width: 6em;
					height: 6em;
				}
			}
			@media only screen and (max-width: $bp-900) {
				right: 0em;
				left: 0em;
				bottom: 0em;
				padding: 0;
				.centered {
					border-radius: 0;
					padding-bottom: 3em;
				}
			}
			@media only screen and (max-width: 710px) {
				.column {
					margin-bottom: 2em;
					&:last-child {
						margin-bottom: 0;
					}
				}
				.columns {
					flex-direction: column;
				}
			}
		}
	}

	.memo-section {
		font-size: 1.4rem;
		&.hidden {
			opacity: 0;
		}
		& > .chat {
			min-width: 800px;
			margin-bottom: 1em;
			/deep/ .message {
				justify-content: center;
				.group {
					background: rgba(255,255,255,0.8) !important;
					color: #000 !important;
				}
			}
			@media only screen and (max-width: $bp-900) {
				min-width: 500px;
			}
		}
	}

	.mood-color {
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		transform: scale(1.5);
		background-size: cover;
		background-position: center;
		opacity: 0.5;
	}

	.pre-title {
		font-size: 1.2em;
		line-height: 1.2em;
		opacity: 0.8;
		text-shadow: 0 0.1em 0.2em rgba(#000, 0.5);
	}

	.title {
		font-size: 1.75em;
		line-height: 1.2em;
		text-align: left;
		margin: 0;
		text-shadow: 0 0.2em 0.8em rgba(#000, 0.5);
	}

	.post-title {
		display: flex;
		align-items: center;
		margin-top: 0.25em;
		line-height: 1em;
		text-shadow: 0 0.2em 0.8em rgba(#000, 0.5);
	}

	.companies {
		margin-right: 0.5em;
		margin-top: 0.4em;
		.company {
			width: 4em;
			height: 2.2em;
			border-radius: 0.3em;
			margin-right: 0.4em;
			background-size: contain;
			background-repeat: no-repeat;
			background-position: center;
			&-clearsky {
				background-color: #FFF;
				background-image: url(../../../../sys/assets/icons/companies/logo_clearsky_l.svg);
			}
			&-coyote {
				background-color: #111;
				background-image: url(../../../../sys/assets/icons/companies/logo_coyote_l.svg);
			}
			&-skyparktravel {
				background-color: #FFF;
				background-image: url(../../../../sys/assets/icons/companies/logo_skyparktravel_l.svg);
			}
		}
	}

	.pre-routecode {
		text-align: center;
		font-size: 1em;
		opacity: 0.5;
		margin-bottom: 0.8em;
	}

	.routecode {
		position: relative;
		display: inline-block;
		margin: 0 auto;
		font-size: 1.5em;
		background-color: #222;
		background-image: url(../../../../sys/assets/icons/bright/share.svg);
		background-size: 1em;
		background-position: 0.2em center;
		background-repeat: no-repeat;
		color: #FFF;
		padding: 0.1em 0.4em 0.1em 0.4em;
		border-radius: 0.5em;
		z-index: 2;
		span {
			font-family: "SkyOS-SemiBold";
			letter-spacing: 0.05em;
			margin-left: 1.2em;
		}
		@media only screen and (max-width: $bp-900) {
			font-size: 1.4em;
		}
	}

	textarea,
	input[type=text] {
		display: block;
		border: none;
		outline: none;
		min-width: 100%;
		width: 100%;
		resize: none;
		box-sizing: border-box;
		cursor: text;
		padding: 0;
		appearance: none;
    	overflow: hidden;
		background: transparent;
		color: #FFF;
		&:hover {
			resize: both;
		}
	}

	.pilot-requires {
		margin-bottom: 1em;
		opacity: 0.5;
	}

	.pilot-require {
		display: flex;
		justify-content: stretch;
		text-align: left;
		font-size: 0.8em;
		@media only screen and (max-width: 720px) {
			font-size: 1em;
		}
		& > div {
			flex-basis: 0;
			flex-grow: 1;
		}
		.label {
			text-align: right;
			margin-right: 0.6em;
		}
		.value {
			margin-left: 0.6em;
			font-family: "SkyOS-SemiBold";
		}
	}

	@media only screen and (max-width: 720px) {
		font-size: 1.6em;
	}

	@media only screen and (max-height: 720px) {
		.map {
			filter: blur(10px);
		}
		.info-section {
			&-top {
				display: none;
			}
			&-bottom {
				display: none;
			}
		}
	}
}



</style>