<template>
	<modal type="grow" width="narrow" class="translucent" @close="closeChangeLog">
		<content_controls_stack :translucent="true" :scroller_offset="{ top: 10 }">
			<template v-slot:content>
				<div class="helper_edge_padding_half">
					<Changelog :limit="2"/>
				</div>
			</template>
			<template v-slot:tab>
				<div class="helper_edge_margin_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<toggle class="small no-wrap" v-model="dontShow">Don't show again</toggle>
						</div>
						<div class="column column_narrow">
							<button_action @click.native="closeChangeLog" class="go">Close</button_action>
						</div>
					</div>
				</div>
			</template>
		</content_controls_stack>
	</modal>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "invoice",
	props: ['md'],
	methods: {
		closeChangeLog() {
			if(this.dontShow) {
				this.$root.$data.config.ui.changelogShow = false;
				(this.$root as any).stateSave();
			}
			this.$emit('close');
		}
	},
	components: {
		Changelog: () => import("@/sys/components/changelog/log.vue"),
	},
	beforeDestroy() {
	},
	data() {
		return {
			dontShow: false,
		}
	}
});
</script>

<style lang="scss" scoped>
	/deep/ .modal_content {
		border-top-left-radius: 23px;
		border-top-right-radius: 23px;
	}
</style>