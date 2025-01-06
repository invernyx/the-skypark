<template>
	<div>
		<div class="buttons_list_half">
			<textbox class="listed" placeholder="To-do label" type="text" v-model="Action.Params.Label"></textbox>
			<textbox class="listed" placeholder="Button label" type="text" v-model="Action.Params.Button"></textbox>
		</div>
		<adventureactions
			placeholder="Button was pressed"
			styleStr="is-black is-subaction border-success"
			:Exclusions="[]"
			:SituationActions="Action.Params.EnterActions"
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
	name: "scenrAdventure_event_trigger_engine_stop",
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
