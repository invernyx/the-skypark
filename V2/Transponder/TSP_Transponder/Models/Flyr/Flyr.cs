using System.Linq;
using System.Collections.Generic;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Transactor;
using TSP_Transponder.Models.DataStore;
using LiteDB;

namespace TSP_Transponder.Models.Flyr
{
    public class Flyr
    {
        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "next":
                    {
                        lock (LiteDbService.DB)
                        {
                            var DBCollection = LiteDbService.DB.Database.GetCollection("app_strip");
                            DBCollection.EnsureIndex("_id", true);

                            Airports.AirportsLib Lib = SimLibrary.SimList[0].AirportsLib;

                            if (payload_struct.ContainsKey("Review"))
                            {
                                Airport Apt = Lib.GetByICAO((string)payload_struct["Review"]["ICAO"]);
                                bool Rating = (bool)payload_struct["Review"]["Like"];

                                DBCollection.Upsert(Apt.ICAO, new BsonDocument()
                                {
                                    ["Rating"] = Rating,
                                });

                                GoogleAnalyticscs.TrackEvent("App_Strip", "Airport", Apt.ICAO, Rating ? 1 : 0, true);
                            }

                            var CoveredAirports = DBCollection.Query().Select(x => x["_id"]).ToList();
                            List<Airport> Contenders = Lib.AllAirports.Values.Where(x => !CoveredAirports.Contains(x.ICAO)).ToList();


                            WeightedRandom<Airport> AptChoices = new WeightedRandom<Airport>();
                            foreach (var Apt1 in Contenders)
                            {
                                AptChoices.AddEntry(Apt1, Apt1.Relief);
                            }

                            if (AptChoices.Count > 0)
                            {
                                Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
                            {
                                { "Airport", AptChoices.GetRandom().ToSummary(true) }
                            };
                                Socket.SendMessage("flyr:next", App.JSSerializer.Serialize(rs), (Dictionary<string, dynamic>)structure["meta"]);
                            }
                            else
                            {
                                Socket.SendMessage("flyr:next", App.JSSerializer.Serialize(null), (Dictionary<string, dynamic>)structure["meta"]);
                            }
                        }
                        break;
                    }
                case "matches":
                    {
                        Socket.SendMessage("flyr:matches", App.JSSerializer.Serialize(null), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }
                
    }
}
