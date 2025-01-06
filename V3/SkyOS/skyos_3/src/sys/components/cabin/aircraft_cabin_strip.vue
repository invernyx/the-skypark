<template>
	<button_listed class="arrow" :simple="true" @click.native="open()">
		<div class="columns core">
			<div class="column column_narrow">
				<div
					class="human-icon"
					:class="[
						'human-icon-' + human.state.icon,
						'human-icon-type-' + human.type,
						'human-icon-action-' + human.state.action,
						'human-icon-sub-' + human.state.sub_action,
						{
							'human-icon-window-right': human.state.x == 0 && human.state.action == 'seated' ,
							'human-icon-window-left': human.state.x == cabin.levels[human.state.z][0] - 1 && human.state.action == 'seated' ,
						}
					]">
					<div>
						<div></div>
					</div>
				</div>
			</div>
			<div class="column column_justify_center">

				<div class="columns">
					<div class="column column_2">
						<strong>{{ human.first_name }}</strong>
					</div>
					<div class="column column_1 column_justify_center" v-if="metric == 'happiness'">
						<progress_bar class="deep small" :percent="human.state.happiness" :ranges="[{min:0,max:40,class:'red'},{min:50,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'health'">
						<progress_bar class="deep small" :percent="human.state.health" :ranges="[{min:0,max:40,class:'red'}]" :blink="[{min:0,max:10}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'comfort'">
						<progress_bar class="deep small" :percent="human.state.comfort" :ranges="[{min:0,max:30,class:'red'}]" :blink="[{min:0,max:10}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'nausea'">
						<progress_bar class="deep small" :percent="human.state.nausea" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'fear'">
						<progress_bar class="deep small" :percent="human.state.fear" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'entertained'">
						<progress_bar class="deep small" :percent="human.state.entertained" :ranges="[{min:0,max:10,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:90,max:100}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'energy'">
						<progress_bar class="deep small" :percent="human.state.energy" :ranges="[{min:0,max:20,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'patience'">
						<progress_bar class="deep small" :percent="human.state.patience" :ranges="[{min:0,max:20,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'hunger'">
						<progress_bar class="deep small" :percent="human.state.hunger" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'thirst'">
						<progress_bar class="deep small" :percent="human.state.thirst" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
					</div>
					<div class="column column_1 column_justify_center" v-else-if="metric == 'bathroom'">
						<progress_bar class="deep small" :percent="human.state.bathroom" :ranges="[{min:80,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
					</div>
				</div>


			</div>
		</div>
	</button_listed>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';

export default Vue.extend({
	props:{
		cabin :Object as () => AircraftCabin,
		human :Object as () => any,
		metric :String,
	},
	components: {
	},
	data() {
		return {
		}
	},
	mounted() {
	},
	methods: {
		open() {
			this.$emit('open', this.human);
		},
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

$human-icon-size: 30px;
@import './human-icon.scss';

.core {
	flex-grow: 1;
}

.human-icon {
	margin-right: 10px;
	margin-top: 2px;
	margin-bottom: 2px;
}

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

</style>