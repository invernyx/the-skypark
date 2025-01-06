<template>
	<div>
		<div v-if="Action.Action == 'scene_load'">
			<div class="columns">
				<div class="column">
					<p>
						<strong>{{ Action.UID }}</strong>
					</p>
				</div>
				<div class="column">
					<p>Unique Ident</p>
				</div>
			</div>
			<div class="columns" v-if="Action.Params.Scene !== null">
				<div class="column">
					<p>
						<strong>{{ Action.Params.Scene.Name }}</strong>
					</p>
				</div>
			</div>
			<div class="columns">
				<div class="column">
					<b-button class="is-small is-fullwidth" type="is-bright" @click="Import">
						<span>Import</span>
					</b-button>
				</div>
				<div class="column" v-if="Action.Params.Scene !== null">
					<b-button class="is-small is-fullwidth" type="is-success" @click="Edit">
						<span>Edit Scene</span>
					</b-button>
				</div>
			</div>
			<div class="columns notification is-success">
				<div class="column">
					<adventureactions
						placeholder="Scene has been created"
						styleStr="is-black is-subaction border-success"
						:Exclusions="[]"
						:SituationActions="Action.Params.EnterActions"
						:app="app"
						:Situation="Situation"
						:Adventure="Adventure"
					></adventureactions>
				</div>
			</div>
		</div>
		<div v-if="Action.Action == 'scene_unload'">
			<div class="columns">
				<div class="column">

						<selector
							placeholder="What?"
							size="is-small"
							expanded
							v-model="Action.Params.Link"
							@input="RefreshLinks"
						>
							<option
								v-for="Item in Available"
								v-bind:key="Item.UID"
								:value="Item.UID"
							>{{ Item.Params.Scene.Name + " | " + Item.UID }}</option>
						</selector>

				</div>
			</div>
			<div class="columns notification is-danger">
				<div class="column">
					<adventureactions
						placeholder="Scene got removed"
						styleStr="is-black is-subaction border-danger"
						:Exclusions="[]"
						:SituationActions="Action.Params.ExitActions"
						:app="app"
						:Situation="Situation"
						:Adventure="Adventure"
					></adventureactions>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import AdventureProjectActionInterface from "./../../../interfaces/adventure/action";
//import ScenrProject from "./../../../../components/classes/scenes/project";

export default Vue.extend({
	name: "scenrAdventure_event_scene",
	props: ["app", "Actions", "Action", "Situation", "Adventure"],
	beforeCreate() {
		this.$options.components.adventureactions = require("./../actions.vue").default;
	},
	data() {
		return {
			Available: [] as AdventureProjectActionInterface[]
		};
	},
	created() {
		const self = this;

		//if (typeof self.Action.Params.Scene !== typeof ScenrProject) {
		//	self.Action.Params.Scene = new ScenrProject(
		//		this,
		//		self.Action.Params.Scene
		//	);
		//}

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
			const self = this;

			this.Available = this.Adventure.GetAvailable(
				this.Action,
				"scene_load",
				"scene_unload"
			);
			if (this.Available.length > 0) {
				if (this.Action.Params.Link === null) {
					this.Action.Params.Link = this.Available[0].UID;
				}
			} else if (this.Action.Params.Link) {
				this.Action.Params.Link = null;
			}
		},
		Import() {
			const self = this;
			const dlAnchorElem = document.createElement("input");
			dlAnchorElem.setAttribute("type", "file");
			dlAnchorElem.setAttribute("accept", ".p42scn");
			dlAnchorElem.onchange = (evt: Event) => {
				try {
					const files = (evt.target as HTMLInputElement).files;
					if (!files.length) {
						return;
					}
					const file = files[0];
					const reader = new FileReader();
					reader.onload = (event: any) => {
						const dateFormat = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{0,3}Z$/;

						function reviver(key: any, value: any) {
							if (
								typeof value === "string" &&
								dateFormat.test(value)
							) {
								return new Date(value);
							}
							return value;
						}

						const obj = JSON.parse(
							event.target.result as string,
							reviver
						);

						//self.Action.Params.Scene = new ScenrProject(this, obj);
					};
					reader.readAsText(file);
				} catch (err) {
					console.error(err);
				}
				dlAnchorElem.remove();
			};
			document.body.appendChild(dlAnchorElem);
			dlAnchorElem.click();
		},
		Edit() {
			this.$root.$emit(
				"scenr_adventure_scene_edit",
				this.Action.Params.Scene
			);
		}
	},
	computed: {}
});
</script>