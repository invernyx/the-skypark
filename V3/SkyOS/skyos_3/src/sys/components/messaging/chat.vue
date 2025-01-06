<template>
	<div class="chat" v-if="data.messages.length" :class="{ 'no_avatar': no_avatar, 'collapse_messages': collapse_messages }">
		<div class="thread">
			<div v-for="(message, index) in recent_first ? data.messages.slice() : data.messages.slice().reverse()" v-bind:key="index" class="message" :class="['type-' + message.content_type.toLowerCase(),  { 'outbound': message.from_self }]">
				<ChatAvatar class="avatar" v-if="!no_avatar && !message.from_self" :handle="data.handles.find(x => x.id == message.handle)" />
				<div class="feed">
					<div class="name" v-if="!no_title">{{ data.handles.find(x => x.id == message.handle).first_name }} - <div class="time"><countdown :time="message.date"></countdown></div></div>
					<AudioPlayer :chat="data" :message="message" v-if="(message.content_type == 'Audio' || message.content_type == 'Call') && !read_only && !no_audio" class="type-audio"/>
					<MessageUI :message_content="message.content" />
					<Chat_contract :contract_id="id" v-for="id in message.contract_topic_ids" v-bind:key="id" />
				</div>
				<ChatAvatar class="avatar" v-if="!no_avatar && message.from_self" :handle="data.handles.find(x => x.id == message.handle)" />
			</div>
		</div>
	</div>
	<div class="chat message_empty" v-else-if="empty_message_source">
		<div class="message">
			<ChatAvatar v-if="!no_avatar" class="avatar" :member="empty_message_source[0]" :member_name="empty_message_source[1]" />
			<div class="feed">
				<div class="name">{{ empty_message_source[1] }}</div>
				<div class="group">
					<MessageUI :message_content="empty_message" />
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import ChatAvatar from './chat_avatar.vue';
import Chat from '@/sys/classes/messaging/chat';
import MessageUI from '@/sys/components/messaging/chat_message.vue';
import AudioPlayer from "@/sys/components/messaging/chat_audio_player.vue"
import Message from '@/sys/classes/messaging/message';
import Chat_contract from './chat_contract.vue';

export default Vue.extend({
	props: {
		data :Chat,
		no_avatar :Boolean,
		no_title: Boolean,
		no_audio: Boolean,
		recent_first: Boolean,
		read_only: Boolean,
		empty_message :String,
		empty_message_source :Array,
		collapse_messages: Boolean
	},
	methods: {
		init() {
			let last_unread_message = null as Message;
			let last_read = this.data.read_at_date;

			if(this.data.last_message) {
				this.data.messages.forEach(message => {
					if(message.content_type != "Call" && last_read ? message.date.getTime() > this.data.read_at_date.getTime() : true) {
						last_unread_message = message;
						this.data.read_at_date = message.date
					}
				});

				if(last_unread_message){
					this.$os.api.send_ws('messaging:chat:read', {
						id: last_unread_message.chat_id,
						date: last_unread_message.date
					});
				}
			}

		}
	},
	components: {
    ChatAvatar,
    AudioPlayer,
    MessageUI,
    Chat_contract
},
	mounted() {
		this.init();
	},
	beforeDestroy() {
	},
	data() {
		return {
			user_handle: '%user%'
		}
	},
	watch: {
		data: {
			handler: function (val, oldVal) {
				this.init();
			},
			immediate: true,
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
$borderRadius: 0.75em;

.chat {
	position: relative;

	.theme--bright & {
		color: $ui_colors_bright_shade_5;
		.message {
			.avatar {
				@include shadowed_shallow($ui_colors_bright_shade_5);
			}
			&.outbound {
				.group {
					background: $ui_colors_bright_button_info;
					@include shadowed_shallow($ui_colors_bright_button_info);
				}
			}
			.type-audio {
				//background: rgba($ui_colors_bright_shade_0, 0.3);
				@include shadowed_shallow($ui_colors_bright_shade_5);
			}
		}
	}

	.theme--dark & {
		color: $ui_colors_dark_shade_5;
		.message {
			.avatar {
				@include shadowed_shallow($ui_colors_dark_shade_0);
			}
			&.outbound {
				.group {
					background: $ui_colors_dark_button_info;
					@include shadowed_shallow($ui_colors_dark_button_info);
				}
			}
			.type-audio {
				//background: rgba($ui_colors_dark_shade_0, 0.3);
				@include shadowed_shallow($ui_colors_dark_shade_0);
			}
		}
	}

	&.blured {
		.group {
			backdrop-filter: blur(5px);
		}
	}

	.thread {
		flex-grow: 1;
	}

	.message {
		display: flex;
		flex-grow: 1;
		position: relative;
		margin-bottom: 8px;
		align-items: flex-end;
		justify-content: flex-start;
		&:last-child {
			margin-bottom: 0;
		}
		&.outbound {
			align-self: flex-end;
			justify-content: flex-end;
			.avatar {
				margin-left: 0.30em;
				margin-right: 0;
			}
			.feed {
				align-items: flex-end;
				.group {
					&:last-child {
						border-bottom-left-radius: $borderRadius;
						border-bottom-right-radius: $borderRadius / 2;
					}
				}
			}
		}
		.type-audio {
			display: flex;
			flex-direction: row;
			align-items: center;
			justify-content: space-between;
			border-radius: $borderRadius;
			padding: 0.5em 0.75em;
			flex-grow: 1;
			margin-bottom: 8px;
			.waves {
				display: flex;
				height: 14px;
				& > div {
					@keyframes chat_message_type_audio_wave {
						0%  { height: 20%; }
						25% { height: 60%; }
						50% { height: 40%; }
						75% { height: 80%; }
						100% { height: 100%; }
					}
					animation-iteration-count: infinite;
					animation-direction: alternate;
					background-color: #FFF;
					align-self: center;
					width: 4px;
					margin-right: 2px;
					border-radius: 2px;
					&:nth-child(1) {
						height: 20%;
						animation-delay: 0.3s;
						animation-duration: 1.2s;
					}
					&:nth-child(2) {
						height: 80%;
						animation-delay: 0.9s;
						animation-duration: 1.7s;
					}
					&:nth-child(3) {
						height: 40%;
						animation-delay: 0.1s;
						animation-duration: 1.3s;
					}
					&:nth-child(4) {
						height: 20%;
						animation-delay: 0.36s;
						animation-duration: 1.8s;
					}
					&:nth-child(5) {
						height: 60%;
						animation-delay: 0.2s;
						animation-duration: 1.4s;
					}
					&:nth-child(6) {
						height: 100%;
						animation-delay: 0.0s;
						animation-duration: 1.7s;
					}
					&:nth-child(7) {
						height: 70%;
						animation-delay: 0.6s;
						animation-duration: 1.9s;
					}
					&.playing {
						animation-name: chat_message_type_audio_wave;
					}
				}
			}
		}
		.avatar {
			position: relative;
			min-width: 2em;
			min-height: 2em;
			margin-left: 0;
			margin-right: 0.30em;
		}
		.feed {
			display: flex;
			flex-direction: column;
			max-width: 75%;
			.name {
				font-size: 0.75em;
				margin-bottom: 0.30em;
				margin-right: $borderRadius / 4;
				margin-left: $borderRadius / 4;
				.time {
					display: inline-block;
				}
			}

		}
	}

	&.full_width {
		.message {
			.feed {
				max-width: 100%;
			}
		}
	}

	&.no_avatar {
		.message {
			.feed {
				.group {
					border-radius: $borderRadius;
				}
			}
		}
	}
}
</style>