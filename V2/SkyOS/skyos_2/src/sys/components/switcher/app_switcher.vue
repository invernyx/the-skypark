<template>
	<div class="app_switcher" :class="{ 'open': open }" @mouseleave="close" v-if="app">
		<div>
			<Icon v-for="(app, index) in appHistory.slice(-5, app.app_type == 0 ? -1 : appHistory.length)" :show="open" :key="index" :ind="index" :app="app" :canOpen="true" />
			<Icon class="type-active" :show="open" :ind="5" :app="app" :current="true" v-if="app.app_type == 0" />
		</div>
		<div>
			<Icon class="type-theme" :show="true" :canOpen="true" :ind="6" @act="toggleTheme" />
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { Route } from 'vue-router'
import Icon from "./icon.vue"
import { AppInfo } from '@/sys/foundation/app_bundle';

export default Vue.extend({
	name: "app_switcher",
	props: ['open'],
	data() {
		return {
			app: AppInfo,
			appHistory: []
		}
	},
	components: {
		Icon
	},
	mounted() {
		this.$router.afterEach((to :Route, from :Route) => {
			this.app = to.meta.app;
		});
		this.appHistory = this.$os.getAppHistory();
	},
	methods: {
		close() {
			this.$emit('close');
		},
		toggleTheme() {
			this.$os.setConfig(['ui','themeAuto'], false);
			if(this.$os.getConfig(['ui', 'theme']) == 'theme--dark') {
				this.$os.setConfig(['ui', 'theme'], 'theme--bright');
			} else {
				this.$os.setConfig(['ui', 'theme'], 'theme--dark');
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';
.app_switcher {
	position: absolute;
	bottom: 0;
	left: 0;
	right: 0;
	height: 75px;
	z-index: 3;
	padding-bottom: $nav-size;
	display: flex;
	justify-content: center;
	padding-top: 50px;
	padding-left: 10px;
	padding-right: 10px;
	pointer-events: none;
	transform: translateY(100%);
	transition: transform 0.2s cubic-bezier(.5,0,1,.72);

	.theme--bright &,
	&.theme--bright {
		background-image: linear-gradient(to bottom, rgba($ui_colors_bright_shade_5, 0), cubic-bezier(.2,0,.4,1), rgba($ui_colors_bright_shade_5, 0.9));
	}
	.theme--dark &,
	&.theme--dark {
		background-image: linear-gradient(to bottom, rgba($ui_colors_dark_shade_0, 0), cubic-bezier(.2,0,.4,1), rgba($ui_colors_dark_shade_0, 0.9));
	}

	& > div {
		display: flex;
		justify-content: center;
		&:first-child {
			margin-right: 20px;
		}
	}
	&.open {
		pointer-events: all;
		transform: translateY(0%);
		transition: transform 0.3s cubic-bezier(0,1,0,1);
	}

	& .tile {
		margin-right: 10px;
		&:last-child {
			margin-right: 0;
		}
		&.type-active {
			margin-left: 20px;
		}
	}
}
</style>