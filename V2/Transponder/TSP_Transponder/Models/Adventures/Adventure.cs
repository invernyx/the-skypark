using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using LiteDB;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Transactor;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.FlightPlans;
using TSP_Transponder.Models.Topography;
using TSP_Transponder.Models.Topography.Utils;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.Messaging;
using TSP_Transponder.Models.Adventures.AdventureInvoices;
using static TSP_Transponder.App;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.Adventures.AdventuresBase;
using static TSP_Transponder.Models.FlightPlans.FlightPlan;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Contractors;

namespace TSP_Transponder.Models.Adventures
{
    public class Adventure
    {
        public AdventureTemplate Template = null;
        public long ID = 0;
        private AState _State = AState.Listed;
        public AState State
        {
            get
            {
                return _State;
            }
            set
            {
                AState PrevState = _State;
                _State = value;
                
                switch (value)
                {
                    case AState.Saved:
                        {
                            if (RequestedAt == null)
                            {
                                Console.WriteLine("Adventure " + Route + " - " + Template.FileName + " (" + ID + ") is Saved");

                                RequestedAt = DateTime.UtcNow;
                                if (!Template.RunningClock)
                                {
                                    if (Template.TimeToComplete > 0)
                                    {
                                        ExpireAt = DateTime.UtcNow.AddHours(Template.TimeToComplete);
                                        PullAt = DateTime.UtcNow.AddHours(Template.ExpireMax);
                                    }
                                    else
                                    {
                                        ExpireAt = DateTime.MaxValue;
                                        PullAt = DateTime.UtcNow.AddHours(Template.ExpireMax);
                                    }
                                }

                                foreach (action_base action in SavedActions)
                                {
                                    action.Enter();
                                }

                                Saved();
                                GenerateFlightPlan();
                                RevalidateAircraft();
                                BroadcastState();
                                CacheAudio();

                                Save();

                                GoogleAnalyticscs.TrackEvent("Tier", UserData.Get("tier"), "Requested", (int)Math.Round(DistanceNM));
                                GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, "Requested", (int)Math.Round(DistanceNM));
                            }
                            break;
                        }
                    case AState.Active:
                        {
                            if (PrevState != AState.Active)
                            {
                                Console.WriteLine("Adventure " + Route + " - " + Template.FileName + " (" + ID + ") has Started");

                                CheckInvoices(Invoice.MOMENT.START);
                                
                                if (StartedAt == null)
                                {
                                    StartedAt = DateTime.UtcNow;
                                }

                                if (RequestedAt == null)
                                {
                                    RequestedAt = StartedAt;
                                }

                                LastResumed = StartedAt;
                                if (!Template.RunningClock)
                                {
                                    if (Template.TimeToComplete > 0)
                                    {
                                        ExpireAt = DateTime.UtcNow.AddHours(Template.TimeToComplete);
                                        PullAt = DateTime.UtcNow.AddHours(Template.ExpireMax);
                                    }
                                    else
                                    {
                                        ExpireAt = DateTime.MaxValue;
                                        PullAt = DateTime.UtcNow.AddHours(Template.ExpireMax);
                                    }
                                }

                                foreach (action_base action in StartedActions)
                                {
                                    action.Enter();
                                }

                                foreach (action_base action in Actions)
                                {
                                    action.Starting();
                                }
                                

                                Started();
                                RevalidateAircraft();
                                GenerateFlightPlan();
                                BroadcastState();
                                BroadcastInteractions();
                                BroadcastPath();
                                BroadcastInvoices();
                                CacheAudio();
                                PlayWelcomeAudio();

                                Save();

                                RichPresence.Update();

                                AdventureTemplateFeaturedService.DismissFeatured(Template.FileName);
                                
                                GoogleAnalyticscs.TrackEvent("Tier", UserData.Get("tier"), "Started", (int)Math.Round(DistanceNM));
                                GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, "Started", (int)Math.Round(DistanceNM));
                            }
                            break;
                        }
                    case AState.Failed:
                        {
                            if (PrevState != AState.Failed)
                            {
                                Console.WriteLine("Adventure " + Route + " - " + Template.FileName + " (" + ID + ") has Failed");

                                foreach (action_base action in StartedActions)
                                {
                                    action.Exit();
                                }

                                foreach (action_base action in FailedActions)
                                {
                                    action.Enter();
                                }

                                CompletedAt = DateTime.UtcNow;

                                if (PrevState == AState.Active)
                                {
                                    CheckInvoices(Invoice.MOMENT.FAIL);
                                    CheckInvoices(Invoice.MOMENT.END);

                                    foreach (action_base action in Actions)
                                    {
                                        action.Ending();
                                    }

                                    if (Save())
                                    {
                                        if (Template.Published && !WavePenalties)
                                        {
                                            ConfirmedReward = new Dictionary<string, float>()
                                            {
                                                { "Reward", 0 },
                                                { "Karma", 0 },
                                                { "XP", 0 },
                                                { "Reliability", 0 },
                                            };

                                            if (Utils.CalculateLevel(Progress.Progress.XP.Balance) >= 3)
                                            {
                                                ConfirmedReward["Reliability"] = Template.RatingGainFail;
                                            }

                                            Progress.Progress.Transact(ConfirmedReward["Karma"], ConfirmedReward["XP"], ConfirmedReward["Reliability"]);
                                        }
                                    }

                                    if (WavePenalties)
                                    {
                                        EndSummary += " (Penalties have been waved.)";
                                    }

                                    if (FailedActions.Find(x => x.GetType() == typeof(audio_speech_play) || x.GetType() == typeof(audio_effect_play)) == null)
                                    {
                                        Audio.AudioFramework.GetEffect("failed", true);
                                    }

                                    BroadcaseEndCard();

                                    GoogleAnalyticscs.TrackEvent("Tier", UserData.Get("tier"), "Failed", (int)Math.Round(DistanceNM));
                                    GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, "Failed", (int)Math.Round(DistanceNM));

                                    RichPresence.Update();
                                    BroadcastInteractions();
                                    BroadcastPath();
                                    BroadcastState();
                                    BroadcastInvoices();
                                }
                                else
                                {
                                    GoogleAnalyticscs.TrackEvent("Tier", UserData.Get("tier"), "Expired", (int)Math.Round(DistanceNM));
                                    GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, "Expired", (int)Math.Round(DistanceNM));
                                    Remove();
                                }
                            }
                            break;
                        }
                    case AState.Succeeded:
                        {
                            if (PrevState != AState.Succeeded)
                            {
                                Console.WriteLine("Adventure " + Route + " - " + Template.FileName + " (" + ID + ") has Succeeded");

                                foreach (action_base action in StartedActions)
                                {
                                    action.Exit();
                                }
                                foreach (action_base action in SuccessActions)
                                {
                                    action.Enter();
                                }

                                if (SuccessActions.Find(x => x.GetType() == typeof(audio_speech_play) || x.GetType() == typeof(audio_effect_play)) == null)
                                {
                                    Audio.AudioFramework.GetEffect("completed", true);
                                }

                                CompletedAt = DateTime.UtcNow;

                                CheckInvoices(Invoice.MOMENT.SUCCEED);
                                CheckInvoices(Invoice.MOMENT.END);

                                foreach (action_base action in Actions)
                                {
                                    action.Ending();
                                }

                                if (Save())
                                {
                                    if (Template.Published)
                                    {
                                        ConfirmedReward = new Dictionary<string, float>()
                                        {
                                            { "Reward", RewardBux },
                                            { "Karma", (float)Math.Round(RewardKarma, 2) },
                                            { "XP", (float)Math.Round(RewardXP, 2) },
                                            { "Reliability", Template.RatingGainSucceed },
                                        };

                                        switch (Template.FileName)
                                        {
                                            case "ReliabilityBoost":
                                                {
                                                    ConfirmedReward["Reliability"] = 60 - Progress.Progress.Reliability.Balance;
                                                    Progress.Progress.UnlockReliability();
                                                    break;
                                                }
                                            case "ReliabilitySee":
                                                {
                                                    Progress.Progress.UnlockReliability();
                                                    break;
                                                }
                                        }

                                        if (RewardBux != 0 && UserData.Get("tier") != "discovery")
                                        {
                                            Bank.Bank.Transact(new Transaction(Utils.GetNumGUID(), RewardBux, GetDescriptiveName(), Bank.Bank.GetAccount("Checking"), Companies.Get(Template.CompanyStr.Count > 0 ? Template.CompanyStr[0] : "unknown").Name), Bank.Bank.GetAccount("Checking"));
                                            Audio.AudioFramework.GetEffect("kaching", true, 1000);
                                        }
                                        Progress.Progress.Transact(ConfirmedReward["Karma"], ConfirmedReward["XP"], ConfirmedReward["Reliability"]);

                                    }
                                }

                                RichPresence.Update();
                                BroadcastInteractions();
                                BroadcastPath();
                                BroadcastState();
                                BroadcaseEndCard();
                                BroadcastInvoices();

                                GoogleAnalyticscs.TrackEvent("Tier", UserData.Get("tier"), "Completed", (int)Math.Round(DistanceNM));
                                GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, "Completed", (int)Math.Round(DistanceNM));
                                   
                            }
                            break;
                        }
                }

                SetInTemplateList(true);
                ScheduleInteractionBroadcast = true;
                SchedulePathBroadcast = true;
            }
        }
        public bool Ready = false;
        public bool _IsMonitored = false;
        public bool IsMonitored {
            get
            {
                return _IsMonitored;
            }
            set
            {
                _IsMonitored = value;
                Console.WriteLine("--- Adventure " + Route + " (" + ID + ") has monitor to " + _IsMonitored);
                ScheduleInteractionBroadcast = true;
                ScheduleLimitsBroadcast = true;
                ScheduleManifestsBroadcast = true;
                if(_IsMonitored)
                {
                    foreach(var Sit in Situations)
                    {
                        Sit.InRange = false;
                    }
                }
                else
                {
                    foreach (action_base action in Actions)
                    {
                        action.Pausing();
                    }
                }
                BroadcastState();
                Save();
            }
        }
        public short ImageIndex = -1;
        public short DescriptionIndex = -1;
        public bool AircraftCompatible = false;
        public string EndSummary = "";
        public string DescriptionString = "";
        public string DescriptionLongString = "";
        public string SocketID = "";
        public float DistanceNM = 0;
        public float RewardXP = 0;
        public int RewardBux = 0;
        public float RewardKarma = 0;
        public bool WavePenalties = false;
        public Dictionary<string, float> ConfirmedReward = null;
        public List<float> RecommendedAircraft = null;
        public float TotalValue = 0;
        public bool RouteValidated = false;
        public string Route = "";
        public string RouteCode = "";
        public string RouteString = "";
        public List<int> TopographyData = new List<int>();
        public int TopographyRange = 0;
        public int TopographyVariance = -1;
        public DateTime CreatedAt = new DateTime();
        public DateTime? RequestedAt = null;
        public DateTime? StartedAt = null;
        public DateTime? LastResumed = null;
        public DateTime? CompletedAt = null;
        public DateTime ExpireAt = new DateTime();
        public DateTime? PullAt = null;
        public DateTime LastSave = new DateTime();
        public bool Cleanedup = false;
        public int SituationAt
        {
            get
            {
                if (!Template.StrictOrder)
                {
                    return Situations.Count - 1;
                }
                else
                {
                    return Situations.FindIndex(x => !x.Done);
                }
            }
        }

        public ChatThread Memos = new ChatThread();

        public List<Invoice> InvoiceQuote = null;
        public List<Situation> Situations = new List<Situation>();

        public List<KeyValuePair<int, action_base>> ActionsIndex = new List<KeyValuePair<int, action_base>>();
        public List<action_base> ActionsWatch = new List<action_base>();

        public List<action_base> Actions = new List<action_base>();
        public List<action_base> SavedActions = new List<action_base>();
        public List<action_base> StartedActions = new List<action_base>();
        public List<action_base> SuccessActions = new List<action_base>();
        public List<action_base> FailedActions = new List<action_base>();

        public Dictionary<string, dynamic> RestoredActionsPersistence = new Dictionary<string, dynamic>();

        public GeoLoc LastLocationGeo = null;
        public Airport LastLocationApt = null;
        public bool ScheduleInvoiceBroadcast = false;
        public bool ScheduleStateBroadcast = false;
        public bool SchedulePathBroadcast = false;
        public bool ScheduleInteractionBroadcast = false;
        public bool ScheduleLimitsBroadcast = false;
        public bool ScheduleManifestsBroadcast = false;
        public bool ScheduleMemosBroadcast = false;
        public List<FlightPlan> LegsFlightPlans = null;
        public List<FlightPlan> CustomPlans = new List<FlightPlan>();

        public Adventure(long ID, AdventureTemplate Template, Dictionary<string, dynamic> Restore, bool DoSave)
        {
            #region Restore
            if (Restore == null)
            {
                Restore = new Dictionary<string, dynamic>();
            }

            if(Restore.Keys.Count > 0)
            {
                if (Restore.ContainsKey("Actions"))
                {
                    RestoredActionsPersistence = Restore["Actions"];
                }
            }
            #endregion

            #region Create Adventure base
            this.ID = ID;
            this.Template = Template;
            #endregion
            
            #region Root Actions
            foreach (action_base Action in Template.SavedActions)
            {
                SavedActions.Add(CreateAction(this, Template, null, Action.UID, null));
            }

            foreach (action_base Action in Template.StartedActions)
            {
                StartedActions.Add(CreateAction(this, Template, null, Action.UID, null));
            }

            foreach (action_base Action in Template.SuccessActions)
            {
                SuccessActions.Add(CreateAction(this, Template, null, Action.UID, null));
            }

            foreach (action_base Action in Template.FailedActions)
            {
                FailedActions.Add(CreateAction(this, Template, null, Action.UID, null));
            }
            #endregion

            #region Process all Situations
            //float CustomScoreTemp = 0;
            int i = 0;
            foreach (Situation Sit in Template.Situations)
            {
                Situation NewSituation = new Situation(i, Template, this, Sit);

                if (Restore.ContainsKey("Situations"))
                {
                    NewSituation.ImportState((Dictionary<string, dynamic>)Restore["Situations"][i]);
                }

                if (NewSituation.ICAO != string.Empty)
                {
                    try
                    {
                        if(NewSituation.Airport == null)
                        {
                            NewSituation.Airport = SimLibrary.SimList[0].AirportsLib.GetByICAO(NewSituation.ICAO, NewSituation.Location);
                            if(NewSituation.Airport != null)
                            {
                                NewSituation.TriggerRange = NewSituation.Airport.Radius;
                                NewSituation.Location = NewSituation.Airport.Location;
                            }
                        }
                    }
                    catch
                    {
                    }
                    
                    //if(SitObj.Airport.IsCustom > CustomLevel.Bulk)
                    //{
                    //    CustomScoreTemp += (float)((float)SitObj.Airport.IsCustom + SitObj.Airport.Relevancy);
                    //}
                    //else
                    //{
                    //    CustomScoreTemp += (float)SitObj.Airport.IsCustom;
                    //}
                }
                else
                {
                    //CustomScoreTemp += 4;
                }

                foreach (action_base Action in Sit.Actions.OrderBy(x => x.Order))
                {
                    action_base ActionObj = CreateAction(this, Template, NewSituation, Action.UID, null);
                    if (ActionObj != null)
                    {
                        NewSituation.Actions.Add(ActionObj);
                    }
                }

                Situations.Add(NewSituation);
                i++;
            }


            #endregion
            
            #region State
            if (Restore.ContainsKey("State"))
            {
                _State = (AState)((int)Restore["State"]);
            }
            #endregion

            #region Calculate CustomScore
            //CustomScore = CustomScoreTemp / Situations.Count;
            #endregion

            #region Calculate Distance
            float _Distance = 0;
            Situation PrevSit = null;
            foreach (Situation Sit in Situations)
            {
                if (PrevSit != null)
                {
                    PrevSit.DistToNext = (float)Utils.MapCalcDist(PrevSit.Location, Sit.Location, Utils.DistanceUnit.NauticalMiles, true);
                    _Distance += PrevSit.DistToNext;
                }
                PrevSit = Sit;
            }
            DistanceNM = _Distance;
            #endregion

            #region General Restore
            if (Restore.ContainsKey("RouteCode"))
            {
                RouteCode = Restore["RouteCode"];
            }
            
            if (Restore.ContainsKey("EndSummary"))
            {
                EndSummary = Restore["EndSummary"];
            }

            if (Restore.ContainsKey("LastLocationGeo"))
            {
                LastLocationGeo = Restore["LastLocationGeo"] != null ? new GeoLoc(Convert.ToDouble(Restore["LastLocationGeo"][0]), Convert.ToDouble(Restore["LastLocationGeo"][1])) : null;
            }

            if (Restore.ContainsKey("Cleanedup"))
            {
                Cleanedup = Convert.ToBoolean(Restore["Cleanedup"]);
            }

            if (Restore.ContainsKey("StartedAt"))
            {
                StartedAt = Restore["StartedAt"] != null ? DateTime.Parse((string)Restore["StartedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;
            }

            if (Restore.ContainsKey("LastResumed"))
            {
                LastResumed = Restore["LastResumed"] != null ? DateTime.Parse((string)Restore["LastResumed"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;
            }

            if (Restore.ContainsKey("CompletedAt"))
            {
                CompletedAt = Restore["CompletedAt"] != null ? DateTime.Parse((string)Restore["CompletedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;
            }

            if (Restore.ContainsKey("RequestedAt"))
            {
                RequestedAt = Restore["RequestedAt"] != null ? DateTime.Parse((string)Restore["RequestedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;
            }
            
            if (Restore.ContainsKey("ImageIndex"))
            {
                ImageIndex = (short)Restore["ImageIndex"];
            }
            else
            {
                ImageIndex = (short)Utils.GetRandom(Template.ImageURL.Count);
            }

            if (Restore.ContainsKey("DescriptionIndex"))
            {
                DescriptionIndex = (short)Restore["DescriptionIndex"];
            }
            else if(Template.Description != null)
            {
                DescriptionIndex = (short)Utils.GetRandom(Template.Description.Count);
            }
            #endregion

            #region Memos
            if (Restore.ContainsKey("Memos"))
            {
                Memos = new ChatThread((Dictionary<string, dynamic>)Restore["Memos"]);
            }
            #endregion

            #region Recommended aircraft
            if (Restore.ContainsKey("RecommendedAircraft"))
            {
                RecommendedAircraft = new List<float>();
                foreach (var val in ((IEnumerable)Restore["RecommendedAircraft"]))
                {
                    RecommendedAircraft.Add(Convert.ToSingle(val));
                }
            }
            #endregion

            #region Calculate Expire/Create
            if (Template.TimeToComplete == -1 && !Template.RunningClock)
            {
                SetExpire();
            }
            else if (Restore.ContainsKey("PullAt") && Restore.ContainsKey("ExpireAt") && Restore.ContainsKey("CreatedAt"))
            {
                CreatedAt = DateTime.Parse((string)Restore["CreatedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                ExpireAt = DateTime.Parse((string)Restore["ExpireAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                PullAt = DateTime.Parse((string)Restore["PullAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                if (!IsStillValid())
                {
                    lock (Template.ActiveAdventures)
                    {
                        if (Utils.GetRandom(1) == 1 || Template.ActiveAdventures.Count < 2)
                        {
                            Renew();
                        }
                    }
                }
            }
            else
            {
                SetExpire();
            }
            #endregion

            #region Calculate Value, XP and Moral and stuff
            if (Restore.ContainsKey("RewardXP"))
            {
                RewardXP = Convert.ToSingle(Restore["RewardXP"]);
            }
            else
            {
                RewardXP = GenerateXP();
            }

            if (Restore.ContainsKey("RewardBux"))
            {
                RewardBux = Convert.ToInt32(Restore["RewardBux"]);
            }
            else
            {
                GenerateReward();
            }

            if (Restore.ContainsKey("RewardKarma"))
            {
                RewardKarma = Convert.ToInt32(Restore["RewardKarma"]);
            }
            else
            {
                float MoralGain = 0;
                int MoralGainIteration = 0;
                foreach (action_base Acn in Actions)
                {
                    if (Acn.KarmaGain > 0)
                    {
                        MoralGainIteration++;
                    }
                    MoralGain += Acn.KarmaGain;
                }
                if (MoralGainIteration > 0)
                {
                    MoralGain = MoralGain / MoralGainIteration;
                }
                RewardKarma = Template.KarmaGainBase + MoralGain;
            }
            #endregion
            
            #region Topography
            if (Restore.ContainsKey("Topo"))
            {
                if(Restore["Topo"] != null) {
                    string[] TopoEntries = ((string)Restore["Topo"]).Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string Entry in TopoEntries)
                    {
                        TopographyData.Add(Convert.ToInt32(Entry));
                    }
                    Ready = true;
                }
            }
            #endregion

            #region Restore Params
            foreach (action_base Action in Actions)
            {
                Action.ImportState(Action.Parameters);
                Action.Parameters = null;
            }
            #endregion

            if (State == AState.Active)
            {
                RevalidateAircraft();
                foreach (action_base action in Actions)
                {
                    action.Resuming();
                }
                GenerateFlightPlan();
            }
            else if (State == AState.Saved)
            {
                GenerateFlightPlan();
            }

            RestoredActionsPersistence.Clear();
            GenerateStringRoute();
            GenerateDescription();
            SetInTemplateList(false);

            if (DoSave)
            {
                switch (State)
                {
                    case AState.Listed:
                        {
                            Save();
                            break;
                        }
                    default:
                        {
                            if (RestoreCompleted && Ready)
                            {
                                lock (LiteDbService.DB)
                                {
                                    var DBCollection = LiteDbService.DB.Database.GetCollection("adventures_active");
                                    var bdoc = DBCollection.FindById(ID);
                                    if (bdoc == null)
                                    {
                                        ExportContract();
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }

        public void Process()
        {
            if(!Ready)
            {
                Console.WriteLine(Route + " not ready.");
                return;
            }

            switch (State)
            {
                case AState.Active:
                    {
                        CheckMonitorState(false);

                        if(!IsStillValid())
                        {
                            Fail("You've let this " + Template.TypeLabel + " expire!");
                        }

                        if(!TemporalLast.IS_SLEW_ACTIVE && IsLoaded)
                        {
                            foreach (Situation Sit in Situations)
                            {
                                Sit.Process();
                            }

                            if (IsMonitored)
                            {
                                int i = 0;
                                int acc = ActionsWatch.Count;
                                while (i < ActionsWatch.Count)
                                {
                                    var action = ActionsWatch[i];
                                    action.Process();
                                    i += acc - ActionsWatch.Count + 1;
                                    acc = ActionsWatch.Count;
                                }

                                foreach (action_base action in StartedActions)
                                {
                                    action.Process();
                                }

                                //if (Template.NoMilestones)
                                //{
                                bool HasCompleted = true;
                                foreach (KeyValuePair<int, action_base> action in ActionsIndex)
                                {
                                    if (action.Value.Completed != null)
                                    {
                                        if (!(bool)action.Value.Completed)
                                        {
                                            HasCompleted = false;
                                        }
                                    }
                                }

                                if (HasCompleted)
                                {
                                    Succeed(EndSummary);
                                }
                                //}
                                //else
                                //{
                                //    bool HasCompleted = true;
                                //    foreach (KeyValuePair<int, action_base> action in ActionsIndex)
                                //    {
                                //        if (action.Value.GetType() == typeof(adventure_milestone))
                                //        {
                                //            adventure_milestone milestone = (adventure_milestone)action.Value;
                                //            if (!(bool)milestone.Completed)
                                //            {
                                //                HasCompleted = false;
                                //            }
                                //        }
                                //    }
                                //
                                //    if (HasCompleted)
                                //    {
                                //        Succeed(EndSummary);
                                //    }
                                //}

                                if (LastSave.AddMinutes(5) < DateTime.UtcNow)
                                {
                                    LastSave = DateTime.UtcNow;
                                    Save();
                                }
                            }
                        }

                        break;
                    }
                case AState.Saved:
                    {
                        if (!IsStillValid())
                        {
                            Fail("You've let this " + Template.TypeLabel + " expire!");
                            break;
                        }

                        if (!TemporalLast.IS_SLEW_ACTIVE && IsLoaded)
                        {
                            foreach (Situation Sit in Situations)
                            {
                                Sit.Process();
                            }

                            RevalidateAircraft();
                        }

                        break;
                    }
                case AState.Listed:
                    {
                        break;
                    }
                default:
                    {
                        if (!Cleanedup && CompletedAt != null)
                        {
                            if (((DateTime)CompletedAt).AddMinutes(2) < DateTime.UtcNow)
                            {
                                Cleanedup = true;
                                foreach (action_base action in Actions)
                                {
                                    action.Clear();
                                }
                            }
                        }
                        break;
                    }
            }
        }

        public void Saved()
        {

        }

        public void Started()
        {

        }

        public void Fail(string reason)
        {
            Console.WriteLine(Route + " exp is " + ExpireAt.ToLongDateString());
            if (reason == string.Empty)
            {
                reason = "That's unfortunate...";
            }
            EndSummary = reason;
            State = AState.Failed;
        }

        private void Succeed(string reason)
        {
            if (reason == string.Empty)
            {
                if(Template.EndSummary == string.Empty)
                {
                    reason = "Great job!";
                }
                else
                {
                    reason = Template.EndSummary;
                }
            }
            EndSummary = reason;
            State = AState.Succeeded;
        }

        public bool IsStillValid()
        {
            if (Template.TimeToComplete == -1 && !Template.RunningClock)
            {
                return true;
            }

            switch (State)
            {
                case AState.Listed:
                    {
                        if (Template.RunningClock)
                        {
                            TimeSpan PullDiff = (DateTime)PullAt - DateTime.UtcNow;
                            if (PullDiff.TotalSeconds < 0)
                            {
                                return false;
                            }

                            if (PullDiff.TotalHours < 3)
                            {
                                GenerateReward();
                                break;
                            }
                        }
                        else
                        {
                            TimeSpan ExpireDiff = (DateTime)PullAt - DateTime.UtcNow;
                            if (ExpireDiff.TotalSeconds < 0)
                            {
                                return false;
                            }
                        }
                        
                        break;
                    }
                case AState.Active:
                case AState.Saved:
                    {
                        if (Template.CompanyStr.Contains("coyote"))
                        {
                            if (ExpireAt < DateTime.UtcNow)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            TimeSpan ExpiredBy = DateTime.UtcNow - ExpireAt;

                            if (ExpiredBy.TotalMinutes > 0)
                            {
                                if (ExpiredBy.TotalMinutes <= 5)
                                {
                                    return true;
                                }
                                else
                                {
                                    if (ExpireAt.AddMinutes(5 + (DistanceNM * 0.01f)) < DateTime.UtcNow)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        break;
                    }
            }

            return true;
        }
        
        public void ValidateRoute()
        {
            if (State != AState.Listed || MW.IsShuttingDown)
            {
                return;
            }

            try
            {
                if (Template.Routes.Find(x => x.RouteCode == RouteCode) == null)
                {
                    ILiteCollection<BsonDocument> DBCollection = null;
                    lock (LiteDbService.DBCache)
                    {
                        DBCollection = LiteDbService.DBCache.Database.GetCollection("adventures_listed");
                    }

                    lock (LiteDbService.DBCache)
                    {
                        DBCollection.Delete(ID);
                    }

                    Remove();
                }

                RouteValidated = true;
            }
            catch
            {

            }
        }

        public void Remove()
        {
            if(State != AState.Listed && !Cleanedup)
            {
                Console.WriteLine("Clearing Adventure " + ID);
                foreach (action_base action in Actions)
                {
                    action.Clear();
                }
                Cleanedup = true;
                Console.WriteLine("Done Clearing Adventure " + ID);
            }

            lock (AllContracts)
            {
                AllContracts.Remove(this);
            }

            lock (Template.ActiveAdventures)
            {
                Template.ActiveAdventures.Remove(this);
            }

            lock (Template.InactiveAdventures)
            {
                Template.InactiveAdventures.Remove(this);
            }

            lock (LiteDbService.DB)
            {
                LiteDbService.DB.Database.GetCollection("adventures_active").Delete(ID);
            }

            lock(LiteDbService.DBCache)
            {
                LiteDbService.DBCache.Database.GetCollection("adventures_listed").Delete(ID);
            }

            if (State != AState.Listed)
            {
                APIBase.ClientCollection.SendMessage("adventure:delete", JSSerializer.Serialize(new Dictionary<string, dynamic>()
                {
                    { "ID", ID }
                }));
            }


        }

        public void Renew()
        {
            if(State == AState.Listed)
            {
                CreatedAt = DateTime.UtcNow;
                SetExpire();
            }
        }

        public void SetExpire()
        {
            int[] RecommendedAircraftSpeed = new int[]
            {
                120,
                160,
                190,
                500,
                500,
                500
            };

            int SlowestRecommended = RecommendedAircraft.FindIndex(x => x > 10);
            int RecommendedSpeed = SlowestRecommended > -1 ? RecommendedAircraftSpeed[SlowestRecommended] : 100;

            if (Template.RunningClock)
            {
                float MaxHour = Template.ExpireMax;
                float MinHour = (DistanceNM / RecommendedSpeed);

                if (MinHour < Template.ExpireMin)
                {
                    MinHour = Template.ExpireMin;
                }

                if (MinHour > MaxHour)
                {
                    MaxHour = MinHour + 1;
                }

                //if(!IsDev)
                //{
                DateTime ThisHour = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc).AddHours(Utils.GetRandom(1, 4) * 0.25f);
                float RandomHour = Utils.GetRandom((int)Math.Round(MinHour), (int)Math.Round(MaxHour));
                DateTime Exp = ThisHour.AddHours(RandomHour);

                ExpireAt = Exp;
                PullAt = Exp.AddHours(-MinHour);
                //}
                //else
                //{
                //    DateTime Exp = DateTime.UtcNow.AddHours(MinHour + 0.25);
                //
                //    ExpireAt = Exp;
                //    PullAt = Exp.AddHours(-MinHour);
                //}
            }
            else
            {
                if (Template.TimeToComplete > 0)
                {
                    ExpireAt = DateTime.UtcNow.AddHours(Template.TimeToComplete);
                    PullAt = DateTime.UtcNow.AddHours(Template.ExpireMax);
                }
                else
                {
                    ExpireAt = DateTime.MaxValue;
                    PullAt = DateTime.MaxValue;
                }
            }

            CreatedAt = DateTime.UtcNow;
        }

        public void SetInTemplateList(bool CheckRemove)
        {
            switch(_State)
            {
                case AState.Active:
                case AState.Saved:
                case AState.Listed:
                    {
                        if(CheckRemove)
                        {
                            lock (Template.InactiveAdventures)
                            {
                                Template.InactiveAdventures.Remove(this);
                            }
                        }

                        lock (Template.ActiveAdventures)
                        {
                            if(!Template.ActiveAdventures.Contains(this))
                            {
                                Template.ActiveAdventures.Add(this);
                            }
                        }

                        switch (_State)
                        {
                            case AState.Active:
                            case AState.Saved:
                                {
                                    lock (Template.LiveAdventures)
                                    {
                                        if (!Template.LiveAdventures.Contains(this))
                                        {
                                            Template.LiveAdventures.Add(this);
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    if (CheckRemove)
                                    {
                                        lock (Template.LiveAdventures)
                                        {
                                            Template.LiveAdventures.Remove(this);
                                        }
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        if (CheckRemove)
                        {
                            lock (Template.ActiveAdventures)
                            {
                                Template.ActiveAdventures.Remove(this);
                            }

                            lock (Template.LiveAdventures)
                            {
                                Template.LiveAdventures.Remove(this);
                            }
                        }
                        lock (Template.InactiveAdventures)
                        {
                            if (!Template.InactiveAdventures.Contains(this))
                            {
                                Template.InactiveAdventures.Add(this);
                            }
                        }
                        break;
                    }
            }
        }

        public bool Save()
        {
            if(MW.IsShuttingDown)
            {
                return false;
            }

            return ExportContract();
        }
        
        public void SetMonitorState(bool State)
        {
            IsMonitored = State;
            GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, State ? "Paused" : "Unpaused");
            ScheduleManifestsBroadcast = true;
        }
        
        public void CheckMonitorState(bool CanEnable)
        {
            var test = TemporalLiveLast;
            var test1 = TemporalNewBuffer;
            if (LastLocationGeo != null && TemporalLiveLast != null && TemporalNewBuffer != null && State == AState.Active)
            {
                GeoLoc PreviousLocation = new GeoLoc(TemporalNewBuffer.PLANE_LOCATION.Lon, TemporalNewBuffer.PLANE_LOCATION.Lat);

                double DistanceToLastLocation = Utils.MapCalcDist(PreviousLocation, LastLocationGeo, Utils.DistanceUnit.NauticalMiles, true);
                bool IsInRange = DistanceToLastLocation < 10;

                if(IsMonitored)
                {
                    if ((!IsInRange && !IsDev) || !AircraftCompatible)
                    {
                        SetMonitorState(false);
                    }
                }
                else if(CanEnable)
                {
                    if (IsInRange && AircraftCompatible)
                    {
                        SetMonitorState(true);
                    }
                }
                
                if (IsInRange && IsMonitored || IsDev)
                {
                    LastLocationGeo = PreviousLocation;
                    
                }
            }
        }
        
        public void RevalidateAircraft()
        {
            bool PreviousAircraftCompatible = AircraftCompatible;
            if (Template.AircraftRestriction.Count > 0)
            {
                AircraftCompatible = false;
                if(Connectors.SimConnection.Aircraft != null)
                {
                    foreach (string match in Template.AircraftRestriction)
                    {
                        if (Connectors.SimConnection.Aircraft.Name.ToLower().Contains(match.ToLower()))
                        {
                            AircraftCompatible = true;
                            break;
                        }
                    }
                } 
            }
            else
            {
                AircraftCompatible = true;
            }
            
            switch (State)
            {
                case AState.Active:
                    {
                        CheckMonitorState(false);
                        foreach (action_base Act in Actions)
                        {
                            Act.ChangedAircraft();
                        }
                        break;
                    }
            }

            if (PreviousAircraftCompatible != AircraftCompatible)
            {
                ScheduleStateBroadcast = true;
            }
        }

        public void PlayWelcomeAudio()
        {
            if (Utils.GetRandom(5) == 0 || IsDev)
            {
                if (Template.CompanyStr.Contains("clearsky"))
                {
                    if (StartedActions.Find(x => x.GetType() == typeof(audio_speech_play) || x.GetType() == typeof(audio_effect_play)) == null)
                    {
                        Audio.AudioFramework.GetSpeech("characters", "brigit/good_day", true, false, false, null, (member, route, msg) =>
                        {
                            Memos.EnsureMember("brigit", "Brigit");
                            Memos.Add(new ChatThread.Message()
                            {
                                Type = ChatThread.Message.MessageType.Audio,
                                Param = "characters:" + route,
                                Member = member,
                                Content = msg,
                            });
                            Save();
                            ScheduleMemosBroadcast = true;
                        });
                    }
                }
                else if (Template.CompanyStr.Contains("coyote"))
                {
                    if (StartedActions.Find(x => x.GetType() == typeof(audio_speech_play) || x.GetType() == typeof(audio_effect_play)) == null)
                    {
                        Audio.AudioFramework.GetSpeech("characters", "pablo/deliver_some_not_so_good", true, false, false, null, (member, route, msg) =>
                        {
                            Memos.EnsureMember("pablo", "Pablo");
                            Memos.Add(new ChatThread.Message()
                            {
                                Type = ChatThread.Message.MessageType.Audio,
                                Param = "characters:" + route,
                                Member = member,
                                Content = msg,
                            });
                            Save();
                            ScheduleMemosBroadcast = true;
                        });
                    }
                }

            }
        }

        public string GetDescriptiveName()
        {
            string N = "";
            if (Template.Name == string.Empty)
            {
                string path = "";
                foreach (Situation sit in Situations)
                {
                    if (sit.Visible)
                    {
                        if (sit.ICAO != string.Empty)
                        {
                            path += sit.ICAO + " > ";
                        }
                        else
                        {
                            path += sit.Location.Lon + ", " + sit.Location.Lat + " > ";
                        }
                    }
                }

                N = path.TrimEnd(" > ".ToCharArray());
            }
            else
            {
                N = Template.Name;
            }

            return N;
        }

        public List<FlightPlan> GetRelatedFlightPlans()
        {
            List<FlightPlan> Plans = new List<FlightPlan>();
            if (LegsFlightPlans != null)
            {
                List<FlightPlan> ExistingPlans = new List<FlightPlan>();

                if (Situations.Count > 1)
                {
                    Situation FirstSit = Situations[0];
                    Situation LastSit = Situations[Situations.Count - 1];
                    ExistingPlans = FlightPlans.Plans.PlansList.FindAll(x =>
                    {
                        bool isValid = false;

                        if (FirstSit.Airport != null)
                        {
                            if(x.Waypoints[0].Code == FirstSit.Airport.ICAO)
                            {
                                isValid = true;
                            }
                        }

                        if (isValid && LastSit.Airport != null)
                        {
                            if (x.Waypoints.Find(x1 => x1.Code == LastSit.Airport.ICAO) != null)
                            {
                                isValid = true;
                            };
                        }

                        return isValid;
                    });

                    List<KeyValuePair<float, FlightPlan>> Distances = new List<KeyValuePair<float, FlightPlan>>();
                    foreach(FlightPlan Plan in ExistingPlans)
                    {
                        FlightPlan.Waypoint LastWP = Plan.Waypoints[Plan.Waypoints.Count - 1];
                        float Dist = (float)Utils.MapCalcDist(LastWP.Location, LastSit.Location, Utils.DistanceUnit.NauticalMiles, true);
                        if(Dist < 100)
                        {
                            Distances.Add(new KeyValuePair<float, FlightPlan>(Dist, Plan));
                        }
                    }
                    Distances = Distances.OrderBy(x => x.Key).ToList();
                    ExistingPlans.Clear();
                    foreach (var Plan in Distances)
                    {
                        ExistingPlans.Add(Plan.Value);
                    }

                    while (ExistingPlans.Count > 10)
                    {
                        ExistingPlans.RemoveAt(0);
                    }
                }

                Plans.AddRange(ExistingPlans);
            }

            return Plans;
        }

        public List<string> GetMediaLinks()
        {
            List<string> AdvCountries = new List<string>();
            foreach (var Sit in Situations)
            {
                if (Sit.Airport != null)
                {
                    AdvCountries.Add(Sit.Airport.Country);
                }
            }

            string Countries = string.Join("|", AdvCountries);

            List<string> MediaLinkClean = new List<string>();
            List<string> MediaLinkCountries = new List<string>();
            foreach (string Link in Template.MediaLink)
            {
                string MediaLinkSpl = Regex.Replace(Link, @"\[(.*)\]", (Match match) =>
                {
                    string v = match.ToString();
                    string[] s = v.Trim("[]".ToCharArray()).Split(",.|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    MediaLinkCountries = s.ToList();

                    string ns = Link.Remove(match.Index, match.Length);
                    return "";
                });

                if (MediaLinkCountries.Count > 0)
                {
                    foreach (var coutnry in MediaLinkCountries)
                    {
                        if (AdvCountries.Contains(coutnry) && !MediaLinkClean.Contains(MediaLinkSpl))
                        {
                            MediaLinkClean.Add(MediaLinkSpl);
                        }
                    }
                }
                else
                {
                    MediaLinkClean.Add(MediaLinkSpl);
                }

            }

            return MediaLinkClean;
        }
        

        public bool PayQuotedInvoice()
        {
            if (InvoiceQuote != null)
            {
                foreach (Invoice invoice in InvoiceQuote)
                {
                    invoice.Pay();

                    try
                    {
                        switch (invoice.Title)
                        {
                            case "%userrelocation%":
                                {
                                    GoogleAnalyticscs.TrackEvent("Costs", "Relocation Costs", Template.FileName, Convert.ToInt32(Math.Round(invoice.Balance)));
                                    GoogleAnalyticscs.TrackEvent("Costs", "Relocation Distance", Template.FileName, Convert.ToInt32(Math.Round(invoice.Fees.Find(x => x.Code == "relocation").Params["DistanceKM"])));
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to report Reloc analytics: " + ex.Message);
                    }

                }
                InvoiceQuote = null;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private void CheckInvoices(Invoice.MOMENT moment)
        {
            if (UserData.Get("tier") != "discovery")
            {
                switch(moment)
                {
                    case Invoice.MOMENT.START:
                        {
                            if(InvoiceQuote == null)
                            {
                                StagedInvoices.CreateInitialInvoice(this);
                            }
                            PayQuotedInvoice();
                            break;
                        }
                    case Invoice.MOMENT.SUCCEED:
                    case Invoice.MOMENT.FAIL:
                    case Invoice.MOMENT.END:
                        {
                            List<Invoice> invoiceCollection = Invoices.GetAllFromID(ID);

                            foreach(var invoice in invoiceCollection)
                            {
                                invoice.CheckRefund(moment);
                            }

                            break;
                        }
                }
            }
        }

        
        public void CacheAudio()
        {
            foreach(var act in Actions)
            {
                Type at = act.GetType();
                if (at == typeof(audio_effect_play))
                {
                    ((audio_effect_play)act).Cache();
                }
                else if (at == typeof(audio_speech_play))
                {
                    ((audio_speech_play)act).Cache();
                }
            }
        }

        public void GenerateReward()
        {
            float BonusMultiplier = 1;
            if (Template.RunningClock & Template.Company.Count > 0)
            {
                Company Co = Template.Company[0];
                if (Co.Bonification != null)
                {
                    TimeSpan PullDiff = (DateTime)PullAt - DateTime.UtcNow;
                    foreach (var bonus in Co.Bonification)
                    {
                        if (PullDiff.TotalHours < bonus.Key)
                        {
                            BonusMultiplier = bonus.Value;
                            break;
                        }
                    }
                }
            }

            float RewardPerNM = DistanceNM * Template.RewardPerNM;
            float RewardBase = Template.RewardBase;
            float RewardForValue = TotalValue * 0.01f;
            RewardBux = (int)Math.Round((RewardBase + RewardPerNM + RewardForValue) * BonusMultiplier);
            
        }

        public float GenerateXP()
        {
            if (Template.XPBase >= 0)
            {
                float DistanceBonus = ((float)Math.Pow(DistanceNM, 0.2) * 20) + Template.XPBase;

                #region Airport Visit Penalty
                float AirportVisitPenalty = 0;
                int Apts = 0;
                if (!IsDev)
                {
                    foreach (Situation Sit in Situations)
                    {
                        if (Sit.Airport != null)
                        {
                            int Count = EventBus.EventManager.CountAirportVisit(Sit.Airport.ICAO);
                            AirportVisitPenalty += (float)Count / 3;
                            Apts++;
                        }
                    }
                    if (AirportVisitPenalty > 0)
                    {
                        AirportVisitPenalty /= Apts;
                    }
                }
                #endregion

                #region Runway Surface Bonus
                float RunwaySurfaceBonus = 0;
                List<Airport> Airports = Situations.Select(x => x.Airport).ToList();
                foreach (Airport Apt in Airports)
                {
                    if (Apt != null)
                    {
                        List<Surface> Rwys = Apt.Runways.Select(x => x.Surface).ToList();
                        RunwaySurfaceBonus += Rwys.Contains(Surface.Grass) || Rwys.Contains(Surface.Gravel) || Rwys.Contains(Surface.Water) || Rwys.Contains(Surface.Snow) ? 8 : 0;
                    }
                }
                #endregion

                #region Legs Count Bonus
                float LegsCountBonus = Situations.Count > 2 ? (Situations.Count * 8) : 0;
                #endregion

                #region Relief Bonus
                float ReliefBonus = 0;
                int SitIndex = 0;
                foreach (Airport Apt in Airports)
                {
                    if (Apt != null)
                    {
                        ReliefBonus += (float)(SitIndex == 0 ? Apt.Relief * 0.5f : Apt.Relief) * 0.3f;
                    }
                    SitIndex++;
                }
                #endregion

                float XP = 0;
                XP += DistanceBonus;
                XP += ReliefBonus / (Airports.Count * 0.2f);
                XP += LegsCountBonus;
                XP += RunwaySurfaceBonus;
                XP /= (1 + AirportVisitPenalty);
                return XP;
            }
            else
            {
                return 0;
            }
        }

        public void GenerateFlightPlan()
        {
            if(LegsFlightPlans == null)
            {
                LegsFlightPlans = new List<FlightPlan>();
                FlightPlan FullFlightPlan = new FlightPlan(Utils.GetNumGUID());
                FullFlightPlan.Hash = Utils.GetNumGUID().ToString();

                if (Situations.Count > 0)
                {
                    foreach (Situation Sit in Situations)
                    {
                        Waypoint Waypoint = new Waypoint();

                        if (Sit.Airport != null)
                        {
                            Waypoint.Apt = Sit.Airport;
                            Waypoint.Type = "airport";
                            Waypoint.Code = Sit.Airport.ICAO;
                            FullFlightPlan.Airports.Add(Sit.Airport);
                        }
                        else
                        {
                            Waypoint.Type = "user";
                        }

                        Waypoint.Location = Sit.Location;
                        FullFlightPlan.Waypoints.Add(Waypoint);
                    }

                    FullFlightPlan.CalculateDistances();
                    FullFlightPlan.Export(Path.Combine(AppDataDirectory, "Plans", ID + ".pln"), ExportType.PLN);
                    LegsFlightPlans.Add(FullFlightPlan);
                }


                if (Situations.Count > 2)
                {
                    foreach (var Sit in Situations)
                    {
                        int Index = Situations.IndexOf(Sit);
                        if (Index > 0)
                        {
                            FlightPlan LegsFlightPlan = new FlightPlan(Utils.GetNumGUID());
                            LegsFlightPlan.Hash = Utils.GetNumGUID().ToString();

                            if (Situations.Count > 0)
                            {
                                foreach (Situation Sit1 in new Situation[]
                                {
                                Situations[Index - 1],
                                Sit
                                })
                                {
                                    Waypoint Waypoint = new Waypoint();

                                    if (Sit1.Airport != null)
                                    {
                                        Waypoint.Apt = Sit1.Airport;
                                        Waypoint.Type = "airport";
                                        Waypoint.Code = Sit1.Airport.ICAO;
                                        LegsFlightPlan.Airports.Add(Sit1.Airport);
                                    }
                                    else
                                    {
                                        Waypoint.Type = "user";
                                    }

                                    Waypoint.Location = Sit1.Location;
                                    LegsFlightPlan.Waypoints.Add(Waypoint);
                                }

                                LegsFlightPlan.CalculateDistances();
                                LegsFlightPlan.Export(Path.Combine(AppDataDirectory, "Plans", ID + ".pln"), ExportType.PLN);
                            }
                            LegsFlightPlans.Add(LegsFlightPlan);
                        }
                    }
                }
            }
        }

        public void GenerateDescription()
        {
            Action<int> Process = (mode) =>
            {
                string Contruct = "";
                switch (mode)
                {
                    case 0:
                        {
                            if (Template.Description != null)
                            {
                                if (Template.Description.Count > 0)
                                {
                                    Contruct = Template.Description[DescriptionIndex];
                                }
                            }
                            else
                            {
                                return;
                            }
                            break;
                        }
                    case 1:
                        {
                            if (Template.DescriptionLong != null)
                            {
                                if (Template.DescriptionLong.Count > 0)
                                {
                                    if (Template.DescriptionLong.Count > DescriptionIndex)
                                    {
                                        Contruct = Template.DescriptionLong[DescriptionIndex];
                                    }
                                    else
                                    {
                                        Contruct = Template.DescriptionLong[0];
                                    }
                                }
                                else
                                {
                                    return;
                                }

                                if(Template.Description.Count > 0)
                                {
                                    if (Contruct == Template.Description[DescriptionIndex])
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                            break;
                        }
                }
                
                List<string> MentionnedCountries = new List<string>();
                foreach (Situation Sit in Situations)
                {
                    List<string> Tags = new List<string>()
                    {
                        "SIT[" + Sit.Index + "]",
                    };

                    #region Alias tags
                    if (Sit.Index == 0)
                    {
                        Tags.Add("DEP");
                    }
                    else if (Sit.Index == Situations.Count - 1)
                    {
                        Tags.Add("ARR");
                    }
                    #endregion

                    if (Sit.Airport != null)
                    {
                        string Country = Sit.Airport.CountryName;
                        foreach (string Tag in Tags)
                        {
                            Contruct = Contruct.Replace("%" + Tag + "_APT_NAME%", Sit.Airport.Name);
                            Contruct = Contruct.Replace("%" + Tag + "_CITY%", Sit.Airport.City);
                            Contruct = Contruct.Replace("%" + Tag + "_COUNTRY%", Country);

                            try
                            {
                                // %ARR_COUNTRY_IFSAME%\[\W+\]
                                MatchCollection mc = new Regex(@"%" + Tag + @"_COUNTRY_IFUNIQUE%\[.+\]").Matches(Contruct);
                                foreach (Match m in mc)
                                {
                                    if (!MentionnedCountries.Contains(Country))
                                    {
                                        string[] spl = m.Value.Split("[]".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        if (spl.Length > 1)
                                        {
                                            Contruct = Contruct.Replace(m.Value, spl[1] + Country);
                                            continue;
                                        }
                                    }
                                    Contruct = Contruct.Replace(m.Value, "");
                                }
                            }
                            catch
                            {

                            }
                        }
                        MentionnedCountries.Add(Country);
                    }
                    else
                    {

                    }

                    List<action_base> Pickups = Sit.Actions.FindAll(x => x.GetType() == typeof(cargo_pickup) || x.GetType() == typeof(cargo_pickup_2)).ToList();
                    Pickups.AddRange(Sit.ChildActions.FindAll(x => x.GetType() == typeof(cargo_pickup) || x.GetType() == typeof(cargo_pickup_2)).ToList());
                    if (Pickups.Count > 0)
                    {
                        foreach (cargo_pickup_2 Pickup in Pickups.Where(x => x.GetType() == typeof(cargo_pickup_2)))
                        {
                            foreach (string Tag in Tags)
                            {
                                //Contruct = Contruct.Replace("%" + Tag + "_[" + Pickup.UID + "]_ITEM%", Pickup.Cargo.Name);
                            }
                        }

                        foreach (cargo_pickup Pickup in Pickups.Where(x => x.GetType() == typeof(cargo_pickup)))
                        {
                            foreach (string Tag in Tags)
                            {
                                Contruct = Contruct.Replace("%" + Tag + "_[" + Pickup.UID + "]_ITEM%", Pickup.Cargo.Name);
                            }
                        }
                    }
                }

                switch (mode)
                {
                    case 0: DescriptionString = Contruct; break;
                    case 1: DescriptionLongString = Contruct; break;
                }
                
            };

            Process(0);
            Process(1);


        }

        public void GenerateStringRoute()
        {
            List<string> sits = new List<string>();
            if (Situations.Count < 4)
            {
                foreach (Situation Sit in Situations)
                {
                    if (Sit.Airport != null)
                    {
                        RouteString += Sit.Airport.ICAO + ":";
                    }
                    else
                    {
                        RouteString += Sit.Location.ToString() + ":";
                    }

                    if (Sit.Visible)
                    {
                        if (Sit.ICAO != string.Empty)
                        {
                            sits.Add(Sit.ICAO);
                        }
                        else
                        {
                            if (Template.SituationLabels[Sit.Index] != null)
                            {
                                sits.Add(Template.SituationLabels[Sit.Index]);
                            }
                            else
                            {
                                sits.Add(Math.Round(Sit.Location.Lat, 4) + ", " + Math.Round(Sit.Location.Lon, 4));
                            }
                        }
                    }
                }

                Route = string.Join(" ❯ ", sits);
            }
            else
            {
                Situation First = Situations.First();
                Situation Last = Situations.Last();

                if (First.ICAO != string.Empty)
                {
                    sits.Add(First.ICAO);
                }
                else
                {
                    if (Template.SituationLabels[First.Index] != null)
                    {
                        sits.Add(Template.SituationLabels[First.Index]);
                    }
                    else
                    {
                        sits.Add(Math.Round(First.Location.Lat, 4) + ", " + Math.Round(First.Location.Lon, 4));
                    }
                }

                sits.Add((Situations.Count - 2) + " stops");

                if (Last.ICAO != string.Empty)
                {
                    sits.Add(Last.ICAO);
                }
                else
                {
                    if (Template.SituationLabels[Last.Index] != null)
                    {
                        sits.Add(Template.SituationLabels[Last.Index]);
                    }
                    else
                    {
                        sits.Add(Math.Round(Last.Location.Lat, 4) + ", " + Math.Round(Last.Location.Lon, 4));
                    }
                }

                Route = string.Join(" ❯ ", sits);
            }
        }

        public void GenerateTopography(bool Slow = false)
        {
            lock (TopographyData)
            {
                if (TopographyData.Count == 0)
                {
                    double DistRatio = 100 / DistanceNM;
                    foreach (Situation Sit in Situations)
                    {
                        if (Slow) { Thread.Sleep(10); }
                        int Index = Situations.IndexOf(Sit);
                        if (Index > 0)
                        {
                            Situation PreviousSit = Situations[Index - 1];
                            int Count = Index < Situations.Count - 1 ? (int)(PreviousSit.DistToNext * DistRatio) : 100 - TopographyData.Count;
                            List<PointElevation> El = Topo.GetElevationAlongPath(PreviousSit.Location, Sit.Location, Count);
                            TopographyData.AddRange(El.Select(x => (int)x.Elevation));
                        }
                    };
                    Ready = true;
                    Save();
                }
            }
        }

        public void GenerateTopographyVariance()
        {
            if (TopographyData.Count > 0)
            {
                if (TopographyRange == 0)
                {
                    int? Min = null;
                    int? Max = null;
                    foreach (int Topo in TopographyData)
                    {
                        if (Min == null)
                        {
                            Min = Topo;
                            Max = Topo;
                        }
                        else
                        {
                            if (Min > Topo) { Min = Topo; }
                            if (Max < Topo) { Max = Topo; }
                        }
                    }
                    TopographyRange = (int)Max - (int)Min;
                }

                if (TopographyVariance == -1)
                {
                    int Prev = 0;
                    int i = 0;
                    foreach (int Topo in TopographyData)
                    {
                        if (i > 0)
                        {
                            TopographyVariance += Math.Abs(Prev - Topo);
                        }
                        Prev = Topo;
                        i++;
                    }
                }
            }
        }



        public List<Dictionary<string, dynamic>> GenBcastInteractionsStruct()
        {
            List<Dictionary<string, dynamic>> rs = new List<Dictionary<string, dynamic>>();

            foreach (action_base action in Actions)
            {
                action.UpdateInteractions(false);
                foreach (Interaction Inter in action.Interactions)
                {
                    if (Inter.Visible)
                    {
                        rs.Add(Inter.GetStruct());
                    }
                };
            }

            return rs;
        }

        public List<Dictionary<string, dynamic>> GenBcastLimitsStruct()
        {
            List<Dictionary<string, dynamic>> rs = new List<Dictionary<string, dynamic>>();

            if(IsMonitored)
            {
                foreach (action_base action in Actions)
                {
                    if (action.Limit != null)
                    {
                        if (action.Limit.Visible)
                        {
                            rs.Add(action.Limit.GetStruct());
                        }
                    }
                }
            }

            return rs;
        }

        public List<Dictionary<string, dynamic>> GenBcastPathStruct()
        {
            List<Dictionary<string, dynamic>> rs = new List<Dictionary<string, dynamic>>();

            foreach (Situation sit in Situations)
            {
                if (sit.Visible)
                {
                    string Name = sit.Location.Lon + "," + sit.Location.Lat;
                    if (sit.ICAO != "")
                    {
                        Name = sit.ICAO;
                    }

                    Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
                    {
                        { "Done", sit.Done },
                        { "Visited", sit.Visited },
                        { "Range", sit.InRange },
                        { "IsNext", (sit.Index == SituationAt || !sit.Adventure.Template.StrictOrder) && sit.Adventure.State == AState.Active },
                        { "Count", sit.ChildActions.FindAll(x => x.Completed != null).Count - sit.ChildActions.FindAll(x => x.Completed == true).Count },
                        { "Actions", new List<Dictionary<string, dynamic>>() }
                    };

                    foreach (action_base Action in sit.ChildActions)
                    {
                        Dictionary<string, dynamic> Struct = Action.ToListedActions();
                        if (Struct != null)
                        {
                            ns["Actions"].Add(Struct);
                        }
                    }

                    rs.Add(ns);
                }
            }

            return rs;
        }

        public Dictionary<string, dynamic> GenBcastInvoicesStruct()
        {
            if (UserData.Get("tier") != "discovery")
            {
                Dictionary<string, dynamic> ret = new Dictionary<string, dynamic>();
                List<Invoice> invoiceCollection = Invoices.GetAllFromID(ID);

                switch (State)
                {
                    case AState.Listed:
                    case AState.Saved:
                        {
                            invoiceCollection.AddRange(StagedInvoices.CreateInitialInvoice(this));
                            break;
                        }
                    case AState.Active:
                        {
                            invoiceCollection.AddRange(StagedInvoices.CreateActiveInvoice(this));
                            break;
                        }
                }


                bool feesUncertain = false;
                float feesTotal = invoiceCollection.Sum(x => (float)x.Fees.Sum(x1 =>
                {
                    if (x1.Amount != null)
                    {
                        
                        return x1.Amount + (x1.Discounts != null ? x1.Discounts.Sum(x2 => x2.Amount) : 0);
                    }
                    else
                    {
                        feesUncertain = true;
                        return 0;
                    }
                }));


                bool liabilitiessUncertain = false;
                float liabilitiesTotal = invoiceCollection.Sum(x => (float)x.Liability.Sum(x1 => {
                    if (x1.Amount != null)
                    {
                        return x1.Amount + (x1.Discounts != null ? x1.Discounts.Sum(x2 => x2.Amount) : 0);
                    }
                    else
                    {
                        liabilitiessUncertain = true;
                        return 0;
                    }
                }));
                ret.Add("TotalLiabilities", liabilitiesTotal);


                bool refundsUncertain = false;
                float refundsTotal = invoiceCollection.Sum(x => (float)x.Refunds.Sum(x1 => {
                    if (x1.Amount != null)
                    {
                        return x1.Amount + (x1.Discounts != null ? x1.Discounts.Sum(x2 => x2.Amount) : 0);
                    }
                    else
                    {
                        refundsUncertain = true;
                        return 0;
                    }
                }));
                ret.Add("TotalRefunds", refundsTotal);


                ret.Add("TotalFees", feesTotal);
                ret.Add("TotalProfits", RewardBux - feesTotal - refundsTotal);
                ret.Add("UncertainRefunds", refundsUncertain);
                ret.Add("UncertainFees", feesUncertain);
                ret.Add("UncertainLiabilities", liabilitiessUncertain);
                ret.Add("Invoices", invoiceCollection.Select(x => x.ToDictionary()));

                return ret;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, dynamic> GenBcastManifests()
        {
            List<AircraftInstance> aircraft = new List<AircraftInstance>();

            Situation LastWithApt = Template.ImageURL.Count > 0 ? null : Situations.FindLast(x => x.Airport != null);
            string SelImageURL = Template.ImageURL.Count > 0 ? Template.ImageURL[ImageIndex] : LastWithApt != null ? "%imagecdn%/airports/" + LastWithApt.Airport.ICAO + ".jpg" : "";

            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "Cargo", new List<Dictionary<string, dynamic>>() },
                { "Adventure", new Dictionary<string, dynamic>()
                    {
                        { "ID", ID },
                        { "Name", Template.Name },
                        { "FileName", Template.FileName },
                        { "Route", Route },
                        { "ImageURL", SelImageURL.Replace("%imagecdn%", APIBase.CDNImagesHost) },
                    }
                }
            };


            if (IsMonitored)
            {
                foreach (cargo_pickup_2 cargo_pickup in Actions.Where(x => x.GetType() == typeof(cargo_pickup_2)))
                {
                    Dictionary<string, dynamic> pickup_rs = new Dictionary<string, dynamic>()
                    {
                        { "PickupID", cargo_pickup.UID },
                        { "Manifests", cargo_pickup.CargoManifests.Select(x => x.ExportListing()) }
                    };

                    rs["Cargo"].Add(pickup_rs);

                    foreach (var acfList in cargo_pickup.CargoManifests.Select(x => x.GetAircraft()))
                    {
                        foreach (var acf in acfList.Where(x => x != null))
                        {
                            if (!aircraft.Contains(acf))
                            {
                                aircraft.Add(acf);
                            }
                        }
                    }
                }

                rs.Add("Aircraft", aircraft.Count > 0 ? aircraft.Select(x => x.ToListing()) : new List<Dictionary<string, dynamic>>());

                return rs;
            }
            else
            {
                return rs;
            }
        }



        public void BroadcaseEndCard()
        {
            switch(_State)
            {
                case AState.Succeeded:
                case AState.Failed:
                    {
                        string Message = "";
                        if(Template.Name != string.Empty)
                        {
                            Message = "You " + (_State == AState.Succeeded ? "completed" : "failed") + " " + Template.Name  + ". " + EndSummary;
                        }
                        else
                        {
                            Message = "You " + (_State == AState.Succeeded ? "completed" : "failed") + " your " + Route + " " + Template.TypeLabel + ". " + EndSummary;
                        }


                        NotificationService.Add(new Notification()
                        {
                            Title = (_State == AState.Succeeded ? "Completed" : "Failed") + " " + Template.TypeLabel,
                            Message = Message,
                            Type = _State == AState.Succeeded ? NotificationType.Success : NotificationType.Fail,
                            Data = new Dictionary<string, dynamic>()
                            {
                                { "Contract", new Dictionary<string, dynamic>()
                                    {
                                        { "State", GetDescription(_State) },
                                        { "EndSummary", EndSummary },
                                        { "Route", Route },
                                    }
                                },
                                { "Template", new Dictionary<string, dynamic>()
                                    {
                                        { "Name", Template.Name },
                                    }
                                },
                                { "EndCard", ConfirmedReward }
                            }
                        });
                        break;
                    }
            }
        }

        public void BroadcastManifests()
        {
            ScheduleManifestsBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:manifest", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "ID", ID },
                { "Manifests", GenBcastManifests() }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastState()
        {
            ScheduleStateBroadcast = false;

            Dictionary<string, dynamic> ss = new Dictionary<string, dynamic>()
            {
                { "ID", ID },
                { "State", GetDescription(_State) },
                { "IsMonitored", IsMonitored },
                { "RangeAirports", new List<Dictionary<string, dynamic>>() },
                { "AircraftCompatible", AircraftCompatible }
            };
            
            if (State == AState.Active && !IsMonitored && LastLocationGeo != null)
            {
                var aptr = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastLocationGeo, 50);
                if (aptr[0].Value != null)
                {
                    foreach (var Apt in aptr)
                    {
                        if (ss["RangeAirports"].Count < 10)
                        {
                            ss["RangeAirports"].Add(Apt.Value.ToSummary(false));
                        }
                    }
                }
            }

            if (LastLocationGeo != null) { ss.Add("LastLocationGeo", LastLocationGeo.ToList(6)); }
            if (EndSummary != string.Empty) { ss.Add("EndSummary", EndSummary); }
            if (StartedAt != null) { ss.Add("StartedAt", ((DateTime)StartedAt).ToString("O")); }
            if (CompletedAt != null) { ss.Add("CompletedAt", ((DateTime)CompletedAt).ToString("O")); }

            APIBase.ClientCollection.SendMessage("adventure:state", JSSerializer.Serialize(ss), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastLimits()
        {
            ScheduleLimitsBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:limits", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "ID", ID },
                { "Limits", GenBcastLimitsStruct() }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastMemos()
        {
            ScheduleMemosBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:memos", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "ID", ID },
                { "Memos", Memos.ToDictionary() }
            }), null, APIBase.ClientType.Skypad);

        }

        public void BroadcastInteractions()
        {
            ScheduleInteractionBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:interactions", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "ID", ID },
                { "Interactions", GenBcastInteractionsStruct() }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastPath()
        {
            SchedulePathBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:path", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "ID", ID },
                { "Path", GenBcastPathStruct() }
            }), null, APIBase.ClientType.Skypad);
        }
        
        public void BroadcastInvoices()
        {
            ScheduleInvoiceBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:invoices", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "ID", ID },
                { "Invoices", GenBcastInvoicesStruct() }
            }), null, APIBase.ClientType.Skypad);
        }


       
        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            if (!PersistenceRestored)
            {
                Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(new Dictionary<string, dynamic>()
                {
                    { "ID", Convert.ToInt64(payload_struct["ID"]) },
                    { "Success", false },
                    { "Reason", "Not loaded yet" }
                }), (Dictionary<string, dynamic>)structure["meta"]);
                return;
            }

            Adventure Adv = null;

            lock(AllContracts)
            {
                long ID = Convert.ToInt64(payload_struct["ID"]);
                Adv = AllContracts.Find(x => x.ID == ID);
            }
           
            if (Adv != null)
            {
                switch (StructSplit[1])
                {
                    case "routes":
                        {
                            switch (StructSplit[2])
                            {
                                case "get":
                                    {
                                        #region GetRoutes
                                        Dictionary<string, int> Countries = new Dictionary<string, int>();

                                        int Limit = payload_struct["limit"];
                                        List<dynamic> routes = new List<dynamic>();
                                        Dictionary<string, dynamic> ret = new Dictionary<string, dynamic>()
                                        {
                                            { "Routes", routes },
                                        };

                                        lock(Adv.Template.Routes)
                                        {
                                            foreach (var rte in Adv.Template.Routes)
                                            {
                                                routes.Add(rte.ToSummary(5));
                                                foreach (var sit in rte.Situations)
                                                {
                                                    if (sit.Airport != null)
                                                    {
                                                        if (!Countries.ContainsKey(sit.Airport.Country))
                                                        {
                                                            Countries.Add(sit.Airport.Country, 1);
                                                        }
                                                        else
                                                        {
                                                            Countries[sit.Airport.Country]++;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (payload_struct["GetAll"])
                                        {
                                            foreach (var template in Templates.FindAll(x => x.Activated && x.TemplateCode == Adv.Template.TemplateCode && x != Adv.Template))
                                            {
                                                foreach (var rte in template.Routes)
                                                {
                                                    routes.Add(rte.ToSummary(5));
                                                    foreach(var sit in rte.Situations)
                                                    {
                                                        if(sit.Airport != null)
                                                        {
                                                            if(!Countries.ContainsKey(sit.Airport.Country))
                                                            {
                                                                Countries.Add(sit.Airport.Country, 1);
                                                            }
                                                            else
                                                            {
                                                                Countries[sit.Airport.Country]++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        var OrderedCountries = Countries.OrderByDescending(x => x.Value).Select(x => x.Key);

                                        ret.Add("Count", routes.Count);
                                        ret.Add("Countries", OrderedCountries);

                                        while (routes.Count > Limit)
                                        {
                                            routes.RemoveAt(Utils.GetRandom(routes.Count));
                                        }

                                        Socket.SendMessage("adventure:routes:get", JSSerializer.Serialize(ret), (Dictionary<string, dynamic>)structure["meta"]);
                                        #endregion
                                        return;
                                    }
                            }
                            break;
                        }
                    case "invoices":
                        {
                            Socket.SendMessage("adventure:invoices", JSSerializer.Serialize(new Dictionary<string, dynamic>()
                            {
                                { "ID", Adv.ID },
                                { "Invoices", Adv.GenBcastInvoicesStruct() }
                            }), (Dictionary<string, dynamic>)structure["meta"]);
                            return;
                        }
                    case "save":
                        {
                            Dictionary<string, dynamic> ret = new Dictionary<string, dynamic>()
                            {
                                { "Success", false },
                                { "ID", Adv.ID },
                                { "State", GetDescription(Adv.State) },
                                { "Reason", "Unknown" }
                            };
                            
                            if (Adv.State == AState.Listed)
                            {
                                Adv.State = AState.Saved;
                                ret["Success"] = true;
                                ret["State"] = GetDescription(Adv.State);
                                ret.Remove("Reason");
                            }
                            else
                            {
                                ret["Reason"] = Adv.ID + " no longer listed";
                            }

                            Socket.SendMessage("adventure:save", JSSerializer.Serialize(ret), (Dictionary<string, dynamic>)structure["meta"]);
                            return;
                        }
                    case "commit":
                        {
                            Dictionary<string, dynamic> ret = new Dictionary<string, dynamic>()
                            {
                                { "Success", false },
                                { "ID", Adv.ID },
                                { "State", GetDescription(Adv.State) },
                                { "Reason", "Unknown" }
                            };

                            if (UserData.Get("tier") != "discovery")
                            {
                                if (Adv.InvoiceQuote != null)
                                {
                                    Adv.State = AState.Active;
                                    ret["Success"] = true;
                                    ret["State"] = GetDescription(Adv.State);
                                    ret.Remove("Reason");
                                }
                                else
                                {
                                    ret["Reason"] = Adv.ID + " commit quote doesn't match existing quote";
                                }
                            }
                            else
                            {
                                Adv.State = AState.Active;
                                ret["Success"] = true;
                                ret["State"] = GetDescription(Adv.State);
                                ret.Remove("Reason");
                            }

                            Socket.SendMessage("adventure:commit", JSSerializer.Serialize(ret), (Dictionary<string, dynamic>)structure["meta"]);
                            return;
                        }
                    case "flightplan":
                        {
                            switch (StructSplit[2])
                            {
                                case "load":
                                    {
                                        List<string> Content = ((string)payload_struct["content"]).Split('\n').ToList();
                                        FlightPlan Plan = Plans.PlanConverters.Find(x => x.Ext == ".pln").ReadContent(Content);
                                        Adv.CustomPlans.Add(Plan);
                                        Dictionary<string, dynamic> Struct = new Dictionary<string, dynamic>()
                                        {
                                            { "ID", Adv.ID },
                                            { "Plan", Plan.ToDictionary(true) }
                                        };
                                        string command = "adventure:flightplan:add:" + Convert.ToString(payload_struct["ID"]);
                                        Socket.SendMessage(command, JSSerializer.Serialize(Struct), (Dictionary<string, dynamic>)structure["meta"]);
                                        return;
                                    }
                                case "send":
                                    {
                                        if (ConnectedInstance != null)
                                        {
                                            ConnectedInstance.SendFlightPlan(Path.Combine(AppDataDirectory, "Plans", Adv.ID.ToString() + ".pln"));
                                            //Connectors.SimConnection.ConnectedInstance.SendFlightPlan(@"C:\Users\keven\Documents\Prepar3D v4 Files\CYYZKMSP01.pln");
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case "remove":
                        {
                            Adv.Remove();
                            Dictionary<string, dynamic> ret = new Dictionary<string, dynamic>()
                            {
                                { "Success", true },
                                { "ID", Adv.ID },
                            };
                            Socket.SendMessage("adventure:remove", JSSerializer.Serialize(ret), (Dictionary<string, dynamic>)structure["meta"]);
                            break;
                        }
                    case "cancel":
                        {
                            Adv.Fail("You cancelled this " + Adv.Template.TypeLabel + ".");
                            Dictionary<string, dynamic> ret = new Dictionary<string, dynamic>()
                            {
                                { "Success", true },
                                { "ID", Adv.ID },
                            };
                            Socket.SendMessage("adventure:cancel", JSSerializer.Serialize(ret), (Dictionary<string, dynamic>)structure["meta"]);
                            break;
                        }
                    case "pause":
                        {
                            Adv.SetMonitorState(false);
                            break;
                        }
                    case "resume":
                        {
                            Adv.CheckMonitorState(true);
                            break;
                        }
                    case "begin":
                        {
                            Adv.State = AState.Active;
                            break;
                        }
                    case "interaction":
                        {
                            if (Adv.IsMonitored)
                            {
                                KeyValuePair<int, action_base> Action = Adv.ActionsIndex.Find(x => x.Key == (int)Convert.ToInt32(payload_struct["Link"]));
                                if (Action.Value != null)
                                {
                                    Action.Value.Interact(payload_struct);
                                }
                            }
                            break;
                        }
                }

                Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(new Dictionary<string, dynamic>()
                {
                    { "ID", Adv.ID },
                    { "Success", true },
                }), (Dictionary<string, dynamic>)structure["meta"]);
            }
            else
            {
                Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(new Dictionary<string, dynamic>()
                {
                    { "ID", Convert.ToInt64(payload_struct["ID"]) },
                    { "Success", false },
                    { "Reason", "Doesn't exist" }
                }), (Dictionary<string, dynamic>)structure["meta"]);
            }
        }
              


        public bool ExportContract()
        {
            if (MW.IsShuttingDown)
            {
                return false;
            }

            //if(IsDev || IsBeta)
            //{
                //Console.WriteLine("Adventure " + Route + " - " + Template.FileName + " (" + ID + ") will Save");
            //}

            try
            {
                string FN = Template.FileName;
                if (FN != string.Empty)
                {
                    //string PersistenceFolder = Path.Combine(AppDataDirectory, "Persistence", FN);
                    //string FileName = ID + ".dat";
                    //string DocumentsFolder = "";
                    switch (State)
                    {
                        case AState.Active:
                        case AState.Saved:
                        case AState.Succeeded:
                        case AState.Failed:
                            {
                                #region To Db
                                var DBCollection = LiteDbService.DB.Database.GetCollection("adventures_active");
                                var DBCollection1 = LiteDbService.DB.Database.GetCollection("templates_active");
                                var DBCollection2 = LiteDbService.DBCache.Database.GetCollection("adventures_listed");
                                BsonDocument bdoc = BsonMapper.Global.ToDocument(ToDictFull());
                                
                                // Remove from Listed DB
                                try
                                {
                                    lock(LiteDbService.DBCache)
                                    {
                                        var inListed = DBCollection2.FindOne(x => x["_id"] == ID);
                                        if (inListed != null)
                                        {
                                            DBCollection2.Delete(ID);
                                        }
                                    }
                                }
                                catch
                                {
                                }

                                // Add Template to DB
                                try
                                {
                                    lock (LiteDbService.DB)
                                    {
                                        string Ticks = Template.ModifiedOn.ToString("O");
                                        var existing = DBCollection1.FindOne(x => x["File"] == FN && x["ModifiedOn"] == Ticks);

                                        if (existing == null)
                                        {
                                            BsonDocument bd = BsonMapper.Global.ToDocument(Template.ToDictFull());
                                            DBCollection1.Upsert(Template.ModifiedOn.Ticks, bd);
                                        }
                                    }
                                }
                                catch
                                {
                                }

                                // Add contract to persistence
                                try
                                {
                                    lock (LiteDbService.DB.Database)
                                    {
                                        DBCollection.Upsert(this.ID, bdoc);
                                    }
                                }
                                catch (Exception ex1)
                                {
                                    lock (LiteDbService.DB.Database)
                                    {
                                        Console.WriteLine("Failed to update contract in adventures_active. " + ex1.Message);
                                        try
                                        {
                                            DBCollection.Delete(this.ID);
                                            DBCollection.Insert(bdoc);
                                        }
                                        catch (Exception ex2)
                                        {
                                            Console.WriteLine("Failed to fallback update contract in adventures_active. " + ex2.Message);
                                        }
                                    }
                                }

                                #endregion
                                break;
                            }
                        default:
                            {
                                #region To Db
                                if(Template.Ready)
                                {
                                    try
                                    {
                                        var DBCollection = LiteDbService.DBCache.Database.GetCollection("adventures_listed");
                                        BsonDocument bdoc = new BsonDocument()
                                        {
                                            ["json"] = JSSerializer.Serialize(ToDictBase()),
                                        };

                                        lock (LiteDbService.DBCache)
                                        {
                                            try
                                            {
                                                DBCollection.Upsert(this.ID, bdoc);
                                            }
                                            catch (Exception ex1)
                                            {
                                                Console.WriteLine("Failed to update contract in adventures_listed. " + ex1.Message);
                                                try
                                                {
                                                    LiteDbService.DBCache.Reset = true;
                                                    MW.Shutdown(true);
                                                }
                                                catch (Exception ex2)
                                                {
                                                    Console.WriteLine("Failed to fallback update contract in adventures_listed. " + ex2.Message);
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                                #endregion
                                break;
                            }
                    }
                }
                //Console.WriteLine("Adventure " + Route + " - " + Template.FileName + " (" + ID + ") has Saved");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save Contract for " + Template.FileName + " with ID " + ID + " / " + ex.Message + Environment.NewLine + ex.StackTrace);
                NotificationService.Add(new Notification()
                {
                    Title = "Unable to save your progress on " + Template.TypeLabel + " " + Route,
                    Message = "The Transponder failed to save this " + Template.TypeLabel + " to your Documents folder. Please contact Parallel 42 via the official support channel with the following error: (" + ex.HResult.ToString() + ") " + ex.Message + "",
                });
                Thread.Sleep(1000);
                return false;
            }
            
        }
        
        public Dictionary<string, dynamic> ToDictBase()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "RequestStatus", "ready" },
                { "FileName", Template.FileName },
                { "ModifiedOn", Template.ModifiedOn.ToString("O") },
                { "State", (int)_State },
                { "ImageIndex", ImageIndex },
                { "DescriptionIndex", DescriptionIndex },
                { "RecommendedAircraft", RecommendedAircraft },
                { "Distance", DistanceNM },
                { "RewardXP", RewardXP },
                { "RewardBux", RewardBux },
                { "RewardKarma", RewardKarma },
                { "ExpireAt", ExpireAt.ToString("O") },
                { "PullAt", PullAt != null ? ((DateTime)PullAt).ToString("O") : null },
                { "CreatedAt", CreatedAt.ToString("O") },
                { "RouteCode", RouteCode },
                { "Situations", new List<Dictionary<string, dynamic>>() },
                { "Actions", new Dictionary<string, Dictionary<string, dynamic>>() },
                { "Topo", string.Join(":", TopographyData) }
            };

            foreach (KeyValuePair<int, action_base> action in ActionsIndex)
            {
                Dictionary<string, dynamic> Exp = action.Value.ExportState();
                if(Exp.Keys.Count > 0)
                {
                    ns["Actions"].Add(Convert.ToString(action.Key), Exp);
                }
            }

            foreach (Situation Sit in Situations)
            {
                ns["Situations"].Add(Sit.GetExportState());
            }

            return ns;
        }

        public Dictionary<string, dynamic> ToDictFull()
        {
            Dictionary<string, dynamic> ns = ToDictBase();
            foreach(var entry in new Dictionary<string, dynamic>()
            {
                { "StartedAt", StartedAt != null ? ((DateTime)StartedAt).ToString("O") : null },
                { "RequestedAt", RequestedAt != null ? ((DateTime)RequestedAt).ToString("O") : null },
                { "CompletedAt", CompletedAt != null ? ((DateTime)CompletedAt).ToString("O") : null },
                { "LastResumed", LastResumed != null ? ((DateTime)LastResumed).ToString("O") : null },
                { "LastLocationGeo", LastLocationGeo != null ? LastLocationGeo.ToList() : null },
                { "LastLocationApt", LastLocationApt != null ? LastLocationApt.ICAO : null },
                { "EndSummary", EndSummary },
                { "Description", DescriptionString },
                { "DescriptionLong", DescriptionLongString },
                { "Cleanedup", Cleanedup },
                { "Path", GenBcastPathStruct() },
            })
            {
                ns.Add(entry.Key, entry.Value);
            }

            ns.Add("Memos", Memos.ToDictionary());

            return ns;
        }
        
        public Dictionary<string, dynamic> GetListing(bool detailed, bool flightplans)
        {
            Situation LastWithApt = Template.ImageURL.Count > 0 ? null : Situations.FindLast(x => x.Airport != null);
            string SelImageURL = Template.ImageURL.Count > 0 ? Template.ImageURL[ImageIndex] : LastWithApt != null ? "%imagecdn%/airports/" + LastWithApt.Airport.ICAO + ".jpg" : "";

            if (detailed)
            {
                Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
                {
                    { "RequestStatus", "ready" },
                    { "ID", ID },
                    { "IsMonitored", IsMonitored },
                    { "LastLocationGeo", LastLocationGeo != null ? LastLocationGeo.ToList(6) : null },
                    { "RangeAirports", new List<Dictionary<string, dynamic>>() },
                    { "AircraftCompatible", AircraftCompatible },
                    { "RecommendedAircraft", RecommendedAircraft },
                    { "Route", Route },
                    { "EndSummary", EndSummary != string.Empty ? EndSummary : null },
                    { "Description", DescriptionString },
                    { "DescriptionLong", DescriptionLongString },
                    { "FileName", Template.FileName },
                    { "ImageURL", SelImageURL.Replace("%imagecdn%", APIBase.CDNImagesHost) },
                    { "Distance", Math.Round(DistanceNM) },
                    { "State", GetDescription(_State) },
                    { "RouteCode", RouteValidated ? RouteCode : "" },
                    { "MediaLink", GetMediaLinks() },
                    { "ExpireAt", ExpireAt.ToString("O") },
                    { "CompletedAt", CompletedAt != null ? ((DateTime)CompletedAt).ToString("O") : null },
                    { "StartedAt", StartedAt != null ? ((DateTime)StartedAt).ToString("O") : null },
                    { "PullAt", PullAt != null ? ((DateTime)PullAt).ToString("O") : null },
                    { "RequestedAt", null },
                    { "Situations", new List<Dictionary<string, dynamic>>() },
                    { "Interactions", GenBcastInteractionsStruct() },
                    { "Path", GenBcastPathStruct() },
                    { "Manifests", GenBcastManifests() },
                    { "Limits", GenBcastLimitsStruct() },
                    { "Memos", Memos.ToDictionary() },
                    { "Topo", TopographyData },
                    { "Invoices", GenBcastInvoicesStruct() }
                };
                

                if (UserData.Get("tier") != "discovery")
                {
                    ns.Add("RewardXP", RewardXP);
                    ns.Add("RewardKarma", RewardKarma);
                    ns.Add("RewardBux", RewardBux);
                    ns.Add("RewardReliability", Template.RatingGainSucceed);
                }

                if (State == AState.Active && !IsMonitored && LastLocationGeo != null)
                {
                    var aptr = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastLocationGeo, 50);
                    if(aptr[0].Value != null)
                    {
                        foreach (var Apt in aptr)
                        {
                            if (ns["RangeAirports"].Count < 10)
                            {
                                ns["RangeAirports"].Add(Apt.Value.ToSummary(false));
                            }
                        }
                    }
                }

                if (flightplans)
                {
                    List<Dictionary<string, dynamic>> FPs = new List<Dictionary<string, dynamic>>();

                    List<FlightPlan> L = GetRelatedFlightPlans();

                    foreach(var Fp in LegsFlightPlans)
                    {
                        FPs.Add(Fp.ToDictionary(true));
                    }

                    foreach (FlightPlan Plan in CustomPlans)
                    {
                        FPs.Add(Plan.ToDictionary(true));
                    }
                    
                    foreach (FlightPlan Plan in L)
                    {
                        FPs.Add(Plan.ToDictionary(true));
                    }

                    ns.Add("Flightplans", FPs);
                }

                if(RequestedAt != null)
                {
                    ns["RequestedAt"] = ((DateTime)RequestedAt).ToString("O");
                }

                foreach (Situation Sit in Situations)
                {
                    if (Sit.Visible)
                    {
                        ns["Situations"].Add(Sit.ToListing(false));
                    }
                }

                return ns;
            }
            else
            {
                Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
                {
                    { "RequestStatus", "ready" },
                    { "ID", ID },
                    { "Route", Route },
                    { "Description", DescriptionString },
                    { "RecommendedAircraft", RecommendedAircraft },
                    { "FileName", Template.FileName },
                    { "ImageURL", SelImageURL.Replace("%imagecdn%", APIBase.CDNImagesHost) },
                    { "Distance", Math.Round(DistanceNM) },
                    { "State", GetDescription(_State) },
                    { "RouteCode", RouteValidated ? RouteCode : "" },
                    { "ExpireAt", ExpireAt.ToString("O") },
                    { "PullAt", PullAt != null ? ((DateTime)PullAt).ToString("O") : null },
                    { "Situations", new List<Dictionary<string, dynamic>>() },
                    { "Path", GenBcastPathStruct() },
                    { "Topo", TopographyData }
                };

                if (UserData.Get("tier") != "discovery")
                {
                    ns.Add("RewardXP", RewardXP);
                    ns.Add("RewardBux", RewardBux);
                    ns.Add("RewardKarma", RewardKarma);
                    ns.Add("RewardReliability", Template.RatingGainSucceed);
                }

                foreach (Situation Sit in Situations)
                {
                    if (Sit.Visible)
                    {
                        ns["Situations"].Add(Sit.ToListing(false));
                    }
                }

                return ns;
            }
        }



        public override string ToString()
        {
            return ID + ", " + string.Join("|", Template.CompanyStr) + ", " + GetDescription(_State) + ", " + RouteString + ", " + DistanceNM;
        }

        public enum AState
        {
            [EnumValue("Listed")]
            Listed,
            [EnumValue("Saved")]
            Saved,
            [EnumValue("Active")]
            Active,
            [EnumValue("Succeeded")]
            Succeeded,
            [EnumValue("Failed")]
            Failed,
        }
    }

}
