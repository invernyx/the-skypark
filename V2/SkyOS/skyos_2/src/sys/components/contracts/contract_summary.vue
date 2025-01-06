<template>
	<div class="contract_summary"
		:class="[{
			'selected': selected,
			'premium': template.IsCustom,
			'is-expired': state.ui.isExpired,
			'ready': state.ui.ready,
		}]"
		@click="select($event)"
		>
		<div class="card" :style="state.ui.color ? 'background-color:' + ($os.activeApp.app_theme_mode ? $os.activeApp.app_theme_mode == 'theme--bright' ? state.ui.color : state.ui.colorDark : $root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : ''">
			<div class="border"></div>

			<div class="background">
				<div class="blur" v-if="state.ui.ready" :style="'background-image: url(' + contract.ImageURL + ')'"></div>
			</div>

			<div class="split split-top">
				<div class="header">
					<div class="flags">
						<div v-for="(country, index) in state.ui.countries" v-bind:key="index">
							<flags :code="country.toLowerCase()" />
						</div>
					</div>
					<div class="aircraft" v-if="template.AircraftRestrictionLabel">Req. <span>{{ template.AircraftRestrictionLabel }}</span></div>
					<div class="aircraft" v-else><RecomAircraft :contract="contract" :small="true" /></div>
				</div>
				<div class="infos">
					<h1 class="name">{{ template.Name.length ?  template.Name : contract.Route }}</h1>
					<div class="route" v-if="template.Name.length">{{ contract.Route }}</div>
					<div class="description">{{ contract.Description }}</div>
				</div>
				<div class="elevation">
					<svg width="100%" height="100%" viewBox="0 0 100 100" preserveAspectRatio="none">
						<linearGradient :id="'gradient_' + contract.ID" x2="0" y2="1">
							<stop offset="0%" />
							<stop offset="100%" />
						</linearGradient>
						<polygon :fill="'url(#gradient_' + contract.ID + ')'" class="poly" :points="state.ui.elevationPoly"></polygon>
					</svg>
				</div>
			</div>

			<div class="split split-bottom">
				<div class="lower">
					<div>
						<div class="distance"><span>{{ contract.Distance.toLocaleString('en-gb') }}</span> nm{{ contract.Situations.length > 2 ? ' total' : ''}}</div>
						<div class="expires" v-if="template.RunningClock">Exp. <countdown :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt"></countdown></div>
						<div class="expires" v-else-if="contract.State == 'Listed'">Time: <strong>{{ template.TimeToComplete > 0 ? template.TimeToComplete.toLocaleString('en-gb') + 'h' : 'Infinite' }}</strong></div>
						<div class="expires" v-else-if="template.TimeToComplete > 0">Remaining: <countdown :no_fix="true" :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt" :full="false" :only_hours="true"></countdown></div>
						<div class="expires" v-else><strong>Infinite</strong> Time</div>
					</div>
					<div>
						<div :class="'company company-' + company" v-for="(company, index) in template.Company" v-bind:key="index"></div>
						<div class="routecode" v-if="contract.RouteCode.length && $os.getConfig(['ui','tier']) != 'prospect'"><span>{{ contract.RouteCode }}</span></div>
					</div>
				</div>
				<div class="footer">
					<div class="info" v-if="contract.RewardBux !== undefined">
						<span class="reward" v-if="contract.RewardBux != 0"><span class="amt">{{ (contract.RewardBux >= 0 ? '$' : '-$') + Math.abs(contract.RewardBux).toLocaleString('en-gb') }}</span></span>
						<span class="reward" v-else-if="contract.RewardKarma != 0"><span class="amt">{{ contract.RewardKarma > 0 ? '+' : '-' }} Karma</span></span>
						<span class="reward" v-else-if="contract.RewardReliability > 5"><span class="amt">+Reliability</span></span>
						<span class="reward" v-else-if="contract.RewardXP > 0"><span class="amt">+XP</span></span>
						<span class="reward" v-else><span class="amt">no reward</span></span>
					</div>
					<div class="action" v-if="!state.ui.isExpired">
						<div  v-if="contract.State == 'Listed'"></div>
						<button_nav class="info expand" :class="{ 'info': contract.State == 'Listed', 'translucent': contract.State != 'Listed' }" >Details</button_nav>
						<button_nav v-if="contract.State == 'Active'" class="go" @click.native="interactState($event, 'fly')">Manage</button_nav>
						<button_nav v-else-if="contract.State == 'Saved'" class="go" @click.native="interactState($event, 'fly')">Begin</button_nav>
						<!--<button_nav v-if="contract.State == 'Listed'" class="info" :class="{ 'loading disabled': contract.RequestStatus != 'ready' }" @click.native="interactState($event, 'save')">{{ contract.RequestStatus == 'ready' ? $root.$data.config.ui.tier == 'discovery' ? 'Request' : 'Save' : '' }}</button_nav>-->
					</div>
					<div class="action" v-else>
						<button_nav class="expand translucent" v-if="!state.ui.isExpired" >Details</button_nav>
						<button_nav class="disabled">Taken</button_nav>
					</div>
				</div>
			</div>

		</div>
	</div>

