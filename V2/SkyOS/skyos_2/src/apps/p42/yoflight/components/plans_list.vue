<template>
	<div class="plans_list" :class="[theme]" ref="mainlist">
		<FlightplanYoFlight
		v-for="(value, index) in selected.Contract.Flightplans"
		v-bind:key="index"
		:plan="value"
		:index="index"
		:selected="selected.Plan ? selected.Plan.Hash == value.Hash : false"
		@select="planSelect($event)"
		@fly="planFly($event)"
		/>
		<div class="card plans_single" v-if="!selected.Contract.Flightplans.length">
			<span class="label">No Flight plan</span>
			<span class="description">This contract does not have a flight plan.</span>
		</div>
		<div class="card plans_single" @click="planOpen">
			<span class="label">Load .PLN file</span>
			<span class="description">Select and load your own flight plan.</span>
		</div>
		<div></div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import FlightplanYoFlight from "@/sys/components/flightplans/flightplan_yoflight.vue"
import PlanLoader from "@/sys/libraries/plan_loader"

export default Vue.extend({
	name: "plans_list",
	props: ['theme', 'selected'],
	components: {
		FlightplanYoFlight
	},
	beforeDestroy() {
		clearInterval(this.ui.mainlist.scrollInterval);
	},
	mounted() {
		this.ui.mainlist.el = (this.$refs['mainlist'] as any);
		['touchstart', 'wheel', 'mousedown'].forEach((ev) => {
			this.ui.mainlist.el.addEventListener(ev, () => {
				if(this.ui.mainlist.scrollInterval){
					clearInterval(this.ui.mainlist.scrollInterval);
				}
			});
		});
		['wheel'].forEach((ev) => {
			this.ui.mainlist.el.addEventListener(ev, (ev1 :any) => {
				this.ui.mainlist.el.scrollLeft += ev1.deltaY;
				event.preventDefault();
			});
		});
	},
	methods: {
		planSelect(plan :any) {
			this.$emit('select', plan);
		},
		planFly(plan :any) {
			this.$emit('fly', plan);
		},
		planOpen() {
			PlanLoader.LoadPlan((plan :any, fileName :string) => {
				this.$root.$data.services.api.SendWS(
					'adventure:flightplan:load', {
						ID: this.selected.Contract.ID,
						content: plan,
						fileName: fileName
					}
				);
			});
		},


		scrollMove(xOffset: number) {
			clearInterval(this.ui.mainlist.scrollInterval);
			this.ui.mainlist.scrollInterval = setInterval(() => {

				let offsetDif = -(this.ui.mainlist.el.scrollLeft - xOffset);
				if(Math.abs(xOffset - this.ui.mainlist.el.scrollLeft) > 1){
					this.ui.mainlist.el.scrollLeft += Math.ceil(offsetDif / 5);
				} else {
					this.ui.mainlist.el.scrollLeft = xOffset;
					clearInterval(this.ui.mainlist.scrollInterval);
				}
			}, 8);
		},
		scrollTo(index :number) {
			let xOffset = 0;
			if(this.ui.mainlist.el){
				let padding = parseInt(window.getComputedStyle(this.ui.mainlist.el).getPropertyValue('padding-left').replace('px', ''));
				this.ui.mainlist.el.childNodes.forEach((el :HTMLElement, i: number) => {
					if(index >= i && el.nodeName == 'DIV'){
						if(el.classList.contains('flightplan_yoflight')){
							xOffset = el.offsetLeft - padding;
						}
					}
				});
			}
			this.scrollMove(xOffset);
		},
	},
	data() {
		return {
			ui: {
				mainlist: {
					el: null,
					scrollInterval: null,
				},
			},
			state: {
				plans: {
					selected: null,
					list: [],
				},
			}
		}
	},
	watch: {
		'selected.Plan': {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.scrollTo(this.selected.Contract.Flightplans.findIndex((x :any) => x.Hash == newValue.Hash));
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.plans_list {
	display: flex;
	flex-direction: row;
	align-items: stretch;
	white-space: nowrap;
	overflow-x: scroll;
	padding-top: 30px;
	padding-bottom: 30px;
	margin-top: -30px;
	margin-bottom: -30px;
	scrollbar-width: none;

	.theme--bright &,
	&.theme--bright {
		.plans_single {
			background: $ui_colors_bright_shade_1;
		}
	}

	.theme--dark &,
	&.theme--dark {
		.plans_single {
			background: $ui_colors_dark_shade_1;
		}
	}

	&::-webkit-scrollbar {
		display: none;
	}

	& > div {
		position: relative;
		white-space: normal;
		min-height: 1px;
		&:last-child {
			min-width: 100%;
		}
	}

	.plans_single {
		width: 240px;
		min-width: 240px;
		border-radius: 14px;
		padding: 16px;
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		font-family: "SkyOS-SemiBold";
		text-align: center;
		box-sizing: border-box;
		transform: scale(0.95);
		cursor: pointer;
		@include shadowed($ui_colors_dark_shade_0);
		transition: opacity 1s $transition, transform 0.2s $transition;
		&:hover {
			transform: scale(0.98);
		}
		&.disabled {
			opacity: 0.5;
		}
		.label {
			display: block;
		}
		.description {
			opacity: 0.6;
			font-size: 0.75em;
		}
	}

}
</style>