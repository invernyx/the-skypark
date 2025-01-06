<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame" ref="app-frame">

			<content_controls_stack :translucent="true" :status_padding="true" :nav_padding="true" :content_fixed="true" :shadowed="true">

				<template v-slot:content>
					<div class="helper_absolute_full">

						<MapboxFrame
							ref="map"
							mstyle="big"
							id="p42_contrax_map_main"
							:app="app"
							:mapTheme="state.ui.map.theme"
							:hasFog="true"
							@load="mapLoaded"
							@mapMoveDone="mapMoveDone"
							@resetTheme="resetTheme"
							>
							<template>

								<MglGeojsonLayer
									sourceId="locationHistory"
									layerId="locationHistory"
									:clearSource="false"
									:source="state.ui.map.sources.locationHistory"
									:layer="state.ui.map.layers.locationHistory"
								/>

								<MglGeojsonLayer
									sourceId="locationHistory"
									layerId="locationHistoryLabels"
									:source="state.ui.map.sources.locationHistory"
									:layer="state.ui.map.layers.locationHistoryLabels"
								/>

								<MglGeojsonLayer
									sourceId="relocationRings"
									layerId="relocationRingsShadow"
									:source="state.ui.map.sources.relocationRings"
									:layer="state.ui.map.layers.relocationRingsShadow" />

								<MglGeojsonLayer
									sourceId="relocationRings"
									layerId="relocationRings"
									:clearSource="false"
									:source="state.ui.map.sources.relocationRings"
									:layer="state.ui.map.layers.relocationRings" />

								<MglGeojsonLayer
									sourceId="relocationRings"
									layerId="relocationRingsLabels"
									:source="state.ui.map.sources.relocationRings"
									:layer="state.ui.map.layers.relocationRingsLabels" />

								<MglGeojsonLayer
									sourceId="contractPaths"
									layerId="contractPathsShadow"
									:source="state.ui.map.sources.contractPaths"
									:layer="state.ui.map.layers.contractPathsShadow" />

								<MglGeojsonLayer
									sourceId="contractPaths"
									layerId="contractPaths"
									:clearSource="false"
									:source="state.ui.map.sources.contractPaths"
									:layer="state.ui.map.layers.contractPaths" />


								<MglMarker v-if="$root.$data.state.services.simulator.live" :coordinates="[$root.$data.state.services.simulator.location.Lon, $root.$data.state.services.simulator.location.Lat]">
									<div class="map_marker map_marker_position" slot="marker">
										<div>
										</div>
									</div>
								</MglMarker>

								<div v-for="(actions, name, index) in state.ui.map.markers.actions" v-bind:key="index">
									<MglMarker v-for="(action, name1, index) in actions" v-bind:key="index" :coordinates="action.location">
										<div class="map_marker" :class="'map_marker_' + name" slot="marker">
											<div>
											</div>
										</div>
									</MglMarker>
								</div>


								<!-- Airports without shown contracts --->
								<MglGeojsonLayer
									sourceId="situationAirportsGhost"
									layerId="situationAirportsGhost"
									:source="state.ui.map.sources.situationAirportsGhost"
									:layer="state.ui.map.layers.situationAirportsGhost"
								/>
								<MglGeojsonLayer
									sourceId="situationAirportsGhost"
									layerId="situationAirportsGhostLabel"
									:source="state.ui.map.sources.situationAirportsGhost"
									:layer="state.ui.map.layers.situationAirportsGhostLabel"
								/>
								<MglGeojsonLayer
									sourceId="situationAirportsGhost"
									layerId="situationAirportsGhostLabelDetailed"
									:source="state.ui.map.sources.situationAirportsGhost"
									:layer="state.ui.map.layers.situationAirportsGhostLabelDetailed"
								/>


								<!-- Airports with shown contracts --->
								<MglGeojsonLayer
									sourceId="situationAirports"
									layerId="situationAirports"
									:source="state.ui.map.sources.situationAirports"
									:layer="state.ui.map.layers.situationAirports"
								/>
								<MglGeojsonLayer
									sourceId="situationAirports"
									layerId="situationAirportsLabel"
									:source="state.ui.map.sources.situationAirports"
									:layer="state.ui.map.layers.situationAirportsLabel"
								/>
								<MglGeojsonLayer
									sourceId="situationAirports"
									layerId="situationAirportsLabelDetailed"
									:source="state.ui.map.sources.situationAirports"
									:layer="state.ui.map.layers.situationAirportsLabelDetailed"
								/>

								<MglMarker v-if="state.ui.map.markers.clickLayer" :coordinates="state.ui.map.markers.clickLayer.location">
									<MapContext :actions="state.ui.map.markers.clickLayer.actions" slot="marker" @close="state.ui.map.markers.clickLayer = null" @filter="filtersSet($event)" />
								</MglMarker>
							</template>
						</MapboxFrame>

					</div>

					<div class="helper_nav-padding">
						<div class="results_section" :class="{ 'theme--dark': state.ui.map.displayLayer.sat }">

							<width_limiter size="screen">
								<div class="results_header">
									<div class="columns columns_2 helper_edge_margin_lateral">
										<div class="column column_2 column_left">
											<div class="results_nav">
												<button_action class="results_nav_back transparent" :class="{ 'theme--dark': state.ui.map.displayLayer.sat, 'disabled': state.contracts.search.Contracts.length < 2 }" @click.native="filterMove('search_results', -1)"></button_action>
												<button_action class="results_nav_forward transparent" :class="{ 'theme--dark': state.ui.map.displayLayer.sat, 'disabled': state.contracts.search.Contracts.length < 2 }" @click.native="filterMove('search_results', 1)"></button_action>
											</div>
											<div class="data-block">
												<span class="label">Destinations</span>
												<span class="value">{{ state.contracts.search.Destinations > 0 ? state.contracts.search.Destinations.toLocaleString('en-gb') : '~' }}</span>
											</div>
											<div class="data-block">
												<span class="label">Contracts</span>
												<span class="value">{{ state.contracts.search.Limit > 0 ? state.contracts.search.Count > state.contracts.search.Limit ? state.contracts.search.Limit.toLocaleString('en-gb') + "/" + (state.contracts.search.Count > 999 ? Math.round(state.contracts.search.Count / 1000) + 'k' : state.contracts.search.Count.toLocaleString('en-gb')) : state.contracts.search.Count.toLocaleString('en-gb') : '~' }}</span>
											</div>
										</div>
										<div class="column column_2 column_right">
											<button_action class="bright search-rgn-btn" :class="{'invisible': !state.ui.filters.boundsMoved || !$root.$data.state.services.api.connected }" :shadowed="true" @click.native="filterSetBounds">Search this region</button_action>
										</div>
									</div>
								</div>
							</width_limiter>

							<div class="results_list">
								<ContractList
									class="helper_edge_padding_lateral"
									:class="{ 'theme--dark': state.ui.map.displayLayer.sat }"
									ref="search_results"
									@select="mapSelectContract($event)"
									@featuredSelect="featuredSelect($event)"
									@featuredHide="featuredHide($event)"
									@featuredBrowse="featuredBrowse($event)"
									@expand="mapSelectContract($event, true);"
									@expire="contractExpire($event, true);"
									:hasFilters="state.ui.filterString.length"
									:selected="state.contracts.selected"
									:contracts="state.contracts.search" >
								</ContractList>
							</div>

						</div>
					</div>

				</template>

			</content_controls_stack>

			<div class="filter_banner"  :data-thook="'contrax_section_filters'">
				<width_limiter size="screen">
					<button_action :class="['filter_dice_btn', 'info', { 'disabled': state.ui.filters.sent }]" @click.native="filtersSearch(true)"></button_action>
					<button_action :class="['filter_open_btn', { 'cancel': state.ui.filterRestricted }]" @click.native="state.ui.currentModal = 'filters'" v-html="state.ui.filterString ? state.ui.filterString : 'Filters'"></button_action>
					<button_action :class="['filter_clear_btn', 'cancel']" v-if="state.ui.filterRestricted" @click.native="filterClearAndReset">Clear</button_action>
				</width_limiter>

				<width_limiter size="screen">
					<div class="map_controls">
						<div class="buttons_list shadowed">
							<button_action class="listed map_controls_sat" @click.native="mapToggleSat()" :class="{ 'info': state.ui.map.displayLayer.sat }"></button_action>
						</div>
					</div>
				</width_limiter>

			</div>

			<modal type="grow" :app="app" v-if="state.ui.currentModal == 'filters'" width="narrow" @close="modelClose(false)">
				<content_controls_stack :preview="true" :shadowed="true" :translucent="true">
					<template v-slot:nav>
						<button_nav shape="back" class="translucent" @click.native="state.ui.currentModal = null">Back</button_nav>
						<h2 class="abs-center">Filters</h2>
						<div></div>
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

						<div class="filter_section helper_edge_padding helper_nav-margin">

							<div class="filter_entry">
								<textbox class="shadowed" type="text" placeholder="Smart Search" focusPlaceholder="(ID, ICAOs, whatever else)" v-model="state.ui.filters.query" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
							</div>

							<div class="filter_entry">
								<div class="columns columns_margined">
									<div class="column column_h-stretch">
										<button_listed class="aircraft_type aircraft_type_heli" :class="[{ 'selected': !state.ui.filters.types.includes(0) }]" @click.native="filterSetType(0)"><span></span><span>Helis</span></button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="aircraft_type aircraft_type_ga" :class="[{ 'selected': !state.ui.filters.types.includes(1) }]" @click.native="filterSetType(1)"><span></span><span>Piston</span></button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="aircraft_type aircraft_type_turbo" :class="[{ 'selected': !state.ui.filters.types.includes(2) }]" @click.native="filterSetType(2)"><span></span><span>Turbo</span></button_listed>
									</div>
								</div>
							</div>
							<div class="filter_entry">
								<div class="columns columns_margined">
									<div class="column column_h-stretch">
										<button_listed class="aircraft_type aircraft_type_jet" :class="[{ 'selected': !state.ui.filters.types.includes(3) }]" @click.native="filterSetType(3)"><span></span><span>Biz Jets</span></button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="aircraft_type aircraft_type_narrow" :class="[{ 'selected': !state.ui.filters.types.includes(4) }]" @click.native="filterSetType(4)"><span></span><span>Narrow</span></button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="aircraft_type aircraft_type_wide" :class="[{ 'selected': !state.ui.filters.types.includes(5) }]" @click.native="filterSetType(5)"><span></span><span>Wide</span></button_listed>
									</div>
								</div>
							</div>

							<!-- companies: ['offduty','clearsky','coyote','skyparktravel','lastflight','orbit','fliteline'], company_filter -->

							<div class="filter_entry">
								<div class="columns columns_margined">
									<div class="column column_h-stretch">
										<button_listed class="company_filter company_filter_clearsky" :class="[{ 'selected': !state.ui.filters.companies.includes('clearsky') }]" @click.native="filterSetCompany('clearsky')"><span></span><span>ClearSky</span></button_listed>
									</div>
									<div class="column column_h-stretch" v-if="$os.transponderSettings.illicit == '1'">
										<button_listed class="company_filter company_filter_coyote" :class="[{ 'selected': !state.ui.filters.companies.includes('coyote') }]" @click.native="filterSetCompany('coyote')"><span></span><span>Coyote</span></button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="company_filter company_filter_skyparktravel" :class="[{ 'selected': !state.ui.filters.companies.includes('skyparktravel') }]" @click.native="filterSetCompany('skyparktravel')"><span></span><span>Travels</span></button_listed>
									</div>
								</div>
							</div>

							<!--
							<div class="filter_entry">
								<div class="columns columns_margined">
									<div class="column column_h-stretch">
										<button_listed class="company_filter company_filter_lastflight" :class="[{ 'selected': !state.ui.filters.companies.includes('lastflight') }]" @click.native="filterSetCompany('lastflight')"></button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="company_filter company_filter_orbit" :class="[{ 'selected': !state.ui.filters.companies.includes('orbit') }]" @click.native="filterSetCompany('orbit')"></button_listed>
									</div>
									<div class="column column_h-stretch">
										<button_listed class="company_filter company_filter_fliteline" :class="[{ 'selected': !state.ui.filters.companies.includes('fliteline') }]" @click.native="filterSetCompany('fliteline')"></button_listed>
									</div>
								</div>
							</div>
							-->

							<div class="filter_entry">
								<div class="columns columns_2 columns_margined">
									<div class="column column_2 column_h-stretch">
										<div class="buttons_list shadowed">
											<button_listed :class="[{ 'selected': state.ui.filters.type == 'any' }]" @click.native="filtersSetFirst('any')">Any</button_listed>
											<button_listed :class="[{ 'selected': state.ui.filters.type == 'Tour' }]" @click.native="filtersSetFirst('Tour')">Tours</button_listed>
											<button_listed :class="[{ 'selected': state.ui.filters.type == 'Experience' }]" @click.native="filtersSetFirst('Experience')">Experiences</button_listed>
											<!--<button_listed :class="['disabled', { 'selected': state.ui.filters.type == 'Pax' }]" @click.native="filtersSetFirst('Pax')">Passengers</button_listed>-->
										</div>
									</div>
									<div class="column column_2 column_h-stretch">
										<div class="buttons_list shadowed">
											<button_listed :class="[{ 'selected': state.ui.filters.type == 'Contract' }]" @click.native="filtersSetFirst('Contract')">Cargo</button_listed>
											<button_listed :class="[{ 'selected': state.ui.filters.type == 'Ferry' }]" @click.native="filtersSetFirst('Ferry')">Ferries</button_listed>
											<button_listed :class="[{ 'selected': state.ui.filters.type == 'Bush Trip' }]" @click.native="filtersSetFirst('Bush Trip')">Bush trips</button_listed>
										</div>
									</div>
								</div>
							</div>

							<div class="filter_entry">
								<div class="columns columns_2 columns_margined">
									<div class="column column_2 column_h-stretch">
										<toggle v-model="state.ui.filters.requiresLight">Require runway lighting</toggle>
									</div>
								</div>
							</div>

							<div class="filter_entry">
								<div class="columns columns_2 columns_margined">
									<div class="column column_2 column_h-stretch">
										<toggle v-model="state.ui.filters.requiresILS">Require ILS on arrival</toggle>
									</div>
								</div>
							</div>

							<div class="filter_entry">
								<div class="columns columns_2 columns_margined">
									<div class="column column_2 column_h-stretch">
										<toggle v-model="state.ui.filters.onlyCustomContracts" class="gold">Only show custom contracts</toggle>
									</div>
								</div>
							</div>

							<div class="columns dice-section">
								<div class="column column_h-center">
									<div class="dice" @click="filtersSearch(true)"></div>
								</div>
								<div class="column">
									<div class="dice-title">Feeling Lucky?</div>
									<p class="dice-notice notice">Request a random Contract. <br/>Increased Cancellation Fees Apply</p>
								</div>
							</div>

							<!--
							<div class="filter_entry">
								<div class="buttons_list shadowed">
									<button_listed
										:class="['naved', {
											'naved-out': state.ui.filters.type != '' && state.ui.filters.type != 'any',
											'selected': state.ui.filters.type == 'any'
										}]"
										@click.native="filtersSetFirst('any')">Any</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': state.ui.filters.type != '' && state.ui.filters.type != 'airliners',
											'selected': state.ui.filters.type == 'airliners'
										}]"
											@click.native="filtersSetFirst('airliners')">Airliners</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': state.ui.filters.type != '' && state.ui.filters.type != 'ga',
											'selected': state.ui.filters.type == 'ga'
										}]"
										@click.native="filtersSetFirst('ga')">General Aviation</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': state.ui.filters.type != '' && state.ui.filters.type != 'heli',
											'selected': state.ui.filters.type == 'heli'
										}]"
										@click.native="filtersSetFirst('heli')">Helicopters</button_listed>
								</div>
							</div>

							<div class="filter_entry">
								<div class="buttons_list shadowed">

									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'hops') || state.ui.filters.type != 'ga',
											'selected': state.ui.filters.subType == 'hops'
										}]"
										@click.native="filtersSetSecond('hops')">Hops</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'bush_trips') || state.ui.filters.type != 'ga',
											'selected': state.ui.filters.subType == 'bush_trips'
										}]"
										@click.native="filtersSetSecond('bush_trips')">Bush Trips</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'heli_trips') || state.ui.filters.type != 'heli',
											'selected': state.ui.filters.subType == 'heli_trips'
										}]"
										@click.native="filtersSetSecond('heli_trips')">Helicopter Trips</button_listed>

									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'airliners_short') || state.ui.filters.type != 'airliners',
											'selected': state.ui.filters.subType == 'airliners_short'
										}]"
										@click.native="filtersSetSecond('airliners_short')">Short Haul</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'airliners_medium') || state.ui.filters.type != 'airliners',
											'selected': state.ui.filters.subType == 'airliners_medium'
										}]"
										@click.native="filtersSetSecond('airliners_medium')">Medium Haul</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'airliners_long') || state.ui.filters.type != 'airliners',
											'selected': state.ui.filters.subType == 'airliners_long'
										}]"
										@click.native="filtersSetSecond('airliners_long')">Long Haul</button_listed>

									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'tours') || state.ui.filters.type == '',
											'selected': state.ui.filters.subType == 'tours'
										}]"
										@click.native="filtersSetSecond('tours')">Tours</button_listed>
									<button_listed
										:class="['naved', {
											'naved-out': (state.ui.filters.subType != '' && state.ui.filters.subType != 'events') || state.ui.filters.type == '',
											'selected': state.ui.filters.subType == 'events'
										}]"
										@click.native="filtersSetSecond('events')">Events</button_listed>
								</div>
							</div>
							-->
							<div class="filter_entry lined lined-t">
								<collapser :withArrow="true" :default="false" :preload="true">
									<template v-slot:title>
										<div>
											<h3>Advanced Filters</h3>
										</div>
										<div class="collapser_arrow"></div>
									</template>
									<template v-slot:content>

										<div class="filter_entry">
											<!--<h3 class="margin-b">Distance</h3>-->
											<div class="buttons_list shadowed">
												<div class="columns columns_2 ">
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MIN Distance" postUnit="nm" :min="10" :max="10000" :step="50" v-model="state.ui.filters.range[0]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MAX Distance" postUnit="nm" :min="10" :max="9999" :step="50" v-model="state.ui.filters.range[1]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
												</div>
											</div>
										</div>

										<div class="filter_entry">
											<h3 class="margin-b">Runways</h3>
											<div class="buttons_list shadowed">
												<div class="columns columns_2 ">
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MIN Length" postUnit="ft" :min="10" :max="20000" :step="50" v-model="state.ui.filters.runways[0]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MAX Length" postUnit="ft" :min="10" :max="20000" :step="50" v-model="state.ui.filters.runways[1]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
												</div>
											</div>
										</div>
										<!--
										<div class="filter_entry">
											<div class="buttons_list shadowed">
												<div class="columns columns_2 ">
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MIN Count" :min="1" :max="20" :step="1" v-model="state.ui.filters.rwyCount[0]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MAX Count" :min="1" :max="20" :step="1" v-model="state.ui.filters.rwyCount[1]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
												</div>
											</div>
										</div>
										-->
										<div class="filter_entry">
											<div class="columns columns_2 columns_margined">
												<div class="column column_2 column_h-stretch">
													<div class="buttons_list shadowed">
														<button_listed
															:class="[{ 'selected': state.ui.filters.rwySurface == 'any' }]"
															@click.native="filtersSetRwySurface('any')">Any</button_listed>
														<button_listed
															:class="[{ 'selected': state.ui.filters.rwySurface == 'soft' }]"
															@click.native="filtersSetRwySurface('soft')">Soft</button_listed>
														<button_listed
															:class="[{ 'selected': state.ui.filters.rwySurface == 'hard' }]"
															@click.native="filtersSetRwySurface('hard')">Hard</button_listed>
													</div>
												</div>
												<div class="column column_2 column_h-stretch">
													<div class="buttons_list shadowed">
														<button_listed
															:class="[{ 'selected': state.ui.filters.rwySurface == 'grass' }]"
															@click.native="filtersSetRwySurface('grass')">Grass</button_listed>
														<button_listed
															:class="[{ 'selected': state.ui.filters.rwySurface == 'dirt' }]"
															@click.native="filtersSetRwySurface('dirt')">Dirt</button_listed>
														<button_listed
															:class="[{ 'selected': state.ui.filters.rwySurface == 'water' }]"
															@click.native="filtersSetRwySurface('water')">Water</button_listed>
													</div>
												</div>
											</div>
										</div>
										<p class="notice">This filter will try to find contracts with the most amount of airports matching your desired runway surface.</p>

										<div class="filter_entry">
											<h3 class="margin-b">Legs</h3>
											<div class="buttons_list shadowed">
												<div class="columns columns_2 ">
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MIN Legs" :min="1" :max="50" :step="1" v-model="state.ui.filters.legsCount[0]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
													<div class="column column_2 column_h-stretch">
														<textbox class="listed_h" type="number" placeholder="MAX Legs" :min="1" :max="50" :step="1" v-model="state.ui.filters.legsCount[1]" @changed="filtersChanged()" @returned="filterSetBounds"></textbox>
													</div>
												</div>
											</div>
										</div>

										<div class="filter_entry">
											<h3 class="margin-b">Sort by</h3>
											<div class="buttons_list shadowed">
												<div class="columns columns_2">
													<div class="column column_2 column_h-stretch">
														<button_listed class="listed_h"
															:class="[{
																'selected': state.ui.filters.sortAsc && (state.ui.filters.sort != 'aircraft' && state.ui.filters.sort != 'relevance'),
																'disabled': state.ui.filters.sort == 'aircraft' || state.ui.filters.sort == 'relevance',
															}]"
															@click.native="filtersSetSortAsc(true)">Ascending</button_listed>
													</div>
													<div class="column column_2 column_h-stretch">
														<button_listed class="listed_h"
															:class="[{
																'selected': !state.ui.filters.sortAsc && (state.ui.filters.sort != 'aircraft' && state.ui.filters.sort != 'relevance'),
																'disabled': state.ui.filters.sort == 'aircraft' || state.ui.filters.sort == 'relevance',
															}]"
															@click.native="filtersSetSortAsc(false)">Descending</button_listed>
													</div>
												</div>
											</div>
										</div>

										<div class="filter_entry">
											<div class="buttons_list shadowed">
												<button_listed
													:class="[{ 'selected': state.ui.filters.sort == 'relevance' }]"
													@click.native="filtersSetSort('relevance')">Relevance</button_listed>
												<button_listed
													:class="[{ 'selected': state.ui.filters.sort == 'topography_var' }]"
													@click.native="filtersSetSort('topography_var')">Topography variations</button_listed>
												<button_listed
													v-if="$root.$data.state.services.simulator.live"
													:class="[{ 'selected': state.ui.filters.sort == 'aircraft' }]"
													@click.native="filtersSetSort('aircraft')">Current aircraft location</button_listed>
												<button_listed
													:class="[{ 'selected': state.ui.filters.sort == 'distance' }]"
													@click.native="filtersSetSort('distance')">Distance</button_listed>
												<button_listed
													:class="[{ 'selected': state.ui.filters.sort == 'ending' }]"
													@click.native="filtersSetSort('ending')">Ending soon</button_listed>
												<button_listed
													v-if="$root.$data.config.ui.tier == 'endeavour' && $os.isDev"
													:class="[{ 'selected': state.ui.filters.sort == 'reward' }]"
													@click.native="filtersSetSort('reward')">Pay</button_listed>
												<button_listed
													v-if="$root.$data.config.ui.tier == 'endeavour'"
													:class="[{ 'selected': state.ui.filters.sort == 'xp' }]"
													@click.native="filtersSetSort('xp')">XP</button_listed>
											</div>
										</div>

									</template>
								</collapser>
							</div>


							<!--
							<div class="filter_entry">
								<collapser :withArrow="true" :default="false" :preload="true">
									<template v-slot:title>
										<h3 class="helper_edge_margin_vertical_half">Weather</h3>
									</template>
									<template v-slot:content>
										<div class="filter_entry">
											<div class="buttons_list shadowed">
												<div class="columns columns_5">
													<div class="column column_5 column_h-stretch">
														<button_listed class="weather_type weather_type_dry listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.precip.includes('dry') }]" @click.native="filterSetWx('precip','dry')"><span></span><span>Dry</span></button_listed>
													</div>
													<div class="column column_5 column_h-stretch">
														<button_listed class="weather_type weather_type_rain listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.precip.includes('rain') }]" @click.native="filterSetWx('precip','rain')"><span></span><span>Rain</span></button_listed>
													</div>
													<div class="column column_5 column_h-stretch">
														<button_listed class="weather_type weather_type_thunderstorm listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.precip.includes('thunderstorm') }]" @click.native="filterSetWx('precip','thunderstorm')"><span></span><span>TS</span></button_listed>
													</div>
													<div class="column column_5 column_h-stretch">
														<button_listed class="weather_type weather_type_snow listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.precip.includes('snow') }]" @click.native="filterSetWx('precip','snow')"><span></span><span>Snow</span></button_listed>
													</div>
												</div>
											</div>
										</div>
										<div class="filter_entry">
											<div class="buttons_list shadowed">
												<div class="columns columns_5">
													<div class="column column_5 column_h-stretch">
														<button_listed class="weather_type weather_type_calm listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.wind.includes('calm') }]" @click.native="filterSetWx('wind','calm')"><span></span><span>Calm</span></button_listed>
													</div>
													<div class="column column_5 column_h-stretch">
														<button_listed class="weather_type weather_type_windy listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.wind.includes('windy') }]" @click.native="filterSetWx('wind','windy')"><span></span><span>Windy</span></button_listed>
													</div>
													<div class="column column_5 column_h-stretch">
														<button_listed class="weather_type weather_type_crosswind listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.wind.includes('crosswind') }]" @click.native="filterSetWx('wind','crosswind')"><span></span><span>X-wind</span></button_listed>
													</div>
												</div>
											</div>
										</div>
										<div class="filter_entry">
											<div class="buttons_list shadowed">
												<div class="columns columns_2">
													<div class="column column_2 column_h-stretch">
														<button_listed class="weather_type weather_type_dry listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.vis.includes('lowvis') }]" @click.native="filterSetWx('vis','lowvis')"><span></span><span>Low vis</span></button_listed>
													</div>
													<div class="column column_2 column_h-stretch">
														<button_listed class="weather_type weather_type_rain listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.vis.includes('medvis') }]" @click.native="filterSetWx('vis','medvis')"><span></span><span>Med vis</span></button_listed>
													</div>
													<div class="column column_2 column_h-stretch">
														<button_listed class="weather_type weather_type_rain listed_h" :class="[{ 'selected': !state.ui.filters.weatherExcl.vis.includes('hivis') }]" @click.native="filterSetWx('vis','hivis')"><span></span><span>High vis</span></button_listed>
													</div>
												</div>
											</div>
										</div>
									</template>
								</collapser>
							</div>
							-->

						</div>
					</template>
					<template v-slot:tab>
						<div class="filter_footer">
							<div class="columns columns_2 columns_margined_half">
								<div class="column column_2">
									<button_action @click.native="filtersClear" class="translucent">Clear</button_action>
								</div>
								<div class="column column_2">
									<button_action @click.native="filterSetBounds" class="go">Search</button_action>
								</div>
							</div>
						</div>
					</template>
				</content_controls_stack>
			</modal>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo, StatusType } from "@/sys/foundation/app_bundle"
