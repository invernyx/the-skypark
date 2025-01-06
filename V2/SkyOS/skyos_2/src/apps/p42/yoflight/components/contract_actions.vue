<template>
	<div class="state-control">

		<div class="state-control-background">
			<div :style="(state.ui.color ? 'background-color:' + (this.$root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : '')">
				<div :style="'background-image: url(' + contract.ImageURL + ');'"></div>
			</div>
		</div>

		<div class="state-control-content">
			<div class="state-control--ready" v-if="contract.State == 'Active' && contract.IsMonitored">
				<div>
					<div class="label" v-if="Interactions.length == 0">Ready to go!</div>
					<div class="label" v-else>To do</div>
					<button_nav class="cancel" :class="{ 'disabled': Requesting }" @click.native="interactState($event, 'pause')">Pause</button_nav>
					<button_nav shape="forward" @click.native="interactState($event, 'manage')">Manage</button_nav>
				</div>
				<div v-if="Interactions.length == 0">
					<p>All your papers are in order.</p>
					<p class="notice">Follow the plan below and complete the contract.</p>
				</div>
				<div class="interaction" v-for="(interaction, index) in Interactions" v-bind:key="index">
					<div class="interaction-label">{{ interaction.Label }}</div>
					<div class="interaction-actions">
						<button_action
							v-for="(action, index) in interaction.Actions.filter(x => x.Label != '')" v-bind:key="index"
							@click.native="interactAction($event, action)"
							class="go" :class="{ 'disabled': !action.Enabled }">
							<span>{{ action.Label }}</span>
							<span v-if="action.Expire"><countdown :precise="true" :stop_zero="true" :time="new Date(action.Expire)"></countdown></span>
						</button_action>
					</div>
				</div>
			</div>

			<div class="state-control--ready" v-if="contract.State == 'Active' && !contract.IsMonitored && !contract.LastLocationGeo">
				<div>
					<div class="label">Let's go!</div>
					<button_nav class="cancel" :hold="true" :class="{ 'disabled': Requesting }" @hold="interactState($event, 'cancel')">Cancel</button_nav>
					<button_nav shape="forward" @click.native="interactState($event, 'manage')">Manage</button_nav>
				</div>
				<div v-if="contract.AircraftCompatible">
					<p v-if="contract.Situations[0].Airport">Go to <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) to begin.</p>
					<p v-else>Go to {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} to begin.</p>
				</div>
				<div v-else>
					<p v-if="contract.Situations[0].Airport">Go to <strong>{{ contract.Situations[0].Airport.ICAO }}</strong> ({{ contract.Situations[0].Airport.Name }}) in the <strong>{{ template.AircraftRestrictionLabel }}</strong> to begin.</p>
					<p v-else>Go to {{ contract.Situations[0].Location[0] + ', ' + contract.Situations[0].Location[1] }} in the <strong>{{ template.AircraftRestrictionLabel }}</strong> to begin.</p>
				</div>
			</div>

			<div class="state-control--paused" v-if="contract.State == 'Active' && !contract.IsMonitored && contract.LastLocationGeo && (contract.AircraftCompatible || !$root.$data.state.services.simulator.connected)">
				<div>
					<div class="label">Paused</div>
					<button_nav @click.native="interactState($event, 'manage')">Manage</button_nav>
					<button_nav shape="forward" class="go" :class="{ 'disabled': (!CanResume || Requesting || !$root.$data.state.services.simulator.live) && !$os.isDev }" @click.native="interactState($event, 'resume')">Resume</button_nav>
				</div>
			</div>

			<div class="state-control--aircraft" v-if="(contract.State == 'Active' || contract.State == 'Saved') && contract.LastLocationGeo && (!contract.AircraftCompatible && $root.$data.state.services.simulator.connected)">
				<div>
					<div class="label">Specific aircraft required</div>
					<button_nav class="cancel" :hold="true" :class="{ 'disabled': Requesting }" @hold="interactState($event, 'cancel')">Cancel</button_nav>
				</div>
				<div>
					<p>Requires the <strong>{{ template.AircraftRestrictionLabel }}</strong>.</p>
					<p>Load the aircraft in sim to resume.</p>
				</div>
			</div>

			<div class="state-control--completed" v-if="contract.State == 'Succeeded'">
				<div>
					<div class="label">Completed in {{ moment(CompletedAt).from(StartedAt, true) }}</div>
				</div>
				<div>
					<p>{{ contract.EndSummary.length ? contract.EndSummary : 'Great job!' }}</p>
				</div>
			</div>

			<div class="state-control--failed" v-if="contract.State == 'Failed'">
				<div>
					<div class="label">Failed</div>
				</div>
				<div>
					<p>{{ contract.EndSummary != null ? contract.EndSummary : "That's unfortunate..." }}</p>
				</div>
			</div>
		</div>

	</div>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';

