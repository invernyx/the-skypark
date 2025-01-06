using LiteDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Notifications;

namespace TSP_Transponder.Models.Adventures.RouteGenerationV2
{

    class RouteGenWorker
    {
        public static void FindRoutes(AdventureTemplate Template, Action<List<Route>> Callback)
        {
            var RG = new RouteGen(Template, Template.RouteLimit)
            {
                ExcludeICAO = Template.ExcludeICAO,
                IncludeICAO = Template.IncludeICAO,
            };
            RG.Find();

            List<Route> routes = new List<Route>();
            lock (RG.Routes)
            {
                foreach (var route in RG.Routes)
                {
                    Route NR = new Route(Template)
                    {
                        RecommendedAircraft = route.RecommendedAircraft,
                    };

                    foreach (var Sit in route.Situations)
                    {
                        NR.Situations.Add(new Route.RouteSituation()
                        {
                            UID = Sit.UID,
                            Airport = Sit.Airport,
                            Location = Sit.Location,
                            LocationName = Sit.LocationName
                        });
                    }

                    NR.CalculateDistance();
                    NR.CalculateString();
                    NR.CreateRouteCode();

                    routes.Add(NR);

                    lock (Template.Routes)
                    {
                        Template.Routes.Add(NR);
                    }
                }
            }

            Callback(routes);
        }
    }

    class RouteGen
    {
        public int TargetLimit = 0;

        public int TaskID = Utils.GetRandom(Int32.MaxValue - 100000);
        public AdventureTemplate Template = null;
        public List<RouteGenSituation> SourceSituations = new List<RouteGenSituation>();
        public Dictionary<string, List<BsonDocument>> LocationsLib = new Dictionary<string, List<BsonDocument>>();
        public Dictionary<Airport, uint[]> AirportUses = new Dictionary<Airport, uint[]>();

        internal SimLibrary.Simulator Sim = null;
        internal List<RouteGenRoute> Routes = new List<RouteGenRoute>();
        internal Action<RouteGenRoute> Callback = null;
        internal List<Airport> PossibleAirports = new List<Airport>();
        internal List<Airport> IncludeICAO = new List<Airport>();
        internal List<Airport> ExcludeICAO = new List<Airport>();
        internal List<Airport> DepICAO = new List<Airport>();
        internal List<Airport> ArrICAO = new List<Airport>();
        internal List<Airport> RequireICAO = null;

        internal int? GenSeedIndex = null;
        internal List<List<bool>> SuccessIndex = new List<List<bool>>();

        internal float? MaxDistanceKM = null;
        internal float? MinDistanceKM = null;

        public float[] Range = new float[] { 0, float.MaxValue };
        public int[] RwyCount = new int[] { 0, int.MaxValue };
        public float[] RwyLength = new float[] { 0, float.MaxValue };
        public string RwySurface = "any";

        private object LastStatusLock = new object();
        private DateTime LastStatus = DateTime.UtcNow;
        private int LastStatusCount = 0;

        public RouteGen(AdventureTemplate Template, int TargetLimit) { 

            this.TargetLimit = TargetLimit;
            this.Template = Template;

            InitPossibleAirports();

            ushort Index = 0;
            foreach(var situation_dic in Template.InitialStructure["Situations"])
            {
                var situation = new RouteGenSituation(this, null, Index, situation_dic);
                SourceSituations.Add(situation);
                SuccessIndex.Add(new List<bool>());

                Index++;
            }

            foreach (var situation in SourceSituations)
            {
                situation.Init();
            }

        }

        public void LogAirportUse(Airport Airport, bool Used)
        {
            lock(AirportUses)
            {
                KeyValuePair<Airport, uint[]> existing = new KeyValuePair<Airport, uint[]>();
                if (AirportUses.ContainsKey(Airport))
                {
                    existing = new KeyValuePair<Airport, uint[]>(Airport, AirportUses[Airport]);
                }
                if (existing.Key == null)
                {
                    existing = new KeyValuePair<Airport, uint[]>(Airport, new uint[] { 0, 0 });
                    AirportUses.Add(existing.Key, existing.Value);
                }

                existing.Value[Used ? 0 : 1]++;
            }
        }

        private void InitPossibleAirports()
        {
            PossibleAirports = SimLibrary.SimList[0].AirportsLib.AllAirports.Values.Where(x => !x.IsClosed && !x.IsMilitary).ToList();
        }

        public void Find()
        {
            int Fails = 0;
            int FailLimit = Math.Max(10000, (int)(TargetLimit * 0.8));

            int NumbThreads = 5;

            while (Routes.Count < TargetLimit && Fails < FailLimit)
            {
                Task[] tasks = new Task[NumbThreads];
                for (int i = 0; i < NumbThreads; i++)
                {
                    tasks[i] = Task.Factory.StartNew(() =>
                    {
                        RouteGenRoute new_route = new RouteGenRoute(this);
                        bool result_valid = new_route.Find();
                        if (result_valid)
                        {
                            Fails = 0;
                            lock (Routes)
                            {
                                Routes.Add(new_route);
                            }
                            //Console.WriteLine("Found route for " + Template.FileName + " " + Routes.Count + "/" + TargetLimit + " " + new_route.Distance + "km (" + String.Join(" -> ", new_route.Situations.Select(x => x.ToString())) + ")");
                        }
                        else
                        {
                            Fails++;
                        }

                        // Log airport uses in order to optimize ongoing route search
                        foreach (var Situations in new_route.Situations.Where(x => x.Airport != null))
                        {
                            LogAirportUse(Situations.Airport, result_valid);
                        }

                        #region Notify loop
                        lock (LastStatusLock)
                        {
                            if (DateTime.UtcNow - LastStatus > TimeSpan.FromMilliseconds(1000))
                            {
                                if (App.IsDev)
                                {
                                    double RoutesPerSecond = ((Routes.Count - LastStatusCount) / (DateTime.UtcNow - LastStatus).TotalSeconds) * 60;
                                    NotificationService.Add(new Notification()
                                    {
                                        UID = 9503 + TaskID,
                                        Title = "Routes are generating for " + Template.Name,
                                        Message = Math.Round(((float)Routes.Count / TargetLimit) * 100f, 2).ToString("0.00", CultureInfo.InvariantCulture) + "% - " + Routes.Count + "/" + TargetLimit + " - " + (RoutesPerSecond > 0 ? Math.Round(RoutesPerSecond) : RoutesPerSecond) + "rpm",
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

                    });
                }
                // wait for all tasks to complete
                Task.WaitAll(tasks);
            }

            if (App.IsDev)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1000);
                    lock (LastStatusLock)
                    {
                        NotificationService.RemoveFromID(9503 + TaskID);
                    }
                });
            }
        }


    }
}
