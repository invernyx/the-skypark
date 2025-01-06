<template>
	<div class="p42_onboarding_top" :class="{ 'visible': visible }">
		<div class="center-block">
			<div class="helper_edge_padding">
				<div class="pin-icon"></div>
				<h1>Always on top</h1>
				<p>Do you want The Skypad to stay on top of the simulator and other windows?</p>
				<div class="pin-buttons">
					<button_nav @click.native="no">No</button_nav>
					<button_nav @click.native="yes">Yes</button_nav>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_onboarding_top",
	data() {
		return {
			visible: false,
		};
	},
	mounted() {
		setTimeout(() => {
			this.visible = true;
		}, 100);
	},
	methods: {
		yes() {
			this.step('+');
			(this.$root as any).setTopmost(true);
		},
		no() {
			this.step('+');
			(this.$root as any).setTopmost(false);
		},
		step(o :string) {
			this.visible = false;
			setTimeout(() => {
				this.$emit('step', o);
			}, 1000);
		}
	}
});
</script>

<style lang="scss" scoped>
	@import '../../../../sys/scss/colors.scss';
	.p42_onboarding_top {
		display: flex;
		flex-grow: 1;
		justify-content: center;
		align-items: center;
		text-align: center;
		opacity: 0;
		transition: opacity 0.5s ease-out;
		&.visible {
			opacity: 1;
		}

		.theme--bright &,
		&.theme--bright {
			.pin-icon {
				background-image: url(./../assets/pin_black.svg);
			}
		}
		.theme--dark &,
		&.theme--dark {
			.pin-icon {
				background-image: url(./../assets/pin_white.svg);
			}
		}

		.center-block {
			max-width: 400px;
			& > div {
				display: flex;
				flex-direction: column;
				align-items: center;
			}
		}

		.pin-icon {
			width: 90px;
			height: 90px;
			background-repeat: no-repeat;
			background-size: contain;
			background-position: center;
		}

		.pin-buttons {
			display: flex;
			flex-direction: row;
			margin-top: 20px;
			& > div {
				position: relative;
				width: 60px;
				border-radius: 8px;
				padding: 10px;
				cursor: pointer;
				&:first-child {
					background-color: #444;
					color: #FFF;
					margin-right: 10px;
					&:hover {
						background-color: lighten(#444, 5%);
					}
				}
				&:last-child {
					background-color: rgb(70, 70, 200);
					color: #FFF;
					&:hover {
						background-color: lighten(rgb(70, 70, 200), 5%);
					}
				}
			}
		}
	}
</style>