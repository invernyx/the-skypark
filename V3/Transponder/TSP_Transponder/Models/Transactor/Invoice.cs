using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Contractors;
using static TSP_Transponder.App;
using static TSP_Transponder.Attributes.EnumAttr;

namespace TSP_Transponder.Models.Transactor
{
    public class Invoice
    {
        private static long InvoiceCount = 0;

        [BsonId]
        public long ID { get; set; } = DateTime.UtcNow.Ticks;
        [BsonField("Title")]
        public string Title { get; set; } = "";
        [BsonField("Description")]
        public string Description { get; set; } = "";
        [BsonField("Link")]
        public long Link { get; set; } = 0;
        [BsonField("Status")]
        public STATUS Status { get; set; } = STATUS.QUOTE;
        [BsonField("Term")]
        public float Term { get; set; } = 0;

        [BsonField("PayeeType")]
        public ACCOUNTTYPE PayeeType { get; set; } = ACCOUNTTYPE.NONE;
        [BsonField("PayeeAccount")]
        public string PayeeAccount { get; set; } = null;
        
        [BsonField("ClientType")]
        public ACCOUNTTYPE ClientType { get; set; } = ACCOUNTTYPE.NONE;
        [BsonField("ClientAccount")]
        public string ClientAccount { get; set; } = null;

