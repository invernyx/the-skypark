<template>
	<div>
		<div v-if="Action.Action == 'cargo_pickup' && Cargo.length > 0">

			<div class="buttons_list_half">
				<textbox class="listed" :disabled="true" placeholder="Unique ID" type="text" :value="Action.UID"></textbox>
				<selector class="listed" placeholder="Cargo Model" v-model="Action.Params.Model" @input="RefreshLinks">
					<option v-for="Item in Cargo" v-bind:key="Item" :value="Item">{{ Item }}</option>
					<option v-if="!Cargo.includes(Action.Params.Model)" :value="Action.Params.Model">{{ Action.Params.Model }}</option>
				</selector>
				<selector class="listed" placeholder="Transport" v-if="Action.Params.Model !== null && $Situation.SituationType !== 'Geo'" v-model="Action.Params.Transport" @input="RefreshLinks">
					<option v-for="Veh in Transport" v-bind:key="Veh.Name" :value="Veh.Name">{{ Veh.Descr }}</option>
					<option v-if="!Transport.find(x => x.Name == Action.Params.Transport)" :value="Action.Params.Transport">{{ Action.Params.Transport }}</option>
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
		<div v-if="Action.Action == 'cargo_dropoff' && Cargo.length > 0">
			<div class="buttons_list_half">
				<selector class="listed" placeholder="What are we dropping?" v-model="Action.Params.Link" @input="RefreshLinks">
					<option
						v-for="Item in Available"
						v-bind:key="Item.UID"
						:value="Item.UID"
					>{{ Cargo.find(x => x === Item.Params.Model) + " | " + Item.UID }}</option>
				</selector>
				<selector class="listed" placeholder="Transport" v-if="Action.Params.Link !== null && $Situation.SituationType !== 'Geo'" v-model="Action.Params.Transport" @input="RefreshLinks">
					<option v-for="Veh in Transport" v-bind:key="Veh.Name" :value="Veh.Name">{{ Veh.Descr }}</option>
					<option v-if="!Transport.find(x => x.Name == Action.Params.Transport)" :value="Action.Params.Transport">{{ Action.Params.Transport }}</option>
				</selector>
			</div>
			<adventureactions
				placeholder="User has unloaded the cargo"
				styleStr="is-black is-subaction border-success"
				:Exclusions="['cargo_dropoff']"
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
		<div v-if="Cargo.length == 0">
			<p>This action requires the Transponder</p>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import AdventureProjectActionInterface from "./../../../interfaces/adventure/action";

export default Vue.extend({
	name: "scenr_Adventure_event_cargo",
	props: ["app", "Action", "Situation", "Adventure"],
	beforeCreate() {
		this.$options.components.adventureactions = require("./../actions.vue").default;
	},
	data() {
		return {
			$Adventure: null,
			$Situation: null,
			Available: [] as AdventureProjectActionInterface[],
			Cargo: [],
			Transport: [
				{
					Name: "TSP_van_white",
					Descr: "Van White"
				},
				{
					Name: "Veh_Car_Mini4_White_sm",
					Descr: "Car Vintage White"
				}
			]
		};
	},
	created() {
		this.$Situation = this.Situation;
		this.$Adventure = this.Adventure;

		this.Cargo = this.app.$data.state.project.cargoTags;

		['action_updated','action_added','action_removed'].forEach(event => {
			this.app.$on(event, (e: any) => {
				if (e !== this.Situation) {
					this.GetAvailable();
				}
			});
		});

		this.GetAvailable();
		this.app.$emit("action_updated", this.Action);
	},
	methods: {
		RefreshLinks() {
			this.app.$emit("action_updated", this.Action);
			this.GetAvailable();
		},
		GetAvailable() {
			this.Available = this.Adventure.GetAvailable(this.Action, "cargo_pickup", "cargo_dropoff");
			if (this.Available.length > 0) {
				if (this.Action.Params.Link === null) {
					this.Action.Params.Link = this.Available[0].UID;
				}
			} else if (this.Action.Params.Link) {
				this.Action.Params.Link = null;
			}
		}
	},
	computed: {}
});
</script>
