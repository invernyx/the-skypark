<template>
	<div class="content_controls_stack" :class="[theme]">
		<navigation_bar :theme="theme" :status_padding="status_padding" :transparent="transparent" :translucent="translucent" :shadowed="shadowed" ref="navigation_bar" :style="state.ui.context_vars_style" v-if="!!$slots['nav']">
			<slot name="nav"></slot>
		</navigation_bar>
		<div class="content_controls_stack_content" :style="content_fixed ? { 'padding-top': state.ui.main_scroll_offset.top + 'px' } : ''">
			<scroll_view v-if="!content_fixed" :preview="preview" :theme="theme" :isFixedScroll="content_fixed" :hasGutter="hasGutter" :offsets="state.ui.main_scroll_offset" :scroller_offset="scroller_offset" @scroll="content_scroll($event)">
				<slot name="content"></slot>
			</scroll_view>
			<slot v-else name="content"></slot>
		</div>
		<tab_bar :theme="theme" :nav_padding="nav_padding" :shadowed="shadowed" :transparent="transparent" :translucent="translucent" ref="tab_bar" v-if="!!$slots['tab']">
			<slot name="tab"></slot>
		</tab_bar>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "content_controls_stack",
	props: ['theme', 'status_padding', 'nav_padding', 'hasGutter', 'preview', 'scroller_offset', 'content_fixed', 'translucent', 'transparent', 'shadowed'],
	methods: {
		// eslint-disable-next-line
		content_scroll(location: any) {
			this.state.ui.context_vars_style = {
				'--context-scroll-opacity-out': location.opacityOut,
				'--context-scroll-opacity-in': location.opacityIn,
			}
		}
	},
	mounted() {
		if(this.$refs.navigation_bar) {
			this.state.ui.main_scroll_offset.top = (this.$refs.navigation_bar as Vue).$el.clientHeight + 1;
		}
		if(this.$refs.tab_bar) {
			this.state.ui.main_scroll_offset.bottom = (this.$refs.tab_bar as Vue).$el.clientHeight + 1;
		}
		if(this.content_fixed){
			this.content_scroll({
				opacityOut: 0,
				opacityIn: 1
			});
		} else {
			this.content_scroll({
				opacityOut: 1,
				opacityIn: 0
			});
		}
	},
	data() {
		return {
			state: {
				ui: {
					context_vars_style: {},
					main_scroll_offset: {
						top: 0,
						bottom: 0
					},
				}
			}
		}
	}
});
</script>

<style lang="scss">
.content_controls_stack {
	display: flex;
	position: absolute;
	left: 0;
	right: 0;
	top: 0;
	bottom: 0;
	overflow: hidden;
	&_content {
		display: flex;
		flex-grow: 1;
	}
	& > div {
		width: 100%;
		box-sizing: border-box;
	}
	& .scroll-fade-out {
		opacity: var(--context-scroll-opacity-out);
	}
	& .scroll-fade-in {
		opacity: var(--context-scroll-opacity-in);
	}
}
</style>