<template>
		<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ bottom: 15 }">
			<template v-slot:nav>
				<h2>Gameplay</h2>
			</template>
			<template v-slot:content>

				<div class="tier_mode_container" :class="'tier_mode_container_' + previewMode">
					<div class="tier_mode_container_backgrounds" ref="tier_mode_container_backgrounds">
						<div class="tier_mode_container_backgrounds_prospect" v-if="isDev"></div>
						<div class="tier_mode_container_backgrounds_discovery"></div>
						<div class="tier_mode_container_backgrounds_endeavour"></div>
						<div class="tier_mode_container_backgrounds_challenger"></div>
						<div class="tier_mode_container_backgrounds_enterprise"></div>
					</div>

					<div class="tier_mode_selector_container">
						<div class="tier_mode_selector_container_arrow tier_mode_selector_container_arrow_left" @click="previewOffset(-1)"></div>
						<div class="tier_mode_selector_container_arrow tier_mode_selector_container_arrow_right" @click="previewOffset(1)"></div>
						<div class="tier_mode_selector" ref="tier_mode_selector" @scroll="previewChange">
							<!--<div class="tier_mode_selector_prospect" data-type="prospect" v-if="isDev"></div>-->
							<div class="tier_mode_selector_discovery" data-type="discovery"></div>
							<div class="tier_mode_selector_endeavour" data-type="endeavour"></div>
							<!--<div class="tier_mode_selector_challenger" data-type="challenger"></div>-->
							<!--<div class="tier_mode_selector_enterprise" data-type="enterprise">enterprise</div>-->
						</div>
					</div>

					<div class="helper_edge_padding">

						<div class="columns columns_margined tier_mode">
							<div class="column">
								<div class="tier_mode_switch_container">
									<button_action v-if="tier == previewMode" :class="['tier_mode_switch', 'tier_mode_switch_' + previewMode, 'disabled']">Current Mode</button_action>
									<button_action v-else-if="tier == 'prospect'" :class="['tier_mode_switch', 'tier_mode_switch_' + previewMode]" @click.native="buy()">Buy Skypark</button_action>
									<button_action v-else-if="previewModeUnlocked || !$root.$data.state.services.api.connected" :class="['tier_mode_switch', 'tier_mode_switch_' + previewMode, { 'disabled': !$root.$data.state.services.api.connected }]" @click.native="setTier(previewMode)">Switch to {{ previewModes[previewMode].name }}</button_action>
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

					<div class="helper_edge_padding helper_nav-margin">

						<h2>Content</h2>
						<div class="columns columns_margined">
							<div class="column column_h-stretch">
								<toggle v-model="illicit" @modified="changeSetting('illicit', $event === true ? '1' : '0')" :disabled="!$root.$data.state.services.api.connected">Enable illicit content</toggle>
								<p class="notice">Coyote contracts include counterfeit, stolen &amp; illegal merchandise. This change will take effect the next time you restart the Skypark Transponder.</p>
							</div>
						</div>

						<h2>Life</h2>
						<div class="buttons_list shadowed">
							<button_listed icon="theme/reload" @click.native="resetLife" :class="{ 'disabled': !$root.$data.state.services.api.connected }">Reset this life</button_listed>
						</div>

					</div>
				</div>

			</template>
		</content_controls_stack>

</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';

export default Vue.extend({
	name: "p42_settings_tier",
	data() {
		return {
			illicit: this.$os.transponderSettings.illicit == '1',
			tier: this.$os.getConfig(['ui','tier']),
			isBeta: this.$os.isBeta,
			isDev: this.$os.isDev,
			previewMode: this.$os.getConfig(['ui','tier']),
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

		previewChange(ev :any) {
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
							if(Eljs.CalculateRank(this.$root.$data.state.services.userProgress.XP.Balance, 0).level >= this.previewModes[this.previewMode].level) {
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

		previewSet(mode :string) {
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

		previewOffset(change :number) {
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
								this.previewSet((selEl.childNodes[currentIndex + 1] as HTMLElement).dataset.type);
							} else {
								this.previewSet((selEl.childNodes[0] as HTMLElement).dataset.type);
							}
						} else {
							if(currentIndex > 0) {
								this.previewSet((selEl.childNodes[currentIndex - 1] as HTMLElement).dataset.type);
							} else {
								this.previewSet((selEl.childNodes[selEl.childNodes.length - 1] as HTMLElement).dataset.type);
							}
						}

						triggered = true;
						return;
					}
				}
			});
		},

		setTier(tier :string) {
			this.$root.$data.services.api.SendWS('transponder:set', {
				param: 'tier',
				value: tier,
			});
		},

		changeSetting(tag :string, state :string) {
			this.$root.$data.services.api.SendWS('transponder:set', {
				param: tag,
				value: state,
			});
		},

		resetLife() {
			this.$os.modalPush({
				type: 'ask',
				title: 'Are you sure you want to reset your life?',
				text: [
					"The Skypark is built in a way that allows you to move between karma and reliability extremes quickly. You don't have to reset to overwrite \"bad\" decisions.",
					"Karma: Flow between Coyote and ClearSky with a few key hops.",
					"Reliability: Demonstrate increased reliability as you would in the real world.",
					"This will reset everything and cannot be undone."
				],
				actions: {
					yes: 'Do it!',
					no: 'Cancel'
				},
				data: {
				},
				func: (state :boolean) => {
					if(state) {
						this.$root.$data.services.api.SendWS('transponder:resetlife', {});
					}
				}
			});
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.illicit = wsmsg.payload.set.illicit == '1';
							this.tier = wsmsg.payload.set.tier;
							this.previewSet(this.tier);
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
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	mounted() {
		this.previewSet(this.tier);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
});
</script>

<style lang="scss" scoped>
	@import '../../../../sys/scss/helpers.scss';
	@import '../../../../sys/scss/colors.scss';
	@import '../../../../sys/scss/mixins.scss';
	.p42_settings_tier {

		.theme--bright &,
		&.theme--bright {
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
		.theme--dark &,
		&.theme--dark {
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
						height: 800px;
						z-index: -1;
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