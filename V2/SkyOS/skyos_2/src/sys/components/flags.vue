<template>
	<span class="flag flag-icon" v-if="code[0] != '+'" :class="['flag-icon-' + code]"></span>
	<span class="flag flag-icon flag-texted" v-else><span>{{ code }}</span></span>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "flags",
	props: ['code'],
});
</script>

<style lang="scss" scoped>
@import "flags/flag-icon.scss";

.flag-texted {
	display: flex;
	align-items: center;
	justify-content: center;
	&:before {
		display: none;
	}
}
</style>