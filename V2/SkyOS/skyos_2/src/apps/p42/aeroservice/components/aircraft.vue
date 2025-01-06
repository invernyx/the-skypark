<template>
	<div class="aircraft">
		<div class="columns columns_margined_half">
			<div class="column">
				<div class="aircraft-background">
					<div class="aircraft-background-image">
						<img src="https://i.imgur.com/rKdM0w1.png" alt="">
					</div>
				</div>
			</div>
			<div class="column">
				<div class="aircraft-header">
					<button_nav class="aircraft-header-delete cancel compact">Erase</button_nav>
					<div class="aircraft-header-manufacturer">{{ meta.Manufacturer }}</div>
					<div class="aircraft-header-model">{{ meta.Model }}</div>
					<div class="aircraft-header-creator">{{ meta.Creator }}</div>
				</div>
				<Progress label="Damage" :percent="meta.DamagePcnt" type="cancel"/>
			</div>
		</div>
		<div class="aircraft-actions">
			<div>
			</div>
			<div>

			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Progress from './../components/progress.vue'

/*
	//LastLivery

	//InsuranceLast
	//InsuranceExpire
	//InsuranceBilled

	//TotaledInstances
	//DamagePcnt

	//DutyFirstFlown
	//DutyLastFlown
	//DutyFlightHoursMonth
	//DutyFlightHours
	//DutyLastLocation
	//Simulators
	//DirectoryName
	//DirectoryFull


*/

export default Vue.extend({
	name: "aircraft",
	props: ['meta'],
	components: {
		Progress
	},
	beforeDestroy() {

	},
	mounted() {
	},
	methods: {

	},
	data() {
		return {

		}
	},
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';

.aircraft {

	.theme--bright &,
	&.theme--bright {
		background: $ui_colors_bright_shade_0;
		@include shadowed_shallow($ui_colors_bright_shade_5);
	}

	.theme--dark &,
	&.theme--dark {
		background: $ui_colors_dark_shade_0;
	}

	position: relative;
	display: block;
	border-radius: 16px;
	padding: 8px;

	& > .columns {
		& > .column {
			&:last-child {
				min-width: 60%;
    			flex-grow: 0;
			}
		}
	}


	&-header {
		position: relative;
		margin-bottom: 8px;
		&-delete {
			position: absolute;
			top: 0;
			right: 0;
		}
		&-manufacturer {
			font-size: 16px;
			line-height: 1.2em;
		}
		&-model {
			font-size: 16px;
			line-height: 1.2em;
			font-family: "SkyOS-SemiBold";
		}
		&-creator {
			font-size: 12px;
			line-height: 1em;
			margin-top: 2px;
		}
	}

	&-background {
		position: relative;
		&-image {
			position: relative;
			box-sizing: border-box;
			img {
				display: block;
				width: 100%;
				height: auto;
				border-radius: 8px;
				box-sizing: border-box;
			}
		}
		/*
		.shade-top {
			position: absolute;
			left: 0;
			right: 0;
			top: 0;
			border-radius: 8px;
			padding: 8px 8px 4px 8px;
			background: linear-gradient(to bottom, rgba(#FFFFFF,0.8), cubic-bezier(.2,0,.4,1), rgba(#FFFFFF,0));
		}
		.shade-bottom {
			position: absolute;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 8px;
			padding: 8px 8px 4px 8px;
			background: linear-gradient(to top, rgba(#FFFFFF,0.8), cubic-bezier(.2,0,.4,1), rgba(#FFFFFF,0));
		}
		*/
	}

	&-actions {
		display: flex;
		justify-content: space-between;
		margin-top: 8px;
	}

}
</style>