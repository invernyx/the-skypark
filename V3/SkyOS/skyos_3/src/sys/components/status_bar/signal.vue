<template>
	<div class="os-status_signal" v-bind:class="{'is-remote': remote }">
		<div class="os-status_signal_wifi" v-bind:class="{
			'searching': !local,
			'active': local
			}">
			<div class="os-status_signal_wifi_1"></div>
			<div class="os-status_signal_wifi_2"></div>
			<div class="os-status_signal_wifi_3"></div>
		</div>
		<div class="os-status_signal_lte" v-bind:class="{ 'active': remote }">
			<div class="os-status_signal_lte_1"></div>
			<div class="os-status_signal_lte_2"></div>
			<div class="os-status_signal_lte_3"></div>
			<div class="os-status_signal_lte_4"></div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	data() {
		return {
			remote: this.$os.api.connectedRemote,
			local: this.$os.api.connectedLocal,
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	methods: {
		listener_ws(wsmsg :any) {
			switch(wsmsg.name[0]){
				case 'connect':
				case 'disconnect': {
					this.remote = this.$os.api.connectedRemote;
					this.local = this.$os.api.connectedLocal;
					break;
				}
			}
		}
	},
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
</style>