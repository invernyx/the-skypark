<template>
	<div :class="['p42_conduit', app.app_theme_mode]" :data-appname="this.appName">
		<div class="app-frame app-frame--clear-bottom">

			<width_limiter size="screen" :shadowed="true">
				<split_view>
					<sidebar class="split_view_narrow" :statusspaced="true">
						<content_controls_stack theme="is-sidebar" :transparent="true" :scroller_offset="{ top: 100, bottom: 0 }" :hasGutter="true">
							<template v-slot:nav>
								<div class="helper_edge_margin_left_half scroll-fade-out">
									<h1 class="">Conduit</h1>
									<div class="connection_status">
										<span v-if="$root.$data.state.services.simulator.connected" class="connection_status_connected">{{ $root.$data.state.services.simulator.name }}</span>
										<span v-else class="connection_status_disconnected">No simulator</span>
									</div>
								</div>
							</template>
							<template v-slot:content>
								<div class="helper_edge_margin_left_half helper_edge_margin_bottom">
									<div class="manifest-btn" @click="state.contracts.selected.Contract = null" v-if="$os.isDev">Manifest</div>
									<div class="no-contract-btn" v-if="$root.$data.state.services.api.connected && state.contracts.Contracts.filter(x => x.State == 'Active' || x.State == 'Saved').length == 0" @click="$router.push({name: 'p42_contrax'})">
										<strong>No Saved Contracts</strong>
											<section class="app_icon">
												<div class="border"></div>
												<div class="background" :style="'background-image: url(' + $root.$os.getAppIcon('p42', 'contrax') + ');'"></div>
											</section>
										<span>Open the Contrax app to browse available contracts.</span>
									</div>
									<div v-if="state.contracts.Contracts.filter(x => x.State == 'Saved').length">
										<collapser :withArrow="true" :default="true" :preload="true">
											<template v-slot:title>
												<div>
													<h2 class="helper_edge_margin_left_half">Saved</h2>
												</div>
												<div class="collapser_arrow"></div>
											</template>
											<template v-slot:content>
												<div class="contract_strips">
													<ContractStrip
														v-for="(contract, name, index) in state.contracts.Contracts.filter(x => x.State == 'Saved')"
														v-bind:key="index"
														:contract="contract"
														:selected="state.contracts.selected.Contract == contract"
														:templates="state.contracts.Templates"
														@select="contractSelect(contract)"
													/>
												</div>
											</template>
										</collapser>
									</div>
									<div v-if="state.contracts.Contracts.filter(x => x.State == 'Active').length">
										<collapser :withArrow="true" :default="true" :preload="true">
											<template v-slot:title>
												<div>
													<h2 class="helper_edge_margin_left_half">Started</h2>
												</div>
												<div class="collapser_arrow"></div>
											</template>
											<template v-slot:content>
												<div class="contract_strips">
													<ContractStrip
														v-for="(contract, name, index) in state.contracts.Contracts.filter(x => x.State == 'Active')"
														v-bind:key="index"
														:contract="contract"
														:selected="state.contracts.selected.Contract == contract"
														:templates="state.contracts.Templates"
														@select="contractSelect(contract)"
													/>
												</div>
											</template>
										</collapser>
									</div>
									<div v-if="state.contracts.Contracts.filter(x => x.State == 'Succeeded').length">
										<collapser :withArrow="true" :default="false" :preload="true">
											<template v-slot:title>
												<div>
													<h2 class="helper_edge_margin_left_half">Completed</h2>
												</div>
												<div class="collapser_arrow"></div>
											</template>
											<template v-slot:content>
												<div class="contract_strips">
													<ContractStrip
														v-for="(contract, name, index) in state.contracts.Contracts.filter(x => x.State == 'Succeeded')"
														v-bind:key="index"
														:contract="contract"
														:selected="state.contracts.selected.Contract == contract"
														:templates="state.contracts.Templates"
														@select="contractSelect(contract)"
													/>
												</div>
											</template>
										</collapser>
									</div>
									<div v-if="state.contracts.Contracts.filter(x => x.State == 'Failed').length">
										<collapser :withArrow="true" :default="false" :preload="true">
											<template v-slot:title>
												<div>
													<h2 class="helper_edge_margin_left_half">Failed</h2>
												</div>
												<div class="collapser_arrow"></div>
											</template>
											<template v-slot:content>
												<div class="contract_strips">
													<ContractStrip
														v-for="(contract, name, index) in state.contracts.Contracts.filter(x => x.State == 'Failed')"
														v-bind:key="index"
														:contract="contract"
														:selected="state.contracts.selected.Contract == contract"
														:templates="state.contracts.Templates"
														@select="contractSelect(contract)"
													/>
												</div>
											</template>
										</collapser>
									</div>
								</div>

							</template>
						</content_controls_stack>
					</sidebar>
					<keep-alive>
						<ContractContent
							v-if="state.contracts.selected.Contract && state.contracts.selected.Template"
							:app="app"
							:contract="state.contracts.selected.Contract"
							:templates="[state.contracts.selected.Template]"
							class="background narrow-margin"
							ref="conduit_content"
							@interactState="interactState"
						/>
						<ManifestContent
							v-else-if="$os.isDev"
							:app="this"
						/>
					</keep-alive>
				</split_view>
			</width_limiter>

		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"