</template>

<script lang="ts">
import Vue from 'vue';
import RecomAircraft from "@/sys/components/contracts/contract_recom_aircraft.vue"

export default Vue.extend({
	name: "contract_summary",
	props: ['contract', 'templates', 'selected', 'canRedirect', 'index'],
	components: {
		RecomAircraft,
	},
	data() {
		return {
			template: null,
			state: {
				ui: {
					ready: false,
					delayTO: null,
					expireInterval: null,
					elevationPoly: "",
					isExpired: false,
					colorIsDark: false,
					color: null,
					colorDark: null,
					countries: [],
					expireAt: new Date(),
					pullAt: new Date()
				}
			}
		}
	},
	methods: {
		initCard() {
			this.state.ui.delayTO = setTimeout(() => {
				clearInterval(this.state.ui.expireInterval);

				if(this.contract.ImageURL.length) {
					this.state.ui.delayTO = null;
					this.$root.$data.services.colorSeek.find(this.contract.ImageURL, (color :any) => {
						this.state.ui.color = color.color;
						this.state.ui.colorDark = color.colorDark;
						this.state.ui.colorIsDark = color.colorIsDark;
						this.state.ui.ready = true;
					});
				} else {
					this.state.ui.colorIsDark = false;
					this.state.ui.color = null;
					this.state.ui.colorDark = null;
					this.state.ui.ready = true;
				}

				let shortest = null as any;
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

				this.state.ui.expireInterval = setInterval(() => {
					this.checkExpired();
				}, 5000);
				this.checkExpired();
				this.createElevation();

			}, this.index * 100);
		},
		createElevation() {
			let pointList = [] as string[];
			pointList.push("0, 103");

			const total = this.contract.Topo.length - 1;
			let maxVal = 2000;
			this.contract.Topo.forEach((topo :any) => {
				if(topo > maxVal) { maxVal = topo + 300; }
			});
			this.contract.Topo.forEach((topo :any, index :number) => {
				pointList.push(((index / total) * 100) + "," + (100 - ((topo / maxVal) * 100)));
			});

			pointList.push("100, 103");
			this.state.ui.elevationPoly = pointList.join(' ');
		},
		interactState(ev: Event, name: string) {
			this.$ContractMutator.Interact(this.contract, name, null, (response) => {
				this.state.ui.expireAt = new Date(this.contract.ExpireAt);
				this.state.ui.pullAt = new Date(this.contract.PullAt);
			});
		},
		select(ev: any) {
			if(ev.target.className.includes('expand')) {
				if(!this.state.ui.isExpired) {
					this.$emit('expand', this.contract);
				}
			} else if(!ev.target.className.includes('button_nav')){
				if(!this.selected || !this.state.ui.isExpired) {
					this.$emit('select', this.contract);
				}
			}
		},

		checkExpired() {
			if(this.contract.State == 'Listed') {
				if(this.state.ui.pullAt < new Date()){
					this.state.ui.isExpired = true;
					clearInterval(this.state.ui.expireInterval);
					this.$emit('expire', this.contract);
				}
			}
		},

		listenerWs(wsmsg: any) {

		}
	},
	mounted() {
		this.$emit('init');
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		clearTimeout(this.state.ui.delayTO);
		clearInterval(this.state.ui.expireInterval);
		this.$root.$off('ws-in', this.listenerWs);
	},
	beforeMount() {
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.template = this.templates.find((x: any) => x.FileName == newValue.FileName);
					this.state.ui.expireAt = new Date(this.contract.ExpireAt);
					this.state.ui.pullAt = new Date(this.contract.PullAt);
					this.state.ui.countries = [];
					this.state.ui.isExpired = false;
					this.initCard();
				}
			}
		}
	},
});
</script>

