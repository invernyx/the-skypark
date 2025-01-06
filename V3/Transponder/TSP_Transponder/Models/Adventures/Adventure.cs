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
using static TSP_Transponder.Attributes.EnumAttr;
using TSP_Transponder.Utilities;
using TSP_Transponder.Models.Connectors;
using System.Diagnostics;
using TSP_Transponder.Models.Payload;

namespace TSP_Transponder.Models.Adventures
{
    public class Adventure
    {
        public AdventureTemplate Template = null;
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
                                SetPayload();

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
                                SetPayload();

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

                                    foreach(var situation in Situations)
                                    {
                                        situation.ResetStates();
                                    }

                                    SetMonitorState(false);

                                    GoogleAnalyticscs.TrackEvent("Tier", UserData.Get("tier"), "Failed", (int)Math.Round(DistanceNM));
                                    GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, "Failed", (int)Math.Round(DistanceNM));

                                    RichPresence.Update();
                                    BroadcastInteractions();
                                    BroadcaseEndCard();
                                    BroadcastPath();
                                    BroadcastState();
                                    BroadcastInvoices();
                                    SetPayload();
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

                                foreach (var situation in Situations)
                                {
                                    situation.ResetStates();
                                }

                                SetMonitorState(false);

                                GoogleAnalyticscs.TrackEvent("Tier", UserData.Get("tier"), "Completed", (int)Math.Round(DistanceNM));
                                GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, "Completed", (int)Math.Round(DistanceNM));

                                RichPresence.Update();
                                BroadcastInteractions();
                                BroadcastPath();
                                BroadcastState();
                                BroadcaseEndCard();
                                BroadcastInvoices();
                                SetPayload();
                                
                            }
                            break;
                        }
                }

                SetInTemplateList(true);
                ScheduleInteractionBroadcast = true;
                SchedulePathBroadcast = true;
            }
        }
        public bool IsMonitored {
            get
            {
                return _IsMonitored;
            }
            set
            {
                if(_IsMonitored != value)
                {
                    Console.WriteLine("--- Adventure " + Route + " (" + ID + ") has monitor to " + value);

                    if(value)
                    {
                        TotalActiveTimeTimer.Restart();
                    }
                    else
                    {
                        TotalActiveTimeSeconds = TotalActiveTimeTimer.Elapsed.TotalSeconds;
                        TotalActiveTimeTimer.Reset();
                    }

                    if (!value)
                    {
                        foreach (action_base action in Actions)
                        {
                            action.PausingPreview();
                        }
                    }

                    _IsMonitored = value;

                    if (SimConnection.Aircraft != null)
                        SimConnection.Aircraft.Process();

                    if (_IsMonitored)
                    {
                        if (!AircraftUsed.Contains(SimConnection.Aircraft.LastLivery))
                        {
                            AircraftUsed.Add(SimConnection.Aircraft.LastLivery);
                        }

                        foreach (var Sit in Situations)
                        {
                            Sit.InRange = false;
                        }
                    }

                    if (!value)
                    {
                        foreach (action_base action in Actions)
                        {
                            action.Pausing();
                        }
                    }
                    
                    SchedulePayloadUpdate = true;
                    ScheduleInteractionBroadcast = true;
                    ScheduleLimitsBroadcast = true;
                    ScheduleManifestsBroadcast = true;
                    ScheduleManifestsStateBroadcast = true;
                    ScheduleStateBroadcast = true;
                    ProcessBroadcasts();
                    Save();

                }
            }
        }
        public short ImageIndex = -1;
        public short DescriptionIndex = -1;
        public string Title = null;
        public string SocketID = "";
        public float DistanceNM = 0;
        public float RewardXP = 0;
        public int RewardBux = 0;
        public float RewardKarma = 0;
        public ushort[] DurationRange = new ushort[] { 0, 0 }; // In Minutes
        public Dictionary<string, float> ConfirmedReward = null;
        public bool WavePenalties = false;
        public float TotalValue = 0;
        public bool RouteValidated = false;
        public string RouteCode = "";
        public string RouteString = "";
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
        public Stopwatch TotalActiveTimeTimer = new Stopwatch();
        public double TotalActiveTimeSeconds = 0;
        public int? NextCheckedForUpcomingContracts = 0;
        public bool Cleanedup = false;
        public int SituationAt
        {
            get
            {
                //if (!Template.StrictOrder)
                //{
                //    return Situations.Count - 1;
                //}
                //else
                //{
                    return Situations.FindIndex(x => !x.Done);
                //}
            }
        }

        [ClassSerializerField("state")]
        private AState _State = AState.Listed;
        [ClassSerializerField("id")]
        public long ID = 0;
        [ClassSerializerField("ready")]
        public bool Ready = false;
        [ClassSerializerField("is_monitored")]
        public bool _IsMonitored = false;
        [ClassSerializerField("aircraft_compatible")]
        public bool AircraftCompatible = false;
        [ClassSerializerField("end_summary")]
        public string EndSummary = null;
        [ClassSerializerField("description")]
        public string DescriptionString = "";
        [ClassSerializerField("description_long")]
        public string DescriptionLongString = "";
        [ClassSerializerField("route")]
        public string Route = "";
        [ClassSerializerField("operated_for")]
        public List<string> OperatedFor = null;
        [ClassSerializerField("recommended_aircraft")]
        public List<float> RecommendedAircraft = null;
        [ClassSerializerField("topo")]
        public List<int> TopographyData = new List<int>();
        [ClassSerializerField("aircraft_used")]
        public List<string> AircraftUsed = new List<string>();

        public List<Dictionary<string, dynamic>> Limits
        {
            get
            {
                List<Dictionary<string, dynamic>> rs = new List<Dictionary<string, dynamic>>();

                // Aircraft Reqs
                if (Template.AircraftRestriction.Count > 0)
                {
                    var new_limit = new Limit(0, this)
                    {
                        Visible = true,
                        Enabled = false,
                        Params = new dynamic[] { Template.AircraftRestrictionLabel, Template.AircraftRestriction },
                        Type = "aircraft_type",
                    };
                    rs.Add(new_limit.GetStruct());
                }

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

                // Cargo Pods Reqs
                var req_pods = GetRequiredPods();
                if (req_pods > 0)
                {
                    var new_limit = new Limit(0, this)
                    {
                        Visible = true,
                        InSummary = false,
                        IsPreStart = true,
                        Params = new dynamic[] { req_pods },
                        Type = "cargo_required",
                    };
                    rs.Add(new_limit.GetStruct());
                }

                return rs;
            }
        }
        public List<Dictionary<string, dynamic>> Interactions
        {
            get
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
        }
        
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
        public bool ScheduleManifestsStateBroadcast = false;
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
            if (Restore.ContainsKey("Title"))
                Title = Restore["Title"];

            if (Restore.ContainsKey("OperatedFor"))
                OperatedFor = Restore["OperatedFor"] != null ? ((ArrayList)Restore["OperatedFor"]).Cast<string>().ToList() : null;
            
            if (Restore.ContainsKey("AircraftUsed"))
                AircraftUsed = ((ArrayList)Restore["AircraftUsed"]).Cast<string>().ToList();

            if (Restore.ContainsKey("RouteCode"))
                RouteCode = Restore["RouteCode"];
            
            if (Restore.ContainsKey("EndSummary"))
                EndSummary = Restore["EndSummary"];

            if (Restore.ContainsKey("LastLocationGeo"))
                LastLocationGeo = Restore["LastLocationGeo"] != null ? new GeoLoc(Convert.ToDouble(Restore["LastLocationGeo"][0]), Convert.ToDouble(Restore["LastLocationGeo"][1])) : null;

            if (Restore.ContainsKey("Cleanedup"))
                Cleanedup = Convert.ToBoolean(Restore["Cleanedup"]);

            if (Restore.ContainsKey("StartedAt"))
                StartedAt = Restore["StartedAt"] != null ? DateTime.Parse((string)Restore["StartedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;

            if (Restore.ContainsKey("LastResumed"))
                LastResumed = Restore["LastResumed"] != null ? DateTime.Parse((string)Restore["LastResumed"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;

            if (Restore.ContainsKey("CompletedAt"))
                CompletedAt = Restore["CompletedAt"] != null ? DateTime.Parse((string)Restore["CompletedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;

            if (Restore.ContainsKey("RequestedAt"))
                RequestedAt = Restore["RequestedAt"] != null ? DateTime.Parse((string)Restore["RequestedAt"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : (DateTime?)null;

            if (Restore.ContainsKey("TotalActiveTimeSeconds"))
                TotalActiveTimeSeconds = Convert.ToDouble(Restore["TotalActiveTimeSeconds"]);

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
            //if (Restore.ContainsKey("Memos"))
            //{
            //    try
            //    {
            //        Memos = new Chat((Dictionary<string, dynamic>)Restore["Memos"]);
            //    }
            //    catch
            //    {
            //    }
            //}
            #endregion

            #region Recommended aircraft
            if(Template.AircraftRecommendation != null)
            {
                RecommendedAircraft = new List<float>();
                foreach (var val in Template.AircraftRecommendation)
                {
                    RecommendedAircraft.Add(Convert.ToSingle(val));
                }
            }
            else if (Restore.ContainsKey("RecommendedAircraft"))
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

            #region Restore Actions
            foreach (action_base Action in Actions)
            {
                Action.ImportState(Action.Parameters);
                Action.Parameters = null;
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
            GenerateDurationRange();
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
                GenerateTopography(false);
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

                        if (!TemporalLast.IS_SLEW_ACTIVE && IsLoaded)
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
                                    action.Value.ProcessAdv();

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

        public void Start()
        {
            List<Dictionary<string, dynamic>> objections_list = new List<Dictionary<string, dynamic>>();

            foreach (action_base action in Actions)
            {
                action.PreStart((objections) =>
                {
                    objections_list.AddRange(objections);
                });
            }

            // Validate the amount of cargo pods required
            var req = GetRequiredPods();
            var cur = SimConnection.Aircraft.Cabin.GetAvailablePods();
            if (req > cur)
            {
                objections_list.Add(new Dictionary<string, dynamic>()
                {
                    { "cargo_missing", new int[] { req, cur } }
                });
            }
            
            if (objections_list.Count == 0)
            {
                State = AState.Active;
            }
            else
            {

            }
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
            if (reason == null)
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

                        CheckUpcomingContracts();
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
                        DBCollection = LiteDbService.DBCache.Database.GetCollection("adventures_listed");

                    lock (LiteDbService.DBCache)
                        DBCollection.Delete(ID);

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

            Ready = false;

            if (State != AState.Listed && !Cleanedup)
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

            lock (Template.LiveAdventures)
            {
                Template.LiveAdventures.Remove(this);
            }
            
            bool f = false;
            lock (LiteDbService.DB)
            {
                var Collection = LiteDbService.DB.Database.GetCollection("adventures_active");
                var Existing = Collection.FindById(ID);
                if(Existing != null)
                {
                    if (Collection.Delete(ID))
                    {
                        LiteDbService.DB.Database.Checkpoint();
                        f = true;
                    }
                    else
                    {

                    }
                }
                else
                {
                    f = true;
                }
            }

            lock(LiteDbService.DBCache)
            {
                var Collection = LiteDbService.DBCache.Database.GetCollection("adventures_listed");

                var Existing = Collection.FindById(ID);
                if (Existing != null)
                {
                    var count = Collection.LongCount();

                    if (Collection.Delete(ID))
                    {
                        LiteDbService.DBCache.Database.Checkpoint();
                        f = true;
                    }
                    else
                    {

                    }

                    if (Collection.LongCount() == count)
                    {

                    }
                }
                else
                {
                    f = true;
                }


            }

            if(!f)
            {
                Console.WriteLine("Failed to remove adventure with ID " + ID + " from adventures_listed or adventures_active");
            }

            if (State != AState.Listed)
            {
                APIBase.ClientCollection.SendMessage("adventure:remove", JSSerializer.Serialize(new Dictionary<string, dynamic>()
                {
                    { "id", ID }
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
                    MinHour = Template.ExpireMin;

                if (MinHour > MaxHour)
                    MaxHour = MinHour + 1;
                
                DateTime ThisHour = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0, DateTimeKind.Utc).AddHours(Utils.GetRandom(1, 4) * 0.25f);


                var trigger_times = Actions.FindAll(x => typeof(trigger_time) == x.GetType()).Cast<trigger_time>()?.Min(x => x.TriggerTime);
                if(trigger_times != null)
                {
                    float RandomHour = Utils.GetRandom((int)Math.Round(MinHour), (int)Math.Round(MaxHour));
                    DateTime Exp = ((DateTime)trigger_times).AddHours(RandomHour);

                    PullAt = Exp.AddHours(-MinHour);
                    ExpireAt = Exp;
                }
                else
                {
                    if(MaxHour < 2)
                    {
                        double RandomHour = Utils.GetRandom(MinHour, MaxHour);
                        DateTime Exp = ThisHour.AddHours(RandomHour);
                        PullAt = Exp.AddHours(-MinHour);
                        ExpireAt = Exp;
                    }
                    else
                    {
                        float RandomHour = Utils.GetRandom((int)Math.Ceiling(MinHour), (int)Math.Round(MaxHour));
                        DateTime Exp = ThisHour.AddHours(RandomHour);
                        PullAt = Exp.AddHours(-MinHour);
                        ExpireAt = Exp;
                    }
                }

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
            if(IsMonitored != State)
            {
                IsMonitored = State;
                GoogleAnalyticscs.TrackEvent("Adventure", Template.FileName, State ? "Paused" : "Unpaused");
                ScheduleManifestsBroadcast = true;
                ScheduleManifestsStateBroadcast = true;
            }
        }
        
        public void CheckMonitorState(bool CanEnable)
        {
            var test = TemporalLiveLast;
            var test1 = TemporalNewBuffer;
            if (LastLocationGeo != null && TemporalLiveLast != null && TemporalNewBuffer != null && State == AState.Active)
            {
                GeoLoc PreviousLocation = new GeoLoc(TemporalNewBuffer.PLANE_LOCATION.Lon, TemporalNewBuffer.PLANE_LOCATION.Lat);

                double DistanceToLastLocation = Utils.MapCalcDist(PreviousLocation, LastLocationGeo, Utils.DistanceUnit.NauticalMiles, true);
                bool IsInRange = DistanceToLastLocation < 10 || IsDev;

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
                
                if (IsInRange && IsMonitored)
                {
                    LastLocationGeo = PreviousLocation;
                }
            }
        }
        
        public void CabinRefresh()
        {
            foreach (cargo_pickup_2 pickup in Actions.FindAll(x => typeof(cargo_pickup_2) == x.GetType()))
            {
                foreach (var manifest in pickup.CargoManifests)
                {
                    foreach (var group in manifest.Groups)
                    {
                        group.CabinRefresh(SimConnection.Aircraft.Cabin);
                        group.Resuming();
                    }
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
                        if (Connectors.SimConnection.Aircraft.DirectoryName.Replace("_", " ").ToLower().Contains(match.Replace("_", " ").ToLower()))
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

            if (PreviousAircraftCompatible != AircraftCompatible)
            {
                ScheduleStateBroadcast = true;
            }
        }
        
        public void ChangedAircraft(AircraftInstance old_aircraft, AircraftInstance new_aircraft)
        {
            foreach (action_base Act in Actions)
            {
                Act.ChangedAircraft(old_aircraft, new_aircraft);
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
                        Audio.AudioFramework.GetSpeech("characters", "brigit/good_day", null, (member, route, msg) =>
                        {
                            Chat.SendFromHandleIdent("brigit", new Message(this)
                            {
                                Content = msg,
                                AudioPath = "characters:" + route,
                                ContentType = Message.MessageType.Audio
                            });

                            Save();
                            ScheduleMemosBroadcast = true;

                            //Memos.EnsureMember("pablo", "Pablo");
                            //Memos.Add(new Chat.Message()
                            //{
                            //    Type = Chat.Message.MessageType.Audio,
                            //    Param = "characters:" + route,
                            //    Member = member,
                            //    Content = msg,
                            //});
                            //Save();
                            //ScheduleMemosBroadcast = true;
                        });
                    }
                }
                else if (Template.CompanyStr.Contains("coyote"))
                {
                    if (StartedActions.Find(x => x.GetType() == typeof(audio_speech_play) || x.GetType() == typeof(audio_effect_play)) == null)
                    {
                        Audio.AudioFramework.GetSpeech("characters", "pablo/deliver_some_not_so_good", null, (member, route, msg) =>
                        {
                            Chat.SendFromHandleIdent("pablo", new Message(this)
                            {
                                Content = msg,
                                AudioPath = "characters:" + route,
                                ContentType = Message.MessageType.Audio
                            });

                            Save();
                            ScheduleMemosBroadcast = true;

                            //Memos.EnsureMember("pablo", "Pablo");
                            //Memos.Add(new Chat.Message()
                            //{
                            //    Type = Chat.Message.MessageType.Audio,
                            //    Param = "characters:" + route,
                            //    Member = member,
                            //    Content = msg,
                            //});
                            //Save();
                            //ScheduleMemosBroadcast = true;
                        });
                    }
                }

            }
        }

        public void CheckUpcomingContracts()
        {
            if (NextCheckedForUpcomingContracts == null || TotalActiveTimeTimer.Elapsed.TotalMinutes < 5)
                return;

            if (TotalActiveTimeTimer.Elapsed.TotalMinutes - NextCheckedForUpcomingContracts > 0)
            {
                NextCheckedForUpcomingContracts = (int)Math.Round(TotalActiveTimeTimer.Elapsed.TotalMinutes + 5);
            }
            else
            {
                return;
            }

            List<Adventure> ArrivalContracts = new List<Adventure>(); 
            var last_sit = Situations.Last();
            if(last_sit.Airport != null)
            {
                GetAllLive((x) =>
                {
                    if (x.State != AState.Listed)
                        return;

                    var first_sit = x.Situations.First();
                    if (first_sit.Airport != null)
                    {
                        if (last_sit.Airport == first_sit.Airport)
                        {
                            if(x.IsStillValid())
                                ArrivalContracts.Add(x);
                        }
                    }
                });
            }

            if(ArrivalContracts.Count > 0)
            {
                Adventure target = ArrivalContracts.First();
                
                Audio.AudioFramework.GetSpeech("characters", "brigit/load", null, (member, route, msg) =>
                {
                    Chat.SendFromHandleIdent("brigit", new Message(this)
                    {
                        Content = msg,
                        AudioPath = "characters:" + route,
                        ContentType = Message.MessageType.Call,
                        ContractTopicIDs = new List<long>() { target.ID },
                    });

                    ScheduleMemosBroadcast = true;
                    Save();
                });

                NextCheckedForUpcomingContracts = null;
                /*
                NotificationService.Add(new Notification()
                {
                    Title = "New Contracts available at your destination.",
                    Message = "",
                    Type = NotificationType.DestinationContracts,
                    AppName = "p42_contrax",
                    CanOpen = true,
                    IsTransponder = true,
                    LaunchArgument = new Dictionary<string, dynamic>()
                    {
                        { "id", target.ID }
                    }
                });
                */
            }

            //CheckedForUpcomingContracts
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
        
        public double GetLoadedPayloads()
        {
            double cumulative = 0;
            if(_IsMonitored && _State == AState.Active)
            {
                foreach (cargo_pickup_2 pickup in Actions.FindAll(x => x.GetType() == typeof(cargo_pickup_2)))
                {
                    foreach (var manifest in pickup.CargoManifests)
                    {
                        cumulative += manifest.GetLoadedWeight();
                    }
                }

                foreach (pax_pickup_2 pickup in Actions.FindAll(x => x.GetType() == typeof(pax_pickup_2)))
                {
                    foreach (var manifest in pickup.PAXManifests)
                    {
                        cumulative += manifest.GetLoadedWeight();
                    }
                }
            }
            return cumulative;
        }

        public double GetTotalWeight()
        {
            double cumulative = 0;
            foreach (cargo_pickup_2 pickup in Actions.FindAll(x => x.GetType() == typeof(cargo_pickup_2)))
            {
                foreach (var manifest in pickup.CargoManifests)
                {
                    cumulative += manifest.TotalQualtity * manifest.Definition.WeightKG;
                }
            }
            return cumulative;
        }

        public int GetRequiredPods()
        {
            Dictionary<Situation, List<CargoGroup>> Pickups = new Dictionary<Situation, List<CargoGroup>>();
            Dictionary<Situation, List<CargoGroup>> Dropoffs = new Dictionary<Situation, List<CargoGroup>>();

            foreach (var situation in Situations)
            {
                var pickup_groups = new List<CargoGroup>();
                var dropoff_groups = new List<CargoGroup>();

                Pickups.Add(situation, pickup_groups);
                Dropoffs.Add(situation, dropoff_groups);
            }

            int max = 0;
            int cumulative = 0;
            foreach (var situation in Situations)
            {
                var pickup_groups = Pickups[situation];
                var dropoff_groups = Dropoffs[situation];

                foreach (cargo_pickup_2 pickup in situation.Actions.FindAll(x => x.GetType() == typeof(cargo_pickup_2)))
                {
                    foreach(var manifest in pickup.CargoManifests)
                    {
                        pickup_groups.AddRange(manifest.Groups.Where(x => x.CountPercent > 0));
                        foreach(var dropoffpair in manifest.DropoffPoints)
                        {
                            var action = Actions.Find(x => x.UID == dropoffpair.Key);
                            var group = manifest.Groups.Find(x => manifest.Destinations[x.DestinationIndex].Action == action);
                            if(group != null ? group.CountPercent > 0 : false)
                            {
                                Dropoffs[action.Situation].Add(group);
                            }
                        }
                    }
                }

                cumulative -= dropoff_groups.Count;
                cumulative += pickup_groups.Count;

                max = Math.Max(max, cumulative);
            }
            
            return max;
        }

        public bool PayQuotedInvoice(Invoice.MOMENT moment)
        {
            if (InvoiceQuote != null)
            {
                foreach (Invoice invoice in InvoiceQuote)
                {
                    if(invoice.MomentCondition == moment || invoice.MomentCondition == Invoice.MOMENT.NONE)
                        invoice.Pay();

                    try
                    {
                        switch (invoice.Title)
                        {
                            case "%userrelocation%":
                                {
                                    GoogleAnalyticscs.TrackEvent("Costs", "Relocation Costs", Template.FileName, Convert.ToInt32(Math.Round(invoice.Balance)));
                                    GoogleAnalyticscs.TrackEvent("Costs", "Relocation Distance", Template.FileName, Convert.ToInt32(Math.Round(invoice.Fees.Find(x => x.Code == "relocation").Params["distance"])));
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
                            if (InvoiceQuote == null)
                                StagedInvoices.CreateInitialInvoice(this);

                            PayQuotedInvoice(moment);
                            break;
                        }
                    case Invoice.MOMENT.SUCCEED:
                    case Invoice.MOMENT.FAIL:
                    case Invoice.MOMENT.END:
                        {
                            if (InvoiceQuote == null)
                                StagedInvoices.CreateActiveInvoice(this);

                            PayQuotedInvoice(moment);
                            break;
                        }
                }

                switch (moment)
                {
                    case Invoice.MOMENT.SUCCEED:
                    case Invoice.MOMENT.FAIL:
                    case Invoice.MOMENT.END:
                        {
                            List<Invoice> invoiceCollection = Invoices.GetAllFromID(ID);
                            foreach (var invoice in invoiceCollection)
                            {
                                invoice.CheckRefund(moment);
                            }

                            break;
                        }
                }


            }
        }
        
        public void ProcessBroadcasts()
        {
            if (ScheduleStateBroadcast)
            {
                BroadcastState();
            }

            if (ScheduleInteractionBroadcast)
            {
                BroadcastInteractions();
            }

            if (SchedulePathBroadcast)
            {
                BroadcastPath();
            }

            if (ScheduleLimitsBroadcast)
            {
                BroadcastLimits();
            }

            if (ScheduleMemosBroadcast)
            {
                BroadcastMemos();
            }

            if (ScheduleInvoiceBroadcast)
            {
                BroadcastInvoices();
            }

            if (ScheduleManifestsBroadcast)
            {
                BroadcastManifests();
            }

            if (ScheduleManifestsStateBroadcast)
            {
                BroadcastManifestsState();
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
            float RewardForValue = TotalValue * 0.002f;
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

        public void GenerateDurationRange()
        {
            int dist_km = (int)(Math.Round(DistanceNM * 1.852));
            int minSpeed = int.MaxValue;
            int maxSpeed = int.MinValue;
            ushort[] speeds = new ushort[] {
                259, // Heli
                302, // Prop
                537, // Turbo
                835, // Jet
                902, // Narrow
                902 // Wide
            };

            int i = 0;
            foreach (var recommend in RecommendedAircraft)
            {
                if(recommend > 10)
                {
                    if (minSpeed > speeds[i])
                        minSpeed = speeds[i];

                    if (maxSpeed < speeds[i])
                        maxSpeed = speeds[i];
                }
                i++;
            }

            DurationRange = new ushort[] { (ushort)Math.Round((((float)dist_km / maxSpeed) + 0.25) * 60), (ushort)Math.Round((((float)dist_km / minSpeed) + 0.25) * 60) };
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

                    #region Airports specific
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
                    #endregion

                    #region Cargo Pickups/Dropoffs
                    List<action_base> Pickups = Sit.Actions.FindAll(x => x.GetType() == typeof(cargo_pickup) || x.GetType() == typeof(cargo_pickup_2)).ToList();
                    Pickups.AddRange(Sit.ChildActions.FindAll(x => x.GetType() == typeof(cargo_pickup) || x.GetType() == typeof(cargo_pickup_2)).ToList());
                    Pickups.AddRange(Sit.ChildActions.FindAll(x => x.GetType() == typeof(pax_pickup_2) || x.GetType() == typeof(pax_pickup_2)).ToList());
                    if (Pickups.Count > 0)
                    {
                        foreach (cargo_pickup_2 Pickup in Pickups.Where(x => x.GetType() == typeof(cargo_pickup_2)))
                        {
                            foreach (string Tag in Tags)
                            {
                                int i = 0;
                                foreach(var manifest in Pickup.CargoManifests)
                                {
                                    Contruct = Contruct.Replace("%" + Tag + "_[" + Pickup.UID + "]_ITEM_" + i + "%", manifest.Definition.Name);
                                    i++;
                                }
                            }
                        }

                        foreach (cargo_pickup Pickup in Pickups.Where(x => x.GetType() == typeof(cargo_pickup)))
                        {
                            foreach (string Tag in Tags)
                            {
                                Contruct = Contruct.Replace("%" + Tag + "_[" + Pickup.UID + "]_ITEM%", Pickup.Cargo.Name);
                            }
                        }

                        foreach (pax_pickup_2 Pickup in Pickups.Where(x => x.GetType() == typeof(pax_pickup_2)))
                        {
                            foreach (string Tag in Tags)
                            {
                                int i = 0;
                                foreach (var manifest in Pickup.PAXManifests)
                                {
                                    Contruct = Contruct.Replace("%" + Tag + "_[" + Pickup.UID + "]_ITEM_" + i + "_PERCENT%", Math.Round(manifest.TotalPercent) + "%");
                                    i++;
                                }
                            }
                        }

                        foreach (cargo_pickup_2 Pickup in Pickups.Where(x => x.GetType() == typeof(cargo_pickup_2)))
                        {
                            foreach (string Tag in Tags)
                            {
                                int i = 0;
                                foreach (var manifest in Pickup.CargoManifests)
                                {
                                    Contruct = Contruct.Replace("%" + Tag + "_[" + Pickup.UID + "]_ITEM_" + i + "_PERCENT%", Math.Round(manifest.TotalPercent) + "%");
                                    i++;
                                }
                            }
                        }
                    }
                    #endregion
                }
                
                #region Airline
                if (OperatedFor != null)
                {
                    Contruct = Contruct.Replace("%AIRLINE%", OperatedFor[1]);
                    Contruct = Contruct.Replace("%ACF_TYPE%", OperatedFor[4] != null ? OperatedFor[4] : "unknown");
                    
                    /*
                    (string)NR.Parameters["flight"],
                    (string)NR.Parameters["airline"],
                    (string)NR.Parameters["airline_code"],
                    (string)NR.Parameters["airline_code_iata"],
                    (string)NR.Parameters["model_code"],
                    */

                }
                #endregion

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
            RouteString = "";
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
            }
            
            List<string> sits = new List<string>();
            if (Situations.Count < 3)
            {
                foreach (Situation Sit in Situations)
                {

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
                                sits.Add("•"); //Template.SituationLabels[Sit.Index]
                            }
                            else
                            {
                                sits.Add(Math.Round(Sit.Location.Lat, 3) + ", " + Math.Round(Sit.Location.Lon, 3));
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
                        sits.Add(Math.Round(First.Location.Lat, 3) + ", " + Math.Round(First.Location.Lon, 3));
                    }
                }

                sits.Add("+" + (Situations.Count - 2));

                if (Last.ICAO != string.Empty)
                {
                    sits.Add(Last.ICAO);
                }
                else
                {
                    if (Template.SituationLabels[Last.Index] != null)
                    {
                        sits.Add("•"); //Template.SituationLabels[Last.Index]
                    }
                    else
                    {
                        sits.Add(Math.Round(Last.Location.Lat, 3) + ", " + Math.Round(Last.Location.Lon, 3));
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
                        { "done", sit.Done },
                        { "visited", sit.Visited },
                        { "range", sit.InRange },
                        { "is_next", (sit.Index == SituationAt || !sit.Adventure.Template.StrictOrder) && sit.Adventure.State == AState.Active },
                        { "count", sit.ChildActions.FindAll(x => x.Completed != null).Count - sit.ChildActions.FindAll(x => x.Completed == true).Count },
                        { "actions", new List<Dictionary<string, dynamic>>() }
                    };

                    foreach (action_base Action in sit.ChildActions)
                    {
                        Dictionary<string, dynamic> Struct = Action.ToListedActions();
                        if (Struct != null)
                        {
                            ns["actions"].Add(Struct);
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
                ret.Add("total_liabilities", liabilitiesTotal);


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
                ret.Add("total_refunds", refundsTotal);


                ret.Add("total_fees", feesTotal);
                ret.Add("total_profits", RewardBux - feesTotal - refundsTotal);
                ret.Add("uncertain_refunds", refundsUncertain);
                ret.Add("uncertain_fees", feesUncertain);
                ret.Add("uncertain_liabilities", liabilitiessUncertain);
                ret.Add("invoices", invoiceCollection.Select(x => x.ToDictionary()));

                return ret;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, dynamic> GenBcastManifests(Dictionary<string, dynamic> fields)
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "aircraft", new List<Dictionary<string, dynamic>>() },
                { "total_weight", 0 },
                { "total_cargo_percent", 0 },
                { "total_pax_percent", 0 },
                { "max_cargo_pods", GetRequiredPods() },
                { "max_pax_seats", 0 },
                { "cargo", new List<Dictionary<string, dynamic>>() },
                { "pax", new List<Dictionary<string, dynamic>>() },
                { "contract", null },
            };

            rs["contract"] = this.Serialize(new Dictionary<string, dynamic>()
            {
                { "name", true },
                { "id", true },
                { "file_name", true },
                { "route", true },
                { "image_url", true },
            });


            List<AircraftInstance> aircraft = new List<AircraftInstance>();

            // Cargo
            foreach (cargo_pickup_2 cargo_pickup in Actions.Where(x => x.GetType() == typeof(cargo_pickup_2)))
            {
                Dictionary<string, dynamic> pickup_rs = new Dictionary<string, dynamic>()
                {
                    { "pickup_id", cargo_pickup.UID },
                    { "dropoff_ids", cargo_pickup.CargoManifests.Select(x => x.Destinations.Select(x1 => x1.Action.UID)).SelectMany(x2 => x2).Distinct() },
                    { "manifests", cargo_pickup.CargoManifests.Select(x => x.Serialize(fields)) }
                };

                rs["total_weight"] += cargo_pickup.CargoManifests.Select(x => x.Definition != null ? (int)x.Definition.WeightKG * (int)x.TotalQualtity : 0).Sum();
                rs["total_cargo_percent"] += cargo_pickup.CargoManifests.Select(x => x.TotalPercent).Sum();
                rs["cargo"].Add(pickup_rs);

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


            if (aircraft.Count > 0)
            {
                rs["aircraft"] = aircraft.Select(x => x.Serialize(null));
            }

            return rs;
        }

        public Dictionary<string, dynamic> GenBcastManifestsState(Dictionary<string, dynamic> fields)
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "cargo", new List<Dictionary<string, dynamic>>() },
                { "pax", new List<Dictionary<string, dynamic>>() },
                { "contract", null },
            };


            rs["contract"] = this.Serialize(new Dictionary<string, dynamic>()
            {
                { "state", true },
                { "is_monitored", true },
                { "last_location_geo", true },
                { "id", true },
            });

            // Cargo
            foreach (cargo_pickup_2 cargo_pickup in Actions.Where(x => x.GetType() == typeof(cargo_pickup_2)))
            {
                Dictionary<string, dynamic> pickup_rs = new Dictionary<string, dynamic>()
                {
                    { "manifests", cargo_pickup.CargoManifests.Select(x => x.SerializeState(fields)) }
                };
                
                rs["cargo"].Add(pickup_rs);
            }
            
            // PAX
            foreach (pax_pickup_2 cargo_pickup in Actions.Where(x => x.GetType() == typeof(pax_pickup_2)))
            {
                Dictionary<string, dynamic> pickup_rs = new Dictionary<string, dynamic>()
                {
                    { "manifests", cargo_pickup.PAXManifests.Select(x => x.SerializeState(fields)) }
                };
                
                rs["pax"].Add(pickup_rs);
            }
            
            return rs;
        }

        public List<Dictionary<string, dynamic>> GenBcastMemosStruct()
        {
            var chats = Chat.GetChatFromContract(ID, int.MaxValue);
            if(chats.Count > 0)
            {
                var chats_structure = chats.Select(x => x.Key.Serialize(new Dictionary<string, dynamic>()
                {
                    { "id", true },
                    { "handles", true },
                    { "read_at_date", true },
                    { "messages", x.Value.Select(x1 => x1.Serialize(null)).ToList() }
                }));

                var result = chats_structure.ToList();
                return result;
            }

            return new List<Dictionary<string, dynamic>>();
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
                                { "contract", new Dictionary<string, dynamic>()
                                    {
                                        { "state", GetDescription(_State) },
                                        { "end_summary", EndSummary },
                                        { "route", Route },
                                    }
                                },
                                { "template", new Dictionary<string, dynamic>()
                                    {
                                        { "name", Template.Name },
                                    }
                                },
                                { "end_card", ConfirmedReward }
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
                { "id", ID },
                { "manifests", GenBcastManifests(null) }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastManifestsState()
        {
            ScheduleManifestsStateBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:manifest_state", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "manifests_state", GenBcastManifestsState(null) }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastState()
        {
            ScheduleStateBroadcast = false;

            Airport nearest = null;
            if (LastLocationGeo != null)
            {
                var nearest_range = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastLocationGeo, 3).FirstOrDefault();
                if (nearest_range.Value != null)
                {
                    nearest = nearest_range.Value;
                }
            };

            Dictionary<string, dynamic> ss = new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "state", GetDescription(_State) },
                { "is_monitored", IsMonitored },
                { "last_location_geo", LastLocationGeo != null ? LastLocationGeo.ToList(6) : null },
                { "last_location_airport", nearest != null ? nearest.Serialize(null) : null },
                { "range_airports", new List<Dictionary<string, dynamic>>() },
                { "aircraft_compatible", AircraftCompatible },
                { "aircraft_used", AircraftUsed }
            };

            if (State == AState.Active && !IsMonitored && LastLocationGeo != null)
            {
                var aptr = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastLocationGeo, 50);
                if (aptr[0].Value != null)
                {
                    foreach (var Apt in aptr)
                    {
                        if (ss["range_airports"].Count < 10)
                        {
                            ss["range_airports"].Add(Apt.Value.Serialize(null));
                        }
                    }
                }
            }
            
            if (EndSummary != string.Empty) { ss.Add("end_summary", EndSummary); }
            if (StartedAt != null) { ss.Add("started_at", ((DateTime)StartedAt).ToString("O")); }
            if (CompletedAt != null) { ss.Add("completed_at", ((DateTime)CompletedAt).ToString("O")); }

            APIBase.ClientCollection.SendMessage("adventure:state", JSSerializer.Serialize(ss), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastLimits()
        {
            ScheduleLimitsBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:limits", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "limits", Limits }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastMemos()
        {
            ScheduleMemosBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:memos", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "memos", GenBcastMemosStruct() }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastInteractions()
        {
            ScheduleInteractionBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:interactions", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "interactions", Interactions }
            }), null, APIBase.ClientType.Skypad);
        }

        public void BroadcastPath()
        {
            SchedulePathBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:path", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "situation_at", SituationAt },
                { "path", GenBcastPathStruct() }
            }), null, APIBase.ClientType.Skypad);
        }
        
        public void BroadcastInvoices()
        {
            ScheduleInvoiceBroadcast = false;
            APIBase.ClientCollection.SendMessage("adventure:invoices", JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "id", ID },
                { "invoices", GenBcastInvoicesStruct() }
            }), null, APIBase.ClientType.Skypad);
        }


       
        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            if (!PersistenceRestored)
            {
                RequestState reply = new RequestState()
                {
                    ReferenceID = Convert.ToInt64(payload_struct["ID"]),
                    Reason = "Not loaded yet"
                };
                Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(reply.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
                return;
            }

            Adventure Adv = null;
            long ID = Convert.ToInt64(payload_struct["id"]);

            lock (AllContracts)
            {
                Adv = AllContracts.Find(x => x.ID == ID);
            }
           
            if (Adv != null)
            {
                RequestState reply = new RequestState()
                {
                    Status = RequestState.STATUS.SUCCESS,
                    ReferenceID = Adv.ID
                };

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
                                            { "routes", routes },
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

                                        if (payload_struct["get_all"])
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

                                        ret.Add("count", routes.Count);
                                        ret.Add("countries", OrderedCountries);

                                        while (routes.Count > Limit)
                                        {
                                            routes.RemoveAt(Utils.GetRandom(routes.Count));
                                        }

                                        reply.Data = ret;
                                        #endregion
                                        break;
                                    }
                            }
                            break;
                        }
                    case "invoices":
                        {
                            reply.Data = Adv.GenBcastInvoicesStruct();
                            break;
                        }
                    case "save":
                        {                            
                            if (Adv.State == AState.Listed)
                            {
                                Adv.State = AState.Saved;
                            }
                            else
                            {
                                reply.Status = RequestState.STATUS.FAILED;
                                reply.Reason = Adv.ID + " no longer listed";
                            }

                            Socket.SendMessage("response", JSSerializer.Serialize(reply.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
                            return;
                        }
                    case "commit":
                        {
                            if (UserData.Get("tier") != "discovery")
                            {
                                if (Adv.InvoiceQuote != null)
                                {
                                    Adv.Start();
                                }
                                else
                                {
                                    reply.Status = RequestState.STATUS.FAILED;
                                    reply.Reason = Adv.ID + " commit quote doesn't match existing quote";
                                }
                            }
                            else
                            {
                                Adv.Start();
                            }

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
                                        reply.Data = Plan.Serialize(null);
                                        break;
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
                            break;
                        }
                    case "cancel":
                        {
                            Adv.Fail("You cancelled this " + Adv.Template.TypeLabel + ".");
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
                            Adv.Start();
                            break;
                        }
                    case "interaction":
                        {
                            if (Adv.IsMonitored)
                            {
                                KeyValuePair<int, action_base> Action = Adv.ActionsIndex.Find(x => x.Key == (int)Convert.ToInt32(payload_struct["link"]));
                                if (Action.Value != null)
                                {
                                    Action.Value.Interact(payload_struct);
                                }
                            }
                            break;
                        }
                }


                Socket.SendMessage("response", JSSerializer.Serialize(reply.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
            }
            else
            {
                RequestState reply = new RequestState()
                {
                    Status = RequestState.STATUS.FAILED,
                    ReferenceID = ID,
                    Reason = "Does not exist"
                };

                Socket.SendMessage("response", JSSerializer.Serialize(reply.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
            }
        }
          


        public bool ExportContract()
        {
            if (MW.IsShuttingDown || !Ready)
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
                { "OperatedFor", OperatedFor },
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
                { "TotalActiveTimeSeconds", TotalActiveTimeSeconds },
                { "EndSummary", EndSummary },
                { "Title", Title },
                { "Description", DescriptionString },
                { "DescriptionLong", DescriptionLongString },
                { "AircraftUsed", AircraftUsed },
                { "Cleanedup", Cleanedup },
                { "Path", GenBcastPathStruct() },
            })
            {
                ns.Add(entry.Key, entry.Value);
            }

            //ns.Add("Memos", Memos.ToDictionary());

            return ns;
        }

        
        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<Adventure> cs = new ClassSerializer<Adventure>(this, fields);
            cs.Generate(typeof(Adventure), fields);
            
            cs.Get("file_name", fields, (f) => Template.FileName);
            cs.Get("title", fields, (f) => Title != null ? Title : Template.Name);
            cs.Get("state", fields, (f) => GetDescription(_State));
            cs.Get("request_status", fields, (f) => "ready");
            cs.Get("last_location_geo", fields, (f) => LastLocationGeo != null ? LastLocationGeo.ToList(6) : null);
            cs.Get("last_location_airport", fields, (f) =>
            {   
                if(LastLocationGeo != null)
                {
                    Airport nearest = null;
                    var nearest_range = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastLocationGeo, 3).FirstOrDefault();
                    if (nearest_range.Value != null)
                    {
                        nearest = nearest_range.Value;
                    }
                    return nearest != null ? nearest.Serialize(f) : null;
                }
                else
                {
                    return null;
                }

            });
            cs.Get("modified_on", fields, (f) => Template.ModifiedOn.ToString("O"));
            cs.Get("requested_at", fields, (f) => RequestedAt != null ? ((DateTime)RequestedAt).ToString("O") : null);
            cs.Get("distance", fields, (f) => Math.Round(DistanceNM * 1.852));
            cs.Get("duration_range", fields, (f) => new float[] { (float)Math.Round((float)DurationRange[0] / 60, 1), (float)Math.Round((float)DurationRange[1] / 60, 1) });
            cs.Get("route_code", fields, (f) => RouteValidated ? RouteCode : "");
            cs.Get("media_link", fields, (f) => GetMediaLinks());
            cs.Get("expire_at", fields, (f) => ExpireAt.ToString("O"));
            cs.Get("completed_at", fields, (f) => CompletedAt != null ? ((DateTime)CompletedAt).ToString("O") : null);
            cs.Get("started_at", fields, (f) => StartedAt != null ? ((DateTime)StartedAt).ToString("O") : null);
            cs.Get("pull_at", fields, (f) => PullAt != null ? ((DateTime)PullAt).ToString("O") : null);
            cs.Get("interactions", fields, (f) => Interactions);
            cs.Get("path", fields, (f) => GenBcastPathStruct());
            cs.Get("manifests", fields, (f) => GenBcastManifests(f));
            cs.Get("manifests_state", fields, (f) => GenBcastManifestsState(f));
            cs.Get("limits", fields, (f) => Limits);
            cs.Get("invoices", fields, (f) => GenBcastInvoicesStruct());
            cs.Get("memos", fields, (f) => GenBcastMemosStruct());
            cs.Get("situations", fields, (f) => Situations.Where(x => x.Visible).Select(x => x.Serialize(f)));
            cs.Get("situation_at", fields, (f) => SituationAt);
            cs.Get("image_url", fields, (f) =>
            {
                Situation LastWithApt = Template.ImageURL.Count > 0 ? null : Situations.FindLast(x => x.Airport != null);
                string SelImageURL = Template.ImageURL.Count > 0 ? Template.ImageURL[ImageIndex] : LastWithApt != null ? "%imagecdn%/airports/" + LastWithApt.Airport.ICAO + ".jpg" : "";
                return SelImageURL.Replace("%imagecdn%", APIBase.CDNImagesHost);
            });

            cs.Get("flight_plans", fields, (f) =>
            {
                List<Dictionary<string, dynamic>> ret = new List<Dictionary<string, dynamic>>();
                ret.AddRange(GetRelatedFlightPlans().Select(x => x.Serialize(f)));
                if(LegsFlightPlans != null)
                {
                    ret.AddRange(LegsFlightPlans.Select(x => x.Serialize(f)));
                }
                ret.AddRange(CustomPlans.Select(x => x.Serialize(f)));
                return ret;
            });


            if (UserData.Get("tier") != "discovery")
            {
                cs.Get("reward_xp", fields, (f) => RewardXP);
                cs.Get("reward_karma", fields, (f) => RewardKarma);
                cs.Get("reward_bux", fields, (f) => RewardBux);
                cs.Get("reward_reliability", fields, (f) => Template.RatingGainSucceed);
            }

            if (State == AState.Active && !IsMonitored && LastLocationGeo != null)
            {
                cs.Get("range_airports", fields, (f) =>
                {
                    var results = new List<Dictionary<string, dynamic>>();
                    var aptr = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastLocationGeo, 50);
                    if (aptr[0].Value != null)
                    {
                        foreach (var Apt in aptr)
                        {
                            if (results.Count < 10)
                            {
                                results.Add(Apt.Value.Serialize(null));
                            }
                        }
                    }
                    return results;
                });
            }
            
            var result = cs.Get();
            return result;
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
