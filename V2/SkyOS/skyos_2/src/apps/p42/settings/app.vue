<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame app-frame--clear-bottom">

			<width_limiter size="screen">
				<split_view>
					<sidebar class="split_view_narrow shadowed" :statusspaced="true">

						<content_controls_stack theme="is-sidebar" :translucent="true">
							<template v-slot:nav>
								<div class="helper_edge_margin_left_half">
									<h1>Settings</h1>
								</div>
							</template>
							<template v-slot:content>
								<div class="helper_edge_padding">
									<router-link :to="{name: 'p42_settings_updates'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/gift">Software Updates</button_sidebar></router-link>
									<router-link :to="{name: 'p42_settings_display'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/display">Display</button_sidebar></router-link>
									<!--<router-link :to="{name: 'p42_settings_region'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/region">Region</button_sidebar></router-link>-->
									<router-link :to="{name: 'p42_settings_tier'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/gameplay">Gameplay</button_sidebar></router-link>
									<router-link :to="{name: 'p42_settings_config'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/config">Configuration</button_sidebar></router-link>
								</div>
							</template>
							<template v-slot:tab>
								<div class="helper_edge_padding">
									<button_sidebar icon="theme/discord" @click.native="openDiscord()">Discord</button_sidebar>
									<button_sidebar icon="theme/support" @click.native="openHelp()">Help</button_sidebar>
									<router-link :to="{name: 'p42_settings_legal'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/config">Legal</button_sidebar></router-link>
								</div>
							</template>
						</content_controls_stack>

					</sidebar>
					<router-view></router-view>
				</split_view>
			</width_limiter>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "./../../../sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_settings",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
	mounted() {
		this.$emit('loaded');
	},
	methods: {
		openHelp() {
			window.open("https://support.tfdidesign.com/", "_blank");
		},
		openDiscord() {
			window.open("https://tfdidesign.com/chat", "_blank");
		}
	},
	data() {
		return {
			ready: true,
		}
	},
});
</script>

<style lang="scss">
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
.p42_settings {
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