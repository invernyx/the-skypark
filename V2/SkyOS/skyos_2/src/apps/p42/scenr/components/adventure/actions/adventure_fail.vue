<template>
	<div>
		<div class="buttons_list_half">
			<textbox class="listed" placeholder="Failure description" type="text" v-model="Action.Params.Reason"></textbox>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "scenrAdventure_event_adventure_fail",
	props: ["app", "Actions", "Action", "Situation", "Adventure"],
	beforeCreate() {

	},
	data() {
		return {};
	},
});
</script>
