<template>
	<div class="enroute_bar" ref="root">
		<div class="enroute_scroller" :class="{ 'expanded': isexpanded, 'is-dct': legs_render ? legs_render.length == 2 : false }" ref="mainScroller" @scroll="hasScrolled" @mouseleave="isexpanded = false">
			<div class="enroute_canvas" :style="{ 'width': scrollWidth + 'px' }">
				<div class="overlayed">
					<div>
						<div
						v-for="(node, index) in legs_render"
						v-bind:key="index"
						class="node-group"
						:class="{
							'has-estimates': node.leg ? node.leg.ete > -1 : false,
							'is-next': node.leg ? node.leg.is_next : true,
							'is-hovered': node.is_hovered,
							'is-small': node.is_small }"
						:style="node.div.styleStr"
						@mouseenter="node.is_hovered = true;"
						@mouseleave="node.is_hovered = false;" >
							<div class="node-overlay" v-if="node.leg">
								<div class="node-topo-max" :style="node.div.styleAltStr" v-if="node.leg.max_alt_ft > 0">{{ (node.leg.max_alt_ft > 1000) ? (Math.round(node.leg.max_alt_ft / 500) * 500 / 1000) + 'k' : Math.round(node.leg.max_alt_ft).toLocaleString('en-gb') }}</div>
							</div>
							<div class="node-data"
							:style="node.current_data_style_string"
							:class="node.current_data_class"
							ref="node-data"
							@click="scrollMove(node.svg.offset - 200, false)">
								<div class="node-code" v-if="node.prev.start.code == 'TIMECRUIS'">T/C</div>
								<div class="node-code" v-else-if="node.prev.start.code == 'TIMEDSCNT'">T/D</div>
								<div class="node-code" v-else>{{ node.prev.start.code }}</div>
								<div class="node-estimates" v-if="node.leg ? (node.leg.progress == 0) : true">
									<div class="node-ete">ETE <duration :time="node.prev.ete" :decimals="1" :brackets="{ to_minutes : 1 }" v-if="node.prev.ete >= 0"/> <span v-else>~</span></div>
									<div class="node-dist">DIS <distance :amount="node.prev.dist_to_go" :decimals="0" v-if="node.prev.dist_to_go >= 0"/><span v-else>~</span></div>
								</div>
							</div>
						</div>
					</div>

					<div class="aircraft_location" :style="aircraft_location_style" :class="aircraft_location_class"></div>

					<svg :viewBox="'0 0 ' + scrollWidth + ' 100'" preserveAspectRatio="none">
						<g
						v-for="(node, index) in legs_render" v-bind:key="index"
						:transform="node.svg.transformStr"
						:class="{
							'is-hovered': node.is_hovered,
							'is-last': node.leg == null
						}"
						@mouseenter="node.is_hovered = true;"
						@mouseleave="node.is_hovered = false;"
						@click="isexpanded = true">
							<linearGradient class="node-topo-backdrop" :id="'gradient_bg_' + index" x2="0" y2="1" >
								<stop offset="0%" />
								<stop offset="100%" />
							</linearGradient>
							<linearGradient class="node-topo-plan" :id="'gradient_er_' + index" x2="0" y2="1" >
								<stop offset="0%" />
								<stop offset="100%" />
							</linearGradient>
							<linearGradient class="node-topo-plan" :id="'gradient_er_' + index" x2="0" y2="1" >
								<stop offset="0%" />
								<stop offset="100%" />
							</linearGradient>
							<linearGradient class="node-topo-done" :id="'gradient_er_done_' + index" x2="0" y2="1">
								<stop offset="0%" />
								<stop offset="100%" />
							</linearGradient>
							<linearGradient class="node-topo-danger" :id="'gradient_er_danger_' + index" x2="0" y2="1">
								<stop offset="0%" />
								<stop offset="100%" />
							</linearGradient>
							<rect :fill="'url(#gradient_bg_' + index + ')'" class="node-topo-backdrop" :width="node.svg.width" height="100"/>
							<polygon :fill="'url(#gradient_er_danger_' + index + ')'" :points="node.svg.topoDanger"></polygon>
							<polygon :fill="'url(#gradient_er_done_' + index + ')'" :points="node.svg.topo_done"></polygon>
							<polygon :fill="'url(#gradient_er_' + index + ')'" :points="node.svg.topo_base"></polygon>
							<path class="node-topo-limit" v-if="node.leg != null" :d="node.svg.topoLimit" vector-effect="non-scaling-stroke"></path>
						</g>
						<linearGradient class="node-topo-aircraft-plot" :id="'gradient_ac_plot'" :x1="state.aircraftPlotData.x2 > state.aircraftPlotData.x1 ? 0 : 1" :x2="state.aircraftPlotData.x2 > state.aircraftPlotData.x1 ? 1 : 0">
							<stop offset="0%" />
							<stop offset="100%" />
						</linearGradient>
						<line class="node-topo-aircraft-plot" :x1="state.aircraftPlotData.x1" :y1="state.aircraftPlotData.y1" :x2="state.aircraftPlotData.x2" :y2="state.aircraftPlotData.y2" :stroke="'url(#gradient_ac_plot)'" vector-effect="non-scaling-stroke"/>
					</svg>

				</div>
			</div>
			<div class="enroute_nodes" ref="mainList">
				<div
				class="enroute_node waypoint"
				v-for="(node, index) in legs_render"
				v-bind:key="index"
				:class="[
				'type-' + (node.leg ? node.leg.start.type : 'end'),
				'airway-index-' + (node.leg ? node.leg.start.airway_index : 'end'),
				{
					'is-current': (node.leg ? node.leg.is_current : false),
					'is-last': node.leg == null,
					'has-airway': (node.leg ? node.leg.start.airway != '' : false),
					'is-first-airway': (node.leg ? node.leg.start.airway_first : false),
					'is-last-airway': (node.leg ? node.leg.start.airway_last : false)
				}]"
				>
					<div class="node-track">
						<div class="node-progress" v-if="node.leg" :style="{ 'width': node.leg.progress + '%' }"></div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import Flightplan, { Waypoint } from '@/sys/classes/flight_plans/plan';
