<template>
	<div class="enroute_bar" ref="root">
		<div class="enroute_scroller" :class="{ 'expanded': ui.isexpanded, 'is-dct': state.planNodes.length == 2 }" ref="mainScroller" @scroll="hasScrolled" @mouseleave="ui.isexpanded = false">
			<div class="enroute_canvas"
				:class="{ 'is-hovered': ui.isHovered }"
				:style="{ 'width': ui.scrollWidth + 'px' }">
				<div class="enroute_background"></div>
				<div class="overlayed">
					<div>
						<div
						:class="{
							'has-estimates': node.ete.length,
							'is-hovered': node.isHovered,
							'is-last': node.isLast,
							'is-next': node == state.nextNode,
							'is-small': node.isSmall }"
						class="node-group"
						v-for="(node, index) in state.planNodes"
						v-bind:key="index"
						:style="node.div.styleStr"
						@mouseenter="node.isHovered = true; ui.isHovered = true;"
						@mouseleave="node.isHovered = false; ui.isHovered = false;" >
							<div class="node-overlay">
								<div class="node-topo-max" :style="node.div.styleAltStr" v-if="node.maxAltFt > 0">{{ (node.maxAltFt > 1000) ? (Math.round(node.maxAltFt / 500) * 500 / 1000) + 'k' : Math.round(node.maxAltFt).toLocaleString('en-gb') }}</div>
							</div>
							<div class="node-data"
							:style="node.currentDataStyleStr"
							:class="node.currentDataClass"
							ref="node-data"
							@click="scrollMove(node.svg.offset - 200)">
								<div class="node-code" v-if="node.wp.Code == 'TIMECRUIS'">T/C</div>
								<div class="node-code" v-else-if="node.wp.Code == 'TIMEDSCNT'">T/D</div>
								<div class="node-code" v-else>{{ node.wp.Code }}</div>
								<div class="node-estimates">
									<div class="node-ete">ETE <span>{{ node.ete.length ? node.ete : '~' }}</span></div>
									<div class="node-dist">DTG <span>{{ node.distTGO ? Math.round(node.distTGO).toLocaleString('en-gb') + ' nm' : '~' }}</span></div>
								</div>
							</div>
						</div>
					</div>
					<svg :viewBox="'0 0 ' + ui.scrollWidth + ' 100'" preserveAspectRatio="none">
						<g
						v-for="(node, index) in state.planNodes" v-bind:key="index"
						:transform="node.svg.transformStr"
						:class="{ 'is-hovered': node.isHovered, 'is-last': node.isLast }"
						@mouseenter="node.isHovered = true; ui.isHovered = true;"
						@mouseleave="node.isHovered = false; ui.isHovered = false;"
						@click="ui.isexpanded = true">
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
							<polygon :fill="'url(#gradient_er_done_' + index + ')'" :points="node.svg.topoDone"></polygon>
							<polygon :fill="'url(#gradient_er_' + index + ')'" :points="node.svg.topoBase"></polygon>
							<path class="node-topo-limit" v-if="!node.isLast" :d="node.svg.topoLimit" vector-effect="non-scaling-stroke"></path>
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
				v-for="(node, index) in state.planNodes"
				v-bind:key="index"
				:class="[
				'type-' + node.wp.Type,
				'airway-index-' + node.wp.AirwayIndex,
				{
					'is-current': node == state.currentNode,
					'has-airway': node.wp.Airway != '',
					'is-last': node.index == state.planNodes.length - 1,
					'is-first-airway': node.wp.AirwayFirst,
					'is-last-airway': node.wp.AirwayLast
				}]"
				>
					<div class="node-track">
						<div class="node-progress" :style="{ 'width': node.progress + '%' }"></div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from './../../../../sys/libraries/elem';

