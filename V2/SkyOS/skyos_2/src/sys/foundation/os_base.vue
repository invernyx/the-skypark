<template>
	<div id="app" :class="{ 'task-switch': true }" @mousemove="tap">

		<div class="view-manager"><ViewManager :inst="$root"/></div>
		<section class="os-modals" v-if="$root.$data.state.ui.modals.queue.length">
			<Modals/>
		</section>

		<Tutorials v-if="$root.$data.state.ui.tutorials" @done="tutorials_done"/>
		<AppSwitcher :open="state.ui.task_switcher" @close="task_close()" />

		<div class="os-notifications" :class="[state.ui.theme.status, { 'is-shaded': state.ui.theme.statusShaded }]">
			<div class="os-notifications-tray" :class="[{'os-notifications-tray-open': state.ui.notification_tray_open }]">
				<div>
					<scroll_view :scroller_offset="{ top: 30, bottom: 10 }" :dynamic="true">
						<div class="os-notifications-tray-list">
							<div class="os-notifications-tray-controls">
								<div>
									<div v-if="$root.$data.state.services.notifications.list.length > 0" class="os-notifications-tray-label">{{ $root.$data.state.services.notifications.list.length }} notification{{ $root.$data.state.services.notifications.list.length == 1 ? '' : 's' }}</div>
									<div v-else class="os-notifications-tray-label text-center">No notifications</div>
								</div>
								<div>
									<button_action @click.native="$os.clearAllNotifications(); state.ui.notification_tray_open = false; notification_clear()" v-if="$root.$data.state.services.notifications.list.filter(x => x.CanDismiss !== false).length > 0">Clear all</button_action>
									<button_action class="cancel" @click.native="state.ui.notification_tray_open = false">Close</button_action>
								</div>
							</div>
							<Notification
								v-for="(notification, index) in $root.$data.state.services.notifications.list.slice().reverse()"
								v-bind:key="index"
								:isvisible="true"
								:notification="notification"
								@click.native="notification_clickList"
							/>
						</div>
					</scroll_view>
				</div>
			</div>
			<div class="os-notifications-live" :class="{ 'expanded': state.ui.notifications.visible }">
				<Notification
					v-if="state.ui.notifications.queue.length"
					:isvisible="state.ui.notifications.visible"
					:notification="state.ui.notifications.queue[0]"
					@click.native="notification_click"
				/>
			</div>
		</div>

		<section class="os-status" :class="[state.ui.theme.status, { 'is-shaded': state.ui.theme.statusShaded }]">
			<div class="os-status_background">
				<div class="os-status_background_bright"></div>
				<div class="os-status_background_dark"></div>
			</div>
			<div class="os-status_stack">
				<div>
					<div class="os-status_signal" v-bind:class="{'is-remote': this.$root.$data.state.services.api.connectedRemote}">
						<div class="os-status_signal_wifi" v-bind:class="{
							'searching': !this.$root.$data.state.services.api.connectedLocal,
							'active': this.$root.$data.state.services.api.connectedLocal
							}">
							<div class="os-status_signal_wifi_1"></div>
							<div class="os-status_signal_wifi_2"></div>
							<div class="os-status_signal_wifi_3"></div>
						</div>
						<div class="os-status_signal_lte" v-bind:class="{ 'active': this.$root.$data.state.services.api.connectedRemote }">
							<div class="os-status_signal_lte_1"></div>
							<div class="os-status_signal_lte_2"></div>
							<div class="os-status_signal_lte_3"></div>
							<div class="os-status_signal_lte_4"></div>
						</div>
					</div>
					<div class="os-status_notice">
						<span>The Skypark</span>
					</div>
				</div>
				<div>
					<div class="os-status_user">
						<span><strong>{{ clockLocal }}</strong> - <strong>{{ clockUTC }}</strong>{{ this.$root.$data.state.services.simulator.live ? ' - SIM' : ''}}</span>
					</div>
					<div class="os-status_notification" :class="{ 'active': $root.$data.state.services.notifications.list.length > 0 }" @click="state.ui.notification_tray_open = !state.ui.notification_tray_open">
						<div class="os-status_notification_bell"></div>
						<span>{{ $root.$data.state.services.notifications.list.length }}</span>
					</div>
				</div>
			</div>
		</section>

		<section v-if="state.ui.currentApp" class="os-nav" :class="state.ui.theme.nav">
			<div class="os-nav_back" @click="task_back"/>
			<div class="os-nav_home" @mousedown="home_hold_start" @mouseleave="home_hold_cancel" @mouseup="home_hold_cancel" @click="home_click"/>
			<div class="os-nav_task-switch" @mouseenter="task_enter" @mouseleave="task_leave" @click="task_click"/>
		</section>
	</div>
