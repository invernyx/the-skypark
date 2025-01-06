<template>
	<div class="items_list">
		<div class="content-block_half">
			<adventureactions
				placeholder="Add event: Contract saved"
				styleStr="is-dark border-info"
				:app="app"
				:Exclusions="['cargo_pickup', 'cargo_dropoff', 'adventure_succeed', 'adventure_milestone', 'adventure_fail', 'trigger_alt_start', 'trigger_alt_end', 'trigger_g_start', 'trigger_g_end', 'trigger_ias_start', 'trigger_ias_end', 'trigger_gs_start', 'trigger_gs_end']"
				:SituationActions="Adventure.SavedActions"
				:Adventure="Adventure"
			></adventureactions>
		</div>
		<div class="content-block_half">
			<adventureactions
				placeholder="Add event: Contract started"
				styleStr="is-dark border-info"
				:app="app"
				:Exclusions="['cargo_pickup', 'cargo_dropoff', 'adventure_succeed', 'adventure_milestone', 'adventure_fail', 'trigger_alt_start', 'trigger_alt_end', 'trigger_g_start', 'trigger_g_end', 'trigger_ias_start', 'trigger_ias_end', 'trigger_gs_start', 'trigger_gs_end']"
				:SituationActions="Adventure.StartedActions"
				:Adventure="Adventure"
			></adventureactions>
		</div>
		<div class="content-block_half">
			<button_action class="" @click.native="Adventure.AddSituation(0)">Insert Situation at the start</button_action>
		</div>
		<div class="separator"></div>
		<adventuresituation v-for="Situation in Adventure.Situations" v-bind:key="Situation.UID" class="items_list_item_group" :app="app" :Adventure="Adventure" :Situation="Situation" v-on:AddSituationAfter="AddSituationAfter" v-on:RemoveSituation="RemoveSituation" v-on:CopySituation="CopySituation"></adventuresituation>
		<div class="separator"></div>
		<div class="content-block_half" v-if="Adventure.Situations.length > 0">
			<button_action class="" @click.native="AddEndSituation">Insert Situation at the end</button_action>
		</div>
		<div class="content-block_half">
			<adventureactions
				placeholder="Add event: Contract completed"
				styleStr="is-dark border-success"
				:app="app"
				:Exclusions="['cargo_pickup', 'cargo_dropoff', 'adventure_succeed', 'adventure_milestone', 'adventure_fail', 'trigger_alt_start', 'trigger_alt_end', 'trigger_g_start', 'trigger_g_end', 'trigger_ias_start', 'trigger_ias_end', 'trigger_gs_start', 'trigger_gs_end']"
				:SituationActions="Adventure.SuccessActions"
				:Adventure="Adventure"
			></adventureactions>
		</div>
		<div class="content-block_half">
			<adventureactions
				placeholder="Add event: Contract failed"
				styleStr="is-dark border-danger"
				:app="app"
				:Exclusions="['cargo_pickup', 'cargo_dropoff', 'adventure_succeed', 'adventure_milestone', 'adventure_fail', 'trigger_alt_start', 'trigger_alt_end', 'trigger_g_start', 'trigger_g_end', 'trigger_ias_start', 'trigger_ias_end', 'trigger_gs_start', 'trigger_gs_end']"
				:SituationActions="Adventure.FailedActions"
				:Adventure="Adventure"
			></adventureactions>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import AdventureProj from './../../classes/adventure';
import AdventureProjectSituation from './../../interfaces/adventure/situation';

import adventureactions from './actions.vue';
import adventuresituation from './situation.vue';

export default Vue.extend({
	name: 'scenr_adventure_situations',
	props: ['app', 'Adventure'],
	components: {
		adventureactions,
		adventuresituation
	},
	data() {
		return {};
	},
	methods: {
		AddSituationAfter(situation: AdventureProjectSituation) {
			const index = this.Adventure.Situations.findIndex((x: AdventureProjectSituation) => x.UID === situation.UID);
			this.Adventure.AddSituation(index + 1);
		},
		RemoveSituation(situation: AdventureProjectSituation) {
			this.Adventure.RemoveSituation(situation.UID);
		},
		CopySituation(situation: AdventureProjectSituation) {
			this.Adventure.CopySituation(situation.UID);
		},
		AddEndSituation() {
			this.Adventure.AddSituation(this.Adventure.Situations.length);
		}
	}
});
</script>

<style lang="scss" scoped>

	.separator {
		display: flex;
		height: 20px;
		width: 1px;
		background: #000;
		opacity: 0.2;
		margin-left: 50%;
		margin-top: 8px;
		margin-bottom: 8px;
	}
</style>