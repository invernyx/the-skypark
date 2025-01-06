import Vue from 'vue'
import VueRouter, { RouteConfig } from 'vue-router'
import { AppInfo, AppEvents } from "../foundation/app_bundle"

Vue.use(VueRouter)

const routes: Array<RouteConfig> = [
	{ path: '*', redirect: '/z' },
]
const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

router.afterEach((to, from) => {
  if(from.matched.length > 0){
    (from.matched[0].props["default"].app.app_events as AppEvents).close();
    from.matched[0].props["default"].app.app_events = new AppEvents();
  }
  if(to.matched.length > 0){
	((to.matched[0].props["default"].app) as AppInfo).StateLoad();
	(to.matched[0].props["default"].app.app_events as AppEvents).open();
  }
});

export default router
