<template>

	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ bottom: 15 }">
		<template v-slot:nav>
			<h2>Display</h2>
		</template>
		<template v-slot:content>
			<div class="helper_edge_padding helper_nav-margin">

				<h2>Skypad</h2>
				<div class="columns columns_margined">
					<div class="column column_h-stretch">
						<div class="buttons_list shadowed">
							<button_listed icon="theme/smaller" @click.native="setSize('-')">Smaller</button_listed>
						</div>
					</div>
					<div class="column column_h-stretch">
						<div class="buttons_list shadowed">
							<button_listed icon="theme/larger" @click.native="setSize('+')">Larger</button_listed>
						</div>
					</div>
				</div>
				<p class="notice">Easily toggle between 2 sizes using the buttons on the left of the Skypad. To assign a size to each, first, click the button, then select your desired size above.</p>

				<br>

				<div class="columns columns_margined">
					<div class="column column_h-stretch">
						<toggle v-model="topmost" @modified="setAlwaysOnTop">Always on top</toggle>
						<p class="notice">Keep the Skypad on top of other apps and the simulator. You can toggle "Always on top" at any time from the top right of the Skypad interface, next to the window controls.</p>
					</div>
				</div>

				<br>

				<div class="columns columns_margined">
					<div class="column column_h-stretch">
						<toggle v-model="stowCan" @modified="setStowCan">Double-click bezel to stow</toggle>
						<p class="notice">Double-click the left or right bezel of the Skypad to send it to the opposite edge of your display.</p>
					</div>
				</div>

				<br>

				<div class="columns columns_margined" v-if="stowCan">
					<div class="column column_h-stretch">
						<toggle v-model="stowAuto" @modified="setStowAuto" :disabled="!stowCan">Auto re-stow</toggle>
						<p class="notice">Skypad will automatically stow to the edge of the screen when you click away. Skypad must be previously stowed.</p>
					</div>
				</div>

				<br v-if="stowCan">

				<div class="columns columns_margined">
					<div class="column column_h-stretch">
						<toggle v-model="framed" @modified="setFramed">Window frame</toggle>
						<p class="notice">Improved compatibility to VR and window capture utilities. Restart your Skypad to apply.</p>
					</div>
				</div>



				<h2>Theme</h2>
				<div class="columns columns_margined">
					<div class="column column_h-stretch">
						<div class="buttons_list shadowed theme--bright">
							<button_listed @click.native="setTheme('theme--bright')">Light</button_listed>
						</div>
					</div>
					<div class="column column_h-stretch">
						<div class="buttons_list shadowed theme--dark">
							<button_listed @click.native="setTheme('theme--dark')">Dark</button_listed>
						</div>
					</div>
				</div>
				<br>
				<div class="columns columns_margined">
					<div class="column column_h-stretch">
						<toggle v-model="themeAuto" @modified="setAutoTheme">Automatic theme switching</toggle>
						<p class="notice">Automatically set the theme of your Skypad to match the local time in-sim.</p>
					</div>
				</div>

				<h2>Discord</h2>
				<div class="columns columns_margined">
					<div class="column column_h-stretch">
						<toggle v-model="discord_presence" @modified="changeSetting('discord_presence', $event === true ? '1' : '0')" :disabled="!$root.$data.state.services.api.connected">Enable rich presence</toggle>
						<p class="notice">Share what you fly to your friends on Discord.</p>
					</div>
				</div>


			</div>
		</template>
		<!--
		<template v-slot:tab>
			<div></div>
		</template>
		-->
	</content_controls_stack>

</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_settings_display",
	data() {
		return {
			discord_presence: this.$os.transponderSettings.discord_presence == '1',
			topmost: false,
			themeAuto: false,
			framed: false,
			stowCan: true,
			stowAuto: true,
		}
	},
	methods: {
		setTheme(theme :string) {
			this.setAutoTheme(false);
			this.$os.setConfig(['ui', 'theme'], theme);
		},
		setAutoTheme(state :boolean) {
			this.$os.setConfig(['ui','themeAuto'], state);
		},
		setAlwaysOnTop(state :boolean) {
			(this.$root as any).setTopmost(state);
		},
		setSize(change: string) {
			(this.$root as any).changeSize(change);
		},
		setFramed(state :boolean) {
			(this.$root as any).setWindowFramed(state);
		},
		setStowAuto(state :boolean) {
			(this.$root as any).setWindowStowAuto(state);
		},
		setStowCan(state :boolean) {
			(this.$root as any).setWindowStowCan(state);
		},
		listenerConfig(path: string[], value :any){
			switch(path[0]){
				case 'ui': {
					switch(path[1]){
						default: {
							this[path[1]] = this.$os.getConfig([path[0],path[1]]);
							break;
						}
					}
					break;
				}
			}
		},

		changeSetting(tag :string, state :string) {
			this.$root.$data.services.api.SendWS('transponder:set', {
				param: tag,
				value: state,
			});
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.discord_presence = wsmsg.payload.set.discord_presence == '1';
							break;
						}
					}
					break;
				}
			}
		},
	},
	created() {
		this.framed = this.$os.getConfig(['ui','framed']);
		this.topmost = this.$os.getConfig(['ui','topmost']);
		this.stowCan = this.$os.getConfig(['ui','stowCan']);
		this.stowAuto = this.$os.getConfig(['ui','stowAuto']);
		this.themeAuto = this.$os.getConfig(['ui','themeAuto']);
		this.$root.$on('ws-in', this.listenerWs);
		this.$root.$on('configchange', this.listenerConfig);
	},
	beforeDestroy() {
		this.$root.$off('configchange', this.listenerConfig);
		this.$root.$off('ws-in', this.listenerWs);
	}
});
</script>

<style lang="scss">
</style>