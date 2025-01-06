<template>
	<div :class="[this.appName, this.app.app_nav_class]">
		<div class="app-frame">
			<width_limiter size="screen" :shadowed="true">
				<Step0 v-if="step == 0" @step="stepAction"/>
				<Step1 v-else-if="step == 1" @step="stepAction"/>
				<Step2 v-else-if="step == 2" @step="stepAction"/>
			</width_limiter>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_bundle"

export default Vue.extend({
	name: "p42_onboarding",
	props: {
		inst: Object,
		app: AppInfo,
		appName: String
	},
	components: {
		Step0: () => import('./views/theme.vue'),
		Step1: () => import('./views/top.vue'),
		Step2: () => import('./views/done.vue'),
	},
	mounted() {
		this.$emit('loaded');
	},
	methods: {
		stepAction(o :string) {
			const os = o.split('_');
			switch(os[0]) {
				case '+': {
					this.step++;
					break;
				}
				case '-': {
					this.step--;
					break;
				}
			}

			if(this.step >= this.max) {
				this.$os.setConfig(['onboarding', 'hasSetup'], true);
				const p = this.$os.getState(['services','userProgress']);
				if(p.XP.Balance == 0) {
					this.$os.setState(['ui','tutorials'], true);
					setTimeout(() => {
						this.$router.push({ name: 'p42_home' });
					}, 1000);
				} else {
					this.$router.push({ name: 'p42_home' });
				}
			}
		}
	},
	data() {
		return {
			ready: true,
			step: 0,
			max: 3
		}
	},
});
</script>

<style lang="scss">
@import './../../../sys/scss/sizes.scss';
@import './../../../sys/scss/colors.scss';
.p42_onboarding {

	.theme--bright &,
	&.theme--bright {
		.app-frame {


		}
	}
	.theme--dark &,
	&.theme--dark {
		.app-frame {


		}
	}

	.width_limiter_content {
		justify-content: stretch;
	}

	.center-block {

	}
}
</style>