</template>

<style lang="sass">
  @import '../scss/base.scss'
</style>

<script lang="ts">
import Vue from "vue";
import ViewManager from "./os_view_manager.vue"
import Apps, { StatusType, NavType, AppType } from "./app_bundle"
import Eljs from '@/sys/libraries/elem';
import { setInterval } from 'timers';

export default Vue.extend({
	components: {
		ViewManager,
		Modals: () => import("@/sys/components/modals/modals.vue"),
		AppSwitcher: () => import("@/sys/components/switcher/app_switcher.vue"),
		Changelog: () => import("@/sys/components/changelog/log.vue"),
		Tutorials: () => import("@/sys/components/tutorials/overlay.vue"),
		Notification: () => import("@/sys/components/notification.vue"),
	},

	data() {
		return {
			state: {
				ui: {
					currentApp: null,
					task_switcher: false,
					task_switcher_time: new Date(),
					task_switcher_timeout: null,
					notification_tray_open: false,
					theme: {
						nav: null as string,
						status: null as string,
						statusShaded: false,
					},
					notifications: {
						queue: [],
						visible: false,
						timer: null,
					}
				}
			},
			hold_hold_timeout: null,
			clockLocal: '-',
			clockUTC: '-',
		}
	},

	mounted() {
		this.$os.initAppHistory();
		this.set_theme();
		this.$root.$on('notif-svc', this.listenerNotif);

		// Initiate Sleep Timer
		this.$router.afterEach((to, from) => {
			this.state.ui.task_switcher = false;
			this.state.ui.currentApp = Apps.find(x => to.name.startsWith(x.app_vendor + "_" + x.app_ident));
			this.$os.setCurrentApp(this.state.ui.currentApp);
			this.$os.activeAppRouter = to;
			this.set_theme();
			this.resetSleepTimer();

			this.state.ui.notification_tray_open = false;
		});

		this.$root.$on('themechange', () => {
			this.set_theme();
		});

		this.$root.$on('configchange', (path :string, value :any) => {
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

		//let time = moment();
		const ref = () => {
			if(!this.$os.getState(['ui','is1142'])) {

				if(this.$root.$data.state.services.simulator.live) {
					const d = new Date(this.$root.$data.state.services.simulator.location.SimTimeZulu - (this.$root.$data.state.services.simulator.location.SimTimeOffset * 1000));
					this.clockLocal = Eljs.getTime(d, {
						timeZone: 'UTC',
						hour: 'numeric',
						minute: 'numeric',
					});
					const dt = new Date(this.$root.$data.state.services.simulator.location.SimTimeZulu);
					this.clockUTC = Eljs.getTime(dt, {
						timeZone: 'UTC',
						hour: 'numeric',
						minute: 'numeric',
					}) + " GMT";
				} else {
					const d = new Date();
					this.clockLocal = Eljs.getTime(d, {
						hour: 'numeric',
						minute: 'numeric',
					});
					this.clockUTC = Eljs.getTime(d, {
						timeZone: 'UTC',
						hour: 'numeric',
						minute: 'numeric',
					}) + " GMT";
				}
				/*
				if(this.$root.$data.state.services.simulator.live) {
					time = moment.utc(this.$root.$data.state.services.simulator.location.SimTimeZulu); //SimTimeOffset
					this.clockLocal = time.clone().add(-this.$root.$data.state.services.simulator.location.SimTimeOffset, 'seconds').format('HH:mm');
					this.clockUTC = time.format('HH:mm [GMT]');
				} else {
					time = moment();
					this.clockLocal = time.format('HH:mm');
					this.clockUTC = time.utc().format('HH:mm [GMT]');
				}
				*/
			} else {
				this.clockLocal = "11:42 AM";
				this.clockUTC = "11:42 AM GMT";
			}
		};

		// Clock
		setInterval(() => {
			ref();
		}, 1000);
		ref();


		this.$root.$emit('ws-in', { name: ['disconnect'] });
	},

	methods: {
		home_hold_start() {
			this.hold_hold_timeout = setTimeout(() => {
				clearTimeout(this.hold_hold_timeout);
				const appToReset = this.$os.activeApp;
				this.$router.push({name: 'p42_home'});
				localStorage.removeItem('store_' + appToReset.app_vendor + '_' + appToReset.app_ident);
				appToReset.loaded_state = null;
			}, 5000)
		},
		home_hold_cancel() {
			clearTimeout(this.hold_hold_timeout);
		},
		home_click() {
			if(this.$root.$data.state.ui.modals.queue.length) {
				this.$os.modalClose();
			} else {
				const history = this.$os.getAppAllHistory().filter(x => x.app_type != AppType.SLEEP && x.app_type != AppType.LOCKED);
				if(history.length > 1 && history[history.length - 1] != this.$os.activeApp) {
					const lastApp = history[history.length - 1];
					this.$router.push({ name: lastApp.app_vendor + "_" + lastApp.app_ident });
				} else {
					this.$router.push({ name: 'p42_home' });
				}
			}
		},

		task_enter() {
			clearTimeout(this.state.ui.task_switcher_timeout);
			this.state.ui.task_switcher_timeout = setTimeout(() => {
				if(!this.state.ui.task_switcher) {
					this.task_click();
				}
			}, 200);
		},
		task_leave() {
			clearTimeout(this.state.ui.task_switcher_timeout);
		},
		task_click() {
			if(new Date().getTime() - this.state.ui.task_switcher_time.getTime() > 700) {
				this.state.ui.task_switcher_time = new Date();
				this.state.ui.task_switcher = !this.state.ui.task_switcher;
				this.resetSleepTimer();
				this.set_theme();
			}
		},
		task_back() {
			//window.history.back();
			const history = this.$os.getAppHistory();
			if(history.length > 1) {
				let lastApp = null;
				if(history[history.length - 1] != this.$os.activeApp) {
					lastApp = history[history.length - 1];
				} else {
					lastApp = history[history.length - 2];
				}
				this.$router.push({ name: lastApp.app_vendor + "_" + lastApp.app_ident });
			}
		},
		task_close() {
			this.state.ui.task_switcher = false;
			this.resetSleepTimer();
			this.set_theme();
		},

		tutorials_done() {
			this.$root.$data.state.ui.tutorials = false;
		},

		set_theme() {
			if(this.state.ui.currentApp) {

				const osth = this.$os.themeAccentOverride;
				const appth = this.state.ui.currentApp.app_theme;
				const appthovr = this.state.ui.currentApp.app_theme_override;
				const appopt = this.state.ui.currentApp.app_theme_option;

				let pointerNav = this.state.ui.currentApp.app_theme.bright.nav;
				let pointerStatus = this.state.ui.currentApp.app_theme.bright.status;
				let pointerStatusShaded = this.state.ui.currentApp.app_theme.bright.shaded;

				switch(this.$os.getConfig(['ui', 'theme'])) {
					case 'theme--bright': {
						if(osth) {
							pointerNav = osth.nav.bright;
							pointerStatus = osth.status.bright;
							pointerStatusShaded = osth.status.shaded;
						} else if(appthovr) {
							pointerNav = appthovr.nav.bright;
							pointerStatus = appthovr.status.bright;
							pointerStatusShaded = appthovr.status.shaded;
						} else if(appopt) {
							pointerNav = appopt.nav.bright;
							pointerStatus = appopt.status.bright;
							pointerStatusShaded = appopt.status.shaded;
						} else {
							pointerNav = appth.bright.nav;
							pointerStatus = appth.bright.status;
							pointerStatusShaded = appth.bright.shaded;
						}
						break;
					}
					case 'theme--dark': {
						if(osth) {
							pointerNav = osth.nav.dark;
							pointerStatus = osth.status.dark;
							pointerStatusShaded = osth.status.shaded;
						} else if(appthovr) {
							pointerNav = appthovr.nav.dark;
							pointerStatus = appthovr.status.dark;
							pointerStatusShaded = appthovr.status.shaded;
						} else if(appopt) {
							pointerNav = appopt.nav.dark;
							pointerStatus = appopt.status.dark;
							pointerStatusShaded = appopt.status.shaded;
						} else {
							pointerNav = appth.dark.nav;
							pointerStatus = appth.dark.status;
							pointerStatusShaded = appth.dark.shaded;
						}
						break;
					}
				}

				if(this.state.ui.task_switcher) {
					pointerNav = StatusType.BRIGHT;
				}

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

		notification_show(notificationInstance :Notification) {
			this.state.ui.notifications.queue.push(notificationInstance);
			if(!this.state.ui.notification_tray_open) {
				const loop = () => {

					if(this.state.ui.notifications.queue.length) {
						while(!this.$root.$data.state.services.notifications.list.includes(this.state.ui.notifications.queue[0])){
							this.state.ui.notifications.queue.splice(0,1);
							if(this.state.ui.notifications.queue.length == 0){
								break;
							}
						}
					}

					if(this.state.ui.notifications.queue.length) {
						this.state.ui.notifications.visible = true;
						this.state.ui.notifications.timer = setTimeout(() => {
							this.state.ui.notifications.visible = false;
							setTimeout(() => {
								if(this.state.ui.notifications.queue.length > 1) {
									this.state.ui.notifications.visible = true;
									loop();
								} else {
									clearTimeout(this.state.ui.notifications.timer);
									this.state.ui.notifications.timer = null;
								}
								this.state.ui.notifications.queue.splice(0, 1);
							}, 300);
						}, 8000);
					}
				}

				if(this.state.ui.notifications.timer == null) {
					loop();
				}
			}
		},
		notification_clear() {
			this.state.ui.notifications.queue = [];
		},
		notification_click() {
			this.state.ui.notifications.visible = false;
		},
		notification_clickList() {
			if(this.$root.$data.state.services.notifications.list.length == 0) {
				this.state.ui.notification_tray_open = false;
			}
		},

		resetSleepTimer() {
			clearTimeout(this.$root.$data.state.ui.sleepTimer);
			const doit = () => {
				if(this.$route.name != 'p42_sleep') {
					this.$router.push({name: 'p42_sleep'});
				}
			};
			if(!this.$os.getState(['ui','tutorials'])) {
				switch(this.$os.activeAppRouter.name){
					case 'p42_locked': {
						this.$root.$data.state.ui.sleepTimer = setTimeout(doit, this.$root.$data.config.ui.sleepTimerLocked);
						break;
					}
					case 'p42_sleep': {
						break;
					}
					default: {
						if(this.$os.activeApp) {
							if(this.$os.activeApp.app_can_sleep) {
								this.$root.$data.state.ui.sleepTimer = setTimeout(doit, this.$root.$data.config.ui.sleepTimer);
							}
						}
						break;
					}
				}
			}
		},
		tap() {
			this.resetSleepTimer();
		},
		saveState() {
		},

		listenerNotif(notif: any) {
			this.notification_show(notif);
		}
	},

	computed: {
		isAuthed(): boolean {
			return this.$root.$data.services.auth.authenticated;
		},
	},

});
</script>
