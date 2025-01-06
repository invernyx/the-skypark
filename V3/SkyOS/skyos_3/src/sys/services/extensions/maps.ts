import _Vue from 'vue';
import MapboxExt from '@/sys/libraries/mapboxExt'

export default class Maps {
	private os: any;
	private vue: _Vue;

	public untracked = {};

	public main = null;
	public token = "pk.eyJ1IjoiYmlhcnplZCIsImEiOiJja3N1d3ltaWgxNDU1MnFwanZhNzB5dDVuIn0.jV7FuhxjP7J3ltZTLVXDAA";
	public bounds = { nw: [0,0], se: [0,0] };
	public padding = {
		top: 0,
		right: 0,
		bottom: 0,
		left: 0
	}
	public display_layer = {
		sat: false,
		hill: true,
		fleet: {
			enabled: true,
		},
		wx: {
			radar: {
				enabled: true,
			}
		},
		sectional: {
			us: {
				enabled: false,
			}
		},
		airports: {
			icaos: {
				enabled: true,
			},
			layout: {
				enabled: true,
			}
		}
	}
	public cities = [];

	public set_layer(path :string[], value :any) {
		const deepen = (obj :any,  depth :number) :boolean => {
			const keys = Object.keys(obj);
			if(keys.includes(path[depth])) {
				if(depth == path.length - 1) {
					if(obj[path[depth]] != value) {
						obj[path[depth]] = value;
						this.os.eventsBus.Bus.emit("statechange", {
							name: path,
							payload: value
						});
					}
					return true;
				} else {
					return deepen(obj[path[depth]], depth + 1);
				}
			} else {
				return false;
			}
		}
		deepen(this.display_layer, 0);
		this.os.eventsBus.Bus.emit('map', { name: 'layer', payload: { path: path, value: value } });
	}

	public ensurePadding(map :any, new_padding :any) {
		MapboxExt.ensurePadding(map, new_padding);
	}


	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}