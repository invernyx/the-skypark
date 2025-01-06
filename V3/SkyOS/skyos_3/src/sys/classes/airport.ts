export default class Airport {
	public name = null as string
	public icao = null as string
	public country = null as string
	public country_name = null as string
	public state = null as string
	public city = null as string
	public location = null as number[]
	public elevation = null as number
	public radius = null as number
	public runways = null as Runway[]
	public parkings = null as Parking[]
	public wx = null as any

	constructor(init?:Partial<Airport>) {
        Object.assign(this, init);
	}
}

export class Runway {
	heading: number
	length: number
	lit: boolean
	location: number[]
	primary_ils: boolean
	primary_name: string
	secondary_ils: boolean
	secondary_name: string
	surface: string
	width: number
}

export class Parking {
	airline_codes: string[]
	diameter: number
	heading: number
	location: number[]
	name: string
	number: number
}