        [BsonField("Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        [BsonField("PaidDate")]
        public DateTime? PaidDate { get; set; } = null;
        [BsonField("Fees")]
        public List<Fee> Fees { get; set; } = new List<Fee>();
        [BsonField("Refund")]
        public List<Fee> Refunds { get; set; } = new List<Fee>();
        [BsonField("Liability")]
        public List<Fee> Liability { get; set; } = new List<Fee>();
        
        public MOMENT MomentCondition { get; set; } = MOMENT.NONE;

        public float Balance
        {
            get {
                float Cumul = 0;
                foreach (Fee f in Fees)
                {
                    Cumul += f.Amount != null ? (float)f.Amount + (f.Discounts != null ? (float)f.Discounts.Sum(x => x.Amount) : 0) : 0;
                }
                return Cumul;
            }
        }

        public float RefundBalance
        {
            get
            {
                float Cumul = 0;
                foreach (Fee f in Refunds)
                {
                    Cumul += f.Amount != null ? (float)f.Amount : 0;
                }
                return Cumul;
            }
        }


        public Invoice()
        {
            ID = DateTime.UtcNow.Ticks + InvoiceCount;
            InvoiceCount++;
        }

        public void AddEntry(Fee newFee)
        {
            lock(Fees)
            {
                int Index = 0;
                Fee Existing = Fees.Find(x => x.Code == newFee.Code);

                if (Existing != null)
                {
                    Index = Fees.IndexOf(Existing);
                    Fees.Remove(Existing);
                    Fees.Insert(Index, newFee);
                }
                else
                {
                    Fees.Add(newFee);
                }
            }
        }

        public void RemoveEntry(Fee fee)
        {
            Fees.Remove(fee);
        }
        
        private string FindPayeeName()
        {
            string PayeeName = "Unknown";
            switch (PayeeType)
            {
                case ACCOUNTTYPE.AGENCY:
                case ACCOUNTTYPE.SERVICE:
                    {
                        Company Co = Companies.List.Find(x => x.Code == PayeeAccount);
                        if (Co != null)
                        {
                            PayeeName = Co.Name;
                        }
                        break;
                    }
            }
            return PayeeName;
        }

        public void Pay(float Days = 0)
        {
            if(Days == 0)
            {
                // Remove Undevined fees
                List<Fee> UndefinedFees = Fees.FindAll(x => x.Amount == null);
                foreach(Fee fee in UndefinedFees)
                {
                    Fees.Remove(fee);
                }

                // Find Payee name
                string PayeeName = FindPayeeName();

                // Pay based on Client info
                switch (ClientType)
                {
                    case ACCOUNTTYPE.PRIVATE:
                        {
                            List<Fee> fees = new List<Fee>();
                            foreach(var fee in Fees)
                            {
                                Fee nf = new Fee(fee.ToDictionary());
                                nf.Amount = -nf.Amount;
                                fees.Add(nf);

                                if(fee.Discounts != null)
                                {
                                    foreach (var d in fee.Discounts)
                                    {
                                        Fee df = new Fee();
                                        df.Amount = -d.Amount;
                                        df.Code = d.Code;
                                        df.Params = d.Params;
                                        fees.Add(df);
                                    }
                                }
                            }

                            Account FoundAccount = Bank.Bank.Accounts.Find(x => x.AccountIdent == ClientAccount);
                            if(FoundAccount != null)
                            {
                                if (Balance != 0)
                                {
                                    FoundAccount.Transact(new Transaction()
                                    {
                                        InvoiceID = ID,
                                        Title = Title,
                                        Description = Description,
                                        InitialAmount = 0,
                                        OtherParty = PayeeName,
                                        Fees = fees
                                    });
                                }
                            }
                            break;
                        }
                }
                Status = STATUS.PAID;
                PaidDate = DateTime.UtcNow;

                #region BroadcastEvent
                Loadmaster.InvoicePaid(this);
                #endregion
            }
            else
            {
                Status = STATUS.OPEN;
                Term = Days;
            }
            Invoices.Upsert(this);
        }

        public bool CheckRefund(MOMENT moment)
        {
            if(Refunds.Count > 0)
            {
                foreach(var refund in Refunds.Where(x => x.RefundMoment == moment))
                {
                    // Find Payee name
                    string PayeeName = FindPayeeName();

                    // Pay based on Client info
                    switch (ClientType)
                    {
                        case ACCOUNTTYPE.PRIVATE:
                            {
                                Account FoundAccount = Bank.Bank.Accounts.Find(x => x.AccountIdent == ClientAccount);
                                if (FoundAccount != null)
                                {
                                    if (Balance != 0)
                                    {
                                        FoundAccount.Transact(new Transaction()
                                        {
                                            InvoiceID = ID,
                                            Title = Title,
                                            Description = Description,
                                            IsRefund = true,
                                            InitialAmount = -(float)refund.Amount,
                                            OtherParty = PayeeName
                                        });
                                    }
                                }
                                break;
                            }
                    }

                    Status = STATUS.REFUNDED;
                    refund.Refunded = true;
                    Invoices.Upsert(this);

                    //Status = STATUS.REFUNDED;
                    //Status = STATUS.PARTREFUNDED;
                }
            }
            
            return true;
        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "status", GetDescription(Status) },
                { "payee_type", GetDescription(PayeeType) },
                { "payee_account", PayeeAccount },
                { "client_type", GetDescription(ClientType) },
                { "client_account", ClientAccount },
                { "date", Date.ToString("O") },
            };

            if (Fees.Count > 0)
            {
                rs.Add("fees", new List<Dictionary<string, dynamic>>());
                foreach (Fee f in Fees)
                {
                    rs["fees"].Add(f.ToListing());
                }
            }

            if (Liability.Count > 0)
            {
                rs.Add("liability", new List<Dictionary<string, dynamic>>());
                foreach (Fee f in Liability)
                {
                    rs["liability"].Add(f.ToListing());
                }
            }

            if (Refunds.Count > 0)
            {
                rs.Add("refunds", new List<Dictionary<string, dynamic>>());
                foreach (Fee f in Refunds)
                {
                    rs["refunds"].Add(f.ToListing());
                }
            }

            return rs;
        }

        public enum ACCOUNTTYPE
        {
            [EnumValue("NONE")]
            NONE = 0,
            [EnumValue("AGENCY")]
            AGENCY = 1,
            [EnumValue("SERVICE")]
            SERVICE = 2,
            [EnumValue("PRIVATE")]
            PRIVATE = 3,
        }

        public enum STATUS
        {
            [EnumValue("QUOTE")]
            QUOTE = 0,
            [EnumValue("OPEN")] //Standard = 10, 30, 90
            OPEN = 1,
            [EnumValue("PAID")]
            PAID = 2,
            [EnumValue("LATE")]
            LATE = 3,
            [EnumValue("REFUNDED")]
            REFUNDED = 4,
            [EnumValue("PARTREFUNDED")]
            PARTREFUNDED = 5,
        }

        public enum MOMENT
        {
            [EnumValue("NONE")]
            NONE = 0,
            [EnumValue("START")]
            START = 1,
            [EnumValue("END")]
            END = 2,
            [EnumValue("SUCCEED")]
            SUCCEED = 3,
            [EnumValue("FAIL")]
            FAIL = 4,
        }
    }
}
