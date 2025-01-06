<template>
	<scroll_view :scroller_offset="{ top: 36, bottom: 30 }">
		<div class="app-panel-wrap">
			<div class="app-panel-content">
				<div class="app-panel-hit">
					<h2>Software Updates</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<toggle v-model="changelog_show" @modified="set_changelog_show">Show after every update</toggle>
					</div>
					<div class="h_edge_padding_vertical_half">
						<Changelog :limit="4"/>
					</div>
				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Changelog from "@/sys/components/changelog/log.vue"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		Changelog
	},
	data() {
		return {
			changelog_show: this.$os.userConfig.get(['ui','changelog','show']),
		}
	},
	methods: {
		set_changelog_show(state :boolean) {
			this.changelog_show = state;
			this.$os.userConfig.set(['ui','changelog','show'], state);
		},
	},
	mounted() {
		this.$os.system.set_cover(true);
	},
	beforeDestroy() {
	}
});
</script>

<style lang="scss" scoped>
	.app-panel-content {
		max-width: 390px;
	}
</style>