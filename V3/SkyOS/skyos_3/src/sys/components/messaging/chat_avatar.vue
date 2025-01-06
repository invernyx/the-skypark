<template>
	<div class="chat_avatar" :class="{ 'is-dark': backgroundcolor_is_dark }" :style="{ 'backgroundImage': (imageURL ? 'url(' + imageURL + ')' : 'none'), 'backgroundColor': (backgroundColor ? 'rgb(' + backgroundColor[0] + ',' + backgroundColor[1] + ',' + backgroundColor[2] + ')' : null) }">
		<div v-if="!imageURL" class="initials">{{ memberInitials }}</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import Handle from '@/sys/classes/messaging/handle';

export default Vue.extend({
	name: "chat",
	props: {
		handle :Object
	},
	methods: {
		init() {
			this.imageURL = null;
			this.backgroundColor = Eljs.GetColorFromRandomString(this.handle.ident);
			this.backgroundcolor_is_dark = Eljs.isDark(this.backgroundColor[0], this.backgroundColor[1], this.backgroundColor[2]);

			if(this.handle.last_name) {
				this.memberInitials = [this.handle.first_name, this.handle.last_name].map(x => x[0]).join('');
			} else {
				this.memberInitials = [this.handle.first_name].map(x => x[0]).join('');
			}

			this.getAvatarStyle(this.handle.ident);
		},
		getAvatarStyle(name :string) {
			const url = this.$os.api.getCDN('avatars', name + ".jpg");
			if(!Eljs.is_cached(url)) {
				Eljs.UrlExists(url, (state) => {
					this.imageURL = state ? url : null;
				});
			} else {
				this.imageURL = url;
			}
		}
	},
	components: {
	},
	mounted() {
		this.init();
	},
	beforeDestroy() {
	},
	data() {
		return {
			backgroundColor: null,
			imageURL: null,
			memberInitials: null,
			backgroundcolor_is_dark: false,
		}
	},
	watch: {
		handle: {
			immediate: true,
			handler(newValue, oldValue) {
				this.init();
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.chat_avatar {
	display: flex;
	justify-content: center;
	align-items: center;
	border-radius: 50%;
	background-size: cover;
	background-position: center;
	color: $ui_colors_bright_shade_5;
	&.is-dark {
		color: $ui_colors_bright_shade_0;
	}
	.initials {
		font-family: "SkyOS-SemiBold";
	}
}
</style>