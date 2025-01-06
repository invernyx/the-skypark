<template>
	<div>
		<div v-if="Action.Action == 'cargo_pickup_2' && CargoTags.length > 0">

			<!--
			<div class="buttons_list_half">
				<selector class="listed" placeholder="Transport" v-if="Action.Params.Models.length > 0 && $Situation.SituationType !== 'Geo'" v-model="Action.Params.Transport" @input="UpdateManifest">
					<option v-for="Veh in Transport" v-bind:key="Veh.Name" :value="Veh.Name">{{ Veh.Descr }}</option>
					<option v-if="!Transport.find(x => x.Name == Action.Params.Transport)" :value="Action.Params.Transport">{{ Action.Params.Transport }}</option>
				</selector>
			</div>
			-->

			<div v-for="(Manifest, index) in Action.Params.Manifests" v-bind:key="Manifest.UID" class="buttons_list_half">
				<selector class="listed" placeholder="Manifest" v-model="Manifest.Tag" @input="UpdateManifest(index)">
					<option :value="null" v-if="Action.Params.Manifests.length > 1">-- Remove</option>
					<option v-for="Item in CargoTags" v-bind:key="Item" :value="Item">{{ Item }}</option>
					<option v-if="!CargoTags.includes(Manifest.Tag)" :value="Manifest.Tag">{{ Manifest.Tag }}</option>
				</selector>
				<selector class="listed" placeholder="Auto Bracket Weights" v-model="Manifest.AutoBracket">
					<option :value="'recommended_aircraft_min'">Yes (Min recom. aircraft)</option>
					<option :value="'no'">No</option>
				</selector>
				<div class="columns">
					<div class="column">
						<textbox class="listed listed_h no-radius-top" placeholder="Min Weight (kg)" type="number" v-model="Manifest.MinWeight"></textbox>
					</div>
					<div class="column">
						<textbox class="listed listed_h no-radius-top" placeholder="Max Weight (kg)" type="number" v-model="Manifest.MaxWeight"></textbox>
					</div>
				</div>
			</div>

			<div class="buttons_list_half">
				<selector class="listed" placeholder="Add Manifest" v-model="AddManifestEntry" @input="AddManifest">
					<option v-for="Item in CargoTags" v-bind:key="Item" :value="Item">{{ Item }}</option>
				</selector>
			</div>


			<adventureactions
				placeholder="After transport vehicle is done"
				styleStr="is-black is-subaction border-info"
				:Exclusions="[]"
				:SituationActions="Action.Params.EndActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
			<adventureactions
				placeholder="User has loaded the cargo"
				styleStr="is-black is-subaction border-success"
				:Exclusions="[]"
				:SituationActions="Action.Params.LoadedActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
			<adventureactions
				placeholder="User has forgotten and left the area"
				styleStr="is-black is-subaction border-danger"
				:Exclusions="[]"
				:SituationActions="Action.Params.ForgotActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>

		</div>
		<div v-if="Action.Action == 'cargo_dropoff_2' && CargoTags.length > 0" style="color:#FFF">
			<div v-for="entity in Action.Params.Manifests" v-bind:key="entity.UID" class="buttons_list_half">
				<div v-for="(Manifest, index1) in entity.Manifests" v-bind:key="Manifest.ID" class="buttons_list_half">
					<textbox class="listed" :disabled="true" placeholder="Manifest" type="text" v-if="Available.find(x => x.UID == entity.UID)" :value="Available.find(x => x.UID == entity.UID).Params.Manifests[index1].Tag"></textbox>
					<div class="columns">
						<div class="column">
							<textbox class="listed listed_h no-radius-top" placeholder="Min Delivery (%)" type="number" :min="0" :max="100" v-model="Manifest.MinDeliveryRatio"></textbox>
						</div>
						<div class="column">
							<textbox class="listed listed_h no-radius-top" placeholder="Max Delivery (%)" type="number" :min="0" :max="100" v-model="Manifest.MaxDeliveryRatio"></textbox>
						</div>
					</div>
				</div>
			</div>


			<adventureactions
				placeholder="User has unloaded the cargo"
				styleStr="is-black is-subaction border-success"
				:Exclusions="['cargo_dropoff_2']"
				:SituationActions="Action.Params.UnloadedActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
			<adventureactions
				placeholder="User has forgotten and left the area"
				styleStr="is-black is-subaction border-danger"
				:Exclusions="[]"
				:SituationActions="Action.Params.ForgotActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
			<adventureactions
				placeholder="After transport vehicle is done"
				styleStr="is-black is-subaction border-info"
				:Exclusions="[]"
				:SituationActions="Action.Params.EndActions"
				:app="app"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>

		</div>
		<div v-if="CargoTags.length == 0">
			<p>This action requires the Transponder</p>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import AdventureProjectActionInterface from "../../../interfaces/adventure/action";

