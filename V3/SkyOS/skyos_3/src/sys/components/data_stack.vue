<template>
	<div class="data-stack" v-if="$slots.default">
		<div class="value"><slot/></div>
		<div class="label">{{ label }}</div>
	</div>
	<div class="data-stack" v-else>
		<div class="value"><slot name="value"></slot></div>
		<div class="label"><slot name="label"></slot></div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	props: ['label', 'value'],
	data() {
		return {

		}
	},
	mounted() {
		console.log();
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';

.data-stack {
	display: flex;
	flex-direction: column;
	.label {
		line-height: 1;
		opacity: 0.7;
		white-space: nowrap;
		margin-top: 0.2em;
	}
	.value {
		font-family: "SkyOS-SemiBold";
		font-size: 21px;
		line-height: 1.2;
	}
	&.end {
		align-items: flex-end;
	}
	&.center {
		align-items: center;
		text-align: center;
	}
	&.text-wrap {
		.value {
			white-space: normal;
		}
	}
	&.small {
		.value {
			font-size: 16px;
			line-height: 1.2;
		}
	}
}

</style>