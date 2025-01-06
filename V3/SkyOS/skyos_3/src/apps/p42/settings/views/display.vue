<template>
	<scroll_view :scroller_offset="{ top: 36, bottom: 30 }">
		<div class="app-panel-wrap">
			<div class="app-panel-content">
				<div class="app-panel-hit">

					<h2>Theme</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="columns columns_margined h_edge_padding_bottom">
								<div class="column column_h-stretch">
									<div class="buttons_list shadowed-shallow theme--bright">
										<button_listed icon="dark/itheme" @click.native="set_theme('theme--bright')">Light</button_listed>
									</div>
								</div>
								<div class="column column_h-stretch">
									<div class="buttons_list shadowed-shallow theme--dark">
										<button_listed icon="bright/itheme" @click.native="set_theme('theme--dark')">Dark</button_listed>
									</div>
								</div>
							</div>
							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_h-stretch">
									<toggle v-model="theme_auto" @modified="set_theme_auto" :notice="'Automatically set the theme of your Skypad to match the local time in-sim.'">Automatic theme switching</toggle>
								</div>
							</div>
						</div>
					</div>

					<h2>Skypad</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="h_edge_padding_bottom">
								<div class="columns columns_margined">
									<div class="column column_h-stretch">
										<div class="buttons_list shadowed-shallow">
											<button_listed icon="theme/smaller" @click.native="set_size('-')">Smaller</button_listed>
										</div>
									</div>
									<div class="column column_h-stretch">
										<div class="buttons_list shadowed-shallow">
											<button_listed icon="theme/larger" @click.native="set_size('+')">Larger</button_listed>
										</div>
									</div>
									<div class="column column_h-stretch">
										<div class="buttons_list shadowed-shallow">
											<button_listed class="cancel" icon="theme/reload" @click.native="set_size('r')">Reset</button_listed>
										</div>
									</div>
								</div>
								<p class="notice">Easily toggle between 2 sizes using the buttons on the left of the Skypad. To assign a size to each, first, click the button, then select your desired size above.</p>
							</div>

							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_h-stretch">
									<toggle v-model="topmost" @modified="set_topmost" :notice="'Keep the Skypad on top of other apps and the simulator. You can toggle &quot;Always on top&quot; at any time from the top right of the Skypad interface, next to the window controls.'">Always on top</toggle>
								</div>
							</div>

							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_h-stretch">
									<toggle v-model="stow_can" @modified="set_stow_can" :notice="'Double-click the left or right bezel of the Skypad to send it to the opposite edge of your display.'">Double-click bezel to stow</toggle>
								</div>
							</div>

							<div class="columns columns_margined h_edge_padding_top" v-if="stow_can">
								<div class="column column_h-stretch">
									<toggle v-model="stow_auto" @modified="set_stow_auto" :disabled="!stow_can" :notice="'Skypad will automatically stow to the edge of the screen when you click away. Skypad must be previously stowed.'">Auto re-stow</toggle>
								</div>
							</div>

							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_h-stretch">
									<toggle v-model="framed" @modified="set_framed" :notice="'Improved compatibility to VR and window capture utilities. Restart your Skypad to apply.'">Window frame</toggle>
								</div>
							</div>
						</div>
					</div>

					<h2>Units</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="columns columns_margined">
								<div class="column column_1">
									<p class="h_no-margin"><strong>Numbers, Dates & Currency</strong></p>
								</div>
								<div class="column column_3">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="units_numbers" @input="set_units_numbers">
											<optgroup label="Common">
												<option value="en-US">English (United States)</option>
												<option value="en-GB">English (United Kingdom)</option>
												<option value="fr-FR">French (France)</option>
												<option value="de-DE">German (Germany)</option>
												<option value="es-ES">Spanish (Spain)</option>
											</optgroup>
											<optgroup label="-----------">
												<option v-for="(locale, name) in locales" :key="locale" :value="name">{{ locale }}</option>
											</optgroup>
										</selector>
									</div>
									<div class="buttons_list shadowed-shallow h_no-margin h_edge_padding_top_half">
										<selector v-model="units_currency" @input="set_units_currency">
											<option value="USD">$ Dollar</option>
											<option value="EUR">€ Euro</option>
											<option value="GBP">£ Pound</option>
										</selector>
									</div>
									<div class="h_edge_padding_top_half">
										<p class="notice h_no-margin">Numbers: <strong>{{ locale_example_number }}</strong></p>
										<p class="notice h_no-margin">Dates & Times: <strong>{{ locale_example_date_time }}</strong></p>
										<p class="notice h_no-margin">Currency: <strong>{{ locale_example_currency }}</strong></p>
									</div>
								</div>
							</div>
							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_1">
									<p class="h_no-margin"><strong>Distances</strong></p>
								</div>
								<div class="column column_3">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="units_distances" @input="set_units_distances">
											<option value="nautical_miles">Nautical miles</option>
											<option value="miles">Miles</option>
											<option value="kilometers">Kilometers</option>
										</selector>
									</div>
									<p class="notice h_no-margin h_edge_padding_top_half">Distance between airports</p>
								</div>
							</div>
							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_1">
									<p class="h_no-margin"><strong>Heights</strong></p>
								</div>
								<div class="column column_3">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="units_heights" @input="set_units_heights">
											<option value="feet">Feet</option>
											<option value="meters">Meters</option>
										</selector>
									</div>
									<p class="notice h_no-margin h_edge_padding_top_half">Altitudes, airport elevations</p>
								</div>
							</div>
							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_1">
									<p class="h_no-margin"><strong>Lengths</strong></p>
								</div>
								<div class="column column_3">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="units_lengths" @input="set_units_lengths">
											<option value="feet">Feet</option>
											<option value="meters">Meters</option>
										</selector>
									</div>
									<p class="notice h_no-margin h_edge_padding_top_half">Runway lengths, wingspan</p>
								</div>
							</div>
							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_1">
									<p class="h_no-margin"><strong>Speeds</strong></p>
								</div>
								<div class="column column_3">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="units_speeds" @input="set_units_speeds">
											<option value="knots">Knots</option>
											<option value="mph">Miles per hour</option>
											<option value="kph">Kilometers per hour</option>
										</selector>
									</div>
									<p class="notice h_no-margin h_edge_padding_top_half">Airspeeds, ground speeds</p>
								</div>
							</div>
							<div class="columns columns_margined h_edge_padding_top">
								<div class="column column_1">
									<p class="h_no-margin"><strong>Weights</strong></p>
								</div>
								<div class="column column_3">
									<div class="buttons_list shadowed-shallow h_no-margin">
										<selector v-model="units_weights" @input="set_units_weights">
											<option value="kg">Kilograms</option>
											<option value="lbs">Pounds</option>
										</selector>
									</div>
									<p class="notice h_no-margin h_edge_padding_top_half">Payload, aircraft weights</p>
								</div>
							</div>
						</div>
					</div>

					<h2>Simulator</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="columns columns_margined">
								<div class="column column_h-stretch">
									<toggle v-model="sim_tips" @modified="set_sim_tips" :disabled="!has_transponder" :notice="'Messages will be shown in sim when landing at airports and when relevant info needs to be communicated about a contract.'">Enable Tips in the simulator</toggle>
								</div>
							</div>
						</div>
					</div>

					<h2>Discord</h2>
					<div class="app-box shadowed-deep nooverflow h_edge_padding">
						<div>
							<div class="columns columns_margined">
								<div class="column column_h-stretch">
									<toggle v-model="discord_presence" @modified="set_discord_presence" :disabled="!has_transponder" :notice="'Share what you fly to your friends on Discord.'">Enable rich presence</toggle>
								</div>
							</div>
						</div>
					</div>

				</div>
			</div>
		</div>
	</scroll_view>
