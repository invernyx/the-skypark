<template>
	<div id="app" :class="{ 'task-switch': true, 'app-expand': state.ui.app_expand, 'theme--sat': state.ui.is_sat, 'map-covered': state.ui.covered }" @mousemove="tap">

		<ViewManager :root="$root"/>

		<ActionPreviews />

		<Modals/>

		<Tutorials v-if="$root.$data.state.ui.tutorials" @done="tutorials_done"/>

		<Notifications :isOpen="state.ui.notifications.tray" :isShaded="state.ui.theme.statusShaded" :theme="state.ui.theme.status" @close="state.ui.notifications.tray = false" />

		<StatusBar :theme="state.ui.theme" @toggle="state.ui.notifications.tray = !state.ui.notifications.tray" />

		<CallScreen />

		<AppSwitcher />

		<!--<section class="os-nav" :class="state.ui.theme.nav">
			<div class="os-nav-icon os-nav_task-switch" @mouseenter="task_enter" @mouseleave="task_leave" @click="task_click"/>
			<div class="os-nav-icon os-nav_home" :class="{ 'disabled': state.ui.is_home }" @mousedown="home_hold_start" @mouseleave="home_hold_cancel" @mouseup="home_hold_cancel" @click="home_click"/>
		</section>-->

		<Boot :ready="state.ui.ready"/>

	</div>
</template>

<style lang="sass">
  @import '@/sys/scss/base.scss'
</style>

<script lang="ts">
import Vue from "vue";
import Boot from "./os_boot.vue"
import ViewManager from "./os_view_manager.vue"
import StatusBar from "@/sys/components/status_bar/base.vue"
import { StatusType, NavType, AppType, AppInfo } from "./app_model"
import Apps from "./app_bundle"

