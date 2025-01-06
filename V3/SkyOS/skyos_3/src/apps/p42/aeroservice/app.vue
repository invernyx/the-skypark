<template>
	<div :class="[appName, app.nav_class]">

		<div class="app-frame" :class="{ 'has-subcontent': ui.subframe !== null }">
			<scroll_stack :sid="'p42_aeroservice_aircrafts_list'" class="app-box shadowed-deep">
				<!--
				<template v-slot:top>
					<div class="controls-top h_edge_padding">
					</div>
				</template>
				-->
				<template v-slot:content class="confine">
					<div class="searching" v-if="state.status == 1">
						<span class="title">Searching...</span>
						<p>This might take a few seconds.</p>
					</div>
					<div class="no-transponder" v-else-if="state.status == 2">
						<span class="title">No Transponder</span>
						<p>The Skypark Transponder is required to browse your fleet.</p>
						<p>Please start your Transponder.</p>
					</div>
					<div class="no-results" v-else-if="state.status == 3">
						<span class="title">No results</span>
						<p>Try moving the map to another location or adjust your filters.</p>
					</div>
					<div v-else-if="!ui.cabin_editor">
						<collapser :withArrow="true" :default="section.code == 'active'" v-for="(section, index) in state_sections.filter(x => x.fleet.length)" v-bind:key="index">
							<template v-slot:title>
								<div class="section_header">
									<h2><span class="notice">{{section.fleet.length }}</span> {{ section.name }}</h2>
									<div class="collapser_arrow"></div>
								</div>
							</template>
							<template v-slot:content>
								<AircraftBox
									v-for="(aircraft, index) in section.fleet.slice(section.offset, section.offset + section.limit)"
									v-bind:key="index"
									:index="index"
									:aircraft="aircraft"
									:selected="state.selected"
									@details="aircraft_select(aircraft)"/>
								<pagination
									class="h_edge_padding_vertical_half"
									:qty="section.fleet.length"
									:limit="section.limit"
									:offset="section.offset"
									@set_offset="section.offset = $event;"/>
							</template>
						</collapser>
					</div>
					<div v-else>
						<CabinPanels class="app-box shadowed-deep" v-if="state.selected" :aircraft="state.selected" :cabin="ui.cabin_editor" :state="ui.cabin_state" />
					</div>
				</template>
			</scroll_stack>
		</div>

		<app_panel :has_content="ui.panel" :has_subcontent="ui.subframe !== null" :scroll_top="ui.panel_scroll_top">
			<transition :duration="800">
				<router-view @scroll="ui.panel_scroll_top = $event.scrollTop"></router-view>
			</transition>
		</app_panel>
  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import SearchStates from "@/sys/enums/search_states";
