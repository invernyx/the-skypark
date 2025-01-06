<template>
	<div class="contract_strip"
		:class="[theme, {
			'ready': state.ui.ready,
			'selected': selected,
			'paused': contract.IsMonitored === false && contract.LastLocationGeo && contract.State == 'Active'
		}]" @click="contractSelect">
		<div class="card" :style="state.ui.color ? 'background-color:' + state.ui.colorDark : ''">

			<div class="background" v-if="state.ui.ready">
				<div class="background-blur" :style="'background-image: url(' + contract.ImageURL + ')'"></div>
			</div>

			<div class="content">
				<div class="header">
					<div>
						<h1 class="name">{{ template.Name.length ?  template.Name : contract.Route }}</h1>
						<div class="route" v-if="template.Name.length">{{ contract.Route }}</div>
					</div>
				</div>

				<div class="infos">
					<div v-if="contract.State == 'Saved' || contract.State == 'Active'">
						<div class="expires" v-if="template.RunningClock">Exp. <countdown :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt"></countdown></div>
						<div class="expires" v-else-if="contract.State == 'Listed'">Time: <strong>{{ template.TimeToComplete > 0 ? template.TimeToComplete.toLocaleString('en-gb') + 'h' : 'Infinite' }}</strong></div>
						<div class="expires" v-else-if="template.TimeToComplete > 0">Remaining: <countdown :no_fix="true" :has_warn="true" :warn_for="state.ui.pullAt" :time="state.ui.expireAt" :full="false" :only_hours="true"></countdown></div>
						<div class="expires" v-else><strong>Infinite</strong> Time</div>
					</div>
					<div class="distance">{{ contract.Distance.toLocaleString('en-gb') }} nm</div>
				</div>
			</div>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "contract_strip",
	props: ['theme', 'contract', 'templates', 'selected'],
	mounted() {

	},
	data() {
		return {
			index: 0,
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
					pullAt: new Date(),
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
			}, this.index ? this.index * 100 : 10);
		},
		contractSelect() {
			this.$emit("select", this.contract);
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
	beforeMount() {
		this.initCard();
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
.contract_strip {
	position: relative;
	display: flex;
	flex-grow: 1;
	margin-bottom: 4px;

	.theme--bright &,
	&.theme--bright {
		.content {
			background-color: rgba($ui_colors_bright_shade_1, 0.4);
		}
		.card {
			background-color: darken($ui_colors_bright_shade_2, 10%);
			&::after {
				border-color: rgba($ui_colors_bright_shade_5, 0.1);
			}
		}
		.background {
			&-blur {
				filter: blur(30px) brightness(1) contrast(1);
			}
		}
		&:hover {
			.background {
				&-blur {
					filter: blur(10px) brightness(3) contrast(3);
				}
			}
		}
		&.selected {
			.card {
				&::after {
					background: $ui_colors_bright_shade_5;
					border-color: $ui_colors_bright_shade_5;
				}
			}
			.content {
				background-color: rgba($ui_colors_bright_shade_1, 0.4);
				border-color: #000;
			}
			.background {
				&-blur {
					filter: blur(10px) brightness(5) contrast(1);
				}
			}
		}
		&.paused {
			.card {
				&::after {
					background: $ui_colors_bright_button_cancel;
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		.content {
			background-color: rgba($ui_colors_dark_shade_1, 0.8);
		}
		.card {
			background-color: darken($ui_colors_dark_shade_2, 5%);
			&::after {
				border-color: rgba($ui_colors_dark_shade_5, 0.1);
			}
		}
		.background {
			&-blur {
				filter: blur(30px) brightness(0.2) contrast(1);
			}
		}
		&:hover {
			.background {
				&-blur {
					filter: blur(10px) brightness(1) contrast(3);
				}
			}
		}
		&.selected {
			.card {
				&::after {
					background: $ui_colors_dark_shade_5;
					border-color: $ui_colors_dark_shade_5;
				}
			}
			.content {
				background-color: rgba($ui_colors_dark_shade_1, 0.4);
				border-color: #FFF;
			}
			.background {
				&-blur {
					filter: blur(10px) brightness(1) contrast(3);
				}
			}
		}
		&.paused {
			.card {
				&::after {
					background: $ui_colors_dark_button_cancel;
				}
			}
		}
	}

	&:hover {
		.infos {
			opacity: 1;
		}
		.background {
			&-blur {
				transform: scale(1.5, 1.7);
			}
		}
	}

	&.selected {
		.infos {
			opacity: 1;
		}
		.background {
			&-blur {
				filter: blur(10px) brightness(1) contrast(1);
				transform: scale(1.5, 1.7);
			}
		}
	}

	&.ready {
		.card {
			opacity: 1;
		}
	}

	.card {
		$transition: cubic-bezier(.14,0,0,1);
		flex-grow: 1;
		display: flex;
		flex-direction: column;
		opacity: 0;
		border-radius: 8px;
		overflow: hidden;
		cursor: pointer;
		will-change: transform;
		mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 1) 100%);
		@include shadowed($ui_colors_dark_shade_1);
		transition: transform 0.3s $transition, opacity 1s cubic-bezier(.25,0,.14,1);
		&::after {
			content: '';
			display: block;
			position: absolute;
			top: 6px;
			right: 6px;
			bottom: 6px;
			width: 4px;
			border: 1px solid transparent;
			z-index: 10;
			border-radius: 4px;
			transition: border 0.5s $transition, background 0.5s $transition;
			animation-duration: 1s;
			animation-iteration-count: infinite;
			animation-direction: alternate;
		}
	}

	.content {
		position: relative;
		z-index: 2;
		padding: 5px 7px;
		padding-right: 16px;
		margin: 2px;
		border-radius: 6px;
		transition: background 0.3s $transition;
	}

	.background {
		position: absolute;
		left: 0;
		right: 0;
		top: 0;
		bottom: 0;
		overflow: hidden;
		&-blur {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			transform: scale(1);
			background-size: cover;
			background-position: center;
			will-change: transform, filter;
			transition: transform 1s $transition, opacity 1s $transition, filter 1s $transition;
		}
	}

	.header {
		display: flex;
		position: relative;
		z-index: 2;
		overflow: hidden;
		.name {
			font-size: 18px;
			line-height: 1em;
			white-space: normal;
			font-family: "SkyOS-SemiBold";
			margin-top: 0;
			margin-bottom: 4px;
		}
		.route {
			font-size: 12px;
			margin-top: 0;
		}
	}

	.infos {
		display: flex;
		flex-direction: column;
		position: relative;
		opacity: 0.5;
		overflow: hidden;
		.expires {
			font-size: 12px;
			.countdown {
				font-family: "SkyOS-SemiBold";
			}
		}
		.distance {
			font-size: 12px;
			margin-top: 0;
			font-family: "SkyOS-SemiBold";
		}
	}
}
</style>