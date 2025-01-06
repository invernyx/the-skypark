<template>
	<div class="companies">
		<div class="company" :class="'company-' + company" v-for="(company, index) in companies" v-bind:key="index"></div>
		<div class="company company-airline" v-if="operated_for" :style="'background-image:url(' + $os.api.getCDN('images', 'airlines/s/' + operated_for[2]+ '.png') + ')'"></div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
	props: {
		companies: Array,
		operated_for: Array,
		show_all: Boolean
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';

.companies {
	display: flex;
	flex-direction: row;
	will-change: transform;
	margin-top: 4px;
	.company {
		width: 30px;
		height: 30px;
		border-radius: 50%;
		margin-right: 8px;
		margin-left: -20px;
		background-size: cover;
		background-repeat: no-repeat;
		background-position: center;
		box-shadow: 0 0 10px rgba(0,0,0,0.5);
		&:first-child {
			margin-left: 0;
		}
		&-airline {
			background-color: #FFF;
			background-size: contain;
			background-size: 70%;
		}
		&-clearsky {
			background-color: #FFF;
			background-image: url(../../sys/assets/icons/companies/logo_clearsky.svg);
		}
		&-coyote {
			background-color: #111;
			background-image: url(../../sys/assets/icons/companies/logo_coyote.svg);
		}
		&-skyparktravel {
			background-color: #FFF;
			background-image: url(../../sys/assets/icons/companies/logo_skyparktravel.svg);
		}
		&-oceanicair {
			background-color: #FFF;
			background-image: url(../../sys/assets/icons/companies/logo_oceanicair.svg);
		}
		&-lastflight {
			background-color: #FFF;
			background-image: url(../../sys/assets/icons/companies/logo_lastflight.svg);
		}
	}
	&.bshadowed {
		.company{
			box-shadow: var(--box-shadow);
		}
	}
}

</style>