<template>
	<scroll_stack :sid="'p42_aircraft_cabin'" class="cabin_data">
		<template v-slot:top>
			<div class="controls-top h_edge_padding">
				<div class="columns">
					<div class="column column_narrow">
						<button_nav class="no-wrap shadowed-shallow" shape="back" @click.native="$emit('close')">Done</button_nav>
					</div>
					<div class="column column_justify_center column_align_end h_edge_padding_right_half">
						<strong>Cell {{ cell[0] }}, {{ cell[1] }}</strong>
					</div>
					<div class="column column_narrow column_justify_center column_align_end">
						<button_nav class="no-wrap" @click.native="$emit('edit_cell')">Edit</button_nav>
					</div>
				</div>
			</div>
		</template>
		<template v-slot:content>
			<div class="h_edge_margin">

				<div v-for="(feature, index) in cabin.features.filter(f => f.x == cell[0] && f.y == cell[1] && f.z == cell[2])" v-bind:key="index">
					{{ feature }}
				</div>

			</div>
		</template>
	</scroll_stack>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';

export default Vue.extend({
	props:{
		aircraft :Aircraft,
		cabin :Object as () => AircraftCabin,
		cell: Array as () => Array<number>,
	},
	data() {
		return {
		}
	},
	methods: {

	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.cabin_data {
	flex-grow: 1;
}

</style>