<template>
	<div class="os-notification" :class="['os-notification-' + notification.Type.toLowerCase(), { 'hidden': !expanded, 'locked': notification.CanDismiss === false }]" ref="content" @click="click">
		<div>
			<div class="os-notification-header">
				<div class="os-notification-title">{{ notification.Title }}</div>
				<div class="os-notification-time"><countdown :time="notification.Time"></countdown></div>
			</div>
			<div class="os-notification-message">{{ notification.Message }}</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_notification",
	props: ['notification', 'isvisible'],
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
				this.$os.removeNotificationFromID(this.notification.UID);
			}

			if(this.notification.App && this.notification.CanOpen) {
				this.$router.push({name: this.notification.App, params: this.notification.LaunchArgument });
			}
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
	@import './../../sys/scss/sizes.scss';
	@import './../../sys/scss/colors.scss';
	@import './../../sys/scss/mixins.scss';
	.os-notification {
		position: relative;
		max-width: 600px;
		margin: 0 auto;
		margin-bottom: 8px;
		overflow: visible;
		border-radius: 8px;
		cursor: pointer;
		@include shadowed(#000);
		transform-origin: center top;
		transition: height 300ms ease-out, margin 300ms ease-out, transform 300ms cubic-bezier(.2,0,.4,1), opacity 100ms 200ms ease-out;

		.theme--bright & {
			& > div {
				color: $ui_colors_bright_shade_5;
				background: $ui_colors_bright_shade_1;
			}
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

		.theme--dark & {
			& > div {
				color: $ui_colors_dark_shade_5;
				background: $ui_colors_dark_shade_2;
			}
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

		&-header {
			display: flex;
		}
		&-title {
			font-family: "SkyOS-SemiBold";
			flex-grow: 1;
		}
		&-time {
			font-family: "SkyOS-SemiBold";
			margin-left: 1em;
			white-space: nowrap;
		}
		&-closed {
			opacity: 0;
		}
		& > div {
			padding: 8px 10px;
			border-radius: 8px;
		}
		&.locked {
			cursor: default;
		}
		&.hidden {
			height: 0;
			margin-bottom: 0;
			opacity: 0;
			transform: translateY(-18px) scale(0.9);
			transition: height 300ms ease-out, margin 300ms ease-out, transform 300ms cubic-bezier(.2,0,.4,1), opacity 100ms ease-out;
		}

	}
</style>