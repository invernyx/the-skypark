<template>
	<div class="os-notification" :class="[
		'notit-type-' + notification.Type.toLowerCase(),
		{
			'is_hidden': !expanded,
			'is_dismissable': notification.CanDismiss
		}
	]" ref="content" @click="click">
		<div>
			<div>
				<Icon :app="getApp" />
			</div>
			<div>
				<div class="os-notification-header">
					<div class="os-notification-title">{{ notification.Title }}</div>
					<div class="os-notification-time"><countdown :time="notification.Time"></countdown></div>
				</div>
				<div class="os-notification-message">{{ notification.Message }}</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Icon from "./icon.vue"
import { AppInfo } from "@/sys/foundation/app_model"

export default Vue.extend({
	name: "p42_notification",
	props: ['notification', 'isvisible'],
	components: {
		Icon
	},
	data() {
		return {
			doneTimeout: null,
			expanded: false,
		}
	},
	mounted() {
		this.expanded = this.isvisible !== undefined ? this.isvisible : true;
		if(!this.expanded) {
			const content = (this.$refs.content as HTMLElement);
			content.style.height = '0px';
		}
		this.set(this.expanded);
	},
	beforeDestroy() {
		clearTimeout(this.doneTimeout);
	},
	methods: {
		set(state :boolean) {
			clearTimeout(this.doneTimeout);
			this.expanded = state;
			const content = (this.$refs.content as HTMLElement);
			content.style.height = content.scrollHeight + 'px';
			if(!state) {
				content.style.height = content.scrollHeight + 'px';
				window.requestAnimationFrame(() => { setTimeout(() => { content.style.height = '0px'; }, 1); });
			}
			this.doneTimeout = setTimeout(() => {
				this.doneTimeout = null;
				if(state) {
					content.style.height = null;
				}
			}, 300);
		},
		click() {
			if(this.notification.CanDismiss !== false) {
				this.$os.notifications.removeNotificationFromID(this.notification.UID);
			}

			if(this.notification.App && this.notification.CanOpen) {
				this.$os.routing.goTo({ name: this.notification.App, params: this.notification.LaunchArgument });
			}
		},
	},
	computed: {
		getApp() {
			return this.$os.apps.find((x: AppInfo) => x.vendor + '_' + x.ident == this.notification.App);
		}
	},
	watch: {
		isvisible: {
			immediate: true,
			handler(newValue, oldValue) {
				if(this.$refs.content) {
					this.set(newValue);
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
	@import '@/sys/scss/sizes.scss';
	@import '@/sys/scss/colors.scss';
	@import '@/sys/scss/mixins.scss';
	.os-notification {
		position: relative;
		max-width: 600px;
		margin: 0 auto;
		margin-bottom: 8px;
		overflow: visible;
		border-radius: 18px;
		transform-origin: center top;
		@include shadowed(#000);
		transition: height 300ms ease-out, margin 300ms ease-out, transform 300ms cubic-bezier(.2,0,.4,1), opacity 100ms 200ms ease-out;

		.theme--bright & {
			& > div {
				color: $ui_colors_bright_shade_5;
				background: $ui_colors_bright_shade_1;
			}
			&.notit-type {
				&-success {
					& > div {
						color: $ui_colors_bright_shade_0;
						background: $ui_colors_dark_button_go;
					}
				}
				&-fail {
					& > div {
						color: $ui_colors_bright_shade_0;
						background: $ui_colors_dark_button_cancel;
					}
				}
			}
			&.is_dismissable {
				&:hover {
					& > div {
						background: $ui_colors_bright_shade_2;
					}
					&.notit-type {
						&-success {
							& > div {
								background: darken($ui_colors_dark_button_go, 10%);
							}
						}
						&-fail {
							& > div {
								background: darken($ui_colors_dark_button_cancel, 10%);
							}
						}
					}
				}
			}
		}

		.theme--dark & {
			& > div {
				color: $ui_colors_dark_shade_5;
				background: $ui_colors_dark_shade_2;
			}
			&.notit-type {
				&-success {
					& > div {
						color: $ui_colors_dark_shade_5;
						background: $ui_colors_dark_button_go;
					}
				}
				&-fail {
					& > div {
						color: $ui_colors_dark_shade_5;
						background: $ui_colors_dark_button_cancel;
					}
				}
			}
			&.is_dismissable {
				&:hover {
					& > div {
						background: $ui_colors_dark_shade_1;
					}
					&.notit-type {
						&-success {
							& > div {
								background: darken($ui_colors_dark_button_go, 10%);
							}
						}
						&-fail {
							& > div {
								background: darken($ui_colors_dark_button_cancel, 10%);
							}
						}
					}
				}
			}
		}

		&:hover {
			transition: background 0.1s ease-out;
		}

		&:last-child {
			margin-bottom: 0;
		}

		&-header {
			display: flex;
		}
		&-title {
			font-family: "SkyOS-SemiBold";
			flex-grow: 1;
		}
		&-time {
			margin-left: 1em;
			white-space: nowrap;
			opacity: 0.8;
		}
		&-closed {
			opacity: 0;
		}
		& > div {
			padding: 8px 10px;
			border-radius: 18px;
			display: flex;
    		align-items: center;
			transition: background 0.3s ease-out;
			& > div {
				&:first-child {
					margin-right: 12px;
				}
				&:last-child {
					flex-grow: 1;
				}
			}
		}
		&.is_dismissable {
			cursor: pointer;
		}
		&.is_hidden {
			height: 0;
			margin-bottom: 0;
			opacity: 0;
			transform: translateY(-18px) scale(0.9);
			transition: height 300ms ease-out, margin 300ms ease-out, transform 300ms cubic-bezier(.2,0,.4,1), opacity 100ms ease-out;
		}

	}
</style>