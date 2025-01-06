import _Vue from 'vue';
import ColorThief from 'colorthief';
import Eljs from '@/sys/libraries/elem';

class Service {

	public images = [] as any[];
	private vue: _Vue;

	constructor(Vue: _Vue) {
		this.vue = Vue;
	}

	public find(url :string, cb: Function) {
		let found = this.images.find(x => x.url == url);
		if(found){
			if(!found.done){
				found.callbacks.push(cb);
			} else {
				cb(found);
			}
		} else {
			found = {
				done: false,
				url: url,
				colorReal: null,
				color: null,
				colorDark: null,
				colorIsDark: false,
				callbacks: [cb]
			};
			this.images.push(found);

			const img = new Image();
			img.onload = () => {
				const test = ColorThief.prototype.getColor(img)
				if (Eljs.isDark(test[0], test[1], test[2])) {
					found.colorIsDark = true;
				} else {
					found.colorIsDark = false;
				}

				found.colorReal = "rgb(" + (test[0]) + "," + (test[1]) + "," + (test[2]) + ")";
				const colorHSL = Eljs.RGBToHSL(test[0], test[1], test[2]);
				if(!found.colorIsDark) {
					const newColor = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, colorHSL.l);
					const newColorDark = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, Eljs.limiter(0, 40, colorHSL.l));
					found.color = "rgb(" + (newColor.r) + "," + (newColor.g) + "," + (newColor.b) + ")"; // Bright card on Bright mode
					found.colorDark = "rgb(" + (newColorDark.r) + "," + (newColorDark.g) + "," + (newColorDark.b) + ")"; // Bright card on Dark mode
				} else {
					const newColor = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, Eljs.limiter(70, 100, colorHSL.l));
					const newColorDark = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, Eljs.limiter(0, 20, colorHSL.l));
					found.color = "rgb(" + (newColor.r) + "," + (newColor.g) + "," + (newColor.b) + ")"; // Dark card on Bright mode
					found.colorDark = "rgb(" + (newColorDark.r) + "," + (newColorDark.g) + "," + (newColorDark.b) + ")"; // Dark card on Dark mode
				}

				found.done = true;
				found.callbacks.forEach((e :Function) => e(found));
				found.callbacks = null;
			};
			img.onerror = () => {
				found.color = "#FFFFFF";
				found.colorDark = "#111111";
				found.colorIsDark = false;
				found.done = true;
				found.callbacks.forEach((e :Function) => e(found));
				found.callbacks = null;
			}
			img.crossOrigin = 'Anonymous';
			img.src = url;
		}

	}

}

export default {
	install: (Vue: typeof _Vue, options?: any) => {
		let installed = false;
		Vue.mixin({
			beforeCreate() {
				if (!installed) {
					installed = true;
					Vue.prototype.$colorSeek = new Service(this);
				}
			}
		});
	}
};

declare module 'vue/types/vue' {
	interface Vue {
		$colorSeek: Service;
	}
}
