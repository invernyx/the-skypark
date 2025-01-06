<template>
	<div class="modal" :class="[theme, type ? 'type-' + type : '', width ? 'width-' + width : '', height ? 'height-' + height : '']" @mousedown="mouseDown" @mouseup="mouseUp" v-on:keyup.esc="closeEsc">
		<div class="modal_hit" :class="{ 'modal_spacer': !no_spacer}">
			<div class="modal_content">
				<slot></slot>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import { NavType, StatusType } from "@/sys/foundation/app_model"

export default Vue.extend({
	name: "modal",
	props: ['app', 'theme', 'width', 'height', 'type', 'no_spacer'],
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
		this.$os.theme.setThemeLayer({
			name: 'modal',
			bright: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false
			},
			dark: {
				status: StatusType.BRIGHT,
				nav: NavType.BRIGHT,
				shaded: false
			}
		})
	},
	beforeDestroy() {
		document.removeEventListener('keyup', this.closeEsc);
		this.$os.theme.setThemeLayer(null, 'modal');
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
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
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
		background: rgba($ui_colors_dark_shade_2, 0.9);
		&.translucent {
			.modal_content {
				background: rgba($ui_colors_dark_shade_1, 0.3);
				border: 1px solid rgba($ui_colors_dark_shade_2, 0.3);
			}
		}
		.modal_content {
			background: $ui_colors_dark_shade_1;
			border: 1px solid rgba($ui_colors_dark_shade_5, 0.1);
		}
	}

	&.type-max {
		.modal_content {
			width: 100%;
			flex-grow: 1;
			border-radius: 0;
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

	& > div {
		&:first-child {
			flex-grow: 1;
		}
		&:last-child {
			flex-grow: 1;
		}
	}

	&_hit {
		display: flex;
		flex-direction: column;
		justify-content: center;
	}

	&_spacer {
		margin-top: $status-size;
		margin-bottom: $nav-size;
		align-items: center;
	}

	&_content {
		position: relative;
		border-radius: 14px;
		margin: 0 auto;
		overflow: hidden;
		box-sizing: border-box;
		mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 1) 100%);
		box-shadow: 0px 2px 3px rgba($ui_colors_dark_shade_0, 0.2), 0px 3px 10px rgba($ui_colors_dark_shade_0, 0.2);
	}

	&.transparent {
		.modal_content {
			background: none;
			border: none;
		}
	}

	&.black {
		.modal_content {
			background: #000;
			border: none;
		}
	}

	&.no-blur {
		backdrop-filter: none;
	}


	&.v-enter {
		opacity: 0;
		.modal_content {
			transform: translateY(10px);
		}
	}
	&.v-enter-active {
		transition: opacity 0.3s 0.1s ease-out;
		.modal_content {
			transition: transform 1s 0.1s cubic-bezier(.37,.87,.5,1);
		}
	}
	&.v-leave,
	&.v-enter-to {
		opacity: 1;
		.modal_content {
			transform: translateY(0px);
		}
	}
	&.v-leave {
		pointer-events: none;
	}
	&.v-leave-active {
		transition: opacity 0.1s ease-out;
		.modal_content {
			transition: transform 0.1s cubic-bezier(.37,.87,.5,1);
		}
		.simplebar-track {
			display: none;
		}
	}
	&.v-leave-to {
		opacity: 0;
		.modal_content {
			transform: translateY(0px);
		}
	}

}
</style>