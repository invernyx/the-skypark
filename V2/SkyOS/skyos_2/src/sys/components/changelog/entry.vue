<template>
	<div class="changelog-entry">
		<div v-if="color && entry.image" class="changelog-entry-background" :style="'background-color:' + ($root.$data.config.ui.theme == 'theme--bright' ? color : colorDark) + ';background-image:url(' + entry.image + ')'"></div>
		<div class="changelog-entry-content">
			<div class="changelog-entry-title" v-if="entry.title">{{ entry.title }}</div>
			<div class="changelog-entry-image" v-if="entry.image"><img :src="entry.image"/></div>
			<div class="changelog-entry-date" :class="{ 'small': entry.title && !entry.image }">{{  moment.utc(entry.date).calendar(null, { sameDay: "[Today at] HH:mm [GMT]", lastDay: "[Yesterday at] HH:mm [GMT]", lastWeek: "dddd", sameElse: "MMM Do, YYYY" }) }}</div>
			<div class="changelog-entry-build">{{ entry.build }}<span class="hotfix" v-if="entry.hotfix === true">Hotfix</span></div>
			<div class="changelog-entry-notes" v-if="entry.notes ? entry.notes.length : false">
				<div class="changelog-entry-note" v-for="(note, index) in entry.notes" v-bind:key="index">{{ note }}</div>
			</div>
			<div class="changelog-entry-changes">
				<div class="changelog-entry-type" :class="'type-' + typeName" v-for="(type, typeName, index) in entry.changes" v-bind:key="index">
					<div class="changelog-entry-typename">{{ typeName }}</div>
					<div class="changelog-entry-change" v-for="(change, index) in type" v-bind:key="index">
						<div class="changelog-entry-change-type"></div>
						<div class="changelog-entry-change-content">
							<div class="changelog-entry-change-title" v-if="change.title">{{ change.title }}</div>
							<div class="changelog-entry-change-image" v-if="change.image"><img :src="change.image"/></div>
							<div class="changelog-entry-change-label" v-for="(label, index) in change.label" v-bind:key="index">{{ label }}</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue'

