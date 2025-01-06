<template>
	<scroll_stack :sid="'p42_yoflight_navigate'" :key="'p42_yoflight_navigate'">
		<template v-slot:top>
			<div class="app-box app-box-margined shadowed-deep h_edge_padding_half">
				<div class="columns">
					<div class="column column_narrow h_edge_margin_right">
						<button_nav class="no-wrap shadowed-shallow" shape="back" @click.native="$emit('close')">Flight plans</button_nav>
					</div>
				</div>
			</div>
		</template>
		<template v-slot:content class="confine">
			<div class="h_edge_padding_top_half h_edge_padding_bottom_half">

				<!--
				<Navigation_next :leg="active_leg_node" v-if="active_leg_node && (contract ? (contract.state == 'Active' && contract.is_monitored) : true)" />
				<Navigation_airport v-if="active_leg_node ? active_leg_node.start.airport : false" :leg="active_leg_node" />
				<Navigation_airport v-if="prev_leg_node ? prev_leg_node.start.airport : false" :leg="prev_leg_node" />
				-->

				<Navigation_humans v-if="humans.length" :contract="contract" :humans="humans"/>

				<Navigation_contract :contract="contract" :type="'small'" />

				<!--<Navigation_destination  v-if="active_leg_node != last_leg_node" :active_leg="active_leg_node" :leg="last_leg_node" />-->

			</div>
		</template>
		<template v-slot:bottom>
			<div class="app-box nav-info app-box-margined shadowed-deep h_edge_padding_half">
				<div class="columns h_edge_padding_bottom">
					<div class="column">
						<data_stack class="center small" label="Next Wp">
							<span v-if="active_leg_node">{{ active_leg_node.start.code }}</span>
							<span v-else>~</span>
						</data_stack>
					</div>
					<div class="column">
						<data_stack class="center small" label="Elevation">
							<span v-if="active_leg_node"><height :amount="active_leg_node.start.airport.elevation" :decimals="0"/></span>
							<span v-else>~</span>
						</data_stack>
					</div>
					<div class="column">
						<data_stack class="center small" label="Track">
							<span v-if="active_leg_node ? active_leg_node.dist_to_go >= 0 : false"><heading :amount="active_leg_node.bearing_to - $os.simulator.location.MagVar" :decimals="0"/>°</span>
							<span v-else>~</span>
						</data_stack>
					</div>
				</div>
				<div class="columns">
					<div class="column">
						<data_stack class="center small" label="Distance">
							<span v-if="active_leg_node ? active_leg_node.dist_to_go >= 0 : false"><distance :amount="active_leg_node.dist_to_go" :decimals="0"/></span>
							<span v-else>~</span>
						</data_stack>
					</div>
					<div class="column">
						<data_stack class="center small" label="ETE">
							<span v-if="active_leg_node ? active_leg_node.ete >= 0 : false"><duration :time="active_leg_node.ete" :decimals="1" :brackets="{ to_minutes : 1 }"/></span>
							<span v-else>~</span>
						</data_stack>
					</div>
					<div class="column">
						<data_stack class="center small" label="To Track">
							<div class="to_track" v-if="active_leg_node ? active_leg_node.dist_to_go >= 0 : false">
								<div class="to_track_left_arrow" :class="{ 'visible': active_leg_node.bearing_dif <= -0.5 }">❮</div>
								<span v-if="Math.abs(active_leg_node.bearing_dif) > 0.5"><number :amount="active_leg_node.bearing_dif" :decimals="0"/>°</span>
								<span v-else>|</span>
								<div class="to_track_right_arrow" :class="{ 'visible': active_leg_node.bearing_dif >= 0.5 }">❯</div>
							</div>
							<span v-else>~</span>
						</data_stack>
					</div>
				</div>
			</div>
		</template>
	</scroll_stack>
</template>

<script lang="ts">
import Vue from 'vue';
import Flightplan from '@/sys/classes/flight_plans/plan';
import Contract from '@/sys/classes/contracts/contract';
import { NavLeg } from '@/sys/services/extensions/navigation';
import Navigation_next from './navigation_next.vue';
//import Navigation_destination from './navigation_destination.vue';
import Navigation_airport from './navigation_airport.vue';
import Navigation_contract from './navigation_contract.vue';
import Navigation_humans from './navigation_humans.vue';

export default Vue.extend({
	props: {
		contract :Contract,
		plan :Flightplan,
		index :Number,
		humans: Array
	},
	components: {
		Navigation_next,
		//Navigation_destination,
		Navigation_airport,
		Navigation_contract,
		Navigation_humans
	},
	data() {
		return {
			navigation_nodes: null,
			prev_leg_node: null as NavLeg,
			active_leg_node: null as NavLeg,
			next_leg_node: null as NavLeg,
			last_leg_node: null as NavLeg,
			next_leg_node_airport: null as NavLeg,
		}
	},
	methods: {
		init() {

		},

		listener_app_navigation(wsmsg :any) {
			switch(wsmsg.name){
				case 'data': {
					this.navigation_nodes = wsmsg.payload;

					this.active_leg_node = this.navigation_nodes.nodes.find(x => x.progress != 0 && x.progress != 100);
					const current_index = this.navigation_nodes.nodes.indexOf(this.active_leg_node);

					this.last_leg_node = this.navigation_nodes.nodes.at(-1);
					this.prev_leg_node = current_index > 1 ? this.navigation_nodes.nodes[current_index - 1] : null;
					this.next_leg_node_airport = this.navigation_nodes.nodes.find(x => x.index >= current_index && x.start.airport);

					const next_index = current_index + 1;
					this.next_leg_node = this.navigation_nodes.nodes[next_index];

					break;
				}
			}
		},
	},
	mounted() {
	},
	beforeMount() {
		this.init();

		this.$os.eventsBus.Bus.on("navigation", this.listener_app_navigation);
	},
	beforeDestroy() {

		this.$os.eventsBus.Bus.off("navigation", this.listener_app_navigation);
	},
	watch: {
		plan: {
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

	/deep/ .simplebar {
		mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 0) 5px, rgba(0, 0, 0, 1) 30px, rgba(0, 0, 0, 1) calc(100% - 30px), rgba(0, 0, 0, 0) calc(100% - 5px));
	}

	.scroll_stack {
		margin-left: -14px;
		margin-right: -14px;
	}

	.nav-info {
		.theme--bright & {
			background: $ui_colors_bright_magenta;
			color: $ui_colors_bright_shade_0;
		}
		.theme--dark & {
			background: $ui_colors_dark_magenta;
			color: $ui_colors_dark_shade_0;
		}
	}

	.app-box-margined {
		margin-left: 14px;
		margin-right: 14px;
	}


	.scroll_stack {
		overflow: visible;
	}

	.to_track {
		display: flex;
		flex-direction: row;
		justify-content: space-between;
		&_left_arrow {
			display: none;
			margin-right: 8px;
			&.visible {
				display: block;
			}
		}
		&_right_arrow {
			display: none;
			margin-left: 4px;
			&.visible {
				display: block;
			}
		}
	}

</style>