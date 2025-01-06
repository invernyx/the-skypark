<template>
	<div class="os-notifications" :class="{ 'is-open': isOpen }">
		<div class="os-notifications-tray app-box">
			<div>
				<scroll_view :scroller_offset="{ top: 30, bottom: 10 }" :dynamic="true">
					<div class="os-notifications-tray-list">
						<div class="os-notifications-tray-controls">
							<div>
								<div class="os-notifications-tray-label">Notification Center</div>
							</div>
							<div>
								<button_action @click.native="$os.notifications.clearAllNotifications(); $emit('close'); $emit('clear')" v-if="list.filter(x => x.CanDismiss !== false).length > 0">Clear all</button_action>
								<button_action class="cancel icon icon-close" @click.native="$emit('close')"></button_action>
							</div>
						</div>
						<Notification
							v-for="(notification, index) in list.slice().reverse()"
							v-bind:key="index"
							:isvisible="true"
							:notification="notification"
							@click.native="notification_clickList"
						/>
						<div v-if="!list.length" class="no-notif">
							No unread notifications 👍
						</div>
					</div>
				</scroll_view>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
	props: ['theme', 'isOpen', 'isShaded'],
	components: {
		Notification: () => import("@/sys/components/notifications/notification.vue"),
	},
	data() {
		return {
			list: []
		};
	},
	mounted() {
		this.$os.eventsBus.Bus.on('notif-svc', this.listener_notif);
	},
	methods: {
		notification_clickList() {
			if(this.$os.notifications.list.length == 0) {
				this.$emit('close');
			}
		},

		listener_notif(wsmsg :any) {
			switch(wsmsg.name){
				default: {
					this.list = wsmsg.payload.list;
					break;
				}
			}
		}
	},
	//watch: {
	//	'$os.notifications.list': {
	//		handler: function (val, oldVal) {
	//			this.list = val;
	//		},
	//		immediate: true,
	//	}
	//}

});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
	.os-notifications {
		position: absolute;
		justify-content: stretch;
		top: 0;
		right: $edge-margin;
		padding-top: $status-size;
		max-width: 500px;
		min-width: 350px;
		width: 50%;
		z-index: 3;
		transform: translateY(calc(-100% - 50px));
		will-change: transform;
		transition: transform 0.2s cubic-bezier(.5,0,.5,1);
		pointer-events: none;

		.theme--bright & {
			&-tray {
				//border: 1px solid rgba($ui_colors_bright_shade_5, 0.2);
				background-color: $ui_colors_bright_shade_4;
				color: $ui_colors_bright_shade_0;
				@include shadowed_deep($ui_colors_bright_shade_5);
				//border-top: 0;
			}
		}

		.theme--dark & {
			&-tray {
				//border: 1px solid rgba($ui_colors_dark_shade_5, 0.2);
				background-color: $ui_colors_dark_shade_4;
				color: $ui_colors_dark_shade_0;
				@include shadowed_deep($ui_colors_dark_shade_0);
				//border-top: 0;
			}
		}

		&.is-open {
			pointer-events: all;
			transform: translateY(0);
			transition: transform 0.2s cubic-bezier(.5,0,.5,1);
		}

		&-tray {
			display: flex;
			flex-direction: column;
			align-items: stretch;
			border-radius: 26px;
			border-top-left-radius: 16px;
			border-top-right-radius: 16px;
			overflow: hidden;
			&-list {
				padding: 8px;
			}
			&-label {
				font-family: "SkyOS-SemiBold";
			}
			&-controls {
				display: flex;
				margin-left: 4px;
				margin-bottom: 8px;
				& > div {
					display: flex;
					align-items: center;
					flex-grow: 1;
					&:last-child {
						justify-content: flex-end;
					}
					.button_action {
						padding: 0.2em 0.5em;
						margin-left: 4px;
					}
				}
			}

			& > div {
				//mask-image: linear-gradient(
				//	to bottom,
				//	rgba(0, 0, 0, 0.1) $status-size - 15,
				//	rgba(0, 0, 0, 1) $status-size,
				//	rgba(0, 0, 0, 1) calc(100% - 20px),
				//	rgba(0, 0, 0, 0) 100%
				//);
			}

			.scroll_view {
				max-height: 500px;
			}

		}

		.no-notif {
			display: flex;
			justify-content: center;
			margin: 8px;
		}
	}
</style>