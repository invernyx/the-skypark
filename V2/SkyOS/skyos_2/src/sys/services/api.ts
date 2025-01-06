import _Vue from 'vue';
import axios, { Method, AxiosInstance } from 'axios';

enum APIChannels {
	Production = "Production",
	Staging = "Staging",
	Development = "Development",
}


class Service {

	public endpointPath = "http://api-dev.theskypark.com/";
	public endpoint = this.endpointPath;
	public channel = APIChannels.Production;
	public axios = axios.create({
		baseURL: this.endpointPath
	});
	private forceRemoteWS = false;
	private vue: _Vue;

	constructor(Vue: _Vue) {
		this.vue = Vue;

		if(this.forceRemoteWS) {
			console.warn("forceRemoteWS set to TRUE in api.ts");
		}
	}

	public updateToken(token: string) {
		this.axios.defaults.headers.common = { 'Authorization': "Bearer " + token }
	}

	public SendWS(name: string, message: object, callbackFn?: Function, callbackType?: number) {
		if(this.vue.$root.$wsLocalService.WSReady && !this.forceRemoteWS) {
			this.vue.$root.$wsLocalService.Send(name, message, callbackFn, callbackType);
		} else if(this.vue.$root.$wsRemoteService.WSReady) {
			this.vue.$root.$wsRemoteService.Send(name, message, callbackFn, callbackType);
		} else {
			console.error("--> WS message dropped");
		}
	}

}

export default {
	install: (Vue: typeof _Vue, options?: any) => {
		let installed = false;
		Vue.mixin({
			beforeCreate() {
				if (!installed) {
					installed = true;
					Vue.prototype.$apiService = new Service(this);
				}
			}
		});
	}
};

declare module 'vue/types/vue' {
	interface Vue {
		$apiService: Service;
	}
}
