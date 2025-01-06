using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Transactor;

namespace TSP_Transponder.Models.Progress
{
    class Progress
    {
        internal static DateTime ReliabilityUnlock = new DateTime(2000,1,1);
        internal static Account Reliability = null;
        internal static Account Karma = null;
        internal static Account XP = null;

        internal static void Startup()
        {
            Reliability = new Account("Reliability", "progress_reliability", false, 50, 0, 100);
            Karma = new Account("Karma", "progress_karma", false, 0, -42, 42);
            XP = new Account("XP", "progress_xp", false, 0, 0, 600000);
        }

        public static void Transact(float KarmaChange, float XPChange, float ReliabilityChange)
        {
            double PreviousXP = XP.Balance;

            Reliability.Transact(new Transaction(Utils.GetNumGUID(), ReliabilityChange, Reliability));
            XP.Transact(new Transaction(Utils.GetNumGUID(), XPChange, XP));
            Karma.Transact(new Transaction(Utils.GetNumGUID(), KarmaChange, Karma));

            APIBase.ClientCollection.SendMessage("progress:get", App.JSSerializer.Serialize(ToListing()), null, APIBase.ClientType.Skypad);
            

            if(Utils.CalculateLevel(PreviousXP) < 3 && Utils.CalculateLevel(XP.Balance) > 3)
            {
                NotificationService.Add(new Notification()
                {
                    Title = "Congratulations, you unlocked game modes!",
                    Message = "Choose from multiple modes, Discovery & Endeavour; you're currently on Endeavour. Switch between them at any time, and The Skypark will save your progression data independently. Go to the Settings app under the Gameplay section to learn more.",
                    Type = NotificationType.Status,
                    CanOpen = true,
                    AppName = "p42_settings_tier"
                });
            }
        }

        public static void UnlockReliability()
        {
            ReliabilityUnlock = DateTime.UtcNow.AddHours(6);
            APIBase.ClientCollection.SendMessage("progress:get", App.JSSerializer.Serialize(ToListing()), null, APIBase.ClientType.Skypad);
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "get":
                    {
                        Socket.SendMessage("progress:get", App.JSSerializer.Serialize(ToListing()), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }

        public static void WSConnected(SocketClient Socket)
        {
            Socket.SendMessage("progress:get", App.JSSerializer.Serialize(ToListing()));
        }

        public static Dictionary<string, dynamic> ToListing()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "reliability_unlock", ReliabilityUnlock.ToString("O") },
                { "reliability", Reliability.ToTrend() },
                { "xp", XP.ToTrend() },
                { "karma", Karma.ToTrend() },
                { "bank", Bank.Bank.GetAccount("Checking").ToTrend() },
            };

            return rs;
        }

        public static void RestoreLegacyPersistence(bool OpenBackup)
        {
            string PersistTempPath = Path.Combine(App.AppDataDirectory, "e9f9aa1.dat");
            string PersistPath = Path.Combine(App.DocumentsDirectory, "e9f9aa1.dat");

            FileInfo PersistMain = new FileInfo(PersistPath);
            FileInfo PersistSecond = new FileInfo(PersistTempPath);

            string OpenPath = PersistPath;

            if (OpenBackup)
            {
                OpenPath = PersistSecond.FullName;
            }
            
            try
            {
                if (File.Exists(OpenPath))
                {
                    using (StreamReader r = new StreamReader(new FileStream(OpenPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        var json = r.ReadToEnd();
                        Dictionary<string, dynamic> Persisted = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(Utils.Decrypt(json, "c796ae91-7baf-4e0f-9fa3-3e06652cad58"));
                        try
                        {
                            Karma.LoadLegacy((Dictionary<string, dynamic>)Persisted["Karma"]);
                            XP.LoadLegacy((Dictionary<string, dynamic>)Persisted["XP"]);
                            Reliability.LoadLegacy((Dictionary<string, dynamic>)Persisted["Reliability"]);
                            ReliabilityUnlock = DateTime.Parse((string)Persisted["ReliabilityUnlock"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                        }
                        catch
                        {
                            Console.WriteLine("Failed to fully load Progress Persistence");
                        }

                        GoogleAnalyticscs.TrackEvent("Progress", "Karma", Math.Round(Karma.Balance).ToString(), (int)Math.Round(Karma.Balance, 3));
                        GoogleAnalyticscs.TrackEvent("Progress", "Level", Utils.CalculateLevel(XP.Balance).ToString(), (int)Math.Round(XP.Balance));
                        GoogleAnalyticscs.TrackEvent("Progress", "Reliability", Math.Round(Reliability.Balance).ToString(), (int)Math.Round(Reliability.Balance));

                    }
                }
            }
            catch (Exception ex)
            {
                if (OpenBackup)
                {
                    GoogleAnalyticscs.TrackEvent("Errors", "Database Load", "e9f9aa1.dat - Backup: " + ex.Message);
                    Console.WriteLine("Failed to load Progress Persistence Backup: " + ex.Message + " - " + ex.StackTrace);
                    MessageBox.Show("Failed to load your progress. Please send the Console files located in " + App.AppDataDirectory + " to Parallel 42 via the official support portal on their website. Sorry for the inconvenience.", "Failed to load The Skypark", MessageBoxButton.OK, MessageBoxImage.Stop);
                    Environment.Exit(0);
                }
                else
                {
                    GoogleAnalyticscs.TrackEvent("Errors", "Database Load", "e9f9aa1.dat - Primary: " + ex.Message);
                    Console.WriteLine("Failed to load Progress Persistence: " + ex.Message + " - " + ex.StackTrace);
                    RestoreLegacyPersistence(true);
                    return;
                }
            }
        }


    }
}
