using LiteDB;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.WeatherModel;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.PostProcessing;
using TSP_Transponder.Models.Transactor;
using TSP_Transponder.Models.Contractors;
using static TSP_Transponder.App;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.Adventures.AdventureTemplate;
using static TSP_Transponder.Attributes.EnumAttr;
using TSP_Transponder.Models.Messaging;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.Dev;

namespace TSP_Transponder.Models.Adventures
{
    public class AdventuresBase
    {
        public static int MaxContracts = 20000;
        public static int TotalTemplatePoints = 0;
        public static List<float> AircraftBias = new List<float>() { 100, 100, 100, 100, 100, 100 };
        public static float DistanceBias = 0;

        public static int RouteVersion = 19;
        public static int AdventureVersion = 3;
        public static List<Adventure> AllContracts = new List<Adventure>();
        public static int TemplatesCount = 0;
        private static Task ValidityThread = null;
        public static List<AdventureTemplate> Templates = new List<AdventureTemplate>();
        public static List<string> Sources = new List<string>();

        public static TemporalData TemporalLiveLast = null;
        public static TemporalData TemporalLast = null;
        public static TemporalData TemporalNewBuffer = null;

        internal static bool RestoreStarted = false;
        internal static bool RestoreCompleted = false;
        internal static bool PersistenceRestored = false;
        internal static Dictionary<string, List<long>> ContractsSummary = new Dictionary<string, List<long>>();
                
        private static List<Type> InstantTypes = new List<Type>()
        {
            typeof(cargo_pickup_2),
            typeof(cargo_pickup),
            typeof(trigger_g_start),
            typeof(trigger_gs_start),
            typeof(trigger_ias_start),
            typeof(flight_timer_start),
            typeof(trigger_time),
            typeof(trigger_distance),
        };

        internal static bool SchedulePayloadUpdate = false;

