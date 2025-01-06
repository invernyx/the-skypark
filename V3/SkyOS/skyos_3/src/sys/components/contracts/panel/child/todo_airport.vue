<template>
	<div class="content-airport">
		<collapser :state="airport.runways.length && contract.state == 'Listed' && (contract.path[index].is_next || contract.path[index].range)" :withArrow="true" :preload="false">
			<template v-slot:title>
				<div class="background-map" v-if="airport">
					<div class="background-map-image" :style="'background-image:url(' + $os.api.getCDN('images', 'airports/' + airport.icao + '.jpg') + ')'"></div>
					<div class="background-map-overlay">
						<div v-if="!current">
							<span v-if="!contract.path[index].done">{{ index > 0 ? contract.situations[index - 1].dist_to_next > 1 ? contract.situations[index - 1].dist_to_next.toLocaleString('en-gb') + '&thinsp;nm' : "Come back" : "Get" }} to </span><strong>{{ airport.icao }}</strong>
						</div>
						<div v-else>
							<span>Current Location <strong>{{ airport.icao }}</strong></span>
						</div>
						<div class="location-info">
							<span><flags class="location-info-flag" :code="airport.country.toLowerCase()" />&nbsp;</span>
							<span>{{ airport.name }}</span>
						</div>
					</div>
					<div class="collapser_arrow"></div>
				</div>
				<div class="description" v-else>
					<span v-if="index > 0"><distance :amount="contract.situations[index - 1].dist_to_next" /></span>
					<span v-else>Get to <strong>{{ contract.situations[index - 1].location[0].toFixed(6) + ', ' + contract.situations[index - 1].location[1].toFixed(6) }}</strong></span>
				</div>
			</template>
			<template v-slot:content>
				<div class="details" v-if="airport">

					<div class="airport-info box">
						<div class="info">
							<span v-if="airport.city.length">{{ airport.city }}<br></span>
							<span v-if="airport.state.length">{{ airport.state }}, </span>
							<span>{{ airport.country_name }}</span>
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
									<span>
										<span class="name" :class="{'hasILS': runway.primary_ils }">{{ runway.primary_name }}</span>-<span class="name" :class="{'hasILS': runway.secondary_ils }">{{ runway.secondary_name }}</span>
									</span>
									<span class="bullet">|</span>
									<length :amount="runway.length" />
									<span class="bullet">|</span>
									<span class="surface">{{ runway.surface }}</span>
								</span>
							</div>
						</div>
					</div>

				</div>
			</template>
		</collapser>
	</div>
</template>

<script lang="ts">
import Airport from "@/sys/classes/airport";
import Contract from "@/sys/classes/contracts/contract";
import Vue from "vue";

export default Vue.extend({
	props: {
		contract :Contract,
		airport :Object,
		index :Number,
		current :Boolean
	},
	components: {

	},
	data() {
		return {

		}
	},
	methods: {
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.content-airport {
	flex-grow: 1;

	.todolist.small & {
		.location-info {
			opacity: 0;
		}
		.background-map {
			height: 35px;
		}
		.collapser {
			&.collapser_expanded {
				transform: scale(1);
				.background-map {
					height: 150px;
				}
				.location-info {
					opacity: 1;
				}
			}
		}
	}

	.theme--bright & {
		.box {
			background-color: rgba($ui_colors_bright_shade_0, 1);
			@include shadowed_shallow($ui_colors_bright_shade_5);
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
			background-color: rgba($ui_colors_dark_shade_0, 0.5);
			border-color: rgba($ui_colors_dark_shade_5, 0.2);
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

	.collapser {
		transition: transform 0.1s cubic-bezier(.3,0,.24,1);
		will-change: transform;
		&:hover {
			transform: scale(1.02);
		}
		&.collapser_expanded {
			transform: scale(1);

			.background-map {
				height: 150px;
			}
		}
	}

	.box {
		padding: 4px 8px;
		margin-bottom: 4px;
		border-radius: 8px;
		border: 1px solid transparent;
		&:last-of-type {
			margin-bottom: 0;
		}
	}

	.background-map {
		position: relative;
		height: 65px;
		border-radius: 8px;
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
				border: 1px solid rgba(0,0,0,0.2);
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
		.collapser_arrow {
			position: absolute;
			right: 8px;
			top: 8px;
		}
	}
	.location-info {
		transition: 0.3s ease-out opacity;
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
		margin-bottom: 8px;
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
				display: flex;
				justify-content: center;
				& > span {
					display: inline-block;
					overflow: hidden;
					padding: 2px 0px;
				}
				.surface {
					max-width: 60px;
					overflow: hidden;
					text-overflow: ellipsis;
				}
				.name {
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