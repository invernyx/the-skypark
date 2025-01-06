<template>
	<div class="todolist">
		<div class="situation-box"
		v-for="(situation, index) in contract.Situations"
		v-bind:key="index"
		:class="{
			'done': contract.Path[index].Done && contract.State == 'Active',
			'none': contract.Path[index].Count == 0,
			'range': contract.Path[index].Range,
			'next': contract.Path[index].IsNext,
			'visited': contract.Path[index].Visited,
			'forgot': contract.Path[index].Visited && !contract.Path[index].Done && !contract.Path[index].Range,
			'hidden': exclude ?
			   (exclude.includes('done') && (contract.Path[index].Done && !contract.Path[index].Range))
			|| (exclude.includes('future') && (!contract.Path[index].Done && !contract.Path[index].IsNext))
			: false
		}">

			<div class="list-item" :class="{ 'completed': ((contract.Path[index].Range && contract.Path[index].IsNext) || contract.Path[index].Done) }">
				<div class="list-item-state"></div>
				<div class="list-item-content" v-if="situation.Airport">
					<collapser :state="situation.Airport.Runways.length && (contract.Path[index].IsNext || contract.Path[index].Range || contract.State == 'Listed')" :preload="true">
						<template v-slot:title>
							<div class="list-item-background-map" v-if="situation.Airport">
								<div class="list-item-background-map-image" :style="'background-image:url(' + $os.getCDN('images', 'airports/' + situation.Airport.ICAO + '.jpg') + ')'"></div>
								<div class="list-item-background-map-overlay">
									<div>
										<span v-if="!contract.Path[index].Done">{{ index > 0 ? contract.Situations[index - 1].DistToNext > 1 ? contract.Situations[index - 1].DistToNext.toLocaleString('en-gb') + '&thinsp;nm' : "Come back" : "Get" }} to </span><strong>{{ situation.Airport.ICAO }}</strong>
									</div>
									<div>
										<flags class="location-info-flag" :code="situation.Airport.Country.toLowerCase()" /> {{ situation.Airport.CountryName }}
									</div>
								</div>
								<div class="list-item-background-map-actions" v-if="contract.Path[index].Visited || contract.Path[index].IsNext || (index == 0 && $root.$data.state.services.simulator.live )">
									<button_nav icon="theme--dark/warn" shape="forward" class="cancel compact" @click.native="airportReport($event, situation.Airport)"></button_nav>
								</div>
							</div>
							<div class="list-item-description" v-else>{{ index > 0 ? contract.Situations[index - 1].DistToNext.toLocaleString('en-gb') : "Get" }} to <strong>{{ situation.Location[0] + ', ' + situation.Location[1] }}</strong></div>
						</template>
						<template v-slot:content>
							<div class="list-item-details" v-if="situation.Airport">

								<div class="list-item-country list-item-box">
									<span><strong>{{ situation.Airport.Name }}</strong></span><br>
									<span v-if="situation.Airport.City.length">{{ situation.Airport.City }}{{ situation.Airport.State.length ? ',&nbsp;' : ''}}</span>
									<span v-if="situation.Airport.State.length">{{ situation.Airport.State }}</span>
								</div>

								<div class="list-item-runway-info list-item-box">
									<div class="header">
										<div class="elevation"><span class="label">Elevation</span> <span class="value">{{ situation.Airport.Elevation.toLocaleString('en-gb') }}′</span></div>
										<div class="features">
											<div class="lit" v-if="situation.Airport.Runways.filter(x => x.Lit ).length">Lit</div>
											<div class="unlit" v-else>Unlit</div>
											<div class="ils" v-if="situation.Airport.Runways.filter(x => x.PrimaryILS || x.SecondaryILS ).length">ILS</div>
										</div>
									</div>
									<div class="runways">
										<div class="runway" :class="{'hasLights': runway.Lit}" v-for="(runway, index) in situation.Airport.Runways" v-bind:key="index">
											<span class="badge">
												<icons class="icon icon--dark icon-runway">
													<span :style="'transform: rotate(' + runway.Heading + 'deg);'"></span>
												</icons>
											</span>
											<span class="value"><span class="name" :class="{'hasILS': runway.PrimaryILS }">{{ runway.PrimaryName }}</span>-<span class="name" :class="{'hasILS': runway.SecondaryILS }">{{ runway.SecondaryName }}</span>- {{ runway.LengthFT.toLocaleString('en-gb') }}′ - {{ runway.Surface }}</span>
										</div>
									</div>
								</div>
								<!--
								<div class="list-item-wx-info list-item-box">
									<weather class="compact" :include="['winds','visibility','altimeter']" :wx="situation.Airport.Weather"/>
								</div>
								-->

							</div>
						</template>
					</collapser>
				</div>
				<div class="list-item-content" v-else>
					<div class="list-item-description">{{ index > 0 ? contract.Situations[index - 1].DistToNext > 1 ? contract.Situations[index - 1].DistToNext.toLocaleString('en-gb') + '&thinsp;nm' : "Come back" : "Get" }} to {{ situation.Label.length ? situation.Label : 'location' }}</div>
					<div class="list-item-location"><strong>{{ situation.Location[1] }}</strong>, <strong>{{ situation.Location[0] }}</strong></div>
					<!--<div class="list-item-location" v-if="situation.Height > 0">Below <strong>{{ situation.Height }}</strong>ft AGL</div>-->
				</div>
				<div class="list-item-interaction"></div>
			</div>

			<div v-if="exclude ? contract.Path[index].Range : true">
				<div class="list-item sub-items" v-for="(action, index) in contract.Path[index].Actions" v-bind:key="index" :class="{ 'completed': action.Completed }">
					<div class="list-item-state"></div>
					<div class="list-item-content">
						<div class="list-item-description">{{ action.Action + " " + action.Description }}</div>
						<div class="list-item-interactions" v-if="contract.IsMonitored">
							<button_action
							v-for="(interaction, index) in contract.Interactions.filter(x => x.UID == action.UID && x.Label != '')"
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

		<div class="situation-box" :class="{ 'hidden': exclude ? (exclude.includes('future') && contract.State != 'Succeeded') : false }">
			<div class="list-item final" :class="{ 'completed': contract.State == 'Succeeded' }">
				<div class="list-item-state"></div>
				<div class="list-item-content">
					<div class="list-item-description">Completed</div>
				</div>
				<div class="list-item-interaction"></div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "contract_todo",
	props: ['contract', 'template', 'exclude'],
	methods: {
		interactAction(ev: Event, interaction: any) {
			this.$emit('interactAction', ev, interaction);
		},
		airportReport(ev: Event, airport :any) {
			ev.stopPropagation();
			this.$os.modalPush({
				type: 'report_airport',
				title: 'Report airport',
				data: {
					App: this.$os.activeApp,
					Airport: airport
				}
			});
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';
.todolist {
	.theme--bright &,
	&.theme--bright {
		color: $ui_colors_bright_shade_5;
		.situation-box {

			// Next Situation
			&.next {

				& > .list-item {
					.list-item-state {
						background: $ui_colors_bright_button_info;
						&::before,
						&::after {
							background-color: $ui_colors_bright_shade_5;
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
							&::before,
							&::after {
								background-color: $ui_colors_bright_shade_5;
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
						//border-color: transparent;
						&::before,
						&::after {
							background-color: $ui_colors_bright_shade_5;
							mask: url(../../../sys/assets/icons/state_mask_alert.svg);
						}
					}
				}
				.sub-items {
					.list-item-state {
						background: $ui_colors_bright_button_warn;
						//border-color: transparent;
						&::before,
						&::after {
							background-color: $ui_colors_bright_shade_5;
							mask: url(../../../sys/assets/icons/state_mask_alert.svg);
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
				&-runway-info {
					.ils {
						border-color: $ui_colors_bright_button_go;
					}
					.runways {
						.runway {
							.value {
								.hasILS {
									border-color: $ui_colors_bright_button_go;
								}
							}
						}
					}
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
		.situation-box {

			// Next Situation
			&.next {

				& > .list-item {
					.list-item-state {
						background: $ui_colors_dark_button_info;
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
							mask: url(../../../sys/assets/icons/state_mask_alert.svg);
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
							mask: url(../../../sys/assets/icons/state_mask_alert.svg);
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
				&-runway-info {
					.ils {
						border-color: $ui_colors_dark_button_go;
					}
					.runways {
						.runway {
							.value {
								.hasILS {
									border-color: $ui_colors_dark_button_go;
								}
							}
						}
					}
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

	&.no-actions {
		.situation-box {
			.list-item {
				margin-bottom: 16px;
				&-state {
					margin-left: 4px;
					min-width: 16px;
					height: 16px;
				}
				&-content {
					padding-top: 0;
					padding-bottom: 0;
				}
				&.completed {
					&:after {
						height: calc(100% - 10px);
						top: 22px;
					}
				}
			}
		}
	}

	position: relative;
	z-index: 5;
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
			height: 0;
			opacity: 0;
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
					//border: 1px solid transparent;
					opacity: 1;
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
				& .list-item-background-map {
					height: 65px;
					&-image {
						opacity: 0.4;
					}
				}
				.collapser {
					&.collapser_expanded {
						& .list-item-background-map {
							height: 200px;
							&-image {
								opacity: 1;
								transition: opacity 0.6s cubic-bezier(.25,0,.14,1);
							}
						}
					}
				}
				&:after {
					height: calc(100% - 26px);
				}
			}
			&:after {
				content: '';
				position: absolute;
				top: 30px;
				opacity: 0.5;
				margin-bottom: -100%;
				height: calc(0% - 20px);
				left: 13px;
				margin-left: -1px;
				transition: height 0.3s ease-out;
			}
			&-background-map {
				position: relative;
				height: 200px;
				border-radius: 8px;
				margin-top: -4px;
				color: #FFF;
				box-sizing: border-box;
				background-color: rgba(#000000, 0.2);
				transition: height 0.6s cubic-bezier(.25,0,.14,1);
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
				&-actions {
					position: absolute;
					top: 4px;
					right: 4px;
					.button_nav  {
						border-radius: 4px;
						font-size: 15px;
					}
				}
			}

			&-state {
				position: relative;
				min-width: 24px;
				height: 24px;
				border-radius: 50%;
				margin-right: 10px;
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
					mask: url(../../../sys/assets/icons/state_mask_todo.svg);
				}
				&::after {
					mask: url(../../../sys/assets/icons/state_mask_done.svg);
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
				position: relative;
				padding-top: 4px;
				padding-bottom: 4px;
				align-self: center;
				flex-grow: 1;
			}
			&-details {
				//margin-left: 8px;
				padding-top: 4px;
				padding-bottom: 4px;
			}
			&-country {
				padding-top: 4px;
				padding-bottom: 4px;
			}
			&-location {
				font-size: 12px;
			}
			&-runway-info {
				.header {
					display: flex;
					justify-content: space-between;
					align-items: center;
					.features {
						display: flex;
						& > div {
							margin-left: 0.5em;
						}
					}
				}
				.lit {
					display: flex;
					align-items: center;
					position: relative;
					font-family: "SkyOS-SemiBold";
					&::after {
						display: block;
						content: '';
						width: 0.5em;
						height: 0.5em;
						margin-left: 0.1em;
						background: #FFF;
						border: 1px solid #000;
						border-radius: 50%;
						z-index: 2;
					}
				}
				.unlit {
					display: flex;
					align-items: center;
					position: relative;
					font-family: "SkyOS-SemiBold";
					&::after {
						display: block;
						content: '';
						width: 0.5em;
						height: 0.5em;
						margin-left: 0.1em;
						background: #000;
						border: 1px solid #000;
						border-radius: 50%;
						z-index: 2;
					}
				}
				.ils {
					padding: 0 0.1em;
					margin: 0 0.1em;
					border-radius: 0.4em;
					font-family: "SkyOS-SemiBold";
					border: 2px solid transparent;
				}
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
						height: 25px;
						&.hasLights {
							.badge {
								&::after {
									position: absolute;
									display: block;
									content: '';
									top: 0;
									right: 0;
									width: 0.5em;
									height: 0.5em;
									margin-right: -0.2em;
									margin-top: -0.2em;
									background: #FFF;
									border: 1px solid #000;
									border-radius: 50%;
									z-index: 2;
								}
							}
						}
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
						.value {
							.name {
								display: inline-block;
								padding: 0 0.1em;
								margin: 0;
								border-radius: 0.4em;
								font-family: "SkyOS-SemiBold";
								border: 2px solid transparent;
								&:first-child {
									margin-left: 0;
								}
							}
						}
					}
				}
			}
			&-interactions {
				display: flex;
				.button_action  {
					padding: 2px 8px
				}
			}
		}
	}

}
</style>