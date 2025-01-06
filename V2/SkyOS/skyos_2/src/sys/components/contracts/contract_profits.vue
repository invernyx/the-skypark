<template>
	<div class="contract-profits">
		<div class="data-stack data-stack--vertical">
			<div>
				<span class="label">Pay</span>
				<span class="value" v-if="contract.RewardBux != 0"><span class="amt">{{ (contract.RewardBux >= 0 ? '$' : '-$') + Math.abs(contract.RewardBux).toLocaleString('en-gb') }}</span></span>
				<span class="value" v-else-if="contract.RewardKarma != 0"><span class="amt">{{ contract.RewardKarma > 0 ? '+' : '-' }} Karma</span></span>
				<span class="value" v-else-if="contract.RewardReliability > 5"><span class="amt">+ Reliability</span></span>
				<span class="value" v-else-if="contract.RewardXP > 0"><span class="amt">+XP</span></span>
				<span class="value" v-else><span class="amt">no reward</span></span>
			</div>
			<div>
				<span class="label">Fees</span>
				<span class="value" v-if="contract.State != 'Succeeded' && contract.State != 'Failed'">${{ invoiceData.TotalFees.toLocaleString('en-gb') }}{{ contract.Invoices.TotalRefunds != 0 ? '*' : '' }}</span>
				<span class="value" v-else>${{ (invoiceData.TotalFees + invoiceData.TotalRefunds).toLocaleString('en-gb') }}</span>
			</div>
			<div :class="[invoiceData.TotalProfits > 0 ? 'positive' : 'negative']">
				<span class="label">Profits</span>
				<span class="value">${{ invoiceData.TotalProfits.toLocaleString('en-gb') }}</span>
			</div>
		</div>
		<p class="notice" v-if="contract.Invoices.TotalRefunds != 0 && contract.State != 'Succeeded' && contract.State != 'Failed'">* Some fees will be refunded upon completion</p>
		<!--
		<div class="data-stack data-stack--vertical helper_edge_margin_bottom">
			<div>
				<span class="label">Liability</span>
				<span class="value">${{ invoiceData.TotalLiabilities.toLocaleString('en-gb') }}</span>
			</div>
		</div>
		-->
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "contract_profits",
	props: ['contract', 'template', 'invoiceData'],
	data() {
		return {
		}
	},
	mounted() {
	},
	methods: {
		init() {
		},
	},
	watch: {
		template: {
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
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';
.contract-profits {
	padding-bottom: 8px;
	.notice {
		margin-top: 0;
	}
}
</style>