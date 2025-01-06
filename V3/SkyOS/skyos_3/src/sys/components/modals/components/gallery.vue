<template>
	<modal type="max" :no_spacer="true" class="black no-blur">
		<transition :duration="{ enter: 1000, leave: 1000 }">
			<div class="background" :key="md.data.images[selected_index]">
				<div class="blur" :style="{ 'background-image': 'url(' + md.data.images[selected_index] + ')' }"></div>
				<div class="image" :style="{ 'background-image': 'url(' + md.data.images[selected_index] + ')' }"></div>
			</div>
		</transition>
		<div class="top-gradient"></div>
		<div class="gallery">
			<div class="header">
				<div class="columns">
					<div class="column">
						<h2 v-if="md.data.title">{{ md.data.title }}</h2>
					</div>
					<div class="column column_narrow">
						<button_action @click.native="close" class="cancel shadowed-deep icon icon-close"></button_action>
					</div>
				</div>
			</div>
			<div class="large">
				<div class="controls" @click="close" v-if="this.md.data.images.length > 1">
					<div class="control control_left shadowed-deep" @click="left"></div>
					<div class="control control_right shadowed-deep" @click="right"></div>
				</div>
				<div class="controls" @click="close" v-else></div>
			</div>
			<div class="thumbs" v-if="this.md.data.images.length > 1">
				<scroll_view :sid="id" :mirror_track="true" :horizontal="true" :scroller_offset="{ left: 10, right: 10 }" :offsets="{ left: 16, right: 16 }">
					<div class="image-frame">
						<div v-for="(image, index) in md.data.images" v-bind:key="index" @click="select(index, $event)">
							<div class="image shadowed-deep" :class="{ 'selected': index == selected_index }" :style="{ 'background-image': 'url(' + image + ')' }" ></div>
						</div>
					</div>
				</scroll_view>
			</div>
		</div>
	</modal>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: ['md'],
	components: {

	},
	mounted() {
		document.addEventListener('keydown', this.keyPressed);
		(this.$el as HTMLElement).focus();

	},
	beforeMount() {
		this.selected_index = this.md.data.selected_index;
	},
	beforeDestroy() {
		document.removeEventListener('keydown', this.keyPressed);
	},
	data() {
		return {
			id: Eljs.genGUID(),
			selected_index: 0,
		}
	},
	methods: {

		left(ev :PointerEvent) {

			if(ev)
				ev.stopPropagation();

			if(this.selected_index == 0) {
				this.selected_index = this.md.data.images.length - 1;
			} else {
				this.selected_index--;
			}
		},

		right(ev :PointerEvent) {

			if(ev)
				ev.stopPropagation();

			if(this.selected_index == this.md.data.images.length - 1) {
				this.selected_index = 0;
			} else {
				this.selected_index++;
			}
		},

		keyPressed(e: KeyboardEvent) {
			switch(e.key) {
				case 'ArrowRight': {
					this.right(null);
					break;
				}
				case 'ArrowLeft': {
					this.left(null);
					break;
				}
				case 'ArrowDown': {
					this.close();
					break;
				}
			}
		},

		select(index :number, ev :PointerEvent) {
			this.selected_index = index;
		},

		close() {
			this.$emit('close');
		},
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.background {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	background: #000;
	.blur {
		display: none;
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		opacity: 0.5;
		filter: blur(10px);
		background-position: center;
		background-size: cover;
		background-repeat: no-repeat;
	}
	.image {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		background-position: center;
		background-size: cover;
		background-repeat: no-repeat;
		transition: background-size 0.3s ease-out;
	}

	&.v-enter {
		transform: scale(1.02);
		opacity: 0;
	}
	&.v-enter-active {
		transition: transform 1s 0.1s cubic-bezier(.37,.87,.5,1), opacity 0.3s 0.1s ease-out;
	}
	&.v-leave,
	&.v-enter-to {
		pointer-events: none;
		opacity: 1;
	}
	&.v-leave-active {
		transition: transform 1s cubic-bezier(.87,.37,1,.5), opacity 0.3s 0.3s ease-out;
	}
	&.v-leave-to {
		transform: scale(1);
		opacity: 0;
	}

	@media (min-aspect-ratio: 16/7) {
		.blur {
			display: block;
		}
		.image {
			background-size: contain;
		}
	}
	@media (max-aspect-ratio: 4/3) {
		.blur {
			display: block;
		}
		.image {
			background-size: contain;
			background-position: center calc(50% - 50px);
		}
	}
}

.top-gradient {
	position: absolute;
	top: 0;
	right: 0;
	left: 0;
	height: 150px;
	background-image: linear-gradient(to bottom, rgba($ui_colors_bright_shade_5, 0.6), cubic-bezier(.54,0,.65,1), rgba($ui_colors_bright_shade_5, 0));
}

.gallery {
	position: absolute;
	top: 0;
	right: 0;
	bottom: 0;
	left: 0;
	display: flex;
	flex-direction: column;

	h2 {
		color: $ui_colors_bright_shade_0;
	}

	.header {
		margin: 16px;
		margin-bottom: 0;
		margin-top: $status-size;
		z-index: 2;
	}
	.large {
		position: relative;
		flex-grow: 1;
		margin-bottom: 8px;
		display: flex;
		justify-content: center;
		align-items: stretch;
		.controls {
			position: relative;
			flex-grow: 1;
			z-index: 3;
			cursor: pointer;
			.control {
				position: absolute;
				top: 50%;
				opacity: 0.3;
				transform: translateY(-50%);
				width: 64px;
				height: 64px;
				margin: 0 30px;
				border-radius: 50%;
				background-color: $ui_colors_bright_shade_4;
				background-size: 17px;
				background-position: center;
				background-repeat: no-repeat;
				will-change: transform;
				transition: transform 1s cubic-bezier(0, 1, 0.15, 1), opacity 0.1s ease-out;
				&:hover {
					opacity: 1;
					transform: translateY(-50%) scale(1.1);
				}
				&_left {
					left: $edge-margin;
					background-image: url(../../../../sys/assets/icons/bright/arrow_left.svg);
				}
				&_right {
					right: $edge-margin;
					background-image: url(../../../../sys/assets/icons/bright/arrow_right.svg);
				}
			}
		}
	}
	.thumbs {
		position: relative;
		height: 95px;
		margin-bottom: $nav-size;
		.image-frame {
			position: relative;
			white-space: nowrap;
			display: flex;
			justify-content: center;
			margin-top: 14px;
			//display: flex;
			//flex-direction: row;
			& > div {
				display: inline-block;
				&:last-child {
					.image {
						margin-right: 0;
					}
				}
				.image {
					width: 100px;
					height: 60px;
					background-size: cover;
					background-position: center;
					border-radius: 8px;
					margin-right: 8px;
					margin-top: 8px;
					margin-bottom: 8px;
					cursor: pointer;
					transition: transform 0.1s cubic-bezier(.3,0,.24,1);
					&:hover {
						transform: scale(1.05);
					}
					&.selected {
						transform: translateY(-6px);
					}
				}
			}
		}
	}

}

</style>