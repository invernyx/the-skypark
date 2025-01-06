
interface NotificationConfig {
	UID?: number;
	Type?: string;
	Time?: Date;
	Expire?: Date;
	CanOpen?: boolean;
	CanDismiss?: boolean;
	LaunchArgument?: string;
	App?: string;
	Title?: string;
	Message?: string;
	Group?: string;
	Data?: any;
  }

export default class Notification implements NotificationConfig {

	public UID: number;
	public Type: string;
	public Time: Date;
	public Expire?: Date;
	public CanOpen: boolean;
	public CanDismiss: boolean;
	public LaunchArgument: string;
	public App = 'p42_system';
	public Title: string;
	public Message: string;
	public Group: string;
	public Data: any;

	public constructor(config :NotificationConfig) {

		Object.assign(this, config);
	}

}