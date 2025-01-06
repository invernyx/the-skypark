<template>
	<div class="endcard" :class="'endcard-state-' + result.Data.Contract.State.toLowerCase()" v-if="result.Data.EndCard">
		<div class="endcard-background"></div>
		<div class="endcard-stack">
			<div class="endcard-stack-status">
				<div>
					<div class="endcard-stack-circle">
						<div class="endcard-stack-circle-completed" v-if="result.Data.Contract.State == 'Succeeded'"></div>
						<div class="endcard-stack-circle-failed" v-else></div>
						<div class="endcard-stack-state" v-if="result.Data.Contract.State == 'Succeeded'">Completed!</div>
						<div class="endcard-stack-state" v-else>Failed!</div>
					</div>
				</div>
				<div>
					<div class="endcard-stack-contract">
						<div class="endcard-stack-title" v-if="result.Data.Template.Name.length">{{ result.Data.Template.Name }}</div>
						<div class="endcard-stack-title" v-else>{{ result.Data.Contract.Route }}</div>
						<div class="endcard-stack-route" v-if="result.Data.Template.Name.length">{{ result.Data.Contract.Route }}</div>
					</div>
					<div class="endcard-stack-summary">{{ result.Data.Contract.EndSummary }}</div>
				</div>
			</div>
			<div v-if="$root.$data.config.ui.tier != 'discovery'">
				<ProgressModule_Bank v-if="state.progress.Bank" :progress="state.progress.Bank" :change="result.Data.EndCard.Reward" />
				<ProgressModule_XP v-if="state.progress.XP" :progress="state.progress.XP" :change="result.Data.EndCard.XP" />
				<ProgressModule_Karma v-if="state.progress.Karma" :progress="state.progress.Karma" :change="result.Data.EndCard.Karma" />
				<!--
				<div class="endcard-stack-data" v-if="state.progress.XP">
					<div class="key">XP</div>
					<div>
						<div class="value counting">{{ result.Data.EndCard.XP.toLocaleString('en-gb') }}</div>
						<div class="sub">{{ state.progress.XP.Balance.toLocaleString('en-gb') }}</div>
					</div>
				</div>
				-->
				<!--
				<div class="endcard-stack-data" v-if="state.progress.Bank">
					<div class="key">Payout</div>
					<div>
						<div class="value counting" :class="{ 'p': result.Data.EndCard.Reward > 0, 'n': result.Data.EndCard.Reward < 0 }">{{ (result.Data.EndCard.Reward >= 0 ? '+$' : '-$') + Math.abs(result.Data.EndCard.Reward).toLocaleString('en-gb', { minimumFractionDigits: 2 }) }}</div>
						<div class="sub">${{state.progress.Bank.Balance.toLocaleString('en-gb', { minimumFractionDigits: 2 }) }}</div>
					</div>
				</div>
				-->
				<!--
				<div class="endcard-stack-data" v-if="state.progress.Karma">
					<div class="key">Karma</div>
					<div>
						<div class="value counting" :class="{ 'p': result.Data.EndCard.Karma > 0, 'n': result.Data.EndCard.Karma < 0 }">{{ (result.Data.EndCard.Karma >= 0 ? '+' : '') +result.Data.EndCard.Karma }}</div>
						<div class="sub">{{ state.progress.Karma.Balance.toLocaleString('en-gb') }}</div>
					</div>
				</div>
				-->
			</div>
			<div class="endcard-stack-actions">
				<div class="columns columns_margined_half">
					<div class="column_narrow">
						<button_nav @click.native="close">Close</button_nav>
					</div>
					<div class="column">
						<button_nav @click.native="contracts" class="go">Browse Contracts</button_nav>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "endcard",
	props: ['result'],
	components: {
		ProgressModule_Bank: () => import("@/sys/components/progress/bank_compact.vue"),
		ProgressModule_XP: () => import("@/sys/components/progress/xp_compact.vue"),
		ProgressModule_Karma: () => import("@/sys/components/progress/karma_compact.vue"),
	},
	data() {
		return {
			state: {
				progress: {
					Reliability: null,
					Bank: null,
					Karma: null,
					XP: null,
				}
			}
		}
	},
	mounted() {
		if(!this.result.Data.EndCard) {
			this.$os.modalClose();
		}
		this.refreshData();
	},
	methods: {
		refreshData() {

			const p = this.$os.getState(['services','userProgress']);

			this.state.progress.Reliability = p.Reliability;
			this.state.progress.Bank = p.Bank;
			this.state.progress.Karma = p.Karma;
			this.state.progress.XP = p.XP;

		},
		close() {
			this.$os.modalClose();
			this.$os.removeNotificationFromID(this.result.UID);
		},
		progress() {
			this.$os.modalClose();
			this.$router.push({name: 'p42_progress'});
		},
		contracts() {
			this.$os.modalClose();
			this.$router.push({name: 'p42_contrax'});
		}
	},
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

.endcard {
	display: flex;
	align-items: center;
	justify-content: center;
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	color: #FFF;
	background: rgba(rgb(127, 127, 127), 0.5);
	backdrop-filter: blur(10px);
	&-background {
		position: absolute;
		top: 0;
		bottom: 0;
		right: 0;
		left: 0;
		background: #000;
		opacity: 0.8;
	}
	&-state {
		&-succeeded {
			.endcard-background {
				background: #030;
			}
		}
		&-failed {
			.endcard-background {
				background: #300;
			}
		}
	}
	&-stack {
		position: relative;
		z-index: 2;
		margin-top: $status-size;
		margin-bottom: $nav-size;
		&-status {
			margin-bottom: 2em;
			display: flex;
			flex-direction: column;
			justify-content: center;
			align-items: center;
			text-align: center;
		}
		&-circle {
			display: flex;
			align-items: center;
			position: relative;
			margin: 0 auto;
			border-radius: 4em;
			padding-right: 2em;
			background: rgba(0,0,0,0.4);
			&-completed {
				position: relative;
				width: 5em;
				height: 5em;
				background: #0F0;
				mask: url(../../../sys/assets/icons/state_mask_done.svg);
			}
			&-failed {
				position: relative;
				width: 5em;
				height: 5em;
				background: #F00;
				mask: url(../../../sys/assets/icons/state_mask_failed.svg);
			}
		}
		&-state {
			font-family: "SkyOS-SemiBold";
			font-size: 2em;
			margin: 0;
		}
		&-title {
			font-family: "SkyOS-SemiBold";
			font-size: 1.4em;
		}
		&-route {
			font-size: 1em;
		}
		&-contract {
			margin-top: 1em;
		}
		&-summary {
			margin-top: 1em;
		}
		&-actions {
			margin-top: 3em;
			& > div {
				max-width: 250px;
				margin: 0 auto;
			}
		}
	}
}

</style>

<style scoped>
/* https://css-tricks.com/animating-number-counters/ */
/*
@property --percent {
  syntax: "<number>";
  initial-value: 0;
  inherits: false;
}
@property --temp {
  syntax: "<number>";
  initial-value: 0;
  inherits: false;
}
@property --v1 {
  syntax: "<integer>";
  initial-value: 0;
  inherits: false;
}
@property --v2 {
  syntax: "<integer>";
  initial-value: 0;
  inherits: false;
}
*/
</style>