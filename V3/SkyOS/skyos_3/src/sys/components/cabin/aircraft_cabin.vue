<template>
	<div class="cabin columns"
		v-if="state"
		:class="[
			state.ui_mode,
			{
				'compact': cabin.levels[state.level][0] > 20,
				'large': cabin.levels[state.level][1] < 4,
				'drawing': state.draw_type
			}
		]"
		:key="cabin.livery">
		<div class="column column_align_center column_justify_center cabin_container">
			<div class="cabin_wrap">
					<div class="cabin_nose"></div>
				<div class="cabin_layout" @mouseleave="layoute_mouse_leave">

					<div class="cabin_layout_humans">
						<div
							v-for="(human) in humans_in_layer"
							:key="human.guid"
							class="cabin_layout_humans_human human-icon"
							@click="state.human_selected = human; set_mode('peep')"
							:class="[
								'human-icon-' + human.state.icon,
								'human-icon-type-' + human.type,
								'human-icon-action-' + human.state.action,
								'human-icon-sub-' + human.state.sub_action,
								{
									'selected': state.human_selected ? state.human_selected.guid == human.guid : false,
									'service': human.state.service_from,
									'boarded': human.state.boarded,
									'highlight': check_highlight(human),
									'side_left': human.state.x == (cabin.levels[state.level][2]),
									'side_right': human.state.x == (cabin.levels[state.level][0] + cabin.levels[state.level][2] - 1),
									'thoughts': human.state.thoughts.filter(x1 => x1[0] > (last_human_update - 30000)).length
								}
							]"
							:style="{ 'transform': 'translate(' + Math.round(((human.state.x * size_ref.x) + (size_ref.x * (human.state.offet_x / 100)))) + 'px, ' + Math.round(((human.state.y * size_ref.y) + (size_ref.y * (human.state.offet_y / 100))) + ((size_ref.y - size_ref.x) / 2)) + 'px)' }"
							>
						</div>
					</div>

					<div class="cabin_layout_cargos">
						<div
							v-for="(cargo) in cargos_in_layer"
							:key="cargo.guid"
							class="cabin_layout_cargos_cargo cargo-icon"
							@click="state.cargo_selected = cargo; set_mode('cargo')"
							:class="[
								//'cargo-icon-' + cargo.state.icon,
								'cargo-icon-generic',
								//'cargo-icon-type-' + cargo.type,
								{
									'selected': state.cargo_selected == cargo,
									'boarded': cargo.state.boarded,
								}
							]"
							:style="{ 'transform': 'translate(' + Math.round((cargo.state.x * size_ref.x)) + 'px, ' + Math.round((cargo.state.y * size_ref.y) + ((size_ref.y - size_ref.x) / 2)) + 'px)' }"
							>
						</div>
					</div>

					<div class="cabin_layout_row cabin_layout_col_labels">
						<div v-for="x in cabin.levels[state.level][0]" :key="x" class="cabin_layout_col_label">{{ state.cols_labels[state.level][x-1] }}</div>
					</div>

					<div v-for="y in state.maximums.y" :key="y" class="cabin_layout_row">
						<div class="cabin_layout_row_label">{{ state.row_labels[state.level][y-1] }}</div>
						<div class="cabin_layout_row_content">
							<div
								v-for="x in state.maximums.x"
								:key="x"
								class="cabin_layout_cell"
								:class="{
									'window_left': x == (1 + cabin.levels[state.level][2]),
									'window_right': x == (cabin.levels[state.level][0] + cabin.levels[state.level][2]),
									'selected': state.cell_selected ? (state.cell_selected[0] == x-1 && state.cell_selected[1] == y-1) : false,
									'void_right': (x > cabin.levels[state.level][0] + cabin.levels[state.level][2]),
									'void_left': (x <= cabin.levels[state.level][2]),
									'void': (y > cabin.levels[state.level][1] + cabin.levels[state.level][3]) || (y <= cabin.levels[state.level][3]),
									'connects_stairs_below': state.level > 0 ? (stairs_in_layer_below.filter(f => f.x == x-1 && f.y == y-1).length == 1) : false,
									'connects_stairs_above': state.level < cabin.levels.length ? (stairs_in_layer_above.filter(f => f.x == x-1 && f.y == y-1).length == 1) : false,
								}"
								:ref="(x-1) + '_' + (y-1)"
								@click="cell_select(x-1,y-1)"
								@mousedown="cell_mouse_down($event, x-1, y-1)"
								@mouseenter="cell_mouse_enter($event, x-1, y-1)"
								@mouseleave="cell_mouse_leave($event, x-1, y-1)"
								@mouseup="cell_mouse_up($event, x-1, y-1)"
							>
								<div v-for="(feature) in features_in_layer.filter(f => f.x == x-1 && f.y == y-1)"
								v-bind:key="feature.x + ',' + feature.y + ',' + feature.z + ',' + feature.type + ',' + feature.sub_type + ',' + feature.orientation"
								class="feature"
								:class="[
									'feature-' + feature.type.toLowerCase(),
									'feature-' + feature.type.toLowerCase() + '-' + feature.sub_type.toLowerCase(),
									'orient-' + feature.orientation
								]"
								></div>

							</div>
						</div>
					</div>

				</div>
				<div class="cabin_tail"></div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from 'vue';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';
