<template>
	<div :class="[appName, app.nav_class]">
		<div class="app-frame" :class="{ 'has-subcontent': ui.subframe !== null }">
			<scroll_stack class="app-box shadowed-deep">
				<template v-slot:content>
					<div class="h_edge_padding_lateral h_edge_padding_vertical_half">
						<router-link :to="{name: 'p42_settings_updates'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/gift">Software Updates</button_sidebar></router-link>
						<router-link :to="{name: 'p42_settings_display'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/display">Display</button_sidebar></router-link>
						<!--<router-link :to="{name: 'p42_settings_region'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/region">Region</button_sidebar></router-link>-->
						<router-link :to="{name: 'p42_settings_tier'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/gameplay">Gameplay</button_sidebar></router-link>
						<router-link :to="{name: 'p42_settings_config'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/config">Configuration</button_sidebar></router-link>
					</div>
				</template>
				<template v-slot:bottom>
					<div class="controls-bottom h_edge_padding_lateral h_edge_padding_vertical_half">
						<button_sidebar icon="theme/discord" @click.native="open_discord()">Discord</button_sidebar>
						<button_sidebar icon="theme/support" @click.native="open_help()">Help</button_sidebar>
						<router-link :to="{name: 'p42_settings_legal'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/config">Legal</button_sidebar></router-link>
					</div>
				</template>
			</scroll_stack>
		</div>
		<div class="app-subframes">
			<div class="app-subframe" :class="{ 'has-content': ui.subframe !== null }">
				<transition :duration="1000">
					<PanelLegal v-if="ui.subframe == 'legal'" @close="ui.subframe = null"/>
					<PanelLegal2 v-if="ui.subframe == 'legal2'" @close="ui.subframe = null"/>
				</transition>
			</div>
		</div>
		<app_panel :app="app" :has_content="$route.matched.length > 1" :has_subcontent="ui.subframe !== null" :scroll_top="ui.panel_scroll_top">
			<transition :duration="1000">
				<router-view @scroll="ui.panel_scroll_top = $event.scrollTop"></router-view>
			</transition>
		</app_panel>
  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	data() {
		return {
			ui: {
				subframe: null,
				panel_scroll_top: 0,
			}
		}
	},
	methods: {
		open_help() {
			window.open("http://help.parallel42.com/", "_blank");
		},
		open_discord() {
			window.open("https://discord.com/invite/6Kp33hmS7E", "_blank");
		}
	},
	mounted() {
		this.$emit('loaded');
	},
});
</script>

<style lang="scss">
	@import '@/sys/scss/colors.scss';



</style>