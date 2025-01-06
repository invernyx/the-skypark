export default class Message {
	public id :number
	public chat_id :number
	public contract_id :number
	public contract_topic_ids :Number[]
	public from_self :Boolean
	public handle :number
	public situation_index :number
	public date :Date
	public audio_path :string
	public content :string
	public content_type :string

	constructor(init?:Partial<Message>) {
        Object.assign(this, init);
	}
}