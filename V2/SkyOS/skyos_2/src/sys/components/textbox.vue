<template>
	<div class="textbox" :class="[theme, {'has-data': inValue !== '' || hasFocus, 'has-focus': hasFocus, 'is-uppercase': isUppercase}]">
		<label class="placeholder"><span>{{ placeholder }}</span>&emsp;<span> {{ focusPlaceholder }} </span></label>
		<div class="border">
			<input v-if="type == 'number'" :disabled="disabled" :step="step" type="number" v-model.number="inValue" @focus="focus" @blur="blur" v-on:keydown.enter.prevent="enter" @input="input"/>
			<input v-else-if="type == 'text'" :disabled="disabled" :maxlength="max" type="text" v-model.trim="inValue" @focus="focus" @blur="blur" v-on:keydown.enter.prevent="enter" @input="input"/>
			<textarea v-else-if="type == 'multiline'" :disabled="disabled" :maxlength="max" :rows="rows ? rows : 1" type="text" ref="inputEl" v-model.trim="inValue" @focus="focus" @blur="blur" v-on:keydown.enter="enter" @input="input"/>
			<input v-else-if="type == 'date'" :disabled="disabled" type="date" v-model.trim="inValue" @focus="focus" @blur="blur" v-on:keydown.enter.prevent="enter" @input="input"/>
			<div class="unit" v-if="postUnit">{{ postUnit }}</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "textbox",
	props: ['value', 'theme', 'type', 'placeholder', 'focusPlaceholder', 'disabled', 'postUnit', 'min', 'max', 'rows', 'step', 'isUppercase'],
	data() {
		return {
			inValue: null as any,
			focusedValue: null as any,
			hasFocus: false,
		}
	},
	model: {
		prop: 'value',
		event: 'modified'
	},
	created() {
		this.setVal(this.value);

		//setInterval(() => {
		//	console.log(this.value);
		//}, 2000);
	},
	mounted() {
		if(this.$refs.inputEl){
			this.resize(this.$refs.inputEl as HTMLElement);
		}
	},
	methods:{
		setVal(val :any) {
			let newVal = this.value;
			if(newVal != undefined){
				if(this.isUppercase) {
					newVal = newVal.toUpperCase();
				}
				switch(this.type) {
					case 'date':{
						this.inValue = newVal.toISOString().substr(0, 10);
						break;
					}
					case 'multiline': {
						this.inValue = newVal;
						this.resize(this.$refs.inputEl as HTMLElement);
						break;
					}
					default: {
						this.inValue = newVal;
						break;
					}
				}
			}
		},
		focus($ev :any) {
			this.hasFocus = true;
			this.focusedValue = this.inValue;
			this.$emit("focus", true);
		},
		blur($ev: any) {
			this.hasFocus = false;
			switch(this.type){
				case 'number':{
					let val = parseFloat($ev.target.value);
					if(this.min > val || $ev.target.value === '') {
						val = this.min;
					}
					if(this.max < val) {
						val = this.max;
					}
					this.inValue = val;
					break;
				}
				case 'date':{
					this.inValue = new Date($ev.target.value);
					break;
				}
				default: {
					this.inValue = $ev.target.value;
				}
			}
			this.$emit("modified", this.inValue, this.focusedValue);
			this.$emit("changed", this.inValue, this.focusedValue);
			this.$emit("blur", true);
		},
		enter($ev: any) {
			if(!$ev.shiftKey && !$ev.ctrlKey) {
				if(this.type != 'multiline') {
					$ev.target.blur();
				}
				window.requestAnimationFrame(() => {
					this.$emit("returned", this.inValue, this);
				});
			}
		},
		input($ev: any) {
			this.resize($ev.target);
			this.$emit("input", $ev.target.value);
		},
		resize(el :HTMLElement) {
			if(this.$refs.inputEl && this.type == 'multiline') {
				const computedStyle = window.getComputedStyle(el);
				let borderTop = parseInt(computedStyle.getPropertyValue('border-top-width').replace('px', ''));
				let borderBottom = parseInt(computedStyle.getPropertyValue('border-bottom-width').replace('px', ''));
				let paddingTop = parseInt(computedStyle.getPropertyValue('padding-top').replace('px', ''));
				let paddingBottom = parseInt(computedStyle.getPropertyValue('padding-bottom').replace('px', ''));
				el.style.height = 'auto';
				el.style.height = (el.scrollHeight - paddingTop - paddingBottom ) + 'px';
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
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
@import '../scss/mixins.scss';
.textbox {
	position: relative;
	display: flex;
	font-size: 1em;
	$transition: 0.3s cubic-bezier(.25,0,.14,1);

	.theme--bright & {
		.border {
			background: $ui_colors_bright_shade_0;
		}
		textarea,
		input {
			background: $ui_colors_bright_shade_0;
			color: $ui_colors_bright_button_info;
			&:hover {
				background: $ui_colors_bright_shade_0;
			}
			&:disabled {
				color: $ui_colors_bright_shade_5;
				background: rgba($ui_colors_bright_shade_0, 0.7);
			}
		}
		.unit {
			color: $ui_colors_bright_button_info;
		}
		&.has-data {
			.placeholder {
				color: $ui_colors_bright_shade_4;
			}
		}
		&.has-focus {
			.border {
				border-color: $ui_colors_bright_button_info;
			}
		}
	}
	.theme--dark & {
		.border {
			background: $ui_colors_dark_shade_2;
		}
		textarea,
		input {
			background: $ui_colors_dark_shade_2;
			color: $ui_colors_dark_shade_5;
			&:hover {
				background: $ui_colors_dark_shade_2;
			}
			&:disabled {
				color: $ui_colors_dark_shade_5;
				background: rgba($ui_colors_dark_shade_2, 0.7);
			}
		}
		.unit {
			color: $ui_colors_dark_shade_5;
		}
		&.has-data {
			.placeholder {
				color: $ui_colors_dark_shade_3;
			}
		}
		&.has-focus {
			.border {
				border-color: $ui_colors_dark_button_info;
			}
		}
	}

	&.no-radius-top {
		.border {
			border-top-left-radius: 0px !important;
			border-top-right-radius: 0px !important;
		}
	}

	&.no-radius-bottom {
		.border {
			border-bottom-left-radius: 0px !important;
			border-bottom-right-radius: 0px !important;
		}
	}

	&.shadowed {
		.border {
			@include shadowed_shallow(#000);
		}
	}

	.placeholder {
		position: absolute;
		display: flex;
		pointer-events: none;
		padding: 1em 0.6em;
		transform-origin: left;
		white-space: nowrap;
		transition: transform $transition, color $transition;
		& span {
			&:first-child {
				margin-right: 0.3em;
			}
			&:last-child {
				opacity: 0;
				transition: opacity $transition;
				font-family: "SkyOS-SemiBold";
			}
		}
	}

	.border {
		display: flex;
		flex-grow: 1;
		overflow: hidden;
		border: 2px solid transparent;
		border-radius: 8px;
	}

	.unit {
		padding: 1.4em 0.6em 0.3em 0;
		font-size: 1em;
	}

	textarea,
	input {
		display: flex;
		border: none;
		outline: none;
		flex-grow: 1;
		flex-shrink: 1;
		min-width: 0;
		width: 0;
		resize: none;
		cursor: text;
		font-size: 1em;
		white-space: pre-wrap;
		padding: 1.4em 0 0.3em 0.6em;
		appearance: none;
		&:last-child {
			padding-right: 0.3em
		}
	}

	& > * {
		font-size: 1em;
	}

	.column:first-of-type > & {
		&.listed_h {
			.border {
				border-top-right-radius: 0;
				border-bottom-right-radius: 0;
				margin-right: 1px;
			}
		}
	}
	.column:last-of-type > & {
		&.listed_h {
			.border {
				border-top-left-radius: 0;
				border-bottom-left-radius: 0;
			}
		}
	}

	&.listed {
		margin-bottom: 1px;
		.border {
			border-radius: 0;
		}
		&:first-child {
			.border {
				border-top-right-radius: 9px;
				border-top-left-radius: 9px;
			}
		}
		&:last-child {
			margin-bottom: 0;
			.border {
				border-bottom-right-radius: 9px;
				border-bottom-left-radius: 9px;
			}
		}
	}

	&.is-uppercase {
		textarea,
		input {
			text-transform: uppercase;
		}
	}

	&.has-data {
		.placeholder {
			transform: translate(0.2em, -0.6em) scale(0.75);
			& span {
				&:last-child {
					opacity: 1;
				}
			}
		}
	}
}
</style>