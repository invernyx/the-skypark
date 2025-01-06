'use strict'

import { ipcMain, screen } from 'electron'
import { app, protocol, BrowserWindow } from 'electron'
import { createProtocol } from 'vue-cli-plugin-electron-builder/lib'
import installExtension, { VUEJS_DEVTOOLS } from 'electron-devtools-installer'
const isDevelopment = process.env.NODE_ENV !== 'production'
const electron = require('electron');
const fs = require('fs');

const product_company = "Parallel 42";
const product_name = "The Skypark";
const product_version = 3;

let UserData = process.env.APPDATA + "/" + product_company + "/" + product_name + "/v" + product_version + "/Skypad";
let win: BrowserWindow | null
let hasFrame = false;
let ontop = false;
let stow_can = true;
let stow_auto = true;
let isStowed = null as string;
let isStowTransitionning = false;
let StowMem = null as string;

const width = 900;
const height = 680;
const minWidth = 450;
const minHeight = 650;
const persistedWindow = {
	x: 100,
	y: 100,
	w: width,
	h: height,
	f: 0,
};

// Google Analytics
// https://www.npmjs.com/package/electron-google-analytics
import Analytics from 'electron-google-analytics';
const AnalyticsInstance = new Analytics('UA-131324058-1');

// Switches
app.commandLine.appendSwitch('disable-features', 'OutOfBlinkCors')
app.commandLine.appendSwitch('disable-pinch');

// Scheme must be registered before the app is ready
protocol.registerSchemesAsPrivileged([ { scheme: 'app', privileges: { secure: true, standard: true } } ])

const setCacheFolder = () => {
	// Set Cache dir
	if (fs.existsSync(process.env.APPDATA + "/" + product_company + "/" + product_name + "/" + product_version + "/DEV.txt")) {
		if (process.env.WEBPACK_DEV_SERVER_URL) {
			UserData = process.env.APPDATA + "/" + product_company + "/" + product_name + " DEV/" + product_version + "/Skypad_Debug";
		} else {
			UserData = process.env.APPDATA + "/" + product_company + "/" + product_name + " DEV/" + product_version + "/Skypad";
		}
	}
	app.setPath('userData', UserData);
}

const createWindow = () => {

	// Create the browser window.
	hasFrame = persistedWindow.f == 0 ? false : true;
	win = new BrowserWindow({
		transparent: persistedWindow.f == 0 ? true : false,
		resizable: true,
		show: false,
		frame: hasFrame,
		maximizable: true,
		thickFrame: false,
		titleBarStyle: 'hidden',
		width: width,
		height: height,
		minWidth: minWidth,
		minHeight: minHeight,
		webPreferences: {
			// Use pluginOptions.nodeIntegration, leave this alone
			// See nklayman.github.io/vue-cli-plugin-electron-builder/guide/security.html#node-integration for more info
			//nodeIntegration: (process.env.ELECTRON_NODE_INTEGRATION as unknown) as boolean
			webSecurity: false,
			nodeIntegrationInWorker: true,
			nodeIntegration: true,
			contextIsolation: false,
			//backgroundThrottling: false,
			spellcheck: false,
		}
	})

	win.setPosition(persistedWindow.x, persistedWindow.y);
	win.setPosition(persistedWindow.x, persistedWindow.y);
	win.setSize(persistedWindow.w, persistedWindow.h);

	if(!isDevelopment) {
		win.removeMenu();
	}

	win.webContents.on('before-input-event', (event, evt) => {
		// disable zooming
		if ((evt.code == "Minus" || evt.code == "Equal") && (evt.control || evt.meta)) { event.preventDefault() }
	});

	win.webContents.on('did-finish-load', () => {
		win.webContents.send('asynchronous-reply', 'framed', persistedWindow.f == 0 ? false : true);
		//win.webContents.setZoomFactor(1);
		//win.webContents.setVisualZoomLevelLimits(1, 1);
	});

	const loadPage = () => {
		if (process.env.WEBPACK_DEV_SERVER_URL) {
			// Load the url of the dev server if in development mode
			win.loadURL(process.env.WEBPACK_DEV_SERVER_URL as string);
		} else {
			// Load the index.html when not in development
			createProtocol('app')
			win.loadURL('app://./index.html');
		}
	}

	win.on('closed', () => {
		win = null
	})

	win.on('maximize', (event :Event) => {
		//windowMaximized = true;
	});

	win.on('unmaximize', (event :Event) => {
		//windowMaximized = false;
	});

	win.on('minimize', (event :Event) => {
		win.webContents.send('asynchronous-reply', 'minimized');
	});

	win.on('restore', (event :Event) => {
		win.webContents.send('asynchronous-reply', 'restored');
	});

	win.on('moved', (event :Event) => {
		saveLocation();
	});

	win.once('ready-to-show', () => {
		win.show();

		// Ensure the window is visible on displays
		const windowBounds = win.getBounds();
		const closestDisplay = screen.getDisplayMatching(windowBounds);

		let inBounds = false;
		if(windowBounds.x > closestDisplay.bounds.x && windowBounds.x < closestDisplay.bounds.x + closestDisplay.bounds.width) {
			if(windowBounds.y > closestDisplay.bounds.y && windowBounds.y < closestDisplay.bounds.y + closestDisplay.bounds.height) {
				inBounds = true;
			}
		}

		if(!inBounds) {
			screen.getPrimaryDisplay();
			win.setPosition(100, 100);
		}

	})

	// Create listener that will handle the white screen issue
	win.webContents.on('did-fail-load', () => {
		loadPage();
	})

	// forward new window to browser
	win.webContents.on('new-window', (event, url) => {
		event.preventDefault();
		require('electron').shell.openExternal(url);
	});

	loadPage();
}

