<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame app-frame--clear-bottom">

			<width_limiter size="screen" :shadowed="true">
				<div class="content_layout">
					<div class="app-header helper_status-padding">
						<div class="app-header--logo"></div>
					</div>

					<split_view>
						<sidebar class="split_view_narrow shadowed">
							<content_controls_stack theme="is-sidebar">
								<template v-slot:content>
									<div class="helper_edge_padding">
										<div>
											<router-link :to="{name: 'p42_holdings_activity'}" exact-active-class="is-active" tag="div"><button_sidebar>Activity</button_sidebar></router-link>
											<!--<router-link :to="{name: 'p42_holdings_statements'}" exact-active-class="is-active" tag="div" class="disabled"><button_sidebar>Statements</button_sidebar></router-link>-->
										</div>
									</div>
								</template>
							</content_controls_stack>
						</sidebar>
						<router-view></router-view>
					</split_view>
				</div>
			</width_limiter>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "./../../../sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_holdings",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
	mounted() {
		this.$emit('loaded');
	},
	data() {
		return {
			ready: true,
		}
	},
	//mounted() {
	//	console.log(this.$route);
	//}
});
</script>

<style lang="scss">
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
.p42_holdings {
	.theme--bright &,
	&.theme--bright {
		.app-frame {

		}
	}
	.theme--dark &,
	&.theme--dark {
		.app-frame {

		}
	}

	.app-frame {
		background-size: 80px;
		background-image: url(./assets/background-pattern.svg);
	}

	.content_layout {
		display: flex;
		flex-grow: 1;
		justify-content: stretch;
		flex-direction: column;
	}

	.app-header {
		background-image: linear-gradient(to right, #346133 0%, #559249 40%, #57964A 50%, #559249 60%, #346133 100%);
		padding-bottom: 10px;
		&--logo {
			height: 70px;
			background-image: url(./assets/sh_logo.svg);
			background-position: center;
			background-repeat: no-repeat;
		}
	}
}
</style>