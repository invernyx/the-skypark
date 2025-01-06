<template>
	<div class="p42_onboarding_theme" :class="theme">
		<div class="theme-paddle">
			<div class="bright" @click="selectBright">
				<div></div>
			</div>
			<div class="dark" :class="{ 'active': theme == 'dark', 'selected': theme != null }"  @click="selectDark">
				<div></div>
			</div>
		</div>
		<div class="center-content">
			<div class="helper_edge_padding">
				<h1 class="title">Select Your Theme</h1>
				<div class="split">
					<div>Light</div>
					<div>Dark</div>
				</div>
				<p class="notice">You can change this theme at any time in the Settings app.</p>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "p42_onboarding_theme",
	data() {
		return {
			theme: null
		};
	},
	methods: {
		selectBright() {
			this.theme = 'theme--bright';
			this.$os.setConfig(['ui', 'theme'], this.theme, true);
			setTimeout(() => {
				this.step('+');
			}, 1000);
		},
		selectDark() {
			this.theme = 'theme--dark';
			this.$os.setConfig(['ui', 'theme'], this.theme, true);
			setTimeout(() => {
				this.step('+');
			}, 1000);
		},
		step(o :string) {
			this.$emit('step', o);
		}
	}
});
</script>

<style lang="scss" scoped>
	@import '../../../../sys/scss/colors.scss';
	.p42_onboarding_theme {
		position: relative;
		flex-grow: 1;
		display: flex;
		flex-direction: column;

		&.theme {
			&--dark,
			&--bright {
				.title {
					opacity: 0;
				}
				.notice {
					opacity: 0;
				}
				.theme-paddle {
					opacity: 0;
				}
				.center-content {
					opacity: 0;
				}
			}
			&--bright {
				.theme-paddle {
					& > div {
						&.dark {
							clip-path: polygon(200% 100%, 100% 100%, 200% 0, 200% 0);
						}
					}
				}
				.center-content {
					transform: translateX(100px);
					.split {
						& div:last-child {
							opacity: 0;
						}
					}
				}
			}
			&--dark {
				.theme-paddle {
					& > div {
						&.dark {
							clip-path: polygon(100% 100%, -100% 100%, 0 0, 100% 0);
						}
					}
				}
				.center-content {
					transform: translateX(-100px);
					.split {
						& div:first-child {
							opacity: 0;
						}
					}
				}
			}
		}

		.theme-paddle {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			background: $ui_colors_bright_shade_1;
			transition: opacity 1s cubic-bezier(.08,0,.1,1);
			& > div {
				position: absolute;
				top: 0;
				bottom: 0;
				left: 0;
				right: 0;
				overflow: hidden;
				transition: clip-path 0.4s cubic-bezier(.08,0,.1,1);
				&.bright {
					& > div {
						background-image: url(../assets/day.jpg);
					}
				}
				&.dark {
					background-color: $ui_colors_dark_shade_1;
					clip-path: polygon(100% 100%, 0 100%, 100% 0, 100% 0);
					& > div {
						background-image: url(../assets/night.jpg);
					}
				}
				&:hover {
					& > div {
						opacity: 0.2;
						transition: opacity 0.4s cubic-bezier(.08,0,.1,1);
					}
				}
				& > div {
					position: absolute;
					top: 0;
					bottom: 0;
					right: 0;
					left: 0;
					opacity: 1;
					background-position: center center;
					background-repeat: no-repeat;
					background-size: cover;
					transition: opacity 3s cubic-bezier(.08,0,.1,1);
				}
			}
		}

		.center-content {
			position: absolute;
			left: 0;
			right: 0;
			top: 0;
			bottom: 0;
			display: flex;
			justify-content: center;
			align-items: center;
			pointer-events: none;
			z-index: 2;
			transition: opacity 0.3s 0.7s cubic-bezier(.08,0,.1,1), transform 0.5s cubic-bezier(.08,0,.1,1);
			.title {
				position: relative;
				z-index: 3;
				color: $ui_colors_dark_shade_1;
				pointer-events: none;
				transition: opacity 0.2s ease-out;
				text-align: center;
				margin-bottom: 200px;
			}
			.notice {
				width: 250px;
				margin: 0 auto;
				font-size: 1em;
				z-index: 3;
				font-size: 1;
				text-align: center;
				color: $ui_colors_bright_shade_1;
				margin-top: 200px;
				transition: opacity 0.2s ease-out;
			}
			.split {
				display: flex;
				flex-direction: row;
				& > div {
					font-family: "SkyOS-SemiBold";
					text-transform: uppercase;
					font-size: 1.4em;
					width: 200px;
					text-align: center;
					&:first-child {
						color: $ui_colors_dark_shade_1;
					}
					&:last-child {
						color: $ui_colors_bright_shade_1;
					}
				}
			}
		}
	}
</style>