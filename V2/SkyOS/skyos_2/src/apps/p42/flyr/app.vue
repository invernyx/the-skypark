<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame">
			<width_limiter size="screen" :shadowed="true">

				<MapboxFrame
					ref="map"
					mstyle="sat3d"
					id="p42_flyr_map_main"
					:class="[{ 'move-right': state.ui.anim == 'right', 'move-left': state.ui.anim == 'left' }]"
					:app="app"
					:mapTheme="$os.getConfig(['ui', 'theme'])"
					@load="mapLoaded"
					>

					<MglGeojsonLayer
						layerId="runway"
						sourceId="runway"
						:source="state.ui.map.sources.runway"
						:layer="state.ui.map.layers.runway" />

				</MapboxFrame>

				<div class="section-bottom">
					<div class="section-background"></div>
					<div class="stack">
						<div class="btns" :class="{ 'disabled': state.ui.loading, 'move-right': state.ui.anim == 'right', 'move-left': state.ui.anim == 'left' }">
							<div class="btn btn-no" @click="act(false)"></div>
							<div class="btn btn-yes" @click="act(true)"></div>
						</div>
						<div class="cnt">

						</div>
					</div>
				</div>

			</width_limiter>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import * as turf from '@turf/turf';
import { AppInfo } from "./../../../sys/foundation/app_bundle"
import MapboxExt from '@/sys/libraries/mapboxExt';
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import { MglMap, MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import '@/sys/libraries/mapbox-vue/mapbox-gl.css';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "p42_flyr",
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
	},
	data() {
		return {
			state: {
				ui: {
					loading: true,
					airport: null,
					anim: null,
					map: {
						sources: {
							runway: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
						},
						layers: {
							runway: {
								type: "line",
								source: "runway",
								paint: {
									'line-color': '#FFFFFF',
									'line-width': 2,
									'line-opacity': 1,
									'line-dasharray': [1, 3],
								}
							},
						}
					}
				}
			}
		}
	},
	activated() {
		this.act();
	},
	mounted() {

	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
		document.addEventListener('keyup', this.keyStroke);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
		document.removeEventListener('keyup', this.keyStroke);
	},
	methods: {
		keyStroke(e :any) {
			switch(e.keyCode) {
				case 39: { // Yep
					this.act(true);
					break;
				}
				case 37: { // Nope
					this.act(false);
					break;
				}
			}
		},
		act(state = null) {
			if(this.$os.UntrackedMap['p42_flyr_map_main'] && (!this.state.ui.loading || !this.state.ui.airport)) {

				this.state.ui.loading = true;

				const opt = {};
				if(state != null) {
					opt['Review'] = {
						ICAO: this.state.ui.airport.ICAO,
						Like: state,
					}

					this.state.ui.anim = state ? 'right' : 'left';
				}

				this.state.ui.airport = null;

				this.$root.$data.services.api.SendWS(
					'flyr:next', opt, (d) => {

						setTimeout(() => {
							this.state.ui.airport = d.payload.Airport;

							const nodes = [this.state.ui.airport.Location];
							let longest = null;


							this.state.ui.airport.Runways.forEach(runway => {

								const threshold1 = Eljs.MapOffsetPosition(runway.Location[0], runway.Location[1], (runway.LengthFT / 2 * 0.3048) + 100, runway.Heading);
								const threshold2 = Eljs.MapOffsetPosition(runway.Location[0], runway.Location[1], (runway.LengthFT / 2 * 0.3048) + 100, runway.Heading + 180);

								nodes.push(threshold1);
								nodes.push(threshold2);

								if(longest) {
									if(longest.LengthFT < runway.LengthFT) {
										longest = runway;
									}
								} else {
									longest = runway;
								}

							});

							MapboxExt.fitBoundsExt(this.$os.UntrackedMap['p42_flyr_map_main'].map, turf.bbox(turf.lineString(nodes)), {
								padding: { left: 100, top: 100, right: 100, bottom: 270 },
								pitch: 0,
								bearing: longest.Heading,
								duration: 0,
							}, null);

							setTimeout(() => {
								this.state.ui.anim = null;
							}, 300);

							this.state.ui.loading = false;
						}, 300);

					}
				);
			}
		},

		mapLoaded(map: any) {
			this.$emit('loaded');
			this.$os.TrackMapLoad(this.$route.path);
			this.act();
			this.$os.UntrackedMap['p42_flyr_map_main'].map.on('idle', this.mapIdle);
		},

		mapIdle() {
			this.state.ui.map.sources.runway.data.features = [];
			if(this.state.ui.airport) {

				const runwayFeature = {
					type: 'Feature',
					geometry: {
						type: 'MultiPolygon',
						coordinates: [] as any
					}
				}

				const frameOffset = 20; // meters
				this.state.ui.airport.Runways.forEach(runway => {

					const rect = [];

					const thresholdOffset1 = Eljs.MapOffsetPosition(runway.Location[0], runway.Location[1], (runway.LengthFT / 2 * 0.3048) + frameOffset, runway.Heading);
					const thresholdOffset2 = Eljs.MapOffsetPosition(runway.Location[0], runway.Location[1], (runway.LengthFT / 2 * 0.3048) + frameOffset, runway.Heading + 180);

					const rectStart = Eljs.MapOffsetPosition(thresholdOffset1[0], thresholdOffset1[1], (runway.WidthMeters / 2 * 0.3048) + frameOffset, runway.Heading - 90);
					rect.push([
						rectStart,
						Eljs.MapOffsetPosition(thresholdOffset1[0], thresholdOffset1[1], (runway.WidthMeters / 2 * 0.3048) + frameOffset, runway.Heading + 90),
						Eljs.MapOffsetPosition(thresholdOffset2[0], thresholdOffset2[1], (runway.WidthMeters / 2 * 0.3048) + frameOffset, runway.Heading + 90),
						Eljs.MapOffsetPosition(thresholdOffset2[0], thresholdOffset2[1], (runway.WidthMeters / 2 * 0.3048) + frameOffset, runway.Heading - 90),
						rectStart
					])
					runwayFeature.geometry.coordinates.push(rect);

				});
				this.state.ui.map.sources.runway.data.features.push(runwayFeature);
			}

		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.act();
					break;
				}
				case 'flyr': {

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
.p42_flyr {

	.app-frame {
		background: #000000;
	}

	.map {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		transform-origin: center bottom;
		&.move {
			&-right {
				transform: translateX(100%) rotate(20deg);
				transition: transform 0.3s cubic-bezier(.37,.25,.37,1);
			}
			&-left {
				transform: translateX(-100%) rotate(-20deg);
				transition: transform 0.3s cubic-bezier(.37,.25,.37,1);
			}
		}
	}

	.section {
		&-bottom {
			position: absolute;
			left: 0;
			right: 0;
			bottom: 0;
			padding-bottom: $nav-size;
			display: flex;
			.section-background {
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				background: rgba(#FFFFFF, 0.9);
				clip-path: circle(1200px at 50% 1200px);
			}
			.stack {
				display: flex;
				flex-grow: 1;
				justify-content: center;
				margin-top: -40px;
				padding-bottom: 30px;
				.btns {
					position: relative;
					display: flex;
					z-index: 2;
					&.disabled {
						pointer-events: none;
					}
					&.move {
						&-right {
							.btn {
								pointer-events: none;
								&-no {
									transition: transform 0.2s cubic-bezier(0,.55,0,1);
									transform: scale(0.4) rotate(0) translateY(40%);
								}
								&-yes {
									transition: transform 0.5s cubic-bezier(0,1.29,.21,1);
									transform: scale(2) rotate(0) translateY(-30%);
								}
							}
						}
						&-left {
							.btn {
								pointer-events: none;
								&-no {
									transition: transform 0.5s cubic-bezier(0,1.29,.21,1);
									transform: scale(2) rotate(0) translateY(-30%);
								}
								&-yes {
									transition: transform 0.2s cubic-bezier(0,.55,0,1);
									transform: scale(0.4) rotate(0) translateY(40%);
								}
							}
						}
					}
					.btn {
						display: block;
						width: 100px;
						height: 100px;
						border-radius: 50%;
						background-repeat: no-repeat;
						background-size: 50px;
						background-position: center;
						transition: transform 0.3s cubic-bezier(0,1.29,.21,1);
						&-no {
							background-color: rgba(200, 0, 0, 1);
							background-image: url(../../../sys/assets/icons/bright/no.svg);
							&:hover {
								transform: scale(1.3) rotate(-15deg) translateY(0%)
							}
							&:active {
								transform: scale(1.2) rotate(-20deg) translateY(0%)
							}
						}
						&-yes {
							background-color: rgba(0, 200, 0, 1);
							background-image: url(../../../sys/assets/icons/bright/yes.svg);
							margin-left: 20px;
							&:hover {
								transform: scale(1.3) rotate(15deg) translateY(0%)
							}
							&:active {
								transform: scale(1.2) rotate(20deg) translateY(0%)
							}
						}
					}
				}
			}
		}
	}
}
</style>