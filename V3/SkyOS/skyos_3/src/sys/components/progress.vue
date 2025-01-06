<template>
	<span class="progress" :class="p_class">
		<div class="label" v-if="label">{{ label }}</div>
		<div class="track">
			<div :style="{ 'width': percent + '%' }"></div>
		</div>
	</span>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	props: {
		label: String,
		percent: Number,
		ranges: Array as () => {
			min: Number,
			max :Number,
			class :String
		}[],
		blink: Array as () => {
			min: Number,
			max :Number,
		}[]
	},
	data() {
		return {
			p_class: [] as String[]
		}
	},
	methods: {
		update_range() {
			this.p_class = [];
			if(this.ranges) {
				this.ranges.forEach(range => {
					if(this.percent >= range.min && this.percent <= range.max){
						this.p_class.push(range.class);
					}
				});
			}
			if(this.blink) {
				this.blink.forEach(blink => {
					if(this.percent >= blink.min && this.percent <= blink.max){
						this.p_class.push('blink');
					}
				});
			}
		}
	},
	mounted() {
		this.update_range();
	},
	watch: {
		percent() {
			this.update_range();
		}
	}
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
	@import '@/sys/scss/helpers.scss';

	.progress {

		.theme--bright & {
			.track {
				& > div {
					background-color: $ui_colors_bright_shade_3;
				}
				&:after {
					border-color: $ui_colors_bright_shade_3;
				}
			}
			&.deep {
				.track {
					& > div {
						background-color: $ui_colors_bright_shade_5;
					}
					&:after {
						border-color: $ui_colors_bright_shade_5;
					}
				}
			}
			&.green {
				.track {
					& > div {
						background-color: $ui_colors_bright_button_go;
					}
					&:after {
						border-color: $ui_colors_bright_button_go;
					}
				}
			}
			&.red {
				.track {
					& > div {
						background-color: $ui_colors_bright_button_cancel;
					}
					&:after {
						border-color: $ui_colors_bright_button_cancel;
					}
				}
			}
		}
		.theme--dark & {
			.track {
				& > div {
					background-color: $ui_colors_dark_shade_3;
				}
				&:after {
					border-color: $ui_colors_dark_shade_3;
				}
			}
			&.deep {
				.track {
					& > div {
						background-color: $ui_colors_dark_shade_5;
					}
					&:after {
						border-color: $ui_colors_dark_shade_5;
					}
				}
			}
			&.green {
				.track {
					& > div {
						background-color: $ui_colors_dark_button_go;
					}
					&:after {
						border-color: $ui_colors_dark_button_go;
					}
				}
			}
			&.red {
				.track {
					& > div {
						background-color: $ui_colors_dark_button_cancel;
					}
					&:after {
						border-color: $ui_colors_dark_button_cancel;
					}
				}
			}
		}

		&.small {
			.track {
				overflow: visible;
				& > div {
					height: 8px;
					border-radius: 8px;
					min-width: 8px;
				}
				&:after {
					border-width: 1px;
					top: 50%;
					height: 0;
					opacity: 0.5;
					margin-top: -1px;
				}
			}
		}

		&.blink {
			.track {
				& > div {
					@keyframes progress-bar-blink {
						0%   { opacity: 1; }
						50%  { opacity: 0.3; }
						100% { opacity: 1; }
					}
					animation-name: progress-bar-blink;
					animation-duration: 1s;
					animation-iteration-count: infinite;
				}
			}
		}

		.track {
			position: relative;
			overflow: hidden;
			border-radius: 8px;
			transition: border-color 0.4s ease-out;
			& > div {
				height: 12px;
				background: transparent;
				transition: width 0.4s ease-out, background-color 0.4s ease-out;
				z-index: 2;
				min-width: 12px;
			}
			&:after {
				content: '';
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				border-radius: 8px;
				border: 2px solid transparent;
				transition: border 0.4s ease-out;
			}
		}
	}

</style>