export default Vue.extend({
	name: "scenr_Adventure_event_cargo_2",
	props: ["app", "Action", "Situation", "Adventure"],
	beforeCreate() {
		this.$options.components.adventureactions = require("./../actions.vue").default;
	},
	data() {
		return {
			$Adventure: null,
			$Situation: null,
			Available: [] as AdventureProjectActionInterface[],
			CargoTags: [],
			AddManifestEntry: null
		};
	},
	created() {
		this.$Situation = this.Situation;
		this.$Adventure = this.Adventure;

		this.CargoTags = this.app.$data.state.project.cargoTags;
		this.GetAvailable();

		switch(this.Action.Action) {
			case 'cargo_pickup_2': {
				if(!this.Action.Params.Manifests.length) {
					this.Action.Params.Manifests.push(this.GetNewManifest('any'));
					this.app.$emit("action_updated", this.Action);
				}
				break;
			}
			case 'cargo_dropoff_2': {
				['action_updated','action_added','action_removed'].forEach(event => {
					this.app.$on(event, (e: AdventureProjectActionInterface) => {
						if(e.Action == 'cargo_pickup_2') {
							switch(event) {
								case 'action_added': {
									this.Action.Params.Manifests.push({
										ID: e.UID,
										MinDeliveryRatio: 1,
										MaxDeliveryRatio: 100,
									});
									break;
								}
								case 'action_removed': {
									const index = this.Action.Params.Manifests.findIndex(x => x.ID == e.UID);
									this.Action.Params.Manifests.splice(index, 1);
									break;
								}
							}
							this.GetAvailable();
							this.Validate();
						}
					});
				});
				break;
			}
		}

		this.Validate();
	},
	methods: {
		GetNewManifest(tag :string) {
			return {
				UID: Eljs.getNumGUID(),
				Tag: tag,
				MinWeight: 300,
				MaxWeight: 99999,
				AutoBracket: 'recommended_aircraft_min'
			};
		},
		AddManifest() {
			this.Action.Params.Manifests.push(this.GetNewManifest(this.AddManifestEntry));
			setTimeout(() => {
				this.AddManifestEntry = null;
			}, 100);
			this.app.$emit("action_updated", this.Action);
			this.GetAvailable();
		},
		UpdateManifest(index :number = null) {
			if(index !== null && this.Action.Params.Manifests[index].Tag == null) {
				this.Action.Params.Manifests.splice(index, 1);
			}
			this.app.$emit("action_updated", this.Action);
			this.GetAvailable();
		},
		Validate() {
			switch(this.Action.Action) {
				case 'cargo_dropoff_2': {
					const map = this.Adventure.GetActionSituationMaps();
					const currentDropoffIndex = map.find(x => x.ActionID == this.Action.UID);
					const pickups = this.Adventure.Actions.filter(x => x.Action == 'cargo_pickup_2');
					pickups.forEach(pickup => {

						// Loop all configured manifests and remove if not needed
						this.Action.Params.Manifests.forEach((entry, index) => {
							const pickup = pickups.find(x => x.UID == entry.UID);
							if(!pickup) {
								this.Action.Params.Manifests.splice(index, 1);
							} else {
								entry.Manifests.forEach((manifest, index) => {
									const actual = pickup.Params.Manifests.find(x => x.UID == manifest.ID);
									if(!actual) {
										entry.Manifests.splice(index, 1);
									}
								});
							}
						});

						// Skip if later
						const currentPickupIndex = map.find(x => x.ActionID == pickup.UID);
						if(currentDropoffIndex && currentPickupIndex) {
							if(currentDropoffIndex.Sitution <= currentPickupIndex.Sitution) {
								return;
							}
						}

						// Check if already exists, else create it
						let pickupStruct = this.Action.Params.Manifests.find(x => x.UID == pickup.UID);
						if(!pickupStruct) {
							pickupStruct = {
								UID: pickup.UID,
								Manifests: []
							}
							this.Action.Params.Manifests.push(pickupStruct);
						}

						// Loop all actually existing manifests
						pickup.Params.Manifests.forEach(manifest => {
							if(!pickupStruct.Manifests.find(x => x.ID == manifest.UID)) {
								pickupStruct.Manifests.push({
									ID: manifest.UID,
									MinDeliveryRatio: 1,
									MaxDeliveryRatio: 100
								});
							}
						});

					});
					break;
				}
			}
		},
		GetAvailable() {
			this.Available = this.Adventure.GetAvailable(this.Action, "cargo_pickup_2", "cargo_dropoff_2", false);
			//this.Available.forEach((Entity) => {
			//	this.Action.Params.Links[Entity.UID.toString()] = [];
			//	Entity.Params.Models.forEach((Model) => {
			//		this.Action.Params.Links[Entity.UID.toString()].push(false);
			//	})
			//})
		}
	},
	computed: {}
});
</script>
