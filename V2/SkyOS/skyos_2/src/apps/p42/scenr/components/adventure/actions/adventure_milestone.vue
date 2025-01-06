<template>
	<div>
		<div class="buttons_list_half">
			<textbox class="listed" placeholder="UI Action Label" type="text" v-model="Action.Params.Label"></textbox>
		</div>
		<adventureactions
			placeholder="Completed Action"
			styleStr="is-black is-subaction border-success"
			:Exclusions="[]"
			:SituationActions="Action.Params.SuccessActions"
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
	name: "scenrAdventure_event_adventure_milestone",
	props: ["app", "Actions", "Action", "Situation", "Adventure"],
	beforeCreate() {
		this.$options.components.adventureactions = require("./../actions.vue").default;
	},
	data() {
		return {};
	},
});
</script>
