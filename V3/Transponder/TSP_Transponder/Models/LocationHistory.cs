using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.DataStore;

namespace TSP_Transponder.Models
{
    class LocationHistory
    {
        public static Dictionary<string, Dictionary<string, float>> RelocationMultipliers = new Dictionary<string, Dictionary<string, float>>();
        public static float RelocationCost = 0.2f; // 0.2f perfect

        public static void Startup()
        {
            List<KeyValuePair<float, float>> Seasons = new List<KeyValuePair<float, float>>()
            {
                new KeyValuePair<float, float>(0, 0.2f), // 1 Jan
                new KeyValuePair<float, float>(31, 0.08f), // 1 Feb
                new KeyValuePair<float, float>(79, 0.1f), // 21 Mar
                new KeyValuePair<float, float>(181, 0.2f), // 1 Jul
                new KeyValuePair<float, float>(200, 0.2f), // 20 Jul
                new KeyValuePair<float, float>(272, 0.1f), // 30 Sept
                new KeyValuePair<float, float>(358, 0.2f), // 25 Dec
                new KeyValuePair<float, float>(367, 0.2f), // 91 Dec
            };

            Func<int, float> getCost = (day) => {
                var end = Seasons.Find(x => x.Key > day);
                var start = Seasons[Seasons.IndexOf(end) - 1];
                var delta = end.Key - start.Key;
                var factor = (day - start.Key) / delta;
                var eased = Easings.Interpolate(factor, Easings.EasingFunctions.SineEaseInOut);

                var costDelta = end.Value - start.Value;
                var costCalc = start.Value + (eased * costDelta);

                return Convert.ToSingle(costCalc);
            };

            RelocationCost = getCost(DateTime.UtcNow.DayOfYear);


            int inclusive_days = 4;
            int day_of_year = DateTime.UtcNow.DayOfYear;
            string holidays_json = App.ReadResourceFile("TSP_Transponder.Utilities.Holidays.holidays.json");
            List<Dictionary<string, dynamic>> holidays = App.JSSerializer.Deserialize<List<Dictionary<string, dynamic>>>(holidays_json);
            List<Dictionary<string, dynamic>> holidays_today = holidays.FindAll(x => x["DayOfYear"] < day_of_year + inclusive_days && x["DayOfYear"] > day_of_year - inclusive_days);

            foreach(var holiday in holidays_today)
            {
                float impact = ((float)(inclusive_days - Math.Abs(day_of_year - holiday["DayOfYear"])) / inclusive_days);
                float factor = ((((float)holiday["Multiplier"]) - 1) * impact) + 1;

                if (!RelocationMultipliers.ContainsKey(holiday["Country"]))
                {
                    RelocationMultipliers.Add(holiday["Country"], new Dictionary<string, float>());
                }

                if(!RelocationMultipliers[holiday["Country"]].ContainsKey(holiday["Name"]))
                {
                    RelocationMultipliers[holiday["Country"]].Add(holiday["Name"], factor);
                }
                
            }
#if DEBUG
            int i = 0;
            while(i < 366)
            {
                Console.WriteLine(i + "\t" + getCost(i));
                i++;
            }
#endif

        }

        public static void UpdateLastLocation(Airport Apt, GeoLoc Location)
        {
            if(Progress.Progress.XP.Balance > 0)
            {
                try
                {
                    lock (LiteDbService.DB)
                    {
                        var DBCollection = LiteDbService.DB.Database.GetCollection("location_history");
                        var All = DBCollection.FindAll();

                        Action UpdateAct = () =>
                        {
                            var LastLocationBson = new BsonDocument();
                            LastLocationBson["Date"] = DateTime.UtcNow;
                            LastLocationBson["Name"] = Apt == null ? Location.ToString(3) : Apt.ICAO;
                            LastLocationBson["Country"] = Utilities.Countries.GetCountry(Location).Code;
                            LastLocationBson["Longitude"] = Apt == null ? Location.Lon : Apt.Location.Lon;
                            LastLocationBson["Latitude"] = Apt == null ? Location.Lat : Apt.Location.Lat;

                            DBCollection.Insert(LastLocationBson);
                            Console.WriteLine("Updated last location to " + (Apt == null ? Location.ToString() : Apt.ICAO));

                            APIBase.ClientCollection.SendMessage("locationhistory:latest", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                            {
                                { "Name", (string)LastLocationBson["Name"] },
                                { "Date", (DateTime)LastLocationBson["Date"] },
                                { "Country", (string)LastLocationBson["Country"] },
                                { "CostPerNM", UserData.Get("tier") == "discovery" ? 0 : RelocationCost },
                                { "Location", new List<double>() { (double)LastLocationBson["Longitude"], (double)LastLocationBson["Latitude"] } },
                            }), null, APIBase.ClientType.Skypad);
                        };

                        if (All.Count() > 0)
                        {
                            var Last = All.Last();
                            if (Utils.MapCalcDist(Location, new GeoLoc((double)Last["Longitude"], (double)Last["Latitude"])) > 2) { UpdateAct(); }
                        }
                        else
                        {
                            UpdateAct();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to save last location because " + ex.Message);
                }
            }
        }

        public static GeoLoc GetLastLocation()
        {
            try
            {
                lock (LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection("location_history");
                    var All = DBCollection.FindAll();

                    if (All.Count() > 0)
                    {
                        var Last = All.Last();

                        if (Last != null)
                        {
                            return new GeoLoc(Last["Longitude"], Last["Latitude"]);
                        }
                    }

                }
            }
            catch
            {
            }
            return null;
        }

        public static void WSConnected(SocketClient Socket)
        {
            try
            {
                lock (LiteDbService.DB)
                {
                    var DBCollection = LiteDbService.DB.Database.GetCollection("location_history");
                    var All = DBCollection.FindAll();

                    if (All.Count() > 0)
                    {
                        var Last = All.Last();

                        if(Last != null)
                        {
                            Socket.SendMessage("locationhistory:latest", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                            {
                                { "Name", Last.ContainsKey("Name") ? (string)Last["Name"] : "" },
                                { "Date", Last.ContainsKey("Date") ? (DateTime)Last["Date"] : DateTime.UtcNow },
                                { "Country", Last.ContainsKey("Country") ? (string)Last["Country"] : "" },
                                { "CostPerNM", UserData.Get("tier") == "discovery" ? 0 : RelocationCost },
                                { "Location", new List<double>() {Last["Longitude"], Last["Latitude"] } },
                            }));
                        }
                    }

                }
            }
            catch
            {
            }
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure, int Depth = 0)
        {
            try
            {
                Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
                switch (StructSplit[1])
                {
                    case "latest":
                        {
                            lock (LiteDbService.DB)
                            {
                                var DBCollection = LiteDbService.DB.Database.GetCollection("location_history");
                                BsonDocument LastLocationEntry = DBCollection.FindAll().Last();

                                Socket.SendMessage("locationhistory:latest", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                                {
                                    { "Name", (string)LastLocationEntry["Name"] },
                                    { "Date", (DateTime)LastLocationEntry["Date"] },
                                    { "Country", (string)LastLocationEntry["Country"] },
                                    { "CostPerNM", UserData.Get("tier") == "discovery" ? 0 : RelocationCost },
                                    { "Location", new List<double>() { LastLocationEntry["Longitude"], LastLocationEntry["Latitude"] } },
                                }), (Dictionary<string, dynamic>)structure["meta"]);
                            }

                            break;
                        }
                }
            }
            catch
            {

            }

        }
    }
}
