<template>
	<div class="data_block hitbox">
		<div class="label">{{ ui.title }}</div>
		<div class="value">{{ ui.value.toLocaleString('en-gb') }}</div>
	</div>
</template>

<script lang="ts">
import Eljs from '@/sys/libraries/elem';
import { now } from 'moment';
import Vue from 'vue';

export default Vue.extend({
	name: "data_block",
	props: ['app', 'type'],
	data() {
		return {
			ui: {
				proc: (v) => {
					return Math.round(v);
				},
				title: '',
				last: 0,
				value: 0,
			},
		}
	},
	methods: {
		init() {
			switch(this.type) {
				case 'Alt': {
					this.ui.title = "Alt (ASL)";
					this.ui.proc = (v) => {
						if(Math.abs(v - this.ui.last) < 10) {
							return Math.round(v);
						} else {
							return Math.round(v / 10) * 10;
						}
					}
					break;
				}
				case 'GAlt': {
					this.ui.title = "Alt (AGL)";
					this.ui.proc = (v) => {
						if(Math.abs(v - this.ui.last) < 10) {
							return Math.round(v);
						} else {
							return v < 30 ? Math.round(v) : Math.round(v / 10) * 10;
						}
					}
					break;
				}
				case 'FPM': {
					this.ui.title = "VS";
					this.ui.proc = (v) => {
						return Math.round(v / 10) * 10;
					}
					break;
				}
				case 'HDG': {
					this.ui.title = "HDG";
					this.ui.proc = (v) => {
						return Math.round(v);
					}
					break;
				}
				case 'CRS': {
					this.ui.title = "CRS";
					this.ui.proc = (v) => {
						return Math.round(v);
					}
					break;
				}
				case 'GS': {
					this.ui.title = "GS";
					this.ui.proc = (v) => {
						if(Math.abs(v - this.ui.last) < 5) {
							return Math.round(v);
						} else {
							return v < 30 ? Math.round(v) : Math.round(v / 10) * 10;
						}
					}
					break;
				}
				case 'AirTime': {
					this.ui.title = "Air time";
					this.ui.proc = (v) => {
						return Math.round(v);
					}
					break;
				}
			}
		},
		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'eventbus': {
					switch(wsmsg.name[1]){
						case 'meta': {
							if(!this.app.app_sleeping) {
								/*
								AirTime: 0
								AircraftName: "Asobo 208B GRAND CARAVAN EX"
								Airports: ""
								Alt: 6279
								BlockTime: 0
								CRS: 0
								CoverPhoto: ""
								DistanceTraveled: 0
								FPM: 0
								GAlt: 8
								GS: 0
								HDG: 88.505
								ID: 1606766867936
								IsValid: false
								Lat: 42.045902
								Lon: -110.967883
								MagVar: 12
								PhotoCount: 0
								Platform: 0
								SimTimeOffset: 25200
								SimTimeZulu: "2020-11-30T20:34:57.0000000Z"
								Started: false
								Time: "2020-11-30T17:07:22.8503459-05:00"
								TimeZulu: "2020-11-30T22:07:22.8503459Z"
								TurnRate: 0
								VMajor: 0
								VMinor: 0
								*/

								if(wsmsg.payload[this.type]) {
									this.ui.value = this.ui.proc(wsmsg.payload[this.type]);
									this.ui.last = this.ui.value;
								} else {
									this.ui.value = 0;
								}

								//console.log(wsmsg.payload)
							}
							break;
						}
					}
					break;
				}
			}
		},
	},
	mounted() {
		this.init();
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
	},
	watch: {
		type() {
			this.init();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';
.data_block {
	display: flex;
	position: relative;
	justify-content: center;
	font-size: 1.2em;
	height: 32px;
	overflow: hidden;
	font-family: "SkyOS-SemiBold";
	background: rgba($ui_colors_dark_shade_5, 0);
	$transition: cubic-bezier(.25,0,.14,1);
	transition: background .2s $transition;

	.theme--bright &,
	&.theme--bright {
		&:hover {
			background: rgba($ui_colors_bright_shade_5, 0.2);
		}
	}

	.theme--dark &,
	&.theme--dark {
		&:hover {
			background: rgba($ui_colors_dark_shade_5, 0.2);
		}
	}

	&:hover {
		cursor: pointer;
	}

	& > div {
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
	}

	.label {
		font-size: 10px;
		text-transform: uppercase;
		text-align: center;
	}

	.value {
		padding-top: 8px;
		text-align: center;
	}

	.odometer {
		padding-top: 8px;
		mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 0) 5px, rgba(0, 0, 0, 1) 15px, rgba(0, 0, 0, 1) calc(100% - 5px), rgba(0, 0, 0, 0) 100%);
	}
}
</style>