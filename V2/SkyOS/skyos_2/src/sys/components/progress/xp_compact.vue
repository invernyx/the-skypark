<template>
	<div class="progress" v-if="progress" :data-thook="'progress_section_xp'">
		<div class="progress-box">
			<div class="progress-box--header">
				<div>
					<div class="progress-box--title">Level</div>
					<div class="progress-box--balance">{{ rank.level != null ? rank.level : '?' }}</div>
				</div>
				<div>
					<div class="progress-box--title">XP</div>
					<div class="progress-box--balance">{{ progress.Balance != undefined ? Math.round(progress.Balance).toLocaleString('en-gb') : '?' }} <span class="change">(+{{ change }})</span></div>
				</div>
			</div>
			<div class="progress-box--trend">
				<svg viewBox="0 0 100 100" preserveAspectRatio="none">
					<linearGradient id="gradient_p_xp" x2="0" y2="1">
						<stop offset="0%" />
						<stop offset="100%" />
					</linearGradient>
					<polygon :fill="'url(#gradient_p_xp)'" class="polygon" :points="trendPoly"></polygon>
					<polyline class="polyline" :points="trendLine" vector-effect="non-scaling-stroke"></polyline>
				</svg>
			</div>
		</div>
		<div class="progress-box">
			<div class="progress-box--steps">
				<div class="progress-box--steps-progress">
					<div :style="'right:' + levelProg"></div>
				</div>
				<div class="progress-box--steps-level">{{ rank.level != null ? (rank.level + 1) : '?' }}</div>
				<div class="progress-box--steps-label-top">Level Up</div>
				<div class="progress-box--steps-label-bottom"><span>{{ rank.delta != null ? Math.round(rank.delta).toLocaleString('en-gb') : '?' }}</span>/<span>{{ rank.range != null ? rank.range.toLocaleString('en-gb') : '?' }}</span></div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "xp_compact",
	props: ['progress', 'change'],
	data() {
		return {
			trendLine: '',
			trendPoly: '',
			rank: {
				level: null,
				range: null,
				delta: null,
				progress: null,
				gainProgress: null
			},
			progressInRank: null,
			gainInRank: null,
			levelProg: '0%',
		}
	},
	methods: {
		makeTrend() {
			if(this.progress.Trend) {
				const line = [] as string[];
				const poly = [] as string[];
				const count = this.progress.Trend.length - 1;
				let max = Math.max.apply(Math, (this.progress.Trend.map((x :any) => x.Value)));
				let min = Math.min.apply(Math, (this.progress.Trend.map((x :any) => x.Value)));
				if(max < 10) {
					max = 10;
				}
				const delta = max - min;

				poly.push('0, 100');
				this.progress.Trend.forEach((val :any, index :number) => {
					const date = new Date(val.Date);
					const loc = ((100/count) * index) + ',' + (100 + (-(100/delta) * (val.Value - min)));
					poly.push(loc);
					line.push(loc);
				});
				poly.push('100, 0');
				poly.push('100, 100');

				this.trendLine = line.join(' ');
				this.trendPoly = poly.join(' ');
			}
		},
		getRank() {
			if(this.progress.Balance != null) {
				this.rank = Eljs.CalculateRank(this.progress.Balance, 0);
				this.progressInRank = this.rank.progress * 100;
				this.gainInRank = this.rank.gainProgress * 100;
				this.levelProg = (100 - this.progressInRank) + '%';
			}
		},
	},
	mounted() {
		this.getRank();
		this.makeTrend();
	},
	watch: {
		progress() {
			this.getRank();
			this.makeTrend();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

	.progress {

		.theme--bright &,
		&.theme--bright {
			&-box {
				&--trend {
					svg {
						filter: drop-shadow(0 0px 8px $ui_colors_bright_button_info);
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_bright_button_info, 0.2);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_bright_button_info, 0);
						}
						.polyline {
							stroke: $ui_colors_bright_button_info;
						}
					}
				}
				&--steps {
					&-progress {
						background: $ui_colors_bright_shade_2;
						& > div {
							background: $ui_colors_bright_button_info;
							filter: drop-shadow(0 0px 8px $ui_colors_bright_button_info);
						}
					}
					&-level {
						border-color: $ui_colors_bright_shade_2;
					}
				}
			}
		}
		.theme--dark &,
		&.theme--dark {
			&-box {
				&--trend {
					svg {
						filter: drop-shadow(0 0px 8px $ui_colors_dark_button_info);
						stop:nth-child(1) {
							stop-color: rgba($ui_colors_dark_button_info, 0.2);
						}
						stop:nth-child(2) {
							stop-color: rgba($ui_colors_dark_button_info, 0);
						}
						.polyline {
							stroke: $ui_colors_dark_button_info;
						}
					}
				}
				&--steps {
					&-progress {
						background: $ui_colors_dark_shade_1;
						& > div {
							background: $ui_colors_dark_button_info;
							filter: drop-shadow(0 0px 8px $ui_colors_dark_button_info);
						}
					}
					&-level {
						border-color: $ui_colors_dark_shade_1;
					}
				}
			}
		}

		&-box {
			position: relative;
			margin-bottom: 16px;
			&--header {
				position: absolute;
				display: flex;
				z-index: 2;
				& > div {
					margin-right: 2em;
				}
			}
			&--title {
				font-size: 1em;
			}
			&--balance {
				font-size: 1.6em;
				line-height: 1em;
				font-family: "SkyOS-SemiBold";
				.change {
					font-family: "SkyOS-Regular";
					font-size: 0.75em;
				}
			}
			&--trend {
				svg {
					display: block;
					overflow: visible;
					height: 50px;
					width: 100%;
					margin: 0;
					.polyline {
						fill: none;
						stroke-width: 2;
					}
				}
			}
			&--steps {
				display: flex;
				justify-content: stretch;
				align-items: center;
				padding: 0;
				&-progress {
					position: relative;
					height: 2px;
					margin: 0;
					border-radius: 1px;
					flex-grow: 1;
					& > div {
						position: absolute;
						left: 0;
						top: 0;
						bottom: 0;
						border-radius: 3px;
					}
				}
				&-level {
					display: flex;
					min-width: 1.75em;
					height: 1.75em;
					padding: 0 0.4em;
					margin: 6px;
					margin-right: 0;
					font-family: "SkyOS-SemiBold";
					font-size: 1.5em;
					border-radius: 1em;
					align-items: center;
					box-sizing: border-box;
					justify-content: center;
					border: 2px solid transparent;
				}
				&-label {
					&-top {
						position: absolute;
						left: 0;
						top: 0;
					}
					&-bottom {
						position: absolute;
						left: 0;
						bottom: 0;
						& span {
							font-family: "SkyOS-SemiBold";
							display: inline-block;
							&:first-child {
								margin-right: 0.2em;
							}
							&:last-child {
								margin-left: 0.2em;
							}
						}
					}
				}
			}
			&--footer {
				margin-bottom: 2em;
			}
		}
	}

</style>