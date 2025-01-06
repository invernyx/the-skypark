<template>
	<section class="os-modals">
		<transition :duration="{ enter: 800, leave: 200 }">

			<!-- CONTRACT -->
			<ContractModal v-if="queue.length ? queue[0].type == 'contract' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- GALLERY -->
			<GalleryModal v-else-if="queue.length ? queue[0].type == 'gallery' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- SHARECARD -->
			<ShareModal v-else-if="queue.length ? queue[0].type == 'sharecard' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- INVOICE -->
			<InvoiceModal v-else-if="queue.length ? queue[0].type == 'invoice' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- CHANGELOG -->
			<ChangelogModal v-else-if="queue.length ? queue[0].type == 'changelog' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- REPORT AIRPORT -->
			<ReportAirportModal v-else-if="queue.length ? queue[0].type == 'report_airport' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- CALL -->
			<CallModal v-else-if="queue.length ? queue[0].type == 'call' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- ASK -->
			<AskModal v-else-if="queue.length ? queue[0].type == 'ask' : false" :md="queue[0]" @close="$os.modals.close()"/>

			<!-- ASK -->
			<NotifyModal v-else-if="queue.length ? queue[0].type == 'notify' : false" :md="queue[0]" @close="$os.modals.close()"/>

		</transition>
	</section>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "modals",
	components: {
		AskModal: () => import("@/sys/components/modals/components/ask.vue"),
		NotifyModal: () => import("@/sys/components/modals/components/notify.vue"),
		GalleryModal: () => import("@/sys/components/modals/components/gallery.vue"),
		ChangelogModal: () => import("@/sys/components/modals/components/changelog.vue"),
		ReportAirportModal: () => import("@/sys/components/modals/components/report_airport.vue"),
		CallModal: () => import("@/sys/components/modals/components/call.vue"),
		ShareModal: () => import("@/sys/components/modals/components/contract_share.vue"),
	},
	data() {
		return {
			queue: [] as any
		}
	},
	methods: {
		listener_modals(wsmsg :any) {
			switch(wsmsg.name){
				case 'open': {
					this.queue.unshift(wsmsg.payload);
					break;
				}
				case 'close': {
					this.queue.splice(0, 1);
					break;
				}
			}
		},
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('modals', this.listener_modals);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('modals', this.listener_modals);
	},
});
</script>