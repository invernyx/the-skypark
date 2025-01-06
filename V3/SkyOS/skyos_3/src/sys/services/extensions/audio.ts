import _Vue from 'vue';
import { OS } from '@/sys/services/os'
import Chat from '@/sys/classes/messaging/chat';
import Message from '@/sys/classes/messaging/message';

export default class AudioPlayer {
	private $os: OS;

	private currently_playing = {
		audio: null as HTMLAudioElement,
		type: null as String,
		chat: null as Chat,
		message: null as Message
	}

	public reset() {
		this.currently_playing = {
			audio: null,
			type: null,
			chat: null,
			message: null
		}
	}

	public ingest(url :string, type :string, chat :Chat, message :Message, play :Boolean) {
		this.stop();
		this.currently_playing = {
			audio: new Audio(url),
			type: type,
			chat: chat,
			message: message
		}

		this.currently_playing.audio.addEventListener('ended', () => {
			this.$os.eventsBus.Bus.emit('audio', {
				name: 'ended',
				payload: this.currently_playing
			});
		});

		switch(type) {
			case "Audio": {
				this.play();
				break;
			}
			case "Call": {
				if(play) {
					this.play("Audio");
				} else {
					this.$os.eventsBus.Bus.emit('audio', {
						name: 'ring',
						payload: this.currently_playing
					});
				}
				break;
			}
		}
	}

	public play(type :string = null) {
		this.$os.eventsBus.Bus.emit('audio', {
			name: 'playing',
			payload: {
				audio: this.currently_playing.audio,
				type: !type ? this.currently_playing.type : type,
				chat: this.currently_playing.chat,
				message: this.currently_playing.message
			}
		});
		this.currently_playing.audio.play();
	}

	public stop() {
		if(this.currently_playing.audio) {
			this.currently_playing.audio.pause();
			this.$os.eventsBus.Bus.emit('audio', {
				name: 'stopped',
				payload: this.currently_playing
			});
			this.reset();
		}
	}

	public pause() {
		if(this.currently_playing.audio) {
			this.currently_playing.audio.pause();
			this.$os.eventsBus.Bus.emit('audio', {
				name: 'paused',
				payload: this.currently_playing
			});
		}
	}

	public get_playing() {
		return this.currently_playing;
	}

	constructor(os :any, vue :_Vue) {
		this.$os = os;
	}

	public startup() {
		this.$os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	}

	public listener_ws(wsmsg: any, self: AudioPlayer){
		switch(wsmsg.name[0]){
			case 'messaging': {
				switch(wsmsg.name[1]){
					case 'add': {
						switch(wsmsg.payload.message.content_type){
							case "Audio": {
								const chat = new Chat(wsmsg.payload.chat);
								const message = new Message(wsmsg.payload.message);
								const path_spl = message.audio_path.split(':');
								const type = path_spl[0];
								const path = path_spl[1];
								const url = this.$os.api.getCDN('sounds', type + '/' + path + ".mp3");
								self.ingest(url, message.content_type, chat, message, false);
								break;
							}
						}
						break;
					}
				}
				break;
			}
		}
	}
}