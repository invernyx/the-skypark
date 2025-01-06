<template>
	<div>
		<div v-if="Action.Action == 'trigger_ias_start'">
			<div class="buttons_list_half">
				<textbox class="listed" :disabled="true" placeholder="Unique ID" type="text" :value="Action.UID"></textbox>
				<textbox class="listed" placeholder="UI Label" type="text" v-model="Action.Params.Label"></textbox>
			</div>
			<div class="buttons_list_half">
				<div class="columns columns_margined_half">
					<div class="column">
						<textbox class="listed" placeholder="Minimum" type="number" v-model="Action.Params.Min"></textbox>
					</div>
					<div class="column">
						<textbox class="listed" placeholder="Maximum" type="number" v-model="Action.Params.Max"></textbox>
					</div>
				</div>
			</div>
			<adventureactions
				placeholder="Aircraft is within limits"
				styleStr="is-black is-subaction border-success"
				:Exclusions="[]"
				:SituationActions="Action.Params.EnterActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
			<adventureactions
				placeholder="Aircraft is outside limits"
				styleStr="is-black is-subaction border-danger"
				:Exclusions="[]"
				:SituationActions="Action.Params.ExitActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
		</div>
		<div v-if="Action.Action == 'trigger_ias_end'">

			<div class="buttons_list_half">
				<selector class="listed" placeholder="What are we ending?" v-model="Action.Params.Link" @input="RefreshLinks">
					<option
						v-for="Item in Available"
						v-bind:key="Item.UID"
						:value="Item.UID"
					>{{ Item.Params.Min + " to " + Item.Params.Max + " | " + Item.UID }}</option>
				</selector>
				<textbox class="listed" placeholder="Cancel Radius" type="number" v-model="Action.Params.CancelRange"></textbox>
			</div>
			<adventureactions
				placeholder="Aircraft did not exceed the limit"
				styleStr="is-black is-subaction border-success"
				:Exclusions="[]"
				:SituationActions="Action.Params.SuccessActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
			<adventureactions
				placeholder="Aircraft exceeded the limit"
				styleStr="is-black is-subaction border-danger"
				:Exclusions="[]"
				:SituationActions="Action.Params.FailActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import AdventureProjectActionInterface from "../../../interfaces/adventure/action";

export default Vue.extend({
	name: "scenrAdventure_event_limit_speed",
	props: ["app", "Actions", "Action", "Situation", "Adventure"],
	beforeCreate() {
		this.$options.components.adventureactions = require("./../actions.vue").default;
	},
	data() {
		return {
			Available: [] as AdventureProjectActionInterface[],
		};
	},
	created() {
		['action_updated','action_added','action_removed'].forEach(event => {
			this.app.$on(event, (e: any) => {
				if (e !== this.Situation) {
					this.GetAvailable();
				}
			});
		});

		this.GetAvailable();
		this.app.$emit("action_updated", this.Action);
	},
	methods: {
		RefreshLinks() {
			this.app.$emit("action_updated", this.Action);
			this.GetAvailable();
		},
		GetAvailable() {
			const self = this;

			this.Available = this.Adventure.GetAvailable(this.Action, "trigger_ias_start", "trigger_ias_end");
			if (this.Available.length > 0) {
				if (this.Action.Params.Link === null) {
					this.Action.Params.Link = this.Available[0].UID;
				}
			} else if (this.Action.Params.Link) {
				this.Action.Params.Link = null;
			}
		}
	},
	computed: {}
});
</script>