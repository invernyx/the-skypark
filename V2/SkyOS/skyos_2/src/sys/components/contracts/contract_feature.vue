<template>
	<div class="contract_feature" v-if="feature.FeatureType == 'featured'" :class="['type-featured', { 'selected': selected, 'ready': state.ui.ready }]" @click="select($event)">
		<div class="card" :style="state.ui.color ? 'background-color:' + ($os.activeApp.app_theme_mode ? $os.activeApp.app_theme_mode == 'theme--bright' ? state.ui.color : state.ui.colorDark : $root.$data.config.ui.theme == 'theme--bright' ? state.ui.color : state.ui.colorDark) : ''">
			<div class="border"></div>

			<div class="background" v-if="feature.ImageURL.length">
				<div class="blur" v-if="state.ui.ready" :style="'background-image: url(' + feature.ImageURL + ')'"></div>
			</div>

			<div class="stack">
				<span class="pre">New {{ feature.TypeLabel }} available!</span>
				<span class="name">{{ feature.Name }}</span>
				<div class="flags">
					<div v-for="(country, index) in this.state.ui.countries" v-bind:key="index">
						<flags :code="country.toLowerCase()" />
					</div>
				</div>
				<span class="contracts"><span class="amt">{{ Math.abs(feature.Contracts).toLocaleString('en-gb') }}</span> contract{{ (feature.Contracts != 1) ? 's' : '' }}</span>
				<div class="actions">
					<button_nav @click.native="hide" class="translucent">Hide</button_nav>
					<button_nav @click.native="browse" class="info" v-if="feature.TemplateCode.length">#{{ feature.TemplateCode }}</button_nav>
					<button_nav @click.native="browse" class="info" v-else>Search</button_nav>
				</div>
			</div>
		</div>
	</div>
	<div class="contract_feature" v-else-if="feature.FeatureType == 'carrot'" :class="['type-carrot', { 'selected': selected, 'ready': state.ui.ready }]" @click="select($event)">
		<div class="card" :style="state.ui.color ? 'background-color:' + (this.$root.$data.config.ui.theme == 'theme--bright' && !state.ui.colorIsDark ? state.ui.color : state.ui.colorDark) : ''">
			<div class="border"></div>

			<div class="background" v-if="feature.ImageURL.length">
				<div class="blur" v-if="state.ui.ready" :style="'background-image: url(' + feature.ImageURL + ')'"></div>
			</div>

			<div class="stack">
				<span class="name">{{ feature.Name }}</span>
				<span class="post">{{ state.ui.reqsPhrase }}</span>
				<div class="requirements" v-if="state.ui.reqsShow">
					<div v-for="(req, name, index) in state.ui.reqs" v-bind:key="index" :class="{ 'blocking': req[3] === false }">
						<div>{{ req[0] }}</div>
						<div>
							<span class="req">{{ req[1] }}</span>
							<span class="current" v-if="req[2]">{{ req[2] }}</span>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from './../../../sys/libraries/elem';

