<template>
	<div class="selector" :class="[{ 'has-data': inValue || inValue === false }]">
		<label class="placeholder">{{ placeholder }}</label>
		<select placeholder="placeholder" v-model="inValue" @change="sel($event)" >
			<slot></slot>
		</select>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "selector",
	props: ['value', "placeholder"],
	data() {
		return {
			inValue: "",
		}
	},
	model: {
		event: 'modified'
	},
	created() {
		this.setVal(this.value);
	},
	methods:{
		setVal(val :string) {
			if(val !== undefined){
				this.inValue = val;
			}
		},
		sel(selected :string) {
			this.$emit("modified", this.inValue);
			this.$emit("input", this.inValue);
		}
	},
	watch: {
		value() {
			this.setVal(this.value);
		}
	},
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
@import '../scss/mixins.scss';
.selector {
	display: flex;
	$transition: 0.3s cubic-bezier(.25,0,.14,1);

	.theme--bright & {
		select {
			background: $ui_colors_bright_shade_0;
			color: $ui_colors_bright_button_info;
			&:focus:active {
				border-color: $ui_colors_bright_button_info;
			}
		}
	}
	.theme--dark & {
		select {
			background: $ui_colors_dark_shade_2;
			color: $ui_colors_dark_shade_5;
			&:focus:active {
				border-color: $ui_colors_dark_button_info;
			}
		}
	}

	&.compact {
		display: flex;
		align-items: center;
		label {
			padding: 0.5em;
		}
		select {
			padding: 0px;
		}
		&.has-data {
			.placeholder {
				display: none;
			}
		}
	}

	&.shadowed {
		select {
			@include shadowed_shallow(#000);
		}
	}

	* {
		font-size: 1em;
		line-height: 1.4em;
	}

	.placeholder {
		position: absolute;
		display: flex;
		pointer-events: none;
		padding: 1em 0.6em;
		transform-origin: left;
		transition: transform $transition, color $transition;
	}

	select {
		display: flex;
		border: none;
		outline: none;
		font-size: 1em;
		flex-grow: 1;
		flex-shrink: 1;
		border-radius: 8px;
		min-width: 0;
		width: 0;
		border: 2px solid transparent;
		cursor: pointer;
		padding: 1.4em 0.4em 0.3em 0.3em;
	}

	&.listed {
		margin-bottom: 1px;
		select {
			border-radius: 0;
		}
		&:first-child {
			select {
				border-top-right-radius: 9px;
				border-top-left-radius: 9px;
			}
		}
		&:last-child {
			margin-bottom: 0;
			select {
				border-bottom-right-radius: 9px;
				border-bottom-left-radius: 9px;
			}
		}
	}

	&.has-data {
		.placeholder {
			transform: translate(0.2em, -0.6em) scale(0.75);
		}
	}

}
</style>