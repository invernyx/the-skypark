<template>
	<div class="modal modal_hit" :class="[theme, type ? 'type-' + type : '', width ? 'width-' + width : '', height ? 'height-' + height : '']" @mousedown="mouseDown" @mouseup="mouseUp" v-on:keyup.esc="closeEsc">
		<div class="modal_spacer modal_hit">
			<div class="modal_content">
				<slot></slot>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import { NavType, StatusType } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "modal",
	props: ['app', 'theme', 'width', 'height', 'type'],
	methods: {
		mouseDown(e :any) {
			this.state.ui.mouseMoveX = e.offsetX;
			this.state.ui.mouseMoveY = e.offsetY;
		},
		mouseUp(e :any) {
			if(e.offsetX == this.state.ui.mouseMoveX && e.offsetY == this.state.ui.mouseMoveY) {
				const path = Eljs.getDOMParents(e.target) as HTMLElement[];
				if(path[0].classList) {
					if(path[0].classList.contains('modal_hit')) {
						this.$emit("close");
					}
				}
			}
		},
		closeEsc(e :any) {
			if(e.keyCode == 27) { // Escape
				switch(e.target.nodeName) {
					case "TEXTAREA":
					case "INPUT": {
						break;
					}
					default: {
						this.$emit("close");
						break;
					}
				}
			}
		}
	},
	mounted() {
		document.addEventListener('keyup', this.closeEsc);
		if(!this.app) {
			this.$os.setThemeAccent({
				status: {
					bright: StatusType.BRIGHT,
					dark: StatusType.BRIGHT,
					shaded: false,
				},
				nav: {
					bright: StatusType.BRIGHT,
					dark: StatusType.BRIGHT,
					shaded: false,
				}
			})
		} else {
			this.app.SetThemeOverride(this.$root, {
				status: {
					bright: StatusType.BRIGHT,
					dark: StatusType.BRIGHT,
					shaded: false,
				},
				nav: {
					bright: StatusType.BRIGHT,
					dark: StatusType.BRIGHT,
					shaded: false,
				}
			});
		}
	},
	beforeDestroy() {
		document.removeEventListener('keyup', this.closeEsc);
		if(!this.app) {
			this.$os.setThemeAccent(null);
		} else {
			this.app.SetThemeOverride(this.$root, null);
		}
	},
	data() {
		return {
			state: {
				ui: {
					mouseMoveX: 0,
					mouseMoveY: 0,
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
.modal {
	display: flex;
	flex-direction: column;
	position: absolute;
	left: 0;
	right: 0;
	top: 0;
	bottom: 0;
	z-index: 30;
	backdrop-filter: blur(8px);

	.theme--bright &,
	&.theme--bright {
		background: rgba($ui_colors_bright_shade_5, 0.8);
		&.translucent {
			.modal_content {
				background: rgba($ui_colors_bright_shade_1, 0.1);
				border: 1px solid rgba($ui_colors_bright_shade_2, 0.1);
			}
		}
		.modal_content {
			background: $ui_colors_bright_shade_1;
			border: 1px solid $ui_colors_bright_shade_2;
		}
	}

	.theme--dark &,
	&.theme--dark {
		background: rgba($ui_colors_dark_shade_0, 0.6);
		&.translucent {
			.modal_content {
				background: rgba($ui_colors_dark_shade_1, 0.3);
				border: 1px solid rgba($ui_colors_dark_shade_2, 0.3);
			}
		}
		.modal_content {
			background: $ui_colors_dark_shade_1;
			border: 1px solid $ui_colors_dark_shade_2;
		}
	}

	&.type-grow {
		.modal_content {
			flex-grow: 1;
			max-height: 850px;
		}
	}

	&.width-narrow {
		.modal_content {
			width: 320px;
			@media only screen and (max-width: 320px) {
				width: 100%;
				border-radius: 0;
			}
		}
	}

	&.width-normal {
		.modal_content {
			width: 400px;
			@media only screen and (max-width: 400px) {
				width: 100%;
				border-radius: 0;
			}
		}
	}

	&.height-small {
		.modal_content {
		}
	}

	&.transparent {

	}

	& > div {
		&:first-child {
			flex-grow: 1;
		}
		&:last-child {
			flex-grow: 1;
		}
	}

	&_spacer {
		margin-top: $status-size;
		margin-bottom: $nav-size;
		display: flex;
		align-items: center;
		flex-direction: column;
		justify-content: center;
	}

	&_content {
		position: relative;
		border-radius: 14px;
		margin: 0 auto;
		overflow: hidden;
		mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 1) 100%);
		box-shadow: 0px 2px 3px rgba($ui_colors_dark_shade_0, 0.2), 0px 3px 10px rgba($ui_colors_dark_shade_0, 0.2);
	}

}
</style>