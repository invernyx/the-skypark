'use strict'

import { ipcMain, screen, remote } from 'electron'
import { app, protocol, BrowserWindow } from 'electron'
import { createProtocol } from 'vue-cli-plugin-electron-builder/lib'
import installExtension, { VUEJS_DEVTOOLS } from 'electron-devtools-installer'
const isDevelopment = process.env.NODE_ENV !== 'production'
const electron = require('electron');
const fs = require('fs');

// Google Analytics
// https://www.npmjs.com/package/electron-google-analytics
import Analytics from 'electron-google-analytics';
const AnalyticsInstance = new Analytics('UA-131324058-1');

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let win: BrowserWindow | null
let hasFrame = false;
let windowMoved = false;
let windowMaximized = false;
let windowStartPosition = null;
let windowStartSize = null;
let ontop = false;
let stowCan = true;
let stowAuto = true;
let isStowed = null as string;
let isStowTransitionning = false;
let StowMem = null as string;
let w = 606;
let h = 785;
let liveLocation = null;
let persistedLocation = [100,100,0];

// Set Cache dir
let UserData = process.env.APPDATA + "/Parallel 42/The Skypark/2/Skypad";
if (fs.existsSync(process.env.APPDATA + "/Parallel 42/The Skypark/2/DEV.txt")) {
	if (process.env.WEBPACK_DEV_SERVER_URL) {
		UserData = process.env.APPDATA + "/Parallel 42/The Skypark DEV/2/Skypad_Debug";
	} else {
		UserData = process.env.APPDATA + "/Parallel 42/The Skypark DEV/2/Skypad";
	}
}
app.setPath('userData', UserData);

// Switches
app.commandLine.appendSwitch('disable-features', 'OutOfBlinkCors')
app.commandLine.appendSwitch('disable-pinch');

// Scheme must be registered before the app is ready
protocol.registerSchemesAsPrivileged([ { scheme: 'app', privileges: { secure: true, standard: true } } ])

readSettings();

const reframe = () => {
	win.resizable = true;
	const zoom = win.webContents.getZoomFactor();
	const contentBounds = win.getContentBounds();
	const bounds = win.getBounds();

	const newSize = [Math.ceil(w * zoom) + (bounds.width - contentBounds.width), Math.ceil(h * zoom) + (bounds.height - contentBounds.height)];

	win.setSize(newSize[0], newSize[1]);
	win.resizable = false;
}

