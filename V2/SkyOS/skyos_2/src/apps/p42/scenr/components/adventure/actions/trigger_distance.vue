<template>
	<div>
		<div class="buttons_list_half">
			<div class="columns columns_margined_half">
				<div class="column">
					<textbox class="listed" placeholder="Distance formula (NM)" type="text" v-model="Action.Params.Distance"></textbox>
				</div>
			</div>
		</div>
		<adventureactions
			placeholder="Has entered range"
			styleStr="is-black is-subaction border-success"
			:Exclusions="[]"
			:SituationActions="Action.Params.EnterActions"
			:app="app"
			:Situation="Situation"
			:Adventure="Adventure"
		></adventureactions>
		<adventureactions
			placeholder="Has left range"
			styleStr="is-black is-subaction border-success"
			:Exclusions="[]"
			:SituationActions="Action.Params.ExitActions"
			:app="app"
			:Situation="Situation"
			:Adventure="Adventure"
		></adventureactions>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import AdventureProjectActionInterface from "./../../../interfaces/adventure/action";

export default Vue.extend({
	name: "scenrAdventure_event_trigger_distance",
	props: ["app", "Actions", "Action", "Situation", "Adventure"],
	beforeCreate() {
		this.$options.components.adventureactions = require("./../actions.vue").default;
	},
	methods: {
	},
	data() {
		return {

		};
	},
});
</script>
