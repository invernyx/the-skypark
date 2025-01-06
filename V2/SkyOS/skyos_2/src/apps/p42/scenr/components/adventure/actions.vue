<template>
	<div class="adventure_actions" :class="styleStr">
		<!-- Add new Action Dropdown -->
		<selector class="compact" :placeholder="placeholder" v-model="ActionDropdown" @input="AddAction">
			<optgroup v-for="ActionCat in AvailableActions" v-bind:key="ActionCat.title" :label="ActionCat.title">
				<option v-for="ActionCode in Object.keys(ActionCat.actions).filter(x => !Exclusions.includes(x))" v-bind:key="ActionCode" :value="ActionCode">{{ ActionCat.actions[ActionCode] }}</option>
			</optgroup>
		</selector>
		<!-- Loop the current Situation's Actions -->
		<div class="adventure_action" v-for="Action in Actions" v-bind:key="Action.UID" :class="'adventure_action_style_' + Action.Action">

			<!-- Basic Controls -->
			<div class="columns adventure_action_controls">
				<div class="column">
					<strong>{{ Action.Action | GetActionName(AvailableActions) }}</strong>
				</div>
				<div class="column column_narrow">
					<!--
					<button_action type="is-dark">
						<span>⮝</span>
					</button_action>&nbsp;
					<button_action type="is-dark">
						<span>⮟</span>
					</button_action>&nbsp;
					-->
					<button_action class="cancel" @click.native="RemoveAction(Action)">X</button_action>
				</div>
			</div>

			<!-- Audio Speech Actions -->
			<div v-if="Action.Action == 'audio_speech_play'">
				<AdvAction_AudioSpeech :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_AudioSpeech>
			</div>

			<!-- Audio Effect Actions -->
			<div v-if="Action.Action == 'audio_effect_play'">
				<AdvAction_AudioEffect :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_AudioEffect>
			</div>

			<!-- Cargo Actions -->
			<div v-if="Action.Action == 'cargo_pickup' || Action.Action == 'cargo_dropoff'">
				<AdvAction_Cargo :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_Cargo>
			</div>

			<!-- Cargo v2 Actions -->
			<div v-if="Action.Action == 'cargo_pickup_2' || Action.Action == 'cargo_dropoff_2'">
				<AdvAction_Cargo_2 :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_Cargo_2>
			</div>

			<!-- G-Force Actions -->
			<div v-if="Action.Action == 'trigger_g_start' || Action.Action == 'trigger_g_end'">
				<AdvAction_LimitG :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_LimitG>
			</div>

			<!-- Alt Actions -->
			<div v-if="Action.Action == 'trigger_alt_start' || Action.Action == 'trigger_alt_end'">
				<AdvAction_LimitAlt :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_LimitAlt>
			</div>

			<!-- Timer Actions -->
			<div v-if="Action.Action == 'flight_timer_start' || Action.Action == 'flight_timer_end'">
				<AdvAction_FlightTimer :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_FlightTimer>
			</div>

			<!-- IAS Actions -->
			<div v-if="Action.Action == 'trigger_ias_start' || Action.Action == 'trigger_ias_end'">
				<AdvAction_LimitIAS :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_LimitIAS>
			</div>

			<!-- IAS Actions -->
			<div v-if="Action.Action == 'trigger_gs_start' || Action.Action == 'trigger_gs_end'">
				<AdvAction_LimitGS :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_LimitGS>
			</div>

			<!-- Distance -->
			<div v-if="Action.Action == 'trigger_distance'">
				<AdvAction_Distance :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_Distance>
			</div>

			<!-- Engine Stop -->
			<div v-if="Action.Action == 'trigger_engine_stop'">
				<AdvAction_EngineStop :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_EngineStop>
			</div>

			<!-- Trigger Button -->
			<div v-if="Action.Action == 'trigger_button'">
				<AdvAction_Button :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_Button>
			</div>

			<!-- Adventure Milestone -->
			<div v-if="Action.Action == 'adventure_milestone'">
				<AdvAction_AdventureMilestone :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_AdventureMilestone>
			</div>

			<!-- Adventure Fail -->
			<div v-if="Action.Action == 'adventure_fail'">
				<AdvAction_AdventureFail :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_AdventureFail>
			</div>



			<!-- Scene -->
			<!--
			<div v-if="Action.Action == 'scene_load' || Action.Action == 'scene_unload'">
				<AdvAction_Scene :app="app" :Actions="AvailableActions" :Action="Action" :Situation="Situation" :Adventure="Adventure"></AdvAction_Scene>
			</div>
			-->
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

import AdventureProjectActionInterface from './../../interfaces/adventure/action';
//import ScenrProject from './../../../components/classes/scenes/project';