const saveLocation = () => {
	StowMem = null;
	if(!isStowed) {
		const pos = win.getPosition();
		persistedLocation[0] = pos[0];
		persistedLocation[1] = pos[1];
		try { fs.writeFileSync(UserData + '/location.json', JSON.stringify(persistedLocation), 'utf-8'); } catch(e) { }
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
				let x = persistedLocation[0];
				let y = persistedLocation[1];
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
				transitionLocation(persistedLocation[0], persistedLocation[1], 400, () => {
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

function createWindow() {

	// Create the browser window.
	hasFrame = persistedLocation[2] == 0 ? false : true;
	win = new BrowserWindow({
		transparent: persistedLocation[2] == 0 ? true : false,
		resizable: false,
		show: false,
		frame: hasFrame,
		maximizable: false,
		thickFrame: false,
		titleBarStyle: 'hidden',
		width: w,
		height: h,
		minWidth: w,
		minHeight: h,
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

	try {
		win.setPosition(persistedLocation[0], persistedLocation[1]);
		win.setPosition(persistedLocation[0], persistedLocation[1]);
	} catch (e) {
		win.setPosition(100, 100);
		win.setPosition(100, 100);
	}

	if(!isDevelopment) {
		win.removeMenu();
	}

	win.webContents.on('before-input-event', (event, evt) => {
		// disable zooming
		if ((evt.code == "Minus" || evt.code == "Equal") && (evt.control || evt.meta)) { event.preventDefault() }
	});

	win.webContents.on('did-finish-load', () => {
		reframe();
		win.webContents.send('asynchronous-reply', 'framed', persistedLocation[2] == 0 ? false : true);
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
		windowMaximized = true;
	});

	win.on('unmaximize', (event :Event) => {
		windowMaximized = false;
	});

	win.on('minimize', (event :Event) => {
		win.webContents.send('asynchronous-reply', 'minimized');
	});

	win.on('restore', (event :Event) => {
		win.webContents.send('asynchronous-reply', 'restored');
	});

	win.on('moved', (event :Event) => {
		//saveLocation();
	});

	// Don't show until we are ready and loaded
	win.once('ready-to-show', () => {
		win.show();

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

	win.webContents.on('new-window', (event, url) => {
		event.preventDefault();
		require('electron').shell.openExternal(url);
	});

	loadPage();
}

function readSettings() {
	try{
		const settings = fs.readFileSync(UserData + '/location.json', 'utf8');

		if (settings) {
			try{
				const str =  JSON.parse(settings);
				persistedLocation[0] = parseInt(str[0]);
				persistedLocation[1] = parseInt(str[1]);
				persistedLocation[2] = parseInt(str[2]);
			} catch(e) {}
		}

		if(persistedLocation[2] != 0) {
			app.commandLine.appendSwitch('disable-gpu');
			app.disableHardwareAcceleration();
		}
	}
	catch {

	}

}

// Quit when all windows are closed.
app.on('window-all-closed', () => {
	// On macOS it is common for applications and their menu bar
	// to stay active until the user quits explicitly with Cmd + Q
	if (process.platform !== 'darwin') {
		app.quit()
	}
})

app.on('activate', () => {
	// On macOS it's common to re-create a window in the app when the
	// dock icon is clicked and there are no other windows open.
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
	if(!isStowed && StowMem != null && stowAuto) {
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

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', async () => {

	console.log(app.getLocale());

	if (isDevelopment && !process.env.IS_TEST) {
		// Install Vue Devtools
		try {
		await installExtension(VUEJS_DEVTOOLS)
		} catch (e) {
		console.error('Vue Devtools failed to install:', e.toString())
		}
	}

	createWindow();
})

// Exit cleanly on request from parent process in development mode.
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

ipcMain.on('a-msg', (event, cmds, param) => {
	const cmd = cmds.split(':');
	switch(cmd[0]){
		case 'min': {
			win.minimize();
			break;
		}
		case 'cls': {
			app.quit();
			break;
		}
		case 'center': {
			win.center();
			break;
		}
		case 'framed': {
			persistedLocation[2] = param ? 1 : 0;
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
			reframe();
			event.reply('asynchronous-reply', 'zoom', zoom);
			break;
		}

		case 'format-set': {
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
			if(stowCan && !win.resizable) {
				setStow(param);
			}
			break;
		}
		case 'stow-can': {
			stowCan = param;
			if(!stowCan) {
				StowMem = null;
				if(isStowed) {
					setStow(false);
				}
			}
			break;
		}
		case 'stow-auto': {
			stowAuto = param;
			break;
		}
		case 'moving': {
			try{
				const { x, y } = electron.screen.getCursorScreenPoint();
				const zoom = win.webContents.getZoomFactor();
				const xDiff = x - (param[0] * zoom);
				const yDiff = y - (param[1] * zoom);

				if(windowStartPosition == null) {
					windowStartPosition = { x, y };
					windowStartSize = win.getSize();
					if(!win.maximizable) {
						reframe();
						win.resizable = true;
					}
				}

				windowMoved = Math.abs(windowStartPosition.x - x) > 10 || Math.abs(windowStartPosition.y - y) > 10;

				if(!hasFrame) {

					const xs = Math.round(xDiff);
					const ys = Math.round(yDiff);

					if(liveLocation == null || (liveLocation[0] != xs || liveLocation[1] != ys)) {
						liveLocation = [xs, ys];
						win.setPosition(xs, ys);
						win.setSize(windowStartSize[0], windowStartSize[1]);
						//if(!win.resizable) {
						//	reframe();
						//}
					}

					if(windowMoved && isStowed) {
						setStow(null, false);
					}
				}
			} catch {
			}
			break;
		}
		case 'moved': {
			windowStartPosition = null;
			if(!win.maximizable) {
				win.resizable = false;
			}
			if(windowMoved) {
				if(!isStowed) {
					saveLocation();
				}
			}
			break;
		}
		case 'resize': {
			const checkFlag = () => {
				if(win.webContents.isLoading()) {
					setTimeout(checkFlag, 100); /* this checks the flag every 100 milliseconds*/
				} else {
					if(win.resizable != param) {
						if(param) {
							const windowBounds = win.getBounds();
							const closestDisplay = screen.getDisplayMatching(windowBounds);
							win.resizable = true;
							win.maximizable = true;

							var newWidth = closestDisplay.size.width - 100 - win.getPosition()[0];
							var newHeight = closestDisplay.size.height - 100;

							win.setSize(newWidth < 400 ? 400 : newWidth > 1900 ? 1900 : newWidth, newHeight < 400 ? 400 : newHeight);
							//ontop = true;
							win.setPosition(win.getPosition()[0],50);
							//win.setAlwaysOnTop(ontop);
							//event.reply('asynchronous-reply', 'pin', ontop);
						} else {
							if(windowMaximized) {
								win.restore();
								win.unmaximize();
								win.setPosition(100,100);
							}
							reframe();
							setTimeout(() => {
								win.resizable = false;
								win.maximizable = false;
							}, 300);
						}
					}
				}
			}
			checkFlag();
			break;
		}
		case 'pin': {
			if(param !== undefined) {
				ontop = param;
			} else {
				ontop = !ontop;
			}
			win.setAlwaysOnTop(ontop, 'normal');

			event.reply('asynchronous-reply', 'pin', ontop);
			break;
		}
		case 'analytics': {
			switch(cmd[1]) {
				case 'page': {
					//console.log('Analytics Page send', param);
					AnalyticsInstance.pageview('https://skypad.theskypark.com', param.URL, param.Title, param.CID)
					.then((response :any) => {
						//console.log("Analytics Page Sent ", response)
					  	return response;
					}).catch((err :any) => {
						//console.log("Analytics Page Error ", err)
					});
					break;
				}
				case 'event': {
					//console.log('Analytics Event send', param);
					AnalyticsInstance.event(param.Category, param.Action, { evLabel: param.Label, evValue: param.Value, clientID: param.CID })
					.then((response :any) => {
						//console.log("Analytics Event Sent ", response)
					  	return response;
					}).catch((err :any) => {
						//console.log("Analytics Event Error ", err)
					});
					break;
				}
				// AnalyticsInstance
			}
			break;
		}
	}
})