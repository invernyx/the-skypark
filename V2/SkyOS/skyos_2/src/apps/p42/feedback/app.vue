<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame app-frame--clear-bottom">
			<width_limiter size="screen">
				<iframe src="https://docs.google.com/forms/d/e/1FAIpQLScUs_gSASvfux4aUjV4I781DwS8tH_1TjSxCQFO0YUN__SjoQ/viewform?embedded=true" frameborder="0" marginheight="0" marginwidth="0">Loading…</iframe>
			</width_limiter>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_feedback",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {},
	mounted() {
		this.$emit('loaded');
	},
	data() {
		return {

		}
	},
});
</script>

<style lang="scss">
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
.p42_feedback {
	.theme--bright &,
	&.theme--bright {
		.app-frame {
			background: $ui_colors_bright_shade_1;
		}
	}
	.theme--dark &,
	&.theme--dark {
		.app-frame {
			background: $ui_colors_dark_shade_4;
		}
	}

	iframe {
		display: block;
		margin-top: $status-size;
		margin-bottom: 18px;
		width: 100%;
		height: 100%;
	}
}
</style>