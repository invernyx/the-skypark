<template>
	<span>{{ output }}</span>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';

export default Vue.extend({
	props: ["date", 'timeStyle'],
	beforeMount() {
		this.init();
	},
	methods: {
		init() {
			const format = this.$os.userConfig.get(['ui','units','numbers']);

			const t = new Intl.DateTimeFormat(format, {
				timeStyle: this.timeStyle
			}).format(Eljs.convertDateToUTC(this.date)).replace(/([A-Z])\w+/, '').trim();
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