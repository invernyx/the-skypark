<template>
	<div class="slider">
		<input type="range" :min="min" :max="max" v-model.number="inValue" @input="setVal($event.target.value)">
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "slider",
	props: ['value', 'min', 'max'],
	data() {
		return {
			inValue: 0,
		}
	},
	model: {
		prop: 'value',
		event: 'modified'
	},
	methods:{
		setVal(val :any) {
			this.inValue = val as number;
			if(this.inValue !== undefined) {
				this.$emit('input', this.inValue);
			}
		}
	},
	watch: {
		value: {
			handler: function (val, oldVal) {
				this.setVal(this.value);
			},
			deep: true,
			immediate: true,
		}
	}
});
</script>

<style lang="scss" scoped>
@import './../../sys/scss/sizes.scss';
@import './../../sys/scss/colors.scss';

.slider {

	.theme--bright &,
	#app &.theme--bright {
		input {
			&:before {
				background: $ui_colors_bright_shade_2;
			}
			&::-webkit-slider-thumb {
				border-color: $ui_colors_bright_shade_2;
				background: $ui_colors_bright_shade_5;
			}
		}
	}
	.theme--dark &,
	#app &.theme--dark {
		input {
			&:before {
				background: $ui_colors_dark_shade_2;
			}
			&::-webkit-slider-thumb {
				border-color: $ui_colors_dark_shade_2;
				background: $ui_colors_dark_shade_5;
			}
		}
	}

	input {
		display: block;
		position: relative;
		font-size: inherit;
		appearance: none;
		width: 100%;
		height: 0.3em;
		margin: 0;
		background: transparent;
		box-sizing: border-box;
		outline: none;
		&:before {
			content: '';
			position: absolute;
			left: 0.3em;
			right: 0.3em;
			border-radius: 0.15em;
			top: 0;
			bottom: 0;
			z-index: 1;
		}
		&:hover {

		}

		&::-webkit-slider-thumb {
			position: relative;
			appearance: none;
			width: 1em;
			height: 1em;
			border-radius: 0.5em;
			border: 1px solid transparent;
			cursor: pointer;
			z-index: 2;
			transition: transform 0.2s cubic-bezier(0,2.01,.57,1);
			&:hover {
				transform: scale(1.4);
			}
		}

		&::-moz-range-thumb {
			position: relative;
			width: 1em;
			height: 1em;
			border-radius: 0.5em;
			border: 1px solid transparent;
			cursor: pointer;
			z-index: 2;
			transition: transform 0.2s cubic-bezier(0,2.01,.57,1);
			&:hover {
				transform: scale(1.4);
			}
		}
	}
}

</style>