export default Vue.extend({
	name: "contract_feature",
	props: ['feature', 'selected', 'canRedirect', 'index'],
	data() {
		return {
			template: null,
			state: {
				ui: {
					ready: false,
					delayTO: null,
					expireInterval: null,
					colorIsDark: false,
					color: null,
					colorDark: null,
					countries: [],
					reqsShow: true,
					reqsPhrase: '',
					reqs: {
						level: [],
						karma: [],
						reliability: [],
						dates: [],
					}
				}
			}
		}
	},
	methods: {
		initCard() {
			this.state.ui.delayTO = setTimeout(() => {
				clearInterval(this.state.ui.expireInterval);

				if(this.feature.ImageURL.length) {
					this.state.ui.delayTO = null;
					this.$root.$data.services.colorSeek.find(this.feature.ImageURL, (color :any) => {
						this.state.ui.color = color.color;
						this.state.ui.colorDark = color.colorDark;
						this.state.ui.colorIsDark = color.colorIsDark;
						this.state.ui.ready = true;
					});
				} else {
					this.state.ui.colorIsDark = false;
					this.state.ui.color = null;
					this.state.ui.colorDark = null;
					this.state.ui.ready = true;
				}

			}, this.index * 100);

			if(this.feature.Requirements) {

				let levelOffset = 0;
				let reliabilityOffset = 0;
				let karmaOffset = 0;
				this.feature.Requirements.forEach(ft => {
					switch(ft.Key){
						case "dates": {
							let endDate = new Date(ft.Value[1]);
							const diffTime = endDate.getTime() - new Date().getTime();
							if(diffTime < 3.154e+10) {
								this.state.ui.reqs.dates[0] = "Expired";
								this.state.ui.reqs.dates[1] = endDate.toLocaleString('en-gb', {
									timeZone: 'UTC',
									year: 'numeric',
									month: 'long',
									day: 'numeric'
								});
								this.state.ui.reqs.dates[3] = false;
							}
							break;
						}
						case "level": {
							this.state.ui.reqs.level[0] = "Level";

							if(ft.Value[0] >= 1) {
								if(ft.Value[1] > 9999) {
									this.state.ui.reqs.level[1] = ft.Value[0] + ' or +';
								} else {
									this.state.ui.reqs.level[1] = ft.Value[0] + ' to ' + ft.Value[1];
								}
							} else {
								if(ft.Value[1] > 9999) {
									this.state.ui.reqs.level[1] = 'Any';
								} else {
									this.state.ui.reqs.level[1] = ft.Value[1] + ' or -';
								}
							}

							this.state.ui.reqs.level[2] = "You: " + Eljs.round(ft.Value[2], 2);
							this.state.ui.reqs.level[3] = ft.Value[2] >= ft.Value[0] && ft.Value[2] <= ft.Value[1];

							levelOffset = Math.abs((() => {
								if(ft.Value[2] < ft.Value[0]) {
									return ft.Value[2] - ft.Value[0];
								} else if(ft.Value[2] > ft.Value[1]) {
									return ft.Value[2] - ft.Value[1];
								}
								return 0;
							})()) * 2;
							break;
						}
						case "reliability": {
							this.state.ui.reqs.reliability[0] = "Reliability";

							if(ft.Value[0] > 0) {
								if(ft.Value[1] >= 100) {
									this.state.ui.reqs.reliability[1] = ft.Value[0] + ' or +';
								} else {
									this.state.ui.reqs.reliability[1] = ft.Value[0] + ' to ' + ft.Value[1];
								}
							} else {
								if(ft.Value[1] >= 100) {
									this.state.ui.reqs.reliability[1] = 'Any';
								} else {
									this.state.ui.reqs.reliability[1] = ft.Value[1] + ' or -';
								}
							}

							if(ft.Value[2] < ft.Value[0]) {
								this.state.ui.reqs.reliability[2] = "You: Too low";
								this.state.ui.reqs.reliability[3] = false;
							} else if(ft.Value[2] > ft.Value[1]){
								this.state.ui.reqs.reliability[2] = "You: Too high";
								this.state.ui.reqs.reliability[3] = false;
							} else {
								this.state.ui.reqs.reliability[2] = "You: Good";
								this.state.ui.reqs.reliability[3] = true;
							}

							reliabilityOffset = Math.abs((() => {
								if(ft.Value[2] < ft.Value[0]) {
									return ft.Value[2] - ft.Value[0];
								} else if(ft.Value[2] > ft.Value[1]) {
									return ft.Value[2] - ft.Value[1];
								}
								return 0;
							})()) / 3;
							break;
						}
						case "karma": {
							this.state.ui.reqs.karma[0] = "Karma";

							if(ft.Value[0] > -42) {
								if(ft.Value[1] >= 42) {
									this.state.ui.reqs.karma[1] = ft.Value[0] + ' or +';
								} else {
									this.state.ui.reqs.karma[1] = ft.Value[0] + ' to ' + ft.Value[1];
								}
							} else {
								if(ft.Value[1] >= 42) {
									this.state.ui.reqs.karma[1] = 'Any';
								} else {
									this.state.ui.reqs.karma[1] = ft.Value[1] + ' or -';
								}
							}

							this.state.ui.reqs.karma[2] = "You: " + Eljs.round(ft.Value[2], 2);
							this.state.ui.reqs.karma[3] = ft.Value[2] >= ft.Value[0] && ft.Value[2] <= ft.Value[1];

							karmaOffset = Math.abs((() => {
								if(ft.Value[2] < ft.Value[0]) {
									return ft.Value[2] - ft.Value[0];
								} else if(ft.Value[2] > ft.Value[1]) {
									return ft.Value[2] - ft.Value[1];
								}
								return 0;
							})()) / 2;
							break;
						}
					}
				});

				let farthest = 0;
				if(farthest < levelOffset) {
					farthest += levelOffset;
				}
				if(farthest < reliabilityOffset) {
					farthest += reliabilityOffset;
				}
				if(farthest < karmaOffset) {
					farthest += karmaOffset;
				}

				this.state.ui.reqsShow = true;

				if(this.$os.getConfig(['ui','tier']) == 'discovery' || this.$os.getConfig(['ui','tier']) == 'prospect') {
					this.state.ui.reqsShow = false;
					this.state.ui.reqsPhrase = "Contracts are still being generated. Please come back a bit later...";
				} else {
					if(farthest > 10) {
						this.state.ui.reqsPhrase = "You've got some work to do!";
					} else if(farthest > 5) {
						this.state.ui.reqsPhrase = "Not quite there yet.";
					} else if(farthest > 0) {
						this.state.ui.reqsPhrase = "You're almost there!";
					} else {
						this.state.ui.reqsShow = false;
						this.state.ui.reqsPhrase = "Contracts are still being generated. Please come back a bit later...";
					}
				}

			}

			if(this.feature.Countries) {
				this.state.ui.countries = this.feature.Countries;
				if(this.state.ui.countries.length > 7) {
					const fullList = this.state.ui.countries;
					const spacing = (fullList.length / 6);
					this.state.ui.countries = [];
					for (let i = 0; i < 6; i++) {
						const country = fullList[Math.floor(i * spacing)];
						this.state.ui.countries.push(country);
					}
					this.state.ui.countries.push("+" + (fullList.length - this.state.ui.countries.length - 1));
				}
			}
		},
		hide() {
			this.$emit('hide', this.feature.FileName);
		},
		browse() {
			this.$emit('browse', this.feature.TemplateCode.length ? '#' + this.feature.TemplateCode : this.feature.FileNameE );
		},
		select(ev: any) {
			if(!ev.target.className.includes('button_nav')){
				if(!this.selected) {
					this.$emit('select', this.feature);
				}
			}
		},
	},
	mounted() {
		this.$emit('init');
	},
	created() {
	},
	beforeDestroy() {
		clearTimeout(this.state.ui.delayTO);
		clearInterval(this.state.ui.expireInterval);
	},
	beforeMount() {
	},
	watch: {
		feature: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.initCard();
				}
			}
		}
	},
});
</script>

