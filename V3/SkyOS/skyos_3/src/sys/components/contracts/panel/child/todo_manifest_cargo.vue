<template>
	<div class="cargo" v-if="manifests.length">

		<!--<div class="title" v-if="is_pickup && index == 0"><strong>Pickups</strong></div>
		<div class="title" v-if="is_dropoff && index == 0"><strong>Dropoffs</strong></div>-->

		<div v-for="(manifest, mi) in manifests" v-bind:key="'m' + mi" class="cargo_manifest">

			<div v-for="(group, gi) in manifest.groups.filter(x => manifest.destinations[x.meta.destination_index].action_id == action.id || manifest.origin.action_id == action.id)" v-bind:key="'g' + gi" class="cargo_group">

				<div class="columns">

					<div class="column column_narrow column_justify_center">
						<div class="cargo_group_icon"></div>
					</div>

					<div class="column column_align_start" v-if="manifest.total_weight > 0">
						<div class="name"><strong>{{ manifest.cargo_name }}</strong></div>

						<div v-if="is_dropoff" class="cargo_group_pallet">{{ manifest.name }}<br/><span class="faded"><number :amount="group.state.count" :decimals="0"/> from {{ manifest.origin.airport.icao }}</span></div>

						<div v-if="is_pickup" class="cargo_group_pallet">{{ manifest.name }}<br/><span class="faded"><number :amount="group.state.count" :decimals="0"/> to {{ manifest.destinations[group.meta.destination_index].airport.icao }}</span></div>

					</div>
					<div class="column column_align_start" v-else>
						<div class="name"><strong>{{ manifest.cargo_name }}</strong></div>

						<div v-if="is_dropoff" class="cargo_group_pallet">{{ manifest.name }}<br/><span class="faded"><span v-if="group.meta.count_percent != 100"><number :amount="group.meta.count_percent" :decimals="0"/>%</span><span v-else>Everything</span> from {{ manifest.origin.airport.icao }}</span></div>

						<div v-if="is_pickup" class="cargo_group_pallet">{{ manifest.name }}<br/><span class="faded"><span v-if="group.meta.count_percent != 100"><number :amount="group.meta.count_percent" :decimals="0"/>%</span><span v-else>Everything</span> to {{ manifest.destinations[group.meta.destination_index].airport.icao }}</span></div>

					</div>

					<div class="column column_narrow column_justify_stretch cargo_group_interactions" v-if="contract.is_monitored">

						<div v-if="group.state.loaded_on && selected_aircraft ? group.state.loaded_on.id != selected_aircraft.id : false">
							<button_action class="go small inline" :class="{ 'disabled': !group.state.transferable }" @click.native.stop="transfer($event, contract.id, cargo.pickup_id, manifest.id, group.meta.guid )">Transfer</button_action>
						</div>

						<div v-else-if="group.state.loaded_on && group.state.transition_to == null">
							<button_action class="small inline disabled">Deboarding</button_action>
						</div>
						<div v-else-if="group.state.transition_to != null && !group.state.loaded_on">
							<button_action class="small inline disabled">Boarding</button_action>
						</div>

						<div v-else-if="!group.state.loaded_on && group.state.loadable && range">
							<button_action class="go small inline" :class="{ 'disabled': !group.state.loadable }" @click.native.stop="load($event, contract.id, cargo.pickup_id, manifest.id, group.meta.guid )">Load</button_action>
						</div>
						<div v-else-if="group.state.deliverable && range">
							<button_action class="go small inline" @click.native.stop="unload($event, contract.id, cargo.pickup_id, manifest.id, group.meta.guid )">Deliver</button_action>
						</div>
						<div v-else-if="group.state.loaded_on && group.state.unloadable &&range && (selected_aircraft ? group.state.loaded_on.id == selected_aircraft.id : false) && manifest.origin.action_id == action.id">
							<button_action class="small inline" @click.native.stop="unload($event, contract.id, cargo.pickup_id, manifest.id, group.meta.guid )">Unload</button_action>
						</div>

						<div v-else-if="group.state.delivered">
							<button_action class="small inline disabled">Delivered</button_action>
						</div>
						<div v-else-if="group.state.loaded_on && (selected_aircraft ? group.state.loaded_on.id == selected_aircraft.id : false)">
							<button_action class="small inline disabled">Loaded</button_action>
						</div>

					</div>

				</div>

			</div>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';
import Contract from '@/sys/classes/contracts/contract';
import SearchStates from '@/sys/enums/search_states';
import { AppInfo } from '@/sys/foundation/app_model';

