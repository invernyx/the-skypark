<template>
	<div>

		<Humans_summary :contract="contract" :humans="humans" v-if="contract.is_monitored"></Humans_summary>

		<div>
			<div v-for="(pax, index) in contract.manifests.pax" v-bind:key="'p' + index" class="payload">
				<div v-for="(manifest, index) in pax.manifests" v-bind:key="'pm' + index" class="payload_manifest">
					<div class="columns">
						<div class="column column_narrow column_justify_center">
							<div class="payload_manifest_image payload_manifest_image_pax"></div>
						</div>
						<div class="column">

							<div class="columns">
								<div class="column">
									<div class="title" v-if="manifest.total > 0"><strong>{{ manifest.total }} {{ manifest.name }}</strong></div>
									<div class="title" v-else><strong>{{ manifest.name }}</strong></div>
								</div>
								<div class="column column_narrow">
									<span v-if="$os.fleetService.aircraft_current && !manifest.total_weight && manifest.total == 0">≈&nbsp;<weight :amount="Math.floor($os.fleetService.aircraft_current.cabin.features.filter(f => f.type == 'seat' && f.sub_type != 'pilot' && f.sub_type != 'copilot' && f.sub_type != 'jumpseat').length * (manifest.total_percent / 100)) * 82.11" :decimals="0" /></span>
									<span v-else-if="manifest.total_weight"><weight :amount="manifest.total_weight" :decimals="0" /></span>
									<span v-else-if="manifest.total > 0">≈&nbsp;<weight :amount="manifest.total * 82.11" :decimals="0" /></span>
									<span v-else class="nowrap"><number :amount="manifest.total_percent"/>%</span>
								</div>
							</div>

							<progress_bar class="deep small h_edge_margin_vertical_half" :percent="manifest.total_percent" v-if="manifest.total_percent > 0"/>

							<div>
								<span class="from" v-if="manifest.origin.airport">
									<span>From </span>
									<strong>{{ manifest.origin.airport.icao }}</strong>
								</span>
								<span>
									<span class="to" v-for="(destination, index) in manifest.destinations" v-bind:key="'d' + index">
										<span v-if="index == 0"> to </span>
										<span v-if="index == manifest.destinations.length - 1 && index > 0"> and </span>
										<span v-else-if="index > 0">, </span>
										<span v-if="destination.airport">
											<strong>{{ destination.airport.icao }}</strong>
										</span>
										<span v-else-if="destination.location_name.length">{{ destination.location_name }}</span>
										<span v-else>{{ destination.location[0] + ', ' + destination.location[1] }}</span>
									</span>
								</span>
							</div>
						</div>
					</div>
				</div>
			</div>
			<div v-for="(cargo, index) in contract.manifests.cargo" v-bind:key="'c' + index" class="payload">
				<div v-for="(manifest, index) in cargo.manifests" v-bind:key="'cm' + index" class="payload_manifest">
					<div class="columns">
						<div class="column column_narrow column_justify_center">
							<div class="payload_manifest_image payload_manifest_image_cargo"></div>
						</div>
						<div class="column">

							<div class="columns">
								<div class="column">
									<div class="title" v-if="manifest.total > 0"><strong>{{ manifest.total }} {{ manifest.name }}</strong></div>
									<div class="title" v-else><strong>{{ manifest.name }}</strong></div>
								</div>
								<div class="column column_narrow">
									<span v-if="$os.fleetService.aircraft_current && !manifest.total_weight">≈&nbsp;<weight :amount="Math.floor($os.fleetService.aircraft_current.cabin.features.filter(f => f.type == 'cargo').length * (manifest.total_percent / 100)) * manifest.weight" :decimals="0" /></span>
									<span v-else-if="manifest.total_weight"><weight :amount="manifest.total_weight" :decimals="0" /></span>
									<span v-else class="nowrap"><number :amount="manifest.total_percent"/>%</span>
								</div>
							</div>

							<progress_bar class="deep small h_edge_margin_vertical_half" :percent="manifest.total_percent"/>

							<div>
								<span class="from" v-if="manifest.origin.airport">
									<span>From </span>
									<strong>{{ manifest.origin.airport.icao }}</strong>
								</span>
								<span>
									<span class="to" v-for="(destination, index) in manifest.destinations" v-bind:key="'d' + index">
										<span v-if="index == 0"> to </span>
										<span v-if="index == manifest.destinations.length - 1 && index > 0"> and </span>
										<span v-else-if="index > 0">, </span>
										<span v-if="destination.airport">
											<strong>{{ destination.airport.icao }}</strong>
										</span>
										<span v-else-if="destination.location_name.length">{{ destination.location_name }}</span>
										<span v-else>{{ destination.location[0] + ', ' + destination.location[1] }}</span>
									</span>
								</span>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>

		<p class="notice text-center" v-if="contract.manifests.total_cargo_perent > 0 || contract.manifests.total_pax_perent > 0">% of available cargo slots or passenger seats.</p>

	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import { AppInfo } from '@/sys/foundation/app_model';
import Human from '@/sys/components/cabin/human.vue';
import Humans_summary from './humans_summary.vue';
import Vue from 'vue';
import { watch } from 'fs';

export default Vue.extend({
	props: {
		app: AppInfo,
		contract: Contract,
		humans :Array
	},
    components: { Human, Humans_summary },
	data() {
		return {
		}
	},
	methods: {
		manage() {
			this.$os.routing.goTo({ name: 'p42_conduit_contract', params: { id: this.contract.id, contract: this.contract, expand: true }})
		},
	},
	mounted() {

	},
	beforeDestroy() {

	},
	computed: {
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.payload {

	.theme--bright &,
	&.theme--bright {
		&_manifest {
			background-color: $ui_colors_bright_shade_2;
		}
	}

	.theme--dark &,
	&.theme--dark {
		&_manifest {
			background-color: $ui_colors_dark_shade_2;
		}
	}

	&:last-child {
		.payload_manifest {
			&:last-child {
				margin-bottom: 0;
			}
		}
	}

	&_manifest {
		margin-bottom: 8px;
		padding: 8px;
		border-radius: 8px;
		.title {
			font-size: 16px;
			line-height: 1.4em;
			display: flex;
			justify-content: space-between;
		}
		.from {
			opacity: 0.5;
		}
		.to {
			opacity: 0.5;
		}
		&_image {
			width: 36px;
			min-width: 36px;
			height: 36px;
			margin-right: 12px;
			background-size: contain;
			background-position: center;
			&_pax {
				background-image: url(../../../../../sys/assets/pax/default.png);
			}
			&_cargo {
				background-image: url(../../../../../sys/assets/cargo/default.png);
			}
		}
	}

}

</style>