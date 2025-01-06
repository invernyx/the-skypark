<template>
	<div class="payload">
		<div v-for="(location, index) in payload" v-bind:key="'pl' + index" class="payload_location">

			<div class="payload_location_background" :style="location.DestColor ? 'background-color:' + location.DestColor.colorReal : ''"></div>

			<div class="payload_location_header" v-if="location.Aircraft">
				<span>On-board <strong>{{ location.Aircraft.Manufacturer }}</strong> {{ location.Aircraft.Model }}</span>
				<!--<div class="background-map">
					<div class="background-map-blur" :style="'background-image: url(' + location.ImageURL + ')'"></div>
					<div class="background-map-image" :style="'background-image: url(' + location.ImageURL + ')'"></div>
					<div class="background-map-overlay">
						<div>
							<span>Currently in <strong>{{ location.Aircraft.Manufacturer }}</strong> {{ location.Aircraft.Model }}</span>
						</div>
					</div>
				</div>-->
			</div>

			<div class="payload_location_header" v-if="location.Airport">
				<div class="background-map">
					<div class="background-map-blur" :style="'background-image: url(' + location.ImageURL + ')'"></div>
					<div class="background-map-image" :style="'background-image: url(' + location.ImageURL + ')'"></div>
					<div class="background-map-overlay">
						<div>
							<span>At <strong>{{ location.Airport.ICAO }}</strong> {{ location.Airport.Name }}</span>
						</div>
						<div>
							<flags class="location-info-flag" :code="location.Airport.Country.toLowerCase()" /> {{ location.Airport.CountryName }}
						</div>
					</div>
				</div>
			</div>

			<div v-for="(entry, index) in location.Sets" v-bind:key="'pa' + index" class="payload_adventure">
				<div class="payload_adventure_content">
					<div class="payload_pallets">
						<div v-for="(pallet, index1) in entry.Pallets" v-bind:key="'pp' + index + '_' + index1" class="payload_pallet" :style="pallet.DestColor ? 'background-color:' + ($root.$data.config.ui.theme == 'theme--bright' ? pallet.DestColor.color : pallet.DestColor.colorDark) : ''">

							<div class="payload_pallet_header">
								<div class="payload_pallet_destination" v-if="entry.Destinations[pallet.Dest].Airport">Going to <strong>{{ entry.Destinations[pallet.Dest].Airport.ICAO }}</strong> {{ entry.Destinations[pallet.Dest].Airport.Name }}</div>
								<div class="payload_pallet_destination" v-else>Going to <strong>{{ entry.Destinations[pallet.Dest].Location }}</strong></div>
								<div class="payload_pallet_weight" v-if="pallet.Groups.length > 1">{{ pallet.WeightKG.toLocaleString('en-gb') }}kg</div>
							</div>

							<div v-for="(group, index2) in pallet.Groups" v-bind:key="'pg' + index + '_' + index1 + '_' + index2" class="payload_group">
								<collapser :withArrow="true" :default="false" :preload="true">
									<template v-slot:title>
										<div class="payload_group_header">
											<div class="payload_group_image"></div>
											<div class="payload_group_info">
												<div class="payload_group_name">{{ group.Boxes.length.toLocaleString('en-gb') }} of <strong>{{ group.Name }}</strong></div>
												<div class="payload_group_weight">{{ group.WeightKG.toLocaleString('en-gb') }}kg</div>
											</div>
										</div>
										<div class="collapser_arrow"></div>
										<div class="payload_group_load">
											<button_action v-if="location.Aircraft" class="go" :class="{ 'disabled': !group.Unloadable }" :shadowed="true" @click.native.stop="unload($event, entry.Adventure.ID, entry.Action, entry.Manifest, group.GUID )">Unload</button_action>
											<button_action v-else class="go" :class="{ 'disabled': !group.Loadable }" :shadowed="true" @click.native.stop="load($event, entry.Adventure.ID, entry.Action, entry.Manifest, group.GUID )">Load</button_action>
										</div>
									</template>
									<template v-slot:content="props">
										<div class="payload_fold_header">
											<div class="payload_fold_adventure">{{ entry.Adventure.Name }}</div>
											<div class="payload_fold_weight">{{ pallet.UnitWeightKG }}kg/u</div>
										</div>
										<div class="payload_boxes">
											<div v-for="(box, index3) in group.Boxes" v-bind:key="'pb' + index + '_' + index1 + '_' + index2 + '_' + index3" class="payload_box" :style="{ transitionDelay: props.expanded ? Math.sqrt(index3 * 10000) + 'ms' : '0s' }">
												<span v-if="box.Health != 100">{{ box.Health }}</span>
											</div>
										</div>
									</template>
									<template v-slot:sub>
										<div class="payload_group_health">
											<div class="payload_group_health_state" v-if="group.Health == 0">Totalled</div>
											<div class="payload_group_health_state" v-else-if="group.Health < 100">Damaged</div>
											<div class="payload_group_health_state" v-else>Healthy</div>
											<div class="payload_group_health_bar">
												<div :style="'width:' + group.Health + '%'"></div>
											</div>
										</div>
									</template>

								</collapser>
							</div>

						</div>
					</div>
				</div>
			</div>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "payload_layout",
	props: ['app','payload'],
	components: {
	},
	methods: {
		load(event :any, adventure :number, linkID :number, manifest :any = null, group :any = null, units :any[] = null, ) {
			this.$root.$data.services.api.SendWS(
				"adventure:interaction",
				{
					ID: adventure,
					Link: linkID,
					Verb: 'load',
					Data: {
						Manifest: manifest,
						Group: group,
						Units: units
					},
				}
			);
		},
		unload(event :any, adventure :number, linkID :number, manifest :any = null, group :any = null, units :any[] = null, ) {
			this.$root.$data.services.api.SendWS(
				"adventure:interaction",
				{
					ID: adventure,
					Link: linkID,
					Verb: 'unload',
					Data: {
						Manifest: manifest,
						Group: group,
						Units: units
					},
				}
			);
		},
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';
.payload {

	.theme--bright &,
	&.theme--bright {
		&_location {
			background-color: rgba($ui_colors_bright_shade_1, 0.4);
			@include shadowed_shallow($ui_colors_bright_shade_5);
			&::after {
				border-color: rgba($ui_colors_bright_shade_5, 0.3);
			}
			&_header {
				.background-map {
					@include shadowed($ui_colors_bright_shade_5);
				}
			}
		}
		&_adventure {
			//@include shadowed_shallow($ui_colors_bright_shade_5);
			//&_content {
			//	background-color: rgba($ui_colors_bright_shade_1, 0.4);
			//}
		}
		&_group {
			background-color: rgba($ui_colors_bright_shade_1, 0.3);
			&:hover {
				background-color: rgba($ui_colors_bright_shade_1, 0.6);
			}
			&_health {
				&_bar {
					&:after {
						border-color: rgba($ui_colors_bright_shade_5, 0.1);
					}
					& > div {
						background-color: $ui_colors_bright_button_go;
						@include shadowed_shallow($ui_colors_bright_button_go);
						border-color: rgba($ui_colors_bright_shade_5, 0.1);
					}
				}
			}
		}
		&_pallet {
			border-color: rgba($ui_colors_bright_shade_5, 0.3);
		}
		&_box {
			background-color: $ui_colors_bright_button_go;
			@include shadowed_shallow($ui_colors_bright_button_go);
			border-color: rgba($ui_colors_bright_shade_5, 0.2);
		}
	}

	.theme--dark &,
	&.theme--dark {
		&_location {
			background-color: rgba($ui_colors_dark_shade_1, 0.4);
			@include shadowed_shallow($ui_colors_dark_shade_0);
			&_header {
				.background-map {
					@include shadowed($ui_colors_dark_shade_0);
				}
			}
		}
		&_adventure {
			//@include shadowed_shallow($ui_colors_dark_shade_0);
			//&_content {
			//	background-color: rgba($ui_colors_dark_shade_1, 0.4);
			//}
		}
		&_group {
			background-color: rgba($ui_colors_dark_shade_1, 0.3);
			&:hover {
				background-color: rgba($ui_colors_dark_shade_1, 0.6);
			}
			&_health {
				&_bar {
					&:after {
						border-color: rgba($ui_colors_dark_shade_5, 0.1);
					}
					& > div {
						background-color: $ui_colors_dark_button_go;
						@include shadowed_shallow($ui_colors_dark_button_go);
						border-color: rgba($ui_colors_dark_shade_5, 0.1);
					}
				}
			}
		}
		&_pallet {
			border-color: rgba($ui_colors_dark_shade_0, 0.3);
		}
		&_box {
			background-color: $ui_colors_dark_button_go;
			@include shadowed_shallow($ui_colors_dark_button_go);
			border-color: rgba($ui_colors_dark_shade_5, 0.2);
		}
	}

	&_location {
		position: relative;
		padding: 4px;
		padding-bottom: 8px;
		border-radius: 14px;
		margin-bottom: 8px;
		overflow: hidden;
		&::after {
			content: '';
			position: absolute;
			left: 0;
			right: 0;
			top: 0;
			bottom: 0;
			margin: -1px;
			border-radius: 15px;
			border: 2px solid transparent;
		}
		&_header {
			position: relative;
			margin: 4px;
			margin-bottom: 8px;
			z-index: 2;
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
	}

	&_adventure {
		position: relative;
		z-index: 2;
		//margin-bottom: 4px;
		//border-radius: 8px;
		//overflow: hidden;
		//&:last-child {
		//	margin-bottom: 0;
		//}
		//&_content {
			//position: relative;
			//margin: 2px;
			//padding: 2px;
			//border-radius: 6px;
			//z-index: 2;
		//}
		&_header {
			position: relative;
			display: flex;
			flex-grow: 1;
			justify-content: space-between;
			align-items: center;
			padding: 0 4px;
			border-radius: 6px;
			z-index: 2;
		}
	}
	&_pallets {
		//padding: 2px;
	}
	&_pallet {
		position: relative;
		padding: 4px;
		padding-bottom: 0;
		margin: 4px;
		overflow: hidden;
		border-radius: 8px;
		border: 1px solid transparent;
		&:last-child {
			margin-bottom: 0;
		}
		&_header {
			position: relative;
			display: flex;
			flex-grow: 1;
			padding: 0 4px;
			justify-content: space-between;
			margin-bottom: 4px;
			z-index: 2;
		}
	}
	&_group {
		position: relative;
		margin-bottom: 4px;
		border-radius: 4px;
		transition: background 0.2s ease-out;
		&:hover {
			transition: background 0.05s ease-out;
		}
		& .collapser_arrow {
			margin-right: 8px;
		}
		&_load {
			display: flex;
			margin: 8px;
			margin-left: 0;
		}
		&_image {
			width: 36px;
			min-width: 36px;
			height: 36px;
			margin-right: 8px;
			background-image: url(../../../../sys/assets/cargo/default.png);
			background-size: contain;
			background-position: center;
		}
		&_info {
			flex-basis: 1;
			flex-grow: 1;
		}
		&_header {
			display: flex;
			align-items: center;
			padding: 8px;
			& > div {
				flex-basis: 0;
			}
		}
		&_health {
			border-top: 1px solid rgba(0,0,0,0.1);
			position: relative;
			margin: 0 8px;
			margin-bottom: 6px;
			padding-top: 4px;
			display: flex;
			align-items: center;
			&_bar {
				position: relative;
				display: flex;
				flex-grow: 1;
				height: 8px;
				& > div {
					border-radius: 8px;
					border: 1px solid transparent;
				}
				&:after {
					position: absolute;
					top: 0;
					right: 0;
					bottom: 0;
					left: 0;
					content: '';
					border: 1px solid transparent;
					border-radius: 8px;
				}
			}
			&_state {
				margin-right: 8px;
			}
		}
	}
	&_fold {
		&_header {
			display: flex;
			justify-content: space-between;
			margin: 0 8px;
			margin-bottom: 2px;
			border-top: 1px solid rgba(0,0,0,0.1);
			padding-top: 4px;
		}
	}
	&_boxes {
		position: relative;
		display: flex;
		flex-wrap: wrap;
		margin: 4px;
		margin-top: 0;
	}
	&_box {
		display: flex;
		justify-content: center;
		align-items: center;
		border-radius: 8px;
		min-width: 34px;
		min-height: 30px;
		margin: 4px;
		opacity: 0;
		transition: opacity 0.5s 0s;
		border: 1px solid transparent;
		.payload_group > .collapser_expanded & {
			opacity: 1;
			transition: opacity 1s ease-out;
		}
	}
}
</style>