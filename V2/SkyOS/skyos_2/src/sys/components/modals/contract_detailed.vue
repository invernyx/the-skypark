<template>
	<modal type="grow" width="narrow" height="small" @close="closeModal()">
		<content_controls_stack :preview="true" :shadowed="false" :translucent="true" :scroller_offset="md.data.Results ? { top: 0, bottom: 5 } : { top: 5, bottom: 5 }">
			<template v-slot:nav v-if="md.data.Results">
				<button_nav shape="back" class="translucent" :class="{ 'disabled': md.data.Results.Contracts.length < 2 }" @click.native="md.offset(-1)">Previous</button_nav>
				<button_nav shape="forward" class="translucent" :class="{ 'disabled': md.data.Results.Contracts.length < 2 }" @click.native="md.offset(1)">Next</button_nav>
			</template>
			<template v-slot:content>
				<ContractDetailed :app="md.data.App" :contained="true" :contract="md.data.Selected.Contract" :templates="[md.data.Selected.Template]"/>
			</template>
			<template v-slot:tab>
				<div class="helper_edge_margin_half">
					<div class="columns columns_margined_half">
						<div class="column column_narrow">
							<button_action @click.native="closeModal()" class="translucent">Close</button_action>
						</div>
						<div class="column">
							<button_action v-if="md.data.Selected.Contract.State == 'Active'" shape="forward" class="go" @click.native="interactState($event, 'manage')">Manage</button_action>
							<button_action v-if="md.data.Selected.Contract.State == 'Saved'" shape="forward" class="go" @click.native="interactState($event, 'fly')">Begin</button_action>
							<button_action v-if="md.data.Selected.Contract.State == 'Listed'" shape="forward" class="info" :class="{ 'loading disabled': md.data.Selected.Contract.RequestStatus != 'ready' }" @click.native="interactState($event, 'save')">{{ md.data.Selected.Contract.RequestStatus == 'ready' ? $root.$data.config.ui.tier == 'discovery' ? 'Request' : 'Save' : '' }}</button_action>
						</div>
					</div>
				</div>
			</template>
		</content_controls_stack>
	</modal>
</template>

<script lang="ts">
import Vue from 'vue';
import ContractDetailed from "@/sys/components/contracts/contract_detailed.vue"

export default Vue.extend({
	name: "contract_detailed",
	props: ['md'],
	components: {
		ContractDetailed
	},
	methods: {
		closeModal() {
			this.md.func();
			this.$emit('close');
		},
		interactState(ev: Event, name: string) {
			switch(name) {
				case 'fly':
				case 'manage': {
					this.closeModal();
					break;
				}
			}
			this.$ContractMutator.Interact(this.md.data.Selected.Contract, name, null, (response) => { });
		},
	},
	data() {
		return {

		}
	},
	mounted() {

	},
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

</style>