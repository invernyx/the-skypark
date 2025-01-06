<template>
	<modal width="narrow" height="small" @close="closeAsk(false)">
		<div class="helper_edge_padding">
			<h2>{{ md.title }}</h2>
			<p v-for="(value, index) in md.text" v-bind:key="index">{{ value }}</p>
		</div>
		<div class="helper_edge_margin_half">
			<div class="columns columns_margined_half">
				<div class="column">
					<button_action @click.native="closeAsk(false)">{{ md.actions ? md.actions.no : 'Cancel' }}</button_action>
				</div>
				<div class="column">
					<button_action @click.native="closeAsk(true)">{{ md.actions ? md.actions.yes : 'Ok' }}</button_action>
				</div>
			</div>
		</div>
	</modal>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "ask",
	props: ['md'],
	methods: {
		closeAsk(state :boolean) {
			this.md.func(state);
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

</style>