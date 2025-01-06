<template>
	<scroll_stack :sid="'p42_aircraft_cabin'" class="cabin_data cabin_data_primary columns">
		<template v-slot:top>
			<div class="controls-top h_edge_padding">
				<div class="columns">
					<div class="column">
						{{ cabin.livery }}
					</div>
					<div class="column column_narrow">
						<button_nav class="no-wrap" @click.native="$emit('edit')">Edit</button_nav>
					</div>
				</div>
			</div>
		</template>
		<template v-slot:content>
			<div class="h_edge_margin_lateral h_edge_margin_bottom">

				<div class="columns columns_margined h_edge_margin_bottom">
					<div class="column">
						<data_stack class="center" label="Souls">
							<number :amount="state.humans.filter(x => x.state.boarded).length" :decimals="0"/>
						</data_stack>
					</div>
					<div class="column">
						<data_stack class="center" label="Passenger Seats">
							<number :amount="cabin.features.filter(f => f.type == 'seat' && f.sub_type != 'jumpseat' && f.sub_type != 'pilot' && f.sub_type != 'copilot').length" :decimals="0"/>
						</data_stack>
					</div>
				</div>

				<div class="h_edge_padding_bottom" v-if="state.humans.length">
					<collapser :default="false">
						<template v-slot:title>
							<div>

								<div class="columns">
									<div class="column column_1">
										<strong>Happiness</strong>
									</div>
									<div class="column column_1 column_justify_center">
										<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.happiness))" :ranges="[{min:0,max:40,class:'red'},{min:50,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
									</div>
								</div>

								<div class="columns">
									<div class="column column_1">
										<strong>Health</strong>
									</div>
									<div class="column column_1 column_justify_center">
										<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.health))" :ranges="[{min:0,max:40,class:'red'}]" :blink="[{min:0,max:10}]" />
									</div>
								</div>

								<div class="columns">
									<div class="column column_1">
										<strong>Comfort</strong>
									</div>
									<div class="column column_1 column_justify_center">
										<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.comfort))" :ranges="[{min:0,max:30,class:'red'}]" :blink="[{min:0,max:10}]" />
									</div>
								</div>

								<div class="columns">
									<div class="column column_1">
										<strong>Fear</strong>
									</div>
									<div class="column column_1 column_justify_center">
										<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).filter(x => x.state.boarded).map(h => h.state.fear))" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
									</div>
								</div>
								<div class="columns">
									<div class="column column_1">
										<strong>Nausea</strong>
									</div>
									<div class="column column_1 column_justify_center">
										<progress_bar class="deep small" :percent="average(state.humans.map(h => h.state.nausea))" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
									</div>
								</div>

							</div>
						</template>
						<template v-slot:content>

							<div class="columns">
								<div class="column column_1">
									<strong>Entertained</strong>
								</div>
								<div class="column column_1 column_justify_center">
									<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.entertained))" :ranges="[{min:0,max:10,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
								</div>
							</div>

							<div class="columns">
								<div class="column column_1">
									<strong>Energy</strong>
								</div>
								<div class="column column_1 column_justify_center">
									<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.energy))" :ranges="[{min:0,max:20,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
								</div>
							</div>

							<div class="columns">
								<div class="column column_1">
									<strong>Patience</strong>
								</div>
								<div class="column column_1 column_justify_center">
									<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.patience))" :ranges="[{min:0,max:20,class:'red'},{min:60,max:100,class:'green'}]" :blink="[{min:0,max:10}]" />
								</div>
							</div>

							<div class="columns">
								<div class="column column_1">
									<strong>Hunger</strong>
								</div>
								<div class="column column_1 column_justify_center">
									<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.hunger))" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
								</div>
							</div>

							<div class="columns">
								<div class="column column_1">
									<strong>Thirst</strong>
								</div>
								<div class="column column_1 column_justify_center">
									<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.thirst))" :ranges="[{min:60,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
								</div>
							</div>

							<div class="columns">
								<div class="column column_1">
									<strong>Bathroom</strong>
								</div>
								<div class="column column_1 column_justify_center">
									<progress_bar class="deep small" :percent="average(state.humans.filter(x => x.state.boarded).map(h => h.state.bathroom))" :ranges="[{min:80,max:100,class:'red'}]" :blink="[{min:90,max:100}]" />
								</div>
							</div>
						</template>
					</collapser>
				</div>

				<div class="h_edge_padding_bottom" v-if="state.humans.length">
					<collapser :withArrow="true" :default="false">
						<template v-slot:title>
							<div class="h_edge_padding_bottom_half">
								<div class="columns">
									<div class="column">
										<h3 class="separator h_edge_margin_bottom_none">Seatbelts</h3>
									</div>
									<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
										<div class="collapser_arrow"></div>
									</div>
								</div>
							</div>
						</template>
						<template v-slot:content>
							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_4 column_narrow column_h-stretch">
										<button_listed class="listed_h" :class="[{ 'selected': cabin.seatbelts_behavior == 0 }]" @click.native="set_seatbelts(0)">Off</button_listed>
									</div>
									<div class="column column_4 column_narrow column_h-stretch">
										<button_listed class="listed_h" :class="[{ 'selected':  cabin.seatbelts_behavior == 1 }]" @click.native="set_seatbelts(1)">On</button_listed>
									</div>
									<div class="column column_4 column_h-stretch">
										<button_listed class="listed_h" :class="[{ 'selected': cabin.seatbelts_behavior == 2 }]" @click.native="set_seatbelts(2)">Auto&nbsp;({{ cabin.seatbelts_state ? 'On' : 'Off' }})</button_listed>
									</div>
								</div>
							</div>
						</template>
					</collapser>
				</div>

				<div class="h_edge_padding_bottom" v-if="state.humans.length">
					<collapser :withArrow="true" :default="false">
						<template v-slot:title>
							<div class="h_edge_padding_bottom_half">
								<div class="columns">
									<div class="column">
										<h3 class="separator h_edge_margin_bottom_none">Top Metrics</h3>
									</div>
									<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
										<div class="collapser_arrow"></div>
									</div>
								</div>
							</div>
						</template>
						<template v-slot:content>

							<div class="columns columns_margined_half h_edge_margin_bottom_half">
								<div class="column">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="sort_metric">
											<option value="happiness">Happiness</option>
											<option value="health">Health</option>
											<option value="comfort">Comfort</option>
											<option value="nausea">Nausea</option>
											<option value="fear">Fear</option>
											<option value="entertained">Entertained</option>
											<option value="energy">Energy</option>
											<option value="patience">Patience</option>
											<option value="hunger">Hunger</option>
											<option value="thirst">Thirst</option>
											<option value="bathroom">Bathroom</option>
										</selector>
									</div>
								</div>
								<div class="column">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="sort_order">
											<option value="highest">Highest</option>
											<option value="lowest">Lowest</option>
										</selector>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow">
								<AircraftCabinStrip
									v-for="(human) in state.humans.filter(x => x.state.boarded).slice().sort((x1, x2) => sort_order == 'highest' ? (x2.state[sort_metric] - x1.state[sort_metric]) : (x1.state[sort_metric] - x2.state[sort_metric]) ).slice(0, 10)"
									v-bind:key="human.first_name + ' - ' + human.last_name"
									:cabin="cabin"
									:human="human"
									:metric="sort_metric"
									@open="open(human)"
								/>
							</div>

						</template>
					</collapser>

				</div>

				<div class="h_edge_padding_bottom" v-if="state.humans.length">
					<collapser :withArrow="true" :default="false">
						<template v-slot:title>
							<div class="h_edge_padding_bottom_half">
								<div class="columns">
									<div class="column">
										<h3 class="separator h_edge_margin_bottom_none">Actions</h3>
									</div>
									<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
										<div class="collapser_arrow"></div>
									</div>
								</div>
							</div>
						</template>
						<template v-slot:content>

							<p class="text-center">Be careful what you wish for!</p>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('perfect', null)">Make everything perfect</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('fart', null)">Throw a fart bomb</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('hunger-', null)">Hunger--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('hunger+', null)">Hunger++</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('thirst-', null)">Thirst--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('thirst+', null)">Thirst++</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('bathroom-', null)">Bathroom--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('bathroom+', null)">Bathroom++</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('fear-', null)">Fear--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('fear+', null)">Fear++</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('health-', null)">Health--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('health+', null)">Health++</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('energy-', null)">Energy--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('energy+', null)">Energy++</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('patience-', null)">Patience--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('patience+', null)">Patience++</button_listed>
									</div>
								</div>
							</div>

							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('entertained-', null)">Entertained--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('entertained+', null)">Entertained++</button_listed>
									</div>
								</div>
							</div>
							<div class="buttons_list shadowed-shallow ">
								<div class="columns">
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('comfort-', null)">Comfort--</button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="listed_h" @click.native="send_event('comfort+', null)">Comfort++</button_listed>
									</div>
								</div>
							</div>

						</template>
					</collapser>
				</div>

				<div class="h_edge_padding_bottom" v-if="state.humans.length">
					<collapser :withArrow="true" :default="false">
						<template v-slot:title>
							<div class="h_edge_padding_bottom_half">
								<div class="columns">
									<div class="column">
										<h3 class="separator h_edge_margin_bottom_none">Thoughts</h3>
									</div>
									<div class="column column_narrow column_justify_center h_edge_padding_lateral_half">
										<div class="collapser_arrow"></div>
									</div>
								</div>
							</div>
						</template>
						<template v-slot:content>

							<Human_thoughts_grouped :human_states="state.humans"/>

						</template>
					</collapser>
				</div>


			</div>
		</template>
	</scroll_stack>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';
import AircraftCabinStrip from './aircraft_cabin_strip.vue'
import AircraftCabinFeature from '@/sys/classes/cabin/aircraft_cabin_feature';
import Human_thoughts_grouped from './human_thoughts_grouped.vue';

export default Vue.extend({
	props:{
		aircraft :Aircraft,
		cabin :Object as () => AircraftCabin,
		state: Object as () => AircraftCabinState,
	},
	components: {
		AircraftCabinStrip,
		Human_thoughts_grouped
	},
	data() {
		return {
			sort_metric: 'happiness',
			sort_order: 'highest',
		}
	},
	mounted() {
	},
	methods: {
		open(human) {
			this.$emit('open_human', human);
		},
		average(array) {
			return array.length ? (array.reduce((a, b) => a + b) / array.length) : 0;
		},
		set_seatbelts(state :number) {
			this.send_event('seatbelts_behavior', { 'state': state }, null);
		},
		send_event(event :string, data :any, feature :AircraftCabinFeature = null) {
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					verb: 'TriggerEvent',
					livery: this.cabin.livery,
					guid: feature ? feature.guid : undefined,
					event: event,
					data: data
				},
				(response: any) => {}
			);
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
}

</style>