import AircraftCabinFeature from '@/sys/classes/cabin/aircraft_cabin_feature';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';

export default Vue.extend({
	props:{
		aircraft :Aircraft,
		cabin :Object as () => AircraftCabin,
		state: Object as () => AircraftCabinState,
		level: Number
	},
	data() {
		return {
			last_human_update: 0,
			cell_hovered: null,
			size_ref: {
				x: 0,
				y: 0,
			}
		}
	},
	mounted() {
		this.update_size();
	},
	beforeMount() {
		this.refresh();
		this.$os.eventsBus.Bus.on('fleet', this.listener_os_fleet);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('fleet', this.listener_os_fleet);
	},
	methods: {

		check_highlight(human) {
			if(this.state.human_selected) {
				switch(this.state.human_selected.type) {
					case 'flight_attendant': {
						return this.state.human_selected.state.service_to.includes(human.guid);
					}
					case 'passenger': {
						return this.state.human_selected.state.service_from == human.guid;
					}
				}
			}
			return false;
		},

		update_size() {
			this.$nextTick(() => {
				const ref_el = this.$refs['0_0'] as HTMLElement;
				this.size_ref.x = ref_el[0].offsetWidth;
				this.size_ref.y = ref_el[0].offsetHeight;
			});
		},

		refresh() {
			//maximums
			this.state.maximums.x = Math.max(...this.cabin.levels.map(l => l[0] + l[2]));
			this.state.maximums.y = Math.max(...this.cabin.levels.map(l => l[1] + l[3]));
			this.row_count_reset();
		},

		row_count_reset() {
			let i = 0;
			let offset = 0;
			let cumul_x = 0;
			let cumul_y = 0;
			this.state.row_labels = [];
			this.state.cols_labels = [];

			this.cabin.levels.forEach((level, index) => {

				this.state.row_labels.push([]);
				this.state.cols_labels.push([]);

				// Rows
				i = 0;
				offset = 0;
				while(i < level[1] + level[3]) {
					const features_seat = this.cabin.features.find(f => f.y == i && f.z == index && f.type == 'seat' && f.sub_type != 'pilot' && f.sub_type != 'copilot' && f.sub_type != 'jumpseat');
					if(features_seat) {
						this.state.row_labels[index].push((cumul_y + i - offset + 1).toString());
					} else {
						offset++;
						this.state.row_labels[index].push('');
					}
					i++;
				}
				cumul_y = i - offset;

				// Columns
				i = 0;
				offset = 0;
				while(i < level[0] + level[2]) {
					const features_seat = this.cabin.features.find(f => f.x == i && f.z == index && f.type == 'seat' && f.sub_type != 'pilot' && f.sub_type != 'copilot' && f.sub_type != 'jumpseat');
					if(features_seat) {
						this.state.cols_labels[index].push(AircraftCabin.seat_x[(i - offset)]);
					} else {
						offset++;
						this.state.cols_labels[index].push('');
					}
					i++;
				}

			});

			this.update_size();
		},

		set_draw_type(type :string) {
			this.state.draw_type = type;
		},

		set_mode(mode :string) {
			this.set_draw_type(null);
			this.state.ui_mode_previous = this.state.ui_mode;
			this.state.ui_mode = mode;
			switch(mode){
				case 'edit':
				case 'data': {
					this.state.human_selected = null;
					this.state.cell_selected = null;
					break;
				}
				case 'peep':{
					this.state.cell_selected = null;
					break;
				}
				default: {
					this.state.human_selected = null;
				}
			}
		},

		feature_add_many(features :AircraftCabinFeature[]) {
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					verb: 'AddFeatures',
					features: features
				},
				(response: any) => {
					response.payload.data.forEach((guid, index) => {
						features[index].guid = guid;
					});
					this.refresh_cabin();
				}
			);
		},

		refresh_cabin(callback? :Function) {
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					verb: 'Refresh',
				},
				(response: any) => {
					if(callback) {
						callback();
					}
				}
			);
		},

		feature_delete_many(cells :number[][]) {
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					verb: 'RemoveCells',
					orientation: this.state.drawing_orientation,
					cells: cells
				},
				(response: any) => {}
			);
		},

		drawing_get_feature(x :number, y :number) {
			const spl = this.state.draw_type.split(':');
			return new AircraftCabinFeature({
				type: spl[0],
				sub_type: spl[1],
				layer: spl[2],
				orientation: this.state.drawing_orientation,
				x: x,
				y: y,
				z: this.state.level,
			});
		},

		drawing_cell(x :number, y :number) {
			if(this.state.drawing) {
				const drawn = this.state.drawing_cells.find(c => c[0] == x && c[1] == y && c[2] == this.state.level);
				if(!drawn) {

					const is_del = this.state.draw_type == 'delete';
					const in_cell = this.cabin.features.filter(f => f.x == x && f.y == y && f.z == this.state.level);

					// Add the cell to drawn cells.
					this.state.drawing_cells.push([x,y,this.state.level]);

					switch(this.state.draw_type){
						case 'delete': {
							// delete everything in the cell
							in_cell.filter(x => x.layer == 'wall' ? x.orientation == this.state.drawing_orientation : true).forEach(feature => {
								this.cabin.features.splice(this.cabin.features.indexOf(feature), 1);
							});
							break;
						}
						default: {
							const spl = this.state.draw_type.split(':');
							const type = spl[0];
							const sub_type = spl[1];
							const layer = spl[2];

							in_cell.forEach(feature => {
								switch(feature.layer) {
									case 'wall': {
										if(layer == 'wall') {
											if(feature.orientation == this.state.drawing_orientation) {
												console.log(in_cell, spl, feature, this.state.drawing_orientation);
												this.cabin.features.splice(this.cabin.features.indexOf(feature), 1);
											}
										}
										break;
									}
									default: {
										if(feature.layer == layer) {
											this.cabin.features.splice(this.cabin.features.indexOf(feature), 1);
										}
										break;
									}
								}
							});

							const new_feature = this.drawing_get_feature(x,y);
							this.cabin.features.push(new_feature)
							this.state.drawing_features.push(new_feature);
							break;
						}
					}

				}
			}
		},

		drawing_end() {
			this.state.drawing = false;

			if(this.state.drawing_cells) {
				if(this.state.draw_type != 'delete'){
					this.feature_add_many(this.state.drawing_features);
				} else {
					this.feature_delete_many(this.state.drawing_cells);
				}

			}

			this.state.drawing_features = null;
			this.state.drawing_cells = null;
		},

		layoute_mouse_leave(ev) {
			this.drawing_end();
		},
		cell_mouse_down(ev, x :number, y :number) {
			if(this.state.draw_type) {
				this.state.drawing_cells = [];
				this.state.drawing_features = [];
				this.state.drawing = true;
				this.drawing_cell(x,y);
			}
		},
		cell_mouse_leave(ev, x :number, y :number) {
			this.cell_hovered = null;
		},
		cell_mouse_enter(ev, x :number, y :number) {
			this.drawing_cell(x,y);
			this.cell_hovered = [x,y];
		},
		cell_mouse_up(ev, x :number, y :number) {
			this.drawing_end();
		},
		cell_select(x :number, y :number) {

			this.state.human_selected = null;

			if(!this.state.draw_type) {
				if(this.state.cell_selected) {
					if(this.state.cell_selected[0] == x && this.state.cell_selected[1] == y) {
						this.state.cell_selected = null;
						this.set_mode(this.state.ui_mode_previous);
						return;
					}
				}

				this.state.cell_selected = [x,y,this.state.level];

				switch(this.state.ui_mode) {
					case 'edit': {
						this.set_mode('edit_cell');
						break;
					}
					case 'edit_cell':
					case 'cell': {
						break;
					}
					default: {
						this.set_mode('cell');
						break;
					}
				}
			}
		},
		human_select(human :any) {

			this.state.cell_selected = null;

			if(this.state.human_selected) {
				this.state.human_selected = null;
				this.set_mode(this.state.ui_mode_previous);
			} else {
				this.state.human_selected = human;
				this.set_mode('peep');
			}
		},

		humans_update() {
			this.last_human_update = new Date().getTime();
		},

		listener_os_fleet(wsmsg :any) {
			switch(wsmsg.name) {
				case 'cabin': {
					this.refresh();
					break;
				}
				case 'mutate': {
					this.refresh();
					break;
				}
			}
		},
	},
	computed: {
		features_in_layer():any {

			var written_features = this.cabin.features.filter(f => f.z == this.state.level);

			if(this.state.draw_type) {
				if(this.cell_hovered) {

					if(this.state.draw_type != 'delete') {

						var dts = this.state.draw_type.split(':');

						var hf = new AircraftCabinFeature({
							x: this.cell_hovered[0],
							y: this.cell_hovered[1],
							z: this.state.level,
							type: dts[0],
							sub_type: dts[1],
							orientation: this.state.drawing_orientation,
						});

						switch(dts[2]){
							case 'wall': {

								const existing = written_features.filter(x =>
									x.x == this.cell_hovered[0]
									&& x.y == this.cell_hovered[1]
									&& x.z == this.state.level
									&& x.layer == 'wall'
									&& x.orientation == this.state.drawing_orientation
								);

								if(existing.length) {
									existing.forEach(x => {
										written_features.splice(written_features.indexOf(x), 1);
									});
								}
								break;
							}
							default: {
								const existing = written_features.findIndex(x =>
									x.x == this.cell_hovered[0]
									&& x.y == this.cell_hovered[1]
									&& x.z == this.state.level
									&& x.layer == dts[2]
								);

								if(existing > -1) written_features.splice(existing, 1);
								break;
							}
						}

						if(!written_features.find(x => x.guid == hf.guid)) written_features.push(hf);

					} else {
						const existing = written_features.filter(x =>
							x.x == this.cell_hovered[0]
							&& x.y == this.cell_hovered[1]
							&& x.z == this.state.level
						);
						existing.filter(x => x.layer == 'wall' ? x.orientation == this.state.drawing_orientation : true).forEach(x => {
							const existing = written_features.findIndex(y => y == x);
							if(existing > -1) {
								written_features.splice(existing, 1);
							}
						});
					}


				}
			}

			return written_features;
		},
		humans_in_layer():any {
			this.humans_update();
			return this.state.humans.filter(x => x.state && x.state.z == this.state.level);
		},
		cargos_in_layer():any {
			return this.state.cargos.filter(x => x.state && x.state.z == this.state.level);
		},
		stairs_in_layer_below():any {
			return this.cabin.features.filter(f => f.z == (this.state.level - 1) && f.type == 'stairs');
		},
		stairs_in_layer_above():any {
			return this.cabin.features.filter(f => f.z == (this.state.level + 1) && f.type == 'stairs');
		}
	},
	watch: {
		cabin() {
			this.refresh();
		},
		level() {
			this.refresh();
		},
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

@import '@/sys/components/cabin/cabin_features.scss';

.cabin {
	min-height: 550px;
	$human_oversize: 0.1em;
	//$size_x: 20px;
	//$size_y: 30px;
	//$size_compact_x: 14px;
	//$size_compact_y: 21px;
	//$size_large_x: 30px;
	//$size_large_y: 45px;
	$size_x: 3em;
	$size_y: 3em;
	$spacing: 1px;
	$padding: 3px;

	.theme--bright & {
		&_layout {
			background-color: $ui_colors_bright_shade_1;
			&::before {
				border-color: rgba($ui_colors_bright_shade_5, 0.2);
			}
			&_humans {
				&_human {
					&.thoughts {
						&::after {
							background-color: $ui_colors_bright_shade_3;
						}
					}
					&.service {
						&::after {
							background-color: $ui_colors_bright_button_info;
						}
					}
				}
			}
			&_cell {
				&.void {
					&:before {
						background-color: $ui_colors_bright_shade_4;
					}
				}
				&.void_left {
					&:before {
						background-color: $ui_colors_bright_shade_4;
					}
				}
				&.void_right {
					&:before {
						background-color: $ui_colors_bright_shade_4;
					}
				}
				&.selected {
					&:after {
						border-color: $ui_colors_bright_button_info;
						background-color: rgba($ui_colors_bright_button_info, 0.2);
						box-shadow: 0 0 20px rgba($ui_colors_bright_button_info, 1), 0 0 5px rgba($ui_colors_bright_button_info, 1);
					}
				}
				&.window {
					&_left {
						.feature {
							&-util {
								&-lavatories,
								&-galley,
								&-fill {
									background: linear-gradient(to right, rgba($ui_colors_bright_shade_5, 0.3), rgba($ui_colors_bright_shade_3, 0.8));
								}
							}
							&-seat {
								&-jumpseat {
									background: linear-gradient(to right, rgba($ui_colors_bright_shade_5, 0.3), rgba($ui_colors_bright_shade_3, 0.8));
								}
							}
						}
					}
					&_right {
						.feature {
							&-util {
								&-lavatories,
								&-galley,
								&-fill {
									background: linear-gradient(to left, rgba($ui_colors_bright_shade_5, 0.3), rgba($ui_colors_bright_shade_3, 0.8));
								}
							}
							&-seat {
								&-jumpseat {
									background: linear-gradient(to left, rgba($ui_colors_bright_shade_5, 0.3), rgba($ui_colors_bright_shade_3, 0.8));
								}
							}
						}
					}
				}
				&.connects_stairs_above {
					.feature {
						&-stairs {
							border-color: rgba($ui_colors_bright_shade_3, 0.8);
							&.orient{
								&-up {
									border-top: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_up_up.svg);
									}
								}
								&-right {
									border-right: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_up_right.svg);
									}
								}
								&-bottom {
									border-bottom: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_up_bottom.svg);
									}
								}
								&-left {
									border-left: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_up_left.svg);
									}
								}
							}
							&:before {
								background-size: cover;
							}
						}
					}
				}
				&.connects_stairs_below {
					.feature {
						&-stairs {
							border-color: rgba($ui_colors_bright_shade_3, 0.8);
							&.orient{
								&-up {
									border-top: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_down_up.svg);
									}
								}
								&-right {
									border-right: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_down_right.svg);
									}
								}
								&-bottom {
									border-bottom: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_down_bottom.svg);
									}
								}
								&-left {
									border-left: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/dark/stairs_down_left.svg);
									}
								}
							}
							&:before {
								background-size: cover;
							}
						}
					}
				}
			}
		}
		&.edit_cell,
		&.edit {
			.cabin {
				&_layout {
					&_cell {
						&.connects_stairs_above,
						&.connects_stairs_below {
							&:before {
								display: block;
								background-color: rgba($ui_colors_bright_button_info, 0.3)
							}
						}
						&:before {
							display: block;
							border-color: rgba($ui_colors_bright_shade_3, 0.5);
							//background: linear-gradient(135deg, rgba($ui_colors_bright_shade_3, 0.3), rgba($ui_colors_bright_shade_3, 0.6));
						}
					}
				}
			}
		}
		&_nose {
			background-image: url(../../../sys/assets/pax/dark/cabin_nose.svg);
		}
		&_tail {
			background-image: url(../../../sys/assets/pax/dark/cabin_tail.svg);
		}
	}

	.theme--dark & {
		&_layout {
			background-color: $ui_colors_dark_shade_1;
			&:before {
				border-color: rgba($ui_colors_dark_shade_5, 0.2);
			}
			&_humans {
				&_human {
					&.thoughts {
						&::after {
							background-color: $ui_colors_dark_shade_3;
						}
					}
					&.service {
						&::after {
							background-color: $ui_colors_dark_button_info;
						}
					}
				}
			}
			&_cell {
				&.void {
					&:before {
						background-color: $ui_colors_dark_shade_4;
					}
				}
				&.void_left {
					&:before {
						background-color: $ui_colors_dark_shade_4;
					}
				}
				&.void_right {
					&:before {
						background-color: $ui_colors_dark_shade_4;
					}
				}
				&.selected {
					&:after {
						border-color: $ui_colors_dark_button_info;
						background-color: rgba($ui_colors_dark_button_info, 0.3);
						box-shadow: 0 0 20px rgba($ui_colors_dark_button_info, 1), 0 0 5px rgba($ui_colors_dark_button_info, 1);
					}
				}
				&.window {
					&_left {
						.feature {
							&-util {
								&-lavatories,
								&-galley,
								&-fill {
									background: linear-gradient(to right, rgba($ui_colors_dark_shade_5, 0.2), rgba($ui_colors_dark_shade_3, 0.8));
								}

							}
							&-seat {
								&-jumpseat {
									background: linear-gradient(to right, rgba($ui_colors_dark_shade_5, 0.2), rgba($ui_colors_dark_shade_3, 0.8));
								}
							}
						}
					}
					&_right {
						.feature {
							&-util {
								&-lavatories,
								&-galley,
								&-fill {
									background: linear-gradient(to left, rgba($ui_colors_dark_shade_5, 0.2), rgba($ui_colors_dark_shade_3, 0.8));
								}
							}
							&-seat {
								&-jumpseat {
									background: linear-gradient(to left, rgba($ui_colors_dark_shade_5, 0.2), rgba($ui_colors_dark_shade_3, 0.8));
								}
							}
						}
					}
				}
				&.connects_stairs_above {
					.feature {
						&-stairs {
							border-color: rgba($ui_colors_dark_shade_3, 0.8);
							&.orient{
								&-up {
									border-top: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_up_up.svg);
									}
								}
								&-right {
									border-right: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_up_right.svg);
									}
								}
								&-bottom {
									border-bottom: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_up_bottom.svg);
									}
								}
								&-left {
									border-left: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_up_left.svg);
									}
								}
							}
							&:before {
								background-size: cover;
							}
						}
					}
				}
				&.connects_stairs_below {
					.feature {
						&-stairs {
							border-color: rgba($ui_colors_dark_shade_3, 0.8);
							&.orient{
								&-up {
									border-top: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_down_up.svg);
									}
								}
								&-right {
									border-right: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_down_right.svg);
									}
								}
								&-bottom {
									border-bottom: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_down_bottom.svg);
									}
								}
								&-left {
									border-left: 0;
									&:before {
										background-image: url(../../../sys/assets/pax/bright/stairs_down_left.svg);
									}
								}
							}
							&:before {
								background-size: cover;
							}
						}
					}
				}
			}
		}
		&.edit_cell,
		&.edit {
			.cabin {
				&_layout {
					border-color: rgba($ui_colors_dark_shade_3, 0.1);
					&_cell {
						&.connects_stairs_above,
						&.connects_stairs_below {
							&:before {
								display: block;
								background-color: rgba($ui_colors_dark_button_info, 0.3)
							}
						}
						&:before {
							display: block;
							border-color: rgba($ui_colors_dark_shade_3, 0.1);
						}
					}
				}
			}
		}
		&_nose {
			background-image: url(../../../sys/assets/pax/bright/cabin_nose.svg);
		}
		&_tail {
			background-image: url(../../../sys/assets/pax/bright/cabin_tail.svg);
		}
	}

	&.drawing {
		.cabin {
			&_layout {
				&_cell {
					cursor: crosshair;
				}
				&_humans {
					&_human {
						pointer-events: none;
						opacity: 0.5;
					}
				}
			}
		}
	}

	&_container {
		display: flex;
		justify-content: center;
	}
	&_nose {
		height: 90px;
		width: 100%;
		flex-grow: 1;
		opacity: 0.2;
		mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 0) 0%, rgba(0, 0, 0, 1) 100%);
		background-size: 100% auto;
		background-repeat: no-repeat;
		background-position: bottom;
	}
	&_tail {
		height: 90px;
		width: 100%;
		flex-grow: 1;
		opacity: 0.2;
		mask-image: linear-gradient(to bottom, rgba(0, 0, 0, 1) 0%, rgba(0, 0, 0, 0) 100%);
		background-size: 100% auto;
		background-repeat: no-repeat;
		background-position: top;
	}
	&_wrap {
		display: flex;
		justify-content: flex-start;
		align-items: center;
		flex-direction: column;
		margin-left: 20px;
	}
	&_layout {
		position: relative;
		padding: $padding;
		padding-top: 0;
		padding-bottom: 0;
		margin: 2px;
		&:before {
			content: '';
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			margin: -2px;
			border: 2px solid transparent;
		}
		&_humans {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			margin: 0 $padding;
			z-index: 5;
			pointer-events: none;
			&_human {
				position: absolute;
				width: $size_x + ($human_oversize * 2);
				height: $size_x + ($human_oversize * 2);
				padding: $spacing;
				margin: -$human_oversize;
				transition: transform 0.5s cubic-bezier(.55,.33,.6,1), opacity 0.5s cubic-bezier(.55,.33,.6,1);
				background-image: url(../../../sys/assets/pax/icons.svg);
				background-size: 800%;
				$offset_x: 100 / 7;
				$offset_y: 100 / 7;
				pointer-events: none;
				opacity: 0;
				cursor: pointer;
				&::before {
					position: absolute;
					top: 0;
					right: 0;
					bottom: 0;
					left: 0;
					content: '';
					pointer-events: none;
					background-position: 0% 50%;
					background-repeat: no-repeat;
				}
				&.boarded {
					opacity: 1;
					pointer-events: all;
				}
				&.highlight{
					filter: drop-shadow(0 0 8px $ui_colors_bright_button_info) drop-shadow(0 0 5px $ui_colors_bright_button_info) drop-shadow(0 0 2px $ui_colors_bright_button_info);
					z-index: 3;
				}
				&.selected {
					filter: drop-shadow(0 0 8px $ui_colors_bright_button_cancel) drop-shadow(0 0 5px $ui_colors_bright_button_cancel) drop-shadow(0 0 2px $ui_colors_bright_button_cancel);
					z-index: 3;
				}
				&.thoughts {
					&::after {
						@keyframes human_thought {
							from {
								top: 6px;
								right: 6px;
								width: 2px;
								height: 2px;
								margin-top: -2px;
								margin-left: -1px;
							}
							to {
								top: 0;
								right: 0;
								width: 10px;
								height: 10px;
								margin-top: -2.5px;
								margin-left: -5px;
							}
						}
						position: absolute;
						top: 0;
						right: 0;
						content: '';
						width: 8px;
						height: 8px;
						margin-top: -2px;
						margin-left: -4px;
						border-radius: 50%;
						z-index: 3;
						animation: human_thought 2s infinite alternate;
					}
				}
				&.service {
					&::after {
						position: absolute;
						top: 0;
						right: 0;
						content: '';
						width: 8px;
						height: 8px;
						margin-top: -2px;
						margin-right: -2px;
						border-radius: 50%;
						z-index: 3;
						animation: none;
					}
				}
				&.human-icon {
					&-neutral {
						background-position: $offset_x*0% calc(#{$offset_y}*0%);
					}
					&-happy {
						background-position: $offset_x*1% calc(#{$offset_y}*0%);
					}
					&-annoyed {
						background-position: $offset_x*2% calc(#{$offset_y}*0%);
					}
					&-angry {
						background-position: $offset_x*3% calc(#{$offset_y}*0%);
					}
					&-sleeping {
						background-position: $offset_x*4% calc(#{$offset_y}*0%);
					}
					&-fear {
						background-position: $offset_x*5% calc(#{$offset_y}*0%);
					}
					&-dead {
						background-position: $offset_x*6% calc(#{$offset_y}*0%);
					}
					&-sick {
						background-position: $offset_x*7% calc(#{$offset_y}*0%);
					}
					&-type {
						&-flight_attendant {
							&::before {
								height: 200%;
								margin-top: -50%;
								background-image: url(../../../sys/assets/pax/icon-attendant.svg);
							}
						}
					}
					&-sub {
						&-movie {
							&::before {
								background-image: url(../../../sys/assets/pax/icon-music.svg);
							}
						}
					}
				}
			}
		}
		&_cargos {
			position: absolute;
			top: 0;
			right: 0;
			bottom: 0;
			left: 0;
			margin: 0 $padding;
			z-index: 3;
			pointer-events: none;
			&_cargo {
				position: absolute;
				width: $size_x + ($human_oversize * 2);
				height: $size_x + ($human_oversize * 2);
				padding: $spacing;
				margin: -$human_oversize;
				transition: transform 0.5s cubic-bezier(.55,.33,.6,1), opacity 0.5s cubic-bezier(.55,.33,.6,1);
				background-image: url(../../../sys/assets/cargo/icons.svg);
				background-size: 800%;
				$offset_x: 100 / 7;
				$offset_y: 100 / 7;
				pointer-events: none;
				opacity: 0;
				cursor: pointer;
				&::before {
					position: absolute;
					top: 0;
					right: 0;
					bottom: 0;
					left: 0;
					content: '';
					pointer-events: none;
					background-position: 0% 50%;
					background-repeat: no-repeat;
				}
				&.boarded {
					opacity: 1;
					pointer-events: all;
				}
				&.highlight{
					filter: drop-shadow(0 0 8px $ui_colors_bright_button_info) drop-shadow(0 0 5px $ui_colors_bright_button_info) drop-shadow(0 0 2px $ui_colors_bright_button_info);
					z-index: 3;
				}
				&.selected {
					filter: drop-shadow(0 0 8px $ui_colors_bright_button_cancel) drop-shadow(0 0 5px $ui_colors_bright_button_cancel) drop-shadow(0 0 2px $ui_colors_bright_button_cancel);
					z-index: 3;
				}
				&.cargo-icon {
					&-generic {
						background-position: $offset_x*0% calc(#{$offset_y}*0%);
					}
				}
			}
		}
		&_col {
			&_labels {
				display: flex;
				height: 20px;
				margin-top: -20px;
			}
			&_label {
				position: relative;
				width: $size_x;
				padding: $spacing;
				display: flex;
				justify-content: center;
				opacity: 0.3;
			}
		}
		&_row {
			position: relative;
			&_content {
				display: flex;
			}
			&_label {
				text-align: right;
				position: absolute;
				margin-left: -30px;
				width: 20px;
				opacity: 0.3;
			}
		}
		&_cell {
			position: relative;
			width: $size_x;
			height: $size_y;
			padding: $spacing;
			border-radius: 2px;
			border: 0 solid transparent;
			cursor: pointer;
			&:before {
				display: none;
				content: '';
				pointer-events: none;
				position: absolute;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				border: 0 dashed transparent;
				border-right-width: 1px;
				border-bottom-width: 1px;
				//transition: box-shadow 0.1s ease-out, background 0.1s ease-out;
			}
			&:first-child {
				&:before {
					border-left-width: 1px;
				}
			}
			&:after {
				display: none;
				content: '';
				pointer-events: none;
				position: absolute;
				top: 0;
				right: 0;
				bottom: 0;
				left: 0;
				margin: -5px;
				border-radius: 6px;
				border: 2px solid transparent;
				box-shadow: 0 0 0 transparent, 0 0 0 transparent;
			}
			&.selected {
				&:after {
					display: block;
				}
			}
			&.window {
				&_left {
					.feature {
						&-util {
							margin-left: -$spacing - 3;
							&:before {
								left: 5px;
							}
						}
						&-seat {
							&-jumpseat {
								margin-left: -$spacing - 3;
								&:before {
									left: 5px;
								}
							}
						}
					}
					&.void {
						&:before {
							margin-left: -3px;
						}
					}
				}
				&_right {
					.feature {
						&-util {
							margin-right: -$spacing - 3;
							&:before {
								right: 4px;
							}
						}
						&-seat {
							&-jumpseat {
								margin-right: -$spacing - 3;
								&:before {
									right: 4px;
								}
							}
						}
					}
					&.void {
						&:before {
							margin-right: -3px;
						}
					}
				}
			}
			&.void {
				&:before {
					display: block;
				}
			}
			&.void_left {
				&:before {
					display: block;
					margin-left: -3px;
					margin-right: 3px;
				}
			}
			&.void_right {
				&:before {
					display: block;
					margin-right: -3px;
					margin-left: 3px;
				}
			}
			&.void,
			&.void_left,
			&.void_right {
				opacity: 0.2;
				border-radius: 0;
			}
		}
	}

}

</style>
