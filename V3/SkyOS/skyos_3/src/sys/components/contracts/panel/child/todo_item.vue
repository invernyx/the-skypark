<template>
	<div class="situation-box" :class="{
		'monitored': contract.is_monitored && contract.state == 'Active',
		'done': contract.path[index].done,
		'none': contract.path[index].count == 0,
		'next': contract.path[index].is_next,
		'range': contract.path[index].range && contract.is_monitored && !contract.path[index].done,
		'visited': contract.path[index].visited,
		'forgot': contract.path[index].visited && !contract.path[index].done && !contract.path[index].range && contract.template.strict_order,
		'latest': index == contract.situation_at - 1,
		'last': index == contract.path.length - 1 && !(features ? features.completed_node : true)
	}">

		<div class="list-item" v-for="(chat, chat_index) in get_chats(index)" v-bind:key="'c1' + chat_index">
			<ChatAvatar class="avatar small" :handle="chat.handles[0]" />
			<div class="content">
				<ChatUI class="full_width" :data="chat" :no_avatar="true" :recent_first="true" :collapse_messages="true" />
			</div>
		</div>

		<div class="list-item is-location" v-if="index == contract.situation_at && contract.state == 'Active' && (contract.is_monitored ? (index < contract.path.length ? !contract.path[index].range : false) : true)">
			<div class="state"></div>
			<div class="content">
				<!--<AirportUI :airport="contract.last_location_airport" :contract="contract" :index="index" :current="true"/>-->

				<div class="content-location boxed focus" v-if="!contract.is_monitored && contract.last_location_geo">
					<div>
						<div class="description">Resume location{{ contract.last_location_airport ? ' near ' + contract.last_location_airport.icao : '' }}</div>
						<div class="location"><strong>{{ contract.last_location_geo[1] }}</strong>, <strong>{{ contract.last_location_geo[0] }}</strong></div>
					</div>
					<button_action class="go small" @click.native="copyLocation">COPY</button_action>
				</div>
				<div class="content-location boxed" v-else>
					<div>
						<div class="description"><strong>Current Location</strong></div>
						<div class="location">{{ sim_location[1].toFixed(6) }}, {{ sim_location[0].toFixed(6) }}</div>
					</div>
					<button_action class="outlined small" @click.native="copyLocation">COPY</button_action>
				</div>

			</div>
		</div>

		<div class="list-item">
			<div class="state"></div>
			<div class="content">
				<AirportUI :airport="situation.airport" :contract="contract" v-if="situation.airport" :index="index"/>
				<div class="content-location boxed" v-else>
					<div>
						<div class="description">{{ index > 0 ? contract.situations[index - 1].dist_to_next > 1 ? contract.situations[index - 1].dist_to_next.toLocaleString('en-gb') + '&thinsp;nm' : "Come back" : "Get" }} to {{ situation.label.length ? situation.label : 'location' }}</div>
						<div class="location"><strong>{{ situation.location[1] }}</strong>, <strong>{{ situation.location[0] }}</strong></div>
					</div>
				</div>
				<div>
					<div v-for="(action, index1) in contract.path[index].actions" v-bind:key="index1" class="list-item sub-items" :class="{ 'completed': action.completed }">
						<div class="state"></div>
						<!-- Pickup Cargo-->
						<div class="content" v-if="contract.manifests.cargo.filter(x => x.pickup_id == action.id).length">
							<div class="description">Cargo Pickups</div>
							<div class="boxed">
								<ManifestUI v-for="(cargo, index2) in contract.manifests.cargo.filter(x => x.pickup_id == action.id)" v-bind:key="'c' + index2" :index="contract.manifests.cargo.indexOf(cargo)" :range="contract.path[index].range" :cargo="cargo" :contract="contract" :action="action" />
							</div>
						</div>
						<!-- Dropoff Cargo-->
						<div class="content" v-else-if="contract.manifests.cargo.filter(x => x.dropoff_ids.includes(action.id)).length">
							<div class="description">Cargo Dropoffs</div>
							<div class="boxed">
								<ManifestUI v-for="(cargo, index2) in contract.manifests.cargo.filter(x => x.dropoff_ids.includes(action.id))" v-bind:key="'c' + index2" :index="contract.manifests.cargo.indexOf(cargo)" :range="contract.path[index].range" :cargo="cargo" :contract="contract" :action="action" />
							</div>
						</div>
						<!-- Pickup PAX-->
						<div class="content" v-else-if="contract.manifests.pax.filter(x => x.pickup_id == action.id).length">
							<div class="description">Passenger Pickups</div>
							<div class="boxed">
								<ManifestPAXUI v-for="(pax, index2) in contract.manifests.pax.filter(x => x.pickup_id == action.id)" v-bind:key="'c' + index2" :index="contract.manifests.pax.indexOf(pax)" :range="contract.path[index].range" :pax="pax" :contract="contract" :action="action" />
							</div>
						</div>
						<!-- Dropoff PAX-->
						<div class="content" v-else-if="contract.manifests.pax.filter(x => x.dropoff_ids.includes(action.id)).length">
							<div class="description">Passenger Dropoff</div>
							<div class="boxed">
								<ManifestPAXUI v-for="(pax, index2) in contract.manifests.pax.filter(x => x.dropoff_ids.includes(action.id))" v-bind:key="'c' + index2" :index="contract.manifests.pax.indexOf(pax)" :range="contract.path[index].range" :pax="pax" :contract="contract" :action="action" />
							</div>
						</div>
						<!-- Default-->
						<div class="content" v-else>
							<div class="description">{{ action.action + " " + action.description }}</div>
							<div class="interactions" v-if="contract.is_monitored">
								<button_action
								v-for="(interaction, index) in contract.interactions.filter(x => x.id == action.id && x.label != '' && x.essential)"
								v-bind:key="index"
								@click.native="interact($event, interaction)"
								class="go" :class="{ 'disabled': !interaction.enabled }">
									<span>{{ interaction.label }}</span>
									<span v-if="interaction.contract_expire"><countdown :precise="true" :stop_zero="true" :time="interaction.contract_expire"></countdown></span>
								</button_action>
							</div>
						</div>
					</div>
				</div>
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
import ManifestPAXUI from './todo_manifest_pax.vue'
import Vue from 'vue';
import { Features } from './todo.vue'
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: {
		contract :Contract,
		situation :Object,
		features: {
			type: Object as () => Features
		}
	},
	components: {
		ChatUI,
		ChatAvatar,
		AirportUI,
		ManifestUI,
		ManifestPAXUI
	},
	data() {
		return {
			index: this.contract.situations.indexOf(this.situation),
			sim_location: [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat],
			nearest_airport: this.$os.simulator.location.NearestAirport,
		}
	},
	methods: {

		get_chats(index :number) {
			var output = [] as Chat[];
			this.contract.memos.forEach(chat => {
				var messages = chat.messages.filter(x => x.situation_index == index);
				if(messages.length > 0) {
					output.push(new Chat({
						handles: chat.handles,
						id: chat.id,
						messages: messages
					}));
				}
			});
			return output;
		},

		interact(ev: Event, interaction: any) {
			this.$os.contract_service.interact(this.contract, 'interaction', {
				id: this.contract.id,
				link: interaction.id,
				verb: interaction.verb,
				data: {},
			});
		},

		copyLocation() {
			if(!this.contract.is_monitored && this.contract.last_location_airport) {
				Eljs.copyTextToClipboard(this.contract.last_location_airport.icao);
				this.$os.modals.add({
					type: 'notify',
					title: this.contract.last_location_airport.icao,
					text: [
						this.contract.last_location_airport.icao +' has been copied to your clipboard.',
						'You can paste this ICAO in the search field of the MSFS world map.'
					],
				});
			} else {

				let location = this.sim_location;
				if(!this.contract.is_monitored && this.contract.last_location_geo) {
					location = this.contract.last_location_geo;
				}

				Eljs.copyTextToClipboard(location[1] + ', ' + location[0]);
				this.$os.modals.add({
					type: 'notify',
					title: 'Coordinates',
					text: [
						'Coordinates have been copied.',
						location[1] + ', ' + location[0],
						'You can paste this location in the search field of the MSFS world map.',
						'Be aware that MSFS does not allow you to start on the ground when using coordinates.'
					],
				});
			}

		},

		listener_sim(wsmsg :any) {
			switch(wsmsg.name){
				case 'meta': {
					this.sim_location = [this.$os.simulator.location.Lon, this.$os.simulator.location.Lat]
					this.nearest_airport = this.$os.simulator.location.NearestAirport;
					break;
				}
			}
		}
	},
	created() {
		this.$os.eventsBus.Bus.on('sim', this.listener_sim);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('sim', this.listener_sim);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.todolist {
	&.small {
		.situation-box {
			.list-item {
				padding-bottom: 4px;
				padding-top: 4px;

				&:after {
					top: 26px;
					left: 9px;
					margin-left: -1px;
					height: calc(100% - 25px);
				}
				.state {
					min-width: 15px;
					height: 15px;
					margin-right: 5px;
					&::after {
						mask-size: 130%;
						mask-position: center;
					}
				 }
				.avatar {
					min-width: 20px;
					height: 20px;
					margin-right: 5px;
					margin-top: -1px;
					margin-left: -1px;
				}
				&.is-location {
					.state {
						min-width: 18px;
						height: 18px;
						border: 0;
						&::after {
							mask-size: 100%;
						}
					}
				}
				&.sub-items {
					&:after {
						top: 30px;
						left: 9px;
						margin-left: -1px;
						height: calc(100% - 25px);
					}
					&:first-of-type {
						margin-top: 16px;
						&:before {
							top: 0;
							left: 9px;
							opacity: 0.5;
							height: 12px;
							margin-top: -13px;
							margin-left: -1px;
						}
					}
				}
			}
		}

	}
}

.situation-box {
	position: relative;
	z-index: 5;

	.theme--bright & {
		color: $ui_colors_bright_shade_5;
		&.range {
			& > .list-item {
				.state {
					&::after {
						background-color: $ui_colors_bright_shade_5;
					}
				}
			}
			&.monitored {
				& > .list-item {
					.sub-items {
						.state {
							background-color: $ui_colors_bright_button_info;
							border-color: $ui_colors_bright_button_info;
							&::after {
								background-color: $ui_colors_bright_shade_0;
							}
						}
					}
				}
			}
		}

		&.forgot {
			&.monitored {
				& > .list-item {
					.state {
						background: $ui_colors_bright_button_warn;
						border-color: $ui_colors_bright_button_warn;
						&::after {
							background-color: $ui_colors_bright_shade_5;
						}
					}
				}
			}
		}

		&.done {
			& > .list-item {
				.state {
					&:after {
						background-color: $ui_colors_bright_button_info;
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
						background-color: $ui_colors_bright_button_info;
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
							background-color: $ui_colors_bright_button_info;
						}
					}
				}
			}
		}

		&.next {
			.is-location {
				.state {
					background: transparent;
					border-color: transparent;
				}
			}
			&.monitored {
				.is-location {
					.state {
						background: transparent;
						border-color: transparent;
						&::after {
							background-color: $ui_colors_bright_shade_5;
						}
					}
					&:after {
						background-color: rgba($ui_colors_bright_shade_5, 0.4);
						opacity: 0.5;
					}
				}
			}
		}

		&.latest {
			&.monitored {
				.list-item {
					&:last-child {
						&:after {
							//background-color: transparent;
							//background-image: linear-gradient(to bottom, rgba($ui_colors_bright_button_info, 1) 0%, rgba($ui_colors_bright_shade_5, 0.2) 100%);
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
				background-color: rgba($ui_colors_bright_shade_5, 0.4);
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
					background-color: rgba($ui_colors_bright_shade_0, 1);
					@include shadowed_shallow($ui_colors_bright_shade_5);
					&.boxed.focus {
						background: $ui_colors_bright_button_info;
						color: $ui_colors_bright_shade_0;
						//border: 2px solid $ui_colors_bright_button_info;
					}
				}
				.boxed {
					background-color: rgba($ui_colors_bright_shade_0, 1);
					@include shadowed_shallow($ui_colors_bright_shade_5);
				}
			}
			&.sub-items {
				&:first-of-type {
					&:before {
						background-color: rgba($ui_colors_bright_shade_5, 0.4);
					}
				}
			}

		}
	}

	.theme--dark & {
		color: $ui_colors_dark_shade_5;

		&.range {
			& > .list-item {
				.state {
					&::after {
						background-color: $ui_colors_dark_shade_5;
					}
				}
			}
			&.monitored {
				& > .list-item {
					.sub-items {
						.state {
							background-color: $ui_colors_dark_button_info;
							border-color: $ui_colors_dark_button_info;
							&::after {
								background-color: $ui_colors_dark_shade_5;
							}
						}
					}
				}
			}
		}

		&.forgot {
			&.monitored {
				& > .list-item {
					.state {
						background: $ui_colors_dark_button_warn;
						border-color: $ui_colors_dark_button_warn;
						&::after {
							background-color: $ui_colors_dark_shade_0;
						}
					}
				}
			}
		}

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
			}
			&.monitored {
				& > .list-item {
					&:after {
						background-color: $ui_colors_dark_button_info;
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
							background-color: $ui_colors_dark_button_info;
						}
					}
				}
			}
		}

		&.next {
			.is-location {
				.state {
					background: transparent;
					border-color: transparent;
				}
			}
			&.monitored {
				.is-location {
					.state {
						background: transparent;
						border-color: transparent;
						&::after {
							background-color: $ui_colors_dark_shade_5;
						}
					}
					&:after {
						background-color: rgba($ui_colors_dark_shade_5, 0.4);
						opacity: 0.5;
					}
				}
			}
		}

		&.latest {
			&.monitored {
				.list-item {
					&:last-child {
						&:after {
							//background-color: transparent;
							//background-image: linear-gradient(to bottom, rgba($ui_colors_dark_button_info, 1) 0%, rgba($ui_colors_dark_shade_5, 0.2) 100%);
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
				background: rgba($ui_colors_dark_shade_5, 0.4);
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
					background-color: rgba($ui_colors_dark_shade_0, 0.5);
					&.boxed.focus {
						background: $ui_colors_dark_button_info;
						//border: 1px solid rgba($ui_colors_dark_shade_3, 0.3);
					}
				}
				.boxed {
					background-color: rgba($ui_colors_dark_shade_0, 0.5);
					border-color: rgba($ui_colors_dark_shade_5, 0.2);
				}
			}
			&.sub-items {
				&:first-of-type {
					&:before {
						background-color: rgba($ui_colors_dark_shade_5, 0.4);
					}
				}
				&.completed {
					&:before {
						background-color: $ui_colors_dark_button_info;
					}
				}
			}
		}

	}

	& > div {
		position: relative;
	}

	&.last {
		.list-item {
			&:last-child {
				&::after {
					display: none;
				}
			}
		}
	}

	&.next {
		.list-item {
			& > .state {
				@keyframes list-item-state-bounce {
					0%   { transform: scale(1); }
					10%  { transform: scale(1.2); }
					100% { transform: scale(1); }
				}
				animation-name: list-item-state-bounce;
				animation-duration: 1s;
				&::after {
					opacity: 1;
					mask-image: url(../../../../../sys/assets/icons/state_mask_todo.svg);
				}
			}
		}
		&.monitored {
			.list-item {
				//& > .state {
					//opacity: 0.5;
					//border-color: transparent;
				//}
				&.is-location {
					.state {
						opacity: 1;
					}
				}
			}
		}
	}

	&.range {
		.list-item {
			& .state {
				@keyframes list-item-state-bounce {
					0%   { transform: scale(1); }
					10%  { transform: scale(1.2); }
					100% { transform: scale(1); }
				}
				animation-name: list-item-state-bounce;
				animation-duration: 1s;
				opacity: 1;
				border-color: transparent;
				&::after {
					mask-image: url(../../../../../sys/assets/icons/state_mask_location.svg);
				}
			}
			.sub-items {
				.state {
					&::after {
						mask-image: url(../../../../../sys/assets/icons/state_mask_todo.svg);
					}
				}
			}
			.background-map {
				height: 65px;
				&-image {
					opacity: 0.4;
				}
			}
		}
	}

	&.forgot {
		.list-item {
			& > .state {
				@keyframes list-item-state-bounce {
					0%   { transform: scale(1); }
					10%  { transform: scale(1.2); }
					100% { transform: scale(1); }
				}
				animation-name: list-item-state-bounce;
				animation-duration: 1s;
				&::after {
					opacity: 1;
					mask-image: url(../../../../../sys/assets/icons/state_mask_alert.svg);
				}
			}
		}
		&.monitored {
			.list-item {
				& > .state {
					animation-iteration-count: infinite;
					opacity: 1;
				}
			}
		}
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
				&::after {
					opacity: 1;
					mask-image: url(../../../../../sys/assets/icons/state_mask_done.svg);
				}
			}
		}
		&.monitored {
			.list-item {
				& > .state {
					opacity: 1;
				}
				&:after {
					opacity: 1;
				}
			}
		}
	}

	&.hidden {
		height: 0;
		opacity: 0;
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
		&:after {
			content: '';
			position: absolute;
			top: 42px;
			opacity: 0.5;
			width: 2px;
			height: calc(100% - 40px);
			left: 14px;
			margin-left: -1px;
			transition: height 0.3s ease-out;
		}
		.state {
			position: relative;
			min-width: 24px;
			height: 24px;
			border-radius: 50%;
			margin-right: 10px;
			align-self: start;
			overflow: hidden;
			opacity: 0.3;
			border: 2px solid transparent;
			background-size: 100%;
			background-repeat: no-repeat;
			background-position: center;
			animation-name: list-item-state-bounce-out;
			animation-duration: 1s;
			//transition: opacity 0.4s ease-out;
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
		.avatar {
			position: relative;
			min-width: 28px;
			height: 28px;
			border-radius: 50%;
			margin-right: 10px;
			align-self: start;
			overflow: hidden;
			opacity: 1;
			background-size: 100%;
			background-repeat: no-repeat;
			background-position: center;
			animation-name: list-item-state-bounce-out;
			animation-duration: 1s;
			transition: opacity 0.4s ease-out;
		}
		.content {
			position: relative;
			display: flex;
			flex-direction: column;
			align-self: center;
			flex-grow: 1;
			.boxed {
				padding: 4px 8px;
				border-radius: 8px;
				margin-top: 4px;
				margin-bottom: 4px;
				//margin-left: -4px;
				border: 1px solid transparent;
			}
			&-location {
				padding: 4px;
				padding-left: 8px;
				border-radius: 8px;
				flex-grow: 1;
				display: flex;
				justify-content: space-between;
				.collapser {
					transition: transform 0.1s cubic-bezier(.3,0,.24,1);
					will-change: transform;
					&:hover {
						transform: scale(1.02);
					}
					&.collapser_expanded {
						transform: scale(1);
					}
				}
				.button_action  {
					padding: 0 8px;
					border-radius: 6px;
					margin-right: -4px;
					text-transform: uppercase;
				}
			}
		}
		.country {
			padding-top: 4px;
			padding-bottom: 4px;
		}
		.location {
			font-size: 12px;
		}
		.interactions {
			display: flex;
			.button_action  {
				padding: 2px 8px
			}
		}
		.collapser {
			flex-grow: 1;
			&.collapser_expanded {
				& .list-item-background-map {
					height: 200px;
				}
			}
		}

		&.sub-items {
			padding-bottom: 2px;
			padding-top: 2px;
			margin-left: -8px;
			&:after {
				content: '';
				position: absolute;
				top: 36px;
				opacity: 0.5;
				height: calc(100% - 40px);
				left: 14px;
				margin-left: -1px;
				transition: height 0.3s ease-out;
			}
			&:first-of-type {
				margin-top: 16px;
				&:before {
					content: '';
					position: absolute;
					top: 0;
					left: 14px;
					opacity: 0.5;
					height: 9px;
					margin-top: -13px;
					margin-left: -1px;
					width: 2px;
					transition: height 0.3s ease-out;
				}
			}
			&:last-of-type {
				&:after {
					display: none;
				}
			}
		}

		&.is-location {
			.state {
				animation-name: none;
				&::after {
					mask-image: url(../../../../../sys/assets/icons/state_mask_location.svg);
				}
			}
		}

	}

}
</style>