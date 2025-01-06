<template>
	<span>{{ output }}</span>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: ["amount", "decimals"],
	beforeMount() {
		this.init();
	},
	methods: {
		init() {
			const unit = this.$os.userConfig.get(['ui','units','lengths']);
			const locale = this.$os.userConfig.get(['ui','units','numbers']);
			const format = new Intl.NumberFormat(locale);
			let amount = this.amount;
			let post = '';

			switch(unit) {
				case 'feet': {
					amount = this.amount * 3.28084;
					post = '\u00A0ft';
					break;
				}
				case 'meters': {
					amount = this.amount;
					post = '\u00A0m';
					break;
				}
			}

			this.output = format.format(Eljs.round(amount, this.decimals)) + post;
		}
	},
	data() {
		return {
			output: this.amount,
		}
	},
	watch: {
		amount: {
			immediate: true,
			handler(newValue, oldValue) {
				this.init();
			}
		}
	}
});
</script>