<template>
	<div class="columns columns_margined_half">
		<div class="column column_narrow" v-if="contract.state == 'Active' || contract.state == 'Saved'">
			<div v-if="contract.state == 'Active'">
				<button_action v-if="contract.is_monitored" class="cancel" @click.native="contract_pause_btn">Pause</button_action>
				<button_action v-else @hold="contract_cancel_btn" :hold="true">Abandon</button_action>
			</div>
			<div v-if="contract.state == 'Saved'">
				<button_action @hold="contract_unsave_btn" :hold="true">Unsave</button_action>
			</div>
		</div>
		<div class="column">
			<div v-if="contract.state == 'Active'">
				<button_action v-if="contract.is_monitored" class="info" @click.native="contract_navigate_btn">Navigate</button_action>
				<button_action v-else class="go" :class="{ 'disabled': !can_resume }" @click.native="contract_resume_btn">Resume {{ contract.template.type_label }}</button_action>
			</div>
			<div v-if="contract.state == 'Listed'">
				<button_action v-if="contract.state == 'Listed' && available" class="go" @click.native="contract_save_btn">Save {{ contract.template.type_label }}</button_action>
				<button_action v-else class="disabled">No longer available</button_action>
			</div>
			<div v-if="contract.state == 'Saved'">
				<button_action v-if="available" class="info" @click.native="contract_begin_btn" :class="{ 'disabled': !contract || !sim_live || !contract.path[0].range || !contract.aircraft_compatible }">
					<span v-if="contract.invoices ? contract.invoices.total_fees != 0 : false">Pay <currency :amount="contract.invoices.total_fees" :decimals="0"/> &amp;&nbsp;begin</span>
					<span v-else>Begin</span>
				</button_action>
				<button_action v-else class="disabled">No longer available</button_action>
			</div>
			<button_action v-if="contract.state == 'Failed'" class="disabled">Failed {{ contract.template.type_label }}</button_action>
			<button_action v-if="contract.state == 'Succeeded'" class="disabled">Completed {{ contract.template.type_label }}</button_action>
		</div>
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import { AppInfo } from '@/sys/foundation/app_model';
import Eljs from '@/sys/libraries/elem';
import Vue from 'vue';

export default Vue.extend({
	props: {
		app: AppInfo,
		contract: Contract
	},
	data() {
		return {
			sid: this.app.vendor + '_' + this.app.ident,
			sim_live: this.$os.simulator.live,
			can_resume: false,
			contract_expire_timeout: null,
			available: true,
		}
	},
	methods: {

		contract_begin_btn(ev :PointerEvent) {
			this.$os.contract_service.interact(this.contract, 'begin');
			//this.$os.routing.goTo({ name: this.sid, params: { contract_id: this.contract.id }});
		},

		contract_pause_btn(ev :PointerEvent) {
			this.$os.contract_service.interact(this.contract, 'pause');
			//this.$os.routing.goTo({ name: this.sid, params: { contract_id: this.contract.id }});
		},


		contract_resume_btn(ev :PointerEvent) {
			this.$os.contract_service.interact(this.contract, 'resume');
			//this.$os.routing.goTo({ name: this.sid, params: { contract_id: this.contract.id }});
		},

		contract_unsave_btn(ev :PointerEvent) {
			this.$os.contract_service.interact(this.contract, 'remove');
			this.$os.routing.goTo({ name: this.sid })
		},

		contract_cancel_btn(ev :PointerEvent) {
			this.$os.contract_service.interact(this.contract, 'cancel');
			//this.$os.routing.goTo({ name: this.sid })
		},

		contract_save_btn(ev :PointerEvent) {
			this.$os.contract_service.interact(this.contract, 'save')
		},

		contract_navigate_btn() {
			this.$os.routing.goTo({ path: '/yoflight/contract/' + this.contract.id }, true);
		},

		contract_expire() {
			this.available = false;
		},

		range_check() {
			if(this.contract.state == 'Active' && this.contract.last_location_geo) {
				const dist = Eljs.GetDistance(this.$os.simulator.location.Lat, this.$os.simulator.location.Lon, this.contract.last_location_geo[1], this.contract.last_location_geo[0], "N");
				this.can_resume = dist < 10;
			} else {
				this.can_resume = false;
			}
		},

		listener_os_contracts(wsmsg :any) {
			switch(wsmsg.name) {
				case 'contract_expire': {
					if(wsmsg.payload == this.contract.id) {
						this.contract_expire();
					}
					break;
				}
			}
		},
		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'live': {
					this.sim_live = wsmsg.payload;
					if(!this.sim_live) {
						this.can_resume = false;
					}
					break;
				}
				case 'meta': {
					this.range_check();
					break;
				}
			}
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
		this.$os.eventsBus.Bus.on('contracts', this.listener_os_contracts);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
		this.$os.eventsBus.Bus.off('contracts', this.listener_os_contracts);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.button_action {
	padding: 20px 16px;
}

</style>