<template>
	<div class="toggle" :class="{ 'l': !ready, 'd': disabled }">
		<input class="checkbox" type="checkbox" @click="sel" v-model="inValue">
		<div class="label" v-if="hasDefaultSlot" @click="sel">
			<span class="title"><slot></slot></span>
			<p class="notice" v-if="notice">{{ notice }}</p>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "toggle",
	props: ['value', 'disabled', 'notice'],
	data() {
		return {
			ready: false,
			inValue: null as boolean,
		}
	},
	model: {
		event: 'modified'
	},
	created() {
		this.setVal(this.value);
	},
	methods:{
		setVal(val :boolean) {
			if(val !== undefined){
				this.inValue = val;
			}
		},
		sel() {
			this.setVal(!this.inValue);
			this.$emit("modified", this.inValue);
		}
	},
	watch: {
		value() {
			this.setVal(this.value);
		}
	},
	mounted() {
		requestAnimationFrame(() => {
			this.ready = true;
		})
	},
	computed: {
		hasDefaultSlot() {
			const test = this.$slots.default;
			return !!test;
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.toggle {
	display: flex;

	.theme--bright & {
		&.d {
			.checkbox {
				&:checked:before,
				&:before {
					background: $ui_colors_bright_shade_2;
				}

			}
		}
		.checkbox {
			background: $ui_colors_bright_shade_0;
			&:before {
				background: desaturate($ui_colors_bright_button_cancel, 40%);
			}
			&:after {
				background: $ui_colors_bright_shade_0;
			}
			&:checked {
				&:before {
					background: $ui_colors_bright_button_go;
				}
				&:after {
					background: $ui_colors_bright_shade_0;
				}
			}
		}
		&.gold {
			.checkbox {
				&:checked {
					&:before {
						background: $ui_colors_bright_button_gold;
						box-shadow: 0 0 20px $ui_colors_bright_button_gold;
					}
				}
			}
		}
	}

	.theme--dark & {
		&.d {
			.checkbox {
				&:checked:before,
				&:before {
					background: $ui_colors_dark_shade_2;
				}
			}
		}
		.checkbox {
			background: $ui_colors_dark_shade_0;
			&:before {
				background: desaturate($ui_colors_dark_button_cancel, 40%);
			}
			&:after {
				background: $ui_colors_dark_shade_5;
			}
			&:checked {
				&:before {
					background: $ui_colors_dark_button_go;
				}
				&:after {
					background: $ui_colors_dark_shade_5;
				}
			}
		}
		&.gold {
			.checkbox {
				&:checked {
					&:before {
						background: $ui_colors_dark_button_gold;
						box-shadow: 0 0 20px $ui_colors_dark_button_gold;
					}
				}
			}
		}
	}

	&.no-wrap {
		.label {
			white-space: nowrap;
		}
	}

	&.small {
		font-size: 0.75em;
	}

	&.l {
		.checkbox {
			&::before {
				transition: none;
			}
			&:after {
				transition: none;
			}
		}
	}

	&.d {
		.checkbox {
			pointer-events: none;
			&:after {
				opacity: 0.5;
			}
		}
		.label {
			pointer-events: none;
		}
	}

	.checkbox {
		position: relative;
		display: flex;
		align-items: stretch;
		justify-content: center;
		font-size: inherit;
		height: 2em;
		width: 3em;
		min-width: 3em;
		margin: 0;
		appearance: none;
		border: none;
		outline: none;
		border-radius: 8px;
		cursor: pointer;
		&::before {
			display: block;
			position: absolute;
			left: 0;
			top: 0;
			bottom: 0;
			right: 0;
			opacity: 0.8;
			content: '';
			border-radius: 8px;
			transition: background 0.4s ease-out, opacity 0.1s ease-out, box-shadow 0.4s ease-out;
		}
		&:after {
			width: 0.75em;
			display: block;
			margin: 0.3em;
			border-radius: 4px;
			transform: translateX(-0.75em);
			transition: transform 0.1s cubic-bezier(1,0,.8,1.5);
			content: '';
			@include shadowed_shallow(#000);
		}
		&:hover {
			&:before {
				opacity: 1;
			}
		}
		&:checked {
			&:after {
				transform: translateX(0.75em);
			}
		}
	}

	.notice {
		margin-top: 0.5em;
	}

	.title {
		font-family: "SkyOS-Bold";
	}

	.label {
		cursor: pointer;
		align-self: center;
		padding-left: 1em;
	}
}
</style>