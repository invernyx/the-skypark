<template>
	<div v-if="!isFixedScroll" class="scroll_view" :class="[theme, { 'l': loading && preview, 'd': delayShow, 'fixed': isFixedScroll, 'is-hasGutter': hasGutter, 'is-dynamic': dynamic, 'no-x': !horizontal }]">
		<simplebar class="simplebar" v-if="!isFixedScroll" :style="offsetVarsStyle" ref="simplebar">
			<div :style="scrollOffsetStyle">
				<slot></slot>
			</div>
		</simplebar >
		<div v-else :style="scrollOffsetStyle">
			<slot></slot>
		</div>
	</div>
	<div v-else>
		<slot></slot>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import simplebar from 'simplebar-vue';
import 'simplebar-vue/dist/simplebar.min.css';

export default Vue.extend({
	name: "scroll_view",
	components: {
		simplebar
	},
	props: ['theme', 'offsets', 'preview', 'hasGutter', 'dynamic', 'isFixedScroll', 'scroller_offset', 'horizontal'],
	data() {
		return {
			sbObj: null,
			delayShow: true,
			loading: true,
			scrollElement: null,
			scrollOpacityRange: 70,
			scrollOffsetStyle: {},
			offsetVarsStyle: {},
			validateInterfal: null,
		}
	},
	mounted() {

		if(!this.isFixedScroll) {
			this.scrollElement = (this.$refs.simplebar as any).scrollElement;
			this.sbObj = (this.$refs.simplebar as any).SimpleBar;

			// eslint-disable-next-line
			this.scrollElement.addEventListener("scroll", (ev: Event) => {
				let scrollTop = (ev.target as Element).scrollTop;
				this.$emit('scroll', {
					scrollTop: scrollTop,
					opacityOut: this.easeOutCubic(scrollTop > 0 ? (scrollTop < this.scrollOpacityRange ? 1 - (scrollTop/this.scrollOpacityRange) : 0) : 1),
					opacityIn: this.easeOutCubic(scrollTop > 0 ? (scrollTop < this.scrollOpacityRange ? (scrollTop/this.scrollOpacityRange) : 1) : 0)
				})
			});
		}

		this.scrollOffsetStyle = this.calcOffsetStyle();
		this.offsetVarsStyle = this.calcOffsetVarsStyle();

		if(!this.isFixedScroll) {
			const el = document.body.getElementsByClassName('simplebar-dummy-scrollbar-size');
			if(el.length){ el[0].remove(); }

			requestAnimationFrame(() => {
				this.loading = false;
				requestAnimationFrame(() => {
					this.setValidateInterval();
				});
			});

			//const previewDuration = 1000;
			//const previewHeight = 10;
			//const d = new Date();
			//let p = new Date();
			//const loop = () => {
			//	const nd = new Date();
			//	const pct = (nd.getTime() - d.getTime()) / previewDuration;
			//	const dif = nd.getTime() - d.getTime();
			//	const offset = this.easeOutCubic(pct)
			//	p = nd;
			//	if(pct < 1) {
			//		console.log(previewHeight - ( previewHeight * pct));
			//		this.scrollElement.scrollTo(0, previewHeight - ( previewHeight * pct));
			//		requestAnimationFrame(() => {
			//			loop();
			//		});
			//	} else {
			//		this.scrollElement.scrollTo(0, 0);
			//	}
			//};
			//requestAnimationFrame(() => {
			//	loop();
			//});
		}

	},
	beforeDestroy() {
		if(!this.isFixedScroll) {
			clearTimeout(this.validateInterfal);
		}
	},
	deactivated() {
		if(!this.isFixedScroll) {
			this.delayShow = true;
			clearTimeout(this.validateInterfal);
		}
	},
	activated() {
		if(!this.isFixedScroll) {
			this.setValidateInterval();
		}
	},
	methods: {
		setValidateInterval() {
			this.validateInterfal = setTimeout(() => {
				this.sbObj.recalculate();
				this.delayShow = false;
			}, 500);
		},
		easeOutCubic(x: number): number {
			return 1 - Math.pow(1 - x, 3);
		},
		easeInCubic(x: number): number {
			return x * x * x;
		},
		calcOffsetStyle () {
			if(this.offsets) {
				return {
					'margin-top': (this.offsets.top) + 'px',
					'margin-bottom': (this.offsets.bottom) + 'px',
				}
			} else {
				return {
					'margin-top': (0) + 'px',
					'margin-bottom': (0) + 'px',
				}
			}
		},
		calcOffsetVarsStyle () {
			let cumul = {
				top: 0,
				bottom: 0,
			}

			if(this.scroller_offset) {
				cumul.top += ((this.scroller_offset.top ? this.scroller_offset.top : 0) + 5);
				cumul.bottom += ((this.scroller_offset.bottom ? this.scroller_offset.bottom : 0) + 5);
			}

			if(this.offsets) {
				cumul.top += ((this.offsets.top) + 2);
				cumul.bottom += ((this.offsets.bottom) + 2);
			}

			return {
				'--top-margin': (cumul.top != 0 ? cumul.top : 5) + 'px',
				'--bottom-margin': (cumul.bottom != 0 ? cumul.bottom : 5) + 'px',
			}
		}
	},
	watch: {
		'scroller_offset.top'() {
			this.scrollOffsetStyle = this.calcOffsetStyle();
			this.offsetVarsStyle = this.calcOffsetVarsStyle();
		},
		'scroller_offset.bottom'() {
			this.scrollOffsetStyle = this.calcOffsetStyle();
			this.offsetVarsStyle = this.calcOffsetVarsStyle();
		},
		'offsets.top'() {
			this.scrollOffsetStyle = this.calcOffsetStyle();
			this.offsetVarsStyle = this.calcOffsetVarsStyle();
		},
		'offsets.bottom'() {
			this.scrollOffsetStyle = this.calcOffsetStyle();
			this.offsetVarsStyle = this.calcOffsetVarsStyle();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';

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
@import '../scss/sizes.scss';
@import '../scss/colors.scss';

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
	&.is-hasGutter {
		.simplebar {
			&-content-wrapper {
				margin-right: 12px;
			}
		}
	}
	&.no-x {
		.simplebar-content {
			overflow-x: hidden;
		}
		.simplebar-horizontal {
			display: none;
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
	.simplebar {
		&-content-wrapper {
			scrollbar-width: none;
			&::-webkit-scrollbar { display: none; }
		}
		&-content {
			overflow: hidden;
			transition: transform 0.7s cubic-bezier(0,.17,0,1);
		}
		&-track {
			cursor: pointer;
			margin-top: var(--top-margin);
			margin-bottom: var(--bottom-margin);
			transition: opacity 1s cubic-bezier(0,.17,0,1);
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