export default Vue.extend({
	name: "changelog_entry",
	props: ['entry'],
	data() {
		return {
			colorIsDark: false,
			color: null,
			colorDark: null,
		}
	},
	methods: {
	},
	mounted() {
		if(this.entry.image) {
			this.$root.$data.services.colorSeek.find(this.entry.image, (color :any) => {
				this.color = color.color;
				this.colorDark = color.colorDark;
				this.colorIsDark = color.colorIsDark;
			});
		} else {
			this.colorIsDark = false;
			this.color = null;
			this.colorDark = null;
		}
	},
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';

.changelog {

	.theme--bright &,
	&.theme--bright {
		&-entry {
			background: $ui_colors_bright_shade_0;
			@include shadowed_shallow($ui_colors_bright_shade_5);
			&-build {
				color: rgba($ui_colors_bright_shade_5, 0.5);
				.hotfix {
					background-color:  $ui_colors_bright_button_cancel;
					color: $ui_colors_bright_shade_0;
				}
			}
			&-type {
				&.type {
					&-new {
						.changelog-entry-typename {
							background-color:  $ui_colors_bright_button_go;
						}
						.changelog-entry-change-type {
							background: $ui_colors_bright_button_go;
						}
					}
					&-improved {
						.changelog-entry-typename {
							background-color:  $ui_colors_bright_button_gold;
						}
						.changelog-entry-change-type {
							background: $ui_colors_bright_button_gold;
						}
					}
					&-fixed {
						.changelog-entry-typename {
							background-color:  $ui_colors_bright_button_info;
						}
						.changelog-entry-change-type {
							background: $ui_colors_bright_button_info;
						}
					}
					&-removed {
						.changelog-entry-typename {
							background-color:  $ui_colors_bright_shade_3;
						}
						.changelog-entry-change-type {
							background: $ui_colors_bright_shade_3;
						}
					}
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		&-entry {
			background: $ui_colors_dark_shade_2;
			@include shadowed_shallow($ui_colors_dark_shade_1);
			&-build {
				color: rgba($ui_colors_dark_shade_5, 0.3);
				.hotfix {
					background-color: $ui_colors_dark_button_cancel;
					color: $ui_colors_dark_shade_5;
				}
			}
			&-type {
				&.type {
					&-new {
						.changelog-entry-typename {
							background-color:  $ui_colors_dark_button_go;
						}
						.changelog-entry-change-type {
							background: $ui_colors_dark_button_go;
						}
					}
					&-improved {
						.changelog-entry-typename {
							background-color:  $ui_colors_dark_button_gold;
						}
						.changelog-entry-change-type {
							background: $ui_colors_dark_button_gold;
						}
					}
					&-fixed {
						.changelog-entry-typename {
							background-color:  $ui_colors_dark_button_info;
						}
						.changelog-entry-change-type {
							background: $ui_colors_dark_button_info;
						}
					}
					&-removed {
						.changelog-entry-typename {
							background-color:  $ui_colors_dark_shade_1;
						}
						.changelog-entry-change-type {
							background: $ui_colors_dark_shade_1;
						}
					}
				}
			}
		}
	}

	&-entry {
		position: relative;
		padding: 16px;
		padding-top: 9px;
		margin-bottom: 16px;
		border-radius: 16px;
		overflow: hidden;
		&-background {
			position: absolute;
			top: 0;
			right: 0;
			height: 450px;
			left: 0;
			opacity: 0.8;
			background-repeat: no-repeat;
			background-position: center;
			background-size: cover;
			transform: scale(1.01);
			filter: blur(20px) brightness(1) contrast(1);
			mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) 0, rgba(0, 0, 0, 0) 90%);
		}
		&-content {
			position: relative;
			z-index: 2;
		}
		&-title {
			font-family: "SkyOS-SemiBold";
			font-size: 1.4em;
			line-height: 1.2em;
			margin-bottom: 0.75em;
		}
		&-date {
			font-family: "SkyOS-SemiBold";
			font-size: 1.4em;
			&.small {
				font-family: inherit;
				font-size: 1em;
			}
		}
		&-build {
			margin-bottom: 1em;
			display: flex;
			.hotfix {
				display: inline-flex;
				align-items: center;
				font-size: 10px;
				border-radius: 4px;
				padding: 0 4px;
				margin-left: 8px;
				text-transform: uppercase;
			}
		}
		&-note {
			margin-bottom: 0.5em;
		}
		&-notes {
			margin-bottom: 1.4em;
		}
		&-type {
			display: flex;
			flex-direction: column;
			align-items: flex-start;
			margin-bottom: 1em;
			&:last-child {
				margin-bottom: 0;
			}
		}
		&-typename {
			font-family: "SkyOS-SemiBold";
			color: #FFF;
			text-transform: uppercase;
			padding: 0.2em 0.6em;
			margin-bottom: 0.5em;
			border-radius: 4px;
			flex-shrink: 1;
		}
		&-image {
			margin-left: -8px;
			margin-right: -8px;
			margin-bottom: -8px;
			img {
				height: auto;
				width: 100%;
				border-radius: 8px;
				margin-bottom: 1em;
				box-shadow: 0 0 1px rgba(0,0,0,0.8), 0 4px 16px rgba(0,0,0,0.3);
			}
		}
		&-change {
			display: flex;
			align-content: stretch;
			position: relative;
			margin-bottom: 0.5em;
			&-type {
				display: block;
				content: '';
				width: 4px;
				border-top-left-radius: 4px;
				border-bottom-left-radius: 4px;
				margin-top: 0.3em;
				margin-right: 0.7em;
			}
			&-content {
				flex-basis: 0;
				flex-grow: 1;
				padding-top: 0.5em;
				padding-bottom: 0.5em;
				& > :last-child {
					margin-bottom: 0;
				}
			}
			&-title {
				font-family: "SkyOS-SemiBold";
				margin-bottom: 0.5em;
			}
			&-label {
				margin-bottom: 0.5em;
			}
			&-image {
				img {
					display: block;
					width: 100%;
					border-radius: 8px;
					margin-bottom: 1em;
					box-shadow: 0 0 1px rgba(0,0,0,0.8), 0 4px 16px rgba(0,0,0,0.3);
				}
			}
		}
	}
}
</style>