const readSettings = () => {
	try{
		const settings = fs.readFileSync(UserData + '/window.json', 'utf8');

		if (settings) {
			try{
				const str =  JSON.parse(settings);
				persistedWindow.x = parseInt(str.x);
				persistedWindow.y = parseInt(str.y);
				persistedWindow.w = parseInt(str.w);
				persistedWindow.h = parseInt(str.h);
				persistedWindow.f = parseInt(str.f);
			} catch(e) {}
		}

		if(persistedWindow.f != 0) {
			app.commandLine.appendSwitch('disable-gpu');
			app.disableHardwareAcceleration();
		}
	}
	catch {

	}

}

const saveLocation = () => {
	StowMem = null;
	if(!isStowed) {
		const pos = win.getPosition();
		const size = win.getSize();
		persistedWindow.x = pos[0];
		persistedWindow.y = pos[1];
		persistedWindow.w = size[0];
		persistedWindow.h = size[1];
		try { fs.writeFileSync(UserData + '/window.json', JSON.stringify(persistedWindow), 'utf-8'); } catch(e) { }
	}
}

const transitionLocation = (x, y, durationMs, cb) => {

	const fpMs = 15 / 1000;
	const frameDurMS = durationMs * fpMs;
	const easeOutCubic = (t) => 1+(--t)*t*t*t*t;
	const startLocation = win.getPosition();
	let interval = null;

	let pcnt = 0;
	interval = setInterval(() => {
		if(pcnt < 1) {
			try{
				const easedPcnt = easeOutCubic(pcnt);
				const xDif = startLocation[0] + ((x - startLocation[0]) * easedPcnt);
				const yDif = startLocation[1] + ((y - startLocation[1]) * easedPcnt);
				win.setPosition(Math.round(xDif), Math.round(yDif));
			} catch {
			}
			pcnt += 1000 / durationMs * fpMs;
		} else {
			try{
				win.setPosition(x, y);
			} catch {
			}
			clearInterval(interval);
			cb();
		}
	}, frameDurMS);

}

const setStow = (mode, restore = true) => {
	switch(mode) {
		case 't':
		case 'b': {
			return;
		}
	}

	StowMem = isStowed;
	if(!isStowTransitionning && !hasFrame) {
		if(restore) {
			isStowTransitionning = true;

			let stow = isStowed == null;
			if(mode !== null && mode !== false) {
				stow = true;
			}

			if(stow) {
				const zoom = win.webContents.getZoomFactor();
				const margin = 22 * zoom;
				const TaskbarMargin = 80 * zoom;
				let x = persistedWindow.x;
				let y = persistedWindow.y;
				const windowBounds = win.getBounds();
				const closestDisplay = screen.getDisplayMatching(windowBounds);
				const closestDisplayOffset = closestDisplay.bounds;

				switch(mode) {
					case 'l': {
						x = closestDisplayOffset.x - win.getSize()[0] + margin;
						break;
					}
					case 'r': {
						x = closestDisplayOffset.x + closestDisplay.size.width - margin;
						break;
					}
					default: {
						return;
					}
					//case 't': {
					//	y = closestDisplayOffset.y - win.getSize()[1] + TaskbarMargin;
					//	break;
					//}
					//case 'b': {
					//	y = closestDisplayOffset.y + closestDisplay.size.height - TaskbarMargin;
					//	break;
					//}
				}

				isStowed = mode;
				transitionLocation(x, y, 400, () => {
					isStowTransitionning = false;
				});
				win.webContents.send('asynchronous-reply', 'sleep');

				ontop = true;
				win.webContents.send('asynchronous-reply', 'pin', ontop);
				win.setAlwaysOnTop(ontop, 'normal');
			} else {
				transitionLocation(persistedWindow.x, persistedWindow.y, 400, () => {
					isStowed = null;
					isStowTransitionning = false;
					win.webContents.send('asynchronous-reply', 'unlock');
				});
			}
		} else {
			isStowed = null;
		}
	}
}

