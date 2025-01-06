import Vue from "vue";

import navigation_bar from "./../components/navigation_bar.vue";
import tab_bar from "./../components/tab_bar.vue";
import scroll_view from "./../components/scroll_view.vue";
import split_view from "./../components/split_view.vue";
import sidebar from "./../components/sidebar.vue";
import selector from "./../components/selector.vue";
import countdown from "./../components/countdown.vue";
import flags from "./../components/flags.vue";
import icons from "./../components/icons.vue";
import weather from "./../components/weather.vue";
import modal from "../components/modals/modal.vue";
import toggle from "./../components/toggle.vue";
import slider from "./../components/slider.vue";
import width_limiter from "./../components/width_limiter.vue";
import content_controls_stack from "./../components/content_controls_stack.vue";
import collapser from "./../components/collapser.vue";
import label_collapser from "./../components/label_collapser.vue";

import button_action from "./../components/button_action.vue"; // Regular custom buttons
import button_nav from "./../components/button_nav.vue"; // Nav buttons
import button_listed from "./../components/button_listed.vue"; // List buttons
import button_sidebar from "./../components/button_sidebar.vue"; // Sidebar (nav) buttons

import textbox from "./../components/textbox.vue";

export default () => {

	Vue.component("navigation_bar", navigation_bar)
	Vue.component("tab_bar", tab_bar)
	Vue.component("scroll_view", scroll_view)
	Vue.component("split_view", split_view)
	Vue.component("sidebar", sidebar)
	Vue.component("selector", selector)
	Vue.component("countdown", countdown)
	Vue.component("flags", flags)
	Vue.component("icons", icons)
	Vue.component("weather", weather)
	Vue.component("modal", modal)
	Vue.component("toggle", toggle)
	Vue.component("slider", slider)
	Vue.component("width_limiter", width_limiter)
	Vue.component("content_controls_stack", content_controls_stack)
	Vue.component("collapser", collapser)
	Vue.component("label_collapser", label_collapser)

	Vue.component("button_action", button_action)
	Vue.component("button_nav", button_nav)
	Vue.component("button_listed", button_listed)
	Vue.component("button_sidebar", button_sidebar)

	Vue.component("textbox", textbox)
};