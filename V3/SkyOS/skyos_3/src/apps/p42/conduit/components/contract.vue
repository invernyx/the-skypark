<template>
	<div class="contract" :class="{ 'selected': selected ? selected.id == contract.id : false, 'unavailable': contract.expired, 'expired': contract.expired }" @click="select()" :data-contract="contract.id" :data-scrollvisibility="'hidden'">

		<div class="background" :class="{ 'loading': !color_bright }" v-if="contract.image_url ? contract.image_url.length : false" :style="{ 'background-color': background_color }">
			<!--<div :style="'background-image: url(' + contract.image_url + ')'"></div>-->
		</div>

		<div class="content">
			<div class="columns header">

				<Company_badge class="column column_narrow" :companies="contract.template.company" :operated_for="contract.operated_for"/>
				<div class="column">
					<div class="columns">
						<div class="column">
							<div class="title" v-if="contract.title">{{ contract.title }}</div>
							<div class="title" v-else>Direct Flight</div>
						</div>
					</div>
					<div class="columns">
						<div class="column">
							<expire class="expire" :contract="contract"/>
						</div>
					</div>
				</div>
				<div class="column column_narrow bookmarks" v-if="contract.state != 'Listed'">
					<div class="bookmark info" :class="['bookmark-' + contract.state.toLowerCase()]">
						<div></div>
						<div></div>
					</div>
				</div>
			</div>
			<div class="body">
				<div class="columns">
					<div class="column">
						<span class="description">{{ contract.description }}</span>
					</div>
				</div>
				<div class="columns">
					<div class="column">
						<div>
							<distance class="distance" :amount="contract.distance" :decimals="0"/>
						</div>
					</div>
					<div class="column column_narrow">
						<span class="route">{{ contract.route }}</span>
					</div>
				</div>
				<div class="columns">
					<div class="column column_narrow column_justify_center">
						<div class="aircraft" v-if="contract.template.aircraft_restriction_label">In <strong>{{ contract.template.aircraft_restriction_label }}</strong></div>
						<div class="aircraft" v-else><RecomAircraft :contract="contract" :small="true" /></div>
					</div>
					<div></div>
					<div class="column column_narrow column_justify_end">
						<span class="reward" v-if="contract.reward_bux != 0"><currency class="reward_bux" :amount="contract.reward_bux" :decimals="0"/></span>
						<span class="reward" v-else-if="contract.reward_karma != 0"><span class="amt">{{ contract.reward_karma > 0 ? '+' : '-' }} Karma</span></span>
						<span class="reward" v-else-if="contract.reward_reliability > 5"><span class="amt">+Reliability</span></span>
						<span class="reward" v-else-if="contract.reward_xp > 0"><span class="amt">+XP</span></span>
						<span class="reward" v-else><span class="amt">No reward</span></span>
					</div>
				</div>
			</div>
			<div class="elevation">
				<svg width="100%" height="100%" viewBox="0 0 100 100" preserveAspectRatio="none">
					<linearGradient :id="'p42_contrax_gradient_' + contract.id" x2="0" y2="1">
						<stop offset="0%" />
						<stop offset="100%" />
					</linearGradient>
					<polygon :fill="'url(#p42_contrax_gradient_' + contract.id + ')'" class="poly" :points="elevation_poly"></polygon>
				</svg>
			</div>
		</div>

		<div class="unavailable-label" v-if="contract.expired">
			<div></div>
			<div>No longer available</div>
		</div>

		<!--
		<div class="data">
			<collapser :default="false" :state="selected">
				<template v-slot:content>
					<div class="content-sub">

						<div class="columns">
							<div class="column">
							</div>
							<div class="column column_narrow">
								<button_action class="action_share" v-if="contract.route_code" @click.native="$emit('route_code')">{{ contract.route_code }}</button_action>
							</div>
						</div>

					</div>
				</template>
			</collapser>
		</div>
		-->

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Contract from "@/sys/classes/contracts/contract";
import Template from "@/sys/classes/contracts/template";
import Eljs from '@/sys/libraries/elem';
import RecomAircraft from "@/sys/components/contracts/contract_recom_aircraft.vue"
import Company_badge from '@/sys/components/company_badge.vue';

