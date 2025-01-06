<template>
	<div class="app-panel"
		ref="panel"
		:class="{
			'is-collapsed': panel_collapsed && has_content,
			'has-content': has_content && loaded,
			'has-subcontent': has_subcontent,
			'can-scroll': can_scroll && has_content
		}"
		:style="{
			'--spacing-top': collapse_height + 'px'
		}">
		<slot/>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		app: AppInfo,
		has_subcontent: Boolean,
		has_content: Boolean,
		scroll_top :Number
	},
	data() {
		return {
			interval: null,
			collapse_height: 0,
			can_scroll: false,
			panel_collapsed: false,
			loaded: false,
		}
	},
	mounted() {
		window.addEventListener('resize', this.resize);
		this.interval = setInterval(this.resize, 300);
		this.init();
	},
	methods: {
		init() {
			this.resize();
			this.make_events();
			this.loaded = true;
		},
		make_events() {
			const panelEl = (this.$refs.panel as HTMLElement);
			['touchstart', 'touchmove', 'mousemove', 'mouseenter'].forEach((ev) => {
				panelEl.addEventListener(ev, this.pointer_events_enter);
			});
		},
		pointer_events_enter(e :Event) {
			const parents = Eljs.getDOMParents(e.target as Node);
			let foundHit = false;
			let index = 0;
			while(!foundHit && index != parents.length - 1) {
				if((parents[index] as HTMLElement).classList.contains('app-panel-hit')){
					foundHit = true;
					break;
				}
				index++;
			}
			this.can_scroll = foundHit;
		},
		resize() {
			let newh = 0;
			const offset = 8;
			const height = (this.$refs.panel as HTMLElement).offsetHeight;
			newh = height - 300;
			this.collapse_height = newh + offset;
		},
		panel_scroll() {
			if(this.has_content) {
				if(this.scroll_top <= 0) {
					if(this.panel_collapsed) {
						this.$os.system.set_cover(false);
						this.resize();
					}
				} else {
					if(!this.panel_collapsed) {
						this.$os.system.set_cover(true);
						this.resize();
					}
				}
			} else {
				this.$os.system.set_cover(false);
			}
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					//if(wsmsg.payload != this.app) {
					//	//this.loaded = false;
					//	//this.init();
					//	this.panel_scroll();
					//}
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'covered': {
					this.panel_collapsed = wsmsg.payload;
					break;
				}
			}
		},
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
		this.panel_collapsed = this.$os.system.map_covered;
	},
	beforeDestroy() {
		clearInterval(this.interval);
		this.loaded = false;
		window.removeEventListener('resize', this.resize);
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
		this.$os.system.set_cover(false);
	},
	watch: {
		scroll_top() {
			if(this.loaded)
				this.panel_scroll();
		}
	}
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

</style>