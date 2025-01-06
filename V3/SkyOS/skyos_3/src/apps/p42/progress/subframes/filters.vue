<template>
	<div class="filters" v-if="filters">
		<scroll_stack class="app-box shadowed-deep" :has_round_corners="true">
			<template v-slot:top>
				<div class="controls-top h_edge_padding">
					<div class="filter_entry">
						<textbox class="shadowed-shallow" type="text" placeholder="Smart Search" placeholder_focused="(ID, ICAOs, whatever else)" v-model="filters.query" @changed="changed()" @returned="set_query"></textbox>
					</div>
				</div>
			</template>
			<template v-slot:content>
				<!--
					Off-duty? burger runs
					ClearSky
					Coyote
					Skypark Travels
					LastFlight
					Orbit
					Flightline
				-->
				<div class="filter_section h_edge_padding">

					<div class="filter_entry">
						<div class="columns columns_margined">
							<div class="column column_3 column_h-stretch">
								<button_listed class="aircraft_type aircraft_type_heli" :class="[{ 'selected': !filters.types.includes(0) }]" @click.native="set_type(0)">
									<span class="c">
										<span></span>
										<span>Helis</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_3 column_h-stretch">
								<button_listed class="aircraft_type aircraft_type_ga" :class="[{ 'selected': !filters.types.includes(1) }]" @click.native="set_type(1)">
									<span class="c">
										<span></span>
										<span>Piston</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_3 column_h-stretch">
								<button_listed class="aircraft_type aircraft_type_turbo" :class="[{ 'selected': !filters.types.includes(2) }]" @click.native="set_type(2)">
									<span class="c">
										<span></span>
										<span>Turbo</span>
									</span>
								</button_listed>
							</div>
						</div>
					</div>
					<div class="filter_entry">
						<div class="columns columns_margined">
							<div class="column column_3 column_h-stretch">
								<button_listed class="aircraft_type aircraft_type_jet" :class="[{ 'selected': !filters.types.includes(3) }]" @click.native="set_type(3)">
									<span class="c">
										<span></span>
										<span>Biz Jets</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_3 column_h-stretch">
								<button_listed class="aircraft_type aircraft_type_narrow" :class="[{ 'selected': !filters.types.includes(4) }]" @click.native="set_type(4)">
									<span class="c">
										<span></span>
										<span>Narrow</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_3 column_h-stretch">
								<button_listed class="aircraft_type aircraft_type_wide" :class="[{ 'selected': !filters.types.includes(5) }]" @click.native="set_type(5)">
									<span class="c">
										<span></span>
										<span>Wide</span>
									</span>
								</button_listed>
							</div>
						</div>
					</div>

					<!-- companies: ['offduty','clearsky','coyote','skyparktravel','lastflight','orbit','fliteline'], company_filter -->

					<div class="filter_entry">
						<div class="columns columns_margined">
							<div class="column column_3 column_h-stretch">
								<button_listed class="company_filter company_filter_clearsky" :class="[{ 'selected': !filters.companies.includes('clearsky') }]" @click.native="set_company('clearsky')">
									<span class="c">
										<span></span>
										<span>ClearSky</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_3 column_h-stretch" v-if="ui.illicit == '1'">
								<button_listed class="company_filter company_filter_coyote" :class="[{ 'selected': !filters.companies.includes('coyote') }]" @click.native="set_company('coyote')">
									<span class="c">
										<span></span>
										<span>Coyote</span>
									</span>
								</button_listed>
							</div>
							<div class="column column_3 column_h-stretch">
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
								<div class="buttons_list shadowed-shallow">
									<button_listed :class="[{ 'selected': filters.type == 'any' }]" @click.native="set_first('any')">Any</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Tour' }]" @click.native="set_first('Tour')">Tours</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Experience' }]" @click.native="set_first('Experience')">Experiences</button_listed>
									<!--<button_listed :class="['disabled', { 'selected': filters.type == 'Pax' }]" @click.native="set_first('Pax')">Passengers</button_listed>-->
								</div>
							</div>
							<div class="column column_2 column_h-stretch">
								<div class="buttons_list shadowed-shallow">
									<button_listed :class="[{ 'selected': filters.type == 'Contract' }]" @click.native="set_first('Contract')">Cargo</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Ferry' }]" @click.native="set_first('Ferry')">Ferries</button_listed>
									<button_listed :class="[{ 'selected': filters.type == 'Bush Trip' }]" @click.native="set_first('Bush Trip')">Bush trips</button_listed>
								</div>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<div class="buttons_list shadowed-shallow">
							<div class="columns columns_2 ">
								<div class="column column_2 column_h-stretch">
									<textbox class="listed_h" type="number" placeholder="MIN Distance" input_unit="distances" :min="10" :max="37040" :step="50" :decimals="0" v-model="filters.range[0]" @changed="changed()"></textbox>
								</div>
								<div class="column column_2 column_h-stretch">
									<textbox class="listed_h" type="number" placeholder="MAX Distance" input_unit="distances" :min="10" :max="37040" :step="50" :decimals="0" v-model="filters.range[1]" @changed="changed()"></textbox>
								</div>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<div class="columns columns_2 columns_margined">
							<div class="column column_2 column_h-stretch">
								<toggle v-model="filters.requiresLight">Require runway lighting</toggle>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<div class="columns columns_2 columns_margined">
							<div class="column column_2 column_h-stretch">
								<toggle v-model="filters.requiresILS">Require ILS on arrival</toggle>
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

					<div class="filter_entry">
						<h3 class="h_edge_margin_bottom_half h_edge_margin_top">Runways</h3>
						<div class="buttons_list shadowed-shallow">
							<div class="columns columns_2 ">
								<div class="column column_2 column_h-stretch">
									<textbox class="listed_h" type="number" placeholder="MIN Length" input_unit="lengths" :min="0.03048" :max="6.096" :step="10" :decimals="0" v-model="filters.runways[0]" @changed="changed()"></textbox>
								</div>
								<div class="column column_2 column_h-stretch">
									<textbox class="listed_h" type="number" placeholder="MAX Length" input_unit="lengths" :min="0.03048" :max="6.096" :step="10" :decimals="0" v-model="filters.runways[1]" @changed="changed()"></textbox>
								</div>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<div class="columns columns_margined">
							<div class="column column_2 column_h-stretch">
								<div class="buttons_list shadowed-shallow">
									<button_listed
										:class="[{ 'selected': filters.rwySurface == 'any' }]"
										@click.native="set_rwy_surface('any')">Any</button_listed>
									<button_listed
										:class="[{ 'selected': filters.rwySurface == 'soft' }]"
										@click.native="set_rwy_surface('soft')">Soft</button_listed>
									<button_listed
										:class="[{ 'selected': filters.rwySurface == 'hard' }]"
										@click.native="set_rwy_surface('hard')">Hard</button_listed>
								</div>
							</div>
							<div class="column column_2 column_h-stretch">
								<div class="buttons_list shadowed-shallow">
									<button_listed
										:class="[{ 'selected': filters.rwySurface == 'grass' }]"
										@click.native="set_rwy_surface('grass')">Grass</button_listed>
									<button_listed
										:class="[{ 'selected': filters.rwySurface == 'dirt' }]"
										@click.native="set_rwy_surface('dirt')">Dirt</button_listed>
									<button_listed
										:class="[{ 'selected': filters.rwySurface == 'water' }]"
										@click.native="set_rwy_surface('water')">Water</button_listed>
								</div>
							</div>
						</div>
					</div>
					<p class="notice">This filter will try to find contracts with the most amount of airports matching your desired runway surface.</p>

					<div class="filter_entry">
						<h3 class="h_edge_margin_bottom_half h_edge_margin_top">Legs</h3>
						<div class="buttons_list shadowed-shallow">
							<div class="columns columns_2 ">
								<div class="column column_2 column_h-stretch">
									<textbox class="listed_h" type="number" placeholder="MIN Legs" :min="1" :max="50" :step="1" v-model="filters.legsCount[0]" @changed="changed()"></textbox>
								</div>
								<div class="column column_2 column_h-stretch">
									<textbox class="listed_h" type="number" placeholder="MAX Legs" :min="1" :max="50" :step="1" v-model="filters.legsCount[1]" @changed="changed()"></textbox>
								</div>
							</div>
						</div>
					</div>

					<div class="filter_entry">
						<h3 class="h_edge_margin_bottom_half h_edge_margin_top">Sort by</h3>
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
						<div class="column column_narrow">
							<button_action class="cancel shadowed-shallow icon icon-close" @click.native="$emit('close')"></button_action>
						</div>
						<div class="column column_narrow">
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
				sim_live: this.$os.simulator.live,
			},
			filterReferences: {
				types: [0,1,2,3,4,5],
				companies: ['clearsky','coyote','skyparktravel'], //'offduty','lastflight','orbit','fliteline'
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

		set_type(type :number) {
			if(this.filters.types.length == 0) {
				Object.assign(this.filters.types, this.filterReferences.types);
				this.filters.types.splice(this.filters.types.indexOf(type), 1);
			} else {
				if(this.filters.types.includes(type)) {
					this.filters.types.splice(this.filters.types.indexOf(type), 1);
				} else {
					this.filters.types.push(type);
				}
			}
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
		},
		set_wx(cat: string, value: string) {
			const weatherExcl = this.filters.weatherExcl;
			if(weatherExcl[cat].includes(value)) {
				weatherExcl[cat].splice(weatherExcl[cat].indexOf(value), 1);
			} else {
				weatherExcl[cat].push(value);
			}
			switch(cat) {
				case "precip": {
					if(weatherExcl[cat].length == 4) {
						weatherExcl[cat].splice(weatherExcl[cat].indexOf('dry'), 1);
					}
					break;
				}
				case "wind": {
					if(weatherExcl[cat].length == 3) {
						weatherExcl[cat].splice(weatherExcl[cat].indexOf('calm'), 1);
					}
					break;
				}
				case "vis": {
					if(weatherExcl[cat].length == 3) {
						weatherExcl[cat].splice(weatherExcl[cat].indexOf('hivis'), 1);
					}
					break;
				}
			}
		},
		set_rwy_surface(cat: string) {
			this.filters.rwySurface != cat ? this.filters.rwySurface = cat : this.filters.rwySurface = 'any';
			this.changed();
		},
		set_first(cat: string){
			this.filters.type != cat ? this.filters.type = cat : this.filters.type = 'any';  this.filters.subType = 'any';
			this.changed();
		},
		set_second(cat: string){
			this.filters.subType != cat ? this.filters.subType = cat : this.filters.subType = '';
			this.changed();
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
			}

			this.changed();
		},
		set_sort_asc(cat: boolean){
			this.filters.sortAsc = cat;
			this.changed();
		},

		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.ui.sim_live = wsmsg.payload;
					break;
				}
			}
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
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('configchange', this.listener_config);
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.filters {
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