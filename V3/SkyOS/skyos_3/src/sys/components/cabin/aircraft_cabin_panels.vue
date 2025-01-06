<template>
	<transition :duration="8000" v-if="state">
		<keep-alive>
			<AircraftCabinData :aircraft="aircraft" :state="state" :cabin="cabin" v-if="state.ui_mode == 'data'" @edit="set_mode('edit')" @open_human="open_human($event)" />
			<AircraftCabinPeep :aircraft="aircraft" :cabin="cabin" :state="state" :human="state.human_selected" v-else-if="state.ui_mode == 'peep'" @close="set_mode('data')" />
			<AircraftCabinEdit :aircraft="aircraft" :cabin="cabin" :state="state" :draw_type="state.draw_type" v-else-if="state.ui_mode == 'edit'" @close="set_mode('data')" @draw_type="set_draw_type" @refresh="refresh" />
			<AircraftCabinCell :aircraft="aircraft" :cabin="cabin" :state="state" :cell="state.cell_selected" v-else-if="state.ui_mode == 'cell'" @close="set_mode('data')" @edit_cell="set_mode('edit_cell')" />
			<AircraftCabinCellEdit :aircraft="aircraft" :cabin="cabin" :state="state" :cell="state.cell_selected" v-else-if="state.ui_mode == 'edit_cell'" @close="set_mode(state.ui_mode_previous)" />
		</keep-alive>
	</transition>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';

import AircraftCabinData from './aircraft_cabin_data.vue';
import AircraftCabinEdit from './aircraft_cabin_edit.vue';
import AircraftCabinCell from './aircraft_cabin_cell.vue';
import AircraftCabinPeep from './aircraft_cabin_peep.vue';
import AircraftCabinCellEdit from './aircraft_cabin_cell_edit.vue';

export default Vue.extend({
	props:{
		aircraft :Object as () => Aircraft,
		cabin :Object as () => AircraftCabin,
		state: Object as () => AircraftCabinState,
	},
	components: {
		AircraftCabinData,
		AircraftCabinPeep,
		AircraftCabinEdit,
		AircraftCabinCell,
		AircraftCabinCellEdit
	},
	methods: {
		set_draw_type(type :string) {
			this.state.draw_type = type;
		},
		set_mode(mode :string) {
			this.set_draw_type(null);
			this.state.ui_mode_previous = this.state.ui_mode;
			this.state.ui_mode = mode;
			switch(mode){
				case 'edit':
				case 'data': {
					this.state.human_selected = null;
					this.state.cell_selected = null;
					break;
				}
				case 'peep':{
					this.state.cell_selected = null;
					break;
				}
				default: {
					this.state.human_selected = null;
				}
			}
		},
		open_human(human) {
			this.state.human_selected = human;
			this.set_mode('peep');
		},
		refresh() {
			this.$emit('refresh');
			this.$os.eventsBus.Bus.emit('fleet', { name: 'cabin', payload: {
				livery: this.cabin.livery,
				cabin: this.cabin
			}});
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.cabin {
	&_data {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		margin-bottom: 0;
		&.v-enter {
			opacity: 0;
		}
		&.v-enter-active {
			transition: opacity 0.3s 0.1s ease-out;
		}
		&.v-leave,
		&.v-enter-to {
			opacity: 1;
		}
		&.v-leave {
			pointer-events: none;
		}
		&.v-leave-active {
			transition: opacity 0.1s ease-out;
		}
		&.v-leave-to {
			opacity: 0;
		}
	}
}

.cabin_panels {
	min-width: 240px;
	display: flex;
	flex-grow: 1;
	align-items: stretch;
	justify-content: stretch;
	transition: min-width 0.1s ease-out;
}

</style>