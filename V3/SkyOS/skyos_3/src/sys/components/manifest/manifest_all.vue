<template>
	<div>
		<Manifest :app="app" :aircraft="aircraft" :payload="onboard ? [onboard] : []" :is_aircraft="true" v-if="show_aircraft" />
		<Manifest :app="app" :aircraft="aircraft" :payload="locations" v-if="locations.length" />
	</div>
</template>

<script lang="ts">
import Aircraft from '@/sys/classes/aircraft';
import { AppInfo } from '@/sys/foundation/app_model';
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';
import Manifest from './manifest.vue'

export default Vue.extend({
	name: "payload_layout",
	props: {
		app: AppInfo,
		payload: Array as () => Array<any>,
		payload_state: Array as () => Array<any>,
		show_aircraft: Boolean,
		show_inactives: Boolean
	},
	components: {
		Manifest
	},
	data() {
		return {
			aircraft: null as Aircraft,
			locations: [],
			onboard: null,
		}
	},
	methods: {
		split_segments() {
			this.onboard = null;
			this.locations = [];


			const create_segment = (type, cargo, entry, entry_state, group, group_state, manifest) => {
				let onboard = group_state.loaded_on && (this.aircraft ? group_state.loaded_on.id == this.aircraft.id : false);
				let aircraft = null;
				let sets = [];
				let target = null;

				if(onboard) {
					target = this.onboard;
					if(!target) {
						target = {
							name: group_state.location.join(', '),
							location: null,
							dest_color: null,
							image_url: null,
							airport: null,
							sets: [],
							aircraft: []
						}
						this.onboard = target;
					}
				} else {
					target = this.locations.find(x => {
						if((x.airport && group_state.nearest_airport ? x.airport.icao == group_state.nearest_airport.icao : false)) {
							return true;
						} else {
							const dist = Eljs.GetDistance(group_state.location[1], group_state.location[0], x.location[1], x.location[0], 'm');
							return dist < 30
						}
					});

					if(!target) {
						target = {
							name: group_state.location.join(', '),
							location: group_state.location,
							dest_color: null,
							image_url: null,
							airport: group_state.nearest_airport,
							sets: [],
							aircraft: []
						}
						this.locations.push(target);
					}
				}

				if(!onboard && group_state.loaded_on) {
					aircraft = target.aircraft.find(x => x.aircraft.id == group_state.loaded_on.id);
					if(!aircraft) {
						aircraft = {
							aircraft: group_state.loaded_on,
							aircraft_current: this.aircraft ? group_state.loaded_on.id == this.aircraft.id : false,
							airport: group_state.nearest_airport,
							sets: sets,
						}
						target.aircraft.push(aircraft);
					} else {
						sets = aircraft.sets;
					}
				} else {
					sets = target.sets;
				}

				if(target.airport) {
					target.image_url = this.$os.api.getCDN('images', 'airports/' + target.airport.icao + '.jpg');
					this.$os.colorSeek.find(target.image_url, 127, (result :any) => {
						target.dest_color = result.color.h;
					});
				}

				const ui_containers = [];

				// Find existing pallet (per destination) or create it
				let container = ui_containers.find(x => {
					const newDest = manifest.destinations[group.destination_index];
					const dist = Eljs.GetDistance(x.dest.location[0], x.dest.location[1], newDest.location[0], newDest.location[1], "m")
					return dist < 30;
				});

				if(!container) {
					container = {
						type: type,
						weight: 0,
						unit_weight: manifest.weight,
						dest: manifest.destinations[group.destination_index],
						dest_color: null,
						origin: manifest.origin,
						groups: []
					}

					if(container.dest.airport) {
						this.$os.colorSeek.find(this.$os.api.getCDN('images', 'airports/' + container.dest.airport.icao + '.jpg'), 127, (result :any) => {
							container.dest_color = result.color.h;
						});
					}

					ui_containers.push(container);
				}

				// Find existing group (per cargo name)
				let uigroup = container.groups.find(x => x.guid == manifest.guid);
				if(!uigroup) {
					uigroup = {
						guid: group.guid,
						manifest: manifest.id,
						action: cargo.pickup_id,
						cargo_guid: type == 'cargo' ? manifest.cargo_guid : null,
						name: type == 'cargo' ? manifest.name : 'Passengers',
						loadable: group_state.loadable,
						unloadable: group_state.unloadable,
						delivered: group_state.delivered,
						loaded_on_id: group_state.loaded_on ? group_state.loaded_on.id : null,
						transferable: group_state.transferable,
						deliverable: group_state.deliverable,
						weight: 0,
						boxes: [],
					}
					container.groups.push(uigroup);
				}

				while(uigroup.boxes.length < group.count) {
					uigroup.weight += manifest.weight;
					uigroup.boxes.push({
						guid: '',
						health: group_state.health,
					});
				}
				container.weight += uigroup.weight;

				// Add contract to offload
				let ui_entry = sets.find(x => x.contract.id == entry.contract.id);
				if(!ui_entry) {
					ui_entry = {
						contract: entry.contract,
						contract_state: entry_state.contract,
						weight: 0,
						containers: [],
						destinations: manifest.destinations,
						dest_color: null,
					}

					const lastDest = container.dest;
					if(lastDest.airport) {
						this.$os.colorSeek.find(this.$os.api.getCDN('images', 'airports/' + lastDest.airport.icao + '.jpg'), 127, (result :any) => {
							ui_entry.dest_color = result.color.h;
						});
					}

					sets.push(ui_entry);
				}

				// Add pallets
				ui_containers.forEach(pallet => {
					let exisitng_pallet = ui_entry.containers.find(x => {
					const newDest = pallet.dest;
					const dist = Eljs.GetDistance(x.dest.location[0], x.dest.location[1], newDest.location[0], newDest.location[1], "m")
					return dist < 30;
				});
					if(!exisitng_pallet) {
						ui_entry.containers.push(pallet);
					} else {
						exisitng_pallet.weight += pallet.weight;
						pallet.groups.forEach(box => {
							exisitng_pallet.groups.push(box);
						});
					}
					ui_entry.weight += pallet.weight;
				});
			}




			this.payload.forEach((entry, i1) => {
				const entry_state = this.payload_state[i1];

				// Cargo
				entry.cargo.forEach((cargo, i2) => {
					const cargo_state = entry_state.cargo[i2];
					cargo.manifests.forEach((manifest, i3) => {
						const manifest_state = cargo_state.manifests[i3];
						if(manifest.groups.length) {
							manifest.groups.forEach((group, i4) => {
								const group_state = manifest_state.groups[i4];
								create_segment('cargo', cargo, entry, entry_state, group, group_state, manifest);
							});
						}
					});
				});

				// Pax
				entry.pax.forEach((pax, i2) => {
					const pax_state = entry_state.pax[i2];
					pax.manifests.forEach((manifest, i3) => {
						const manifest_state = pax_state.manifests[i3];
						if(manifest.groups.length) {
							manifest.groups.forEach((group, i4) => {
								const group_state = manifest_state.groups[i4];
								create_segment('pax', pax, entry, entry_state, group, group_state, manifest);
							});
						}
					});
				});

			});

		},

		listener_fleet(wsmsg: any) {
			switch(wsmsg.name){
				case 'current_aircraft': {
					this.aircraft = wsmsg.payload.aircraft as Aircraft
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.eventsBus.Bus.on('payload_updated', this.split_segments);
		this.$os.eventsBus.Bus.on('fleet', this.listener_fleet);
		this.aircraft = this.$os.fleetService.aircraft_current;
		this.split_segments();
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('payload_updated', this.split_segments);
		this.$os.eventsBus.Bus.off('fleet', this.listener_fleet);
	},
	watch: {
		payload: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.split_segments();
				}
			}
		},
	}
});
</script>
