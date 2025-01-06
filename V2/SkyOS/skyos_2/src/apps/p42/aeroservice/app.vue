<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame app-frame--clear-bottom">

			<width_limiter size="screen">
				<split_view>
					<sidebar class="split_view_narrow shadowed" :statusspaced="true">

						<content_controls_stack theme="is-sidebar" :translucent="true">
							<template v-slot:nav>
							</template>
							<template v-slot:content>
								<div class="helper_edge_padding">
									<div>
										<router-link :to="{name: 'p42_aeroservice_overview'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/discord">Fleet</button_sidebar></router-link>
										<router-link :to="{name: 'p42_aeroservice_account'}" exact-active-class="is-active" tag="div"><button_sidebar icon="theme/discord">Insurance</button_sidebar></router-link>
									</div>
								</div>
							</template>
						</content_controls_stack>

					</sidebar>
					<router-view :appvue="this" :fleet="state.fleet"></router-view>
				</split_view>
			</width_limiter>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "./../../../sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_aeroservice",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
	mounted() {
		this.$emit('loaded');
		this.getFleet();
	},
	data() {
		return {
			ready: true,
			captionTitle: '',
			caption: '',
			state: {
				fleet: []
			}
		}
	},
	methods: {
		getFleet() {
			const queryoptions = {
				limit: 20,
			}

			const responseFn = (fleetData: any) => {
				this.state.fleet = fleetData.payload
			}

			this.$root.$data.services.api.SendWS(
				'fleet:getall', queryoptions, responseFn
			);
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.getFleet();
					break;
				}
				case 'disconnect': {

					break;
				}
			}
		}
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	}
});
</script>

<style lang="scss" scoped>
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
@import './../../../sys/scss/mixins.scss';

.p42_aeroservice {
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