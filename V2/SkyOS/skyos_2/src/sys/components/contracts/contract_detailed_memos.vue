<template>
	<div class="contract_detailed_memos">
		<ChatFrame v-if="template.Company.includes('coyote')" :thread="contract.Memos" :empty_message_source="['pablo', 'Pablo']" :empty_message="'Why are you looking here, I did not call you. \nBack to work!'" :hasScroll="false" />
		<ChatFrame v-else :thread="contract.Memos" :empty_message_source="['brigit', 'Brigit']" :empty_message="'Here you\'ll find a transcript of any incoming voicemails for this ' + template.TypeLabel + '. You can even replay them. \nThere are none at this time.'" :hasScroll="false" />
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import ChatFrame from "@/sys/components/chat.vue"

export default Vue.extend({
	name: "contract_detailed_memos",
	props: ['contract', 'template'],
	components: {
		ChatFrame
	},
	data() {
		return {

		}
	},
	computed: {

	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

.contract_detailed_memos {

	.chat {
		padding-bottom: 14px;
	}

}
</style>