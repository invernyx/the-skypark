<template>
	<div class="media-embed">
		<div v-for="(urlObj, index) in list" v-bind:key="index">
			<div v-if="urlObj.type == 'general'">
				<button_action @click.native="openURL(urlObj.url)">{{ urlObj.param }}</button_action>
			</div>
			<div v-if="urlObj.type == 'youtube'" @click="openURL('https://www.youtube.com/watch?v=' + urlObj.param)">
				<div class="youtube_frame shadowed">
					<div :style="'background-image: url(https://img.youtube.com/vi/' + urlObj.param + '/0.jpg);'"></div>
				</div>
			</div>
			<div v-else-if="urlObj.type == 'spotify'">
				<div class="spotify_frame shadowed">
					<iframe ref="spotify_iframe" @load="spotify_frame_load" :src="'https://open.spotify.com/embed/playlist/' + urlObj.param" frameborder="0" allowtransparency="true" allow="encrypted-media"></iframe>
				</div>
				<p class="notice spaced-top"><u>Spotify embeds do not allow for volume control.</u> Make sure you lower your device's sound settings before playing.</p>
				<button_action @click.native="openURL(urlObj.url)" class="go">Open in Spotify</button_action>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';

export default Vue.extend({
	name: "media_embed",
	props: ['contract', 'template'],
	data() {
		return {
			list: []
		}
	},
	mounted() {
	},
	methods: {
		init() {
			this.list = [];
			this.contract.MediaLink.forEach((url :string) => {
				const urlSpl = url.replace("www.", "").replace("http://", "").replace("https://", "").split('/');
				switch(urlSpl[0]) {
					case "youtube.com":
					case "youtu.be": {
						//http://www.youtube.com/watch?v=0zM3nApSvMg&feature=feedrec_grec_index
						//http://www.youtube.com/user/IngridMichaelsonVEVO#p/a/u/1/QdK8U-VIH_o
						//http://www.youtube.com/v/0zM3nApSvMg?fs=1&amp;hl=en_US&amp;rel=0
						//http://www.youtube.com/watch?v=0zM3nApSvMg#t=0m10s
						//http://www.youtube.com/embed/0zM3nApSvMg?rel=0
						//http://www.youtube.com/watch?v=0zM3nApSvMg
						//http://youtu.be/0zM3nApSvMg
						var regExp = /^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#&?]*).*/;
						var match = url.match(regExp);
						var result = (match&&match[7].length==11)? match[7] : false;
						this.list.push({
							type: 'youtube',
							url: url,
							param: result,
						});
						break;
					}
					case "open.spotify.com": {
						//https://developer.spotify.com/documentation/widgets/generate/embed/
						//spotify:playlist:47WFcAFtwHpjgQlomBOvFK
						//https://open.spotify.com/playlist/47WFcAFtwHpjgQlomBOvFK
						// <iframe src="https://open.spotify.com/embed/album/1DFixLWuPkv3KT3TnV35m3" width="300" height="380" frameborder="0" allowtransparency="true" allow="encrypted-media"></iframe>

						let result1 = url.split('?')[0].split('/');
						let result2 = result1[result1.length-1];
						this.list.push({
							type: 'spotify',
							url: url,
							param: result2,
						});
						break;
					}
					default: {
						const structure = {
							type: 'general',
							url: url,
							param: urlSpl[0],
						};

						this.list.push(structure);

						break;
					}
				}

			});
		},
		openURL(url :string) {
			window.open(url, '_blank');
		},
		spotify_frame_load() {
		}
	},
	watch: {
		template: {
			immediate: true,
			handler(newValue, oldValue) {
				if(newValue){
					this.init();
				}
			}
		}
	}
});
</script>

<style lang="scss" scoped>
@import '../../scss/sizes.scss';
@import '../../scss/colors.scss';
@import '../../scss/mixins.scss';
.media-embed {
	> div {
		margin-bottom: 8px;
		.shadowed {
			@include shadowed_shallow(#000);
		}
	}

	p.spaced-top {
		margin-top: 0.5em;
	}

	.youtube_frame {
		position: relative;
		overflow: hidden;
		border-radius: 8px;
		height: 160px;
		background: #000;
		cursor: pointer;
		&:hover {
			div {
				opacity: 0.7;
				transform: scale(1.02,1.02);
				transition: transform 0.2s ease-out, opacity 0.2s ease-out;
			}
			&::after {
				transition: transform 0.2s ease-out;
				transform: scale(1.1,1.1);
			}
		}
		&:active:hover {
			&::after {
				transform: scale(1.05,1.05);
			}
		}
		div {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			opacity: 0.5;
			background-position: center;
			background-repeat: no-repeat;
			background-size: cover;
			background-image: url(../../../sys/assets/icons/youtube_play.svg);
			transition: transform 0.5s ease-out, opacity 0.5s ease-out;
		}
		&::after {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			content: '';
			background-position: center;
			background-repeat: no-repeat;
			background-size: 90px;
			background-image: url(../../../sys/assets/icons/youtube_play.svg);
			transition: transform 0.5s ease-out;
		}
	}


	.spotify_frame {
		position: relative;
		overflow: hidden;
		border-radius: 8px;
		background: #000;
		iframe {
			display: block;
			width: 100%;
			height: 80px;
		}
	}
}
</style>