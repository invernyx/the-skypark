<template>
	<div class="thoughts_grouped" v-if="human_states.length">

		<div v-for="(g, i) in groups" :key="i" class="thought_group">

			<div class="columns">
				<div class="column">
					<div class="name">{{ get_cat(g.category) }}</div>
				</div>
				<div class="column column_narrow">
					<div class="count_total">{{ g.total_count }}</div>
				</div>
			</div>

			<div class="buttons_list shadowed-shallow">
				<button_listed class="arrow thought" :simple="true" v-for="(t, i) in g.thoughts" :key="i">
					<div class="columns">
						<div class="column column_narrow">
							<div class="count">{{ t.humans.length }}x</div>
						</div>
						<div class="column">
							<div class="text">{{ t.text }}</div>
						</div>
					</div>
				</button_listed>
			</div>
		</div>

	</div>
	<div v-else>
		<p class="text-center">No Thoughts</p>
	</div>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import moment from 'moment';
import Vue from 'vue';
export default Vue.extend({
	props:{
		human_states :Array,
	},
	data() {
		return {
			Eljs: Eljs,
			moment: moment,
			categories: {
				comfort: "Comfort",
				bathroom: "Bathroom",
				nauseous: "Nauseous",
				food: "Food",
				energy: "Energy",
				energized: "Energized",
				neighbors: "Neighbors",
				entertainment: "Entertainment",
				turbulence: "Turbulence",
				sleeping: "Sleeping",
				joke: "Joke",
				work: "Work"
			}
		}
	},
	methods: {
		get_cat(name :string) {
			if(Object.keys(this.categories).includes(name)){
				return this.categories[name];
			}
			return "Unknown category";
		}
	},
	mounted() {
	},
	computed: {
		groups() {
			var groups = [];

			if(this.human_states){
				this.human_states.forEach((human :any) => {
					human.state.thoughts.forEach(thought => {
						const cat = thought[2].split('_')[0];
						let existing = groups.find(x => x.category == cat)
						if(!existing) {
							existing = {
								category: cat,
								thoughts: [],
								total_count: 0,
								humans: [],
							}
							groups.push(existing);
						}
						existing.humans.push(human);

						let existing_words = existing.thoughts.find(x => x.text == thought[1]);
						if(!existing_words) {
							existing_words = {
								text: thought[1],
								humans: []
							}
							existing.thoughts.push(existing_words);
						}
						existing_words.humans.push(human);
						existing.total_count++;

					});
				});
			}

			groups.forEach(g => {
				g.thoughts.sort((a,b) => {
					return b.humans.length - a.humans.length
				});
			});

			groups.sort((a, b) => {
				return b.total_count - a.total_count
			});

			return groups;
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.thoughts_grouped {
	.thought_group {
		.count {
			margin-right: 8px;
			font-family: "SkyOS-Bold";
		}
		.count_total {

		}
		.name {
			font-family: "SkyOS-Bold";
			font-size: 16px;
			line-height: 1.2em;
		}
		.buttons_list {
			margin-bottom: 8px;
			margin-top: 4px;
		}
	}
}

</style>