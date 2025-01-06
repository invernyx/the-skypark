<template>
	<div class="flightplan_detailed"
		:class="[{
			'plan': plan,
		}]"
		@click="select($event)"
		>

		<div class="h_edge_padding_lateral">
			<br>
			<div class="infos" v-if="plan.Name">
				<div>
					<div class="label">Flightplan</div>
					<div class="value">{{ plan.Name }}</div>
				</div>
			</div>
			<div class="infos" v-if="plan.File">
				<div>
					<div class="label">File</div>
					<div class="value">{{ plan.File }}</div>
				</div>
			</div>
		</div>

		<div class="infos">
			<div>
				<div class="waypoints">
					<div class="waypoint"
					v-for="(waypoint, index) in plan.Waypoints"
					v-bind:key="index"
					:class="['type-' + waypoint.Type, 'airway-index-' + waypoint.AirwayIndex, { 'has-airway': waypoint.Airway != '', 'is-first-airway': waypoint.AirwayFirst, 'is-last-airway': waypoint.AirwayLast }]"
					>
						<div class="waypoint-code" v-if="waypoint.Code == 'TIMECRUIS'">T/C</div>
						<div class="waypoint-code" v-else-if="waypoint.Code == 'TIMEDSCNT'">T/D</div>
						<div class="waypoint-code" v-else>{{ waypoint.Code.length ? waypoint.Code : waypoint.Location[1].toFixed(4) + ', ' + waypoint.Location[0].toFixed(4) }}</div>
						<div class="airway-label">{{  waypoint.AirwayFirst ? waypoint.Airway : '' }}</div>
					</div>
				</div>
			</div>

			<!--<pre>{{ plan }}</pre>-->
		</div>
	</div>

</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "flightplan_detailed",
	props: ['plan'],
	methods: {
		initCard() {


		},
		select(ev: any) {

		},

		listener_ws(wsmsg: any) {
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
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
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
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.flightplan_detailed {

	.theme--bright &,
	&.theme--bright {
		//color: $ui_colors_bright_shade_5;
		.waypoints {
			background-color: $ui_colors_bright_shade_0;
		}
	}

	.theme--dark &,
	&.theme--dark {
		//color: $ui_colors_dark_shade_5;
		.waypoints {
			background-color: $ui_colors_dark_shade_0;
		}
	}

	.infos {
		width: 100%;
		display: flex;
		margin-bottom: 14px;
		font-size: 12px;
		.label,
		.value {
			display: block;
		}
		.value {
			font-family: "SkyOS-SemiBold";
		}
		& > div {
			flex-grow: 1;
		}
	}

	.waypoints {
		display: flex;
		flex-direction: row;
		flex-wrap: wrap;
		padding: $edge-margin;
		font-size: 1.4em;
		line-height: 1em;
		.waypoint {
			display: inline-flex;
			flex-direction: column;
			align-items: stretch;
			margin-bottom: 8px;
			overflow: hidden;
			&.type-airport {
				font-family: "SkyOS-SemiBold";
			}
			&.is-last-airway {
				margin-right: 4px;
			}
			&.airway-index {
				$backgrounds: #45f0f4, #4eb4f9, #0ba09c, #43edac, #aef72f, #bcba5e, #a56429, #ef4b62, #ffa039, #fcdf35, #7c40ff, #ef52d1, #ba8aff, #828282;
				@for $i from 1 through length($backgrounds) {
					&-#{$i} {
						&.is-first-airway {
							.airway-label {
								background-color: rgba(nth($backgrounds, $i), 0.2);
							}
						}
						.waypoint-code {
							border-color: rgba(nth($backgrounds, $i), 0.3);
							//color: nth($colors, $i);
						}
					}
				}
			}
			.airway-label {
				display: flex;
				align-self: flex-start;
				align-items: flex-end;
				font-family: "SkyOS-SemiBold";
				height: 1.6em;
				font-size: 0.6em;
				letter-spacing: 0.05em;
				padding: 0 0.75em;
			}
			.waypoint-code {
				border-bottom: 2px solid transparent;
				padding: 0 0.3em 0 0.3em;
			}
			& > div {
				display: inline-block;
			}
		}
	}
}
</style>