<template>
	<div class="adventure_situation">
		<button_action class="info adventure_situation_collapsed" :class="{ 'expanded ': IsExpanded }" @click.native="IsExpanded = !IsExpanded">
			<span>{{ Situation.Index + 1 }} — </span>
			<span v-if="Situation.SituationType == 'Any'">Any airport</span>
			<span v-if="Situation.SituationType == 'ICAO'">ICAO: {{ Situation.ICAO }}</span>
			<span v-if="Situation.SituationType == 'Country'">Country: {{ Situation.Country }}</span>
			<span v-if="Situation.SituationType == 'Geo'">Location: {{ Situation.Lon + ", " + Situation.Lat }}</span>
			<span v-if="Situation.Actions.length > 0"> — {{ Situation.Actions.length }}</span>
		</button_action>
		<div class="adventure_situation_card" v-if="IsExpanded">
			<!-- Basic Controls -->
			<div class="content-block_half">
				<div class="columns columns_margined_half adventure_situation_controls">
					<div class="column">
						<selector placeholder="Situation Type" v-model="Situation.SituationType" @modified="QueryAirports">
							<option value="Any">Any Airport</option>
							<option value="Country">Any Airport in Country</option>
							<option value="ICAO">ICAO</option>
							<option value="Geo">Longitude / Latitude</option>
						</selector>
					</div>
					<div class="column column_narrow" v-if="Situation.Actions.length == 0">
						<button_action class="go" @click.native="CopySituation(Situation)">C</button_action>
					</div>
					<div class="column column_narrow" v-if="Situation.Actions.length == 0">
						<button_action class="cancel" @click.native="RemoveSituation(Situation)">X</button_action>
					</div>
				</div>
			</div>

			<div class="content-block_half" v-if="Situation.SituationType == 'Any' || Situation.SituationType == 'Country'">
				<button_action class="" v-if="app.$data.state.ui.mapControlMode.reference != Situation" @click.native="DrawOnMap(true)">Draw on Map</button_action>
				<button_action class="cancel" v-else @click.native="DrawOnMap(false)">Done</button_action>
			</div>

			<div class="content-block_half" v-if="Situation.SituationType == 'Country'">
				<selector class="listed" placeholder="Country" v-model="Situation.Country" @modified="QueryAirports">
					<option
						v-for="Country in Countries"
						:key="Country.code"
						:value="Country.code"
					>{{ Country.code + ' - ' + Country.name }}</option>
				</selector>
			</div>

			<div class="content-block_half" v-if="Situation.SituationType == 'ICAO'">
				<textbox type="text" placeholder="ICAO" :isUppercase="true" v-model="Situation.ICAO" @changed="QueryAirports"></textbox>
			</div>

			<div class="content-block_half" v-if="Situation.SituationType == 'Geo'">
				<div class="columns columns_margined_half adventure_situation_controls">
					<div class="column">
						<textbox class="listed" placeholder="Latitude" type="number" :min="-90" :max="90" v-model="Situation.Lat"></textbox>
					</div>
					<div class="column">
						<textbox class="listed" placeholder="Longitude" type="number" :min="-180" :max="180" v-model="Situation.Lon"></textbox>
					</div>
					<div class="column column_narrow">
						<button_action v-if="pinSetName == 'loc'" @click.native="pinSetName=null" class="cancel">Cancel</button_action>
						<button_action v-else @click.native="pinSetName='loc'">Set</button_action>
					</div>
				</div>
			</div>

			<div class="content-block_half" v-if="Situation.SituationType == 'Geo'">
				<div class="columns columns_margined_half adventure_situation_controls">
					<div class="column">
						<textbox placeholder="Situation Description" type="text" v-model="Situation.Label"></textbox>
					</div>
				</div>
			</div>

			<div class="content-block_half" v-if="Situation.SituationType == 'Geo'">
				<div class="columns columns_margined_half adventure_situation_controls">
					<div class="column">
						<textbox placeholder="nm Trigger Range" type="number" v-model="Situation.TriggerRange" :min="0.25" :max="10" :step="0.25" @changed="RefreshSituations"></textbox>
					</div>
				</div>
			</div>


			<div class="content-block_half" v-if="Situation.SituationType == 'Any' || Situation.SituationType == 'Country'">

				<div class="content-block_half">
					<selector placeholder="Surface" v-model="Situation.Surface" @input="QueryAirports">
						<option value="Any">Any (except water)</option>
						<option value="Hard">Hard</option>
						<option value="Soft">Soft</option>
						<option value="Dirt">Dirt</option>
						<option value="Grass">Grass</option>
						<option value="Water">Water</option>
					</selector>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<textbox placeholder="Helipads Min" type="number" v-model="Situation.HeliMin" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
						<div class="column">
							<textbox placeholder="Helipads Max" type="number" v-model="Situation.HeliMax" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
					</div>
				</div>

				<div class="content-block_half">
					<textbox placeholder="Airport Name Query (; sep)" type="multiline" v-model="Situation.Query" @changed="QueryAirports"></textbox>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<textbox placeholder="Rwys Min" type="number" v-model="Situation.RwyMin" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
						<div class="column">
							<textbox placeholder="Rwys Max" type="number" v-model="Situation.RwyMax" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
					</div>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<textbox placeholder="Parkings Min" type="number" v-model="Situation.ParkMin" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
						<div class="column">
							<textbox placeholder="Parkings Max" type="number" v-model="Situation.ParkMax" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
					</div>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<textbox placeholder="Park Wid Min (ft)" type="number" v-model="Situation.ParkWidMin" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
						<div class="column">
							<textbox placeholder="Park Wid Max (ft)" type="number" v-model="Situation.ParkWidMax" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
					</div>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<textbox placeholder="Elev Min (ft)" type="number" v-model="Situation.ElevMin" :min="-2000" :step="100" @changed="QueryAirports"></textbox>
						</div>
						<div class="column">
							<textbox placeholder="Elev Max (ft)" type="number" v-model="Situation.ElevMax" :min="-2000" :step="100" @changed="QueryAirports"></textbox>
						</div>
					</div>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<textbox placeholder="Rwy Len. Min (ft)" type="number" v-model="Situation.RwyLenMin" :min="0" :step="100" @changed="QueryAirports"></textbox>
						</div>
						<div class="column">
							<textbox placeholder="Rwy Len. Max (ft)" type="number" v-model="Situation.RwyLenMax" :min="0" :step="100" @changed="QueryAirports"></textbox>
						</div>
					</div>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<textbox placeholder="Rwy Wid. Min (ft)" type="number" v-model="Situation.RwyWidMin" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
						<div class="column">
							<textbox placeholder="Rwy Wid. Max (ft)" type="number" v-model="Situation.RwyWidMax" :min="0" :step="1" @changed="QueryAirports"></textbox>
						</div>
					</div>
				</div>

				<div class="content-block_half">
					<div class="columns columns_margined_half">
						<div class="column">
							<toggle v-model="Situation.RequireLights" @modified="QueryAirports">Require runway lighting</toggle>
						</div>
					</div>
				</div>

			</div>


			<div class="content-block_half">
				<div class="columns columns_margined_half">
					<div class="column column_h-center">
						<textbox placeholder="ft AGL Height" type="number" v-model="Situation.Height" :min="0" :max="100000" :step="100"></textbox>
					</div>
					<div class="column">
						<toggle v-model="Situation.Visible">Visible</toggle>
					</div>
				</div>
			</div>

			<adventureactions
				placeholder="Add event: Within this situation"
				styleStr="is-dark"
				:app="app"
				:Exclusions="[]"
				:SituationActions="Situation.Actions"
				:Situation="Situation"
				:Adventure="Adventure"
			></adventureactions>
		</div>

		<div class="adventure_situation_tonext" v-if="NextIsUnknown !== null">
			<div class="separator"></div>
			<div class="content-block_half">
				<div class="columns columns_margined_half" v-if="NextIsUnknown">
					<div class="column">
						<textbox class="shadowed" placeholder="Min dist (nm)" type="number" v-model="Situation.DistToNextMin" :min="0" :max="100000" :step="1"></textbox>
					</div>
					<div class="column">
						<textbox class="shadowed" placeholder="Max dist (nm)" type="number" v-model="Situation.DistToNextMax" :min="0" :max="100000" :step="1"></textbox>
					</div>
				</div>
			</div>
			<div class="content-block_half">
				<button_action class="" @click.native="AddSituationAfter">Insert Situation</button_action>
			</div>
			<div class="separator"></div>
		</div>
	</div>
