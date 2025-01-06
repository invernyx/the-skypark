<template>
	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ top: 30, bottom: 10 }">
		<template v-slot:content>
			<div class="helper_edge_padding_lateral helper_status-margin">
				<div class="progress-section">
					<div class="row" v-for="(row, rowIndex) in ui.progressSections" v-bind:key="rowIndex">
						<div class="column" v-for="(column, columnIndex) in row" v-bind:key="columnIndex">
							<ProgressModule_Bank v-if="column == 'Bank' && state.progress[column]" :progress="state.progress[column]" @mouseenter.native="setCaption('bank')" />
							<ProgressModule_XP v-if="column == 'XP' && state.progress[column]" :progress="state.progress[column]" @mouseenter.native="setCaption('xp')" />
							<ProgressModule_Karma v-if="column == 'Karma' && state.progress[column]" :progress="state.progress[column]" @mouseenter.native="setCaption('karma')" />
							<ProgressModule_Reliability v-if="column == 'Reliability' && state.progress[column]" :progress="state.progress[column]" :unlock="state.progress['ReliabilityUnlock'] > new Date()" @mouseenter.native="setCaption('reliability')" />
						</div>
					</div>
				</div>
			</div>
		</template>
	</content_controls_stack>

</template>

<script lang="ts">
import { AppInfo } from '@/sys/foundation/app_bundle';
import Vue from 'vue';

export default Vue.extend({
	name: "p42_progress_overview",
	components: {
		ProgressModule_XP: () => import("@/sys/components/progress/xp.vue"),
		ProgressModule_Karma: () => import("@/sys/components/progress/karma.vue"),
		ProgressModule_Reliability: () => import("@/sys/components/progress/reliability.vue"),
		ProgressModule_Bank: () => import("@/sys/components/progress/bank.vue"),
	},
	props: {
		inst: Object,
		appvue: Vue,
		app: AppInfo,
		appName: String
	},
	data() {
		return {
			codeCumul: "",
			codeTO: null as any,
			ui: {
				progressSections: [
					["Bank"],
					["XP"],
					["Karma"],
					["Reliability"]
				]
			},
			state: {
				progress: {
					ReliabilityUnlock: null,
					Reliability: null,
					Bank: null,
					Karma: null,
					XP: null,
				}
			}
		}
	},
	methods: {
		refreshData() {

			const p = this.$os.getState(['services','userProgress']);

			this.state.progress.ReliabilityUnlock = new Date(p.ReliabilityUnlock);
			this.state.progress.Reliability = p.Reliability;
			this.state.progress.Bank = p.Bank;
			this.state.progress.Karma = p.Karma;
			this.state.progress.XP = p.XP;

		},
		setCaption(d :string) {
			switch(d) {
				case 'xp': {
					this.$props.appvue.$emit('caption', 'Level/XP', "XP Stands for Experience Points - It is a measure of your experience as a pilot. \n\nXP always moves up as there is something to new to learn in every adventure. \n\nYou'll want to earn XP as quickly as you can so you level up and get more exciting contracts.");
					break;
				}
				case 'bank': {
					this.$props.appvue.$emit('caption', 'Bank Balance', "Money doesn't buy happiness, but I'd much rather cry in a bizjet. This is your current bank balance. \n\nOver time you'll have additional operational costs, so you'll want to keep a close eye on your finances. \n\nSee the \"Holdings\" app for more details.");
					break;
				}
				case 'karma': {
					this.$props.appvue.$emit('caption', 'Karma', "Karma moves in the direction of good or bad, depending on the types of Contracts you accept. \n\nYour Karma has a direct impact on what company contracts are available to you. Working for ClearSky moves Karma up; running for Coyote will do the opposite. \n\nNot seeing the contracts you enjoy? How's your Karma?");
					break;
				}
				case 'reliability': {
					this.$props.appvue.$emit('caption', 'Reliability', "Reliability scores are confidential, but if you fly into KMKC (and take Brigit out to lunch), you can see yours for a few hours. \n\nThis tracks how reliable you are when accepting contracts. If you're a punctual pilot, it goes up; if you continuously cancel or fail, it plummets. \n\nHigher scores mean better-pay and additional perks in the future.");
					break;
				}
			}
		},

		keyPressed(e: KeyboardEvent) {
			clearTimeout(this.codeTO);
			this.codeTO = setTimeout(() => {
				clearTimeout(this.codeTO);
				this.codeCumul = "";
				this.codeTO = null;
			}, 500);

			this.codeCumul += e.key;
			switch(this.codeCumul) {
				case 'rel42': {
					if(this.$os.isDev) {
						this.state.progress.ReliabilityUnlock = new Date(2042, 1, 1);
					}
					break;
				}
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'progress': {
					this.refreshData();
					break;
				}
			}
		},
	},
	mounted() {
		this.refreshData();
		document.addEventListener('keypress', this.keyPressed);
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
		document.removeEventListener('keypress', this.keyPressed);
	},
});
</script>

<style lang="scss">
	@import '../../../../sys/scss/colors.scss';

	.p42_progress {
		.progress {
			margin-bottom: 1.4em;
		}
	}
</style>