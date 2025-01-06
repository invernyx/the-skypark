<template>
	<div class="filters_sub" v-if="filters">
		<scroll_stack class="app-box shadowed-deep" :has_round_corners="true">
			<template v-slot:top>
				<div class="controls-top h_edge_padding">
					<div class="filter_entry">
						<div class="columns columns_margined_half">
							<div class="column column_narrow">
								<button_action class="shadowed-shallow icon icon-close" @click.native="$emit('close')"></button_action>
							</div>
							<div class="column"></div>
							<div class="column column_justify_center column_narrow">
								<h2>Sort</h2>
							</div>
						</div>
					</div>
				</div>
			</template>
			<template v-slot:content>
				<div class="filter_section h_edge_padding">

					<p class="text-center">Lorem ipsum dolor sit amet consectetur adipisicing elit.</p>


					<div class="filter_entry">
						<div class="buttons_list shadowed-shallow">
							<div class="columns columns_2">
								<div class="column column_2 column_h-stretch">
									<button_listed class="listed_h"
										:class="[{
											'selected': filters.sortAsc && (filters.sort != 'aircraft' && filters.sort != 'relevance'),
											'disabled': filters.sort == 'aircraft' || filters.sort == 'relevance',
										}]"
										@click.native="set_sort_asc(true)">Ascending</button_listed>
								</div>
								<div class="column column_2 column_h-stretch">
									<button_listed class="listed_h"
										:class="[{
											'selected': !filters.sortAsc && (filters.sort != 'aircraft' && filters.sort != 'relevance'),
											'disabled': filters.sort == 'aircraft' || filters.sort == 'relevance',
										}]"
										@click.native="set_sort_asc(false)">Descending</button_listed>
								</div>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<div class="buttons_list shadowed-shallow">
							<button_listed
								:class="[{ 'selected': filters.sort == 'relevance' }]"
								@click.native="set_sort('relevance')">Relevance</button_listed>
							<button_listed
								:class="[{ 'selected': filters.sort == 'topography_var' }]"
								@click.native="set_sort('topography_var')">Topography variations</button_listed>
							<button_listed
								:class="[{ 'selected': filters.sort == 'aircraft' }]"
								@click.native="set_sort('aircraft')">Aircraft location</button_listed>
							<button_listed
								:class="[{ 'selected': filters.sort == 'distance' }]"
								@click.native="set_sort('distance')">Distance</button_listed>
							<button_listed
								:class="[{ 'selected': filters.sort == 'ending' }]"
								@click.native="set_sort('ending')">Ending soon</button_listed>
							<button_listed
								:class="[{ 'selected': filters.sort == 'payload' }]"
								@click.native="set_sort('payload')">Payload</button_listed>
							<button_listed
								v-if="ui.tier == 'endeavour' && $os.system.isDev"
								:class="[{ 'selected': filters.sort == 'reward' }]"
								@click.native="set_sort('reward')">Pay</button_listed>
							<button_listed
								v-if="ui.tier == 'endeavour'"
								:class="[{ 'selected': filters.sort == 'xp' }]"
								@click.native="set_sort('xp')">XP</button_listed>
						</div>
					</div>

				</div>
			</template>
			<template v-slot:bottom>
				<div class="controls-bottom h_edge_padding">
					<div class="columns columns_margined_half">
						<div class="column column_narrow column_h-stretch">
							<button_action class="shadowed-shallow" @click.native="clear_and_reset()">Clear All</button_action>
						</div>
						<div class="column column_3 column_h-stretch">
							<button_action class="go shadowed-shallow" @click.native="search()">Search</button_action>
						</div>
					</div>
				</div>
			</template>
		</scroll_stack>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Filter from "../classes/filter";

