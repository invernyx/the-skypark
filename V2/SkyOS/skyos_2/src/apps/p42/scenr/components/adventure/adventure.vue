<template>
	<content_controls_stack class="p42_scenr_adventure" :nav_padding="true" :translucent="true" :shadowed="true">
		<template v-slot:nav>
			<div class="rows">
				<div class="row content-block">
					<div class="h-stack">
						<button_nav shape="back" @hold="CloseProject(false)" :hold="true" class="cancel" :class="{ 'disabled': adventure.File.length == 0 || (!state.ui.saveAvailable || state.ui.generating) && $root.$data.state.services.api.connected }">X</button_nav>
						<button_nav @click.native="Save(null)" class="go" :class="{ 'disabled': adventure.File.length == 0 || !state.ui.saveAvailable || state.ui.generating }">{{ state.ui.saveAvailable ? 'Save' : 'Saving...' }}</button_nav>
						<!--<button_nav @click.native="Export()">Export</button_nav>-->
					</div>
					<div class="h-stack">
						<button_nav @click.native="RegenContracts()" class="info" :class="{ 'disabled': adventure.File.length == 0 || !state.ui.saveAvailable || !$root.$data.state.services.api.connected || app.$data.state.ui.map.sources.routes.data.features.length == 0 || state.ui.generating }">Gen Ctx</button_nav>
						<button_nav @click.native="RegenRoutes()" class="info" :class="{ 'disabled': adventure.File.length == 0 || !state.ui.saveAvailable || !$root.$data.state.services.api.connected || state.ui.generating }">{{ state.ui.generating ? '...' : 'Gen Rte' }} {{ app.$data.state.ui.map.sources.routes.data.features.length }}</button_nav>
					</div>
				</div>
			</div>
		</template>

		<template v-slot:content>
			<div class="helper_edge_padding" v-if="adventure.Loaded">

				<div style="height: 190px">
					<ContractSummary
						v-if="state.ui.sampleContract && state.ui.sampleTemplate"
						:shadowed="true"
						:index="0"
						:contract="state.ui.sampleContract"
						:selectable="true"
						:templates="[state.ui.sampleTemplate]"
						:selected="false"
						:canRedirect="false"
						@select="GetSample"
						@expand="OpenSample"
					/>
					<button_action v-else @click.native="GetSample" class="info" :class="{ 'loading': state.ui.waitingSample, 'disabled': !$root.$data.state.services.api.connected || state.ui.generating || state.ui.waitingSample }">Get card sample</button_action>
				</div>
				<br/>

				<div class="buttons_list shadowed">
					<textbox class="listed" type="text" placeholder="File Name" v-model="adventure.File" @modified="Rename"></textbox>
				</div>

				<div class="buttons_list shadowed">
					<textbox class="listed" type="text" placeholder="Adventure Displayed Name" v-model="adventure.Name"></textbox>
				</div>

				<div class="buttons_list shadowed">
					<textbox class="listed" type="text" placeholder="Template Share Code" v-model="adventure.TemplateCode"></textbox>
				</div>

				<div class="content-block">
					<toggle v-model="adventure.Published">Published</toggle>
				</div>

				<div class="buttons_list shadowed">
					<button_listed :class="['naved', { 'naved-in': adventure.Tiers.includes('prospect') }]" @click.native="ToggleTier('prospect')">prospect</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Tiers.includes('discovery') }]" @click.native="ToggleTier('discovery')">discovery</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Tiers.includes('endeavour') }]" @click.native="ToggleTier('endeavour')">endeavour</button_listed>
				</div>

				<h3 class="helper_edge_margin_vertical_half">Media</h3>
				<div class="buttons_list shadowed">
					<div v-for="(url, index) in adventure.ImageURL" v-bind:key="index">
						<div class="image-sample" :style="'background-image: url(' + $os.getCDNBase(url) + ')'"></div>
						<textbox class="listed" type="text" :placeholder="'Banner Image ' + (index + 1) + ' (Erase to remove)'" v-model="adventure.ImageURL[index]" @changed="!$event.length ? adventure.ImageURL.splice(index, 1) : '';"></textbox>
						<span></span>
					</div>
					<textbox class="listed" type="text" placeholder="+ Banner Img URL (Hit return)" @returned="($event) => { if($event.length) { adventure.ImageURL.push($event) } state.ui.newImageURLField = '' }" v-model="state.ui.newImageURLField"></textbox>
				</div>

				<div class="buttons_list shadowed">
					<textbox v-for="(url, index) in adventure.GalleryURL" v-bind:key="index" class="listed" type="text" :placeholder="'Gallery Image ' + (index + 1) + ' (Erase to remove)'" v-model="adventure.GalleryURL[index]" @changed="!$event.length ? adventure.GalleryURL.splice(index, 1) : '';"></textbox>
					<textbox class="listed" type="text" placeholder="+ Gallery Img URL (Hit return)" @returned="($event) => { if($event.length) { adventure.GalleryURL.push($event); } state.ui.newGalleryURLField = ''; }" v-model="state.ui.newGalleryURLField"></textbox>
				</div>

				<h3 class="helper_edge_margin_vertical_half">Descriptions</h3>
				<div class="buttons_list shadowed">
					<textbox v-for="(description, index) in adventure.Description" v-bind:key="index" class="listed" type="multiline" :placeholder="'Summary ' + (index + 1) + ' (Erase to remove)'" v-model="adventure.Description[index]" @changed="!$event.length ? adventure.Description.splice(index, 1) : '';"></textbox>
					<textbox :max="110" class="listed" type="multiline" placeholder="Add Summary (Hit return)" @returned="($event) => { if($event.length) { adventure.Description.push($event) } state.ui.newDescriptionField = ''; }" v-model="state.ui.newDescriptionField"></textbox>
				</div>

				<div class="buttons_list shadowed">
					<textbox v-for="(description, index) in adventure.DescriptionLong" v-bind:key="index" class="listed" type="multiline" :placeholder="'Description ' + (index + 1) + ' (Erase to remove)'" v-model="adventure.DescriptionLong[index]" @changed="!$event.length ? adventure.DescriptionLong.splice(index, 1) : '';"></textbox>
					<textbox class="listed" type="multiline" placeholder="Add Description (Hit return)" @returned="($event) => { if($event.length) { adventure.DescriptionLong.push($event) } state.ui.newDescriptionLongField = ''; }" v-model="state.ui.newDescriptionLongField"></textbox>
				</div>

				<div class="buttons_list shadowed">
					<textbox v-for="(link, index) in adventure.MediaLink" v-bind:key="index" class="listed" type="multiline" :placeholder="'Ref URL ' + (index + 1) + ' (Erase to remove)'" v-model="adventure.MediaLink[index]" @changed="!$event.length ? adventure.MediaLink.splice(index, 1) : '';"></textbox>
					<textbox class="listed" type="multiline" placeholder="Add Ref URL (Hit return)" @returned="($event) => { if($event.length) { adventure.MediaLink.push($event) } state.ui.newMediaLinkField = ''; }" v-model="state.ui.MediaLinkField"></textbox>
				</div>

				<h3 class="helper_edge_margin_vertical_half">Adventure-wide airports filters</h3>
				<div class="buttons_list shadowed">
					<textbox class="listed is-uppercase" type="multiline" placeholder="Include ICAOs (; sep)" v-model="adventure.IncludeICAO"></textbox>
					<textbox class="listed is-uppercase" type="multiline" placeholder="Exclude ICAOs (; sep)" v-model="adventure.ExcludeICAO"></textbox>
				</div>

				<div class="buttons_list shadowed">
					<textbox class="listed is-uppercase" type="multiline" placeholder="Include airport name (; sep)" v-model="adventure.IncludeAPTName"></textbox>
					<textbox class="listed is-uppercase" type="multiline" placeholder="Exclude airport name (; sep)" v-model="adventure.ExcludeAPTName"></textbox>
				</div>


				<h3 class="helper_edge_margin_vertical_half">Flight &amp; Situations</h3>
				<div class="buttons_list shadowed">
					<textbox class="listed" type="text" placeholder="UI Label" v-model="adventure.TypeLabel"></textbox>
				</div>

				<div class="buttons_list shadowed">
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Cargo') }]" @click.native="ToggleType('Cargo')">Cargo</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Pax') }]" @click.native="ToggleType('Pax')">Passengers</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Tour') }]" @click.native="ToggleType('Tour')">Tour</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Ferry') }]" @click.native="ToggleType('Ferry')">Ferry</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Bush Trip') }]" @click.native="ToggleType('Bush Trip')">Bush Trip</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Repo') }]" @click.native="ToggleType('Repo')">Repo</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Experience') }]" @click.native="ToggleType('Experience')">Experience</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Type.includes('Flight') }]" @click.native="ToggleType('Flight')">Flight</button_listed>
				</div>

				<div class="buttons_list shadowed">
					<button_listed :class="['naved', { 'naved-in': adventure.Company.includes('clearsky') }]" @click.native="ToggleCompany('clearsky')">ClearSky</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Company.includes('coyote') }]" @click.native="ToggleCompany('coyote')">Coyote</button_listed>
					<button_listed :class="['naved', { 'naved-in': adventure.Company.includes('skyparktravel') }]" @click.native="ToggleCompany('skyparktravel')">Skypark Travel</button_listed>



				</div>

				<div class="content-block">
					<toggle v-model="adventure.StrictOrder">Strict Order</toggle>
				</div>

				<div class="content-block">
					<AdventureSituations :app="app" :Adventure="adventure" />
				</div>

				<div class="buttons_list shadowed">
					<textbox class="listed" type="text" placeholder="Adventure Completed Notice" v-model="adventure.EndSummary"></textbox>
				</div>

				<h3 class="helper_edge_margin_vertical_half">Availability</h3>
				<div class="content-block">
					<toggle v-model="adventure.Unlisted">Unlisted</toggle>
				</div>
				<div class="content-block">
					<toggle v-model="adventure.DirectStart">Auto-Start on Request</toggle>
				</div>
				<div class="content-block">
					<toggle v-model="adventure.RunningClock">Running Countdown</toggle>
				</div>
				<div class="content-block">
					<div class="columns columns_2 columns_margined">
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Instances" v-model="adventure.Instances"></textbox>
						</div>
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Routes" v-model="adventure.RouteLimit"></textbox>
						</div>
						<!--
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Max / Day" v-model="adventure.MaxPerDay"></textbox>
						</div>
						-->
					</div>
				</div>
				<div class="content-block">
					<div class="columns columns_margined">
						<div class="column column_h-stretch">
							<textbox class="shadowed" type="number" v-if="adventure.RunningClock" placeholder="Min duration (hrs)" v-model="adventure.ExpireMin"></textbox>
							<textbox class="shadowed" type="number" v-else placeholder="Renew every X hrs" v-model="adventure.ExpireMax"></textbox>
						</div>
						<div class="column column_h-stretch">
							<textbox class="shadowed" type="number" v-if="adventure.RunningClock" placeholder="Max duration (hrs)" v-model="adventure.ExpireMax"></textbox>
						</div>
					</div>
				</div>
				<div class="buttons_list shadowed">
					<textbox class="listed" type="number" placeholder="Time to complete (hrs)" v-if="!adventure.RunningClock" v-model="adventure.TimeToComplete"></textbox>
				</div>
				<div class="buttons_list shadowed">
					<textbox class="listed" type="date" placeholder="Available From" v-model="adventure.Dates[0]"></textbox>
					<textbox class="listed" type="date" placeholder="To" v-model="adventure.Dates[1]"></textbox>
				</div>




				<h3 class="helper_edge_margin_vertical_half">Rewards</h3>
				<div class="content-block">
					<div class="columns columns_3 columns_margined">
						<div class="column column_3 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Base Pay" v-model="adventure.RewardBase"></textbox>
						</div>
						<div class="column column_3 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Pay /nm" v-model="adventure.RewardPerItem"></textbox>
						</div>
						<div class="column column_3 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Pay /Item" v-model="adventure.RewardPerNM"></textbox>
						</div>
					</div>
				</div>
				<div class="content-block">
					<div class="columns columns_2 columns_margined">
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="XP Shift" v-model="adventure.XPBase"></textbox>
						</div>
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Karma Shift" v-model="adventure.KarmaGain"></textbox>
						</div>
					</div>
				</div>
				<div class="content-block">
					<div class="columns columns_2 columns_margined">
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Reli. Shift Fail" v-model="adventure.RatingGainFail"></textbox>
						</div>
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Reli. Shift Succeed" v-model="adventure.RatingGainSucceed"></textbox>
						</div>
					</div>
				</div>

				<h3 class="helper_edge_margin_vertical_half">Fee Discounts</h3>
				<div class="content-block buttons_list shadowed" v-for="(Discount) in adventure.DiscountFees" v-bind:key="adventure.DiscountFees.indexOf(Discount)">
					<div class="columns">
						<div class="column column_h-stretch">
							<selector placeholder="Fee Discount" v-model="Discount.code" @modified="$event == null ? adventure.DiscountFees.splice(adventure.DiscountFees.indexOf(Discount), 1) : false">
								<option :value="null">- Remove</option>
								<option :value="'%userrelocation%'">Relocation</option>
								<option :value="'%bobservice%'">Bob's Aero Service</option>
							</selector>
						</div>
						<div class="column column_h-stretch">
							<selector placeholder="Moment" v-model="Discount.moment">
								<option :value="'START'">Start</option>
								<option :value="'END'">End</option>
								<option :value="'SUCCEED'">Success</option>
								<option :value="'FAIL'">Fail</option>
							</selector>
						</div>
						<div class="column column_h-stretch">
							<textbox type="number" placeholder="% Discount" v-model="Discount.discount" :min="0" :max="100"></textbox>
						</div>
					</div>
				</div>
				<div class="content-block">
					<button_action @click.native="adventure.DiscountFees.push({ code: '%userrelocation%', moment: 'SUCCEED', discount: 100 })" class="info">+</button_action>
				</div>

				<h3 class="helper_edge_margin_vertical_half">Aircraft Requirements</h3>
				<div class="buttons_list shadowed">
					<textbox class="listed" type="text" placeholder="SimObjects folder to match (; sep)" v-model="adventure.AircraftRestriction"></textbox>
					<textbox class="listed" type="text" placeholder="Label shown in the UI" v-model="adventure.AircraftRestrictionLabel"></textbox>
				</div>

				<h3 class="helper_edge_margin_vertical_half">Ranking Requirements</h3>
				<div class="content-block">
					<div class="columns columns_2 columns_margined">
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Min Level" v-model="adventure.LvlMin"></textbox>
						</div>
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Max Level" v-model="adventure.LvlMax"></textbox>
						</div>
					</div>
				</div>
				<div class="content-block">
					<div class="columns columns_2 columns_margined">
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Min Reli." v-model="adventure.RatingMin"></textbox>
						</div>
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Max Reli." v-model="adventure.RatingMax"></textbox>
						</div>
					</div>
				</div>
				<div class="content-block">
					<div class="columns columns_2 columns_margined">
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Min Karma" v-model="adventure.KarmaMin"></textbox>
						</div>
						<div class="column column_2 column_h-stretch">
							<textbox class="shadowed" type="number" placeholder="Max Karma" v-model="adventure.KarmaMax"></textbox>
						</div>
					</div>
				</div>


				<h3 class="helper_edge_margin_vertical_half">POIs</h3>
				<div class="content-block" v-if="adventure.POIs.length > 3">
					<div class="columns columns_margined">
						<div class="column column_h-stretch">
							<textbox class="shadowed" type="text" placeholder="Search POIs" v-model="state.ui.poiQuery"></textbox>
						</div>
					</div>
				</div>
				<div class="content-block buttons_list shadowed" v-for="(POI) in adventure.POIs.filter(x => state.ui.poiQuery == '' ? true : x[2].toLowerCase().includes(state.ui.poiQuery.toLowerCase()))" v-bind:key="adventure.POIs.indexOf(POI)">
					<div class="columns" v-if="!state.ui.poiReload">
						<div class="column column_h-stretch column_narrow">
							<button_action v-if="state.ui.pinSetName == 'poi' && state.ui.pinSetIndex==adventure.POIs.indexOf(POI)" @click.native="state.ui.pinSetName=null" class="cancel">Cancel</button_action>
							<button_action v-else @click.native="state.ui.pinSetName='poi';state.ui.pinSetIndex=adventure.POIs.indexOf(POI)" class="info">Set</button_action>
						</div>
						<div class="column column_h-stretch">
							<textbox class="listed_h" type="number" placeholder="Lat" v-model="POI[1]" @returned="state.ui.pinSetName=null;DrawPOIs()"></textbox>
						</div>
						<div class="column column_h-stretch">
							<textbox class="listed_h" type="number" placeholder="Lon" v-model="POI[0]" @returned="state.ui.pinSetName=null;DrawPOIs()"></textbox>
						</div>
						<div class="column column_h-stretch">
							<textbox class="listed_h" type="number" placeholder="Range NM" v-model="POI[3]"></textbox>
						</div>
					</div>
					<div class="columns">
						<div class="column column_h-stretch">
							<textbox type="text" placeholder="Name" v-model="POI[2]" @returned="PlaceSearch(POI); DrawPOIs()"></textbox>
						</div>
						<div class="column column_h-stretch column_narrow">
							<button_action @click.native="adventure.POIs.splice(adventure.POIs.indexOf(POI), 1);DrawPOIs();state.ui.pinSetName=null" class="cancel">Delete</button_action>
						</div>
					</div>
				</div>
				<div class="content-block">
					<button_action @click.native="adventure.POIs.push([0,0,'',0]);DrawPOIs();state.ui.pinSetName='poi';state.ui.pinSetIndex=adventure.POIs.length-1" class="info">+</button_action>
				</div>


				<br>
				<br>
				<br>
				<br>
				<br>
				<br>
				<br>

			</div>
		</template>
	</content_controls_stack>
