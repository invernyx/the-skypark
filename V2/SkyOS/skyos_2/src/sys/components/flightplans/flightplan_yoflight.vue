<template>
	<div class="flightplan_yoflight"
		:class="[theme, {
			'selected': selected,
			'ready': state.ui.ready,
			'theme--dark': state.ui.colorIsDark
		}]"
		@click="select($event)"
		>
		<div class="card">

			<div class="split">
				<div class="waypoints" v-if="state.ui.route.length > 2">
					<div class="waypoint"
					v-for="(waypoint, index) in state.ui.route"
					v-bind:key="index"
					:class="['type-' + (waypoint.Airway.length && waypoint.Type != 'airport' ? 'airway' : waypoint.Type) ]"
					>
						<div>{{ waypoint.Airway && waypoint.Type != 'airport' ? waypoint.Airway : waypoint.Code.length ? waypoint.Code : waypoint.Location[1].toFixed(4) + ', ' + waypoint.Location[0].toFixed(4) }}</div>
					</div>
				</div>
				<div class="waypoints" v-else>
					<div class="waypoint type-airport">{{ state.ui.route[0].Code.length ? state.ui.route[0].Code : state.ui.route[0].Location[1].toFixed(4) + ', ' + state.ui.route[0].Location[0].toFixed(4) }}</div>
					<div class="waypoint">DCT</div>
					<div class="waypoint type-airport">{{ state.ui.route[1].Code.length ? state.ui.route[1].Code : state.ui.route[1].Location[1].toFixed(4) + ', ' + state.ui.route[1].Location[0].toFixed(4) }}</div>
				</div>
				<div class="actions">
					<button_action class="go expand">Fly</button_action>
				</div>
			</div>
			<div class="border"></div>

		</div>
	</div>

</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from './../../../sys/libraries/elem';

export default Vue.extend({
	name: "flightplan_yoflight",
	props: ['theme', 'index', 'plan', 'selected'],
	data() {
		return {
			template: null,
			state: {
				ui: {
					ready: false,
					delayTO: null,
					route: []
				}
			}
		}
	},
	methods: {
		initCard() {
			this.state.ui.route = [];
			let airway = '';
			let indexCount = 0;
			const limit = 12;

			this.plan.Waypoints.forEach((wp :any) => {
				if(this.plan.Waypoints.length > 2) {
					if(wp.Airway == '' || wp.Type == 'airport'){
						this.state.ui.route.push(wp);
					} else if(wp.Airway != airway) {
						this.state.ui.route.push(wp);
					}
				} else {
					this.state.ui.route.push(wp);
				}
				airway = wp.Airway;
			});

			const toRemove = this.state.ui.route.length - limit;
			if(toRemove > 0){
				const nonAirports = this.state.ui.route.filter(x => x.Type != 'airport');
				const length = this.state.ui.route.length;
				const trimCount = (length - limit);
				const trimStartLocation = Math.ceil((length / 2) - (trimCount / 2));

				this.state.ui.route.splice(trimStartLocation, trimCount);
				this.state.ui.route.splice(trimStartLocation, 0, {
					Airway: '',
					Type: 'more',
					Code: '+' + trimCount
				});
			}

			this.state.ui.ready = true;
		},
		select(ev: any) {
			const path = Eljs.getDOMParents(ev.target) as HTMLElement[];
			if(path[0].className.includes('expand')) {
				this.$emit('fly', this.plan);
			} else if(!path[0].className.includes('button')){
				this.$emit('select', this.plan);
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'adventure': {

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
		this.initCard();
	},
	watch: {
		plan: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
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
.flightplan_yoflight {
	display: flex;
	align-items: stretch;
	cursor: pointer;
	min-width: 250px;
	transition: min-width 0.5s $transition, opacity 0.2s 0.5s $transition;

	.theme--bright &,
	&.theme--bright {
		color: $ui_colors_bright_shade_5;
		.card {
			background: $ui_colors_bright_shade_1;
		}
		.waypoints {
			.waypoint {
				&.type-more {
					background: $ui_colors_bright_shade_2;
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		color: $ui_colors_dark_shade_5;
		.card {
			background: $ui_colors_dark_shade_1;
		}
		.waypoints {
			.waypoint {
				&.type-more {
					background: $ui_colors_dark_shade_2;
				}
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

	.card {
		display: flex;
		flex-direction: column;
		background-color: #FFF;
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

	.waypoints {
		flex-grow: 1;
		padding: 8px 14px;
		.waypoint {
			display: inline-flex;
			margin-right: 5px;
			opacity: 0.7;
			&.type-airport {
				opacity: 1;
				font-family: "SkyOS-SemiBold";
			}
			&.type-airway {
				text-decoration: underline;
			}
			&.type-more {
				opacity: 1;
				border-radius: 4px;
				padding: 0 0.4em;
			}
		}
	}
	.split {
		display: flex;
		flex-grow: 1;
		flex-direction: row;
		align-self: stretch;
		z-index: 2;
		.actions {
			display: flex;
			align-self: stretch;
			margin: 8px;
		}
	}

}
</style>