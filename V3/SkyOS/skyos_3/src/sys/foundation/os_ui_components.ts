import Vue from "vue";

import app_icon from "./../components/app_icon.vue";
import scroll_stack_top from "./../components/scroll_stack_top.vue";
import scroll_stack_bottom from "../components/scroll_stack_bottom.vue";
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
import scroll_stack from "./../components/scroll_stack.vue";
import collapser from "./../components/collapser.vue";
import label_collapser from "./../components/label_collapser.vue";
import textbox from "./../components/textbox.vue";
import data_stack from "./../components/data_stack.vue";
import pagination from "./../components/pagination.vue";
import progress_bar from "./../components/progress.vue";

import app_header from "./../components/app_header/header.vue";
import app_panel from "./../components/app_panel.vue";

import button_action from "./../components/button_action.vue"; // Regular custom buttons
import button_tile from "./../components/button_tile.vue"; // Regular custom buttons
import button_nav from "./../components/button_nav.vue"; // Nav buttons
import button_listed from "./../components/button_listed.vue"; // List buttons
import button_sidebar from "./../components/button_sidebar.vue"; // Sidebar (nav) buttons

import currency from "./../components/units/currency.vue";
import time_date from "./../components/units/time_date.vue";
import time_hour from "./../components/units/time_hour.vue";
import duration from "./../components/units/duration.vue";
import expire from "./../components/units/expire.vue";
import distance from "./../components/units/distance.vue";
import height from "../components/units/height.vue";
import number from "../components/units/number.vue";
import heading from "../components/units/heading.vue";
import length from "../components/units/length.vue";
import weight from "../components/units/weight.vue";

export default () => {

	Vue.component("app_icon", app_icon)
	Vue.component("scroll_stack_top", scroll_stack_top)
	Vue.component("scroll_stack_bottom", scroll_stack_bottom)
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
	Vue.component("scroll_stack", scroll_stack)
	Vue.component("collapser", collapser)
	Vue.component("label_collapser", label_collapser)
	Vue.component("textbox", textbox)
	Vue.component("data_stack", data_stack)
	Vue.component("pagination", pagination)
	Vue.component("progress_bar", progress_bar)

	Vue.component("app_header", app_header)
	Vue.component("app_panel", app_panel)

	Vue.component("button_action", button_action)
	Vue.component("button_tile", button_tile)
	Vue.component("button_nav", button_nav)
	Vue.component("button_listed", button_listed)
	Vue.component("button_sidebar", button_sidebar)

	Vue.component("currency", currency)
	Vue.component("time_date", time_date)
	Vue.component("time_hour", time_hour)
	Vue.component("duration", duration)
	Vue.component("expire", expire)
	Vue.component("distance", distance)
	Vue.component("height", height)
	Vue.component("number", number)
	Vue.component("heading", heading)
	Vue.component("length", length)
	Vue.component("weight", weight)
};