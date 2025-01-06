import _Vue from 'vue';

export default class Analytics {
	public os: any;
	public vue: _Vue;
	public analytics: any = null;
	public analyticsPingTimeout: any = null;

	//#region Google Analytics
	public SetGA() {
		//const tag = this.get(['services','ga','tag']);
		//this.analytics = ua('ANALYTICS_KEY', tag.length ? tag : null);

		//this.analytics.pageview("/", "http://theskypark.com");
		this.ResetGAPing();
	}
	public ResetGAPing() {
		clearTimeout(this.analyticsPingTimeout);
		this.analyticsPingTimeout = setTimeout(() => {
			this.TrackEvent("Ping", "Ping", "Ping");
			this.ResetGAPing();
		}, 240000);
	}
	public TrackEvent(Category :string, Action :string, Label :string, Value = 1) {
		if(this.vue.$data.state.device.is_electron) {
			this.os.api.ipcR.send('asynchronous-message', 'analytics:event', {
				Category: Category,
				Action: Action,
				Label: Label,
				Value: Value,
				CID: this.os.userConfig.get(['services','ga','tag'])
			});
		}
	}
	public TrackPage(URL :string, Title :string) {
		if(this.vue.$data.state.device.is_electron) {
			this.os.api.ipcR.send('asynchronous-message', 'analytics:page', {
				URL: URL,
				Title: Title,
				CID: this.os.userConfig.get(['services','ga','tag'])
			});
		}
	}
	public TrackMapLoad(app :string) {
		this.TrackEvent("UI", "MapLoad", app);
	}
	//#endregion

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

}