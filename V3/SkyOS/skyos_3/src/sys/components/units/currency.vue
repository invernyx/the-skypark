<template>
	<span>{{ output }}</span>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	props: ["amount", "decimals"],
	beforeMount() {
		this.init();
	},
	methods: {
		init() {
			const currency = this.$os.userConfig.get(['ui','units','currency']);
			const format = this.$os.userConfig.get(['ui','units','numbers']);

			this.output = new Intl.NumberFormat(format, {
				style: 'currency',
				currency: currency,
				minimumFractionDigits: this.decimals,
				maximumFractionDigits: this.decimals
			}).format(this.amount).replace(/([A-Z])\w+/, '').trim();
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