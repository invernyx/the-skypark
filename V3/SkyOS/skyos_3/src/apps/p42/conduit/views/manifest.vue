<template>
	<scroll_view :scroller_offset="{ top: 36, bottom: 30 }">
		<div class="app-panel-wrap">
			<div class="app-panel-content">
				<div class="app-panel-hit">
					<PayloadLayout :app="app" :show_aircraft="true" :payload="payload" :payload_state="payload_state" v-if="payload"/>
				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import PayloadLayout from '@/sys/components/manifest/manifest_all.vue';
import Aircraft from '@/sys/classes/aircraft';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		PayloadLayout
	},
	activated() {
		this.init();
	},
	data() {
		return {
			aircraft: this.$os.fleetService.aircraft_current as Aircraft,
			payload: null,
			payload_state: null,
		}
	},
	methods: {
		init() {
			this.$os.api.send_ws(
				"adventures:manifests:get-all", {
					fields: null
				},
				(result) => {
					this.payload = result.payload.meta;
					this.payload_state = result.payload.state;
					//this.app.$emit('payloadUpdated');
				}
			);
		},

		listenerFleet(wsmsg: any) {
			switch(wsmsg.name){
				case 'current_aircraft': {
					this.aircraft = wsmsg.payload.aircraft as Aircraft
					break;
				}
			}
		},
		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'adventure': {
					switch(wsmsg.name[1]) {
						case 'manifest': {
							const existing = this.payload.findIndex(x => x.contract.id == wsmsg.payload.manifests.contract.id);
							this.payload[existing] = wsmsg.payload.manifests;
							this.$os.eventsBus.Bus.emit('payload_updated');
							break;
						}
						case 'manifest_state': {
							const existing = this.payload_state.findIndex(x => x.contract.id == wsmsg.payload.manifests_state.contract.id);
							this.payload_state[existing] = wsmsg.payload.manifests_state;
							this.$os.eventsBus.Bus.emit('payload_updated');
							break;
						}
					}
					break;
				}
			}
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('fleet', this.listenerFleet);
		this.$os.eventsBus.Bus.on('ws-in', this.listenerWs);
		this.$os.system.set_cover(true);
		this.init();
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listenerWs);
		this.$os.eventsBus.Bus.off('fleet', this.listenerFleet);
		this.$os.system.set_cover(false);
	},
});
</script>

<style lang="scss" scoped>
	.app-panel-content {
		max-width: 500px;
	}
</style>