</template>

<script lang="ts">
import Vue from "vue";
import AdventureProjectSituation from './../../interfaces/adventure/situation';

export default Vue.extend({
	name: "scenr_adventure_situation",
	props: ["app", "Adventure", "Situation"],
	components: {
		adventureactions: () => import("./actions.vue")
	},
	data() {
		return {
			IsExpanded: true,
			NextIsUnknown: null,
			QueryAirportTimeout: null as any,
			pinSetName: null,
			Countries: [
				{ name: "Afghanistan", code: "AF" },
				{ name: "Åland Islands", code: "AX" },
				{ name: "Albania", code: "AL" },
				{ name: "Algeria", code: "DZ" },
				{ name: "American Samoa", code: "AS" },
				{ name: "AndorrA", code: "AD" },
				{ name: "Angola", code: "AO" },
				{ name: "Anguilla", code: "AI" },
				{ name: "Antarctica", code: "AQ" },
				{ name: "Antigua and Barbuda", code: "AG" },
				{ name: "Argentina", code: "AR" },
				{ name: "Armenia", code: "AM" },
				{ name: "Aruba", code: "AW" },
				{ name: "Australia", code: "AU" },
				{ name: "Austria", code: "AT" },
				{ name: "Azerbaijan", code: "AZ" },
				{ name: "Bahamas", code: "BS" },
				{ name: "Bahrain", code: "BH" },
				{ name: "Bangladesh", code: "BD" },
				{ name: "Barbados", code: "BB" },
				{ name: "Belarus", code: "BY" },
				{ name: "Belgium", code: "BE" },
				{ name: "Belize", code: "BZ" },
				{ name: "Benin", code: "BJ" },
				{ name: "Bermuda", code: "BM" },
				{ name: "Bhutan", code: "BT" },
				{ name: "Bolivia", code: "BO" },
				{ name: "Bosnia and Herzegovina", code: "BA" },
				{ name: "Botswana", code: "BW" },
				{ name: "Bouvet Island", code: "BV" },
				{ name: "Brazil", code: "BR" },
				{ name: "British Indian Ocean Territory", code: "IO" },
				{ name: "Brunei Darussalam", code: "BN" },
				{ name: "Bulgaria", code: "BG" },
				{ name: "Burkina Faso", code: "BF" },
				{ name: "Burundi", code: "BI" },
				{ name: "Cambodia", code: "KH" },
				{ name: "Cameroon", code: "CM" },
				{ name: "Canada", code: "CA" },
				{ name: "Cape Verde", code: "CV" },
				{ name: "Cayman Islands", code: "KY" },
				{ name: "Central African Republic", code: "CF" },
				{ name: "Chad", code: "TD" },
				{ name: "Chile", code: "CL" },
				{ name: "China", code: "CN" },
				{ name: "Christmas Island", code: "CX" },
				{ name: "Cocos (Keeling) Islands", code: "CC" },
				{ name: "Colombia", code: "CO" },
				{ name: "Comoros", code: "KM" },
				{ name: "Congo", code: "CG" },
				{ name: "Congo, The Democratic Republic of the", code: "CD" },
				{ name: "Cook Islands", code: "CK" },
				{ name: "Costa Rica", code: "CR" },
				{ name: "Cote D'Ivoire", code: "CI" },
				{ name: "Croatia", code: "HR" },
				{ name: "Cuba", code: "CU" },
				{ name: "Cyprus", code: "CY" },
				{ name: "Czech Republic", code: "CZ" },
				{ name: "Denmark", code: "DK" },
				{ name: "Djibouti", code: "DJ" },
				{ name: "Dominica", code: "DM" },
				{ name: "Dominican Republic", code: "DO" },
				{ name: "Ecuador", code: "EC" },
				{ name: "Egypt", code: "EG" },
				{ name: "El Salvador", code: "SV" },
				{ name: "Equatorial Guinea", code: "GQ" },
				{ name: "Eritrea", code: "ER" },
				{ name: "Estonia", code: "EE" },
				{ name: "Ethiopia", code: "ET" },
				{ name: "Falkland Islands (Malvinas)", code: "FK" },
				{ name: "Faroe Islands", code: "FO" },
				{ name: "Fiji", code: "FJ" },
				{ name: "Finland", code: "FI" },
				{ name: "France", code: "FR" },
				{ name: "French Guiana", code: "GF" },
				{ name: "French Polynesia", code: "PF" },
				{ name: "French Southern Territories", code: "TF" },
				{ name: "Gabon", code: "GA" },
				{ name: "Gambia", code: "GM" },
				{ name: "Georgia", code: "GE" },
				{ name: "Germany", code: "DE" },
				{ name: "Ghana", code: "GH" },
				{ name: "Gibraltar", code: "GI" },
				{ name: "Greece", code: "GR" },
				{ name: "Greenland", code: "GL" },
				{ name: "Grenada", code: "GD" },
				{ name: "Guadeloupe", code: "GP" },
				{ name: "Guam", code: "GU" },
				{ name: "Guatemala", code: "GT" },
				{ name: "Guernsey", code: "GG" },
				{ name: "Guinea", code: "GN" },
				{ name: "Guinea-Bissau", code: "GW" },
				{ name: "Guyana", code: "GY" },
				{ name: "Haiti", code: "HT" },
				{ name: "Heard Island and Mcdonald Islands", code: "HM" },
				{ name: "Holy See (Vatican City State)", code: "VA" },
				{ name: "Honduras", code: "HN" },
				{ name: "Hong Kong", code: "HK" },
				{ name: "Hungary", code: "HU" },
				{ name: "Iceland", code: "IS" },
				{ name: "India", code: "IN" },
				{ name: "Indonesia", code: "ID" },
				{ name: "Iran, Islamic Republic Of", code: "IR" },
				{ name: "Iraq", code: "IQ" },
				{ name: "Ireland", code: "IE" },
				{ name: "Isle of Man", code: "IM" },
				{ name: "Israel", code: "IL" },
				{ name: "Italy", code: "IT" },
				{ name: "Jamaica", code: "JM" },
				{ name: "Japan", code: "JP" },
				{ name: "Jersey", code: "JE" },
				{ name: "Jordan", code: "JO" },
				{ name: "Kazakhstan", code: "KZ" },
				{ name: "Kenya", code: "KE" },
				{ name: "Kiribati", code: "KI" },
				{ name: "Korea, Democratic People'S Republic of", code: "KP" },
				{ name: "Korea, Republic of", code: "KR" },
				{ name: "Kuwait", code: "KW" },
				{ name: "Kyrgyzstan", code: "KG" },
				{ name: "Lao People'S Democratic Republic", code: "LA" },
				{ name: "Latvia", code: "LV" },
				{ name: "Lebanon", code: "LB" },
				{ name: "Lesotho", code: "LS" },
				{ name: "Liberia", code: "LR" },
				{ name: "Libyan Arab Jamahiriya", code: "LY" },
				{ name: "Liechtenstein", code: "LI" },
				{ name: "Lithuania", code: "LT" },
				{ name: "Luxembourg", code: "LU" },
				{ name: "Macao", code: "MO" },
				{
					name: "Macedonia, The Former Yugoslav Republic of",
					code: "MK"
				},
				{ name: "Madagascar", code: "MG" },
				{ name: "Malawi", code: "MW" },
				{ name: "Malaysia", code: "MY" },
				{ name: "Maldives", code: "MV" },
				{ name: "Mali", code: "ML" },
				{ name: "Malta", code: "MT" },
				{ name: "Marshall Islands", code: "MH" },
				{ name: "Martinique", code: "MQ" },
				{ name: "Mauritania", code: "MR" },
				{ name: "Mauritius", code: "MU" },
				{ name: "Mayotte", code: "YT" },
				{ name: "Mexico", code: "MX" },
				{ name: "Micronesia, Federated States of", code: "FM" },
				{ name: "Moldova, Republic of", code: "MD" },
				{ name: "Monaco", code: "MC" },
				{ name: "Mongolia", code: "MN" },
				{ name: "Montserrat", code: "MS" },
				{ name: "Morocco", code: "MA" },
				{ name: "Mozambique", code: "MZ" },
				{ name: "Myanmar", code: "MM" },
				{ name: "Namibia", code: "NA" },
				{ name: "Nauru", code: "NR" },
				{ name: "Nepal", code: "NP" },
				{ name: "Netherlands", code: "NL" },
				{ name: "Netherlands Antilles", code: "AN" },
				{ name: "New Caledonia", code: "NC" },
				{ name: "New Zealand", code: "NZ" },
				{ name: "Nicaragua", code: "NI" },
				{ name: "Niger", code: "NE" },
				{ name: "Nigeria", code: "NG" },
				{ name: "Niue", code: "NU" },
				{ name: "Norfolk Island", code: "NF" },
				{ name: "Northern Mariana Islands", code: "MP" },
				{ name: "Norway", code: "NO" },
				{ name: "Oman", code: "OM" },
				{ name: "Pakistan", code: "PK" },
				{ name: "Palau", code: "PW" },
				{ name: "Palestinian Territory, Occupied", code: "PS" },
				{ name: "Panama", code: "PA" },
				{ name: "Papua New Guinea", code: "PG" },
				{ name: "Paraguay", code: "PY" },
				{ name: "Peru", code: "PE" },
				{ name: "Philippines", code: "PH" },
				{ name: "Pitcairn", code: "PN" },
				{ name: "Poland", code: "PL" },
				{ name: "Portugal", code: "PT" },
				{ name: "Puerto Rico", code: "PR" },
				{ name: "Qatar", code: "QA" },
				{ name: "Reunion", code: "RE" },
				{ name: "Romania", code: "RO" },
				{ name: "Russian Federation", code: "RU" },
				{ name: "RWANDA", code: "RW" },
				{ name: "Saint Helena", code: "SH" },
				{ name: "Saint Kitts and Nevis", code: "KN" },
				{ name: "Saint Lucia", code: "LC" },
				{ name: "Saint Pierre and Miquelon", code: "PM" },
				{ name: "Saint Vincent and the Grenadines", code: "VC" },
				{ name: "Samoa", code: "WS" },
				{ name: "San Marino", code: "SM" },
				{ name: "Sao Tome and Principe", code: "ST" },
				{ name: "Saudi Arabia", code: "SA" },
				{ name: "Senegal", code: "SN" },
				{ name: "Serbia and Montenegro", code: "CS" },
				{ name: "Seychelles", code: "SC" },
				{ name: "Sierra Leone", code: "SL" },
				{ name: "Singapore", code: "SG" },
				{ name: "Slovakia", code: "SK" },
				{ name: "Slovenia", code: "SI" },
				{ name: "Solomon Islands", code: "SB" },
				{ name: "Somalia", code: "SO" },
				{ name: "South Africa", code: "ZA" },
				{
					name: "South Georgia and the South Sandwich Islands",
					code: "GS"
				},
				{ name: "Spain", code: "ES" },
				{ name: "Sri Lanka", code: "LK" },
				{ name: "Sudan", code: "SD" },
				{ name: "Suriname", code: "SR" },
				{ name: "Svalbard and Jan Mayen", code: "SJ" },
				{ name: "Swaziland", code: "SZ" },
				{ name: "Sweden", code: "SE" },
				{ name: "Switzerland", code: "CH" },
				{ name: "Syrian Arab Republic", code: "SY" },
				{ name: "Taiwan, Province of China", code: "TW" },
				{ name: "Tajikistan", code: "TJ" },
				{ name: "Tanzania, United Republic of", code: "TZ" },
				{ name: "Thailand", code: "TH" },
				{ name: "Timor-Leste", code: "TL" },
				{ name: "Togo", code: "TG" },
				{ name: "Tokelau", code: "TK" },
				{ name: "Tonga", code: "TO" },
				{ name: "Trinidad and Tobago", code: "TT" },
				{ name: "Tunisia", code: "TN" },
				{ name: "Turkey", code: "TR" },
				{ name: "Turkmenistan", code: "TM" },
				{ name: "Turks and Caicos Islands", code: "TC" },
				{ name: "Tuvalu", code: "TV" },
				{ name: "Uganda", code: "UG" },
				{ name: "Ukraine", code: "UA" },
				{ name: "United Arab Emirates", code: "AE" },
				{ name: "United Kingdom", code: "GB" },
				{ name: "United States", code: "US" },
				{ name: "United States Minor Outlying Islands", code: "UM" },
				{ name: "Uruguay", code: "UY" },
				{ name: "Uzbekistan", code: "UZ" },
				{ name: "Vanuatu", code: "VU" },
				{ name: "Venezuela", code: "VE" },
				{ name: "Viet Nam", code: "VN" },
				{ name: "Virgin Islands, British", code: "VG" },
				{ name: "Virgin Islands, U.S.", code: "VI" },
				{ name: "Wallis and Futuna", code: "WF" },
				{ name: "Western Sahara", code: "EH" },
				{ name: "Yemen", code: "YE" },
				{ name: "Zambia", code: "ZM" },
				{ name: "Zimbabwe", code: "ZW" }
			],

		};
	},
	methods: {
		AddSituationAfter() {
			this.DrawOnMap(false);
			this.$emit("AddSituationAfter", this.Situation);
		},
		CopySituation() {
			this.DrawOnMap(false);
			this.$emit("CopySituation", this.Situation);
			this.QueryAirports();
		},
		RemoveSituation() {
			this.DrawOnMap(false);
			this.$emit("RemoveSituation", this.Situation);
			this.QueryAirports();
		},
		RefreshSituations() {
			this.app.$emit('interaction', { cmd: 'map:situations'});
		},
		DrawOnMap(state :boolean) {
			if(state){
				this.app.$emit('interaction', { cmd: 'map:draw', state: true, type: "situation_bounds", sit: this.Situation });
			} else {
				this.app.$emit('interaction', { cmd: 'map:draw', state: false});
				this.QueryAirports();
			}
		},
		QueryAirports() {
			clearTimeout(this.QueryAirportTimeout);
			this.QueryAirportTimeout = setTimeout(() => {
				this.app.$emit('interaction', { cmd: 'map:airports:query' });
			}, 200);
		},
		CheckIfNextIsUnknown() {
			const index = this.Adventure.Situations.findIndex(
				(x: AdventureProjectSituation) => x.UID === this.Situation.UID
			);

			if (this.Adventure.Situations.length - 1 > index) {
				const type = this.Adventure.Situations[index + 1].SituationType;
				switch (type) {
					case "Any":
					case "Country": {
						this.NextIsUnknown = true;
						break;
					}
					default: {
						switch (this.Situation.SituationType) {
							case "Any":
							case "Country": {
								this.NextIsUnknown = true;
								break;
							}
							default: {
								this.NextIsUnknown = false;
								break;
							}
						}
						break;
					}
				}
			} else {
				this.NextIsUnknown = null;
			}
		},
		SetStruct() {
			const self = this;

			this.Situation.DistToNextMin = 10;
			this.Situation.DistToNextMax = 800;

			switch (this.Situation.SituationType) {
				case "Any":
				case "Country": {
					this.Situation.Surface = "Any";
					this.Situation.HeliMin = 0;
					this.Situation.HeliMax = 9999;
					this.Situation.RwyMin = 1;
					this.Situation.RwyMax = 9999;
					this.Situation.ParkMin = 2;
					this.Situation.ParkMax = 9999;
					this.Situation.ParkWidMin = 1;
					this.Situation.ParkWidMax = 9999;
					this.Situation.RwyLenMin = 600;
					this.Situation.RwyLenMax = 19000;
					this.Situation.RwyWidMin = 5;
					this.Situation.RwyWidMax = 200;
					this.Situation.ElevMin = -2000;
					this.Situation.ElevMax = 19000;
					break;
				}
			}

			switch (this.Situation.SituationType) {
				case "ICAO":
				case "Geo": {
					this.app.$data.state.ui.map.sources.situationBoundary.data.features[this.Situation.Index].geometry.coordinates = [[]];
					break;
				}
			}


			switch (this.Situation.SituationType) {
				case "Any": {
					break;
				}
				case "Country": {
					this.Situation.Country = "US";
					break;
				}
				case "ICAO": {
					this.Situation.ICAO = "";
					break;
				}
				case "Geo": {
					this.Situation.Lon = 0;
					this.Situation.Lat = 0;
					this.Situation.TriggerRange = 1;
					break;
				}
			}
		},
		MapClick(ev :any) {
			if(this.pinSetName) {
				switch(this.pinSetName) {
					case 'loc': {
						this.Situation.Lon = ev.lngLat.lng;
						this.Situation.Lat = ev.lngLat.lat;
						this.app.$emit('interaction', { cmd: 'map:draw', state: true, type: "situation_location", sit: this.Situation });
						break;
					}
				}
				this.pinSetName = null;
			}
		},
	},
	created() {
		//this.app.$on("interaction", (e: any) => {
		//	if(e.cmd == 'map:draw' && e.state == false) {
		//		this.QueryAirports();
		//	}
		//});
		this.QueryAirports();
		this.app.$on("adventure:situation:update", (e: any) => { this.CheckIfNextIsUnknown(); });
		this.app.$emit("adventure:situation:update", this);

		this.app.$on('map:click', this.MapClick);
	},
	beforeDestroy() {
		this.app.$off('map:click', this.MapClick);
	},
	watch: {
		"Adventure.IncludeICAO"() {
			this.QueryAirports();
		},
		"Adventure.ExcludeICAO"() {
			this.QueryAirports();
		},
		"Adventure.IncludeAPTName"() {
			this.QueryAirports();
		},
		"Adventure.ExcludeAPTName"() {
			this.QueryAirports();
		},
		"Situation.SituationType"() {
			this.SetStruct();
			this.app.$emit("adventure:situation:update", this);
		}
	}
});
</script>

