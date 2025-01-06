import Eljs from '@/sys/libraries/elem';
import _Vue from 'vue';

export default class Companies {
	private os: any;
	private vue: _Vue;

	public library = [
		{
			code: 'clearsky',
			name: 'ClearSky Logistics',
			short: 'ClearSky',
			abr: 'CS',
		},
		{
			code: 'coyote',
			name: 'Transportes Coyote',
			short: 'Coyote',
			abr: 'TC',
		},
		{
			code: 'skyparktravel',
			name: 'Skypark Travels',
			short: 'Travels',
			abr: 'ST',
		},
		{
			code: 'oceanicair',
			name: 'Oceanic Air',
			short: 'Oceanic',
			abr: 'OA',
		},
		{
			code: 'lastflight',
			name: 'Last Flight',
			short: 'Last Flight',
			abr: 'LF',
		}
	];

	public GetFromCode(code :string) {
		return this.library.find(x => x.code == code);
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}