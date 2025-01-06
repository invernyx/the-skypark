<template>
	<div class="app-panel-wrap">
		<div class="app-panel-content" v-if="selected_chat">
			<div class="app-panel-hit" @mouseenter="$emit('can_scroll', true)" @mouseleave="$emit('can_scroll', false)">
				<scroll_stack :sid="'p42_messages_chat'" class="app-box shadowed-deep nooverflow" :has_round_corners="true">
					<template v-slot:top>

						<div class="header controls-top h_edge_padding">
							<ChatAvatar :handle="selected_chat.handles[0]" />
							<div>
								<h2>{{ selected_chat.handles[0].first_name }}</h2>
							</div>
						</div>

					</template>
					<template v-slot:content class="confine">

						<div class="messages h_edge_padding" :class="{ 'hidden': loading }">
							<ChatUI :data="selected_chat" />
						</div>

					</template>
				</scroll_stack>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import ChatUI from '@/sys/components/messaging/chat.vue';
import ChatAvatar from "@/sys/components/messaging/chat_avatar.vue"
import Chat from '@/sys/classes/messaging/chat';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ChatAvatar,
		ChatUI
	},
	data() {
		return {
			loading: true,
			selected_chat: null as Chat,
		}
	},
	methods: {

		init() {
			const chat_id = this.$route.params.id;
			this.loading = true;
			this.$os.api.send_ws('messaging:chat:get-from-id',
				{
					id: parseInt(chat_id),
					fields: null
				},
				(chatData: any) => {
					this.selected_chat = new Chat(chatData.payload);
					window.requestAnimationFrame(() => {
						this.send_bottom();
						setTimeout(() => {
							this.send_bottom();
							this.loading = false;
						}, 200);
					});

				}
			);
		},

		send_bottom() {
			var scroller = this.$os.scrollView.get_entity('p42_messages_chat');
			if(scroller) {
				const scroll_element = scroller.SimpleBar.contentWrapperEl;
				const to_end = scroll_element.scrollHeight - scroll_element.offsetHeight;
				this.$os.scrollView.scroll_to('p42_messages_chat', 0, to_end, 0);
			}
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					if(this.$route.name == 'p42_messages_chat') {
						this.init();
					}
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.system.set_cover(true);
	},
	beforeMount() {
		//this.selected_chat = (this.$route.params as any).chat as Chat;
		this.init();
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.header {
	display: flex;
	align-items: center;
}

.messages {
	transition: 0.3s ease-out;
	&.hidden {
		opacity: 0;
		transition: none;
	}
}

.chat_avatar {
	width: 2em;
	height: 2em;
	margin-right: 10px;
}

.app-panel-wrap {
	flex-grow: 1;
	width: 100%;
}

.app-panel-content {
	max-width: 400px;
	box-sizing: border-box;
}

.app-panel-hit {
	position: absolute;
	top: 50px;
	right: 0;
	bottom: 0;
	left: 0;
}
</style>