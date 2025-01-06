<template>
	<div class="chat_preview" :class="{ 'selected': selected ? selected.id == chat.id : false }" @click="$emit('details')" :data-chat="chat.id">
		<div class="participants">
			<ChatAvatar class="avatar" :handle="chat.handles[0]" />
		</div>
		<div class="content">
			<div class="title">{{ chat.handles[0].first_name }}</div>
			<div class="last_message">{{ chat.last_message.content }}</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Chat from '@/sys/classes/messaging/chat';
import ChatAvatar from "@/sys/components/messaging/chat_avatar.vue"

export default Vue.extend({
	props: {
		chat: Chat,
		index :Number,
		selected: Chat
	},
	components: {
		ChatAvatar
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
	},
	mounted() {
		this.init();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	},
	watch: {
		data: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
		scroll_visible: {
			immediate: true,
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

.chat_preview {
	position: relative;
	display: flex;
	overflow: hidden;
	cursor: pointer;
	padding: 14px 16px;
	border-bottom: 1px solid transparent;
	transition: background 1s cubic-bezier(0,.5,.5,1);

	.theme--bright & {
		border-bottom-color: rgba($ui_colors_bright_shade_5, 0.1);
		background: $ui_colors_bright_shade_1;
		&:hover {
			background: $ui_colors_bright_shade_2;
		}
		&.selected {
			background: $ui_colors_bright_shade_2;
		}
	}
	.theme--dark & {
		border-bottom-color: rgba($ui_colors_dark_shade_5, 0.1);
		background: $ui_colors_dark_shade_1;
		&:hover {
			background: $ui_colors_dark_shade_2;
		}
		&.selected {
			background: $ui_colors_dark_shade_2;
		}
	}

	&:hover {
		transition: background 0.2s cubic-bezier(0,.5,.5,1);
	}

	&:last-child {
		border-bottom: none;
	}

	&::after {
		content: '';
		position: absolute;
		top: 0;
		left: 0;
		bottom: 0;
		width: 80px;
		transform: translateX(-100%);
		transition: transform 1s ease-out;
		pointer-events: none;
	}

	.participants {
		display: flex;
		align-items: center;
	}

	.chat_avatar {
		width: 34px;
		height: 34px;
	}

	.content {
		position: relative;
		max-width: 100%;
		margin-left: 14px;
	}

	.title {
		font-size: 16px;
		font-family: "SkyOS-Bold";
		line-height: 1.2em;
		margin-bottom: 0;
		margin-right: 8px;
	}


	.last_message {
		position: relative;
		opacity: 0.5;
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		max-width: 200px;
	}

}
</style>