<template>
	<div class="p42_onboarding_done" :class="{ 'visible': visible }">
		<div class="center-block">
			<div class="helper_edge_padding">
				<div class="pin-icon"></div>
				<h1>Let's fly!</h1>
				<p>We're done here, let's get to the flying bit!</p>
				<div class="pin-buttons">
					<button_nav @click.native="go">Go!</button_nav>
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
		go() {
			this.step('+');
		},
		step(o :string) {
			this.$emit('step', o);
		}
	}
});
</script>

<style lang="scss" scoped>
	@import '../../../../sys/scss/colors.scss';
	.p42_onboarding_done {
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
				background-image: url(../../../../sys/assets/icons/dark/yes.svg);
			}
		}
		.theme--dark &,
		&.theme--dark {
			.pin-icon {
				background-image: url(../../../../sys/assets/icons/bright/yes.svg);
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