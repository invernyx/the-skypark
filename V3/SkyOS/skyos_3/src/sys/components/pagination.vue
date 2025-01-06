<template>
	<div class="pagination" v-if="pages > 0">
		<div class="columns">
			<div class="column previous">
				<button_nav class="translucent compact" v-if="current > previous" shape="back" @click.native="go_previous()">{{ previous + 1 }}</button_nav>
				<button_nav class="translucent compact disabled" v-else shape="back">-</button_nav>
			</div>
			<div class="column current">
				{{ current + 1 }}
			</div>
			<div class="column next">
				<button_nav class="translucent compact" v-if="current < next" shape="forward" @click.native="go_next()">{{ next + 1 }}</button_nav>
				<button_nav class="translucent compact disabled" v-else shape="forward">-</button_nav>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	props: {
		qty :Number,
		limit :Number,
		offset :Number,
	},
	data() {
		return {
			pages: 0,
			previous: 0,
			current: 0,
			next: 0,
		}
	},
	methods: {
		update() {
			this.pages = Math.floor((this.qty - 1) / this.limit);

			this.current = Math.floor(this.offset > 0 ? this.offset / this.limit : 0);
			this.previous = this.current > 0 ? this.current - 1 : 0;
			this.next = this.current < this.pages ? this.current + 1 : this.pages;
		},
		go_previous() {
			this.$emit('set_offset', (this.offset - this.limit))
		},
		go_next() {
			this.$emit('set_offset', (this.offset + this.limit))
		}
	},
	mounted() {
		this.update();
	},
	watch: {
		qty: {
			handler: function (val, oldVal) {
				this.update();
			},
		},
		limit: {
			handler: function (val, oldVal) {
				this.update();
			},
		},
		offset: {
			handler: function (val, oldVal) {
				this.update();
			},
		}
	}
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.pagination {
	display: flex;
	justify-content: center;

	.current {
		display: flex;
		justify-content: center;
		margin: 0 10px;
	}

	.button_nav {
		min-width: 40px;
		text-align: center;
	}
}
</style>