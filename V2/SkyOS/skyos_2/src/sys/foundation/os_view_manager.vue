<template>
	<transition
		enter-class=""
		enter-active-class=""
		enter-to-class=""
		leave-class=""
		leave-active-class=""
		leave-to-class=""
		v-on:before-enter="transition_beforeEnter"
		v-on:enter="transition_enter"
		v-on:before-leave="transition_beforeLeave"
		v-on:leave="transition_leave"
		v-on:after-leave="transition_afterLeave"
		>
			<keep-alive :max="3" :include="app_cache_list">
				<router-view class="os-app" :inst="inst"/>
			</keep-alive>
		</transition>
</template>

<script lang="ts">
import Vue from "vue";
import { AppType, AppInfo } from "../foundation/app_bundle";

enum Transition {
	NONE,
	FADE,
	ZOOM,
	FORWARD,
	BACKWARDS
}

export default Vue.extend({
	props: ["inst"],

	data() {
		return {
			app_cache_list: [] as string[],

			app_transition_in: null as AppInfo,
			app_transition_out: null as AppInfo,
			app_transition_el_in: null as HTMLElement,
			app_transition_el_out: null as HTMLElement,
			app_transition_frame_el_in: null as HTMLElement,
			app_transition_frame_el_out: null as HTMLElement,
			app_transition_type_in: Transition.NONE,
			app_transition_type_out: Transition.NONE,

			transition_in_class_in: "",
			transition_in_class_pre:"",
			transition_in_class: "",
			transition_in_duraction: 0,

			transition_out_class_in: "",
			transition_out_class_pre:"",
			transition_out_class: "",
			transition_out_duraction: 0,
		};
	},

	methods: {
		// Screenshots: https://html2canvas.hertzen.com/

		setTransitionType() {

			switch(this.app_transition_in.app_type){
				case AppType.SLEEP: { // IN

					switch(this.app_transition_out.app_type){
						case AppType.HOME: { // Out
							this.app_transition_type_in = Transition.NONE;
							this.app_transition_type_out = Transition.ZOOM;
							break;
						}
						default: { // Out
							this.app_transition_type_in = Transition.NONE;
							this.app_transition_type_out = Transition.FADE;
							break;
						}
					}

					break;
				}
				case AppType.LOCKED: { // IN

					switch(this.app_transition_out.app_type){
						case AppType.SLEEP: { // Out
							this.app_transition_type_in = Transition.NONE;
							this.app_transition_type_out = Transition.NONE;
							break;
						}
						default: { // Out
							this.app_transition_type_in = Transition.NONE;
							this.app_transition_type_out = Transition.FADE;
							break;
						}
					}

					break;
				}
				case AppType.HOME: { // IN

					switch(this.app_transition_out.app_type){
						case AppType.LOCKED: // Out
						case AppType.SLEEP: { // Out
							this.app_transition_type_in = Transition.ZOOM;
							this.app_transition_type_out = Transition.NONE;
							break;
						}
						default: { // Out
							this.app_transition_type_in = Transition.ZOOM;
							this.app_transition_type_out = Transition.ZOOM;
							break;
						}
					}

					break;
				}
				default: {

					switch(this.app_transition_out.app_type){
						case AppType.SLEEP: { // Out
							this.app_transition_type_in = Transition.ZOOM;
							this.app_transition_type_out = Transition.NONE;
							break;
						}
						default: { // Out
							this.app_transition_type_in = Transition.ZOOM;
							this.app_transition_type_out = Transition.ZOOM;
							break;
						}
					}

					break;
				}
			}
		},

		// --------
		// ENTERING
		// --------
		transition_beforeEnter(el: HTMLElement) {
			this.app_transition_el_in = el;
			this.app_transition_frame_el_in = el.firstElementChild as HTMLElement;
			this.app_transition_in = this.inst.apps.find((x: AppInfo) => x.app_vendor + "_" + x.app_ident == el.dataset.appname);
		},
		transition_enter(el: HTMLElement, done: Function) {

			switch(this.app_transition_type_in){
				case Transition.NONE: {
					this.transition_in_class_pre = "transition-in-none-pre";
					this.transition_in_class = "transition-in-none";
					this.transition_in_class_in = "";
					this.transition_in_duraction = 1000;
					break;
				}
				case Transition.FADE: {
					this.transition_in_class_pre = "transition-in-fade-pre";
					this.transition_in_class = "transition-in-fade";
					this.transition_in_class_in = "";
					this.transition_in_duraction = 1000;
					break;
				}
				case Transition.ZOOM: {
					this.transition_in_class_pre = "transition-in-zoom-pre";
					this.transition_in_class = "transition-in-zoom";
					this.transition_in_class_in = "is-zoomed-in";
					this.transition_in_duraction = 1000;
					break;
				}
				case Transition.FORWARD: {
					this.transition_in_class_pre = "transition-in-fwd-pre";
					this.transition_in_class = "transition-in-fwd";
					this.transition_in_class_in = "is-zoomed-in";
					this.transition_in_duraction = 1000;
					break;
				}
				case Transition.BACKWARDS: {
					this.transition_in_class_pre = "transition-in-bwd-pre";
					this.transition_in_class = "transition-in-bwd";
					this.transition_in_class_in = "is-zoomed-in";
					this.transition_in_duraction = 1000;
					break;
				}
			}

			if(this.app_transition_frame_el_in && this.transition_in_class_in != ''){
				this.app_transition_frame_el_in.classList.add(this.transition_in_class_in);
			}
			el.classList.add(this.transition_in_class_pre);

			this.app_transition_in.Navigate(this.$root, this.$root.$route.name);

			const l_app_transition_frame_el_in = this.transition_in_class_pre;
			const l_transition_in_class_pre = this.transition_in_class_pre;
			const l_transition_in_class = this.transition_in_class;
			const l_transition_in_class_in = this.transition_in_class_in;

			window.requestAnimationFrame(() => {
				setTimeout(() => {
					el.classList.remove(l_app_transition_frame_el_in);
					el.classList.add("transition");
					el.classList.add(l_transition_in_class);
					if(this.app_transition_frame_el_in && l_transition_in_class_in != ''){
						this.app_transition_frame_el_in.classList.remove(l_transition_in_class_in);
					}
					setTimeout(() => {
						done();
						el.classList.remove("transition");
						el.classList.remove(l_transition_in_class);
						this.app_transition_type_in = Transition.NONE;
					}, this.transition_in_duraction);
				}, 1);
			});
		},
		//v-on:after-enter="transition_afterEnter"
		//transition_afterEnter(el: HTMLElement) { },
		//v-on:enter-cancelled="transition_enterCancelled"
		//transition_enterCancelled(el: HTMLElement) {},



		// --------
		// LEAVING
		// --------
		transition_beforeLeave(el: HTMLElement) {
			this.app_transition_el_out = el;
			this.app_transition_frame_el_out = el.firstElementChild as HTMLElement;
			this.app_transition_out = this.inst.apps.find((x: AppInfo) => x.app_vendor + "_" + x.app_ident == el.dataset.appname);

			this.setTransitionType();
			switch(this.app_transition_type_out){
				case Transition.NONE: {
					this.transition_out_class_pre = "transition-out-none-pre";
					this.transition_out_class = "transition-out-none";
					this.transition_out_class_in = "";
					this.transition_out_duraction = 1000;
					break;
				}
				case Transition.FADE: {
					this.transition_out_class_pre = "transition-out-fade-pre";
					this.transition_out_class = "transition-out-fade";
					this.transition_out_class_in = "";
					this.transition_out_duraction = 1000;
					break;
				}
				case Transition.ZOOM: {
					this.transition_out_class_pre = "transition-out-zoom-pre";
					this.transition_out_class = "transition-out-zoom";
					this.transition_out_class_in = "is-zoomed-out";
					this.transition_out_duraction = 1000;
					break;
				}
				case Transition.FORWARD: {
					this.transition_out_class_pre = "transition-out-fwd-pre";
					this.transition_out_class = "transition-out-fwd";
					this.transition_out_class_in = "is-zoomed-out";
					this.transition_out_duraction = 1000;
					break;
				}
				case Transition.BACKWARDS: {
					this.transition_out_class_pre = "transition-out-bwd-pre";
					this.transition_out_class = "transition-out-bwd";
					this.transition_out_class_in = "is-zoomed-out";
					this.transition_out_duraction = 1000;
					break;
				}
			}
			if(this.transition_out_class_in != '' && this.app_transition_frame_el_out){
				this.app_transition_frame_el_out.classList.add(this.transition_out_class_in);
			}
			el.classList.add(this.transition_out_class_pre);
		},
		transition_leave(el: HTMLElement, done: Function) {
			const l_transition_out_class_in = this.transition_out_class_in;
			const l_transition_out_class = this.transition_out_class;
			const l_transition_out_class_pre = this.transition_out_class_pre;
			setTimeout(() => {
				window.requestAnimationFrame(() => {
					el.classList.remove(l_transition_out_class_pre);
					el.classList.add("transition");
					el.classList.add(l_transition_out_class);
					if(l_transition_out_class_in != '') {
						this.app_transition_frame_el_out.classList.remove(l_transition_out_class_in);
					}
					setTimeout(() => {
						el.classList.remove("transition");
						el.classList.remove(l_transition_out_class);
						this.app_transition_out = null;
						this.app_transition_type_out = Transition.NONE;
						done();
					}, this.transition_out_duraction);
				});
			}, 1)
		},
		transition_afterLeave(el: HTMLElement) {
		},
		//v-on:after-leave="transition_afterLeave"
		//transition_leaveCancelled(el: HTMLElement) { // leaveCancelled only available with v-show }
		//v-on:leave-cancelled="transition_leaveCancelled"


	},

	mounted() {
		const cache_apps = this.inst.apps.filter((x :AppInfo) => x.app_cache);

		cache_apps.forEach((app :AppInfo) => {
			this.app_cache_list.push(app.app_vendor + '_' + app.app_ident);
		});
	}
});
</script>
