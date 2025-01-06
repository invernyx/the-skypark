<template>
	<div class="app-box transparent nooutline small actions" v-if="aircraft">
		<div class="columns columns_margined_half">
			<div class="column column_narrow">
				<div class="columns columns_margined_half">
					<div class="column">
						<button_action class="shadowed info icon icon-next" :class="{ 'disabled': !aircraft.location }" @click.native="stops_next"></button_action>
					</div>
				</div>
			</div>
			<div class="column column_justify_center">

			</div>
			<div class="column column_narrow">
				<div class="columns columns_margined_half">
					<div class="column">
						<button_action class="action_collapse shadowed info icon icon-collapse" @click.native="app.events.emitter.emit('aircraft_collapse')"></button_action>
					</div>
					<div class="column">
						<button_action class="cancel shadowed icon icon-close" @click.native="$os.eventsBus.Bus.emit('map_select', { name: 'fleet', payload: null } )"></button_action>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Aircraft from '@/sys/classes/aircraft';

export default Vue.extend({
	props: {
		app: AppInfo,
		aircraft: Aircraft
	},
	data() {
		return {
			sid: this.app.vendor + '_' + this.app.ident
		}
	},
	methods: {
		stops_next() {
			if(this.$os.scrollView.get_scroll_offset_top(this.sid + '_aircraft') > 0) {
				this.$os.scrollView.scroll_to(this.sid + '_aircraft', 0, 0, 300);
				setTimeout(() => {
					this.$os.eventsBus.Bus.emit('map', { name: 'aircraft_frame' })
				}, 300);
			} else {
				this.$os.eventsBus.Bus.emit('map', { name: 'aircraft_frame' })
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

	.actions {

		.action_collapse {
			opacity: 0;
		}

		.app-panel-content.is-open & {
			.action_collapse {
				opacity: 1;
			}
		}
	}

</style>