<style lang="scss">
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.contract_summary {
	display: flex;
	cursor: pointer;
	pointer-events: all;

	.theme--bright &,
	&.theme--bright {
		color: $ui_colors_bright_shade_5;
		&:hover {
			.background {
				.blur {
					filter: blur(30px) brightness(1) contrast(5);
				}
			}
		}
		&.selected {
			.card {
				.border {
					border-color: $ui_colors_bright_button_info;
				}
			}
			&.premium {
				.card {
					.border {
						border-color: $ui_colors_dark_button_gold;
					}
				}
			}
		}
		.card {
			background-color: rgba($ui_colors_bright_shade_1, 0.8);
		}
		.split {
			&-bottom {
				background: linear-gradient(to bottom, rgba($ui_colors_bright_shade_5, 0.1), cubic-bezier(.54,0,.14,1), rgba($ui_colors_bright_shade_5, 0));
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
		.background {
			.blur {
				filter: blur(30px) brightness(1.2) contrast(1);
			}
		}
		.lower {
			.routecode {
				color: $ui_colors_bright_shade_5;
				background-color: rgba($ui_colors_bright_shade_1, 0.3);
				background-image: url(../../../sys/assets/icons/dark/share.svg);
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		color: $ui_colors_dark_shade_5;
		&:hover {
			.background {
				.blur {
					filter: blur(30px) brightness(1) contrast(3);
				}
			}
		}
		&.selected {
			.card {
				.border {
					border-color: $ui_colors_bright_button_info;
				}
			}
			&.premium {
				.card {
					.border {
						border-color: $ui_colors_dark_button_gold;
					}
				}
			}
		}
		.card {
			background-color: rgba($ui_colors_dark_shade_1, 0.8);
		}
		.split {
			&-bottom {
				background: linear-gradient(to bottom, rgba($ui_colors_dark_shade_5, 0.1), cubic-bezier(.54,0,.14,1), rgba($ui_colors_dark_shade_5, 0));
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
		.background {
			.blur {
				filter: blur(30px) brightness(1) contrast(1);
			}
		}
		.infos {
			text-shadow: none;
		}
		.lower {
			.routecode {
				color: $ui_colors_dark_shade_5;
				background-color: rgba($ui_colors_dark_shade_1, 0.3);
				background-image: url(../../../sys/assets/icons/bright/share.svg);
			}
		}
	}

	&:hover {
		.background {
			.blur {
				transform: scale(1.5, 1.7);
			}
		}
	}

	&.ready {
		.card {
			opacity: 1;
		}
	}
	&.selected {
		.card {
			transform: scale(1);
		}
	}

	.card {
		display: flex;
		flex-direction: column;
		width: 290px;
		height: 185px;
		min-width: 240px;
		border-radius: 14px;
		margin-right: 8px;
		opacity: 0;
		overflow: hidden;
		backdrop-filter: blur(5px);
		@include shadowed($ui_colors_dark_shade_0);
		transform: scale(1) translateZ(0);
		transition: opacity 1s $transition, transform 0.2s $transition;
	}
	.border {
		border: 4px solid transparent;
		position: absolute;
		left: 0;
		right: 0;
		top: 0;
		bottom: 0;
		margin: -1px;
		border-radius: 15px;
		z-index: 10;
		pointer-events: none;
	}

	.split {
		&-top {
			position: relative;
			flex-grow: 1;
		}
		&-bottom {
			z-index: 2;
		}
	}

	.header {
		display: flex;
		flex-direction: row;
		align-items: center;
		margin-left: 16px;
		margin-right: 16px;
		margin-top: 8px;
		flex-grow: 1;
		justify-content: space-between;
		.aircraft {
			font-size: 0.75em;
			text-align: right;
			span {
				font-family: "SkyOS-SemiBold";
			}
			.recommended_aircraft  {
				margin: -5px;
			}
		}
	}
	.flags {
		font-size: 1em;
		margin: 0;
		display: flex;
		align-items: stretch;
		font-family: "SkyOS-SemiBold";
		& > div {
			margin-right: 5px;
			display: flex;
			align-items: stretch;
			color: #FFF;
			font-family: "SkyOS-SemiBold";
		}
		.flag {
			border-radius: 4px;
			box-shadow: 0 1.5px 4px rgba(0,0,0,0.2);
			&.flag-texted {
				font-size: 1em;
				padding: 0 0.2em;
			}
			span {
				font-size: 0.75em;
				line-height: 0.75em;
				display: flex;
			}
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
		}
	}
	.background {
		position: absolute;
		left: 0;
		right: 0;
		top: 0;
		bottom: 0;
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
		.blur {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			transform: scale(1.2);
			opacity: 0.5;
			background-size: cover;
			background-position: center;
			will-change: transform, filter;
			transition: transform 1s $transition, filter 1s $transition;
		}
	}
	.infos {
		position: relative;
		margin: 0 14px;
		z-index: 2;
		flex-grow: 1;
		overflow: hidden;
		.name {
			font-size: 18px;
			line-height: 1em;
			white-space: normal;
			font-family: "SkyOS-SemiBold";
			margin-top: 4px;
			margin-bottom: 0;
		}
		.route {
			font-size: 12px;
			margin-top: 0;
			font-family: "SkyOS-SemiBold";
		}
		.description {
			font-size: 0.8em;
			line-height: 1.4em;
			margin-top: 4px;
		}
		.limits {
			width: 100%;
			display: flex;
			margin-top: 14px;
			font-size: 12px;
			.label,
			.value {
				display: block;
			}
			.value {
				font-family: "SkyOS-SemiBold";
			}
			.runway-length {
				flex-grow: 1;
			}
			.highest {
				flex-grow: 1;
			}
			.dist {
				flex-grow: 1;
			}
		}
	}
	.lower {
		position: relative;
		justify-content: stretch;
		align-items: center;
		display: flex;
		margin: 0 14px;
		margin-top: 2px;
		font-size: 12px;
		z-index: 2;
		& > div {
			&:first-child {
				flex-grow: 1;
			}
			&:last-child {
				display: flex;
				flex-direction: row;
				align-items: center;
				margin-right: -6px;
				margin-bottom: -6px;
			}
		}
		.distance {
			span {
				font-family: "SkyOS-SemiBold";
			}
		}
		.expires {
			span {
				font-family: "SkyOS-SemiBold";
			}
		}
		.routecode {
			font-size: 12px;
			line-height: 12px;
			background-size: 12px;
			background-position: 3px center;
			background-repeat: no-repeat;
			padding: 2px 6px 2px 6px;
			border-radius: 6px;
			span {
				font-family: "SkyOS-SemiBold";
				letter-spacing: 0.05em;
				margin-left: 1.2em;
			}
		}
		.company {
			width: 20px;
			height: 20px;
			border-radius: 50%;
			margin-right: 4px;
			background-size: cover;
			background-repeat: no-repeat;
			background-position: center;
			&-clearsky {
				background-color: #FFF;
				background-image: url(../../../sys/assets/icons/companies/logo_clearsky.svg);
			}
			&-coyote {
				background-color: #111;
				background-image: url(../../../sys/assets/icons/companies/logo_coyote.svg);
			}
			&-skyparktravel {
				background-color: #FFF;
				background-image: url(../../../sys/assets/icons/companies/logo_skyparktravel.svg);
			}
		}
	}
	.footer {
		margin: 8px;
		margin-top: 0;
		display: flex;
		min-height: 27px;
		align-items: flex-end;
		z-index: 2;
		& >:first-child {
			flex-grow: 1;
		}
		& > div {
			&.info {
				margin: 8px;
				margin-top: 0;
				margin-bottom: 0;
				flex-grow: 1;
			}
			&.action {
				display: flex;
				align-items: stretch;
				& > div {
					margin-left: 4px;
					&:first-child {
						margin-left: 0;
					}
				}
			}
		}
		.reward {
			font-size: 18px;
			margin-right: 12px;
			margin-left: -2px;
			.amt {
				font-family: "SkyOS-SemiBold";
			}
		}
		.action {
			display: flex;
			justify-content: space-between;
			.button_nav {
				padding-top: 2px;
				padding-bottom: 3px;
				text-transform: uppercase;
			}
		}
	}
}
</style>