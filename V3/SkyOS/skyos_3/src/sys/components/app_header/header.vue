<template>
	<div class="app_header" :class="{ 'is-hidden': isHidden, 'is-open': isOpen }">
		<div class="columns">
			<!--
			<div class="column column_narrow">
				<div class="nav_home" @mousedown="home_hold_start" @mouseleave="home_hold_cancel" @mouseup="home_hold_cancel" @click="home_click"></div>
			</div>
			-->
			<div class="column column_narrow">
				<Icon :app="app" @open="open"/>
			</div>
			<div class="column column_justify_center">
				<h2>{{ app.name }}</h2>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import { AppInfo, StatusType, NavType, AppType } from "@/sys/foundation/app_model"
import Icon from "./icon.vue"

export default Vue.extend({
	props: {
		app: AppInfo,
		isHidden: Boolean
	},
	components: {
		Icon
	},
	data() {
		return {
			isOpen: true,
			hold_hold_timeout: null,
		}
	},
	methods: {
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'sidebar': {
					this.isOpen = wsmsg.payload;
					break;
				}
			}
		},
		open() {
			this.$os.system.setSidebar(!this.$os.system.sidebarOpen);
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
	},
	mounted() {

	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.app_header {

	position: absolute;
	top:  $status-size;
	left: $app-margin;
	width: $app-width;
	bottom: $nav-size;
	transition: transform 0.3s cubic-bezier(.37,.87,.5,1), opacity 0.3s cubic-bezier(.37,.87,.5,1);

	.map-covered.theme--sat.theme--bright &,
	.theme--bright & {
		color: $ui_colors_bright_shade_5;
		@include shadowed_text($ui_colors_bright_shade_2);
		.nav_home {
			background-image: url(../../assets/icons/home_dark.svg);
		}
	}

	.theme--sat &,
	.theme--dark & {
		color: $ui_colors_dark_shade_5;
		@include shadowed_text($ui_colors_dark_shade_2);
		.nav_home {
			background-image: url(../../assets/icons/home_bright.svg);
		}
	}

	&.is-hidden {
		opacity: 0;
		transform: translateX(50px);
	}

	.tile {
		pointer-events: all;
	}

	.column {
		h2 {
			margin-left: 8px;
		}
	}

	.nav_home {
		height: 20px;
		width: 20px;
		margin: 10px 15px;
		will-change: auto;
		pointer-events: all;
		cursor: pointer;
		transition: opacity 1s ease-out, transform 1s cubic-bezier(0,1,.1,1), background 0.2s 0s ease-out;
		&:active,
		&:hover {
			transform: scale(1.2);
			transition: transform 0.3s cubic-bezier(0,1,.1,1);
		}
	}

	/*
	@media only screen and (max-width: 700px) {
		.tile {
			pointer-events: all;
		}

		&.is-hidden {
			opacity: 1;
			transform: translateX(0);
		}

	}
	*/


	.app-expand & {
		&.is-open.is-hidden {
			opacity: 0;
			transform: translateX(50px);
		}
	}

}
</style>