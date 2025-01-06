<template>
	<div class="app-box shadowed small nooverflow content" v-if="contract">

		<div class="background-gradient" :style="{ 'background-image': accent_right.combined ? 'linear-gradient(to bottom, rgba(' + accent_right.combined + ',0.3), rgba(' + accent_right.combined + ',0)' : null }"></div>

		<div class="background-layer h_edge_margin">

			<div class="columns columns_reverse columns_margined columns_break_1000">
				<div class="column column_2">

					<div class="shaded">
						<div class="actions">
							<Controls :contract="contract" :app="app" />

							<div class="restrictions" v-if="contract.limits.length">
								<ContractLimits :contract="contract" :summary="true"/>
							</div>
						</div>
						<div>
							<ContractProfits :contract="contract"/>
							<collapser :preload="true" :default="false" :state="fees_visible || contract.state == 'Succeeded' || contract.state == 'Failed'">
								<template v-slot:content>
									<Invoices class="h_edge_margin_top"  :contract="contract"  :default_page="contract.state != 'Succeeded' && contract.state != 'Failed' ? 'estimates' : 'paid'" />
								</template>
							</collapser>
							<button_action class="small h_edge_margin_top" @click.native="fees_visible = !fees_visible" v-if="contract.state != 'Succeeded' && contract.state != 'Failed'">{{ fees_visible ? 'Hide Fees' : 'View Fees' }}</button_action>
						</div>
					</div>

					<!--
					<div class="shaded h_edge_margin_top"  v-if="contract.operated_for">
						<time_date :date="contract.operated_for[5]"/>
						{{ contract.operated_for }}
					</div>-->

					<div class="shaded h_edge_margin_top" v-if="contract.manifests ? (contract.manifests.max_pax_seats > 0 || contract.manifests.total_weight > 0 || contract.manifests.total_pax_percent > 0 || contract.manifests.total_cargo_percent > 0) : false">
						<Manifest :contract="contract" :humans="humans" :app="app" />
					</div>

					<div class="shaded h_edge_margin_top" v-if="contract.state != 'Succeeded' && contract.state != 'Failed'">
						<SliderSmallXP :gain="contract.reward_xp" />
						<SliderSmallReliability class="h_edge_margin_top" :gain="contract.reward_reliability"/>
						<SliderSmallKarma class="h_edge_margin_top" :range="contract.template.requires_karma" :gain="contract.reward_karma" />
					</div>

					<div class="media h_edge_margin_top" v-if="contract.media_link.length">
						<ContractMedia :contract="contract" v-if="open"/>
					</div>

				</div>
				<div class="column column_2">

					<div class="aircraft h_break_1000_edge_margin_top" v-if=" contract.state == 'Active'">
						<data_stack class="center small text-wrap" label="Aircraft used">
							{{ contract.aircraft_used.at(-1) }}
						</data_stack>
					</div>
					<div class="aircraft h_break_1000_edge_margin_top" v-else-if="contract.state != 'Succeeded' && contract.state != 'Failed'">
						<RecomAircraft :contract="contract"/>
					</div>

					<div class="columns columns_margined h_break_800_edge_margin_top">
						<div class="column">
							<data_stack class="center h_edge_margin_top" label="Total Distance">
								<distance :amount="contract.distance" :decimals="0"/>
							</data_stack>
						</div>
						<div class="column">
							<data_stack class="center h_edge_margin_top" label="Length" v-if="contract.duration_range[0] != contract.duration_range[1]">
								<duration :time="contract.duration_range[0]" :decimals="1"/>&nbsp;-&nbsp;<duration :time="contract.duration_range[1]" :decimals="1"/>
							</data_stack>
							<data_stack class="center h_edge_margin_top" label="Length" v-else>
								<duration :time="contract.duration_range[0]" :decimals="1"/>
							</data_stack>
						</div>
						<!--
						<div class="column">
							<data_stack class="center h_edge_margin_top" label="High. Obstacle">
								<height :amount="Math.max.apply(Math, (contract.topo))" :decimals="0"/>
							</data_stack>
						</div>
						-->
					</div>

					<div class="description h_edge_margin_top" v-if="contract.description_long.length">{{ contract.description_long }}</div>
					<div class="description h_edge_margin_top" v-else>{{ contract.description }}</div>

					<div class="todo h_edge_margin_top">
						<ToDos :contract="contract" v-if="open" />
					</div>

				</div>
			</div>

		</div>

		<div class="h_edge_margin text-center">
			<p class="notice">{{ contract.file_name }} <span class="dot"/> {{ contract.route }} <span class="dot"/> {{ contract.route_code.length ? contract.route_code : "no-route-code" }} <span class="dot"/> {{ contract.template.template_code.length ? '#' + contract.template.template_code : "no-template-code" }}</p>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import Invoices from "@/sys/components/contracts/panel/child/invoices.vue"
