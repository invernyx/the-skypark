using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Adventures.RouteGeneration
{
    public static class RouteReader
    {
#if DEBUG

        //https://airlabs.co/docs/airlines
        //https://airlabs.co/api/v9/routes?api_key=26cde7bd-a6b8-4ffc-bb81-77db8c301f2d&airline_icao=SIA

        //https://content.airhex.com/content/logos/airlines_PSC_350_100_s.png
        //https://airlinecodes.info/airlinelogos/AC.svg
        //https://www.kayak.com/rimg/provider-logos/airlines/v/BB.png

        //https://storage.googleapis.com/gilfoyle/the-skypark/v3/common/images/airlines/s/BB.png

        private static string api_key = "26cde7bd-a6b8-4ffc-bb81-77db8c301f2d";
        private static string cache_trim = "https://airlabs.co/api/v9/";


        private static List<string> airlines_d_fields = new List<string>()
        {
            "name",
            "iata_code",
            "icao_code",
            "country_code",
            "is_scheduled",
            "is_passenger",
            "is_cargo",
        };

        private static List<string> routes_d_fields = new List<string>()
        {
            //"airline_iata",
            //"airline_icao",
            //"flight_number",
            //"dep_icao",
            //"dep_time_utc",
            //"arr_icao",
            //"arr_time_utc",
            //"dep_terminals",
            //"arr_terminals",
            //"days",
            //"duration"
        };

        private static List<string> days_bind = new List<string>()
        {
            "mon",
            "tue",
            "wed",
            "thu",
            "fri",
            "sat",
            "sun"
        };

        public static void Startup()
        {
            //GetRealRoutesAsync();
            //GetAirlineImages();
        }

        private static List<Dictionary<string, dynamic>> getjson(string url)
        {
            string query_path = Path.Combine(App.AppDataDirectory, "Airlines", "Cache", url.Replace(cache_trim, "").Replace("&api_key=" + api_key, "").Replace("?", "_").Replace("/", "_").Replace(".", "_").Replace("&", "_").Replace("=", "_").Replace(":", "_") + ".json");

            if (File.Exists(query_path))
            {
                var fi = new FileInfo(query_path);

                if ((DateTime.UtcNow - fi.LastWriteTimeUtc).TotalDays < 180)
                {
                    var r = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(File.ReadAllText(query_path));
                    return ((ArrayList)r["response"]).Cast<Dictionary<string, dynamic>>().ToList();
                }

            }

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;

                    File.WriteAllText(query_path, result);

                    var r = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(result);

                    return ((ArrayList)r["response"]).Cast<Dictionary<string, dynamic>>().ToList();
                }
            }
            catch
            {
                return null;
            }

            return null;

        }

        public static void GetRealRoutesAsync()
        {
            var total_added_count = 0;
            
            CancellationTokenSource cts = new CancellationTokenSource();
            var po = new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = cts.Token };

            var DBCollection_routes = LiteDbService.DBAdv.Database.GetCollection("routes");
            LiteDbService.DBAdv.Database.DropCollection("routes");
            LiteDbService.DBAdv.Database.Rebuild();

            var airlines_d = getjson(@"https://airlabs.co/api/v9/airlines?_fields=" + string.Join(",", airlines_d_fields) + "&api_key=" + api_key);
            var airlines_d_filtered = airlines_d.FindAll(x => x.ContainsKey("icao_code") && x.ContainsKey("iata_code") && x.ContainsKey("is_scheduled") && x.ContainsKey("is_passenger") && x.ContainsKey("is_cargo"));
            airlines_d_filtered = airlines_d_filtered.FindAll(x => x["is_scheduled"] == 1);
            int total_routes = 0;

            List<BsonDocument> ToAdd = new List<BsonDocument>();
            

            Console.WriteLine("Getting routes from " + airlines_d_filtered.Count + " Airlines...");
            Parallel.ForEach(((List<Dictionary<string, dynamic>>)airlines_d_filtered), po, (airline_d, state, i) =>
            {
                if (App.MW.IsShuttingDown)
                {
                    cts.Cancel();
                    return;
                }

                Console.WriteLine("... " + i + " of " + airlines_d_filtered.Count + " - " + total_routes + " routes");
                var routes_d = getjson(@"https://airlabs.co/api/v9/routes?airline_icao=" + airline_d["icao_code"] + "&_fields=" + string.Join(",", routes_d_fields) + "&api_key=" + api_key);
                

                // Create new routes
                foreach(var route in routes_d)
                {
                    if (route["cs_flight_number"] != null)
                        continue;

                    Airport departure_apt = null;
                    Airport arrival_apt = null;

                    try
                    {
                        departure_apt = SimLibrary.SimList[0].AirportsLib.GetByICAO(route["dep_icao"]);
                        arrival_apt = SimLibrary.SimList[0].AirportsLib.GetByICAO(route["arr_icao"]);
                    }
                    catch
                    {
                        return;
                    }

                    // Skip if we're missing airports
                    if (departure_apt == null || arrival_apt == null)
                        return;
                    
                    // Skip if flight is less than 5km
                    var dist = Utils.MapCalcDistFloat((float)arrival_apt.Location.Lat, (float)arrival_apt.Location.Lon, (float)departure_apt.Location.Lat, (float)departure_apt.Location.Lon, Utils.DistanceUnit.Kilometers);
                    if (dist < 5)
                        return;

                    #region Convert times and day of week
                    var day_offset = 0;
                    Timezones.Timezone agmt = Timezones.GetTimezone(departure_apt.Location);
                    Timezones.Timezone dgmt = Timezones.GetTimezone(arrival_apt.Location);

                    if (agmt == null || dgmt == null)
                        return;


                    DateTime dep_time_raw = ((DateTime)DateTime.ParseExact(route["dep_time"], "HH:mm", CultureInfo.InvariantCulture));
                    DateTime dep_time1 = new DateTime(1, 1, 2, dep_time_raw.Hour, dep_time_raw.Minute, 0);

                    DateTime arr_time_raw = DateTime.UtcNow;
                    DateTime arr_time1 = DateTime.UtcNow;
                    if (route["arr_time"] != null)
                    {
                        arr_time_raw = ((DateTime)DateTime.ParseExact(route["arr_time"], "HH:mm", CultureInfo.InvariantCulture));
                        arr_time1 = new DateTime(1, 1, 2, arr_time_raw.Hour, arr_time_raw.Minute, 0);
                    }
                    else
                    {
                        var duration_guess = (dist / 1000);
                        arr_time1 = new DateTime(1, 1, 2, 0, 0, 0).AddHours(duration_guess);
                    }

                    dep_time1 = dep_time1.AddHours(-Convert.ToDouble(dgmt.Offset));
                    arr_time1 = arr_time1.AddHours(-Convert.ToDouble(dgmt.Offset));

                    if (dep_time1.Day == 1)
                        day_offset = -1;

                    if (dep_time1.Day == 3)
                        day_offset = 1;

                    var dep_time_utc = Math.Round(dep_time1.Hour + ((float)dep_time1.Minute / 60), 3);
                    var arr_time_utc = Math.Round(arr_time1.Hour + ((float)arr_time1.Minute / 60), 3);

                    //if (route["flight_icao"] == "SWA912")
                    //{

                    //}

                    // Find days of week
                    var days_utc_int = new BsonArray();
                    foreach (var day in route["days"])
                    {
                        var index = days_bind.IndexOf(day);
                        if (index != null)
                        {
                            var new_day_index = index + day_offset;

                            if (new_day_index > 6)
                            {
                                days_utc_int.Add(0);
                            }
                            else if (new_day_index < 0)
                            {
                                days_utc_int.Add(6);
                            }
                            else
                            {
                                days_utc_int.Add(new_day_index);
                            }

                        }
                    }
                    #endregion

                    #region Flight Types
                    var flight_types = -1;

                    if (airline_d["is_passenger"] == 1 && airline_d["is_cargo"] == 1)
                    {
                        flight_types = 1;
                    }
                    else
                    {
                        if (airline_d["is_passenger"] == 1)
                            flight_types = 1;

                        if (airline_d["is_cargo"] == 1)
                            flight_types = 2;
                    }

                    #endregion

                    // Add to db
                    var bsd = new BsonDocument()
                    {
                        ["days"] = days_utc_int,
                        ["departure"] = departure_apt.ICAO,
                        ["arrival"] = arrival_apt.ICAO,
                        ["flight"] = route["flight_icao"],
                        ["dep_time"] = dep_time_utc,
                        ["arr_time"] = arr_time_utc,
                        ["airline_icao"] = airline_d["icao_code"],
                        ["airline_iata"] = airline_d.ContainsKey("iata_code") ? airline_d["iata_code"] : null,
                        ["airline"] = airline_d["name"],
                        ["types"] = flight_types,
                        ["model_code"] = route.ContainsKey("aircraft_icao") ? route["aircraft_icao"] : null,
                        ["distance"] = dist,
                    };
                    
                    lock (LiteDbService.DBAdv)
                    {
                        total_routes++;
                    }

                    lock (ToAdd)
                        ToAdd.Add(bsd);
                };
                
            });

            lock (LiteDbService.DBAdv)
                DBCollection_routes.InsertBulk(ToAdd);

            Console.WriteLine("Done");



















            /*

            Console.WriteLine("Getting Airports from Aviation Edge");
            var airports_d = getjson(@"https://aviation-edge.com/v2/public/airportDatabase?key=" + api_key);
            var airports_l = SimLibrary.SimList[0].AirportsLib.GetAirportsCopy();
            var airports_m = new List<KeyValuePair<Airport, Dictionary<string, dynamic>>>();

            Console.WriteLine("Matching Aviation Edge airports with Library airports");
            Parallel.ForEach(airports_d.Where(x => x["codeIcaoAirport"] != string.Empty), (airport_d) =>
            {
                var airport_l_match = airports_l.Find(x => x.ICAO == airport_d["codeIcaoAirport"]);
                if (airport_l_match != null)
                {
                    if(airport_l_match.Parkings.Count > 4 && !airport_l_match.IsClosed && !airport_l_match.IsMilitary)
                    {
                        // Check if we already have that airport in the database
                        BsonDocument existing = null;
                        lock (LiteDbService.DBAdv)
                            existing = LiteDbService.DBAdv.Database.GetCollection("routes_meta").FindById(airport_l_match.ICAO);
                        
                        if (existing != null)
                        {
                            var existing_time = new DateTime(existing["pull_date"]);

                            // Skip airport if data is newer than 180 days
                            if ((DateTime.UtcNow - existing_time).TotalDays < 180)
                            {
                                return;
                            }
                            else
                            {
                                // Reset airport, delete all references to it
                                lock (LiteDbService.DBAdv)
                                {
                                    var DBCollection_routes = LiteDbService.DBAdv.Database.GetCollection("routes");
                                    var DBCollection_meta = LiteDbService.DBAdv.Database.GetCollection("routes_meta");

                                    DBCollection_routes.DeleteMany(x => x["departure"] == airport_l_match.ICAO);
                                    DBCollection_meta.Delete(airport_l_match.ICAO);
                                }

                            }

                            // Skip if we know the airport has no routes
                            if (existing["count"] == 0)
                                return;
                        }

                        var kv = new KeyValuePair<Airport, Dictionary<string, dynamic>>(airport_l_match, airport_d);

                        lock (airports_m)
                            airports_m.Add(kv);
                    }
                }
            });

            Console.WriteLine("Sorting airports from largest to smallest");
            airports_m = airports_m.OrderByDescending(x => x.Key.Parkings.Count).ToList();

            Console.WriteLine("Adding airline routes to the db");
            var airports_r = new List<KeyValuePair<Airport, List<List<Dictionary<string, dynamic>>>>>();
            var date_start = Utils.StartOfWeek(DateTime.Now, DayOfWeek.Monday).AddDays(14);
            
            Console.WriteLine("Getting Airlines from Aviation Edge");
            var airlines_d = getjson(@"https://aviation-edge.com/v2/public/airlineDatabase?key=" + api_key);
            airlines_d = airlines_d.FindAll(x => x["sizeAirline"] > 0);

            var total_added_count = 0;

            List<Airport> done_airports = new List<Airport>();
            var time_started = DateTime.UtcNow;

            var myCI = new CultureInfo("en-US");
            var myCal = myCI.Calendar;



            // Update
            //var DBCollection_routes1 = LiteDbService.DBAdv.Database.GetCollection("routes");
            //foreach (var route in DBCollection_routes1.FindAll())
            //{
            //    var fn = airlines_d.Find(x => x["codeIcaoAirline"] == route["airline_code"]);
            //    route.Add("airline_code_iata", (string)fn["codeIataAirline"]);
            //    DBCollection_routes1.Update(route);
            //}
            //LiteDbService.DBAdv.Database.Checkpoint();

            
            List<string> AirlineCodes = new List<string>();

            lock (LiteDbService.DBAdv)
            {
                var DBCollection_routes = LiteDbService.DBAdv.Database.GetCollection("routes");
                foreach (var route in DBCollection_routes.FindAll())
                {
                    string airline_code_2 = airlines_d.Find(x => x["codeIcaoAirline"] == route["airline_code"])["codeIataAirline"];
                    lock (AirlineCodes)
                        if (!AirlineCodes.Contains(airline_code_2))
                            AirlineCodes.Add(airline_code_2);
                }
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            Parallel.ForEach(airports_m, new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = cts.Token }, (airport_m, state, index) =>
            {
                Console.WriteLine("Getting started with " + airport_m.Key.ICAO);
                Thread.Sleep(Utils.GetRandom(5000));
                
                if (App.MW.IsShuttingDown)
                {
                    cts.Cancel();
                    return;
                }
                
                var schedules = new List<List<Dictionary<string, dynamic>>>()
                {
                    new List<Dictionary<string, dynamic>>(),
                    new List<Dictionary<string, dynamic>>(),
                    new List<Dictionary<string, dynamic>>(),
                    new List<Dictionary<string, dynamic>>(),
                    new List<Dictionary<string, dynamic>>(),
                    new List<Dictionary<string, dynamic>>(),
                    new List<Dictionary<string, dynamic>>(),
                };

                //int day_offset = 0;
                //while (day_offset < 7)
                //{
                var day_date = date_start.AddDays(7);
                //var day_schedules = new List<Dictionary<string, dynamic>>();
                
                List<Dictionary<string, dynamic>> schedules_d = null;
                schedules_d = getjson(@"https://aviation-edge.com/v2/public/flightsFuture?key=" + api_key + "&" + string.Join("&", new List<string>()
                {
                    "iataCode=" + airport_m.Value["codeIataAirport"],
                    "type=departure",
                    "week=" + myCal.GetWeekOfYear(day_date, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday),
                    //"date=" + day_date.Year.ToString("D4") + "-" + day_date.Month.ToString("D2") + "-" + day_date.Day.ToString("D2")
                }));

                if(schedules_d != null)
                {
                    if (schedules_d.Count > 0)
                    {
                        foreach (var schedule_d in schedules_d)
                        {
                            var arr_icao = ((string)schedule_d["arrival"]["icaoCode"]).ToUpper();
                            var arr_apt = airports_l.Find(x => x.ICAO == arr_icao);
                                
                            if (arr_apt == null)
                                continue;

                            if (schedule_d["airline"]["icaoCode"] == string.Empty
                            || schedule_d["flight"]["icaoNumber"] == string.Empty
                            || schedule_d["departure"]["scheduledTime"] == string.Empty
                            || schedule_d["arrival"]["scheduledTime"] == string.Empty)
                                continue;

                            var airline_d = airlines_d.Find(x => x["codeIcaoAirline"] == ((string)schedule_d["airline"]["icaoCode"]).ToUpper());
                            if (airline_d == null)
                                continue;

                            int day = Convert.ToInt16(schedule_d["weekday"]) - 1;
                            schedules[day].Add(schedule_d);
                            //day_schedules.Add(schedule_d);

                        }
                    }
                }
                else
                {
                    return;
                }

                //schedules.Add(day_schedules);
                //day_offset++;
                //}
                
                lock (airports_r)
                    airports_r.Add(new KeyValuePair<Airport, List<List<Dictionary<string, dynamic>>>>(airport_m.Key, schedules));

                var DBCollection_routes = LiteDbService.DBAdv.Database.GetCollection("routes"); 
                var DBCollection_meta = LiteDbService.DBAdv.Database.GetCollection("routes_meta");
                
                int day_index = 0;
                int count = 0;
                foreach (var day in schedules)
                {
                    foreach(var schedule_d in day)
                    {
                        var dep_icao = ((string)schedule_d["departure"]["icaoCode"]).ToUpper();
                        var arr_icao = ((string)schedule_d["arrival"]["icaoCode"]).ToUpper();

                        if (dep_icao == arr_icao)
                            continue;

                        var arr_apt = airports_l.Find(x => x.ICAO == arr_icao);

                        if (arr_apt != null)
                            continue;

                        Utilities.Timezones.Timezone agmt = Utilities.Timezones.GetTimezone(airport_m.Key.Location);
                        Utilities.Timezones.Timezone dgmt = Utilities.Timezones.GetTimezone(arr_apt.Location);
                        
                        if (agmt == null || dgmt == null)
                            continue;
                        
                        var dep_time_raw = ((DateTime)DateTime.ParseExact(schedule_d["departure"]["scheduledTime"], "HH:mm", CultureInfo.InvariantCulture));
                        var arr_time_raw = ((DateTime)DateTime.ParseExact(schedule_d["arrival"]["scheduledTime"], "HH:mm", CultureInfo.InvariantCulture));
                        
                        var dep_time1 = new DateTime(1, 1, 2, dep_time_raw.Hour, dep_time_raw.Minute, 0);
                        var arr_time1 = new DateTime(1, 1, 2, arr_time_raw.Hour, arr_time_raw.Minute, 0);
                        
                        dep_time1 = dep_time1.AddHours(-Convert.ToDouble(dgmt.Offset));
                        arr_time1 = arr_time1.AddHours(-Convert.ToDouble(dgmt.Offset));

                        if (dep_time1.Day == 1)
                            day_index = day_index == 0 ? (int)6 : (int)day_index - 1;

                        if (dep_time1.Day == 3)
                            day_index = day_index == 6 ? (int)0 : (int)day_index + 1;

                        var dep_time = Math.Round(dep_time1.Hour + ((float)dep_time1.Minute / 60), 3);
                        var arr_time = Math.Round(arr_time1.Hour + ((float)arr_time1.Minute / 60), 3);

                        var dist = Utils.MapCalcDistFloat((float)arr_apt.Location.Lat, (float)arr_apt.Location.Lon, (float)airport_m.Key.Location.Lat, (float)airport_m.Key.Location.Lon, Utils.DistanceUnit.Kilometers);

                        if (dist < 5)
                            continue;
                        
                        var bsd = new BsonDocument()
                        {
                            ["day"] = day_index,
                            ["departure"] = airport_m.Key.ICAO,
                            ["arrival"] = arr_icao,
                            ["flight"] = ((string)schedule_d["flight"]["icaoNumber"]).ToUpper(),
                            ["dep_time"] = dep_time,
                            ["arr_time"] = arr_time,
                            ["airline_code"] = ((string)schedule_d["airline"]["icaoCode"]).ToUpper(),
                            ["airline_code_iata"] = ((string)schedule_d["airline"]["iataCode"]).ToUpper(),
                            ["airline"] = airlines_d.Find(x => x["codeIcaoAirline"] == ((string)schedule_d["airline"]["icaoCode"]).ToUpper())["nameAirline"],
                            ["model_code"] = ((string)schedule_d["aircraft"]["modelCode"]).ToUpper(),
                            ["distance"] = dist,
                        };


                        lock (LiteDbService.DBAdv)
                            DBCollection_routes.Insert(bsd);

                        count++;

                        string airline_code_2 = airlines_d.Find(x => x["codeIcaoAirline"] == bsd["airline_code"])["codeIataAirline"];
                        lock (AirlineCodes)
                            if (!AirlineCodes.Contains(airline_code_2))
                                AirlineCodes.Add(airline_code_2);
                    }
                    day_index++;
                }

                total_added_count += count;

                var nbd = new BsonDocument()
                {
                    ["pull_date"] = DateTime.UtcNow.Ticks,
                    ["count"] = count,
                };
                
                DBCollection_meta.Upsert(airport_m.Key.ICAO, nbd);

                lock (done_airports)
                    done_airports.Add(airport_m.Key);
                
                // Consol log status statistics 
                var done_count = 0;
                lock (done_airports)
                    done_count = done_airports.Count;

                var time_lapse = DateTime.UtcNow - time_started;
                var airport_per_hour = time_lapse.TotalHours / done_count;
                var time_left = airport_per_hour * (airports_m.Count - done_count);

                Console.WriteLine("Done with " + airport_m.Key.ICAO + " (" + airport_m.Value["codeIataAirport"] + "): " + count + " routes. (" + done_count + " of " + airports_m.Count + ") " + Math.Round(time_left, 3) + "h left");

            });



            // Download airline images
            string AirlinesPath = Path.Combine(App.AppDataDirectory, "Airlines");
            if (!Directory.Exists(AirlinesPath))
                Directory.CreateDirectory(AirlinesPath);

            Parallel.ForEach(AirlineCodes, new ParallelOptions { MaxDegreeOfParallelism = 5 }, (airline, state, index) =>
            {
                string airline_code_2 = airlines_d.Find(x => x["codeIataAirline"] == airline)["codeIcaoAirline"];
                string airline_path = Path.Combine(AirlinesPath, airline_code_2 + ".png");
                if (!File.Exists(airline_path))
                {
                    Thread.Sleep(100);
                    try
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile("https://pics.avs.io/128/128/" + airline + "@2x.png", airline_path);
                    }
                    catch
                    {
                    }
                }
            });

            */

            var total_count = 0;
            lock (LiteDbService.DBAdv)
            {
                DBCollection_routes.EnsureIndex("departure");
                DBCollection_routes.EnsureIndex("dep_time");
                DBCollection_routes.EnsureIndex("days");
                LiteDbService.DBAdv.Database.Checkpoint();
            }
            
            Console.WriteLine("Airline Routes: " + total_count);
            Console.WriteLine("Added Airline Routes: " + total_added_count);

            Environment.Exit(0);
        }

        public static void GetAirlineImages()
        {
            string AirlinesPath = Path.Combine(App.AppDataDirectory, "Airlines", "Images");

            var airlines_d = getjson(@"https://airlabs.co/api/v9/airlines?_fields=" + string.Join(",", airlines_d_fields) + "&api_key=" + api_key);
            var airlines_d_filtered = airlines_d.FindAll(x => x.ContainsKey("icao_code") && x.ContainsKey("iata_code") && x.ContainsKey("is_scheduled") && x.ContainsKey("is_passenger") && x.ContainsKey("is_cargo"));
            airlines_d_filtered = airlines_d_filtered.FindAll(x => x["is_scheduled"] == 1);

            Parallel.ForEach(airlines_d_filtered, new ParallelOptions { MaxDegreeOfParallelism = 5 }, (airline, state, index) =>
            {
                string airline_path = Path.Combine(AirlinesPath, airline["icao_code"] + ".png");
                if (!File.Exists(airline_path))
                {
                    Thread.Sleep(100);
                    try
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile("https://content.airhex.com/content/logos/airlines_" + airline["icao_code"] + "_100_100_s.png", airline_path);
                    }
                    catch
                    {
                    }
                }
            });
        }
#endif
    }
}
