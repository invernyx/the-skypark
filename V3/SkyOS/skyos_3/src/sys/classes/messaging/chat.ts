import Vue from "vue"
import Handle from "./handle"
import Message from "./message"

export default class Chat {
	public id :number
	public handles :Handle[]
	public messages :Message[]
	public last_message :Message
	public read_at_date :Date

	constructor(init?:Partial<Chat>) {
        Object.assign(this, init);
	}
}