<template>
	<div class="todolist" v-if="contract.path">

		<!-- Completed -->
		<collapser :preload="false" :withArrow="true" :default="false" v-if="completed_situations.length">
			<template v-slot:title="props">
				<Button_action v-if="!props.expanded" class="outlined small justify h_edge_margin_bottom_half">
					<span class="model-state"></span>
					<span>Show {{ completed_situations.length }} Completed</span>
					<span class="collapser_arrow"></span>
				</Button_action>
				<Button_action v-else class="outlined small justify h_edge_margin_bottom_half">
					<span class="model-state"></span>
					<span>Hide Completed</span>
					<span class="collapser_arrow"></span>
				</Button_action>
			</template>
			<template v-slot:content>
				<Todo_item v-for="situation in completed_situations" v-bind:key="contract.situations.indexOf(situation)" :features="features" :contract="contract" :situation="situation" />
			</template>
		</collapser>

		<!-- Upcoming -->
		<Todo_item v-for="situation in upcoming_situations" v-bind:key="contract.situations.indexOf(situation)" :features="features" :contract="contract" :situation="situation" />

		<!-- Collapsed Upcoming -->
		<collapser :preload="false" :withArrow="true" :default="false" :isReversed="true" v-if="upcoming_situations_collapse.length">
			<template v-slot:title="props">
				<Button_action v-if="!props.expanded" class="outlined small justify h_edge_margin_top_half">
					<span class="model-state"></span>
					<span>Show {{ upcoming_situations_collapse.length}} more</span>
					<span class="collapser_arrow"></span>
				</Button_action>
				<Button_action v-else class="outlined small justify h_edge_margin_top_half">
					<span class="model-state"></span>
					<span>Show less</span>
					<span class="collapser_arrow"></span>
				</Button_action>
			</template>
			<template v-slot:content>
				<Todo_item v-for="situation in upcoming_situations_collapse" v-bind:key="contract.situations.indexOf(situation)" :features="features" :contract="contract" :situation="situation" />
			</template>
		</collapser>



		<div class="situation-box monitored" :class="{ 'done': contract.state == 'Succeeded' }" v-if="features ? features.completed_node : true">
			<div class="list-item final">
				<div class="state"></div>
				<div class="content">
					<div class="description">Completed</div>
				</div>
				<div class="interaction"></div>
			</div>
		</div>

	</div>
</template>

<script lang="ts">
import Contract, { Situation } from '@/sys/classes/contracts/contract';
import Chat from '@/sys/classes/messaging/chat';
import ChatUI from '@/sys/components/messaging/chat.vue';
import ChatAvatar from '@/sys/components/messaging/chat_avatar.vue';
import AirportUI from './todo_airport.vue'
import ManifestUI from './todo_manifest_cargo.vue'
import Todo_item from './todo_item.vue';
import Vue from 'vue';
import Button_action from '@/sys/components/button_action.vue';
import { watch } from 'fs';

export interface Features {
	current_location,
	completed_node,
	all_next
}

