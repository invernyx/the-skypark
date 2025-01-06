
export default class Template {

	public name = null;
	public file_name = null;
	public is_custom = null;
	public modified_on = null as Date;
	public description = null;
	public image_url = null as string[];
	public gallery_url = null as string[];
	public type = null;
	public pois = null;
	public company = null;
	public type_label = null;
	public template_code = null;
	public running_clock = null;
	public strict_order = false;
	public time_to_complete = null;
	public aircraft_restriction_label = null;
	public situations = null;
	public requires_level = null;
	public requires_karma = null;
	public requires_reliability = null;

	constructor(init?:Partial<Template>) {
        Object.assign(this, init);
	}
}






