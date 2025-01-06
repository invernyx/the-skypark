<template>
	<section class="tile" @click="open">
		<div class="border"></div>
		<div class="background" :style="'background-image: url(' + getIcon() + ');'" v-if="app">
			<div v-for="(c, index) in app.icon_html" v-bind:key="index" :class="c.class" :style="c.styles.join(';')"></div>
		</div>
		<div class="background" v-else></div>
	</section>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
	props: ["app"],
	data() {
		return {
			shown: false,
			appIconScript: null,
		};
	},
	methods: {
		refresh() {
			if(this.app) {
				if(this.app.icon_method) {
					const md = () => {
						if(this.app.icon_method) {
							this.app.icon_method(this, this.app);
						} else {
							clearInterval(this.appIconScript);
						}
					}
					this.appIconScript = setInterval(md, 1000);
					md();
				}
			}
		},
		getIcon() {
			if(this.app) {
				try {
					return require('../../../apps/' + this.app.vendor + '/' + this.app.ident + '/icon.svg');
				} catch (e) {
					return require('../../../sys/assets/framework/icon_default.svg');
				}
			}
		},
		open() {
			this.$emit("open");
		}
	},
	mounted() {
		this.refresh();
	},
	beforeDestroy() {
		clearInterval(this.appIconScript);
	},
	watch: {
		app() {
			this.refresh();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';
.tile {
	height: 40px;
	width: 40px;
	color: $ui_colors_bright_shade_0;
	will-change: transform;
	cursor: pointer;
	transition-property: transform, opacity;
	transition-timing-function: cubic-bezier(0, 1, 0.63, 1), ease-out;
	transition-duration: 0.7s, 0.2s;
	transform: rotate3d(0, 1, 0, 1deg) scale(1);

	.theme--bright & {
		&.type {
			&-theme {
				.background {
					background-color: $ui_colors_bright_shade_5;
					background-image: url(../../../sys/assets/icons/bright/itheme.svg);
				}
			}
		}
		label {
			color: $ui_colors_bright_shade_0;
		}
	}
	.theme--dark & {
		&.type {
			&-theme {
				.background {
					background-color: $ui_colors_dark_shade_5;
					background-image: url(../../../sys/assets/icons/dark/itheme.svg);
				}
			}
		}
		label {
			color: $ui_colors_dark_shade_5;
		}
	}

	&:hover {
		transform: scale(1.05);
	}

	.background {
		position: absolute;
		left: 0;
		top: 0;
		right: 0;
		bottom: 0;
		z-index: 10;
		background-repeat: no-repeat;
		background-size: 100%;
		background-position: center;
		background-color: #777;
		mask-image: url(../../../sys/assets/framework/icon_mask.svg);
		mask-repeat: no-repeat;
		mask-position: center;
		mask-composite: exclude;
		border-radius: 10px;
		box-shadow: inset 0px 1px 1px rgba(255, 255, 255, 0.2);
	}
	.border {
		position: absolute;
		left: 1px;
		top: 1px;
		right: 1px;
		bottom: 1px;
		border-radius: 25%;
		transition: box-shadow ease-out 0.2s;
		box-shadow: 0px 3px 30px rgba(0, 0, 0, 0.4), 0px 1px 5px rgba(0, 0, 0, 0.4);
	}
}
</style>