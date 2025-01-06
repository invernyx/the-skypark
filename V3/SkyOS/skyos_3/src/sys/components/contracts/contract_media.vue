<template>
	<div class="media-embed">
		<div v-for="(url_obj, index) in list" v-bind:key="index">
			<div v-if="url_obj.type == 'general'">
				<button_action @click.native="openURL(url_obj.url)">{{ url_obj.param }}</button_action>
			</div>
			<div v-if="url_obj.type == 'youtube'" @click="openURL('http://parallel42.com/r.php?u=https://www.youtube.com/watch?v=' + url_obj.param)">
				<div class="youtube_frame shadowed">
					<div :style="'background-image: url(https://img.youtube.com/vi/' + url_obj.param + '/0.jpg);'"></div>
				</div>
			</div>
			<div v-else-if="url_obj.type == 'spotify'">
				<!--<div class="spotify_frame shadowed">
					<iframe ref="spotify_iframe" @load="spotify_frame_load" :src="'https://open.spotify.com/embed/playlist/' + url_obj.param" frameborder="0" allowtransparency="true" allow="encrypted-media"></iframe>
				</div>
				<p class="notice spaced-top"><u>Spotify embeds do not allow for volume control.</u> Make sure you lower your device's sound settings before playing.</p>
				-->
				<button_action @click.native="openURL(url_obj.url)" class="go">Open Spotify Playlist</button_action>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Contract from '@/sys/classes/contracts/contract';
import Vue from 'vue';

export default Vue.extend({
	props:{
		contract :Contract
	},
	data() {
		return {
			list: []
		}
	},
	mounted() {
		this.init();
	},
	methods: {
		init() {
			this.list = [];
			this.contract.media_link.forEach((url :string) => {
				const url_spl = url.replace("www.", "").replace("http://", "").replace("https://", "").split('/');
				switch(url_spl[0]) {
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
							param: url_spl[0],
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
		contract: {
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
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.media-embed {

	> div {
		margin-bottom: 8px;
		&:last-child {
			margin-bottom: 0
		}
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