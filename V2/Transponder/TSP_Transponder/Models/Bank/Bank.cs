using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Transactor;
using static TSP_Transponder.Models.API.WSLocal;

namespace TSP_Transponder.Models.Bank
{
    class Bank
    {
        internal static List<Account> Accounts = null;

        internal static void Startup()
        {
            Accounts = new List<Account>()
            {
                new Account("Checking", "bank_checking", true)
            };
        }

        public static void Transact(Transaction Transaction, Account Account)
        {
            Account.Transact(Transaction);
        }

        public static Account GetAccount(string AccountName)
        {
            return Accounts.Find(x => x.AccountName == AccountName);
        }

        public static void WSConnected(SocketClient Socket)
        {
            //Socket.SendMessage("bank:accounts", App.JSSerializer.Serialize(ToDictionary()), null);
            
            //Transact(new Transaction(0, 1000, "Test", GetAccount("Checking"), "Unknown"));
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "accounts":
                    {
                        Socket.SendMessage("bank:accounts", App.JSSerializer.Serialize(ToListing(20, DateTime.UtcNow)), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "trend":
                    {
                        List<List<Dictionary<string, dynamic>>> RetTrend = new List<List<Dictionary<string, dynamic>>>();
                        if (payload_struct.ContainsKey("account"))
                        {
                            RetTrend.Add(GetAccount((string)payload_struct["account"]).GetTrend(new TimeSpan(180, 0, 0, 0)));
                        }
                        else
                        {
                            foreach (Account Acct in Accounts)
                            {
                                RetTrend.Add(Acct.GetTrend(new TimeSpan(180,0,0,0)));
                            }
                        }
                        Socket.SendMessage("bank:trend", App.JSSerializer.Serialize(RetTrend), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "get":
                    {
                        List<Dictionary<string, dynamic>> RetAccount = new List<Dictionary<string, dynamic>>();
                        if (payload_struct.ContainsKey("account"))
                        {
                            RetAccount.Add(GetAccount((string)payload_struct["account"]).ToListing(20, DateTime.UtcNow));
                        }
                        else
                        {
                            foreach(Account Acct in Accounts)
                            {
                                RetAccount.Add(Acct.ToListing(20, DateTime.UtcNow));
                            }
                        }
                        Socket.SendMessage("bank:get", App.JSSerializer.Serialize(RetAccount), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }
        
        public static List<Dictionary<string, dynamic>> ToListing(int limit, DateTime start)
        {
            List<Dictionary<string, dynamic>> AccountsList = new List<Dictionary<string, dynamic>>();
            foreach (var Account in Accounts)
            {
                AccountsList.Add(Account.ToListing(limit, start));
            }

            return AccountsList;
        }
        
        public static void RestoreLegacyPersistence(bool OpenBackup)
        {
            string PersistTempPath = Path.Combine(App.AppDataDirectory, "c049ccb.dat");
            string PersistPath = Path.Combine(App.DocumentsDirectory, "c049ccb.dat");
            
            FileInfo PersistMain = new FileInfo(PersistPath);
            FileInfo PersistSecond = new FileInfo(PersistTempPath);

            string OpenPath = PersistPath;

            if (OpenBackup)
            {
                OpenPath = PersistSecond.FullName;
            }

            try
            {
                if(File.Exists(OpenPath))
                {
                    using (StreamReader r = new StreamReader(new FileStream(OpenPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        var json = r.ReadToEnd();
                        List<Dictionary<string, dynamic>> Persisted = App.JSSerializer.Deserialize<List<Dictionary<string, dynamic>>>(Utils.Decrypt(json, "415925e1-097d-492a-bd57-31cdd68ab265"));
                            
                        foreach (Account Account in Accounts)
                        {
                            var PersistedOne = Persisted.Find(x => x["AccountName"] == Account.AccountName);
                            if (PersistedOne != null)
                            {
                                Account.LoadLegacy(PersistedOne);
                            }
                        }
                    }
                }
            }
            catch
            {
                if(OpenBackup)
                {
                    MessageBox.Show("Failed to load your bank data. Please send the Console files located in " + App.AppDataDirectory + " to Parallel 42 via the official support portal on their website. Sorry for the inconvenience.", "Failed to load The Skypark", MessageBoxButton.OK, MessageBoxImage.Stop);
                    Environment.Exit(0);
                }
                else
                {
                    RestoreLegacyPersistence(true);
                    return;
                }
            }

            double Balance = GetAccount("Checking").Balance;
            if (Balance > 10000000)
            {
                GoogleAnalyticscs.TrackEvent("Progress", "Bank", (Math.Round(Balance / 10000000) * 10000000).ToString(), (int)Balance);
            }
            else if(Balance > 1000000)
            {
                GoogleAnalyticscs.TrackEvent("Progress", "Bank", (Math.Round(Balance / 1000000) * 1000000).ToString(), (int)Balance);
            }
            else if (Balance > 100000)
            {
                GoogleAnalyticscs.TrackEvent("Progress", "Bank", (Math.Round(Balance / 100000) * 100000).ToString(), (int)Balance);
            }
            else if (Balance > 10000)
            {
                GoogleAnalyticscs.TrackEvent("Progress", "Bank", (Math.Round(Balance / 10000) * 10000).ToString(), (int)Balance);
            }
            else
            {
                GoogleAnalyticscs.TrackEvent("Progress", "Bank", (Math.Round(Balance / 1000) * 1000).ToString(), (int)Balance);
            }
        }
        
    }
}
