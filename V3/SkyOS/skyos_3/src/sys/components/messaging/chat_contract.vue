<template>
	<div class="contract app-box shadowed-deep h_edge_padding_half" v-if="loading">

	</div>
	<div class="contract app-box shadowed-deep h_edge_padding_half" v-else-if="contract" :style="{ 'background-color': background_color }">
		<div class="columns columns_margined">
			<div class="column">
				<div class="title" v-if="contract.title.length">{{ contract.title }}</div>
				<div>
					<span class="route">{{ contract.route }}</span>
					<span class="dot"></span>
					<distance class="distance" :amount="contract.distance" :decimals="0"/>
				</div>
			</div>
			<div class="column column_narrow column_justify_stretch interactions">
				<button_action class="go small inline" @click.native.stop="">Save</button_action>
			</div>
		</div>
	</div>
	<div class="contract app-box shadowed-deep h_edge_padding_half" v-else>
		<div>Contract no longer available.</div>
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import Vue from 'vue';

export default Vue.extend({
	props: {
		contract_id :Number
	},
	methods: {
		init() {

			if(this.contract)
				this.$os.contract_service.dispose_single(this.contract);

			this.loading = true;
			this.$os.api.send_ws('adventures:query-from-id',
				{
					id: this.contract_id,
					fields: {
						contract: {
							id: true,
							route: true,
							file_name: true,
							state: true,
							description: true,
							image_url: true,
							recommended_aircraft: true,
							operated_for: true,
							modified_on: true,
							requested_at: true,
							distance: true,
							expire_at: true,
							completed_at: true,
							started_at: true,
							pull_at: true,
							reward_bux: true,
						},
						template: {
							time_to_complete: true,
							running_clock: true,
							file_name: true,
							type_label: true,
							aircraft_restriction_label: true,
							company: true,
							modified_on: true,
						}
					}
				},
				(contractsData: any) => {
					console.log(contractsData.payload.contract);
					if(contractsData.payload.contract) {
						this.contract = this.$os.contract_service.ingest(contractsData.payload.contract, contractsData.payload.template);
						this.colorize();
					} else {
						this.contract = null;
					}

					this.loading = false;
				}
			);
		},
		colorize() {
			if(this.contract) {
				if(this.contract.image_url.length) {
					this.$os.colorSeek.find(this.contract.image_url, 150, (color :any) => {
						this.color_bright = color.color_bright.h;
						this.color_dark = color.color_dark.h;
						this.color_is_dark = color.color_is_dark;
						this.background_color = this.theme == 'theme--dark' ? this.color_dark : this.color_bright;
					});
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'themechange': {
					this.theme = this.$os.userConfig.get(['ui','theme']);
					this.colorize();
					break;
				}
			}
		},
	},
	mounted() {
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);

		if(this.contract)
			this.$os.contract_service.dispose_single(this.contract);
	},
	data() {
		return {
			loading: true,
			contract: null as Contract,
			color_bright: null,
			color_dark: null,
			color_is_dark: null,
			background_color: null,
			theme: this.$os.userConfig.get(['ui','theme']),
		}
	},
	watch: {
		contract_id: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.contract {
	transition: background 0.5s ease-out;

	.theme--bright & {
		background-color: $ui_colors_bright_shade_0;
	}
	.theme--dark & {
		background-color: $ui_colors_dark_shade_0;
	}

	.title {
		font-size: 16px;
		font-family: "SkyOS-Bold";
		line-height: 1.4em;
	}

	.route {
		font-family: "SkyOS-Bold";
	}

	.interactions {
		display: flex;
		& > div {
			display: flex;
			flex-grow: 1;
		}
		.button_action {
			padding: 0 8px;
			text-transform: uppercase;
		}
	}
}

</style>