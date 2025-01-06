<template>
	<div class="textbox" :class="{'has-data': value_displayed !== '' || hasFocus, 'has-focus': hasFocus, 'is-uppercase': is_uppercase}">
		<label class="placeholder"><span>{{ placeholder }}</span>&emsp;<span> {{ placeholder_focused }} </span></label>
		<div class="border">
			<input v-if="type == 'number'" :disabled="disabled" :step="step" type="number" v-model.number="value_displayed" @focus="focus" @blur="blur" v-on:keydown.enter.prevent="enter" @input="input"/>
			<input v-else-if="type == 'text'" :disabled="disabled" :maxlength="max" type="text" v-model.trim="value_displayed" @focus="focus" @blur="blur" v-on:keydown.enter.prevent="enter" @input="input"/>
			<textarea v-else-if="type == 'multiline'" :disabled="disabled" :maxlength="max" :rows="rows ? rows : 1" type="text" ref="inputEl" v-model.trim="value_displayed" @focus="focus" @blur="blur" v-on:keydown.enter="enter" @input="input"/>
			<input v-else-if="type == 'date'" :disabled="disabled" type="date" v-model.trim="value_displayed" @focus="focus" @blur="blur" v-on:keydown.enter.prevent="enter" @input="input"/>
			<div class="unit" v-if="unit_displayed">{{ unit_displayed }}</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "textbox",
	props: {
		value: [String, Number, Object],
		decimals: {
			type: Number,
			default: 2
		},
		type: String,
		disabled: Boolean,
		is_uppercase: Boolean,
		placeholder: String,
		placeholder_focused: String,
		input_unit: String,
		min: Number,
		max: Number,
		rows: Number,
		step: Number,
	},
	data() {
		return {
			value_reported: null as string | number | object,
			value_reported_focused: null as string | number | object,
			value_displayed: null as string | number | object,
			value_displayed_focused: null as string | Number | object,
			unit_displayed: '',
			hasFocus: false,
		}
	},
	model: {
		prop: 'value',
		event: 'modified'
	},
	created() {
		this.set_input(this.value);
	},
	mounted() {
		if(this.$refs.inputEl){
			this.resize(this.$refs.inputEl as HTMLElement);
		}
	},
	methods:{
		convert(newVal :number, toKm :Boolean) {
			switch(this.input_unit) {
				case 'lengths': {
					switch(this.$os.userConfig.get(['ui','units','lengths'])) {
						case 'feet': {
							this.unit_displayed = 'ft';
							return toKm ? newVal * 0.0003048 : newVal * 3280.84;
						}
						case 'meters': {
							this.unit_displayed = 'm';
							return toKm ? newVal * 0.001 : newVal * 1000;
						}
					}
					break;
				}
				case 'distances': {
					switch(this.$os.userConfig.get(['ui','units','distances'])) {
						case 'nautical_miles': {
							this.unit_displayed = 'nm';
							return toKm ? newVal * 1.852 : newVal * 0.539957;
						}
						case 'miles': {
							this.unit_displayed = 'mi';
							return toKm ? newVal * 1.60934 : newVal * 0.621371;
						}
						case 'kilometers': {
							this.unit_displayed = 'km';
							return newVal;
						}
					}
					break;
				}
			}
			return newVal;
		},

		set_input(val :any) {

			let newVal = this.value;
			if(newVal !== undefined){
				if(this.is_uppercase) {
					newVal = (newVal as String).toUpperCase();
				}

				switch(this.type) {
					case 'date':{
						this.value_displayed = (newVal as Date).toISOString().substr(0, 10);
						break;
					}
					case 'multiline': {
						this.value_displayed = newVal;
						this.resize(this.$refs.inputEl as HTMLElement);
						break;
					}
					case 'number': {
						this.value_displayed =  Eljs.round(this.convert(newVal, false), this.decimals);
						break;
					}
					default: {
						this.value_displayed =  newVal;
						break;
					}
				}
			}
		},
		focus($ev :any) {
			this.hasFocus = true;
			this.value_displayed_focused = this.value_displayed;
			this.$emit("focus", true);
		},
		blur($ev: any) {
			this.hasFocus = false;

			switch(this.type){
				case 'number':{
					let val = parseFloat($ev.target.value);
					let valKm = this.convert(val, true);
					if(this.min) {
						if(this.min > valKm || $ev.target.value === '') {
							val = this.convert(this.min, false);
							valKm = this.min;
						}
					}
					if(this.max) {
						if(this.max < valKm) {
							val = this.convert(this.max, false);
							valKm = this.max;
						}
					}
					this.value_displayed = Eljs.round(val, this.decimals);
					this.value_reported = Eljs.round(valKm, 5);
					break;
				}
				case 'date':{
					this.value_displayed = new Date($ev.target.value);
					this.value_reported = this.value_displayed;
					break;
				}
				default: {
					this.value_displayed = $ev.target.value;
					this.value_reported = this.value_displayed;
				}
			}

			this.$emit("modified", this.value_reported, this.value_reported_focused);
			this.$emit("changed", this.value_reported, this.value_reported_focused);
			this.$emit("blur", true);
		},
		enter($ev: any) {
			if(!$ev.shiftKey && !$ev.ctrlKey) {
				if(this.type != 'multiline') {
					$ev.target.blur();
				}
				window.requestAnimationFrame(() => {
					this.$emit("returned", this.value, this);
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
				this.set_input(this.value);
			},
			immediate: true,
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.textbox {
	position: relative;
	display: flex;
	font-size: 1em;
	border-radius: 8px;
	overflow: hidden;
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
		border-top-left-radius: 0px !important;
		border-top-right-radius: 0px !important;
		.border {
			border-top-left-radius: 0px !important;
			border-top-right-radius: 0px !important;
		}
	}

	&.no-radius-bottom {
		border-bottom-left-radius: 0px !important;
		border-bottom-right-radius: 0px !important;
		.border {
			border-bottom-left-radius: 0px !important;
			border-bottom-right-radius: 0px !important;
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
			border-top-right-radius: 0;
			border-bottom-right-radius: 0;
			.border {
				border-top-right-radius: 0;
				border-bottom-right-radius: 0;
				margin-right: 1px;
			}
		}
	}
	.column:last-of-type > & {
		&.listed_h {
			border-top-left-radius: 0;
			border-bottom-left-radius: 0;
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