<style lang="scss">
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

$transition: cubic-bezier(.25,0,.14,1);
.contract_feature {
	display: flex;
	pointer-events: all;

	.theme--bright &,
	&.theme--bright {
		color: $ui_colors_bright_shade_5;
		&:hover {
			.background {
				.blur {
					filter: blur(30px) brightness(1.5) saturate(2);
				}
			}
		}
		.background {
			.blur {
				filter: blur(30px) brightness(1.5) saturate(1);
			}
		}
		.card {
			background-color: rgba($ui_colors_bright_shade_1, 0.8);
		}
		.stack {
			.requirements {
				.blocking {
					background: linear-gradient(to bottom, rgba($ui_colors_bright_button_cancel, 0.6), rgba($ui_colors_bright_button_cancel, 0.8));
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		color: $ui_colors_dark_shade_5;
		&:hover {
			.background {
				.blur {
					filter: blur(30px) brightness(1) saturate(2);
				}
			}
		}
		.background {
			.blur {
				filter: blur(30px) brightness(1) saturate(1);
			}
		}
		.card {
			background-color: rgba($ui_colors_dark_shade_1, 0.8);
		}
		.stack {
			.requirements {
				.blocking {
					background: linear-gradient(to bottom, rgba($ui_colors_dark_button_cancel, 0.6), rgba($ui_colors_dark_button_cancel, 0.8));
				}
			}
		}
	}

	&:hover {
		.background {
			.blur {
				transform: scale(1.5, 1.7);
			}
		}
	}

	&.ready {
		.card {
			opacity: 1;
		}
	}

	.card {
		display: flex;
		flex-direction: column;
		width: 290px;
		height: 185px;
		min-width: 240px;
		border-radius: 14px;
		margin-right: 8px;
		opacity: 0;
		overflow: hidden;
		backdrop-filter: blur(5px);
		@include shadowed($ui_colors_dark_shade_0);
		transform: scale(1) translateZ(0);
		transition: opacity 1s $transition, transform 0.2s $transition;
	}
	.border {
		border: 4px solid transparent;
		position: absolute;
		left: 0;
		right: 0;
		top: 0;
		bottom: 0;
		margin: -1px;
		border-radius: 15px;
		z-index: 10;
		pointer-events: none;
	}

	.flags {
		font-size: 1.2em;
		margin-top: 8px;
		display: flex;
		align-items: stretch;
		font-family: "SkyOS-SemiBold";
		& > div {
			margin-right: 5px;
			display: flex;
			align-items: stretch;
			color: #FFF;
			font-family: "SkyOS-SemiBold";
			&:last-child {
				margin-right: 0;
			}
		}
		.flag {
			border-radius: 4px;
			box-shadow: 0 1.5px 4px rgba(0,0,0,0.2);
			span {
				font-size: 0.75em;
				line-height: 0.75em;
				padding: 0 0.25em;
				display: flex;
			}
		}
	}

	.background {
		position: absolute;
		left: 0;
		right: 0;
		top: 0;
		bottom: 0;
		.image {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 8px;
			background-size: cover;
			background-position: center;
		}
		.blur {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			transform: scale(1.2);
			opacity: 1;
			background-size: cover;
			background-position: center;
			will-change: transform, filter;
			transition: transform 1s $transition, filter 1s $transition;
		}
	}
	.stack {
		display: flex;
		flex-grow: 1;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		text-align: center;
		margin-left: 16px;
		margin-right: 16px;
		z-index: 2;
		& > span {
			display: block;
		}
		.name {
			font-size: 1.5em;
			line-height: 1em;
			font-family: "SkyOS-SemiBold";
		}
		.post {
			margin-top: 4px;
		}
		.contracts {
			margin-top: 16px;
			margin-bottom: 8px;
			.amt {
				font-family: "SkyOS-SemiBold";
			}
		}
		.requirements {
			margin-top: 16px;
			margin-bottom: 8px;
			& > div {
				display: flex;
				justify-content: space-between;
				text-align: left;
				border-radius: 4px;
				padding: 1px 3px;
				margin: -1px -3px;
				& > div {
					display: flex;
					flex-grow: 1;
					&:first-child {
						width: 80px;
						flex-grow: 0;
					}
					& > span {
						display: block;
						//flex-grow: 1;
						//flex-basis: 0;
						&:last-child {
							font-family: "SkyOS-SemiBold";
						}
						&.req {
							width: 65px;
							flex-grow: 1;
						}
						&.current {
							width: 100px;
						}
					}
				}
			}
		}
		.actions {
			display: flex;
			.button_nav {
				padding-top: 2px;
				padding-bottom: 3px;
				margin-right: 4px;
				text-transform: uppercase;
				&:last-child {
					margin: 0;
				}
			}
		}
	}
}
</style>