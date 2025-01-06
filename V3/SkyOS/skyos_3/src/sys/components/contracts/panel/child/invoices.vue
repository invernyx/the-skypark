<template>
	<div>
		<div class="navigation">
			<div :class="{ 'active': ui.invoice_page == 'estimates' }" @click="ui.invoice_page = 'estimates'">Pending<span class="badge" v-if="invoice_quoted.length">{{ invoice_quoted.length }}</span></div>
			<div :class="{ 'active': ui.invoice_page == 'paid' }" @click="ui.invoice_page = 'paid'">Paid<span class="badge" v-if="invoice_paid.length">{{ invoice_paid.length }}</span></div>
			<!--<button_action :class="{ 'active': ui.invoice_page == 'requirements' }" @click="ui.invoice_page = 'requirements'">Requirements</button_action>-->
		</div>

		<div v-if="ui.invoice_page == 'estimates'">
			<div v-if="invoice_quoted.length > 0">
				<InvoiceFrame v-for="(invoice, index) in invoice_quoted" v-bind:key="'A' + index" :content="invoice" />
			</div>
			<div v-else class="invoice-none">No estimates to show</div>
		</div>
		<div v-else>
			<div v-if="invoice_paid.length > 0">
				<InvoiceFrame v-for="(invoice, index) in invoice_paid" v-bind:key="'A' + index" :content="invoice" />
			</div>
			<div v-else class="invoice-none">No paid invoices to show</div>
		</div>
	</div>
</template>

<script lang="ts">
import Contract from "@/sys/classes/contracts/contract";
import InvoiceFrame from "@/sys/components/invoices/invoice.vue"
import Vue from "vue";

export default Vue.extend({
	props: {
		contract :Contract,
		default_page :String
	},
	components: {
		InvoiceFrame
	},
	data() {
		return {
			ui: {
				invoice_page: this.default_page ? this.default_page : 'estimates'
			}
		}
	},
	methods: {
	},
	computed: {
		invoice_quoted() {
			return (this.contract as Contract).invoices != null ? this.contract.invoices.invoices.filter(x => x.status == 'QUOTE') : [];
		},
		invoice_paid() {
			return (this.contract as Contract).invoices != null ? this.contract.invoices.invoices.filter(x => x.status != 'QUOTE') : [];
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.navigation {
	position: relative;
	display: flex;
	justify-content: center;
	margin-bottom: 16px;
	z-index: 2;

	.theme--bright &,
	&.theme--bright {
		& > div {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			&:hover {
				background-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
			&:active {
				background-color: rgba($ui_colors_dark_shade_5, 0.3);
			}
			&.active {
				background-color: rgba($ui_colors_dark_shade_5, 0.3);
				border-bottom-color: $ui_colors_dark_shade_5;
				@include shadowed_shallow($ui_colors_bright_shade_5);
			}
			.badge {
				background-color: $ui_colors_bright_button_cancel;
				&.positive {
					background-color: $ui_colors_bright_button_go;
				}
			}
		}
	}
	.theme--dark &,
	&.theme--dark {
		& > div {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
			&:hover {
				background-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
			&:active {
				background-color: rgba($ui_colors_dark_shade_5, 0.3);
			}
			&.active {
				background-color: rgba($ui_colors_dark_shade_5, 0.3);
				border-bottom-color: $ui_colors_dark_shade_5;
				@include shadowed_shallow($ui_colors_dark_shade_0);
			}
			.badge {
				background-color: $ui_colors_dark_button_cancel;
				&.positive {
					background-color: $ui_colors_dark_button_go;
				}
			}
		}
	}

	& > div {
		display: flex;
		font-family: "SkyOS-SemiBold";
		cursor: pointer;
		font-size: 14px;
		background: transparent;
		padding: 2px 14px;
		margin-bottom: -4px;
		margin-right: 4px;
		border-radius: 4px;
		border-bottom: 4px solid transparent;
		transition: background 0.3s ease-out;
		&:hover {
			transition: background 0.02s ease-out;
		}
		&:last-child {
			margin-right: 0;
		}
		.icon {
			width: 18px;
			height: 18px;
			margin: -4px;
		}
		.label {
			margin-left: 8px;
		}
		.badge {
			padding: 0 4px;
			border-radius: 3px;
			margin-left: 8px;
			margin-right: -6px;
		}

	}
}

.invoice-none {
	text-align: center;
}

</style>