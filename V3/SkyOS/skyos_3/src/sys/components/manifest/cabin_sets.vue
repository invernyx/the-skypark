<template>
	<div class="payload_set">
		<div v-for="(entry, index) in sets" v-bind:key="'pa' + index" class="payload_set_adventure">
			<div class="payload_set_adventure_content">
				<div class="payload_set_pallets">

					<div v-for="(pallet, index1) in entry.pallets" v-bind:key="'pp' + index + '_' + index1" class="payload_set_pallet" :style="pallet.dest_color ? 'background-color:' + pallet.dest_color.color : ''">

						<div class="payload_set_pallet_header">
							<div class="payload_set_pallet_destination" v-if="pallet.dest.airport">Going to <strong>{{ pallet.dest.airport.icao }}</strong> {{ pallet.dest.airport.name }}</div>
							<div class="payload_set_pallet_destination" v-else>Going to <strong>{{ pallet.dest.location }}</strong></div>
							<div class="payload_set_pallet_sub">
								<div class="payload_set_pallet_weight"><strong><weight :amount="pallet.weight" :decimals="0" /></strong></div>
							</div>
						</div>

						<div v-for="(group, index2) in pallet.groups" v-bind:key="'pg' + index + '_' + index1 + '_' + index2" class="payload_set_group">
							<collapser :withArrow="true" :default="false">
								<template v-slot:title>
									<div class="payload_set_group_header">
										<div class="payload_set_group_image"></div>
										<div class="payload_set_group_info">
											<div class="payload_set_group_name"><number :amount="group.boxes.length" :decimals="0" /> of {{ group.name }}</div>
											<div class="payload_set_group_sub">
												<div class="payload_set_group_weight"><strong><weight :amount="group.weight" :decimals="0" /></strong></div>
												<div class="payload_set_group_health">
													<span v-if="group.health == 0">Totalled</span>
													<span v-else-if="group.health < 50">Beyond repair</span>
													<span v-else-if="group.health < 90">Damaged</span>
													<span v-else-if="group.health < 100">Slightly damaged</span>
													<span v-else>Healthy</span>
												</div>
											</div>
										</div>
									</div>
									<div class="collapser_arrow"></div>
									<div class="payload_set_group_load">
										<button_action v-if="group.delivered" class="go disabled" :shadowed="true">Delivered</button_action>
										<button_action v-else-if="aircraft && aircraft_current ? aircraft.id != aircraft_current.id : false" class="go" :class="{ 'disabled': !group.transferable }" :shadowed="true" @click.native.stop="transfer($event, entry.adventure.id, group.action, group.manifest, group.guid )">Transfer</button_action>
										<button_action v-else-if="!group.loaded_on_id" class="go" :class="{ 'disabled': !group.loadable }" :shadowed="true" @click.native.stop="load($event, entry.adventure.id, group.action, group.manifest, group.guid )">Load</button_action>
										<button_action v-else-if="group.deliverable" class="go" :shadowed="true" @click.native.stop="unload($event, entry.adventure.id, group.action, group.manifest, group.guid )">Deliver</button_action>
										<button_action v-else class="" :class="{ 'disabled': !group.unloadable }" :shadowed="true" @click.native.stop="unload($event, entry.adventure.id, group.action, group.manifest, group.guid )">Unload</button_action>
									</div>
								</template>
								<template v-slot:content>
									<div class="payload_set_fold_header">
										<div class="payload_set_fold_adventure">{{ entry.adventure.name ? entry.adventure.name : entry.adventure.route }}</div>
										<div class="payload_set_fold_weight"><weight :amount="pallet.unit_weight" :decimals="0" />/u</div>
									</div>
									<div class="payload_set_boxes">
										<!--
											 ="props"
										<div v-for="(box, index3) in group.boxes" v-bind:key="'pb' + index + '_' + index1 + '_' + index2 + '_' + index3" class="payload_set_box" :style="{ transitionDelay: props.expanded ? Math.sqrt(index3 * 10000) + 'ms' : '0s' }">
											<span v-if="box.health != 100">{{ box.health }}</span>
										</div>
										-->
									</div>
								</template>
							</collapser>
						</div>

					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Aircraft from '@/sys/classes/aircraft';
import { AppInfo } from '@/sys/foundation/app_model';
import Vue from 'vue';