export default Vue.extend({
	name: "enroute_bar",
	props: ['selected'],
	mounted() {
		this.ui.mainList = (this.$refs['mainList'] as any);
		this.ui.mainScroller.el = (this.$refs['mainScroller'] as any);

		['touchstart', 'wheel', 'mousedown'].forEach((ev) => {
			this.ui.mainScroller.el.addEventListener(ev, () => {
				this.resetTracking();
				if(this.ui.mainScroller.scrollInterval){
					clearInterval(this.ui.mainScroller.scrollInterval);
				}
			});
		});
		['wheel'].forEach((ev) => {
			this.ui.mainScroller.el.addEventListener(ev, (ev1 :any) => {
				this.ui.mainScroller.el.scrollLeft += ev1.deltaY;
				event.preventDefault();
			});
		});

		window.addEventListener("resize", this.nodesCalculateWidth);

		//this.ui.nodesProcessInterval = setInterval(this.nodesProcess, 5000);
		setTimeout(() => {
			this.ui.isReady = true;
			this.nodesProcess();

			this.$nextTick(function () {
				if(this.ui.planNodesRerender){
					this.ui.planNodesRerender = false;
					this.nodesCalculateWidth();
				}
			})
		}, 500);

	},
	data() {
		return {
			ui: {
				isReady: false,
				isTracking: true,
				trackingLocation: 0,
				isTrackingTimeout: null,
				planNodesRerender: false,
				nodesProcessInterval: null,
				isCompact: false,
				isexpanded: false,
				scrollWidth: 0,
				scrollOffset: 0,
				frameWidth: 0,
				mainScroller: {
					el: null,
					scrollInterval: null,
				},
				mainList: null,
			},
			state: {
				topoMax: 2000,
				topoYRatio: 1,
				topoXRatio: 5,
				aircraftPlotData: {
					x1: 0,
					x2: 100,
					y1: 50,
					y2: 50
				},
				waitingTopo: false,
				planNodes: [],
				nextNode: null,
				currentNode: null,
				totalDistance: 0
			}
		}
	},
	methods: {

		// Process the nodes and make sure the data is up to date
		nodesProcess() {
			if(this.ui.isReady) {
				let previousNode = null as any;
				let nodes = this.state.planNodes;
				this.state.totalDistance = 0;
				const topoPaths = [];
				const topoNodes = [];
				this.selected.Plan.Waypoints.forEach((waypoint :any, index :number) => {
					let newNode = nodes.find(x => x.wp == waypoint);
					if(!newNode){
						newNode = {
							index: index,
							location: waypoint.Location,
							bearingTo: 0,
							//bearingNode: 0,
							distToNext: 0,
							distDCT: 0,
							distTOT: 0,
							distTGO: 0,
							ete: '',
							topo: null,
							maxAlt: 0,
							maxAltFt: 0,
							maxAltOffset: 0,
							isSmall: false,
							isLast: false,
							isHovered: false,
							currentDataStyleStr: '',
							currentDataClass: '',
							div: {
								styleStr: '',
								styleAltStr: '',
								aircraft: '',
							},
							svg: {
								offset: 0,
								width: 0,
								transformStr: '',
								topoBase: '',
								topoDone: '',
								topoDanger: '',
								topoLimit: '',
							},
							progress: 0,
							wp: waypoint,
						};
						nodes.push(newNode);

						this.state.totalDistance += newNode.distToNext;

						if(index > 0){
							const prev = nodes[newNode.index - 1];
							newNode.distTOT = index > 0 ? prev.distTOT + Eljs.GetDistance(waypoint.Location[1], waypoint.Location[0], prev.location[1], prev.location[0], "N") : 0;
							//newNode.bearingNode = index > 0 ? Math.abs(Eljs.MapCompareBearings(newNode.bearing, prev.bearing)) : 0;
							if(prev.topo == null) {
								prev.topo = [];
								topoNodes.push([prev, newNode]);
								topoPaths.push([prev.location, newNode.location]);
							}
						}

						if(previousNode){
							previousNode.distToNext = Eljs.GetDistance(waypoint.Location[1], waypoint.Location[0], previousNode.location[1], previousNode.location[0], "N");
						}
						previousNode = newNode;
					}

					newNode.bearingTo = Eljs.GetBearing(this.$root.$data.state.services.simulator.location.Lat, this.$root.$data.state.services.simulator.location.Lon, waypoint.Location[1], waypoint.Location[0]);
					newNode.distDCT = Eljs.GetDistance(waypoint.Location[1], waypoint.Location[0], this.$root.$data.state.services.simulator.location.Lat, this.$root.$data.state.services.simulator.location.Lon, "N");
				});

				if(topoPaths.length) {
					this.nodeGetTopo(topoPaths, topoNodes);
				}

				// Find current and next waypoint
				let lNextNode = null as any;
				let lCurNode = null as any;
				let lCurNodeIndex = null as number;

				const legs = [];
				const sortedLegs = [];
				let cumulDist = 0;
				nodes.forEach((node, index) => {
					if(index > 0) {

						const loc0 = Eljs.DistPointToPoly(
							nodes[index - 1].location[0],
							nodes[index - 1].location[1],
							node.location[0],
							node.location[1],
							this.$root.$data.state.services.simulator.location.Lon,
							this.$root.$data.state.services.simulator.location.Lat,
						);
						const d0 = Eljs.GetDistance(loc0[2], loc0[1], this.$root.$data.state.services.simulator.location.Lat, this.$root.$data.state.services.simulator.location.Lon, "N");

						const loc1Dist = 15;
						const loc1Offset = Eljs.MapOffsetPosition(this.$root.$data.state.services.simulator.location.Lon, this.$root.$data.state.services.simulator.location.Lat, this.$root.$data.state.services.simulator.location.GS * 0.514444 * loc1Dist, this.$root.$data.state.services.simulator.location.CRS);
						const loc1 = Eljs.DistPointToPoly(
							nodes[index - 1].location[0],
							nodes[index - 1].location[1],
							node.location[0],
							node.location[1],
							loc1Offset[0],
							loc1Offset[1],
						);
						const d1 = Eljs.GetDistance(loc1[2], loc1[1], loc1Offset[1], loc1Offset[0], "N");


						//console.log(nodes[index - 1].wp.Type);
						let s = {
							n: nodes[index - 1].wp.Code,
							i: index - 1,
							d0: d0,
							d1: d1,
							//p: [loc0[1], loc0[2]],
							c: loc0[3],
							b: Math.abs(Eljs.MapCompareBearings(node.bearingTo, this.$root.$data.state.services.simulator.location.Hdg)),
							ir: Eljs.GetDistance(nodes[index - 1].location[1], nodes[index - 1].location[0], this.$root.$data.state.services.simulator.location.Lat, this.$root.$data.state.services.simulator.location.Lon, "N"),
							n0: nodes[index - 1],
							n1: node,
						};
						legs.push(s);
						sortedLegs.push(s);
					}
				});

				sortedLegs.sort((x1, x2) => {
					const dists = ((x1.d0 + x1.d1) - (x2.d0 + x2.d1));
					const apt = (x1.n0.wp.Type == 'airport' && x1.ir < 1 ? -3000 : 0);
					const hdg = (Math.abs(x2.b) / 180) - (Math.abs(x1.b) / 180);
					return dists + apt - hdg;
				});

				//console.log(sortedLegs[0]);
				//console.log(sortedLegs[1]);
				//console.log(sortedLegs[0].n1.bearingTo);
				//console.log("----");

				lCurNodeIndex = sortedLegs[0].i;
				lCurNode = sortedLegs[0].n0;
				lNextNode = sortedLegs[0].n1;

				this.selected.At = lCurNodeIndex;

				this.state.nextNode = lNextNode;
				this.state.currentNode = lCurNode;

				if(this.state.nextNode) {
					legs.forEach((thisLeg :any, i: number) => {
						if(this.$root.$data.state.services.simulator.live) {
							if(lCurNodeIndex < i) {
								thisLeg.n0.progress = 0;
							} else if(lCurNodeIndex > i) {
								thisLeg.n0.progress = 100;
							} else {
								if(thisLeg.n0.topo && thisLeg.n0.topo.length) {
									thisLeg.n0.progress = Math.round(thisLeg.c * thisLeg.n0.topo.length) / (thisLeg.n0.topo.length - 1) * 100;
								} else {
									thisLeg.n0.progress = thisLeg.c * 100;
								}
							}
						} else {
							thisLeg.n0.progress = 0;
						}
						this.createTopoShapes(thisLeg.n0);
					});
				}

				nodes.forEach((node, index) => {

					// Figure out distance to go
					if(index > 0) {
						if(index > lCurNodeIndex + 1) {
							cumulDist += nodes[index - 1].distToNext;
							node.distTGO = cumulDist;
						} else if(index == lCurNodeIndex + 1) {
							node.distTGO = Eljs.GetDistance(this.$root.$data.state.services.simulator.location.Lat, this.$root.$data.state.services.simulator.location.Lon, node.location[1], node.location[0], "N");
							cumulDist += node.distTGO;
						} else {
							node.distTGO = 0;
						}
					}

					// Calculate ETA and DTG
					if(index < this.state.nextNode.index || !this.$root.$data.state.services.simulator.live || this.$root.$data.state.services.simulator.location.GS < 20){
						node.ete = "";
					} else {
						const ETEh = (node.distTGO / this.$root.$data.state.services.simulator.location.GS);
						let hStr = "";
						let mStr = "";
						if(ETEh > 1) {
							hStr = Math.floor(ETEh) + 'h';
						}
						const pad = "00";
						const mNum = Math.round(((ETEh - Math.floor(ETEh)) * 60)) + '';
						mStr = pad.substring(0, pad.length - mNum.length) + mNum + 'm';
						node.ete = hStr + mStr;
					}
				});

				this.state.planNodes = nodes;
				this.updateAircraftPlot();

				this.ui.trackingLocation = lCurNode.svg.offset + (lCurNode.svg.width * (lCurNode.progress / 100)) - 100;
				if(this.ui.isTracking) {
					this.scrollMove(this.ui.trackingLocation);
				}

				this.ui.planNodesRerender = true;
			}
		},

		// Calculate and resize everything
		nodesCalculateWidth() {
			if(this.ui.mainList){

				this.ui.scrollWidth = 0;
				this.ui.frameWidth = this.ui.mainScroller.el.scrollWidth;

				this.ui.mainList.childNodes.forEach((el :HTMLElement, nodeIndex: number) => {
					if(el.nodeName == 'DIV'){
						if(el.classList.contains('enroute_node')){

							const node = this.state.planNodes[nodeIndex];
							node.isLast = nodeIndex == this.state.planNodes.length - 1;
							if(this.state.planNodes.length > 2) {

								const proposedWidth = node.distToNext * this.state.topoXRatio;

								if(!node.isLast) {
									el.style.minWidth = proposedWidth + 'px';
									//node.toScale = proposedWidth > scrollWidth; // If the proposed with is greater than the minimal width
									this.ui.scrollWidth += proposedWidth;
								} else {
									el.style.minWidth = null;
									//node.toScale = true;
									this.ui.scrollWidth += el.scrollWidth;
								}


								let cumulDist = 0;
								for (var i = 0; i < node.index; i++) {
									cumulDist += this.state.planNodes[i].distToNext;
								}
								node.svg.offset = ((cumulDist * this.state.topoXRatio));
								node.svg.transformStr = 'translate(' + node.svg.offset + ' 0)';

							} else if(this.state.planNodes.length == 2) {

								if(!node.isLast) {
									const w = (this.$refs.root as HTMLElement).offsetWidth - 150;
									this.state.topoXRatio = w / node.distToNext;
									this.ui.scrollWidth += w;
									el.style.minWidth = w + 'px';
								} else {
									el.style.minWidth = null;
									this.ui.scrollWidth += 150;
								}

								let cumulDist = 0;
								for (var i1 = 0; i1 < node.index; i1++) {
									cumulDist += this.state.planNodes[i1].distToNext;
								}
								node.svg.offset = ((cumulDist * this.state.topoXRatio));
								node.svg.transformStr = 'translate(' + node.svg.offset + ' 0)';
							}

							const newWidth = (node.distToNext * this.state.topoXRatio) - 1;
							if(!node.isLast) {
								node.svg.width = newWidth > 0 ? newWidth : 0;
							} else {
								node.svg.width = (this.ui.scrollWidth - node.svg.offset);
							}

							node.div.styleStr = 'left: ' + node.svg.offset + 'px; width: ' + node.svg.width + 'px';

						}
					}
				});


				this.updateAircraftPlot();

				if(this.ui.isTracking && this.state.currentNode) {
					this.scrollTo(this.state.currentNode.index);
				}

				this.processScroll();

			}
		},

		// Get topo between 2 nodes
		nodeGetTopo(paths, nodes) {
			this.$root.$data.services.api.SendWS(
				'topography:getForPath', {
					paths: paths,
					resolution: 3,
				}, (topoData :any) => {
					nodes.forEach((node, index) => {
						node[0].topo = topoData.payload[index];
					});
					//node1.topo = topoData.payload;
					this.state.planNodes.forEach((node, index) => {
						this.createTopoShapes(node);
					});
					//this.ui.isReady = true;
				}
			);
		},

		// Create the svg and bar items
		createTopoShapes(node :any) {
			let pointListStatic = [] as string[];
			let pointListDone = [] as string[];

			if(node.topo != null) {
				if(node.topo.length) {
					pointListStatic.push("0, 100.001");
					pointListDone.push("0, 100.001");

					const totalTopoLength = node.topo.length - 1;
					const progressCutoffIndex = (totalTopoLength / 100) * node.progress;
					node.topo.forEach((topo :any) => {
						if(topo + 500 > this.state.topoMax) {
							this.state.topoMax = topo + 500;
						}
					});
					this.state.topoYRatio = 1 / this.state.topoMax;


					const StaticData = { r: false, x: 0, y: 0 }
					const DoneData = { r: false, x: 0, y: 0 }
					node.topo.forEach((topo :any, index :number) => {
						StaticData.x = DoneData.x = index > 0 ? (index / totalTopoLength) * node.svg.width : 0;
						StaticData.y = DoneData.y = 100 - ((topo * this.state.topoYRatio) * 100);

						const newTop = topo;//(Math.ceil(((topo * 3.28084) + 500) / 100) * 100) * 0.3048;

						if(node.maxAlt < newTop) {
							node.maxAlt = newTop;
							node.maxAltFt = newTop * 3.28084;
							node.maxAltOffset = StaticData.x;
						}
						if(index > progressCutoffIndex - 1) {
							if(!DoneData.r){
								DoneData.r = true;
								pointListDone.push(DoneData.x + "," + DoneData.y);
								pointListStatic.push(StaticData.x + "," + 100);
								pointListStatic.push(StaticData.x + "," + StaticData.y);
							}
							DoneData.y = 100;
						} else {
							StaticData.y = 100;
						}
						pointListDone.push(DoneData.x + "," + DoneData.y);
						pointListStatic.push(StaticData.x + "," + StaticData.y);
					});

					// Position aircraft div
					let aircraftAltPct = (1 / (this.state.topoYRatio * 3.28084) * this.$root.$data.state.services.simulator.location.Alt) * 100;
					if(node.progress == 0 || node.progress == 100) {
						node.div.aircraft = '';
					} else {
						node.div.aircraft = 'bottom: ' + (aircraftAltPct > 100 ? 100 : aircraftAltPct) + '%; left: ' + node.progress + '%;';
					}
				}
			}

			// Join poly
			pointListDone.push(node.svg.width + ", 0");
			pointListDone.push(node.svg.width + ", 100.001");
			pointListStatic.push(node.svg.width + ", 0");
			pointListStatic.push(node.svg.width + ", 100.001");
			node.svg.topoBase = pointListStatic.join(' ');
			node.svg.topoDone = pointListDone.join(' ');

			//Altitude Limit
			let sltLimitPct = (this.state.topoYRatio * node.maxAlt) * 100;
			node.svg.topoLimit = 'M0 ' + (100 - sltLimitPct) + ' l' + node.svg.width + ' 0';

			// Set div counterpart
			node.div.styleAltStr = 'left: ' + node.maxAltOffset + 'px; bottom: ' + sltLimitPct + '%';
		},

		// Process Scroll
		processScroll() {
			const nodes = this.state.planNodes;
			const rightPadd = 0;
			let foundNext = false;

			nodes.forEach((node, index) => {
				const refObj = this.$refs['node-data'][index];
				if(refObj) {
					const pxToEnd = (node.svg.offset + node.svg.width - this.ui.scrollOffset) - refObj.clientWidth;

					node.isSmall = node.svg.width - refObj.clientWidth < 0;
					if(!node.isSmall){
						if(pxToEnd > 0){
							if(node.svg.offset - this.ui.scrollOffset >= -3) {
								const OffsetToEnd = node.svg.offset - this.ui.scrollOffset - this.ui.frameWidth + refObj.clientWidth + rightPadd;
								if(OffsetToEnd > 0 && !foundNext && index > 0) {
									foundNext = true;
									const prevNode = nodes[index - 1];
									const prevRefObj = this.$refs['node-data'][index - 1];
									const offset = (this.ui.scrollOffset + this.ui.frameWidth - node.svg.offset - refObj.clientWidth);
									const prevContact = -(prevNode.svg.width + offset - prevRefObj.clientWidth);
									if(prevContact > 0) {
										node.currentDataClass = "is-right";
										node.currentDataStyleStr = 'transform: translateX(' + (offset + prevContact) + 'px)'; // Cap to the screen right
									} else {
										node.currentDataClass = "is-right";
										node.currentDataStyleStr = 'transform: translateX(' + offset + 'px)'; // Cap to the screen right
									}
								} else {
									node.currentDataClass = "";
									node.currentDataStyleStr = 'transform: translateX(' + 0 + 'px)'; // Cap to the start
								}
							} else {
								node.currentDataClass = "is-left";
								node.currentDataStyleStr = 'transform: translateX(' + (this.ui.scrollOffset - node.svg.offset) + 'px)'; // Follow scroll
							}
						} else if(index > 0) {
							node.currentDataClass = "is-left";
							node.currentDataStyleStr = 'transform: translateX(' + (node.svg.width - refObj.offsetWidth) + 'px)'; // Cap to the end
						}
					} else {
						node.currentDataClass = "";
						node.currentDataStyleStr = 'transform: translateX(0px)';
					}
				}
			});

		},

		// Update the aircraft plot
		updateAircraftPlot() {
			const minutes = 5;
			const xOffset = ((this.state.topoXRatio * (this.$root.$data.state.services.simulator.location.GS / 60))) * minutes;
			const yOffset = ((this.state.topoYRatio * this.$root.$data.state.services.simulator.location.FPM * 0.3048 * 60)) * minutes;

			let aircraftYPos = (1 / (this.state.topoMax * 3.28084) * this.$root.$data.state.services.simulator.location.Alt) * 100;
			aircraftYPos = (100 - (aircraftYPos > 100 ? 100 : aircraftYPos));
			let aircraftXPos = this.state.currentNode.svg.offset + (this.state.currentNode.progress > 0 ? this.state.currentNode.svg.width * (this.state.currentNode.progress / 100) : 0);
			const bearingDif = Math.abs(Eljs.MapCompareBearings(this.state.nextNode.bearingTo, this.$root.$data.state.services.simulator.location.CRS));
			let crsCos = Math.cos(bearingDif * Math.PI / 180 );

			this.state.aircraftPlotData.x1 = aircraftXPos;
			this.state.aircraftPlotData.y1 = aircraftYPos;
			this.state.aircraftPlotData.x2 = aircraftXPos + (xOffset * crsCos);
			this.state.aircraftPlotData.y2 = aircraftYPos - yOffset == aircraftYPos ? aircraftYPos + 0.0001 : aircraftYPos - yOffset;
		},

		// Reset Tracking Scroll
		resetTracking() {
			if(this.ui.isTrackingTimeout) {
				this.ui.isTracking = false;
				clearTimeout(this.ui.isTrackingTimeout);
			}
			this.ui.isTrackingTimeout = setTimeout(() => {
				this.ui.isTracking = true;
				this.scrollMove(this.ui.trackingLocation);
			}, 10000);
		},

		hasScrolled(ev :any) {
			this.ui.scrollOffset = ev.target.scrollLeft;
			this.ui.frameWidth = this.ui.mainScroller.el.offsetWidth;
			this.processScroll();
		},
		scrollMove(xOffset: number) {
			clearInterval(this.ui.mainScroller.scrollInterval);
			let prevOffset = 0;
			this.ui.mainScroller.scrollInterval = setInterval(() => {
				if(prevOffset == this.ui.mainScroller.el.scrollLeft) {
					clearInterval(this.ui.mainScroller.scrollInterval);
				}
				prevOffset = this.ui.mainScroller.el.scrollLeft;
				let offsetDif = -(this.ui.mainScroller.el.scrollLeft - xOffset);
				if(Math.abs(xOffset - this.ui.mainScroller.el.scrollLeft) > 1){
					this.ui.mainScroller.el.scrollLeft += Math.ceil(offsetDif / 8);
				} else {
					this.ui.mainScroller.el.scrollLeft = xOffset;
					clearInterval(this.ui.mainScroller.scrollInterval);
				}
			}, 8);
		},
		scrollTo(index :number) {
			let xOffset = 0;
			if(this.ui.mainScroller.el){
				let padding = parseInt(window.getComputedStyle(this.ui.mainScroller.el).getPropertyValue('padding-left').replace('px', ''));
				this.ui.mainScroller.el.childNodes.forEach((el :HTMLElement, i: number) => {
					if(index >= i && el.nodeName == 'DIV'){
						if(el.classList.contains('enroute_node')){
							xOffset = el.offsetLeft - padding;
						}
					}
				});
			}
			this.scrollMove(xOffset);
		},

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
					if(this.ui.isReady) {
						this.nodesProcess();
					}
					break;
				}
			}
		},
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	activated() {
		this.nodesProcess();
		this.nodesCalculateWidth();
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);

		clearInterval(this.ui.nodesProcessInterval);
		clearInterval(this.ui.mainScroller.scrollInterval);

		window.removeEventListener("resize", this.nodesCalculateWidth);
	},
	watch: {
		'selected.Plan': {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.state.planNodes = [] as any[];
					this.ui.isTracking = true;
					this.nodesProcess();
					//this.scrollTo(0);
				}
			}
		},
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.enroute_bar {

	.enroute_scroller {
		position: relative;
		overflow-x: scroll;
		pointer-events: none;
		scrollbar-width: none;
		backdrop-filter: blur(10px);

		.theme--bright &,
		&.theme--bright {
			&.expanded {
				.enroute_background {
					background: rgba($ui_colors_bright_shade_1, 0.2);
				}
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
					//&.is-next {
					//	.node-data {
					//		.node-estimates {
					//			color: $ui_colors_bright_magenta;
					//		}
					//	}
					//}
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
						.node-topo-plan {
							stop:nth-child(1) {
								stop-color: rgba($ui_colors_bright_shade_5, 1);
							}
						}
						.node-topo-done {
							stop:nth-child(2) {
								stop-color: rgba($ui_colors_bright_magenta, 0.8);
							}
						}
						.node-topo-backdrop {
							stop:nth-child(1) {
								stop-color: rgba($ui_colors_dark_shade_5, 0.2);
							}
							stop:nth-child(2) {
								stop-color: rgba($ui_colors_dark_shade_5, 0);
							}
						}
						.node-topo-limit {
							opacity: 1;
						}
					}
					.node-topo-backdrop {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_dark_shade_5, 0);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_dark_shade_5, 0);
						}
					}
					.node-topo-plan {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_bright_shade_5, 0.5);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_bright_shade_5, 0.1);
						}
					}
					.node-topo-done {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_bright_magenta, 1);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_bright_magenta, 0.5);
						}
					}
				}
			}
			.enroute_node {
				&.is-last {
					.node-track {
						background: linear-gradient(to right, rgba($ui_colors_bright_shade_5, 0.1), cubic-bezier(.2,0,.4,1), rgba($ui_colors_bright_shade_5, 0));
					}
				}
				.node-track {
					background: rgba($ui_colors_bright_shade_5, 0.1);
					.node-progress {
						background: linear-gradient(to bottom, rgba($ui_colors_bright_magenta, 0.4), cubic-bezier(.1,0,0,1), rgba($ui_colors_bright_magenta, 0));
					}
				}
				.node-estimates {
					text-shadow: 0 1px 3px rgba($ui_colors_bright_shade_0, 1);
				}
			}
		}

		.theme--dark &,
		&.theme--dark {
			&.expanded {
				.enroute_background {
					background: rgba($ui_colors_dark_shade_1, 0.2);
				}
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
						.node-topo-plan {
							stop:nth-child(1) {
								stop-color: rgba($ui_colors_dark_shade_5, 1);
							}
						}
						.node-topo-done {
							stop:nth-child(2) {
								stop-color: rgba($ui_colors_dark_magenta, 0.8);
							}
						}
						.node-topo-backdrop {
							stop:nth-child(1) {
								stop-color: rgba($ui_colors_dark_shade_5, 0.2);
							}
							stop:nth-child(2) {
								stop-color: rgba($ui_colors_dark_shade_5, 0);
							}
						}
						.node-topo-limit {
							opacity: 1;
						}
					}
					.node-topo-backdrop {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_dark_shade_5, 0);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_dark_shade_5, 0);
						}
					}
					.node-topo-plan {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_dark_shade_5, 0.5);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_dark_shade_5, 0.1);
						}
					}
					.node-topo-done {
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_dark_magenta, 1);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_dark_magenta, 0.5);
						}
					}
				}
			}
			.enroute_node {
				&.is-last {
					.node-track {
						background: linear-gradient(to right, rgba($ui_colors_dark_shade_5, 0.1), cubic-bezier(.2,0,.4,1), rgba($ui_colors_dark_shade_5, 0));
					}
				}
				.node-track {
					background: rgba($ui_colors_dark_shade_5, 0.1);
					.node-progress {
						background: linear-gradient(to bottom, rgba($ui_colors_dark_magenta, 0.4), cubic-bezier(.1,0,0,1), rgba($ui_colors_dark_magenta, 0));
					}
				}
			}
		}

		&::-webkit-scrollbar {
			display: none;
		}

		&.expanded {
			pointer-events: all;
			cursor: pointer;
			.enroute_background {
				backdrop-filter: blur(5px);
			}
			.enroute_canvas {
				> .overlayed > svg {
					height: 300px;
				}
				.node-group {
					.node-data {
						&.is-left {
							opacity: 1;
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
					}
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

		.enroute_background {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			transition: background .5s $transition, backdrop-filter .5s $transition;
			mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) calc(100% - 100px), rgba(0, 0, 0, 0) 100%);
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
				//	&.is-hovered,
				//	&.is-last.is-hovered,
				//	&.is-next.is-hovered,
				//	&.is-last.is-hovered {
				//		.node-data {
				//			.node-estimates {
				//				opacity: 1;
				//			}
				//		}
				//	}
				//	&.is-last,
				//	&.is-next {
				//		.node-data {
				//			.node-estimates {
				//				opacity: 0;
				//			}
				//		}
				//	}
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
					height: 50px;
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
					height: 60px;
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
						opacity: 0;
					}
					.node-code {
						font-family: "SkyOS-SemiBold";
						margin-bottom: 4px;
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
						opacity: 0;
						white-space: nowrap;
						transform: translateY(5px);
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
				}
				.node-track {
					position: relative;
					height: 4px;
					padding-bottom: $nav-size;
					height: 30px;
					width: 100%;
					border-bottom-left-radius: 2px;
					border-bottom-right-radius: 2px;
					overflow: hidden;
					.node-progress {
						position: absolute;
						top: 1px;
						left: 0;
						right: 0;
						bottom: 0;
						width: 0%;
					}
				}
			}
		}
	}

}
</style>