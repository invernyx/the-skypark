<template>
	<div class="action_preview_aircraft app-box shadowed-deep nooverflow h_edge_padding">

		<div class="background" :style="{ 'background-image': 'url(' + aircraft.image_blob + ')'}"></div>

		<div class="content">

			<div class="columns">
				<div class="column">
					<data_stack :label="aircraft.manufacturer + ' / ' + aircraft.creator" class="start small">{{ aircraft.model }}</data_stack>
				</div>
				<div class="column column_narrow column_justify_center">
					<div class="buttons_list shadowed-shallow">
						<button_action class="arrow small info" @click.native="expand">Open</button_action>
					</div>
				</div>
			</div>

			<div class="columns h_edge_padding_top">
				<div class="column">
					<data_stack :label="aircraft.location ? (aircraft.location[1].toFixed(8) + ', ' + aircraft.location[0].toFixed(8)) : 'No aircraft relocation fee on your first flight.'" class="start small" v-if="aircraft">
						<span v-if="aircraft.nearest_airport">{{ aircraft.nearest_airport.icao }} - <strong>{{ aircraft.nearest_airport.name }}</strong></span>
						<span v-else-if="aircraft.location">Off-field</span>
						<span v-else>Brand new, sealed box</span>
					</data_stack>
				</div>
				<div class="column column_narrow column_justify_center">
					<div class="buttons_list shadowed-shallow">
						<button_action class="small go" v-if="aircraft.location" @click.native="copyLocation">Copy</button_action>
					</div>
				</div>
			</div>

			<!--
			<div class="location h_edge_margin_top_half">
				<div class="columns columns_margined">
					<div class="column">
						<data_stack :label="aircraft.location ? (aircraft.location[1].toFixed(4) + ', ' + aircraft.location[0].toFixed(4)) : 'No aircraft relocation fee on your first flight.'" class="start small" v-if="aircraft">
							<span v-if="aircraft.nearest_airport">{{ aircraft.nearest_airport.icao }}<span class="h_hide_1000"> - <strong>{{ aircraft.nearest_airport.name }}</strong></span></span>
							<span v-else-if="aircraft.location">Off-field</span>
							<span v-else>Brand new, sealed box</span>
						</data_stack>
					</div>
					<div class="column column_narrow column_justify_center">
						<button_action class="go small" v-if="aircraft.location">Copy coords</button_action>
					</div>
				</div>
			</div>
			-->

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import ActionPreviewData from '@/sys/classes/action_preview';
import Aircraft from '@/sys/classes/aircraft';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		app: AppInfo,
		data: ActionPreviewData,
	},
	components: {
	},
	data() {
		return {
			aircraft: null as Aircraft
		}
	},
	methods: {
		expand() {
			this.$os.routing.goTo({ name: 'p42_aeroservice_aircraft', params: { id: this.aircraft.id, aircraft: this.aircraft }})
		},

		copyLocation() {
			if(this.aircraft.nearest_airport) {
				Eljs.copyTextToClipboard(this.aircraft.nearest_airport.icao);
				this.$os.modals.add({
					type: 'notify',
					title: this.aircraft.nearest_airport.icao,
					text: [
						this.aircraft.nearest_airport.icao +' has been copied to your clipboard.',
						'You can paste this ICAO in the search field of the MSFS world map.'
					],
				});
			} else {
				Eljs.copyTextToClipboard(this.aircraft.location[1] + ', ' + this.aircraft.location[0]);
				this.$os.modals.add({
					type: 'notify',
					title: 'Off-field',
					text: [
						'Coordinates have been copied.',
						this.aircraft.location[1] + ', ' + this.aircraft.location[0],
						'You can paste this location in the search field of the MSFS world map.',
						'Be aware that MSFS does not allow you to start on the ground when using coordinates.'
					],
				});
			}

		},

		listener_navigate(wsmsg :any) {
			//switch(wsmsg.name){
			//	case 'to': {
			//		this.$emit('close');
			//		break;
			//	}
			//}
		},
	},
	beforeMount() {
		this.aircraft = this.data.data as Aircraft;
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	},
	watch: {
		data() {
			this.aircraft = this.data.data as Aircraft;
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.action_preview_aircraft {
	max-width: 400px;

	.background {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		opacity: 0.8;
		background-size: cover;
		background-position: center;
		filter: blur(5px);
		opacity: 0.4;
		transition: background 0.4s ease-out;
	}

	.content {
		position: relative;
		z-index: 2;
	}

	.data-stack {
		margin-right: 32px;
	}

	.location {
		padding: 8px;
		margin: -8px;
		border-radius: 6px;
		margin-top: 4px;
		.theme--bright & {
			background: rgba($ui_colors_bright_shade_0,0.2);
		}
		.theme--dark & {
			background: rgba($ui_colors_dark_shade_0,0.2);
		}
	}
}

</style>