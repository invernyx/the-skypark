<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame">
			<div class="backsplash">
				<div class="backsplash-image" :style="'background-image: url(' + inst.config.ui.wallpaper.replace('%theme%', $root.$data.config.ui.theme) + ')'"></div>
			</div>
			<div class="helper_status-margin helper_nav-margin">
				<div class="tiles">
					<Icon v-for="(app, index) in inst.apps.filter(x => x.app_type == 0 && (x.app_visible || (x.app_needs_dev && isDev)) && (x.app_tier.length ? x.app_tier.includes($root.$data.config.ui.tier) : true))" :key="index" :inst="inst" :ind="index" :app="app" />
				</div>
			</div>
		</div>
  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Icon from "./icon.vue"
import { AppInfo } from "./../../../sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_home",
		props: {
		inst: Object,
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
			isDev: this.$os.isDev,
		}
	},
	methods: {

		/*
		keyPressed(e: KeyboardEvent) {
			clearTimeout(this.codeTO);
			this.codeTO = setTimeout(() => {
				clearTimeout(this.codeTO);
				this.codeCumul = "";
				this.codeTO = null;
			}, 500);

			this.codeCumul += e.key;
			switch(this.codeCumul) {
				case 'dev42': {
					if(this.$os.isDev) {
						this.isDev = !this.isDev;
					}
					break;
				}
			}
		},
		*/

		listenerWs(wsmsg: any) {
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
		//document.addEventListener('keypress', this.keyPressed);
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
		//document.removeEventListener('keypress', this.keyPressed);
	}
});
</script>

<style lang="scss">
  @import '../../../sys/scss/colors.scss';
  .p42_home {

    .tiles {
      perspective: 800px;
      margin-top: 50px;
      position: absolute;
      top: 10%;
      bottom: 23%;
      left: 20%;
      right: 20%;
      display: block;

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