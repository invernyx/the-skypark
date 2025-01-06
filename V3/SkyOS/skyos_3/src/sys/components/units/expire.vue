<template>
	<span class="contract_expires" v-if="contract.template.running_clock">Expires <strong><countdown :has_warn="true" :warn_for="contract.pull_at" :time="contract.expire_at"></countdown></strong></span>
	<span class="contract_expires" v-else-if="contract.state == 'Listed'">Time: <strong>{{ contract.template.time_to_complete > 0 ? contract.template.time_to_complete.toLocaleString($os.userConfig.get(['ui','units','numbers'])) + 'h' : 'Infinite' }}</strong></span>
	<span class="contract_expires" v-else-if="contract.template.time_to_complete > 0">Remaining: <countdown :no_fix="true" :has_warn="true" :warn_for="contract.pull_at" :time="contract.expire_at" :full="false" :only_hours="true"></countdown></span>
	<span class="contract_expires" v-else><strong>Infinite</strong> Time</span>
</template>

<script lang="ts">
import Vue from 'vue';
import Contract from "@/sys/classes/contracts/contract";
import Template from "@/sys/classes/contracts/template";

export default Vue.extend({
	props: {
		contract :Contract
	},
	beforeMount() {
		this.init();
	},
	methods: {
		init() {
		}
	},
	data() {
		return {

		}
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				this.init();
			}
		}
	}
});
</script>