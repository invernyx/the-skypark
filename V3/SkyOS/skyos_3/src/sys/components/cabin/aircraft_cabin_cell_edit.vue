<template>
	<scroll_stack :sid="'p42_aircraft_cabin'" class="cabin_data columns">
		<template v-slot:top>
			<div class="controls-top h_edge_padding">
				<div class="columns">
					<div class="column column_narrow">
						<button_nav class="no-wrap" shape="back" @click.native="$emit('close')">Done</button_nav>
					</div>
					<div class="column column_justify_center column_align_end">
						<strong>Editing cell {{ cell[0] }}, {{ cell[1] }}</strong>
					</div>
				</div>
			</div>
		</template>
		<template v-slot:content>
			<div class="h_edge_margin">

				<!--
				<div v-if="cabin.features.filter(f => f.x == cell[0] && f.y == cell[1] && f.z == cell[2]).length">
					<collapser class="outlined h_edge_margin_bottom_half" :withArrow="true" :default="false" v-for="(feature, index) in cabin.features.filter(f => f.x == cell[0] && f.y == cell[1] && f.z == cell[2])" v-bind:key="index">
						<template v-slot:title>
							<div class="columns h_edge_padding_half">
								<div class="column column_justify_center">
									<strong>{{ feature.name }}</strong>
								</div>
								<div class="column column_narrow column_justify_center">{{ feature.type }}</div>
								<div class="column column_narrow column_justify_center">
									<div class="collapser_arrow"></div>
								</div>
							</div>
						</template>
						<template v-slot:content>
							<div class="h_edge_padding_half">
								<div class="h_edge_margin_bottom_half">
									{{ feature }}
								</div>
								<div class="columns">
									<div class="column column_narrow column_justify_center">
									</div>
									<div class="column column_narrow column_justify_center">
										<button_nav class="cancel compact shadowed-shallow" @click.native="feature_remove(feature)">Remove</button_nav>
									</div>
								</div>
							</div>
						</template>
					</collapser>
				</div>
				<div v-else class="text-center h_edge_margin_bottom">
					<strong>This cell is empty.</strong>
				</div>
				-->

				<div v-for="(feature, index) in cell_features" v-bind:key="index">
					<div class="buttons_list shadowed-shallow ">
						<div class="columns">
							<div class="column column_4 column_h-stretch">
								<button_listed class="listed_h" :class="[{ 'selected': feature.orientation == 'left' }]" @click.native="set_orient(feature,'left')">Left</button_listed>
							</div>
							<div class="column column_4 column_h-stretch">
								<button_listed class="listed_h" :class="[{ 'selected': feature.orientation == 'up' }]" @click.native="set_orient(feature,'up')">Up</button_listed>
							</div>
							<div class="column column_4 column_h-stretch">
								<button_listed class="listed_h" :class="[{ 'selected': feature.orientation == 'bottom' }]" @click.native="set_orient(feature,'bottom')">Bottom</button_listed>
							</div>
							<div class="column column_4 column_h-stretch">
								<button_listed class="listed_h" :class="[{ 'selected': feature.orientation == 'right' }]" @click.native="set_orient(feature,'right')">Right</button_listed>
							</div>
						</div>
					</div>
				</div>

			</div>
		</template>
	</scroll_stack>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';
import AircraftCabinFeature from '@/sys/classes/cabin/aircraft_cabin_feature';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';

export default Vue.extend({
	props:{
		aircraft :Aircraft,
		cabin :Object as () => AircraftCabin,
		state: Object as () => AircraftCabinState,
		cell: Array as () => Array<number>,
	},
	data() {
		return {
			is_exit: false,
			can_exit: false
		}
	},
	beforeMount() {
		this.init();
		this.$os.eventsBus.Bus.on('fleet', this.listener_os_fleet);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('fleet', this.listener_os_fleet);
	},
	methods: {

		init() {
			this.is_exit = this.cell_features.find(x => x.type == "door") != null;

			this.can_exit =
				(
					this.cell[0] == (this.cabin.levels[this.state.level][2])
				 || this.cell[0] == (this.cabin.levels[this.cell[2]][0] + this.cabin.levels[this.cell[2]][2]) - 1)
				&& (this.cell_features.length ? this.cell_features.filter(x => x.type != 'seat' && x.type != 'door' && x.type != 'cargo').length == 0 : true);
		},

		set_exit(val :boolean) {
			if(val) {
				const nf = new AircraftCabinFeature({
					type: 'door',
					x: this.cell[0],
					y: this.cell[1],
					z: this.cell[2],
					orientation:  this.state.drawing_orientation
				});
				this.cell_features.push(nf);
				this.feature_add(nf)
			} else {
				const found = this.cell_features.find(f => f.x == this.cell[0] && f.y == this.cell[1] && f.z == this.cell[2] && f.type == "door");
				if(found) {
					this.feature_remove(found);
				}
			}
		},

		set_orient(feature :AircraftCabinFeature, va :string) {
			feature.orientation = va;
			this.save_feature(feature);
		},

		feature_add(feature :AircraftCabinFeature) {
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					verb: 'AddFeature',
					feature: feature
				},
				(response: any) => {}
			);
		},

		save_feature(feature :AircraftCabinFeature) {
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					guid: feature.guid,
					data: feature,
				},
				(response: any) => {}
			);
		},

		feature_remove(feature :AircraftCabinFeature) {
			this.cabin.features.splice(this.cabin.features.indexOf(feature), 1);

			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					verb: 'RemoveFeature',
					guid: feature.guid
				},
				(response: any) => {}
			);
		},

		listener_os_fleet(wsmsg :any) {
			switch(wsmsg.name) {
				case 'mutate': {
					this.init();
					break;
				}
			}
		},
	},
	computed: {
		cell_features():any {
			return this.cabin.features.filter(f => f.x == this.cell[0] && f.y == this.cell[1] && f.z == this.cell[2]);
		},
	},
	watch: {
		cell() {
			this.init();
		},
		cell_features() {
			this.init();
		}
	}
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