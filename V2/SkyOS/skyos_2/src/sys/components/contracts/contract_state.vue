<template>
	<div class="state-control" v-if="type == 'full'">

		<!-- Saved -->
		<div class="state-control--ready" v-if="contract.State == 'Saved'">
			<div>
				<div class="title">Ready?</div>
			</div>
			<div v-if="contract.Invoices">
				<div class="data-stack data-stack--vertical">
					<!--
					<div>
						<span class="label">Liability</span>
						<span class="value">${{ contract.Invoices.TotalLiabilities.toLocaleString('en-gb') }}{{  contract.Invoices.UncertainLiabilities ? '+' : '' }}</span>
					</div>
					-->
					<div>
						<span class="label">Start fees</span>
						<span class="value" >${{ contract.Invoices.TotalFees.toLocaleString('en-gb') }}{{ contract.Invoices.UncertainFees ? '+' : '' }}</span>
					</div>
					<div>
						<span class="label">Profit</span>
						<span class="value">${{ contract.Invoices.TotalProfits.toLocaleString('en-gb') }}</span>
					</div>
				</div>
			</div>
			<div>
				<div v-if="contract.AircraftCompatible || !template.AircraftRestrictionLabel.length">
					<p v-if="contract.Situations[0].Airport">Load into <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) to begin.</p>
					<p v-else>Load at {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} to begin.</p>
				</div>
				<div v-else>
					<p v-if="contract.Situations[0].Airport">Choose the <strong>{{ template.AircraftRestrictionLabel }}</strong> and load into <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) to finalize quotes &amp; begin.</p>
					<p v-else>Choose the <strong>{{ template.AircraftRestrictionLabel }}</strong> and load at {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} to finalize quotes &amp; begin.</p>
				</div>
			</div>
			<div class="footer" v-if="contract.Invoices">
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'remove')">Unsave</button_nav>
				<div class="helper_flex">
					<button_nav v-if="contract.Invoices.TotalFees != 0 || contract.Invoices.TotalLiabilities != 0" class="theme--dark transparent outlined" @click.native="viewQuote">View fees</button_nav>
					<button_nav class="info" :class="{ 'disabled': !$root.$data.state.services.simulator.live || !contract.Path[0].Range || !contract.AircraftCompatible }" shape="forward"  @click.native="interactState($event, 'commit')">{{ contract.Invoices.TotalFees != 0 ? 'Pay $' + contract.Invoices.TotalFees.toLocaleString('en-gb') + ' &amp; ' : '' }}Begin</button_nav>
				</div>
			</div>
			<div class="footer" v-else>
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'remove')">Unsave</button_nav>
				<button_nav class="info" :class="{ 'disabled': !$root.$data.state.services.simulator.live || !contract.Path[0].Range || !contract.AircraftCompatible }" shape="forward"  @click.native="interactState($event, 'commit')">Begin</button_nav>
			</div>
		</div>

		<!-- Active and Monitored -->
		<div class="state-control--ready" v-if="contract.State == 'Active' && contract.IsMonitored">
			<div v-if="Interactions.length == 0">
				<div class="title">Ready to go!</div>
				<p>All your papers are in order.</p>
				<p class="notice">Follow the plan below and complete the contract.</p>
			</div>
			<div v-else>
				<div class="title">To do</div>
				<p>Here's what needs to be done right now:</p>
			</div>
			<div class="interaction" v-for="(interaction, index) in Interactions" v-bind:key="index">
				<div class="interaction-label">{{ interaction.Label }}</div>
				<div class="interaction-actions">
					<button_action
						v-for="(action, index) in interaction.Actions.filter(x => x.Label != '')" v-bind:key="index"
						@click.native="interactAction($event, action)"
						class="go" :class="{ 'disabled': !action.Enabled || contract.RequestStatus != 'ready' }">
							<span>{{ action.Label }}</span>
							<span v-if="action.Expire"><countdown :precise="true" :stop_zero="true" :time="new Date(action.Expire)"></countdown></span>
						</button_action>
				</div>
			</div>
			<div class="footer">
				<button_nav class="cancel" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @click.native="interactState($event, 'pause')">Pause{{ template.TypeLabel ? ' ' + template.TypeLabel : '' }}</button_nav>
				<button_nav class="info" shape="forward" @click.native="interactState($event, 'plan')">Plan Flight</button_nav>
			</div>
		</div>

		<!-- Ready to Start -->
		<div class="state-control--ready" v-if="contract.State == 'Active' && !contract.IsMonitored && !contract.LastLocationGeo">
			<div v-if="contract.AircraftCompatible">
				<div class="title">Let's go!</div>
				<p v-if="contract.Situations[0].Airport">Go to <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) to begin.</p>
				<p v-else>Go to {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} to begin.</p>
			</div>
			<div v-else>
				<div class="title">Let's go!</div>
				<p v-if="contract.Situations[0].Airport">Go to <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) in the <strong>{{ template.AircraftRestrictionLabel }}</strong> to begin.</p>
				<p v-else>Go to {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} in the <strong>{{ template.AircraftRestrictionLabel }}</strong> to begin.</p>
			</div>
			<div class="footer">
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'cancel')">Cancel{{ template.TypeLabel ? ' ' + template.TypeLabel : '' }}</button_nav>
				<button_nav class="info" shape="forward" @click.native="interactState($event, 'plan')">Plan Flight</button_nav>
			</div>
		</div>

		<!-- Paused -->
		<div class="state-control--paused" v-if="contract.State == 'Active' && !contract.IsMonitored && contract.LastLocationGeo && (contract.AircraftCompatible || !$root.$data.state.services.simulator.connected)">
			<div>
				<div class="title">Paused</div>
				<p>Paused because you changed location/aircraft, closed the simulator or manually paused it.</p>
				<p class="notice">To resume, go to the location circled on the map and press <em>Resume</em>.</p>
				<p class="notice">{{ 'Lat: ' + contract.LastLocationGeo[1] }}, {{ 'Lng: ' + contract.LastLocationGeo[0] }}</p>
			</div>
			<div class="footer">
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'cancel')">Cancel{{ template.TypeLabel ? ' ' + template.TypeLabel : '' }}</button_nav>
				<button_nav shape="forward" class="go" :class="{ 'disabled': (!CanResume || contract.RequestStatus != 'ready' || !$root.$data.state.services.simulator.live) && !$os.isDev }" @click.native="interactState($event, 'resume')">Resume</button_nav>
			</div>
		</div>

		<!-- Specific Aircraft Required -->
		<div class="state-control--aircraft" v-if="(contract.State == 'Active' || contract.State == 'Saved') && contract.LastLocationGeo && (!contract.AircraftCompatible && $root.$data.state.services.simulator.connected)">
			<div>
				<div class="title">Specific aircraft required</div>
				<p>Requires the <strong>{{ template.AircraftRestrictionLabel }}</strong>.</p>
				<p class="notice">Load the aircraft in sim to resume.</p>
			</div>
			<div class="footer">
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'cancel')">Cancel{{ template.TypeLabel ? ' ' + template.TypeLabel : '' }}</button_nav>
			</div>
		</div>

		<!-- Completed -->
		<div class="state-control--completed" v-if="contract.State == 'Succeeded'">
			<div>
				<div class="title">Completed in {{ moment(CompletedAt).from(StartedAt, true) }}</div>
				<p>{{ contract.EndSummary.length ? contract.EndSummary : 'Great job!' }}</p>
			</div>
			<!--
			<div class="footer">
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'remove')">Erase{{ template.TypeLabel ? ' ' + template.TypeLabel : '' }}</button_nav>
			</div>
			-->
		</div>

		<!-- Failed -->
		<div class="state-control--failed" v-if="contract.State == 'Failed'">
			<div>
				<div class="title">Failed</div>
				<p>{{ contract.EndSummary != null ? contract.EndSummary : "That's unfortunate..." }}</p>
			</div>
			<!--
			<div class="footer">
				<button_nav class="theme--dark cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'remove')">Erase{{ template.TypeLabel ? ' ' + template.TypeLabel : '' }}</button_nav>
			</div>
			-->
		</div>

	</div>
	<div class="state-control state-control--small" v-else-if="type == 'small'">

		<div class="state-control--ready" v-if="contract.State == 'Saved'">

			<div class="header">
				<div class="title">Ready?</div>
				<div v-if="contract.AircraftCompatible || !template.AircraftRestrictionLabel.length">
					<p v-if="contract.Situations[0].Airport">Load into <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) to begin.</p>
					<p v-else>Load at {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} to begin.</p>
				</div>
				<div v-else>
					<p v-if="contract.Situations[0].Airport">Choose the <strong>{{ template.AircraftRestrictionLabel }}</strong> and load into <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) to finalize quotes &amp; begin.</p>
					<p v-else>Choose the <strong>{{ template.AircraftRestrictionLabel }}</strong> and load at {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} to finalize quotes &amp; begin.</p>
				</div>
			</div>
			<div class="footer" v-if="contract.Invoices">
				<div></div>
				<div class="helper_flex">
					<button_nav v-if="contract.Invoices.TotalFees != 0 || contract.Invoices.TotalLiabilities != 0" class="theme--dark compact transparent outlined" @click.native="viewQuote">View fees</button_nav>
					<button_nav class="info compact" :class="{ 'disabled': !$root.$data.state.services.simulator.live || !contract.Path[0].Range || !contract.AircraftCompatible }" shape="forward"  @click.native="interactState($event, 'commit')">{{ contract.Invoices.TotalFees != 0 ? 'Pay $' + contract.Invoices.TotalFees.toLocaleString('en-gb') + ' &amp; ' : '' }}Begin</button_nav>
				</div>
			</div>
			<div class="footer" v-else>
				<div></div>
				<div class="helper_flex">
					<button_nav class="info compact" :class="{ 'disabled': !$root.$data.state.services.simulator.live || !contract.Path[0].Range || !contract.AircraftCompatible }" shape="forward"  @click.native="interactState($event, 'commit')">Begin</button_nav>
				</div>
			</div>
		</div>

		<!-- Paused -->
		<div class="state-control--paused" v-if="contract.State == 'Active' && !contract.IsMonitored && contract.LastLocationGeo && (contract.AircraftCompatible || !$root.$data.state.services.simulator.connected)">
			<div class="header">
				<div>
					<div class="title">Paused</div>
				</div>
				<div>
					<button_nav shape="forward" class="compact go" :class="{ 'disabled': (!CanResume || contract.RequestStatus != 'ready' || !$root.$data.state.services.simulator.live) && !$os.isDev }" @click.native="interactState($event, 'resume')">Resume</button_nav>
				</div>
			</div>
			<div>
				<p class="notice">To resume, go to the location circled on the map and press <em>Resume</em>.</p>
				<p class="notice">{{ 'Lat: ' + contract.LastLocationGeo[1] }}, {{ 'Lng: ' + contract.LastLocationGeo[0] }}</p>
			</div>
		</div>

		<!-- Specific Aircraft Required -->
		<div class="state-control--aircraft" v-if="(contract.State == 'Active' || contract.State == 'Saved') && contract.LastLocationGeo && (!contract.AircraftCompatible && $root.$data.state.services.simulator.connected)">
			<div class="header">
				<div>
					<div class="title">Requires the <strong>{{ template.AircraftRestrictionLabel }}</strong>.</div>
				</div>
				<div>
					<button_nav class="theme--dark compact cancel transparent outlined" :hold="true" :class="{ 'disabled': contract.RequestStatus != 'ready' }" @hold="interactState($event, 'cancel')">Cancel</button_nav>
				</div>
			</div>
			<div>
				<p class="notice">Load the aircraft in sim to resume.</p>
			</div>
		</div>

		<!-- Failed -->
		<div class="state-control--failed" v-if="contract.State == 'Failed'">
			<div class="header">
				<div>
					<div class="title">Failed</div>
				</div>
				<div></div>
			</div>
			<div>
				<p>{{ contract.EndSummary != null ? contract.EndSummary : "That's unfortunate..." }}</p>
			</div>
		</div>

	</div>
	<div v-else>
	</div>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';