import { NavLeg } from '@/sys/services/extensions/navigation';

export default Vue.extend({
	props: {
		plan :Flightplan,
		contract :Contract
	},
	data() {
		return {
			isReady: false,
			track_init: false,
			isTracking: true,
			trackingLocation: 0,
			isTrackingTimeout: null,
			nodesProcessInterval: null,
			isCompact: false,
			isexpanded: false,
			scrollWidth: 0,
			scroll_offset: 0,
			frameWidth: 0,
			mainScroller: {
				el: null,
				scrollInterval: null,
			},
			mainList: null,
			nav_data:{
				legs: null as NavLeg[],
				contract: null as Contract,
				plan: null as Flightplan,
			},
			aircraft_location_style: '',
			aircraft_location_class: '',
			legs_render: [] as {
				leg: NavLeg,
				is_small: boolean,
				is_hovered: boolean,
				current_data_style_string: string,
				current_data_class: string,
				prev: NavLeg,
				div: {
					styleStr: string,
					styleAltStr: string,
					aircraft: string,
				},
				svg: {
					offset: number,
					width: number,
					transformStr: string,
					topo_base: string,
					topo_done: string,
					topoDanger: string,
					topoLimit: string,
				},
			}[],
			state: {
				topo_max: 2000,
				topo_y_ratio: 1,
				topoXRatio: 5,
				aircraftPlotData: {
					x1: 0,
					x2: 100,
					y1: 50,
					y2: 50
				},

			}
		}
	},
	methods: {

		init() {
			this.state = {
				topo_max: 2000,
				topo_y_ratio: 1,
				topoXRatio: 5,
				aircraftPlotData: {
					x1: 0,
					x2: 100,
					y1: 50,
					y2: 50
				}
			}

			this.nav_data.legs = this.$os.navigation.legs;
			this.nav_data.contract = this.$os.navigation.contract;
			this.nav_data.plan = this.$os.navigation.plan;

			this.isReady = true;
			this.build();
		},

		build() {

			/*
			console.log(JSON.stringify(this.nav_data.legs.map(x => {
				if(x) {
					console.log(x);
					return {
						index: x.index,
						is_next: x.is_next,
						is_current: x.is_current,
						bearing_to: x.bearing_to,
						bearing_from: x.bearing_from,
						bearing_dif: x.bearing_dif,
						dist_direct: x.dist_direct,
						dist_total_at_start: x.dist_total_at_start,
						dist_to_next: x.dist_to_next,
						dist_to_go: x.dist_to_go,
						ete: x.ete,
						topo: x.topo,
						max_alt: x.max_alt,
						max_alt_ft: x.max_alt_ft,
						max_alt_offset: x.max_alt_offset,
						is_last: x.is_last,
						is_range: x.is_range,
						progress: x.progress,
						wp: x.start,

						dist_to_leg: x.dist_to_leg,
						dist_ahead_to_leg: x.dist_ahead_to_leg,
						closest_on_leg: x.closest_on_leg,
						track_offset: x.track_offset,
						distance: x.distance,
						next: x.next ? x.next.index : null,
						prev: x.prev ? x.prev.index : null,
					}
				} else {
					return null;
				}
			})));
			*/

			this.legs_render = [];
			if(this.nav_data.legs) {
				this.nav_data.legs.forEach((leg, index) => {
					if(index > 0) {
						this.legs_render.push({
							leg: leg,
							is_small: false,
							is_hovered: false,
							current_data_style_string: '',
							current_data_class: '',
							prev: leg.prev,
							div: {
								styleStr: '',
								styleAltStr: '',
								aircraft: '',
							},
							svg: {
								offset: 0,
								width: 0,
								transformStr: '',
								topo_base: '',
								topo_done: '',
								topoDanger: '',
								topoLimit: '',
							}
						})
					}
				});

				this.legs_render.push({
					leg: null,
					is_small: false,
					is_hovered: false,
					current_data_style_string: '',
					current_data_class: '',
					prev: this.nav_data.legs[this.nav_data.legs.length - 1],
					div: {
						styleStr: '',
						styleAltStr: '',
						aircraft: '',
					},
					svg: {
						offset: 0,
						width: 0,
						transformStr: '',
						topo_base: '',
						topo_done: '',
						topoDanger: '',
						topoLimit: '',
					}
				})

				this.update();
				this.resize();
			}
		},

		// Process the nodes and make sure the data is up to date
		update() {
			this.$nextTick(() => {
				if(this.isReady && this.legs_render.length) {

					const node_current = this.legs_render.find(x => x.leg ? x.leg.is_current : false);
					//const node_current_index = this.legs_render.indexOf(node_current);
					//const node_prev = this.legs_render[node_current_index - 1];

					if(node_current) {
						this.trackingLocation = node_current.svg.offset + (node_current.svg.width * (node_current.leg.progress / 100)) - 100;

						this.create_topo();

						if(this.isTracking) {
							this.scrollMove(this.trackingLocation, this.track_init);
						}
					} else {
						this.trackingLocation = 0;
					}

					this.update_aircraft_plot();
					this.track_init = true;
				}
			});
		},

		// Calculate and resize everything
		resize() {
			this.$nextTick(() => {
				if(this.mainList && this.legs_render.length){

					this.scrollWidth = 0;
					this.frameWidth = this.mainScroller.el.scrollWidth;
					let offset = 0;

					this.mainList.childNodes.forEach((el :HTMLElement, nodeIndex: number) => {
						if(el.nodeName == 'DIV'){
							if(el.classList.contains('enroute_node')){

								const rn = this.legs_render[nodeIndex];

								//if(rn.leg) {
								//	rn.leg.is_last = nodeIndex == this.legs_render.length - 1;
								//}

								if(this.legs_render.length > 2) {

									rn.svg.offset = offset;
									rn.svg.transformStr = 'translate(' + rn.svg.offset + ' 0)';

									if(rn.leg) {
										const divWidth = Math.round(rn.leg.dist_to_next * this.state.topoXRatio);
										offset += divWidth;
										el.style.minWidth = divWidth + 'px';
										this.scrollWidth += divWidth;
									} else {
										el.style.minWidth = null;
										this.scrollWidth += el.scrollWidth;
									}

								} else if(this.legs_render.length == 2) {

									if(rn.leg) {
										const w = Math.round((this.$refs.root as HTMLElement).offsetWidth - 150);
										this.state.topoXRatio = w / rn.leg.dist_to_next;
										this.scrollWidth += w;
										el.style.minWidth = w + 'px';
									} else {
										el.style.minWidth = null;
										this.scrollWidth += 150;
									}

									if(rn.leg) {
										let cumulDist = 0;
										rn.svg.offset = Math.round((cumulDist * this.state.topoXRatio));
										rn.svg.transformStr = 'translate(' + rn.svg.offset + ' 0)';
									} else {
										rn.svg.offset = Math.round(((rn.prev.dist_total_at_start + rn.prev.dist_to_next) * this.state.topoXRatio));
										rn.svg.transformStr = 'translate(' + rn.svg.offset + ' 0)';
									}

								}

								// Set SVG width
								if(rn.leg) {
									const newWidth = (rn.leg.dist_to_next * this.state.topoXRatio) - 1;
									rn.svg.width = newWidth > 0 ? Math.round(newWidth) : 0;
								} else {
									rn.svg.width = Math.round(this.scrollWidth - rn.svg.offset);
								}

								rn.div.styleStr = 'left: ' + rn.svg.offset + 'px; width: ' + rn.svg.width + 'px';
							}

						}
					});

					this.create_topo();
					this.update_aircraft_plot();
					this.update_scroll();

					//const node_current = this.legs_render.find(x => x.leg.is_current);
					//const node_prev_index = node_current.leg.prev.index;

					//if(this.isTracking && node_prev_index > -1) {
						//this.scrollTo(node_prev_index);
					//}

				}
			});
		},

		// Create the svg and bar items
		create_topo() {

			// Find Y Scale and max topo
			this.state.topo_max = (this.$os.simulator.location.Alt * 0.3048) + 1000;
			this.legs_render.forEach((render_node, index) => {
				if(render_node.leg) {
					if(render_node.leg.topo) {
						render_node.leg.topo.forEach((topo :any) => {
							if(topo + 500 > this.state.topo_max) {
								this.state.topo_max = topo + 500;
							}
						});
					}
				}
			});
			this.state.topo_y_ratio = 1 / this.state.topo_max;

			this.legs_render.forEach((render_node, index) => {
				let point_list_static = [] as string[];
				let point_list_done = [] as string[];

				if(render_node.leg) {
					if(render_node.leg.topo ? render_node.leg.topo.length : false) {
						point_list_static.push("0, 100.001");
						point_list_done.push("0, 100.001");

						// Find max elevation
						const total_topo_length = render_node.leg.topo.length - 1;
						const progress_cutoff_index = (total_topo_length / 100) * render_node.leg.progress;

						const static_data = { reached: false, x: 0, y: 0 }
						const done_data = { reached: false, x: 0, y: 0 }

						// Create topo geometry
						render_node.leg.topo.forEach((topo :any, index :number) => {

							static_data.x = done_data.x = Math.round(index > 0 ? (index / total_topo_length) * render_node.svg.width : 0);
							static_data.y = done_data.y = 100 - ((topo * this.state.topo_y_ratio) * 100);

							if(render_node.leg.max_alt < topo) {
								render_node.leg.max_alt = topo;
								render_node.leg.max_alt_ft = topo * 3.28084;
								render_node.leg.max_alt_offset = static_data.x;
							}


							if(index >= Math.floor(progress_cutoff_index)) { // If we've passed the current progress
								if(!done_data.reached){
									done_data.reached = true;


									if(progress_cutoff_index != index) {

										const current_y = topo;
										const next_y = render_node.leg.topo[index + 1];
										const diff_y = next_y - current_y;
										const dec = progress_cutoff_index - index;

										done_data.x = (index > 0 ? (progress_cutoff_index / total_topo_length) * render_node.svg.width : 0);
										done_data.y = 100 - (((topo + (diff_y * dec)) * this.state.topo_y_ratio) * 100);

										static_data.x = done_data.x = index > 0 ? (progress_cutoff_index / total_topo_length) * render_node.svg.width : 0;

									}

									point_list_done.push(done_data.x + "," + done_data.y);

									point_list_static.push(static_data.x + "," + 100);
									point_list_static.push(static_data.x + "," + static_data.y);
								}
								done_data.y = 100;
							} else { // If we're still in the progressed zone
								static_data.y = 100;
							}

							point_list_done.push(done_data.x + "," + done_data.y);
							point_list_static.push(static_data.x + "," + static_data.y);
						});
					}

					// Join poly
					point_list_done.push(render_node.svg.width + ", 0");
					point_list_done.push(render_node.svg.width + ", 100.001");
					point_list_static.push(render_node.svg.width + ", 0");
					point_list_static.push(render_node.svg.width + ", 100.001");
					render_node.svg.topo_base = point_list_static.join(' ');
					render_node.svg.topo_done = point_list_done.join(' ');

					//Altitude Limit
					let slt_limit_pct = (this.state.topo_y_ratio * render_node.leg.max_alt) * 100;
					render_node.svg.topoLimit = 'M0 ' + (100 - slt_limit_pct) + ' l' + render_node.svg.width + ' 0';

					// Set div counterpart
					render_node.div.styleAltStr = 'left: ' + render_node.leg.max_alt_offset + 'px; bottom: ' + slt_limit_pct + '%';
				}
			});

		},

		// Process Scroll
		update_scroll() {
			const legs_render = this.legs_render;
			const right_padding = 0;
			let found_next = false;

			legs_render.forEach((leg_render, index) => {
				const ref_el = this.$refs['node-data'][index];
				if(ref_el) {
					const px_to_end = (leg_render.svg.offset + leg_render.svg.width - this.scroll_offset) - ref_el.clientWidth;

					leg_render.is_small = leg_render.svg.width - ref_el.clientWidth < 0;
					if(!leg_render.is_small){
						if(px_to_end > 0){
							if(leg_render.svg.offset - this.scroll_offset >= -3) {
								const offset_to_end = leg_render.svg.offset - this.scroll_offset - this.frameWidth + ref_el.clientWidth + right_padding;
								if(offset_to_end > 0 && !found_next && index > 0) {
									found_next = true;
									const prev_leg = legs_render[index - 1];
									const prev_ref_el = this.$refs['node-data'][index - 1];
									const offset = (this.scroll_offset + this.frameWidth - leg_render.svg.offset - ref_el.clientWidth);
									const prev_contact = -(prev_leg.svg.width + offset - prev_ref_el.clientWidth);
									if(prev_contact > 0) {
										leg_render.current_data_class = "is-right";
										leg_render.current_data_style_string = 'transform: translateX(' + (offset + prev_contact) + 'px)'; // Cap to the screen right
									} else {
										leg_render.current_data_class = "is-right";
										leg_render.current_data_style_string = 'transform: translateX(' + offset + 'px)'; // Cap to the screen right
									}
								} else {
									leg_render.current_data_class = "";
									leg_render.current_data_style_string = 'transform: translateX(' + 0 + 'px)'; // Cap to the start
								}
							} else {
								leg_render.current_data_class = "is-left";
								leg_render.current_data_style_string = 'transform: translateX(' + (this.scroll_offset - leg_render.svg.offset) + 'px)'; // Follow scroll
							}
						} else if(index > 0) {
							leg_render.current_data_class = "is-left";
							leg_render.current_data_style_string = 'transform: translateX(' + (leg_render.svg.width - ref_el.offsetWidth) + 'px)'; // Cap to the end
						}
					} else {
						leg_render.current_data_class = "";
						leg_render.current_data_style_string = 'transform: translateX(0px)';
					}
				}
			});

		},

		// Reset Tracking Scroll
		reset_scroll() {
			if(this.isTrackingTimeout) {
				this.isTracking = false;
				clearTimeout(this.isTrackingTimeout);
			}
			this.isTrackingTimeout = setTimeout(() => {
				this.isTracking = true;
				this.scrollMove(this.trackingLocation, false);
			}, 10000);
		},

		// Update the aircraft plot
		update_aircraft_plot() {

			const node_current = this.legs_render.find(x => x.leg ? x.leg.is_current : false);
			//const node_current_index = this.legs_render.indexOf(node_current);
			//const node_prev = this.legs_render[node_current_index - 1];

			//if(node_prev) {
				const minutes = 5;
				const xOffset = ((this.state.topoXRatio * (this.$os.simulator.location.GS / 60))) * minutes;
				const yOffset = ((this.state.topo_y_ratio * this.$os.simulator.location.FPM * 0.3048 * 60)) * minutes;

				let aircraftYPos = (1 / (this.state.topo_max * 3.28084) * this.$os.simulator.location.Alt) * 100;
				aircraftYPos = (100 - (aircraftYPos > 100 ? 100 : aircraftYPos));

				let aircraftXPos = node_current.svg.offset + (node_current.leg.progress > 0 ? node_current.svg.width * (node_current.leg.progress / 100) : 0);

				const bearingDif = Math.abs(Eljs.MapCompareBearings(node_current.leg.bearing_to, this.$os.simulator.location.CRS));

				let crsCos = Math.cos(bearingDif * Math.PI / 180 );

				this.state.aircraftPlotData.x1 = aircraftXPos;
				this.state.aircraftPlotData.y1 = aircraftYPos;
				this.state.aircraftPlotData.x2 = aircraftXPos + (xOffset * crsCos);
				this.state.aircraftPlotData.y2 = aircraftYPos - yOffset == aircraftYPos ? aircraftYPos + 0.0001 : aircraftYPos - yOffset;

				// Position aircraft div
				this.aircraft_location_class = this.state.aircraftPlotData.x1 > this.state.aircraftPlotData.x2 ? 'rev' : '';
				this.aircraft_location_style = 'top: ' + aircraftYPos + '%;left: ' + aircraftXPos + 'px;';
			//}
		},

		window_resized() {
			if(!this.isCompact) {
				this.update_scroll();
				this.resize();
				this.update();
			}
		},

		hasScrolled(ev :any) {
			this.scroll_offset = ev.target.scrollLeft;
			this.frameWidth = this.mainScroller.el.offsetWidth;
			this.update_scroll();
		},
		scrollMove(xOffset: number, instant :boolean) {
			clearInterval(this.mainScroller.scrollInterval);

			const diff = Math.abs(this.mainScroller.el.scrollLeft - xOffset);

			if(!instant && diff > 10) {
				let prevOffset = 0;
				this.mainScroller.scrollInterval = setInterval(() => {
					if(prevOffset == this.mainScroller.el.scrollLeft) {
						clearInterval(this.mainScroller.scrollInterval);
					}
					prevOffset = this.mainScroller.el.scrollLeft;
					let offsetDif = -(this.mainScroller.el.scrollLeft - xOffset);
					if(Math.abs(xOffset - this.mainScroller.el.scrollLeft) > 1){
						this.mainScroller.el.scrollLeft += Math.ceil(offsetDif / 8);
					} else {
						this.mainScroller.el.scrollLeft = xOffset;
						clearInterval(this.mainScroller.scrollInterval);
					}
				}, 8);
			} else {
				this.mainScroller.el.scrollLeft = xOffset;
			}
		},
		//scrollTo(index :number, instant :boolean) {
		//	let xOffset = 0;
		//	if(this.mainScroller.el){
		//		let padding = parseInt(window.getComputedStyle(this.mainScroller.el).getPropertyValue('padding-left').replace('px', ''));
		//		this.mainScroller.el.childNodes.forEach((el :HTMLElement, i: number) => {
		//			if(index >= i && el.nodeName == 'DIV'){
		//				if(el.classList.contains('enroute_node')){
		//					xOffset = el.offsetLeft - padding;
		//				}
		//			}
		//		});
		//	}
		//	this.scrollMove(xOffset, instant);
		//},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					break;
				}
				case 'disconnect': {
					break;
				}
				case 'adventure': {
					break;
				}
				case 'eventbus': {
					//if(this.isReady) {
					//	switch(wsmsg.name[1]){
					//		case 'meta': {
					//			//this.update();
					//			break;
					//		}
					//	}
					//}
					break;
				}
			}
		},
		listener_navigation(wsmsg :any) {
			switch(wsmsg.name){
				case 'data': {

					if(this.nav_data.contract != wsmsg.payload.contract) {
						this.isReady = false;
						this.nav_data.contract = wsmsg.payload.contract;
						this.init();
						this.build();
					}

					if(this.nav_data.plan != wsmsg.payload.plan) {
						this.isReady = false;
						this.nav_data.plan = wsmsg.payload.plan;
						this.init();
						this.build();
					}

					if(this.nav_data.legs != wsmsg.payload.nodes) {
						this.isReady = false;
						this.nav_data.legs = wsmsg.payload.nodes;
						this.init();
						this.build();
					}

					this.isReady = true;
					this.update();
					break;
				}
			}
		},

	},
	mounted() {
		this.mainList = (this.$refs['mainList'] as any);
		this.mainScroller.el = (this.$refs['mainScroller'] as any);

		['touchstart', 'wheel', 'mousedown'].forEach((ev) => {
			this.mainScroller.el.addEventListener(ev, () => {
				this.reset_scroll();
				if(this.mainScroller.scrollInterval){
					clearInterval(this.mainScroller.scrollInterval);
				}
			});
		});
		['wheel'].forEach((ev) => {
			this.mainScroller.el.addEventListener(ev, (ev1 :any) => {
				this.mainScroller.el.scrollLeft += ev1.deltaY;
				ev1.preventDefault();
			});
		});

		this.init();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('ws-in', this.listenerWs);
		this.$os.eventsBus.Bus.on('navigation', this.listener_navigation);

		window.addEventListener("resize", this.window_resized);
	},
	beforeDestroy() {
		clearInterval(this.nodesProcessInterval);
		clearInterval(this.mainScroller.scrollInterval);

		this.$os.eventsBus.Bus.off('ws-in', this.listenerWs);
		this.$os.eventsBus.Bus.off('navigation', this.listener_navigation);

		window.removeEventListener("resize", this.window_resized);
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.enroute_bar {
	border-radius: 12px;
	overflow: hidden;

	.theme--bright & {
		background: rgba($ui_colors_bright_shade_1, 0.5);
		&:after {
			border-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		.aircraft_location {
			background-image: url(../assets/dark/aircraft.svg);
		}
	}

	.theme--dark & {
		background: rgba($ui_colors_dark_shade_1, 0.5);
		&:after {
			border-color: rgba($ui_colors_dark_shade_4, 0.2);
		}
		.aircraft_location {
			background-image: url(../assets/bright/aircraft.svg);
		}
	}

	&:after {
		content: '';
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		border-radius: inherit;
		border: 1px solid transparent;
		pointer-events: none;
		z-index: 2;
	}

	.enroute_scroller {
		position: relative;
		overflow-x: scroll;
		pointer-events: none;
		scrollbar-width: none;
		transition: background .5s $transition;
		//backdrop-filter: blur(10px);
		//backdrop-filter: blur(5px);

		.theme--bright & {
			color: $ui_colors_bright_shade_0;
			&.expanded {
				background: $ui_colors_bright_shade_1;
				.enroute_canvas {
					g {
						.node-topo-limit {
							stroke: $ui_colors_bright_button_cancel;
						}
					}
				}
			}
			.enroute_canvas {
				.node-group {
					&.is-next,
					&.is-hovered {
						&.is-small {
							.node-data {
								background: $ui_colors_bright_shade_1;
								@include shadowed_shallow($ui_colors_dark_shade_0);
							}
						}
					}
				}
				.node-topo-aircraft-plot {
					//stroke: $ui_colors_bright_shade_5;
					stop:nth-child(1) {
						stop-color: rgba($ui_colors_bright_shade_5, 1);
					}
					stop:nth-child(2) {
						stop-color: rgba($ui_colors_bright_shade_5, 0);
					}
				}
				g {
					&.is-hovered {
						.node-topo-backdrop {
							stop:nth-child(1) {
								stop-color: $ui_colors_bright_shade_2;
							}
							stop:nth-child(2) {
								stop-color: $ui_colors_bright_shade_2;
							}
						}
						.node-topo-limit {
							opacity: 1;
						}
					}
					.node-topo-backdrop {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_bright_shade_2, 0);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_bright_shade_2, 0);
						}
					}
					.node-topo-plan {
						stop:nth-child(1) {
							stop-color: $ui_colors_bright_shade_4;
						}
						stop:nth-child(2) {
							stop-color: $ui_colors_bright_shade_4;
						}
					}
					.node-topo-done {
						stop:nth-child(1) {
							stop-color: $ui_colors_bright_magenta;
						}
						stop:nth-child(2) {
							stop-color: $ui_colors_bright_magenta;
						}
					}
				}
			}
			.enroute_node {
				&.is-last {
					.node-track {
						background: linear-gradient(to right, rgba($ui_colors_bright_shade_4, 1), cubic-bezier(.2,0,.4,1), rgba($ui_colors_bright_shade_4, 0.3));
					}
				}
				.node-track {
					background: $ui_colors_bright_shade_4;
					.node-progress {
						background: $ui_colors_bright_magenta;
					}
				}
				.node-estimates {
					text-shadow: 0 1px 3px $ui_colors_bright_shade_0;
				}
			}
		}

		.theme--dark & {
			color: $ui_colors_dark_shade_0;
			&.expanded {
				background: $ui_colors_dark_shade_1;
				g {
					.node-topo-limit {
						stroke: $ui_colors_dark_button_cancel;
					}
				}
			}
			.enroute_canvas {
				.node-group {
					&.is-next,
					&.is-hovered {
						&.is-small {
							.node-data {
								background: $ui_colors_dark_shade_1;
							}
						}
					}
					//&.is-next {
					//	.node-data {
					//		.node-estimates {
					//			color: $ui_colors_dark_magenta;
					//		}
					//	}
					//}
				}
				.node-topo-aircraft-plot {
					//stroke: $ui_colors_dark_shade_5;
					stop:nth-child(1) {
						stop-color: rgba($ui_colors_dark_shade_5, 1);
					}
					stop:nth-child(2) {
						stop-color: rgba($ui_colors_dark_shade_5, 0);
					}
				}
				g {
					&.is-hovered {
						.node-topo-backdrop {
							stop:nth-child(1) {
								stop-color: $ui_colors_dark_shade_1;
							}
							stop:nth-child(2) {
								stop-color: $ui_colors_dark_shade_1;
							}
						}
						.node-topo-limit {
							opacity: 1;
						}
					}
					.node-topo-backdrop {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_dark_shade_4, 0);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_dark_shade_4, 0);
						}
					}
					.node-topo-plan {
						stop:nth-child(1) {
							stop-color: $ui_colors_dark_shade_3;
						}
						stop:nth-child(2) {
							stop-color: $ui_colors_dark_shade_3;
						}
					}
					.node-topo-done {
						stop:nth-child(1) {
							stop-color: $ui_colors_dark_magenta;
						}
						stop:nth-child(2) {
							stop-color: $ui_colors_dark_magenta;
						}
					}
				}
			}
			.enroute_node {
				&.is-last {
					.node-track {
						background: linear-gradient(to right, rgba($ui_colors_dark_shade_3, 1), cubic-bezier(.2,0,.4,1), rgba($ui_colors_dark_shade_3, 0.3));
					}
				}
				.node-track {
					background: $ui_colors_dark_shade_3;
					.node-progress {
						background: $ui_colors_dark_magenta;
					}
				}
			}
		}

		.theme--sat.theme--bright & {
			.enroute_canvas {
				g {
					filter:
						drop-shadow(0 -1px 0 rgba($ui_colors_bright_shade_0, 1));
				}
			}
		}
		.theme--sat.theme--dark & {
			.enroute_canvas {
				g {
					filter:
						drop-shadow(0 -1px 0 rgba($ui_colors_dark_shade_0, 1));
				}
			}
		}

		&::-webkit-scrollbar {
			display: none;
		}

		&.expanded {
			pointer-events: all;
			cursor: pointer;
			.enroute_canvas {
				> .overlayed > svg {
					height: 300px;
				}
				.node-group {
					.node-overlay {
						.node-topo-max {
							opacity: 0.5;
						}
					}
				}
			}
			.enroute_node {
				.node-topo {
					&-altitude-limit {
						opacity: 1;
					}
					&-aircraft {
						&-plot {
							opacity: 1;
						}
					}
				}
			}
		}

		&.is-dct {
			.enroute_node {
				&:first-child {
					flex-grow: 1;
				}
			}
			&::after {
				min-width: $edge-margin;
			}
		}

		.enroute_canvas {
			position: relative;
			flex-grow: 1;
			z-index: 2;
			&.is-hovered {
				.node-group {
					&.is-hovered {
						.node-overlay {
							.node-topo-max {
								opacity: 1;
							}
						}
					}
				}
			}
			.overlayed {
				position: relative;
				pointer-events: all;
				cursor: pointer;
				> div {
					position: absolute;
					left: 0;
					top: 0;
					bottom: 0;
					right: 0;
					pointer-events: none;
				}
				> svg {
					display: block;
					height: 80px;
					width: 100%;
					flex-grow: 1;
					transition: height .5s $transition;
					g {
						border-radius: 10px;
						pointer-events: all;
					}
				}
			}
			.node-group {
				position: absolute;
				top: 0;
				bottom: 0;
				margin-bottom: -40px;
				box-sizing: border-box;
				overflow: visible;
				&.is-next {
					.node-data {
						z-index: 1;
						.node-estimates {
							opacity: 1;
							transform: translateY(0);
						}
					}
					&.is-small {
						.node-data {
							opacity: 1;
							mask-image: none;
							right: auto;
							left: auto;
						}
					}
				}
				&.is-last {
					.node-data {
						.node-estimates {
							opacity: 1;
							transform: translateY(0);
						}
					}
				}
				&.is-hovered {
					&.is-small {
						.node-data {
							opacity: 1;
							mask-image: none;
							right: auto;
							left: auto;
							z-index: 5;
						}
					}
					.node-data {
						.node-estimates {
							opacity: 1;
							transform: translateY(0);
						}
					}
				}
				&.is-small {
					.node-data {
						pointer-events: none;
						border-radius: 4px;
						overflow: hidden;
						position: absolute;
						right: -5px;
						left: 0;
						white-space: nowrap;
						mask-image: linear-gradient(to right, rgba(0, 0, 0, 0.5) calc(100% - 30px), rgba(0, 0, 0, 0) calc(100% - 5px));
						&.is-right,
						&.is-left {
							position: relative;
							left: auto;
							right: auto;
							mask-image: none;
							::before,
							::after {
								display: block;
							}
						}
						.node-code {
							::before,
							::after {
								display: none;
							}
						}
					}
				}
				.node-overlay {
					position: absolute;
					top: 0;
					right: 0;
					bottom: 40px;
					left: 0;
					.node-topo-max {
						position: absolute;
						font-size: 0.75em;
						font-family: "SkyOS-SemiBold";
						opacity: 0;
						transform: translateX(-100%);
						transition: opacity .2s $transition;
					}
				}
				.node-data {
					position: absolute;
					display: flex;
					flex-direction: column;
					justify-content: flex-start;
					align-items: flex-start;
					height: 35px;
					bottom: 0;
					padding: 0 8px;
					display: flex;
					pointer-events: all;
					transition: opacity 0.3s $transition;
					&.is-right {
						.node-code {
							padding: 0 8px;
							&:after {
								opacity: 1;
							}
						}
						.node-estimates {
							padding: 0 8px;
						}
					}
					&.is-left {
						.node-code {
							padding: 0 8px;
							&:before {
								content: '❮ ';
							}
						}
						.node-estimates {
							padding: 0 8px;
						}
					}
					.node-code {
						font-family: "SkyOS-SemiBold";
						transition: padding .2s $transition;
						&:after {
							content: ' ❯';
							opacity: 0;
						}
					}
					.node-estimates {
						display: flex;
						flex-direction: column;
						justify-content: flex-end;
						//opacity: 0;
						white-space: nowrap;
						transition: opacity .2s $transition, transform .2s $transition, padding .2s $transition;
						.node-dist,
						.node-ete {
							font-size: 12px;
							margin-right: 4px;
							z-index: 2;
							span {
								font-family: "SkyOS-SemiBold";
							}
						}
					}
				}
			}
			.node-topo-limit {
				opacity: 0.5;
				stroke-width: 1px;
				stroke-dasharray: 2 4;
				transition: opacity .2s $transition;
			}
			.node-topo-aircraft-plot {
				opacity: 1;
				stroke-width: 2px;
				transition: all 0.4s ease-out;
			}
			stop {
				transition: stop-color .2s $transition;
			}
		}

		.enroute_nodes {
			position: relative;
			display: flex;
			align-items: stretch;
			flex-direction: row;
			pointer-events: all;
			.enroute_node {
				position: relative;
				display: flex;
				flex-direction: column;
				justify-content: flex-end;
				min-width: 0px;
				box-sizing: border-box;
				padding-right: 1px;
				&.is-current {
					.node-topo {
						&-aircraft {
							display: block;
						}
					}
				}
				&.is-last {
					min-width: 150px;
					padding-right: 0;
				}
				.node-track {
					position: relative;
					height: 4px;
					padding-bottom: 70px;
					height: 0;
					width: 100%;
					overflow: hidden;
					.node-progress {
						position: absolute;
						top: 0;
						left: 0;
						right: 0;
						bottom: 0;
						width: 0%;
					}
				}
			}
		}
	}

	.aircraft_location {
		position: absolute;
		width: 24px;
		height: 13px;
		margin-left: -23px;
		margin-top: -12px;
		background-position: right bottom;
		background-size: contain;
		background-repeat: no-repeat;
		&.rev {
			transform: scale(-1,1);
			margin-left: -1px;
			margin-top: -12px;
		}
	}

}
</style>