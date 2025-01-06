import AircraftCabinFeature from "./aircraft_cabin_feature";

export default class AircraftCabinState {

	public ui_mode = 'data';
	public ui_mode_previous = 'data';
	public level = 0;
	public row_labels = [] as string[][];
	public cols_labels = [] as string[][];
	public maximums = {
		x: 0,
		y: 0,
	};
	public draw_type = null as string;
	public drawing = false;
	public drawing_orientation = 'up' as string;
	public drawing_cells = null as number[][];
	public drawing_features = null as AircraftCabinFeature[];
	public cell_selected = null as number[];
	public human_selected = null;
	public cargo_selected = null;
	public service_from = null as string;
	public service_to = null as string[];
	public humans = [];
	public cargos = [];

	constructor(init?:Partial<AircraftCabinState>) {
        Object.assign(this, init);
	}

	public dispose() {

	}
}