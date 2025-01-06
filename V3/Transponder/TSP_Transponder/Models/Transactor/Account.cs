using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.Notifications;

namespace TSP_Transponder.Models.Transactor
{
    public class Account
    {
        [BsonId]
        public string AccountIdent { get; set; } = "";
        [BsonField("AccountName")]
        public string AccountName { get; set; } = "";
        [BsonField("Balance")]
        public float Balance { get; set; } = 0;
        [BsonField("StartDate")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        internal float MaxLimit = float.MaxValue;
        internal float MinLimit = float.MinValue;
        internal bool IsInitial = false;
        internal bool CanNotify = false;

        public Account()
        {

        }

        public Account(string Name, string Ident, bool CanNotify = false, float InitialBalance = 0, float MinLimit = float.MinValue, float MaxLimit = float.MaxValue)
        {
            AccountIdent = Ident;
            AccountName = Name;
            this.CanNotify = CanNotify;
            this.MinLimit = MinLimit;
            this.MaxLimit = MaxLimit;

            lock(LiteDbService.DB)
            {
                var Transactions = LiteDbService.DB.Database.GetCollection<Transaction>("account_" + AccountIdent);

                var Accounts = LiteDbService.DB.Database.GetCollection<Account>("accounts");
                var ExistingAccount = Accounts.FindOne(x => x.AccountIdent == AccountIdent);
                if (ExistingAccount == null)
                {
                    if (InitialBalance > 0)
                    {
                        Transact(new Transaction(0, InitialBalance, "Initial Balance", this, ""));
                    }
                    CalculateBalance();
                    IsInitial = true;
                }
                else
                {
                    StartDate = ExistingAccount.StartDate;
                    Balance = ExistingAccount.Balance;
                }
            }

            LoadStatsFile();
            ValidateLimits();
        }

        private void ValidateLimits()
        {
            if (Balance > MaxLimit)
            {
                Transact(new Transaction()
                {
                    Title = "Max Limit Adjustment",
                    InitialAmount = MaxLimit - Balance,
                    OtherParty = "Account Manager"
                });
                Balance = MaxLimit;
            }

            if (Balance < MinLimit)
            {
                Transact(new Transaction()
                {
                    Title = "Min Limit Adjustment",
                    InitialAmount = MinLimit - Balance,
                    OtherParty = "Account Manager"
                });
                Balance = MinLimit;
            }
        }

        private void LoadStatsFile()
        {
            if (File.Exists(Path.Combine(App.AppDataDirectory, "STATS.txt")) && UserData.Get("tier") != "discovery")
            {
                try
                {
                    using (StreamReader r = new StreamReader(new FileStream(Path.Combine(App.AppDataDirectory, "STATS.txt"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        string str = r.ReadToEnd();
                        string[] strSpl = str.Split('\n');
                        foreach (string entry in strSpl)
                        {
                            string[] strSpl2 = entry.Split(':');
                            if(strSpl2[0].Trim() == "account_" + AccountIdent)
                            {
                                float DesiredBalance = Convert.ToSingle(strSpl2[1]);
                                if(DesiredBalance != Balance)
                                {
                                    Transact(new Transaction()
                                    {
                                        Title = DesiredBalance - Balance > 0 ? "Stimulus payment" : "Stimulus repayment",
                                        InitialAmount = DesiredBalance - Balance,
                                        OtherParty = "Federal Reserve"
                                    });
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }
                
        public void UpdateAccountMeta()
        {
            lock(LiteDbService.DB)
            {
                var Transactions = LiteDbService.DB.Database.GetCollection<Account>("accounts");
                Transactions.Upsert(this);
            }
        }

        public void CalculateBalance()
        {
            float bl = 0;
            lock (LiteDbService.DB)
            {
                var Transactions = LiteDbService.DB.Database.GetCollection<Transaction>("account_" + AccountIdent);
                foreach (Transaction tx in Transactions.FindAll())
                {
                    bl += tx.NetAmount;
                }
            }

            Balance = bl;
            
            UpdateAccountMeta();
        }
        
        public void BroadcastMeta(Transaction Transaction)
        {
            if(APIBase.ClientCollection != null)
            {
                Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
                {
                    { "balance", Balance },
                    { "account_name", AccountName },
                    { "transaction", Transaction.ToListing() }
                };
                APIBase.ClientCollection.SendMessage("bank:transaction", App.JSSerializer.Serialize(rs), null, APIBase.ClientType.Skypad);
            }
        }

        public void Transact(Transaction Transaction)
        {
            lock (LiteDbService.DB)
            {
                var Transactions = LiteDbService.DB.Database.GetCollection<Transaction>("account_" + AccountIdent);

                if (Balance + Transaction.NetAmount < MinLimit)
                {
                    Transaction.InitialAmount += (MinLimit - Balance) - (Transaction.NetAmount);
                }
                if (Balance + Transaction.NetAmount > MaxLimit)
                {
                    Transaction.InitialAmount += (MaxLimit - Balance) - (Transaction.NetAmount);
                }

                Transactions.Upsert(Transaction);
            }

            Balance += Transaction.NetAmount;
            UpdateAccountMeta();
            BroadcastMeta(Transaction);

            if(AccountIdent == "bank_checking")
            {
                if (Transaction.NetAmount != 0)
                {
                    if (Transaction.NetAmount > 0)
                    {
                        NotificationService.Add(new Notification()
                        {
                            Title = "Money was deposited in your bank account",
                            Message = "$" + Transaction.NetAmount + " was deposited from " + Transaction.OtherParty + ".",
                            Type = NotificationType.Status,
                            AppName = "p42_holdings",
                            CanOpen = true,
                            IsTransponder = true,
                        });
                    }
                    else
                    {
                        NotificationService.Add(new Notification()
                        {
                            Title = "Money was withdrawn from your bank account",
                            Message = "$" + (-Transaction.NetAmount) + " was sent to " + Transaction.OtherParty + ".",
                            Type = NotificationType.Status,
                            AppName = "p42_holdings",
                            CanOpen = true,
                            IsTransponder = true,
                        });
                    }
                }
            }
        }

        public List<Dictionary<string, dynamic>> GetTrend(TimeSpan Span)
        {
            List<KeyValuePair<DateTime, List<Transaction>>> Trend = new List<KeyValuePair<DateTime, List<Transaction>>>();
            while(Trend.Count < Span.TotalDays)
            {
                DateTime DateOffset = DateTime.UtcNow.AddDays(-Trend.Count);
                DateTime DateDay = new DateTime(DateOffset.Year, DateOffset.Month, DateOffset.Day, 0, 0, 0, DateTimeKind.Utc);
                Trend.Add(new KeyValuePair<DateTime, List<Transaction>>(DateDay, new List<Transaction>()));
            }

            lock (LiteDbService.DB)
            {
                var Transactions = LiteDbService.DB.Database.GetCollection<Transaction>("account_" + AccountIdent);
                DateTime target = DateTime.UtcNow.Subtract(Span);
                foreach (Transaction Tr in Transactions.Find(x => x.Date > target))
                {
                    DateTime DateDay = new DateTime(Tr.Date.Year, Tr.Date.Month, Tr.Date.Day, 0, 0, 0, DateTimeKind.Utc);
                    List<Transaction> TrendSelect = Trend.Find(x => x.Key.Ticks == DateDay.Ticks).Value;
                    if (TrendSelect != null)
                    {
                        TrendSelect.Add(Tr);
                    }
                }
            }

            List<Dictionary<string, dynamic>> Struct = new List<Dictionary<string, dynamic>>();
            float TrendValue = 0;
            Trend.Reverse();
            foreach (var TrendNode in Trend)
            {
                var Values = TrendNode.Value.Select(x => x.NetAmount);
                foreach(float Val in Values)
                {
                    TrendValue += Val;
                }
                Struct.Add(new Dictionary<string, dynamic>()
                {
                    { "Date", TrendNode.Key.ToString("O") },
                    { "Value", TrendValue },
                });
            }
            
            return Struct;
        }

        public void LoadLegacy(Dictionary<string, dynamic> Persisted) {

            if(IsInitial)
            {
                lock(LiteDbService.DB)
                {
                    var Transactions = LiteDbService.DB.Database.GetCollection<Transaction>("account_" + AccountIdent);
                    Transactions.DeleteAll();

                    AccountName = Persisted["AccountName"];
                    Transaction RolloverBalance = new Transaction((Dictionary<string, dynamic>)Persisted["RolloverBalance"], this);
                    if (RolloverBalance.InitialAmount != 0)
                    {
                        Transactions.Upsert(RolloverBalance);
                    }

                    foreach (Dictionary<string, dynamic> tx in Persisted["Transactions"])
                    {
                        if (!double.IsNaN(Convert.ToDouble(tx["InitialAmount"])))
                        {
                            Transaction tx1 = new Transaction(tx, this);
                            if (StartDate > tx1.Date)
                            {
                                StartDate = tx1.Date;
                            }
                            Transactions.Upsert(tx1.ID, tx1);
                        }

                    }
                }
                
                CalculateBalance();
            }
        }

        public Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "AccountName", AccountName },
                { "Balance", Balance },
                { "Transactions", new List<Dictionary<string, dynamic>>() },
            };

            //foreach (Transaction tx in Transactions)
            //{
            //    rs["Transactions"].Add(tx.ToDictionary());
            //}

            return rs;
        }

        public Dictionary<string, dynamic> ToTrend()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "balance", Balance },
                { "date_start", new DateTime(StartDate.Date.Year, StartDate.Date.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(StartDate.Date.Day - 2).ToString("O") },
                { "trend", GetTrend(new TimeSpan(15,0,0,0)) },
            };
            
            return rs;
        }

        public Dictionary<string, dynamic> ToListing(int limit, DateTime start)
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "account_name", AccountName },
                { "balance", Balance },
                { "transactions", new List<Dictionary<string, dynamic>>() },
            };

            lock (LiteDbService.DB)
            {
                var Transactions = LiteDbService.DB.Database.GetCollection<Transaction>("account_" + AccountIdent);

                foreach (Transaction tx in Transactions.Query()
                .OrderByDescending(x => x.Date)
                .Where(x => x.Date < start)
                .Limit(limit)
                .ToList())
                {
                    rs["transactions"].Add(tx.ToListing());
                }
            }

            return rs;
        }

        public override string ToString()
        {
            return Balance + " on account " + AccountIdent;
        }
    }
}
