<template>
	<div class="contract_content" ref="contract_content" :class="[theme, { 'no-actions': (contract.State != 'Active' && contract.State != 'Saved') }]">
		<div class="card" :style="state.ui.color ? 'background-color:' + (this.$root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : ''">
			<scroll_view :horizontal="false" :offsets="{ top: 0, bottom: 10 }" :scroller_offset="{ top: 30, bottom: 20 }">
				<width_limiter size="tablet">
					<div class="infos-in narrow-sidebar-padding helper_status-margin" :style="state.ui.color ? 'background-color:' + (this.$root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : ''">
						<ContractDetailed :app="app" :defaultPage="'todos'" :showstate="true" :map_id="'p42_conduit_main'" :contract="contract" :templates="templates" :nobg="true"/>
					</div>
				</width_limiter>
			</scroll_view>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import ProgressXPComp from "@/sys/components/progress/xp_small.vue"
import ProgressMoralComp from "@/sys/components/progress/karma_small.vue"
import ContractMedia from "@/sys/components/contracts/contract_media.vue"
import ContractDetailed from "@/sys/components/contracts/contract_detailed.vue"
import RecomAircraft from "@/sys/components/contracts/contract_recom_aircraft.vue"
import ContractTodo from "@/sys/components/contracts/contract_todo.vue"

export default Vue.extend({
	name: "contract_content",
	props: ['app', 'theme', 'contract', 'templates'],
	components: {
		ProgressXPComp,
		ProgressMoralComp,
		ContractMedia,
		ContractTodo,
		ContractDetailed,
		RecomAircraft,
	},
	beforeMount() {
		this.initCard();
	},
	mounted() {
	},
	data() {
		return {
			index: 0,
			template: null,
			state: {
				ui: {
					colorIsDark: false,
					color: null,
					colorDark: null,
					requesting: false,
					countries: [],
					expireAt: new Date(),
					pullAt: new Date()
				}
			}
		}
	},
	methods: {
		initCard() {

			if(this.contract.ImageURL.length) {
				this.$root.$data.services.colorSeek.find(this.contract.ImageURL, (color :any) => {
					this.state.ui.color = color.color;
					this.state.ui.colorDark = color.colorDark;
					this.state.ui.colorIsDark = color.colorIsDark;
				});
			} else {
				this.state.ui.colorIsDark = false;
				this.state.ui.color = null;
				this.state.ui.colorDark = null;
			}

			this.state.ui.countries = [];
			this.contract.Situations.forEach((sit :any) => {
				if(sit.Airport){
					if(!this.state.ui.countries.includes(sit.Airport.Country)) {
						this.state.ui.countries.push(sit.Airport.Country);
					}
				}
			});

			if(this.state.ui.countries.length > 5) {
				const fullList = this.state.ui.countries;
				const spacing = (fullList.length / 4);
				this.state.ui.countries = [];
				for (let i = 0; i < 4; i++) {
					const country = fullList[Math.floor(i * spacing)];
					this.state.ui.countries.push(country);
				}
				this.state.ui.countries.push("+" + (fullList.length - this.state.ui.countries.length - 1));
				this.state.ui.countries.push(fullList[fullList.length - 1]);
			}

		},

		interactAction(ev: Event, interaction: any) {
			this.$root.$data.services.api.SendWS(
				"adventure:interaction",
				{
					ID: this.contract.ID,
					Link: interaction.UID,
					Verb: interaction.Verb,
					Data: {},
				}
			);
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'adventure': {
					this.state.ui.requesting = false;
					break;
				}
			}
		}
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.template = this.templates.find((x: any) => x.FileName == newValue.FileName);
					this.state.ui.expireAt = new Date(this.contract.ExpireAt);
					this.state.ui.pullAt = new Date(this.contract.PullAt);
					this.initCard();
				}
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';
$transition: cubic-bezier(.25,0,.14,1);
.contract_content {
	flex-grow: 1;
	display: flex;

	.theme--bright &,
	&.theme--bright {
		color: $ui_colors_bright_shade_5;
		.background {
			.blur {
				filter: blur(30px) brightness(1) contrast(3);
				.device-apple & {
					filter: blur(30px) brightness(1) contrast(1);
				}
			}
		}
		.infos-in {
			background: $ui_colors_bright_shade_2;
		}
	}

	.theme--dark &,
	&.theme--dark {
		color: $ui_colors_dark_shade_5;
		.background {
			.blur {
				filter: blur(30px) brightness(1.5) contrast(5);
				.device-apple & {
					filter: blur(30px) brightness(1.5) contrast(1);
				}
			}
		}
		.map-spacer {
			background: $ui_colors_dark_shade_2;
		}
		.infos-in {
			background: $ui_colors_dark_shade_2;
		}
	}

	.card {
		display: flex;
		flex-grow: 1;
		flex-direction: column;
		background-color: transparent;
		overflow: hidden;
		will-change: transform;
		transition: background 1s $transition;
	}

	.map-spacer {
		padding-top: 250px;
		mask-image: linear-gradient(to top, rgba(0, 0, 0, 1) 0, rgba(0, 0, 0, 0) 60px);
		transition: background 1s $transition, padding 1s $transition;
	}

	.infos-in {
		flex-grow: 1;
		transition: background 1s $transition;
		& > div {
			padding: 8px;
			padding-top: 0;
		}
	}

}
</style>