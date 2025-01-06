<template>
	<div class="payload">

		<div v-for="(location, index) in payload" v-bind:key="'pl' + index" class="payload_location app-box shadowed-deep nooverflow" :class="{ 'is-aircraft': is_aircraft }">

			<!-- Location is Airport -->
			<div class="payload_location_header" v-if="location.airport">
				<div class="background-map">
					<div class="background-map-blur" :style="'background-image: url(' + location.image_url + ')'"></div>
					<div class="background-map-image" :style="'background-image: url(' + location.image_url + ')'"></div>
					<div class="background-map-overlay">
						<div>
							<span>At <strong>{{ location.airport.icao }}</strong> {{ location.airport.name }}</span>
						</div>
						<div>
							<flags class="location-info-flag" :code="location.airport.country.toLowerCase()" /> {{ location.airport.country_name }}
						</div>
					</div>
				</div>
			</div>

			<!-- Location is Onboard -->
			<div class="payload_location_header" v-else-if="is_aircraft && aircraft">
				<h2>Onboard current aircraft</h2>
				<div>{{ aircraft.manufacturer }} <strong>{{ aircraft.model }}</strong></div>
				<div class="payload-ref">
					<div class="payload-ref-track">
						<div :style="{ 'width': ((($os.simulator.location.PayloadTotal - aircraft.empty_weight) / (aircraft.max_weight - aircraft.empty_weight)) * 100) + '%' }"></div>
					</div>
					<div class="payload-ref-data">
						<data_stack class="center small" label="Empty"><weight :amount="aircraft.empty_weight" :decimals="0" /></data_stack>
						<data_stack class="center small" label="Current"><weight :amount="$os.simulator.location.PayloadTotal" :decimals="0" /></data_stack>
						<data_stack class="center small" label="Max"><weight :amount="aircraft.max_weight" :decimals="0" /></data_stack>
						<data_stack class="center small" label="Available"><weight :amount="aircraft.max_weight - $os.simulator.location.PayloadTotal" :decimals="0" /></data_stack>
					</div>
				</div>
			</div>

			<!-- Location is Off-Field -->
			<div class="payload_location_header" v-else>
				<h2>{{ $os.maps.cities.find(x => x.code == Math.round(location.location[0] * 100000) / 100000 + ',' + Math.round(location.location[1] * 100000) / 100000).name }}</h2>
				<div>{{ Math.round(location.location[0] * 100000) / 100000 + ', ' + Math.round(location.location[1] * 100000) / 100000 }}</div>
			</div>


			<div class="payload_aircraft"  v-for="(location_aircraft, index) in location.aircraft" v-bind:key="'pl' + index">
				<div class="payload_aircraft_header" v-if="location.aircraft">
					<span>On-board {{ location_aircraft.aircraft.manufacturer }} <strong>{{ location_aircraft.aircraft.model }}</strong></span>
				</div>
				<Sets :sets="location_aircraft.sets" :aircraft="location_aircraft.aircraft" :aircraft_current="aircraft" :is_aircraft="is_aircraft"/>
			</div>

			<Sets :sets="location.sets" :aircraft_current="aircraft" :is_aircraft="is_aircraft"/>

		</div>

		<div v-if="aircraft && !payload.length && is_aircraft && aircraft.manufacturer" class="payload_location app-box shadowed-deep nooverflow is-aircraft">
			<div class="payload_location_header">
				<h2>Current aircraft is empty</h2>
				<div>{{ aircraft.manufacturer }} <strong>{{ aircraft.model }}</strong></div>
				<div class="payload-ref">
					<div class="payload-ref-track">
						<div :style="{ 'width': ((($os.simulator.location.PayloadTotal - aircraft.empty_weight) / (aircraft.max_weight - aircraft.empty_weight)) * 100) + '%' }"></div>
					</div>
					<div class="payload-ref-data">
						<data_stack class="center small" label="Empty"><weight :amount="aircraft.empty_weight" :decimals="0" /></data_stack>
						<data_stack class="center small" label="Current"><weight :amount="$os.simulator.location.PayloadTotal" :decimals="0" /></data_stack>
						<data_stack class="center small" label="Max"><weight :amount="aircraft.max_weight" :decimals="0" /></data_stack>
						<data_stack class="center small" label="Available"><weight :amount="aircraft.max_weight - $os.simulator.location.PayloadTotal" :decimals="0" /></data_stack>
					</div>
				</div>
			</div>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import { AppInfo } from '@/sys/foundation/app_model';
import Sets from './manifest_sets.vue'
import Cabin from './cabin_sets.vue'