export default Vue.extend({
	components: {
		ViewManager,
		Boot,
		StatusBar,
		CallScreen: () => import("@/sys/components/messaging/call_screen.vue"),
		ActionPreviews: () => import("@/sys/components/action_previews/action_previews.vue"),
		Modals: () => import("@/sys/components/modals/modals.vue"),
		AppSwitcher: () => import("@/sys/components/switcher/app_switcher.vue"),
		Changelog: () => import("@/sys/components/changelog/log.vue"),
		Tutorials: () => import("@/sys/components/tutorials/overlay.vue"),
		Notifications: () => import("@/sys/components/notifications/os_notifications.vue"),
	},

	data() {
		return {
			state: {
				ui: {
					covered: false,
					ready: false,
					task_switcher: false,
					task_switcher_time: new Date(),
					task_switcher_timeout: null,
					app_expand: true,
					is_home: false,
					is_sat: false,
					theme: {
						nav: null as string,
						status: null as string,
						statusShaded: false,
					},
					notifications: {
						tray: false,
					},
					sleep: {
						timer: null,
					}
				}
			},
			hold_hold_timeout: null,
		}
	},

	beforeMount() {
		this.$os.routing.initAppHistory();
		this.set_theme();
		this.$os.eventsBus.Bus.on('notif-svc', this.listener_notif);

		// Initiate Sleep Timer
		this.$router.afterEach((to, from) => {
			const app = Apps.find(x => to.name.startsWith(x.vendor + "_" + x.ident));
			this.state.ui.task_switcher = false;
			this.$os.routing.setCurrentRoute(app, to);
			this.set_theme();
			this.resetsleep_timer();
			this.state.ui.notifications.tray = false;
			this.home_check(app);
		});

		this.$os.eventsBus.Bus.on('os', this.listener_os);

		this.$os.eventsBus.Bus.on('configchange', (path) => {
			switch(path[0]){
				case 'ui': {
					switch(path[1]){
						case 'theme': {
							this.set_theme();
							break;
						}
					}
					break;
				}
			}
		});

		this.$os.eventsBus.Bus.on('map', (path) => {
			switch(path.name){
				case 'loaded': {
					this.state.ui.ready = true;
					break;
				}
			}
		});

		this.$os.eventsBus.Bus.emit('ws-in', { name: ['disconnect'] });

	},

	mounted() {
		this.home_check(this.$os.routing.activeApp);
	},

	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
	},

	methods: {
		home_check(app :AppInfo) {
			if(app){
				this.state.ui.is_home = app.vendor + "_" + app.ident == 'p42_home';
			} else {
				return;
			}
		},

		home_hold_start() {
			this.hold_hold_timeout = setTimeout(() => {
				clearTimeout(this.hold_hold_timeout);
				const appToReset = this.$os.routing.activeApp;
				this.$os.routing.goTo({name: 'p42_home'});
				localStorage.removeItem('store_' + appToReset.vendor + '_' + appToReset.ident);
				appToReset.loaded_state = null;
			}, 5000)
		},
		home_hold_cancel() {
			clearTimeout(this.hold_hold_timeout);
		},
		home_click() {
			if(this.$os.modals.queue.length) {
				this.$os.modals.close();
			} else {
				const history = this.$os.routing.getAppAllHistory().filter(x => x.type != AppType.SLEEP && x.type != AppType.LOCKED);
				if(history.length > 1 && history[history.length - 1] != this.$os.routing.activeApp) {
					const lastApp = history[history.length - 1];
					this.$os.routing.goTo({ name: lastApp.vendor + "_" + lastApp.ident }, true);
				} else {
					this.$os.routing.goTo({ name: 'p42_home' });
				}
			}
		},

		tutorials_done() {
			this.$root.$data.state.ui.tutorials = false;
		},

		set_theme() {

			let pointerNav = NavType.NONE;
			let pointerStatus = StatusType.NONE;
			let pointerStatusShaded = false;

			this.state.ui.is_sat = this.$os.maps.display_layer.sat;

			if(this.$os.routing.activeApp) {

				const theme = this.$os.userConfig.get(['ui','theme']);
				const components = this.$os.theme.getTheme();
				pointerNav = components.nav;
				pointerStatus = components.status;
				pointerStatusShaded = components.shaded;

				// Override if Task switcher
				if(this.state.ui.task_switcher) {
					pointerNav = NavType.BRIGHT;
				}

				// Override if Covered
				if(this.state.ui.covered && theme == 'theme--bright') {
					pointerNav = NavType.DARK;
					pointerStatus = StatusType.DARK;
				}

				// Set Classes
				switch(pointerNav) {
					case NavType.NONE: {
						this.state.ui.theme.nav = 'is-invisible';
						break;
					}
					case NavType.BRIGHT: {
						this.state.ui.theme.nav = 'is-bright';
						break;
					}
					case NavType.DARK: {
						this.state.ui.theme.nav = 'is-dark';
						break;
					}
				}
				switch(pointerStatus) {
					case StatusType.NONE: {
						this.state.ui.theme.status = 'is-invisible';
						break;
					}
					case StatusType.BRIGHT: {
						this.state.ui.theme.status = 'is-bright';
						break;
					}
					case StatusType.DARK: {
						this.state.ui.theme.status = 'is-dark';
						break;
					}
				}

				this.state.ui.theme.statusShaded = pointerStatusShaded;
			}
		},

		resetsleep_timer() {

			clearTimeout(this.state.ui.sleep.timer);
			const doit = () => {
				if(this.$route.name != 'p42_sleep') {
					this.$os.routing.goTo({ name: 'p42_sleep' });
				}
			};
			if(!this.$os.userConfig.get(['ui','tutorials'])) {
				switch(this.$os.routing.activeAppRouter.name){
					//case 'p42_locked': {
					//	this.state.ui.sleep.timer = setTimeout(doit, this.$root.$data.config.ui.sleep_timer_locked);
					//	break;
					//}
					case 'p42_sleep': {
						break;
					}
					default: {
						if(this.$os.routing.activeApp) {
							if(this.$os.routing.activeApp.can_sleep) {
								this.state.ui.sleep.timer = setTimeout(doit, this.$os.userConfig.get(['ui','sleep_timer']));
							}
						}
						break;
					}
				}
			}
		},
		tap() {
			this.resetsleep_timer();
		},
		saveState() {
		},

		listener_notif(notif: any) {
			switch(notif.name) {
				case 'add': {
					this.state.ui.notifications.tray = true;
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name) {
				case 'covered': {
					this.state.ui.covered = wsmsg.payload;
					break;
				}
				case 'themechange': {
					this.set_theme();
					break;
				}
				case 'sidebar': {
					this.state.ui.app_expand = wsmsg.payload;
					break;
				}
			}
		},
	},

});
</script>
