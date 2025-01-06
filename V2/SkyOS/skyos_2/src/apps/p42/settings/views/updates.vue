<template>

	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ bottom: 15 }">
		<template v-slot:nav>
			<h2>Software Updates</h2>
		</template>
		<template v-slot:content>
			<div class="helper_edge_padding helper_nav-margin">
				<toggle v-model="showOnUpdate" @modified="setShowOnUpdate">Show after every update</toggle>
				<br>
				<Changelog :limit="4"/>
			</div>
		</template>
	</content_controls_stack>

</template>

<script lang="ts">
import Vue from 'vue';
import Changelog from "@/sys/components/changelog/log.vue"

export default Vue.extend({
	name: "p42_settings_updates",
	components: {
		Changelog
	},
	data() {
		return {
			showOnUpdate: false,
		}
	},
	mounted() {
		var d = new Date();
		var n = d.toUTCString();

		this.showOnUpdate = this.$root.$data.config.ui.changelogShow;
	},
	methods: {
		setShowOnUpdate(state :boolean) {
			this.$root.$data.config.ui.changelogShow = this.showOnUpdate;
			(this.$root as any).stateSave();
		}
	}
});
</script>

<style lang="scss">
	@import '../../../../sys/scss/colors.scss';
	.p42_settings_changelog {

	}
</style>