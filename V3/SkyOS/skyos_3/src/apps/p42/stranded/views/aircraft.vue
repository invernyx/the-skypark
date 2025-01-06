<template>
	<scroll_view :sid="'p42_aeroservice_aircraft'" :scroller_offset="{ top: 36, bottom: 30 }" @scroll="$emit('scroll', $event)">
		<div class="app-panel-wrap">
			<div class="app-panel-content" ref="app_panel_content" :class="{ 'is-open': open }" :style="{ 'padding-top': 'var(--spacing-top)' }">
				<div class="app-spacer"></div>
				<div class="app-panel-hit" @mouseenter="$emit('can_scroll', true)" @mouseleave="$emit('can_scroll', false)">

					<AircraftPanel :app="app" :loading="loading" :aircraft="selected_aircraft" :app_panel_content="$refs.app_panel_content" />

				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Aircraft from "@/sys/classes/aircraft";
import AircraftPanel from "@/sys/components/aircraft/aircraft_panel.vue"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		AircraftPanel
	},
	data() {
		return {
			open: false,
			loading: true,
			selected_aircraft: null as Aircraft,
		}
	},
	methods: {

		init() {
			this.loading = true;
			this.$os.fleetService.dispose_single(this.selected_aircraft);
			const aircraft_id = parseInt(this.$route.params.id);
			this.$os.api.send_ws('fleet:get',
				{
					id: aircraft_id,
					fields: null
				},
				(aircraftData: any) => {
					this.selected_aircraft = this.$os.fleetService.ingest(aircraftData.payload);
					this.loading = false;
				}
			);
		},

		listener_os_fleet(wsmsg :any) {
			switch(wsmsg.name) {
				case 'remove':
				case 'mutate': {
					this.$os.fleetService.event([wsmsg.name], wsmsg.payload.id, wsmsg.payload.aircraft, this.selected_aircraft)
					break;
				}
			}
		},
		listener_map(wsmsg: any) {
			switch(wsmsg.name){
				case 'interact': {
					this.$os.scrollView.scroll_to('p42_aeroservice_aircraft', 0, 0, 300);
					break;
				}
			}
		},
		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					if(this.$route.name == 'p42_aeroservice_aircraft') {
						this.init();
					} else {
						this.$os.fleetService.dispose_single(this.selected_aircraft);
					}
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'uncover': {
					this.$os.scrollView.scroll_to('p42_aeroservice_aircraft', 0, 0, 100);
					break;
				}
				case 'covered': {
					this.open = wsmsg.payload;
					break;
				}
			}
		},
	},
	beforeMount() {
		if(this.$route.params.expand) {
			this.open = true;
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('map', this.listener_map);
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.on('fleet', this.listener_os_fleet);
		this.init();
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('map', this.listener_map);
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
		this.$os.eventsBus.Bus.off('fleet', this.listener_os_fleet);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.app-panel-content {
	max-width: 480px;
}
</style>