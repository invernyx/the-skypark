
export class Invoice {

	public id :number;
	public status :string;
	public payee_type :string;
	public payee_account :string;
	public client_type :string;
	public client_account :string;
	public date :Date;

	public fees = null;
	public liability = null;
	public refunds = null;
}

export class Fee {
	public code :string;
	public refund_moment :string
	public params :Object
	public incl_if :String
	public excl_if :String
	public refunded :Boolean
	public required :Boolean
	public discounts :Discount[]

}

export class Discount {
	public code :string;
	public amount :string;
	public params :string;
}


export default class Invoices {

	public total_liabilities = null;
	public total_refunds = null;

	public total_fees = null;
	public total_profits = null;
	public uncertain_refunds = null;
	public uncertain_fees = null;
	public uncertain_liabilities = null;

	public invoices :Invoice[] = null;

	constructor(init?:Partial<Invoices>) {
        Object.assign(this, init);
	}
}
