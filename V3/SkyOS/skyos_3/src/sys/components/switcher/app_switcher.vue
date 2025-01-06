<template>
	<div class="app_switcher columns" :class="{ 'open': show }" v-if="app">
		<div class="column">
			<Icon :show="true" :ind="0" :app="app_home" :canOpen="true" v-if="app_home" />
		</div>
		<div class="column" v-for="(app, index) in appHistory.slice(-5, app.type == 0 ? -1 : appHistory.length)" :key="index">
			<Icon :show="true" :ind="index" :app="app" :canOpen="true" />
		</div>
		<div class="column">
			<Icon class="type-active" :show="true" :ind="5" :app="app" :current="true" />
		</div>
		<!--
		<div class="column">
			<Icon class="type-theme" :show="true" :canOpen="true" :ind="1" @act="toggleTheme" />
		</div>
		-->
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Route } from 'vue-router'
import Icon from "./icon.vue"
import { AppInfo, AppType } from '@/sys/foundation/app_model';

export default Vue.extend({
	data() {
		return {
			show: true,
			app_home: null as AppInfo,
			app: null as AppInfo,
			appHistory: []
		}
	},
	components: {
		Icon
	},
	mounted() {
		this.app_home = this.$os.apps.find(x => x.ident == 'home');
		this.app = this.$os.routing.activeApp;

		this.show = this.app.type != AppType.GENERAL;

		this.$router.afterEach((to :Route, from :Route) => {
			if(to.meta.app.type == AppType.GENERAL) {
				this.app = to.meta.app;
			}

			this.show = to.meta.app.type != AppType.GENERAL;
		});
		this.appHistory = this.$os.routing.getAppHistory();
	},
	methods: {
		toggleTheme() {
			this.$os.theme.toggleMode();
		}
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.app_switcher {
	position: absolute;
	bottom: 12px;
	left: $app-margin;
	width: $app-width;
	border-radius: 21px;
	z-index: 3;
	//display: flex;
	//justify-content: center;
	transition: transform 0.3s cubic-bezier(0,1,0,1);

	&.open {
		transform: translateY(200%);
		transition: transform 0.2s cubic-bezier(.5,0,1,.72);
	}

	& > div {
		display: flex;
		align-items: center;
	//	&:first-child {
	//		margin-right: 20px;
	//	}
	}

	& .tile {
		margin-right: 10px;
		&:last-child {
			margin-right: 0;
		}
	}
}
</style>