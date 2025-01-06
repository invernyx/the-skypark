<template>
	<div :class="{ 'loading': loading }">

		<ManifestActions
			:app="app"
			:contract="contract"
			@route_code="$emit('route_code')" />

		<ManifestHeader
			:app="app"
			:contract="contract"
			:accent="accent"
			:accent_right="accent_right"
			:app_panel_content="app_panel_content"
			:countries="countries"
			class="h_edge_margin_bottom" />

		<PayloadLayout class="manifest_layout" :app="app" :show_aircraft="true" :payload="[manifest]" :payload_state="[manifest_state]" v-if="manifest"/>

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from '@/sys/foundation/app_model';
import Contract, { Situation } from '@/sys/classes/contracts/contract';
import ManifestActions from "@/sys/components/manifest/panel/actions.vue"
import ManifestHeader from "@/sys/components/manifest/panel/header.vue"
import PayloadLayout from '@/sys/components/manifest/manifest_all.vue';

export default Vue.extend({
	props:{
		app: AppInfo,
		contract :Contract,
		manifest :Object,
		manifest_state :Object,
		loading :Boolean,
		app_panel_content :HTMLDivElement
	},
	components: {
		PayloadLayout,
		ManifestActions: ManifestActions,
		ManifestHeader: ManifestHeader,
	},
	data() {
		return {
			countries: [] as string[],
			accent: {
				loading: true,
				dark: false,
				combined: null,
				shadow: '',
				color: {
					r: null,
					g: null,
					b: null,
					h: null
				}
			},
			accent_right: {
				loading: true,
				dark: false,
				combined: null,
				shadow: '',
				color: {
					r: null,
					g: null,
					b: null,
					h: null
				}
			},
		}
	},
	mounted() {
	},
	methods: {
		init() {
			this.countries = [];

			this.contract.situations.forEach((sit :Situation) => {
				if(sit.airport){
					if(!this.countries.includes(sit.airport.country)) {
						this.countries.push(sit.airport.country);
					}
				}
			});

			if(this.countries.length > 5) {
				const fullList = this.countries;
				const spacing = (fullList.length / 4);
				this.countries = [];
				for (let i = 0; i < 4; i++) {
					const country = fullList[Math.floor(i * spacing)];
					this.countries.push(country);
				}
				this.countries.push("+" + (fullList.length - this.countries.length - 1));
				this.countries.push(fullList[fullList.length - 1]);
			}

			this.colorize();
		},

		colorize() {
			if(this.contract.image_url.length) {

				this.accent.loading = true;
				this.accent.color = null;
				this.accent.combined = null;
				this.accent.dark = null;
				this.accent.shadow = null;

				this.accent_right.loading = true;
				this.accent_right.color = null;
				this.accent_right.combined = null;
				this.accent_right.dark = null;
				this.accent_right.shadow = null;

				window.requestAnimationFrame(() => {

					// Top
					this.$os.colorSeek.crop(this.contract.image_url, {
						left: 0,
						top: 0.25,
						right: 0.5,
						bottom: 0.40,
						width: 300
					}, (res) => {
						this.$os.colorSeek.find(res.data, 160, (color :any) => {
							this.accent.color = color.color;
							this.accent.combined = this.accent.color.r + ',' + this.accent.color.g + ',' + this.accent.color.b;
							this.accent.dark = color.color_is_dark;
							this.accent.shadow = '0 1px 3px rgba(0,0,0,' + (this.accent.dark ? '0.5' : '0') + '), 0 0 7px rgba(' + this.accent.combined + ',1), 0 0 18px rgba(' + this.accent.combined + ',1), 0 7px 30px rgba(' + this.accent.combined + ',1)'

							window.requestAnimationFrame(() => {
								this.accent.loading = false;
							});
						});
					});

					// Right
					this.$os.colorSeek.crop(this.contract.image_url, {
						left: 0.8,
						top: 0.4,
						right: 1,
						bottom: 0.6,
						width: 300
					}, (res) => {
						this.$os.colorSeek.find(res.data, 150, (color :any) => {
							this.accent_right.color = color.color;
							this.accent_right.combined = this.accent_right.color.r + ',' + this.accent_right.color.g + ',' + this.accent_right.color.b;
							this.accent_right.dark = color.color_is_dark;
							this.accent_right.shadow = '0 1px 3px rgba(0,0,0,' + (this.accent_right.dark ? '0.5' : '0') + '), 0 0 7px rgba(' + this.accent_right.combined + ',1), 0 0 18px rgba(' + this.accent_right.combined + ',1), 0 7px 30px rgba(' + this.accent_right.combined + ',1)'

							window.requestAnimationFrame(() => {
								this.accent_right.loading = false;
							});
						});
					});

				})

			}
		},
	},
	watch: {
		contract: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.manifest_layout {
	opacity: 0;
	pointer-events: none;
	transition: opacity 0.2s ease-out;

	.is-open & {
		opacity: 1;
		pointer-events: all;
	}
}

</style>