</template>

<script lang="ts">
import Vue from 'vue';
import AdventureProj from './../../classes/adventure';
import ContractSummary from "@/sys/components/contracts/contract_summary.vue"

export default Vue.extend({
	name: "p42_scenr_startup",
	components: {
		ContractSummary,
		AdventureSituations: () => import('./situations.vue'),
	},
	props: {
		app :Vue,
		adventure :AdventureProj
	},
	methods: {
		CloseProject(save :boolean) {
			if(save) {
				this.Save(() => {
					this.app.$emit('interaction', { cmd: 'adventure:close' });
					//this.$root.$data.services.api.SendWS('scenr:adventuretemplate:test', { File: this.adventure.File, State: false }, (confirm: any) => {});
				});
			} else {
				this.app.$emit('interaction', { cmd: 'adventure:close' });
				//this.$root.$data.services.api.SendWS('scenr:adventuretemplate:test', { File: this.adventure.File, State: false }, (confirm: any) => {});
			}
		},
		Rename(newVal :string, oldVal :string) {
			if(this.state.ui.renameFrom == '') {
				this.state.ui.renameFrom = oldVal;
			}
		},
		RegenContracts() {
			this.state.ui.generating = true;
			this.state.ui.sampleContract = null;
			this.state.ui.sampleTemplate = null;
			this.Save(() => {
				this.$root.$data.services.api.SendWS('scenr:adventuretemplate:regen', { File: this.adventure.File, State: true }, (confirm: any) => {
					if(confirm.payload.success) {
						this.state.ui.generating = false;
						this.GetSample();
					}
				}, 1);
			});
		},
		RegenRoutes() {
			this.state.ui.generating = true;
			this.state.ui.sampleContract = null;
			this.state.ui.sampleTemplate = null;
			this.adventure.ModifiedOn = new Date();

			this.state.ui.generating = true;
			this.app.$data.state.ui.map.sources.routes.data.features = [];

			this.Save(() => {
				this.$root.$data.services.api.SendWS('scenr:adventuretemplate:test', { File: this.adventure.File, State: true }, (confirm: any) => {
					if(confirm.meta.callbackType == 0) {
						this.state.ui.generating = false;
					}
				}, 2);
			});

		},
		ToggleType(t :string) {
			if(this.adventure.Type.includes(t)) {
				this.adventure.Type.splice(this.adventure.Type.indexOf(t), 1);
			} else {
				this.adventure.Type.push(t);
			}
		},
		ToggleCompany(t :string) {
			if(this.adventure.Company.includes(t)) {
				this.adventure.Company.splice(this.adventure.Company.indexOf(t), 1);
			} else {
				this.adventure.Company.push(t);
			}
		},
		ToggleTier(t :string) {
			if(this.adventure.Tiers.includes(t)) {
				this.adventure.Tiers.splice(this.adventure.Tiers.indexOf(t), 1);
			} else {
				this.adventure.Tiers.push(t);
			}
		},
		GetSample() {
			this.state.ui.waitingSample = true;
			this.$root.$data.services.api.SendWS(
				'adventures:query-from-template',
				{
					file: this.adventure.File,
					detailed: true
				},
				(contractsData: any) => {
					if(contractsData.payload.Contracts.length) {

						this.state.ui.sampleContract = contractsData.payload.Contracts[0];
						this.state.ui.sampleTemplate = contractsData.payload.Templates[0];

						//this.state.ui.sampleTemplate.Name = this.adventure.Name;

						//if(this.adventure.ImageURL.length) {
						//	this.state.ui.sampleContract.ImageURL = this.adventure.ImageURL[Math.round(Math.random() * (this.adventure.ImageURL.length - 1))];
						//} else {
						//	this.state.ui.sampleContract.ImageURL = '';
						//}

					}
					this.state.ui.waitingSample = false;
					this.app.$emit('interaction', { cmd: 'adventure:contract', contract: this.state.ui.sampleContract, template: this.state.ui.sampleTemplate })
				}
			);
		},
		DrawPOIs() {
			this.app.$emit('interaction', { cmd: 'map:pois', list: this.adventure.POIs })
		},
		OpenSample() {
			this.app.$emit('interaction', { cmd: 'adventure:modal' })
		},
		Save(callback :Function = null) {
			this.state.ui.sampleContract = null;
			this.state.ui.sampleTemplate = null;
			this.state.ui.saveAvailable = false;
			this.state.ui.generating = true;
			this.adventure.ValidateGhosts();
			this.$root.$data.services.api.SendWS('scenr:adventuretemplate:save', this.adventure, (confirm: any) => {
				this.state.ui.generating = false;
				if(confirm.payload.success) {
					if(this.state.ui.renameFrom != '' && this.state.ui.renameFrom.replace('.p42adv', '') != this.adventure.File.replace('.p42adv', '')) {
						this.$root.$data.services.api.SendWS('scenr:adventuretemplate:delete', { File: this.state.ui.renameFrom }, (confirm: any) => {});
					}
					this.state.ui.renameFrom = '';
					if(callback != null) {
						callback();
					}
				}
				this.state.ui.saveAvailable = true;
			}, 1);
		},
		Export() {
			let FileName = 'Adv_' + this.adventure.Name.replace(/\W/g, '') + '.p42adv';
			if (this.adventure.File !== '') {
				FileName = this.adventure.File;
			}
			//const d = new Date();
			const dataStr = 'data:text/json;charset=utf-8,' + encodeURIComponent(JSON.stringify(this.adventure));
			const dlAnchorElem = document.createElement('a');
			dlAnchorElem.setAttribute('href', dataStr);
			dlAnchorElem.setAttribute('download', FileName);
			document.body.appendChild(dlAnchorElem);
			dlAnchorElem.click();
			dlAnchorElem.remove();
		},
		PlaceSearch(poi: any) {
			const encoded = encodeURIComponent("https://maps.googleapis.com/maps/api/place/findplacefromtext/json?key=AIzaSyCbKw7PTMwdixCCGh1zPW91HKQqHzlOjzo&inputtype=textquery&fields=name,geometry&input=" + poi[2].replaceAll(' ', '+'));
			//const url = "https://parallel42.com/proxy.php?path=" + encoded;
			const url = "";
			this.state.ui.pinSetName = null;

			fetch(url).then(res => res.json()).then((out) => {
				console.log(out);
				if(out.status) {
					switch(out.status) {
						case "OK": {
							if(out.candidates.length) {
								console.log(poi);
								this.$set(poi, 1, out.candidates[0].geometry.location.lat);
								this.$set(poi, 0, out.candidates[0].geometry.location.lng);
								this.DrawPOIs();
								this.app.$emit('interaction', { cmd: 'map:move', location: [out.candidates[0].geometry.location.lng, out.candidates[0].geometry.location.lat] })
							}
							break;
						}
						case "ZERO_RESULTS": {
							break;
						}
					}
				}
			})
			.catch(err => { console.log(err) });
		},

		MapClick(ev :any) {
			if(this.state.ui.pinSetName) {
				switch(this.state.ui.pinSetName) {
					case 'poi': {
						this.adventure.POIs[this.state.ui.pinSetIndex][0] = ev.lngLat.lng;
						this.adventure.POIs[this.state.ui.pinSetIndex][1] = ev.lngLat.lat;
						this.DrawPOIs();
						break;
					}
				}
				this.state.ui.pinSetName = null;
			}
		},

		listenerWs(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'disconnect': {
					this.state.ui.waitingSample = false;
					this.state.ui.saveAvailable = false;
					break;
				}
				case 'connect': {
					this.state.ui.waitingSample = false;
					this.state.ui.saveAvailable = true;
					break;
				}
				case 'adventure': {
					if(this.state.ui.sampleContract) {
						this.$ContractMutator.Event(wsmsg, this.state.ui.sampleContract.Contract, this.state.ui.sampleTemplate);
					}
					break;
				}
			}
		}
	},
	mounted() {
		this.GetSample();
		this.DrawPOIs();
	},
	data() {
		return {
			state: {
				ui: {
					poiReload: false,
					generating: false,
					saveAvailable: this.$root.$data.state.services.api.connected,
					renameFrom: '',
					poiQuery: '',
					newGalleryURLField: '',
					newImageURLField: '',
					newDescriptionField: '',
					newDescriptionLongField: '',
					newMediaLinkField: '',
					waitingSample: false,
					pinSetName: null,
					pinSetIndex: 0,
					sampleContract: null,
					sampleTemplate: null,
				}
			}
		}
	},
	watch: {
		'$root.$data.state.services.api.connected'() {
			if(!this.$root.$data.state.services.api.connected) {
				this.state.ui.generating = false;
			}
		},
		'adventure.Name'() {
			if(this.state.ui.sampleTemplate) {
				this.state.ui.sampleTemplate.Name = this.adventure.Name;
			}
		},
		'adventure.ImageURL'() {
			if(this.adventure.ImageURL.length) {
				this.state.ui.sampleContract.ImageURL = this.adventure.ImageURL[Math.round(Math.random() * (this.adventure.ImageURL.length - 1))];
			} else {
				this.state.ui.sampleContract.ImageURL = '';
			}
		}
	},
	created() {
		this.$root.$on('ws-in', this.listenerWs);
		this.app.$on('map:click', this.MapClick);
	},
	beforeDestroy() {
		this.$root.$off('ws-in', this.listenerWs);
		this.app.$off('map:click', this.MapClick);
	},
});
</script>

<style lang="scss">
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';
.p42_scenr_adventure {
	.image-sample {
		height: 170px;
		background-size: cover;
		background-position: center;
		background-repeat: no-repeat;
	}
}
</style>