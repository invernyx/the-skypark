<template>
	<div v-if="aircraft">
		<div class="app-box shadowed small nooverflow content">
			<div class="h_edge_margin">

				<div class="columns columns_margined h_edge_margin_top">
					<div class="column column_narrow column_justify_center">
						<div class="aircraft-type" :class="'aircraft-type-' + aircraft.size"></div>
					</div>
					<div class="column">
						<data_stack :label="aircraft.manufacturer + ' / ' + aircraft.creator" class="start" v-if="aircraft">
							{{ aircraft.model }}
						</data_stack>
					</div>
				</div>

				<div class="columns columns_margined h_edge_margin_top">
					<div class="column">
						<data_stack label="Wingspan" class="start" v-if="aircraft">
							<length :amount="aircraft.wingspan" />
						</data_stack>
					</div>
					<div class="column">
						<data_stack label="Max Load" class="start" v-if="aircraft">
							<weight :amount="aircraft.max_weight - aircraft.empty_weight" />
						</data_stack>
					</div>
					<div class="column">
						<data_stack label="Duty Hours" class="start" v-if="aircraft">
							<duration :time="aircraft.duty_flight_hours" :decimals="1"/>
						</data_stack>
					</div>
				</div>

			</div>
		</div>

		<div class="app-box shadowed small nooverflow content">
			<div class="h_edge_margin">

				<div class="columns columns_margined">
					<div class="column">
						<data_stack :label="aircraft.location ? (aircraft.location[1].toFixed(8) + ', ' + aircraft.location[0].toFixed(8)) : 'No aircraft relocation fee on your first flight.'" class="start" v-if="aircraft">
							<span v-if="aircraft.nearest_airport">{{ aircraft.nearest_airport.icao }} - <strong>{{ aircraft.nearest_airport.name }}</strong></span>
							<span v-else-if="aircraft.location">Off-field</span>
							<span v-else>Brand new, sealed box</span>
						</data_stack>
					</div>
					<div class="column column_narrow column_justify_center">
						<button_action class="go" v-if="aircraft.location" @click.native="copyLocation">Copy</button_action>
					</div>
				</div>

			</div>
		</div>

		<div class="app-box shadowed small nooverflow content" v-if="!cabin_editor">
			<div class="h_edge_margin">

				<div class="columns h_edge_margin_bottom">
					<div class="column">
						<h2>Cabins</h2>
					</div>
					<div class="column column_narrow h_non-break">
						<p class="notice">Per livery</p>
					</div>
				</div>

				<div class="buttons_list shadowed-shallow" v-if="current_cabin">
					<button_listed @click.native="cabin_edit(current_cabin)" class="selected">
						<template>{{ current_cabin.livery }}</template>
						<template v-slot:right>
							<span class="h_non-break">
								<strong>{{ current_cabin.get_feature_count('seat') }}</strong> Seats, <strong>{{ current_cabin.get_feature_count('cargo') }}</strong> Cargo
							</span>
						</template>
					</button_listed>
				</div>

				<div class="buttons_list shadowed-shallow" v-if="available_cabins.length || current_cabin">
					<button_listed v-for="cabin in available_cabins" :key="cabin.livery" @click.native="cabin_edit(cabin)">
						<template>{{ cabin.livery }}</template>
						<template v-slot:right>
							<span class="h_non-break">
								<strong>{{ cabin.get_feature_count('seat') }}</strong> Seats, <strong>{{ cabin.get_feature_count('cargo') }}</strong> Cargo
							</span>
						</template>
					</button_listed>
				</div>
				<div v-else>
					<p>To configure a new cabin for this aircraft, first load into the desired aircraft and livery in-sim.</p>
				</div>

			</div>
		</div>
		<div class="app-box shadowed small nooverflow content" v-else>
			<div class="h_edge_margin">

				<div class="columns columns_margined h_edge_margin_bottom">
					<div class="column column_narrow h_non-break">
						<button_nav shape="back" @click.native="cabin_edit(null)">Cabins</button_nav>
					</div>
					<div class="column">
						<h2>{{ cabin_editor.livery }}</h2>
					</div>
				</div>

				<div class="cabin-levels columns columns_margined_half h_edge_margin_bottom" v-if="cabin_editor.levels.length > 1">
					<div class="column" v-for="(l, i) in get_levels()" :key="i">
						<button_action @click.native="set_level(l)" class="listed map-control" :class="{ 'info': cabin_state.level == l }">{{ l + 1 }}. {{ cabin_editor.get_level_label(l) }}</button_action>
					</div>
				</div>

				<Cabin :aircraft="aircraft" :cabin="cabin_editor" :state="cabin_state" />

			</div>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Aircraft from '@/sys/classes/aircraft';
import Eljs from '@/sys/libraries/elem';
import Cabin from '@/sys/components/cabin/aircraft_cabin.vue';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';

