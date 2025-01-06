<template>
	<content_controls_stack :nav_padding="true" :translucent="true" :shadowed="true" class="p42_scenr_startup">
		<template v-slot:nav>
			<div class="helper_edge_margin_left_half">
				<h1>Scenr Tool</h1>
			</div>
		</template>
		<template v-slot:content>
			<div class="helper_edge_padding">
				<div class="buttons_list shadowed">
					<button_listed class="go" @click.native="app.$emit('interaction', { cmd: 'adventure:create' })">New Adventure</button_listed>
					<button_listed class="go" @click.native="app.$emit('interaction', { cmd: 'adventure:open' })">Open Adventure</button_listed>
				</div>

				<div class="buttons_list shadowed">
					<button_listed class="go" @click.native="app.$emit('interaction', { cmd: 'map:images:open' })">Map Image Maker</button_listed>
				</div>

				<div class="buttons_list shadowed">
					<textbox class="listed" type="text" placeholder="Search" v-model="searchQuery" @input="search"></textbox>
				</div>

				<h2>Documents</h2>
				<p class="notice">{{ listedPath }}</p>
				<div class="buttons_list shadowed" v-for="(published, file, index) in searchQueryActive == '' ? adventuresAppdata : Object.keys(adventuresAppdata).filter(key => key.toLowerCase().includes(searchQueryActive.toLowerCase())).reduce((obj, key) => {obj[key] = adventuresAppdata[key];return obj;}, {})" v-bind:key="index">
					<div class="columns columns_2">
						<div class="column column_2 column_h-stretch">
							<button_listed class="listed sharp" :class="{ 'unpublished': !published }" style="white-space: break-spaces;" @click.native="app.$emit('interaction', { cmd: 'adventure:load', file: file, type: 'appdata' })">{{ file }}</button_listed>
						</div>
						<div class="column column_2 column_narrow column_h-stretch">
							<button_action class="cancel listed" @hold="app.$emit('interaction', { cmd: 'adventure:delete', file: file, type: 'appdata', callback: getTemplates })" :hold="true">X</button_action>
						</div>
					</div>
				</div>

				<h2>Integrated</h2>
				<div class="buttons_list shadowed" v-for="(published, file, index) in searchQueryActive == '' ? adventuresIntegrated : Object.keys(adventuresIntegrated).filter(key => key.toLowerCase().includes(searchQueryActive.toLowerCase())).reduce((obj, key) => {obj[key] = adventuresIntegrated[key];return obj;}, {})" v-bind:key="index + 100000">
					<div class="columns columns_2">
						<div class="column column_2 column_h-stretch">
							<button_listed class="go listed sharp" :class="{ 'unpublished': !published }" style="white-space: break-spaces;" @click.native="app.$emit('interaction', { cmd: 'adventure:load', file: file, type: 'integrated' })">{{ file }}</button_listed>
						</div>
						<div class="column column_2 column_narrow column_h-stretch">
							<button_action class="cancel listed" @hold="app.$emit('interaction', { cmd: 'adventure:delete', file: file, type: 'appdata', callback: getTemplates })" :hold="true">X</button_action>
						</div>
					</div>
				</div>

			</div>
		</template>
	</content_controls_stack>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_scenr_startup",
	props: {
		app :Vue,
	},
	data() {
		return {
			searchQuery: "",
			searchQueryActive: "",
			listedPath: "",
			adventuresAppdata: [],
			adventuresIntegrated: []
		}
	},
	mounted() {
		this.getTemplates();
	},
	methods: {
		search(val :string) {
			this.searchQueryActive = val;
		},

		getTemplates() {
			this.$root.$data.services.api.SendWS('scenr:adventuretemplate:list', {}, (list: any) => {
				this.listedPath = list.payload.path;
				this.adventuresAppdata = list.payload.appdata;
				this.adventuresIntegrated = list.payload.integrated;
			});
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							if(wsmsg.payload.persistence) {
								this.getTemplates();
							}
							break;
						}
					}
					break;
				}
				case 'disconnect': {
					this.adventuresAppdata = [];
					this.adventuresIntegrated = [];
					this.listedPath = "";
					break;
				}
			}
		},
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
});
</script>

<style lang="scss">
@import '@/sys/scss/colors.scss';
.p42_scenr_startup {
	.button_listed {
		&.unpublished {
			border-left-color: rgb(255, 44, 44);
		}

	}
}
</style>