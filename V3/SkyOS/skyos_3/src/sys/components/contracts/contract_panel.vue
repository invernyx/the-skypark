<template>
	<div :class="{ 'loading': loading || accent.loading || accent_right.loading }">

		<ContractActions
			:app="app"
			:contract="contract"
			@route_code="$emit('route_code', $event)"
			@template_code="$emit('template_code', $event)" />

		<ContractHeader
			:app="app"
			:contract="contract"
			:accent="accent"
			:accent_right="accent_right"
			:app_panel_content="app_panel_content"
			:countries="countries"
			class="h_edge_margin_bottom" />

		<ContractContent
			:app="app"
			:accent_right="accent_right"
			:contract="contract"
			:humans="humans" />

	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Contract, { Situation } from '@/sys/classes/contracts/contract';
import ContractActions from "@/sys/components/contracts/panel/actions.vue"
import ContractHeader from "@/sys/components/contracts/panel/header.vue"
import ContractContent from "@/sys/components/contracts/panel/content.vue"
import { AppInfo } from '@/sys/foundation/app_model';
import Aircraft from '@/sys/classes/aircraft';
import SearchStates from '@/sys/enums/search_states';

export default Vue.extend({
	props:{
		app: AppInfo,
		contract :Contract,
		loading :Boolean,
		app_panel_content :HTMLDivElement
	},
	components: {
		ContractActions,
		ContractHeader,
		ContractContent
	},
	data() {
		return {
			countries: [] as string[],
            selected_aircraft: null as Aircraft,
            humans: [],
            cargos: [],
            state: {
                status: this.$os.api.connected ? SearchStates.Idle : SearchStates.NoTransponder,
            },
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
	methods: {
		init() {
			if(this.contract) {
				this.get_cabin();

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
			}
		},

        get_cabin() {

            if (this.selected_aircraft) {
                this.state.status = SearchStates.Searching;
                this.$os.api.send_ws("cabin:get-all", {
                    fields: null
                }, (wsmsg) => {
                    this.state.status = SearchStates.Idle;
                    if (wsmsg.payload.humans) {
                        this.humans = wsmsg.payload.humans;
                    }
                    if (wsmsg.payload.humans_state) {
                        wsmsg.payload.humans_state.forEach(human_state => {
                            const host = this.humans.find(x => x.guid == human_state.guid);
                            if (host) {
                                this.$set(host, "state", human_state.state);
                            }
                        });
                    }
					if(wsmsg.payload.cargos_state) {
						wsmsg.payload.cargos_state.forEach(cargo_state => {
							const host = this.cargos.find(x => x.guid == cargo_state.guid);
							if(host) {
								this.$set(host, 'state', cargo_state.state);
							}
						});
					}
                });
            }
            else {
                this.state.status = SearchStates.NoResults;
            }
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
						if(res.data) {
							this.$os.colorSeek.find(res.data, 160, (color :any) => {
								this.accent.color = color.color;
								this.accent.combined = this.accent.color.r + ',' + this.accent.color.g + ',' + this.accent.color.b;
								this.accent.dark = color.color_is_dark;
								this.accent.shadow = '0 1px 3px rgba(0,0,0,' + (this.accent.dark ? '0.5' : '0') + '), 0 0 7px rgba(' + this.accent.combined + ',1), 0 0 18px rgba(' + this.accent.combined + ',1), 0 7px 30px rgba(' + this.accent.combined + ',1)'

								window.requestAnimationFrame(() => {
									this.accent.loading = false;
								});
							});
						} else {
							this.accent.loading = false;
						}
					});

					// Right
					this.$os.colorSeek.crop(this.contract.image_url, {
						left: 0.8,
						top: 0.4,
						right: 1,
						bottom: 0.6,
						width: 300
					}, (res) => {
						if(res.data) {
							this.$os.colorSeek.find(res.data, 150, (color :any) => {
								this.accent_right.color = color.color;
								this.accent_right.combined = this.accent_right.color.r + ',' + this.accent_right.color.g + ',' + this.accent_right.color.b;
								this.accent_right.dark = color.color_is_dark;
								this.accent_right.shadow = '0 1px 3px rgba(0,0,0,' + (this.accent_right.dark ? '0.5' : '0') + '), 0 0 7px rgba(' + this.accent_right.combined + ',1), 0 0 18px rgba(' + this.accent_right.combined + ',1), 0 7px 30px rgba(' + this.accent_right.combined + ',1)'

								window.requestAnimationFrame(() => {
									this.accent_right.loading = false;
								});
							});
						} else {
							this.accent_right.loading = false;
						}
					});

				})

			}
		},

        listener_os_contracts(wsmsg: any) {
            switch (wsmsg.name) {
                case "remove":
                case "mutate": {
                    if (wsmsg.payload.contract.state || wsmsg.payload.contract.is_monitored) {
                        this.get_cabin();
                    }
                    break;
                }
            }
        },

        listener_ws(wsmsg: any) {
            switch (wsmsg.name[0]) {
                case "cabin": {
                    if (wsmsg.payload.humans) {
                        this.humans = wsmsg.payload.humans;
                    }
                    wsmsg.payload.humans_removed.forEach(human => {
                        const found = this.humans.find(h => h.guid == human);
                        const found_index = this.humans.indexOf(found);
                        if (found_index > -1) {
                            this.humans.splice(found_index, 1);
                        }
                    });
                    wsmsg.payload.humans_state.forEach(human_state => {
                        const host = this.humans.find(x => x.guid == human_state.guid);
                        if (host) {
                            this.$set(host, "state", human_state.state);
                        }
                    });
                    break;
                }
            }
        },

        listenerFleet(wsmsg: any) {
            switch (wsmsg.name) {
                case "current_aircraft": {
                    this.selected_aircraft = this.$os.fleetService.aircraft_current;
                    break;
                }
            }
        },
	},
	mounted() {
        this.selected_aircraft = this.$os.fleetService.aircraft_current;

        this.$os.eventsBus.Bus.on("fleet", this.listenerFleet);
        this.$os.eventsBus.Bus.on("contracts", this.listener_os_contracts);
        this.$os.eventsBus.Bus.on("ws-in", this.listener_ws);

		this.init();
	},
    beforeDestroy() {
        this.$os.eventsBus.Bus.off("fleet", this.listenerFleet);
        this.$os.eventsBus.Bus.off("contracts", this.listener_os_contracts);
        this.$os.eventsBus.Bus.off("ws-in", this.listener_ws);
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
</style>