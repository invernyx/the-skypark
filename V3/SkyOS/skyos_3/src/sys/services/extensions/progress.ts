import _Vue from 'vue';

export default class Progress {
	public os: any;
	public vue: any;

	public reliability_unlock = 0;
	public reliability_contract_expire = 0;
	public reliability = {
		balance: null,
		date_start: null,
		trend: null,
	}
	public bank = {
		balance: null,
		date_start: null,
		trend: null,
	}
	public karma = {
		balance: null,
		date_start: null,
		trend: null,
	}
	public xp = {
		balance: null,
		date_start: null,
		trend: null,
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

	public startup() {
		this.os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	}

	public listener_ws(wsmsg: any, self: Progress){
		switch(wsmsg.name[0]){
			case 'progress': {
				switch(wsmsg.name[1]){
					case 'get': {

						self.bank = wsmsg.payload.bank;
						self.karma = wsmsg.payload.karma;
						self.xp = wsmsg.payload.xp;
						self.reliability = wsmsg.payload.reliability;
						self.reliability_unlock = new Date(wsmsg.payload.reliability_unlock).getTime();

						break;
					}
				}
				break;
			}
		}
	}

}