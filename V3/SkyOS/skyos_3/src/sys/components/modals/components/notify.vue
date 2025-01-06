<template>
	<modal width="narrow" height="small" @close="closeAsk(false)">
		<div class="h_edge_padding">
			<h2>{{ md.title }}</h2>
			<p v-for="(value, index) in md.text" v-bind:key="index">{{ value }}</p>
		</div>
		<div class="h_edge_margin_half">
			<button_action @click.native="closeAsk(true)">{{ md.actions ? md.actions.ok : 'Ok' }}</button_action>
		</div>
	</modal>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: ['md'],
	methods: {
		closeAsk(state :boolean) {
			if(this.md.func) {
				this.md.func(state);
			}
			this.$emit('close');
		},
	},
	components: {

	},
	mounted() {
	},
	beforeDestroy() {
	},
	data() {
		return {
		}
	}
});
</script>

<style lang="scss" scoped>
	p {
		margin: 0;
		margin-bottom: 0.4em;
	}
</style>