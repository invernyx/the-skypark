<template>
	<div class="thoughts" v-if="thoughts.length">
		<div v-for="(t, i) in [...thoughts].reverse()" :key="i" class="thought">
			<div class="date">{{ moment(new Date(t[0])).fromNow() }}</div>
			<div class="text">{{ t[1] }}</div>
		</div>
	</div>
	<div v-else>
		<p class="text-center">No Thoughts</p>
	</div>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import moment from 'moment';
import Vue from 'vue';
export default Vue.extend({
	props:{
		thoughts :Array,
	},
	data() {
		return {
			Eljs: Eljs,
			moment: moment
		}
	},
	methods: {
	},
	mounted() {

	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.thoughts {
	.thought {
		margin-bottom: 8px;
		.date {
			opacity: 0.5;
			min-width: 80px;
		}
	}
}

</style>