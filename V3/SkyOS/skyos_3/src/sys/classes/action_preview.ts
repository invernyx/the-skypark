export default class ActionPreviewData {
	public type = null as string
	public data = null as any

	constructor(init?:Partial<ActionPreviewData>) {
        Object.assign(this, init);

	}
}