import Airport from "@/sys/classes/airport";
import { AppInfo } from "@/sys/foundation/app_model"
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		AircraftBox: () => import("./components/aircraft.vue"),
		CabinPanels: () => import("@/sys/components/cabin/aircraft_cabin_panels.vue"),
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			ui: {
				panel: false,
				subframe: null,
				panel_scroll_top: 0,
				cabin_editor: null as AircraftCabin,
				cabin_state: null as AircraftCabinState,
			},
			state: {
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
				selected: null as Aircraft,
				fleet: [] as Aircraft[],
			},
			state_sections: [
				{
					code: "active",
					name: "Active",
					offset: 0,
					limit: 5,
					fleet: [] as Aircraft[],
				},
				{
					code: "inactive",
					name: "Inactive",
					offset: 0,
					limit: 5,
					fleet: [] as Aircraft[],
				}
			]
		}
	},
	methods: {

		update_results() {
			this.state_sections[0].offset = 0;
			this.state_sections[1].offset = 0;

			this.state_sections[0].fleet = this.state.fleet.filter(x => x.empty_weight > 0);
			this.state_sections[1].fleet = this.state.fleet.filter(x => x.empty_weight > 99999);
		},

		filters_search() {

			if(this.state.fleet.length) {
				this.aircraft_select(null);
			}

			if(this.has_transponder) {
				this.state.status = SearchStates.Searching;
				this.state.fleet = this.$os.fleetService.dispose_list(this.state.fleet);

				const queryoptions = {
					fields: null
				}

				this.$os.api.send_ws(
					"fleet:get_all",
					queryoptions,
					(fleetData) => {

						if(fleetData.payload.length) {
							this.state.fleet = this.$os.fleetService.ingest_list(fleetData.payload);
							this.state.status = SearchStates.Idle;

							if(this.$route.params.id) {
								const found = this.state.fleet.find(x => x.id == parseInt(this.$route.params.id));
								this.state.selected = found;
							}
						} else {
							this.state.status = SearchStates.NoResults;
						}

						this.update_results();
					}
				);
			}
		},

		aircraft_select(aircraft :Aircraft) {
			this.$os.eventsBus.Bus.emit('map_select', { name: 'fleet', payload: aircraft } );
		},
		aircraft_select_apply(aircraft :Aircraft) {
			if(aircraft) {
				if(this.state.selected ? (this.state.selected.id != aircraft.id) : true) {
					this.state.selected = aircraft;
					this.$os.routing.goTo({ name: 'p42_aeroservice_aircraft', params: { id: aircraft.id, aircraft: aircraft }});
				}
			} else {
				if(this.state.selected) {
					this.state.selected = null;
					this.$os.routing.goTo({ name: 'p42_aeroservice' });
				}
			}
		},

		aircraft_collapse() {
			this.$os.eventsBus.Bus.emit('os', { name: 'uncover', payload: true });
		},

		set_cabin_editor(data) {
			this.ui.cabin_editor = data.cabin;
			this.ui.cabin_state = data.state;
		},

		listener_os_fleet(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.fleetService.event_list([wsmsg.name], wsmsg.payload.id, wsmsg.payload.aircraft, this.state.fleet);
					this.update_results();
					break;
				}
			}
		},
		listener_map_select(wsmsg :any) {
			switch(wsmsg.name) {
				case 'fleet': {
					this.aircraft_select_apply(wsmsg.payload != null ? this.state.fleet.find(x => x.id == wsmsg.payload.id) : null);
					break;
				}
				default: {
					if(wsmsg.payload) {
						this.aircraft_select_apply(null);
					}
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to_pre': {
					this.ui.panel = wsmsg.route.matched.length > 1;
					switch(wsmsg.route.name) {
						case 'p42_aeroservice_aircraft': {
							this.state.selected  = wsmsg.route.params.aircraft;
							break;
						}
						default: {
							this.state.selected  = null;
							break;
						}
					}
					break;
				}
			}
		},
		listener_map(wsmsg: any) {
			switch(wsmsg.name){
				case 'loaded': {
					this.filters_search();
					break;
				}
			}
		},
		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					this.filters_search();
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					this.state.status = SearchStates.NoTransponder;
					this.state.fleet = [];
					break;
				}
			}
		},
	},
	mounted() {
		this.$emit('loaded');
		this.ui.panel = this.$route.matched.length > 1;

		this.app.events.emitter.on('aircraft_collapse', this.aircraft_collapse);
		this.app.events.emitter.on('set_cabin_editor', this.set_cabin_editor);

		if(this.$os.maps.main)
			this.filters_search();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('fleet', this.listener_os_fleet);
		this.$os.eventsBus.Bus.on('map_select', this.listener_map_select);

	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('map', this.listener_map);
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.off('fleet', this.listener_os_fleet);
		this.$os.eventsBus.Bus.off('map_select', this.listener_map_select);

		this.app.events.emitter.off('aircraft_collapse', this.aircraft_collapse);
		this.app.events.emitter.off('set_cabin_editor', this.set_cabin_editor);

		this.aircraft_select(null);
	}
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
	@import '@/sys/scss/helpers.scss';

	.searching {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		padding: 30px;
		text-align: center;
		&::before {
			content: '';
			opacity: 0.3;
			width: 130px;
			height: 140px;
			background-size: 130px;
			background-position: center;
			background-repeat: no-repeat;
			background-position: center top;
			.theme--bright & {
				background-image: url(../../../sys/assets/icons/dark/tickets.svg);
			}
			.theme--dark & {
				background-image: url(../../../sys/assets/icons/bright/tickets.svg);
			}
		}
		.title {
			font-family: "SkyOS-SemiBold";
			font-size: 18px;
			display: block;
		}
		p {
			margin: 0;
			margin-top: 8px;
		}
	}

	.section_header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 8px 16px;
		padding-top: 16px;
		h2 {
			margin: 0;
			.notice {
				opacity: 0.5;
			}
		}
	}

	.map-controls {
		pointer-events: all;
		position: absolute;
		right: $edge-margin + 60px;
		top: $status-size;
		.map-covered & {
			display: none;
		}
	}

</style>