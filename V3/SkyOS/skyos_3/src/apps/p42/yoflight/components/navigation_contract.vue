<template>
	<div class="state-control">

		<!-- Saved -->
		<div class="state-control--ready color-go app-box app-box-margined shadowed-deep h_edge_padding_top_half" v-if="contract.state == 'Saved'">

			<div class="h_edge_padding_bottom_half">
				<h3 class="separator h_edge_margin_bottom_none">{{ contract.route }}</h3>
				<p v-if="contract.title.length" class="text-center">{{ contract.title }}</p>
				<div class="text-center"><expire class="expire" :contract="contract"/></div>
			</div>
			<div class="h_edge_padding_lateral h_edge_padding_bottom">

				<div class="text-center h_edge_padding_vertical">
					<div v-if="contract.aircraft_compatible || !contract.template.aircraft_restriction_label.length">
						<p v-if="contract.situations[0].airport">Load into <strong>{{ contract.situations[0].airport.icao }}</strong> ({{ contract.situations[0].airport.name}}) to begin.</p>
						<p v-else>Load at {{ contract.situations[0].location[0] + ', ' + contract.situations[0].location[1] }} to begin.</p>
					</div>
					<div v-else>
						<p v-if="contract.situations[0].airport">Choose the <strong>{{ contract.template.aircraft_restriction_label }}</strong> and load into <strong>{{ contract.situations[0].airport.icao }}</strong> ({{ contract.situations[0].airport.name}}) to finalize quotes &amp; begin.</p>
						<p v-else>Choose the <strong>{{ contract.template.aircraft_restriction_label }}</strong> and load at {{ contract.situations[0].location[0] + ', ' + contract.situations[0].location[1] }} to finalize quotes &amp; begin.</p>
					</div>
				</div>

				<div v-if="contract.invoices" class="h_edge_padding_bottom">
					<ContractProfits :contract="contract"/>
					<collapser :preload="true" :default="false" :state="fees_visible || contract.state == 'Succeeded' || contract.state == 'Failed'">
						<template v-slot:content>
							<Invoices class="h_edge_margin_top"  :contract="contract"  :default_page="contract.state != 'Succeeded' && contract.state != 'Failed' ? 'estimates' : 'paid'" />
						</template>
					</collapser>
					<button_action class="small outlined theme--dark h_edge_margin_top" @click.native="fees_visible = !fees_visible" v-if="contract.state != 'Succeeded' && contract.state != 'Failed'">{{ fees_visible ? 'Hide Fees' : 'View Fees' }}</button_action>
				</div>

				<div class="footer" v-if="contract.invoices">
					<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.request_status != 0 }" @hold="interactState($event, 'remove')">Unsave</button_nav>
					<button_nav class="info" :class="{ 'disabled': !$os.simulator.live || !contract.path[0].range || !contract.aircraft_compatible }" shape="forward"  @click.native="interactState($event, 'commit')"><span v-if="contract.invoices.total_fees != 0">Pay <currency class="reward_bux" :amount="contract.invoices.total_fees" :decimals="0"/> & </span>Begin</button_nav>
				</div>
				<div class="footer" v-else>
					<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.request_status != 0 }" @hold="interactState($event, 'remove')">Unsave</button_nav>
					<button_nav class="info" :class="{ 'disabled': !$os.simulator.live || !contract.path[0].range || !contract.aircraft_compatible }" shape="forward"  @click.native="interactState($event, 'commit')">Begin</button_nav>
				</div>
			</div>
		</div>

		<!-- Active and Monitored -->
		<div class="state-control--ready color-active app-box app-box-margined shadowed-deep h_edge_padding_top_half" v-if="contract.state == 'Active' && contract.is_monitored">

			<collapser :withArrow="true" :default="true">
				<template v-slot:title>
					<div class="h_edge_padding_bottom">
						<div class="columns">
							<div class="column">
								<h3 class="separator h_edge_margin_bottom_none">{{ contract.route }}</h3>
								<p v-if="contract.title.length" class="text-center">{{ contract.title }}</p>
								<div class="text-center"><expire class="expire" :contract="contract"/></div>
							</div>
							<div class="column column_narrow h_edge_padding_top_quarter h_edge_padding_lateral_half">
								<div class="collapser_arrow"></div>
							</div>
						</div>
					</div>
				</template>
				<template v-slot:content>
					<div class="h_edge_padding_lateral h_edge_padding_bottom">

						<Todo class="small" :contract="contract" :features="{
							current_location: false,
							completed_node: false,
							all_next: false
						}"/>

						<div class="columns">
							<div class="column">
								<button_action class="cancel" :class="{ 'disabled': contract.request_status != 0 }" @click.native="interactState($event, 'pause')">Pause{{ contract.template.type_label ? ' ' + contract.template.type_label : '' }}</button_action>
								<!--<button_nav class="info" shape="forward" @click.native="interactState($event, 'plan')">Plan Flight</button_nav>-->
							</div>
						</div>

					</div>
				</template>
			</collapser>
		</div>

		<!-- Ready to Start -->
		<div class="state-control--ready color-go app-box app-box-margined shadowed-deep h_edge_padding_top_half" v-if="contract.state == 'Active' && !contract.is_monitored && !contract.last_location_geo">
			<div class="h_edge_padding_bottom_half">
				<h3 class="separator h_edge_margin_bottom_none">{{ contract.route }}</h3>
				<p v-if="contract.title.length" class="text-center">{{ contract.title }}</p>
				<div class="text-center"><expire class="expire" :contract="contract"/></div>
			</div>
			<div class="h_edge_padding_lateral h_edge_padding_bottom">
				<div v-if="contract.aircraft_compatible" class="content h_edge_padding_bottom_half text-center">
					<p>The bags are packed, let's go!</p>
					<p class="notice" v-if="contract.situations[0].airport">Go to <strong>{{ contract.situations[0].airport.icao }}</strong> ({{ contract.situations[0].airport.name}}) to begin.</p>
					<p class="notice" v-else>Go to {{ contract.situations[0].location[0] + ', ' + contract.situations[0].location[1] }} to begin.</p>
				</div>
				<div v-else class="content h_edge_padding_bottom_half">
					<p>Let's go!</p>
					<p class="notice" v-if="contract.situations[0].airport">Go to <strong>{{ contract.situations[0].airport.icao }}</strong> ({{ contract.situations[0].airport.name}}) in the <strong>{{ contract.template.aircraft_restriction_label }}</strong> to begin.</p>
					<p class="notice" v-else>Go to {{ contract.situations[0].location[0] + ', ' + contract.situations[0].location[1] }} in the <strong>{{ contract.template.aircraft_restriction_label }}</strong> to begin.</p>
				</div>
				<div class="columns">
					<div class="column">
						<button_nav class="cancel outlined" :hold="true" :class="{ 'disabled': contract.request_status != 0 }" @hold="interactState($event, 'cancel')">Cancel{{ contract.template.type_label ? ' ' + contract.template.type_label : '' }}</button_nav>
					</div>
				</div>
			</div>
		</div>

		<!-- Paused -->
		<div class="state-control--paused app-box app-box-margined shadowed-deep h_edge_padding_top_half" v-if="contract.state == 'Active' && !contract.is_monitored && contract.last_location_geo && (contract.aircraft_compatible || !$os.simulator.connected)">
			<div class="h_edge_padding_bottom_half">
				<h3 class="separator h_edge_margin_bottom_none">{{ contract.route }}</h3>
				<p v-if="contract.title.length" class="text-center">{{ contract.title }}</p>
				<div class="text-center"><expire class="expire" :contract="contract"/></div>
			</div>
			<div class="h_edge_padding_lateral h_edge_padding_bottom text-center">
				<div class="content h_edge_padding_bottom_half">
					<p class="h_edge_padding_bottom_half">This {{  contract.template.type_label }} is paused because the aircraft/location has changed, the simulator was closed or you manually paused it.</p>

					<div class="columns" v-if="contract.last_location_geo">
						<div class="column">
							<div class="description">Resume location{{ contract.last_location_airport ? ' near ' + contract.last_location_airport.icao : '' }}</div>
							<div class="location"><strong>{{ contract.last_location_geo[1] }}</strong>, <strong>{{ contract.last_location_geo[0] }}</strong></div>
						</div>
						<div class="column column_narrow column_justify_center">
							<button_action class="go small" @click.native="copyLocation">COPY</button_action>
						</div>
					</div>

					<div class="aircraft h_edge_padding_top_half">{{ contract.aircraft_used[contract.aircraft_used.length-1] }}</div>

				</div>
				<div class="footer">
					<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.request_status != 0 }" @hold="interactState($event, 'cancel')">Cancel{{ contract.template.type_label ? ' ' + contract.template.type_label : '' }}</button_nav>
					<button_nav shape="forward" class="go" :class="{ 'disabled': ((!CanResume || contract.request_status != 0 ) && (!$os.system.isDev)) || !sim_live }" @click.native="interactState($event, 'resume')">Resume</button_nav>
				</div>
			</div>
		</div>

		<!-- Specific Aircraft Required -->
		<div class="state-control--aircraft app-box app-box-margined shadowed-deep h_edge_padding_top_half" v-if="(contract.state == 'Active' || contract.state == 'Saved') && contract.last_location_geo && (!contract.aircraft_compatible && $os.simulator.connected)">
			<div>
				<div class="title">Specific aircraft required</div>
				<p>Requires the <strong>{{ contract.template.aircraft_restriction_label }}</strong>.</p>
				<p class="notice">Load the aircraft in sim to resume.</p>
			</div>
			<div class="footer">
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.request_status != 0 }" @hold="interactState($event, 'cancel')">Cancel{{ contract.template.type_label ? ' ' + contract.template.type_label : '' }}</button_nav>
			</div>
		</div>

		<!-- Completed -->
		<div class="state-control--completed app-box app-box-margined shadowed-deep h_edge_padding_top_half" v-if="contract.state == 'Succeeded'">
			<div class="h_edge_padding_bottom_half">
				<h3 class="separator h_edge_margin_bottom_none">{{ contract.route }}</h3>
				<p v-if="contract.title.length" class="text-center">{{ contract.title }}</p>
			</div>
			<div class="h_edge_padding_lateral h_edge_padding_bottom text-center">
				<p>Completed in {{ moment(CompletedAt).from(StartedAt, true) }}. {{ contract.end_summary.length ? contract.end_summary : 'Great job!' }}</p>
			</div>
		</div>

		<!-- Failed -->
		<div class="state-control--failed app-box app-box-margined shadowed-deep h_edge_padding_top_half" v-if="contract.state == 'Failed'">
			<div class="h_edge_padding_bottom_half">
				<h3 class="separator h_edge_margin_bottom_none">{{ contract.route }}</h3>
				<p v-if="contract.title.length" class="text-center">{{ contract.title }}</p>
				<div class="text-center"><expire class="expire" :contract="contract"/></div>
			</div>
			<div class="h_edge_padding_lateral h_edge_padding_bottom text-center">
				<p>Failed. {{ contract.end_summary != null ? contract.end_summary : "That's unfortunate..." }}</p>
			</div>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Contract, { RequestStatus } from '@/sys/classes/contracts/contract';
