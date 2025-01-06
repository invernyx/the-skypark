<template>
	<div>
		<div class="aircraft-payload">
			<div class="aircraft-payload-layout" ref="frame">
				<div class="aircraft-payload-layout-grid">
					<div class="aircraft-payload-layout-grid-v" :style="'left:' + graph.cg_x + '%'"></div>
					<div class="aircraft-payload-layout-grid-h" :style="'top:' + graph.cg_z + '%'"></div>
				</div>
				<div class="aircraft-payload-layout-stations">
					<div class="aircraft-payload-layout-stations-station" v-for="(station, index) in graph.stations" v-bind:key="index" :index="index" :style="'top:' + station.z + '%;left:' + station.x + '%'">
						<span>{{ station.l }}</span>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "aircraft_payload_layout",
	props: ['app','payloads'],
	components: {
	},
	beforeMount() {
	},
	mounted() {
		this.initPayload();
	},
	activated() {
	},
	data() {
		return {
			graph: {
				max: 0,
				min: 0,
				cg_z: 50,
				cg_x: 50,
				stations: []
			}
		}
	},
	methods: {
		initPayload() {
			if(this.payloads) {
				const frameSize = {
					w: (this.$refs.frame as HTMLElement).offsetWidth,
					h: (this.$refs.frame as HTMLElement).offsetHeight,
					r: 1,
				}
				const min = {
					x: null,
					z: null,
				};
				const max = {
					x: null,
					z: null,
				};
				const center_offset = {
					x: 0,
					z: 0,
				}
				const depth = {
					x: 2,
					z: 2,
				};
				this.payloads.forEach(station => {
					const flip_x = station.X;
					const flip_z = -station.Z;
					if(!min.x || flip_x < min.x) {
						min.x = flip_x;
					}
					if(!min.z || flip_z < min.z) {
						min.z = flip_z;
					}
					if(!max.x || flip_x > max.x) {
						max.x = flip_x;
					}
					if(!max.z || flip_z > max.z) {
						max.z = flip_z;
					}
				});

				frameSize.r = frameSize.w / frameSize.h;
				let depth_ref = null;
				depth.x = max.x - min.x;
				depth.z = max.z - min.z;

				if(frameSize.r < 1) {
					depth_ref = depth.z;
					center_offset.x = (100 / 2) - ((depth.x / depth.z * 100) / 2);
					center_offset.z = 0;
				} else {
					depth_ref = depth.x;
					center_offset.z = (100 / 2) - ((depth.z / depth.x * 100) / 2);
					center_offset.x = 0;
				}

				this.graph.cg_z = (-min.z  / depth_ref * 100) + center_offset.z;
				this.graph.cg_x = (-min.x  / depth_ref * 100) + center_offset.x;

				this.graph.stations = [];
				this.payloads.forEach(station => {
					this.graph.stations.push({
						x: (((station.X) - min.x) / depth_ref * 100) + center_offset.x,
						z: (((-station.Z) - min.z) / depth_ref * 100) + center_offset.z,
						l: station.Load
					})
				});
			}
		}
	},
	watch: {
		payloads() {
			this.initPayload();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';
.aircraft-payload {
	$padding: 40px;
	background: rgba(255,255,255,0.2);
	position: relative;
	border-radius: 8px;
	padding: $padding;
	&-layout {
		position: relative;
		height: 300px;
		margin: 0 auto;
		&-grid {
			&-v,
			&-h {
				display: flex;
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
			}
			&-v {
				width: 1px;
				background: linear-gradient(to bottom, rgba(#000, 0.1), rgba(#000, 0.2), rgba(#000, 0.1));
				margin-top: -$padding;
				margin-bottom: -$padding;
			}
			&-h {
				flex-direction: column;
				height: 1px;
				background: linear-gradient(to right, rgba(#000, 0.1), rgba(#000, 0.2), rgba(#000, 0.1));
				margin-left: -$padding;
				margin-right: -$padding;
			}
		}
		&-stations {
			&-station {
				position: absolute;
				width: 30px;
				height: 30px;
				border-radius: 50%;
				background: rgba(0,0,0,0.3);
				box-shadow: 0 0 1px #000;
				transform: translate(-50%, -50%);
			}
		}
	}
}
</style>