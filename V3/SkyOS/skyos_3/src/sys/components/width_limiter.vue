<template>
	<div class="width_limiter" :class="{ 'shadowed': shadowed }">
		<div class="width_limiter_content" :class="'width_limiter_' + size">
			<slot></slot>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "width_limiter",
	props: ['shadowed', 'size']
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
.width_limiter {
	display: flex;
	position: relative;
	justify-content: center;
	flex-grow: 1;
	&::before {
		content: "";
		display: flex;
		flex-grow: 1;
	}
	&::after {
		content: "";
		display: flex;
		flex-grow: 1;
	}
	&_filler {
		flex-grow: 0.1;
	}
	&_content {
		display: flex;
		flex-direction: column;
		flex-grow: 10000;
	}
	&_phone {
		max-width: 400px;
	}
	&_tablet {
		max-width: 700px;
	}
	&_screen {
		max-width: 920px;
	}
	&_widecreen {
		max-width: 1920px;
	}
}
</style>