<template>
	<div class="map_context" ref="container" @click="$emit('close')">
		<div v-if="actions[0].type == 'airport'" :class="'type-' + actions[0].type">
			<div class="type-airport-background-map">
				<div class="type-airport-background-map-image" :style="'background-image:url(' + $os.getCDN('images', 'airports/' + actions[0].data.airport.ICAO + '.jpg') + ')'"></div>
				<div class="type-airport-background-map-overlay">
					<div>
						<strong>{{ actions[0].data.airport.ICAO }}</strong> <span>{{ actions[0].data.airport.Name }}</span>
					</div>
					<div>
						<span><flags class="location-info-flag" :code="actions[0].data.airport.Country.toLowerCase()" />{{ actions[0].data.airport.CountryName }}</span>
					</div>
				</div>
			</div>
			<div class="type-airport-actions">
				<button_action @click.native="filter('-' + actions[0].data.airport.ICAO)">Arrivals</button_action>
				<button_action @click.native="filter(actions[0].data.airport.ICAO + '-')">Departures</button_action>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "map_context",
	props: ["actions"],
	methods: {
		filter(query :string) {
			this.$emit('filter', query);
		}
	},
	components: {

	},
	mounted() {
		(this.$refs.container as HTMLElement).addEventListener('mouseup', (e :any) => {
			e.stopPropagation();
		});
	},
	beforeDestroy() {

	},
	data() {
		return {
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.map_context {
	position: relative;
	border-radius: 8px;
	overflow: hidden;
	max-width: 200px;
	cursor: not-allowed;
	backdrop-filter: blur(10px);

	.theme--bright &,
	&.theme--bright,
	#app .theme--bright & {
		.type {
			&-airport {
				&-background-map {
					background: $ui_colors_bright_shade_5;
				}
				&-actions {
					.button_action {
						background-color: rgba($ui_colors_bright_shade_1, 0.5);
						&:hover {
							background-color: rgba($ui_colors_bright_shade_2, 0.8);
						}
					}
				}
			}
		}
		@include shadowed_shallow($ui_colors_bright_shade_5);
	}

	.theme--dark &,
	&.theme--dark,
	#app .theme--dark & {
		.type {
			&-airport {
				&-background-map {
					background: $ui_colors_dark_shade_0;
				}
				&-actions {
					.button_action {
						background-color: rgba($ui_colors_dark_shade_1, 0.5);
						&:hover {
							background-color: rgba($ui_colors_dark_shade_2, 0.8);
						}
					}
				}
			}
		}
		@include shadowed_shallow($ui_colors_dark_shade_0);
	}


	.type {
		&-airport {
			&:after {
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				border-radius: 8px;
				content: '';
				border: 1px solid rgba(255,255,255,0.2);
				pointer-events: none;
			}
			&-background-map {
				position: relative;
				&-image {
					width: 100%;
					height: 150px;
					background-size: cover;
					background-position: center center;
					will-change: transform;
					transition: opacity 5s cubic-bezier(.25,0,.14,1);
				}
				&-overlay {
					& > div {
						position: absolute;
						box-sizing: border-box;
						overflow: hidden;
						text-overflow: ellipsis;
						white-space: nowrap;
						max-width: calc(100% - 8px);
						color: $ui_colors_bright_shade_0;
						background-color: rgba(#000000, 0.6);
						//backdrop-filter: blur(3px);
						border-radius: 4px;
						padding: 4px 8px;
						&:first-child {
							top: 4px;
							left: 4px;
						}
						&:last-child {
							bottom: 4px;
							left: 4px;
						}
					}
				}
				.flag  {
					margin-right: 4px;
				}
			}
			&-actions {
				display: flex;
				margin-top: 1px;
				.button_action {
					flex-grow: 1;
					border-radius: 0;
					margin-right: 1px;
					background-color: transparent;
					&:last-child {
						margin-right: 0;
					}
				}
			}
		}
	}
}
</style>