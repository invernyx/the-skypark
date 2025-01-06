using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Notifications;
using static TSP_Transponder.App;
using static TSP_Transponder.Models.Adventures.AdventureTemplate;
using static TSP_Transponder.Models.Airports.AirportsLib;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures
{
    public partial class RouteGenerator
    {
        internal bool IsCanceled = false;
        Action<double, string> StatusUpdate = (percent, text) => { };
        RouteGenParams Parameters = null;
        Dictionary<string, dynamic> Template = null;
        List<GeneratedRoute> Routes = new List<GeneratedRoute>();
        
        internal RouteGenerator(Dictionary<string, dynamic> Template, RouteGenParams Parameters, Action<double, string> StatusUpdate)
        {
            this.StatusUpdate = StatusUpdate;
            this.Parameters = Parameters;
            this.Template = Template;
        }

        public void Cancel()
        {
            IsCanceled = true;
        }

        internal List<GeneratedRoute> ReadRoutes()
        {
            return new List<GeneratedRoute>(Routes);
        }

        internal List<GeneratedRoute> Generate()
        {
            // Initiate the Situations array to be used as the reference
            ArrayList Situations = null;
            GeneratedRoute RouteBase = null;
            List<RouteSituation> KnownSituations = new List<RouteSituation>();

            try
            {
                // Create the Situations Ref Array
                Situations = ((ArrayList)Template["Situations"]);

                // Create the Base Route Reference
                RouteBase = new GeneratedRoute();

                // If there is less than 2 Situations, it's not a valid adventure, return empty
                if (Situations.Count < 2)
                {
                    return Routes;
                }

                // Make Route Situations
                foreach (Dictionary<string, dynamic> Situation in Situations)
                {
                    // Create the Situation for the Route
                    RouteSituation RS = MakeSituation(Situation);
                    RS.Index = RouteBase.Situations.Count;

                    // Keep track of Known Situations
                    if (RS.Location != null)
                    {
                        KnownSituations.Add(RS);
                    }

                    // Add Waypoint
                    RouteBase.Situations.Add(RS);
                }


                #region Required Airports
                List<KeyValuePair<Airport, List<bool>>> AirportListPerSituation = null;
                if (Parameters.RequireICAO != null)
                {
                    AirportListPerSituation = CheckFitsICAOS(RouteBase.Situations.Select(x => x.Location), Parameters.RequireICAO, true);

                    int SitIndex = 0;
                    foreach (var Sit in RouteBase.Situations)
                    {
                        if (AirportListPerSituation != null)
                        {
                            foreach (var m in AirportListPerSituation.FindAll(x => x.Value[SitIndex]))
                            {
                                Sit.Airport = m.Key;
                                Sit.Location = m.Key.Location;
                            }
                        }
                        SitIndex++;
                    }
                }
                #endregion

                #region Find Max Distance
                foreach (RouteSituation Sit in RouteBase.Situations)
                {
                    if(Sit.Index < RouteBase.Situations.Count - 1)
                    {
                        RouteSituation NextSit = RouteBase.Situations[Sit.Index + 1];
                        if (Sit.Location != null && NextSit.Location != null)
                        {
                            Parameters.MaxDistanceKM += (float)Utils.MapCalcDist(Sit.Location, NextSit.Location, Utils.DistanceUnit.Kilometers);
                        }
                        else
                        {
                            Parameters.MaxDistanceKM += Sit.DistToNextMaxKM;
                        }
                    }
                }
                #endregion

                #region List possible Airports
                if (AirportListPerSituation != null ? AirportListPerSituation.Count != RouteBase.Situations.Count : false)
                {
                    if (Parameters.IncludeICAO.Count == 0)
                    {
                        Parameters.PossibleAirports = Parameters.Sim.AirportsLib.GetAirportsCopy().FindAll(x => !x.IsClosed && !x.IsMilitary);
                    }
                    else
                    {
                        Parameters.PossibleAirports = Parameters.IncludeICAO;
                    }

                    // Remove what we know we don't want!
                    if (Parameters.ExcludeICAO.Count > 0)
                    {
                        Parameters.PossibleAirports = Parameters.PossibleAirports.FindAll(x => Parameters.ExcludeICAO.Find(x1 => x1 == x) == null);
                    }
                }
                else
                {
                    Parameters.PossibleAirports = Parameters.Sim.AirportsLib.GetAirportsCopy().FindAll(x => !x.IsClosed && !x.IsMilitary);
                }
                #endregion

                #region Start searching
                // If there are knowns, start from there
                if (KnownSituations.Count > 0)
                {
                    // Process knowns first and then propagate
                    Routes = FindRoutes(RouteBase, KnownSituations[0], KnownSituations, Parameters);
                    
                }
                // Nothing is known, start from null (first Sit.)
                else
                {
                    // Start from Nothing
                    Routes = FindRoutes(RouteBase, null, new List<RouteSituation>(), Parameters);
                }
                #endregion

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to generate Adventures from Template: " + ex.Message + " - " + ex.StackTrace);
            }

            return Routes;
        }

        internal static RouteSituation MakeSituation(Dictionary<string, dynamic> Situation)
        {
            // Convert the Dictionary to RouteSituation
            RouteSituation RouteSituation = new RouteSituation()
            {
                UID = Convert.ToUInt32(Situation["UID"]),
                Height = Convert.ToSingle(Situation["Height"]),
                DistToNextMaxKM = Convert.ToSingle(Situation["DistToNextMax"]) * 1.852f, //Convert.ToSingle(Utils.Limiter(0, 15343.49, Convert.ToDouble(Situation["DistToNextMax"]) * 1.852)),
                DistToNextMinKM = Convert.ToSingle(Situation["DistToNextMin"]) * 1.852f,
            };
            
            if (Situation.ContainsKey("Boundaries"))
            {
                foreach (var Bound in Situation["Boundaries"])
                {
                    RouteSituation.Boundaries.Add(new GeoLoc(Convert.ToDouble(Bound[0]), Convert.ToDouble(Bound[1])));
                }
            }

            if (Situation.ContainsKey("Query"))
            {
                RouteSituation.Query = Situation["Query"];
            }

            if (Situation.ContainsKey("RequireLights"))
            {
                RouteSituation.RequiresLight = Situation["RequireLights"];
            }
            
            switch ((string)Situation["SituationType"])
            {
                case "Any":
                case "Country":
                    {
                        RouteSituation.Surface = Situation["Surface"];
                        RouteSituation.HeliMin = Situation["HeliMin"];
                        RouteSituation.HeliMax = Situation["HeliMax"];
                        RouteSituation.RwyMin = Situation["RwyMin"];
                        RouteSituation.RwyMax = Situation["RwyMax"];
                        RouteSituation.ParkMin = Situation["ParkMin"];
                        RouteSituation.ParkMax = Situation["ParkMax"];
                        RouteSituation.ParkWidMin = Situation["ParkWidMin"];
                        RouteSituation.ParkWidMax = Situation["ParkWidMax"];
                        RouteSituation.RwyLenMin = Situation["RwyLenMin"];
                        RouteSituation.RwyLenMax = Situation["RwyLenMax"];
                        RouteSituation.RwyWidMin = Situation["RwyWidMin"];
                        RouteSituation.RwyWidMax = Situation["RwyWidMax"];
                        RouteSituation.ElevMin = Situation["ElevMin"];
                        RouteSituation.ElevMax = Situation["ElevMax"];
                        break;
                    }
            }
            switch (Situation["SituationType"])
            {
                case "Any":
                    {
                        RouteSituation.SituationType = RouteSituationType.Any;
                        break;
                    }
                case "Country":
                    {
                        RouteSituation.SituationType = RouteSituationType.AnyInCountry;
                        RouteSituation.Country = (string)Situation["Country"];
                        break;
                    }
                case "ICAO":
                    {
                        RouteSituation.SituationType = RouteSituationType.ICAO;
                        string[] ICAOs = ((string)Situation["ICAO"]).Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if(ICAOs.Length > 1)
                        {
                            RouteSituation.PossibleAirports = new List<Airport>();
                            foreach (var ICAO in ICAOs)
                            {
                                RouteSituation.PossibleAirports.Add(SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO));
                            }
                        }
                        else
                        {
                            RouteSituation.Airport = SimLibrary.SimList[0].AirportsLib.GetByICAO((string)Situation["ICAO"]);
                            if (RouteSituation.Airport != null)
                            {
                                RouteSituation.Location = RouteSituation.Airport.Location;
                            }
                        }
                        break;
                    }
                case "Geo":
                    {
                        RouteSituation.SituationType = RouteSituationType.Geo;
                        RouteSituation.Location = new GeoLoc(Convert.ToDouble(Situation["Lon"]), Convert.ToDouble(Situation["Lat"]));
                        RouteSituation.TriggerRange = Convert.ToSingle(Situation["TriggerRange"]);
                        break;
                    }
            }

            return RouteSituation;
        }

        internal static List<KeyValuePair<Airport, List<bool>>> CheckFitsICAOS(IEnumerable<GeoLoc> RouteSits, List<Airport> RequireICAO, bool FalseKnown)
        {
            List<GeoLoc> RouteSitsL = RouteSits.ToList();
            List<KeyValuePair<Airport, List<bool>>> AirportListPerSituation = new List<KeyValuePair<Airport, List<bool>>>();
            
            int AirportIndex = 0;
            //List<Airport> ActualReqICAOs = Params.RequireICAO.FindAll(x => x != null);
            foreach (Airport Apt in RequireICAO)
            {
                // loop all airports
                if (Apt != null)
                {
                    KeyValuePair<Airport, List<bool>> AptKVP = new KeyValuePair<Airport, List<bool>>(Apt, new List<bool>());

                    // loop all situations
                    int SituationIndex = 0;
                    foreach (GeoLoc Sit in RouteSitsL)
                    {
                        // Check if known
                        if (Sit != null && FalseKnown)
                        {
                            AptKVP.Value.Add(false);
                            SituationIndex++;
                            continue;
                        }

                        if (SituationIndex == 0) // First Situation
                        {
                            if (AirportIndex > 0) // Check if current airport is arrival
                            {
                                if (RequireICAO[AirportIndex - 1] == null) // Is arrival
                                {
                                    AptKVP.Value.Add(false);
                                    SituationIndex++;
                                    continue;
                                }
                            }
                        }

                        if (SituationIndex > 0) // Last Situation
                        {
                            if (AirportIndex < RequireICAO.Count - 1) // Check if current airport is departure
                            {
                                if (RequireICAO[AirportIndex + 1] == null) // Is departure
                                {
                                    AptKVP.Value.Add(false);
                                    SituationIndex++;
                                    continue;
                                }
                            }
                        }

                        AptKVP.Value.Add(true);
                        SituationIndex++;
                        continue;
                    }

                    AirportListPerSituation.Add(AptKVP);
                }
                AirportIndex++;
            }

            return AirportListPerSituation;
        }

        private List<GeneratedRoute> FindRoutes(GeneratedRoute RouteBase, RouteSituation From, List<RouteSituation> Knowns, RouteGenParams Params)
        {
            if (IsDev)
            {
                NotificationService.Add(new Notification()
                {
                    UID = 9503 + Params.UID,
                    Title = "Routes are generating for " + Params.Name,
                    Message = "Getting possible airports...",
                    Type = NotificationType.Status,
                    CanDismiss = false,
                    IsTransponder = true,
                });
            }

            // Init Available Routes
            List<GeneratedRoute> Routes = new List<GeneratedRoute>();
            int? FilterMinRangeKM = null;
            int? FilterMaxRangeKM = null;

            // Check if we have filters and that the distances are possible, return if not
            if (Params.Filters != null)
            {
                float TemplateMinRange = RouteBase.Situations.Sum(x => x.DistToNextMinKM);
                float TemplateMaxRange = RouteBase.Situations.Sum(x => x.DistToNextMaxKM);

                FilterMinRangeKM = (int)(Params.Filters.Range[0] * 1.852f);
                FilterMaxRangeKM = (int)(Params.Filters.Range[1] * 1.852f);

                if(FilterMinRangeKM > TemplateMaxRange || FilterMaxRangeKM < TemplateMinRange)
                {
                    return Routes;
                }
            }
            
            // Find all our Possible Airports from Known Airports
            foreach (RouteSituation Sit in RouteBase.Situations)
            {
                if (Sit.Location != null)
                {
                    int SitIndex = RouteBase.Situations.IndexOf(Sit);

                    // Look for Next
                    RouteSituation NextSit = null;
                    if(SitIndex < RouteBase.Situations.Count - 1)
                    {
                        NextSit = RouteBase.Situations[SitIndex + 1];
                        if (NextSit.Location == null && NextSit.PossibleAirports == null)
                        {
                            int DistMin = (int)(Sit.DistToNextMinKM);
                            int DistMax = (int)(Sit.DistToNextMaxKM);
                            if (FilterMinRangeKM != null && FilterMaxRangeKM != null)
                            {
                                DistMin = (int)((int)FilterMinRangeKM > Sit.DistToNextMinKM ? (int)FilterMinRangeKM : Sit.DistToNextMinKM);
                                DistMax = (int)((int)FilterMaxRangeKM > Sit.DistToNextMaxKM ? Sit.DistToNextMaxKM : (int)FilterMaxRangeKM);
                            }

                            NextSit.PossibleAirports = new List<Airport>(Params.PossibleAirports);
                            NextSit.PossibleAirports = FilterAirportsDistance(NextSit.PossibleAirports, Sit.Location, DistMin, DistMax);
                            NextSit.PossibleAirports = FilterAirportsMeta(NextSit.PossibleAirports, Params, NextSit);
                            NextSit.PossibleAirports = FilterAirportsBounds(NextSit.PossibleAirports, NextSit);
                        }
                    }

                    // Look for Previous
                    RouteSituation PrevSit = null;
                    if (SitIndex > 0)
                    {
                        PrevSit = RouteBase.Situations[SitIndex - 1];
                        if (PrevSit.Location == null && PrevSit.PossibleAirports == null)
                        {
                            int DistMin = (int)(PrevSit.DistToNextMinKM);
                            int DistMax = (int)(PrevSit.DistToNextMaxKM);
                            if (FilterMinRangeKM != null && FilterMaxRangeKM != null)
                            {
                                DistMin = (int)((int)FilterMinRangeKM > PrevSit.DistToNextMinKM ? (int)FilterMinRangeKM : PrevSit.DistToNextMinKM);
                                DistMax = (int)((int)FilterMaxRangeKM > PrevSit.DistToNextMaxKM ? PrevSit.DistToNextMaxKM : (int)FilterMaxRangeKM);
                            }

                            PrevSit.PossibleAirports = new List<Airport>(Params.PossibleAirports);
                            PrevSit.PossibleAirports = FilterAirportsDistance(PrevSit.PossibleAirports, Sit.Location, DistMin, DistMax);
                            PrevSit.PossibleAirports = FilterAirportsMeta(PrevSit.PossibleAirports, Params, PrevSit);
                            PrevSit.PossibleAirports = FilterAirportsBounds(PrevSit.PossibleAirports, PrevSit);
                        }
                    }
                }
                else
                {

                }
            }

            return ExpandRoutes(RouteBase, Params);
        }
        
        private List<GeneratedRoute> ExpandRoutes(GeneratedRoute RouteBase, RouteGenParams Params)
        {
            List<string> RoutesString = new List<string>();
            List<GeneratedRoute> Routes = new List<GeneratedRoute>();

            //double AirportArrivalAverage = 0;
            //double AirportDepartureAverage = 0;
            Dictionary<Airport, int> AirportArrivalCount = new Dictionary<Airport, int>();
            Dictionary<Airport, int> AirportDepartureCount = new Dictionary<Airport, int>();
            Dictionary<Airport, int> AirportRejectedCount = new Dictionary<Airport, int>();

            // Set default limit
            int Limit = Parameters.Limit;
            if(Limit < 0)
            {
                Limit = 1000;
            }

            float MinDist = -1;
            float MaxDist = -1;
            if (Params.Filters != null)
            {
                MinDist = Params.Filters.Range[0] * 1.852f;
                MaxDist = Params.Filters.Range[1] * 1.852f;
            }

            #region Init Departures Count
            foreach(var Aptt in Params.PossibleAirports)
            {
                if(!AirportDepartureCount.ContainsKey(Aptt))
                {
                    AirportDepartureCount.Add(Aptt, 0);
                    AirportArrivalCount.Add(Aptt, 0);
                    AirportRejectedCount.Add(Aptt, 0);
                }
            }
            #endregion

            // Generate Routes
            object LastStatusLock = new object();
            DateTime LastStatus = DateTime.UtcNow;
            int LastStatusCount = 0;
            int LimitStop = (int)(Limit * 0.3);
            int LimitTipping = (int)(LimitStop * 0.5);
            int Pass = 0;
            int StartedThreads = 0;
            object StartedThreadsObj = new object();


            Action Run = null;
            Run = () =>
            {
                while(Routes.Count < Limit && Pass <= LimitStop && !IsCanceled && !MW.IsShuttingDown)
                {
                    // Create Instance Route from Base Route
                    GeneratedRoute Route = new GeneratedRoute(RouteBase);
                    bool IsImpossible = false;
                    int FirstLegHeading = 0;

                    #region Notify loop
                    lock (LastStatusLock)
                    {
                        if (DateTime.UtcNow - LastStatus > TimeSpan.FromMilliseconds(1000))
                        {
                            if (IsDev)
                            {
                                double RoutesPerSecond = ((Routes.Count - LastStatusCount) / (DateTime.UtcNow - LastStatus).TotalSeconds) * 60;
                                NotificationService.Add(new Notification()
                                {
                                    UID = 9503 + Params.UID,
                                    Title = "Routes are generating for " + Params.Name,
                                    Message = Math.Round(((float)Routes.Count / Limit) * 100f, 2).ToString("0.00", CultureInfo.InvariantCulture) + "% - " + Routes.Count + "/" + Limit + " - " + (RoutesPerSecond > 0 ? Math.Round(RoutesPerSecond) : RoutesPerSecond) + "rpm",
                                    Type = NotificationType.Status,
                                    CanDismiss = false,
                                    IsTransponder = true,
                                });
                            }

                            LastStatus = DateTime.UtcNow;
                            LastStatusCount = Routes.Count;
                        }
                    }
                    #endregion

                    #region Create Params
                    // Params for this Route
                    RouteGenParams RteParams = new RouteGenParams(Params);
                    int SitIndex = 0;
                    #endregion

                    #region Each situations
                    // Choose Airports and create the final route
                    // Find what's missing on the way there
                    foreach (RouteSituation Sit in Route.Situations)
                    {
                        if (IsImpossible)
                        {
                            break;
                        }
                        
                        RouteSituation PrevSit = SitIndex > 0 ? Route.Situations[SitIndex - 1] : null;

                        if (Sit.Location == null)
                        {
                            Airport Apt = null;

                            #region Filter Possible Airports
                            if (Sit.PossibleAirports == null)
                            {
                                Sit.PossibleAirports = new List<Airport>(RteParams.PossibleAirports);

                                lock (Sit.PossibleAirports)
                                {
                                    if (SitIndex > 0)
                                    {
                                        // Find new airport from the previous chosen airport
                                        Sit.PossibleAirports = FilterAirportsDistance(Sit.PossibleAirports, PrevSit.Location, (int)PrevSit.DistToNextMinKM, (int)PrevSit.DistToNextMaxKM);
                                    }

                                    // Filter the already watered down possible airports for that particlular situation
                                    Sit.PossibleAirports = FilterAirportsMeta(Sit.PossibleAirports, Params, Sit);
                                    Sit.PossibleAirports = FilterAirportsBounds(Sit.PossibleAirports, Sit);
                                }
                            }
                            #endregion

                            lock (Sit.PossibleAirports)
                            {
                                #region Pick an airport at random
                                if (Sit.PossibleAirports.Count > 0)
                                {
                                    if (Pass <= LimitTipping)
                                    {
                                        if (Sit.PossibleAirports.Count > 1)
                                        {
                                            if (SitIndex == 0) // Departure
                                            {
                                                if (Routes.Count < Limit * 0.5f)
                                                {
                                                    int i = Utils.GetRandom(Sit.PossibleAirports.Count);
                                                    Apt = Sit.PossibleAirports[i];
                                                }
                                                else
                                                {
                                                    #region Weighted Results
                                                    WeightedRandom<Airport> DepartureCounts = new WeightedRandom<Airport>();
                                                    lock (AirportArrivalCount)
                                                    {
                                                        lock (AirportDepartureCount)
                                                        {
                                                            int total_count = Sit.PossibleAirports.Count;
                                                            foreach (var Apt1 in Sit.PossibleAirports)
                                                            {
                                                                //int InOutCount = 1 + AirportArrivalCount[Apt1] + AirportDepartureCount[Apt1];
                                                                //double Factor = 100 + ((AirportArrivalCount[Apt1] * (20 / InOutCount)) - AirportDepartureCount[Apt1] - (AirportRejectedCount[Apt1] * 5)) + Apt1.Radius; // - ((float)Apt1.Density / 80)
                                                                double Factor = 100 + ((AirportArrivalCount[Apt1] * total_count) - (AirportDepartureCount[Apt1] * total_count) - (AirportRejectedCount[Apt1] * total_count * 5));
                                                                DepartureCounts.AddEntry(Apt1, Factor < 1 ? 1 : Factor);
                                                            }
                                                            Apt = DepartureCounts.GetRandom();
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                            else if (SitIndex == Route.Situations.Count - 1) // Arrival
                                            {
                                                #region Prevent return routes
                                                List<GeneratedRoute> parallelRoutes = null;
                                                lock (Routes)
                                                {
                                                    parallelRoutes = Routes.FindAll(x =>
                                                    {
                                                        if (x.Situations[0].Airport != null)
                                                        {
                                                            if (Route.Situations[0].Airport == x.Situations[x.Situations.Count - 1].Airport)
                                                            {
                                                                return true;
                                                            }
                                                        }
                                                        return false;
                                                    });
                                                }

                                                if (parallelRoutes.Count > 0)
                                                {
                                                    foreach (var rte in parallelRoutes)
                                                    {
                                                        lock (Sit.PossibleAirports)
                                                        {
                                                            Sit.PossibleAirports.Remove(rte.Situations[0].Airport);
                                                        }
                                                    }
                                                }
                                                #endregion

                                                #region Weighted Results
                                                WeightedRandom<Airport> DepartureCounts = new WeightedRandom<Airport>();
                                                lock (Sit.PossibleAirports)
                                                {
                                                    foreach (var Apt1 in Sit.PossibleAirports)
                                                    {
                                                        double headingDifferential = 0;
                                                        if (SitIndex > 1)
                                                        {
                                                            double newHeading = Math.Round(Utils.MapCalcBearing(PrevSit.Location, Apt1.Location));
                                                            double diff = Math.Abs(Utils.MapCompareBearings(FirstLegHeading, newHeading));
                                                            if (diff > 90)
                                                            {
                                                                continue;
                                                            }
                                                            headingDifferential = Math.Pow(90 - diff, 3);
                                                        }

                                                        lock (AirportRejectedCount)
                                                        {
                                                            lock (AirportArrivalCount)
                                                            {
                                                                lock (AirportDepartureCount)
                                                                {
                                                                    double Factor = 100 + ((AirportDepartureCount[Apt1] * 20) - AirportArrivalCount[Apt1] - (AirportRejectedCount[Apt1] * 5) + headingDifferential);
                                                                    DepartureCounts.AddEntry(Apt1, Factor < 1 ? 1 : Factor);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                Apt = DepartureCounts.GetRandom();
                                                #endregion
                                            }
                                            else if (SitIndex > 1) // Everything else
                                            {
                                                #region Weighted Results
                                                WeightedRandom<Airport> TargetCounts = new WeightedRandom<Airport>();
                                                foreach (var Apt1 in Sit.PossibleAirports)
                                                {
                                                    double newHeading = Math.Round(Utils.MapCalcBearing(PrevSit.Location, Apt1.Location));
                                                    double diff = Math.Abs(Utils.MapCompareBearings(FirstLegHeading, newHeading));
                                                    if (diff > 90)
                                                    {
                                                        continue;
                                                    }
                                                    double headingDifferential = Math.Pow(90 - diff, 3);

                                                    double Factor = 100 + headingDifferential;
                                                    TargetCounts.AddEntry(Apt1, Factor < 1 ? 1 : Factor);
                                                }
                                                Apt = TargetCounts.GetRandom();
                                                #endregion
                                            }
                                            else // 2nd airport
                                            {
                                                int i = Utils.GetRandom(Sit.PossibleAirports.Count);
                                                Apt = Sit.PossibleAirports[i];
                                            }
                                        }
                                        else
                                        {
                                            Apt = Sit.PossibleAirports[0];
                                        }

                                    }
                                    else
                                    {
                                        int i = Utils.GetRandom(Sit.PossibleAirports.Count);
                                        Apt = Sit.PossibleAirports[i];
                                    }
                                }
                                #endregion
                            }

                            if (Apt != null)
                            {
                                Sit.Location = Apt.Location;
                                Sit.Airport = Apt;
                                RteParams.PossibleAirports.Remove(Apt);
                                if (SitIndex == 0)
                                {
                                    RteParams.PossibleAirports = FilterAirportsDistance(RteParams.PossibleAirports, Sit.Location, 0, (int)(MaxDist > -1 ? MaxDist : Parameters.MaxDistanceKM));
                                }
                            }
                            else
                            {
                                IsImpossible = true;
                                //if (SitIndex > 0)
                                //{
                                //    AirportRejectedCount[Route.Situations[0].Airport] = (int)((AirportRejectedCount[Route.Situations[0].Airport] + 1) * 1.2);
                                //}
                            }

                            if (SitIndex == 1 && Sit.Location != null)
                            {
                                FirstLegHeading = (int)Math.Round(Utils.MapCalcBearing(Route.Situations[0].Location, Sit.Location));
                            }

                            #region Check if previous leg is possible
                            if (SitIndex > 0 && !IsImpossible)
                            {
                                switch (Sit.SituationType)
                                {
                                    case RouteSituationType.Any:
                                    case RouteSituationType.AnyInCountry:
                                    case RouteSituationType.ICAO:
                                        {
                                            switch (PrevSit.SituationType)
                                            {
                                                case RouteSituationType.Any:
                                                case RouteSituationType.AnyInCountry:
                                                case RouteSituationType.ICAO:
                                                    {
                                                        // Don't check if both Previous and Current Sit are known
                                                        if (PrevSit.SituationType == Sit.SituationType && (Sit.SituationType == RouteSituationType.ICAO || Sit.SituationType == RouteSituationType.Geo))
                                                        {
                                                            break;
                                                        }

                                                        // Check the distance between Previous and Current Sit
                                                        double Dist = Utils.MapCalcDist(PrevSit.Location, Sit.Location, Utils.DistanceUnit.Kilometers, true);
                                                        if (Dist < PrevSit.DistToNextMinKM || Dist > PrevSit.DistToNextMaxKM)
                                                        {
                                                            IsImpossible = true;
                                                            //Console.WriteLine("Impossible route found (" + PrevSit.Airport.ICAO + " / " + Sit.Airport.ICAO + " at " + Dist);
                                                            break;
                                                        }
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case RouteSituationType.Geo:
                                        {
                                            break;
                                        }

                                }

                            }
                            #endregion
                        }

                        SitIndex++;
                    }
                    #endregion
                    
                    #region Validate if the route already exists
                    bool Add = false;
                    if (!IsImpossible)
                    {
                        Route.CalculateDistance();
                        string NewRouteString = Route.ToString();
                        lock (RoutesString)
                        {
                            if (!RoutesString.Contains(NewRouteString))
                            {
                                RoutesString.Add(NewRouteString);

                                #region Check Aircraft recommendations
                                Route.GenerateRecommendedAircraft();
                                if (Route.RecommendedAircraft.Max() < 10 && Route.Situations.Find(x => x.Airport != null) != null)
                                {
                                    IsImpossible = true;
                                    Pass++;
                                }
                                else
                                {
                                    // Check if we have filters and that the distances are possible, skip if not
                                    if (Params.Filters != null)
                                    {
                                        float FilterMinRange = Params.Filters.Range[0] * 1.852f;
                                        float FilterMaxRange = Params.Filters.Range[1] * 1.852f;
                                        if (FilterMinRange < Route.Distance && FilterMaxRange > Route.Distance)
                                        {
                                            Add = true;
                                        }
                                    }
                                    else
                                    {
                                        Add = true;
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                Pass++;
                            }
                        }
                    }
                    else
                    {
                        Pass++;
                    }
                    #endregion

                    #region Add route
                    if (Add)
                    {
                        //Console.WriteLine("Route " + Route.ToString() + " / " + ((double)Routes.Count / Limit));
                        if (Route.Distance == 0)
                        {
                            Route.CalculateDistance();
                        }

                        lock (Routes)
                        {
                            Routes.Add(Route);
                        }

                        if (Parameters.Callback != null)
                        {
                            lock (Parameters.Callback)
                            {
                                Parameters.Callback(Route);
                            }
                        }

                        Pass = Pass > LimitTipping ? LimitTipping : 0;


                        Airport Dep = Route.Situations[0].Airport;
                        Airport Dest = Route.Situations[Route.Situations.Count - 1].Airport;

                        lock (AirportDepartureCount)
                        {
                            if (Dep != null)
                            {
                                if (!AirportDepartureCount.ContainsKey(Dep))
                                {
                                    AirportDepartureCount.Add(Dep, 1);
                                }
                                else
                                {
                                    AirportDepartureCount[Dep]++;
                                }
                            }
                            //AirportDepartureAverage = AirportDepartureCount.Select(x => x.Value).ToList().Average();
                        }

                        lock (AirportArrivalCount)
                        {
                            if (Dest != null)
                            {
                                if(!AirportArrivalCount.ContainsKey(Dest))
                                {
                                    AirportArrivalCount.Add(Dest, 1);
                                }
                                else
                                {
                                    AirportArrivalCount[Dest]++;
                                }
                            }
                            //AirportArrivalAverage = AirportArrivalCount.Select(x => x.Value).ToList().Average();
                        }
                    }
                    else if (Route.Situations[0].Airport != null)
                    {
                        lock (AirportRejectedCount)
                        {
                            if (!AirportRejectedCount.ContainsKey(Route.Situations[0].Airport))
                            {
                                AirportRejectedCount.Add(Route.Situations[0].Airport, 0);
                            }
                            AirportRejectedCount[Route.Situations[0].Airport] = (int)((AirportRejectedCount[Route.Situations[0].Airport] + 1) * 1.2);
                        }
                    }
                    #endregion

                }

                lock(StartedThreadsObj)
                {
                    StartedThreads--;
                }
            };

            int ThreadsCount = 0;
            while (ThreadsCount < 10)
            {
                Thread nrt = new Thread(() => {
                    Run();
                });
                nrt.IsBackground = true;
                nrt.Start();
                lock (StartedThreadsObj)
                {
                    StartedThreads++;
                }
                ThreadsCount++;
            }

            while(StartedThreads > 0 || !AdventuresBase.PersistenceRestored)
            {
                Thread.Sleep(1000);
            }
            
            if (IsDev)
            {
                NotificationService.RemoveFromID(9503 + Params.UID);
            }

            return Routes;
        }

        internal static List<Airport> FilterAirportsMeta(List<Airport> SelectedAirports, RouteGenParams Params, RouteSituation Sit)
        {
            SelectedAirports = new List<Airport>(SelectedAirports);

            // Template limits
            #region Filter for Template Countries
            if(Sit.Country != null)
            {
                SelectedAirports = SelectedAirports.FindAll(x =>
                {
                    return x.Country == Sit.Country;
                });
            }
            #endregion

            #region Filter from Template Search Query
            if (Sit.Query.Trim() != "")
            {
                SelectedAirports = SelectedAirports.FindAll(x =>
                {
                    return x.Name.ToLower().Contains(Sit.Query.ToLower());
                });
            }
            #endregion

            #region Apply Filters for Template Runways
            List<Surface> FilterSurfaceTypes = new List<Surface>();
            switch (Sit.Surface)
            {
                default:
                    {
                        break;
                    }
                case "Soft":
                    {
                        FilterSurfaceTypes.Add(Surface.Grass);
                        FilterSurfaceTypes.Add(Surface.Gravel);
                        FilterSurfaceTypes.Add(Surface.Sand);
                        FilterSurfaceTypes.Add(Surface.Clay);
                        FilterSurfaceTypes.Add(Surface.Snow);
                        FilterSurfaceTypes.Add(Surface.Dirt);
                        break;
                    }
                case "Hard":
                    {
                        FilterSurfaceTypes.Add(Surface.Asphalt);
                        FilterSurfaceTypes.Add(Surface.Concrete);
                        FilterSurfaceTypes.Add(Surface.Bituminous);
                        FilterSurfaceTypes.Add(Surface.Tarmac);
                        FilterSurfaceTypes.Add(Surface.Ice);
                        break;
                    }
                case "Dirt":
                    {
                        FilterSurfaceTypes.Add(Surface.Dirt);
                        break;
                    }
                case "Gravel":
                    {
                        FilterSurfaceTypes.Add(Surface.Gravel);
                        FilterSurfaceTypes.Add(Surface.Sand);
                        FilterSurfaceTypes.Add(Surface.Clay);
                        break;
                    }
                case "Grass":
                    {
                        FilterSurfaceTypes.Add(Surface.Grass);
                        break;
                    }
                case "Water":
                    {
                        FilterSurfaceTypes.Add(Surface.Water);
                        break;
                    }
            }

            SelectedAirports = SelectedAirports.FindAll(x =>
            {
                if (x.Runways.Count >= Sit.RwyMin && x.Runways.Count <= Sit.RwyMax)
                {
                    bool HasMetaMatch = false;
                    bool FoundLight = !Sit.RequiresLight;
                    foreach (Airport.Runway rwy in x.Runways)
                    {
                        if(Sit.RequiresLight && (rwy.CenterLight > 0 || rwy.EdgeLight > 0))
                        {
                            FoundLight = true;
                        }

                        if ((FilterSurfaceTypes.Count > 0 ? FilterSurfaceTypes.Contains(rwy.Surface) : true)
                        && rwy.LengthFT >= Sit.RwyLenMin
                        && rwy.LengthFT <= Sit.RwyLenMax
                        && rwy.WidthMeters >= Sit.RwyWidMin
                        && rwy.WidthMeters <= Sit.RwyWidMax
                        && rwy.AltitudeFeet >= Sit.ElevMin
                        && rwy.AltitudeFeet <= Sit.ElevMax)
                        {
                            HasMetaMatch = true;
                        }
                    }

                    return HasMetaMatch && FoundLight;
                }
                return false;
            });
            #endregion

            #region Apply Filters for Template Parkings
            SelectedAirports = SelectedAirports.FindAll(x =>
            {
                bool Valid = true;

                if (x.Parkings.Count < Sit.ParkMin || x.Parkings.Count > Sit.ParkMax)
                {
                    Valid = false;
                }

                if (x.Parkings.Count > 0)
                {
                    // Min
                    if (x.Parkings.Find(pk => pk.Diameter * 3.28084f > Sit.ParkWidMin) == null)
                    {
                        Valid = false;
                    }

                    // Max
                    if (x.Parkings.Find(pk => pk.Diameter * 3.28084f < Sit.ParkWidMax) == null)
                    {
                        Valid = false;
                    }
                }

                return Valid;
            });
            #endregion

            #region Apply Filters for Template Names
            // Apply Filters to exlude Some Names
            List<string> ExcludedNames = new List<string>()
            {
                "Air Base",
                " AFB",
                " AB",
                "Fliegerhorst",
                "close",
                "Close",
                "Ignore"
            };
            SelectedAirports = SelectedAirports.FindAll(x =>
            {
                foreach (string Name in ExcludedNames)
                {
                    if (x.Name.Contains(Name))
                    {
                        return false;
                    }
                }

                return true;
            });
            #endregion

            // User Limits
            #region Filter for User Runways
            if (Params.Filters != null)
            {
                #region Runways
                SelectedAirports = new AirportFilters.FilterRunways()
                {
                    List = SelectedAirports,
                    CountMin = Params.Filters.RwyCount[0],
                    CountMax = Params.Filters.RwyCount[1],
                    LengthMin = Params.Filters.RwyLength[0],
                    LengthMax = Params.Filters.RwyLength[1],
                    RwySurface = Params.Filters.RwySurface,
                }.Get();
                #endregion
                
            }
            #endregion
            
            return SelectedAirports;
        }

        internal static List<Airport> FilterAirportsDistance(List<Airport> SelectedAirports, GeoLoc Location, int MinKM, int MaxKM)
        {
            //const float LatitudeDistanceM = 111100;
            //float LimitDist = (float)Max * 1000;
            //float LatRange = LimitDist / LatitudeDistanceM;
            //double North = Location.Lat + LatRange;
            //double South = Location.Lat - LatRange;
            //GeoLoc West = Utils.MapOffsetPosition(Location, LimitDist, 270);
            //GeoLoc East = Utils.MapOffsetPosition(Location, LimitDist, 90);

            //List<Airport> Boxed = SelectedAirports.FindAll(x =>
            //{
            //    if(x.Location.Lon > West.Lon && x.Location.Lon < East.Lon)
            //    {
            //        if (x.Location.Lat > South && x.Location.Lat < North)
            //        {
            //            return true;
            //        }
            //    }
            //    return false;
            //});

            //List<Airport> Sel = Boxed.FindAll(x =>
            //{
            //    float Dist = (float)Utils.MapCalcDist(Location, x.Location, Utils.DistanceUnit.Kilometers, true);
            //    if (Dist > Min && Dist < Max)
            //    {
            //        return true;
            //    }
            //    return false;
            //});

            float LatitudeRangeN = (float)Utils.MapOffsetPosition(Location, MaxKM * 1000, 0).Lat;
            float LatitudeRangeS = (float)Utils.MapOffsetPosition(Location, MaxKM * 1000, 180).Lat;

            List<Airport> SelTot = SelectedAirports.FindAll(x =>
            {
                float Lat = (float)x.Location.Lat;
                if (LatitudeRangeN > Lat && LatitudeRangeS < Lat)
                {
                    float Dist = (float)Utils.MapCalcDist(Location, x.Location, Utils.DistanceUnit.Kilometers, true);
                    if (Dist > MinKM && Dist < MaxKM)
                    {
                        return true;
                    }
                }
                return false;
            });

            return SelTot;
        }

        internal static List<Airport> FilterAirportsBounds(List<Airport> SelectedAirports, RouteSituation Sit)
        {
            if(Sit.Boundaries.Count > 2)
            {
                List<Airport> InBounds = SelectedAirports.FindAll(x =>
                {
                    return Utils.IsPointInPoly(Sit.Boundaries, x.Location);
                });
                return InBounds;
            } else
            {
                return SelectedAirports;
            }
        }


        public class GeneratedRoute
        {
            public List<sbyte> RecommendedAircraft = null;
            public List<RouteSituation> Situations = new List<RouteSituation>();
            public double Distance = 0;

            public GeneratedRoute()
            {
            }

            public GeneratedRoute(GeneratedRoute Base)
            {
                foreach (RouteSituation wp in Base.Situations)
                {
                    Situations.Add(wp.Copy());
                }
                Distance = Base.Distance;
            }
            
            public double CalculateDistance()
            {
                int Index = 0;
                foreach (RouteSituation Sit in Situations)
                {
                    if (Index > 0)
                    {
                        Distance += Utils.MapCalcDist(Situations[Index - 1].Location, Sit.Location, Utils.DistanceUnit.Kilometers);
                    }
                    Index++;
                }

                return Distance;
            }

            public void GenerateRecommendedAircraft()
            {
                try
                {
                    // Helis
                    // GA
                    // Turboprop
                    // Small jets
                    // Narrow-body
                    // Wide-body
                    int segments = 6;

                    // Calculators
                    Func<float, float, float, float> CalcFromTarget = (TippingPoint, Spread, Value) =>
                    {
                        float Dif = -Math.Abs(Value - TippingPoint);
                        float Multiplier = (100 / Spread) * 2;

                        return 100 + (Dif * Multiplier);
                    };
                    Func<float, float, float, float> CalcRelevance = (TippingPoint, Spread, Value) =>
                    {
                        float Dif = Value - TippingPoint;
                        float Multiplier = (100 / Spread);

                        return Dif * Multiplier;
                    };


                    List<Dictionary<string, float?>> Tracking = new List<Dictionary<string, float?>>();
                    RecommendedAircraft = new List<sbyte>() { 0, 0, 0, 0, 0, 0 };

                    while (Tracking.Count < segments)
                    {
                        Tracking.Add(new Dictionary<string, float?>()
                        {
                            { "Distance", 0 },
                            { "Surface", 0 },
                            { "Length", 0 },
                            { "Parking", 0 },
                            { "Elevation", 0 },
                        });
                    }

                    float DistToNextKM = 0;
                    foreach (RouteSituation Sit in Situations)
                    {
                        if(Sit.Airport != null)
                        {
                            int Longest = (Sit.Airport.Runways.Select(x => x.LengthFT).Max());
                            int Index = Situations.IndexOf(Sit);

                            if (Index < Situations.Count - 1)
                            {
                                DistToNextKM = (float)Utils.MapCalcDist(Sit.Location, Situations[Index + 1].Location, Utils.DistanceUnit.Kilometers, true);
                            }

                            if (Sit.Airport != null)
                            {
                                if (DistToNextKM != 0)
                                {
                                    float DistToNext = DistToNextKM * 0.539957f;
                                    Tracking[0]["Distance"] = Tracking[0]["Distance"] != null ? Tracking[0]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(0, 80, DistToNext)) / (Situations.Count - 1)) : null; // Heli
                                    Tracking[1]["Distance"] = Tracking[1]["Distance"] != null ? Tracking[1]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(0, 800, DistToNext)) / (Situations.Count - 1)) : null; // Cessna
                                    Tracking[2]["Distance"] = Tracking[2]["Distance"] != null ? Tracking[2]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(500, 1200, DistToNext)) / (Situations.Count - 1)) : null; // Turboprop
                                    Tracking[3]["Distance"] = Tracking[3]["Distance"] != null ? Tracking[3]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(1500, 4000, DistToNext)) / (Situations.Count - 1)) : null; // Jet
                                    Tracking[4]["Distance"] = Tracking[4]["Distance"] != null ? Tracking[4]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(1500, 4000, DistToNext)) / (Situations.Count - 1)) : null; // Narrow
                                    Tracking[5]["Distance"] = Tracking[5]["Distance"] != null ? Tracking[5]["Distance"] + ((float)Utils.Limiter(-300, 300, CalcFromTarget(4000, 8000, DistToNext)) / (Situations.Count - 1)) : null; // Wide
                                }

                                Tracking[0]["Length"] = Tracking[0]["Length"] != null ? Tracking[0]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(300, 6000, Longest)) / Situations.Count) : null;
                                Tracking[1]["Length"] = Tracking[1]["Length"] != null ? Tracking[1]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(600, 4000, Longest)) / Situations.Count) : null;
                                Tracking[2]["Length"] = Tracking[2]["Length"] != null && Longest > 2000 ? Tracking[2]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(2000, 800, Longest)) / Situations.Count) : null;
                                Tracking[3]["Length"] = Tracking[3]["Length"] != null && Longest > 3500 ? Tracking[3]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(3500, 1000, Longest)) / Situations.Count) : null;
                                Tracking[4]["Length"] = Tracking[4]["Length"] != null && Longest > 6000 ? Tracking[4]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(6000, 1100, Longest)) / Situations.Count) : null;
                                Tracking[5]["Length"] = Tracking[5]["Length"] != null && Longest > 7000 ? Tracking[5]["Length"] + ((float)Utils.Limiter(-100, 100, CalcRelevance(7000, 1100, Longest)) / Situations.Count) : null;

                                Tracking[0]["Parking"] = Tracking[0]["Parking"] != null ? 100 : (float?)null;
                                Tracking[1]["Parking"] = Tracking[1]["Parking"] != null ? Tracking[1]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 5).Count * 100)) / Situations.Count) : null;
                                Tracking[2]["Parking"] = Tracking[2]["Parking"] != null ? Tracking[2]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 7).Count * 100)) / Situations.Count) : null;
                                Tracking[3]["Parking"] = Tracking[3]["Parking"] != null ? Tracking[3]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 12).Count * 100)) / Situations.Count) : null;
                                Tracking[4]["Parking"] = Tracking[4]["Parking"] != null ? Tracking[4]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 50).Count * 100)) / Situations.Count) : null;
                                Tracking[5]["Parking"] = Tracking[5]["Parking"] != null ? Tracking[5]["Parking"] + ((float)Utils.Limiter(-100, 100, -100 + (Sit.Airport.Parkings.FindAll(x => x.Diameter > 55).Count * 100)) / Situations.Count) : null;

                                Tracking[0]["Elevation"] = Tracking[0]["Elevation"] != null ? Tracking[0]["Elevation"] - (CalcRelevance(3000, 15000, Sit.Airport.Elevation) / Situations.Count) : null;
                                Tracking[1]["Elevation"] = Tracking[1]["Elevation"] != null ? Tracking[1]["Elevation"] - (CalcRelevance(2000, 10000, Sit.Airport.Elevation) / Situations.Count) : null;
                                Tracking[2]["Elevation"] = Tracking[2]["Elevation"] != null ? Tracking[2]["Elevation"] - (CalcRelevance(4000, 10000, Sit.Airport.Elevation) / Situations.Count) : null;
                                Tracking[3]["Elevation"] = Tracking[3]["Elevation"] != null ? Tracking[3]["Elevation"] - (CalcRelevance(7000, 10000, Sit.Airport.Elevation) / Situations.Count) : null;
                                Tracking[4]["Elevation"] = Tracking[4]["Elevation"] != null ? Tracking[4]["Elevation"] - (CalcRelevance(7000, 10000, Sit.Airport.Elevation) / Situations.Count) : null;
                                Tracking[5]["Elevation"] = Tracking[5]["Elevation"] != null ? Tracking[5]["Elevation"] - (CalcRelevance(7000, 10000, Sit.Airport.Elevation) / Situations.Count) : null;

                                bool IsSoft = Sit.Airport.Runways.Select(x => x.Surface).ToList().Where(x =>
                                {
                                    return x == Surface.Asphalt
                                    || x == Surface.Concrete
                                    || x == Surface.Bituminous
                                    || x == Surface.Tarmac;
                                }).Count() == 0;

                                if (IsSoft)
                                {
                                    // Skip Heli
                                    Tracking[1]["Surface"] = Tracking[1]["Surface"] != null ? Tracking[1]["Surface"] + (IsSoft ? 10 : 0 / Situations.Count) : null;
                                    Tracking[2]["Surface"] = Tracking[2]["Surface"] != null ? Tracking[2]["Surface"] + (IsSoft ? 5 : 0 / Situations.Count) : null;
                                    Tracking[3]["Surface"] = Tracking[3]["Surface"] != null ? Tracking[3]["Surface"] + (IsSoft ? -50 : 0 / Situations.Count) : null;
                                    Tracking[4]["Surface"] = Tracking[4]["Surface"] != null ? Tracking[4]["Surface"] + (IsSoft ? -80 : 0 / Situations.Count) : null;
                                    Tracking[5]["Surface"] = Tracking[5]["Surface"] != null ? Tracking[5]["Surface"] + (IsSoft ? -100 : 0 / Situations.Count) : null;
                                }
                            }
                        }
                    }

                    int index = 0;
                    foreach (var Track in Tracking)
                    {
                        if (!Track.Values.Contains(null))
                        {
                            RecommendedAircraft[index] = (sbyte)Utils.Limiter(-100, 100, (double)((Tracking[index]["Distance"] * 0.5f) + (Tracking[index]["Length"] * 0.5f) + (Tracking[index]["Parking"] * 0.1f) + (Tracking[index]["Elevation"] * 0.2f) + (Tracking[index]["Surface"] * 0.2f)));
                        }
                        else
                        {
                            RecommendedAircraft[index] = -100;
                        }
                        index++;
                    }

                    sbyte maxValue = RecommendedAircraft.Max();
                    if (maxValue > 10)
                    {
                        int maxIndex = RecommendedAircraft.IndexOf(maxValue);
                        RecommendedAircraft[maxIndex] = maxValue > 71 ? maxValue : (sbyte)71;
                    }
                }
                catch
                {

                }

            }
            
            public override string ToString()
            {
                string List = "";
                
                int i = 0;
                foreach (RouteSituation Sit in Situations)
                {
                    int DistToNext = 0;
                    string Annex = "";
                
                    if (i < Situations.Count - 1)
                    {
                        DistToNext = (int)Utils.MapCalcDist(Sit.Location, Situations[i + 1].Location, Utils.DistanceUnit.NauticalMiles);
                        Annex = " --" + DistToNext + "--> ";
                    }
                
                    if (Sit.Airport == null)
                    {
                        List += Sit.Location.ToString() + Annex;
                    }
                    else
                    {
                        List += Sit.Airport.ToString() + Annex;
                    }
                    i++;
                }
                
                return List + " @ " + (int)(Distance * 0.539957) + "nm";
               
            }

        }

        internal class RouteGenParams
        {
            internal static int Instance = 0;
            internal long UID = 0;
            internal string Name = "";
            internal SimLibrary.Simulator Sim = null;
            internal Action<GeneratedRoute> Callback = null;
            internal List<Airport> PossibleAirports = new List<Airport>();
            internal List<Airport> IncludeICAO = new List<Airport>();
            internal List<Airport> ExcludeICAO = new List<Airport>();
            internal List<Airport> DepICAO = new List<Airport>();
            internal List<Airport> ArrICAO = new List<Airport>();
            internal List<Airport> RequireICAO = null;
            internal RouteGenFilters Filters = null;
            internal float MaxDistanceKM = 0;
            internal int Limit = -1;

            internal RouteGenParams()
            {
                UID = Instance++;
            }

            internal RouteGenParams(RouteGenParams Base)
            {
                Sim = Base.Sim;
                PossibleAirports = new List<Airport>(Base.PossibleAirports);
                RequireICAO = Base.RequireICAO != null ? new List<Airport>(Base.RequireICAO) : null;
                IncludeICAO = new List<Airport>(Base.IncludeICAO);
                ExcludeICAO = new List<Airport>(Base.ExcludeICAO);
                DepICAO = new List<Airport>(Base.DepICAO);
                ArrICAO = new List<Airport>(Base.ArrICAO);
                MaxDistanceKM = Base.MaxDistanceKM;
                Filters = Base.Filters;
                Limit = Base.Limit;
                Callback = Base.Callback;
            }
        }

        public class RouteGenFilters
        {
            public float[] Range = new float[] { 0, float.MaxValue };
            public int[] RwyCount = new int[] { 0, int.MaxValue };
            public float[] RwyLength = new float[] { 0, float.MaxValue };
            public string RwySurface = "any";
        }

        public class RouteSituation
        {
            public int Index = -1;
            public uint UID = 0;
            public RouteSituationType SituationType;
            public Airport Airport;
            public GeoLoc Location;
            public float Height;
            public float DistToNextMinKM;
            public float DistToNextMaxKM;
            public bool RequiresLight;
            public float TriggerRange;
            public string Query = "";
            public string Country;
            public string Surface;
            public int HeliMin;
            public int HeliMax;
            public int RwyMin;
            public int RwyMax;
            public int ParkMin;
            public int ParkMax;
            public int ParkWidMin;
            public int ParkWidMax;
            public int RwyLenMin;
            public int RwyLenMax;
            public int RwyWidMin;
            public int RwyWidMax;
            public int ElevMin;
            public int ElevMax;
            public List<GeoLoc> Boundaries = new List<GeoLoc>();

            public List<Airport> PossibleAirports = null;

            public RouteSituation Copy()
            {
                return (RouteSituation)this.MemberwiseClone();
            }

            public override string ToString()
            {
                if (Airport != null)
                {
                    return Airport.ICAO;
                }
                else if (Location != null)
                {
                    return Location.ToString();
                }
                else
                {
                    return "?";
                }
            }
        }
    }
}
