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
							<div class="column column_justify_center column_narrow nowrap">
								<h2>Flight type</h2>
							</div>
						</div>
					</div>
				</div>
			</template>
			<template v-slot:content>
				<div class="filter_section h_edge_padding">

					<p class="text-center">Lorem ipsum dolor sit amet consectetur adipisicing elit.</p>

					<div class="filter_entry">
						<div class="columns columns_margined">
							<div class="column column_2 column_h-stretch">
								<button_listed class="company_filter company_filter_clearsky" :class="[{ 'selected': !filters.companies.includes('clearsky') }]" @click.native="set_company('clearsky')">
									<span class="c">
										<span></span>
										<span>ClearSky</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_2 column_h-stretch" v-if="ui.illicit == '1'">
								<button_listed class="company_filter company_filter_coyote" :class="[{ 'selected': !filters.companies.includes('coyote') }]" @click.native="set_company('coyote')">
									<span class="c">
										<span></span>
										<span>Coyote</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_2 column_h-stretch">
								<button_listed class="company_filter company_filter_skyparktravel" :class="[{ 'selected': !filters.companies.includes('skyparktravel') }]" @click.native="set_company('skyparktravel')">
									<span class="c">
										<span></span>
										<span>Travels</span>
									</span>
								</button_listed>
							</div>
						</div>
					</div>
					<div class="filter_entry">
						<div class="columns columns_margined">
							<div class="column column_2 column_h-stretch">
								<button_listed class="company_filter company_filter_oceanicair" :class="[{ 'selected': !filters.companies.includes('oceanicair') }]" @click.native="set_company('oceanicair')">
									<span class="c">
										<span></span>
										<span>Oceanic Air</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_2 column_h-stretch">
								<button_listed class="company_filter company_filter_lastflight" :class="[{ 'selected': !filters.companies.includes('lastflight') }]" @click.native="set_company('lastflight')">
									<span class="c">
										<span></span>
										<span>Last Flight</span>
									</span>
								</button_listed>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<div class="columns columns_margined">
							<div class="column column_2 column_h-stretch">
								<div class="buttons_list shadowed-shallow">
									<button_listed :class="[{ 'selected': filters.type == 'any' }]" @click.native="set_first('any')">Any</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Cargo' }]" @click.native="set_first('Cargo')">Cargo</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Passengers' }]" @click.native="set_first('Passengers')">Passengers</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Rescue' }]" @click.native="set_first('Rescue')">Rescue</button_listed>
								</div>
							</div>
							<div class="column column_2 column_h-stretch">
								<div class="buttons_list shadowed-shallow">
									<button_listed :class="[{ 'selected': filters.type == 'Tour' }]" @click.native="set_first('Tour')">Tours</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Experience' }]" @click.native="set_first('Experience')">Experiences</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Ferry' }]" @click.native="set_first('Ferry')">Ferries</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Bush Trip' }]" @click.native="set_first('Bush Trip')">Bush trips</button_listed>
								</div>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<div class="columns columns_2 columns_margined">
							<div class="column column_2 column_h-stretch">
								<toggle v-model="filters.onlyCustomContracts" class="gold">Only show custom contracts</toggle>
							</div>
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
				illicit: this.$os.userConfig.get(['ui','illicit']) == '1',
			},
			filterReferences: {
				types: [0,1,2,3,4,5],
				companies: ['clearsky','coyote','skyparktravel','oceanicair','lastflight'], //'offduty','lastflight','orbit','fliteline'
			},
		}
	},
	methods: {
		set_query(query :string) {
			this.filters.query = query;
		},
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

		set_company(company: string) {
			if(this.filters.companies.length == 0) {
				Object.assign(this.filters.companies, this.filterReferences.companies);
				this.filters.companies.splice(this.filters.companies.indexOf(company), 1);
			} else {
				if(this.filters.companies.includes(company)) {
					this.filters.companies.splice(this.filters.companies.indexOf(company), 1);
				} else {
					this.filters.companies.push(company);
				}
			}
			this.changed();
		},
		set_first(cat: string){
			this.filters.type != cat ? this.filters.type = cat : this.filters.type = 'any';  this.filters.subType = 'any';
			this.changed();
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.ui.illicit = wsmsg.payload.set.illicit == '1';
							break;
						}
					}
					break;
				}
			}
		},
		listener_config(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'ui': {
					switch(wsmsg.name[1]){
						case 'illicit': {
							this.ui.illicit = wsmsg.payload;
							break;
						}
					}
					break;
				}
			}
		},
	},
	created() {
		this.$os.eventsBus.Bus.on('configchange', this.listener_config);
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('configchange', this.listener_config);
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
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