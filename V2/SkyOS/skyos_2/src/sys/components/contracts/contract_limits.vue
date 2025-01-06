<template>
	<div class="limits">
		<div class="list list-detailed" v-if="contract.State == 'Active'">
			<div v-for="(limit, index) in contract.Limits" v-bind:key="index" :class="{ 'enabled': limit.Enabled }">
				<div class="state">{{ limit.Enabled ? 'Monitored' : 'Inactive' }}</div>
				<div class="text">{{ limit.Label }}</div>
			</div>
		</div>
		<div class="list list-summary" v-else>
			<div v-for="(limit, index) in contract.Limits" v-bind:key="index">
				<div class="state"></div>
				<div class="text">{{ limit.Type }}</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "limits",
	props: ['contract', 'template', 'essential'],
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

.limits {

	.theme--bright &,
	&.theme--bright {
		.list-detailed {
		 	& > div {
				&.enabled {
					.state {
						color: $ui_colors_bright_shade_0;
						animation-name: contract_limit_state_pulse;
						animation-duration: 1s;
						animation-iteration-count: infinite;
						animation-direction: alternate;
						@keyframes contract_limit_state_pulse {
							from {
								background: $ui_colors_bright_button_cancel;
								box-shadow: 0 0 5px rgba($ui_colors_bright_button_cancel, 1), 0 0 20px rgba($ui_colors_bright_button_cancel, 0.5);
							}
							to {
								background: rgba($ui_colors_bright_button_cancel, 0.7);
								box-shadow: 0 0 5px rgba($ui_colors_bright_button_cancel, 0.2), 0 0 20px rgba($ui_colors_bright_button_cancel, 0.2);
							}
						}
					}
				}
				.state {
					background: rgba($ui_colors_bright_shade_1, 0.3);
					color: $ui_colors_bright_shade_4;
				}
			}
		}
		.list-summary {
			.state {
				background: rgba($ui_colors_bright_shade_4, 0.4);
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		.list-detailed {
			& > div {
				&.enabled {
					.state {
						color: $ui_colors_dark_shade_5;
						animation-name: contract_limit_state_pulse;
						animation-duration: 1s;
						animation-iteration-count: infinite;
						animation-direction: alternate;
						@keyframes contract_limit_state_pulse {
							from {
								background: $ui_colors_dark_button_cancel;
								box-shadow: 0 0 5px rgba($ui_colors_dark_button_cancel, 1), 0 0 20px rgba($ui_colors_dark_button_cancel, 0.5);
							}
							to {
								background: rgba($ui_colors_dark_button_cancel, 0.7);
								box-shadow: 0 0 5px rgba($ui_colors_dark_button_cancel, 0.2), 0 0 20px rgba($ui_colors_dark_button_cancel, 0.2);
							}
						}
					}
				}
				.state {
					background: rgba($ui_colors_dark_shade_3, 0.3);
					color: $ui_colors_dark_shade_3;
				}
			}
		}
		.list-summary {
			.state {
				background: rgba($ui_colors_dark_shade_4, 0.4);
			}
		}
	}

	.list {
		display: flex;
		flex-direction: column;
		> div {
			position: relative;
			display: flex;
			align-items: center;
			border-radius: 14px;
			margin-bottom: 4px;
			box-sizing: border-box;
			&.enabled {
				z-index: 2;
			}
			&:last-child {
				margin-bottom: 0;
			}
		}
		&.list-detailed {
			.state {
				flex-grow: 0;
				border-radius: 6px;
				padding: 0 6px;
			}
		}
		&.list-summary {
			.state {
				width: 12px;
				height: 12px;
				border-radius: 50%;
			}
		}
		.text {
			margin: 0 10px;
			font-family: "SkyOS-SemiBold";
		}
	}
}

</style>