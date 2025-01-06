<template>

	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ bottom: 15 }">
		<template v-slot:nav>
			<h2>Configuration</h2>
		</template>
		<template v-slot:content>
			<div class="helper_edge_padding helper_nav-margin">
				<div class="buttons_list shadowed">
					<button_listed icon="theme/reload" @click.native="rewatchTutorial">Rewatch Tutorial</button_listed>
				</div>
				<p class="notice">Missed out on the tutorial during the initial setup of your Skypad? Rewatch it now and learn new tricks!</p>
				<br><br>
				<div class="buttons_list shadowed">
					<button_listed icon="theme/reload" @click.native="resetAppsConfig">Reconfigure Skypad</button_listed>
				</div>
				<p class="notice">This will reset your Skypad apps to their original settings. If you want to reset a specific app, you can hold the home button to 5 seconds while in the app.</p>
			</div>
		</template>
	</content_controls_stack>

</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_settings_config",
	data() {
		return {
			themeAuto: false,
		}
	},
	methods: {
		resetAppsConfig() {
			this.$root.$data.config = JSON.parse(JSON.stringify(this.$root.$data.defaultConfig));
			(this.$root as any).stateSave();
			this.$router.push({ name: 'p42_onboarding' })
		},
		rewatchTutorial() {
			this.$os.setState(['ui','tutorials'], true);
		},
		changeSetting(tag :string, state :string) {
			this.$root.$data.services.api.SendWS('transponder:set', {
				param: tag,
				value: state,
			});
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							break;
						}
					}
					break;
				}
			}
		},
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
});
</script>

<style lang="scss">
	@import '../../../../sys/scss/colors.scss';
	.p42_settings_config {

	}
</style>