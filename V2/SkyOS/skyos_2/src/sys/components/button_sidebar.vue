<template>
	<div class="button_sidebar" :class="[theme]">
		<span class="icon" v-if="icon" :style="'background-image: url(' + getIcon() + ')'"></span>
		<span class="label"><slot></slot></span>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "button_sidebar",
	props: ['theme', 'icon'],
	methods: {
		getIcon() {
			let fn = this.icon;
			if((this.icon as string).startsWith('theme/')) {
				if(this.theme ? this.theme.includes('theme--') : false) {
					if(this.$el.classList.contains('theme--dark')) {
						fn = (this.icon as string).replace('theme/', 'dark/');
					} else {
						fn = (this.icon as string).replace('theme/', 'bright/');
					}
				} else {
					fn = (this.icon as string).replace('theme/', this.$os.getConfig(['ui','theme']) == 'theme--bright' ? 'dark/' : 'bright/');
				}
			}

			try {

				return require('@/sys/assets/icons/' + fn + '.svg');
			} catch (e) {
				return require('@/sys/assets/icons/state_mask_alert.svg');
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
@import '../scss/mixins.scss';
.button_sidebar {
	display: flex;
	align-items: center;
	position: relative;
	padding: 0.5em 0.7em;
	margin-left: -$edge-margin / 2;
	margin-right: -$edge-margin / 2;
	margin-bottom: 2px;
	border-radius: 8px;
	cursor: pointer;
	overflow: hidden;
	font-family: "SkyOS-SemiBold";
	transition: background 0.3s ease-out;
	&:hover {
		z-index: 2;
		transition: background 0.1s ease-out;
	}

	@mixin bright() {
		&:hover {
			background-color: rgba($ui_colors_bright_shade_2, 0.3);
		}
	}

	@mixin dark() {
		&:hover {
			background-color: rgba($ui_colors_dark_shade_2, 0.3);
		}
	}

	@mixin active_bright() {
		background-color: $ui_colors_bright_shade_2;
	}

	@mixin active_dark() {
		background-color: $ui_colors_dark_shade_2;
	}

	.icon {
		// https://thenounproject.com/prosymbols/collection/set-of-line-essentials-icons/
		width: 1.5em;
		min-width: 1.5em;
		height: 1.5em;
		margin-top: -0.5em;
		margin-bottom: -0.5em;
		margin-right: 0.4em;
		margin-left: -0.2em;
		background-repeat: no-repeat;
		background-size: contain;
	}

	.theme--bright &,
	#app .theme--bright & {
		@include bright;
		&.is-active {
		 	@include active_bright;
		}
	}
	.theme--dark &,
	#app .theme--dark & {
		@include dark;
		&.is-active {
		 	@include active_dark;
		}
	}

	.is-active > & {
		.theme--bright &,
		#app .theme--bright & {
		 	@include active_bright;
		}
		.theme--dark &,
		#app .theme--dark & {
		 	@include active_dark;
		}
	}

	.split_view_strip & {
		font-size: 16px;
		.icon {
			margin-right: 0;
		}
		.label {
			opacity: 0;
			white-space: nowrap;
		}
	}

}
</style>