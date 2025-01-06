<template>
	<div class="content-airport app-box app-box-margined shadowed-deep" :style="{ 'background-color': theme == 'theme--dark' ? color_dark : color_bright }" v-if="airport ? (distance * 0.539957 < 5 || (leg.ete > 0 ? (leg.ete < 0.25) : false)) : false">
		<div class="background-map" v-if="airport">
			<div class="background-map-image" :style="'background-image:url(' + $os.api.getCDN('images', 'airports/' + airport.icao + '.jpg') + ')'"></div>
			<div class="background-map-overlay">
				<div v-if="airport.radius > distance">
					<span>At <strong>{{ airport.icao }}</strong></span>
				</div>
				<div v-else-if="(leg.ete > 0 ? (leg.ete < 0.25) : false)">
					<span>Approaching <strong>{{ airport.icao }}</strong></span>
				</div>
				<div v-else-if="distance * 0.539957 < 5">
					<span>Vincinity of <strong>{{ airport.icao }}</strong></span>
				</div>
				<div>
					<flags class="location-info-flag" :code="airport.country.toLowerCase()" /> {{ airport.country_name }}
				</div>
			</div>
		</div>

		<div class="airport-info h_edge_padding_lateral h_edge_padding_vertical_half">
			<div class="info">
				<span><strong>{{ airport.name }}</strong></span><br>
				<span v-if="airport.city.length">{{ airport.city }}{{ airport.state.length ? ',&nbsp;' : ''}}</span>
				<span v-if="airport.state.length">{{ airport.state }}</span>
			</div>
			<div class="header">
				<div class="elevation"><span class="label">Elevation</span> <span class="value"> <height :amount="airport.elevation" :decimals="1"/></span></div>
				<div class="features">
					<div class="lit" v-if="airport.runways.filter(x => x.lit ).length">Lit</div>
					<div class="unlit" v-else>Unlit</div>
					<div class="ils" v-if="airport.runways.filter(x => x.primary_ils || x.secondary_ils ).length">ILS</div>
				</div>
			</div>
			<div class="runways">
				<div class="runway" :class="{'hasLights': runway.lit}" v-for="(runway, index) in airport.runways" v-bind:key="index">
					<span class="badge">
						<icons class="icon icon--dark icon-runway">
							<span :style="'transform: rotate(' + runway.heading + 'deg);'"></span>
						</icons>
					</span>
					<span class="value">
						<div>
							<span class="name" :class="{'hasILS': runway.primary_ils }">{{ runway.primary_name }}</span>-<span class="name" :class="{'hasILS': runway.secondary_ils }">{{ runway.secondary_name }}</span> <span class="h_dim">/</span> <length :amount="runway.length" /> <span class="h_dim">/</span> {{ runway.surface }}
						</div>
					</span>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Airport from '@/sys/classes/airport';
