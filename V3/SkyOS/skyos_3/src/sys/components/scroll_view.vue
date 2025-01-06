<template>
	<div v-if="!is_fixed_scroll" class="scroll_view" :class="[theme, {
		'l': loading && anim_preview,
		'd': show_delay,
		'fixed': is_fixed_scroll,
		'is-has_gutter': has_gutter,
		'is-dynamic': dynamic,
		'mirror_track': mirror_track
		 }]">
		<simplebar class="simplebar" v-if="!is_fixed_scroll" :style="offset_vars_style" ref="simplebar">
			<div :style="scroll_offset_style" ref="host">
				<slot ref="slt"></slot>
			</div>
		</simplebar >
		<div v-else :style="scroll_offset_style" ref="host">
			<slot ref="slt"></slot>
		</div>
	</div>
	<div ref="host" v-else>
		<slot ref="slt"></slot>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import simplebar from 'simplebar-vue';
import 'simplebar-vue/dist/simplebar.min.css';

export default Vue.extend({
	components: {
		simplebar
	},
	props: ['theme', 'sid', 'offsets', 'anim_preview', 'has_gutter', 'dynamic', 'is_fixed_scroll', 'scroller_offset', 'horizontal', 'mirror_track'],
	data() {
		return {
			observer: null,
			scroll_object: null,
			targets: null,
			show_delay: true,
			loading: true,
			lazyload_padding: 100,
			scroll_element: null,
			scroll_opacity_range: 70,
			scroll_offset_style: {},
			offset_vars_style: {},
			validator_interval: null,
			scroll_animate: null,
		}
	},
	mounted() {
		if(this.sid)
			this.$os.scrollView.add(this.sid, (this.$refs.simplebar as any));

		this.$os.eventsBus.Bus.on('scrollview', this.listener_scrollview);

		if(!this.is_fixed_scroll) {
			this.scroll_element = (this.$refs.simplebar as any).scrollElement;
			this.scroll_object = (this.$refs.simplebar as any).SimpleBar;

			this.$emit('scroll_element', this.scroll_element);

			// Cancel scroll animation if mouse enter
			// eslint-disable-next-line
			["mousedown", "touchstart", "wheel"].forEach((t) => {
				this.scroll_element.addEventListener(t, (ev: Event) => {
					clearInterval(this.scroll_animate);
				});
			});

			// Scroll event
			// eslint-disable-next-line
			//let last_emit = new Date();
			this.scroll_element.addEventListener("scroll", (ev: Event) => {

				//const now = new Date();
				//if(now.getTime() - last_emit.getTime() < 200) {
				//	return;
				//}

				if(!this.horizontal) {
					let scrollTop = (ev.target as Element).scrollTop;
					this.set_visible_childs();
					this.$emit('scroll', {
						scrollTop: scrollTop,
						scrollLeft: 0,
						opacityOut: this.easeing_out_cubic(scrollTop > 0 ? (scrollTop < this.scroll_opacity_range ? 1 - (scrollTop/this.scroll_opacity_range) : 0) : 1),
						opacityIn: this.easeing_out_cubic(scrollTop > 0 ? (scrollTop < this.scroll_opacity_range ? (scrollTop/this.scroll_opacity_range) : 1) : 0)
					})
				} else {
					let scrollLeft = (ev.target as Element).scrollLeft;
					this.set_visible_childs();
					this.$emit('scroll', {
						scrollTop: 0,
						scrollLeft: scrollLeft,
						opacityOut: this.easeing_out_cubic(scrollLeft > 0 ? (scrollLeft < this.scroll_opacity_range ? 1 - (scrollLeft/this.scroll_opacity_range) : 0) : 1),
						opacityIn: this.easeing_out_cubic(scrollLeft > 0 ? (scrollLeft < this.scroll_opacity_range ? (scrollLeft/this.scroll_opacity_range) : 1) : 0)
					})
				}

			});

		}

		this.scroll_offset_style = this.calc_offset_style();
		this.offset_vars_style = this.calc_offset_vars_style();

		if(!this.is_fixed_scroll) {
			const el = document.body.getElementsByClassName('simplebar-dummy-scrollbar-size');
			if(el.length){ el[0].remove(); }

			requestAnimationFrame(() => {
				this.loading = false;
				requestAnimationFrame(() => {
					this.start_validator();
				});
			});

			this.start_child_observer();
			window.addEventListener('resize', this.set_visible_childs);
		}

	},
	beforeDestroy() {
		if(this.sid)
			this.$os.scrollView.remove(this.sid);

		this.$os.eventsBus.Bus.off('scrollview', this.listener_scrollview);

		if(!this.is_fixed_scroll) {
			clearTimeout(this.validator_interval);
			this.observer.disconnect();
			window.removeEventListener('resize', this.set_visible_childs);
		}
	},
	deactivated() {
		if(!this.is_fixed_scroll) {
			this.show_delay = true;
			clearTimeout(this.validator_interval);
			this.observer.disconnect();
			window.removeEventListener('resize', this.set_visible_childs);
		}
	},
	activated() {
		if(!this.is_fixed_scroll) {
			this.start_validator();
			this.start_child_observer();
			this.set_visible_childs();
			window.addEventListener('resize', this.set_visible_childs);
		}
	},
	methods: {
		start_child_observer() {
			const observer = new MutationObserver(() => {
				this.update_child_targets();
			});

			observer.observe((this.$refs.host as Node), {
				attributes: true,
				childList: true,
				subtree: true
			});
			this.observer = observer;
		},
		update_child_targets() {
			const parent_el = (((this.$refs.simplebar as any).$el) as HTMLElement);
			const targets = parent_el.querySelectorAll("[data-scrollvisibility]");
			this.targets = targets;
			this.set_visible_childs();
		},
		start_validator() {
			this.validator_interval = setTimeout(() => {
				this.scroll_object.recalculate();
				this.show_delay = false;
			}, 500);
		},
		set_visible_childs() {

			if(!this.$refs.simplebar || !this.$slots.default)
				return;

			const parent_el = (((this.$refs.simplebar as any).$el) as HTMLElement);
			const parent_render = parent_el.getBoundingClientRect();

			if(this.targets) {
				this.targets.forEach((el) => {
					const child_render = el.getBoundingClientRect();
					const top = child_render.top - parent_render.top;
					const left = child_render.left - parent_render.left;

					const child_offset = {
						bottom_limit: parent_render.height - top + this.lazyload_padding,
						right_limit: parent_render.width - left + this.lazyload_padding,
						top_limit: top + child_render.height + this.lazyload_padding,
						left_limit: left + child_render.width - this.lazyload_padding,
					}

					const comp = (el as any).__vue__;
					if(comp) {
						if(comp.$data.scroll_visible !== undefined) {
							if(child_offset.bottom_limit > 0 && child_offset.top_limit > 0 && child_offset.left_limit > 0 && child_offset.right_limit > 0) {
								comp.$data.scroll_visible = true;
							} else {
								comp.$data.scroll_visible = false;
							}
						}
					}
				});
			}

			/*
			this.$slots.default.filter(x => x.elm.nodeName == "DIV").forEach(slot => {
				const el = slot.elm as HTMLElement;
				const child_render = el.getBoundingClientRect();
				const top = child_render.top - parent_render.top;
				const left = child_render.left - parent_render.left;

				const child_offset = {
					bottom_limit: parent_render.height - top + this.lazyload_padding,
					right_limit: parent_render.width - left + this.lazyload_padding,
					top_limit: top + child_render.height - this.lazyload_padding,
					left_limit: left + child_render.width - this.lazyload_padding,
				}

				if(slot.componentInstance) {
					if(slot.componentInstance.$data.scroll_visible !== undefined) {
						if(child_offset.bottom_limit > 0 && child_offset.top_limit > 0 && child_offset.left_limit > 0 && child_offset.right_limit > 0) {
							slot.componentInstance.$data.scroll_visible = true;
						} else {
							slot.componentInstance.$data.scroll_visible = false;
						}
					}
				}
			});
			*/
		},
		easeing_out_cubic(x: number): number {
			return 1 - Math.pow(1 - x, 3);
		},
		easeing_in_cubic(x: number): number {
			return x * x * x;
		},
		scroll_animate_to(x :number, y :number, d :number) {
			const animDuration = d;
			const start = new Date();
			const el = (this.scroll_element as HTMLElement);
			const scroll = [el.scrollLeft, el.scrollTop];
			const deltas = [scroll[0] - x, scroll[1] - y];
			clearInterval(this.scroll_animate);
			this.scroll_animate = setInterval(() => {
				const now = new Date();
				const delta = now.getTime() - start.getTime();
				const progress = this.easeing_out_cubic(delta / animDuration);
				if(progress > 1) {
					clearInterval(this.scroll_animate);
					el.scrollTo(scroll[0] - deltas[0], scroll[1] - deltas[1]);
				} else {
					el.scrollTo(scroll[0] - (deltas[0] * progress), scroll[1] - (deltas[1] * progress));
				}
			}, 16.6666666667);
		},
		calc_offset_style() {
			if(this.offsets) {
				return {
					'padding-top': (this.offsets.top) + 'px',
					'padding-bottom': (this.offsets.bottom) + 'px',
					'padding-left': (this.offsets.left) + 'px',
					'padding-right': (this.offsets.right) + 'px',
				}
			} else {
				return {
					'padding-top': (0) + 'px',
					'padding-bottom': (0) + 'px',
					'padding-left': (0) + 'px',
					'padding-right': (0) + 'px',
				}
			}
		},
		calc_offset_vars_style() {
			let cumul = {
				top: 0,
				bottom: 0,
				left: 0,
				right: 0,
			}

			if(this.scroller_offset) {
				cumul.top += ((this.scroller_offset.top ? this.scroller_offset.top : 0) + 5);
				cumul.bottom += ((this.scroller_offset.bottom ? this.scroller_offset.bottom : 0) + 5);
				cumul.left += ((this.scroller_offset.left ? this.scroller_offset.left : 0) + 5);
				cumul.right += ((this.scroller_offset.right ? this.scroller_offset.right : 0) + 5);
			}

			if(this.offsets) {
				cumul.top += this.offsets.top != undefined ? ((this.offsets.top) + 2) : 0;
				cumul.bottom += this.offsets.top != undefined ? ((this.offsets.bottom) + 2) : 0;
				cumul.left += this.offsets.top != undefined ? ((this.offsets.left) + 2) : 0;
				cumul.right += this.offsets.top != undefined ? ((this.offsets.right) + 2) : 0;
			}

			return {
				'--top-margin': (cumul.top != 0 ? cumul.top : 5) + 'px',
				'--bottom-margin': (cumul.bottom != 0 ? cumul.bottom : 5) + 'px',
				'--left-margin': (cumul.left != 0 ? cumul.left : 5) + 'px',
				'--right-margin': (cumul.right != 0 ? cumul.right : 5) + 'px',
			}
		},

		listener_scrollview(wsmsg: any){
			switch(wsmsg.payload.event){
				case 'set': {
					if(wsmsg.payload.sid == this.sid) {
						this.scroll_animate_to(wsmsg.payload.x, wsmsg.payload.y, wsmsg.payload.duration);
					}
					break;
				}
			}
		}
	},
	watch: {
		'scroller_offset.top'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		},
		'scroller_offset.bottom'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		},
		'scroller_offset.left'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		},
		'scroller_offset.right'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		},
		'offsets.top'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		},
		'offsets.bottom'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		},
		'offsets.left'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		},
		'offsets.right'() {
			this.scroll_offset_style = this.calc_offset_style();
			this.offset_vars_style = this.calc_offset_vars_style();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';

.scroll_view {
	position: absolute;
	left: 0;
	right: 0;
	top: 0;
	bottom: 0;
	display: flex;
	justify-content: stretch;
	scrollbar-width: none;
	::-webkit-scrollbar { display: none; }
	& > div {
		flex-grow: 1;
	}
	&.bg-2 {
		.theme--bright & {
			background: $ui_colors_bright_shade_2;
		}
		.theme--dark & {
			background: $ui_colors_dark_shade_2;
		}
	}
	&.is-dynamic {
		position: relative;
	}
	&.fixed {
		position: relative;
		& > div {
			display: flex;
		}
	}
}
</style>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';

.theme--dark {
	.simplebar {
		&-scrollbar:before {
			background: $ui_colors_bright_shade_0;
		}
	}
}

.theme--bright {
	.simplebar {
		&-scrollbar:before {
			background: $ui_colors_dark_shade_0;
		}
	}
}

.scroll_view {
	& .scroll-fade-out {
		opacity: var(--context-scroll-opacity-out);
	}
	&.is-has_gutter {
		.simplebar {
			&-content-wrapper {
				margin-right: 12px;
			}
		}
	}
	&.l {
		.simplebar-content {
			transform: translateY(-15px);
		}
	}
	&.d {
		.simplebar-track {
			opacity: 0;
		}
	}
	&.mirror_track {
		.simplebar-track {
			&.simplebar-horizontal {
				bottom: unset;
				top: 0;
			}
		}
	}
	.simplebar {
		&-content-wrapper {
			scrollbar-width: none;
			&::-webkit-scrollbar { display: none; }
		}
		&-content {
			//overflow: hidden;
			transition: transform 0.7s cubic-bezier(0,.17,0,1);
			& > div {
				display: inline-block;
				min-width: 100%;
				box-sizing: border-box;
			}
		}
		&-track {
			cursor: pointer;
			transition: opacity 1s cubic-bezier(0,.17,0,1);
			&.simplebar-vertical {
				margin-top: var(--top-margin);
				margin-bottom: var(--bottom-margin);
			}
			&.simplebar-horizontal {
				margin-left: var(--left-margin);
				margin-right: var(--right-margin);
			}
		}
		&.simplebar-scrolling,
		&.simplebar-mouse-entered {
			.simplebar-scrollbar {
				&:before {
					transition: opacity 0.1s cubic-bezier(.54,0,.72,.89);
				}
			}
		}
		.simplebar-scrollbar {
			&:before {
				transition: opacity 2s 2s cubic-bezier(.54,0,.72,.89);
			}
		}
	}
}
</style>