import ContractLimits from "@/sys/components/contracts/contract_limits.vue"
import RecomAircraft from "@/sys/components/contracts/contract_recom_aircraft.vue"
import ContractMedia from "@/sys/components/contracts/contract_media.vue"
import ContractProfits from "@/sys/components/contracts/contract_profits.vue"
import SliderSmallKarma from "@/sys/components/progress/slider_karma.vue"
import SliderSmallXP from "@/sys/components/progress/slider_xp.vue"
import SliderSmallReliability from "@/sys/components/progress/slider_reliability.vue"
import Controls from "./child/controls.vue";
import ToDos from "./child/todo.vue"
import Manifest from "./child/manifest.vue"

export default Vue.extend({
	props: {
		app: AppInfo,
		contract: Contract,
		accent_right: Object,
		humans :Array
	},
	components: {
		RecomAircraft,
		ContractMedia,
		ContractProfits,
		Manifest,
		Invoices,
		ContractLimits,
		SliderSmallKarma,
		SliderSmallXP,
		SliderSmallReliability,
		Controls,
		ToDos,
	},
	data() {
		return {
			open: false,
			sim_live: this.$os.simulator.live,
			sid: this.app.vendor + '_' + this.app.ident,
			fees_visible: false,
		}
	},
	methods: {

		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.sim_live = wsmsg.payload;
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'uncover': {
					this.open = wsmsg.payload;
					break;
				}
				case 'covered': {
					this.open = wsmsg.payload;
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.content {
	opacity: 0;
	pointer-events: none;
	transition: opacity 0.2s ease-out;

	.is-open & {
		opacity: 1;
		pointer-events: all;
	}

	.loading & {
		opacity: 0;
		pointer-events: none;
	}

	.background-gradient {
		position: absolute;
		top: 0;
		right: 0;
		left: 0;
		height: 300px;
	}

	.background-layer {
		position: relative;
		z-index: 2;
	}

	.aircraft {
		white-space: nowrap;
	}

	.restrictions {
		//font-size: 12px;
		//border-radius: 8px;
		//padding: 6px 8px;
		//border: 2px solid transparent;
		//border-top: none;
		//border-bottom: none;
		.limits {
			margin-top: 16px;
		}
	}

	.description {
		font-size: 16px;
		line-height: 1.4em;
		white-space: pre-wrap;
	}

	.shaded {
		border-radius: 20px;
		padding: 12px;
	}

	.theme--bright &,
	&.theme--bright {
		.shaded {
			background: $ui_colors_bright_shade_0;
			//box-shadow: 0 3px 8px rgba($ui_colors_bright_shade_5, 0.2);
			@include shadowed_shallow($ui_colors_bright_shade_5);
		}

		//.restrictions {
		//	background-color: rgba($ui_colors_bright_shade_0, 0.3);
		//	border-color: rgba($ui_colors_bright_shade_0, 0.3);
		//}

	}

	.theme--dark &,
	&.theme--dark {
		.shaded {
			background: rgba($ui_colors_dark_shade_0, 0.5);
		}

		//.restrictions {
		//	background-color: rgba($ui_colors_dark_shade_0, 0.1);
		//	border-color: rgba($ui_colors_dark_shade_5, 0.8);
		//}
	}

}

</style>