<template>
	<scroll_stack class="cabin_data columns" :sid="'p42_aircraft_cabin'">
		<template v-slot:top>
			<div class="controls-top h_edge_padding">
				<div class="columns">
					<div class="column column_narrow">
						<button_nav class="no-wrap shadowed-shallow" shape="back" @click.native="$emit('close')">Done</button_nav>
					</div>
					<div class="column column_justify_center column_align_end h_edge_padding_right_half">
						<strong>Editing cabin</strong>
					</div>
				</div>
			</div>
		</template>
		<template v-slot:content>
			<div class="h_edge_margin_bottom">

				<collapser :withArrow="true" :default="false" class="h_edge_margin outlined h_edge_margin_bottom_half">
					<template v-slot:title>
						<div class="columns h_edge_padding_half">
							<div class="column column_justify_center">
								<strong>Edit Floors</strong>
							</div>
							<div class="column column_narrow column_justify_center">
								<div class="collapser_arrow"></div>
							</div>
						</div>
					</template>
					<template v-slot:content>
						<div class="h_edge_padding_half">
							<div class="floors">
								<div class="floor_add h_edge_margin_bottom_half">
									<button_action class="small shadowed-shallow" @click.native="add_level(cabin.levels[cabin.levels.length - 1], cabin.levels.length)">Insert floor above {{ cabin.levels.length }}</button_action>
								</div>
								<div class="floor" v-for="(l, i) in get_levels()" :key="i">
									<div class="app-box app-box-margined shadowed-deep floor_box" :class="{ 'selected': state.level == l.i }">
										<div class="h_edge_padding_half">
											<div class="columns columns_margined_half h_edge_margin_bottom_half">
												<div class="column column_justify_center">
													<button_action class="shadowed-shallow justify grow" :class="{ 'info': state.level == l.i }" @click.native="set_level(l.i)">{{ l.i + 1 }}. {{ get_level_label(l.i) }}</button_action>
												</div>
												<div class="column column_narrow">
													<button_action :class="{ 'disabled': i == 0 }" @click.native="move_level(l.i, l.i + 1)" class="icon icon-up shadowed-shallow"></button_action>
												</div>
												<div class="column column_narrow">
													<button_action :class="{ 'disabled': i >= cabin.levels.length - 1 }" @click.native="move_level(l.i, l.i - 1)" class="icon icon-down shadowed-shallow"></button_action>
												</div>
												<div class="column column_narrow">
													<button_action :class="{ 'disabled': cabin.levels.length == 1 }" @click.native="remove_level(l.i)" class="cancel icon icon-close shadowed-shallow"></button_action>
												</div>
											</div>
											<div class="h_edge_padding_half">
												<span>{{ l.l[0] }}x{{ l.l[1] }}</span>,
												<span>{{ cabin.features.filter(f => f.z == l.i && f.type == 'seat').length }} Seats</span>,
												<span>{{ cabin.features.filter(f => f.z == l.i && f.type == 'cargo').length }} Cargo</span>
											</div>
											<div>
												<collapser :state="state.level == l.i">
													<template v-slot:content>
														<div class="h_edge_padding_half">

															<div class="columns columns_margined h_edge_padding_top_half">
																<div class="column column_justify_center">
																	<p class="h_no-margin">Width</p>
																</div>
																<div class="column">
																	<div class="buttons_list shadowed-shallow h_no-margin">
																		<selector v-model="cabin.levels[state.level][0]" @input="set_x">
																			<option v-for="i in 12" :key="i" :value="i" >{{i}}</option>
																		</selector>
																	</div>
																</div>
															</div>

															<div class="columns columns_margined h_edge_padding_top_half">
																<div class="column column_justify_center">
																	<p class="h_no-margin">Rows</p>
																</div>
																<div class="column">
																	<div class="buttons_list shadowed-shallow h_no-margin">
																		<selector v-model="cabin.levels[state.level][1]" @input="set_y">
																			<option v-for="i in 64" :key="i" :value="i" >{{i}}</option>
																		</selector>
																	</div>
																</div>
															</div>

															<div class="columns columns_margined h_edge_padding_top_half">
																<div class="column column_justify_center">
																	<p class="h_no-margin">Offset X</p>
																</div>
																<div class="column">
																	<div class="buttons_list shadowed-shallow h_no-margin">
																		<selector v-model="cabin.levels[state.level][2]" @input="set_ox">
																			<option v-for="i in 13" :key="i - 1" :value="i - 1" >{{i - 1}}</option>
																		</selector>
																	</div>
																</div>
															</div>

															<div class="columns columns_margined h_edge_padding_top_half">
																<div class="column column_justify_center">
																	<p class="h_no-margin">Offset Y</p>
																</div>
																<div class="column">
																	<div class="buttons_list shadowed-shallow h_no-margin">
																		<selector v-model="cabin.levels[state.level][3]" @input="set_oy">
																			<option v-for="i in 65" :key="i - 1" :value="i - 1" >{{i - 1}}</option>
																		</selector>
																	</div>
																</div>
															</div>

														</div>
													</template>
												</collapser>
											</div>
										</div>
									</div>
									<div class="floor_add h_edge_margin_bottom_half">
										<button_action class="small shadowed-shallow" @click.native="add_level(l.l, l.i)">Insert floor below {{ (l.i + 1) }}</button_action>
									</div>
								</div>
							</div>
						</div>
					</template>
				</collapser>

				<collapser :withArrow="true" :default="false" class="h_edge_margin outlined">
					<template v-slot:title>
						<div class="columns h_edge_padding_half">
							<div class="column column_justify_center">
								<strong>Import/Export/Reset</strong>
							</div>
							<div class="column column_narrow column_justify_center">
								<div class="collapser_arrow"></div>
							</div>
						</div>
					</template>
					<template v-slot:content>
						<div class="h_edge_padding_half">
							<div class="columns columns_margined">
								<div class="column">
									<div class="buttons_list shadowed-shallow">
										<button_listed @click.native="import_file()">Import</button_listed>
									</div>
								</div>
								<div class="column">
									<div class="buttons_list shadowed-shallow">
										<button_listed @click.native="export_file()">Export</button_listed>
									</div>
								</div>
								<div class="column">
									<div class="buttons_list shadowed-shallow">
										<button_listed @click.native="reset()">Reset</button_listed>
									</div>
								</div>
							</div>
						</div>
					</template>
				</collapser>

				<div class="h_edge_padding">
					<div class="columns columns_margined h_edge_margin_bottom">
						<div class="column">
							<data_stack class="center small" label="Seats">
								<number :amount="cabin.features.filter(f => f.type == 'seat').length" :decimals="0"/>
							</data_stack>
						</div>
						<div class="column">
							<data_stack class="center small" label="To max">
								<number class="distance" :amount="((cabin.features.filter(f => f.type == 'seat').length * 85) / (aircraft.max_weight - aircraft.empty_weight)) * 100" :decimals="0"/>%
							</data_stack>
						</div>
					</div>
					<div class="columns columns_margined">
						<div class="column">
							<data_stack class="center small" label="Fully loaded">
								~<weight class="distance" :amount="cabin.features.filter(f => f.type == 'seat').length * 85" :decimals="0"/>
							</data_stack>
						</div>
						<div class="column">
							<data_stack class="center small" label="Weight left">
								<weight :amount="(aircraft.max_weight - aircraft.empty_weight) - (cabin.features.filter(f => f.type == 'seat').length * 85)" :decimals="0"/>
							</data_stack>
						</div>
					</div>
				</div>

				<div class="h_edge_margin buttons_list shadowed-shallow">
					<div class="columns">
						<div class="column column_h-stretch">
							<button_listed class="listed_h button_done" :class="[{ 'disabled': !draw_type }]" @click.native="set_draw_type(null)">Done</button_listed>
						</div>
						<div class="column column_h-stretch">
							<button_listed class="listed_h" :class="[{ 'selected': draw_type == 'delete' }]" @click.native="set_draw_type('delete')">Eraser</button_listed>
						</div>
					</div>
				</div>

				<div class="h_edge_margin h_flex buttons_list">
					<div class="h_margin_center">
						<div class="columns">
							<div class="column column_3 h_edge_margin_quarter">
							</div>
							<div class="column column_3 h_edge_margin_quarter">
								<button_tile class="shadowed-shallow" :class="[{ 'selected': state.drawing_orientation == 'up' }]" @click.native="set_orient('up')">Up</button_tile>
							</div>
							<div class="column column_3 h_edge_margin_quarter">
							</div>
						</div>
						<div class="columns">
							<div class="column column_3 h_edge_margin_quarter">
								<button_tile class="shadowed-shallow" :class="[{ 'selected': state.drawing_orientation == 'left' }]" @click.native="set_orient('left')">Left</button_tile>
							</div>
							<div class="column column_3 h_edge_margin_quarter">
							</div>
							<div class="column column_3 h_edge_margin_quarter">
								<button_tile class="shadowed-shallow" :class="[{ 'selected': state.drawing_orientation == 'right' }]" @click.native="set_orient('right')">Right</button_tile>
							</div>
						</div>
						<div class="columns">
							<div class="column column_3 h_edge_margin_quarter">
							</div>
							<div class="column column_3 h_edge_margin_quarter">
								<button_tile class="shadowed-shallow" :class="[{ 'selected': state.drawing_orientation == 'bottom' }]" @click.native="set_orient('bottom')">Bottom</button_tile>
							</div>
							<div class="column column_3 h_edge_margin_quarter">
							</div>
						</div>
					</div>
				</div>

				<div class="cabin_features_picker" v-for="(type, index) in cabin_options" v-bind:key="index" :index="index">
					<div class="section_header">
						<h2><span class="notice">{{type.sub_types.length }}</span> {{ type.name }}</h2>
					</div>

					<div class="buttons_list h_edge_padding_lateral h_edge_padding_bottom">
						<button_listed
							v-for="(sub_type, index1) in type.sub_types"
							v-bind:key="index1"
							:index="index1"
							class="cabin_layout shadowed-shallow"
							:class="[
								{
									'selected': draw_type == type.type + ':' + sub_type.sub_type + ':' + sub_type.layer
								}
							]"
							@click.native="set_draw_type(type.type + ':' + sub_type.sub_type + ':' + sub_type.layer)"
						>
							<template v-slot:icon>
								<span class="icon">
									<span class="feature"
										:class="[
											'feature-' + type.type ,
											'feature-' + type.type + '-' + sub_type.sub_type
										]"></span>
								</span>
							</template>
							<template v-slot:label>
								{{ sub_type.name }}
							</template>
							<template v-slot:right>
								<number :amount="cabin.features.filter(f => f.type == type.type && f.sub_type == sub_type.sub_type).length" />
							</template>
						</button_listed>
					</div>

				</div>
			</div>
		</template>
	</scroll_stack>
