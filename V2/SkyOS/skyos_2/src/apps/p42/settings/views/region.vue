<template>

	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ bottom: 15 }">
		<template v-slot:nav>
			<h2>Region</h2>
		</template>
		<template v-slot:content>
			<div class="helper_edge_padding helper_nav-margin">

			</div>
		</template>
	</content_controls_stack>

</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_settings_region",
	data() {
		return {
		}
	},
	methods: {

		listenerConfig(path: string[], value :any){
			switch(path[0]){
				case 'ui': {
					switch(path[1]){
						case 'topmost': {
							break;
						}
					}
					break;
				}
			}
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
		this.$root.$on('configchange', this.listenerConfig);

		console.log(navigator.languages);
	},
	beforeDestroy() {
		this.$root.$off('configchange', this.listenerConfig);
		this.$root.$off('ws-in', this.listenerWs);
	}
});
</script>

<style lang="scss">
</style>