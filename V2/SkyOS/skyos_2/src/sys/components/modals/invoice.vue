<template>
	<modal type="grow" width="narrow" height="small" @close="closeAsk(false)">
		<content_controls_stack :preview="true" :shadowed="true" :translucent="true">
			<template v-slot:nav>
				<button_nav shape="back" @click.native="closeAsk(false)">Back</button_nav>
				<h2 class="abs-center">{{ md.title }}</h2>
				<div></div>
			</template>
			<template v-slot:content>
				<div class="helper_edge_padding_lateral helper_edge_padding_vertical_half">
					<InvoiceFrame v-for="(invoice, index) in md.data.InvoiceData.Invoices" v-bind:key="'A' + index" :content="invoice" />
					<ContractProfits class="invoice_summary helper_edge_padding_bottom" :invoiceData="md.data.InvoiceData" :contract="md.data.Contract"/>
					<p v-for="(value, index) in md.text" v-bind:key="'B' + index">{{ value }}</p>
					<p><em>THE FINE PRINT: These estimates are subject to change based on travel distance from the current airport to a contract starting position or aircraft selection. Before starting a new contract, actual costs will be presented, and your final approval will be required.</em></p>
				</div>
			</template>
			<template v-slot:tab>
				<div class="helper_edge_margin_half">
					<div class="columns columns_margined_half">
						<div class="column" :class="[md.actions.yes ? 'column_narrow' : '']">
							<button_action @click.native="closeAsk(false)">{{ md.actions.no }}</button_action>
						</div>
						<div class="column" v-if="md.actions.yes">
							<button_action @click.native="closeAsk(true)" class="info">{{ md.actions.yes }}</button_action>
						</div>
					</div>
				</div>
			</template>
		</content_controls_stack>
	</modal>
</template>

<script lang="ts">
import Vue from 'vue';
import InvoiceFrame from "../invoices/invoice.vue"
import ContractProfits from "./../contracts/contract_profits.vue"

export default Vue.extend({
	name: "invoice",
	props: ['md'],
	methods: {
		init() {

		},
		closeAsk(state :boolean) {
			this.md.func(state);
			this.$emit('close');
		},
	},
	components: {
		InvoiceFrame,
		ContractProfits,
	},
	mounted() {
	},
	beforeDestroy() {
	},
	data() {
		return {

		}
	},
	watch: {
		md: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
	.invoice {
		margin-bottom: 14px;
	}
</style>