<template>
	<div class="todolist">
		<div class="todolist-background" :style="state.ui.color ? 'background-color:' + (this.$root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : ''">
		</div>
		<div class="todolist-header">
			<div>
				<div class="todolist-title" v-if="template.Name.length">{{ template.Name }}</div>
				<div class="todolist-title" v-else>{{ contract.Route }}</div>
			</div>
			<div>
				<button_nav class="compact cancel" v-if="contract.State == 'Succeeded' || contract.State == 'Failed'" @click.native="close">Close</button_nav>
				<button_nav class="compact translucent" @click.native="contractDetail" v-if="contract.State == 'Saved' || contract.State == 'Active'">
					<div class="expires">
						<div v-if="template.RunningClock">Exp. <countdown :has_warn="true" :warn_for="new Date(contract.PullAt)" :time="new Date(contract.ExpireAt)" :only_hours="true" :short="true"></countdown></div>
						<div v-else-if="contract.State == 'Listed'">Time: <strong>{{ template.TimeToComplete > 0 ? template.TimeToComplete.toLocaleString('en-gb') + 'h' : '∞' }}</strong></div>
						<div v-else-if="template.TimeToComplete > 0">Rem.: <countdown :no_fix="true" :has_warn="true" :warn_for="new Date(contract.PullAt)" :time="new Date(contract.ExpireAt)" :full="false" :only_hours="true" :short="true"></countdown></div>
						<div v-else><strong>∞</strong> Time</div>
					</div>
				</button_nav>
				<button_nav shape="forward" class="info compact" @click.native="contractManage">Manage</button_nav>
			</div>
		</div>
		<div class="todolist-content" v-if="(contract.State == 'Active' || contract.State == 'Succeeded') && (contract.IsMonitored || contract.LastLocationGeo == null)">
			<scroll_view :scroller_offset="{ top: 0, bottom: 0 }" :dynamic="true">
				<div class="situations">
					<div class="situation-box"
						v-for="(situation, index) in contract.Situations"
						v-bind:key="index"
						:class="{
							'done': contract.Path[index].Done & contract.State == 'Active',
							'none': contract.Path[index].Count == 0,
							'range': contract.Path[index].Range,
							'next': contract.Path[index].IsNext,
							'visited': contract.Path[index].Visited,
							'forgot': contract.Path[index].Visited && !contract.Path[index].Done && !contract.Path[index].Range,
							'hidden': ((contract.Path[index].Done && !contract.Path[index].Range)) || ((!contract.Path[index].Done && !contract.Path[index].IsNext))
					}">
						<div class="list-item" :class="{ 'completed': ((contract.Path[index].Range && contract.Path[index].IsNext) || contract.Path[index].Done) }">
							<div class="list-item-state"></div>
							<div class="list-item-content">
								<div class="list-item-description" v-if="situation.Airport">Get to <strong>{{ situation.Airport.ICAO }}</strong> {{ situation.Airport.Name }}</div>
								<div class="list-item-description" v-else>Get to <strong>{{ situation.Location[1] }}</strong>, <strong>{{ situation.Location[0] }}</strong>{{ situation.Label.length ? ' (' + situation.Label + ')' : '' }}</div>
							</div>
							<div class="list-item-interaction"></div>
						</div>

						<div v-if="contract.Path[index].Range || (contract.Path[index].Visited && !contract.Path[index].Done)">
							<div class="list-item sub-items" v-for="(action, index) in contract.Path[index].Actions" v-bind:key="index" :class="{ 'completed': action.Completed }">
								<div class="list-item-state"></div>
								<div class="list-item-content">
									<div class="list-item-description">{{ action.Action + " " + action.Description }}</div>
									<div class="list-item-interactions" v-if="contract.IsMonitored">
										<button_action
										v-for="(interaction, index) in contract.Interactions.filter(x => x.UID == action.UID && x.Label != '' && x.Essential)"
										v-bind:key="index"
										@click.native="interactAction($event, interaction)"
										class="go" :class="{ 'disabled': !interaction.Enabled }">
											<span>{{ interaction.Label }}</span>
											<span v-if="interaction.Expire"><countdown :precise="true" :stop_zero="true" :time="new Date(interaction.Expire)"></countdown></span>
										</button_action>
									</div>
								</div>
							</div>
						</div>

					</div>
					<div class="situation-box" :class="{ 'hidden': contract.State != 'Succeeded' }">
						<div class="list-item final" :class="{ 'completed': contract.State == 'Succeeded' }">
							<div class="list-item-state"></div>
							<div class="list-item-content">
								<div class="list-item-description">Completed</div>
							</div>
							<div class="list-item-interaction"></div>
						</div>
					</div>
				</div>
			</scroll_view>
		</div>
		<div v-else>
			<ContractState type="small" :contract="contract" :template="template" @interactState="interactState"/>
		</div>
		<div class="todolist-footer" v-if="contract.Limits.length">
			<ContractLimits :contract="contract" :template="template" :essential="true"/>
		</div>
	</div>
