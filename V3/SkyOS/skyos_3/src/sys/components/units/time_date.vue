<template>
	<span>{{ output }}</span>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	props: ["date", 'dateStyle', 'timeStyle'],
	beforeMount() {
		this.init();
	},
	methods: {
		init() {
			const format = this.$os.userConfig.get(['ui','units','numbers']);

			this.output = new Intl.DateTimeFormat(format, {
				dateStyle: this.dateStyle,
				timeStyle: this.timeStyle
			}).format(this.date).replace(/([A-Z])\w+/, '').trim();
		}
	},
	data() {
		return {
			output: this.date,
		}
	},
	watch: {
		date: {
			immediate: true,
			handler(newValue, oldValue) {
				this.init();
			}
		}
	}
});
</script>