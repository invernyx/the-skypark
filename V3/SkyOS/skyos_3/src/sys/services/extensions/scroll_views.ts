import _Vue from 'vue';

export default class ScrollView {
	public os: any;
	public vue: _Vue;

	private entities = [] as {
		sid :string,
		entity: any
	}[];

	constructor(os :any, vue :_Vue) {
		this.os = os;
		this.vue = vue;
	}

	public startup() {
		this.os.eventsBus.Bus.on('scrollview', (e) => this.listener_scrollview(e, this));
	}

	public add(sid :string, entitiy :any) {
		const existing = this.entities.find(x => x.sid == sid);
		if(!existing) {
			this.entities.push({
				sid: sid,
				entity: entitiy
			})
		}
	}

	public remove(sid :string) {
		const existing = this.entities.find(x => x.sid == sid);
		if(existing)
			this.entities.splice(this.entities.indexOf(existing), 1);
	}

	public get_entity(sid :string) {
		const existing = this.entities.find(x => x.sid == sid);
		if(existing){
			return existing.entity;
		} else {
			return null;
		}
	}

	public get_relative_offset(sid :string, anchor :string) {
		const existing = this.entities.find(x => x.sid == sid) as any;
		if(existing){
			const element = existing.entity.$el.querySelector("[data-anchor='" + anchor + "']");

			if(element) {
				const existing_offset_raw = existing.entity.$el.getBoundingClientRect();
				const element_offset_raw = element.getBoundingClientRect();

				const existing_scroll = [existing.entity.SimpleBar.contentWrapperEl.scrollTop, existing.entity.SimpleBar.contentWrapperEl.scrollLeft];
				const element_offset = [element_offset_raw.top - existing_offset_raw.top + existing_scroll[0], element_offset_raw.left - existing_offset_raw.left + existing_scroll[1]];

				return element_offset;
			}
		}

		return null;
	}

	public get_scroll_offset_top(sid :string) {
		const existing = this.entities.find(x => x.sid == sid) as any;
		if(existing){
			const scroll_element = existing.entity.SimpleBar.contentWrapperEl;
			return scroll_element.scrollTop;
		} else {
			return null;
		}
	}

	public scroll_to_el(sid: string, anchor :string, offset :number[], duration :number) {
		const r_offset = this.get_relative_offset(sid, anchor);
		if(r_offset) {
			this.scroll_to(sid, r_offset[1] + offset[1], r_offset[0] + offset[0], duration);
		}
	}

	public scroll_to(sid: string, x :number, y :number, duration :number) {
		this.os.eventsBus.Bus.emit('scrollview', { name: ['scroll'], payload: {
			sid: sid,
			event: 'set',
			x: x,
			y: y,
			duration: duration
		}});
	}

	public listener_scrollview(wsmsg: any, self: ScrollView){
		//switch(wsmsg.event){
		//	case 'changed': {
		//		break;
		//	}
		//	case 'set': {
		//		break;
		//	}
		//}
	}

}