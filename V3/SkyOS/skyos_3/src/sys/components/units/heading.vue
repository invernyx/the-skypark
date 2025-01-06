<template>
	<span>{{ output }}</span>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';

export default Vue.extend({
	props: ["amount", "decimals", "basis"],
	beforeMount() {
		this.init();
	},
	methods: {
		init() {

			let new_amount = this.amount;
			switch(this.basis) {
				case 180: {
					new_amount = this.normalizeAngle180(new_amount);
					break;
				}
				default: {
					new_amount = this.normalizeAngle360(new_amount);
					break;
				}
			}

			const locale = this.$os.userConfig.get(['ui','units','numbers']);
			const format = new Intl.NumberFormat(locale);
			this.output = format.format(Eljs.round(new_amount, this.decimals));
		},
		normalizeAngle180(angle: number) {
			let newAngle = angle;
			while (newAngle <= -180) { newAngle += 360 }
			while (newAngle > 180) { newAngle -= 360 }
			return newAngle;
		},
		normalizeAngle360(angle: number) {
			let newAngle = angle;
			while (newAngle <= 0) { newAngle += 360 }
			while (newAngle > 360) { newAngle -= 360 }
			return newAngle;
		}
	},
	data() {
		return {
			output: this.amount,
		}
	},
	watch: {
		amount: {
			immediate: true,
			handler(newValue, oldValue) {
				this.init();
			}
		}
	}
});
</script>