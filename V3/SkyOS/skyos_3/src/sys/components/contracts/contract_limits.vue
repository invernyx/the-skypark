<template>
	<div class="limits" v-if="limits.length">

		<div class="list list-detailed" v-if="contract.state == 'Active' && contract.is_monitored">
			<div v-if="summary">
				<div class="state"></div>
				<div class="text">
					<expire class="expire" :contract="contract" v-if="contract.state != 'Succeeded' && contract.state != 'Failed'"/>
					<div v-else-if="contract.state == 'Succeeded'">Completed on <time_date :date="contract.completed_at"/></div>
				</div>
			</div>
			<div v-for="(limit, index) in limits" v-bind:key="index" :class="{ 'enabled': limit.enabled }">
				<div class="state"></div>
				<div class="text" v-html="translateLabel(limit)"></div>
			</div>
		</div>
		<div class="list list-summary" v-else>
			<div v-if="summary">
				<div class="state"></div>
				<div class="text">
					<expire class="expire" :contract="contract" v-if="contract.state != 'Succeeded' && contract.state != 'Failed'"/>
					<div v-else-if="contract.state == 'Succeeded'">Completed on <time_date :date="contract.completed_at"/></div>
				</div>
			</div>
			<div v-for="(limit, index) in limits" v-bind:key="index">
				<div class="state"></div>
				<div class="text" v-html="translateLabel(limit)"></div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import moment from 'moment';
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "limits",
	props: {
		contract: Contract,
		summary: Boolean,
	},
	data() {
		return {
			html: '',
			interval: null
		}
	},
	mounted() {
		this.interval = setInterval(() => {
			this.$forceUpdate();
		}, 5000);
	},
	beforeDestroy() {
		clearInterval(this.interval);
	},
	methods: {
		translateLabel(limit :any) {
			switch(limit.type) {
				case 'trigger_time': {

					let th = null;
					if(this.contract.state == 'Saved' || this.contract.state == 'Active' || this.contract.state == 'Listed') {
						th = Eljs.hours_ago(limit.params[0]);
					}
					const format = this.$os.userConfig.get(['ui','units','numbers']);
					const t = new Intl.DateTimeFormat(format, {
						timeStyle: 'short'
					}).format(Eljs.convertDateToUTC(limit.params[0])).replace(/([A-Z])\w+/, '').trim();

					return 'Wheels-up <strong>at ' + t + ' GMT</strong>' + (th ? ' (' + th + ')' : '');
				}
				case 'pax_required': {
					return 'Requires at least <strong>' + limit.params[0] + '</strong> passenger seat' + (limit.params[0] != 1 ? 's' : '') + '.';
				}
				case 'cargo_required': {
					return 'Requires at least <strong>' + limit.params[0] + '</strong> cargo slot' + (limit.params[0] != 1 ? 's' : '') + '.';
				}
				case 'aircraft_type': {
					return 'Requires <strong>' + limit.params[0] + '</strong>.';
				}
				case 'alt_trigger': {
					if(limit.params[0] == 0 && limit.params[2] == 'AGL') {
						return 'Stay between ground and ' + limit.params[1] + 'ft AGL';
					} else {
						return 'Stay between ' + limit.params[0] + 'ft and ' + limit.params[1] + 'ft ' + limit.params[2];
					}
				}
				case 'g_trigger': {
					return 'Stay between ' + limit.params[0] + ' and ' + limit.params[1] + ' Gs';
				}
				default: return 'unknown (' + limit.type + ')';
			}
		}
	},
	computed: {
		limits():any {
			return this.contract.limits.filter(l => this.summary ? true : l.in_summary).filter(l => l.is_prestart ? (this.contract.state == 'Listed' || this.contract.state == 'Saved') : true);
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

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
			box-sizing: border-box;
			margin-bottom: 0.25em;
			&.enabled {
				z-index: 2;
			}
			&:last-child {
				margin-bottom: 0;
			}
		}
		&.list-detailed {
			.state {
				width: 4px;
				height: 2px;
			}
		}
		&.list-summary {
			.state {
				width: 4px;
				height: 2px;
			}
		}
		.text {
			margin: 0 10px;
		}
	}
}

</style>