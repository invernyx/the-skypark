<template>
	<div class="chat" :class="{ 'blured': blured }">
		<scroll_view :isFixedScroll="!hasScroll" :horizontal="false" class="content" v-if="thread.Messages.length">
			<div class="thread">
				<div v-for="(messageObj, index) in thread.Messages.slice().reverse()" v-bind:key="index" class="message" :class="['type-' + messageObj.Type,  { 'outbound': messageObj.Member == null }]">
					<ChatAvatar class="avatar" v-if="messageObj.Member != null" :member="messageObj.Member" :member_name="messageObj.Member ? thread.Members[messageObj.Member] : 'You'" />
					<div class="feed">
						<div class="name">{{ messageObj.Member ? thread.Members[messageObj.Member] : "You" }} - <div class="time"><countdown :time="new Date(messageObj.Sent)"></countdown></div></div>
						<div class="group">
							<div v-if="messageObj.Type == 'audio' && !readOnly" class="badge-audio">
								<div class="label">Audio Message</div>
								<button_nav class="translucent compact" shape="forward" @click.native="replaySpeech(messageObj.Param)">Play</button_nav>
							</div>
							<div class="message_content" v-for="(message, index) in messageObj.Content.split('\n').filter(x => x.length)" v-bind:key="index">
								<div>{{ message }}</div>
							</div>
						</div>
					</div>
					<ChatAvatar class="avatar" v-if="messageObj.Member == null" :member="'default'" :member_name="'You'" />
				</div>
			</div>
		</scroll_view>
		<div v-else class="message_empty">
			<div class="message">
				<ChatAvatar class="avatar" :member="empty_message_source[0]" :member_name="empty_message_source[1]" />
				<div class="feed">
					<div class="name">{{ empty_message_source[1] }}</div>
					<div class="group">
						<div class="message_content" v-for="(message, index) in empty_message.split('\n')" v-bind:key="index">
							<div>{{ message }}</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import ChatAvatar from './chat_avatar.vue';

export default Vue.extend({
	name: "chat",
	props: ["thread", "hasScroll", "empty_message", "empty_message_source", "readOnly", "blured"],
	methods: {
		replaySpeech(msg :string) {
			this.$root.$data.services.api.SendWS('speech:play', {
				param: msg
			});
		}
	},
	components: {
		ChatAvatar
	},
	mounted() {

	},
	beforeDestroy() {
	},
	data() {
		return {
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.chat {
	position: relative;

	.theme--bright &,
	&.theme--bright,
	#app .theme--bright & {
		.message {
			.avatar {
				@include shadowed_shallow($ui_colors_bright_shade_5);
			}
			.group {
				background: rgba($ui_colors_bright_shade_0, 0.3);
				@include shadowed_shallow($ui_colors_bright_shade_5);
				.badge-audio {
					.button_nav {
						@include shadowed_shallow($ui_colors_bright_button_go);
					}
				}
			}
			&.outbound {
				.group {
					background: $ui_colors_bright_button_info;
					@include shadowed_shallow($ui_colors_bright_button_info);
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark,
	#app .theme--dark & {
		.message {
			.avatar {
				@include shadowed_shallow($ui_colors_dark_shade_0);
			}
			.group {
				background: rgba($ui_colors_dark_shade_0, 0.3);
				@include shadowed_shallow($ui_colors_dark_shade_0);
				.badge-audio {
					.button_nav {
						@include shadowed_shallow($ui_colors_dark_button_go);
					}
				}
			}
			&.outbound {
				.group {
					background: $ui_colors_dark_button_info;
					@include shadowed_shallow($ui_colors_dark_button_info);
				}
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
		$borderRadius: 0.75em;
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
		&.type {
			&-audio {
				padding-top: 15px;
				.group {
					.message_content {
						&:first-child {
							margin-top: 4px;
						}
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
			.group {
				display: flex;
				flex-direction: column;
				border-radius: $borderRadius;
				border-bottom-left-radius: $borderRadius / 2;
				border-bottom-right-radius: $borderRadius;
				.message_content {
					padding: 0.5em 0.75em;
					margin-bottom: 0.30em;
					&:first-child {
						margin-top: 0;
					}
					&:last-child {
						margin-bottom: 0;
					}
				}
				.badge-audio {
					display: flex;
					align-items: center;
					padding: 6px 10px;
					.label {
						flex-grow: 1;
						margin-right: 8px;
					}
				}
			}

		}
	}
}
</style>