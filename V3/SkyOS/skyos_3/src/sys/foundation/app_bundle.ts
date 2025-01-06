import { AppInfo, AppType } from './app_model';

const Apps: Array<AppInfo> = [
	new AppInfo({
		vendor: "p42",
		ident: "system",
		name: "System",
		url: "/system",
		visible: false,
		type: AppType.SYSTEM,
		loaded_state: {
			services: {
				wsRemote: {
					room: '',
				},
				ga: {
					tag: '',
				},
			},
			progress: {
				xp: 0,
			},
			ui: {
				tier: 'endeavour',
				illicit: false,
				load_time: 'minutes',
				sim_tips: true,
				topmost: true,
				framed: false,
				stow: {
					can: true,
					auto: true,
				},
				device_resize: {
					selected: 1,
					size_1: 0.9,
					size_2: 1,
				},
				discord_presence: true,
				changelog: {
					last: null,
					show: true,
				},
				theme: 'theme--dark',
				theme_auto: true,
				sleep_timer: 300000,
				sleep_timer_locked: 300000,
				wallpaper: "/assets/wallpapers/%theme%/0.jpg",
				wallpaper_locked: "/assets/wallpapers/1.jpg",
				units: {
					numbers: 'en-US',
					currency: 'USD',
					distances: 'nautical_miles',
					heights: 'feet',
					speeds: 'knots',
					lengths: 'feet',
					weights: 'kg'
				}
			},
			onboarding: {
				has_setup: false,
				has_requested: false,
			},
		}
	}),
	new AppInfo({
		vendor: "p42",
		ident: "home",
		name: "Home",
		url: "/",
		cache: true,
		type: AppType.HOME,
		padding: {
			left: 0,
			right: 0,
			top: 0,
			bottom: 0,
		}
	}),
	new AppInfo({
		vendor: "p42",
		ident: "sleep",
		name: "Sleep",
		url: "/z",
		cache: true,
		type: AppType.SLEEP,
	}),
	new AppInfo({
		vendor: "p42",
		ident: "contrax",
		name: "Contrax",
		url: "/contrax",
		icon_color: '#4D4D4F',
		cache: true,
		can_sleep: false,
		type: AppType.GENERAL,
		child_routes: [
			{ name: 'contract', path: 'contract/:id' }
		],
		padding: {
			left: 0,
			right: 0,
			top: 0,
			bottom: 250,
		}
	}),
	new AppInfo({
		vendor: "p42",
		ident: "conduit",
		name: "Conduit",
		url: "/conduit",
		icon_color: '#FFFFFF',
		type: AppType.SYSTEM,
		child_routes: [
			{ name: 'contract', path: 'contract/:id' },
			{ name: 'manifest_contract', path: 'manifest/:id' },
			{ name: 'manifest', path: 'manifest' }
		],
		padding: {
			left: 0,
			right: 0,
			top: 0,
			bottom: 250,
		},
		show: {
			contracts: true,
			manifests: true,
			fleet: true,
			plans: false,
		}
	}),
	new AppInfo({
		vendor: "p42",
		ident: "yoflight",
		name: "yoFlight",
		url: "/yoflight",
		icon_color: '#000073',
		cache: true,
		can_sleep: false,
		type: AppType.GENERAL,
		child_routes: [
			{ name: 'contract', path: 'contract/:id' },
			{ name: 'plan', path: 'plan/:id' }
		],
		padding: {
			left: 0,
			right: 0,
			top: 0,
			bottom: 250,
		},
		show: {
			contracts: false,
			manifests: false,
			fleet: true,
			plans: true,
		},
		icon_method: (comp :Vue, app :AppInfo) => {
			const content = [
				{
					class: 'full',
					styles: [
						"background-image:url(" + require('../../apps/p42/yoflight/icon_1.svg') + ")",
						"transform:rotate(" + comp.$os.simulator.location.Hdg + "deg)"
					]
				}
			]
			app.icon_html = content;
		}
	}),
	new AppInfo({
		vendor: "p42",
		ident: "inflight",
		name: "inFlight",
		url: "/inflight",
		icon_color: '#2C72AE',
		type: AppType.GENERAL,
	}),
	new AppInfo({
		vendor: "p42",
		ident: "progress",
		name: "Progress",
		url: "/progress",
		icon_color: '#F9B618',
		type: AppType.GENERAL,
		child_routes: [
			{ name: 'contract', path: 'contract/:id' }
		],
		padding: {
			left: 0,
			right: 0,
			top: 0,
			bottom: 250,
		}
	}),
	new AppInfo({
		vendor: "p42",
		ident: "aeroservice",
		name: "Aero Service",
		url: "/aeroservice",
		icon_color: '#EADCE4',
		type: AppType.GENERAL,
		child_routes: [
			{ name: 'aircraft', path: 'aircraft/:id' }
		],
		padding: {
			left: 0,
			right: 0,
			top: 0,
			bottom: 250,
		}
	}),
	new AppInfo({
		vendor: "p42",
		ident: "messages",
		name: "Messages",
		url: "/messages",
		icon_color: '#000000',
		type: AppType.GENERAL,
		child_routes: [
			{ name: 'chat', path: 'chat/:id' }
		],
	}),
	new AppInfo({
		vendor: "p42",
		ident: "settings",
		name: "Settings",
		url: "/settings",
		icon_color: '#2B2B2D',
		type: AppType.GENERAL,
		child_routes: [
			{ name: 'updates', path: 'updates' },
			{ name: 'display', path: 'display' },
			{ name: 'region', path: 'region' },
			{ name: 'config', path: 'config' },
			{ name: 'tier', path: 'tier' },
			{ name: 'tier', path: 'tier' },
			{ name: 'legal', path: 'legal' },
		],
	}),
];

export default Apps;