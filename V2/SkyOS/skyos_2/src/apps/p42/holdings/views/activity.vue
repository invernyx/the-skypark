<template>
	<div>
		<content_controls_stack :translucent="true" :shadowed="true" :scroller_offset="{ bottom: 15 }">
			<template v-slot:content>
				<div class="header helper_edge_padding">
					<div class="data-stack data-stack--vertical">
						<div>
							<span class="value" v-if="ui.accountIndex > -1">${{ state.accounts[ui.accountIndex].Balance.toLocaleString('en-gb', { minimumFractionDigits: 2 }) }}</span>
							<span class="value" v-if="!$root.$data.state.services.api.connected">Start the Skypark Transponder to access your bank balance.</span>
						</div>
					</div>
					<p class="notice">NOTICE: While in Early Access, we're introducing operation costs slowly to ensure appropriate profits/loss for Endeavour Mode players. Today relocation costs, tomorrow much more... //42 Team</p>
				</div>
				<div class="helper_edge_padding_lateral helper_edge_padding_vertical_half helper_nav-margin">
					<div class="buttons_list shadowed" v-if="ui.accountIndex > -1">
						<div class="transaction" v-for="(transaction, index) in state.accounts[ui.accountIndex].Transactions" v-bind:key="index">

							<div class="info">
								<div class="name" v-if="transaction.Title.length">{{ getTitle(transaction.Title) }}</div>
								<div class="description" v-if="transaction.Description.length">{{ transaction.Description }}</div>
								<div class="fees">
									<div class="fee" v-for="(fee, index) in transaction.Fees" v-bind:key="index">
										<span>{{ getCodeString(fee) }}</span> <span>${{ (fee.Amount).toLocaleString('en-gb', { minimumFractionDigits: 2 }) }}</span>
									</div>
								</div>
								<div class="accounts">{{ transaction.NetAmount > 0 ? transaction.OtherParty + " ❯ " + state.accounts[ui.accountIndex].AccountName : state.accounts[ui.accountIndex].AccountName + " ❯ " + transaction.OtherParty }}</div>
							</div>
							<div class="values">
								<div class="value" :class="transaction.NetAmount > 0 ? 'p' : ''">${{ (transaction.NetAmount).toLocaleString('en-gb', { minimumFractionDigits: 2 }) }}</div>
								<div class="date"><countdown :time="new Date(transaction.Date)" :short="true"></countdown></div>
								<div class="refund" v-if="transaction.IsRefund">Refund</div>
							</div>
						</div>
					</div>
				</div>
			</template>
		</content_controls_stack>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_holdings_general",
	data() {
		return {
			ui: {
				accountIndex: -1,
			},
			state: {
				accounts: [],
			}
		}
	},
	methods: {

		getTitle(title :string) {
			switch(title) {
				case '%bobservice%': return "Bob's Aero Service";
				case '%userrelocation%': return "Oceanic Air";
				default: return title;
			}
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
				case "relocation_discount_reliability": case "discountrel": return 'Reliability discount';
				case "staff_loadmaster": return 'Staffing: Loadmaster';
				default: return fee.Code;
			}
		},

		refreshData() {
			this.$root.$data.services.api.SendWS(
				'bank:get', {
					limit: 30,
				}, (bankData: any) => {
					this.state.accounts = bankData.payload;
					this.ui.accountIndex = 0;
				}
			);
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.refreshData();
					break;
				}
				case 'disconnect': {
					this.ui.accountIndex = -1;
					this.state.accounts = [];
					break;
				}
				case 'bank': {
					switch(wsmsg.name[1]){
						case 'transaction': {
							switch(wsmsg.payload.AccountName) {
								case 'bank_checking': {
									this.refreshData();
									break;
								}
							}
							break;
						}
					}
					break;
				}
			}
		},
	},
	mounted() {
		this.refreshData();
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
});
</script>

<style lang="scss">
	@import '../../../../sys/scss/colors.scss';

	.p42_holdings {
		.theme--bright &,
		&.theme--bright {
			&.p42_holdings_activity {
				.transaction {
					border-bottom-color: rgba($ui_colors_bright_shade_5,0.1);
					background: $ui_colors_bright_shade_0;
					.refund {
						background: $ui_colors_bright_button_cancel;
						color: $ui_colors_bright_shade_0;
					}
				}
			}
		}
		.theme--dark &,
		&.theme--dark {
			&.p42_holdings_activity {
				.transaction {
					border-bottom-color: rgba($ui_colors_dark_shade_5,0.1);
					background: $ui_colors_dark_shade_0;
					.refund {
						background: $ui_colors_dark_button_cancel;
					}
				}
			}
		}

		&_activity {
			.app-frame {
				background-size: 80px;
				background-image: url(./../assets/background-pattern.svg);
			}
			.transaction {
				display: flex;
				flex-direction: row;
				padding: 0.6em 1em 0.5em 1em;
				border-bottom: 1px solid transparent;
				&:last-child {
					border-bottom: 0;
				}
				.info {
					flex-grow: 1;
					display: flex;
					flex-direction: column;
					justify-content: center;
					.name {
						font-family: "SkyOS-SemiBold";
						font-size: 16px;
						line-height: 16px;
					}
					.description {
						font-size: 12px;
					}
					.accounts {
						font-size: 12px;
						opacity: 0.5;
					}
					.fees {
						font-size: 12px;
						.fee {
							//display: flex;
							//justify-content: space-between;
							span {
								display: inline-block;
								&:first-child {
									font-family: "SkyOS-SemiBold";
								}
								&:last-child {
									margin-left: 8px;
								}
							}
						}
					}
				}
				.values {
					text-align: right;
					margin-left: 1em;
					min-width: 6em;
					.value {
						font-family: 'Courier New', sans-serif;
						font-weight: bold;
						font-size: 1.2em;
						line-height: 1em;
						color: #C00;
						&.p {
							color: #57964A;
						}
					}
					.date {
						font-size: 0.8em;
					}
					.refund {
						display: inline-block;
						font-size: 11px;
						line-height: 12px;
						padding: 2px 4px;
						border-radius: 4px;
						text-transform: uppercase;
						font-family: "SkyOS-SemiBold";
					}
				}
			}
			.header {

			}
		}
	}

</style>