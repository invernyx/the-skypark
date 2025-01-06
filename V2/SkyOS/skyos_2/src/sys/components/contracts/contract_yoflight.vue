<template>
	<div class="contract_yoflight"
		:class="[{
			'selected': selected,
			'hidden': hidden,
			'ready': state.ui.ready,
		}]"
		@click="select($event)"
		>
		<div class="card" :style="state.ui.color ? 'background-color:' + (this.$root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : ''">
			<div class="border"></div>
			<div class="background">
				<div class="blur" v-if="state.ui.ready" :style="'background-image: url(' + contract.ImageURL + ')'"></div>
			</div>
			<div class="split">
				<div class="infos">
					<h1 class="name">{{ template.Name.length ?  template.Name : contract.Route }}</h1>
					<div class="route" v-if="template.Name.length">{{ contract.Route }}</div>
					<div class="expires" v-if="template.RunningClock">Exp. <countdown :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt"></countdown></div>
					<div class="expires" v-else-if="contract.State == 'Listed'">Time: <strong>{{ template.TimeToComplete > 0 ? template.TimeToComplete.toLocaleString('en-gb') + 'h' : 'Infinite' }}</strong></div>
					<div class="expires" v-else-if="template.TimeToComplete > 0">Remaining: <countdown :no_fix="true" :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt" :full="false" :only_hours="true"></countdown></div>
					<div class="expires" v-else><strong>Infinite</strong> Time</div>
				</div>
				<div class="actions">
					<button_action class="expand info">Plan</button_action>
				</div>
			</div>
		</div>
	</div>

</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from './../../../sys/libraries/elem';

export default Vue.extend({
	name: "contract_yoflight",
	props: ['contract', 'templates', 'selected', 'index', 'hidden'],
	data() {
		return {
			template: null,
			state: {
				ui: {
					ready: false,
					delayTO: null,
					colorIsDark: false,
					color: null,
					colorDark: null,
					requesting: false,
					highestElev: null,
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
					this.state.ui.countries.push(fullList[fullList.length - 1]);
					this.state.ui.countries.push("+" + (fullList.length - this.state.ui.countries.length - 1));
				}

			}, this.index * 100);
		},
		select(ev: any) {
			const path = Eljs.getDOMParents(ev.target) as HTMLElement[];
			if(path[0].className.includes('expand')) {
				this.$emit('expand', this.contract);
			} else if(!path[0].className.includes('button')){
				this.$emit('select', this.contract);
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'adventure': {
					this.state.ui.requesting = false;
					this.state.ui.expireAt = new Date(this.contract.ExpireAt);
					this.state.ui.pullAt = new Date(this.contract.PullAt);
					break;
				}
			}
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
.contract_yoflight {
	display: flex;
	align-items: stretch;
	cursor: pointer;
	min-width: 250px;
	transition: min-width 0.5s $transition, opacity 0.2s 0.5s $transition;

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
		.background {
			.blur {
				filter: blur(30px) brightness(1.2) contrast(1);
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
		.background {
			.blur {
				filter: blur(30px) brightness(1) contrast(1);
			}
		}
		.infos {
			text-shadow: none;
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
			.border {
				border-color: $ui_colors_bright_button_info;
			}
			&:hover {
				transform: scale(1.03);
			}
		}
	}
	&.hidden {
		min-width: 0;
		opacity: 0;
		transition: min-width 0.5s 0.1s $transition, opacity 0.2s $transition;
	}

	.card {
		display: flex;
		align-self: stretch;
		flex-direction: column;
		width: 250px;
		min-width: 250px;
		border-radius: 14px;
		opacity: 0;
		overflow: hidden;
		will-change: transform;
		transform: scale(0.95);
		@include shadowed($ui_colors_dark_shade_0);
		transition: opacity 1s $transition, transform 0.2s $transition;
		&:hover {
			transform: scale(0.98);
		}
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
		.shade-top {
			position: absolute;
			left: 0;
			right: 0;
			top: 0;
			height: 30px;
			border-radius: 8px;
			background: linear-gradient(to bottom, rgba(0,0,0,0.2), cubic-bezier(.2,0,.4,1), rgba(0,0,0,0));
		}
		.shade-bottom {
			position: absolute;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 8px;
			padding: 8px;
			background: linear-gradient(to top, rgba(0,0,0,0.5), cubic-bezier(.2,0,.4,1), rgba(0,0,0,0));
			.name {
				font-size: 18px;
				white-space: normal;
				font-family: "SkyOS-SemiBold";
				margin-top: 32px;
				color: #FFF;
				text-shadow: 0px 2px 5px #000;
			}
			.route {
				font-size: 12px;
				color: #FFF;
			}
		}
	}
	.infos {
		position: relative;
		display: flex;
		flex-direction: column;
		margin: 8px 14px;
		z-index: 2;
		flex-grow: 1;
		overflow: hidden;
		.name {
			font-size: 18px;
			line-height: 1em;
			white-space: normal;
			flex-grow: 1;
			font-family: "SkyOS-SemiBold";
			margin-top: 0;
			margin-bottom: 0;
		}
		.route {
			font-size: 12px;
			margin-top: 0;
		}
		.description {
			font-size: 0.9em;
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
	.split {
		display: flex;
		flex-grow: 1;
		flex-direction: row;
		align-self: stretch;
		z-index: 2;
		.expires {
			font-size: 12px;
			.countdown {
				font-family: "SkyOS-SemiBold";
			}
		}
		.actions {
			display: flex;
			align-self: stretch;
			margin: 8px;
		}
	}

}
</style>