</template>

<script lang="ts">
import Vue from 'vue';
import { AppInfo } from "@/sys/foundation/app_model"

export default Vue.extend({
	props: {
		root: Object,
		app: AppInfo,
		appName: String
	},
	components: {
	},
	data() {
		return {
			has_transponder: this.$os.api.connected,
			sim_tips: this.$os.userConfig.get(['ui', 'sim_tips']),
			units_numbers: this.$os.userConfig.get(['ui','units','numbers']),
			units_currency: this.$os.userConfig.get(['ui','units','currency']),
			units_distances: this.$os.userConfig.get(['ui','units','distances']),
			units_heights: this.$os.userConfig.get(['ui','units','heights']),
			units_speeds: this.$os.userConfig.get(['ui','units','speeds']),
			units_lengths: this.$os.userConfig.get(['ui','units','lengths']),
			units_weights: this.$os.userConfig.get(['ui','units','weights']),
			framed: this.$os.userConfig.get(['ui', 'framed']),
			topmost: this.$os.userConfig.get(['ui', 'topmost']),
			stow_can: this.$os.userConfig.get(['ui', 'stow','can']),
			stow_auto: this.$os.userConfig.get(['ui', 'stow','auto']),
			theme: this.$os.userConfig.get(['ui', 'theme']),
			theme_auto: this.$os.userConfig.get(['ui', 'theme_auto']),
			discord_presence: this.$os.userConfig.get(['ui', 'discord_presence']),

			locale_example_number: null,
			locale_example_date_time: null,
			locale_example_currency: null,

			locales: {
				"af-NA": "Afrikaans (Namibia)",
				"af-ZA": "Afrikaans (South Africa)",
				"af": "Afrikaans",
				"ak-GH": "Akan (Ghana)",
				"ak": "Akan",
				"sq-AL": "Albanian (Albania)",
				"sq": "Albanian",
				"am-ET": "Amharic (Ethiopia)",
				"am": "Amharic",
				"ar-DZ": "Arabic (Algeria)",
				"ar-BH": "Arabic (Bahrain)",
				"ar-EG": "Arabic (Egypt)",
				"ar-IQ": "Arabic (Iraq)",
				"ar-JO": "Arabic (Jordan)",
				"ar-KW": "Arabic (Kuwait)",
				"ar-LB": "Arabic (Lebanon)",
				"ar-LY": "Arabic (Libya)",
				"ar-MA": "Arabic (Morocco)",
				"ar-OM": "Arabic (Oman)",
				"ar-QA": "Arabic (Qatar)",
				"ar-SA": "Arabic (Saudi Arabia)",
				"ar-SD": "Arabic (Sudan)",
				"ar-SY": "Arabic (Syria)",
				"ar-TN": "Arabic (Tunisia)",
				"ar-AE": "Arabic (United Arab Emirates)",
				"ar-YE": "Arabic (Yemen)",
				"ar": "Arabic",
				"hy-AM": "Armenian (Armenia)",
				"hy": "Armenian",
				"as-IN": "Assamese (India)",
				"as": "Assamese",
				"asa-TZ": "Asu (Tanzania)",
				"asa": "Asu",
				"az-Cyrl": "Azerbaijani (Cyrillic)",
				"az-Cyrl-AZ": "Azerbaijani (Cyrillic, Azerbaijan)",
				"az-Latn": "Azerbaijani (Latin)",
				"az-Latn-AZ": "Azerbaijani (Latin, Azerbaijan)",
				"az": "Azerbaijani",
				"bm-ML": "Bambara (Mali)",
				"bm": "Bambara",
				"eu-ES": "Basque (Spain)",
				"eu": "Basque",
				"be-BY": "Belarusian (Belarus)",
				"be": "Belarusian",
				"bem-ZM": "Bemba (Zambia)",
				"bem": "Bemba",
				"bez-TZ": "Bena (Tanzania)",
				"bez": "Bena",
				"bn-BD": "Bengali (Bangladesh)",
				"bn-IN": "Bengali (India)",
				"bn": "Bengali",
				"bs-BA": "Bosnian (Bosnia and Herzegovina)",
				"bs": "Bosnian",
				"bg-BG": "Bulgarian (Bulgaria)",
				"bg": "Bulgarian",
				"my-MM": "Burmese (Myanmar [Burma])",
				"my": "Burmese",
				"yue-Hant-HK": "Cantonese (Traditional, Hong Kong SAR China)",
				"ca-ES": "Catalan (Spain)",
				"ca": "Catalan",
				"tzm-Latn": "Central Morocco Tamazight (Latin)",
				"tzm-Latn-MA": "Central Morocco Tamazight (Latin, Morocco)",
				"tzm": "Central Morocco Tamazight",
				"chr-US": "Cherokee (United States)",
				"chr": "Cherokee",
				"cgg-UG": "Chiga (Uganda)",
				"cgg": "Chiga",
				"zh-Hans": "Chinese (Simplified Han)",
				"zh-Hans-CN": "Chinese (Simplified Han, China)",
				"zh-Hans-HK": "Chinese (Simplified Han, Hong Kong SAR China)",
				"zh-Hans-MO": "Chinese (Simplified Han, Macau SAR China)",
				"zh-Hans-SG": "Chinese (Simplified Han, Singapore)",
				"zh-Hant": "Chinese (Traditional Han)",
				"zh-Hant-HK": "Chinese (Traditional Han, Hong Kong SAR China)",
				"zh-Hant-MO": "Chinese (Traditional Han, Macau SAR China)",
				"zh-Hant-TW": "Chinese (Traditional Han, Taiwan)",
				"zh": "Chinese",
				"kw-GB": "Cornish (United Kingdom)",
				"kw": "Cornish",
				"hr-HR": "Croatian (Croatia)",
				"hr": "Croatian",
				"cs-CZ": "Czech (Czech Republic)",
				"cs": "Czech",
				"da-DK": "Danish (Denmark)",
				"da": "Danish",
				"nl-BE": "Dutch (Belgium)",
				"nl-NL": "Dutch (Netherlands)",
				"nl": "Dutch",
				"ebu-KE": "Embu (Kenya)",
				"ebu": "Embu",
				"en-AS": "English (American Samoa)",
				"en-AU": "English (Australia)",
				"en-BE": "English (Belgium)",
				"en-BZ": "English (Belize)",
				"en-BW": "English (Botswana)",
				"en-CA": "English (Canada)",
				"en-GU": "English (Guam)",
				"en-HK": "English (Hong Kong SAR China)",
				"en-IN": "English (India)",
				"en-IE": "English (Ireland)",
				"en-IL": "English (Israel)",
				"en-JM": "English (Jamaica)",
				"en-MT": "English (Malta)",
				"en-MH": "English (Marshall Islands)",
				"en-MU": "English (Mauritius)",
				"en-NA": "English (Namibia)",
				"en-NZ": "English (New Zealand)",
				"en-MP": "English (Northern Mariana Islands)",
				"en-PK": "English (Pakistan)",
				"en-PH": "English (Philippines)",
				"en-SG": "English (Singapore)",
				"en-ZA": "English (South Africa)",
				"en-TT": "English (Trinidad and Tobago)",
				"en-UM": "English (U.S. Minor Outlying Islands)",
				"en-VI": "English (U.S. Virgin Islands)",
				"en-GB": "English (United Kingdom)",
				"en-US": "English (United States)",
				"en-ZW": "English (Zimbabwe)",
				"en": "English",
				"eo": "Esperanto",
				"et-EE": "Estonian (Estonia)",
				"et": "Estonian",
				"ee-GH": "Ewe (Ghana)",
				"ee-TG": "Ewe (Togo)",
				"ee": "Ewe",
				"fo-FO": "Faroese (Faroe Islands)",
				"fo": "Faroese",
				"fil-PH": "Filipino (Philippines)",
				"fil": "Filipino",
				"fi-FI": "Finnish (Finland)",
				"fi": "Finnish",
				"fr-BE": "French (Belgium)",
				"fr-BJ": "French (Benin)",
				"fr-BF": "French (Burkina Faso)",
				"fr-BI": "French (Burundi)",
				"fr-CM": "French (Cameroon)",
				"fr-CA": "French (Canada)",
				"fr-CF": "French (Central African Republic)",
				"fr-TD": "French (Chad)",
				"fr-KM": "French (Comoros)",
				"fr-CG": "French (Congo - Brazzaville)",
				"fr-CD": "French (Congo - Kinshasa)",
				"fr-CI": "French (Côte d’Ivoire)",
				"fr-DJ": "French (Djibouti)",
				"fr-GQ": "French (Equatorial Guinea)",
				"fr-FR": "French (France)",
				"fr-GA": "French (Gabon)",
				"fr-GP": "French (Guadeloupe)",
				"fr-GN": "French (Guinea)",
				"fr-LU": "French (Luxembourg)",
				"fr-MG": "French (Madagascar)",
				"fr-ML": "French (Mali)",
				"fr-MQ": "French (Martinique)",
				"fr-MC": "French (Monaco)",
				"fr-NE": "French (Niger)",
				"fr-RW": "French (Rwanda)",
				"fr-RE": "French (Réunion)",
				"fr-BL": "French (Saint Barthélemy)",
				"fr-MF": "French (Saint Martin)",
				"fr-SN": "French (Senegal)",
				"fr-CH": "French (Switzerland)",
				"fr-TG": "French (Togo)",
				"fr": "French",
				"ff-SN": "Fulah (Senegal)",
				"ff": "Fulah",
				"gl-ES": "Galician (Spain)",
				"gl": "Galician",
				"lg-UG": "Ganda (Uganda)",
				"lg": "Ganda",
				"ka-GE": "Georgian (Georgia)",
				"ka": "Georgian",
				"de-AT": "German (Austria)",
				"de-BE": "German (Belgium)",
				"de-DE": "German (Germany)",
				"de-LI": "German (Liechtenstein)",
				"de-LU": "German (Luxembourg)",
				"de-CH": "German (Switzerland)",
				"de": "German",
				"el-CY": "Greek (Cyprus)",
				"el-GR": "Greek (Greece)",
				"el": "Greek",
				"gu-IN": "Gujarati (India)",
				"gu": "Gujarati",
				"guz-KE": "Gusii (Kenya)",
				"guz": "Gusii",
				"ha-Latn": "Hausa (Latin)",
				"ha-Latn-GH": "Hausa (Latin, Ghana)",
				"ha-Latn-NE": "Hausa (Latin, Niger)",
				"ha-Latn-NG": "Hausa (Latin, Nigeria)",
				"ha": "Hausa",
				"haw-US": "Hawaiian (United States)",
				"haw": "Hawaiian",
				"he-IL": "Hebrew (Israel)",
				"he": "Hebrew",
				"hi-IN": "Hindi (India)",
				"hi": "Hindi",
				"hu-HU": "Hungarian (Hungary)",
				"hu": "Hungarian",
				"is-IS": "Icelandic (Iceland)",
				"is": "Icelandic",
				"ig-NG": "Igbo (Nigeria)",
				"ig": "Igbo",
				"id-ID": "Indonesian (Indonesia)",
				"id": "Indonesian",
				"ga-IE": "Irish (Ireland)",
				"ga": "Irish",
				"it-IT": "Italian (Italy)",
				"it-CH": "Italian (Switzerland)",
				"it": "Italian",
				"ja-JP": "Japanese (Japan)",
				"ja": "Japanese",
				"kea-CV": "Kabuverdianu (Cape Verde)",
				"kea": "Kabuverdianu",
				"kab-DZ": "Kabyle (Algeria)",
				"kab": "Kabyle",
				"kl-GL": "Kalaallisut (Greenland)",
				"kl": "Kalaallisut",
				"kln-KE": "Kalenjin (Kenya)",
				"kln": "Kalenjin",
				"kam-KE": "Kamba (Kenya)",
				"kam": "Kamba",
				"kn-IN": "Kannada (India)",
				"kn": "Kannada",
				"kk-Cyrl": "Kazakh (Cyrillic)",
				"kk-Cyrl-KZ": "Kazakh (Cyrillic, Kazakhstan)",
				"kk": "Kazakh",
				"km-KH": "Khmer (Cambodia)",
				"km": "Khmer",
				"ki-KE": "Kikuyu (Kenya)",
				"ki": "Kikuyu",
				"rw-RW": "Kinyarwanda (Rwanda)",
				"rw": "Kinyarwanda",
				"kok-IN": "Konkani (India)",
				"kok": "Konkani",
				"ko-KR": "Korean (South Korea)",
				"ko": "Korean",
				"khq-ML": "Koyra Chiini (Mali)",
				"khq": "Koyra Chiini",
				"ses-ML": "Koyraboro Senni (Mali)",
				"ses": "Koyraboro Senni",
				"lag-TZ": "Langi (Tanzania)",
				"lag": "Langi",
				"lv-LV": "Latvian (Latvia)",
				"lv": "Latvian",
				"lt-LT": "Lithuanian (Lithuania)",
				"lt": "Lithuanian",
				"luo-KE": "Luo (Kenya)",
				"luo": "Luo",
				"luy-KE": "Luyia (Kenya)",
				"luy": "Luyia",
				"mk-MK": "Macedonian (Macedonia)",
				"mk": "Macedonian",
				"jmc-TZ": "Machame (Tanzania)",
				"jmc": "Machame",
				"kde-TZ": "Makonde (Tanzania)",
				"kde": "Makonde",
				"mg-MG": "Malagasy (Madagascar)",
				"mg": "Malagasy",
				"ms-BN": "Malay (Brunei)",
				"ms-MY": "Malay (Malaysia)",
				"ms": "Malay",
				"ml-IN": "Malayalam (India)",
				"ml": "Malayalam",
				"mt-MT": "Maltese (Malta)",
				"mt": "Maltese",
				"gv-GB": "Manx (United Kingdom)",
				"gv": "Manx",
				"mr-IN": "Marathi (India)",
				"mr": "Marathi",
				"mas-KE": "Masai (Kenya)",
				"mas-TZ": "Masai (Tanzania)",
				"mas": "Masai",
				"mer-KE": "Meru (Kenya)",
				"mer": "Meru",
				"mfe-MU": "Morisyen (Mauritius)",
				"mfe": "Morisyen",
				"naq-NA": "Nama (Namibia)",
				"naq": "Nama",
				"ne-IN": "Nepali (India)",
				"ne-NP": "Nepali (Nepal)",
				"ne": "Nepali",
				"nd-ZW": "North Ndebele (Zimbabwe)",
				"nd": "North Ndebele",
				"nb-NO": "Norwegian Bokmål (Norway)",
				"nb": "Norwegian Bokmål",
				"nn-NO": "Norwegian Nynorsk (Norway)",
				"nn": "Norwegian Nynorsk",
				"nyn-UG": "Nyankole (Uganda)",
				"nyn": "Nyankole",
				"or-IN": "Oriya (India)",
				"or": "Oriya",
				"om-ET": "Oromo (Ethiopia)",
				"om-KE": "Oromo (Kenya)",
				"om": "Oromo",
				"ps-AF": "Pashto (Afghanistan)",
				"ps": "Pashto",
				"fa-AF": "Persian (Afghanistan)",
				"fa-IR": "Persian (Iran)",
				"fa": "Persian",
				"pl-PL": "Polish (Poland)",
				"pl": "Polish",
				"pt-BR": "Portuguese (Brazil)",
				"pt-GW": "Portuguese (Guinea-Bissau)",
				"pt-MZ": "Portuguese (Mozambique)",
				"pt-PT": "Portuguese (Portugal)",
				"pt": "Portuguese",
				"pa-Arab": "Punjabi (Arabic)",
				"pa-Arab-PK": "Punjabi (Arabic, Pakistan)",
				"pa-Guru": "Punjabi (Gurmukhi)",
				"pa-Guru-IN": "Punjabi (Gurmukhi, India)",
				"pa": "Punjabi",
				"ro-MD": "Romanian (Moldova)",
				"ro-RO": "Romanian (Romania)",
				"ro": "Romanian",
				"rm-CH": "Romansh (Switzerland)",
				"rm": "Romansh",
				"rof-TZ": "Rombo (Tanzania)",
				"rof": "Rombo",
				"ru-MD": "Russian (Moldova)",
				"ru-RU": "Russian (Russia)",
				"ru-UA": "Russian (Ukraine)",
				"ru": "Russian",
				"rwk-TZ": "Rwa (Tanzania)",
				"rwk": "Rwa",
				"saq-KE": "Samburu (Kenya)",
				"saq": "Samburu",
				"sg-CF": "Sango (Central African Republic)",
				"sg": "Sango",
				"seh-MZ": "Sena (Mozambique)",
				"seh": "Sena",
				"sr-Cyrl": "Serbian (Cyrillic)",
				"sr-Cyrl-BA": "Serbian (Cyrillic, Bosnia and Herzegovina)",
				"sr-Cyrl-ME": "Serbian (Cyrillic, Montenegro)",
				"sr-Cyrl-RS": "Serbian (Cyrillic, Serbia)",
				"sr-Latn": "Serbian (Latin)",
				"sr-Latn-BA": "Serbian (Latin, Bosnia and Herzegovina)",
				"sr-Latn-ME": "Serbian (Latin, Montenegro)",
				"sr-Latn-RS": "Serbian (Latin, Serbia)",
				"sr": "Serbian",
				"sn-ZW": "Shona (Zimbabwe)",
				"sn": "Shona",
				"ii-CN": "Sichuan Yi (China)",
				"ii": "Sichuan Yi",
				"si-LK": "Sinhala (Sri Lanka)",
				"si": "Sinhala",
				"sk-SK": "Slovak (Slovakia)",
				"sk": "Slovak",
				"sl-SI": "Slovenian (Slovenia)",
				"sl": "Slovenian",
				"xog-UG": "Soga (Uganda)",
				"xog": "Soga",
				"so-DJ": "Somali (Djibouti)",
				"so-ET": "Somali (Ethiopia)",
				"so-KE": "Somali (Kenya)",
				"so-SO": "Somali (Somalia)",
				"so": "Somali",
				"es-AR": "Spanish (Argentina)",
				"es-BO": "Spanish (Bolivia)",
				"es-CL": "Spanish (Chile)",
				"es-CO": "Spanish (Colombia)",
				"es-CR": "Spanish (Costa Rica)",
				"es-DO": "Spanish (Dominican Republic)",
				"es-EC": "Spanish (Ecuador)",
				"es-SV": "Spanish (El Salvador)",
				"es-GQ": "Spanish (Equatorial Guinea)",
				"es-GT": "Spanish (Guatemala)",
				"es-HN": "Spanish (Honduras)",
				"es-419": "Spanish (Latin America)",
				"es-MX": "Spanish (Mexico)",
				"es-NI": "Spanish (Nicaragua)",
				"es-PA": "Spanish (Panama)",
				"es-PY": "Spanish (Paraguay)",
				"es-PE": "Spanish (Peru)",
				"es-PR": "Spanish (Puerto Rico)",
				"es-ES": "Spanish (Spain)",
				"es-US": "Spanish (United States)",
				"es-UY": "Spanish (Uruguay)",
				"es-VE": "Spanish (Venezuela)",
				"es": "Spanish",
				"sw-KE": "Swahili (Kenya)",
				"sw-TZ": "Swahili (Tanzania)",
				"sw": "Swahili",
				"sv-FI": "Swedish (Finland)",
				"sv-SE": "Swedish (Sweden)",
				"sv": "Swedish",
				"gsw-CH": "Swiss German (Switzerland)",
				"gsw": "Swiss German",
				"shi-Latn": "Tachelhit (Latin)",
				"shi-Latn-MA": "Tachelhit (Latin, Morocco)",
				"shi-Tfng": "Tachelhit (Tifinagh)",
				"shi-Tfng-MA": "Tachelhit (Tifinagh, Morocco)",
				"shi": "Tachelhit",
				"dav-KE": "Taita (Kenya)",
				"dav": "Taita",
				"ta-IN": "Tamil (India)",
				"ta-LK": "Tamil (Sri Lanka)",
				"ta": "Tamil",
				"te-IN": "Telugu (India)",
				"te": "Telugu",
				"teo-KE": "Teso (Kenya)",
				"teo-UG": "Teso (Uganda)",
				"teo": "Teso",
				"th-TH": "Thai (Thailand)",
				"th": "Thai",
				"bo-CN": "Tibetan (China)",
				"bo-IN": "Tibetan (India)",
				"bo": "Tibetan",
				"ti-ER": "Tigrinya (Eritrea)",
				"ti-ET": "Tigrinya (Ethiopia)",
				"ti": "Tigrinya",
				"to-TO": "Tonga (Tonga)",
				"to": "Tonga",
				"tr-TR": "Turkish (Turkey)",
				"tr": "Turkish",
				"uk-UA": "Ukrainian (Ukraine)",
				"uk": "Ukrainian",
				"ur-IN": "Urdu (India)",
				"ur-PK": "Urdu (Pakistan)",
				"ur": "Urdu",
				"uz-Arab": "Uzbek (Arabic)",
				"uz-Arab-AF": "Uzbek (Arabic, Afghanistan)",
				"uz-Cyrl": "Uzbek (Cyrillic)",
				"uz-Cyrl-UZ": "Uzbek (Cyrillic, Uzbekistan)",
				"uz-Latn": "Uzbek (Latin)",
				"uz-Latn-UZ": "Uzbek (Latin, Uzbekistan)",
				"uz": "Uzbek",
				"vi-VN": "Vietnamese (Vietnam)",
				"vi": "Vietnamese",
				"vun-TZ": "Vunjo (Tanzania)",
				"vun": "Vunjo",
				"cy-GB": "Welsh (United Kingdom)",
				"cy": "Welsh",
				"yo-NG": "Yoruba (Nigeria)",
				"yo": "Yoruba",
				"zu-ZA": "Zulu (South Africa)",
				"zu": "Zulu"
			}
		}
	},
	methods: {

		update_number_examples() {

			// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat/DateTimeFormat
			const formatter1 = new Intl.NumberFormat(this.units_numbers, {
				style: 'currency',
				currency: this.units_currency,
				minimumFractionDigits: 2,
				maximumFractionDigits: 2
			});
			this.locale_example_currency = formatter1.format(1142.99).replace(/([A-Z])\w+/, '').trim();

			const formatter2 = new Intl.DateTimeFormat(this.units_numbers, {
				dateStyle: 'short',
				timeStyle: 'short'
			});
			this.locale_example_date_time = formatter2.format(new Date);

			const formatter3 = new Intl.NumberFormat(this.units_numbers);
			this.locale_example_number = formatter3.format(1142.99);
		},

		set_units_numbers(state :string) {
			this.units_numbers = state;
			this.$os.userConfig.set(['ui','units','numbers'], state);
			this.update_number_examples();
		},
		set_units_currency(state :string) {
			this.units_currency = state;
			this.$os.userConfig.set(['ui','units','currency'], state);
			this.update_number_examples();
		},
		set_units_distances(state :string) {
			this.units_distances = state;
			this.$os.userConfig.set(['ui','units','distances'], state);
		},
		set_units_heights(state :string) {
			this.units_heights = state;
			this.$os.userConfig.set(['ui','units','heights'], state);
		},
		set_units_speeds(state :string) {
			this.units_speeds = state;
			this.$os.userConfig.set(['ui','units','speeds'], state);
		},
		set_units_lengths(state :string) {
			this.units_lengths = state;
			this.$os.userConfig.set(['ui','units','lengths'], state);
		},
		set_units_weights(state :string) {
			this.units_weights = state;
			this.$os.userConfig.set(['ui','units','weights'], state);
		},
		set_size(change: string) {
			(this.$root as any).changeSize(change);
		},
		set_framed(state :boolean) {
			this.framed = state;
			this.$os.userConfig.set(['ui','framed'], state);
		},
		set_topmost(state :boolean) {
			this.topmost = state;
			this.$os.userConfig.set(['ui','topmost'], state);
			(this.$root as any).setTopmost(state);
		},
		set_stow_can(state :boolean) {
			this.stow_can = state;
			this.$os.userConfig.set(['ui','stow','can'], state);
		},
		set_stow_auto(state :boolean) {
			this.stow_auto = state;
			this.$os.userConfig.set(['ui','stow','auto'], state);
		},
		set_theme(theme :string) {
			this.set_theme_auto(false);
			this.theme = theme;
			this.$os.userConfig.set(['ui', 'theme'], theme);
		},
		set_theme_auto(state :boolean) {
			this.theme_auto = state;
			this.$os.userConfig.set(['ui','theme_auto'], state);
		},
		set_discord_presence(state :boolean) {
			this.discord_presence = null;
			this.send_transponder('discord_presence', state === true ? '1' : '0')
		},
		set_sim_tips(state :boolean) {
			this.send_transponder('sim_tips', state === true ? '1' : '0')
		},

		send_transponder(tag :string, state :string) {
			this.$os.api.send_ws('transponder:set', {
				param: tag,
				value: state,
			});
		},

		listener_ws(wsmsg: any) {
			switch(wsmsg.name[0]){
				case 'connect': {
					this.has_transponder = true;
					break;
				}
				case 'disconnect': {
					this.has_transponder = false;
					break;
				}
				case 'transponder': {
					switch(wsmsg.name[1]){
						case 'state': {
							this.discord_presence = wsmsg.payload.set.discord_presence == '1';
							this.$os.userConfig.set(['ui','discord_presence'], this.discord_presence);

							this.sim_tips = wsmsg.payload.set.sim_tips == '1';
							this.$os.userConfig.set(['ui','sim_tips'], this.sim_tips);
							break;
						}
					}
					break;
				}
			}
		},
	},
	mounted() {
		this.$os.system.set_cover(true);
		this.update_number_examples();
	},
	created() {
		this.$os.eventsBus.Bus.on('ws-in', this.listener_ws);
	},
	beforeDestroy() {
		this.$os.eventsBus.Bus.off('ws-in', this.listener_ws);
	}
});
</script>

<style lang="scss" scoped>
	.app-panel-content {
		max-width: 390px;
	}
</style>