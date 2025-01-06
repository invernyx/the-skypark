import _Vue from 'vue';

export default class AdventureSituation {
	UID = 0;
	Index = 0;
	Actions: number[] = [];
	Label = "";
	SituationType = "ICAO";
	DistToNextMin = 0;
	DistToNextMax = 300;
	TriggerRange = 1;
	Height = 0;
	Visible = true;
	Boundaries: any[] = [];

	Query = "";
	Country = "";
	ICAO = "";
	Surface = "Any";
	Lon = 0;
	Lat = 0;
	HeliMin = 0;
	HeliMax = 999;
	RwyMin = 1;
	RwyMax = 999;
	ParkMin = 0;
	ParkMax = 999;
	ParkWidMin = 0;
	ParkWidMax = 999;
	RwyLenMin = 200;
	RwyLenMax = 99999;
	RwyWidMin = 10;
	RwyWidMax = 999;
	ElevMin = -2000;
	ElevMax = 30000;
	RequireLights = false;

	constructor(structure?: any) {
		if (structure !== undefined) {
			Object.assign(this, structure);
		}
	}
}
