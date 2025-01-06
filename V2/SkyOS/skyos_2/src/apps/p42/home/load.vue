<template>
	<div :data-appname="this.appName">
		<ap v-bind="$props"/>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"
import ap from './app.vue';

export default Vue.extend({
	name: "p42_home",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		ap,
	},
});
</script>

<style lang="scss" scoped>

div {
	z-index: 0 !important;
}

.transition {

	&.is-zoomed-in {
		transform: scale(1);
	}
	&.is-zoomed-out {
		transform: scale(1);
		transition: transform 0.3s cubic-bezier(.37,.87,.5,1);
	}

	&-in-zoom {
		transition: transform 0.3s 0.5s cubic-bezier(.37,.87,.5,1), opacity 0.3s 0.5s cubic-bezier(.37,.87,.5,1);
		transform: scale(1);
		opacity: 1;
		z-index: 0;
		&-pre {
			transform: scale(1);
			opacity: 1;

			/deep/.tiles {
				& .tile {
					transform: translate3D(0, 0, 500px);
				}
			}

			/deep/.backsplash-image {
				filter: contrast(200%);
				transform: scale(1.2);
				transition: filter 0s ease-out 1s, transform 0.4s cubic-bezier(0.5, 0, 0.75, 0.45);
			}
		}
	}

	&-out-zoom {
		transform: scale(1);
		opacity: 0;
		transition: transform 0.5s cubic-bezier(.37,.87,.5,1), opacity 0.5s cubic-bezier(.37,.87,.5,1);

		/deep/.tiles {
			& .tile {
			transform: translate3D(0, 0, 500px);
			}
		}

		/deep/.backsplash-image {
			filter: contrast(200%);
			transform: scale(2);
			transition: filter 0s ease-out 1s, transform 0.4s cubic-bezier(0.5, 0, 0.75, 0.45);
		}

		&-pre {
			transform: scale(1);
			opacity: 1;
		}
	}

	/deep/.app-frame {
		background-color: #000;
    }
}
</style>