<style lang="scss" scoped>
@import '@/sys/scss/sizes.scss';
@import '@/sys/scss/colors.scss';
@import '@/sys/scss/mixins.scss';

.adventure_situation {

	.separator {
		display: flex;
		height: 20px;
		width: 1px;
		background: #000;
		opacity: 0.2;
		margin-left: 50%;
		margin-top: 8px;
		margin-bottom: 8px;
	}

	&_collapsed {
		font-family: "SkyOS-SemiBold";
		padding: 8px;
		background: $ui_colors_bright_button_info;
		margin-bottom: 0;
		color: #FFF;
		&.expanded {
			opacity: 0.5;
			border-bottom-left-radius: 0;
			border-bottom-right-radius: 0;
			padding-bottom: 15px;
			margin-bottom: -10px;
		}
	}

	&_card {
		padding: 8px;
		background: $ui_colors_bright_button_info;
		border-radius: 12px;
		margin-bottom: 8px;
	}

	&_tonext {
		margin-bottom: 8px;

		.distance {
			text-align: center;
		}
	}

	&_controls {
		margin-bottom: 4px;
		display: flex;
		align-items: stretch;
		& .column {
			&_narrow {
				flex-direction: row;
				.button_action  {
					align-self: stretch;
				}
			}
		}
	}
}
</style>