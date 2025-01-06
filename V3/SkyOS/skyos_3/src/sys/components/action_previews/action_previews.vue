<template>
	<div class="action_previews">
		<transition :duration="800">
			<Aircraft
				:app="app"
				:data="preview_data"
				v-if="(preview_data ? preview_data.type == 'fleet' : false) && !covered && app.uid != 'p42_aeroservice'" @close="close" />
			<Contract
				:app="app"
				:data="preview_data"
				v-else-if="(preview_data ? preview_data.type == 'contracts' : false) && !covered && app.uid != 'p42_conduit' && app.uid != 'p42_contrax' && app.uid != 'p42_yoflight' && app.uid != 'p42_progress'" @close="close" />
		</transition>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import ActionPreviewData from '@/sys/classes/action_preview';

export default Vue.extend({
	components: {
    	Aircraft: () => import("./aircraft.vue"),
    	Contract: () => import("./contract.vue"),
	},
	data() {
		return {
			app: null as AppInfo,
			preview_data: null as ActionPreviewData,
			covered: false,
		}
	},
	methods: {
		close() {
			this.preview_data = null;
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to_pre': {
					this.app = wsmsg.payload as AppInfo;
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
					this.covered = wsmsg.payload;
					break;
				}
			}
		},
		listener_map_selected(wsmsg :any) {
			if(wsmsg.payload) {
				switch(wsmsg.name) {
					case 'fleet':
					case 'contracts': {
						this.preview_data = new ActionPreviewData({
							type: wsmsg.name,
							data: wsmsg.payload
						});
						break;
					}
				}
			} else if(this.preview_data) {
				if(wsmsg.name == this.preview_data.type) {
					this.preview_data = null;
				}
			}
		},
	},
	mounted() {
		this.app = this.$os.routing.activeApp;
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('map_selected', this.listener_map_selected);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('map_selected', this.listener_map_selected);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.action_previews {
	position: absolute;
	left: $app-margin + 200;
	top: $status-size;
	right: 70px;
	z-index: 3;
	pointer-events: none;
	.app-expand & {
		left: $app-width + $app-margin + $app-margin;
	}

	& > div {
		pointer-events: all;
		position: absolute;
		top: 0;
		right: 0;

		&.v-enter {
			transform: translateY(-10px);
			opacity: 0;
		}
		&.v-enter-active {
			transition: transform 1s 0.1s cubic-bezier(.37,.87,.5,1), opacity 0.3s 0.1s ease-out;
		}
		&.v-leave,
		&.v-enter-to {
			transform: translateY(0px);
			opacity: 1;
		}
		&.v-leave {
			pointer-events: none;
		}
		&.v-leave-active {
			transition: transform 0.1s cubic-bezier(.37,.87,.5,1), opacity 0.1s ease-out;
		}
		&.v-leave-to {
			transform: translateY(0px);
			opacity: 0;
		}

	}
}

</style>