import * as turf from '@turf/turf';
import Eljs from '@/sys/libraries/elem';
import MapboxFrame from "@/sys/components/maps/mapbox.vue"
import ContractList from "./components/list.vue"
import MapContext from "@/sys/components/map_context.vue"
import ContractDetailed from "@/sys/components/contracts/contract_detailed.vue"
import MapboxExt from '@/sys/libraries/mapboxExt';
import { MglMap, MglMarker, MglNavigationControl, MglGeojsonLayer } from 'v-mapbox';
import '@/sys/libraries/mapbox-vue/mapbox-gl.css';

export default Vue.extend({
	name: "p42_contrax",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ContractList,
		ContractDetailed,
		MapContext,
		MapboxFrame,
		MglMap,
		MglMarker,
		MglNavigationControl,
		MglGeojsonLayer,
	},
	data() {
		return {
			ready: true,
			state: {
				contracts: {
					selected: {
						restored: false,
						Contract: null,
						Template: null,
						Featured: null,
					},
					search: {
						Limit: 0,
						State: 0,
						Count: 0,
						Destinations: 0,
						Airports: [],
						Contracts: [],
						Templates: [],
						Featured: [],
						FeaturedFull: [],
						FeaturedPrevious: [],
					}
				},
				ui: {
					loaded: false,
					currentModal: null,
					contractModal: null,
					reloadOnWake: false,
					map: {
						loaded: false,
						location: [-97.994961, 31.274877],
						theme: 'theme--dark',
						zoom: 2,
						heading: 0,
						pitch: 0,
						padding: { left: 28, top: 100, right: 28, bottom: 325 },
						reloadOnActivate: false,
						radarFetchTimer: null,
						radarTimer: null,
						radarAnimated: true,
						displayLayer: {
							sat: true,
						},
						lineAnimationInterval: null,
						sources: {
							relocationRings: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							contractPaths: {
								type: 'geojson',
								lineMetrics: true,
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							situationAirports: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							situationAirportsGhost: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							},
							locationHistory: {
								type: 'geojson',
								data: {
									type: 'FeatureCollection',
									features: []
								}
							}
						},
						layers: {
							relocationRings: {
								type: "line",
								source: "relocationRings",
								paint: {
									'line-color': 'rgba(0,0,0,0.4)',
									'line-width': 1,
									'line-opacity': ['get', 'opacity'],
      								'line-dasharray': [2, 1],
								}
							},
							relocationRingsShadow: {
								id: 'relocationRingsShadow',
								type: 'line',
								source: 'relocationRings',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': '#000000',
									'line-width': 4,
									'line-opacity': 0
								},
							},
							relocationRingsLabels: {
								id: "relocationRingsLabels",
        						type: "symbol",
								source: 'relocationRings',
								layout: {
									"symbol-placement": "line",
									"text-field": ['get', 'label'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									"text-offset": [0, -0.6],
									"text-size": 12,
								},
								paint: {
									'text-color': "rgba(255,0,0,0.8)",
								}
							},

							contractPaths: {
								id: 'contractPaths',
								type: 'line',
								source: 'contractPaths',
								layout: {
									"line-join": "round",
									"line-cap": "butt"
								},
								paint: {
									'line-color': 'black' as any,
									'line-width': [
										"case",
										["boolean", ["get", "active"], false], 6,
										["boolean", ["feature-state", "hover"], false], 4,
										2
									],
									'line-opacity': [
										"case",
										["boolean", ["get", "active"], false], 1,
										["boolean", ["feature-state", "hover"], false], 0.9,
										0.8
									]
								},
								filter: ['in', '$type', 'LineString']
							},
							contractPathsShadow: {
								id: 'contractPathsShadow',
								type: 'line',
								source: 'contractPaths',
								layout: {
									'line-cap': 'round',
									'line-join': 'round'
								},
								paint: {
									'line-color': 'black' as any,
									'line-width': 15,
									'line-opacity': 0.1
								},
								filter: ['in', '$type', 'LineString']
							},

							situationAirportsLabelDetailed: {
								id: "situationAirportsLabelDetailed",
								type: 'symbol',
								source: 'situationAirports',
								minzoom: 8,
								layout: {
									"text-field": ['get', 'title'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									'text-variable-anchor': ['left'], // 'top', 'bottom', 'left', 'right'
									'text-justify': 'left',
									'text-radial-offset': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0.5,
										14, 1.5
									],
									"text-size": 12,
								},
								paint: {
									'text-color': "#FFF",
								}
							},
							situationAirportsLabel: {
								id: "situationAirportsLabel",
								type: 'symbol',
								source: 'situationAirports',
								maxzoom: 8,
								layout: {
									"text-field": ['get', 'icao'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									'text-variable-anchor': ['top', 'bottom', 'left', 'right'], // 'top', 'bottom', 'left', 'right'
									'text-justify': 'left',
									'text-radial-offset': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0.5,
										14, 1.5
									],
									"text-size": 12,
								},
								paint: {
									'text-color': "#FFF",
								}
							},
							situationAirports: {
								id: "situationAirports",
								type: 'circle',
								source: 'situationAirports',
								paint: {
									'circle-color': '#4285f4',
									'circle-pitch-alignment': 'map',
									'circle-stroke-color': '#4285f4',
									'circle-stroke-opacity': 0.2,
									'circle-stroke-width': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0,
										3, 0,
										5, 4,
										14, 8
									],
									'circle-radius': [
										"interpolate",
										["linear"],
										["zoom"],
										0, ["case",["boolean", ["feature-state", "hover"], false], 8, 2],
										14, ["case",["boolean", ["feature-state", "hover"], false], 9, 8]
									]

								}
							},

							situationAirportsGhostLabelDetailed: {
								id: "situationAirportsGhostLabelDetailed",
								type: 'symbol',
								source: 'situationAirportsGhost',
								minzoom: 8,
								layout: {
									'text-allow-overlap': true,
									'text-ignore-placement': true,
									"text-field": ['get', 'title'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									'text-variable-anchor': ['left'], // 'top', 'bottom', 'left', 'right'
									'text-justify': 'left',
									'text-radial-offset': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0.5,
										14, 1.5
									],
									"text-size": 12,
								},
								paint: {
									'text-color': "#FFF",
									'text-opacity': [
										"case",
										["boolean", ["get", "active"], false], 1,
										["boolean", ["feature-state", "hover"], false], 1,
										0
									],
								}
							},
							situationAirportsGhostLabel: {
								id: "situationAirportsGhostLabel",
								type: 'symbol',
								source: 'situationAirportsGhost',
								maxzoom: 8,
								layout: {
									'text-allow-overlap': true,
									'text-ignore-placement': true,
									"text-field": ['get', 'icao'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									'text-variable-anchor': ['bottom', 'left', 'right'], // 'top', 'bottom', 'left', 'right'
									'text-justify': 'left',
									'text-radial-offset': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0.8,
										14, 1.5
									],
									"text-size": 12,
								},
								paint: {
									'text-color': "#FFF",
									'text-opacity': [
										"case",
										["boolean", ["get", "active"], false], 1,
										["boolean", ["feature-state", "hover"], false], 1,
										0
									],
								}
							},
							situationAirportsGhost: {
								id: "situationAirportsGhost",
								type: 'circle',
								source: 'situationAirportsGhost',
								paint: {
									'circle-color': '#4285f4',
									'circle-pitch-alignment': 'map',
									'circle-stroke-color': '#4285f4',
									'circle-opacity': [
										"case",
										["boolean", ["get", "active"], false], 1,
										["boolean", ["feature-state", "hover"], false], 1,
										0.2
									],
									'circle-stroke-opacity': 0.1,
									'circle-stroke-width': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0,
										3, 0,
										5, 4,
										14, 8
									],
									'circle-radius': [
										"interpolate",
										["linear"],
										["zoom"],
										0, ["case",["boolean", ["feature-state", "hover"], false], 8, 2],
										14, ["case",["boolean", ["feature-state", "hover"], false], 9, 8]
									]

								}
							},

							locationHistory: {
								id: "locationHistory",
								type: 'circle',
								source: 'locationHistory',
								paint: {
									'circle-color': '#4285f4',
									'circle-pitch-alignment': 'map',
									'circle-stroke-color': '#4285f4',
									'circle-stroke-opacity': 0.2,
									'circle-stroke-width': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0,
										3, 0,
										5, 5,
										14, 9
									],
									'circle-radius': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 3,
										14, 9
									]

								}
							},
							locationHistoryLabels: {
								id: "locationHistoryLabels",
								type: 'symbol',
								source: 'locationHistory',
								layout: {
									"text-field": ['get', 'label'],
									"text-font": ["DIN Offc Pro Medium", "Arial Unicode MS Bold"],
									'text-variable-anchor': ['left'], // 'top', 'bottom', 'left', 'right'
									'text-justify': 'left',
									'text-radial-offset': [
										"interpolate",
										["linear"],
										["zoom"],
										0, 0.5,
										14, 1.5
									],
									"text-size": 12,
								},
								paint: {
									'text-color': "#FFF",
								}
							}
						},
						markers: {
							clickLayer: null,
							airports: [],
							actions: {
								cargo_pickup: [],
								cargo_dropoff: [],
							}
						}
					},
					filterString: "",
					filterRestricted: false,
					filterReferences: {
						types: [0,1,2,3,4,5],
						companies: ['clearsky','coyote','skyparktravel'], //'offduty','lastflight','orbit','fliteline'
					},
					filters: {
						sent: false,
						changed: false,
						boundsMoved: false,
						query: '',
						bounds: null,
						types: [],
						companies: [],
						onlyCustomContracts: false,
						requiresLight: false,
						requiresILS: false,
						weatherExcl: {
							precip: [],
							wind: [],
							vis: [],
						},
						range: [0,0],
						legsCount: [1,50],
						runways: [1,1],
						rwyCount: [1,20],
						rwySurface: 'any',
						type: 'any',
						sort: 'relevance',
						sortAsc:  false,
						subType: '',
					}
				}
			}
		}
	},
	beforeMount() {
		this.filtersClear();

		if(this.app.loaded_state != null){
			try{ this.state.ui.map.location = this.app.loaded_state.ui.map.location; } catch { }
			try{ this.state.ui.map.zoom = this.app.loaded_state.ui.map.zoom; } catch {}
			try{ this.state.ui.map.heading = this.app.loaded_state.ui.map.heading; } catch {}
			try{ this.state.ui.map.pitch = this.app.loaded_state.ui.map.pitch; } catch {}
			try{ this.state.ui.filters = Object.assign(this.state.ui.filters, this.app.loaded_state.ui.filters); } catch {}
			try{ this.state.ui.map.displayLayer = Object.assign(this.state.ui.map.displayLayer, this.app.loaded_state.ui.map.displayLayer); } catch {}
		}
	},
	mounted() {
		document.addEventListener('keydown', this.keyDownEv);
		setTimeout(() => {
			this.$emit('loaded');
		}, 1500);
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		clearInterval(this.state.ui.map.lineAnimationInterval);
		clearInterval(this.state.ui.map.radarFetchTimer);
		clearInterval(this.state.ui.map.radarTimer);
		document.removeEventListener('keydown', this.keyDownEv);
		this.$root.$off('ws-in', this.listenerWs);
	},
	activated() {
		document.addEventListener('keydown', this.keyDownEv);
		if(this.state.ui.reloadOnWake) {
			this.state.ui.reloadOnWake = false;
			this.filterSetBounds();
		}
		this.mapSetLineAnimation();
	},
	deactivated() {
		clearInterval(this.state.ui.map.lineAnimationInterval);
		document.removeEventListener('keydown', this.keyDownEv);
	},
	methods: {
		mapLoaded(map: any) {
			this.$os.TrackMapLoad(this.$route.path);
			window.requestAnimationFrame(() => {
				setTimeout(() => {
					if(this.$root.$data.state.services.simulator.live){
						this.$os.UntrackedMap['p42_contrax_map_main'].map.setCenter([this.$root.$data.state.services.simulator.location.Lon, this.$root.$data.state.services.simulator.location.Lat]);
					} else {
						this.$os.UntrackedMap['p42_contrax_map_main'].map.setCenter([this.state.ui.map.location[0], this.state.ui.map.location[1]]);
					}

					this.state.ui.map.loaded = true;
					this.mapSetLineAnimation();
					this.mapToggleSat(this.state.ui.map.displayLayer.sat);
					//this.mapUpdateRadar();
					this.filtersChanged();
					this.$emit('loaded');

				}, 100)

				this.$os.UntrackedMap['p42_contrax_map_main'].map.setZoom(this.state.ui.map.zoom);
				this.$os.UntrackedMap['p42_contrax_map_main'].map.setBearing(this.state.ui.map.heading);
				this.$os.UntrackedMap['p42_contrax_map_main'].map.setPitch(this.state.ui.map.pitch);
				this.mapMoveDone();
				this.mapUpdateContract();
				this.mapSetLineAnimation();
				this.mapUpdateRelocationRings();

				let ScheduleClearSelected = false;
				let SelectCancel = false;
				['mousedown', 'touchstart'].forEach((ev) => {
					this.$os.UntrackedMap['p42_contrax_map_main'].map.on(ev, (e :any) => {
						SelectCancel = false;
						ScheduleClearSelected = true;
					});
				});

				['move', 'zoom', 'rotate', 'pitch'].forEach((ev) => {
					this.$os.UntrackedMap['p42_contrax_map_main'].map.on(ev, () => {
						if(!SelectCancel){
							SelectCancel = true;
							this.mapClearHover(['contractPaths']);
						}
						ScheduleClearSelected = false;
					});
				});

				['mouseup', 'touchend'].forEach((ev) => {
					this.$os.UntrackedMap['p42_contrax_map_main'].map.on(ev, (e: any) => {
						let clearSelect = false;
						// Cancel select if contextual is clicked
						const path = Eljs.getDOMParents(e.originalEvent.target);
						path.forEach((node :any) => {
							if(node.classList){
								if(node.classList.contains('map_contextual')){
									SelectCancel = true;
								}
							}
						});

						// Check and select on the map
						if(!SelectCancel){
							if(!this.state.ui.map.markers.clickLayer) {
								let features = this.$os.UntrackedMap['p42_contrax_map_main'].map.queryRenderedFeatures(e.point);
								features = features.filter((x :any) => x.layer.id != 'contractPaths');
								if(features.length) {
									// Reaction method
									const react = (feature :any) => {
										if(feature) {
											switch(feature.layer.id) {
												case 'contractPaths':
												case 'contractPathsShadow': {
													const selected = this.state.contracts.search.Contracts.find(x => x.ID == feature.id);
													this.mapSelectContract(selected, false);
													ScheduleClearSelected = false;
													return;
												}
												case 'situationAirports':
												case 'situationAirportsLabel':
												case 'situationAirportsLabelDetailed':
												case 'situationAirportsGhost': {
													const clickLocation = this.$os.UntrackedMap['p42_contrax_map_main'].map.unproject(e.point);
													const clickLayerObj = {
														location: [clickLocation.lng, clickLocation.lat],
														actions: [
															{
																type: 'airport',
																data: {
																	airport: JSON.parse(feature.properties.airport),
																	feature: feature,
																},
															}
														]
													}
													this.state.ui.map.markers.clickLayer = clickLayerObj;
													break;
												}
												default: {
													break;
												}
											}
										}
									}

									// Validate we don't have duplicates
									const featureIDs = [] as number[];
									const filteredFeatures = [] as any[];
									features.forEach((infeature :any, index :number) => {
										switch(infeature.layer.id) {
											case 'contractPathsShadow':
											case 'situationAirports':
											case 'situationAirportsLabel':
											case 'situationAirportsLabelDetailed':
											case 'situationAirportsGhost': {
												if(!featureIDs.includes(infeature.id)){
													featureIDs.push(infeature.id);
													filteredFeatures.push(infeature);
												}
												break;
											}
										}
									});

									react(filteredFeatures[0]);
								}
							} else {
								this.state.ui.map.markers.clickLayer = null;
							}
						}
						if(ScheduleClearSelected){
							ScheduleClearSelected = false;
							this.state.contracts.selected.Contract = null;
							this.state.contracts.selected.Template = null;
							this.mapClearActive();
							this.mapClearContractMarkers();
						}
					});
				});

				['mouseleave'].forEach((ev) => {
					this.$os.UntrackedMap['p42_contrax_map_main'].map.on(ev, 'contractPathsShadow', (e: any) => {
						this.mapClearHover(['contractPaths']);
					});
				});

				['mousemove'].forEach((ev) => {
					this.$os.UntrackedMap['p42_contrax_map_main'].map.on(ev, (e: any) => {
						this.mapClearHover(['contractPaths','situationAirports','situationAirportsGhost']);
						if(!this.state.ui.map.markers.clickLayer){
							let doHover = false;
							let targetIndex = 0;
							const features = this.$os.UntrackedMap['p42_contrax_map_main'].map.queryRenderedFeatures(e.point);
							while(targetIndex < features.length && !doHover) {
								if(features.length) {
									switch(features[targetIndex].layer.id) {
										case 'situationAirportsLabel':
										case 'situationAirportsLabelDetailed':
										case 'situationAirportsGhostLabel':
										case 'situationAirportsGhostLabelDetailed': {
											doHover = false;
											break;
										}
										default: {
											doHover = true;
											break;
										}
									}

									if(doHover && features[targetIndex].id) {
										switch(features[targetIndex].layer.source) {
											case "contractPathsShadow": {
												this.$os.UntrackedMap['p42_contrax_map_main'].map.setFeatureState({
													sourceLayer: features[targetIndex].sourceLayer,
													source: 'contractPaths',
													id: features[targetIndex].id
												}, {
													hover: true
												});
												break;
											}
											case "situationAirports": {
												break;
											}
										}

										this.$os.UntrackedMap['p42_contrax_map_main'].map.setFeatureState({
											sourceLayer: features[targetIndex].sourceLayer,
											source: features[targetIndex].source,
											id: features[targetIndex].id
										}, {
											hover: true
										});
									}
								}
								targetIndex++;
							}
							//this.mapRefresh(['contractPaths','situationAirports','situationAirportsGhost']);
						}
					});
				});

				if(!this.state.ui.loaded){
					this.state.ui.loaded = true;
					this.filtersSearch(false);
				}

				this.mapAddSnowDepth();
			});
		},
		mapContextCommand(feature :any, cmd :string) {
			const airport = JSON.parse(feature.properties.airport);
			switch(cmd) {
				case 'all': {
					this.state.ui.filters.query = airport.ICAO;
					this.filtersSearch(false);
					break;
				}
				case 'dep': {
					this.state.ui.filters.query = airport.ICAO + "-";
					this.filtersSearch(false);
					break;
				}
				case 'arr': {
					this.state.ui.filters.query = "-" + airport.ICAO;
					this.filtersSearch(false);
					break;
				}
			}
		},
		mapMoveDone() {
			if(this.state.ui.map.loaded){

				const centerLocation = this.$os.UntrackedMap['p42_contrax_map_main'].map.getCenter();
				this.state.ui.map.location = [centerLocation.lng, centerLocation.lat];
				this.state.ui.map.heading = this.$os.UntrackedMap['p42_contrax_map_main'].map.getBearing();
				this.state.ui.map.pitch = this.$os.UntrackedMap['p42_contrax_map_main'].map.getPitch();
				this.state.ui.map.zoom = this.$os.UntrackedMap['p42_contrax_map_main'].map.getZoom();

				if(!this.state.contracts.search.Contracts.length && this.state.contracts.selected.restored) {
					this.filterSetBounds();
				} else {
					this.state.ui.filters.boundsMoved = false;
					window.requestAnimationFrame(() => {
						setTimeout(() => {
							this.state.ui.filters.boundsMoved = true;
						}, 10);
					});
				}
				this.stateSave();
			}
		},
		mapSetLineAnimation() {
			if(this.state.ui.map.loaded) {
				const dashLength = 10;
				const gapLength = 1;
				const steps = 15;
				const stepOffset = 0.5;
				const dashSteps = steps * dashLength / (gapLength + dashLength);
				const gapSteps = steps - dashSteps;
				let step = steps;

				clearInterval(this.state.ui.map.lineAnimationInterval);
				this.state.ui.map.lineAnimationInterval = setInterval(() => {
					step -= stepOffset;
					if (step <= 0) step = steps - stepOffset;

					let t, a, b, c, d;
					if (step < dashSteps) {
						t = step / dashSteps;
						a = (1 - t) * dashLength;
						b = gapLength;
						c = t * dashLength;
						d = 0;
					} else {
						t = (step - dashSteps) / (gapSteps);
						a = 0;
						b = (1 - t) * gapLength;
						c = dashLength;
						d = t * gapLength;
					}

					this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty("contractPaths", "line-dasharray", [a, b, c, d]);
				}, 200);
			}
		},
		mapSelectContract(contract: any, open: boolean) {

			this.state.contracts.selected.Featured = null;

			let request = false;
			if(this.state.contracts.selected.Contract){
				if(this.state.contracts.selected.Contract.ID != contract.ID){
					request = true;
				}
			} else {
				request = true;
			}

			if(request) {
				this.mapClearActive();

				if(new Date(contract.PullAt) < new Date() && contract.State == 'Listed'){
					this.mapFrameContract(false);

					this.state.contracts.selected.Contract = contract;
					this.state.contracts.selected.Template = this.state.contracts.search.Templates.find(x => x.FileName == contract.FileName);
				} else {
					this.$root.$data.services.api.SendWS(
						'adventures:query-from-id',
						{
							id: contract.ID,
							detailed: true
						},
						(contractsData: any) => {
							if(contractsData.payload.Contracts.length) {

								const newContract = contractsData.payload.Contracts[0];
								// Create/add Markers
								this.mapClearContractMarkers();
								newContract.Path.forEach((node: any, index: number) => {
									node.Actions.forEach((action: any) => {
										const Sit = newContract.Situations[index];

										if(this.state.ui.map.markers.actions[action.ActionType] == undefined){
											this.state.ui.map.markers.actions[action.ActionType] = [];
										}

										const markerData = {
											location: Sit.Location
										};

										this.state.ui.map.markers.actions[action.ActionType].push(markerData);
									});
								});

								// Set Selected
								this.state.contracts.selected.Contract = newContract;
								this.state.contracts.selected.Template = this.state.contracts.search.Templates.find(x => x.FileName == contract.FileName);

								if(open) {
									this.modalContractOpen();
									this.mapFrameContract(true);
								} else {
									this.mapFrameContract(false);
								}

							}
						}
					);
				}
			} else if(this.state.contracts.selected.Contract.State != 'Listed' || new Date(this.state.contracts.selected.Contract.PullAt) > new Date()) {
				this.modalContractOpen();
			}
		},
		mapUpdateRelocationRings() {

			this.state.ui.map.sources.locationHistory.data.features = [];
			this.state.ui.map.sources.relocationRings.data.features = [];

			const locationHistory = this.$root.$data.state.services.simulator.locationHistory;

			if(locationHistory.costPerNM > 0) {
				const ranges = [
					1000,
					750,
					500,
					250,
					100,
					50,
					10,
				]

				for (let i = 0; i < ranges.length; i++) {
					const km = ranges[i] / locationHistory.costPerNM;
					if(km < 8000) {
						let circle = turf.circle(locationHistory.location, km, {
							steps: 180,
							units: 'kilometers',
							properties: {
								label: '$' + (ranges[i].toLocaleString('en-gb'))
							}
						});

						circle = MapboxExt.turfCircleFix(circle);
						this.state.ui.map.sources.relocationRings.data.features.push(circle);
					}
				}

				this.state.ui.map.sources.locationHistory.data.features.push({
					type: "Feature",
					id: Eljs.getNumGUID(),
					properties: {
						label: locationHistory.name,
					},
					geometry: {
						type: "Point",
						coordinates: locationHistory.location,
					}
				})
			}
		},
		mapUpdateContract() {
			// Clear map features
			this.state.ui.map.sources.contractPaths.data.features = [];
			this.state.ui.map.sources.situationAirports.data.features = [];
			this.state.ui.map.sources.situationAirportsGhost.data.features = [];

			// Restore selected one
			//if(this.state.contracts.selected.Contract != null){
			//	if(!this.state.contracts.search.Contracts.find(x => x.ID == this.state.contracts.selected.Contract.ID)) {
			//		this.state.contracts.search.Contracts.push(this.state.contracts.selected.Contract);
			//		if(!this.state.contracts.search.Templates.find(x => x.FileName == this.state.contracts.selected.Template.FileName)) {
			//			this.state.contracts.search.Templates.push(this.state.contracts.selected.Template);
			//		}
			//	}
			//}

			// Inject all contracts
			const processedAirports = [] as string[];
			const shownFeatures = [];
			this.state.contracts.search.Contracts.forEach((contract: any) => {

				const feature = {
					type: 'Feature',
					id: contract.ID,
					properties: {
						premium: true,
						hover: false,
						active: false,
						state: 'Listed'
					},
					geometry: {
						type: 'MultiLineString',
						coordinates: [] as any
					},
				}

				let previousNode = null as Array<number>;
				contract['Situations'].forEach((situation: any, index: number) => {

					if(situation.Airport) {
						if(!processedAirports.includes(situation.Airport.ICAO)) {
							const airportFeature = {
								type: "Feature",
								id: Eljs.getNumGUID(),
								properties: {
									title: situation.Airport.ICAO + " - " + situation.Airport.Name,
									icao: situation.Airport.ICAO,
									airport: situation.Airport,
								},
								geometry: {
									type: "Point",
									coordinates: situation.Location,
								}
							};
							this.state.ui.map.sources.situationAirports.data.features.push(airportFeature);
							processedAirports.push(situation.Airport.ICAO);
						}
					}

					if(index > 0){
						var start = turf.point(previousNode);
						var end = turf.point(situation['Location']);
						var greatCircle = turf.greatCircle(start, end, {
							properties: {
								name: "",
								id: ""
							}
						});

						if(greatCircle.geometry.type == 'LineString'){
							feature.geometry.coordinates.push(greatCircle.geometry.coordinates);
						} else {
							greatCircle.geometry.coordinates.forEach((pos: any) => {
								feature.geometry.coordinates.push(pos);
							});
						}

						let state = contract.State;

						if(state == "Listed") {
							const template = this.state.contracts.search.Templates.find(x => x.FileName == contract.FileName);
							if(template.IsCustom) {
								state = "Premium";
								feature.properties.premium = true;
							}
						}

						feature.properties.state = state;

						if(this.state.contracts.selected.Contract){
							if(this.state.contracts.selected.Contract.ID == feature.id){
								feature.properties.active = true;
							}
						}
					}
					previousNode = situation['Location'];
				});

				shownFeatures.push(feature);
			});

			// Sort Features
			this.state.ui.map.sources.contractPaths.data.features = shownFeatures.sort((a, b) => (b.properties.premium ? 0 : 1) - (a.properties.premium ? 0 : 1) );

			// Add Airports
			this.state.contracts.search.Airports.forEach((airport: any) => {
				if(!processedAirports.includes(airport.ICAO)) {

					const airportFeature = {
						type: "Feature",
						id: Eljs.getNumGUID(),
						properties: {
							title: airport.ICAO + " - " + airport.Name,
							icao: airport.ICAO,
							airport: airport,
						},
						geometry: {
							type: "Point",
							coordinates: airport.Location,
						}
					};
					this.state.ui.map.sources.situationAirportsGhost.data.features.push(airportFeature);
				}
			});

			//this.mapRefresh(['contractPaths','situationAirports','situationAirportsGhost']);
		},
		mapUpdateContractStates() {
			this.state.contracts.search.Contracts.forEach((f, index) => {
				const contractFeature = this.state.ui.map.sources.contractPaths.data.features.find(x => x.id == f.ID);
				contractFeature.properties.state = f.State;
			});

			this.mapRefresh(['contractPaths']);
		},
		mapClearContractMarkers() {
			Object.keys(this.state.ui.map.markers.actions).forEach((actionKey: string) => {
				this.state.ui.map.markers.actions[actionKey] = [];
			});
		},
		mapClearActive() {
			if(this.state.ui.map.sources.contractPaths.data.features){
				this.state.ui.map.sources.contractPaths.data.features.forEach((f, index) => {
					f.properties.active = false;
				});
				this.mapRefresh(['contractPaths','situationAirports','situationAirportsGhost']);
			}
		},
		mapClearHover(names: string[]) {
			if(this.$os.UntrackedMap['p42_contrax_map_main'].map) {
				names.forEach(name => {
					if(this.state.ui.map.sources[name].data.features){
						this.state.ui.map.sources[name].data.features.forEach((f :any, index :number) => {
							this.$os.UntrackedMap['p42_contrax_map_main'].map.setFeatureState({
								sourceLayer: f.sourceLayer,
								source: name,
								id: f.id
							}, {
								hover: false
							});
						});
					}
				});
			}
		},
		mapAddSnowDepth() {
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.addSource('snow-tiles', {
			//	"type": "raster",
			//	"tiles": [
			//		"https://idpgis.ncep.noaa.gov/arcgis/rest/services/NWS_Observations/NOHRSC_Snow_Analysis/MapServer/export?dpi=96&transparent=true&format=png8&layers=show:3&bbox={left},{bottom},{right},{top}&bboxSR=102100&imageSR=102100&size={tilesize},{tilesize}&f=image"
			//	],
			//	"tileSize": 512
			//});
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.addSource('snow-tiles', {
			//	'type': 'image',
			//	'url': 'https://www.nnvl.noaa.gov/images/globaldata/SnowIceCover_Daily.png',
			//	'coordinates': [
			//		[-179.9, 80.0],
			//		[179.9, 80.0],
			//		[179.9, -80.0],
			//		[-179.9, -80.0]
			//	]
			//});
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.addLayer({
			//	"id": 'snow-tiles',
			//	"type": "raster",
			//	"source": 'snow-tiles',
			//	"minzoom": 0,
			//	"maxzoom": 22
			//});
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.moveLayer('snow-tiles', this.$os.UntrackedMap['p42_contrax_map_main'].map.getStyle().layers[3].id);
		},
		mapUpdateRadar() {
			// https://www.rainviewer.com/api.html
			//"https://tilecache.rainviewer.com/v2/radar/{ts}/{size}/{z}/{x}/{y}/{color}/{options}.png"
			clearInterval(this.state.ui.map.radarFetchTimer);
			let TimeIndex = 0;
			const fetchData = () => {
				fetch('https://api.rainviewer.com/public/maps.json', { method: 'get' })
				.then(response => response.json())
				.then((data) => {
					if(this.state.ui.map.loaded) {
						data.forEach((radarTime :number, index :number) => {
							let existing = this.$os.UntrackedMap['p42_contrax_map_main'].map.getSource('radar-tiles-' + index);
							if(!existing) {
								const size = this.$root.$data.state.device.isAppleWebkit ? 256 : 512;
								this.$os.UntrackedMap['p42_contrax_map_main'].map.addSource('radar-tiles-' + index, {
									"type": "raster",
									"tiles": [
										"https://tilecache.rainviewer.com/v2/radar/" + data[index] + "/" + size + "/{z}/{x}/{y}/4/1_0.png"
									],
									"tileSize": size
								});
								this.$os.UntrackedMap['p42_contrax_map_main'].map.addLayer({
									"id": 'radar-tiles-' + index,
									"type": "raster",
									"source": 'radar-tiles-' + index,
									"minzoom": 0,
									"maxzoom": 22
								});
								this.$os.UntrackedMap['p42_contrax_map_main'].map.moveLayer('radar-tiles-' + index, this.$os.UntrackedMap['p42_contrax_map_main'].map.getStyle().layers[3].id);

								this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('radar-tiles-' + index, 'raster-opacity', 0);
								this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('radar-tiles-' + index, 'raster-fade-duration', 1000);
							} else {
								existing.tiles[0] = "https://tilecache.rainviewer.com/v2/radar/" + data[index] + "/512/{z}/{x}/{y}/4/1_0.png";
								this.$os.UntrackedMap['p42_contrax_map_main'].map.style._sourceCaches['radar-tiles-' + index].clearTiles();
								this.$os.UntrackedMap['p42_contrax_map_main'].map.style._sourceCaches['radar-tiles-' + index].update(this.$os.UntrackedMap['p42_contrax_map_main'].map.transform);
								this.$os.UntrackedMap['p42_contrax_map_main'].map.triggerRepaint();
							}

						});

						clearInterval(this.state.ui.map.radarTimer);
						this.state.ui.map.radarTimer = setInterval(() => {
							if(!this.app.app_sleeping) {
								if(!this.state.ui.map.radarAnimated) {
									TimeIndex = data.length - 1;
								}
								if(TimeIndex < data.length) {
									for (let i = 0; i < data.length; i++) {
										if(i == TimeIndex) {
											this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('radar-tiles-' + i, 'raster-opacity', 0.5);
										} else {
											setTimeout(() => {
												this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('radar-tiles-' + i, 'raster-opacity', 0);
											}, 30);
										}
									}
								}

								if(TimeIndex < data.length + 2) {
									TimeIndex++;
								} else {
									TimeIndex = 0;
								}
							}
						}, 250);
					}
				}).catch((err) => {

				});
			}
			fetchData();
			this.state.ui.map.radarFetchTimer = setInterval(() => fetchData, 600000);
		},
		mapRefresh(sources :string[]) {
			sources.forEach((source) => {
				this.$os.UntrackedMap['p42_contrax_map_main'].map.getSource(source).setData(this.state.ui.map.sources[source].data);
			})
		},
		mapFrameContract(instant :boolean) {

			const nodes = [] as any[];
			this.state.contracts.selected.Contract.Situations.forEach((sit : any) => {
				nodes.push(sit.Location);
			});

			if(!this.state.ui.currentModal) {
				const frameWidth = (this.$refs['app-frame'] as HTMLElement).offsetWidth;
				const paddx = frameWidth * 0.15;
				const paddy = frameWidth * 0.1;


				//const bounds = turf.bbox(turf.lineString(nodes));

				//console.log(bounds);

				//this.$os.UntrackedMap['p42_contrax_map_main'].map.fitBounds(nodes, {
				//	padding: { left: paddx, top: 100 + paddy, right: paddx + 50, bottom: 320 + paddy },
				//	essential: true
				//});

				MapboxExt.fitBoundsExt(this.$os.UntrackedMap['p42_contrax_map_main'].map, turf.bbox(turf.lineString(nodes)), {
					padding: { left: paddx, top: 100 + paddy, right: paddx + 50, bottom: 320 + paddy },
					pitch: 0,
					duration: instant ? 0 : 2000,
				}, null);
			}

			const contractFeature = this.state.ui.map.sources.contractPaths.data.features.find(x => x.id == this.state.contracts.selected.Contract.ID);
			contractFeature.properties.active = true;
			this.mapRefresh(['contractPaths']);
		},
		mapToggleSat(state? :boolean) {
			if(state !== undefined) {
				this.state.ui.map.displayLayer.sat = state;
			} else {
				this.state.ui.map.displayLayer.sat = !this.state.ui.map.displayLayer.sat;
			}

			this.app.app_theme_mode = this.state.ui.map.displayLayer.sat ? 'theme--dark' : null;
			this.$os.UntrackedMap['p42_contrax_map_main'].map.setLayoutProperty('mapbox-satellite', 'visibility', this.state.ui.map.displayLayer.sat ? 'visible' : 'none');
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.setLayoutProperty('mapbox-satellite', 'visibility', 'visible');
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('mapbox-satellite', 'raster-opacity', 0.8);
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('mapbox-satellite', 'raster-fade-duration', 200);
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('mapbox-satellite', 'raster-opacity', 0.1);
			this.resetTheme();
			this.stateSave();
		},

		filtersSet(query :string) {
			//this.filtersClear();
			this.state.ui.filters.query = query;
			this.filterSetBounds();
		},
		filtersContractsClear(state: number) {
			this.mapClearContractMarkers();
			if(state != 1){
				this.state.ui.map.sources.contractPaths.data.features = [];
				this.state.ui.map.sources.situationAirports.data.features = [];
				this.state.ui.map.sources.situationAirportsGhost.data.features = [];
				this.mapRefresh(['contractPaths','situationAirports','situationAirportsGhost']);
			}
			this.state.contracts.search.FeaturedPrevious = this.state.contracts.search.FeaturedFull;

			this.state.contracts.search.State = state;
			this.state.contracts.search.Count = 0;
			this.state.contracts.search.Destinations = 0;
			this.state.contracts.search.Limit = 0;
			this.state.contracts.search.Contracts = [];
			this.state.contracts.search.Templates = [];
			this.state.contracts.search.Featured = [];
		},
		filterSetBounds() {
			if(this.$os.UntrackedMap['p42_contrax_map_main']) {
				const mapPadding = this.state.ui.map.padding;
				const mapRef = (this.$refs.map as any).$el;
				const mapMarkers = {
					top: mapPadding.top,
					right: mapRef.offsetWidth - mapPadding.right,
					bottom: mapRef.offsetHeight - mapPadding.bottom,
					left: mapPadding.left,
				}

				const NW = this.$os.UntrackedMap['p42_contrax_map_main'].map.unproject([mapMarkers.left, mapMarkers.top]);
				const SE = this.$os.UntrackedMap['p42_contrax_map_main'].map.unproject([mapMarkers.right, mapMarkers.bottom]);

				var line = turf.lineString([[NW.lng, NW.lat], [SE.lng, SE.lat]]);
				var bbox = turf.bbox(line) as any;

				this.state.ui.filters.bounds = {
					nw: [bbox[0],bbox[3]],
					se: [bbox[2],bbox[1]],
				}

				this.state.ui.filters.boundsMoved = false;
				this.filtersSearch(false);
			}
		},
		filterMove(ref: string, change: number) {
			if(this.state.contracts.search.Contracts){
				let index = 0;
				if(this.state.contracts.selected.Contract) {
					index = this.state.contracts.search.Contracts.findIndex((x :any) => x.ID == this.state.contracts.selected.Contract.ID);
					index += change;
				}
				let iter = 0;
				if(index >= this.state.contracts.search.Contracts.length){
					index = 0;
				} else if(index < 0){
					index = this.state.contracts.search.Contracts.length - 1;
				}
				while(new Date(this.state.contracts.search.Contracts[index].PullAt) < new Date() && this.state.contracts.search.Contracts[index].State == 'Listed') {
					index += change;
					if(iter >= this.state.contracts.search.Contracts.length - 1){
						if(this.state.ui.currentModal == 'contract'){
							this.state.ui.currentModal = '';
						}
						index = 0;
						return;
					}
					if(index >= this.state.contracts.search.Contracts.length){
						index = 0;
					} else if(index < 0){
						index = this.state.contracts.search.Contracts.length - 1;
					}
					iter++;
				}
				this.mapSelectContract(this.state.contracts.search.Contracts[index], false);
			}
		},
		filtersChanged() {
			this.state.ui.filters.changed = true;
			this.stateSave();
		},
		filtersRefreshTitle() {
			this.state.ui.filterString = "";

			const newFilterStr = [];
			if(this.state.ui.filters.type != "any") {
				newFilterStr.push("Type:&nbsp;<strong>" + this.state.ui.filters.type + "</strong>");
			}
			if(this.state.ui.filters.types.length > 0) {
				if(this.state.ui.filters.types.length < this.state.ui.filterReferences.types.length / 2) {
					const Excluded = [];
					if(this.state.ui.filters.types.includes(0)) { Excluded.push("helis") }
					if(this.state.ui.filters.types.includes(1)) { Excluded.push("small&nbsp;GA") }
					if(this.state.ui.filters.types.includes(2)) { Excluded.push("turboprops") }
					if(this.state.ui.filters.types.includes(3)) { Excluded.push("small&nbsp;jets") }
					if(this.state.ui.filters.types.includes(4)) { Excluded.push("narrow-body") }
					if(this.state.ui.filters.types.includes(5)) { Excluded.push("wide-body") }
					newFilterStr.push("Excludes&nbsp;<strong>" + Excluded.join(', ') + "</strong>");
				} else {
					const Included = [];
					if(!this.state.ui.filters.types.includes(0)) { Included.push("helis") }
					if(!this.state.ui.filters.types.includes(1)) { Included.push("small&nbsp;GA") }
					if(!this.state.ui.filters.types.includes(2)) { Included.push("turboprops") }
					if(!this.state.ui.filters.types.includes(3)) { Included.push("small&nbsp;jets") }
					if(!this.state.ui.filters.types.includes(4)) { Included.push("narrow-body") }
					if(!this.state.ui.filters.types.includes(5)) { Included.push("wide-body") }
					newFilterStr.push("Includes&nbsp;<strong>" + Included.join(', ') + "</strong>");
				}
			}
			if(this.state.ui.filters.companies.length > 0) {
				const Included = [];
				if(!this.state.ui.filters.companies.includes(this.state.ui.filterReferences.companies[0])) { Included.push("ClearSky") }
				if(!this.state.ui.filters.companies.includes(this.state.ui.filterReferences.companies[1])) { Included.push("Coyote") }
				if(!this.state.ui.filters.companies.includes(this.state.ui.filterReferences.companies[2])) { Included.push("Skypark Travel") }
				newFilterStr.push("Includes&nbsp;<strong>" + Included.join(', ') + "</strong>");
			}
			if(this.state.ui.filters.query.length) {
				newFilterStr.push("Search:&nbsp;<strong>" + this.state.ui.filters.query + "</strong>");
			}
			if(this.state.ui.filters.range[0] > 10 || this.state.ui.filters.range[1] < 7800) {
				newFilterStr.push("Dist.:&nbsp;<strong>" + this.state.ui.filters.range[0].toLocaleString('en-gb') + "-" + this.state.ui.filters.range[1].toLocaleString('en-gb') + "nm</strong>");
			}
			if(this.state.ui.filters.runways[1] < 1000) {
				newFilterStr.push("Rwy. Length:&nbsp;<strong>" + this.state.ui.filters.runways[0].toLocaleString('en-gb') + "-" + this.state.ui.filters.runways[1].toLocaleString('en-gb') + "ft</strong>");
			}
			if(this.state.ui.filters.legsCount[0] > 1) {
				newFilterStr.push("Legs:&nbsp;<strong>" + ((this.state.ui.filters.legsCount[0] == this.state.ui.filters.legsCount[1]) ? this.state.ui.filters.legsCount[0].toLocaleString('en-gb') : this.state.ui.filters.legsCount[0].toLocaleString('en-gb') + "-" + this.state.ui.filters.legsCount[1].toLocaleString('en-gb')) + "</strong>");
			}
			if(this.state.ui.filters.rwyCount[1] < 5) {
				newFilterStr.push("Rwy. Count:&nbsp;<strong>" + ((this.state.ui.filters.rwyCount[0] == this.state.ui.filters.rwyCount[1]) ? this.state.ui.filters.rwyCount[0].toLocaleString('en-gb') : this.state.ui.filters.rwyCount[0].toLocaleString('en-gb') + "-" + this.state.ui.filters.rwyCount[1].toLocaleString('en-gb')) + "</strong>");
			}
			if(this.state.ui.filters.rwySurface != 'any') {
				newFilterStr.push("Rwy.&nbsp;Sfc:&nbsp;<strong>" + this.state.ui.filters.rwySurface + "</strong>");
			}
			switch(this.state.ui.filters.sort) {
				case 'topography_var': { newFilterStr.push("Sort&nbsp;by&nbsp;<strong>topography&nbsp;variations</strong>"); break; }
				case 'aircraft': { newFilterStr.push("Sort&nbsp;by&nbsp;<strong>current&nbsp;aircraft&nbsp;loc.</strong>"); break; }
				case 'distance': { newFilterStr.push("Sort&nbsp;by&nbsp;<strong>distance</strong>"); break; }
				case 'ending': { newFilterStr.push("Sort&nbsp;by&nbsp;<strong>ending&nbsp;soon</strong>"); break; }
				case 'xp': { newFilterStr.push("Sort&nbsp;by&nbsp;<strong>XP</strong>"); break; }
				case 'reward': { newFilterStr.push("Sort&nbsp;by&nbsp;<strong>reward</strong>"); break; }
				case 'reward-distance': { newFilterStr.push("Sort by &nbsp;<strong>reward&nbsp;over&nbsp;distance</strong>"); break; }
			}
			if(this.state.ui.filters.requiresLight) {
				newFilterStr.push("Rwy.&nbsp;lights&nbsp;required");
			}
			if(this.state.ui.filters.requiresILS) {
				newFilterStr.push("ILS&nbsp;required");
			}
			if(this.state.ui.filters.onlyCustomContracts) {
				newFilterStr.push("Only&nbsp;custom&nbsp;contracts");
			}

			this.state.ui.filterRestricted = newFilterStr.length > 0;
			this.state.ui.filterString = newFilterStr.join("&emsp;");
		},
		filtersClear() {
			this.state.ui.filters.query = '';
			this.state.ui.filters.type = 'any';
			this.state.ui.filters.subType = '';
			this.state.ui.filters.range[0] = 10;
			this.state.ui.filters.range[1] = 9999;
			this.state.ui.filters.legsCount[0] = 1;
			this.state.ui.filters.legsCount[1] = 50;
			this.state.ui.filters.runways[0] = 10;
			this.state.ui.filters.runways[1] = 20000;
			this.state.ui.filters.rwyCount[0] = 1;
			this.state.ui.filters.rwyCount[1] = 20;
			this.state.ui.filters.types = [];
			this.state.ui.filters.companies = [];
			this.state.ui.filters.weatherExcl = { precip: [], wind: [], vis: [], };
			this.state.ui.filters.sort = 'relevance';
			this.state.ui.filters.rwySurface = 'any';
			this.state.ui.filters.sortAsc = false;
			this.state.ui.filters.changed = false;
			this.state.ui.filters.requiresLight = false;
			this.state.ui.filters.onlyCustomContracts = false;
			this.state.ui.filters.requiresILS = false;
			this.stateSave();
		},
		filtersSearch(dice :boolean) {

			this.state.ui.currentModal = null;
			this.state.ui.filters.sent = true;

			if(this.state.ui.filters.types.length == this.state.ui.filterReferences.types.length) {
				this.state.ui.filters.types = [];
			}

			if(this.state.ui.filters.companies.length == this.state.ui.filterReferences.companies.length) {
				this.state.ui.filters.companies = [];
			}

			this.filtersRefreshTitle();

			if(this.state.contracts.search.State != 1) {
				if(this.$root.$data.state.services.api.connected) {
					this.filtersContractsClear(1);

					if(dice) {
						this.$os.TrackEvent('Contracts', 'Filters', 'Dice', 1);
					}

					// Response function
					const responseFn = (contractsData: any) => {
						this.state.ui.filters.sent = false;

						contractsData.payload.Featured.forEach(f => {
							if(this.state.contracts.search.Featured.length == 0) {
								if(!this.state.contracts.search.FeaturedPrevious.find(x => x.FileNameE == f.FileNameE && f.FeatureType != 'carrot')) {
									this.state.contracts.search.Featured.push(f);
								}
							}
						});

						this.state.contracts.search.FeaturedFull = contractsData.payload.Featured;
						this.state.contracts.search.Contracts = contractsData.payload.Contracts;
						this.state.contracts.search.Templates = contractsData.payload.Templates;
						this.state.contracts.search.Airports = contractsData.payload.Airports;
						this.state.contracts.search.Count = contractsData.payload.Count;
						this.state.contracts.search.Destinations = contractsData.payload.Destinations;
						this.state.contracts.search.Limit = contractsData.payload.Limit;
						this.state.contracts.search.State = 0;

						if(this.app.loaded_state != null) {
							if(!this.state.contracts.selected.restored) {
								this.state.contracts.selected.restored = true;

								if(this.app.loaded_state.contracts.selected.Contract) {
									const restoreSelect = this.state.contracts.search.Contracts.find(x => x.ID == this.app.loaded_state.contracts.selected.Contract);
									if(restoreSelect) {
										this.mapSelectContract(restoreSelect, false);
									}
								}
							}
						} else {
							this.state.contracts.selected.restored = true;
						}

						if(contractsData.payload.Reframe){

							const nodes = [] as any[];
							this.state.contracts.search.Contracts.forEach(contract => {
								contract.Situations.forEach((sit : any) => {
									nodes.push(sit.Location);
								});
							});

							if(nodes.length) {
								const frameWidth = (this.$refs['app-frame'] as HTMLElement).offsetWidth;
								const paddx = frameWidth * 0.25;
								const paddy = frameWidth * 0.1;
								MapboxExt.fitBoundsExt(this.$os.UntrackedMap['p42_contrax_map_main'].map, turf.bbox(turf.lineString(nodes)), {
									padding: { left: paddx, top: 100 + paddy, right: paddx + 50, bottom: 320 + paddy },
									pitch: 0,
									duration: 1000,
								}, null);
							}
						}

						if(dice && this.state.contracts.search.Contracts.length) {
							this.mapSelectContract(this.state.contracts.search.Contracts[0], true);
						}

						this.mapUpdateContract();
						this.stateSave();
					}

					const center = this.$os.UntrackedMap['p42_contrax_map_main'].map.getCenter();
					const zoom = this.$os.UntrackedMap['p42_contrax_map_main'].map.getZoom();

					const queryoptions = {
						type: this.state.ui.filters.type,
						subType: this.state.ui.filters.subType,
						types: this.state.ui.filters.types,
						companies: this.state.ui.filters.companies,
						state: 'Listed,Saved,Active',
						priorityState: 'Saved,Active',
						location: null as number[],
						range: this.state.ui.filters.range,
						legsCount: this.state.ui.filters.legsCount,
						runways: this.state.ui.filters.runways,
						rwySurface: this.state.ui.filters.rwySurface,
						rwyCount: this.state.ui.filters.rwyCount,
						query: this.state.ui.filters.query,
						bounds: zoom > 0.8 ? this.state.ui.filters.bounds : null,
						weatherExcl: this.state.ui.filters.weatherExcl,
						requiresLight: this.state.ui.filters.requiresLight ? this.state.ui.filters.requiresLight : null,
						onlyCustomContracts: this.state.ui.filters.onlyCustomContracts ? this.state.ui.filters.onlyCustomContracts : null,
						requiresILS: this.state.ui.filters.requiresILS ? this.state.ui.filters.requiresILS : null,
						center: [center.lng, center.lat],
						sort: this.state.ui.filters.sort,
						sortAsc: this.state.ui.filters.sortAsc,
						diced: dice,
						detailed: false,
						limit: 20,
					}

					if(this.$root.$data.state.services.simulator.live){
						queryoptions.location = [this.$root.$data.state.services.simulator.location.Lon, this.$root.$data.state.services.simulator.location.Lat]
					}

					this.$root.$data.services.api.SendWS(
						'adventures:query-from-filters', queryoptions, responseFn
					);

				} else {
					this.filtersContractsClear(-1);
				}
			}
		},
		filterClearAndReset() {
			this.filtersClear();
			this.filterSetBounds();
		},
		filterSetType(type :number) {
			if(this.state.ui.filters.types.length == 0) {
				Object.assign(this.state.ui.filters.types, this.state.ui.filterReferences.types);
				this.state.ui.filters.types.splice(this.state.ui.filters.types.indexOf(type), 1);
			} else {
				if(this.state.ui.filters.types.includes(type)) {
					this.state.ui.filters.types.splice(this.state.ui.filters.types.indexOf(type), 1);
				} else {
					this.state.ui.filters.types.push(type);
				}
			}
		},
		filterSetCompany(company: string) {
			if(this.state.ui.filters.companies.length == 0) {
				Object.assign(this.state.ui.filters.companies, this.state.ui.filterReferences.companies);
				this.state.ui.filters.companies.splice(this.state.ui.filters.companies.indexOf(company), 1);
			} else {
				if(this.state.ui.filters.companies.includes(company)) {
					this.state.ui.filters.companies.splice(this.state.ui.filters.companies.indexOf(company), 1);
				} else {
					this.state.ui.filters.companies.push(company);
				}
			}
		},
		filterSetWx(cat: string, value: string) {
			const weatherExcl = this.state.ui.filters.weatherExcl;
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
		filtersSetRwySurface(cat: string) {
			this.state.ui.filters.rwySurface != cat ? this.state.ui.filters.rwySurface = cat : this.state.ui.filters.rwySurface = 'any';
			this.filtersChanged();
		},
		filtersSetFirst(cat: string){
			this.state.ui.filters.type != cat ? this.state.ui.filters.type = cat : this.state.ui.filters.type = 'any';  this.state.ui.filters.subType = 'any';
			this.filtersChanged();
		},
		filtersSetSecond(cat: string){
			this.state.ui.filters.subType != cat ? this.state.ui.filters.subType = cat : this.state.ui.filters.subType = '';
			this.filtersChanged();
		},
		filtersSetSort(cat: string){
			this.state.ui.filters.sort != cat ? this.state.ui.filters.sort = cat : this.state.ui.filters.sort = 'relevance';
			switch(cat) {
				case 'ending': {
					if(this.state.ui.filters.sort == cat) {
						this.filtersSetSortAsc(true);
					}
					break;
				}
				case 'xp': {
					if(this.state.ui.filters.sort == cat) {
						this.filtersSetSortAsc(false);
					}
					break;
				}
				case 'reward': {
					if(this.state.ui.filters.sort == cat) {
						this.filtersSetSortAsc(false);
					}
					break;
				}
				case 'topography_var': {
					if(this.state.ui.filters.sort == cat) {
						this.filtersSetSortAsc(false);
					}
					break;
				}
			}

			this.filtersChanged();
		},
		filtersSetSortAsc(cat: boolean){
			this.state.ui.filters.sortAsc = cat;
			this.filtersChanged();
		},

		contractExpire(contract :any) {
			//this.$os.UntrackedMap['p42_contrax_map_main'].map.setFeatureState({
			//	source: 'contractPaths',
			//	id: contract.ID
			//}, {
			//	state: 'Expired'
			//});
		},

		featuredSelect(TemplateFeature :any){
			this.state.contracts.selected.Contract = null;
			this.state.contracts.selected.Featured = TemplateFeature;
		},
		featuredHide(TemplateFeature :any){
			this.state.contracts.search.Featured.splice(this.state.contracts.search.Featured.findIndex(x => x.FileName == TemplateFeature), 1);
			this.$root.$data.services.api.SendWS('adventures:featured:hide', {
				FileName: TemplateFeature
			});
		},
		featuredBrowse(TemplateQuery :any){
			this.state.ui.filters.query = TemplateQuery;
			this.filtersSearch(false);
		},

		modalContractOpen() {

			// Return if already present
			if(this.$root.$data.state.ui.modals.queue.length) {
				if(this.$root.$data.state.ui.modals.queue[0].type != 'contract') {
					return;
				}
			}

			this.state.ui.contractModal = {
				type: 'contract',
				title: 'Start fees',
				data: {
					App :this.app,
					Results: this.state.contracts.search,
					Selected: this.state.contracts.selected,
				},
				offset: (index :number) => {
					if(this.state.contracts.selected.Contract != null) {
						this.filterMove('search_results', index)
					} else {
						this.mapSelectContract(this.state.contracts.search[0], false);
					}
				},
				func: () => {
					this.state.ui.contractModal = null;
				}
			}
			this.$os.modalPush(this.state.ui.contractModal);

		},

		modelClose(reframe :boolean) {
			this.state.ui.currentModal = null;
			this.resetTheme();
			if(reframe) {
				this.mapFrameContract(true);
			}
		},

		stateSave() {
			this.app.StateSave({
				contracts: {
					selected: {
						Contract: this.state.contracts.selected.Contract ? this.state.contracts.selected.Contract.ID : null,
						Template: this.state.contracts.selected.Template ? this.state.contracts.selected.Template.FileName : null,
					}
				},
				ui: {
					currentModal: this.state.ui.currentModal,
					map: {
						location: this.state.ui.map.location,
						zoom: this.state.ui.map.zoom,
						heading: this.state.ui.map.heading,
						pitch: this.state.ui.map.pitch,
						displayLayer: {
							sat: this.state.ui.map.displayLayer.sat,
						},
					},
					filters: this.state.ui.filters
				}
			});
		},
		keyDownEv(ev :KeyboardEvent) {
			switch(ev.code) {
				case "Space": {
					if(this.state.ui.currentModal == null && this.state.contracts.selected.Contract) {
						this.mapSelectContract(this.state.contracts.selected.Contract, true);
					}
					break;
				}
				case "ArrowRight": {
					this.filterMove('search_results', 1)
					break;
				}
				case "ArrowLeft": {
					this.filterMove('search_results', -1)
					break;
				}
			}
		},

		resetTheme() {
			const layers = this.state.ui.map.layers;
			if(this.state.ui.map.displayLayer.sat) {
				layers.relocationRingsShadow.paint['line-opacity'] = 0.3;
				layers.contractPathsShadow.paint['line-opacity'] = 0.2;
				layers.contractPathsShadow.paint['line-color'] = '#000000';
				layers.situationAirportsLabel.paint['text-color'] = "#FFFFFF";
				layers.situationAirportsLabelDetailed.paint['text-color'] = "#FFFFFF";
				layers.situationAirportsGhostLabel.paint['text-color'] = "#FFFFFF";
				layers.situationAirportsGhostLabelDetailed.paint['text-color'] = "#FFFFFF";
				layers.relocationRings.paint['line-color'] = "rgba(255,165,0,0.8)";
				layers.relocationRingsLabels.paint['text-color'] = "rgba(255,165,0,0.8)";
				layers.locationHistory.paint['circle-color'] = "rgba(255,165,0,0.8)";
				layers.locationHistory.paint['circle-stroke-color'] = "rgba(255,165,0,0.8)";
				layers.locationHistoryLabels.paint['text-color'] = "rgba(255,165,0,0.8)";
				layers.contractPaths.paint['line-color'] = [
					'match',
					['get', 'state'],
					'Listed', '#4285f4',
					'Active', '#46b446',
					'Saved', '#46b446',
					'Failed', '#b44646',
					'Premium', 'rgb(255,165,0)',
					'#CCC'
				]
				this.state.ui.map.theme = 'theme--dark';

				this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('sky-day', 'sky-opacity', 0);
				this.$os.UntrackedMap['p42_contrax_map_main'].map.setPaintProperty('sky-night', 'sky-opacity', 1);
				//this.$os.UntrackedMap['p42_contrax_map_main'].map.setFog({ 'color': 'rgba(31, 31, 31, 1)' });
			} else {
				layers.relocationRingsShadow.paint['line-opacity'] = 0;
				layers.contractPathsShadow.paint['line-opacity'] = 0.1;
				layers.contractPathsShadow.paint['line-color'] = [
					'match',
					['get', 'state'],
					'Listed', '#4285f4',
					'Active', '#46b446',
					'Saved', '#46b446',
					'Failed', '#b44646',
					'Premium', 'rgba(255,165,0,0.3)',
					'#CCC'
				]
				if(this.$os.getConfig(['ui', 'theme']) == 'theme--dark') {
					layers.situationAirportsLabel.paint['text-color'] = "#FFFFFF";
					layers.situationAirportsLabelDetailed.paint['text-color'] = "#FFFFFF";
					layers.situationAirportsGhostLabel.paint['text-color'] = "#FFFFFF";
					layers.situationAirportsGhostLabelDetailed.paint['text-color'] = "#FFFFFF";
					layers.relocationRings.paint['line-color'] = "rgba(255,165,0,0.3)";
					layers.relocationRingsLabels.paint['text-color'] = "rgba(255,165,0,0.8)";
					layers.locationHistory.paint['circle-color'] = "rgba(255,165,0,0.8)";
					layers.locationHistory.paint['circle-stroke-color'] = "rgba(255,165,0,0.8)";
					layers.locationHistoryLabels.paint['text-color'] = "rgba(255,165,0,0.8)";
					layers.contractPaths.paint['line-color'] = [
						'match',
						['get', 'state'],
						'Listed', '#4285f4',
						'Active', '#46b446',
						'Saved', '#46b446',
						'Failed', '#b44646',
						'Premium', 'rgba(255,165,0,1)',
						'#CCC'
					]
					this.state.ui.map.theme = 'theme--dark';

				} else {
					layers.situationAirportsLabel.paint['text-color'] = "#000000";
					layers.situationAirportsLabelDetailed.paint['text-color'] = "#000000";
					layers.situationAirportsGhostLabel.paint['text-color'] = "#000000";
					layers.situationAirportsGhostLabelDetailed.paint['text-color'] = "#000000";
					layers.relocationRings.paint['line-color'] = "rgba(153,99,0,0.4)";
					layers.relocationRingsLabels.paint['text-color'] = "rgb(153,99,0)";
					layers.locationHistory.paint['circle-color'] = "rgba(153,99,0,0.8)";
					layers.locationHistory.paint['circle-stroke-color'] = "rgba(153,99,0,0.8)";
					layers.locationHistoryLabels.paint['text-color'] = "rgba(153,99,0,0.8)";
					layers.contractPaths.paint['line-color'] = [
						'match',
						['get', 'state'],
						'Listed', '#4285f4',
						'Active', '#46b446',
						'Saved', '#46b446',
						'Failed', '#b44646',
						'Premium', 'rgba(153,99,0,1)',
						'#CCC'
					]
					this.state.ui.map.theme = 'theme--bright';
				}
			}

			if(this.state.ui.map.displayLayer.sat) {
				this.app.SetThemeOption(this.$root, {
					status: {
						bright: StatusType.BRIGHT,
						dark: StatusType.BRIGHT,
						shaded: true,
					},
					nav: {
						bright: StatusType.BRIGHT,
						dark: StatusType.BRIGHT,
						shaded: true,
					}
				});
			} else {
				this.app.SetThemeOption(this.$root, null);
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					if(this.$os.UntrackedMap['p42_contrax_map_main'])
					{
						setTimeout(() => {
							this.filterSetBounds();
						}, 5)
					}
					break;
				}
				case 'disconnect': {
					this.filtersContractsClear(-1);
					break;
				}
				case 'adventure': {
					this.$ContractMutator.EventInList(wsmsg, this.state.contracts.search);
					this.$ContractMutator.Event(wsmsg, this.state.contracts.selected.Contract, this.state.contracts.selected.Template)
					this.mapUpdateContractStates();
					break;
				}
				case 'locationhistory': {
					switch(wsmsg.name[1]){
						case 'latest': {
							this.mapUpdateRelocationRings();
							break;
						}
					}
					break;
				}
				case 'notification': {
					switch(wsmsg.payload.Type){
						case 'Success':
						case 'Fail': {
							this.state.ui.reloadOnWake = true;
							break;
						}
					}
					break;
				}
			}
		},
	}
});
</script>

<style lang="scss">
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
@import './../../../sys/scss/mixins.scss';
.p42_contrax {

	.theme--bright &,
	&.theme--bright {
		.results {
			&_section {
				&.theme--dark {
					color: #FFF;
					background: linear-gradient(to bottom, rgba($ui_colors_bright_shade_5, 0) 0, rgba($ui_colors_bright_shade_5, 0.3) 40px, rgba($ui_colors_bright_shade_5, 0.2) 100%);
				}
			}
			&_header {
				.search-rgn-btn {
					&.invisible {
						background-color: $ui_colors_bright_button_go;
					}
				}
			}
			&_nav {
				&_back {
					background-image: url(../../../sys/assets/framework/dark/arrow_left.svg);
					&.theme--dark {
						background-image: url(../../../sys/assets/framework/bright/arrow_left.svg);
					}
				}
				&_forward {
					background-image: url(../../../sys/assets/framework/dark/arrow_right.svg);
					&.theme--dark {
						background-image: url(../../../sys/assets/framework/bright/arrow_right.svg);
					}
				}
			}
		}
		.map {
			background-color: #c6c5c3;
			&_controls {
				&_sat {
					background-image: url(../../../sys/assets/icons/dark/sat.svg);
					&.info {
						background-image: url(../../../sys/assets/icons/bright/sat.svg);
					}
				}
			}
			&_marker {
				&_history {
					background-color: transparent;
					border: 3px solid rgb(153, 99, 0);
				}
				&_position {
					background-color: $ui_colors_bright_button_info;
					border: 3px solid $ui_colors_bright_shade_0;
					@include shadowed($ui_colors_bright_shade_5);
				}
			}
		}
		.filter {
			&_entry {
				&.lined {
					&-t {
						border-top: 1px solid rgba($ui_colors_bright_shade_5, 0.2);
					}
					&-b {
						border-bottom: 1px solid rgba($ui_colors_bright_shade_5, 0.2);
					}
				}
				.aircraft_type {
					> span span:first-child {
						opacity: 0.2;
					}
					&_heli {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/dark/acf_heli.svg);
						}
					}
					&_ga {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/dark/acf_ga.svg);
						}
					}
					&_turbo {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/dark/acf_turbo.svg);
						}
					}
					&_jet {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/dark/acf_jet.svg);
						}
					}
					&_narrow {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/dark/acf_narrow.svg);
						}
					}
					&_wide {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/dark/acf_wide.svg);
						}
					}
					&.selected {
						> span span:first-child {
							opacity: 0.5;
						}
						&.aircraft_type_heli {
							> span span:first-child {
								background-image: url(../../../sys/assets/icons/bright/acf_heli.svg);
							}
						}
						&.aircraft_type_ga {
							> span span:first-child {
								background-image: url(../../../sys/assets/icons/bright/acf_ga.svg);
							}
						}
						&.aircraft_type_turbo {
							> span span:first-child {
								background-image: url(../../../sys/assets/icons/bright/acf_turbo.svg);
							}
						}
						&.aircraft_type_jet {
							> span span:first-child {
								background-image: url(../../../sys/assets/icons/bright/acf_jet.svg);
							}
						}
						&.aircraft_type_narrow {
							> span span:first-child {
								background-image: url(../../../sys/assets/icons/bright/acf_narrow.svg);
							}
						}
						&.aircraft_type_wide {
							> span span:first-child {
								background-image: url(../../../sys/assets/icons/bright/acf_wide.svg);
							}
						}
					}
				}
				.company_filter {
					&_clearsky {
						&.selected {
							> span {
								background-color: #F47F2E;
								background-image: url(../../../sys/assets/icons/companies/bright/logo_clearsky.svg);
								opacity: 1;
							}
						}
						> span {
							background-image: url(../../../sys/assets/icons/companies/dark/logo_clearsky.svg);
							opacity: 0.5;
						}
					}
					&_coyote {
						&.selected {
							> span {
								background-color: #111;
								background-image: url(../../../sys/assets/icons/companies/bright/logo_coyote.svg);
								opacity: 1;
							}
						}
						> span {
							background-image: url(../../../sys/assets/icons/companies/dark/logo_coyote.svg);
							opacity: 0.5;
						}
					}
					&_skyparktravel {
						&.selected {
							> span {
								background-color: #FB45A1;
								background-image: url(../../../sys/assets/icons/companies/bright/logo_skyparktravel.svg);
								opacity: 1;
							}
						}
						> span {
							background-image: url(../../../sys/assets/icons/companies/dark/logo_skyparktravel.svg);
							opacity: 0.5;
						}
					}
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		.results {
			&_section {
				&.theme--dark {
					color: #FFF;
					background: linear-gradient(to bottom, rgba($ui_colors_dark_shade_0, 0) 0, rgba($ui_colors_dark_shade_0, 0.3) 40px, rgba($ui_colors_dark_shade_0, 0.2) 100%);
				}
			}
			&_header {
				.search-rgn-btn {
					&.invisible {
						background-color: $ui_colors_dark_button_go;
					}
				}
			}
			&_nav {
				&_back {
					background-image: url(../../../sys/assets/framework/bright/arrow_left.svg);
				}
				&_forward {
					background-image: url(../../../sys/assets/framework/bright/arrow_right.svg);
				}
			}
		}
		.map {
			background-color: #00000f;
			&_controls {
				&_sat {
					background-image: url(../../../sys/assets/icons/bright/sat.svg);
				}
			}
			&_marker {
				&_history {
					background-color: transparent;
					border: 3px solid rgb(255, 165, 0);
				}
				&_position {
					background-color: $ui_colors_dark_button_info;
					border: 3px solid $ui_colors_dark_shade_5;
				}
			}
		}
		.filter {
			&_entry {
				&.lined {
					&-t {
						border-top: 1px solid rgba($ui_colors_dark_shade_5, 0.2);
					}
					&-b {
						border-bottom: 1px solid rgba($ui_colors_dark_shade_5, 0.2);
					}
				}
				.aircraft_type {
					&_heli {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/bright/acf_heli.svg);
						}
					}
					&_ga {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/bright/acf_ga.svg);
						}
					}
					&_turbo {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/bright/acf_turbo.svg);
						}
					}
					&_jet {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/bright/acf_jet.svg);
						}
					}
					&_narrow {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/bright/acf_narrow.svg);
						}
					}
					&_wide {
						> span span:first-child {
							background-image: url(../../../sys/assets/icons/bright/acf_wide.svg);
						}
					}
				}
				.company_filter {
					&_clearsky {
						&.selected {
							> span {
								background-color: #F47F2E;
							}
						}
						> span {
							background-image: url(../../../sys/assets/icons/companies/bright/logo_clearsky.svg);
						}
					}
					&_coyote {
						&.selected {
							> span {
								background-color: #111;
							}
						}
						> span {
							background-image: url(../../../sys/assets/icons/companies/bright/logo_coyote.svg);
						}
					}
					&_skyparktravel {
						&.selected {
							> span {
								background-color: #FB45A1;
							}
						}
						> span {
							background-image: url(../../../sys/assets/icons/companies/bright/logo_skyparktravel.svg);
						}
					}
				}
			}
		}
	}

	.app-frame {
		align-items: stretch;
		flex-direction: row;
	}

	.filter {
		&_banner {
			position: absolute;
			top: $status-size;
			left: $edge-margin;
			right: $edge-margin;
			//border-radius: 8px;
			.width_limiter_content {
				flex-direction: row;
			}
			.button_action {
				display: block;
				@include shadowed_shallow($ui_colors_bright_shade_5);
			}
			.filter_open_btn {
				flex-grow: 1;
			}
			.filter_dice_btn {
				width: 30px;
				margin-right: $edge-margin / 2;
				background-image: url(./assets/dice.svg);
				background-repeat: no-repeat;
				background-position: center;
			}
			.filter_clear_btn {
				margin-left: $edge-margin / 2;
			}
		}
		&_section {
			padding-top: $edge-margin/2;
			.dice {
				width: 80px;
				height: 50px;
				background-image: url(./assets/dice.svg);
				background-repeat: no-repeat;
				background-position: center;
				transition: transform 0.1s ease-out;
				cursor: pointer;
				&:hover {
					transform: scale(1.1);
				}
				&-section {
					margin-top: 1em;
					margin-bottom: 1em;
					.column {
						&:first-child {
							flex-grow: 0;
							min-width: 90px;
						}
					}
				}
				&-title {
					font-family: "SkyOS-SemiBold";
				}
				&-notice {
					margin: 0;
				}
			}
		}
		&_entry {
			padding-top: $edge-margin / 2;
			padding-bottom: $edge-margin / 2;
			h3 {
				font-family: "SkyOS-SemiBold";
				font-size: 1em;
				line-height: 1.4em;
				margin: 0;
				&.margin-b {
					margin-bottom: $edge-margin / 2;
				}
			}
			.aircraft_type {
				position: relative;
				align-items: flex-end;
				overflow: hidden;
				@include shadowed_shallow($ui_colors_dark_shade_2);
				> span {
					display: block;
					flex-grow: 1;
					height: 60px;
					text-align: left;
				}
				> span span:first-child {
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
			// <!-- companies: ['offduty','clearsky','coyote','skyparktravel','lastflight','orbit','fliteline'], company_filter -->
			.company_filter {
				position: relative;
				align-items: flex-end;
				overflow: hidden;
				padding: 4px;
				padding-left: 1px;
				@include shadowed_shallow($ui_colors_dark_shade_2);
				> span {
					position: relative;
					display: block;
					flex-grow: 1;
					height: 70px;
					text-align: left;
					background-position: center;
					background-repeat: no-repeat;
					background-size: 40px;
					background-position: center 6px;
					border-radius: 4px;
					transition: background-color 0.1s ease-out;
					> span {
						display: flex;
						justify-content: center;
						align-items: flex-end;
						position: absolute;
						top: 0;
						right: 0;
						bottom: 0;
						left: 0;
						padding: 4px 8px;
						text-align: center;
					}
				}
			}

		}
		&_description {
			opacity: 1;
			transition: max-height 0.3s 0.5s ease-out, opacity 0.3s 0.5s ease-out;
			max-height: 300px;
			margin: 0;
			&.hidden {
				opacity: 0;
				max-height: 0;
				transition: max-height 0.1s 0s ease-out, opacity 0.1s 0s ease-out;
			}
		}
		&_footer {
			flex-grow: 1;
			padding: 8px;
		}
	}
	.results {
		&_section {
			position: absolute;
			padding-bottom: $nav-size;
			padding-top: 16px;
			bottom: 0;
			left: 0;
			right: 0;
			margin-bottom: 0;
			pointer-events: none;
		}
		&_header {
			flex-grow: 1;
			margin-bottom: $edge-margin / 2;
			h2 {
				font-family: "SkyOS-SemiBold";
				margin: 0;
			}
			.data-block {
				font-size: 0.9em;
				line-height: 1.4em;
				text-align: left;
				margin-left: 14px;
				span {
					display: block;
					&.value {
						font-family: "SkyOS-SemiBold";
					}
				}
			}
			.search-rgn-btn {
				transition: transform 1s cubic-bezier(0, 1, 0.15, 1), opacity 0.5s cubic-bezier(0, 1, 0.15, 1), background-color 2s 0.4s ease-out;
				&.invisible {
					transition: transform 0s, opacity 0s, background-color 0s;
				}
			}
			.button_action {
				pointer-events: all;
			}
		}
		&_list {
			& > div > div {
				pointer-events: all;
				&:last-child {
					pointer-events: none;
				}
			}
		}
		&_nav {
			display: flex;
			align-items: center;
			pointer-events: all;
			&_back {
				background-size: 18px;
				background-position: center;
				background-repeat: no-repeat;
				align-self: stretch;
			}
			&_forward {
				background-size: 18px;
				background-position: center;
				background-repeat: no-repeat;
				align-self: stretch;
			}
			.button_nav {
				margin-right: 8px;
			}
		}
	}
	.map {
		&_marker {
			&_history {
				width: 10px;
				height: 10px;
				border-radius: 50%;
			}
			&_position {
				width: 10px;
				height: 10px;
				border-radius: 50%;
			}
			&_cargo_pickup {
				width: 24px;
				height: 24px;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
				background-image: url(../../../sys/assets/icons/cargo_pickup.svg);
			}
			&_cargo_dropoff {
				width: 24px;
				height: 24px;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
				background-image: url(../../../sys/assets/icons/cargo_dropoff.svg);
			}
		}
		&_controls {
			position: absolute;
			right: 0;
			top: 8px;
			& > div {
				& > div {
					display: flex;
					align-items: center;
					background-position: center;
					background-repeat: no-repeat;
					height: 1.4em;
					width: 1.4em;
				}
			}
			&_sat {
				background-size: 1.4em;
			}
		}
		.mapboxgl-ctrl-bottom-left {
			z-index: 10;
			margin-left: 8px;
			margin-bottom: 8px;
		}
		.mapboxgl-ctrl-bottom-right {
			z-index: 10;
			margin-right: 4px;
			margin-bottom: 4px;
			opacity: 0.3;
		}
	}
	.modal {
		.results_nav {
			&_back {
				height: 1em;
			}
			&_forward {
				height: 1em;
			}
		}
	}
}
</style>