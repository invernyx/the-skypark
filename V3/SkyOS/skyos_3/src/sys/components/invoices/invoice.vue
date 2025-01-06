<template>
	<div class="invoice" :class="['invoice_type_' + content.payee_type.toLowerCase() + '_' + (content.payee_account != null ? content.payee_account : 'personal')]" v-if="content && this.ready">

		<div class="invoice_header_background"></div>

		<div class="invoice_header">
			<div class="invoice_header_logo"></div>
			<div class="invoice_header_title">{{ getCompanyName(content) }}</div>
			<div class="invoice_header_meta">
				<div class="invoice_header_status" :class="'status_' + content.status.toLowerCase()">{{ getStatus() }}</div>
				<!--<div class="invoice_header_date">{{ moment.utc(date).calendar(null, { sameDay: "HH:mm [GMT]", lastDay: "[Yesterday at] HH:mm [GMT]", lastWeek: "[Last] dddd", sameElse: "MMM Do, YYYY" })  }}</div>-->
			</div>
		</div>

		<div class="invoice_fees invoice_box" v-if="content.fees">
			<collapser :withArrow="true" :default="true" :isReversed="true">
				<template v-slot:title>
					<div class="invoice_box_title">
						<div class="invoice_box_total">
							<div>{{ fees_sum >= 0 ? 'Cost' : 'Reimbursement' }}</div>
							<div v-if="!feesUncertain"><currency :amount="Math.abs(fees_sum)" :decimals="0"/></div>
							<div v-else-if="fees_sum > 0">TBD (<currency :amount="Math.abs(fees_sum)" :decimals="0"/>+)</div>
							<div v-else>TBD</div>
						</div>
						<div class="collapser_arrow"></div>
					</div>
				</template>
				<template v-slot:content>
					<div class="invoice_box_fees">
						<InvoiceFee v-for="(fee, index) in content.fees" v-bind:key="index" :fee="fee" />
					</div>
				</template>
			</collapser>
		</div>

		<div class="invoice_refunds invoice_box" v-if="content.refunds">
			<collapser :withArrow="true" :default="true" :isReversed="true">
				<template v-slot:title>
					<div class="invoice_box_title">
						<div class="invoice_box_total">
							<div>Refund</div>
							<div v-if="!refunds_uncertain"><currency :amount="Math.abs(refunds_sum)" :decimals="0"/></div>
							<div v-else-if="refunds_sum > 0">TBD (<currency :amount="Math.abs(refunds_sum)" :decimals="0"/>+)</div>
							<div v-else>TBD</div>
						</div>
						<div class="collapser_arrow"></div>
					</div>
				</template>
				<template v-slot:content>
					<div class="invoice_box_fees">
						<div>
							<div class="invoice_box_fee" v-for="(fee, index) in content.refunds" v-bind:key="index">
								<div>
									<div>{{ getRefundDescription(fee) }}</div>
									<div class="invoice_box_fee_note highlighted" v-if="!fee.refunded">{{ getRefundNote(fee) }}</div>
									<div class="invoice_box_fee_note" v-else>{{ (fee.params.percentage != 100 ? fee.params.percentage + '% refunded': 'Fully refunded') }}</div>
								</div>
								<div>{{ fee.amount !== null ? '$' + fee.amount.toLocaleString('en-gb') : "TBD" }}</div>
							</div>
						</div>
					</div>
				</template>
			</collapser>
		</div>

		<div class="invoice_total invoice_box" v-if="content.liability || content.refunds">
			<div class="invoice_box_title">
				<div class="invoice_box_total">
					<div>Total Cost</div>
					<div><currency :amount="fees_sum + refunds_sum" :decimals="0"/></div>
				</div>
			</div>
		</div>


		<div class="invoice_liabilities invoice_box" v-if="content.liability">
			<collapser :withArrow="true" :default="true" :isReversed="true">
				<template v-slot:title>
					<div class="invoice_box_title">
						<div class="invoice_box_total">
							<div>Liabilities</div>
							<div v-if="!liabilitiesUncertain"><currency :amount="liabilities_sum" :decimals="0"/></div>
							<div v-else-if="liabilities_sum > 0">TBD (<currency :amount="liabilities_sum" :decimals="0"/>+)</div>
							<div v-else>TBD</div>
						</div>
						<div class="collapser_arrow"></div>
					</div>
				</template>
				<template v-slot:content>
					<div class="invoice_box_fees">
						<InvoiceFee v-for="(fee, index) in content.fees" v-bind:key="index" :fee="fee" />
					</div>
				</template>
			</collapser>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import InvoiceFee from './invoice_fee.vue'

