<template>
	<div class="app-box shadowed small nooverflow header" :style="{ 'background-color': accent_right.combined ? 'rgb(' + accent_right.combined + ')' : null }" @click="header_click()">
		<div>
			<div class="loading">
				<div class="loading-label">
					<span></span>
				</div>
			</div>
			<div class="texture" :style="{'background-image': contract ? 'url(' + contract.image_url + ')' : null }"></div>
			<div class="content columns">
				<div class="column">

					<div class="background">
						<div class="image" :style="{'background-image': contract ? 'url(' + contract.image_url + ')' : null }"></div>
					</div>

					<div class="header-top" :style="{ 'text-shadow': accent.shadow }" :class="{ 'is-dark': accent.dark }">
						<div class="columns">

							<Company_badge class="column column_narrow bshadowed" :show_all="true" :style="{ '--box-shadow': accent.shadow }" :companies="contract ? contract.template.company : []" :operated_for="contract ? contract.operated_for : null"/>

							<div class="column">
								<div class="columns">
									<div class="column" v-if="contract">
										<div class="title" v-if="contract.title">{{ contract.title }}</div>
										<div class="title" v-else>Direct Flight</div>
									</div>
								</div>
								<div class="columns">
									<div class="column" v-if="contract">
										<span class="route">{{ contract.route }}</span>
									</div>
								</div>
							</div>

						</div>
					</div>

					<div class="header-bottom">

						<div class="flags">
							<flags v-for="(country, index) in countries" v-bind:key="index" :code="country.toLowerCase()" />
						</div>

					</div>

				</div>
				<div class="column column_narrow header-content" :class="{ 'is-dark': accent_right.dark }">

					<div class="header-data">

						<div class="columns columns_margined" :style="{ 'text-shadow': accent_right.shadow }">
							<div class="column">
								<data_stack label="Reward" class="end" v-if="contract">
									<span class="reward" v-if="contract.reward_bux != 0"><currency class="reward_bux" :amount="contract.reward_bux" :decimals="0"/></span>
									<span class="reward" v-else-if="contract.reward_karma != 0"><span class="amt">{{ contract.reward_karma > 0 ? '+' : '-' }} Karma</span></span>
									<span class="reward" v-else-if="contract.reward_reliability > 5"><span class="amt">+Reliability</span></span>
									<span class="reward" v-else-if="contract.reward_xp > 0"><span class="amt">+XP</span></span>
									<span class="reward" v-else><span class="amt">No reward</span></span>
								</data_stack>
								<data_stack label="Reward" class="end" v-else>----</data_stack>
							</div>
						</div>

						<div class="columns columns_margined" :style="{ 'text-shadow': accent_right.shadow }" v-if="contract ? contract.state != 'Succeeded' : false">
							<div class="column">
								<data_stack label="Length" class="end h_edge_padding_top_half" v-if="contract.duration_range[0] != contract.duration_range[1]">
									<duration :time="contract.duration_range[0]" :decimals="1"/>&nbsp;-&nbsp;<duration :time="contract.duration_range[1]" :decimals="1"/>
								</data_stack>
								<data_stack label="Length" class="end h_edge_padding_top_half" v-else>
									<duration :time="contract.duration_range[0]" :decimals="1"/>
								</data_stack>
							</div>
						</div>
						<div class="columns columns_margined" :style="{ 'text-shadow': accent_right.shadow }" v-else>
							<div class="column">
								<data_stack label="Completed in" class="end h_edge_padding_top_half" v-if="contract">
									{{ moment(contract.completed_at).from(contract.started_at, true) }}
								</data_stack>
								<data_stack label="Length" class="end h_edge_padding_top_half" v-else>----</data_stack>
							</div>
						</div>

						<div class="columns columns_margined">
							<div class="column">
								<button_action class="go shadowed h_edge_margin_top" v-if="contract">View {{ contract.template.type_label }}</button_action>
								<button_action class="go shadowed h_edge_margin_top" v-else>----</button_action>
							</div>
						</div>

					</div>

					<div class="header-gallery" :style="{ 'text-shadow': accent_right.shadow }" v-if="(contract ? contract.template.gallery_url.length || contract.template.image_url.length : false) && open">
						<div class="image-frame" @click="gallery" v-if="contract.template.gallery_url.length">
							<div class="image shadowed-shallow" :style="{ 'background-image': 'url(' + image + ')' }" v-for="(image, index) in contract.template.gallery_url.slice(0, 5)" v-bind:key="index"></div>
						</div>
						<div class="image-frame" @click="gallery" v-else>
							<div class="image shadowed-shallow" :style="{ 'background-image': 'url(' + image + ')' }" v-for="(image, index) in contract.template.image_url.slice(0, 6)" v-bind:key="index"></div>
						</div>
						<div class="label">{{ contract.template.gallery_url.length + contract.template.image_url.length }} Photo{{ contract.template.gallery_url.length + contract.template.image_url.length != 1 ? 's' : '' }}</div>
					</div>

				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"
import Contract, { Situation } from "@/sys/classes/contracts/contract";
import Company_badge from '../../company_badge.vue';

