<template>
	<div class="contract-status">

	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import Vue from 'vue';

export default Vue.extend({
	props:{
		contract :Contract
	},
	data() {
		return {
		}
	},
	mounted() {
	},
	methods: {
		init() {
		},
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.contract-status {



}
</style>