export default Vue.extend({
	name: "contract_actions",
	props: ['contract', 'template'],
	data() {
		return {
			state: {
				ui: {
					colorIsDark: false,
					color: null,
					colorDark: null,
				}
			},
			Requesting: false,
			CompletedAt: null,
			StartedAt: null,
			CanResume: false,
			Interactions: [],
		}
	},
	mounted() {
		this.init();
	},
	methods: {
		init() {
			if(this.contract.ImageURL.length) {
				this.$root.$data.services.colorSeek.find(this.contract.ImageURL, (color :any) => {
					this.state.ui.color = color.color;
					this.state.ui.colorDark = color.colorDark;
					this.state.ui.colorIsDark = color.colorIsDark;
				});
			} else {
				this.state.ui.colorIsDark = false;
				this.state.ui.color = null;
				this.state.ui.colorDark = null;
			}
		},

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
		interactState(ev: Event, name: string) {
			this.$ContractMutator.Interact(this.contract, name, null, null);
			this.$emit('interactState', ev, name);
		},
		interactAction(ev: Event, interaction: any) {
			this.$root.$data.services.api.SendWS(
				"adventure:interaction",
				{
					ID: this.contract.ID,
					Link: interaction.UID,
					Verb: interaction.Verb,
					Data: {},
				}
			);
		},
		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'adventure': {
					this.calculate();
					this.Requesting = false;
					break;
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
		'$root.$data.state.services.simulator.location.Lat': {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.rangeCheck();
				}
			}
		},
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
					this.calculate();
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';
.state-control {
	position: relative;
	margin-bottom: 14px;
	z-index: 2;

	.theme--bright &,
	&.theme--bright {
		> div {
			.interaction {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
		}
		&--ready {
			//color: $ui_colors_bright_shade_0;
			//$clr: desaturate($ui_colors_bright_button_go, 30%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--paused {
			//color: $ui_colors_bright_shade_0;
			//$clr: desaturate($ui_colors_bright_button_cancel, 60%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--aircraft {
			//color: $ui_colors_bright_shade_0;
			//$clr: desaturate($ui_colors_bright_button_warn, 60%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--completed {
			//color: $ui_colors_bright_shade_0;
			//$clr: desaturate($ui_colors_bright_button_info, 30%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--failed {
			//color: $ui_colors_bright_shade_0;
			//$clr: $ui_colors_bright_shade_4;
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
	}

	.theme--dark &,
	&.theme--dark {
		> div {
			.interaction {
				background-color: rgba($ui_colors_dark_shade_2, 0.4);
			}
		}
		&--ready {
			//color: $ui_colors_dark_shade_5;
			//$clr: desaturate($ui_colors_dark_button_go, 20%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--paused {
			//color: $ui_colors_dark_shade_5;
			//$clr: desaturate($ui_colors_dark_button_cancel, 60%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--aircraft {
			//color: $ui_colors_dark_shade_5;
			//$clr: desaturate($ui_colors_dark_button_warn, 60%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--completed {
			//color: $ui_colors_dark_shade_5;
			//$clr: desaturate($ui_colors_dark_button_info, 30%);
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
		&--failed {
			//color: $ui_colors_dark_shade_5;
			//$clr: $ui_colors_dark_shade_1;
			//background: linear-gradient(to bottom, $clr, cubic-bezier(.2,0,.4,1), darken($clr, 5%));
		}
	}

	&-background {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		mask-image: radial-gradient(circle 400px at 50px -20px, rgba(0,0,0,1) 60%, rgba(0,0,0,0.8) 75%, rgba(0,0,0,0) 100%);
		backdrop-filter: blur(10px);
		& > div {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			opacity: 0.8;
			margin-top: -50px;
			margin-left: -30px;
			mask-image: radial-gradient(circle 400px at 50px -20px, rgba(0,0,0,1) 10%, rgba(0,0,0,0.9) 50%, rgba(0,0,0,0.7) 70%, rgba(0,0,0,0) 100%);
			//mask-image: linear-gradient(to bottom, rgba(0,0,0,1) 70%, rgba(0,0,0,0.7) 80%, rgba(0,0,0,0) 100%);
			& > div {
				position: absolute;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				opacity: 0.9;
				background-position: center;
				background-repeat: no-repeat;
				background-size: cover;
				transition: background 0.3s ease-out;
				filter: blur(20px);
			}
		}
	}

	&-content > div {
		position: relative;
		display: flex;
		flex-direction: column;
		padding-left: $edge-margin;
		padding-right: 100px;
		padding-top: $status-size;
		& > div {
			margin-bottom: 8px;
			z-index: 2;
			&:first-child {
				margin-bottom: 8px;
				display: flex;
				align-content: center;
				justify-content: flex-start;
			}
		}
		.label {
			font-size: 1.4em;
			font-family: "SkyOS-SemiBold";
			align-self: center;
			margin-right: $edge-margin;
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
}
</style>