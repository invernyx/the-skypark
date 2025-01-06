import Eljs from '@/sys/libraries/elem';
import _Vue from 'vue';

export default class AircraftTypes {
	private os: any;
	private vue: _Vue;

	public library = [
		{
			name: 'Helis',
			code: 'heli',
		},
		{
			name: 'Piston',
			code: 'ga',
		},
		{
			name: 'Turbo',
			code: 'turbo',
		},
		{
			name: 'Biz Jets',
			code: 'jet',
		},
		{
			name: 'Narrow',
			code: 'narrow',
		},
		{
			name: 'Wide',
			code: 'wide',
		},
	];

	public GetFromIndex(index :number) {
		return this.library[index];
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}