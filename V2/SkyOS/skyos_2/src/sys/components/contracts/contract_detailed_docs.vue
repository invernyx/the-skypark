<template>
	<div class="contract_detailed_docs">

		<ContractProfits class="invoice_summary helper_edge_padding_bottom" :invoiceData="contract.Invoices" :contract="contract"/>

		<div class="navigation">
			<div :class="{ 'active': state.ui.page == 'estimates' }" @click="state.ui.page = 'estimates'">Estimates<span class="badge" v-if="invoiceQuoted.length">{{ invoiceQuoted.length }}</span></div>
			<div :class="{ 'active': state.ui.page == 'paid' }" @click="state.ui.page = 'paid'">Paid<span class="badge" v-if="invoicePaid.length">{{ invoicePaid.length }}</span></div>
			<!--<button_action :class="{ 'active': state.ui.page == 'requirements' }" @click="state.ui.page = 'requirements'">Requirements</button_action>-->
		</div>

		<div v-if="state.ui.page == 'estimates'">
			<div v-if="invoiceQuoted.length > 0">
				<InvoiceFrame v-for="(invoice, index) in invoiceQuoted" v-bind:key="'A' + index" :content="invoice" />
			</div>
			<div v-else class="invoice-none">No estimates to show</div>
		</div>
		<div v-else>
			<div v-if="invoicePaid.length > 0">
				<InvoiceFrame v-for="(invoice, index) in invoicePaid" v-bind:key="'A' + index" :content="invoice" />
			</div>
			<div v-else class="invoice-none">No paid invoices to show</div>
		</div>

		<p><em>THE FINE PRINT: These estimates are subject to change based on travel distance from the current airport to a contract starting position or aircraft selection. Before starting a new contract, actual costs will be presented, and your final approval will be required.</em></p>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import InvoiceFrame from "@/sys/components/invoices/invoice.vue"
import ContractProfits from "@/sys/components/contracts/contract_profits.vue"

export default Vue.extend({
	name: "contract_detailed_docs",
	props: ['contract'],
	components: {
		InvoiceFrame,
		ContractProfits
	},
	data() {
		return {
			state: {
				ui: {
					page: 'estimates'
				}
			}
		}
	},
	computed: {
		invoiceQuoted() {
			return this.contract.Invoices.Invoices.filter(x => x.Status == 'QUOTE')
		},
		invoicePaid() {
			return this.contract.Invoices.Invoices.filter(x => x.Status != 'QUOTE')
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

.contract_detailed_docs {


	.theme--bright &,
	&.theme--bright {
		.invoice  {
			@include shadowed_shallow($ui_colors_bright_shade_5);
		}
		.invoice-none {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
	}

	.theme--dark &,
	&.theme--dark {
		.invoice  {
			@include shadowed_shallow($ui_colors_dark_shade_0);
		}
		.invoice-none {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
	}

	.invoice  {
		margin-bottom: 16px;
	}

	.invoice-none {
		display: flex;
		align-items: center;
		justify-content: center;
		position: relative;
		font-size: 14px;
		padding: 8px 16px;
		height: 150px;
		margin-bottom: 16px;
		border-radius: 16px;
		overflow: hidden;
		font-family: "SkyOS-SemiBold";
		text-align: center;
	}

	.navigation {
		margin: 0 -14px;
		margin-bottom: 14px;
	}
}
</style>