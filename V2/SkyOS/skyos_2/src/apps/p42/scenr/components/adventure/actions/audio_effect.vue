<template>
	<div>
		<div v-if="Action.Action == 'audio_effect_play'">
			<div class="buttons_list">
				<textbox class="listed" placeholder="File URI (no ext.)" type="text" v-model="Action.Params.Path" @focus="openSearch" @blur="blurSearch" @input="inputSearch"></textbox>
				<div class="buttons_list search" v-if="OpenSearch">
					<button_listed v-for="(value, index) in Effects.filter(x => x.includes(SearchQuery))" v-bind:key="index" @click.native="selectSearch(value)">{{ value }}</button_listed>
				</div>
				<textbox class="listed" placeholder="Delay (Seconds)" type="number" :min="0" v-model="Action.Params.Delay"></textbox>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import Eljs from '@/sys/libraries/elem';
import AdventureProjectActionInterface from "./../../../interfaces/adventure/action";

export default Vue.extend({
	name: "scenrAdventure_event_audio_effect",
	props: ["app", "Actions", "Action", "Situation", "Adventure"],
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
		}
	},
	data() {
		return {
			Available: [] as AdventureProjectActionInterface[],
			SearchQuery: "",
			OpenSearch: false,
			Effects: [
				"noice",
				"kaching",
				"failed",
				"shutter",
				"throat",
				"test",
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