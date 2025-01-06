<template>
	<div class="app-box app-box-margined shadowed-deep h_edge_padding_top_half">
		<h3 class="separator">Passengers</h3>
		<div class="columns h_edge_padding_bottom_half">

			<Humans_summary :contract="contract" :humans="humans"></Humans_summary>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Humans_summary from '@/sys/components/contracts/panel/child/humans_summary.vue';
import Contract from '@/sys/classes/contracts/contract';

export default Vue.extend({
	props: {
		contract: Contract,
		humans :Array
	},
	components: {
		Humans_summary
	},
});
</script>

<style lang="scss" scoped>

.pax_group_humans {
	margin: 0 20px;
}

</style>