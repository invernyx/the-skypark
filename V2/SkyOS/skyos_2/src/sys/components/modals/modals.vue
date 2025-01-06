<template>
	<div v-if="$root.$data.state.ui.modals.queue.length">
		<!-- ASK -->
		<ContractModal v-if="$root.$data.state.ui.modals.queue[0].type == 'contract'" :md="$root.$data.state.ui.modals.queue[0]" @close="$os.modalClose()"/>

		<!-- ENDCARD -->
		<EndModal v-if="$root.$data.state.ui.modals.queue[0].type == 'endcard'" :result="$root.$data.state.ui.modals.queue[0].data" @close="$os.modalClose()"/>

		<!-- SHARECARD -->
		<ShareModal v-else-if="$root.$data.state.ui.modals.queue[0].type == 'sharecard'" :md="$root.$data.state.ui.modals.queue[0]" @close="$os.modalClose()"/>

		<!-- INVOICE -->
		<InvoiceModal v-else-if="$root.$data.state.ui.modals.queue[0].type == 'invoice'" :md="$root.$data.state.ui.modals.queue[0]" @close="$os.modalClose()"/>

		<!-- CHANGELOG -->
		<ChangelogModal v-else-if="$root.$data.state.ui.modals.queue[0].type == 'changelog'" :md="$root.$data.state.ui.modals.queue[0]" @close="$os.modalClose()"/>

		<!-- REPORT AIRPORT -->
		<ReportAirportModal v-else-if="$root.$data.state.ui.modals.queue[0].type == 'report_airport'" :md="$root.$data.state.ui.modals.queue[0]" @close="$os.modalClose()"/>

		<!-- ASK -->
		<AskModal v-else-if="$root.$data.state.ui.modals.queue[0].type == 'ask'" :md="$root.$data.state.ui.modals.queue[0]" @close="$os.modalClose()"/>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "modals",
	components: {
		AskModal: () => import("@/sys/components/modals/ask.vue"),
		InvoiceModal: () => import("@/sys/components/modals/invoice.vue"),
		ChangelogModal: () => import("@/sys/components/modals/changelog.vue"),
		ReportAirportModal: () => import("@/sys/components/modals/report_airport.vue"),
		ShareModal: () => import("@/sys/components/modals/contract_share.vue"),
		EndModal: () => import("@/sys/components/modals/contract_endcard.vue"),
		ContractModal: () => import("@/sys/components/modals/contract_detailed.vue"),
	},
});
</script>