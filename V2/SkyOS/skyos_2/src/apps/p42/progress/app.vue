<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame app-frame--clear-bottom">

			<width_limiter size="screen">
				<split_view>
					<sidebar class="split_view_narrow shadowed" :statusspaced="true">

						<content_controls_stack theme="is-sidebar" :translucent="true">
							<template v-slot:nav>
								<div class="helper_edge_margin_left_half">
									<h1>Progress</h1>
								</div>
							</template>
							<template v-slot:content>
								<div class="helper_edge_padding">
									<div>
										<router-link :to="{name: 'p42_progress_overview'}" exact-active-class="is-active" tag="div"><button_sidebar>Overview</button_sidebar></router-link>
										<!--<router-link :to="{name: 'p42_settings_account'}" exact-active-class="is-active" tag="div"><button_sidebar>Account</button_sidebar></router-link>-->
									</div>
								</div>
							</template>
							<template v-slot:tab>
								<div class="sidebar-caption" v-if="captionTitle">
									<h2>{{ captionTitle }}</h2>
									<p>{{ caption }}</p>
								</div>
							</template>
						</content_controls_stack>

					</sidebar>
					<router-view :appvue="this"></router-view>
				</split_view>
			</width_limiter>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "./../../../sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_progress",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
	mounted() {
		this.$emit('loaded');
	},
	created() {
		this.$on('caption', (title, caption) => {
			this.captionTitle = title,
			this.caption = caption;
		});
	},
	data() {
		return {
			ready: true,
			captionTitle: '',
			caption: '',
		}
	},
});
</script>

<style lang="scss" scoped>
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
@import './../../../sys/scss/mixins.scss';

.p42_progress {
	.theme--bright &,
	&.theme--bright {
		.app-frame {
			.width_limiter {
				&::before {
					background: $ui_colors_bright_shade_0;
				}
			}
		}
	}
	.theme--dark &,
	&.theme--dark {
		.app-frame {
			.width_limiter {
				&::before {
					background: $ui_colors_dark_shade_0;
				}
			}
		}
	}
}
</style>