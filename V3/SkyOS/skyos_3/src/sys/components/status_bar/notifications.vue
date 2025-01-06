<template>
	<div class="os-status_notification" :class="{ 'active': list.length > 0 }" @click="toggle">
		<div class="os-status_notification_bell"></div>
		<span>{{ list.length }}</span>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	data() {
		return {
			list: [],
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('notif-svc', this.listener_notif);
	},
	methods: {
		listener_notif(wsmsg :any) {
			switch(wsmsg.name){
				default: {
					this.list = wsmsg.payload.list;
					break;
				}
			}
		},
		toggle() {
			this.$emit('toggle');
		}
	},
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
</style>