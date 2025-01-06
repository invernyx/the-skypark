<template>
	<div class="os-status_user">
		<span><span class="f"><strong>{{ clockLocal }}</strong> - </span><strong>{{ clockUTC }}</strong>{{ sim_live ? ' - SIM' : ''}}</span>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	data() {
		return {
			sim_live: this.$os.simulator.live,
			clockLocal: '-',
			clockUTC: '-',
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);

		// Clock
		setInterval(() => {
			this.update();
		}, 1000);
		this.update();
	},
	methods: {

		update() {
			if(!this.$os.userConfig.get(['ui','is_1142'])) {

				if(!this.$os.simulator.location.SimTimeZulu) return;

				if(this.$os.simulator.live) {
					const d = new Date(this.$os.simulator.location.SimTimeZulu.getTime() - (this.$os.simulator.location.SimTimeOffset * 1000));
					this.clockLocal = Eljs.getTime(d, {
						timeZone: 'UTC',
						hour: 'numeric',
						minute: 'numeric',
					}, this.$os.userConfig.get(['ui','units','numbers']) );
					const dt = new Date(this.$os.simulator.location.SimTimeZulu);
					this.clockUTC = Eljs.getTime(dt, {
						timeZone: 'UTC',
						hour: 'numeric',
						minute: 'numeric',
					}, this.$os.userConfig.get(['ui','units','numbers'])) + " GMT";
				} else {
					const d = new Date();
					this.clockLocal = Eljs.getTime(d, {
						hour: 'numeric',
						minute: 'numeric',
					}, this.$os.userConfig.get(['ui','units','numbers']));
					this.clockUTC = Eljs.getTime(d, {
						timeZone: 'UTC',
						hour: 'numeric',
						minute: 'numeric',
					}, this.$os.userConfig.get(['ui','units','numbers'])) + " GMT";
				}
			} else {
				this.clockLocal = "11:42";
				this.clockUTC = "11:42 GMT";
			}
		},

		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.sim_live = wsmsg.payload;
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


	@media only screen and (max-width: $bp-800) {
		.f {
			display: none;
		}
	}
</style>