<template>
	<div class="payload_set">
		<div v-for="(entry, index) in sets" v-bind:key="'pa' + index" class="payload_set_adventure">
			<div class="payload_set_adventure_content">
				<div class="payload_set_containers">

					<div v-for="(container, index1) in entry.containers" v-bind:key="'pp' + index + '_' + index1" class="payload_set_container" :style="container.dest_color ? 'background-color:' + container.dest_color.color : ''">

						<div class="payload_set_container_header">
							<div class="payload_set_container_destination" v-if="container.dest.airport">Going to <strong>{{ container.dest.airport.icao }}</strong> {{ container.dest.airport.name }}</div>
							<div class="payload_set_container_destination" v-else>Going to <strong>{{ container.dest.location }}</strong></div>
							<div class="payload_set_container_sub">
								<div class="payload_set_container_weight"><strong><weight :amount="container.weight" :decimals="0" /></strong></div>
							</div>
						</div>

						<div v-for="(group, index2) in container.groups" v-bind:key="'pg' + index + '_' + index1 + '_' + index2" class="payload_set_group">
							<collapser :withArrow="true" :default="false">
								<template v-slot:title>
									<div class="payload_set_group_header">
										<div class="payload_set_group_image" :class="[ 'payload_set_group_image_' + container.type ]"></div>
										<div class="payload_set_group_info">
											<div class="payload_set_group_name"><number :amount="group.boxes.length" :decimals="0" /> {{ group.name }}</div>
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
										<button_action v-if="!entry.contract_state.is_monitored && entry.contract_state.last_location_geo" class="go" :class="{ 'disabled': !resume_states[index] }" :shadowed="true" @click.native.stop="interactState($event, 'resume', entry.contract)">Resume</button_action>
										<button_action v-else-if="group.delivered" class="go disabled" :shadowed="true">Delivered</button_action>
										<button_action v-else-if="aircraft && aircraft_current ? aircraft.id != aircraft_current.id : false" class="go" :class="{ 'disabled': !group.transferable }" :shadowed="true" @click.native.stop="transfer($event, entry.contract.id, group.action, group.manifest, group.guid )">Transfer</button_action>
										<button_action v-else-if="!group.loaded_on_id" class="go" :class="{ 'disabled': !group.loadable }" :shadowed="true" @click.native.stop="load($event, entry.contract.id, group.action, group.manifest, group.guid )">Load</button_action>
										<button_action v-else-if="group.deliverable" class="go" :shadowed="true" @click.native.stop="unload($event, entry.contract.id, group.action, group.manifest, group.guid )">Deliver</button_action>
										<button_action v-else class="" :class="{ 'disabled': !group.unloadable }" :shadowed="true" @click.native.stop="unload($event, entry.contract.id, group.action, group.manifest, group.guid )">Unload</button_action>
									</div>
								</template>
								<template v-slot:content>
									<div class="payload_set_fold_header">
										<div class="payload_set_fold_adventure">{{ entry.contract.name ? entry.contract.name : entry.contract.route }}</div>
										<div class="payload_set_fold_weight"><weight :amount="container.unit_weight" :decimals="0" />/u</div>
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
import Contract from '@/sys/classes/contracts/contract';
import { AppInfo } from '@/sys/foundation/app_model';
import Eljs from '@/sys/libraries/elem';
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
			sim_live: this.$os.simulator.live,
			resume_states: [],
		}
	},
	components: {
	},
	methods: {
		check_resume_states() {
			this.resume_states = [];
			this.sets.forEach((set, i) => {
				if(this.sim_live) {
					if(set.contract_state.state == 'Active' && set.contract_state.last_location_geo) {
						const simSvc = this.$os.simulator;
						const dist = Eljs.GetDistance(simSvc.location.Lat, simSvc.location.Lon, set.contract_state.last_location_geo[1], set.contract_state.last_location_geo[0], "N");
						this.resume_states.push(dist < 10);
						return;
					}
				}
				this.resume_states.push(false);
			});
		},

		interactState(ev: Event, name: string, contract :Contract) {
			switch(name){
				case "commit": {
					this.$os.contract_service.interact(contract, name, { ID: contract.id });
					break;
				}
				default: {
					this.$os.contract_service.interact(contract, name, null, null);
					break;
				}
			}

		},

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


		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.sim_live = wsmsg.payload;
					break;
				}
			}
		},
		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'eventbus': {
					switch(wsmsg.name[1]){
						case 'meta': {
							this.check_resume_states();
							break;
						}
						case 'event': {
							wsmsg.payload.forEach((pl: any) => {
								switch(pl.Type) {
									case 'Position': {
										this.check_resume_states();
										break;
									}
								}
							});
							break;
						}
					}
				}
			}
		}

	},
	beforeMount() {
		this.check_resume_states();
		this.$os.eventsBus.Bus.on('ws-in', this.listenerWs);
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listenerWs);
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
	},
	watch: {
		sets() {
			this.check_resume_states();
		}
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
		&_container {
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
		&_container {
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
	&_container {
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
			width: 48px;
			min-width: 48px;
			height: 48px;
			margin-right: 12px;
			margin-top: -8px;
			margin-left: -4px;
			margin-bottom: -8px;
			background-size: contain;
			background-position: center;
			&_cargo {
				background-image: url(../../../sys/assets/cargo/default.png);
			}
			&_pax {
				background-image: url(../../../sys/assets/pax/default.png);
			}
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