export default Vue.extend({
    props: {
        app: AppInfo,
        contract: Contract,
        action: Object,
        index: Number,
        range: Boolean,
        cargo: Object,
    },
    data() {
        return {
            manifests: [],
            is_pickup: false,
            is_dropoff: false,
            selected_aircraft: null as Aircraft,
        };
    },
    methods: {
        init() {
            this.manifests = [];
            const pickups = this.cargo.manifests.filter(x0 => x0.groups.filter(x => x0.origin.action_id == this.action.id).length);
            const dropoffs = this.cargo.manifests.filter(x0 => x0.groups.filter(x => x0.destinations[x.destination_index].action_id == this.action.id).length);
            [].concat(pickups, dropoffs).forEach(manifest => {
                const newMan = Object.assign({}, manifest);
                const manifest_match = this.contract.manifests.cargo[this.index].manifests.find(x => x.id == manifest.id);
                const manifest_index = this.contract.manifests.cargo[this.index].manifests.indexOf(manifest_match);
                newMan.groups = manifest.groups.map((x, i) => {
                    return {
                        meta: x,
                        state: this.contract.manifests_state.cargo[this.index].manifests[manifest_index].groups[i],
                    };
                });
                this.manifests.push(newMan);
            });
            this.is_pickup = pickups.length;
            this.is_dropoff = dropoffs.length;
        },
        transfer(event: any, adventure: number, link_id: number, manifest: any = null, group: any = null, units: any[] = null) {
            this.$os.api.send_ws("adventure:interaction", {
                id: adventure,
                link: link_id,
                verb: "transfer",
                data: {
                    manifest: manifest,
                    group: group,
                    units: units
                },
            });
        },
        load(event: any, adventure: number, link_id: number, manifest: any = null, group: any = null, units: any[] = null) {
            this.$os.api.send_ws("adventure:interaction", {
                id: adventure,
                link: link_id,
                verb: "load",
                data: {
                    manifest: manifest,
                    group: group,
                    units: units
                },
            });
        },
        unload(event: any, adventure: number, link_id: number, manifest: any = null, group: any = null, units: any[] = null) {
            this.$os.api.send_ws("adventure:interaction", {
                id: adventure,
                link: link_id,
                verb: "unload",
                data: {
                    manifest: manifest,
                    group: group,
                    units: units
                },
            });
        },
        listener_os_contracts(wsmsg: any) {
            switch (wsmsg.name) {
                case "remove":
                case "mutate": {
                    this.init();
                    break;
                }
            }
        },
        listenerFleet(wsmsg: any) {
            switch (wsmsg.name) {
                case "current_aircraft": {
                    this.selected_aircraft = wsmsg.payload.aircraft as Aircraft;
                    break;
                }
            }
        },
    },
    mounted() {
        this.selected_aircraft = this.$os.fleetService.aircraft_current;

        this.$os.eventsBus.Bus.on("fleet", this.listenerFleet);
        this.$os.eventsBus.Bus.on("contracts", this.listener_os_contracts);
        this.init();
    },
    beforeDestroy() {
        this.$os.eventsBus.Bus.off("fleet", this.listenerFleet);
        this.$os.eventsBus.Bus.off("contracts", this.listener_os_contracts);
    },
    watch: {
        contract() {
            this.init();
        },
        cargo() {
            this.init();
        }
    }
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.cargo {

	.theme--bright & {
		&_group {
			&_icon {
				background-image: url(../../../../../sys/assets/icons/dark/cargo.svg);
			}
		}
	}

	.theme--dark & {
		&_group {
			&_icon {
				background-image: url(../../../../../sys/assets/icons/bright/cargo.svg);
			}
		}
	}

	&:last-child {
		.cargo_manifest {
			&:last-child {
				.cargo_group {
					&:last-child {
						margin-bottom: 0;
					}
				}
			}
		}
	}

	.title {
		font-size: 14px;
		line-height: 1.4em;
		margin-bottom: 8px;
	}

	&_group {
		margin-bottom: 4px;

		&_icon {
			width: 18px;
			height: 18px;
			margin-right: 8px;
		}

		&_pallet {
			padding: 0.1em 0;
			border-radius: 4px;
			margin-right: 4px;
		}

		&_interactions {
			display: flex;
			margin-right: -4px;
			& > div {
				display: flex;
				flex-grow: 1;
			}
			.button_action {
				padding: 0 8px;
				border-radius: 4px;
				text-transform: uppercase;
			}
		}
	}


}

</style>