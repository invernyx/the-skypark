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
			this.output = this.$os.units.ConvertDistance(this.amount, this.decimals);
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