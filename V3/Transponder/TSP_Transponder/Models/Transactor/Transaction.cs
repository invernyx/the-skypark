using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace TSP_Transponder.Models.Transactor
{
    public class Transaction
    {
        public Account Account = null;

        [BsonId]
        public int ID { get; set; } = 0;
        [BsonField("InvoiceID")]
        public long InvoiceID { get; set; } = 0;
        [BsonField("IsRefund")]
        public bool IsRefund { get; set; } = false;
        [BsonField("Title")]
        public string Title { get; set; } = "";
        [BsonField("Description")]
        public string Description { get; set; } = "";
        [BsonField("InitialAmount")]
        public float InitialAmount { get; set; } = 0;
        [BsonField("Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;
        [BsonField("Fees")]
        public List<Fee> Fees { get; set; } = new List<Fee>();
        [BsonField("OtherParty")]
        public string OtherParty { get; set; } = "";
        [BsonField("NetAmount")]
        public float NetAmount
        {
            get
            {
                float bl = InitialAmount;
                foreach (Fee fee in Fees)
                {
                    bl += (float)fee.Amount;
                }

                return bl;
            }
        }


        public Transaction()
        {

        }

        public Transaction(int ID, float InitialAmount, string Title, Account Account, string OtherParty)
        {
            this.Account = Account;
            this.ID = ID;
            this.InitialAmount = InitialAmount;
            this.Title = Title;
            this.OtherParty = OtherParty;
        }

        public Transaction(int ID, float InitialAmount, Account Account)
        {
            this.Account = Account;
            this.ID = ID;
            this.InitialAmount = InitialAmount;
        }

        public Transaction(Dictionary<string, dynamic> Restore, Account Account)
        {
            // Legacy

            this.Account = Account;
            ID = (int)Restore["ID"];
            Title = (string)Restore["Description"];
            InitialAmount = (float)Restore["InitialAmount"];
            Date = DateTime.Parse((string)Restore["Date"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            OtherParty = (string)Restore["OtherParty"];

            foreach(Dictionary<string, dynamic> Fee in Restore["Fees"])
            {
                Fees.Add(new Fee(Fee));
            }
        }

        public Dictionary<string, dynamic> ToListing()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "title", Title },
                { "description", Description },
                { "initial_amount", InitialAmount },
                { "is_refund", IsRefund },
                { "date", Date.ToString("O") },
                { "other_party", OtherParty },
                { "net_amount", NetAmount },
                { "fees", new List<Dictionary<string, dynamic>>() },
            };

            foreach (Fee fee in Fees)
            {
                rt["fees"].Add(fee.ToListing());
            }

            return rt;
        }

        public override string ToString()
        {
            return Title + " / " + NetAmount + " / " + Account.AccountName + " / " + Date.ToLongDateString();
        }

    }
}