export default Vue.extend({
	props: {
		filters: Filter,
	},
	data() {
		return {
			ui: {
				tier: this.$os.userConfig.get(['ui','tier']),
				illicit: this.$os.userConfig.get(['ui','illicit']) == '1',
			}
		}
	},
	methods: {
		changed() {
			this.$emit('changed', this.filters);
		},
		contracts_clear() {
			this.$emit('clear');
		},
		clear_and_reset() {
			this.$emit('reset');
		},
		search() {
			this.$emit('search', this.filters);
			this.$emit('close');
		},

		set_sort(cat: string){
			this.filters.sort != cat ? this.filters.sort = cat : this.filters.sort = 'relevance';
			switch(cat) {
				case 'ending': {
					if(this.filters.sort == cat) {
						this.set_sort_asc(true);
					}
					break;
				}
				case 'xp': {
					if(this.filters.sort == cat) {
						this.set_sort_asc(false);
					}
					break;
				}
				case 'reward': {
					if(this.filters.sort == cat) {
						this.set_sort_asc(false);
					}
					break;
				}
				case 'topography_var': {
					if(this.filters.sort == cat) {
						this.set_sort_asc(false);
					}
					break;
				}
				case 'payload': {
					if(this.filters.sort == cat) {
						this.set_sort_asc(false);
					}
					break;
				}
			}

			this.changed();
		},
		set_sort_asc(cat: boolean){
			this.filters.sortAsc = cat;
			this.changed();
		},

	},
	created() {
	},
	beforeDestroy() {
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.filters_sub {
	.theme--bright & {
		.filter {
			&_entry {
				.aircraft_type {
					.c {
						> span:first-child {
							opacity: 0.2;
						}
					}
					&_heli {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/dark/acf_heli.svg);
						}
					}
					&_ga {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/dark/acf_ga.svg);
						}
					}
					&_turbo {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/dark/acf_turbo.svg);
						}
					}
					&_jet {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/dark/acf_jet.svg);
						}
					}
					&_narrow {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/dark/acf_narrow.svg);
						}
					}
					&_wide {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/dark/acf_wide.svg);
						}
					}
					&.selected {
						.c span:first-child {
							opacity: 0.5;
						}
						&.aircraft_type_heli {
							.c span:first-child {
								background-image: url(../../../../sys/assets/icons/bright/acf_heli.svg);
							}
						}
						&.aircraft_type_ga {
							.c span:first-child {
								background-image: url(../../../../sys/assets/icons/bright/acf_ga.svg);
							}
						}
						&.aircraft_type_turbo {
							.c span:first-child {
								background-image: url(../../../../sys/assets/icons/bright/acf_turbo.svg);
							}
						}
						&.aircraft_type_jet {
							.c span:first-child {
								background-image: url(../../../../sys/assets/icons/bright/acf_jet.svg);
							}
						}
						&.aircraft_type_narrow {
							.c span:first-child {
								background-image: url(../../../../sys/assets/icons/bright/acf_narrow.svg);
							}
						}
						&.aircraft_type_wide {
							.c span:first-child {
								background-image: url(../../../../sys/assets/icons/bright/acf_wide.svg);
							}
						}
					}
				}
				.company_filter {
					&_clearsky {
						&.selected {
							.c {
								background-color: #F47F2E;
								background-image: url(../../../../sys/assets/icons/companies/bright/logo_clearsky.svg);
								opacity: 1;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/dark/logo_clearsky.svg);
							opacity: 0.5;
						}
					}
					&_coyote {
						&.selected {
							.c {
								background-color: #111;
								background-image: url(../../../../sys/assets/icons/companies/bright/logo_coyote.svg);
								opacity: 1;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/dark/logo_coyote.svg);
							opacity: 0.5;
						}
					}
					&_skyparktravel {
						&.selected {
							.c {
								background-color: #FB45A1;
								background-image: url(../../../../sys/assets/icons/companies/bright/logo_skyparktravel.svg);
								opacity: 1;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/dark/logo_skyparktravel.svg);
							opacity: 0.5;
						}
					}
					&_oceanicair {
						&.selected {
							.c {
								background-color: #104ba3;
								background-image: url(../../../../sys/assets/icons/companies/bright/logo_oceanicair.svg);
								opacity: 1;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/dark/logo_oceanicair.svg);
							opacity: 0.5;
						}
					}
					&_lastflight {
						&.selected {
							.c {
								background-color: #ba3000;
								background-image: url(../../../../sys/assets/icons/companies/bright/logo_lastflight.svg);
								opacity: 1;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/dark/logo_lastflight.svg);
							opacity: 0.5;
						}
					}

				}
			}
		}
	}

	.theme--dark & {
		.filter {
			&_entry {
				.aircraft_type {
					&_heli {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/bright/acf_heli.svg);
						}
					}
					&_ga {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/bright/acf_ga.svg);
						}
					}
					&_turbo {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/bright/acf_turbo.svg);
						}
					}
					&_jet {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/bright/acf_jet.svg);
						}
					}
					&_narrow {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/bright/acf_narrow.svg);
						}
					}
					&_wide {
						.c span:first-child {
							background-image: url(../../../../sys/assets/icons/bright/acf_wide.svg);
						}
					}
				}
				.company_filter {
					&_clearsky {
						&.selected {
							.c {
								background-color: #F47F2E;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/bright/logo_clearsky.svg);
						}
					}
					&_coyote {
						&.selected {
							.c {
								background-color: #111;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/bright/logo_coyote.svg);
						}
					}
					&_skyparktravel {
						&.selected {
							.c {
								background-color: #FB45A1;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/bright/logo_skyparktravel.svg);
						}
					}
					&_oceanicair {
						&.selected {
							.c {
								background-color: #104ba3;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/bright/logo_oceanicair.svg);
						}
					}
					&_lastflight {
						&.selected {
							.c {
								background-color: #ba3000;
							}
						}
						.c {
							background-image: url(../../../../sys/assets/icons/companies/bright/logo_lastflight.svg);
						}
					}
				}
			}
		}
	}

	.filter {
		&_section {
			padding-top: 0;
		}
		&_entry {
			padding-top: $edge-margin / 2;
			padding-bottom: $edge-margin / 2;
			&:first-child {
				padding-top: 0;
			}
			&:last-child {
				padding-bottom: 0;
			}
		}
	}

	.aircraft_type {
		position: relative;
		align-items: flex-start;
		overflow: hidden;
		text-align: left;
		height: 60px;
		@include shadowed_shallow($ui_colors_dark_shade_2);
		.c {
			span:first-child {
				position: absolute;
				bottom: 0;
				right: 0;
				margin-right: -12px;
				margin-bottom: -30px;
				width: 90px;
				height: 90px;
				display: block;
				opacity: 0.5;
				background-repeat: no-repeat;
				background-position: center;
				background-size: contain;
				transform: rotate(25deg);
				mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 0) 100%);
			}
		}
	}
	.company_filter {
		position: relative;
		align-items: flex-end;
		overflow: hidden;
		padding: 4px;
		padding-left: 1px;
		height: 65px;
		@include shadowed_shallow($ui_colors_dark_shade_2);
		.c {
			display: flex;
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			display: flex;
			justify-content: center;
			align-items: flex-end;
			background-position: center;
			background-repeat: no-repeat;
			background-size: 40px;
			background-position: center 6px;
			border-radius: 4px;
			transition: background-color 0.1s ease-out;
			padding: 4px 8px;
			text-align: center;
			& > span {

			}
		}
	}
}
</style>