<template>
	<scroll_view :sid="'p42_progress_contract'" :scroller_offset="{ top: 36, bottom: 30 }" @scroll="$emit('scroll', $event)">
		<div class="app-panel-wrap">
			<div class="app-panel-content" ref="app_panel_content" :class="{ 'is-open': open }" :style="{ 'padding-top': 'var(--spacing-top)' }">
				<div class="app-spacer"></div>
				<div class="app-panel-hit" @mouseenter="$emit('can_scroll', true)" @mouseleave="$emit('can_scroll', false)">

					<div class="content">

						<div class="columns columns_margined columns_break_1000">
							<div class="column">
								<Xp :progress="$os.progress.xp"></Xp>
							</div>
							<div class="column">
								<Karma :progress="$os.progress.karma"></Karma>
							</div>
							<div class="column">
								<Reliability :progress="$os.progress.reliability"></Reliability>
							</div>
						</div>

					</div>

					<div class="app-box shadowed small nooverflow content">

					</div>

				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"

import Xp from '@/sys/components/progress/graph_xp.vue';
import Reliability from '@/sys/components/progress/graph_reliability.vue';
import Karma from '@/sys/components/progress/graph_karma.vue';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		Xp,
		Reliability,
		Karma
	},
	data() {
		return {
			open: false,
			loading: true,
		}
	},
	methods: {

		init() {
			this.loading = true;
		},

		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'uncover': {
					this.$os.scrollView.scroll_to('p42_progress_contract', 0, 0, 100);
					break;
				}
				case 'covered': {
					this.open = wsmsg.payload;
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.init();
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

.app-panel-content {
	max-width: 700px;
	@media only screen and (max-width: $bp-1000) {
		max-width: 400px;
	}
}

</style>