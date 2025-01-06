<template>
	<div>
		<div class="invoice_box_fee">
			<div>
				<div>{{ getCodeString(fee) }} </div>
				<div class="invoice_box_fee_note">{{ getCodeDescription(fee) }}</div>
				<div class="invoice_box_fee_note" v-for="(line, index) in getCodeNote(fee)" v-bind:key="index">{{ line }}</div>
			</div>
			<div v-if="fee.amount !== null"><currency :amount="fee.amount" :decimals="0"/></div>
			<div v-else>TBD</div>
		</div>
		<div class="invoice_box_fee" v-for="(discount, index) in fee.discounts" v-bind:key="index">
			<div>
				<div>{{ getCodeString(fee) }} discount</div>
				<div class="invoice_box_fee_note">{{ getCodeDiscount(fee, discount) }}</div>
			</div>
			<div><currency :amount="discount.amount" :decimals="0"/></div>
		</div>
	</div>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';

export default Vue.extend({
	name: "invoice_entry",
	props: ['fee'],
	components: {
	},
	mounted() {
		this.init();
	},
	beforeDestroy() {
	},
	data() {
		return {
		}
	},
	methods: {
		init() {
		},
		getCodeString(fee :any) {
			switch(fee.code) {
				case "cargo_insured": return 'Cargo insurance';
				case "cargo_uninsured": return 'Uninsured cargo';
				case "aircraft_registration_quote": return 'Aircraft registration';
				case "aircraft_insurance_quote": return 'Aircraft insurance';
				case "fuel_quote": return 'Fuel';
				case "bonus_ontime_dep": return "On-Time Bonus (" + Math.round(fee.params.percent) + "%)";
				case "relocation": return 'Relocation travel';
				case "relocation_discount_xp": case "discountxp": return 'New ClearSky pilot';
				case "relocation_discount_reliability": case "discountrel": return 'Reliability rating discount';
				case "loadmaster_pay": return 'Loadmaster Pay';
				default: return fee.code;
			}
		},
		getCodeDescription(fee :any) {
			switch(fee.code) {
				case "cargo_insured": return '$' + fee.params.Value.toLocaleString('en-gb') + ' liabilities';
				case "aircraft_registration_quote": return fee.params ? 'Register ' + fee.params.name + ' until ' + new Date(fee.params.contract_expire).toLocaleDateString() : null;
				case "aircraft_insurance_quote": return fee.params ? 'Insure ' + fee.params.name + ' until ' + new Date(fee.params.contract_expire).toLocaleDateString() : null;
				case "relocation": {
					let type = 'Bycicle';
					switch(fee.params.type) {
						case 'bus': type = 'Bus'; break;
						case 'shuttle': type = 'Shuttle'; break;
						case 'flight': type = 'Flight'; break;
					}
					return type + ' from ' + fee.params.from + ' to ' + fee.params.to;
				}
				case "bonus_ontime_dep": {

					const format = this.$os.userConfig.get(['ui','units','numbers']);
					const t = new Intl.DateTimeFormat(format, {
						timeStyle: 'short'
					}).format(Eljs.convertDateToUTC(fee.params.time)).replace(/([A-Z])\w+/, '').trim();

					return "Wheels up time was " + t + " GMT";
				}
				case "loadmaster_pay": return "Paid every day you carry cargo"
				default: return null;
			}
		},
		getCodeNote(fee :any) {
			switch(fee.code) {
				case "relocation": {
					let output = [];

					if(fee.params.details) {
						fee.params.details.forEach(detail => {
							switch(detail.type) {
								case "holiday": {
									output.push("Holiday (" + detail.country + "): " + detail.note + "");
									break;
								}
							}
						});
					}

					return output;
				}
				default: return null;
			}
		},
		getCodeDiscount(fee :any, discount :any) {
			switch(discount.code) {
				case "relocation_discount_xp": return discount.params.percentage + "% discount under level 5";
				case "relocation_discount_reliability": return discount.params.percentage + "% for being a reliable pilot";
				default: return null;
			}
		},
	},
	watch: {
		fee: {
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