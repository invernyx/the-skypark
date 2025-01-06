<template>
	<section class="tile" @click="open" v-if="!is_button">
		<div class="border"></div>
		<div class="background" :style="'background-image: url(' + getIcon() + ');'" v-if="app">
			<div v-for="(c, index) in app.icon_html" v-bind:key="index" :class="c.class" :style="c.styles.join(';')"></div>
		</div>
		<div class="background" v-else></div>
	</section>
	<button_listed v-else-if="is_button" class="arrow" :class="{ 'theme--dark': is_dark }" @click.native="open()" :style="{ 'background-color': app.icon_color }">
		<div class="h_stack">
			<app_icon :app="app" class="inline"></app_icon>
			<span class="label" :class="{ 'is_dark': is_dark }">Open {{ app.name }}</span>
		</div>
	</button_listed>
</template>

<script lang="ts">
import { app } from "electron";
import Vue from "vue";
import { AppInfo } from "../foundation/app_model";
import Eljs from "../libraries/elem";

export default Vue.extend({
	props: {
		app: AppInfo,
		is_button: Boolean,
		can_open: Boolean
	},
	data() {
		return {
			shown: false,
			appIconScript: null,
			is_dark: false,
		};
	},
	methods: {
		refresh() {
			if(this.app) {

				const color_hex = Eljs.HEXToRBG(this.app.icon_color);
				this.is_dark = Eljs.isDark(color_hex.r, color_hex.g, color_hex.b);

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
					return require('../../apps/' + this.app.vendor + '/' + this.app.ident + '/icon.svg');
				} catch (e) {
					return require('../assets/framework/icon_default.svg');
				}
			}
		},
		open() {
			this.$os.routing.goTo({ name: this.app.vendor + "_" + this.app.ident });
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
					background-image: url(../assets/icons/bright/itheme.svg);
				}
			}
		}
	}
	.theme--dark & {
		&.type {
			&-theme {
				.background {
					background-color: $ui_colors_dark_shade_5;
					background-image: url(../assets/icons/dark/itheme.svg);
				}
			}
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
		mask-image: url(../assets/framework/icon_mask.svg);
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

.h_stack {
	display: flex;
	flex-direction: row;
	align-items: center;
	.tile {
		margin-right: 10px;
	}
	.label {
		margin-top: -2px;
	}
}

.buttons_list {
	.label {
		color: $ui_colors_bright_shade_0;
		&.is_dark {
			color: $ui_colors_dark_shade_5;
		}
	}
}
</style>