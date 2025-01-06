<template>
	<div :class="[appName, app.nav_class]">
		<div class="app-frame app-frame-noheader">
			<div class="h_status-margin h_nav-margin">
				<div class="tiles">
					<Icon v-for="(app, index) in $os.apps.filter(x => x.type == 0 && (x.visible || (x.needs_dev && isDev)) && (x.tier.length ? x.tier.includes($root.$data.config.ui.tier) : true))" :key="index" :root="root" :ind="index" :app="app" />
				</div>
			</div>
		</div>
  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Icon from "./icon.vue"
import { AppInfo } from "@/sys/foundation/app_model"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		Icon
	},
	data() {
		return {
			codeCumul: "",
			codeTO: null as any,
			ready: true,
			isDev: this.$os.system.isDev,
		}
	},
	methods: {
		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'disconnect': {
					this.isDev = false;
					break;
				}
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							if(wsmsg.payload.dev) {
								this.isDev = true;
							}
							break;
						}
					}
					break;
				}
			}
		}
	},
	mounted() {
		this.$emit('loaded');
		this.$os.system.setSidebar(true);
	},
	activated() {
		this.$os.system.setSidebar(true);
	},
	created() {
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
	}
});
</script>

<style lang="scss">
  	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/mixins.scss';

  .p42_home {

	.app-frame {
		pointer-events: none !important;
		will-change: transform;
	}

	.app-box {
		pointer-events: none !important;
		.theme--bright & {
			background-color: rgba($ui_colors_bright_shade_2, 0.8);
			@include shadowed_shallow($ui_colors_bright_shade_5);
		}
		.theme--dark & {
			background-color: rgba($ui_colors_dark_shade_2, 0.8);
			@include shadowed_shallow($ui_colors_dark_shade_0);
		}
	}

    .tiles {
		perspective: 800px;
		margin-top: 70px;
		position: absolute;
		top: 0;
		bottom: 23%;
		left: 20%;
		right: 20%;
		max-height: 400px;
		display: block;
		pointer-events: none;
		.tile {
			pointer-events: all;
		}

		&.landscape {
			& .tile {
				&.tile_1,
				&.tile_2,
				&.tile_3,
				&.tile_4 {
					top: 0%;
				}
				&.tile_5,
				&.tile_6,
				&.tile_7,
				&.tile_8 {
					top: 50%;
				}
				&.tile_9,
				&.tile_10,
				&.tile_11,
				&.tile_12 {
					top: 100%;
				}

				&.tile_1,
				&.tile_5,
				&.tile_9 {
					left: 0%;
				}
				&.tile_2,
				&.tile_6,
				&.tile_10 {
					left: 33.3%;
				}
				&.tile_3,
				&.tile_7,
				&.tile_11 {
					left: 66.6%;
				}
				&.tile_4,
				&.tile_8,
				&.tile_12 {
					left: 100%;
				}
			}
		}
	}
}
</style>