import { NavLeg } from '@/sys/services/extensions/navigation';
import { ColorsCallback } from '@/sys/services/extensions/color-seek';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		leg :NavLeg,
	},
	data() {
		return {
			theme: this.$os.userConfig.get(['ui','theme']),
			distance: 99999,
			airport: null as Airport,
			color: null as String,
			color_bright: null as String,
			color_dark: null as String,
		}
	},
	methods: {
		init() {

			this.distance = 99999;
			this.airport = null;
			this.color = null;
			this.color_bright = null;
			this.color_dark = null;

			this.$os.api.send_ws('airports:from-icaos', {
			icaos: this.leg.start.airport.icao,
			fields: null
			}, (ret :any) => {
				if(ret.payload.length) {
					this.airport = new Airport(ret.payload[0]);
					this.$os.colorSeek.find(this.$os.api.getCDN('images', 'airports/' + this.airport.icao + '.jpg'), 160, (color :ColorsCallback) => {
						this.color = color.color.h,
						this.color_bright = color.color_bright.h;
						this.color_dark = color.color_dark.h;
					});
					this.update();
				}
			});
		},
		update() {
			if(this.airport) {
				this.distance = Eljs.GetDistance(this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, this.airport.location[1], this.airport.location[0], "N");
			}
		},
		listener_app_navigation(wsmsg :any) {
			switch(wsmsg.name){
				case 'data': {
					this.update();
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'themechange': {
					this.theme = this.$os.userConfig.get(['ui','theme']);
					break;
				}
			}
		},
	},
	mounted() {
	},
	beforeMount() {
		this.init();
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on("navigation", this.listener_app_navigation);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off("navigation", this.listener_app_navigation);
	},
	watch: {
		leg: {
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
@import '@/sys/scss/helpers.scss';

.content-airport {
	flex-grow: 1;

	.theme--bright & {
		.box {
			background: rgba($ui_colors_bright_shade_2, 0.3);
			border: 1px solid rgba($ui_colors_bright_shade_3, 0.3);
		}
		.info {
			border-color: rgba($ui_colors_bright_shade_3, 0.3);
		}
		.ils {
			border-color: $ui_colors_bright_button_go;
		}
		.runways {
			.runway {
				.value {
					.name {
						border-color: rgba($ui_colors_bright_shade_3, 0.1);
						&.hasILS {
							border-color: $ui_colors_bright_button_go;
						}
					}
				}
			}
		}
	}

	.theme--dark & {
		.box {
			background: rgba($ui_colors_dark_shade_2, 0.3);
			border: 1px solid rgba($ui_colors_dark_shade_3, 0.3);
		}
		.info {
			border-color: rgba($ui_colors_dark_shade_3, 0.3);
		}
		.ils {
			border-color: $ui_colors_dark_button_go;
		}
		.runways {
			.runway {
				.value {
					.name {
						border-color: rgba($ui_colors_dark_shade_3, 0.1);
						&.hasILS {
							border-color: $ui_colors_dark_button_go;
						}
					}
				}
			}
		}
	}

	.box {
		padding: 4px 8px;
		margin-bottom: 4px;
		border-radius: 8px;
		&:last-of-type {
			margin-bottom: 0;
		}
	}

	.background-map {
		position: relative;
		height: 120px;
		border-radius: 12px;
		color: #FFF;
		box-sizing: border-box;
		background-color: rgba(#000000, 0.2);
		transition: height 0.6s cubic-bezier(.25,0,.14,1);
		&-image {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			background-size: cover;
			background-position: center center;
			border-radius: 12px;
			will-change: transform;
			transition: opacity 5s cubic-bezier(.25,0,.14,1);
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
					top: 8px;
					left: 8px;
				}
				&:last-child {
					bottom: 8px;
					left: 8px;
				}
			}
		}
		.collapser_arrow {
			position: absolute;
			right: 8px;
			top: 8px;
		}
	}

	.info {
		margin-bottom: 4px;
		padding-bottom: 4px;
		border-bottom: 1px solid transparent;
	}
	.header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		.features {
			display: flex;
			& > div {
				margin-left: 0.5em;
			}
		}
	}
	.details {
		//margin-left: 8px;
		padding-top: 4px;
		padding-bottom: 4px;
	}
	.lit {
		display: flex;
		align-items: center;
		position: relative;
		font-family: "SkyOS-SemiBold";
		&::after {
			display: block;
			content: '';
			width: 0.5em;
			height: 0.5em;
			margin-left: 0.1em;
			background: #FFF;
			border: 1px solid #000;
			border-radius: 50%;
			z-index: 2;
		}
	}
	.unlit {
		display: flex;
		align-items: center;
		position: relative;
		font-family: "SkyOS-SemiBold";
		&::after {
			display: block;
			content: '';
			width: 0.5em;
			height: 0.5em;
			margin-left: 0.1em;
			background: #000;
			border: 1px solid #000;
			border-radius: 50%;
			z-index: 2;
		}
	}
	.ils {
		padding: 0 0.1em;
		margin: 0 0.1em;
		border-radius: 0.4em;
		font-family: "SkyOS-SemiBold";
		border: 2px solid transparent;
	}
	.elevation {
		margin-bottom: 4px;
		.value {
			font-family: "SkyOS-SemiBold";
		}
	}
	.runways {
		flex-grow: 1;
		.runway {
			display: flex;
			align-items: center;
			margin-bottom: 8px;
			&:last-child {
				margin-bottom: 4px;
			}
			&.hasLights {
				.badge {
					&::after {
						position: absolute;
						display: block;
						content: '';
						top: 0;
						right: 0;
						width: 0.5em;
						height: 0.5em;
						margin-right: -0.2em;
						margin-top: -0.2em;
						background: #FFF;
						border: 1px solid #000;
						border-radius: 50%;
						z-index: 2;
					}
				}
			}
			.label {
				display: block;
			}
			.badge {
				display: inline-block;
				position: relative;
				margin-right: 0.5em;
				background: #FFF;
				border-radius: 50%;
				transition: background 0.4s ease-out, opacity 0.4s ease-out;
				.icon {
					display: block;
					opacity: 0.8;
					margin: -1px;
					span {
						display: block;
						width: 1.5em;
						height: 1.5em;
					}
				}
			}
			.value {
				overflow: hidden;
				white-space: nowrap;
				text-overflow: clip;
				.name {
					display: inline-block;
					padding: 0 0.1em;
					margin: 0;
					border-radius: 0.4em;
					font-family: "SkyOS-SemiBold";
					border: 2px solid transparent;
					&:first-child {
						margin-left: 0;
					}
				}
			}
		}
	}
}

</style>