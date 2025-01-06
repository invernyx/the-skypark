<template>
	<div class="progress" v-if="progress" :data-thook="'progress_section_bank'">
		<div class="progress-box">
			<div class="progress-box--header">
				<div>
					<div class="progress-box--title">Bank Balance</div>
					<div class="progress-box--balance">${{ progress.Balance != null ? Math.round(progress.Balance).toLocaleString('en-gb') : '?' }} <span class="change" v-if="change !== 0">({{ (change >= 0 ? '+$' : '-$') + Math.abs(change) }})</span></div>
				</div>
			</div>
			<div class="progress-box--trend">
				<svg viewBox="0 0 100 100" preserveAspectRatio="none">
					<linearGradient id="gradient_p_bank" x2="0" y2="1">
						<stop offset="0%" />
						<stop offset="100%" />
					</linearGradient>
					<polygon :fill="'url(#gradient_p_bank)'" class="polygon" :points="trendPoly"></polygon>
					<polyline class="polyline" :points="trendLine" vector-effect="non-scaling-stroke"></polyline>
				</svg>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "bank_compact",
	props: ['progress', 'change'],
	data() {
		return {
			trendLine: '',
			trendPoly: '',
			level: 1,
			progressInRank: 0,
			gainInRank: 0,
			levelUp: false,
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
			const rank = Eljs.CalculateRank(this.progress.Balance, 0);
			this.levelUp = rank.levelUp;
			this.level = rank.level;
			this.progressInRank = rank.progress * 100;
			this.gainInRank = rank.gainProgress * 100;
		},
	},
	mounted() {
		this.makeTrend();
		this.getRank();
	},
	watch: {
		progress() {
			this.makeTrend();
			this.getRank();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

	.progress {

		&-box {
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
				box-sizing: border-box;
				svg {
					display: block;
					overflow: visible;
					height: 50px;
					width: 100%;
					margin: 0;
					filter: drop-shadow(0 0px 8px #57964A);
					stop:nth-child(1) {
						stop-color: rgba(#57964A, 0.2);
					}
					stop:nth-child(2) {
						stop-color: rgba(#57964A, 0);
					}
					.polyline {
						fill: none;
						stroke: #57964A;
						stroke-width: 2;
					}
				}
			}
			&--footer {
				margin-bottom: 2em;
			}
		}
	}

</style>