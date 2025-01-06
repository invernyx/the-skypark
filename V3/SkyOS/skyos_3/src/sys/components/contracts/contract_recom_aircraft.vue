<template>
	<div class="recommended_aircraft" :class="{ 'is-small': small }" v-if="contract.recommended_aircraft" @mouseleave="mouseLeave" @mouseenter="mouseEnter">
		<div class="models">
			<div v-if="contract.recommended_aircraft[0] > 10 || !small" class="heli" :class="{ 'hover': currentIndex == 0, 'good': contract.recommended_aircraft[0] > 10, 'great': contract.recommended_aircraft[0] > 70 }" @click="getLegend(0)"></div>
			<div v-if="contract.recommended_aircraft[1] > 10 || !small" class="ga" :class="{ 'hover': currentIndex == 1, 'good': contract.recommended_aircraft[1] > 10, 'great': contract.recommended_aircraft[1] > 70 }" @click="getLegend(1)"></div>
			<div v-if="contract.recommended_aircraft[2] > 10 || !small" class="turbo" :class="{ 'hover': currentIndex == 2, 'good': contract.recommended_aircraft[2] > 10, 'great': contract.recommended_aircraft[2] > 70 }" @click="getLegend(2)"></div>
			<div v-if="contract.recommended_aircraft[3] > 10 || !small" class="jet" :class="{ 'hover': currentIndex == 3, 'good': contract.recommended_aircraft[3] > 10, 'great': contract.recommended_aircraft[3] > 70 }" @click="getLegend(3)"></div>
			<div v-if="contract.recommended_aircraft[4] > 10 || !small" class="narrow" :class="{ 'hover': currentIndex == 4, 'good': contract.recommended_aircraft[4] > 10, 'great': contract.recommended_aircraft[4] > 70 }" @click="getLegend(4)"></div>
			<div v-if="contract.recommended_aircraft[5] > 10 || !small" class="wide" :class="{ 'hover': currentIndex == 5, 'good': contract.recommended_aircraft[5] > 10, 'great': contract.recommended_aircraft[5] > 70 }" @click="getLegend(5)"></div>
		</div>
		<collapser :preload="true" :state="currentIndex != -1" v-if="!small">
			<template v-slot:content>
				<div class="legend">
					<span><strong>{{ currentLabel }}</strong> - {{ currentRecommend }}</span>
				</div>
			</template>
		</collapser>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "recommended_aircraft",
	props: ['contract', 'small'],
	data() {
		return {
			currentIndex: -1,
			currentLabel: "---",
			currentRecommend: "---",
			timeout: null,
		}
	},
	methods: {
		getLegend(index :number) {
			if(this.currentIndex == index) {
				this.currentIndex = -1;
				clearTimeout(this.timeout);
				this.timeout = null;
			} else {
				this.currentIndex = index;
				switch(index) {
					case 0: { this.currentLabel = "Helicopters"; break; }
					case 1: { this.currentLabel = "Piston"; break; }
					case 2: { this.currentLabel = "Turboprop"; break; }
					case 3: { this.currentLabel = "Business jets"; break; }
					case 4: { this.currentLabel = "Narrow-body"; break; }
					case 5: { this.currentLabel = "Wide-body"; break; }
				}

				const recValue = this.contract.recommended_aircraft[this.currentIndex];
				this.currentRecommend = (recValue > 70 ? 'Recommended' :  (recValue > 10 ? 'Feasible' :  'Not Recommended'));
			}
		},
		mouseLeave() {
			clearTimeout(this.timeout);
			this.timerStart();
		},
		mouseEnter() {
			clearTimeout(this.timeout);
			this.timeout = null;
		},
		timerStart() {
			this.timeout = setTimeout(() => {
				clearTimeout(this.timeout);
				this.timeout = null;
				this.currentIndex = -1;
			}, 2000);
		}
	},
	beforeDestroy() {
		clearTimeout(this.timeout);
	},
	mounted() {

	},
	watch: {
		contract() {
			this.currentIndex = -1;
			this.currentLabel = "";
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';

.recommended_aircraft {

	.theme--bright &,
	&.theme--bright {
		.models {
			& > div {
				&.hover {
					background: rgba($ui_colors_bright_shade_0, 0.2);
				}
				&.heli {
					&:after {
						background-image: url(../../../sys/assets/icons/dark/acf_heli.svg);
					}
				}
				&.ga {
					&:after {
						background-image: url(../../../sys/assets/icons/dark/acf_ga.svg);
					}
				}
				&.turbo {
					&:after {
						background-image: url(../../../sys/assets/icons/dark/acf_turbo.svg);
					}
				}
				&.jet {
					&:after {
						background-image: url(../../../sys/assets/icons/dark/acf_jet.svg);
					}
				}
				&.narrow {
					&:after {
						background-image: url(../../../sys/assets/icons/dark/acf_narrow.svg);
					}
				}
				&.wide {
					&:after {
						background-image: url(../../../sys/assets/icons/dark/acf_wide.svg);
					}
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		.models {
			& > div {
				&.hover {
					background: rgba($ui_colors_dark_shade_0, 0.2);
				}
				&.heli {
					&:after {
						background-image: url(../../../sys/assets/icons/bright/acf_heli.svg);
					}
				}
				&.ga {
					&:after {
						background-image: url(../../../sys/assets/icons/bright/acf_ga.svg);
					}
				}
				&.turbo {
					&:after {
						background-image: url(../../../sys/assets/icons/bright/acf_turbo.svg);
					}
				}
				&.jet {
					&:after {
						background-image: url(../../../sys/assets/icons/bright/acf_jet.svg);
					}
				}
				&.narrow {
					&:after {
						background-image: url(../../../sys/assets/icons/bright/acf_narrow.svg);
					}
				}
				&.wide {
					&:after {
						background-image: url(../../../sys/assets/icons/bright/acf_wide.svg);
					}
				}
			}
		}
	}

	&.is-small {
		.models {
			& > div {
				width: 23px;
				height: 23px;
				border: none;
				margin-right: 1px;
				transform: rotate(45deg);
				&:last-child {
					margin-right: 0;
				}
			}
		}
	}

	.models {
		display: flex;
		justify-content: space-between;
		& > div {
			position: relative;
			width: 42px;
			height: 42px;
			border: 2px solid transparent;
			border-radius: 10px;
			box-sizing: border-box;
			transition: opacity 0.5s ease-out;
			cursor: pointer;
			&:after {
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				content: '';
				background-size: 100%;
				background-repeat: no-repeat;
				background-position: center top;
				transition: opacity 0.5s ease-out;
				opacity: 0.1;
			}
			&.good {
				&:after {
					opacity: 0.5;
				}
			}
			&.great {
				&:after {
					opacity: 1;
				}
			}
		}
	}

	.legend {
		text-align: center;
		padding-top: 0.5em;
		padding-bottom: 0.5em;
	}

}
</style>