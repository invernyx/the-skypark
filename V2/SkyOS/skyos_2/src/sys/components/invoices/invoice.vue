<template>
	<div class="invoice" :class="['invoice_type_' + content.PayeeType.toLowerCase() + '_' + (content.PayeeAccount != null ? content.PayeeAccount : 'personal')]" v-if="content">
		<div class="invoice_header_background"></div>
		<div class="invoice_header">
			<div class="invoice_header_logo"></div>
			<div class="invoice_header_title">{{ getCompanyName(content) }}</div>
			<div class="invoice_header_meta">
				<div class="invoice_header_status" :class="'status_' + content.Status.toLowerCase()">{{ getStatus() }}</div>
				<!--<div class="invoice_header_date">{{ moment.utc(date).calendar(null, { sameDay: "HH:mm [GMT]", lastDay: "[Yesterday at] HH:mm [GMT]", lastWeek: "[Last] dddd", sameElse: "MMM Do, YYYY" })  }}</div>-->
			</div>

		</div>

		<div class="invoice_fees invoice_box" v-if="content.Fees">
			<collapser :withArrow="true" :default="true" :isReversed="true">
				<template v-slot:title>
					<div class="invoice_box_title">
						<div class="invoice_box_total">
							<div>{{ feesSum >= 0 ? 'Cost' : 'Reimbursement' }}</div>
							<div v-if="!feesUncertain">${{ feesSum.toLocaleString('en-gb') }}</div>
							<div v-else-if="feesSum > 0">TBD (${{ feesSum.toLocaleString('en-gb') }}+)</div>
							<div v-else>TBD</div>
						</div>
						<div class="collapser_arrow"></div>
					</div>
				</template>
				<template v-slot:content>
					<div class="invoice_box_fees">
						<InvoiceFee v-for="(fee, index) in content.Fees" v-bind:key="index" :fee="fee" />
					</div>
				</template>
			</collapser>
		</div>

		<div class="invoice_refunds invoice_box" v-if="content.Refunds">
			<collapser :withArrow="true" :default="true" :isReversed="true">
				<template v-slot:title>
					<div class="invoice_box_title">
						<div class="invoice_box_total">
							<div>Refund</div>
							<div v-if="!refundsUncertain">${{ refundsSum.toLocaleString('en-gb') }}</div>
							<div v-else-if="refundsSum > 0">TBD (${{ refundsSum.toLocaleString('en-gb') }}+)</div>
							<div v-else>TBD</div>
						</div>
						<div class="collapser_arrow"></div>
					</div>
				</template>
				<template v-slot:content>
					<div class="invoice_box_fees">
						<div>
							<div class="invoice_box_fee" v-for="(fee, index) in content.Refunds" v-bind:key="index">
								<div>

									<div>{{ getRefundDescription(fee) }}</div>
									<div class="invoice_box_fee_note highlighted" v-if="!fee.Refunded">{{ getRefundNote(fee) }}</div>
									<div class="invoice_box_fee_note" v-else>{{ (fee.Params.Percentage != 100 ? fee.Params.Percentage + '% refunded': 'Fully refunded') }}</div>
								</div>
								<div>{{ fee.Amount !== null ? '$' + fee.Amount.toLocaleString('en-gb') : "TBD" }}</div>
							</div>
						</div>
					</div>
				</template>
			</collapser>
		</div>

		<div class="invoice_total invoice_box" v-if="content.Liability || content.Refunds">
			<div class="invoice_box_title">
				<div class="invoice_box_total">
					<div>Total Cost</div>
					<div>{{ '$' + (feesSum + refundsSum).toLocaleString('en-gb') }}</div>
				</div>
			</div>
		</div>


		<div class="invoice_liabilities invoice_box" v-if="content.Liability">
			<collapser :withArrow="true" :default="true" :isReversed="true">
				<template v-slot:title>
					<div class="invoice_box_title">
						<div class="invoice_box_total">
							<div>Liabilities</div>
							<div v-if="!liabilitiesUncertain">${{ liabilitiesSum.toLocaleString('en-gb') }}</div>
							<div v-else-if="liabilitiesSum > 0">TBD (${{ liabilitiesSum.toLocaleString('en-gb') }}+)</div>
							<div v-else>TBD</div>
						</div>
						<div class="collapser_arrow"></div>
					</div>
				</template>
				<template v-slot:content>
					<div class="invoice_box_fees">
						<InvoiceFee v-for="(fee, index) in content.Fees" v-bind:key="index" :fee="fee" />
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
	methods: {
		init() {
			if(this.content) {
				this.date = new Date(this.content.Date);

				this.feesSum = 0;
				this.feesUncertain = false;
				if(this.content.Fees) {
					this.content.Fees.forEach(fee => {
						if(fee.Amount == null) {
							this.feesUncertain = true;
						}
						this.feesSum += fee.Amount !== null ? fee.Amount : 0;
						if(fee.Discounts) {
							fee.Discounts.forEach(discount => {
								this.feesSum += discount.Amount;
							});
						}
					});
				}

				this.liabilitiesSum = 0;
				this.liabilitiesUncertain = false;
				if(this.content.Liability) {
					this.content.Liability.forEach(fee => {
						if(fee.Amount == null) {
							this.liabilitiesUncertain = true;
						}
						this.liabilitiesSum += fee.Amount !== null ? fee.Amount : 0;
						if(fee.Discounts) {
							fee.Discounts.forEach(discount => {
								this.liabilitiesSum += discount.Amount;
							});
						}
					});
				}

				this.refundsSum = 0;
				this.refundsUncertain = false;
				if(this.content.Refunds) {
					this.content.Refunds.forEach(fee => {
						if(fee.Amount == null) {
							this.refundsUncertain = true;
						}
						this.refundsSum += fee.Amount !== null ? fee.Amount : 0;
					});
				}
			}
		},
		getStatus() {
			switch(this.content.Status) {
				case "QUOTE": return 'Estimate';
				case "OPEN": return 'Open';
				case "PAID": return 'Paid';
				case "LATE": return 'Late';
				case "REFUNDED": return 'Refunded';
				default: return 'Unknown';
			}
		},
		getRefundNote(fee :any) {
			switch(fee.RefundMoment) {
				case "SUCCEED": {
					switch(fee.Code) {
						case "%userrelocation%": return (fee.Params.Percentage != 100 ? fee.Params.Percentage + '% refunded upon completion' : 'Full refund upon completion') + '';
						case "%bobservice%": return (fee.Params.Percentage != 100 ? fee.Params.Percentage + '% refunded upon completion' : 'Full refund upon completion') + '';
						default: return fee.Code;
					}
				}
				default: return fee.Code;
			}
		},
		getRefundDescription(fee :any) {
			switch(fee.Code) {
				case "%bobservice%": return 'Aero services costs';
				case "%userrelocation%": return 'Travel costs';
				default: return fee.Code;
			}
		},
		getCompanyName(fee :any) {
			switch(fee.PayeeType) {
				case 'AGENCY': {
					switch(fee.PayeeAccount) {
						case 'clearsky': return "ClearSky";
						default: return 'Unknown';
					}
				}
				case 'PRIVATE': {
					switch(fee.PayeeAccount) {
						case null: return 'Personal expenses';
						default: return 'Unknown';
					}
				}
				case 'SERVICE': {
					switch(fee.PayeeAccount) {
						case 'bobsaeroservice': return "Bob's Aero Service";
						case 'oceanicair': return "Oceanic Air";
						default: return 'Unknown';
					}
				}
				default: return 'Unknown';
			}
		}
	},
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
			status: 'Unknown',
			date: null,
			feesUncertain: false,
			feesSum: 0,
			liabilitiesUncertain: false,
			liabilitiesSum: 0,
			refundsUncertain: false,
			refundsSum: 0,
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