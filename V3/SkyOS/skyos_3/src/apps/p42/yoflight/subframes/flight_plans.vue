<template>
	<div class="flight_plans">
		<scroll_stack class="app-box shadowed-deep" :has_round_corners="true">
			<template v-slot:top>
				<div class="controls-top h_edge_padding">
					<div class="columns">
						<div class="column column_narrow h_edge_margin_right">
							<button_nav class="cancel shadowed-shallow" shape="back" @click.native="$emit('close')">Contracts</button_nav>
						</div>
						<div class="column column_justify_center column_align_end">
							<h2>Flight plans</h2>
						</div>
					</div>
				</div>
			</template>
			<template v-slot:content>
				<div v-if="contract">
					<FlightPlanBox
						v-for="(plan, index) in (contract.flight_plans.slice(offset, offset + limit))"
						v-bind:key="index"
						:index="index"
						:plan="plan"
						@details="plan_preview(plan)"/>
					<pagination
						class="h_edge_padding_vertical_half"
						:qty="contract.flight_plans.length"
						:limit="limit"
						:offset="offset"
						@set_offset="offset = $event;"/>
				</div>
			</template>
		</scroll_stack>
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import FlightPlanBox from '../components/flight_plan.vue'
import Vue from 'vue';
import Flightplan from '@/sys/classes/flight_plans/plan';

export default Vue.extend({
	props: {
		contract :Contract
	},
	components: {
		FlightPlanBox,
	},
	data() {
		return {
			offset: 0,
			limit: 10,
		}
	},
	methods: {
		close(){
			this.$emit('close');
		},

		plan_preview(plan :Flightplan) {
			this.$emit('details', { contract: this.contract, plan: plan });
			//this.$os.routing.goTo({ name: 'p42_yoflight_plan', params: { id: plan.id + '_' + this.contract.id, plan: plan }});
		},

		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {

					break;
				}
			}
		},
	},
	created() {
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.flight_plans {
	.theme--bright & {
	}

	.theme--dark & {
	}

}
</style>