export default Vue.extend({
	props: {
		app: AppInfo,
		payload: Array as () => Array<any>,
		aircraft: Aircraft,
		is_aircraft :Boolean
	},
	components: {
		Sets,
		Cabin
	},
	data() {
		return {
		}
	},
	methods: {
		getCities() {
			this.payload.forEach((location, index) => {

				if(!location.airport) {
					if(location.location) {
						const lon = Math.round(location.location[0] * 100000) / 100000;
						const lat = Math.round(location.location[1] * 100000) / 100000;
						const code = lon + ',' + lat;
						const existing = this.$os.maps.cities.find(x => x.code == code);

						if(!existing) {

							const nc = {
								code: code,
								location: [lon, lat],
								loading: true,
								name: 'Loading...',
							}
							this.$os.maps.cities.push(nc);

							fetch('https://api.mapbox.com/geocoding/v5/mapbox.places/' + location.location[0] + ',' + location.location[1] + '.json?access_token=' + this.$os.maps.token, { method: 'get' })
							.then(response => {
								if (response.ok) {
									return response.json();
								} else {
									nc.loading = false;
									throw new Error('Failed to fetch city from Mapbox');
								}
							})
							.then((data) => {
								console.log(data.features);

								let names = [];

								let poi = data.features.find(x => x.place_type.includes('poi'));
								if(poi) { names.push(poi.text) }

								if(!poi) {
									let neighborhood = data.features.find(x => x.place_type.includes('neighborhood'));
									if(neighborhood) { names.push(neighborhood.text) }
								}

								let place = data.features.find(x => x.place_type.includes('place'));
								if(place) { names.push(place.text) }

								let country = data.features.find(x => x.place_type.includes('country'));
								if(country) { names.push(country.text) }

								nc.name = names.join(', ');
								nc.loading = false;
							}).catch((err) => {
								console.log(err)
							});
						}
					}
				}
			});
		},
	},
	mounted() {
		this.getCities();
	},
	beforeDestroy() {
	},
	watch: {
		payload: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.getCities();
				}
			}
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.payload {
	margin-bottom: 8px;
	.theme--bright &,
	&.theme--bright {
		&_location {
			background-color: $ui_colors_bright_shade_3;
			&_header {
				.background-map {
					@include shadowed($ui_colors_bright_shade_5);
				}
			}
			&.is-aircraft {
				background-color: $ui_colors_bright_button_info;
				.payload_location_header {
					color: $ui_colors_bright_shade_0;
				}
			}
		}
		&_aircraft {
			background-color: $ui_colors_bright_button_info;
			&_header {
				color: $ui_colors_bright_shade_0;
			}
			@include shadowed($ui_colors_bright_shade_5);
		}
	}

	.theme--dark &,
	&.theme--dark {
		&_location {
			background-color: $ui_colors_dark_shade_3;
			&_header {
				.background-map {
					@include shadowed($ui_colors_dark_shade_0);
				}
			}
			&.is-aircraft {
				background-color: $ui_colors_dark_button_info;
				@include shadowed($ui_colors_dark_shade_0);
			}
		}
		&_aircraft {
			background-color: $ui_colors_dark_button_info;
			@include shadowed($ui_colors_dark_shade_0);
		}
	}

	&_location {
		position: relative;
		padding: 4px;
		padding-bottom: 8px;
		overflow: hidden;
		&_header {
			position: relative;
			margin: 4px;
			margin-bottom: 8px;
			z-index: 2;
			&:last-child {
				margin-bottom: 0;
			}

			h2 {
				margin-bottom: 0;
				margin-bottom: 4px;
			}
			.background-map {
				position: relative;
				height: 160px;
				border-radius: 8px;
				color: #FFF;
				box-sizing: border-box;
				background-color: rgba(#000000, 0.2);
				transition: height 0.6s cubic-bezier(.25,0,.14,1);
				&-blur {
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
				&-image {
					position: absolute;
					top: 0;
					left: 0;
					right: 0;
					bottom: 0;
					background-size: cover;
					background-position: center center;
					border-radius: 8px;
					will-change: transform;
					transition: opacity 5s cubic-bezier(.25,0,.14,1);
					&:after {
						position: absolute;
						top: 0;
						left: 0;
						right: 0;
						bottom: 0;
						border-radius: 8px;
						content: '';
						border: 1px solid rgba(255,255,255,0.2);
					}
				}
				&-overlay {
					& > div {
						position: absolute;
						box-sizing: border-box;
						overflow: hidden;
						text-overflow: ellipsis;
						white-space: nowrap;
						max-width: calc(100% - 8px);
						background-color: rgba(#000000, 0.6);
						//backdrop-filter: blur(3px);
						border-radius: 4px;
						padding: 4px 8px;
						&:first-child {
							top: 4px;
							left: 4px;
						}
						&:last-child {
							bottom: 4px;
							left: 4px;
						}
					}
				}
			}
		}
		&_background {
			position: absolute;
			left: 0;
			right: 0;
			top: 0;
			bottom: 0;
			overflow: hidden;
		}

		.payload-ref {
			margin-top: 8px;
			&-track {
				border-radius: 8px;
				overflow: hidden;
				border: 2px solid #CCC;
				& > div {
					height: 10px;
					background: #CCC;
					transition: width 0.4s ease-out;
				}
			}
			&-data {
				display: flex;
				justify-content: space-around;
				margin-top: 4px;
			}
			&.loading {
				.payload-ref-track {
					& > div {
						opacity: 0.4;
					}
				}
			}
		}
	}

	&_aircraft {
		position: relative;
		padding: 4px 0px;
		margin: 4px;
		margin-bottom: 8px;
		border-radius: 10px;
		z-index: 2;
		&_header {
			margin: 0 10px;
		}
	}

}
</style>