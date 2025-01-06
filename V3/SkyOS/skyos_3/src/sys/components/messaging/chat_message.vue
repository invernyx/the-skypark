<template>
	<div class="group" ref="group" :class="{ 'collapse_message': collapse_message }" @click="read_more">
		<div class="message_content" v-for="(message, index) in message_content.split('\\n').filter(x => x.length)" v-bind:key="index">
			<div>{{ message }}</div>
		</div>
		<div class="read_more" v-if="collapse_message">Read More</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	props: {
		message_content :String,
		collapse_messages: Boolean
	},
	methods: {
		init() {
			this.overflowing = true;
			this.collapse_message = this.collapse_messages;
			clearInterval(this.height_check);
			this.height_check = setInterval(() => {
				this.checkHeight();
			}, 1000);
		},
		checkHeight() {
			const el = (this.$refs.group as HTMLElement);
			if(el) {
				this.overflowing = el.scrollHeight > el.clientHeight;
				this.collapse_message = this.overflowing;
			}
		},
		read_more() {
			clearTimeout(this.height_check);
			this.collapse_message = false;
		}
	},
	components: {
	},
	mounted() {
		this.collapse_message = true;
		this.init();
	},
	beforeMount() {
	},
	beforeDestroy() {
		clearInterval(this.height_check);
	},
	data() {
		return {
			height_check: null,
			collapse_message: false,
			overflowing: false,
		}
	},
	watch: {
		message_content: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
$borderRadius: 0.75em;

.group {
	display: flex;
	flex-direction: column;
	overflow: hidden;
	margin-bottom: 8px;
	border-radius: $borderRadius;
	border-bottom-left-radius: $borderRadius / 8;
	border-bottom-right-radius: $borderRadius;

	.theme--bright & {
		background: $ui_colors_bright_shade_0;
		@include shadowed_shallow($ui_colors_bright_shade_5);
		&.collapse_message {
			.read_more {
				background-image: linear-gradient(to top, rgba($ui_colors_bright_shade_1, 1), cubic-bezier(.54,0,.65,1), rgba($ui_colors_bright_shade_1, 0.6));
			}
		}
	}

	.theme--dark & {
		color: $ui_colors_dark_shade_0;
		background: rgba($ui_colors_dark_shade_4, 0.9);
		@include shadowed_shallow($ui_colors_dark_shade_0);
		&.collapse_message {
			.read_more {
				background-image: linear-gradient(to top, rgba($ui_colors_dark_shade_0, 1), cubic-bezier(.54,0,.65,1), rgba($ui_colors_dark_shade_0, 0.6));
			}
		}
	}

	.message_content {
		padding: 0.5em 0.75em;
		padding-bottom: 0;
		margin-bottom: 0.30em;
		&:first-child {
			margin-top: 0;
		}
		&:last-child {
			margin-bottom: 0;
			padding-bottom: 0.5em;
		}
	}


	&.collapse_message {
		cursor: pointer;
		max-height: 108px;
		transition: transform 0.1s cubic-bezier(.3,0,.24,1);
		&:hover {
			transform: scale(1.02);
		}
		.read_more {
			position: absolute;
			right: 0;
			bottom: 0;
			left: 0;
			box-sizing: border-box;
			padding: 8px;
			display: flex;
			justify-content: center;
			align-items: flex-end;
			border-bottom-left-radius: $borderRadius;
			border-bottom-right-radius: $borderRadius;
		}
	}
}
</style>