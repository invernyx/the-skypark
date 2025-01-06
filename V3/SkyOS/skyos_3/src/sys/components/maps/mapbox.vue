<template>
	<div :class="['map', { 'map-hidden': !state.ui.map.loaded }, { 'unauth': state.ui.unauth }]">
		<MglMap
			v-if="state.ui.map.load"
			:keyboard="false"
			:accessToken="state.ui.map.accessToken"
			:mapStyle.sync="state.ui.map.mapStyle"
			:maxBounds="[[-9999999, -90], [9999999, 90]]"
			:maxPitch="85"
			@load="mapLoaded"
			@error="mapError"
		>
			<slot/>
		</MglMap>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { MglMap } from 'v-mapbox';
import '@/sys/libraries/mapbox-vue/mapbox-gl.css';


/*

LAYERS
"id": "background",
"id": "water",
"id": "mapbox-satellite",
"id": "elev-warn",
"id": "Hillshade Highlight",
"id": "Hillshade Shadow",
"id": "hillshade-raster",
"id": "tunnel-street-minor-low",
"id": "tunnel-street-minor-case",
"id": "tunnel-primary-secondary-tertiary-case",
"id": "tunnel-major-link-case",
"id": "tunnel-motorway-trunk-case",
"id": "tunnel-construction",
"id": "tunnel-major-link",
"id": "tunnel-street-minor",
"id": "tunnel-primary-secondary-tertiary",
"id": "tunnel-motorway-trunk",
"id": "turning-feature-outline",
"id": "road-minor-low",
"id": "road-minor-case",
"id": "road-street-low",
"id": "road-street-case",
"id": "road-secondary-tertiary-case",
"id": "road-primary-case",
"id": "road-major-link-case",
"id": "road-motorway-trunk-case",
"id": "road-construction",
"id": "road-major-link",
"id": "road-minor",
"id": "road-street",
"id": "road-secondary-tertiary",
"id": "road-secondary-tertiary-lines",
"id": "road-primary",
"id": "road-primary-lines",
"id": "road-motorway-trunk",
"id": "road-motorway-trunk-lines",
"id": "road-rail",
"id": "road-rail-tracks",
"id": "bridge-street-minor-low",
"id": "bridge-street-minor-case",
"id": "bridge-primary-secondary-tertiary-case",
"id": "bridge-major-link-case",
"id": "bridge-motorway-trunk-case",
"id": "bridge-construction",
"id": "bridge-major-link",
"id": "bridge-street-minor",
"id": "bridge-primary-secondary-tertiary",
"id": "bridge-primary-secondary-tertiary-lines",
"id": "bridge-motorway-trunk",
"id": "bridge-motorway-trunk-lines",
"id": "bridge-major-link-2-case",
"id": "bridge-motorway-trunk-2-case",
"id": "bridge-major-link-2",
"id": "bridge-motorway-trunk-2",
"id": "bridge-motorway-trunk-2-lines",
"id": "rl major blur",
"id": "rl major dots",
"id": "bridge-rail-case",
"id": "bridge-rail",
"id": "bridge-rail-tracks",
"id": "natural-point-label",
"id": "road-intersection",
"id": "road-label",
"id": "road-number-shield",
"id": "road-exit-shield",
"id": "split",
"id": "aeroway-apron",
"id": "aeroway-taxiway",
"id": "aeroway-runway",
"id": "aeroway",
"id": "helipads",
"id": "Taxilines",
"id": "Taxiway-label",
"id": "runway-label",
"id": "building",
"id": "admin-3-4-boundaries",
"id": "admin-2-boundaries",
"id": "admin-2-boundaries-dispute",
"id": "place-islets-archipelago-aboriginal",
"id": "place-islets-archipelago-aboriginal-2",
"id": "place-islands",
"id": "place-islands-2",
"id": "place-city-sm",
"id": "place-city-md-s",
"id": "place-city-md-n",
"id": "place-city-lg-s",
"id": "place-city-lg-n",
"id": "marine-label-sm-ln",
"id": "marine-label-sm-pt",
"id": "marine-label-md-ln",
"id": "marine-label-md-pt",
"id": "marine-label-lg-ln",
"id": "marine-label-lg-pt",
"id": "state-label-sm",
"id": "state-label-md",
"id": "state-label-lg",
"id": "country-label-sm",
"id": "country-label-md",
"id": "country-label-lg",
"id": "waterway-label",
"id": "water-label",
"id": "ckusm2k7wa02j17ph2k3cue91",


*/