setCacheFolder();
readSettings();

// Quit when all windows are closed.
app.on('window-all-closed', () => {
	if (process.platform !== 'darwin') {
		app.quit()
	}
})

app.on('activate', () => {
	if (win === null) {
		createWindow();
	}
})

app.on('browser-window-focus', (event, win) => {
	if(isStowed) {
		setStow(false);
	}
});

app.on('browser-window-blur', (event, win) => {
	if(!isStowed && StowMem != null && stow_auto) {
		setStow(StowMem);
	}
	StowMem = null;
});

app.on('second-instance', (event,args, cwd) => {
	if(win){
		if(win.isMinimized()){
			win.restore() ;
		}
		win.focus() ;
	}
})

app.on('ready', async () => {

	if (isDevelopment && !process.env.IS_TEST) {
		// Install Vue Devtools
		//try {
		//	await installExtension(VUEJS_DEVTOOLS)
		//} catch (e) {
		//	console.error('Vue Devtools failed to install:', e.toString())
		//}
	}

	createWindow();
})



// Only for dev
if (isDevelopment) {
	if (process.platform === 'win32') {
		process.on('message', (data) => {
			if (data === 'graceful-exit') {
				app.quit()
			}
		})
	} else {
		process.on('SIGTERM', () => {
			app.quit()
		})
	}
}

// Event bus
ipcMain.on('asynchronous-message', (event, cmds, param) => {
	const cmd = cmds.split(':');

	switch(cmd[0]){
		case 'pin': {
			if(param !== undefined) {
				ontop = param;
			} else {
				ontop = !ontop;
			}
			win.setAlwaysOnTop(ontop, 'normal');

			event.sender.send('asynchronous-reply','pin', ontop);
			break;
		}
		case 'minimize': {
			win.minimize();
			break;
		}
		case 'quit': {
			app.quit();
			break;
		}
		case 'center': {
			win.center();
			break;
		}
		case 'frame': {
			persistedWindow.f = param ? 1 : 0;
			saveLocation();
			break;
		}
		case 'zoom': {
			let zoom = win.webContents.getZoomFactor();
			switch(param) {
				case '+': {
					zoom += 0.1;
					break;
				}
				case '-': {
					zoom -= 0.1;
					break;
				}
				case 'r': {
					zoom = 1;
					break;
				}
				default: {
					zoom = param;
					break;
				}
			}
			if(zoom < 0.6) {
				zoom = 0.6;
			}
			if(zoom > 3) {
				zoom = 3;
			}

			win.webContents.setZoomFactor(zoom);
			ipcMain.emit('command', 'zoom', zoom);
			break;
		}

		case 'format': {
			switch(param) {
				case '16_9': {
					win.setSize(1920,1080);
					break;
				}
				case '4_3': {
					win.setSize(1280,1080);
					break;
				}
				case '1_1': {
					win.setSize(1080,1080);
					break;
				}
				case '9_16': {
					win.setSize(720,1280);
					break;
				}
				case 'VID': {
					win.setSize(1024,460);
					break;
				}
			}
			break;
		}

		case 'tap': {
			if(isStowed) {
				setStow(null);
			}
			break;
		}
		case 'stow': {
			if(stow_can && !win.resizable) {
				setStow(param);
			}
			break;
		}
		case 'stow-can': {
			stow_can = param;
			if(!stow_can) {
				StowMem = null;
				if(isStowed) {
					setStow(false);
				}
			}
			break;
		}
		case 'stow-auto': {
			stow_auto = param;
			break;
		}
		case 'analytics': {
			switch(cmd[1]) {
				case 'page': {
					AnalyticsInstance.pageview('https://skypad.theskypark.com', param.URL, param.Title, param.CID)
					.then((response :any) => {
					  	return response;
					}).catch((err :any) => { });
					break;
				}
				case 'event': {
					AnalyticsInstance.event(param.Category, param.Action, { evLabel: param.Label, evValue: param.Value, clientID: param.CID })
					.then((response :any) => {
					  	return response;
					}).catch((err :any) => { });
					break;
				}
			}
			break;
		}
	}
})