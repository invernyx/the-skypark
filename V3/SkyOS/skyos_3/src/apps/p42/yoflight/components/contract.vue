<template>
	<div class="contract" :class="{ 'selected': selected ? selected.id == contract.id : false }" @click="$emit('details')" :data-contract="contract.id" :data-scrollvisibility="'hidden'">

		<div class="content">
			<div class="columns header">

				<Company_badge class="column column_narrow" :companies="contract.template.company" :operated_for="contract.operated_for"/>
				<div class="column">
					<div class="columns">
						<div class="column">
							<div class="title" v-if="contract.title">{{ contract.title }}</div>
							<div class="title" v-else>Direct Flight</div>
						</div>
					</div>
					<div class="columns">
						<div class="column">
							<expire class="expire" :contract="contract" v-if="contract.state != 'Succeeded' && contract.state != 'Failed'"/>
							<div v-else-if="contract.state == 'Succeeded'">Completed {{ moment(contract.completed_at).from(new Date) }}</div>
						</div>
					</div>
				</div>
				<div class="column column_narrow bookmarks" v-if="contract.state != 'Listed' && contract.state != 'Succeeded'">
					<div class="bookmark info" :class="['bookmark-' + contract.state.toLowerCase()]">
						<div></div>
						<div></div>
					</div>
				</div>
			</div>
			<div class="body">
				<div class="columns">
					<div class="column">
						<distance class="distance" :amount="contract.distance" :decimals="0"/>
					</div>
					<div class="column column_narrow">
						<span class="route">{{ contract.route }}</span>
					</div>
				</div>
			</div>
		</div>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Contract from "@/sys/classes/contracts/contract";
import RecomAircraft from "@/sys/components/contracts/contract_recom_aircraft.vue"
import Company_badge from '@/sys/components/company_badge.vue';

export default Vue.extend({
	props: {
		contract: Contract,
		index :Number,
		selected: Contract
	},
	components: {
		RecomAircraft,
		Company_badge
	},
	data() {
		return {
			scroll_visible: false,
			load: false,
			theme: this.$os.userConfig.get(['ui','theme']),
		}
	},
	methods: {
		init() {
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'themechange': {
					this.theme = this.$os.userConfig.get(['ui','theme']);
					this.init();
					break;
				}
			}
		},
	},
	mounted() {
		this.init();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	},
	watch: {
		contract: {
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
		scroll_visible: {
			handler(newValue, oldValue) {
				if(newValue && !this.load) {
					this.load = true;
				}
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
@import '@/sys/scss/helpers.scss';

.contract {
	position: relative;
	overflow: hidden;
	cursor: pointer;
	margin: 6px;
	margin-bottom: 0;

	.theme--bright & {
		border-color: rgba($ui_colors_bright_shade_5, 0.4);
		.content {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		&:hover {
			.content {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
		}
		&::after {
			border-color: rgba($ui_colors_bright_shade_5, 0.2s);
		}
		&.selected {
			&::after {
				border-color: $ui_colors_bright_button_info;
			}
			.background {
				& > div {
					opacity: 0.2;
				}
			}
		}
	}
	.theme--dark & {
		border-color: rgba($ui_colors_dark_shade_5, 0.3);
		.content {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		&:hover {
			.content {
				background-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
		}
		&::after {
			border-color: rgba($ui_colors_dark_shade_5, 0.2);
		}
		&.selected {
			&::after {
				border-color: $ui_colors_bright_button_info;
			}
			.background {
				& > div {
					opacity: 0.4;
				}
			}
		}
	}

	&::after {
		content: '';
		position: absolute;
		top: 0;
		left: 0;
		bottom: 0;
		right: 0;
		transition: border 0.2s ease-out;
		border: 1px solid transparent;
		border-radius: 8px;
		pointer-events: none;
	}

	&.selected {
		&::after {
			border-width: 4px;
		}
		.content {
			border-left-color: #FFFFFF;
		}
	}

	.content {
		position: relative;
		padding: 14px 16px;
		border-radius: 8px;
		z-index: 2;
	}

	.title {
		font-size: 16px;
		font-family: "SkyOS-Bold";
		line-height: 1.2em;
		margin-bottom: 0;
		margin-right: 8px;
	}

	.body {
		margin-top: 8px;
	}

	.route {
		//font-family: "SkyOS-Bold";
		//font-size: 16px;
		//line-height: 1.2em;
		white-space: nowrap;
	}

	.bookmark {
		margin-top: -14px;
		margin-right: -4px;
		margin-left: 4px;
	}

}
</style>