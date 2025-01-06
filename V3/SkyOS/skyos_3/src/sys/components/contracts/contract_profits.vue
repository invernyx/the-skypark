<template>
	<div class="contract-profits">

		<div class="columns columns_margined_half columns_break_800">
			<div class="column">
				<data_stack class="center h_edge_margin_top" label="Pay">
					<span class="reward" v-if="contract.reward_bux != 0"><currency class="reward_bux" :amount="contract.reward_bux" :decimals="0"/></span>
					<span class="reward" v-else-if="contract.reward_karma != 0"><span class="amt">{{ contract.reward_karma > 0 ? '+' : '-' }} Karma</span></span>
					<span class="reward" v-else-if="contract.reward_reliability > 5"><span class="amt">+Reliability</span></span>
					<span class="reward" v-else-if="contract.reward_xp > 0"><span class="amt">+XP</span></span>
					<span class="reward" v-else><span class="amt">No reward</span></span>
				</data_stack>
			</div>
			<div class="column">
				<data_stack class="center h_edge_margin_top" :label="contract.invoices.total_fees + contract.invoices.total_refunds >= 0 ? 'Fees' : 'Bonus'">
					<currency :amount="Math.abs(contract.state != 'Succeeded' && contract.state != 'Failed' ? contract.invoices.total_fees : (contract.invoices.total_fees + contract.invoices.total_refunds))" :decimals="0"/>{{ contract.invoices.total_refunds != 0 && contract.state != "Failed" && contract.state != "Succeeded" ? '*' : '' }}
				</data_stack>
			</div>
			<div class="column">
				<data_stack class="center h_edge_margin_top profits" label="Profits" :class="{ 'positive': contract.invoices.total_profits > 0, 'negative': contract.invoices.total_profits < 0 }">
					<currency :amount="contract.invoices.total_profits" :decimals="0"/>
				</data_stack>
			</div>
		</div>

		<p class="notice" v-if="contract.invoices.total_refunds != 0 && contract.state != 'Succeeded' && contract.state != 'Failed'">Some fees will be refunded upon completion.</p>
		<!--
		<div class="data-stack data-stack--vertical helper_edge_margin_bottom">
			<div>
				<span class="label">Liability</span>
				<span class="value">${{ contract.invoices.TotalLiabilities.toLocaleString('en-gb') }}</span>
			</div>
		</div>
		-->
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import Vue from 'vue';

export default Vue.extend({
	props:{
		contract :Contract
	},
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
		contract: {
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
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.contract-profits {

	.notice {
		margin-top: 8px;
		text-align: center;
	}

	.profits {
		border-radius: 8px;

		.theme--bright &,
		&.theme--bright {
			&.positive {
				span {
					color: $ui_colors_bright_button_go;
				}
			}
			&.negative {
				span {
					color: $ui_colors_bright_button_cancel;
				}
			}
		}

		.theme--dark &,
		&.theme--dark {
			&.positive {
				span {
					color: $ui_colors_dark_button_go;
				}
			}
			&.negative {
				span {
					color: $ui_colors_dark_button_cancel;
				}
			}
		}
	}
}
</style>