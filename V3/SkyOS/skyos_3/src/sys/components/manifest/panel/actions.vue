<template>
	<div class="app-box transparent nooutline small actions">
		<div class="columns columns_margined_half">
			<!--
			<div class="column column_narrow">
				<div class="columns columns_margined_half">
					<div class="column">
						<button_action class="shadowed icon icon-path" @click.native="stops_all"></button_action>
					</div>
					<div class="column">
						<button_action class="shadowed icon icon-next" @click.native="stops_next"></button_action>
					</div>
				</div>
			</div>
			-->
			<div class="column">
			</div>
			<div class="column column_narrow">
				<div class="columns columns_margined_half">
					<!--<div class="column">
						<button_action class="action_share shadowed" v-if="contract.template.template_code" @click.native="$emit('template_code')">#{{ contract.template.template_code }}</button_action>
					</div>-->
					<div class="column" v-if="contract">
						<button_action class="action_share shadowed" v-if="contract.route_code" @click.native="$emit('route_code')">#{{ contract.route_code }}</button_action>
					</div>
					<div class="column">
						<button_action class="action_collapse shadowed info icon icon-collapse" @click.native="app.events.emitter.emit('manifests_collapse')"></button_action>
					</div>
					<div class="column">
						<button_action class="shadowed cancel icon icon-close" @click.native="app.events.emitter.emit('manifests_close')"></button_action>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Contract, { Situation } from "@/sys/classes/contracts/contract";

export default Vue.extend({
	props: {
		app: AppInfo,
		contract: Contract
	},
	data() {
		return {
			sid: this.app.vendor + '_' + this.app.ident
		}
	},
	methods: {

		stops_next() {
			if(this.$os.scrollView.get_scroll_offset_top(this.sid + '_contract') > 0) {
				this.$os.scrollView.scroll_to(this.sid + '_contract', 0, 0, 300);
				setTimeout(() => {
					this.$os.eventsBus.Bus.emit('map', { name: 'contract_stops_next' })
				}, 300);
			} else {
				this.$os.eventsBus.Bus.emit('map', { name: 'contract_stops_next' })
			}
		},

		stops_all() {

			if(this.$os.scrollView.get_scroll_offset_top(this.sid + '_contract') > 0) {
				this.$os.scrollView.scroll_to(this.sid + '_contract', 0, 0, 300);
				setTimeout(() => {
					this.$os.eventsBus.Bus.emit('map', { name: 'contract_frame' })
				}, 300);
			} else {
				this.$os.eventsBus.Bus.emit('map', { name: 'contract_frame' })
			}
		},

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

		.loading & {
			.action_share {
				opacity: 0;
				pointer-events: none;
			}
		}

		.button_action.action_share {
			padding: 0;
			font-family: "SkyOS-SemiBold";
			flex-grow: 1;
			color: #FFFFFF;
			background-color: #222;
			background-image: url(../../../../sys/assets/icons/bright/share.svg);
			background-size: 1em;
			background-position: 4px center;
			background-repeat: no-repeat;
			letter-spacing: 0.05em;
			padding: 2px 6px 2px 22px;
			span {
				margin-left: 1.2em;
			}
			&:hover {
				color: #FFFFFF;
				background-color: #222;
			}
		}

		.app-panel-content.is-open & {
			.action_collapse {
				opacity: 1;
			}
		}
	}

</style>