export default Vue.extend({
	name: 'scenr_adventure_action',
	props: ['app', 'Exclusions', 'placeholder', 'styleStr', 'SituationActions', 'Situation', 'Adventure'],
	components: {
		AdvAction_AudioSpeech: () => import('./actions/audio_speech.vue'),
		AdvAction_AudioEffect: () => import('./actions/audio_effect.vue'),
		AdvAction_Cargo: () => import('./actions/cargo.vue'),
		AdvAction_Cargo_2: () => import('./actions/cargo_2.vue'),
		AdvAction_FlightTimer: () => import('./actions/flight_timer.vue'),
		AdvAction_EngineStop: () => import('./actions/trigger_engine_stop.vue'),
		AdvAction_Distance: () => import('./actions/trigger_distance.vue'),
		AdvAction_Button: () => import('./actions/trigger_button.vue'),
		AdvAction_LimitG: () => import('./actions/limit_g.vue'),
		AdvAction_LimitAlt: () => import('./actions/limit_alt.vue'),
		AdvAction_LimitIAS: () => import('./actions/limit_ias.vue'),
		AdvAction_LimitGS: () => import('./actions/limit_gs.vue'),
		AdvAction_AdventureMilestone: () => import('./actions/adventure_milestone.vue'),
		AdvAction_AdventureFail: () => import('./actions/adventure_fail.vue'),
		AdvAction_Scene: () => import('./actions/scene.vue')
	},
	data() {
		return {
			ActionDropdown: null,
			AvailableActions: [
				{
					title: 'Adventure',
					actions: {
						//adventure_succeed: '💰 Succeed & end contract',
						adventure_milestone: '💰 Milestone',
						adventure_fail: '💣 Fail & end contract'
						//adventure_cleanup: "🧹 Cleanup"
					}
				},
				{
					title: 'Cargo',
					actions: {
						cargo_pickup_2: '🔼 Cargo Pickup v2',
						cargo_dropoff_2: '🔽 Cargo Dropoff v2',
						cargo_pickup: '🔼 Cargo Pickup v1',
						cargo_dropoff: '🔽 Cargo Dropoff v1'
						//cargo_eject: "⏏️ Cargo Eject",
						//cargo_damage: "🈶 Cargo Damage"
					}
				},
				{
					title: 'Passengers',
					actions: {
						//pax_pickup: "🙂 Pax Pickup",
						//pax_dropoff: "🙃 Pax Dropoff",
						//pax_satisfaction: "😀 Pax Satisfaction",
						//pax_dissatisfaction: "😠 Pax Dissatisfaction"
					}
				},
				{
					title: 'Audio',
					actions: {
						audio_speech_play: '📼 Play Speech',
						audio_effect_play: '📼 Play Effect'
					}
				},
				{
					title: 'Weather',
					actions: {}
				},
				{
					title: 'Instructions',
					actions: {
						//instructions_show: "📟 Show Instructions",
						//instructions_hide: "📟 Hide Instructions"
					}
				},
				//{
				//	title: 'Scene',
				//	actions: {
				//		//scene_create: "🏗️ Scene Create",
				//		scene_load: '🗿 Scene Load',
				//		scene_unload: '⛏️ Scene Unload'
				//	}
				//},
				{
					title: 'Triggers',
					actions: {
						trigger_button: '🖱️ Button Trigger',
						trigger_distance: '📏 Distance',
						trigger_engine_stop: '🚂 Engine Stop',
						//trigger_cruise: "🍿 Cruise Trigger",
						flight_timer_start: '⏳ Start Time-in-Flight Timer',
						flight_timer_end: '⏳ End Time-in-Flight Timer',
						trigger_g_start: '⏲ Start G Trigger',
						trigger_g_end: '⏲ End G Trigger',
						trigger_alt_start: '🏔 Start Alt Trigger',
						trigger_alt_end: '🏔 End Alt Trigger',
						trigger_ias_start: '🚅 Start IAS Trigger',
						trigger_ias_end: '🚅 End IAS Trigger',
						trigger_gs_start: '🚅 Start GS Trigger',
						trigger_gs_end: '🚅 End GS Trigger',
						//trigger_oat_start: "🟢 Start OAT Trigger",
						//trigger_oat_end: "🟥 End OAT Trigger",
						//trigger_time_start: "🟢 Start Time Trigger",
						//trigger_time_end: "🟥 End Time Trigger",
					}
				},
				{
					id: 'existing',
					title: 'Existing',
					actions: {}
				}
				//{
				//	title: "Vehicles",
				//	actions: {}
				//},
			],
			Actions: [] as AdventureProjectActionInterface[]
		};
	},
	mounted() {
		this.SituationActions.forEach((a: number) => {
			this.Actions.push(this.Adventure.Actions.find((x: AdventureProjectActionInterface) => x.UID === a));
		});

		var target = this.AvailableActions.find(x => x.id == 'existing') as any;
		target.actions = {};
		this.Adventure.Actions.forEach(action => {
			target.actions[action.UID.toString()] = action.Action + " - " + action.UID.toString();
		});
	},
	methods: {
		AddAction(e: any) {
			setTimeout(() => {
				this.ActionDropdown = null;
			}, 100);

			let is_new = true;
			const GUID = Eljs.getNumGUID();
			const NewAction = (() => {
				switch (e) {
					case 'cargo_pickup': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Model: 'any',
								Transport: 'Veh_Car_Mini4_White_sm',
								LoadedActions: [] as AdventureProjectActionInterface[],
								ForgotActions: [] as AdventureProjectActionInterface[],
								EndActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'cargo_dropoff': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Link: null as number,
								Transport: 'Veh_Car_Mini4_White_sm',
								UnloadedActions: [] as AdventureProjectActionInterface[],
								ForgotActions: [] as AdventureProjectActionInterface[],
								EndActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'cargo_pickup_2': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Manifests: [],
								//Transport: 'Veh_Car_Mini4_White_sm',
								//Models: [
								//	{
								//		Tag: 'any',
								//		MinWeight: 0,
								//		MaxWeight: 9999,
								//	}
								//],
								LoadedActions: [] as AdventureProjectActionInterface[],
								ForgotActions: [] as AdventureProjectActionInterface[],
								EndActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'cargo_dropoff_2': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Manifests: [],
								//Links: {},
								//Transport: 'Veh_Car_Mini4_White_sm',
								UnloadedActions: [] as AdventureProjectActionInterface[],
								ForgotActions: [] as AdventureProjectActionInterface[],
								EndActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'scene_load': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Scene: null as any,
								EnterActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'scene_unload': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Link: null as number,
								EnterActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_g_start': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Label: "",
								Min: -1,
								Max: 4,
								EnterActions: [] as AdventureProjectActionInterface[],
								ExitActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_g_end': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Link: null as number,
								SuccessActions: [] as AdventureProjectActionInterface[],
								FailActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_alt_start': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Label: "",
								Min: 0,
								Max: 45000,
								Relation: 'AGL',
								EnterActions: [] as AdventureProjectActionInterface[],
								ExitActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_alt_end': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Link: null as number,
								CancelRange: 15,
								SuccessActions: [] as AdventureProjectActionInterface[],
								FailActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_ias_start': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Label: "",
								Min: 0,
								Max: 600,
								EnterActions: [] as AdventureProjectActionInterface[],
								ExitActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_ias_end': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Link: null as number,
								CancelRange: 15,
								SuccessActions: [] as AdventureProjectActionInterface[],
								FailActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_gs_start': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Label: "",
								Min: 0,
								Max: 600,
								EnterActions: [] as AdventureProjectActionInterface[],
								ExitActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_gs_end': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Link: null as number,
								CancelRange: 15,
								SuccessActions: [] as AdventureProjectActionInterface[],
								FailActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					case 'trigger_engine_stop': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								EnterActions: [] as AdventureProjectActionInterface[],
							}
						};
					}
					case 'trigger_distance': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Distance: 2,
								EnterActions: [] as AdventureProjectActionInterface[],
								ExitActions: [] as AdventureProjectActionInterface[],
							}
						};
					}
					case 'trigger_button': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Label: '',
								Button: '',
								EnterActions: [] as AdventureProjectActionInterface[],
							}
						};
					}
					case 'audio_speech_play': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Delay: 0,
								Type: 'adventures',
								Name: '',
								NameID: '',
								URL: '',
								Transcript: '',
								Path: null as string,
								DonePlayingActions: [] as AdventureProjectActionInterface[],
							}
						};
					}
					case 'audio_effect_play': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Delay: 0,
								Path: null as string,
							}
						};
					}
					case 'flight_timer_start': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Label: "",
								MinDuration: 0,
								MaxDuration: 15,
								LandedInActions: [] as AdventureProjectActionInterface[],
								LandedOutActions: [] as AdventureProjectActionInterface[],
								ExceedMinActions: [] as AdventureProjectActionInterface[],
								ExceedMaxActions: [] as AdventureProjectActionInterface[],
							}
						};
					}
					case 'flight_timer_end': {
						return {
							UID: GUID,
							Action: e,
							Params: {}
						};
					}
					case 'adventure_fail': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Reason: ""
							}
						};
					}
					case 'adventure_milestone': {
						return {
							UID: GUID,
							Action: e,
							Params: {
								Label: "",
								SuccessActions: [] as AdventureProjectActionInterface[]
							}
						};
					}
					default: {
						try {
							var testInt = parseInt(e);
							is_new = false;

							const exising = this.Adventure.Actions.find(x => x.UID == testInt);
							return exising;

						} catch
						{
							return {
								UID: GUID,
								Action: e,
								Params: {}
							};
						}
					}
				}
			})();

			this.SituationActions.push(NewAction.UID);
			this.Actions.push(NewAction);

			if(is_new) {
				this.Adventure.Actions.push(NewAction);
			}

			this.app.$emit('action_added', NewAction);
		},
		RemoveAction(acn: AdventureProjectActionInterface) {

			// Remove Childs
			const loopChild = (act) => {
				Object.keys(act.Params).forEach((k :string, i :number) => {
					if(k.toLocaleLowerCase().endsWith('actions')) {
						act.Params[k].forEach((A3: any) => {
							loopChild(this.Adventure.Actions.find(x => x.UID == A3));
							const index = this.SituationActions.findIndex((x: number) => x === A3);
							this.SituationActions.splice(index, 1);
							this.Actions.splice(index, 1);
							this.Adventure.Actions.splice(this.Adventure.Actions.findIndex((x: AdventureProjectActionInterface) => x.UID === A3), 1);
						});
					}
				});
			}

			// Remove from Parent
			const index = this.SituationActions.findIndex((x: number) => x === acn.UID);
			this.SituationActions.splice(index, 1);
			this.Actions.splice(index, 1);
			this.Adventure.Actions.splice(this.Adventure.Actions.findIndex((x: AdventureProjectActionInterface) => x.UID === acn.UID), 1);
			loopChild(acn);

			//this.Actions.forEach(act => {
			//	loopChild(act);
			//});


			this.app.$emit('action_removed', acn);
		}
	},
	filters: {
		GetActionName(text: string, actions: any) {
			let val = '';
			actions.forEach((ac: any) => {
				if (Object.keys(ac.actions).includes(text)) {
					val = (ac.actions as any)[text];
				}
			});
			return val;
		}
	}
});
</script>

