<template>
	<div class="scroll_stack_bottom">
		<slot></slot>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	data() {
		return {
			observer: null,
		}
	},
	mounted() {
		this.observer = new ResizeObserver(() => {
			this.$emit('mutated')
		});
		this.observer.observe(this.$el, {
			attributes: true,
			childList: true,
			subtree: true
		});
	},
	beforeDestroy() {
		this.observer.disconnect();
		this.$emit('mutated')
	}
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';

.scroll_stack_bottom {
	position: absolute;
	bottom: 0;
	left: 0;
	right: 0;

	.scroll-fade-out {
		transform: translateY(calc((var(--context-scroll-opacity-out) - 1) * 20px));
	}
	.scroll-fade-in {
		transform: translateY(calc(var(--context-scroll-opacity-in) * -10px + 10px));
	}
}
</style>