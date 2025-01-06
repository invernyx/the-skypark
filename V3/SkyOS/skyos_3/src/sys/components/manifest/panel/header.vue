<template>
	<div class="app-box shadowed small nooverflow header" :style="{ 'background-color': accent_right.combined ? 'rgb(' + accent_right.combined + ')' : null }" @click="header_click()">
		<div>
			<div class="loading">
				<div class="loading-label">
					<span></span>
				</div>
			</div>
			<div class="texture" :style="{'background-image': contract ? 'url(' + contract.image_url + ')' : null }"></div>
			<div class="content columns">
				<div class="column">

					<div class="background">
						<div class="image" :style="{'background-image': contract ? 'url(' + contract.image_url + ')' : null }"></div>
					</div>

					<div class="header-top" :style="{ 'text-shadow': accent.shadow }" :class="{ 'is-dark': accent.dark }">
						<div class="columns">

							<Company_badge class="column column_narrow bshadowed" :show_all="true" :style="{ '--box-shadow': accent.shadow }" :companies="contract ? contract.template.company : []" :operated_for="contract ? contract.operated_for : null"/>

							<div class="column">
								<div class="columns">
									<div class="column" v-if="contract">
										<div class="title" v-if="contract.title">{{ contract.title }}</div>
										<div class="title" v-else>Direct Flight</div>
									</div>
								</div>
								<div class="columns">
									<div class="column" v-if="contract">
										<expire class="expire" :contract="contract" v-if="contract.state != 'Succeeded' && contract.state != 'Failed'"/>
										<div v-else-if="contract.state == 'Succeeded'">Completed on <time_date :date="contract.completed_at"/></div>
									</div>
								</div>
							</div>

						</div>
					</div>

					<div class="header-bottom">

						<div class="flags">
							<flags v-for="(country, index) in countries" v-bind:key="index" :code="country.toLowerCase()" />
						</div>

					</div>

				</div>
				<div class="column column_narrow header-content" :class="{ 'is-dark': accent_right.dark }">

					<div class="header-data">

						<div class="columns columns_margined" :style="{ 'text-shadow': accent_right.shadow }">
							<div class="column">
								<data_stack label="Reward" class="end" v-if="contract">
									<span class="reward" v-if="contract.reward_bux != 0"><currency class="reward_bux" :amount="contract.reward_bux" :decimals="0"/></span>
									<span class="reward" v-else-if="contract.reward_karma != 0"><span class="amt">{{ contract.reward_karma > 0 ? '+' : '-' }} Karma</span></span>
									<span class="reward" v-else-if="contract.reward_reliability > 5"><span class="amt">+Reliability</span></span>
									<span class="reward" v-else-if="contract.reward_xp > 0"><span class="amt">+XP</span></span>
									<span class="reward" v-else><span class="amt">No reward</span></span>
								</data_stack>
								<data_stack label="Reward" class="end" v-else>----</data_stack>
							</div>
						</div>

						<div class="columns columns_margined" :style="{ 'text-shadow': accent_right.shadow }">
							<div class="column">
								<data_stack label="Total load" class="end h_edge_padding_top_half" v-if="contract">
									<weight :amount="contract.manifests.total_weight" :decimals="0" />
								</data_stack>
								<data_stack label="Total load" class="end h_edge_padding_top_half" v-else>----</data_stack>
							</div>
						</div>

						<div class="columns columns_margined">
							<div class="column" v-if="contract">
								<button_action class="go shadowed h_edge_margin_top btn_view">View manifest</button_action>
							</div>
							<div class="column" v-else>
								<button_action class="go shadowed h_edge_margin_top" >----</button_action>
							</div>
						</div>

					</div>

				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import { watch } from 'fs';

export default Vue.extend({
	props: {
		app: AppInfo,
		contract: Contract,
		accent: Object,
		accent_right: Object,
		countries: Array as () => Array<String>,
		app_panel_content :HTMLElement
	},
	data() {
		return {
			open: false,
			sid: this.app.vendor + '_' + this.app.ident
		}
	},
	methods: {

		header_click(state? :boolean) {
			const comp = this.$os.scrollView.get_entity(this.sid + '_manifest_contract');
			const scroll_offset = comp.SimpleBar.contentWrapperEl.scrollTop;
			const pTopStr = getComputedStyle(this.app_panel_content).getPropertyValue('--spacing-top');
			const pTopPx = parseInt(pTopStr.replace('px',''))
			if(scroll_offset < pTopPx - 200 || state) {
				this.$os.scrollView.scroll_to(this.sid + '_manifest_contract', 0, pTopPx - 60, 300);
			} else {
				this.$os.scrollView.scroll_to(this.sid + '_manifest_contract', 0, 0, 300);
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
		this.$os.eventsBus.Bus.on('os', this.listener_os);

	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	watch: {
		contract() {
			if(this.contract) {
				if(this.$route.params.expand) {
					this.header_click(true);
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.header {
	overflow: hidden;
	transition: background 1s ease-out, transform 0.1s cubic-bezier(.3,0,.24,1);
	will-change: transform;
	cursor: pointer;
	color: $ui_colors_dark_shade_0;
	//@include shadowed_text($ui_colors_bright_shade_2);
	&:hover {
		transform: scale(1.01);
	}

	.loading & {
		transition: background 0.1s ease-out, transform 0.1s cubic-bezier(.3,0,.24,1);
		.content {
			opacity: 0;
			transition: none;
		}
		.texture {
			opacity: 0;
			transition: none;
		}
		.loading {
			opacity: 1;
			transition: opacity 0.4s 0.1s ease-out;
		}
	}

	.texture,
	.content {
		transition: opacity 1s ease-out;
	}

	.is-open & {
		&:hover {
			transform: scale(1);
		}

		.btn_view {
			opacity: 0;
		}
	}

	& > div {
		transition: 0.5s ease-out;
	}

	.header-top {
		position: absolute;
		left: 0;
		top: 0;
		z-index: 2;
		padding: 14px 16px;
		margin-right: 40px;
	}

	.header-bottom {
		position: absolute;
		left: 0;
		bottom: 0;
		z-index: 2;
		padding: 16px;
	}

	.header-data {
		padding: 14px 16px;
		transform: scale(1);
		transition: opacity 0.2s ease-out, transform 0.2s ease-out;
	}

	.header-content {
		margin-left: -50px;
		opacity: 1;
		color: $ui_colors_bright_shade_5;
	}

	.is-dark {
		color: $ui_colors_dark_shade_5;
	}

	.flags {
		display: flex;
		font-size: 18px;
		& > span {
			margin-right: 4px;
			border-radius: 4px;
			border: 1px solid $ui_colors_dark_shade_2;
		}
	}

	.title {
		font-size: 16px;
		font-family: "SkyOS-Bold";
		line-height: 1.2em;
		margin-bottom: 0;
		margin-right: 8px;
	}

	.background {
		position: relative;
		background-color: rgba(0,0,0,0.1);
		height: 100%;
		border-radius: 8px;
		mask-image: linear-gradient(to left, rgba(#000, 0) 0, rgba(#000, 1) 100px);
		.image {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 8px;
			background-size: cover;
			background-position: center;
		}
	}

	.texture {
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		transform: scale(1);
		opacity: 0.6;
		background-size: cover;
		background-position: center;
		filter: blur(20px) brightness(1) contrast(1);
		will-change: transform, filter;
	}

	.loading {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		font-size: 19px;
		z-index: 10;
		display: flex;
		opacity: 0;
		justify-content: center;
		align-items: center;
		font-family: "SkyOS-Bold";
		pointer-events: none;
	}

}

</style>