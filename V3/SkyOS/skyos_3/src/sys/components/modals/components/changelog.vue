<template>
	<modal type="grow" width="narrow" class="translucent" @close="closeChangeLog">
		<scroll_stack :translucent="true" :scroller_offset="{ top: 10 }">
			<template v-slot:content>
				<div class="h_edge_padding_half">
					<Changelog :limit="2"/>
				</div>
			</template>
			<template v-slot:tab>
				<div class="h_edge_margin_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<toggle class="small no-wrap" v-model="dontShow">Don't show again</toggle>
						</div>
						<div class="column column_narrow">
							<button_action @click.native="closeChangeLog" class="go icon icon-close"></button_action>
						</div>
					</div>
				</div>
			</template>
		</scroll_stack>
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
				this.$os.userConfig.set(['ui','changelog','show'], false);
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