<style lang="scss" >
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.adventure_actions {
	padding: 2px;
	background: rgba(#000000,0.3);
	border-radius: 10px;
	margin-bottom: 8px;

	&:last-child {
		margin-bottom: 0px;
	}

	.theme--dark &,
	.theme--bright & {
		.textbox {
			color: #FFF;
			.placeholder {
				color: #FFF !important;
			}
			input:disabled {
				color: #FFF;
			}
			.border {
				background: transparent;
				border-color: rgba(255,255,255,0.1);
			}
			input:disabled,
			textarea,
			input {
				color: #FFF;
				background-color: rgba(255,255,255,0.1);
				&:hover {
					background-color: rgba(255,255,255,0.1);
				}
			}
		}
		.toggle {
			color: #FFF;
		}
		.button_listed {
			color: #FFF;
			background-color: rgba(255,255,255,0.1);
			font-family: inherit;
		}
		.selector {
			color: #FFF;
			.placeholder {
				opacity: 0.4;
			}
			select {
				color: #FFF;
				background-color: rgba(255,255,255,0.1);
				option,
				optgroup {
					background: #000;
				}
			}
		}
		& > .selector {
			.placeholder {
				opacity: 0.8;
			}
		}
	}

}

.adventure_action {
	background: darken($ui_colors_bright_button_info, 50%);
	border-radius: 8px;
	margin-top: 4px;
	padding: 4px;
	box-shadow: 0 3px 8px rgba(#000, 0.8);
	&_controls {
		color: #FFF;
		margin-bottom: 4px;
		display: flex;
		align-items: stretch;
		.button_action {
			padding: 0.1em 0.5em;
		}
	}

	.adventure_actions {
		& > .adventure_action {
			background: rgb(124, 0, 0);
			.adventure_actions {
				& > .adventure_action {
					background: rgb(126, 93, 9);
					.adventure_actions {
						& > .adventure_action {
							background: rgb(0, 75, 0);
						}
					}
				}
			}
		}
	}

	/*
	&_style {
		&_adventure_fail {
			background: rgb(124, 0, 0);
		}
		&_adventure_milestone {
			background: rgb(187, 137, 12);
		}
		&_adventure_milestone {
			background: rgb(187, 137, 12);
		}
		&_adventure_milestone {
			background: rgb(187, 137, 12);
		}
		&_trigger_alt_start,
		&_trigger_alt_end {
			background: rgb(0, 75, 0);
		}
	}
	*/
}
</style>