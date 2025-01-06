<template>
	<div class="p42_scenr_mapimage" @click="close">

		<MglMap
			class="background"
			ref="map"
			:accessToken="map.accessToken"
			:mapStyle.sync="map.mapStyle"
			:maxBounds="[[-9999999, -90], [9999999, 90]]"
			:maxPitch="85"
			:preserveDrawingBuffer="true"
			@load="mapLoaded"
		>

		</MglMap>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import * as turf from '@turf/turf';
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import MapboxExt from '@/sys/libraries/mapboxExt';
import { MglMap, MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import Eljs from '@/sys/libraries/elem';
import '@/sys/libraries/mapbox-vue/mapbox-gl.css';

let UntrackedMap: any = null;
export default Vue.extend({
	name: "p42_scenr_mapimage",
	props: {
		app: Vue,
		d: Object
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
			ICAO: null as string,
			map: {
				load: false,
				loaded: false,
				location: [0,0],
				zoom: 2,
				heading: 0,
				pitch: 0,
				accessToken: "pk.eyJ1Ijoia2V2ZW5tZW5hcmQiLCJhIjoiY2trb2c0amo2MGNoMzJ2cWRranI2cDExMSJ9.OpawoDudvj_NWm8gH_3oxA",
				moveTimeout: null,
				lineAnimationInterval: null,
				mapStyle: 'mapbox://styles/kevenmenard/ckrpdty9g50cv17piyzf512cr',
			}
		}
	},
	mounted() {

	},
	methods: {
		close() {
			this.app.$emit('interaction', { cmd: 'map:images:close' });
		},

		mapLoaded(map: any) {
			UntrackedMap = map;
			UntrackedMap.map.on('idle', this.mapIdle);

			this.$root.$data.services.api.SendWS('scenr:mapimages:start', { 'yes': true }, (rData: any) => {
				this.mapMove(rData.payload);
			});
			this.ICAO = null;
		},

		mapMove(airport :any) {

			console.log(airport);

			this.ICAO = airport.ICAO;
			const nodes = [airport.Location];
			let longest = null;

			airport.Runways.forEach(runway => {

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

			MapboxExt.fitBoundsExt(UntrackedMap.map, turf.bbox(turf.lineString(nodes)), {
				padding: { left: 50, top: 50, right: 50, bottom: 50 },
				offset: [0,-10],
				pitch: 45,
				bearing: 0, //longest.Heading
				duration: 0,
			}, null);
		},

		mapIdle() {
			window.requestAnimationFrame(() => {
				setTimeout(() => {

					if(this.ICAO != null) {
						var mapEL = (this.$refs.map as Vue).$el;
						var canvasEL = mapEL.querySelector('.mapboxgl-canvas') as HTMLCanvasElement;
						var img = canvasEL.toDataURL("image/png");

						this.$root.$data.services.api.SendWS('scenr:mapimages:getnext', {
							icao: this.ICAO,
							data: img,
						}, (rData: any) => {
							this.mapMove(rData.payload);
						});
						this.ICAO = null;
					}
				}, 100);
			});
		},
	},
});
</script>

<style lang="scss">
@import '@/sys/scss/colors.scss';
.p42_scenr_mapimage {
	position: absolute;
	left: 50%;
	top: 50%;
	width: 800px;
	height: 600px;
	background: #FFF;
	z-index: 100;
	transform: translate(-50%, -50%);

	.mapboxgl-control-container {
		display: none;
	}
}
</style>