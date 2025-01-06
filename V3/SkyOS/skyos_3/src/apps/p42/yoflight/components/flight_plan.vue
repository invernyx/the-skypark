<template>
	<div class="plan" @click="$emit('details')">
		<div class="content">
			<div class="waypoints">
				<div class="waypoint"
				v-for="(waypoint, index) in plan_obj.waypoints"
				v-bind:key="index"
				:class="['type-' + waypoint.type, 'airway-index-' + waypoint.airway_index, { 'has-airway': waypoint.airway != '', 'is-first-airway': waypoint.airway_first, 'is-last-airway': waypoint.airway_last }]"
				>
					<div class="airway-label" v-if="waypoint.airway_first">{{  waypoint.airway_first ? waypoint.airway : '' }}</div>
					<div class="waypoint-code" v-if="waypoint.code == 'TIMECRUIS'">T/C</div>
					<div class="waypoint-code" v-else-if="waypoint.code == 'TIMEDSCNT'">T/D</div>
					<div class="waypoint-code" v-else>{{ waypoint.code.length ? waypoint.code : waypoint.location[1].toFixed(4) + ', ' + waypoint.location[0].toFixed(4) }}</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Flightplan from '@/sys/classes/flight_plans/plan';

export default Vue.extend({
	props: {
		plan :Object,
		index :Number,
	},
	components: {
	},
	data() {
		return {
			plan_obj: null as Flightplan,
			scroll_visible: false,
			theme: this.$os.userConfig.get(['ui','theme']),
		}
	},
	methods: {
		init() {
			this.plan_obj = this.plan as Flightplan;

		},
	},
	mounted() {
	},
	beforeMount() {
		this.init();
	},
	beforeDestroy() {
	},
	watch: {
		plan: {
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
@import '@/sys/scss/helpers.scss';

.plan {
	position: relative;
	overflow: hidden;
	cursor: pointer;
	margin: 6px;
	margin-bottom: 0;

	.theme--bright & {
		border-color: rgba($ui_colors_bright_shade_5, 0.4);
		.content {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		&:hover {
			.content {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
		}
		&::after {
			border-color: rgba($ui_colors_bright_shade_5, 0.2s);
		}
	}
	.theme--dark & {
		border-color: rgba($ui_colors_dark_shade_5, 0.3);
		.content {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		&:hover {
			.content {
				background-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
		}
		&::after {
			border-color: rgba($ui_colors_dark_shade_5, 0.2);
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

	.content {
		position: relative;
		padding: 14px 16px;
		border-radius: 8px;
		z-index: 2;
	}

	.title {
		font-size: 16px;
		font-family: "SkyOS-Bold";
		line-height: 1.2em;
		margin-bottom: 0;
		margin-right: 8px;
	}

	.waypoints {
		display: flex;
		flex-direction: row;
		flex-wrap: wrap;
		font-size: 1em;
		line-height: 1em;
		.waypoint {
			display: inline-flex;
			flex-direction: row;
			align-items: stretch;
			margin-bottom: 4px;
			overflow: hidden;
			&:before {
				content: "❯";
				display: inline-flex;
				margin-right: 0px;
				opacity: 0.2;
				border-bottom: 2px solid transparent;
				padding: 0 5px 0 5px;
			}
			&:first-child {
				&:before {
					display: none;
				}
			}
			&.is-first-airway {
				.waypoint-code {
					padding-left: 4px;
				}
			}
			&.type-airport {
				font-family: "SkyOS-SemiBold";
			}
			&.airway-index {
				$backgrounds: #45f0f4, #4eb4f9, #0ba09c, #43edac, #aef72f, #bcba5e, #a56429, #ef4b62, #ffa039, #fcdf35, #7c40ff, #ef52d1, #ba8aff, #828282;
				@for $i from 1 through length($backgrounds) {
					&-#{$i} {
						.airway-label,
						.waypoint-code {
							border-color: rgba(nth($backgrounds, $i), 1);
						}
						&:before {
							color: nth($backgrounds, $i);
							opacity: 1;
							border-color: rgba(nth($backgrounds, $i), 1);
						}
						&.is-first-airway {
							.airway-label {
								.theme--bright & {
									background-color: rgba(nth($backgrounds, $i), 1);
								}
								.theme--dark & {
									background-color: rgba(nth($backgrounds, $i), 0.5);
								}
							}
							&:before {
								color: inherit;
								opacity: 0.2;
								border-color: transparent;
							}
						}
					}
				}
			}
			.airway-label {
				display: inline-flex;
				align-self: flex-start;
				align-items: flex-end;
				font-family: "SkyOS-SemiBold";
				height: 14px;
				font-size: 12px;
				letter-spacing: 0.05em;
				padding: 0 5px;
				margin-bottom: -2px;
				opacity: 1;
				border-bottom: 2px solid transparent;
			}
			.waypoint-code {
				border-bottom: 2px solid transparent;
			}
			& > div {
				display: inline-block;
			}
		}
	}

}
</style>