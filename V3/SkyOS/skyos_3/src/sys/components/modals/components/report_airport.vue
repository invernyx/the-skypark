<template>
	<modal type="grow" width="narrow" @close="close">
		<scroll_stack :translucent="true" :scroller_offset="{ top: 10 }">
			<template v-slot:content>
				<div class="h_edge_padding_half">
					<div class="background-map" v-if="md.data.Airport">
						<div class="background-map-image" :style="'background-image:url(' + $os.api.getCDN('images', 'airports/' + md.data.Airport.ICAO + '.jpg') + ')'"></div>
						<div class="background-map-overlay">
							<div>
								<span>{{ md.data.Airport.Name }}</span> - <strong>{{ md.data.Airport.ICAO }}</strong>
							</div>
							<div>
								<flags class="location-info-flag" :code="md.data.Airport.Country.toLowerCase()" /> {{ md.data.Airport.CountryName }}
							</div>
						</div>
					</div>

					<h2>Airport</h2>
					<div class="buttons_list shadowed">
						<button_listed icon="theme/warn" @click.native="sendReport('dont_like')">I don't like airports like this</button_listed>
						<button_listed icon="theme/warn" @click.native="sendReport('elevation_glitch')">Elevation gliches</button_listed>
						<button_listed icon="theme/warn" @click.native="sendReport('unaccessible_parking')">Parking location unaccessible</button_listed>
					</div>
					<h2>Runways</h2>
					<div class="buttons_list shadowed">
						<button_listed icon="theme/warn" @click.native="sendReport('runway_too_short')">Too short</button_listed>
						<button_listed icon="theme/warn" @click.native="sendReport('physics_glitch')">Ground physics glitches</button_listed>
						<button_listed icon="theme/warn" @click.native="sendReport('runway_no_exist')">No longer exists</button_listed>
						<button_listed icon="theme/warn" @click.native="sendReport('object_on_runway')">Object on runway</button_listed>
					</div>
				</div>
			</template>
			<template v-slot:tab>
				<div class="h_edge_margin_half">
					<button_action @click.native="close" class="cancel icon icon-close"></button_action>
				</div>
			</template>
		</scroll_stack>
	</modal>
</template>

<script lang="ts">
import Vue from 'vue';
import Notification from '@/sys/models/notification'
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	name: "report_airport",
	props: ['md'],
	methods: {
		close() {
			this.$emit('close');
		},
		sendReport(type :string) {
			this.$os.analytics.TrackEvent("ReportAirport", this.md.data.Airport.ICAO, type);
			this.$os.notifications.addNotification(new Notification({
				UID: Eljs.getNumGUID(),
				Type: 'Status',
				Title: 'New report for ' + this.md.data.Airport.ICAO + " (" + this.md.data.Airport.Name + ")",
				Message: "Thank you for your report.",
			}));
			this.close();
		}
	},
	components: {

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
	.background-map {
		position: relative;
		height: 160px;
		border-radius: 8px;
		color: #FFF;
		box-sizing: border-box;
		background-color: rgba(#000000, 0.2);
		transition: height 0.6s cubic-bezier(.25,0,.14,1);
		&-image {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			background-size: cover;
			background-position: center center;
			border-radius: 8px;
			will-change: transform;
			transition: opacity 5s cubic-bezier(.25,0,.14,1);
			&:after {
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				border-radius: 8px;
				content: '';
				border: 1px solid rgba(255,255,255,0.2);
			}
		}
		&-overlay {
			& > div {
				position: absolute;
				box-sizing: border-box;
				overflow: hidden;
				text-overflow: ellipsis;
				white-space: nowrap;
				max-width: calc(100% - 8px);
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
	}
</style>