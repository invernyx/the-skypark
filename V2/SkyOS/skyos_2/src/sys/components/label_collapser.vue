<template>
	<div class="collapser" :class="{ 'collapser_expanded': expanded, 'collapser_transition': doneTimeout != null }">
		<div class="collapser_icon" @click="toggle">
			<slot name="icon"></slot>
		</div>
		<div class="collapser_label" ref="label">
			<div>
				<slot name="label"></slot>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "label_collapser",
	props: ['state'],
	data() {
		return {
			doneTimeout: null,
			expanded: false,
		}
	},
	beforeMount() {
		if(this.state !== undefined) {
			this.expanded = this.state;
		}
	},
	mounted() {
		if(!this.expanded) {
			const label = (this.$refs.label as HTMLElement);
			label.style.width = '0px';
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
			const label = (this.$refs.label as HTMLElement);
			this.doneTimeout = setTimeout(() => {
				this.doneTimeout = null;
				if(state) {
					label.style.width = null;
				}
			}, 500);
			window.requestAnimationFrame(() => {
				var smallStyles = getComputedStyle(label);
				label.style.width = label.scrollWidth + 'px';
				if(!state) {
					label.style.width = label.scrollWidth + 'px';
					window.requestAnimationFrame(() => { setTimeout(() => { label.style.width = '0px'; }, 1); });
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
	display: inline-flex;
	flex-direction: row;
	align-items: center;
	&.fill {
		.collapser_icon {
			flex-grow: 1;
			align-self: stretch;
		}
	}

	.theme--bright &,
	#app .theme--bright & {

	}

	.theme--dark &,
	#app .theme--dark & {

	}

	.collapser_icon {
		position: relative;
		cursor: pointer;
	}
	.collapser_label {
		opacity: 0;
		align-self: stretch;
		pointer-events: none;
		overflow: hidden;
		white-space: nowrap;
		transform: scale(1);
		transform-origin: center top;
		//transition: transform 0.3s cubic-bezier(.3,0,1,.2), width 0.5s cubic-bezier(.7,0,.4,1), opacity 0.3s cubic-bezier(.3,0,.24,1); // out
		transition: transform 0.3s 0.2s cubic-bezier(0,.3,.2,1), width 0.3s cubic-bezier(.3,0,.2,1), opacity 0.1s cubic-bezier(.3,0,.24,1); // out
	}
	&_transition {
		.collapser_label {
			overflow: visible;
		}
	}
	&_expanded {
		.collapser_label {
			overflow: visible;
			visibility: visible;
			opacity: 1;
			pointer-events: all;
			transform: scale(1,1);
			transition: transform 0.3s 0.2s cubic-bezier(0,.3,.2,1), width 0.2s 0.1s cubic-bezier(.3,0,.2,1), opacity 0.5s 0.2s cubic-bezier(.3,0,.24,1); // in
		}
	}
}
</style>