import Vue from 'vue'
import VueRouter, { RouteConfig } from 'vue-router'
import { AppInfo, AppEvents } from "../foundation/app_model"

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
    (from.matched[0].props["load"].app.events as AppEvents).close();
  }
  if(to.matched.length > 0){
	(to.matched[0].props["load"].app.events as AppEvents).open();
  }
});


export default router
