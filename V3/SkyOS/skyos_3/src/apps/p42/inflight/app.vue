<template>
	<div :class="[appName, app.nav_class]">

		<div class="app-frame">
			<div v-if="state.status != 0" class="app-box shadowed-deep">
				<div class="searching" v-if="state.status == 1">
					<span class="title">Searching...</span>
					<p>This might take a few seconds.</p>
				</div>
				<div class="no-transponder" v-else-if="state.status == 2">
					<span class="title">No Transponder</span>
					<p>The Skypark Transponder is required to browse contracts.</p>
					<p>Please start your Transponder.</p>
				</div>
				<div class="no-results" v-else-if="state.status == 3">
					<span class="title">No Flight</span>
					<p>Load your aircraft in the sim to manage the cabin.</p>
				</div>
			</div>
			<div v-else>
				<CabinPanels class="app-box shadowed-deep " v-if="selected_aircraft" :aircraft="selected_aircraft" :cabin="selected_aircraft.cabin" :state="cabin_state" />
			</div>
		</div>
		<app_panel :app="app" :has_content="true" :scroll_top="ui.panel_scroll_top">
			<transition :duration="800">
				<scroll_view :scroller_offset="{ top: 36, bottom: 30 }">
					<div class="app-panel-wrap">
						<div class="app-panel-content">
							<div class="app-panel-hit">
								<Cabin v-if="selected_aircraft" :aircraft="selected_aircraft" :cabin="selected_aircraft.cabin" :state="cabin_state" :level="cabin_state.level" />
							</div>
						</div>
					</div>
				</scroll_view>
			</transition>

			<div class="app-panel-hit" v-if="selected_aircraft && cabin_state">
				<div class="cabin-levels" v-if="selected_aircraft.cabin.levels.length > 1">
					<div class="app-box nooverflow transparent small shadowed-deep">
						<div class="confine">
							<div v-for="(l, i) in get_levels()" :key="i">
								<button_action @click.native="set_level(l)" class="listed map-control" :class="{ 'info': cabin_state.level == l }">{{ l + 1 }}. {{ selected_aircraft.cabin.get_level_label(l) }}</button_action>
							</div>
						</div>
					</div>
				</div>
			</div>

		</app_panel>
  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import SearchStates from "@/sys/enums/search_states";
import Aircraft from '@/sys/classes/aircraft';
import Cabin from '@/sys/components/cabin/aircraft_cabin.vue';
import CabinPanels from '@/sys/components/cabin/aircraft_cabin_panels.vue';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		Cabin,
		CabinPanels
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			selected_aircraft: null as Aircraft,
			cabin_state: new AircraftCabinState(),
			state: {
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
			},
			ui: {
				panel: false,
				subframe: null,
				panel_scroll_top: 0,
			},
		}
	},
	methods: {

		init() {
			if(this.has_transponder) {
				if(this.selected_aircraft) {
					this.state.status = SearchStates.Searching;
					this.$os.api.send_ws(
						"cabin:get-all", {
							fields: null
						},
						(wsmsg) => {

							this.state.status = SearchStates.Idle;

							if(wsmsg.payload.humans) {
								this.cabin_state.humans = wsmsg.payload.humans;
							}

							if(wsmsg.payload.humans) {
								this.cabin_state.cargos = wsmsg.payload.cargos;
							}

							if(wsmsg.payload.humans_state) {
								wsmsg.payload.humans_state.forEach(human_state => {
									const host = this.cabin_state.humans.find(x => x.guid == human_state.guid);
									if(host) {
										this.$set(host, 'state', human_state.state);
									}
								});
							}

							if(wsmsg.payload.cargos_state) {
								wsmsg.payload.cargos_state.forEach(cargo_state => {
									const host = this.cabin_state.cargos.find(x => x.guid == cargo_state.guid);
									if(host) {
										this.$set(host, 'state', cargo_state.state);
									}
								});
							}
						}
					);

					this.set_level(this.selected_aircraft.cabin.levels.length - 1)
				} else {
					this.state.status = SearchStates.NoResults;
				}
			} else {
				this.state.status = SearchStates.NoTransponder;
			}
		},

		set_level(index :number) {
			this.cabin_state.level = index;
		},

		get_levels() {
			const levels = [];
			const length = this.selected_aircraft.cabin.levels.length;

			this.selected_aircraft.cabin.levels.forEach((level, index) => {
				levels.push(length - index - 1);
			});

			return levels;
		},

		listener_os_fleet(wsmsg :any) {
			switch(wsmsg.name) {
				case 'current_aircraft': {
					this.selected_aircraft = wsmsg.payload.aircraft as Aircraft
					this.cabin_state.humans = [];
					this.cabin_state.cargos = [];
					this.init();
					break;
				}
				case 'remove':
				case 'mutate': {
					this.$os.fleetService.event([wsmsg.name], wsmsg.payload.id, wsmsg.payload.aircraft, this.selected_aircraft);
					break;
				}
			}
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					this.init();
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					this.state.status = SearchStates.NoTransponder;
					this.cabin_state.humans = [];
					this.cabin_state.cargos = [];
					break;
				}
				case 'cabin': {

					if(wsmsg.payload.humans) {
						this.cabin_state.humans = wsmsg.payload.humans;
					}

					if(wsmsg.payload.cargos) {
						this.cabin_state.cargos = wsmsg.payload.cargos;
					}

					//if(wsmsg.payload.cabin) {
					//	Eljs.merge_deep(this.selected_aircraft.cabin, wsmsg.payload.cabin);
					//}

					wsmsg.payload.humans_removed.forEach(human => {
						const found = this.cabin_state.humans.find(h => h.guid == human);
						const found_index = this.cabin_state.humans.indexOf(found);
						if(found_index > -1) {
							this.cabin_state.humans.splice(found_index, 1);
						}
					});

					wsmsg.payload.humans_state.forEach(human_state => {
						const host = this.cabin_state.humans.find(x => x.guid == human_state.guid);
						if(host) {
							this.$set(host, 'state', human_state.state);
						}
					});

					wsmsg.payload.cargos_removed.forEach(cargo => {
						const found = this.cabin_state.cargos.find(h => h.guid == cargo);
						const found_index = this.cabin_state.cargos.indexOf(found);
						if(found_index > -1) {
							this.cabin_state.cargos.splice(found_index, 1);
						}
					});

					wsmsg.payload.cargos_state.forEach(cargo_state => {
						const host = this.cabin_state.cargos.find(x => x.guid == cargo_state.guid);
						if(host) {
							this.$set(host, 'state', cargo_state.state);
						}
					});

					break;
				}
			}
		},
	},
	beforeMount() {
		this.selected_aircraft = this.$os.fleetService.aircraft_current;
		this.$emit('loaded');
		this.$os.system.set_cover(true);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('fleet', this.listener_os_fleet);
		this.init();
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.off('fleet', this.listener_os_fleet);
	}
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';

	.cabin-levels {
		position: absolute;
		top: $status-size + 50px;
		left: 0;
		transition: right 0.4s ease-out, transform 0.4s ease-out;
		z-index: 10;
	}

	.app-panel-hit {
		position: initial;
	}

	.app-panel {
		.theme--bright & {
			background: linear-gradient(to right, rgba($ui_colors_bright_shade_0, 0), $ui_colors_bright_shade_0, rgba($ui_colors_bright_shade_0, 0));
		}
		.theme--dark & {
			background: linear-gradient(to right, rgba($ui_colors_dark_shade_0, 0), $ui_colors_dark_shade_0, rgba($ui_colors_dark_shade_0, 0));
		}

	}

</style>