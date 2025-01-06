<template>

	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true" :scroller_offset="{ bottom: 15 }">
		<template v-slot:nav>
			<h2>Legal</h2>
		</template>
		<template v-slot:content>
			<div class="helper_edge_padding helper_nav-margin">
				For legal agreements, please visit the <a href='https://tfdidesign.com/legal.php' target='_blank'>TFDi Design legal page</a>.

			</div>
		</template>
	</content_controls_stack>

</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_settings_legal",
	methods: {
		resetAppsConfig() {

		},
		RewatchTutorial() {

		}
	}
});
</script>

<style lang="scss">
	@import '../../../../sys/scss/colors.scss';
	.p42_settings_legal {

	}
</style>