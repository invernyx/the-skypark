<template>
	<scroll_stack :sid="'p42_aircraft_cabin'" class="cabin_data cabin_data_primary columns" v-if="human">
		<template v-slot:top>
			<div class="controls-top h_edge_padding">
				<div class="columns">
					<div class="column column_narrow">
						<button_nav class="no-wrap shadowed-shallow" shape="back" @click.native="$emit('close')">Done</button_nav>
					</div>
					<div class="column column_justify_center column_align_end h_edge_padding_right_half scroll-fade-in">
						<strong>{{ human.first_name }} {{ human.last_name }}</strong>
					</div>
				</div>
			</div>
		</template>
		<template v-slot:content>
			<div class="h_edge_margin_lateral h_edge_margin_bottom">

				<div class="text-center h_edge_margin_bottom">

					<Human :human="human" :cabin="cabin" :state="state" :show_actions="true" :style="{ '--human-icon-size': '100px' }"></Human>

					<!--
						'window_left': x == (1 + cabin.levels[state.level][2]),
						'window_right': x == (cabin.levels[state.level][0] + cabin.levels[state.level][2]),
					-->

					<h2 class="nm">{{ human.first_name }} {{ human.last_name }}</h2>
					<div class="columns h_edge_margin_bottom_half">
						<div class="column"><p class="notice">{{ human.occupation }}</p></div>
					</div>
					<p class="notice">
						<weight :amount="human.weight" :decimals="0"/><span class="dot"></span><span><number :amount="human.age" :decimals="0"/> years old</span>
					</p>
					<h3 class="nm">{{ get_state() }}<span v-if="human.state.sub_action != 'none'">, {{ get_sub_state() }}</span></h3>
				</div>

				<div>
					<div class="columns">
						<div class="column column_1">
							<strong>Happiness</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.happiness_trend === true,  'down': human.state.happiness_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.happiness" :ranges="[{min:0,max:40,class:'red'},{min:50,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Health</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.health_trend === true,  'down': human.state.health_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.health" :ranges="[{min:0,max:40,class:'red'}]" :blink="[{min:0,max:10}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Comfort</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.comfort_trend === true,  'down': human.state.comfort_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.comfort" :ranges="[{min:0,max:30,class:'red'}]" :blink="[{min:0,max:10}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Nausea</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.nausea_trend === true,  'down': human.state.nausea_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.nausea" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Fear</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.fear_trend === true,  'down': human.state.fear_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.fear" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Entertained</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.energy_trend === true,  'down': human.state.energy_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.entertained" :ranges="[{min:0,max:10,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Energy</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.energy_trend === true,  'down': human.state.energy_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.energy" :ranges="[{min:0,max:20,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Patience</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.patience_trend === true,  'down': human.state.patience_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.patience" :ranges="[{min:0,max:20,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Hunger</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.hunger_trend === true,  'down': human.state.hunger_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.hunger" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Thirst</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.thirst_trend === true,  'down': human.state.thirst_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.thirst" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
						</div>
					</div>

					<div class="columns">
						<div class="column column_1">
							<strong>Bathroom</strong>
						</div>
						<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
							<div class="trend" :class="{ 'up': human.state.bathroom_trend === true,  'down': human.state.bathroom_trend === false }"></div>
						</div>
						<div class="column column_1 column_justify_center">
							<progress_bar class="deep small" :percent="human.state.bathroom" :ranges="[{min:80,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
						</div>
					</div>
				</div>

				<h3 class="separator">Thoughts</h3>
				<Human_thoughts :thoughts="human.state.thoughts"/>

			</div>
		</template>
	</scroll_stack>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';
import Human from './human.vue';
import Human_thoughts from './human_thoughts.vue';

export default Vue.extend({
	props:{
		aircraft :Aircraft,
		cabin :Object as () => AircraftCabin,
		state: Object as () => AircraftCabinState,
		human :Object as () => any,
	},
	components: {
    Human,
    Human_thoughts
},
	data() {
		return {
		}
	},
	mounted() {
	},
	methods: {
		get_state() {
			return AircraftCabin.human_state[this.human.state.action]
		},
		get_sub_state() {
			return AircraftCabin.human_sub_state[this.human.state.sub_action]
		},
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.cabin_data {
	flex-grow: 1;

	.trend {
		width: 16px;
		height: 16px;
		&.up {
			background-image: url(../../../sys/assets/icons/bright/arrow_up.svg);
		}
		&.down {
			background-image: url(../../../sys/assets/icons/bright/arrow_down.svg);
		}
	}

	.thoughts {
		margin-top: 16px;
	}
}

</style>