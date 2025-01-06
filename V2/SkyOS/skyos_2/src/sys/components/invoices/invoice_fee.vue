<template>
	<div>
		<div class="invoice_box_fee">
			<div>
				<div>{{ getCodeString(fee) }}</div>
				<div class="invoice_box_fee_note">{{ getCodeDescription(fee) }}</div>
				<div class="invoice_box_fee_note" v-for="(line, index) in getCodeNote(fee)" v-bind:key="index">{{ line }}</div>
			</div>
			<div>{{ fee.Amount !== null ? '$' + fee.Amount.toLocaleString('en-gb') : "TBD" }}</div>
		</div>
		<div class="invoice_box_fee" v-for="(discount, index) in fee.Discounts" v-bind:key="index">
			<div>
				<div>{{ getCodeString(fee) }} discount</div>
				<div class="invoice_box_fee_note">{{ getCodeDiscount(fee, discount) }}</div>
			</div>
			<div>${{ discount.Amount.toLocaleString('en-gb') }}</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "invoice_entry",
	props: ['fee'],
	methods: {
		init() {
		},
		getCodeString(fee :any) {
			switch(fee.Code) {
				case "cargo_insured": return 'Cargo insurance';
				case "cargo_uninsured": return 'Uninsured cargo';
				case "aircraft_registration_quote": return 'Aircraft registration';
				case "aircraft_insurance_quote": return 'Aircraft insurance';
				case "fuel_quote": return 'Fuel';
				case "relocation": return 'Relocation travel';
				case "relocation_discount_xp": case "discountxp": return 'New ClearSky pilot';
				case "relocation_discount_reliability": case "discountrel": return 'Reliability rating discount';
				case "staff_loadmaster": return 'Staffing: Loadmaster';
				default: return fee.Code;
			}
		},
		getCodeDescription(fee :any) {
			switch(fee.Code) {
				case "cargo_insured": return '$' + fee.Params.Value.toLocaleString('en-gb') + ' liabilities';
				case "aircraft_registration_quote": return fee.Params ? 'Register ' + fee.Params.Name + ' until ' + new Date(fee.Params.Expire).toLocaleDateString() : null;
				case "aircraft_insurance_quote": return fee.Params ? 'Insure ' + fee.Params.Name + ' until ' + new Date(fee.Params.Expire).toLocaleDateString() : null;
				case "relocation": {
					let type = 'Bycicle';
					switch(fee.Params.Type) {
						case 'bus': type = 'Bus'; break;
						case 'shuttle': type = 'Shuttle'; break;
						case 'flight': type = 'Flight'; break;
					}
					return type + ' from ' + fee.Params.From + ' to ' + fee.Params.To;
				}
				case "staff_loadmaster": return "Paid every day you carry cargo"
				default: return null;
			}
		},
		getCodeNote(fee :any) {
			switch(fee.Code) {
				case "relocation": {
					let output = [];

					fee.Params.Details.forEach(detail => {
						switch(detail.type) {
							case "holiday": {
								output.push("Holiday (" + detail.country + "): " + detail.note + "");
								break;
							}
						}
					});

					return output;
				}
				default: return null;
			}
		},
		getCodeDiscount(fee :any, discount :any) {
			switch(discount.Code) {
				case "relocation_discount_xp": return discount.Params.Percentage + "% discount under level 5";
				case "relocation_discount_reliability": return discount.Params.Percentage + "% for being a reliable pilot";
				default: return null;
			}
		},
	},
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