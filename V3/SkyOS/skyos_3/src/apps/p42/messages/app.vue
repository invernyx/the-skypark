<template>
	<div :class="[appName, app.nav_class]">
		<div class="app-frame">
			<scroll_stack class="app-box shadowed-deep">
				<template v-slot:content class="confine">
					<div class="searching" v-if="state.status == 1">
						<span class="title">Searching...</span>
						<p>This might take a few seconds.</p>
					</div>
					<div class="no-transponder" v-else-if="state.status == 2">
						<span class="title">No Transponder</span>
						<p>The Skypark Transponder is required to browse messages.</p>
						<p>Please start your Transponder.</p>
					</div>
					<div class="no-results" v-else-if="state.status == 3">
						<span class="title">No Messages</span>
						<p>Nothing to see here!</p>
					</div>
					<div v-else>
						<ChatBox
							v-for="(chat, index) in state.chats"
							v-bind:key="index"
							:index="index"
							:chat="chat"
							:selected="state.selected"
							@details="chat_preview(chat)"/>
					</div>
				</template>
			</scroll_stack>
		</div>
		<app_panel :app="app" :has_content="$route.matched.length > 1" :has_subcontent="ui.subframe !== null" :scroll_top="ui.panel_scroll_top">
			<transition :duration="1000">
				<router-view @scroll="ui.panel_scroll_top = $event.scrollTop"></router-view>
			</transition>
		</app_panel>
  	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import SearchStates from "@/sys/enums/search_states";
import ChatBox from "./components/chat.vue"
import Chat from '@/sys/classes/messaging/chat';

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ChatBox
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			ui: {
				panel: false,
				subframe: null,
				panel_scroll_top: 0,
			},
			state: {
				status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
				limit: 0,
				count: 0,
				chats: [] as Chat[],
				selected: null as Chat,
			}
		}
	},
	methods: {

		chats_search() {
			this.chat_preview(null);

			if(this.has_transponder) {
				this.state.status = SearchStates.Searching;

				this.$os.api.send_ws(
					'messaging:chat:get-all', {
						limit: 10,
						offset: 0 ,
						fields: {
							id: true,
							handles: true,
							last_message: {
								id: true,
								handle: true,
								date: true,
								content: true,
								content_type: true
							},
							read_at_date: true,
						}
					},
					(chatsData: any) => {

						if(chatsData.payload.length) {
							this.state.status = SearchStates.Idle;

							chatsData.payload.forEach(chat_str => {
								this.state.chats.push(new Chat(chat_str));
							});

						} else {
							this.state.status = SearchStates.NoResults;
						}
					}
				);

			}
		},

		chat_preview(chat :Chat) {
			if(chat) {

				if(this.state.selected) {
					if(this.state.selected.id == chat.id) {
						this.state.selected = null;
						this.$os.routing.goTo({ name: 'p42_messages' });
						this.app.events.emitter.emit('chat', { name: 'select', payload: null} );
						return;
					}
				}

				this.state.selected = chat;
				this.$os.routing.goTo({ name: 'p42_messages_chat', params: { id: chat.id, chat: chat }});

			} else {
				this.state.selected = null;
				this.$os.routing.goTo({ name: 'p42_messages' });
				this.app.events.emitter.emit('chat', { name: 'select', payload: null} );
			}
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					this.chats_search();
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					this.state.status = SearchStates.NoTransponder;
					break;
				}
			}
		},
	},
	mounted() {
		this.$emit('loaded');
		this.chats_search();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
	}
});
</script>

<style lang="scss">
	@import '@/sys/scss/colors.scss';

</style>