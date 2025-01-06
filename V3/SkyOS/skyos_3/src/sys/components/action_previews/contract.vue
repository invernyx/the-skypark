<template>
	<div class="action_preview_contract app-box shadowed-deep nooverflow h_edge_padding">

		<div class="background" :style="{ 'background-image': 'url(' + contract.image_url + ')'}"></div>

		<div class="content">
			<div class="columns">
				<div class="column">
					<data_stack v-if="contract.title" class="start small">
						<template v-slot:value>
							{{ contract.title }}
						</template>
						<template v-slot:label>
							{{ contract.route }}
							<span class="dot"></span>
							<distance :amount="contract.distance" :decimals="0"/>
							<span class="dot"></span>
							<currency class="reward_bux" :amount="contract.reward_bux" :decimals="0"/>
						</template>
					</data_stack>
					<data_stack v-else class="start small">
						<template v-slot:value>
							{{ contract.route }}
						</template>
						<template v-slot:label>
							<distance :amount="contract.distance" :decimals="0"/>
							<span class="dot"></span>
							<currency class="reward_bux" :amount="contract.reward_bux" :decimals="0"/>
						</template>
					</data_stack>
				</div>
				<div class="column column_narrow column_justify_center">
					<div class="buttons_list shadowed-shallow">
						<button_action class="arrow small info" @click.native="expand">Open</button_action>
					</div>
				</div>
			</div>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import ActionPreviewData from '@/sys/classes/action_preview';
import Contract from '@/sys/classes/contracts/contract';

export default Vue.extend({
	props: {
		app: AppInfo,
		data: ActionPreviewData,
	},
	components: {
	},
	data() {
		return {
			contract: null as Contract
		}
	},
	methods: {
		expand() {
			this.$os.routing.goTo({ name: 'p42_contrax_contract', params: { id: this.contract.id, contract: this.contract }})
		},

		listener_navigate(wsmsg :any) {
			//switch(wsmsg.name){
			//	case 'to': {
			//		this.$emit('close');
			//		break;
			//	}
			//}
		},
	},
	beforeMount() {
		this.contract = this.data.data as Contract;
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	},
	watch: {
		data() {
			this.contract = this.data.data as Contract;
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.action_preview_contract {
	max-width: 400px;

	.background {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		opacity: 0.8;
		background-size: cover;
		background-position: center;
		filter: blur(5px);
		opacity: 0.4;
		transition: background 0.4s ease-out;
	}

	.content {
		position: relative;
		z-index: 2;
	}

	.data-stack {
		margin-right: 32px;
	}


	.location {
		padding: 8px;
		margin: -8px;
		border-radius: 6px;
		margin-top: 4px;
		.theme--bright & {
			background: rgba($ui_colors_bright_shade_0,0.2);
		}
		.theme--dark & {
			background: rgba($ui_colors_dark_shade_0,0.2);
		}
	}
}

</style>