<template>
	<section class="tile" :class="'tile_' + (ind + 1) + ' tile_' + app.vendor + '_' + app.ident" v-on:click="open" :data-thook="'home_icon_' + app.vendor + '_' + app.ident">
		<div class="border"></div>
		<div class="background" :style="'background-image: url(' + getIcon() + ');'">
			<div v-for="(c, index) in app.icon_html" v-bind:key="index" :class="c.class" :style="c.styles.join(';')"></div>
		</div>
		<label>{{ app.name }}</label>
	</section>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
	props: ["root", "ind", "app"],
	data() {
		return {
			appIconScript: null,
		};
	},
	methods: {
		open() {
			if(!this.app.isexternal) {
				if(this.app.url != this.$route.path) {
					this.$os.routing.goTo({ path: this.app.url }, true);
				}
			} else {
				window.open(this.app.url, "_blank");
			}
		},
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
			try {
				return require('@/apps/' + this.app.vendor + '/' + this.app.ident + '/icon.svg');
			} catch (e) {
				return require('@/sys/assets/framework/icon_default.svg');
			}
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
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/mixins.scss';

.tile {
	$size: 69px;
	position: absolute;
	height: $size;
	width: $size;
	margin-left: -$size / 2;
	margin-top: -$size / 2;
	cursor: pointer;
	color: $ui_colors_bright_shade_0;
	//will-change: transform;
	transition-property: transform, opacity, top, left, width, height, margin;
	transition-timing-function: cubic-bezier(0, 1, 0.63, 1), cubic-bezier(0, 1, 0.63, 1), ease-out, ease-out, ease-out, ease-out, ease-out;
	transition-duration: 0.2s, 0.2s, 0.2s, 0.2s, 0.2s, 0.2s;
	transform: rotate3d(0, 1, 0, 1deg) scale(1);

	@media only screen and (max-height: 700px) {
		$size: 50px;
		height: $size;
		width: $size;
		margin-left: -$size / 2;
		margin-top: -$size / 2;
	}

	.theme--bright & {
		label {
			color: $ui_colors_bright_shade_5;
			@include shadowed_text($ui_colors_bright_shade_1);
		}
	}
	.theme--sat &,
	.theme--dark & {
		label {
			color: $ui_colors_dark_shade_5;
			@include shadowed_text($ui_colors_dark_shade_1);
		}
	}


	&:hover {
		transform: scale(1.05);
	}
	&:active:hover {
		transform: scale(0.95);
		& .border {
			box-shadow: 0px 2px 15px rgba(0, 0, 0, 0.4), 0px 1px 5px rgba(0, 0, 0, 0.4);
		}
	}
	&.blank {
		background: transparent;
		cursor: default;
		& .border {
			box-shadow: none;
		}
	}
	.background {
		position: absolute;
		left: 0;
		top: 0;
		right: 0;
		bottom: 0;
		z-index: 10;
		transition: all ease-out 0.5s, background 0s;
		transition-timing-function: cubic-bezier(0.5, 0, 0.5, 1);
		background-repeat: no-repeat;
		background-size: 100%;
		background-position: center;
		background-color: #777;
		mask-image: url(../../../sys/assets/framework/icon_mask.svg);
		mask-repeat: no-repeat;
		mask-position: center;
		mask-composite: exclude;
		box-shadow: inset 0px 1px 2px rgba(#FFFFFF, 0.1);
		.full {
			position: absolute;
			left: 0;
			top: 0;
			right: 0;
			bottom: 0;
		}
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
	label {
		position: absolute;
		bottom: 0;
		left: 50%;
		text-align: center;
		padding-top: 20px;
		margin-bottom: -25px;
		transform: translateX(-50%);
		white-space: nowrap;
		font-family: "SkyOS-SemiBold";
		letter-spacing: 0.03em;
		font-size: calc(14px);
	}

	&.tile_1,
	&.tile_2,
	&.tile_3 {
		top: 0%;
	}
	&.tile_4,
	&.tile_5,
	&.tile_6 {
		top: 33.3%;
	}
	&.tile_7,
	&.tile_8,
	&.tile_9 {
		top: 66.6%;
	}
	&.tile_10,
	&.tile_11,
	&.tile_12 {
		top: 100%;
	}

	&.tile_1,
	&.tile_4,
	&.tile_7,
	&.tile_10 {
		left: 0%;
	}
	&.tile_2,
	&.tile_5,
	&.tile_8,
	&.tile_11 {
		left: 50%;
	}
	&.tile_3,
	&.tile_6,
	&.tile_9,
	&.tile_12 {
		left: 100%;
	}
}
</style>