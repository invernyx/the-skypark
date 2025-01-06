<template>
	<div class="changelog" v-if="log">
		<div class="changelog-notes" v-if="log.notes.length">
			<p v-for="(note, index) in log.notes" v-bind:key="index">{{ note }}</p>
		</div>
		<div class="changelog-entries">
			<Entry v-for="(entry, index) in log.entries.slice(0, realLimit)" v-bind:key="index" :entry="entry"/>
			<button_action @click.native="realLimit += 4" v-if="log.entries.length - realLimit > 0">Load more... ({{ log.entries.length - realLimit }} left)</button_action>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue'
import Changes from './changes.json';
import Entry from './entry.vue';
import moment from 'moment';

export default Vue.extend({
	name: "changelog",
	props: ['limit'],
	components: {
		Entry
	},
	data() {
		return {
			log: null,
			realLimit: 1,
		}
	},
	methods: {
	},
	mounted() {

		this.realLimit = this.limit ? this.limit : 1;

		this.log = {
			notes: Changes.notes,
			entries: []
		}

		Changes.entries.forEach((entry, index) => {
			const date = new Date(entry.date);
			if(index == 0) {
				let consoleStr = 'A new update is available!\n';

				if(entry.title) {
					consoleStr += '**' + entry.title + '**\n';
					consoleStr += moment(date).utc().format('YYYY-MM-DD') + '\n';
				} else {
					consoleStr += '**' + moment(date).utc().format('YYYY-MM-DD') + '**\n';
				}
				consoleStr += 'Build ' + moment(date).utc().format('YYYY.w.d.H') + '\n';

				if(entry.notes) {
					if(entry.notes.length) {
						consoleStr += '\n';
						entry.notes.forEach((note) => {
							consoleStr += note + '\n';
						})
					}
				}

				Object.keys(entry.changes).forEach((name, index) => {
					const e = entry.changes[name];
					consoleStr += '\n**' + name.toUpperCase() + '**\n';
					consoleStr += '**' + "¯".repeat(15) + '**\n';
					e.forEach((note) => {
						if(note.title) {
							consoleStr += '> **' + note.title + '**\n';
						}
						note.label.forEach((label) => {
							consoleStr += '> ' + label + '\n';
						})
						consoleStr += '\n';
					})
				});

				consoleStr += '\nTo update to this version, open the Transponder and click "Check for Updates". If you don\'t see the update notification and the version at the top right of the interface is lower than ' + moment(date).utc().format('YYYY.w.d.H') + ', reinstall The Skypark from Orbx Central.'

				console.log(consoleStr);
			}

			this.log.entries.push({
				date: date,
				title: entry.title,
				build: entry.build,
				image: entry.image,
				notes: entry.notes,
				hotfix: entry.hotfix,
				changes: entry.changes
			})
		});

	},
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

.changelog {
	&-notes {
		margin-bottom: 48px;
	}
}
</style>