        public static action_base CreateAction(Adventure Adv, AdventureTemplate AdvTemplate, Situation Sit, int ActionID, action_base Parent)
        {
            action_base ActionObj = null;
            
            Dictionary<string, dynamic> Action = ((ArrayList)AdvTemplate.InitialStructure["Actions"]).Cast<Dictionary<string, dynamic>>().ToList().Find(x => x["UID"] == ActionID);
            int UID = Convert.ToInt32(Action["UID"]);

            Dictionary<string, dynamic> Params = new Dictionary<string, dynamic>(Action["Params"]);
            if(Adv != null)
            {
                if (Adv.RestoredActionsPersistence.ContainsKey(Convert.ToString(Action["UID"])))
                {
                    foreach (var PersistedEntries in Adv.RestoredActionsPersistence[Convert.ToString(Action["UID"])])
                    {
                        if (Params.ContainsKey(PersistedEntries.Key))
                        {
                            Params[PersistedEntries.Key] = PersistedEntries.Value;
                        }
                        else
                        {
                            Params.Add(PersistedEntries.Key, PersistedEntries.Value);
                        }
                    }
                }
            }
            
            switch (Action["Action"])
            {
                case "adventure_milestone":
                    {
                        ActionObj = new Actions.adventure_milestone(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "adventure_fail":
                    {
                        ActionObj = new Actions.adventure_fail(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "adventure_bonus":
                    {
                        ActionObj = new Actions.adventure_bonus(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "scene_load":
                    {
                        ActionObj = new Actions.scene_load(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "scene_unload":
                    {
                        ActionObj = new Actions.scene_unload(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "cargo_pickup":
                    {
                        ActionObj = new Actions.cargo_pickup(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "cargo_dropoff":
                    {
                        ActionObj = new Actions.cargo_dropoff(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "cargo_pickup_2":
                    {
                        ActionObj = new Actions.cargo_pickup_2(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "cargo_dropoff_2":
                    {
                        ActionObj = new Actions.cargo_dropoff_2(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "pax_pickup_2":
                    {
                        ActionObj = new Actions.pax_pickup_2(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "pax_dropoff_2":
                    {
                        ActionObj = new Actions.pax_dropoff_2(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "audio_speech_play":
                    {
                        ActionObj = new Actions.audio_speech_play(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "audio_effect_play":
                    {
                        ActionObj = new Actions.audio_effect_play(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_alt_start":
                    {
                        ActionObj = new Actions.trigger_alt_start(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_alt_end":
                    {
                        ActionObj = new Actions.trigger_alt_end(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "flight_timer_start":
                    {
                        ActionObj = new Actions.flight_timer_start(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "flight_timer_end":
                    {
                        ActionObj = new Actions.flight_timer_end(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_g_start":
                    {
                        ActionObj = new Actions.trigger_g_start(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_g_end":
                    {
                        ActionObj = new Actions.trigger_g_end(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_ias_start":
                    {
                        ActionObj = new Actions.trigger_ias_start(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_ias_end":
                    {
                        ActionObj = new Actions.trigger_ias_end(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_gs_start":
                    {
                        ActionObj = new Actions.trigger_gs_start(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_gs_end":
                    {
                        ActionObj = new Actions.trigger_gs_end(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_engine_stop":
                    {
                        ActionObj = new Actions.trigger_engine_stop(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_distance":
                    {
                        ActionObj = new Actions.trigger_distance(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_time":
                    {
                        ActionObj = new Actions.trigger_time(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                case "trigger_button":
                    {
                        ActionObj = new Actions.trigger_button(Adv, AdvTemplate, Sit, UID, Params, Parent);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown Situation Action: " + Action["Action"]);
                        return null;
                    }
            }

            return ActionObj;
        }
        
        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure, int Depth = 0)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "query-from-template":
                    {
                        DateTime Timeout = DateTime.UtcNow.AddSeconds(20);
                        Thread WaitThread = new Thread(() =>
                        {
                            Dictionary<string, dynamic> Dict = new Dictionary<string, dynamic>()
                            {
                                { "templates", new List<Dictionary<string, dynamic>>() },
                                { "contracts", new List<Dictionary<string, dynamic>>() },
                            };

                            bool Detailed = false;
                            if (payload_struct.ContainsKey("detailed"))
                            {
                                Detailed = (bool)payload_struct["detailed"];
                            }

                            AdventureTemplate Template = null;
                            Adventure FoundAdv = null;
                            while (!MW.IsShuttingDown && Timeout > DateTime.UtcNow && Socket.IsConnected)
                            {
                                lock (Templates)
                                {
                                    Template = Templates.Find(x => x.Activated && x.FileName == ((string)payload_struct["file"]));
                                }
                                if (Template != null)
                                {
                                    if(Template.ActiveAdventures.Count > 0)
                                    {
                                        break;
                                    }
                                }
                                Thread.Sleep(1000);
                            }
                            if(Template != null)
                            {
                                lock (Template.ActiveAdventures)
                                {
                                    if (Template.ActiveAdventures.Count > 0)
                                    {
                                        FoundAdv = Template.ActiveAdventures[Utils.GetRandom(Template.ActiveAdventures.Count)];
                                        FoundAdv.GenerateTopographyVariance();
                                    }
                                }
                            }

                            if (Socket.IsConnected)
                            {
                                if (FoundAdv != null)
                                {
                                    Dict["contracts"].Add(FoundAdv.Serialize(null));
                                    Dict["templates"].Add(FoundAdv.Template.Serialize(null));
                                }

                                Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(Dict), (Dictionary<string, dynamic>)structure["meta"]);
                            }
                        });
                        WaitThread.IsBackground = true;
                        WaitThread.CurrentCulture = CultureInfo.CurrentCulture;
                        WaitThread.Start();
                        break;
                    }
                case "query-from-id":
                    {
                        Dictionary<string, dynamic> Dict = new Dictionary<string, dynamic>()
                        {
                            { "contract", null },
                            { "template", null },
                        };
                        
                        Adventure FoundAdv = null;
                        lock (AllContracts)
                        {
                            FoundAdv = AllContracts.Find(x => x.ID == ((long)payload_struct["id"]));
                        }

                        if (FoundAdv != null)
                        {
                            FoundAdv.GenerateTopographyVariance();
                            Dict["contract"] = FoundAdv.Serialize((Dictionary<string, dynamic>)payload_struct["fields"]["contract"]);
                            Dict["template"] = FoundAdv.Template.Serialize((Dictionary<string, dynamic>)payload_struct["fields"]["template"]);
                        }
                        
                        Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(Dict), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "query-from-filters":
                    {
                        int Count = 0;

                        bool ShowFeatured = UserData.Get("tier") != "prospect";
                        Dictionary<Airport, int> Airports = new Dictionary<Airport, int>();
                        List<Adventure> Results = null;
                        List<AdventureTemplate> FoundTemplates = new List<AdventureTemplate>();
                        Dictionary<string, dynamic> Dict = new Dictionary<string, dynamic>()
                        {
                            { "count", 0 },
                            { "aircraft", (string)null },
                            { "airports", new List<Dictionary<string, dynamic>>() },
                            { "templates", new List<Dictionary<string, dynamic>>() },
                            { "contracts", new List<Dictionary<string, dynamic>>() },
                            { "featured", new List<Dictionary<string, dynamic>>() },
                        };

                        try
                        {

                            #region Get Dice
                            bool Diced = false;
                            if (payload_struct.ContainsKey("diced"))
                            {
                                Diced = Convert.ToBoolean(payload_struct["diced"]);
                            }
                            #endregion

                            #region Get RequestedOn
                            DateTime? RequestedOn = null;
                            if (payload_struct.ContainsKey("requestedOn"))
                            {
                                RequestedOn = DateTime.Parse((string)payload_struct["requestedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                            }
                            #endregion

                            #region Get Type
                            string Type = "any";
                            if (payload_struct.ContainsKey("type"))
                            {
                                Type = payload_struct["type"];
                            }
                            #endregion

                            #region Get Companies
                            List<Company> CompanyExcludeIDs = new List<Company>();
                            if (payload_struct.ContainsKey("companies"))
                            {
                                foreach(string co in payload_struct["companies"])
                                {
                                    Company nc = Companies.List.Find(x => x.Code == co);
                                    if(nc != null)
                                    {
                                        CompanyExcludeIDs.Add(nc);
                                    }
                                }
                            }
                            #endregion

                            #region Get Aircraft Types
                            List<int> AircraftTypeExcludeIDs = new List<int>();
                            if (payload_struct.ContainsKey("types"))
                            {
                                AircraftTypeExcludeIDs = ((ArrayList)payload_struct["types"]).Cast<int>().ToList();
                            }
                            #endregion

                            #region Get Detailed
                            bool Detailed = false;
                            if (payload_struct.ContainsKey("detailed"))
                            {
                                Detailed = (bool)payload_struct["detailed"];
                            }
                            #endregion
                            
                            #region Get Priority States
                            List<Adventure.AState> PriorityStates = new List<Adventure.AState>();
                            if (payload_struct.ContainsKey("priorityState"))
                            {
                                string[] StatesStr = ((string)payload_struct["priorityState"]).Split(',');
                                foreach (string StateStr in StatesStr)
                                {
                                    PriorityStates.Add((Adventure.AState)GetEnum(typeof(Adventure.AState), StateStr));
                                }
                            }
                            #endregion

                            #region Get States
                            List<Adventure.AState> States = new List<Adventure.AState>();
                            if (payload_struct.ContainsKey("state"))
                            {
                                string[] StatesStr = ((string)payload_struct["state"]).Split(',');
                                foreach (string StateStr in StatesStr)
                                {
                                    Adventure.AState IncludeState = (Adventure.AState)GetEnum(typeof(Adventure.AState), StateStr);
                                    if(!States.Contains(IncludeState))
                                    {
                                        States.Add(IncludeState);
                                    }
                                }
                            }
                            #endregion

                            #region Get Ranges
                            float RangeMin = 0;
                            float RangeMax = int.MaxValue;
                            if (payload_struct.ContainsKey("range"))
                            {
                                RangeMin = Convert.ToSingle(payload_struct["range"][0]);
                                RangeMax = Convert.ToSingle(payload_struct["range"][1]);
                            }
                            #endregion

                            #region Get Legs
                            int LegsMin = 0;
                            int LegsMax = 51;
                            if (payload_struct.ContainsKey("legsCount"))
                            {
                                LegsMin = Convert.ToInt32(payload_struct["legsCount"][0] + 1);
                                LegsMax = Convert.ToInt32(payload_struct["legsCount"][1] + 1);
                            }
                            #endregion

                            #region Filter Custom Contracts
                            bool OnlyCustomContracts = false;
                            if (payload_struct.ContainsKey("onlyCustomContracts"))
                            {
                                OnlyCustomContracts = payload_struct["onlyCustomContracts"] != null ? payload_struct["onlyCustomContracts"] : false;
                            }
                            #endregion

                            #region Get Runways
                            bool? RequiresLight = null;
                            bool? RequiresILS = null;
                            int? RunwayCountMin = null;
                            int? RunwayCountMax = null;
                            float? RunwaysLengthMin = null;
                            float? RunwaysLengthMax = null;
                            string RunwaysType = null;

                            if (payload_struct.ContainsKey("requiresLight"))
                            {
                                RequiresLight = Convert.ToBoolean(payload_struct["requiresLight"]);
                            }
                            if (payload_struct.ContainsKey("requiresILS"))
                            {
                                RequiresILS = Convert.ToBoolean(payload_struct["requiresILS"]);
                            }
                            if (payload_struct.ContainsKey("rwyCount"))
                            {
                                RunwayCountMin = Convert.ToInt32(payload_struct["rwyCount"][0]);
                                RunwayCountMax = Convert.ToInt32(payload_struct["rwyCount"][1]);
                            }
                            if (payload_struct.ContainsKey("runways"))
                            {
                                RunwaysLengthMin = Convert.ToSingle(payload_struct["runways"][0]);
                                RunwaysLengthMax = Convert.ToSingle(payload_struct["runways"][1]);
                            }
                            if (payload_struct.ContainsKey("rwySurface"))
                            {
                                RunwaysType = payload_struct["rwySurface"];
                            }

                            #endregion
                        
                            #region Get Search Query
                            List<List<string>> ICAOGroup = new List<List<string>>();
                            List<string> ShareCodes = new List<string>();
                            List<string> Queries = new List<string>();
                            if (payload_struct.ContainsKey("query"))
                            {
                                if(((string)payload_struct["query"]).Trim().Length > 2)
                                {
                                    try
                                    {
                                        string[] queries = ((string)payload_struct["query"]).Split(",;:&+\t ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string Query in queries)
                                        {
                                            string QueryUP = Query.ToUpper();

                                            // Match ShareCodes
                                            if (Regex.IsMatch(QueryUP, @"[0-9A-Fa-f]{5}"))
                                            {
                                                ShareCodes.Add(QueryUP);
                                            }

                                            // Match ICAOs
                                            if (Regex.IsMatch(Query, @"^(-?[0-9A-Za-z]{3,4}-?){1,2}$"))
                                            {
                                                // ICAOs
                                                List<string> GroupSpl = Regex.Split(QueryUP.Replace(" ", ""), "(-)").ToList();

                                                int i = 0;
                                                while(i < GroupSpl.Count)
                                                {
                                                    string icao = GroupSpl[i];
                                                    if (icao.Length > 2)
                                                    {
                                                        try
                                                        {
                                                            SimLibrary.SimList[0].AirportsLib.GetByICAO(icao);
                                                        }
                                                        catch
                                                        {
                                                            GroupSpl.Remove(icao);
                                                            i--;
                                                        }
                                                    }
                                                    i++;
                                                }

                                                GroupSpl.RemoveAll(x => string.IsNullOrEmpty(x));
                                                if(GroupSpl.Count > 0)
                                                {
                                                    ICAOGroup.Add(GroupSpl);
                                                }
                                                Queries.AddRange(QueryUP.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList());
                                                continue;
                                            }

                                            if (Query.Length > 2)
                                            {
                                                Queries.Add(QueryUP);
                                            }
                                        }
                                        Queries.Add((payload_struct["query"]).ToUpper());
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Failed to read Search Query " + payload_struct["query"] + " - " + ex.Message);
                                    }
                                }
                            }
                            #endregion
                        
                            #region Get Limit
                            int Limit = 30;
                            if (payload_struct.ContainsKey("limit"))
                            {
                                Limit = Convert.ToInt32(payload_struct["limit"]);
                            }
                            #endregion

                            #region Get Limit
                            int Offset = 0;
                            if (payload_struct.ContainsKey("offset"))
                            {
                                Offset = Convert.ToInt32(payload_struct["offset"]);
                            }
                            #endregion

                            #region Get Geo Bounds
                            GeoLoc NW = null;
                            GeoLoc SE = null;
                            if (payload_struct.ContainsKey("bounds"))
                            {
                                if(payload_struct["bounds"] != null)
                                {
                                    NW = new GeoLoc(Convert.ToDouble(payload_struct["bounds"]["nw"][0]), Convert.ToDouble(payload_struct["bounds"]["nw"][1]));
                                    SE = new GeoLoc(Convert.ToDouble(payload_struct["bounds"]["se"][0]), Convert.ToDouble(payload_struct["bounds"]["se"][1]));
                                }
                            }
                            #endregion

                            #region Get Sorting
                            string Sorting = "relevance";
                            bool SortingAsc = true;
                            if (payload_struct.ContainsKey("sort"))
                            {
                                Sorting = payload_struct["sort"];
                                switch (payload_struct["sort"])
                                {
                                    case "aircraft":
                                        {
                                            if (!payload_struct.ContainsKey("location"))
                                            {
                                                Sorting = "relevance";
                                            }
                                            else if (payload_struct["location"] == null)
                                            {
                                                Sorting = "relevance";
                                            }
                                            break;
                                        }
                                }

                                if (payload_struct.ContainsKey("sortAsc"))
                                {
                                    SortingAsc = payload_struct["sortAsc"];
                                }
                            }
                            #endregion
                        
                            lock (AllContracts)
                            {
                                if(Progress.Progress.XP.Balance == 0 && UserData.Get("tier") == "endeavour")
                                {
                                    AdventureTemplate t = Templates.Find(x => x.FileName.StartsWith("IntroTrainingFlight"));
                                    if(t != null)
                                    {
                                        Results = t.ActiveAdventures;
                                        ShowFeatured = false;
                                    }
                                    else
                                    {
                                        Results = new List<Adventure>(AllContracts);
                                    }
                                }
                                else
                                {
                                    Results = new List<Adventure>(AllContracts);
                                }

                                //Results = Results.OrderBy(x => Utils.GetRandom() * Results.Count).ToList();
                            }

                            #region Filter Custom Contracts
                            if(OnlyCustomContracts)
                            {
                                Results = Results.FindAll(x => x.Template.IsCustom);
                            }
                            #endregion

                            #region Filter Companies
                            if (CompanyExcludeIDs.Count > 0)
                            {
                                Results = Results.FindAll(x =>
                                {
                                    if(x.Template.Company.Count > 0)
                                    {
                                        foreach (var co in x.Template.Company)
                                        {
                                            if (CompanyExcludeIDs.Contains(co))
                                            {
                                                return false;
                                            }
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                });
                            }
                            #endregion

                            #region Filter Karma
                            if (UserData.Get("illicit") == "0")
                            {
                                Results = Results.FindAll(x => x.RewardKarma >= 0);
                            }
                            #endregion

                            #region Filter RequestedOn
                            if (RequestedOn != null)
                            {
                                Results = Results.Where(x =>
                                {
                                    if(x.RequestedAt != null) {
                                        if(x.RequestedAt < RequestedOn)
                                        {
                                            return true;
                                        }
                                    }
                                    return false;
                                }).ToList();
                            }
                            #endregion

                            #region Filter Type
                            if(Type != "any")
                            {
                                Results = Results.Where(x => x.Template.Type.Contains(Type)).ToList();
                            }
                            #endregion

                            #region Filter Aircraft Types
                            List<int> InlcudedTypes = null;
                            if (AircraftTypeExcludeIDs.Count > 0)
                            {
                                int[] AircraftTypes = new int[] { 0, 1, 2, 3, 4, 5 };
                                InlcudedTypes = AircraftTypes.Except(AircraftTypeExcludeIDs).ToList();
                                Results = Results.Where(x =>
                                {
                                    bool Include = false;
                                    foreach(int id in InlcudedTypes)
                                    {
                                        if(x.RecommendedAircraft[id] > (10f / ((1 + InlcudedTypes.Count) * 0.5)) && x.RecommendedAircraft[id] > 10)
                                        {
                                            Include = true;
                                        }
                                    }
                                    return Include;
                                }).ToList();
                            }
                            #endregion
                        
                            #region Filter States
                            if (States.Count > 0)
                            {
                                Results = Results.Where(x =>
                                {
                                    if (States.Contains(x.State))
                                    {
                                        switch (x.State)
                                        {
                                            case Adventure.AState.Listed:
                                                {
                                                    return x.IsStillValid();
                                                }
                                            default: return true;
                                        }
                                    }
                                    return false;
                                }).ToList();
                            }
                            #endregion

                            #region Filter Range
                            Results = Results.Where(x => (x.DistanceNM > RangeMin && x.DistanceNM < RangeMax) || x.DistanceNM == 0).ToList();
                            #endregion

                            #region Filter Runways
                            Results = Results.Where(x =>
                            {
                                if(x.Situations.Count == 0)
                                {
                                    return true;
                                }
                                else
                                {
                                    List<Airport> Original = x.Situations.Select(x1 => x1.Airport).ToList();
                                    List<Airport> Filtered = new AirportFilters.FilterRunways()
                                    {
                                        List = Original,
                                        RwySurface = RunwaysType,
                                        CountMin = RunwayCountMin,
                                        CountMax = RunwayCountMax,
                                        LengthMin = RunwaysLengthMin,
                                        LengthMax = RunwaysLengthMax,
                                        RequiresLight = RequiresLight,
                                        RequiresILS = RequiresILS,
                                    }.Get().ToList();
                                
                                    if(Filtered.Count == Original.Count)
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }).ToList();
                            #endregion

                            #region Filter Legs
                            Results = Results.Where(x =>
                            {
                                if (x.Situations.Count >= LegsMin && x.Situations.Count <= LegsMax)
                                {
                                    return true;
                                }
                                return false;
                            }).ToList();
                            #endregion

                            #region Filter Template Codes
                            List<AdventureTemplate> CarrotTemplates = new List<AdventureTemplate>();
                            if (Queries.Count > 0)
                            {
                                foreach (AdventureTemplate Template in Templates.FindAll(x => x.Activated))
                                {
                                    foreach (string word in Queries)
                                    {
                                        // Template Share Code
                                        if (Template.TemplateCode.ToUpper() == word || "#" + Template.TemplateCode.ToUpper() == word)
                                        {
                                            if(Template.ActiveAdventures.Count == 0)
                                            {
                                                if (!CarrotTemplates.Contains(Template)) { CarrotTemplates.Add(Template); }
                                                AdventureTemplateFeaturedService.DismissFeatured(Template.FileName);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Filter Meta for Queries
                            List<KeyValuePair<int, Adventure>> QuerySorting = new List<KeyValuePair<int, Adventure>>();
                            if (Queries.Count > 0)
                            {
                                string QueryStr = ((string)payload_struct["query"]).Trim();
                                Results = Results.Where(x =>
                                {
                                    int Score = 0;
                                
                                    if(x.ID.ToString() == QueryStr)
                                        Score += QueryStr.Length * 2;

                                    if(x.Template.Name.ToUpper().Contains(QueryStr))
                                        Score += QueryStr.Length * 2;

                                    if(x.Title != null)
                                        if (x.Title.ToUpper().Contains(QueryStr))
                                            Score += QueryStr.Length * 2;

                                    if (x.DescriptionString.ToUpper().Contains(QueryStr))
                                        Score += QueryStr.Length * 2;

                                    foreach (string word in Queries)
                                    {
                                        // If titled
                                        if(word.ToUpper() == "TITLED")
                                        {
                                            if(x.Template.Name != string.Empty)
                                                Score += 100;
                                        }

                                        // If custom
                                        if (word.ToUpper() == "CUSTOM")
                                        {
                                            if (x.Template.IsCustom)
                                                Score += 100;
                                        }

                                        // share code
                                        if (word.Length == 5)
                                            Score += 1;

                                        // Companies
                                        foreach (Company co in x.Template.Company)
                                        {
                                            if(co.Tag.Contains(word.ToUpper()))
                                                Score += 100;
                                        }

                                        // Template Share Code
                                        if (x.Template.TemplateCode.ToUpper() == word)
                                        {
                                            ShowFeatured = false;
                                            CarrotTemplates.Remove(x.Template);
                                            AdventureTemplateFeaturedService.DismissFeatured(x.Template.FileName);
                                            Score += 100;
                                        }

                                        // Template Share Code Specific
                                        if ("#" + x.Template.TemplateCode.ToUpper() == word)
                                        {
                                            ShowFeatured = false;
                                            CarrotTemplates.Remove(x.Template);
                                            AdventureTemplateFeaturedService.DismissFeatured(x.Template.FileName);
                                            Score += 10000;
                                        }

                                        // Hex Template name
                                        if (x.Template.EncryptedFileName.ToUpper() == word)
                                        {
                                            ShowFeatured = false;
                                            AdventureTemplateFeaturedService.DismissFeatured(x.Template.FileName);
                                            Score += 100;
                                        }

                                        // Template name
                                        if (x.Template.Name.ToUpper().Contains(word))
                                            Score += word.Length * 2;

                                        // Description
                                        //if (x.DescriptionString.ToUpper().Contains(word))
                                        //    Score += word.Length * 2;

                                        // Actions
                                        foreach(action_base Act in x.Actions)
                                        {
                                            if(Act.GetType() == typeof(cargo_pickup_2))
                                            {
                                                cargo_pickup_2 ActTyped = (cargo_pickup_2)Act;
                                                //if (ActTyped.Cargo.Name.ToUpper().Contains(word))
                                                //    Score += word.Length;
                                            }

                                            if (Act.GetType() == typeof(cargo_pickup))
                                            {
                                                cargo_pickup ActTyped = (cargo_pickup)Act;
                                                if (ActTyped.Cargo.Name.ToUpper().Contains(word))
                                                    Score += word.Length;
                                            }
                                        }

                                        // Airports
                                        foreach(Situation Sit in x.Situations)
                                        {
                                            if(Sit.Airport != null)
                                            {
                                                if (Sit.Airport.ICAO.Contains(word))
                                                    Score += word.Length;

                                                if (Sit.Airport.Name.ToUpper().Contains(word))
                                                    Score += word.Length;

                                                if (Sit.Airport.CountryName.ToUpper().Contains(word))
                                                    Score += word.Length;

                                                if (Sit.Airport.City.ToUpper().Contains(word))
                                                    Score += word.Length;

                                                if (Sit.Airport.State.ToUpper().Contains(word))
                                                    Score += word.Length;
                                            }
                                        }
                                    }
                                
                                    if(Score > 0)
                                    {
                                        QuerySorting.Add(new KeyValuePair<int, Adventure>(Score, x));
                                        return true;
                                    }

                                    return false;
                                }).ToList();
                            }
                            #endregion

                            #region Filter Weather
                            if (payload_struct.ContainsKey("weatherExcl"))
                            {
                                var Excl = payload_struct["weatherExcl"];
                                try
                                {
                                    int Count1 = 0;
                                    foreach(var l in Excl.Values)
                                    {
                                        Count1 += l.Count;
                                    }
                                    if(Count1 > 0)
                                    {
                                        bool WxIncl_Precip_Dry = true;
                                        bool WxIncl_Precip_Rain = true;
                                        bool WxIncl_Precip_Snow = true;
                                        bool WxIncl_Precip_TS = true;

                                        bool WxIncl_Wind_Calm = true;
                                        bool WxIncl_Wind_Windy = true;
                                        bool WxIncl_Wind_Crosswind = true;

                                        bool WxIncl_Vis_Low = true;
                                        bool WxIncl_Vis_Med = true;
                                        bool WxIncl_Vis_High = true;

                                        #region Read Exclusions
                                        foreach (var k in Excl)
                                        {
                                            switch (k.Key)
                                            {
                                                case "precip":
                                                    {
                                                        if (k.Value.Count > 0)
                                                        {
                                                            WxIncl_Precip_Dry = !k.Value.Contains("dry");
                                                            WxIncl_Precip_Rain = !k.Value.Contains("rain");
                                                            WxIncl_Precip_Snow = !k.Value.Contains("snow");
                                                            WxIncl_Precip_TS = !k.Value.Contains("thunderstorm");
                                                        }
                                                        break;
                                                    }
                                                case "wind":
                                                    {
                                                        if (k.Value.Count > 0)
                                                        {
                                                            WxIncl_Wind_Calm = !k.Value.Contains("calm");
                                                            WxIncl_Wind_Windy = !k.Value.Contains("windy");
                                                            WxIncl_Wind_Crosswind = !k.Value.Contains("crosswind");
                                                        }
                                                        break;
                                                    }
                                                case "vis":
                                                    {
                                                        if (k.Value.Count > 0)
                                                        {
                                                            WxIncl_Vis_Low = !k.Value.Contains("lowvis");
                                                            WxIncl_Vis_Med = !k.Value.Contains("medvis");
                                                            WxIncl_Vis_High = !k.Value.Contains("hivis");
                                                        }
                                                        break;
                                                    }
                                            }
                                        }
                                        #endregion

                                        Func<Airport, WeatherData, bool> ProcPrecip = (Apt, Wx) =>
                                        {
                                            if (WxIncl_Precip_Dry) { if (Wx.Precipitation == WeatherData.Precipitation_Types.NO) { return true; } }
                                            if (WxIncl_Precip_Rain) { if (Wx.Precipitation == WeatherData.Precipitation_Types.RA) { return true; } }
                                            if (WxIncl_Precip_TS) { if (Wx.Thunderstorm) { return true; } }
                                            if (WxIncl_Precip_Snow) { if (Wx.Precipitation == WeatherData.Precipitation_Types.SN) { return true; } }
                                            return false;
                                        };

                                        Func<Airport, WeatherData, bool> ProcWind = (Apt, Wx) =>
                                        {
                                            if (WxIncl_Wind_Calm){ if (Wx.WindSpeed < 5) { return true; } }
                                            if (WxIncl_Wind_Windy) { if (Wx.WindSpeed >= 5) { return true; } }
                                            if (WxIncl_Wind_Crosswind) {
                                                foreach (var Rwy in Apt.Runways)
                                                {
                                                    float Offset1 = (float)Math.Abs(Utils.MapCompareBearings(Wx.WindHeading, Rwy.Heading));
                                                    float Offset2 = (float)Math.Abs(Utils.MapCompareBearings(Wx.WindHeading, Rwy.Heading + 180));

                                                    if (Offset1 >= 40 || Offset1 >= 40)
                                                    {
                                                        return true;
                                                    } 
                                                }
                                            }
                                            return false;
                                        };

                                        Func<Airport, WeatherData, bool> ProcVis = (Apt, Wx) =>
                                        {
                                            if (WxIncl_Vis_High) { if (Wx.VisibilitySM < 30) { return true; } }
                                            if (WxIncl_Vis_Med) { if (Wx.VisibilitySM < 8) { return true; } }
                                            if (WxIncl_Vis_Low) { if (Wx.VisibilitySM < 3) { return true; } }
                                            return false;
                                        };

                                        Results = Results.Where(x =>
                                        {
                                            bool Found = false;
                                            bool HasWx = false;
                                            foreach (Airport Apt in x.Situations.Select(x1 => x1.Airport))
                                            {
                                                if(Apt != null)
                                                {
                                                    WeatherData Wx = Apt.GetWx();
                                                    if (Wx != null)
                                                    {
                                                        HasWx = true;
                                                        if (ProcPrecip(Apt, Wx) && ProcWind(Apt, Wx))
                                                        {
                                                            Found = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if(!HasWx) { return false; }
                                            return Found;
                                        }).ToList();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed to filter Weather: " + ex.Message);
                                }
                            }
                            #endregion
                            
                            #region Filter ICAOs
                            bool HasPerfectICAOMatch = false;
                            List<KeyValuePair<int, Adventure>> ICAOSorting = new List<KeyValuePair<int, Adventure>>();
                            if (ICAOGroup.Count > 0)
                            {
                                Results = Results.Where(x =>
                                {
                                    int Matches = 0;
                                    foreach (var ICAOList in ICAOGroup)
                                    {
                                        if (ICAOList.Count > 1)
                                        {
                                            int ICAOIndex = 0;
                                            foreach (string ICAO in ICAOList)
                                            {
                                                if (ICAO != "-")
                                                {
                                                    // Check if arrival
                                                    if (ICAOIndex > 0)
                                                    {
                                                        if (ICAOList[ICAOIndex - 1] == "-") // Is a arrival airport
                                                        {
                                                            int SitIndex = 0;
                                                            foreach (var Sit in x.Situations)
                                                            {
                                                                if (SitIndex > 0)
                                                                {
                                                                    if (Sit.Airport != null)
                                                                    {
                                                                        if (Sit.Airport.ICAO == ICAO)
                                                                        {
                                                                            Matches++;
                                                                        }
                                                                    }
                                                                }
                                                                SitIndex++;
                                                            }
                                                        }
                                                    }

                                                    // Check if departure
                                                    if (ICAOIndex < ICAOList.Count - 1)
                                                    {
                                                        if (ICAOList[ICAOIndex + 1] == "-") // Is a departure airport
                                                        {
                                                            Situation Sit = x.Situations[0];
                                                            if (Sit.Airport != null)
                                                            {
                                                                if (Sit.Airport.ICAO == ICAO)
                                                                {
                                                                    Matches++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                ICAOIndex++;
                                            }

                                        }
                                        else
                                        {
                                            foreach (var Sit in x.Situations)
                                            {
                                                if (Sit.Airport != null)
                                                {
                                                    if (ICAOList[0] == Sit.Airport.ICAO)
                                                    {
                                                        Matches++;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (Matches > 0)
                                    {
                                        if (x.Situations.Count == Matches)
                                        {
                                            HasPerfectICAOMatch = true;
                                        }
                                        if (States.Contains(x.State))
                                        {
                                            ICAOSorting.Add(new KeyValuePair<int, Adventure>(HasPerfectICAOMatch ? 1000 : Matches, x));
                                        }
                                        return true;
                                    }
                                    return false;
                                }).ToList();
                            }
                            #endregion

                            #region Filter Bounds
                            bool Reframe = false;
                            if (!Diced)
                            {
                                if (NW != null && SE != null)
                                {
                                    List<Adventure> TempResults = GetQueryContractsFromBounds(Results, NW, SE, 0, 1000);
                                    //if (TempResults.Count > 3)
                                    //{
                                        Results = TempResults;
                                    //}
                                    //else
                                    //{
                                    //    Reframe = true;
                                    //    GeoLoc Loc = new GeoLoc(Convert.ToDouble(payload_struct["center"][0]), Convert.ToDouble(payload_struct["center"][1]));
                                    //    Results = Results.OrderBy(x => Utils.MapCalcDist(Loc, x.Situations[0].Location, Utils.DistanceUnit.Kilometers, true)).ToList();
                                    //    //Results = Results.Take(Limit).ToList();
                                    //}

                                    foreach (var Adv in ICAOSorting)
                                    {
                                        if (Adv.Key == 1000)
                                        {
                                            if (!Results.Contains(Adv.Value))
                                            {
                                                Results.Add(Adv.Value);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Reframe = true;
                            }
                            #endregion

                            if (Progress.Progress.XP.Balance > 0 && States.Count > 0 ? (States.Contains(Adventure.AState.Listed)) : true)
                            {
                                #region Generate based on ShareCode
                                foreach (string Code in ShareCodes)
                                {
                                    Results = Results.Where(x =>
                                    {
                                        if (x.RouteCode == Code || x.Template.EncryptedFileName == Code || x.Template.TemplateCode.ToUpper() == Code)
                                        {
                                            return true;
                                        }
                                        return false;
                                    }).ToList();
                                    
                                    lock (Templates)
                                    {
                                        foreach (AdventureTemplate Template in Templates.FindAll(x => x.Activated))
                                        {
                                            Route Rte = Template.Routes.Find(x => x.RouteCode == Code);
                                            if (Rte != null)
                                            {
                                                lock(Template.ActiveAdventures)
                                                {
                                                    Adventure NA = Template.ActiveAdventures.Find(x => x.RouteCode == Code);
                                                    if (NA == null)
                                                    {
                                                        NA = Template.ActiveAdventures.Find(x => x.RouteString == Rte.RouteString);
                                                    }

                                                    if (NA == null)
                                                    {
                                                        NA = Template.CreateAdventureFromRoute(Rte, true);
                                                    }

                                                    if (NA != null)
                                                    {
                                                        Results.Add(NA);
                                                        if (NA.PullAt < DateTime.UtcNow && NA.ExpireAt < DateTime.UtcNow)
                                                        {
                                                            NA.Renew();
                                                        }
                                                    }

                                                    if (NA == null)
                                                    {
                                                        if (!CarrotTemplates.Contains(Template))
                                                        {
                                                            CarrotTemplates.Add(Template);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region Generate based on ICAO
                                if ((Results.Count < 3 || ICAOGroup.Find(x1 => x1.Count > 1) != null) && !HasPerfectICAOMatch && ICAOGroup.Count > 0 && UserData.Get("tier") != "prospect")
                                {
                                    foreach (var ICAOList in ICAOGroup)
                                    {
                                        if (ICAOList.FindAll(x => x != "-").Count > 1)
                                        {
                                            List<AdventureTemplate> TL = null;

                                            lock (Templates)
                                            {
                                                TL = Templates.FindAll(x => x.Activated && x.FileName.StartsWith("_default"));
                                            }

                                            foreach (AdventureTemplate Template in TL)
                                            {
                                                lock (Results)
                                                {
                                                    if (!HasPerfectICAOMatch)
                                                    {
                                                        List<Adventure> NA = Template.PopulateAdventuresFromICAO(ICAOList, 1, payload_struct);
                                                        Results.AddRange(NA);
                                                        foreach (Adventure Adv in NA)
                                                        {
                                                            HasPerfectICAOMatch = true;
                                                            lock (ICAOSorting)
                                                            {
                                                                ICAOSorting.Add(new KeyValuePair<int, Adventure>(Utils.GetRandom(10, 1000), Adv));
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        else// if(Depth < 3)
                                        {
                                            List<AdventureTemplate> TL = null;
                                            lock (Templates)
                                            {
                                                TL = Templates.FindAll(x => x.Activated).OrderBy(x => Utils.GetRandom(int.MaxValue)).ToList();
                                            }

                                            foreach (AdventureTemplate Template in TL)
                                            {
                                                lock (Results)
                                                {
                                                    List<Adventure> NA = Template.PopulateAdventuresFromICAO(ICAOList, 3, payload_struct);
                                                    Results.AddRange(NA);
                                                    foreach (Adventure Adv in NA)
                                                    {
                                                        lock (ICAOSorting)
                                                        {
                                                            ICAOSorting.Add(new KeyValuePair<int, Adventure>(Utils.GetRandom(10, 1000), Adv));
                                                        }
                                                    }
                                                }
                                            }

                                            //Depth++;
                                            //Command(Socket, StructSplit, structure, Depth++);
                                            //return;
                                        }
                                    }
                                }
                                #endregion
                            }

                            #region Limit to Valid
                            Results = Results.FindAll(x => x.IsStillValid());
                            #endregion

                            #region Count Destinations
                            List<Airport> DestinationsList = new List<Airport>();
                            foreach(Adventure Adv in Results)
                            {
                                foreach(Situation Sit in Adv.Situations)
                                {
                                    if(Sit.Airport != null)
                                    {
                                        if (!DestinationsList.Contains(Sit.Airport))
                                        {
                                            DestinationsList.Add(Sit.Airport);
                                        }
                                    }
                                }
                            }
                            #endregion
                            
                            #region Primary Sort
                            if (payload_struct.ContainsKey("sort"))
                            {
                                switch (Sorting)
                                {
                                    case "relevance":
                                        {
                                            SortingAsc = true;

                                            #region Framing
                                            float max_distance = 0;
                                            if (NW != null && SE != null)
                                            {
                                                max_distance = Utils.MapCalcDistFloat((float)NW.Lat, (float)NW.Lat, (float)SE.Lat, (float)SE.Lon, Utils.DistanceUnit.NauticalMiles);
                                            }
                                            #endregion

                                            List<KeyValuePair<double, Adventure>> WR = new List<KeyValuePair<double, Adventure>>();
                                            foreach (Adventure Adv in Results)
                                            {
                                                double weight = 0; // Utils.GetRandom((double)20);
                                                
                                                #region Custom Contracts
                                                weight += Adv.Template.IsCustom ? 10 : 0;
                                                #endregion

                                                #region Recommended aircraft (from filters)
                                                if (InlcudedTypes != null)
                                                {
                                                    foreach (int t in InlcudedTypes)
                                                    {
                                                        weight += (-50 + Adv.RecommendedAircraft[t]) / 100;
                                                    }
                                                }
                                                #endregion

                                                /*
                                                #region Sort by aircraft compatibility
                                                if(SimConnection.Aircraft != null)
                                                {
                                                    Dict["aircraft"] = SimConnection.Aircraft.Model;
                                                    weight += Adv.RecommendedAircraft[SimConnection.Aircraft.Size];
                                                }
                                                #endregion
                                                */

                                                #region Framing
                                                if(max_distance != 0 ? (Adv.DistanceNM / 2) > max_distance : false)
                                                {
                                                    weight -= Adv.DistanceNM;
                                                }
                                                #endregion

                                                #region Distance
                                                weight -= Math.Abs((Adv.DistanceNM / Adv.Situations.Count - 1) - DistanceBias) * 0.01;
                                                #endregion

                                                #region Reward/Distance
                                                weight += (Adv.RewardBux > 0 ? (Adv.RewardBux / Adv.DistanceNM) : 0);
                                                weight += (Adv.RewardXP > 0 ? (Adv.RewardXP / Adv.DistanceNM) : 0);
                                                #endregion

                                                #region Topography
                                                Adv.GenerateTopographyVariance();
                                                weight += Adv.TopographyVariance / 1000;
                                                #endregion

                                                #region State
                                                switch (Adv.State)
                                                {
                                                    case Adventure.AState.Active: weight += 1000; break;
                                                    case Adventure.AState.Saved: weight += 999; break;
                                                    case Adventure.AState.Succeeded: weight += 998; break;
                                                    case Adventure.AState.Failed: weight += 997; break;
                                                }
                                                #endregion

                                                if(weight > 150)
                                                {

                                                }
                                                
                                                WR.Add(new KeyValuePair<double, Adventure>(weight, Adv));
                                            }

                                            #region Create diversity
                                            Dictionary<AdventureTemplate, int> TemplateCount = new Dictionary<AdventureTemplate, int>();
                                            Results = WR.OrderByDescending(x =>
                                            {
                                                if (!TemplateCount.ContainsKey(x.Value.Template)) { TemplateCount.Add(x.Value.Template, 0); }
                                                TemplateCount[x.Value.Template]++;

                                                return x.Key - (TemplateCount[x.Value.Template] * 5);
                                                //return x.Key;
                                            }).Select(x => x.Value).ToList();

                                            #endregion
                                            break;
                                        }
                                    case "topography_var":
                                        {
                                            Results = Results.FindAll(x => x.TopographyData.Count > 0);
                                            Results = Results.OrderBy(x =>
                                            {
                                                x.GenerateTopographyVariance();
                                                return x.TopographyVariance;
                                            }).ToList();
                                            break;
                                        }
                                    case "payload":
                                        {
                                            Results = Results.OrderBy(x => x.GetTotalWeight()).ToList();
                                            break;
                                        }
                                    case "aircraft":
                                        {
                                            SortingAsc = true;

                                            bool HasAircraft = false;
                                            Dictionary<Adventure, float> NewResults = new Dictionary<Adventure, float>();
                                            List<GeoPosition> FleetLocations = null;
                                            lock (FleetService.Fleet)
                                            {
                                                FleetLocations = FleetService.Fleet.Select(x => x.Location != null ? new GeoPosition(x.Location[0], x.Location[1], x.Location[2]) : null).Where(x => x != null).ToList();
                                            }

                                            if(payload_struct.ContainsKey("location"))
                                            {
                                                FleetLocations.Add(new GeoPosition(Convert.ToDouble(payload_struct["location"][0]), Convert.ToDouble(payload_struct["location"][1])));
                                            }
                                            
                                            // Loop fleet
                                            if(NW != null && SE != null)
                                            {
                                                foreach (var aircraftLocation in FleetLocations)
                                                {
                                                    if (Utils.IsPointInPoly(new List<GeoLoc>()
                                                    {
                                                        NW,
                                                        new GeoLoc(NW.Lon, SE.Lat),
                                                        SE,
                                                        new GeoLoc(SE.Lon, NW.Lat),
                                                        NW,
                                                    }, new GeoLoc(aircraftLocation)))
                                                    {
                                                        HasAircraft = true;
                                                    }

                                                    foreach (var result in Results)
                                                    {
                                                        float dist = (float)Utils.MapCalcDist(result.Situations[0].Location, new GeoLoc(aircraftLocation), Utils.DistanceUnit.Kilometers, true);
                                                        if (NewResults.ContainsKey(result))
                                                        {
                                                            if (NewResults[result] > dist)
                                                            {
                                                                NewResults[result] = dist;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            NewResults.Add(result, dist);
                                                        }
                                                    }
                                                }
                                            }
                                            
                                            if(HasAircraft)
                                            {
                                                var test = NewResults.OrderBy(x => x.Value).ToList();
                                                var test1 = test.Select(x => x.Key).ToList();
                                                Results = test1;
                                            }
                                            break;
                                        }
                                    case "distance":
                                        {
                                            Results = Results.OrderBy(x => x.DistanceNM).ToList();
                                            break;
                                        }
                                    case "xp":
                                        {
                                            Results = Results.OrderBy(x => x.RewardXP).ToList();
                                            break;
                                        }
                                    case "reward":
                                        {
                                            Results = Results.OrderBy(x => x.RewardBux).ToList();
                                            break;
                                        }
                                    case "reward-distance":
                                        {
                                            Results = Results.OrderBy(x => x.DistanceNM / x.RewardBux).ToList();
                                            break;
                                        }
                                    case "requested":
                                        {
                                            Results = Results.OrderBy(x =>
                                            {
                                                if(x.RequestedAt != null)
                                                {
                                                    return x.RequestedAt;
                                                }
                                                else
                                                {
                                                    return x.ExpireAt;
                                                }
                                            }).ToList();
                                            break;
                                        }
                                    case "ending":
                                        {
                                            Results = Results.OrderBy(x => x.Template.TimeToComplete == -1 && !x.Template.RunningClock ? DateTime.MaxValue : x.ExpireAt).ToList();
                                            break;
                                        }
                                    default:
                                        {
                                            Results = Results.OrderBy(x => x.State).ToList();
                                            break;
                                        }
                                }

                                //int Index = 0;
                                //int TemplateCount = 0;
                                //AdventureTemplate LastTemplate = null;
                                //Results = Results.OrderBy(x =>
                                //{
                                //    if (LastTemplate != x.Template)
                                //    {
                                //        TemplateCount = 0;
                                //        LastTemplate = x.Template;
                                //    }
                                //    TemplateCount++;
                                //    Index++;
                                //
                                //    if (SortingAsc)
                                //    {
                                //        return Index; //* ((float)TemplateCount);
                                //    }
                                //    else
                                //    {
                                //        return Index * (1 + ((float)TemplateCount * 0.1));
                                //    }
                                //}).ToList();

                                if (!SortingAsc)
                                {
                                    Results.Reverse();
                                }

                            }
                            else
                            {
                                Results = Results.OrderBy(x => x.State).ToList();
                            }
                            #endregion
                        
                            #region Query Sort
                            if (QuerySorting.Count > 0)
                            {
                                Results = Results.OrderByDescending(x => QuerySorting.Find(x1 => x1.Value == x).Key).ToList();
                            }
                            #endregion
                        
                            #region ICAO Sort
                            if (ICAOSorting.Count > 0)
                            {
                                Results = Results.OrderByDescending(x =>
                                {
                                    var t = ICAOSorting.Find(x1 => x1.Value == x);
                                    return t.Value != null ? t.Key : -1;
                                }).ToList();
                            }
                            #endregion

                            #region Make the big airports list
                            foreach (Adventure Adv in Results)
                            {
                                if(Airports.Count > 700)
                                {
                                    break;
                                }
                                foreach(Situation Sit in Adv.Situations)
                                {
                                    if(Sit.Airport != null)
                                    {
                                        if (!Airports.ContainsKey(Sit.Airport))
                                        {
                                            Airports.Add(Sit.Airport, 1);
                                        }
                                        else
                                        {
                                            Airports[Sit.Airport]++;
                                        }
                                    }
                                }
                            }
                            
                            GeoLoc LastLoc = LocationHistory.GetLastLocation();
                            if (LastLoc != null)
                            {
                                int rangeCount = 0;
                                foreach (var apt in SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastLoc, 500))
                                {
                                    if(apt.Value != null)
                                    {
                                        if (rangeCount > 30)
                                        {
                                            break;
                                        }
                                        if (!Airports.ContainsKey(apt.Value))
                                        {
                                            Airports.Add(apt.Value, 0);
                                            rangeCount++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            #endregion

                            #region Set Featured Dates
                            List<KeyValuePair<AdventureTemplate, int>> FeaturedTemplates = new List<KeyValuePair<AdventureTemplate, int>>();
                            if (ShowFeatured)
                            {
                                List<AdventureTemplate> TemplatesContenders = new List<AdventureTemplate>();
                                try
                                {
                                    #region Action to figure out if we're within the time brackets for featured cards
                                    Action<AdventureTemplate, int> ProcessFeaturedTemplate = (Template, DaysCount) =>
                                    {
                                        if (FeaturedTemplates.Count < 3)
                                        {
                                            int Days = AdventureTemplateFeaturedService.IsFeatured(Template.FileName);
                                            if (Days < DaysCount && Template.IsCustom)
                                            {
                                                int c = Results.FindAll(x => x.Template == Template).Count;
                                                FeaturedTemplates.Add(new KeyValuePair<AdventureTemplate, int>(Template, c));
                                            }
                                        }
                                    };
                                    #endregion

                                    #region Add all titled templates to the contenders
                                    foreach (var Result in Results)
                                    {
                                        if (!TemplatesContenders.Contains(Result.Template) && Result.Template.IsCustom)
                                        {
                                            TemplatesContenders.Add(Result.Template);
                                        }
                                    }
                                    #endregion

                                    #region Loop through contenders, only keep the ones within 15 days
                                    foreach (var Template in TemplatesContenders)
                                    {
                                        ProcessFeaturedTemplate(Template, 15);
                                    }
                                    #endregion

                                    #region If we didn't find any, pick a random one!
                                    if (FeaturedTemplates.Count == 0)
                                    {
                                        foreach (var Template in Templates.FindAll(x => x.Activated && !TemplatesContenders.Contains(x) && x.ActiveAdventures.Count > 0))
                                        {
                                            if (FeaturedTemplates.Count < 3)
                                            {
                                                ProcessFeaturedTemplate(Template, 4);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed to get Featured cards: " + ex.Message + " - " + ex.StackTrace);
                                }
                            }
                            #endregion

                            /*
                            if(IsDev)
                            {
                                Dictionary<AdventureTemplate,int> QueryTemplates = new Dictionary<AdventureTemplate, int>();
                                foreach(var Adv in Results)
                                {
                                    if (!QueryTemplates.ContainsKey(Adv.Template))
                                    {
                                        QueryTemplates.Add(Adv.Template, 1);
                                    }
                                    else
                                    {
                                        QueryTemplates[Adv.Template]++;
                                    }
                                }

                                foreach(var templ in QueryTemplates)
                                {
                                    Console.WriteLine("Found Template " + templ.Key.FileName + " with " + templ.Value + " out of " + templ.Key.ActiveAdventures.Count + " contracts");
                                }
                            }
                            */

                            
                            #region Limit/Offset amount
                            Count = Results.Count;

                            if(Offset > 0)
                                Results.RemoveRange(0, Math.Min(Results.Count, Offset));

                            if (!Diced)
                            {
                                int LimitOffset = Results.Count - 1;
                                while (Results.Count > Limit && LimitOffset > 0)
                                {
                                    switch (Results[LimitOffset].State)
                                    {
                                        case Adventure.AState.Active:
                                        case Adventure.AState.Saved:
                                            {
                                                LimitOffset -= 2;
                                                break;
                                            }
                                        default:
                                            {
                                                Results.RemoveAt(LimitOffset);
                                                LimitOffset--;
                                                break;
                                            }
                                    }
                                }

                            }
                            else
                            {
                                if (Results.Count == 0)
                                {
                                    GetAllListed(null, (Adv) =>
                                    {
                                        Results.Add(Adv);
                                    });
                                }
                                
                                if(Results.Count > 0)
                                {
                                    WeightedRandom<Adventure> wr = new WeightedRandom<Adventure>();
                                    foreach(var Adv in Results) {
                                        double value = 0;
                                        value += Adv.Template.IsCustom ? 300 : 0;
                                        value += (Adv.Situations.Select(x => x.Airport != null ? x.Airport.Relief : 0).Average() / Adv.Situations.Count) * 0.5;
                                        value += Adv.Template.IsCustom ? Adv.RewardXP : Adv.RewardXP * 0.01;
                                        wr.AddEntry(Adv, value);
                                    }
                                    Results = wr.GetRandoms(5);
                                }
                            }
                            #endregion

                            #region Making sure the results are all ready
                            foreach (Adventure adv in Results)
                            {
                                adv.GenerateTopography();
                            }
                            #endregion

                            #region Build output Dictionary
                            foreach (var Template in CarrotTemplates)
                            {
                                if(Template.IsCustom || Results.Count == 0)
                                {
                                    Dict["featured"].Add(Template.GetCarrot());
                                }
                            }
                            if(CarrotTemplates.Count == 0)
                            {
                                foreach (var Template in FeaturedTemplates.OrderByDescending(x => x.Value))
                                {
                                    Dict["featured"].Add(Template.Key.GetFeatured());
                                }
                            }

                            foreach (Adventure Adv in Results)
                            {
                                if (!FoundTemplates.Contains(Adv.Template))
                                {
                                    FoundTemplates.Add(Adv.Template);
                                    Dict["templates"].Add(Adv.Template.Serialize((Dictionary<string, dynamic>)payload_struct["fields"]["template"]));
                                }
                                Dict["contracts"].Add(Adv.Serialize((Dictionary<string, dynamic>)payload_struct["fields"]["contract"]));
                            }

                            //foreach(KeyValuePair<Airport, int> Apt in Airports)
                            //{
                            //    Dictionary<string, dynamic> AptStruct = Apt.Key.Serialize(null);
                            //    AptStruct.Add("count", Apt.Value);
                            //    Dict["airports"].Add(AptStruct);
                            //}
                            Dict["reframe"] = Reframe;
                            Dict["count"] = Count;
                            Dict["limit"] = Limit;
                            Dict["destinations"] = DestinationsList.Count;
                            #endregion
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to find results: " + ex.Message);
                        }

                        Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(Dict), (Dictionary<string, dynamic>)structure["meta"]);
                        
                        break;
                    }
                case "featured":
                    {
                        switch(StructSplit[2])
                        {
                            case "hide":
                                {
                                    AdventureTemplateFeaturedService.DismissFeatured((string)payload_struct["FileName"]);
                                    break;
                                }
                        }
                        break;
                    }
                case "manifests":
                    {
                        switch (StructSplit[2])
                        {
                            case "get-all":
                                {
                                    Socket.SendMessage(string.Join(":", StructSplit), JSSerializer.Serialize(GetPayload()), (Dictionary<string, dynamic>)structure["meta"]);
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        
        public static List<Adventure> GetQueryContractsFromLocation(GeoLoc Loc, float Min, float Max, int Limit, bool Permissive = false)
        {
            List<Adventure> Results = null;
            
            lock (AllContracts)
            {
                Results = new List<Adventure>(AllContracts);
            }

            Results = Results.Where(x => x.DistanceNM > Min && x.DistanceNM < Max).ToList();
            
            Results = Results.OrderByDescending((x) =>
            {
                float Dist1 = (float)Utils.MapCalcDist(x.Situations.First().Location, Loc, Utils.DistanceUnit.NauticalMiles, true);
                //float Dist2 = (float)Utils.MapCalcDist(x.Situations.Last().Location, Loc, Utils.DistanceUnit.NauticalMiles, true);
                //if(Dist1 > Dist2)
                //{
                    //return Dist2;
                //}
                //else
                //{
                    return Dist1;
                //}
            }).ToList();

            return Results;
        }

        public static List<Adventure> GetQueryContractsFromBounds(List<Adventure> Base, GeoLoc NW, GeoLoc SE, int Min, int Max)
        {
            //double DiagDist = Utils.MapCalcDist(NW, SE, Utils.DistanceUnit.NauticalMiles, true);
            List<Adventure> Results = new List<Adventure>();
            List<Adventure> ResultsPermissive = new List<Adventure>();

            GeoLoc InNW = NW;
            GeoLoc InSE = SE;

            bool Flipped = false;
            if (InNW.Lon - InSE.Lon > 180) { Flipped = true; }
            if (InNW.Lon < -180) { InNW = new GeoLoc(InNW.Lon + 360, InNW.Lat); }
            if (InSE.Lon > 180) { InSE = new GeoLoc(InSE.Lon - 360, InSE.Lat); }
            if (InNW.Lon - InSE.Lon > 180) { Flipped = true; }

            lock (AllContracts)
            {
                foreach (Adventure Adv in Base)
                {
                    List<Situation> Within = new List<Situation>();
                    int i = 0;
                    foreach (Situation Sit in Adv.Situations)
                    {
                        if(!Flipped)
                        {
                            if (Sit.Location.Lon > InNW.Lon)
                            {
                                if (Sit.Location.Lat < InNW.Lat)
                                {
                                    if (Sit.Location.Lon < InSE.Lon)
                                    {
                                        if (Sit.Location.Lat > InSE.Lat)
                                        {
                                            Within.Add(Sit);
                                            if (Within.Count == Adv.Situations.Count)
                                            {
                                                if (!Results.Contains(Adv))
                                                {
                                                    Results.Add(Adv);
                                                }
                                            }
                                            else if (i == 0 || i == Adv.Situations.Count - 1)
                                            {
                                                if (!ResultsPermissive.Contains(Adv))
                                                {
                                                    ResultsPermissive.Add(Adv);
                                                }
                                                continue;
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Sit.Location.Lon > InNW.Lon || Sit.Location.Lon < InSE.Lon)
                            {
                                if (Sit.Location.Lat < InNW.Lat)
                                {
                                    if (Sit.Location.Lat > InSE.Lat)
                                    {
                                        Within.Add(Sit);
                                        if (Within.Count == Adv.Situations.Count)
                                        {
                                            if (!Results.Contains(Adv))
                                            {
                                                Results.Add(Adv);
                                            }
                                        }
                                        else if (i == 0 || i == Adv.Situations.Count - 1)
                                        {
                                            if (!ResultsPermissive.Contains(Adv))
                                            {
                                                ResultsPermissive.Add(Adv);
                                            }
                                            continue;
                                        }

                                    }
                                }
                            }
                        }
                        
                        i++;
                    }
                }
            }

            int i1 = 0;
            while(Results.Count < Max && i1 < ResultsPermissive.Count)
            {
                if (!Results.Contains(ResultsPermissive[i1]))
                {
                    Results.Add(ResultsPermissive[i1]);
                }
                i1++;
            }

            return Results;
        }


        public static void GetAllListed(Adventure.AState[] state, Action<Adventure> cb)
        {
            int ti = 0;
            lock (Templates)
            {
                while (ti < Templates.Count)
                {
                    try
                    {
                        AdventureTemplate T = Templates[ti];
                        if (T != null)
                        {
                            //if (!T.Ready)
                            //{
                            //    ti++;
                            //    continue;
                            //}

                            if(T.Ready)
                            {
                                int i = 0;
                                lock (T.ActiveAdventures)
                                {
                                    while (i < T.ActiveAdventures.Count)
                                    {
                                        try
                                        {
                                            Adventure adv = T.ActiveAdventures[i];
                                            if (adv != null && state != null ? state.Contains(adv.State) : true)
                                            {
                                                cb(adv);
                                            }
                                        }
                                        catch
                                        {
                                        }
                                        i++;
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                    ti++;
                }
            }
        }

        public static void GetAllLive(Action<Adventure> cb)
        {
            int ti = 0;
            while (ti < Templates.Count)
            {
                try
                {
                    AdventureTemplate T = Templates[ti];
                    if (T != null)
                    {
                        int i = 0;
                        lock (T.LiveAdventures)
                        {
                            while (i < T.LiveAdventures.Count)
                            {
                                try
                                {
                                    Adventure adv = T.LiveAdventures[i];
                                    if (adv != null)
                                    {
                                        cb(adv);
                                    }
                                }
                                catch
                                {
                                }
                                i++;
                            }
                        }
                    }
                }
                catch
                {
                }
                ti++;
            }
        }

        public static void Process(TemporalData _TemporalLast, TemporalData _TemporalNewBuffer)
        {
            if (!RestoreStarted)
            {
                RestoreStarted = true;
                ImportPersistence();
            }

            TemporalLast = _TemporalLast;
            TemporalNewBuffer = _TemporalNewBuffer;
            
            GetAllLive((Adventure) =>
            {
                Adventure.Process();
            });

        }
        
        public static void ProcessInstantData(TemporalData _TemporalNewBuffer)
        {
            if(TemporalLiveLast != null)
            {
                GetAllLive((Adventure) =>
                {
                    Adventure.CheckMonitorState(false);
                    if (Adventure.IsMonitored)
                    {
                        int i1 = 0;
                        while (i1 < Adventure.ActionsWatch.Count)
                        {
                            action_base action = Adventure.ActionsWatch[i1];
                            if (InstantTypes.Contains(action.GetType()))
                            {
                                action.ProcessLive(TemporalLiveLast, _TemporalNewBuffer);
                            }
                            i1++;
                        }
                    }
                });
            }

            TemporalLiveLast = _TemporalNewBuffer.Copy();

        }
        
        public static void ProcessBroadcasts()
        {
            GetAllListed(null, (Adv) =>
            {
                Adv.ProcessBroadcasts();
            });

            SetPayload(false);
        }

        public static void StartValidityThread()
        {
            if (ValidityThread == null)
            {
                ValidityThread = Task.Factory.StartNew(async () =>
                {
                    CultureInfo.CurrentCulture = CI;
                    await Task.Delay(10000);


                    int TemplateIndex = 0;
                    while(ValidityThread != null && !MW.IsShuttingDown)
                    {
                        //if(IsDev)
                            //await Task.Delay(3000);

                        if (Templates.Count == 0)
                        {
                            TemplateIndex = 0;
                            await Task.Delay(1500);
                        }
                        else
                        {
                            AdventureTemplate TargetTemplate = null;
                            lock (Templates)
                            {
                                if (TemplateIndex >= Templates.Count)
                                {
                                    TemplateIndex = 0;
                                    RecalibrateSearchBias();
                                    continue;
                                }

                                try
                                {
                                    TargetTemplate = Templates[TemplateIndex];
                                }
                                catch
                                {
                                    TemplateIndex++;
                                    continue;
                                }
                            }

                            if ((TargetTemplate.KarmaGainBase < 0 && UserData.Get("illicit") == "0") || !TargetTemplate.Activated)
                            {
                                await Task.Delay(1500);
                                TemplateIndex++;
                                continue;
                            }

                            bool Cancel = false;
                            int Index = 0;
                            int AdventureCount = TargetTemplate.ActiveAdventures.Count;
                            while (AdventureCount > 0 && AdventureCount > Index && !MW.IsShuttingDown && TargetTemplate.ActiveAdventures.Count > 0)
                            {
                                Adventure TargetAdventure = null;
                                try
                                {
                                    lock (TargetTemplate.ActiveAdventures)
                                    {
                                        if(TargetTemplate.ActiveAdventures.Count < Index)
                                        {
                                            Cancel = true;
                                            break;
                                        }

                                        TargetAdventure = TargetTemplate.ActiveAdventures[Index];
                                    }
                                }
                                catch
                                {
                                    Index = 0;
                                    TemplateIndex = 0;
                                    continue;
                                }

                                if (ValidityThread == null)
                                    return;

                                if (TargetAdventure.Ready && TargetAdventure.State == Adventure.AState.Listed)
                                {
                                    //if(Utils.GetRandom(5) == 1)
                                    //{
                                    //TargetAdventure.Remove();
                                    //TargetAdventure.Save();
                                    //}
                                    //else
                                    //{
                                    if (!TargetAdventure.IsStillValid())
                                    {
                                    //        if (Utils.GetRandom(2) == 1)
                                    //        {
                                    //            //Console.WriteLine(ActiveAdventures[Index].ID + " is no longer valid (" + ActiveAdventures[Index].ExpireAt.ToString("O") + "). Renewing");
                                    //            TargetAdventure.Renew();
                                    //            TargetAdventure.Save();
                                    //        }
                                    //        else
                                    //        {
                                    //            //Console.WriteLine(ActiveAdventures[Index].ID + " is no longer valid (" + ActiveAdventures[Index].ExpireAt.ToString("O") + "). Removing");
                                        TargetAdventure.Remove();
                                    //            TargetAdventure.Save();
                                    //        }
                                    }
                                    //}
                                }

                                AdventureCount = TargetTemplate.ActiveAdventures.Count;
                                await Task.Delay(10);
                                Index++;
                            }

                            if(!Cancel)
                            {
                                TargetTemplate.PopulateAdventures(true, false, AircraftBias);
                                TargetTemplate.SetTopography(true);
                            }
                            TemplateIndex++;
                        }

                    }
                    
                }, ThreadCancel.Token);
            }
        }

        public static void Clear()
        {
            Console.WriteLine("Clearing Adventures");
            lock (AllContracts)
            {
                while(AllContracts.Count > 0)
                {
                    AllContracts[0].Remove();
                }
            }
            Console.WriteLine("Cleared Adventures");
        }

        public static void Disconnect()
        {
            GetAllLive((Adventure) =>
            {
                Adventure.SetMonitorState(false);
            });
        }

        public static void ChangeAircraft(AircraftInstance old_aircraft, AircraftInstance new_aircraft)
        {
            GetAllLive((Adventure) =>
            {
                switch (Adventure.State)
                {
                    case Adventure.AState.Active:
                        {
                            //Adventure.CheckMonitorState(false);
                            Adventure.SetMonitorState(false);
                            Adventure.ChangedAircraft(old_aircraft, new_aircraft);
                            break;
                        }
                }
                Adventure.RevalidateAircraft();
                Adventure.ScheduleStateBroadcast = true;
            });
        }
        
        public static void RecalibrateSearchBias()
        {
            DistanceBias = 0;
            AircraftBias = new List<float>() { 0, 0, 0, 0, 0, 0 };

            List<Adventure> BiasContracts = null;

            lock (AllContracts)
            {
                var td = TimeSpan.FromDays(365);
                var tn = DateTime.UtcNow;
                BiasContracts = AllContracts.Where(x => x.State == Adventure.AState.Succeeded && (tn - x.CompletedAt) < td).ToList();
            }

            List<float> LegDistances = new List<float>();
            foreach (var adventure in BiasContracts)
            {
                #region Aircraft Bias
                int i = 0;
                foreach (var recommendation in adventure.RecommendedAircraft)
                {
                    AircraftBias[i] += (Math.Max(0, recommendation) / BiasContracts.Count) / 100;
                    i++;
                }
                #endregion

                #region Distance
                LegDistances.Add(adventure.DistanceNM / (adventure.Situations.Count - 1));
                #endregion
            }

            if(LegDistances.Count > 0)
            {
                DistanceBias = LegDistances.Average();
            }

            #region Calculate pointage system
            lock (Templates)
            {
                TotalTemplatePoints = Templates.Where(x => x.Activated && x.ValidateTemplate()).Sum(x => x.Instances);
            }
            #endregion
        }

        public static void RevalidateLoadmaster()
        {
            if(UserData.Get("tier") != "discovery")
            {
                if (Utils.CalculateLevel(Progress.Progress.XP.Balance) > 5)
                {
                    Invoice NewInvoice = new Invoice()
                    {
                        Title = "%bobservice%",
                        Status = Invoice.STATUS.QUOTE,
                        PayeeType = Invoice.ACCOUNTTYPE.SERVICE,
                        PayeeAccount = "bobsaeroservice",
                        ClientType = Invoice.ACCOUNTTYPE.PRIVATE,
                        ClientAccount = "bank_checking",
                    };

                    DateTime? last_staff_loadmaster = Loadmaster.GetLastPay();
                    Fee loadmaster_fee = new Fee() { Code = "staff_loadmaster", Amount = 250 };
                    if (last_staff_loadmaster == null)
                    {
                        NewInvoice.Fees.Add(loadmaster_fee);
                    }
                    else
                    {
                        if (DateTime.Now - last_staff_loadmaster > TimeSpan.FromHours(24))
                        {
                            NewInvoice.Fees.Add(loadmaster_fee);
                        }
                    }

                    NewInvoice.Pay();
                }
            }
        }
                
        public static void LoadTemplate(bool IsSave, KeyValuePair<string, string> TemplateFile, Action<AdventureTemplate> Callback)
        {
            Task.Factory.StartNew(() =>
            {
                if(MW.IsShuttingDown)
                {
                    return;
                }

                CultureInfo.CurrentCulture = CI;

                try
                {
                    Dictionary<string, dynamic> TemplateContent = JSSerializer.Deserialize<Dictionary<string, dynamic>>(TemplateFile.Value);
                    
                    if(IsDev)
                    {
                        Console.WriteLine("Loading Template " + TemplateFile.Key);
                        string Fn = (string)TemplateContent["File"];
                        DownloadSpeech(Fn, TemplateContent);

                        if ((bool)(TemplateContent["Published"]))
                        {
#if DEBUG
                            if (MW.IsShuttingDown)
                            {
                                return;
                            }

                            var tv1 = DevProcess.TemplateValidation(Fn, TemplateContent);
                            var tv2 = DevProcess.TemplateDownloadImages(Fn, TemplateContent);
                            
                            lock (LiteDbService.DBAdv)
                            {
                                var DBCollection = LiteDbService.DBAdv.Database.GetCollection("templates");

                                var existing = DBCollection.FindOne(x => x["File"] == Fn);
                                if(existing != null)
                                {
                                    DateTime EMO = DateTime.Parse((string)existing["ModifiedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                                    DateTime NMO = DateTime.Parse((string)TemplateContent["ModifiedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                                    if (EMO.Ticks != NMO.Ticks)
                                    {
                                        if(EMO < NMO)
                                        {
                                            if (MessageBox.Show("Publish " + Fn + " with new routes?", "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                                            {
                                                DBCollection.Upsert(Fn, BsonMapper.Global.ToDocument(TemplateContent));
                                            }
                                        }
                                        else
                                        {
                                            MessageBox.Show("Trying to update " + Fn + " with an older version. \n\nNEW: " + NMO + " \nEXISTING: " + EMO + "", "Error updating " + Fn + ".", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                    else
                                    {
                                        if(tv1 || tv2)
                                        {
                                            DBCollection.Upsert(Fn, BsonMapper.Global.ToDocument(TemplateContent));
                                        }
                                    }
                                }
                                else
                                {
                                    DBCollection.Insert(Fn, BsonMapper.Global.ToDocument(TemplateContent));
                                }

                            }

                            foreach (string p in new string[]
                            {
                                //Path.Combine(DocumentsDirectory, "Adventures", (string)(TemplateContent["File"]) + ".p42adv"),
                                Path.Combine(DocumentsDirectory, "Adventures", (string)(TemplateContent["File"]) + ".rtl"),
                            })
                            {
                                try
                                {
                                    if (File.Exists(p))
                                    {
                                        File.Delete(p);
                                    }
                                }
                                catch
                                {

                                }
                            }
#endif
                        }
                        else
                        {
                            lock (LiteDbService.DBAdv)
                            {
                                var DBCollection = LiteDbService.DBAdv.Database.GetCollection("templates");
                                DBCollection.Delete((string)TemplateContent["File"]);
                            }

                            string SavePath = Path.Combine(DocumentsDirectory, "Adventures");
                            string FilePath = Path.Combine(SavePath, TemplateContent["File"] + ".p42adv");

                            if(!File.Exists(FilePath))
                            {
                                if (!Directory.Exists(SavePath))
                                {
                                    Directory.CreateDirectory(SavePath);
                                }

                                using (StreamWriter writer = new StreamWriter(FilePath, false))
                                {
                                    writer.WriteLine(TemplateFile.Value);
                                }
                            }
                        }
                    }

                    if (!Sources.Contains(TemplateFile.Key))
                    {
                        Sources.Add(TemplateFile.Key);
                    }

                    AdventureTemplate NA = new AdventureTemplate(TemplateFile.Key.Replace(".p42adv", ""), TemplateContent, true);

                    if(App.IsDev)
                    {
                        Console.WriteLine("Filename for " + NA.FileName + " is " + NA.EncryptedFileName);
                    }
                    
                    Callback(NA);
                }
                catch (Exception ex)
                {
                    if(IsDev)
                    {
                        Notifications.NotificationService.Add(new Notifications.Notification()
                        {
                            Title = "Failed to load " + TemplateFile.Key,
                            Message = ex.Message
                        });
                    }
                    Callback(null);
                }
                
            }, ThreadCancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }
        
        public static void LoadTemplates(Action<AdventureTemplate, int, int> TemplateCallback)
        {
            #region Send Notification
            NotificationService.Add(new Notification()
            {
                UID = 951,
                Title = "Contracts are generating...",
                Message = "You might get limited results while contracts are being generated by your system.",
                Type = NotificationType.Status,
                CanDismiss = false,
                IsTransponder = true,
            });
            #endregion
            
            Dictionary<string, string> TemplatesJSON = new Dictionary<string, string>();
                
            #region Load Documents Templates
            if(IsDev)
            {
                string AdventuresDir = Path.Combine(DocumentsDirectory, "Adventures");
                if (Directory.Exists(AdventuresDir))
                {
                    foreach (string TemplateFile in Directory.GetFiles(AdventuresDir, "*.p42adv", SearchOption.AllDirectories))
                    {
                        FileInfo FI = new FileInfo(TemplateFile);
                        try
                        {
                            if (!TemplatesJSON.ContainsKey(FI.Name))
                            {
                                string TemplateJSON = string.Join("", File.ReadAllLines(TemplateFile));
                                TemplatesJSON.Add(FI.Name, TemplateJSON);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to load Contracts Template " + TemplateFile + " because " + ex.Message + " / " + ex.StackTrace);
                        }
                    }
                }
            }
            #endregion

            #region Load integrated Templates
            var DBCollection = LiteDbService.DBAdv.Database.GetCollection("templates");
            IEnumerable<BsonDocument> DBTemplates = DBCollection.FindAll();

            foreach (var TemplateFile in DBTemplates)
            {
                try
                {
                    string FileName = ((string)TemplateFile["File"]) + ".p42adv";

#if DEBUG
                    string AdventuresDir = Path.Combine(DocumentsDirectory, "Adventures");
                    if(!File.Exists(Path.Combine(AdventuresDir, FileName)))
                    {
                        DBCollection.Delete(TemplateFile);
                        continue;
                    }
#endif
                    if (TemplateFile["Loaded"])
                    {
                        var TemplateContent = JSSerializer.Serialize(BsonMapper.Global.Deserialize<Dictionary<string, dynamic>>(TemplateFile));
                        if (!TemplatesJSON.ContainsKey(FileName))
                            TemplatesJSON.Add(FileName, TemplateContent);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load Template because " + ex.Message + " / " + ex.StackTrace);
                }
            }
            #endregion

            //if (!OwnsProduct("TSP"))
            //{
            //    Console.WriteLine("---");
            //    OrbxLic.LicensingManager.Refresh();
            //    Thread.Sleep(2000);
            //    UpdateInstance.Apply(true, true);
            //    Environment.Exit(0);
            //}

            Console.WriteLine("Starting to load Adventure Templates...");
            TemplatesCount = TemplatesJSON.Count;
            int At = 0;
            
            if(TemplatesCount > 0)
            {
                foreach (KeyValuePair<string, string> TemplateFile in TemplatesJSON)
                {
                    int AtLock = At;
                    At++;
                    try
                    {
                        LoadTemplate(false, TemplateFile, (Template) =>
                        {
                            if (Template != null)
                            {
                                AtLock++;
                                TemplateCallback(Template, TemplatesCount, AtLock);
                            }
                            else
                            {
                                TemplatesCount--;
                                TemplateCallback(null, TemplatesCount, AtLock);
                            }
                        });
                    }
                    catch
                    {
                        Console.WriteLine("Failed to load Adventure Templates");
                        TemplatesCount--;
                        TemplateCallback(null, TemplatesCount, AtLock);
                    }
                }
            }
            else
            {
                Console.WriteLine("Adventure Templates loaded");
            }
        }

        public static void LoadCachedContracts()
        {
            Console.WriteLine("Starting to load cached Adventures...");

            #region Load db Cached Contracts
            ILiteCollection<BsonDocument> DBCollection = null;
            lock (LiteDbService.DBCache)
            {
                DBCollection = LiteDbService.DBCache.Database.GetCollection("adventures_listed");
            }

            try
            {

#if DEBUG
#warning Clearing All Contract Cache on launch
                //DBCollection.DeleteAll();
#endif

                // Create a list
                List<BsonDocument> Cached = null;
                lock (LiteDbService.DBCache)
                {
                    Cached = DBCollection.FindAll().ToList();
                }


                int CtxCount = Cached.Count;
                int CtxAt = 0;
                //Parallel.ForEach(Cached, new ParallelOptions() { MaxDegreeOfParallelism = 100 }, (advCached) =>
                //{
                foreach (var advCached in Cached)
                {
                    long ID = (long)advCached["_id"];

                    if (MW.IsShuttingDown)
                    {
                        return;
                    }

                    if(IsDev)
                    {
                        if (CtxAt % 1000 == 0)
                        {
                            Console.WriteLine("Loaded " + CtxAt + " of " + CtxCount);
                        }
                        CtxAt++;
                    }
                    

                    try
                    {
                        if (!advCached.ContainsKey("json"))
                        {
                            lock (LiteDbService.DBCache)
                            {
                                DBCollection.Delete(ID);
                            }
                            continue;
                        }

                        var TemplateContent = JSSerializer.Deserialize<Dictionary<string, dynamic>>(advCached["json"]);
                        
                        string File = TemplateContent["FileName"];
                        DateTime Modified = DateTime.Parse(TemplateContent["ModifiedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                        
                        //Console.WriteLine("Loading " + ID + " on " + File);
                        AdventureTemplate Template = Templates.Find(x => x.ModifiedOn.Ticks == Modified.Ticks && x.FileName == File);
                        if (Template == null)
                        {
                            lock (LiteDbService.DBCache)
                            {
                                DBCollection.Delete(ID);
                            }
                            continue;
                        }

                        // Check if we are exceeding the max amount of contract from this template
                        if (Template.Instances < Template.ActiveAdventures.Count)
                        {
                            lock (LiteDbService.DBCache)
                            {
                                DBCollection.Delete(ID);
                            }
                            continue;
                        }

                        // Check if the name starts with _ and delete existing if true
                        if (Template.Name.StartsWith("_"))
                        {
                            lock (LiteDbService.DBCache)
                            {
                                DBCollection.Delete(ID);
                            }
                            continue;
                        }

                        if (Template.CheckValid())
                        {
                            Adventure Adv = Template.CreateAdventure((long)ID, (Dictionary<string, dynamic>)TemplateContent, false);
                            if (Adv != null)
                            {
                                lock (AllContracts)
                                {
                                    Adventure Existing = AllContracts.Find(x => x.ID == Adv.ID && x != Adv);
                                    if (Existing != null) { Existing.Remove(); }
                                    AllContracts.Add(Adv);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        lock (LiteDbService.DBCache)
                        {
                            DBCollection.Delete(ID);
                        }
                        Console.WriteLine("Failed to load cached Adventures: " + ex.Message + " - " + ex.StackTrace);
                    }

                }
                //});

                Console.WriteLine("Done loading cached Adventures...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load cache DB Adventures: " + ex.Message);
            }
            #endregion

        }

        public static void LoadPersistedContracts()
        {
            #region Db Library
            lock (LiteDbService.DB)
            {
                var DBCollection = LiteDbService.DB.Database.GetCollection("adventures_active");
                var Cached = DBCollection.FindAll();

                foreach (var advCached in Cached)
                {
                    if (MW.IsShuttingDown)
                    {
                        return;
                    }


                    try
                    {
                        var TemplateContent = JSSerializer.Deserialize<Dictionary<string, dynamic>>(JSSerializer.Serialize(BsonMapper.Global.Deserialize<Dictionary<string, dynamic>>(advCached)));

                        string File = TemplateContent["FileName"];
                        long ID = (long)TemplateContent["_id"];
                        DateTime Modified = DateTime.Parse(TemplateContent["ModifiedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                        Console.WriteLine("Loading " + ID + " on " + File);

                        AdventureTemplate Template = Templates.Find(x => x.ModifiedOn.Ticks == Modified.Ticks && x.FileName == File);
                        if (Template == null)
                        {
                            Dictionary<string, dynamic> Dict = null;
                            lock (LiteDbService.DB)
                            {
                                var DBCollection1 = LiteDbService.DB.Database.GetCollection("templates_active");
                                string Ticks = Modified.ToString("O");

                                var existing = DBCollection1.FindOne(x => x["File"] == File && x["ModifiedOn"] == Ticks);
                                Dict = JSSerializer.Deserialize<Dictionary<string, dynamic>>(JSSerializer.Serialize(BsonMapper.Global.Deserialize<Dictionary<string, dynamic>>(existing)));

                                if (Dict != null)
                                {
                                    Dict["RouteCode"] = "";
                                    if (UpgradeProcess.TemplateUpgrade(File, Dict))
                                    {
                                        DBCollection1.Update(Dict["_id"], BsonMapper.Global.ToDocument(Dict));
                                    }
                                }
                            }
                            if(Dict != null)
                            {
                                Template = new AdventureTemplate(File, Dict, false);
                                Templates.Add(Template);
                            }
                        }

                        if(Template != null)
                        {
                            try
                            {
                                // Delete contract if the template name starts with _
                                if (Template.Name.StartsWith("_"))
                                {
                                    DBCollection.Delete(ID);
                                    continue;
                                }
                                
                                Adventure Adv = Template.CreateAdventure((long)ID, (Dictionary<string, dynamic>)TemplateContent, false);

                                if (Adv != null)
                                {
                                    lock (AllContracts)
                                    {
                                        Adventure Existing = AllContracts.Find(x => x.ID == Adv.ID && x != Adv);
                                        if (Existing != null) { Existing.Remove(); }
                                        AllContracts.Add(Adv);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to load persisted Adventure: " + ex.Message + " - " + ex.StackTrace);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to load persisted Adventures Template: " + ex.Message + " - " + ex.StackTrace);
                    }
                }

                PersistenceRestored = true;
                APIBase.ClientCollection.SendState();
                Console.WriteLine("Done loading persisted Adventures...");
            }
            #endregion

            #region Load Legacy Persisted Contracts
            if (UserData.Get("tier") == "endeavour")
            {
                Dictionary<string, string> ContractsJSON = new Dictionary<string, string>();
                string Dir = Path.Combine(DocumentsDirectory, "Persistence");

                #region Gather files
                if (Directory.Exists(Dir))
                {
                    foreach (string ContractFile in Directory.GetFiles(Path.Combine(DocumentsDirectory, "Persistence"), "*.dat", SearchOption.AllDirectories))
                    {
                        FileInfo FI = new FileInfo(ContractFile);
                        try
                        {
                            if (!ContractsJSON.ContainsKey(FI.Name))
                            {
                                ContractsJSON.Add(FI.Name, Utils.Decrypt(File.ReadAllLines(ContractFile)[0], "4f86d7d7-f2d0-4c07-a178-4c5e2a8c8aa6"));
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                File.Delete(ContractFile);
                            }
                            catch
                            {
                            }
                            Console.WriteLine("Failed to load Contract " + FI.Name + " because " + ex.Message + " / " + ex.StackTrace);
                        }
                    }
                }
                #endregion

                #region Process all
                foreach (KeyValuePair<string, string> ContractFile in ContractsJSON)
                {
                    try
                    {
                        Dictionary<string, dynamic> TemplateContent = JSSerializer.Deserialize<Dictionary<string, dynamic>>(ContractFile.Value);
                        string File = TemplateContent["Template"]["File"];

                        lock (AllContracts)
                        {
                            if (AllContracts.Find(x => x.ID == Convert.ToInt64(TemplateContent["ID"])) != null)
                            {
                                continue;
                            }
                        }

                        Console.WriteLine("Loading " + ContractFile.Key + " on " + File);
                        DateTime Modified = DateTime.Parse(TemplateContent["Template"]["ModifiedOn"], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                        AdventureTemplate Template = Templates.Find(x => x.ModifiedOn.Ticks == Modified.Ticks && x.FileName == File);
                        if (Template == null)
                        {
                            Template = new AdventureTemplate(File.Replace(".p42adv", ""), (Dictionary<string, dynamic>)TemplateContent["Template"], false);
                        }
                        
                        // Delete contract if the template name starts with _
                        lock (LiteDbService.DB)
                        {
                            var DBCollection = LiteDbService.DB.Database.GetCollection("adventures_active");
                            if (Template.Name.StartsWith("_"))
                            {
                                DBCollection.Delete((long)Convert.ToInt64(TemplateContent["ID"]));
                                continue;
                            }
                        }
                        
                        if (TemplateContent["Persistence"].ContainsKey("State")) { TemplateContent.Add("State", (int)GetEnum(typeof(Adventure.AState), (string)TemplateContent["Persistence"]["State"])); }
                        if (TemplateContent["Persistence"].ContainsKey("Situations")) { TemplateContent.Add("Situations", TemplateContent["Persistence"]["Situations"]); }
                        if (TemplateContent["Persistence"].ContainsKey("Actions")) { TemplateContent.Add("Actions", TemplateContent["Persistence"]["Actions"]); }
                        if (TemplateContent["Persistence"].ContainsKey("Topo")) { TemplateContent.Add("Topo", TemplateContent["Persistence"]["Topo"]); }
                        if (TemplateContent["Persistence"].ContainsKey("Cleanedup")) { TemplateContent.Add("Cleanedup", TemplateContent["Persistence"]["Cleanedup"]); }
                        if (TemplateContent["Persistence"].ContainsKey("StartedAt")) { TemplateContent.Add("StartedAt", TemplateContent["Persistence"]["StartedAt"]); }
                        if (TemplateContent["Persistence"].ContainsKey("RequestedAt")) { TemplateContent.Add("RequestedAt", TemplateContent["Persistence"]["RequestedAt"]); }
                        if (TemplateContent["Persistence"].ContainsKey("CompletedAt")) { TemplateContent.Add("CompletedAt", TemplateContent["Persistence"]["CompletedAt"]); }
                        if (TemplateContent["Persistence"].ContainsKey("LastResumed")) { TemplateContent.Add("LastResumed", TemplateContent["Persistence"]["LastResumed"]); }
                        if (TemplateContent["Persistence"].ContainsKey("LastLocationGeo")) { TemplateContent.Add("LastLocationGeo", TemplateContent["Persistence"]["LastLocationGeo"]); }
                        if (TemplateContent["Persistence"].ContainsKey("Path")) { TemplateContent.Add("Path", TemplateContent["Persistence"]["Path"]); }
                        if (TemplateContent["Persistence"].ContainsKey("EndSummary")) { TemplateContent.Add("EndSummary", TemplateContent["Persistence"]["EndSummary"]); }

                        Adventure Adv = Template.CreateAdventure((long)Convert.ToInt64(TemplateContent["ID"]), (Dictionary<string, dynamic>)TemplateContent, false);
                        if (Adv != null)
                        {
                            lock (AllContracts)
                            {
                                Adventure Existing = AllContracts.Find(x => x.ID == Adv.ID);
                                AllContracts.Add(Adv);
                                if (Existing != null) { Existing.Remove(); }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to load Persisted Adventures: " + ex.Message + " - " + ex.StackTrace);
                    }
                }
                #endregion
            }
            #endregion
        }

        public static void ImportPersistence()
        {
            Task.Factory.StartNew(() =>
            {
                LoadTemplates((Template, Count, At) =>
                {
                    if (Template != null)
                    {
                        lock (Templates)
                        {
                            AdventureTemplate Existing = Templates.Find(x => x.FileName == Template.FileName && x.Activated);
                            if (Existing != null)
                            {
                                Existing.Unload();
                                Templates.Remove(Existing);
                            }
                            Templates.Add(Template);
                        }

                        Console.WriteLine("Loaded Templates: " + At + " / " + Count);

                        if (At == Count)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                UpgradeProcess.TemplatesValidation();
                                LoadPersistedContracts();

                                APIBase.ClientCollection.DataReady = true;

                                LoadCachedContracts();

                                RecalibrateSearchBias();

                                lock (Templates)
                                {
                                    int Requested = 0;
                                    int Completed = 0;

                                    foreach (AdventureTemplate T in Templates.FindAll(x => x.Activated))
                                    {
                                        Requested++;
                                        Thread lt = new Thread(() =>
                                        {
                                            if (T.Published || IsDev)
                                            {
                                                T.ImportRoutes();

                                                lock (T.ActiveAdventures)
                                                {
                                                    List<Adventure> adv = new List<Adventure>(T.ActiveAdventures);
                                                    foreach (var A in adv)
                                                    {
                                                        A.ValidateRoute();
                                                    }
                                                }

                                                T.Ready = true;
                                                T.PopulateAdventures(false, true, AircraftBias);
                                            }

                                            #region Send Notification
                                            //NotificationService.Add(new Notification()
                                            //{
                                            //    UID = 951,
                                            //    Title = "Contracts are generating... " + Math.Round((float)Completed / Requested * 100) + "%",
                                            //    Message = "You might get limited results while contracts are being generated by your system.",
                                            //    Type = NotificationType.Status,
                                            //    CanDismiss = false,
                                            //    IsTransponder = true,
                                            //});
                                            #endregion
                                            
                                            Completed++;
                                            if (Completed == Requested && !RestoreCompleted)
                                            {
                                                RestoreCompleted = true;
                                                Console.WriteLine("Persistence Loaded.");
                                                NotificationService.RemoveFromID((long)951);

                                                if(IsDev)
                                                {
                                                    var customs = Templates.FindAll(x => x.Activated && x.IsCustom);
                                                    List<Route> routes = new List<Route>();

                                                    foreach (var route in customs.Select(x => x.Routes))
                                                    {
                                                        routes.AddRange(route);
                                                    }

                                                    NotificationService.Add(new Notification()
                                                    {
                                                        Title = "Templates Summary",
                                                        Message = "Custom Templates: " + customs.Count + " \nCustom Routes: " + routes.Count()
                                                    });
                                                }

                                                StartValidityThread();

                                                //AdventuresCompile.ExportToNewFormat();
                                                //AdventuresCompile.ExportToWebsite();
                                            }
                                        });
                                        lt.IsBackground = true;
                                        lt.CurrentCulture = CultureInfo.CurrentCulture;
                                        lt.Start();
                                    }
                                }

                            }, ThreadCancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

                        }

                        Template.Loaded = true;
                    }
                    else
                    {
                        //UpgradeProcess.TemplatesValidation();
                        //LoadPersistedContracts();
                        //LoadCachedContracts();
                        //
                        //Console.WriteLine("Persistence Loaded.");
                        //NotificationService.RemoveFromID((long)951);
                        //StartValidityThread();
                        //RestoreCompleted = true;
                        //APIBase.ClientCollection.DataReady = true;
                    }

                });
            }, ThreadCancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            
        }



        public static Dictionary<string, List<Dictionary<string, dynamic>>> GetPayload()
        {

            Dictionary<string, List<Dictionary<string, dynamic>>> Payloads = new Dictionary<string, List<Dictionary<string, dynamic>>>()
            {
                { "meta", new List<Dictionary<string, dynamic>>() },
                { "state", new List<Dictionary<string, dynamic>>() },
            };

            GetAllListed(new Adventure.AState[] { Adventure.AState.Active }, (Adventure) =>
            {
                Payloads["meta"].Add(Adventure.GenBcastManifests(null));
                Payloads["state"].Add(Adventure.GenBcastManifestsState(null));
            });

            return Payloads;
        }

        public static void SetPayload(bool force = false)
        {
            if (SchedulePayloadUpdate || force)
            {
                SchedulePayloadUpdate = false;

                double cumulative = 0;
                GetAllLive((Adventure) =>
                {
                    cumulative += Adventure.GetLoadedPayloads();
                });

                int i = 0;
                while (i < SimConnection.Aircraft.PayloadStationCount)
                {
                    ActiveSim.Connector.SendPayload(i, Convert.ToSingle(cumulative / SimConnection.Aircraft.PayloadStationCount));
                    i++;
                }
            }
            
        }
        
    }

    public enum AState
    {
        [EnumValue("Listed")]
        Listed,
        [EnumValue("Saved")]
        Saved,
        [EnumValue("Active")]
        Active,
        [EnumValue("Failed")]
        Failed,
        [EnumValue("Succeeded")]
        Succeeded
    }
    
    public enum RouteSituationType
    {
        [EnumValue("Any")]
        Any,
        [EnumValue("Country")]
        Country,
        [EnumValue("ICAO")]
        ICAO,
        [EnumValue("Geo")]
        Geo,
        [EnumValue("Location")]
        Location,
        [EnumValue("Situation")]
        Situation,
        [EnumValue("AirlineRoutes")]
        AirlineRoutes
    }

}