export default Vue.extend({
	props: {
		contract :Contract,
		chat: Chat,
		features: {
			type: Object as () => Features
		}
	},
	components: {
		ChatUI,
		ChatAvatar,
		AirportUI,
		ManifestUI,
		Todo_item,
		Button_action
	},
	data() {
		return {
			upcoming_situations_collapse: null as Situation[],
			upcoming_situations: null as Situation[],
			completed_situations: null as Situation[],
		}
	},
	methods: {
		update_lists() {
			const limit = 2;
			this.completed_situations = this.contract.situations.filter((x, i) => this.contract.path[i].done && !(i == this.contract.situation_at))
			this.upcoming_situations_collapse = this.contract.situations.filter((x, i) => !this.contract.path[i].done).filter((x, i) => (this.features ? (this.features.all_next ? true : i >= limit) : false));
			this.upcoming_situations = this.contract.situations.filter((x, i) => !this.contract.path[i].done || (i == this.contract.situation_at)).filter((x, i) => (this.features ? (this.features.all_next ? true : i < limit) : true))
		}
	},
	created() {

	},
	beforeDestroy() {

	},
	watch: {
		'contract.path': {
			immediate: true,
			deep: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.update_lists();
				}
			}
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.todolist {
	position: relative;
	z-index: 5;

	.theme--bright & {
		color: $ui_colors_bright_shade_5;
		.situation-box {
			&.done {
				& > .list-item {
					.state {
						&:after {
							border-color: $ui_colors_bright_button_info;
						}
						& > .state {
							background-color: transparent;//$ui_colors_bright_button_info;
							border-color: $ui_colors_bright_button_info;
							&::after {
								background-color: $ui_colors_bright_shade_5;
							}
						}
					}
				}
				&.monitored {
					& > .list-item {
						&:after {
							border-color: $ui_colors_bright_button_info;
						}
						.state {
							background-color: transparent;
							border-color: $ui_colors_bright_button_info;
							&::after {
								background-color: $ui_colors_bright_shade_5;
							}
						}
						.sub-items {
							&:before,
							&::after {
								opacity: 1;
								border-color: $ui_colors_bright_button_info;
							}
						}
					}
				}
			}
			.list-item {
				&-box {
					background: rgba($ui_colors_bright_shade_2, 0.3);
					border: 1px solid rgba($ui_colors_bright_shade_3, 0.3);
				}
				&:after {
					border-left: 2px solid rgba($ui_colors_bright_shade_5, 0.4);
				}
				.state {
					border-color: $ui_colors_bright_shade_5;
					&::before,
					&::after {
						background-color: $ui_colors_bright_shade_5;
					}
				}
				.content {
					&-location {
						background: $ui_colors_bright_button_info;
						color: $ui_colors_bright_shade_0;
						@include shadowed_shallow($ui_colors_bright_shade_5);
					}
					.boxed {
						background-color: rgba($ui_colors_bright_shade_0, 1);
						@include shadowed_shallow($ui_colors_bright_shade_5);
					}
				}
				&.sub-items {
					&:first-of-type {
						&:before {
							border-color: rgba($ui_colors_bright_shade_5, 0.4);
						}
					}
				}

			}
		}
	}

	.theme--dark & {
		color: $ui_colors_dark_shade_5;
		.situation-box {
			&.done {
				& > .list-item {
					.state {
						&:after {
							border-color: $ui_colors_dark_button_info;
						}
						& > .state {
							background-color: transparent;//$ui_colors_dark_button_info;
							border-color: $ui_colors_dark_button_info;
							&::after {
								background-color: $ui_colors_dark_shade_5;
							}
						}
					}
				}				&.monitored {
					& > .list-item {
						&:after {
							border-color: $ui_colors_dark_button_info;
						}
						.state {
							background-color: transparent;
							border-color: $ui_colors_dark_button_info;
							&::after {
								background-color: $ui_colors_dark_shade_5;
							}
						}
						.sub-items {
							&:before,
							&::after {
								opacity: 1;
								border-color: $ui_colors_dark_button_info;
							}
						}
					}
				}
			}
			.list-item {
				&-box {
					background: rgba($ui_colors_dark_shade_2, 0.3);
					border: 1px solid rgba($ui_colors_dark_shade_3, 0.3);
				}
				&:after {
					border-left: 2px solid rgba($ui_colors_dark_shade_5, 0.4);
				}
				.state {
					border-color: $ui_colors_dark_shade_5;
					&::before,
					&::after {
						background-color: $ui_colors_dark_shade_5;
					}
				}
				.content {
					&-location {
						background: $ui_colors_dark_button_info;
					}
					.boxed {
						background-color: rgba($ui_colors_dark_shade_0, 0.5);
						border-color: rgba($ui_colors_dark_shade_5, 0.2);
					}
				}
				&.sub-items {
					&:first-of-type {
						&:before {
							border-color: rgba($ui_colors_dark_shade_5, 0.4);
						}
					}
					&.completed {
						&:before {
							border-color: $ui_colors_dark_button_info;
						}
					}
				}
			}
		}
	}

	.situation-box {

		& > div {
			position: relative;
		}

		&.done {
			.list-item {
				& > .state {
					@keyframes list-item-state-bounce {
						0%   { transform: scale(1); }
						10%  { transform: scale(1.2); }
						100% { transform: scale(1); }
					}
					animation-name: list-item-state-bounce;
					animation-duration: 1s;
					opacity: 1;
					&::after {
						opacity: 1;
						mask: url(../../../../../sys/assets/icons/state_mask_done.svg);
					}
				}
			}
		}

		.list-item {
			position: relative;
			display: flex;
			flex-direction: row;
			align-items: center;
			padding-bottom: 8px;
			padding-top: 8px;
			&-box {
				padding: 4px 8px;
				margin-bottom: 4px;
				border-radius: 8px;
				&:last-of-type {
					margin-bottom: 0;
				}
			}
			.state {
				position: relative;
				min-width: 24px;
				height: 24px;
				border-radius: 50%;
				margin-right: 10px;
				align-self: start;
				overflow: hidden;
				border: 2px solid transparent;
				opacity: 0.3;
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center;
				animation-name: list-item-state-bounce-out;
				animation-duration: 1s;
				&::after {
					content: '';
					position: absolute;
					width: 100%;
					height: 100%;
					display: block;
					opacity: 0;
					transition: transform 0.4s ease-out;
					mask-position: center;
					mask-size: 100%;
					mask-repeat: no-repeat;
				}
			}

		}
	}

}
</style>