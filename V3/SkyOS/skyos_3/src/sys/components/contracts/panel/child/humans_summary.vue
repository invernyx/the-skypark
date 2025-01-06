<template>
	<div class="pax_group_humans">
		<div class="pax_group_humans_emotion" v-for="(group, index) in humans_group_by_feel" :key="index">
			<div class="pax_group_humans_stack">
				<Human v-for="(human, index1) in group.humans.slice(0, group.show)" :key="index1" :human="human" :show_actions="false" :style="{ '--human-icon-size': '32px' }"></Human>
			</div>
			<div class="pax_group_humans_hero">
				<Human :human="group.hero" :show_actions="false" :style="{ '--human-icon-size': '32px' }"></Human>
			</div>
			<div class="pax_group_humans_count">
				<number :amount="group.humans.length + 1"></number>
			</div>
		</div>
		<div class="pax_group_humans_more" @click="$os.routing.goTo({ name: 'p42_inflight' })">
			<div>...</div>
		</div>
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import { AppInfo } from '@/sys/foundation/app_model';
import Human from '@/sys/components/cabin/human.vue';
import Vue from 'vue';

export default Vue.extend({
	props: {
		contract: Contract,
		humans :Array
	},
    components: { Human },
	computed: {
        humans_group_by_feel() {
            const groups = [];

			let total = 0;

            this.humans.forEach((human :any) => {
				if(human.state.boarded) {
					const existing_feel = groups.find(x => x.icon == human.state.icon);
					if (existing_feel) {
						existing_feel.humans.push(human);
					}
					else {
						groups.push({
							icon: human.state.icon,
							show: 5,
							humans: [],
							hero: human
						});
					}
					total += 1;
				}
            });

			var total_available = 20;
			var show_per_head = 1 / total * total_available;

			groups.forEach(group => {
				group.show = Math.min(show_per_head * group.humans.length, 8);
			});

            return groups.sort((x1, x2) => x2.humans.length - x1.humans.length);
        },
	},
	watch: {
		humans() {
			this.$forceUpdate();
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.pax {

	.theme--bright &,
	&.theme--bright {
		&_group {
			&_humans {
				&_emotion {
					background-color: $ui_colors_bright_shade_2;
				}
				&_more {
					background-color: $ui_colors_bright_shade_2;
					&:hover {
						background-color: darken($ui_colors_bright_shade_2, 2%);
					}
					&:active {
						background-color: darken($ui_colors_bright_shade_2, 5%);
					}
				}
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		&_group {
			&_humans {
				&_emotion {
					background-color: $ui_colors_dark_shade_2;
				}
				&_more {
					background-color: $ui_colors_dark_shade_2;
					&:hover {
						background-color: lighten($ui_colors_dark_shade_2, 5%);
					}
					&:active {
						background-color: lighten($ui_colors_dark_shade_2, 8%);
					}
				}
			}
		}
	}


	&_group {
		&_humans {
			display: flex;
			font-family: "SkyOS-Bold";
			flex-wrap: wrap;
			&_emotion {
				display: flex;
				align-items: center;
				border-radius: 8px;
				margin-right: 8px;
				margin-bottom: 8px;
			}
			&_more {
				display: flex;
				align-items: center;
				border-radius: 8px;
				margin-right: 8px;
				margin-bottom: 8px;
				padding: 0 0.8em;
				cursor: pointer;
				transition: transform 1s cubic-bezier(0, 1, 0.15, 1), opacity 0.5s cubic-bezier(0, 1, 0.15, 1);
			}
			&_hero {
				display: flex;
				.human-icon {
					margin-bottom: 0;
				}
			}
			&_stack {
				display: flex;
				opacity: 0.5;
				.human-icon {
					margin-right: -25px;
					margin-bottom: 0;
					transform: scale(0.8);
					&:last-child {
						margin-right: -22px;
					}
				}
			}
			&_count {
				text-align: center;
				margin-left: 4px;
				margin-right: 10px;
			}
		}
	}
}

</style>