export default Vue.extend({
	name: "contract_state",
	props: ['contract', 'template', 'type'],
	components: {
		Invoice: () => import("@/sys/components/invoices/invoice.vue"),
	},
	data() {
		return {
			CompletedAt: null,
			StartedAt: null,
			CanResume: false,
			Interactions: [],
		}
	},
	mounted() {

	},
	methods: {
		rangeCheck() {
			if(this.contract.State == 'Active' && this.contract.LastLocationGeo) {
				const simSvc = this.$root.$data.state.services.simulator;
				const dist = Eljs.GetDistance(simSvc.location.Lat, simSvc.location.Lon, this.contract.LastLocationGeo[1], this.contract.LastLocationGeo[0], "N");
				if(dist < 10) {
					this.CanResume = true;
				} else {
					this.CanResume = false;
				}
			}
		},
		interactionsCheck() {
			this.Interactions = [];
			this.contract.Path.forEach((node :any) => {
				node.Actions.forEach((action :any) => {
					const interactions = this.contract.Interactions.filter((x :any) => x.Essential && x.UID == action.UID);
					if(interactions.length) {
						this.Interactions.push({
							Label: action.Description,
							Actions: interactions
						})
					}
				});
			});
		},
		calculate() {
			this.CompletedAt = new Date(this.contract.CompletedAt);
			this.StartedAt = new Date(this.contract.StartedAt);
			this.rangeCheck();
			this.interactionsCheck();
		},
		getInvoice() {
			this.$ContractMutator.Interact(this.contract, "invoices", null, null);
		},
		viewQuote() {
			this.$os.modalPush({
				type: 'invoice',
				title: 'Start fees',
				actions: {
					yes: (this.contract.Path[0].Range && this.contract.AircraftCompatible) ? 'Pay and fly!' : null,
					no: 'Close'
				},
				data: {
					InvoiceData: this.contract.Invoices,
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
					this.$ContractMutator.Interact(this.contract, name, { ID: this.contract.ID }, (state) => {

					});
					break;
				}
				default: {
					this.$emit('interactState', ev, name);
					break;
				}
			}

		},
		interactAction(ev: Event, interaction: any) {
			this.$ContractMutator.Interact(this.contract, "interaction", {
					ID: this.contract.ID,
					Link: interaction.UID,
					Verb: interaction.Verb,
					Data: {},
				}, (md) => {
				if(md.Success) {
					this.calculate();
				}
			});
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
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
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
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';
.state-control {
	position: relative;
	margin-bottom: 14px;
	z-index: 2;

	.theme--bright &,
	&.theme--bright {
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
			color: $ui_colors_bright_shade_0;
			$clr: desaturate($ui_colors_bright_button_go, 30%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
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

	.theme--dark &,
	&.theme--dark {
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
			color: $ui_colors_dark_shade_5;
			$clr: desaturate($ui_colors_dark_button_go, 20%);
			background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
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

	&--small {

	}

	> div {
		position: relative;
		display: flex;
		flex-direction: column;
		padding: $edge-margin / 4 $edge-margin / 2 $edge-margin / 2 $edge-margin / 2 ;
		border-radius: 8px;
		& > div {
			margin-bottom: 8px;
			z-index: 2;
			&.footer {
				margin-bottom: 0;
				display: flex;
				justify-content: space-between;
			}
			&:last-child {
				margin-bottom: 0;
			}
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
}
</style>