</template>

<script lang="ts">
import Vue from 'vue';
import Eljs from '@/sys/libraries/elem';
import Aircraft from '@/sys/classes/aircraft';
import AircraftCabin from '@/sys/classes/cabin/aircraft_cabin';
import AircraftCabinFeature from '@/sys/classes/cabin/aircraft_cabin_feature';
import AircraftCabinState from '@/sys/classes/cabin/aircraft_cabin_state';

export default Vue.extend({
	props:{
		aircraft :Aircraft,
		cabin :Object as () => AircraftCabin,
		state: Object as () => AircraftCabinState,
		draw_type :String,
	},
	data() {
		return {
			cabin_options: AircraftCabin.feature_options,
			z: 0,
		}
	},
	methods: {
		init() {
			this.z = this.cabin.levels.length;
		},
		set_x(state :string) {
			this.cabin.levels[this.state.level][0] = parseInt(state);

			this.save_cabin(() => {
				this.refresh_cabin();
			});
		},
		set_y(state :string) {
			this.cabin.levels[this.state.level][1] = parseInt(state);

			this.save_cabin(() => {
				this.refresh_cabin();
			});
		},
		set_z(state :string) {
			this.z = parseInt(state);

			if(this.state.level >= this.z){
				this.state.level = 0;
			}

			Eljs.resize_array(this.cabin.levels, this.z, [this.cabin.levels[0][0],5,0,0]);

			this.save_cabin(() => {
				this.refresh_cabin();
			});
		},
		set_ox(state :string) {
			this.cabin.levels[this.state.level][2] = parseInt(state);

			this.save_cabin(() => {
				this.refresh_cabin();
			});
		},
		set_oy(state :string) {
			this.cabin.levels[this.state.level][3] = parseInt(state);

			this.save_cabin(() => {
				this.refresh_cabin();
			});
		},
		set_orient(va :string) {
			this.state.drawing_orientation = va;
		},
		set_draw_type(type :string) {
			if(this.draw_type == type) {
				this.$emit('draw_type', null);
			} else {
				this.$emit('draw_type', type);
			}
		},
		features_add(features :AircraftCabinFeature[], callback? :Function) {
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
		features_remove(features :AircraftCabinFeature[], callback? :Function) {

			if(features.length) {
				let guids = [];
				let features_to_remove = Object.assign([], features);
				features_to_remove.forEach(feature => {
					this.cabin.features.splice(this.cabin.features.indexOf(feature), 1);
					guids.push(feature.guid);
				});

				this.$os.api.send_ws(
					'fleet:interact:cabin',
					{
						id: this.aircraft.id,
						livery: this.cabin.livery,
						verb: 'RemoveFeatures',
						guids: guids
					},
					(response: any) => {
						if(callback)
							callback();
						this.refresh_cabin();
					}
				);
			} else {
				if(callback)
					callback();
				this.refresh_cabin();
			}
		},
		save_features(features :AircraftCabinFeature[], callback? :Function) {
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					verb: 'UpdateFeatures',
					features: features,
				},
				(response: any) => {
					if(callback)
						callback();
				}
			);
		},
		save_feature(feature :AircraftCabinFeature) {

			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					guid: feature.guid,
					data: feature,
				},
				(response: any) => {}
			);
		},
		save_cabin(callback? :Function) {
			this.$emit('refresh');
			this.$os.api.send_ws(
				'fleet:interact:cabin',
				{
					id: this.aircraft.id,
					livery: this.cabin.livery,
					verb: 'Update',
					name: this.cabin.name,
					levels: this.cabin.levels,
				},
				(response: any) => {
					if(callback) {
						callback();
					}
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
		reset() {
			this.$os.modals.add({
				type: 'ask',
				title: 'Are you sure you want to reset this cabin layout?',
				text: [
					"This cannot be undone.",
				],
				actions: {
					yes: 'Nuke it!',
					no: 'Cancel'
				},
				data: {
				},
				func: (state :boolean) => {
					if(state) {
						this.features_remove(this.cabin.features, () => { });
					}
				}
			});
		},
		get_level_label(z :number) {
			return this.cabin.get_level_label(z);
		},
		set_level(index :number) {
			this.state.level = index;
		},
		add_level(template: any, index :number){
			this.cabin.levels.splice(index, 0, template);
			this.state.level = index;

			this.save_cabin();

			const features_to_move = this.cabin.features.filter(f => f.z >= index);
			features_to_move.forEach(feature => {
				feature.z += 1;
			});

			this.save_features(features_to_move, () => {
				this.refresh_cabin();
			});
		},
		move_level(old_index :number, new_index :number){

			const features_to_move_new = this.cabin.features.filter(f => f.z == new_index);
			const features_to_move_old = this.cabin.features.filter(f => f.z == old_index);

			features_to_move_new.forEach(feature => {
				feature.z = old_index;
			});

			features_to_move_old.forEach(feature => {
				feature.z = new_index;
			});

			let changed = [];
			changed = changed.concat(features_to_move_new);
			changed = changed.concat(features_to_move_old);

			const old_level = this.cabin.levels[old_index];
			const new_level = this.cabin.levels[new_index];

			this.cabin.levels.splice(old_index, 1);
			this.cabin.levels.splice(old_index, 0, new_level);

			this.cabin.levels.splice(new_index, 1);
			this.cabin.levels.splice(new_index, 0, old_level);

			this.state.level = new_index;

			this.save_features(changed, () => {
				this.save_cabin(() => {
					this.refresh_cabin();
				});
			});
		},
		remove_level(index :number) {

			this.features_remove(this.cabin.features.filter(f => f.z == index), () => {

				const features_to_move = this.cabin.features.filter(f => f.z > index);
				features_to_move.forEach(feature => {
					feature.z -= 1;
				});

				if(features_to_move.length) {
					this.save_features(features_to_move, () => {
						this.cabin.levels.splice(index, 1);
						this.save_cabin(() => {
							this.refresh_cabin();
						});
					});
				} else {
					this.cabin.levels.splice(index, 1);
					this.save_cabin(() => {
						this.refresh_cabin();
					});
				}
			});

			if(this.state.level > index) {
				if(this.state.level > 0) {
					this.state.level -= 1;
				}
			}

		},
		get_levels() {
			const levels = [];
			const length = this.cabin.levels.length;

			this.cabin.levels.forEach((level, index) => {
				const i = length - index - 1;
				levels.push({
					i: i,
					l: this.cabin.levels[i]
				});
			});

			return levels;
		},
		import_file() {

			const doit = () => {
				var input = document.createElement('input');
				input.type = 'file';

				input.onchange = (e :any) => {

					// setting up the reader
					var reader = new FileReader();
					reader.readAsText(e.target.files[0],'UTF-8');

					// here we tell the reader what to do when it's done reading...
					reader.onload = readerEvent => {
						var content = readerEvent.target.result; // this is the content!

						const new_cabin = new AircraftCabin(JSON.parse(content as string));

						this.features_remove(this.cabin.features, () => {
							this.$nextTick(() => {
								this.cabin.name = new_cabin.name;
								this.cabin.levels = new_cabin.levels;
								this.cabin.features = [];
								this.save_cabin(() => {
									new_cabin.features.forEach(feature => {
										const new_feature = new AircraftCabinFeature(feature);
										this.cabin.features.push(new_feature);
									});

									this.features_add(this.cabin.features, () => {
										this.refresh_cabin();
									});
								});
							});

						});

					}
				}
				input.click();
			}

			if(this.cabin.features.length) {
				this.$os.modals.add({
					type: 'ask',
					title: 'Are you sure you want to replace this cabin layout with a new one?',
					text: [
						"This cannot be undone.",
					],
					actions: {
						yes: 'Yes',
						no: 'Cancel'
					},
					data: {
					},
					func: (state :boolean) => {
						if(state) {
							doit();
						}
					}
				});
			} else {
				doit();
			}

		},
		export_file() {
            Eljs.downloadFiles(JSON.stringify(this.cabin), this.cabin.livery + '.json', "text/json")
		},
	},
	mounted() {
		this.init();
	},
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

@import '@/sys/components/cabin/cabin_features.scss';

.cabin_data {
	flex-grow: 1;

	.theme--bright & {
		.button_done {
			color: $ui_colors_bright_shade_1;
			background-color: rgba($ui_colors_bright_button_cancel, 0.8);
		}
		.icon_paint {
			background-image: url(../../../sys/assets/icons/dark/paint.svg);
		}
		.floor_box {
			&.selected {
				&::before {
					border-color: $ui_colors_bright_button_info;
				}
			}
		}
	}
	.theme--dark & {
		.button_done {
			color: $ui_colors_dark_shade_5;
			background-color: rgba($ui_colors_dark_button_cancel, 0.8);
		}
		.icon_paint {
			background-image: url(../../../sys/assets/icons/bright/paint.svg);
		}
		.floor_box {
			&.selected {
				&::before {
					border-color: $ui_colors_dark_button_info;
				}
			}
		}
	}

	.icon_paint {
		width: 56px;
		height: 56px;
		margin: 0 auto;
		opacity: 0.8;
		background-size: contain;
		background-repeat: no-repeat;
		background-position: center;
	}

	.floor_box {
		&.selected {
			&::before {
				border-width: 4px;
			}
		}
		&::before {
			content: '';
			position: absolute;
			top: 0;
			left: 0;
			bottom: 0;
			right: 0;
			transition: border 0.2s ease-out;
			border: 1px solid transparent;
			border-radius: 12px;
			pointer-events: none;
		}
	}

	.section_header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 8px 16px;
		padding-top: 16px;
		h2 {
			margin: 0;
			.notice {
				opacity: 0.5;
			}
		}
	}

	.cabin_layout {
		.icon {
			position: relative;
			.feature {
				display: block;
			}
		}
	}
}

</style>