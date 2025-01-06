<template>
	<div class="collapser" :class="{ 'collapser_expanded': expanded, 'collapser_transition': doneTimeout != null }" v-if="!isReversed">
		<div class="collapser_title collapser_arrowed" :class="{ 'arrow_inline': arrowInline }" @click="toggle" v-if="withArrow">
			<slot name="title" :expanded="expanded"></slot>
		</div>
		<div class="collapser_title" @click="toggle" v-else>
			<slot name="title" :expanded="expanded"></slot>
		</div>
		<div class="collapser_content" ref="content">
			<div>
				<slot name="content" v-if="expanded || doneTimeout != null || animate || preload" :expanded="expanded"></slot>
			</div>
		</div>
		<div class="collapser_sub" @click="toggle" v-if="$slots['sub']">
			<slot name="sub"></slot>
		</div>
	</div>
	<div class="collapser collapser_reversed" :class="{ 'collapser_expanded': expanded, 'collapser_transition': doneTimeout != null }" v-else>
		<div class="collapser_content" ref="content">
			<div>
				<slot name="content" v-if="expanded || doneTimeout != null || animate || preload" :expanded="expanded"></slot>
			</div>
		</div>
		<div class="collapser_title collapser_arrowed" :class="{ 'arrow_inline': arrowInline }" @click="toggle" v-if="withArrow">
			<slot name="title" :expanded="expanded"></slot>
		</div>
		<div class="collapser_title" @click="toggle" v-else>
			<slot name="title" :expanded="expanded"></slot>
		</div>
		<div class="collapser_sub" @click="toggle" v-if="$slots['sub']">
			<slot name="sub"></slot>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "collapser",
	props: ['default', 'isReversed', 'state', 'withArrow', 'arrowInline', 'preload', 'animate'],
	data() {
		return {
			doneTimeout: null,
			expanded: false,
		}
	},
	beforeMount() {
		if(this.default !== undefined) {
			this.expanded = this.default;
		}
		if(this.state !== undefined) {
			this.expanded = this.state;
		}
	},
	mounted() {
		if(!this.expanded) {
			const content = (this.$refs.content as HTMLElement);
			content.style.height = '0px';
		}
		this.set(this.expanded);
	},
	beforeDestroy() {
		clearTimeout(this.doneTimeout);
	},
	methods: {
		set(state :boolean) {
			clearTimeout(this.doneTimeout);
			this.expanded = state;
			const content = (this.$refs.content as HTMLElement);
			this.doneTimeout = setTimeout(() => {
				this.doneTimeout = null;
				if(state) {
					content.style.height = null;
				}
			}, 300);
			window.requestAnimationFrame(() => {
				content.style.height = content.scrollHeight + 'px';
				if(!state) {
					content.style.height = content.scrollHeight + 'px';
					window.requestAnimationFrame(() => { setTimeout(() => { content.style.height = '0px'; }, 1); });
				}
			});
		},
		toggle() {
			this.set(!this.expanded);
		},
	},
	watch: {
		state() {
			this.set(this.state);
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../scss/sizes.scss';
@import '../scss/colors.scss';
@import '../scss/mixins.scss';
.collapser {
	display: flex;
	flex-direction: column;
	align-items: flex-start;
	&.fill {
		.collapser_title {
			flex-grow: 1;
			align-self: stretch;
		}
	}

	.theme--bright &,
	#app .theme--bright & {
		.collapser_arrowed {
			.collapser_arrow {
				background-image: url(../../sys/assets/framework/dark/arrow_right.svg);
			}
		}
	}

	.theme--dark &,
	#app .theme--dark & {
		.collapser_arrowed {
			.collapser_arrow {
				background-image: url(../../sys/assets/framework/bright/arrow_right.svg);
			}
		}
	}

	.collapser_arrowed {
		.collapser_arrow {
			width: 1em;
			height: 1em;
			margin-left: 0.25em;
			background-repeat: no-repeat;
			background-size: contain;
			background-position: center;
			transition: transform 0.1s cubic-bezier(.3,0,.24,1);
		}
	}
	.collapser_sub {
		cursor: pointer;
		width: 100%;
	}
	.collapser_title {
		position: relative;
		cursor: pointer;
		width: 100%;
		display: flex;
		justify-content: space-between;
		align-items: center;
		& > div:first-child {
			flex-grow: 1;
		}
	}
	.collapser_content {
		opacity: 0;
		align-self: stretch;
		pointer-events: none;
		transform: scale(0.9,0.9);
		transform-origin: center top;
		transition: transform 0.3s cubic-bezier(.3,0,1,.2), height 0.5s cubic-bezier(.7,0,.4,1), opacity 0.3s cubic-bezier(.3,0,.24,1); // out
	}
	&_transition {
		.collapser_content {
			overflow: visible;
		}
	}
	&_expanded {
		.collapser_content {
			visibility: visible;
			opacity: 1;
			pointer-events: all;
			transform: scale(1,1);
			transition: transform 0.3s 0.2s cubic-bezier(0,.3,.2,1), height 0.3s cubic-bezier(.3,0,.2,1), opacity 0.5s 0.2s cubic-bezier(.3,0,.24,1); // in
		}
		& > .collapser_arrowed {
			&.collapser_title .collapser_arrow {
				transform: rotate(90deg);
			}
		}
		&.collapser_reversed {
			& > .collapser_arrowed {
				&.collapser_title .collapser_arrow {
					transform: rotate(-90deg);
				}
			}
		}
	}
}
</style>