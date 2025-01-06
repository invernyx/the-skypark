<template>
	<div class="app-box shadowed small nooverflow header" @click="header_click()">
		<div>
			<div class="loading">
				<div class="loading-label">
					<span></span>
				</div>
			</div>
			<div class="texture" :style="{'background-image': aircraft ? 'url(' + aircraft.image_blob + ')' : null }"></div>
			<div class="content columns">
				<div class="column">
					<div class="background">
						<div class="image" :style="{'background-image': aircraft ? 'url(' + aircraft.image_blob + ')' : null }"></div>
					</div>
				</div>
				<div class="column column_narrow header-content">

					<div class="header-data">

						<div class="columns columns_margined">
							<div class="column">
								<data_stack label="Location" class="end" v-if="aircraft">{{ aircraft.location ? (aircraft.nearest_airport ? aircraft.nearest_airport.icao : "Off-field") : "Anywhere" }}</data_stack>
								<data_stack label="Location" class="end" v-else>----</data_stack>
							</div>
						</div>

						<div class="columns columns_margined h_edge_padding_top_half">
							<div class="column">
								<data_stack label="Max Load" class="end" v-if="aircraft">
									<weight :amount="aircraft.max_weight - aircraft.empty_weight" :decimals="0"/>
								</data_stack>
								<data_stack label="Max Load" class="end" v-else>----</data_stack>
							</div>
						</div>

						<div class="columns columns_margined">
							<div class="column">
								<button_action class="go shadowed h_edge_margin_top" v-if="aircraft">View aircraft</button_action>
								<button_action class="go shadowed h_edge_margin_top" v-else>----</button_action>
							</div>
						</div>

					</div>

				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Aircraft from '@/sys/classes/aircraft';

export default Vue.extend({
	props: {
		app: AppInfo,
		aircraft: Aircraft,
		app_panel_content :HTMLElement
	},
	data() {
		return {
			open: false,
			sid: this.app.vendor + '_' + this.app.ident
		}
	},
	methods: {

		header_click(state? :boolean) {
			const comp = this.$os.scrollView.get_entity(this.sid + '_aircraft');
			const scroll_offset = comp.SimpleBar.contentWrapperEl.scrollTop;
			const pTopStr = getComputedStyle(this.app_panel_content).getPropertyValue('--spacing-top');
			const pTopPx = parseInt(pTopStr.replace('px',''))
			if(scroll_offset < pTopPx - 200 || state) {
				this.$os.scrollView.scroll_to(this.sid + '_aircraft', 0, pTopPx - 60, 300);
			} else {
				this.$os.scrollView.scroll_to(this.sid + '_aircraft', 0, 0, 300);
			}
		},

		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'uncover': {
					this.open = wsmsg.payload;
					break;
				}
				case 'covered': {
					this.open = wsmsg.payload;
					break;
				}
			}
		},
	},
	beforeMount() {
		if(this.$route.params.expand) {
			this.open = true;
		}
	},
	mounted() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
	},
	watch: {
		aircraft() {
			if(this.aircraft) {
				if(this.$route.params.expand) {
					this.header_click(true);
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.header {
	overflow: hidden;
	transition: background 1s ease-out, transform 0.1s cubic-bezier(.3,0,.24,1);
	will-change: transform;
	cursor: pointer;
	&:hover {
		transform: scale(1.01);
	}

	.loading & {
		transition: background 0.1s ease-out, transform 0.1s cubic-bezier(.3,0,.24,1);
		.content {
			opacity: 0;
			transition: none;
		}
		.texture {
			opacity: 0;
			transition: none;
		}
		.loading {
			opacity: 1;
			transition: opacity 0.4s 0.1s ease-out;
		}
	}

	.texture,
	.content {
		transition: opacity 1s ease-out;
	}

	.is-open & {
		&:hover {
			transform: scale(1);
		}
		.header-content {
			width: 0;
			min-width: 0;
			opacity: 0;
			height: 220px;
			transition: opacity 0.2s ease-out, width 0.3s cubic-bezier(.3,0,.38,.97), min-width 0.3s cubic-bezier(.3,0,.38,.97), height 0.3s cubic-bezier(.3,0,.38,.97);
		}
		.header-data {
			transform: scale(0.9);
			pointer-events: none;
		}
	}

	& > div {
		transition: 0.5s ease-out;
	}

	.header-top {
		position: absolute;
		left: 0;
		top: 0;
		z-index: 2;
		padding: 14px 16px;
		margin-right: 40px;
	}

	.header-bottom {
		position: absolute;
		left: 0;
		bottom: 0;
		z-index: 2;
		padding: 16px;
	}

	.header-data {
		box-sizing: border-box;
		padding: 14px 16px;
		width: 150px;
		transform: scale(1);
		transition: opacity 0.2s ease-out, transform 0.2s ease-out;
	}

	.header-content {
		opacity: 1;
		height: 165px;
		width: 150px;
		min-width: 150px;
		transition: opacity 0.2s ease-out, width 0.5s cubic-bezier(0,.5,.38,.97), min-width 0.3s cubic-bezier(0,.5,.38,.97), height 0.3s cubic-bezier(0,.5,.38,.97);
	}

	.background {
		position: relative;
		background-color: rgba(0,0,0,0.1);
		height: 100%;
		border-radius: 12px;
		@include shadowed($ui_colors_dark_shade_2);
		.image {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 12px;
			background-size: cover;
			background-position: center;
		}
	}

	.texture {
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		transform: scale(1);
		opacity: 0.6;
		background-size: cover;
		background-position: center;
		filter: blur(20px) brightness(1) contrast(1);
		will-change: transform, filter;
	}

	.loading {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		font-size: 19px;
		z-index: 10;
		display: flex;
		opacity: 0;
		justify-content: center;
		align-items: center;
		font-family: "SkyOS-Bold";
		pointer-events: none;
	}
}

</style>