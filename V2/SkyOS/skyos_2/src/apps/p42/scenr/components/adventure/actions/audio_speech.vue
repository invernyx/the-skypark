<template>
	<div>
		<div class="buttons_list_half">
			<selector class="listed" placeholder="Speech Type" expanded v-model="Action.Params.Type">
				<option v-for="C in Types" v-bind:key="C" :value="C">{{ C + ( C == 'adventures' ? ' /' + Adventure.File + '/' : '') }}</option>
			</selector>
			<textbox class="listed" placeholder="Source URL" focusPlaceholder=""  type="text" v-model="Action.Params.URL"></textbox>
			<textbox class="listed" placeholder="File URI (no ext.)" type="text" v-if="Action.Params.Type == 'characters'" v-model="Action.Params.Path" @focus="openSearch" @blur="blurSearch" @input="inputSearch" @returned="pathReturned($event)"></textbox>
			<textbox class="listed" placeholder="FileName (no ext.)" focusPlaceholder=""  type="text" v-if="Action.Params.Type == 'adventures'" v-model="Action.Params.Path" @returned="pathReturned($event)"></textbox>
		</div>
		<div class="buttons_list_half" v-if="OpenSearch && Action.Params.Type == 'characters'">
			<button_listed v-for="(value, index) in Characters.filter(x => x.includes(SearchQuery))" v-bind:key="index" @click.native="selectSearch(value)">{{ value }}</button_listed>
		</div>
		<div class="buttons_list_half">
			<div class="columns columns_margined_half">
				<div class="column">
					<textbox class="listed" placeholder="Name" type="text" v-model="Action.Params.Name" @input="nameChanged($event)"></textbox>
				</div>
				<div class="column">
					<textbox class="listed" :disabled="true" placeholder="Avatar ID" type="text" v-model="Action.Params.NameID"></textbox>
				</div>
			</div>
		</div>
		<div class="buttons_list_half">
			<button_action class="info" @click.native="playAudio">Play</button_action>
		</div>
		<div class="buttons_list_half">
			<textbox class="listed" type="multiline" placeholder="Transcript (English)" v-model="Action.Params.Transcript"></textbox>
		</div>
		<div class="buttons_list_half">
			<textbox class="listed" placeholder="Delay (Seconds)" type="number" :min="0" v-model="Action.Params.Delay"></textbox>
		</div>
		<adventureactions
			placeholder="Done Playing"
			styleStr="is-black is-subaction border-success"
			:Exclusions="[]"
			:SituationActions="Action.Params.DonePlayingActions"
			:app="app"
			:Situation="Situation"
			:Adventure="Adventure"
		></adventureactions>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import AdventureProjectActionInterface from "./../../../interfaces/adventure/action";

export default Vue.extend({
	name: "scenrAdventure_event_audio_speech",
	props: ["app", "Actions", "Action", "Situation", "Adventure"],
	beforeCreate() {
		this.$options.components.adventureactions = require("./../actions.vue").default;
	},
	beforeMount() {
		if(!this.Action.Params.DonePlayingActions) {
			this.Action.Params.DonePlayingActions = [];
		}
	},
	methods: {
		openSearch() {
			this.OpenSearch = true;
			this.SearchQuery = this.Action.Params.Path;
		},
		blurSearch() {
			setTimeout(() => {
				this.OpenSearch = false;
			}, 500);
		},
		inputSearch(ev :any) {
			this.SearchQuery = ev;
		},
		selectSearch(ev :any) {
			this.Action.Params.Path = ev;
			this.OpenSearch = false;
		},
		playAudio() {
			let path = this.Action.Params.Type;
			if(this.Action.Params.Type == 'adventures') {
				path += ":" + this.Adventure.File + '/' + this.Action.Params.Path;
			}
			this.$root.$data.services.api.SendWS('speech:play', {
				param: path
			});
		},
		nameChanged(text :string) {
			this.Action.Params.NameID = text.replace(/[^A-Za-z0-9]/g, '_').toLowerCase();
		},
		pathReturned(text :string) {
			this.Action.Params.Path = this.Action.Params.Path.toLowerCase();
		}
	},
	data() {
		return {
			Available: [] as AdventureProjectActionInterface[],
			Type: "adventure",
			Types: [
				"adventures",
				"characters",
			],
			SearchQuery: "",
			OpenSearch: false,
			Characters: [
				"brigit/good_day",
				"larry/loaded",
			]
		};
	},
});
</script>

<style lang="scss" scoped>
.search {
	max-height: 200px;
	overflow-y: scroll;
}
</style>