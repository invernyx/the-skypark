<template>

	<content_controls_stack :translucent="true" :status_padding="true" :shadowed="true">
		<template v-slot:nav>
			<h2 class="scroll-fade-in">General</h2>
		</template>
		<template v-slot:content>
			<div class="helper_edge_padding helper_nav-margin">
				<h1 class="scroll-fade-out">General</h1>
				<div class="buttons_list shadowed">
					<button_listed>Test</button_listed>
				</div>
				<div class="buttons_list shadowed">
					<button_listed>Test</button_listed>
					<button_listed>Test</button_listed>
				</div>
				<p class="notice">Lorem ipsum dolor sit amet consectetur adipisicing elit. Neque deserunt sed maiores alias repellendus sapiente qui labore eos unde, atque omnis, quaerat assumenda hic id minus aliquid. Consectetur, exercitationem minima.</p>
				<p>Lorem ipsum dolor sit amet consectetur adipisicing elit. Neque deserunt sed maiores alias repellendus sapiente qui labore eos unde, atque omnis, quaerat assumenda hic id minus aliquid. Consectetur, exercitationem minima.</p>
				<h1>This is a title</h1>
				<div class="buttons_list shadowed">
					<button_listed>Test</button_listed>
					<button_listed>Test</button_listed>
					<button_listed>Test</button_listed>
					<button_listed>Test</button_listed>
				</div>
				<p>Lorem ipsum dolor sit amet consectetur adipisicing elit. Neque deserunt sed maiores alias repellendus sapiente qui labore eos unde, atque omnis, quaerat assumenda hic id minus aliquid. Consectetur, exercitationem minima.</p>
				<p>Lorem ipsum dolor sit amet consectetur adipisicing elit. Maiores, nihil aliquid voluptatum libero veritatis natus, tenetur cupiditate, distinctio voluptate deleniti quia! Ea ipsa soluta asperiores, rerum repellendus dolorum ad ex!</p>
				<p>Lorem ipsum, dolor sit amet consectetur adipisicing elit. Culpa, esse dignissimos ipsam quibusdam saepe omnis harum pariatur et maxime at delectus, inventore, eveniet officiis ut qui possimus impedit ipsum quasi.</p>
				<p>Lorem ipsum, dolor sit amet consectetur adipisicing elit. Culpa, esse dignissimos ipsam quibusdam saepe omnis harum pariatur et maxime at delectus, inventore, eveniet officiis ut qui possimus impedit ipsum quasi.</p>
				<p>Lorem ipsum, dolor sit amet consectetur adipisicing elit. Culpa, esse dignissimos ipsam quibusdam saepe omnis harum pariatur et maxime at delectus, inventore, eveniet officiis ut qui possimus impedit ipsum quasi.</p>
				<p>Lorem ipsum, dolor sit amet consectetur adipisicing elit. Culpa, esse dignissimos ipsam quibusdam saepe omnis harum pariatur et maxime at delectus, inventore, eveniet officiis ut qui possimus impedit ipsum quasi.</p>
			</div>
		</template>
		<template v-slot:tab>
			<div></div>
		</template>
	</content_controls_stack>

</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_settings_general",
});
</script>

<style lang="scss">
	@import '../../../../sys/scss/colors.scss';
	.p42_settings_general {

	}
</style>