export default Vue.extend({
	name: "mapbox_map",
	props: ['app', 'padding', 'mapStyle', 'mapTheme', 'accessToken', 'demExaggeration', 'hasFog'],
	components: {
		MglMap,
	},
	data() {
		return {
			ready: true,
			codeCumul: "",
			codeTO: null as any,
			state: {
				ui: {
					unauth: false,
					map: {
						load: false,
						loaded: false,
						is3d: false,
						location: [-97.994961, 31.274877],
						zoom: 2,
						heading: 0,
						pitch: 0,
						padding: this.padding,
						mapStyle: '',
						accessToken: "pk.eyJ1IjoiYmlhcnplZCIsImEiOiJja3N1d3ltaWgxNDU1MnFwanZhNzB5dDVuIn0.jV7FuhxjP7J3ltZTLVXDAA",
						reloadOnActivate: false,
						moveTimeout: null,
						has3D: false,
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
					}
				}
			}
		}
	},
	beforeMount() {
		this.listener_config({
			name: ['ui','theme'],
			payload: this.mapTheme
		});

		fetch('https://api.mapbox.com/styles/v1/biarzed/cjie4rwi91za72rny0hlv9v7t?access_token=' + this.state.ui.map.accessToken, { method: 'get' })
		.then(response => response.json())
		.then((data) => {
			if(!data.name) {
				if(data.message == 'Not Authorized - Invalid Token'){
					this.state.ui.unauth = true;
					this.state.ui.map.loaded = true;
				}
			}
		}).catch((err) => {

		});

	},
	mounted() {

		if(this.accessToken) {
			this.state.ui.map.accessToken = this.accessToken;
		}

		switch(this.mapStyle) {
			case 'big': {
				this.state.ui.map.mapStyle = 'mapbox://styles/biarzed/ckusm2k7wa02j17ph2k3cue91?1=20220223';
				this.state.ui.map.load = true;
				break;
			}
			case 'sat3d': {
				this.state.ui.map.mapStyle = 'mapbox://styles/biarzed/cki82hahr6hmx19l7fin77k7b';
				this.state.ui.map.load = true;
				break;
			}
			default: {
				this.state.ui.map.mapStyle = this.mapStyle;
				this.state.ui.map.load = true;
				break;
			}
		}
		this.$os.eventsBus.Bus.on('configchange', this.listener_config);
		document.addEventListener('keypress', this.keyPressed);
	},
	beforeDestroy() {
		this.state.ui.map.load = false;
		this.state.ui.map.loaded = false;
		this.$os.eventsBus.Bus.off('configchange', this.listener_config);
	},
	deactivated() {
		if(this.$os.maps.untracked[this.$el.id]) {
			setTimeout(() => {
				Object.keys(this.$os.maps.untracked[this.$el.id].map.style._sourceCaches).forEach((cache :any) => {
					this.$os.maps.untracked[this.$el.id].map.style._sourceCaches[cache].clearTiles();
				});
			}, 800);
		}
		document.removeEventListener('keypress', this.keyPressed);
	},
	activated() {
		if(this.state.ui.map.reloadOnActivate) {
			this.state.ui.map.reloadOnActivate = false;
			if(this.$os.maps.untracked[this.$el.id].map) {
				this.theme_update();
			}
		} else {
			if(this.$os.maps.untracked[this.$el.id]) {
				Object.keys(this.$os.maps.untracked[this.$el.id].map.style._sourceCaches).forEach((cache :any) => {
					this.$os.maps.untracked[this.$el.id].map.style._sourceCaches[cache].update(this.$os.maps.untracked[this.$el.id].map.transform);
				});
			}
		}
		setTimeout(() => {
			if(this.$os.maps.untracked[this.$el.id]) {
				this.$os.maps.untracked[this.$el.id].map.resize();
			}
		}, 800);
		document.addEventListener('keypress', this.keyPressed);
	},
	methods: {
		mapLoaded(map: any) {

			this.$os.maps.untracked[this.$el.id] = Object.freeze(map);
			map.map.transform._fov = 0.70;

			if(this.hasFog) {
				// Add Sky layer
				//map.map.addLayer({
				//	id: 'sky-day',
				//	type: 'sky',
				//	paint: {
				//		'sky-type': 'gradient',
				//		'sky-opacity-transition': { 'duration': 500 },
				//		'sky-atmosphere-color': 'rgba(31, 31, 31, 0.2)',
				//	}
				//});

				//map.map.addLayer({
				//	id: 'sky-night',
				//	type: 'sky',
				//	paint: {
				//		'sky-type': 'atmosphere',
				//		'sky-atmosphere-sun': [90, 0],
				//		'sky-atmosphere-halo-color': 'rgba(255, 255, 255, 0.5)',
				//		'sky-atmosphere-color': 'rgba(255, 255, 255, 0.2)',
				//		'sky-opacity': 0,
				//		'sky-opacity-transition': { 'duration': 500 }
				//	}
				//});

				//map.map.setFog({
				//	range: [0.5, 10],
				//	color: 'white',
				//	'horizon-blend': 0.2
				//});
			}

			window.requestAnimationFrame(() => {

				map.map.setZoom(this.state.ui.map.zoom);
				map.map.setBearing(this.state.ui.map.heading);
				map.map.setPitch(this.state.ui.map.pitch);

				['mousedown', 'touchstart','move', 'zoom', 'rotate', 'pitch'].forEach((ev) => {
					map.map.on(ev, () => {
						clearTimeout(this.state.ui.map.moveTimeout);
					});
				});

				['moveend', 'zoomend', 'rotateend', 'pitchend'].forEach((ev) => {
					map.map.on(ev, () => {
						clearTimeout(this.state.ui.map.moveTimeout);
						this.state.ui.map.moveTimeout = setTimeout(() => {
							this.mapMoveDone();
						}, 300);
					});
				});

				// Create terrain
				//['pitchend'].forEach((ev) => {
				//	map.map.on(ev, () => {
				//		if(map.map.getPitch() < 5) {
				//			if(this.state.ui.map.is3d) {
				//				this.state.ui.map.is3d = false;
				//				this.$os.maps.untracked[this.$el.id].map.setTerrain(null);
				//			}
				//		} else {
				//			if(!this.state.ui.map.is3d) {
				//				this.state.ui.map.is3d = true;
				//				this.$os.maps.untracked[this.$el.id].map.setTerrain({
				//					'source': 'mapbox-dem',
				//					'exaggeration': [
				//						"interpolate",
				//						["linear"],
				//						["zoom"],
				//						14, 1,
				//						15, 0
				//					],
				//				});
				//			}
				//		}
				//	});
				//});

				// Load 3D map source
				this.$os.maps.untracked[this.$el.id].map.addSource('mapbox-dem', {
					type: 'raster-dem',
					url: 'mapbox://mapbox.mapbox-terrain-dem-v1',
					tileSize: 512,
					maxzoom: 14
				});

				this.$emit('load', map);
				this.theme_update();

				setTimeout(() => {
					this.state.ui.map.loaded = true;
				}, 200)
			});
		},
		mapError(ev :any) {
		},
		mapMoveDone() {
			this.$emit('mapMoveDone');
		},

		theme_update() {
			if(this.$el) {
				if(this.$os.maps.untracked[this.$el.id]) {

					let jsonStyle = null;

					switch(this.mapStyle) {
						case 'big': {
							if(this.mapTheme == 'theme--bright'){
								jsonStyle = require('@/sys/libraries/mapbox-vue/bright/style');
							} else {
								jsonStyle = require('@/sys/libraries/mapbox-vue/dark/style');

							}
							break;
						}
					}

					if(jsonStyle) {
						const layers = this.$os.maps.untracked[this.$el.id].map.getStyle().layers;
						jsonStyle.layers.forEach(newLayer => {
							const existingLayer = layers.find(x => x.id == newLayer.id);
							if(existingLayer) {
								if(existingLayer.paint) {
									const keys = Object.keys(existingLayer.paint);
									keys.forEach((key) => {
										this.$os.maps.untracked[this.$el.id].map.setPaintProperty(existingLayer.id, key, newLayer.paint[key]);
									});
								}
							}
						});

						this.$os.maps.untracked[this.$el.id].map.setLight(jsonStyle.light);

					}

					//if(this.hasFog) {
						//if(this.mapTheme == 'theme--dark') {
						//	this.$os.maps.untracked[this.$el.id].map.setPaintProperty('sky-day', 'sky-opacity', 0);
						//	this.$os.maps.untracked[this.$el.id].map.setPaintProperty('sky-night', 'sky-opacity', 1);
						//	this.$os.maps.untracked[this.$el.id].map.setFog({ 'color': 'rgba(15, 15, 15, 1)' });
						//} else {
						//	this.$os.maps.untracked[this.$el.id].map.setPaintProperty('sky-day', 'sky-opacity', 1);
						//	this.$os.maps.untracked[this.$el.id].map.setPaintProperty('sky-night', 'sky-opacity', 0);
						//	this.$os.maps.untracked[this.$el.id].map.setFog({ 'color': 'rgba(235, 231, 208, 1)' });
						//}
					//}

					this.$emit('theme_update');
				}
			}
		},

		keyPressed(e: KeyboardEvent) {
			clearTimeout(this.codeTO);
			this.codeTO = setTimeout(() => {
				clearTimeout(this.codeTO);
				this.codeCumul = "";
				this.codeTO = null;
			}, 500);

			this.codeCumul += e.key;
			if(this.$os.system.isDev) {
				switch(this.codeCumul) {
					case '3d42': {
						if(!this.state.ui.map.has3D){
							this.state.ui.map.has3D = true;

							// Load 3D map source
							this.$os.maps.untracked[this.$el.id].map.addSource('mapbox-dem', {
								type: 'raster-dem',
								url: 'mapbox://mapbox.mapbox-terrain-dem-v1',
								tileSize: 512,
								maxzoom: 14
							});

							// Create terrain
							this.$os.maps.untracked[this.$el.id].map.setTerrain({
								source: 'mapbox-dem',
								exaggeration: this.demExaggeration ? this.demExaggeration : 1,
							});

							console.log('now 3d');
						}
						break;
					}
				}
			}
		},

		listener_config(value :any){
			switch(value.name[0]){
				case 'ui': {
					switch(value.name[1]){
						case 'theme': {
							if(this.app) {
								if(this.app.sleeping) {
									this.state.ui.map.reloadOnActivate = true;
								} else {
									this.theme_update();
								}
							} else {
								this.theme_update();
							}
							break;
						}
					}
					break;
				}
			}
		},
	},
	watch: {
		mapTheme() {
			this.theme_update();
		}
	}
});
</script>

<style lang="scss" scoped>
	.map {
		height: 100%;
		transition: opacity 0.1s ease-out;
		//&-hidden {
		//	opacity: 0;
		//	transition: opacity 0s;
		//}
		/deep/ canvas {
			cursor: pointer;
		}

		&.unauth {
			background-image: url('https://storage.googleapis.com/gilfoyle/the-skypark/v1/common/images/misc/ed.jpg');
			background-size: cover;
			background-position: center;
			.mgl-map-wrapper {
				display: none;
			}
		}
	}
</style>