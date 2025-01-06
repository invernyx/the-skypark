<template>
	<div class="contracts_list" :class="[theme]" ref="mainlist">
		<ContractYoFlight
		v-for="(value, index) in contracts.Contracts"
		v-bind:key="index"
		:index="index"
		:contract="value"
		:templates="contracts.Templates"
		:selected="selected.Contract ? selected.Contract.ID == value.ID : false"
		@select="contractSelect($event)"
		@expand="contractExpand($event)"
		/>
		<div class="contracts_single" v-if="contracts.State === 1">
			<div class="loading-label">
				<span>Searching...</span>
				<span></span>
			</div>
		</div>
		<div class="contracts_single" v-if="contracts.State === 0 && contracts.Contracts.length == 0">
			<span class="label">No contracts</span>
			<span class="description">You haven't started any contract.</span>
		</div>
		<div class="contracts_single" v-if="contracts.State === -1">
			<span class="label">No Transponder</span>
			<span class="description">Please start the Transponder to interact with contracts.</span>
		</div>
		<div></div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import ContractYoFlight from "./../../../../sys/components/contracts/contract_yoflight.vue"

export default Vue.extend({
	name: "contracts_list",
	props: ['theme', 'selected', 'contracts'],
	components: {
		ContractYoFlight
	},
	beforeDestroy() {
		clearInterval(this.ui.mainlist.scrollInterval);
	},
	mounted() {
		this.ui.mainlist.el = (this.$refs['mainlist'] as any);
		['touchstart', 'wheel', 'mousedown'].forEach((ev) => {
			this.ui.mainlist.el.addEventListener(ev, () => {
				if(this.ui.mainlist.scrollInterval){
					clearInterval(this.ui.mainlist.scrollInterval);
				}
			});
		});
		['wheel'].forEach((ev) => {
			this.ui.mainlist.el.addEventListener(ev, (ev1 :any) => {
				this.ui.mainlist.el.scrollLeft += ev1.deltaY;
				event.preventDefault();
			});
		});
	},
	methods: {
		contractSelect(contract :any) {
			this.$emit('select', contract);
		},
		contractExpand(contract :any) {
			this.$emit('expand', contract);
		},
		scrollMove(xOffset: number) {
			clearInterval(this.ui.mainlist.scrollInterval);
			this.ui.mainlist.scrollInterval = setInterval(() => {

				let offsetDif = -(this.ui.mainlist.el.scrollLeft - xOffset);
				if(Math.abs(xOffset - this.ui.mainlist.el.scrollLeft) > 1){
					this.ui.mainlist.el.scrollLeft += Math.ceil(offsetDif / 5);
				} else {
					this.ui.mainlist.el.scrollLeft = xOffset;
					clearInterval(this.ui.mainlist.scrollInterval);
				}
			}, 8);
		},
		scrollTo(index :number) {
			let xOffset = 0;
			if(this.ui.mainlist.el){
				let padding = parseInt(window.getComputedStyle(this.ui.mainlist.el).getPropertyValue('padding-left').replace('px', ''));
				this.ui.mainlist.el.childNodes.forEach((el :HTMLElement, i: number) => {
					if(index >= i && el.nodeName == 'DIV'){
						if(el.classList.contains('contract_yoflight')){
							xOffset = el.offsetLeft - padding;
						}
					}
				});
			}
			this.scrollMove(xOffset);
		},
	},
	data() {
		return {
			ui: {
				mainlist: {
					el: null,
					scrollInterval: null,
				},
			},
			state: {
				contracts: {
					list: {
						Templates: [],
						Contracts: []
					},
				},
			}
		}
	},
	watch: {
		'contracts.Contracts': {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.scrollTo(0);
				}
			}
		},
		'selected.Contract': {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.scrollTo(this.contracts.Contracts.findIndex((x :any) => x.ID == newValue.ID));
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../../../sys/scss/sizes.scss';
@import '../../../../sys/scss/colors.scss';
@import '../../../../sys/scss/mixins.scss';
.contracts_list {
	display: flex;
	flex-direction: row;
	align-self: stretch;
	white-space: nowrap;
	overflow-x: scroll;
	padding-top: 30px;
	padding-bottom: 30px;
	margin-top: -30px;
	margin-bottom: -30px;
	scrollbar-width: none;

	.theme--bright &,
	&.theme--bright {
		.contracts_single {
			background: $ui_colors_bright_shade_1;
		}
	}

	.theme--dark &,
	&.theme--dark {
		.contracts_single {
			background: $ui_colors_dark_shade_1;
		}
	}

	&::-webkit-scrollbar {
		display: none;
	}
	& > div {
		position: relative;
		white-space: normal;
		pointer-events: all;
		&:last-child {
			min-width: 50%;
		}
	}

	.contracts_single {
		width: 240px;
		min-width: 240px;
		border-radius: 14px;
		padding: 16px;
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: center;
		font-family: "SkyOS-SemiBold";
		text-align: center;
		box-sizing: border-box;
		@include shadowed($ui_colors_dark_shade_0);
		.label {
			display: block;
		}
		.description {
			opacity: 0.6;
			font-size: 0.75em;
		}
	}

}
</style>