<template>
	<scroll_view :scroller_offset="{ top: 36, bottom: 30 }">
		<div class="app-panel-wrap">
			<div class="app-panel-content">
				<div class="app-panel-hit">

					<h2>Game Mode</h2>
					<div class="app-box shadowed-deep nooverflow">
						<div>
							<div class="tier_mode_container" :class="'tier_mode_container_' + previewMode">
								<div class="tier_mode_container_backgrounds" ref="tier_mode_container_backgrounds">
									<div class="tier_mode_container_backgrounds_prospect" v-if="isDev"></div>
									<div class="tier_mode_container_backgrounds_discovery"></div>
									<div class="tier_mode_container_backgrounds_endeavour"></div>
									<div class="tier_mode_container_backgrounds_challenger"></div>
									<div class="tier_mode_container_backgrounds_enterprise"></div>
								</div>

								<div class="tier_mode_selector_container">
									<div class="tier_mode_selector_container_arrow tier_mode_selector_container_arrow_left" @click="preview_offset(-1)"></div>
									<div class="tier_mode_selector_container_arrow tier_mode_selector_container_arrow_right" @click="preview_offset(1)"></div>
									<div class="tier_mode_selector" ref="tier_mode_selector" @scroll="preview_change">
										<!--<div class="tier_mode_selector_prospect" data-type="prospect" v-if="isDev"></div>-->
										<div class="tier_mode_selector_discovery" data-type="discovery"></div>
										<div class="tier_mode_selector_endeavour" data-type="endeavour"></div>
										<!--<div class="tier_mode_selector_challenger" data-type="challenger"></div>-->
										<!--<div class="tier_mode_selector_enterprise" data-type="enterprise">enterprise</div>-->
									</div>
								</div>

								<div class="h_edge_padding">

									<div class="columns columns_margined tier_mode h_edge_padding">
										<div class="column">
											<div class="tier_mode_switch_container">
												<button_action v-if="tier == previewMode" :class="['tier_mode_switch', 'tier_mode_switch_' + previewMode, 'disabled']">Current Mode</button_action>
												<button_action v-else-if="tier == 'prospect'" :class="['tier_mode_switch', 'tier_mode_switch_' + previewMode]" @click.native="buy()">Buy Skypark</button_action>
												<button_action v-else-if="previewModeUnlocked || !has_transponder" :class="['tier_mode_switch', 'tier_mode_switch_' + previewMode, { 'disabled': !has_transponder }]" @click.native="set_tier(previewMode)">Switch to {{ previewModes[previewMode].name }}</button_action>
												<button_action v-else :class="['tier_mode_switch', 'tier_mode_switch_' + previewMode, 'disabled']">Requires Level {{ previewModes[previewMode].level }}</button_action>
											</div>
											<p>{{ previewModes[previewMode] ? previewModes[previewMode].description : 'Mode Unavailable' }}</p>
											<div class="tier_mode_features_container">
												<div class="tier_mode_features" v-if="previewModes[previewMode]">
													<div class="tier_mode_feature" v-for="(feature, index) in features" v-bind:key="index">
														<div class="tier_mode_feature_state" :class="{ 'active': previewModes[previewMode].features[index] == 1 }"></div>
														<div class="tier_mode_feature_label">{{ feature.title }}</div>
													</div>
												</div>
											</div>
										</div>
									</div>

								</div>
							</div>
						</div>
					</div>

					<h2>Content</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="columns columns_margined">
								<div class="column column_h-stretch">
									<toggle v-model="illicit" @modified="set_illicit" :disabled="!has_transponder" notice="Coyote contracts include counterfeit, stolen &amp; illegal merchandise. This change will take effect the next time you restart the Skypark Transponder.">Enable illicit content</toggle>
								</div>
							</div>
						</div>
					</div>

					<h2>Payload</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="columns columns_margined">
								<div class="column column_top column_narrow column_3">
									<p class="h_no-margin"><strong>Load/Unload duration</strong></p>
								</div>
								<div class="column">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="load_time" @input="set_load_time" :disabled="!has_transponder">
											<option :value="'minutes'">Minutes</option>
											<option :value="'seconds'">Seconds</option>
											<option :value="'instant'">Instant</option>
										</selector>
									</div>
									<p class="notice h_no-margin h_edge_padding_top_half">If your time is valuable, here's a way to make loading and unloading of your aircraft quicker.</p>
								</div>
							</div>
						</div>
					</div>


				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			load_time: this.$os.userConfig.get(['ui','load_time']),
			illicit: this.$os.userConfig.get(['ui','illicit']) == '1',
			tier: this.$os.userConfig.get(['ui','tier']),
			isBeta: this.$os.system.isBeta,
			isDev: this.$os.system.isDev,
			previewMode: this.$os.userConfig.get(['ui','tier']),
			previewIndex: 0,
			previewModes: {
				//prospect: {
				//	name: "Prospect",
				//	description: "Best for pilots that prefer discovery without the challenges of a game or economy. Explore the world without restrictions. Great for content creators!",
				//	features: [1,1,0,0,0,0,0],
				//	level: 0,
				//},
				discovery: {
					name: "Discovery",
					description: "Best for pilots that prefer discovery without the challenges of a game or economy. Explore the world without restrictions. Great for content creators!",
					features: [1,1,0,0,0,0,0],
					level: 3,
				},
				endeavour: {
					name: "Endeavour",
					description: "Ideal for pilots that prefer the challenges of a game and economy. Your decisions play an active part in what content becomes available and when.",
					features: [0,1,1,1,1,1,0],
					level: 0,
				}
			},
			previewModeUnlocked: true,
			features: [
				{
					title: "All contracts and content unlocked",
				},
				{
					title: "Contract expiration timers",
				},
				{
					title: "XP, Karma & reliability requirements",
				},
				{
					title: "Financial and progression apps",
				},
				{
					title: "Finances, penalties & fines",
				},
				{
					title: "Aircraft fleet management (soon)",
				},
				{
					title: "Spreadsheet interface",
				},
			]
		}
	},
	methods: {

		buy() {
			window.open("https://orbxdirect.com/product/p42-the-skypark", "_blank");
		},

		preview_change(ev :any) {
			const currentWidth = ev.target.offsetWidth;
			const currentScroll = ev.target.scrollLeft;
			let cumulOffset = 0;
			let triggered = false;
			ev.target.childNodes.forEach((element, index) => {
				if(!triggered) {
					cumulOffset += element.offsetWidth;
					if(cumulOffset - (currentWidth / 2) > currentScroll) {
						triggered = true;
						if(this.previewMode != element.dataset.type) {
							this.previewMode = element.dataset.type;
							this.previewIndex = index;
							if(Eljs.CalculateRank(this.$os.userConfig.get(['progress', 'xp']), 0).current.level >= this.previewModes[this.previewMode].level) {
								this.previewModeUnlocked = true;
							} else {
								this.previewModeUnlocked = false;
							}
						}
						return;
					}
				}
			});
		},

		preview_set(mode :string) {
			const bgEl =(this.$refs.tier_mode_container_backgrounds as HTMLElement);
			const selEl = (this.$refs.tier_mode_selector as HTMLElement);
			if(selEl && bgEl) {
				selEl.childNodes.forEach(element => {
					const El = (element as HTMLElement);
					if(El.dataset.type == mode) {
						selEl.scrollLeft = El.offsetLeft;
					}
				});

				if(!selEl.classList.contains("ready")) {
					setTimeout(() => {
						selEl.classList.add("ready");
						bgEl.classList.add("ready");
					}, 300);
				}
			}
		},

		preview_offset(change :number) {
			const selEl = (this.$refs.tier_mode_selector as HTMLElement);
			const currentWidth = selEl.offsetWidth;
			const currentScroll = selEl.scrollLeft;
			let currentIndex = 0;
			let cumulOffset = 0;
			let triggered = false;
			selEl.childNodes.forEach((element, index) => {
				if(!triggered) {
					cumulOffset += (element as HTMLElement).offsetWidth;
					if(cumulOffset - (currentWidth / 2) > currentScroll) {
						currentIndex = index;

						if(change > 0) {
							if(selEl.childNodes.length - 1 > index) {
								this.preview_set((selEl.childNodes[currentIndex + 1] as HTMLElement).dataset.type);
							} else {
								this.preview_set((selEl.childNodes[0] as HTMLElement).dataset.type);
							}
						} else {
							if(currentIndex > 0) {
								this.preview_set((selEl.childNodes[currentIndex - 1] as HTMLElement).dataset.type);
							} else {
								this.preview_set((selEl.childNodes[selEl.childNodes.length - 1] as HTMLElement).dataset.type);
							}
						}

						triggered = true;
						return;
					}
				}
			});
		},

		set_tier(tier :string) {
			this.$os.api.send_ws('transponder:set', {
				param: 'tier',
				value: tier,
			});
		},

		set_load_time(state :boolean) {
			this.load_time = state;
			this.$os.userConfig.set(['ui','load_time'], state);
			this.send_transponder('load_time', state === true ? '1' : '0')
		},

		set_illicit(state :boolean) {
			this.send_transponder('illicit', state === true ? '1' : '0')
		},

		send_transponder(tag :string, state :string) {
			this.$os.api.send_ws('transponder:set', {
				param: tag,
				value: state,
			});
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					break;
				}
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.illicit = wsmsg.payload.set.illicit == '1';
							this.$os.userConfig.set(['ui','illicit'], this.illicit);

							this.tier = wsmsg.payload.set.tier;
							this.$os.userConfig.set(['ui','tier'], this.tier);
							this.preview_set(this.tier);

							if(wsmsg.payload.dev) {
								this.isDev = true;
							}
							if(wsmsg.payload.beta) {
								this.isBeta = true;
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
		this.$os.system.set_cover(true);
		this.preview_set(this.tier);
	},
	created() {
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
	}
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/helpers.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';

	.app-panel-content {
		max-width: 390px;

		.theme--bright & {
			.tier_mode {
				&_container {
					&_backgrounds {
						&_prospect {
							background: linear-gradient(135deg, lighten(#8C8C8C, 10%), #85008C);
						}
						&_discovery {
							background: linear-gradient(135deg, #81CBE8, #73A27B);
						}
						&_endeavour {
							background: linear-gradient(135deg, lighten(#D72E15, 20%), #358BB9);
						}
						&_challenger {
							background: linear-gradient(135deg, #00B2E6, #E8AF3C);
						}
						&_enterprise {
							background: linear-gradient(135deg, #73A27B, #81CBE8);
						}
					}
				}
				&_switch {
					border-color: $ui_colors_bright_shade_1;
					color: $ui_colors_bright_shade_1;
					&_prospect {
						background: linear-gradient(135deg, #85008C, #8C8C8C);
					}
					&_discovery {
						background: linear-gradient(135deg, darken(#81CBE8, 20%), darken(#73A27B, 20%));
					}
					&_endeavour {
						background: linear-gradient(135deg, #D72E15, darken(#358BB9, 20%));
					}
					&_challenger {
						background: linear-gradient(135deg, lighten(#00B2E6, 20%), darken(#E8AF3C, 20%));
					}
					&_enterprise {
						background: linear-gradient(135deg, lighten(#73A27B, 20%), darken(#81CBE8, 20%));
					}
					&_container {
						&::before,
						&::after {
							border-top-color: $ui_colors_bright_shade_1;
						}
					}
				}
				&_feature {
					&_state {
						border-color: $ui_colors_bright_shade_4;
						&.active {
							border-color: $ui_colors_bright_button_go;
							&::after {
								background: $ui_colors_bright_button_go;
							}
						}
						&::after {
							background: $ui_colors_bright_shade_4;
						}
					}
				}
			}
		}
		.theme--dark & {
			.tier_mode {
				&_container {
					&_backgrounds {
						&_prospect {
							background: linear-gradient(135deg, darken(#8C8C8C, 10%), #85008C);
						}
						&_discovery {
							background: linear-gradient(135deg, darken(#81CBE8, 20%), darken(#73A27B, 20%));
						}
						&_endeavour {
							background: linear-gradient(135deg, darken(#D72E15, 20%), darken(#358BB9, 20%));
						}
						&_challenger {
							background: linear-gradient(135deg, darken(#00B2E6, 20%), darken(#E8AF3C, 20%));
						}
						&_enterprise {
							background: linear-gradient(135deg, darken(#73A27B, 20%), darken(#81CBE8, 20%));
						}
					}
				}
				&_switch {
					border-color: $ui_colors_bright_shade_1;
					color: $ui_colors_bright_shade_1;
					&_prospect {
						background: linear-gradient(135deg, #85008C, #8C8C8C);
					}
					&_discovery {
						background: linear-gradient(135deg, darken(#81CBE8, 20%), darken(#73A27B, 20%));
					}
					&_endeavour {
						background: linear-gradient(135deg, #D72E15, 20%, darken(#70797e, 20%));
					}
					&_challenger {
						background: linear-gradient(135deg, lighten(#00B2E6, 20%), darken(#E8AF3C, 20%));
					}
					&_enterprise {
						background: linear-gradient(135deg, lighten(#73A27B, 20%), darken(#81CBE8, 20%));
					}
					&_container {
						&::before,
						&::after {
							border-top-color: $ui_colors_bright_shade_1;
						}
					}
				}
				&_feature {
					&_state {
						border-color: $ui_colors_dark_shade_4;
						&.active {
							border-color: $ui_colors_dark_button_go;
							&::after {
								background: $ui_colors_dark_button_go;
							}
						}
						&::after {
							background: $ui_colors_dark_shade_4;
						}
					}
				}
			}
		}

		.tier_mode {
			&_container {

				&_backgrounds {
					&.ready {
						& > div {
							transition: opacity 0.5s;
							transition-timing-function: ease-out;
						}
					}
					& > div {
						position: absolute;
						top: 0;
						left: 0;
						right: 0;
						bottom: 0;
						border-radius: 10px;
						opacity: 0;
						mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) 20%, rgba(0, 0, 0, 0) 100%);
					}
				}
				&_prospect {
					.tier_mode_container_backgrounds_prospect {
						opacity: 1;
						transition-timing-function: ease-in;
					}
				}
				&_discovery {
					.tier_mode_container_backgrounds_discovery {
						opacity: 1;
						transition-timing-function: ease-in;
					}
				}
				&_endeavour {
					.tier_mode_container_backgrounds_endeavour {
						opacity: 1;
						transition-timing-function: ease-in;
					}
				}
				&_challenger {
					.tier_mode_container_backgrounds_challenger {
						opacity: 1;
						transition-timing-function: ease-in;
					}
				}
				&_enterprise {
					.tier_mode_container_backgrounds_enterprise {
						opacity: 1;
						transition-timing-function: ease-in;
					}
				}
			}
			&_selector {
				display: flex;
				scroll-snap-type: x mandatory;
				overflow-x: scroll;
				padding-top: 10px;
				&.ready {
					scroll-behavior: smooth;
				}
				& > div {
					display: flex;
					align-items: center;
					justify-content: center;
					scroll-snap-stop: normal;
					scroll-snap-align: start;
					width: 100%;
					flex-shrink: 0;
					height: 200px;
					background-size: contain;
					background-position: center;
					background-repeat: no-repeat;
				}
				&_container {
					position: relative;
					&_arrow {
						position: absolute;
						top: 50%;
						transform: translateY(-50%);
						width: 32px;
						height: 32px;
						border-radius: 50%;
						background-color: #222;
						background-size: 17px;
						background-position: center;
						background-repeat: no-repeat;
						will-change: transform;
						transition: transform 1s cubic-bezier(0, 1, 0.15, 1);
						cursor: pointer;
						&:hover {
							transform: translateY(-50%) scale(1.1);
						}
						&_left {
							left: $edge-margin;
							background-image: url(../../../../sys/assets/icons/bright/arrow_left.svg);
						}
						&_right {
							right: $edge-margin;
							background-image: url(../../../../sys/assets/icons/bright/arrow_right.svg);
						}
					}
				}
				&_prospect {
					//background-image: url(../../../../sys/assets/icons/tier/prospect.svg);
				}
				&_discovery {
					background-image: url(../../../../sys/assets/icons/tier/discovery.svg);
				}
				&_challenger {
					background-image: url(../../../../sys/assets/icons/tier/challenger.svg);
				}
				&_endeavour {
					background-image: url(../../../../sys/assets/icons/tier/endeavour.svg);
				}
				&_enterprise {
					//background-image: url(../../../../sys/assets/icons/tier/discovery.svg);
				}
			}
			&_switch {
				position: relative;
				color: $ui_colors_bright_shade_5;
				border: 2px solid transparent;
				border-radius: 30px;
				will-change: transform;
				&:hover {
					transform: scale(1.05);
				}
				&_container {
					display: flex;
					flex-direction: row;
					align-items: center;
					justify-content: space-between;
					&::before,
					&::after {
						display: block;
						content: "";
						border-top: 2px solid transparent;
						flex-grow: 1;
						opacity: 0.5;
					}
					.button_action  {
						margin: 0 15px;
						padding: 0.5em 1em;
						&:not(.disabled) {
							&::after {
								display: inline-block;
								content: "";
								background-image: url(../../../../sys/assets/framework/bright/arrow_right.svg);
								background-size: contain;
								background-repeat: no-repeat;
								background-position: center;
								width: 1em;
								height: 1em;
								margin-left: 0.5em;
								margin-right: -0.25em;
							}
						}
					}
				}
			}
			&_feature {
				display: flex;
				align-items: center;
				margin-bottom: 10px;
				&_state {
					position: relative;
					width: 21px;
					height: 21px;
					border-radius: 50%;
					margin-right: 10px;
					background: transparent;
					border: 1px solid transparent;
					transition: border 0.3s ease-out;
					&.active {
						&::after {
							mask: url(../../../../sys/assets/icons/state_mask_done.svg);
						}
					}
					&::after {
						content: '';
						display: block;
						position: absolute;
						top: 0;
						right: 0;
						bottom: 0;
						left: 0;
						border-radius: 50%;
						mask: url(../../../../sys/assets/icons/state_mask_failed.svg);
						transition: background 0.3s ease-out;
					}
				}
			}
		}
	}
</style>