import ContractStrip from "./components/contract_strip.vue"
import ContractContent from "./components/contract_content.vue"
import ManifestContent from "./components/manifest_content.vue"

export default Vue.extend({
	name: "p42_conduit",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ContractStrip,
		ContractContent,
		ManifestContent,
	},
	mounted() {
		this.$emit('loaded');
		this.loadContracts();
	},
	activated() {
		this.loadContracts();
	},
	data() {
		return {
			ready: true,
			state: {
				contracts: {
					selected: {
						restored: false,
						isVisible: false,
						Contract: null,
						Template: null,
					},
					Contracts: [],
					Templates: []
				},
				ui: {
					data: {
						requested: false,
					}
				}
			}
		}
	},
	methods: {
		loadContracts() {

			this.state.contracts.Contracts = [];
			this.state.contracts.Templates = [];

			const responseFn = (contractsData: any, activate: boolean) => {

				this.state.contracts.Contracts = [];
				this.state.contracts.Templates = [];

				this.state.contracts.Contracts = this.state.contracts.Contracts.concat(contractsData.payload.Contracts);
				this.state.contracts.Templates = this.state.contracts.Templates.concat(contractsData.payload.Templates);

				if(activate) {
					if(this.state.contracts.Contracts.length) {
						if(this.app.loaded_state){
							if(this.app.loaded_state.contracts.selected.Contract) {
								this.app.loaded_state.contracts.selected.restored = true;
								const foundContract = this.state.contracts.Contracts.find(x => x.ID == this.app.loaded_state.contracts.selected.Contract);
								if(foundContract) {
									this.contractSelect(foundContract);
								} else {
									this.contractSelect(this.state.contracts.Contracts[0]);
								}
							} else {
								this.contractSelect(this.state.contracts.Contracts[0]);
							}
						} else {
							this.contractSelect(this.state.contracts.Contracts[0]);
						}
					}

					if(this.$route.params.contract) {
						this.contractSelect(this.state.contracts.Contracts.find(x => x.ID == this.$route.params.contract));
						delete this.$route.params.contract;
					}
				}
			}

			this.state.ui.data.requested = true;

			this.$root.$data.services.api.SendWS(
				'adventures:query-from-filters', {
					state: 'Active,Saved,Succeeded,Failed',
					sort: 'requested',
					sortAsc: false,
					detailed: true,
					limitPerState: true,
					limit: 15,
				}, (contractsData: any) => {
					responseFn(contractsData, true);
					this.state.ui.data.requested = false;
					this.stateSave();
				}
			);

		},

		contractSelect(contract :any) {
			if(contract) {
				if(this.state.contracts.selected.Contract != contract || !this.state.contracts.selected.Contract) {
					this.state.contracts.selected.Contract = contract;
					this.state.contracts.selected.Template = this.state.contracts.Templates.find((x: any) => x.FileName == contract.FileName);
				}

				this.stateSave();
			}
		},

		stateSave() {
			this.app.StateSave({
				contracts: {
					selected: {
						Contract: this.state.contracts.selected.Contract ? this.state.contracts.selected.Contract.ID : null,
						Template: this.state.contracts.selected.Template ? this.state.contracts.selected.Template.FileName : null,
					}
				},
			});
		},

		interactState(ev: Event, name: string) {
			switch(name) {
				case 'remove': {
					const contract = this.state.contracts.selected.Contract;
					const index = this.state.contracts.Contracts.indexOf(contract);
					this.state.contracts.Contracts.splice(index, 1);

					if(this.state.contracts.Contracts.length > 0) {
						this.contractSelect(this.state.contracts.Contracts[0]);
					} else {
						this.state.contracts.selected.Contract = null;
						this.state.contracts.selected.Template = null;
					}
					break;
				}
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							if(wsmsg.payload.persistence) {
								this.loadContracts();
							}
							break;
						}
					}
					break;
				}
				case 'disconnect': {
					this.state.contracts.Contracts = [];
					this.state.contracts.Templates = [];
					this.state.contracts.selected.Contract = null;
					this.state.contracts.selected.Template = null;
					break;
				}
				case 'adventure': {
					this.$ContractMutator.EventInList(wsmsg, this.state.contracts);
					this.$ContractMutator.Event(wsmsg, this.state.contracts.selected.Contract, this.state.contracts.selected.Template);
					switch(wsmsg.name[1]) {
						case 'delete':
						case 'remove': {
							const existing = this.state.contracts.Contracts.findIndex(x => x.ID == wsmsg.payload.ID);
							this.state.contracts.Contracts.splice(existing, 1);
							if(this.state.contracts.selected.Contract) {
								if(wsmsg.payload.ID == this.state.contracts.selected.Contract.ID) {
									this.state.contracts.selected.Contract = null;
									this.state.contracts.selected.Template = null;
								}
							}
							break;
						}
					}
					break;
				}
			}
		},
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
});
</script>

