<template>
	<div class="aircraft" :class="{ 'selected': selected ? selected.id == aircraft.id : false }" @click="$emit('details')" :data-contract="aircraft.id" :data-scrollvisibility="'hidden'">
		<div class="background" :style="{'background-image': aircraft ? 'url(' + aircraft.image_blob + ')' : null }"></div>
		<div class="content">
			<div class="model"><strong>{{ aircraft.model }}</strong></div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';

export default Vue.extend({
	props: {
		aircraft: Aircraft,
		index :Number,
		selected :Aircraft,
	},
	components: {
	},
	data() {
		return {
			scroll_visible: false,
			load: false,
			theme: this.$os.userConfig.get(['ui','theme']),
		}
	},
	methods: {
		init() {
		},

		listener_navigate(wsmsg :any) {
			switch(wsmsg.name){
				case 'to': {
					break;
				}
			}
		},
		listener_os(wsmsg :any) {
			switch(wsmsg.name){
				case 'themechange': {
					this.theme = this.$os.userConfig.get(['ui','theme']);
					this.init();
					break;
				}
			}
		},
	},
	mounted() {
		this.init();
	},
	beforeMount() {
		this.$os.eventsBus.Bus.on('os', this.listener_os);
		this.$os.eventsBus.Bus.on('navigate', this.listener_navigate);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('os', this.listener_os);
		this.$os.eventsBus.Bus.off('navigate', this.listener_navigate);
	},
	watch: {
		contract: {
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		},
		scroll_visible: {
			handler(newValue, oldValue) {
				if(newValue && !this.load) {
					this.load = true;
				}
			}
		}
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
@import '@/sys/scss/helpers.scss';

.aircraft {
	position: relative;
	overflow: hidden;
	cursor: pointer;
	margin: 8px;
	margin-bottom: 0;

	.theme--bright & {
		border-color: rgba($ui_colors_bright_shade_5, 0.4);
		color: $ui_colors_bright_shade_5;
		@include shadowed_text($ui_colors_bright_shade_0);
		.content {
			background-color: rgba($ui_colors_bright_shade_5, 0.1);
		}
		&:hover {
			.content {
				background-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
		}
		&::after {
			border-color: rgba($ui_colors_bright_shade_5, 0.2s);
		}
		&.selected {
			&::after {
				border-color: $ui_colors_bright_button_info;
			}
			.background {
				& > div {
					opacity: 0.2;
				}
			}
		}
	}
	.theme--dark & {
		border-color: rgba($ui_colors_dark_shade_5, 0.3);
		color: $ui_colors_dark_shade_0;
		@include shadowed_text($ui_colors_dark_shade_5);
		.content {
			background-color: rgba($ui_colors_dark_shade_5, 0.1);
		}
		&:hover {
			.content {
				background-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
		}
		&::after {
			border-color: rgba($ui_colors_dark_shade_5, 0.2);
		}
		&.selected {
			&::after {
				border-color: $ui_colors_bright_button_info;
			}
			.background {
				& > div {
					opacity: 0.4;
				}
			}
		}
	}

	&::after {
		content: '';
		position: absolute;
		top: 0;
		left: 0;
		bottom: 0;
		right: 0;
		transition: border 0.2s ease-out;
		border: 1px solid transparent;
		border-radius: 8px;
		pointer-events: none;
	}

	&.selected {
		&::after {
			border-width: 4px;
		}
		.content {
			border-left-color: #FFFFFF;
		}
	}

	.background {
		position: absolute;
		top: 0;
		right: 0;
		bottom: 0;
		left: 0;
		border-radius: 8px;
		background-size: cover;
		background-position: center;
	}

	.content {
		position: relative;
		padding: 14px 16px;
		border-radius: 8px;
		z-index: 2;
		height: 100px;

	}

}
</style>