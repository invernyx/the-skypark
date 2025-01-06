<template>
	<div>
		<div class="app-header helper_edge_padding_lateral helper_status-margin">
			<div class="box helper_edge_padding_half">
				<div class="app-header--logo"></div>

				<div class="app-header--data">
					<div class="data-stack data-stack--vertical" v-if="ui.accountIndex > -1">
						<div>
							<span class="label">Current balance</span>
							<span class="value">{{ state.accounts[ui.accountIndex].Balance.toLocaleString('en-gb', { minimumFractionDigits: 2 }) }}</span>
						</div>
					</div>
				</div>
			</div>
		</div>

		<div class="helper_edge_padding helper_nav-margin">
			<div class="buttons_list shadowed" v-if="ui.accountIndex > -1">
				<div class="transaction" v-for="(transaction, index) in state.accounts[ui.accountIndex].Transactions" v-bind:key="index">
					<div class="info">
						<div class="name">{{ transaction.Description }}</div>
						<div class="description">{{ transaction.NetAmount > 0 ? transaction.OtherParty + " ❯ " + state.accounts[ui.accountIndex].AccountName : state.accounts[ui.accountIndex].AccountName + " ❯ " + transaction.OtherParty }}</div>
					</div>
					<div class="values">
						<div class="value" :class="transaction.NetAmount > 0 ? 'p' : ''">{{ (transaction.NetAmount).toLocaleString('en-gb', { minimumFractionDigits: 2 }) }}</div>
						<div class="date"><countdown :time="new Date(transaction.Date)"></countdown></div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_progress_bank",
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

	.p42_progress {
		.theme--bright &,
		&.theme--bright {
			&.p42_progress_bank {
				.transaction {
					border-bottom: 1px solid rgba($ui_colors_bright_shade_5,0.1);
					background: $ui_colors_bright_shade_0;
				}
			}
		}
		.theme--dark &,
		&.theme--dark {
			&.p42_progress_bank {
				.transaction {
					border-bottom: 1px solid rgba($ui_colors_dark_shade_5,0.1);
					background: $ui_colors_dark_shade_0;
				}
			}
		}

		&_bank {
			.app-frame {
				background-size: 80px;
				background-image: url(./../assets/background-pattern.svg);
			}
			.app-header {
				& > div {
					color: #FFF;
					background-image: linear-gradient(to right, #346133 0%, #559249 40%, #57964A 50%, #559249 60%, #346133 100%);
				}
				&--logo {
					height: 50px;
					background-image: url(./../assets/sh_logo.svg);
					background-position: center;
					background-repeat: no-repeat;
				}
				&--data {
					text-align: center;
					margin-top: 16px;
				}
			}
			.transaction {
				display: flex;
				flex-direction: row;
				padding: 0.5em 1em;
				&:last-child {
				border-bottom: 0;
				}
				.info {
					flex-grow: 1;
					.name {
						font-family: "SkyOS-SemiBold";
					}
				}
				.values {
					text-align: right;
					.value {
						font-family: 'Courier New', sans-serif;
						font-size: 1.2em;
						font-weight: bold;
						color: #C00;
						&.p {
							color: #57964A;
						}
					}
					.date {
						font-size: 0.8em;
					}
				}
			}
		}
	}
</style>