<style lang="scss">
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
@import './../../../sys/scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.p42_conduit {

	.theme--bright &,
	&.theme--bright {
		.app-frame {
			background: $ui_colors_bright_shade_3;
			.sidebar {
				background: transparent;
			}
			.manifest-btn {
				background: rgba($ui_colors_bright_shade_1, 0.8);
				&:hover {
					background: rgba($ui_colors_bright_shade_1, 1);
				}
			}
			.no-contract-btn {
				background: rgba($ui_colors_bright_shade_1, 0.8);
				&:hover {
					background: rgba($ui_colors_bright_shade_1, 1);
				}
			}
			.contract_content  {
				background: $ui_colors_bright_shade_2;
			}
		}
	}
	.theme--dark &,
	&.theme--dark {
		.app-frame {
			background: $ui_colors_dark_shade_2;
			.sidebar {
				background: transparent;
			}
			.manifest-btn {
				background: rgba($ui_colors_dark_shade_1, 0.8);
				&:hover {
					background: rgba($ui_colors_dark_shade_1, 1);
				}
			}
			.no-contract-btn {
				background: rgba($ui_colors_dark_shade_1, 0.8);
				&:hover {
					background: rgba($ui_colors_dark_shade_1, 1);
				}
			}
			.contract_content  {
				background: $ui_colors_dark_shade_2;
			}
		}
	}

	.sidebar {
		.navigation_bar.is-sidebar {
			border-radius: 8px;
		}
		.simplebar-content {
			& > div {
				padding-top: 41px;
			}
		}
		.tab_bar.is-sidebar {
			border-radius: 8px;
		}

		h1 {
			margin-bottom: 0;
		}
		.connection_status {
			& > span {
				display: flex;
				align-items: center;
				&::before {
					display: inline-block;
					content: '';
					width: 1em;
					height: 1em;
					margin-right: 0.5em;
					border-radius: 50%;
				}
			}
			&_connected {
				&::before {
					background: $ui_colors_bright_button_go;
				}
			}
			&_disconnected {
				&::before {
					background: $ui_colors_bright_button_cancel;
				}
			}
		}
	}

	.manifest-btn {
		border-radius: 8px;
		padding: 10px;
		text-align: center;
		border: solid 2px rgba(0,0,0,0.5);
		transition: background 0.2s cubic-bezier(.25,0,.14,1);
		cursor: pointer;
	}

	.no-contract-btn {
		border-radius: 8px;
		padding: 10px;
		text-align: center;
		border: solid 2px rgba(0,0,0,0.5);
		transition: background 0.2s cubic-bezier(.25,0,.14,1);
		cursor: pointer;
		&:hover {
			.app_icon {
				transform: rotate3d(0, 1, 0, 1deg) scale(1.1);
			}
		}
		strong {
			display: block;
			margin-bottom: 0.5em;
			line-height: 1em;
		}
		span {
			display: block;
			font-size: 0.8em;
			line-height: 1.4em;
		}
		.app_icon {
			height: 60px;
			width: 60px;
			margin: 0 auto;
			margin-top: 10px;
			margin-bottom: 10px;
			color: $ui_colors_bright_shade_0;
			will-change: transform;
			transition-property: transform;
			transition-timing-function: cubic-bezier(0, 1, 0.63, 1), ease-out;
			transition-duration: 0.7s, 0.2s;
			transform: rotate3d(0, 1, 0, 1deg) scale(1);
			.background {
				position: absolute;
				left: 0;
				top: 0;
				right: 0;
				bottom: 0;
				z-index: 10;
				background-repeat: no-repeat;
				background-size: 100%;
				background-position: center;
				background-color: #777;
				mask-image: url(../../../sys/assets/framework/icon_mask.svg);
				mask-repeat: no-repeat;
				mask-position: center;
				mask-composite: exclude;
				border-radius: 10px;
			}
			.border {
				position: absolute;
				left: 1px;
				top: 1px;
				right: 1px;
				bottom: 1px;
				border-radius: 25%;
				transition: box-shadow ease-out 0.2s;
			}
		}
	}

	.contract_strips {
		.contract_strip {
			transition: opacity 0.5s cubic-bezier(.25,0,.14,1), filter 0.5s cubic-bezier(.25,0,.14,1);
			&:hover {
				opacity: 1;
			}
			&.selected {
				opacity: 1;
			}
		}
	}

}
</style>