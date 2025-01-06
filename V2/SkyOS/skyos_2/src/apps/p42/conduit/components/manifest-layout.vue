<template>
	<div>
		<Manifest :app="app" :payload="vehicles" />
		<Manifest :app="app" :payload="locations" />
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Manifest from './manifest.vue'

export default Vue.extend({
	name: "payload_layout",
	props: ['app','payload'],
	components: {
		Manifest
	},
	beforeMount() {
	},
	mounted() {
		this.getSegment();
	},
	activated() {
	},
	data() {
		return {
			vehicles: [],
			locations: []
		}
	},
	methods: {
		getSegment() {
			this.vehicles = [];
			this.locations = [];
			this.payload.forEach(entry => {
				entry.Cargo.forEach(cargo => {
					cargo.Manifests.forEach(manifest => {
						manifest.Groups.forEach(group => {
							if(group.Units.length) {

								let target = null;

								if(group.LoadedOn) {
									target = this.vehicles.find(x => x.Name == group.LoadedOn.Name);
									console.log(target);
									if(!target) {
										target = {
											Name: group.LoadedOn.Name,
											Aircraft: group.LoadedOn,
											Sets: [],
										}
										this.vehicles.push(target);
									}
								} else {
									target = this.locations.find(x => x.Location[0] == group.Location[0] && x.Location[1] == group.Location[1]);
									if(!target) {
										target = {
											Name: group.Location.join(', '),
											Location: group.Location,
											DestColor: null,
											ImageURL: null,
											Airport: group.NearestAirport,
											Sets: [],
										}

										if(group.NearestAirport) {
											target.ImageURL = this.$os.getCDN('images', 'airports/' + group.NearestAirport.ICAO + '.jpg');
											this.$root.$data.services.colorSeek.find(target.ImageURL, (color :any) => {
												target.DestColor = color;
											});
										}

										this.locations.push(target);
									}
								}



								const ui_pallets = [];
								group.Units.forEach(unit => {

									// Find existing pallet (per destination) or create it
									let pallet = ui_pallets.find(x => x.Dest == unit.Dest);
									if(!pallet) {
										pallet = {
											WeightKG: 0,
											UnitWeightKG: manifest.WeightKG,
											Dest: unit.Dest,
											DestColor: null,
											Groups: []
										}

										if(manifest.Destinations[unit.Dest].Airport) {

											this.$root.$data.services.colorSeek.find(this.$os.getCDN('images', 'airports/' + manifest.Destinations[unit.Dest].Airport.ICAO + '.jpg'), (color :any) => {
												pallet.DestColor = color;
											});
										}

										ui_pallets.push(pallet);
									}
									pallet.WeightKG += manifest.WeightKG;

									// Find existing group (per cargo name)
									let uigroup = pallet.Groups.find(x => x.Name == manifest.Name);
									if(!uigroup) {
										uigroup = {
											GUID: group.GUID,
											Name: manifest.Name,
											Health: group.Health,
											Loadable: group.Loadable,
											Unloadable: group.Unloadable,
											WeightKG: 0,
											Boxes: [],
										}
										pallet.Groups.push(uigroup);
									}

									uigroup.WeightKG += manifest.WeightKG;
									uigroup.Boxes.push({
										GUID: unit.GUID,
										Health: unit.Health,
									});

								});

								// Add adventure to offload
								let ui_entry = target.Sets.find(x => x.Adventure.ID == entry.Adventure.ID);
								if(!ui_entry) {
									ui_entry = {
										Adventure: entry.Adventure,
										Action: cargo.PickupID,
										Manifest: manifest.UID,
										WeightKG: 0,
										Pallets: [],
										Destinations: manifest.Destinations,
										DestColor: null,
									}

									const lastDest = manifest.Destinations[manifest.Destinations.length - 1];
									if(lastDest.Airport) {

										this.$root.$data.services.colorSeek.find(this.$os.getCDN('images', 'airports/' + lastDest.Airport.ICAO + '.jpg'), (color :any) => {
											ui_entry.DestColor = color;
										});
									}

									target.Sets.push(ui_entry);
								}

								// Add pallets
								ui_pallets.forEach(pallet => {
									let exisitng_pallet = ui_entry.Pallets.find(x => x.Dest == pallet.Dest);
									if(!exisitng_pallet) {
										ui_entry.Pallets.push(pallet);
									} else {
										exisitng_pallet.WeightKG += pallet.WeightKG;
										pallet.Groups.forEach(box => {
											exisitng_pallet.Groups.push(box);
										});
									}
									ui_entry.WeightKG += pallet.WeightKG;
								});

							}
						});
					});
				});
			});
		},

		//listenerWs(wsmsg: any) {
		//	switch(wsmsg.name[0]){
		//		case 'adventure': {
		//			switch(wsmsg.name[1]) {
		//				case 'manifests': {
		//					this.getSegment();
		//					break;
		//				}
		//			}
		//			break;
		//		}
		//	}
		//}
	},
	created() {
		this.app.$on('payloadUpdated', this.getSegment);
		//this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.app.$off('payloadUpdated', this.getSegment);
		//this.$root.$off('ws-in', this.listenerWs);
	},
	//watch: {
	//	payloads() {
	//		this.getSegment();
	//	}
	//}
});
</script>
