<template>
	<div class="scroll_stack" :class="{ 'has_round_corners': has_round_corners }">
		<scroll_stack_top ref="scroll_stack_top" :style="context_vars_style" v-if="!!$slots['top']" @mutated="mutated">
			<slot name="top"></slot>
		</scroll_stack_top>
		<div class="scroll_stack_content">
			<scroll_view :sid="sid" ref="scroll_view" :anim_preview="anim_preview" :has_gutter="has_gutter" :offsets="main_scroll_offset" :scroller_offset="scroller_offset" @scroll="content_scroll($event)">
				<slot name="content"></slot>
			</scroll_view>
		</div>
		<scroll_stack_bottom ref="scroll_stack_bottom" v-if="!!$slots['bottom']" @mutated="mutated">
			<slot name="bottom"></slot>
		</scroll_stack_bottom>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "scroll_stack",
	props: ['status_padding', 'sid', 'has_gutter', 'has_round_corners', 'anim_preview', 'scroller_offset'],
	methods: {
		mutated() {
			if(this.$refs.scroll_stack_top)
				this.main_scroll_offset.top = (this.$refs.scroll_stack_top as Vue).$el.clientHeight + 1;

			if(this.$refs.scroll_stack_bottom)
				this.main_scroll_offset.bottom = (this.$refs.scroll_stack_bottom as Vue).$el.clientHeight + 1;

		},
		content_scroll(location: any) {

			this.context_vars_style = {
				'--context-scroll-opacity-out': location.opacityOut,
				'--context-scroll-opacity-in': location.opacityIn,
			}

		},
		scroll_animate_to(x :number, y :number, d :number) {
			(this.$refs.scroll_view as any).scroll_animate_to(x - this.main_scroll_offset.bottom, y - this.main_scroll_offset.top, d);
		}
	},
	mounted() {
		this.content_scroll({
			opacityOut: 1,
			opacityIn: 0
		});
	},
	data() {
		return {
			context_vars_style: {},
			main_scroll_offset: {
				top: 0,
				bottom: 0
			},
		}
	}
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.scroll_stack {
	.scroll-fade-out {
		opacity: var(--context-scroll-opacity-out);
	}
	.scroll-fade-in {
		opacity: var(--context-scroll-opacity-in);
	}

	.controls {
		.theme--bright & {
			&-top {
				border-color: rgba($ui_colors_bright_shade_5, calc(var(--context-scroll-opacity-in) * 0.1));
				background-image: linear-gradient(to top, rgba($ui_colors_bright_shade_1, 0.8), cubic-bezier(.3,0,0,1), rgba($ui_colors_bright_shade_1, 0.95));
			}
			&-bottom {
				border-color: rgba($ui_colors_bright_shade_5, 0.1);
				background-image: linear-gradient(to bottom, rgba($ui_colors_bright_shade_1, 0.8), cubic-bezier(.3,0,0,1), rgba($ui_colors_bright_shade_1, 0.95));
			}
		}
		.theme--dark & {
			&-top {
				border-color: rgba($ui_colors_dark_shade_5, calc(var(--context-scroll-opacity-in) * 0.1));
				background-image: linear-gradient(to top, rgba($ui_colors_dark_shade_1, 0.8), cubic-bezier(.3,0,0,1), rgba($ui_colors_dark_shade_1, 0.95));
			}
			&-bottom {
				border-color: rgba($ui_colors_dark_shade_5, 0.1);
				background-image: linear-gradient(to bottom, rgba($ui_colors_dark_shade_1, 0.8), cubic-bezier(.3,0,0,1), rgba($ui_colors_dark_shade_1, 0.95));
			}
		}
		&-top {
			backdrop-filter: blur(2px);
			border-bottom: 1px solid transparent;
		}
		&-bottom {
			backdrop-filter: blur(2px);
			border-top: 1px solid transparent;
		}
	}

	&.no-blur {
		.controls {
			&-bottom,
			&-top {
				backdrop-filter: none;
			}
		}
	}

	&.has_round_corners {
		.controls {
			&-top {
				border-top-left-radius: 12px;
				border-top-right-radius: 12px;
			}
			&-bottom {
				border-bottom-left-radius: 12px;
				border-bottom-right-radius: 12px;
			}
		}
	}

	.simplebar-content-wrapper,
	.simplebar-wrapper {
		min-height: 100%;
	}
}
</style>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.scroll_stack {
	display: flex;
	position: absolute;
	left: 0;
	right: 0;
	top: 0;
	bottom: 0;
	overflow: hidden;
	&_top {
		z-index: 1;
	}
	&_content {
		display: flex;
		flex-grow: 1;
	}
	& > div {
		width: 100%;
		box-sizing: border-box;
	}

}
</style>