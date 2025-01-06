import _Vue from 'vue';
import Eljs from '@/sys/libraries/elem'

export default class Simulator {
	public os: any;
	public vue: any;

	public connected = false;
	public live = false;
	public name = '';
	public version = '';
	public aircraft = {
		name: '',
		manufacturer: '',
		model: '',
	};
	public locationHistory = {
		name: '',
		costPerNM: 0,
		location: [0,0],
	};
	public location = {
		Lon: 0,
		Lat: 0,
		Alt: 0,
		GAlt: 0,
		MagVar: 0,
		GS: 0,
		TR: 0,
		Hdg: 0,
		CRS: 0,
		FPM: 0,
		Terrain: null,
		NearestAirport: null as string,
		SimTimeZulu: null as Date,
		SimTimeOffset: 0,
		PayloadTotal: 0,
	};
	public autopilot = {
		On: false,
		HdgOn: false,
		Hdg: false,
	}

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;

	}

	public startup() {
		this.os.eventsBus.Bus.on('ws-in', (e) => this.listener_ws(e, this));
	}

	public listener_ws(wsmsg: any, self: Simulator){

		switch(wsmsg.name[0]){
			case 'disconnect': {
				self.connected = false;
				self.live = false;
				self.name = '';
				self.version = '';

				this.os.eventsBus.Bus.emit('sim', { name: 'live', payload: self.live });
				break;
			}
			case 'sim': {
				switch(wsmsg.name[1]){
					case 'status': {
						self.connected = wsmsg.payload.State;
						if(wsmsg.payload.State){
							self.live = true;
							self.name = wsmsg.payload.Name;
							self.version = wsmsg.payload.Version;
						} else {
							self.live = false;
							self.name = '';
							self.version = '';
						}
						this.os.eventsBus.Bus.emit('sim', { name: 'live', payload: self.live });
						break;
					}
				}
				break;
			}
			case 'locationhistory': {
				switch(wsmsg.name[1]){
					case 'latest': {
						self.locationHistory.name = wsmsg.payload.Name;
						self.locationHistory.costPerNM = wsmsg.payload.CostPerNM;
						self.locationHistory.location[0] = wsmsg.payload.Location[0];
						self.locationHistory.location[1] = wsmsg.payload.Location[1];
						break;
					}
				}
				break;
			}
			case 'eventbus': {
				switch(wsmsg.name[1]){
					case 'meta': {
						const pl = wsmsg.payload;

						const svc_loc = self.location;
						svc_loc.Lon = pl.Lon;
						svc_loc.Lat = pl.Lat;
						svc_loc.Hdg = pl.HDG;
						svc_loc.Alt = pl.Alt;
						svc_loc.GAlt = pl.GAlt;
						svc_loc.MagVar = pl.MagVar;
						svc_loc.TR = pl.TurnRate;
						svc_loc.GS = pl.GS;
						svc_loc.CRS = pl.CRS;
						svc_loc.FPM = pl.FPM;
						svc_loc.Terrain = pl.Terrain;
						svc_loc.PayloadTotal = pl.PayloadTotal;
						svc_loc.SimTimeZulu = new Date(pl.SimTimeZulu);
						svc_loc.SimTimeOffset = pl.SimTimeOffset;

						const svc_acf = self.aircraft;
						svc_acf.manufacturer = pl.Aircraft.Manufacturer;
						svc_acf.model = pl.Aircraft.Model;
						svc_acf.name = pl.Aircraft.Name;

						self.os.time.setSunPosition(svc_loc.SimTimeZulu, pl.Lon, pl.Lat);

						const islive = (Eljs.round(svc_loc.Lat, 3) != 0 && Eljs.round(svc_loc.Lon, 3) != 0) || Math.round(svc_loc.GS) > 70;
						if(islive != self.live) {
							self.live = islive;
							this.os.eventsBus.Bus.emit('sim', { name: 'live', payload: islive });
						}

						this.os.eventsBus.Bus.emit('sim', { name: 'meta', payload: svc_loc });

						break;
					}
					case 'event': {
						wsmsg.payload.forEach((pl: any) => {
							switch(pl.Type) {
								case 'Position': {

									const svc_loc = self.location;
									svc_loc.NearestAirport = pl.Params.NearestAirport;


									break;
								}
								case 'Autopilot': {
									self.autopilot.On = pl.Params.On;
									self.autopilot.HdgOn = pl.Params.HdgOn;
									self.autopilot.Hdg = pl.Params.Hdg;
									break;
								}
							}
						});
						break;
					}
				}
				break;
			}
		}
	}

}