export default Vue.extend({
	props: {
		sets: Array as () => Array<any>,
		aircraft: Object,
		aircraft_current: Object,
		is_aircraft :Boolean
	},
	data() {
		return {
		}
	},
	components: {
	},
	methods: {
		transfer(event :any, adventure :number, link_id :number, manifest :any = null, group :any = null, units :any[] = null, ) {
			this.$os.api.send_ws(
				"adventure:interaction",
				{
					id: adventure,
					link: link_id,
					verb: 'transfer',
					data: {
						manifest: manifest,
						group: group,
						units: units
					},
				}
			);
		},
		load(event :any, adventure :number, link_id :number, manifest :any = null, group :any = null, units :any[] = null, ) {
			this.$os.api.send_ws(
				"adventure:interaction",
				{
					id: adventure,
					link: link_id,
					verb: 'load',
					data: {
						manifest: manifest,
						group: group,
						units: units
					},
				}
			);
		},
		unload(event :any, adventure :number, link_id :number, manifest :any = null, group :any = null, units :any[] = null, ) {
			this.$os.api.send_ws(
				"adventure:interaction",
				{
					id: adventure,
					link: link_id,
					verb: 'unload',
					data: {
						manifest: manifest,
						group: group,
						units: units
					},
				}
			);
		},

	},
	mounted() {
	},
	beforeDestroy() {
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.payload_set {
	.theme--bright &,
	&.theme--bright {
		&_adventure {
			//@include shadowed_shallow($ui_colors_bright_shade_5);
			//&_content {
			//	background-color: rgba($ui_colors_bright_shade_1, 0.4);
			//}
		}
		&_group {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
			&:hover {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
			/*
			&_health {
				border-color: rgba($ui_colors_bright_shade_5, 0.2);
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
			*/
		}
		&_pallet {
			background-color: $ui_colors_bright_shade_1;
			border-color: rgba($ui_colors_bright_shade_5, 0.3);
		}
		&_fold {
			&_header {
				border-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
		}
		&_box {
			background-color: $ui_colors_bright_button_go;
			@include shadowed_shallow($ui_colors_bright_button_go);
			border-color: rgba($ui_colors_bright_shade_5, 0.2);
		}
	}

	.theme--dark &,
	&.theme--dark {
		&_adventure {
			//@include shadowed_shallow($ui_colors_dark_shade_0);
			//&_content {
			//	background-color: rgba($ui_colors_dark_shade_1, 0.4);
			//}
		}
		&_group {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			&:hover {
				background-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
			/*
			&_health {
				border-color: $ui_colors_dark_shade_0;
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
			*/
		}
		&_pallet {
			background-color: $ui_colors_dark_shade_1;
			border-color: rgba($ui_colors_dark_shade_0, 0.3);
		}
		&_fold {
			&_header {
				border-color: $ui_colors_dark_shade_0;
			}
		}
		&_box {
			background-color: $ui_colors_dark_button_go;
			@include shadowed_shallow($ui_colors_dark_button_go);
			border-color: rgba($ui_colors_dark_shade_5, 0.2);
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
		&.is-aircraft {

		}
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
	&_pallet {
		position: relative;
		padding: 8px;
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
		&_empty {
			margin-bottom: 4px;
		}
		&_sub {
			display: flex;
		}
	}
	&_group {
		position: relative;
		margin-bottom: 8px;
		border-radius: 2px;
		transition: background 0.2s ease-out;
		&:hover {
			transition: background 0.05s ease-out;
		}
		& .collapser_arrow {
			margin-right: 8px;
		}
		&_load {
			display: flex;
			margin: 12px;
			margin-left: 0;
		}
		&_image {
			width: 36px;
			min-width: 36px;
			height: 36px;
			margin-right: 12px;
			background-image: url(../../../sys/assets/cargo/default.png);
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
			padding: 12px;
			& > div {
				flex-basis: 0;
			}
		}
		&_sub {
			display: flex;
		}
		&_health {
			margin-left: 8px;
			&::before {
				content: '•';
				margin-right: 8px;
				opacity: 0.2;
			}
		}
		/*
		&_health {
			border-top: 1px solid rgba(0,0,0,0.1);
			position: relative;
			margin: 0 8px;
			padding: 6px;
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
		}*/
	}
	&_fold {
		&_header {
			display: flex;
			justify-content: space-between;
			margin: 0 8px;
			margin-bottom: 2px;
			border-top: 1px solid transparent;
			padding-top: 4px;
		}
	}
	&_boxes {
		position: relative;
		display: flex;
		flex-wrap: wrap;
		padding: 8px;
		padding-top: 0;
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
		.payload_set_group > .collapser_expanded & {
			opacity: 1;
			transition: opacity 1s ease-out;
		}
	}
}
</style>