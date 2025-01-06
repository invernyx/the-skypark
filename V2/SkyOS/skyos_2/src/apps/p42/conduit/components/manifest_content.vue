<template>
	<div class="manifest_content" ref="manifest_content">
		<scroll_view :horizontal="false" :offsets="{ top: 0, bottom: 10 }" :scroller_offset="{ top: 30, bottom: 20 }">
			<width_limiter size="tablet">
				<div class="helper_status-margin helper_edge_margin_lateral_half">

					<div class="aircraft-header" v-if="state.ui.aircraft">
						<div class="aircraft-header-manufacturer">{{ state.ui.aircraft.Manufacturer }}</div>
						<div class="aircraft-header-model">{{ state.ui.aircraft.Model }}</div>
						<div class="aircraft-header-creator">{{ state.ui.aircraft.Creator }}</div>
					</div>

					<PayloadLayout :app="app" :payload="state.ui.payload" v-if="state.ui.payload"/>

				</div>
			</width_limiter>
		</scroll_view>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import PayloadLayout from './manifest-layout.vue';

export default Vue.extend({
	name: "manifest_content",
	props: ['app'],
	components: {
		PayloadLayout
	},
	beforeMount() {
	},
	mounted() {

	},
	activated() {
		this.initCard();
	},
	data() {
		return {
			state: {
				ui: {
					aircraft: null,
					payload: null,
				}
			}
		}
	},
	methods: {
		initCard() {
			this.$root.$data.services.api.SendWS(
				"fleet:getcurrent", { },
				(result) => {
					this.state.ui.aircraft = result.payload;
				}
			);
			this.$root.$data.services.api.SendWS(
				"adventures:manifests:get-all", { },
				(result) => {
					this.state.ui.payload = result.payload;
					//this.app.$emit('payloadUpdated');
				}
			);
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'eventbus': {
					switch(wsmsg.name[1]){
						case 'event': {
							if(wsmsg.payload.Type == "AircraftChange") {
								this.initCard();
							}
							break;
						}
						case 'meta': {
							//if(this.state.ui.aircraft) {
							//	this.state.ui.aircraft.PayloadStations = wsmsg.payload.PayloadStations;
							//}
							break;
						}
					}
					break;
				}
				case 'adventure': {
					switch(wsmsg.name[1]) {
						case 'manifest': {
							const existing = this.state.ui.payload.findIndex(x => x.Adventure.ID == wsmsg.payload.Manifests.Adventure.ID);
							this.state.ui.payload[existing] = wsmsg.payload.Manifests;
							this.app.$emit('payloadUpdated');
							break;
						}
					}
					break;
				}
			}
		}
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';
.manifest_content {

	.theme--bright &,
	&.theme--bright {

	}

	.theme--dark &,
	&.theme--dark {

	}

	.aircraft-header {
		position: relative;
		margin-bottom: 8px;
		&-delete {
			position: absolute;
			top: 0;
			right: 0;
		}
		&-manufacturer {
			font-size: 16px;
			line-height: 1.2em;
		}
		&-model {
			font-size: 16px;
			line-height: 1.2em;
			font-family: "SkyOS-SemiBold";
		}
		&-creator {
			font-size: 12px;
			line-height: 1em;
			margin-top: 2px;
		}
	}
}
</style>