</template>

<script lang="ts">
import ContractState from "@/sys/components/contracts/contract_state.vue"
import ContractLimits from "@/sys/components/contracts/contract_limits.vue"
import Vue from 'vue';

export default Vue.extend({
	name: "contract_todo",
	props: ['contract', 'template'],
	components: {
		ContractState,
		ContractLimits,
	},
	data() {
		return {
			state: {
				ui: {
					colorIsDark: false,
					color: null,
					colorDark: null,
				}
			}
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
		close() {
			this.$emit('close');
		},
		interactAction(ev: Event, interaction: any) {
			this.$emit('interactAction', ev, interaction);
		},
		contractManage() {
			this.$router.push({ name: 'p42_conduit', params: { contract: this.contract.ID }});
		},
		contractDetail() {
			this.$emit('contractDetail');
		},

		interactState(ev: Event, name: string) {
			this.$ContractMutator.Interact(this.contract, name, null, null);
			this.$emit('interactState', ev, name);
		},
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
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
.todolist {
	position: relative;
	border-radius: 8px;
	z-index: 5;
	overflow: hidden;

	.theme--bright &,
	&.theme--bright {
		color: $ui_colors_bright_shade_5;
		@include shadowed_shallow($ui_colors_bright_shade_5);
		.situation-box {

			// Next Situation
			&.next {

				& > .list-item {
					.list-item-state {
						background: $ui_colors_bright_button_info;
						border-color: transparent;
						&::before,
						&::after {
							background-color: $ui_colors_bright_shade_0;
						}
					}
				}
				.sub-items {
					.list-item-state {
						background: transparent;
					}
				}

				&.range {
					.sub-items {
						.list-item-state {
							background: $ui_colors_bright_button_info;
							border-color: transparent;
							&::before,
							&::after {
								background-color: $ui_colors_bright_shade_0;
							}
						}
					}
				}

			}

			// Forgot Situation
			&.forgot {
				& > .list-item {
					.list-item-state {
						background: $ui_colors_bright_button_warn;
						border-color: transparent;
						&::before,
						&::after {
							background-color: $ui_colors_bright_shade_5;
							mask: url(../../../../sys/assets/icons/state_mask_alert.svg);
						}
					}
				}
				.sub-items {
					.list-item-state {
						background: $ui_colors_bright_button_warn;
						border-color: transparent;
						&::before,
						&::after {
							background-color: $ui_colors_bright_shade_5;
							mask: url(../../../../sys/assets/icons/state_mask_alert.svg);
						}
					}
				}
			}

			.list-item {
				&-state {
					border-color: $ui_colors_bright_shade_5;
					&::before,
					&::after {
						background-color: $ui_colors_bright_shade_5;
					}
				}
				&-box {
					background: rgba($ui_colors_bright_shade_2, 0.3);
					border: 1px solid rgba($ui_colors_bright_shade_3, 0.3);
				}
				&.final {
					& > .list-item-state {
						background-color: darken($ui_colors_bright_button_go, 10%) ;
						&::before,
						&::after {
							background-color: $ui_colors_bright_shade_5;
						}
					}
				}
				&:after {
					border-left: 2px solid $ui_colors_bright_shade_5;
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		color: $ui_colors_dark_shade_5;
		@include shadowed_shallow($ui_colors_dark_shade_0);
		.situation-box {

			// Next Situation
			&.next {

				& > .list-item {
					.list-item-state {
						background: $ui_colors_dark_button_info;
						border-color: $ui_colors_dark_shade_5;
						&::before,
						&::after {
							background-color: $ui_colors_dark_shade_5;
						}
					}
				}
				.sub-items {
					.list-item {
						&-state {
							background: transparent;
						}
					}
				}

				&.range {
					.list-item {
						&.sub-items {
							.list-item {
								&-state {
									background: $ui_colors_dark_button_info;
									border-color: $ui_colors_dark_shade_5;
									&::before,
									&::after {
										background-color: $ui_colors_dark_shade_5;
									}
								}
							}
						}
					}
				}
			}

			// Forgot Situation
			&.forgot {
				& > .list-item {
					.list-item-state {
						background: $ui_colors_dark_button_warn;
						border-color: transparent;
						&::before,
						&::after {
							background-color: $ui_colors_dark_shade_0;
							mask: url(../../../../sys/assets/icons/state_mask_alert.svg);
						}
					}
				}
				.sub-items {
					.list-item-state {
						background: $ui_colors_dark_button_warn;
						border-color: transparent;
						&::before,
						&::after {
							background-color: $ui_colors_dark_shade_0;
							mask: url(../../../../sys/assets/icons/state_mask_alert.svg);
						}
					}
				}
			}

			.list-item {
				&-state {
					border-color: $ui_colors_dark_shade_5;
					&::before,
					&::after {
						background-color: $ui_colors_dark_shade_5;
					}
				}
				&-box {
					background: rgba($ui_colors_dark_shade_2, 0.3);
					border: 1px solid rgba($ui_colors_dark_shade_3, 0.3);
				}
				&.final {
					& > .list-item-state {
						background-color: $ui_colors_dark_button_go;
						&::before,
						&::after {
							background-color: $ui_colors_dark_shade_5;
						}
					}
				}
				&:after {
					border-left: 2px solid $ui_colors_dark_shade_5;
				}
			}
		}
	}

	&:hover {
		&.todolist {

		}
	}

	.situations {
		margin: 0 8px;
		margin-top: 4px;
	}

	.situation-box {
		&:last-of-type {
			.list-item {
				&.completed {
					&:last-of-type {
						&:after {
							display: none;
						}
					}
				}
			}
		}
		&.visited,
		&.range {
			.list-item {
				&-state {
					opacity: 1;
					&::before,
					&::after {
						opacity: 1;
					}
				}
			}
		}
		&.next {
			.list-item {
				&:first-of-type {
					.list-item-state {
						opacity: 1;
						&::before,
						&::after {
							opacity: 1;
						}
					}
				}
			}
		}
		&.hidden {
			display: none;
		}
		.list-item {
			position: relative;
			display: flex;
			flex-direction: row;
			align-items: center;
			margin-bottom: 8px;
			&.completed {
				& > .list-item-state {
					@keyframes list-item-state-bounce {
						0%   { transform: scale(1); }
						10%  { transform: scale(1.2); }
						100% { transform: scale(1); }
					}
					animation-name: list-item-state-bounce;
					animation-duration: 1s;
					&::before,
					&::after {
						opacity: 1;
					}
					&::before {
						transform: translateX(100%);
					}
					&::after {
						transform: translateX(0%);
					}
				}
				& > .list-item-description {
					font-family: "SkyOS-SemiBold";
				}
				& > .list-item-content {
					opacity: 0.5;
				}
				&:after {
					height: calc(100% - 26px);
				}
			}
			&:after {
				content: '';
				position: absolute;
				top: 26px;
				opacity: 0.5;
				margin-bottom: -100%;
				height: 0%;
				left: 9px;
				margin-left: -1px;
				transition: height 0.3s ease-out;
			}
			&-airport-name {
				margin-left: 8px;
			}
			&-state {
				position: relative;
				min-width: 16px;
				height: 16px;
				border-radius: 50%;
				margin-right: 8px;
				align-self: start;
				overflow: hidden;
				opacity: 0.3;
				border: 1px solid transparent;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
				animation-name: list-item-state-bounce-out;
				animation-duration: 1s;
				transition: opacity 0.4s ease-out;
				&::before,
				&::after {
					content: '';
					position: absolute;
					width: 100%;
					height: 100%;
					display: block;
					opacity: 0;
					transition: transform 0.4s ease-out;
				}
				&::before {
					mask: url(../../../../sys/assets/icons/state_mask_todo.svg);
				}
				&::after {
					mask: url(../../../../sys/assets/icons/state_mask_done.svg);
					transform: translateX(-100%);
				}
			}
			&-box {
				padding: 4px 8px;
				margin-bottom: 4px;
				border-radius: 8px;
				&:last-of-type {
					margin-bottom: 0;
				}
			}
			&-content {
				display: flex;
				padding-top: 0;
				padding-bottom: 0;
				align-self: center;
				align-items: center;
				flex-grow: 1;
				& > div:first-child {
					flex-grow: 1;
				}
			}
			&-details {
				padding-top: 4px;
				padding-bottom: 4px;
			}
			&-country {
				padding-top: 4px;
				padding-bottom: 4px;
			}
			&-runway-info {
				.elevation {
					margin-bottom: 4px;
					.value {
						font-family: "SkyOS-SemiBold";
					}
				}
				.runways {
					flex-grow: 1;
					.runway {
						display: flex;
						align-items: center;
						height: 1.7em;
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
					}
				}
			}
			&-interactions {
				display: flex;
				.button_action  {
					padding: 0px 8px
				}
			}
		}
	}

	&-background {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		opacity: 0.5;
		z-index: -1;
		transition: background 2s ease-out;
	}

	&-header {
		position: relative;
		display: flex;
		align-items: center;
		margin-bottom: 4px;
		z-index: 2;
		& > div {
			display: flex;
			&:first-child {
				flex-grow: 1;
			}
			&:last-child {
				.button_nav  {
					margin-left: 4px;
				}
			}
		}
	}

	&-title {
		font-family: "SkyOS-SemiBold";
		//margin-left: 8px;
	}

	&-content {
		.scroll_view {
			max-height: 95px;
			pointer-events: all;
			mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 0) 0, rgba(0, 0, 0, 1) 4px, rgba(0, 0, 0, 1) calc(100% - 8px), rgba(0, 0, 0, 0) 100%);
			margin: 0px -8px;
		}
	}

	&-footer {
		position: relative;
		display: flex;
		justify-content: space-between;
		margin-bottom: 8px;
		z-index: 2;
	}

	.state-control {
		margin-bottom: 8px;
	}

}
</style>