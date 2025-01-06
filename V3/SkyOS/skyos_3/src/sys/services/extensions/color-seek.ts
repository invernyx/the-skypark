import _Vue from 'vue';
import ColorThief from 'colorthief';
import Eljs from '@/sys/libraries/elem';

export interface ColorsCallback {
	done: boolean,
	id: number,
	color: {
		r: number,
		g: number,
		b: number,
		h: string
	},
	color_bright: {
		r: number,
		g: number,
		b: number,
		h: string
	},
	color_dark: {
		r: number,
		g: number,
		b: number,
		h: string
	},
	color_is_dark: boolean,
	callbacks: any[]
}


export default class ColorSeek {
	public os: any;
	public vue: any;

	public images = [] as ColorsCallback[];

	public crops = [] as {
		done: boolean,
		id: number,
		code: string,
		data: string,
		callbacks: any[]
	}[];

	public find(url :string, darkTip :number, cb: Function) {
		const id = this.hashCode(url);

		while(this.images.length > 30) {
			this.images.shift();
		}

		let found = this.images.find(x => x.id == id);

		if(found){
			if(!found.done){
				found.callbacks.push(cb);
			} else {
				cb(found);
			}
		} else {
			found = {
				done: false,
				id: id,
				color: {
					r: 0,
					g: 0,
					b: 0,
					h: "rgb(0,0,0)"
				},
				color_bright: {
					r: 0,
					g: 0,
					b: 0,
					h: "rgb(0,0,0)"
				},
				color_dark: {
					r: 0,
					g: 0,
					b: 0,
					h: "rgb(0,0,0)"
				},
				color_is_dark: false,
				callbacks: [cb]
			};
			this.images.push(found);

			const img = new Image();
			img.onload = () => {

				const test = ColorThief.prototype.getColor(img);

				if (Eljs.isDark(test[0], test[1], test[2], darkTip)) {
					found.color_is_dark = true;
				} else {
					found.color_is_dark = false;
				}

				const colorHSL = Eljs.RGBToHSL(test[0], test[1], test[2]);
				let newcolor_bright = null;
				let newcolor_dark = null;
				if(!found.color_is_dark) {
					newcolor_bright = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, colorHSL.l);
					newcolor_dark = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, Eljs.limiter(0, 40, colorHSL.l));
				} else {
					newcolor_bright = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, Eljs.limiter(70, 100, colorHSL.l));
					newcolor_dark = Eljs.HSLToRGB(colorHSL.h, colorHSL.s / 2, Eljs.limiter(0, 20, colorHSL.l));
				}

				found.color = {
					r: test[0],
					g: test[1],
					b: test[2],
					h: "rgb(" + (test[0]) + "," + (test[1]) + "," + (test[1]) + ")"
				};

				found.color_bright = {
					r: newcolor_bright.r,
					g: newcolor_bright.g,
					b: newcolor_bright.b,
					h: "rgb(" + (newcolor_bright.r) + "," + (newcolor_bright.g) + "," + (newcolor_bright.b) + ")"
				};

				found.color_dark = {
					r: newcolor_dark.r,
					g: newcolor_dark.g,
					b: newcolor_dark.b,
					h: "rgb(" + (newcolor_dark.r) + "," + (newcolor_dark.g) + "," + (newcolor_dark.b) + ")"
				}

				found.done = true;
				found.callbacks.forEach((e :Function) => e(found));
				found.callbacks = null;

			};
			img.onerror = () => {
				found.color = {
					r: 127,
					g: 127,
					b: 127,
					h: 'rgb(127,127,127)'
				};
				found.color_bright = {
					r: 255,
					g: 255,
					b: 255,
					h: 'rgb(255,255,255)'
				};
				found.color_dark = {
					r: 0,
					g: 0,
					b: 0,
					h: 'rgb(0,0,0)'
				};
				found.color_is_dark = false;
				found.done = true;
				found.callbacks.forEach((e :Function) => e(found));
				found.callbacks = null;
			}
			img.crossOrigin = 'Anonymous';
			img.src = url;
		}

	}

	public crop(url :string, bounds :{
		left: number,
		top: number,
		right: number,
		bottom: number,
		width?: number,
	}, cb: Function) {
		const id = this.hashCode(url);
		const code = [
			bounds.left.toString(),
			bounds.top.toString(),
			bounds.right.toString(),
			bounds.bottom.toString(),
			bounds.width ? bounds.width.toString() : null
		].join(':');
		let found = this.crops.find(x => x.id == id && x.code == code);

		if(found){
			if(!found.done){
				found.callbacks.push(cb);
			} else {
				cb(found);
			}
		} else {
			found = {
				done: false,
				id: id,
				code: code,
				data: url,
				callbacks: [cb]
			};
			this.crops.push(found);

			// this image will hold our source image data
			const inputImage = new Image();

			inputImage.onerror = () => {

				found.data = null;
				found.done = true;
				found.callbacks.forEach((e :Function) => e(found));
				found.callbacks = null;
			};

			// we want to wait for our image to load
			inputImage.onload = () => {

				// Input values from image
				const inputWidth = inputImage.naturalWidth;
				const inputHeight = inputImage.naturalHeight;

				// Create our outputs
				let outputWidth = inputWidth;
				let outputHeight = inputHeight;
				const outputOffsetX = outputWidth * bounds.left;
				const outputOffsetY = outputHeight * bounds.top;

				// Crop
				outputWidth = outputWidth - (outputWidth * (1 - bounds.right)) - outputOffsetX;
				outputHeight = outputHeight - (outputHeight * (1 - bounds.bottom)) - outputOffsetY;

				let scale = 1;
				if(bounds.width) {
					const outputRatio = outputHeight / outputWidth;
					scale = bounds.width / outputWidth;
					outputWidth = bounds.width;
					outputHeight = bounds.width * outputRatio;
				}

				// create a canvas that will present the output image
				const outputImage = document.createElement("canvas");

				// set it to the same size as the image
				outputImage.width = outputWidth;
				outputImage.height = outputHeight;

				// draw our image
				const scaledX = outputOffsetX * scale;
				const scaledY = outputOffsetY * scale;
				const scaledW = inputWidth * scale;
				const scaledH = inputHeight * scale;
				outputImage.getContext("2d").drawImage(inputImage, -scaledX, -scaledY, scaledW, scaledH);

				//document.getElementById('estfgiuhkfgjfg').append(outputImage);

				found.data = outputImage.toDataURL();
				found.done = true;
				found.callbacks.forEach((e :Function) => e(found));
				found.callbacks = null;

			}

			// start loading our image
			inputImage.src = url;

		}
	}

	private hashCode = (str, seed = 0) => {
		let h1 = 0xdeadbeef ^ seed, h2 = 0x41c6ce57 ^ seed;
		for (let i = 0, ch; i < str.length; i++) {
			ch = str.charCodeAt(i);
			h1 = Math.imul(h1 ^ ch, 2654435761);
			h2 = Math.imul(h2 ^ ch, 1597334677);
		}
		h1 = Math.imul(h1 ^ (h1>>>16), 2246822507) ^ Math.imul(h2 ^ (h2>>>13), 3266489909);
		h2 = Math.imul(h2 ^ (h2>>>16), 2246822507) ^ Math.imul(h1 ^ (h1>>>13), 3266489909);
		return 4294967296 * (2097151 & h2) + (h1>>>0);
	};

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}