import Eljs from '@/sys/libraries/elem';
import moment from 'moment';
import Todo, { Features } from '@/sys/components/contracts/panel/child/todo.vue';
import Invoices from "@/sys/components/contracts/panel/child/invoices.vue"
import ContractProfits from "@/sys/components/contracts/contract_profits.vue"

export default Vue.extend({
	props: {
		contract :Contract,
		type :String
	},
	components: {
		Todo,
		Invoices,
		ContractProfits,
	},
	data() {
		return {
			fees_visible: false,
			sim_live: this.$os.simulator.live,
			sim_location: [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat],
			CompletedAt: null,
			StartedAt: null,
			CanResume: false,
			Interactions: [],
		}
	},
	methods: {
		copyLocation() {
			if(!this.contract.is_monitored && this.contract.last_location_airport) {
				Eljs.copyTextToClipboard(this.contract.last_location_airport.icao);
				this.$os.modals.add({
					type: 'notify',
					title: this.contract.last_location_airport.icao,
					text: [
						this.contract.last_location_airport.icao +' has been copied to your clipboard.',
						'You can paste this ICAO in the search field of the MSFS world map.'
					],
				});
			} else {

				let location = this.sim_location;
				if(!this.contract.is_monitored && this.contract.last_location_geo) {
					location = this.contract.last_location_geo;
				}

				Eljs.copyTextToClipboard(location[1] + ', ' + location[0]);
				this.$os.modals.add({
					type: 'notify',
					title: 'Coordinates',
					text: [
						'Coordinates have been copied.',
						location[1] + ', ' + location[0],
						'You can paste this location in the search field of the MSFS world map.',
						'Be aware that MSFS does not allow you to start on the ground when using coordinates.'
					],
				});
			}

		},
		rangeCheck() {
			if(this.contract.state == 'Active' && this.contract.last_location_geo) {
				const simSvc = this.$os.simulator;
				const dist = Eljs.GetDistance(simSvc.location.Lat, simSvc.location.Lon, this.contract.last_location_geo[1], this.contract.last_location_geo[0], "N");
				if(dist < 10) {
					this.CanResume = true;
				} else {
					this.CanResume = false;
				}
			}
		},
		interactionsCheck() {
			this.Interactions = [];
			this.contract.path.forEach((node :any) => {
				node.actions.forEach((action :any) => {
					const interactions = this.contract.interactions.filter((x :any) => x.essential && x.id == action.id);
					if(interactions.length) {
						this.Interactions.push({
							label: action.Description,
							actions: interactions
						})
					}
				});
			});
		},
		calculate() {
			this.CompletedAt = new Date(this.contract.completed_at);
			this.StartedAt = new Date(this.contract.started_at);
			this.rangeCheck();
			this.interactionsCheck();
		},
		getInvoice() {
			this.$os.contract_service.interact(this.contract, "invoices", null, null);
		},
		viewQuote() {
			this.$os.modals.add({
				type: 'invoice',
				title: 'Start fees',
				actions: {
					yes: (this.contract.path[0].range && this.contract.aircraft_compatible) ? 'Pay and fly!' : null,
					no: 'Close'
				},
				data: {
					InvoiceData: this.contract.invoices,
					Contract: this.contract
				},
				func: (state :boolean) => {
					if(state) {
						this.interactState(null, 'commit')
					}
				}
			});
		},
		interactState(ev: Event, name: string) {
			switch(name){
				case "commit": {
					this.$os.contract_service.interact(this.contract, name, { ID: this.contract.id });
					break;
				}
				default: {
					this.$os.contract_service.interact(this.contract, name, null, null);
					break;
				}
			}

		},
		interactAction(ev: Event, interaction: any) {
			this.$os.contract_service.interact(this.contract, "interaction", {
					id: this.contract.id,
					link: interaction.id,
					verb: interaction.verb,
					Data: {},
				}, (md) => {
				if(md.Success) {
					this.calculate();
				}
			});
		},

		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.calculate();
					break;
				}
			}
		},
		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.sim_live = wsmsg.payload;
					break;
				}
				case 'meta': {
					this.sim_location = [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat];
					break;
				}
			}
		},
		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect':
				case 'disconnect':
				case 'locationhistory': {
					this.getInvoice();
					break;
				}
				case 'adventure': {
					this.calculate();
					break;
				}
				case 'eventbus': {
					switch(wsmsg.name[1]){
						case 'meta': {
							this.rangeCheck();
							break;
						}
						case 'event': {
							wsmsg.payload.forEach((pl: any) => {
								switch(pl.Type) {
									case 'Position': {
										this.rangeCheck();
										break;
									}
									case 'AircraftChange': {
										this.getInvoice();
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
	mounted() {
		this.calculate();
		this.rangeCheck();
		this.getInvoice();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('ws-in', this.listenerWs);
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listenerWs);
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
		this.$os.eventsBus.Bus.off('contracts', this.listener_os_contracts);
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.calculate();
					this.rangeCheck();
					this.getInvoice();
				}
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
@import '@/sys/scss/helpers.scss';

.state-control {
	position: relative;
	z-index: 2;
	margin-left: 14px;
	margin-right: 14px;

	.theme--bright & {
		> div {
			@include shadowed_shallow($ui_colors_bright_shade_5);
			.interaction {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
		}
		&--fees {
			color: $ui_colors_bright_shade_0;
			$clr: darken($ui_colors_bright_button_gold, 20%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--ready {
			&.color-go {
				color: $ui_colors_bright_shade_0;
				$clr: desaturate($ui_colors_bright_button_go, 30%);
				background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
			}
			&.color-active {
				color: $ui_colors_bright_shade_5;
				$clr: desaturate(lighten($ui_colors_bright_button_go, 40%), 80%);
				background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
			}
		}
		&--paused {
			color: $ui_colors_bright_shade_0;
			$clr: desaturate($ui_colors_bright_button_cancel, 60%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--aircraft {
			color: $ui_colors_bright_shade_0;
			$clr: desaturate($ui_colors_bright_button_warn, 60%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--completed {
			color: $ui_colors_bright_shade_0;
			$clr: desaturate($ui_colors_bright_button_info, 30%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--failed {
			color: $ui_colors_bright_shade_0;
			$clr: $ui_colors_bright_shade_4;
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
	}

	.theme--dark & {
		> div {
			@include shadowed_shallow($ui_colors_dark_shade_0);
			.interaction {
				background-color: rgba($ui_colors_dark_shade_2, 0.4);
			}
		}
		&--fees {
			color: $ui_colors_dark_shade_5;
			$clr: darken($ui_colors_dark_button_gold, 20%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--ready {
			&.color-go {
				color: $ui_colors_dark_shade_5;
				$clr: desaturate($ui_colors_dark_button_go, 20%);
				background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
			}
			&.color-active {
				color: $ui_colors_dark_shade_5;
				$clr: desaturate(darken($ui_colors_dark_button_go, 25%), 80%);
				background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
			}
		}
		&--paused {
			color: $ui_colors_dark_shade_5;
			$clr: desaturate($ui_colors_dark_button_cancel, 60%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--aircraft {
			color: $ui_colors_dark_shade_5;
			$clr: desaturate($ui_colors_dark_button_warn, 60%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--completed {
			color: $ui_colors_dark_shade_5;
			$clr: desaturate($ui_colors_dark_button_info, 30%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--failed {
			color: $ui_colors_dark_shade_5;
			$clr: $ui_colors_dark_shade_1;
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
	}

	> div {
		position: relative;
		display: flex;
		flex-direction: column;
		border-radius: 8px;
		margin-bottom: 8px;
		.content {
			margin-bottom: 8px;
			z-index: 2;
		}
		.header {
			display: flex;
			& > div {
				display: flex;
				align-items: center;
				&:first-child {
					flex-grow: 1;
				}
			}
		}
		.footer {
			margin-bottom: 0;
			display: flex;
			justify-content: space-between;
		}
		.title {
			font-size: 1.4em;
			font-family: "SkyOS-SemiBold";
			margin-right: 8px;
		}
		p {
			margin: 0;
			margin-bottom: 0.2em;
			&:last-child {
				margin-bottom: 0;
			}
		}
		.interaction {
			display: flex;
			align-items: stretch;
			padding: 8px;
			border-radius: 8px;
			&-label {
				align-self: center;
				font-family: "SkyOS-SemiBold";
				flex-grow: 1;
				margin-right: 8px;
			}
			&-actions {
				display: flex;
				.button_action  {
					padding: 2px 8px;
					white-space: nowrap;
				}
			}
		}
	}
	.button_nav {
		margin-right: 4px;
		&:last-child {
			margin-right: 0;
		}
	}
	.invoice {
		margin-bottom: 8px;
		&:last-child {
			margin-bottom: 0;
		}
	}

	.todolist {
		position: relative;
		z-index: 5;
		overflow: hidden;
		margin-top: 8px;
		margin-bottom: 4px;
		margin-left: -8px;
		margin-right: -8px;
		padding-left: 8px;
		padding-right: 8px;
	}


}


</style>