export default Vue.extend({
	props: {
		app: AppInfo,
		aircraft: Aircraft,
	},
	components: {
		Cabin
	},
	data() {
		return {
			open: false,
			sim_live: this.$os.simulator.live,
			sid: this.app.vendor + '_' + this.app.ident,
			cabin_editor: null,
			cabin_state: new AircraftCabinState(),
		}
	},
	methods: {

		cabin_edit(cabin) {
			if(cabin) {
				this.cabin_editor = cabin;
				this.cabin_state =  new AircraftCabinState();
				this.cabin_state.ui_mode = 'edit';
			} else {
				this.cabin_editor = null;
				this.cabin_state = null;
			}
			this.app.events.emitter.emit('set_cabin_editor', {
				cabin: cabin,
				state: this.cabin_state,
			})
		},

		set_level(index :number) {
			this.cabin_state.level = index;
		},

		get_levels() {
			const levels = [];
			const length = this.cabin_editor.levels.length;

			this.cabin_editor.levels.forEach((level, index) => {
				levels.push(length - index - 1);
			});

			return levels;
		},

		copyLocation() {
			if(this.aircraft.nearest_airport) {
				Eljs.copyTextToClipboard(this.aircraft.nearest_airport.icao);
				this.$os.modals.add({
					type: 'notify',
					title: this.aircraft.nearest_airport.icao,
					text: [
						this.aircraft.nearest_airport.icao +' has been copied to your clipboard.',
						'You can paste this ICAO in the search field of the MSFS world map.'
					],
				});
			} else {
				Eljs.copyTextToClipboard(this.aircraft.location[1] + ', ' + this.aircraft.location[0]);
				this.$os.modals.add({
					type: 'notify',
					title: 'Off-field',
					text: [
						'Coordinates have been copied.',
						this.aircraft.location[1] + ', ' + this.aircraft.location[0],
						'You can paste this location in the search field of the MSFS world map.',
						'Be aware that MSFS does not allow you to start on the ground when using coordinates.'
					],
				});
			}

		},

		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.sim_live = wsmsg.payload;
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'uncover': {
					this.open = wsmsg.payload;
					break;
				}
				case 'covered': {
					this.open = wsmsg.payload;
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	computed: {
		current_cabin() {
			if(this.$os.fleetService.aircraft_current) {
				const is_current = this.$os.fleetService.aircraft_current.id == this.aircraft.id;
				return is_current ? this.aircraft.cabin : null;
			} else {
				return null;
			}
		},
		available_cabins() {
			return this.aircraft.cabins.filter(x => x.get_feature_count('seat') + x.get_feature_count('cargo') > 0);
		},
	},
	watch: {
		aircraft: {
			immediate: true,
			handler(newValue, oldValue) {
				this.cabin_editor = null;
			}
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.content {
	opacity: 0;
	pointer-events: none;
	transition: opacity 0.2s ease-out;

	.is-open & {
		opacity: 1;
		pointer-events: all;
	}

	.loading & {
		opacity: 0;
		pointer-events: none;
	}

	.background-gradient {
		position: absolute;
		top: 0;
		right: 0;
		left: 0;
		height: 300px;
	}

	.background-layer {
		position: relative;
		z-index: 2;
	}

	.aircraft {
		white-space: nowrap;
	}

	.description {
		font-size: 16px;
		line-height: 1.4em;
		white-space: pre-wrap;
	}

	.shaded {
		border-radius: 20px;
		padding: 12px;
	}

	.aircraft-type {
		width: 36px;
		height: 36px;
		background-position: center;
		background-size: contain;
		background-repeat: no-repeat;
	}

	.cabin-levels {
		z-index: 10;
		.button_action  {
			border-radius: 0.4em;
			margin-bottom: 4px;
		}
	}

	.theme--bright &,
	&.theme--bright {
		.shaded {
			background: $ui_colors_bright_shade_0;
			//box-shadow: 0 3px 8px rgba($ui_colors_bright_shade_5, 0.2);
			@include shadowed_shallow($ui_colors_bright_shade_5);
		}
		.aircraft-type {
			&-0 {
				background-image: url(../../../../sys/assets/icons/dark/acf_heli.svg);
			}
			&-1 {
				background-image: url(../../../../sys/assets/icons/dark/acf_ga.svg);
			}
			&-2 {
				background-image: url(../../../../sys/assets/icons/dark/acf_turbo.svg);
			}
			&-3 {
				background-image: url(../../../../sys/assets/icons/dark/acf_jet.svg);
			}
			&-4 {
				background-image: url(../../../../sys/assets/icons/dark/acf_narrow.svg);
			}
			&-5 {
				background-image: url(../../../../sys/assets/icons/dark/acf_wide.svg);
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		.shaded {
			background: rgba($ui_colors_dark_shade_0, 0.5);
		}
		.aircraft-type {
			&-0 {
				background-image: url(../../../../sys/assets/icons/bright/acf_heli.svg);
			}
			&-1 {
				background-image: url(../../../../sys/assets/icons/bright/acf_ga.svg);
			}
			&-2 {
				background-image: url(../../../../sys/assets/icons/bright/acf_turbo.svg);
			}
			&-3 {
				background-image: url(../../../../sys/assets/icons/bright/acf_jet.svg);
			}
			&-4 {
				background-image: url(../../../../sys/assets/icons/bright/acf_narrow.svg);
			}
			&-5 {
				background-image: url(../../../../sys/assets/icons/bright/acf_wide.svg);
			}
		}
	}

}

</style>