export default Vue.extend({
    props: {
        app: AppInfo,
        contract: Contract,
        accent: Object,
        accent_right: Object,
        countries: Array as () => Array<String>,
        app_panel_content: HTMLElement
    },
    data() {
        return {
            open: false,
            sid: this.app.vendor + "_" + this.app.ident
        };
    },
    methods: {
        header_click() {
            const comp = this.$os.scrollView.get_entity(this.sid + "_contract");
            const scroll_offset = comp.SimpleBar.contentWrapperEl.scrollTop;
            const pTopStr = getComputedStyle(this.app_panel_content).getPropertyValue("--spacing-top");
            const pTopPx = parseInt(pTopStr.replace("px", ""));
            if (scroll_offset < pTopPx - 200) {
                this.$os.scrollView.scroll_to(this.sid + "_contract", 0, pTopPx - 60, 300);
            }
            else {
                this.$os.scrollView.scroll_to(this.sid + "_contract", 0, 0, 300);
            }
        },
        gallery(event: PointerEvent) {
            event.stopPropagation();
            const l = this.contract.template.image_url.concat(this.contract.template.gallery_url);
            this.$os.modals.add({
                type: "gallery",
                data: {
                    images: l,
                    selected_index: 0,
                    title: this.contract.title
                }
            });
        },
        listener_os(wsmsg: any) {
            switch (wsmsg.name) {
                case "uncover": {
                    this.open = wsmsg.payload;
                    break;
                }
                case "covered": {
                    this.open = wsmsg.payload;
                    break;
                }
            }
        },
    },
    mounted() {
        this.$os.eventsBus.Bus.on("os", this.listener_os);
    },
    beforeDestroy() {
        this.$os.eventsBus.Bus.on("os", this.listener_os);
    },
    components: { Company_badge }
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
	color: $ui_colors_dark_shade_0;
	//@include shadowed_text($ui_colors_bright_shade_2);
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
		.header-data {
			opacity: 0;
			transform: scale(0.9);
			pointer-events: none;
		}
		.header-gallery {
			opacity: 1;
			transform: scale(1);
			pointer-events: all;
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
		padding: 14px 16px;
		transform: scale(1);
		transition: opacity 0.2s ease-out, transform 0.2s ease-out;
	}

	.header-gallery {
		opacity: 0;
		position: absolute;
		top: 0;
		left: 0;
		right: 0;
		bottom: 0;
		pointer-events: none;
		display: flex;
		flex-direction: column;
		align-items: center;
    	justify-content: center;
		transform: scale(1.1);
		transition: opacity 0.2s ease-out, transform 0.2s ease-out;
		.image-frame {
			position: relative;
			width: 140px;
			height: 85px;
			.image {
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				border-radius: 8px;
				background-size: cover;
				background-position: center;
				transition: transform 0.1s ease-out;
				transform-origin: left bottom;
				&:nth-of-type(1) {
					transform-origin: center;
					transform: rotate(0) translate(0, 0);
					transition-delay: 0s;
					z-index: 6;
				}
				&:nth-of-type(2) {
					transform: rotate(0) translate(-2px, -2px);
					transition-delay: 0.05s;
					z-index: 5;
				}
				&:nth-of-type(3) {
					transform: rotate(0) translate(-4px, -4px);
					transition-delay: 0.1s;
					z-index: 4;
				}
				&:nth-of-type(4) {
					transform: rotate(0) translate(-6px, -6px);
					transition-delay: 0.15s;
					z-index: 3;
				}
				&:nth-of-type(5) {
					transform: rotate(0) translate(-8px, -8px);
					transition-delay: 0.2s;
					z-index: 2;
				}
				&:nth-of-type(6) {
					transform: rotate(0) translate(-10px, -10px);
					transition-delay: 0.25s;
					z-index: 1;
				}
			}
			&:hover {
				.image {
					&:nth-of-type(1) {
						transform: rotate(0) translate(0, 0) scale(1.04);
					}
					&:nth-of-type(2) {
						transform: rotate(-2deg) translate(-3px, -5px);
					}
					&:nth-of-type(3) {
						transform: rotate(-4deg) translate(-6px, -10px);
					}
					&:nth-of-type(4) {
						transform: rotate(-6deg) translate(-9px, -15px);
					}
					&:nth-of-type(5) {
						transform: rotate(-8deg) translate(-12px, -20px);
					}
					&:nth-of-type(6) {
						transform: rotate(-10deg) translate(-16px, -25px);
					}
				}
			}
		}
		.label {
			margin-top: 4px;
			font-size: 12px;
			font-family: "SkyOS-Bold";
			text-transform: uppercase;
			margin-bottom: -12px;
		}
	}

	.header-content {
		margin-left: -50px;
		min-width: 190px;
		opacity: 1;
		color: $ui_colors_bright_shade_5;
	}

	.is-dark {
		color: $ui_colors_dark_shade_5;
	}

	.flags {
		display: flex;
		font-size: 18px;
		& > span {
			margin-right: 4px;
			border-radius: 4px;
			border: 1px solid $ui_colors_dark_shade_2;
		}
	}

	.title {
		font-size: 16px;
		font-family: "SkyOS-Bold";
		line-height: 1.2em;
		margin-bottom: 0;
		margin-right: 8px;
	}

	.background {
		position: relative;
		background-color: rgba(0,0,0,0.1);
		height: 100%;
		border-radius: 8px;
		mask-image: linear-gradient(to left, rgba(#000, 0) 0, rgba(#000, 1) 100px);
		.image {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			bottom: 0;
			border-radius: 8px;
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