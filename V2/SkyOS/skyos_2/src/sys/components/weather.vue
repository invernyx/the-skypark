<template>
	<div class="weather-info">
		<div v-if="weather && weather.IsNearby">
			<span class="notice">Weather from <strong>{{ weather.Station }}</strong>, {{ weather.IsNearby }} nm away.</span>
		</div>
		<div v-if="weather" class="data-stack data-stack--vertical">
			<div v-if="include ? include.includes('winds') : false">
				<span class="label">Winds</span>
				<span class="value" v-if="weather.WindSpeed > 0">{{ weather.WindHeading + "@" + weather.WindSpeed + (weather.WindGust > 0 ? "G" + weather.WindGust : '') + "KT" }}</span>
				<span v-else class="value">Calm</span>
			</div>
			<div v-if="include ? include.includes('visibility') : false">
				<span class="label">Visibility</span>
				<span class="value" v-if="visibility > -1">{{ visibility }}&thinsp;SM</span>
				<span v-else class="value">Unlimited</span>
			</div>
			<div v-if="include ? include.includes('altimeter') : false">
				<span class="label">Altimeter</span>
				<span class="value">{{ altimeterHg }}&thinsp;Hg<br/> {{ altimeterMb }}&thinsp;mb</span>
			</div>
		</div>
		<div v-if="!weather && !failed" class="loading-label">
			<span>Loading Weather</span>
			<span></span>
		</div>
		<div v-if="failed">
			<span class="label">No weather data</span>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from './../../sys/libraries/elem';

//https://aviationweather.gov/dataserver/example?datatype=metar
export default Vue.extend({
	name: "weather_info",
	props: ['code', 'location', 'wx', 'include'],
	data() {
		return {
			weather: null,
			failed: false,
			visibility: 0,
			altimeterHg: 0,
			altimeterMb: 0,
		}
	},
	methods: {
		getMETAR(){
			this.$root.$data.services.api.SendWS(
				"weather:get",
				{
					icao: this.code
				}
			);
		},
		gotWeather(wx :any) {
			this.weather = wx;
			if(wx){
				this.failed = false;
				this.visibility = Eljs.round(this.weather.Visibility * 0.0006213711922373339, 1);
				this.altimeterHg = this.weather.Altimeter;
				this.altimeterMb = Eljs.round(this.weather.Altimeter * 33.8639, 2);
			} else {
				this.failed = true;
			}
		},
		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'weather': {
					switch(wsmsg.name[1]){
						case 'get': {
							switch(wsmsg.name[2]){
								case 'icao': {
									const Found = wsmsg.payload.find((x: any) => x.Station == this.code);
									if(Found){
										this.gotWeather(Found);
									} else if(this.weather == null){
										this.failed = true;
									}
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
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
		if(this.wx){
			this.gotWeather(this.wx);
		} else if(this.code){
			this.getMETAR();
		}
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
	watch: {
		wx: {
			immediate: true,
			handler(newValue, oldValue) {
				this.gotWeather(this.wx);
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
.weather-info {
	&.compact {
		.data-stack {
			.value {
				font-size: 1em;
			}
		}
	}

	& > div {
		margin-bottom: 0.5em;
		&:last-of-type {
			margin-bottom: 0;
		}
	}
	.columns {
		&_2 {
  			column-count: 2;
		}
	}

}
</style>