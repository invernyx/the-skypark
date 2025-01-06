import _Vue from 'vue';
import { Theme, ThemeAccent, AppInfo, NavType, StatusType } from '@/sys/foundation/app_model';
import Eljs from '@/sys/libraries/elem';

export default class Themes {
	public os: any;
	public vue: _Vue;

	public themeOverrides: Theme[] = [];
	public themeAccents: ThemeAccent[] = [];

	public colors = {
		// Bright Mode
		$ui_colors_bright_button_info: '#4E89F1',
		$ui_colors_bright_button_go: '#00cc00',
		$ui_colors_bright_button_warn: '#cccc00',
		$ui_colors_bright_button_cancel: '#cc0000',
		$ui_colors_bright_button_gold: '#cca34d',

		$ui_colors_bright_magenta: '#9e009e',

		$ui_colors_bright_shade_0: '#FFFFFF',
		$ui_colors_bright_shade_1: '#FAF9F3',
		$ui_colors_bright_shade_2: '#e2e0d9',
		$ui_colors_bright_shade_3: '#AFACA5',
		$ui_colors_bright_shade_4: '#4F4C4B',
		$ui_colors_bright_shade_5: '#070707',

		// Dark Mode
		$ui_colors_dark_button_info: '#1B56BE',
		$ui_colors_dark_button_go: '#00B300',
		$ui_colors_dark_button_warn: '#B3B300',
		$ui_colors_dark_button_cancel: '#B30000',
		$ui_colors_dark_button_gold: '#B38A34',

		$ui_colors_dark_magenta: '#f500f5',

		$ui_colors_dark_shade_0: '#070707',
		$ui_colors_dark_shade_1: '#1A1A1B',
		$ui_colors_dark_shade_2: '#232324',
		$ui_colors_dark_shade_3: '#A6A8AB',
		$ui_colors_dark_shade_4: '#D0D2D3',
		$ui_colors_dark_shade_5: '#F1F1F2',

		// Oddly Specific System Colors
		$ui_colors_background: '#030303'

	}

	public setMode(mode :string) {
		this.os.userConfig.set(['ui','themeAuto'], false);
		this.os.userConfig.set(['ui', 'theme'], mode);
		this.os.eventsBus.Bus.emit('os', { name: 'themechange', payload: this.getTheme() });
	}

	public toggleMode(){
		if(this.os.userConfig.get(['ui', 'theme']) == 'theme--dark') {
			this.setMode('theme--bright');
		} else {
			this.setMode('theme--dark');
		}
	}

	public setThemeAccent(accent :ThemeAccent, name? :string) {
		const index = this.themeAccents.findIndex(x => x.name == name ? name : accent.name);
		if(index >= 0)
			this.themeAccents.splice(index, 1);

		if(accent)
			this.themeAccents.unshift(accent);

		this.os.eventsBus.Bus.emit('os', { name: 'themechange', payload: this.getTheme() });
	}

	public setThemeLayer(layer :Theme, name? :string) {
		const index = this.themeOverrides.findIndex(x => x.name == name ? name : layer.name);
		if(index >= 0)
			this.themeOverrides.splice(index, 1);

		if(layer)
			this.themeOverrides.unshift(layer);

		this.os.eventsBus.Bus.emit('os', { name: 'themechange', payload: this.getTheme() });
	}

	public getTheme() {
		let nav = NavType.NONE;
		let status = StatusType.NONE;
		let shaded = false;
		let accent = null;
		let image = null;

		if(this.os.routing.activeApp) {

			const layers = this.themeOverrides;
			const accents = this.themeAccents;

			// Set the right modes
			switch(this.os.userConfig.get(['ui', 'theme'])) {
				case 'theme--bright': {
					 if(layers.length) {
						nav = layers[0].bright.nav;
						status = layers[0].bright.status;
						shaded = layers[0].bright.shaded;
					} else {
						nav = NavType.DARK;
						status = StatusType.DARK;
						shaded = true;
					}
					if(accents.length) {
						accent = accents[0].bright;
						image = accents[0].image;
					}
					break;
				}
				case 'theme--dark': {
					 if(layers.length) {
						nav = layers[0].dark.nav;
						status = layers[0].dark.status;
						shaded = layers[0].dark.shaded;
					} else {
						nav = NavType.BRIGHT;
						status = StatusType.BRIGHT;
						shaded = true;
					}
					if(accents.length) {
						accent = accents[0].dark;
						image = accents[0].image;
					}
					break;
				}
			}
		}

		return {
			nav: nav,
			status: status,
			shaded: shaded,
			accent: accent,
			image: image
		}
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}