export default Vue.extend({
	props: {
		contract: Contract,
		index :Number,
		selected: Contract,
	},
	components: {
    RecomAircraft,
    Company_badge
},
	data() {
		return {
			scroll_visible: false,
			load: false,
			contract_expire_timeout: null,
			theme: this.$os.userConfig.get(['ui','theme']),
			background_color: null,
			elevation_poly: "",
			//transition_delay: null,
			color_bright: null,
			color_dark: null,
			color_is_dark: null
		}
	},
	methods: {
		init() {
			//this.transition_delay = (this.index * 100) + 'ms';
			this.color_is_dark = false;
			this.color_bright = null;
			this.color_dark = null;
			this.background_color = null;
		},

		select() {
			if(!this.contract.expired) {
				this.$emit('details');
			}
		},

		colorize() {
			if(this.contract.image_url.length) {
				this.$os.colorSeek.find(this.contract.image_url, 150, (color :any) => {
					this.color_bright = color.color_bright.h;
					this.color_dark = color.color_dark.h;
					this.color_is_dark = color.color_is_dark;
					this.background_color = this.theme == 'theme--dark' ? this.color_dark : this.color_bright;
				});
			}
		},

		elevation_create() {
			let pointList = [] as string[];
			pointList.push("0, 103");

			if(this.contract.topo) {
				const total = this.contract.topo.length - 1;
				let maxVal = 2000;
				this.contract.topo.forEach((topo :any) => {
					if(topo > maxVal) { maxVal = topo + 300; }
				});
				this.contract.topo.forEach((topo :any, index :number) => {
					pointList.push(((index / total) * 100) + "," + (100 - ((topo / maxVal) * 100)));
				});
			}

			pointList.push("100, 103");
			this.elevation_poly = pointList.join(' ');
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'themechange': {
					this.theme = this.$os.userConfig.get(['ui','theme']);
					this.init();
					if(this.load) {
						this.colorize();
					}
					break;
				}
			}
		},
	},
	mounted() {
		this.init();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		clearTimeout(this.contract_expire_timeout);
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	},
	watch: {
		contract: {
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
		scroll_visible: {
			handler(newValue, oldValue) {
				if(newValue && !this.load) {
					this.load = true;
					this.elevation_create();
					this.colorize();
				}
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
@import '@/sys/scss/helpers.scss';

.contract {
	position: relative;
	overflow: hidden;
	cursor: pointer;
	margin: 8px;
	margin-bottom: 0;

	.theme--bright & {
		.background {
			& > div {
				filter: blur(20px) contrast(1);
			}
		}
		.elevation {
			svg {
				stop:nth-child(1) {
					stop-color: rgba($ui_colors_bright_shade_5, 0.15);
				}
				stop:nth-child(2) {
					stop-color: rgba($ui_colors_bright_shade_5, 0.1);
				}
			}
		}
		.contract {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		&::after {
			border-color: rgba($ui_colors_bright_shade_5, 0.2s);
		}
		&:hover {
			.background {
				& > div {
					opacity: 0.2;
					filter: blur(20px) contrast(2);
				}
			}
		}
		&.selected {
			&::after {
				border-color: $ui_colors_bright_button_info;
			}
			.background {
				& > div {
					opacity: 0.2;
				}
			}
		}
		&.unavailable {
			.unavailable-label {
				div:first-child {
					background-image: url(../../../../sys/assets/icons/dark/chrono.svg);
				}
			}
		}
	}
	.theme--dark & {
		.background {
			& > div {
				filter: blur(20px) contrast(1);
			}
		}
		.elevation {
			svg {
				stop:nth-child(1) {
					stop-color: rgba($ui_colors_dark_shade_5, 0.15);
				}
				stop:nth-child(2) {
					stop-color: rgba($ui_colors_dark_shade_5, 0.1);
				}
			}
		}
		.contract {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		&::after {
			border-color: rgba($ui_colors_dark_shade_5, 0.2);
		}
		&:hover {
			.background {
				& > div {
					opacity: 0.4;
					filter: blur(20px) contrast(2);
				}
			}
		}
		&.selected {
			&::after {
				border-color: $ui_colors_bright_button_info;
			}
			.background {
				& > div {
					opacity: 0.4;
				}
			}
		}
		&.unavailable {
			.unavailable-label {
				div:first-child {
					background-image: url(../../../../sys/assets/icons/bright/chrono.svg);
				}
			}
		}
	}

	&::after {
		content: '';
		position: absolute;
		top: 0;
		left: 0;
		bottom: 0;
		right: 0;
		transition: border 0.2s ease-out;
		border: 1px solid transparent;
		border-radius: 8px;
		pointer-events: none;
	}

	&.selected {
		&::after {
			border-width: 4px;
		}
		.background {
			& > div {
				transition: filter 0.2s ease-out, opacity 0.2s ease-out;
				filter: blur(20px) contrast(2);
			}
		}
		.content {
			border-left-color: #FFFFFF;
		}
	}

	&.unavailable {
		cursor: default;
		.content {
			filter: blur(5px);
			opacity: 0.3;
		}
		.unavailable-label {
			position: absolute;
			display: flex;
			flex-direction: column;
			justify-content: center;
			align-items: center;
			font-family: "SkyOS-SemiBold";
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			z-index: 2;
			div:first-child {
				height: 50px;
				width: 50px;
				margin-bottom: 8px;
				background-size: contain;
				background-position: center;
				background-repeat: no-repeat;
			}
			div:last-child {
				display: flex;
				align-items: center;
				justify-content: center;
			}
		}
	}

	.background {
		display: flex;
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		border-radius: 8px;
		transition: opacity 1s ease-out;
		&.loading {
			opacity: 0;
		}
		& > div {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			opacity: 0;
			background-size: cover;
			background-position: center;
			transition: filter 0.8s ease-out, opacity 0.8s ease-out;
		}
	}

	.elevation {
		pointer-events: none;
		svg {
			position: absolute;
			left: 0;
			right: 0;
			bottom: 0;
			height: 25%;
			z-index: 1;
			border-radius: 8px;
		}
	}

	.content {
		position: relative;
		padding: 14px 16px;
		border-radius: 8px;
		z-index: 2;
		&-sub {
			position: relative;
			padding: 16px;
			padding-top: 0;
			border-top: 1px solid transparent;
		}
	}

	.contract {
		z-index: 2;
    	position: relative;
		border-radius: 8px;
	}

	.title {
		font-size: 16px;
		font-family: "SkyOS-Bold";
		line-height: 1.2em;
		margin-bottom: 0;
		margin-right: 8px;
	}

	.route {
		//font-family: "SkyOS-Bold";
		//font-size: 16px;
		//line-height: 1.2em;
		white-space: nowrap;
	}

	.reward {
		//font-size: 18px;
		//line-height: 1;
		font-family: "SkyOS-Bold";
	}

	.description {
		margin-top: 8px;
		margin-bottom: 8px;
	}

	.distance {
		font-family: "SkyOS-Bold";
	}

	.aircraft {
		white-space: nowrap;
	}

	.bookmark {
		margin-top: -14px;
		margin-right: -4px;
		margin-left: 4px;
	}

}
</style>