export default Vue.extend({
	name: "invoice",
	props: ['content'],
	components: {
		InvoiceFee
	},
	mounted() {
		this.init();
	},
	beforeDestroy() {
	},
	data() {
		return {
			ready: false,
			status: 'Unknown',
			date: null,
			feesUncertain: false,
			fees_sum: 0,
			liabilitiesUncertain: false,
			liabilities_sum: 0,
			refunds_uncertain: false,
			refunds_sum: 0,
		}
	},
	methods: {
		init() {
			this.ready = false;
			if(this.content) {
				this.date = this.content.cate;

				this.fees_sum = 0;
				this.feesUncertain = false;
				if(this.content.fees) {
					this.content.fees.forEach(fee => {
						if(fee.amount == null) {
							this.feesUncertain = true;
						}
						this.fees_sum += fee.amount !== null ? fee.amount : 0;
						if(fee.Discounts) {
							fee.Discounts.forEach(discount => {
								this.fees_sum += discount.Amount;
							});
						}
					});
				}

				this.liabilities_sum = 0;
				this.liabilitiesUncertain = false;
				if(this.content.liability) {
					this.content.liability.forEach(fee => {
						if(fee.amount == null) {
							this.liabilitiesUncertain = true;
						}
						this.liabilities_sum += fee.amount !== null ? fee.amount : 0;
						if(fee.Discounts) {
							fee.Discounts.forEach(discount => {
								this.liabilities_sum += discount.Amount;
							});
						}
					});
				}

				this.refunds_sum = 0;
				this.refunds_uncertain = false;
				if(this.content.refunds) {
					this.content.refunds.forEach(fee => {
						if(fee.amount == null) {
							this.refunds_uncertain = true;
						}
						this.refunds_sum += fee.amount !== null ? fee.amount : 0;
					});
				}

				window.requestAnimationFrame(() => {
					this.ready = true;
				})
			}
		},
		getStatus() {
			switch(this.content.status) {
				case "QUOTE": return 'Pending';
				case "OPEN": return 'Open';
				case "PAID": return 'Paid';
				case "LATE": return 'Late';
				case "REFUNDED": return 'Refunded';
				default: return 'Unknown';
			}
		},
		getRefundNote(fee :any) {
			switch(fee.refund_moment) {
				case "SUCCEED": {
					switch(fee.code) {
						case "%userrelocation%": return (fee.params.percentage != 100 ? fee.params.percentage + '% refunded upon completion' : 'Full refund upon completion') + '';
						case "%bobservice%": return (fee.params.percentage != 100 ? fee.params.percentage + '% refunded upon completion' : 'Full refund upon completion') + '';
						default: return fee.code;
					}
				}
				default: return fee.code;
			}
		},
		getRefundDescription(fee :any) {
			switch(fee.code) {
				case "%bobservice%": return 'Aero services costs';
				case "%userrelocation%": return 'Travel costs';
				default: return fee.code;
			}
		},
		getCompanyName(fee :any) {
			switch(fee.payee_type) {
				case 'AGENCY': {
					switch(fee.payee_account) {
						case 'clearsky': return "ClearSky";
						default: return 'Unknown';
					}
				}
				case 'PRIVATE': {
					switch(fee.payee_account) {
						case null: return 'Personal expenses';
						default: return 'Unknown';
					}
				}
				case 'SERVICE': {
					switch(fee.payee_account) {
						case 'bobsaeroservice': return "Bob's Aero Service";
						case 'oceanicair': return "Oceanic Air";
						default: return 'Unknown';
					}
				}
				default: return 'Unknown';
			}
		}
	},
	watch: {
		content: {
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
	@import './invoice_style.scss';
</style>