<template>
	<div class="contracts_list" :class="[theme]" ref="mainlist">
		<ContractFeatured
			v-for="(value, index) in contracts.Featured"
			v-bind:key="'f' + index"
			:index="index"
			:feature="value"
			:selected="selected.Featured ? selected.Featured.FileNameE == value.FileNameE : false"
			@select="featuredSelect($event)"
			@hide="featuredHide"
			@browse="featuredBrowse"
		/>
		<ContractSummary
			v-for="(value, index) in contracts.Contracts"
			v-bind:key="'s' + index"
			:index="index"
			:contract="value"
			:templates="contracts.Templates"
			:selected="selected.Contract ? selected.Contract.ID == value.ID : false"
			@select="contractSelect($event)"
			@expand="contractExpand($event)"
			@expire="contractExpire($event)"
		/>
		<div class="contracts_single" v-if="contracts.State === 1">
			<div class="loading-label">
				<span>Searching...</span>
				<span></span>
			</div>
		</div>
		<div class="contracts_single" v-if="contracts.State === 0 && contracts.Contracts.length == 0">
			<span class="label">No results</span>
			<span class="hasFilters" v-if="hasFilters">You have active filters</span>
			<span class="description">Try moving the map to another location <span v-if="hasFilters">or change your filters</span></span>
		</div>
		<div class="contracts_single" v-if="contracts.State === -1">
			<span class="label">No Transponder</span>
			<span class="description">Please start the Transponder to search for contracts.</span>
		</div>
		<div></div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import ContractSummary from "@/sys/components/contracts/contract_summary.vue"
import ContractFeatured from "@/sys/components/contracts/contract_feature.vue"

export default Vue.extend({
	name: "contracts_list",
	props: ['theme', 'selected', 'contracts', 'hasFilters'],
	components: {
		ContractFeatured,
		ContractSummary
	},
	beforeDestroy() {
		clearInterval(this.ui.mainlist.scrollInterval);
	},
	mounted() {
		this.ui.mainlist.el = (this.$refs['mainlist'] as any);

		['keydown'].forEach((ev) => {
			this.ui.mainlist.el.addEventListener(ev, (e :any) => {
				if([32, 37, 38, 39, 40].indexOf(e.keyCode) > -1) {
					e.preventDefault();
				}
			});
		});
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
		featuredSelect(featured :any) {
			this.$emit('featuredSelect', featured);
		},
		featuredHide(featuredName :any) {
			this.$emit('featuredHide', featuredName);
		},
		featuredBrowse(featuredQuery :any) {
			this.$emit('featuredBrowse', featuredQuery);
		},
		contractSelect(contract :any) {
			this.$emit('select', contract);
		},
		contractExpand(contract :any) {
			this.$emit('expand', contract);
		},
		contractExpire(contract :any) {
			this.$emit('expire', contract);
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
						if(el.classList.contains('contract_summary') || el.classList.contains('contract_feature')){
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
					this.scrollTo(this.contracts.Contracts.findIndex((x :any) => x.ID == newValue.ID) + this.contracts.Featured.length);
				}
			}
		},
		'selected.Featured': {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.scrollTo(this.contracts.Featured.findIndex((x :any) => x.FileNameE == newValue.FileNameE));
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

	.theme--bright &,
	&.theme--bright {
		.contracts_single {
			background: $ui_colors_bright_shade_1;
			.hasFilters {
				color: $ui_colors_bright_button_cancel;
			}
		}
	}

	.theme--dark &,
	&.theme--dark {
		.contracts_single {
			background: $ui_colors_dark_shade_1;
			.hasFilters {
				color: $ui_colors_dark_button_cancel;
			}
		}
	}

	display: flex;
	flex-direction: row;
	white-space: nowrap;
	overflow-x: scroll;
	padding-top: 30px;
	padding-bottom: 30px;
	margin-top: -30px;
	margin-bottom: -30px;
	scrollbar-width: none;
	&::-webkit-scrollbar {
		display: none;
	}
	& > div {
		position: relative;
		white-space: normal;
		&:last-child {
			min-width: 50%;
		}
	}
	.contracts_single {
		width: 240px;
		min-width: 240px;
		height: 185px;
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
		.hasFilters {
			font-size: 0.75em;
			margin-bottom: 8px;
		}
	}

}
</style>