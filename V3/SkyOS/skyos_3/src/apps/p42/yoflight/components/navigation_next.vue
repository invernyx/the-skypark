<template>
	<div class="app-box app-box-margined shadowed-deep h_edge_padding_top_half">
		<h3 class="separator h_edge_padding_bottom_half">Next</h3>
		<div class="columns columns_margined h_edge_padding_bottom">
			<div class="column">
				<data_stack class="center small" label="Code">
					<span v-if="leg">{{ leg.start.code }}</span>
					<span v-else>~</span>
				</data_stack>
			</div>
			<div class="column">
				<data_stack class="center small" label="Elevation">
					<span v-if="leg"><height :amount="leg.start.airport.elevation" :decimals="0"/></span>
					<span v-else>~</span>
				</data_stack>
			</div>
			<div class="column">
				<data_stack class="center small" label="Track">
					<span v-if="leg ? leg.dist_to_go >= 0 : false"><heading :amount="leg.bearing_to - $os.simulator.location.MagVar" :decimals="0"/>°</span>
					<span v-else>~</span>
				</data_stack>
			</div>
		</div>
		<div class="columns columns_margined h_edge_padding_bottom">
			<div class="column">
				<data_stack class="center small" label="Distance">
					<span v-if="leg ? leg.dist_to_go >= 0 : false"><distance :amount="leg.dist_to_go" :decimals="0"/></span>
					<span v-else>~</span>
				</data_stack>
			</div>
			<div class="column">
				<data_stack class="center small" label="ETE">
					<span v-if="leg ? leg.ete >= 0 : false"><duration :time="leg.ete" :decimals="1" :brackets="{ to_minutes : 1 }"/></span>
					<span v-else>~</span>
				</data_stack>
			</div>
			<div class="column">
				<data_stack class="center small" label="To Track">
					<div class="to_track" v-if="leg ? leg.dist_to_go >= 0 : false">
						<div class="to_track_left_arrow" :class="{ 'visible': leg.bearing_dif <= -0.5 }">❮</div>
						<span v-if="Math.abs(leg.bearing_dif) > 0.5"><number :amount="leg.bearing_dif" :decimals="0"/>°</span>
						<span v-else>|</span>
						<div class="to_track_right_arrow" :class="{ 'visible': leg.bearing_dif >= 0.5 }">❯</div>
					</div>
					<span v-else>~</span>
				</data_stack>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { NavLeg } from '@/sys/services/extensions/navigation';

export default Vue.extend({
	props: {
		leg :NavLeg,
	},
	components: {
	},
	data() {
		return {
		}
	},
	methods: {
		init() {

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
		airport: {
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


.to_track {
	display: flex;
	flex-direction: row;
	justify-content: space-between;
	&_left_arrow {
		display: none;
		margin-right: 8px;
		&.visible {
			display: block;
		}
	}
	&_right_arrow {
		display: none;
		margin-left: 4px;
		&.visible {
			display: block;
		}
	}
}

</style>