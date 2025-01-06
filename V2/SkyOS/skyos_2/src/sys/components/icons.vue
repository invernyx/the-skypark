<template>
	<span class="icon">
		<span v-if="!hasDefaultSlot"></span>
		<slot v-else/>
	</span>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "icons",
	computed: {
		hasDefaultSlot() {
			return !!this.$slots.default;
		},
	}
});
</script>

<style lang="scss" scoped>

.icon {
	&--block {
		position: relative;
		display: block;
		width: 1em;
		height: 1em;
		& span {
			box-sizing: border-box;
			display: inline-block;
			width: 1em;
			height: 1em;
			position: absolute;
			bottom: 0;
			left: 0;
			background-repeat: no-repeat;
			background-position: center;
			background-size: 1em;
		}
	}

	&--inline {
		position: relative;
		display: inline-block;
		width: 1.3em;
		margin-right: 0.5em;
		margin-bottom: -0.3em;
		&:first-child {
			margin-left: 0;
		}
		&:last-child {
			margin-right: 0;
		}
		& span {
			box-sizing: border-box;
			display: inline-block;
			width: 1.3em;
			height: 1.3em;
			position: absolute;
			bottom: 0;
			left: 0;
			background-repeat: no-repeat;
			background-position: center;
			background-size: contain;
		}
		&--large {
			width: 2.4em;
			margin-left: 0.5em;
			margin-right: 0.5em;
			margin-bottom: -0.3em;
			& span {
				width: 2.4em;
				height: 2.4em;
				margin-bottom: -0.6em;
			}
		}
		&--medium {
			width: 1.5em;
			margin-left: 0.5em;
			margin-right: 0.5em;
			& span {
				width: 1.5em;
				height: 1.5em;
				margin-bottom: -0.0em;
			}
		}
	}

	&--dark {
		filter: brightness(0);
	}

	&-live {
		& span {
			&:after {
				position: absolute;
				top: 50%;
				left: 50%;
				margin-top: -0.4em;
				margin-left: -0.4em;
				height: 0.8em;
				width: 0.8em;
				content: "";
				border-radius: 1.5em;
				background-color: #F00;
			}
		}
	}

	//&-dead {
	//	& span {
	//		background-image: url(../assets/icons/dead.svg);
	//	}
	//}
//
	//&-north {
	//	& span {
	//		background-image: url(../assets/icons/north.svg);
	//	}
	//}
//
	//&-truck {
	//	& span {
	//		background-image: url(../assets/icons/truck.svg);
	//	}
	//}
//
	//&-exit {
	//	& span {
	//		background-image: url(../assets/icons/exit.svg);
	//	}
	//}
//
	//&-large {
	//	& span {
	//		background-image: url(../assets/icons/large.svg);
	//	}
	//}
//
	//&-camera {
	//	& span {
	//		background-image: url(../assets/icons/camera.svg);
	//	}
	//}
//
	//&-camera-plus {
	//	& span {
	//		background-image: url(../assets/icons/camera_plus.svg);
	//	}
	//}
//
	//&-center {
	//	& span {
	//		background-image: url(../assets/icons/recenter.svg);
	//	}
	//}
//
	//&-overview {
	//	& span {
	//		background-image: url(../assets/icons/overview.svg);
	//	}
	//}
//
	//&-skyblogs {
	//	& span {
	//		background-image: url(../assets/icons/skyblogs.svg);
	//	}
	//}
//
	//&-freeflight {
	//	& span {
	//		background-image: url(../assets/icons/freeflight.svg);
	//	}
	//}
//
	//&-support {
	//	& span {
	//		background-image: url(../assets/icons/support.svg);
	//	}
	//}
//
	//&-filter {
	//	& span {
	//		background-image: url(../assets/icons/filter.svg);
	//	}
	//}
//
	//&-exchange {
	//	& span {
	//		background-image: url(../assets/icons/exchange.svg);
	//	}
	//}
//
	//&-book {
	//	& span {
	//		background-image: url(../assets/icons/book.svg);
	//	}
	//}
//
	//&-invest {
	//	& span {
	//		background-image: url(../assets/icons/invest.svg);
	//	}
	//}
//
	//&-send {
	//	& span {
	//		background-image: url(../assets/icons/send.svg);
	//	}
	//}
//
	//&-jobs {
	//	& span {
	//		background-image: url(../assets/icons/jobs.svg);
	//	}
	//}
//
	//&-cargo {
	//	& span {
	//		background-image: url(../assets/icons/cargo.svg);
	//	}
	//}
//
	//&-intersection {
	//	& span {
	//		background-image: url('../assets/map/intersection.svg);
	//	}
	//}
//
	//&-vor {
	//	& span {
	//		background-image: url('../assets/map/vor.svg);
	//	}
	//}
//
	//&-airport {
	//	& span {
	//		background-image: url('../assets/map/airport.svg);
	//	}
	//}
//
	//&-angle {
	//	& span {
	//		background-image: url(../assets/icons/angle.svg);
	//	}
	//}
//
	//&-airplane-right {
	//	& span {
	//		background-image: url(../assets/icons/airplane_right.svg);
	//	}
	//}
//
	//&-back {
	//	& span {
	//		background-image: url(../assets/icons/back.svg);
	//	}
	//}
//
	//&-close {
	//	& span {
	//		background-image: url(../assets/icons/close.svg);
	//	}
	//}
//
	//&-add {
	//	& span {
	//		background-image: url(../assets/icons/add.svg);
	//	}
	//}
//
	&-runway {
		& span {
			background-image: url(../assets/icons/runway.svg);
		}
	}
//
	//&-chrono {
	//	& span {
	//		background-image: url(../assets/icons/chrono.svg);
	//	}
	//}
//
	&-cargo_pickup {
		& span {
			background-image: url(../assets/icons/cargo_pickup.svg);
		}
	}
//
	&-cargo_dropoff {
		& span {
			background-image: url(../assets/icons/cargo_dropoff.svg);
		}
	}

}

</style>