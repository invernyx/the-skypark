<template>
	<scroll_view :scroller_offset="{ top: 36, bottom: 30 }">
		<div class="app-panel-wrap">
			<div class="app-panel-content">
				<div class="app-panel-hit">

					<h2>Tutorial</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div class="buttons_list shadowed-shallow">
							<button_listed icon="theme/reload" @click.native="rewatch_tutorial">Rewatch Tutorial</button_listed>
						</div>
						<p class="notice">Missed out on the tutorial during the initial setup of your Skypad? Rewatch it now and learn new tricks!</p>
					</div>

					<h2>Skypad</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div class="buttons_list shadowed-shallow">
							<button_listed icon="theme/reload" @click.native="reset_apps_config">Reconfigure Skypad</button_listed>
						</div>
						<p class="notice">This will reset your Skypad apps to their original settings. If you want to reset a specific app, you can hold the home button for 5&nbsp;seconds while in the app.</p>
					</div>

					<h2>Life</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="buttons_list shadowed-shallow">
								<button_listed icon="theme/reload" @click.native="reset_life" :class="{ 'disabled': !has_transponder }">Reset this life</button_listed>
							</div>
							<p class="notice">This will reset your life for the current game mode.</p>
						</div>
					</div>

				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
		}
	},
	methods: {

		reset_apps_config() {
			this.$root.$data.config = JSON.parse(JSON.stringify(this.$root.$data.defaultConfig), Eljs.json_parser);
			(this.$root as any).state_reset();
			this.$os.routing.goTo({ name: 'p42_onboarding' });
		},

		rewatch_tutorial() {
			this.$os.userConfig.set(['ui','tutorials'], true);
		},

		reset_life() {
			this.$os.modals.add({
				type: 'ask',
				title: 'Are you sure you want to reset your life?',
				text: [
					"The Skypark is built in a way that allows you to move between karma and reliability extremes quickly. You don't have to reset to overwrite \"bad\" decisions.",
					"Karma: Flow between Coyote and ClearSky with a few key hops.",
					"Reliability: Demonstrate increased reliability as you would in the real world.",
					"This will reset everything and cannot be undone."
				],
				actions: {
					yes: 'Do it!',
					no: 'Cancel'
				},
				data: {
				},
				func: (state :boolean) => {
					if(state) {
						this.$os.api.send_ws('transponder:reset_life', {});
					}
				}
			});
		},

		send_transponder(tag :string, state :string) {
			this.$os.api.send_ws('transponder:set', {
				param: tag,
				value: state,
			});
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.system.set_cover(true);

		/*
		this.$os.api.send_ws(
			'bank:get', {
				limit: 30,
			}, (bankData: any) => {
				console.log(bankData.payload);
			}
		);
		*/









	},
	created() {
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
	}
});
</script>

<style lang="scss" scoped>
	.app-panel-content {
		max-width: 390px;
	}
</style>