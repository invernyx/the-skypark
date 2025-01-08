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

export default Vue.extend({
	name: "mapbox_map",
	props: ['app', 'padding', 'mstyle', 'mapTheme', 'accessToken', 'demExaggeration', 'hasFog'],
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
						location: [-97.994961, 31.274877],
						zoom: 2,
						heading: 0,
						pitch: 0,
						padding: this.padding,
						mapStyle: '',
						accessToken: "pk.eyJ1IjoidHVyYm9mYW5kdWRlIiwiYSI6ImNtMG9idWlrNzAydmMya3Ewcm12MWZ1d2wifQ.kKB63Sv2O6oG7cC1ddWYUg",
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
		this.listenerConfig(['ui','theme'], this.mapTheme);

		fetch('https://api.mapbox.com/styles/v1/turbofandude/cm0yfufpt007t01ql5it44w0b?access_token=' + this.state.ui.map.accessToken, { method: 'get' })
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

		switch(this.mstyle) {
			case 'big': {
				this.state.ui.map.mapStyle = 'mapbox://styles/mapbox/dark-v11';
				this.state.ui.map.load = true;
				break;
			}
			case 'small': {
				this.state.ui.map.mapStyle = 'mapbox://styles/mapbox/dark-v11';
				this.state.ui.map.load = true;
				break;
			}
			case 'sat3d': {
				this.state.ui.map.mapStyle = 'mapbox://styles/mapbox/satellite-v9';
				this.state.ui.map.load = true;
				break;
			}
			default: {
				this.state.ui.map.mapStyle = this.mstyle;
				this.state.ui.map.load = true;
				break;
			}
		}		
		this.$root.$on('configchange', this.listenerConfig);
		document.addEventListener('keypress', this.keyPressed);
	},
	beforeDestroy() {
		this.state.ui.map.load = false;
		this.state.ui.map.loaded = false;
		this.$root.$off('configchange', this.listenerConfig);
	},
	deactivated() {
		if(this.$os.UntrackedMap[this.$el.id]) {
			setTimeout(() => {
				Object.keys(this.$os.UntrackedMap[this.$el.id].map.style._sourceCaches).forEach((cache :any) => {
					this.$os.UntrackedMap[this.$el.id].map.style._sourceCaches[cache].clearTiles();
				});
			}, 800);
		}
		document.removeEventListener('keypress', this.keyPressed);
	},
	activated() {
		if(this.state.ui.map.reloadOnActivate) {
			this.state.ui.map.reloadOnActivate = false;
			if(this.$os.UntrackedMap[this.$el.id].map) {
				this.resetTheme();
			}
		} else {
			if(this.$os.UntrackedMap[this.$el.id]) {
				Object.keys(this.$os.UntrackedMap[this.$el.id].map.style._sourceCaches).forEach((cache :any) => {
					this.$os.UntrackedMap[this.$el.id].map.style._sourceCaches[cache].update(this.$os.UntrackedMap[this.$el.id].map.transform);
				});
			}
		}
		setTimeout(() => {
			if(this.$os.UntrackedMap[this.$el.id]) {
				this.$os.UntrackedMap[this.$el.id].map.resize();
			}
		}, 800);
		document.addEventListener('keypress', this.keyPressed);
	},
	methods: {
		mapLoaded(map: any) {

			this.$os.UntrackedMap[this.$el.id] = map;
			this.$os.UntrackedMap[this.$el.id].map.transform._fov = 0.75;

			if(this.hasFog) {
				// Add Sky layer
				this.$os.UntrackedMap[this.$el.id].map.addLayer({
					id: 'sky-day',
					type: 'sky',
					paint: {
						'sky-type': 'gradient',
						'sky-opacity-transition': { 'duration': 500 },
						'sky-atmosphere-color': 'rgba(31, 31, 31, 0.2)',
					}
				});

				this.$os.UntrackedMap[this.$el.id].map.addLayer({
					id: 'sky-night',
					type: 'sky',
					paint: {
						'sky-type': 'atmosphere',
						'sky-atmosphere-sun': [90, 0],
						'sky-atmosphere-halo-color': 'rgba(255, 255, 255, 0.5)',
						'sky-atmosphere-color': 'rgba(255, 255, 255, 0.2)',
						'sky-opacity': 0,
						'sky-opacity-transition': { 'duration': 500 }
					}
				});
				//this.$os.UntrackedMap[this.$el.id].map.setFog({
				//	range: [0.5, 10],
				//	color: 'white',
				//	'horizon-blend': 0.2
				//});
			}

			window.requestAnimationFrame(() => {

				this.$os.UntrackedMap[this.$el.id].map.setZoom(this.state.ui.map.zoom);
				this.$os.UntrackedMap[this.$el.id].map.setBearing(this.state.ui.map.heading);
				this.$os.UntrackedMap[this.$el.id].map.setPitch(this.state.ui.map.pitch);

				['mousedown', 'touchstart','move', 'zoom', 'rotate', 'pitch'].forEach((ev) => {
					this.$os.UntrackedMap[this.$el.id].map.on(ev, () => {
						clearTimeout(this.state.ui.map.moveTimeout);
					});
				});

				['moveend', 'zoomend', 'rotateend', 'pitchend'].forEach((ev) => {
					this.$os.UntrackedMap[this.$el.id].map.on(ev, () => {
						clearTimeout(this.state.ui.map.moveTimeout);
						this.state.ui.map.moveTimeout = setTimeout(() => {
							this.mapMoveDone();
						}, 300);
					});
				});

				this.$emit('load', map);
				this.resetTheme();

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

		resetTheme() {
			if(this.$el) {
				if(this.$os.UntrackedMap[this.$el.id]) {

					let jsonStyle = null;
					switch(this.mstyle) {
						case 'big': {
							if(this.mapTheme == 'theme--bright'){
								jsonStyle = require('@/sys/libraries/mapbox-vue/bright');
							} else {
								jsonStyle = require('@/sys/libraries/mapbox-vue/dark');
							}
							break;
						}
						case 'small': {
							if(this.mapTheme == 'theme--bright'){
								jsonStyle = require('@/sys/libraries/mapbox-vue/small-bright');
							} else {
								jsonStyle = require('@/sys/libraries/mapbox-vue/small-dark');
							}
							break;
						}
					}

					if(jsonStyle) {
						const layers = this.$os.UntrackedMap[this.$el.id].map.getStyle().layers;
						jsonStyle.layers.forEach(newLayer => {
							const existingLayer = layers.find(x => x.id == newLayer.id);
							if(existingLayer) {
								if(existingLayer.paint) {
									const keys = Object.keys(existingLayer.paint);
									keys.forEach((key) => {
										this.$os.UntrackedMap[this.$el.id].map.setPaintProperty(existingLayer.id, key, newLayer.paint[key]);
									});
								}
							}
						});
					}

					if(this.hasFog) {
						//if(this.mapTheme == 'theme--dark') {
						//	this.$os.UntrackedMap[this.$el.id].map.setPaintProperty('sky-day', 'sky-opacity', 0);
						//	this.$os.UntrackedMap[this.$el.id].map.setPaintProperty('sky-night', 'sky-opacity', 1);
						//	this.$os.UntrackedMap[this.$el.id].map.setFog({ 'color': 'rgba(15, 15, 15, 1)' });
						//} else {
						//	this.$os.UntrackedMap[this.$el.id].map.setPaintProperty('sky-day', 'sky-opacity', 1);
						//	this.$os.UntrackedMap[this.$el.id].map.setPaintProperty('sky-night', 'sky-opacity', 0);
						//	this.$os.UntrackedMap[this.$el.id].map.setFog({ 'color': 'rgba(235, 231, 208, 1)' });
						//}
					}

					this.$emit('resetTheme');
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
			if(this.$os.isDev) {
				switch(this.codeCumul) {
					case '3d42': {
						if(!this.state.ui.map.has3D){
							this.state.ui.map.has3D = true;

							// Load 3D map source
							this.$os.UntrackedMap[this.$el.id].map.addSource('mapbox-dem', {
								type: 'raster-dem',
								url: 'mapbox://mapbox.mapbox-terrain-dem-v1',
								tileSize: 512,
								maxzoom: 14
							});

							// Create terrain
							this.$os.UntrackedMap[this.$el.id].map.setTerrain({
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

		listenerConfig(path: string[], value :any){
			switch(path[0]){
				case 'ui': {
					switch(path[1]){
						case 'theme': {
							if(this.app) {
								if(this.app.app_sleeping) {
									this.state.ui.map.reloadOnActivate = true;
								} else {
									this.resetTheme();
								}
							} else {
								this.resetTheme();
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
			this.resetTheme();
		}
	}
});
</script>

<style lang="scss" scoped>
	.map {
		height: 100%;
		transition: opacity 2s ease-out;
		&-hidden {
			opacity: 0;
			transition: opacity 0s;
		}
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