using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TSP_Transponder.Models.API;
using static TSP_Transponder.Models.SimLibrary;

namespace TSP_Transponder.Models.Airports
{
    public partial class AirportsLib
    {
        private readonly Simulator Sim = null;
        internal Dictionary<string, Airport> AllAirports = new Dictionary<string, Airport>();
        internal Dictionary<char, Dictionary<char, Dictionary<char, List<Airport>>>> AirportsByChar = new Dictionary<char, Dictionary<char, Dictionary<char, List<Airport>>>>();
        internal Dictionary<string, List<Airport>> AirportsByTile = new Dictionary<string, List<Airport>>();

#if DEBUG
        internal static List<Airport> AirportRenderQueue = new List<Airport>();
        internal static Thread AirportRenderThread = null;
        internal AirportsScan APTScan;
#endif

        public AirportsLib(Simulator _Sim)
        {
            Sim = _Sim;
#if DEBUG
            APTScan = new AirportsScan(this, Sim);
#endif
        }
        
        public void Startup()
        {
            LoadDefaultSet();
#if DEBUG
            //APTScan.ScanLittleNavMap();

            //APTScan.AssignCountries(AllAirports.Values.ToList());
            //APTScan.AssignRadius(AllAirports.Values.ToList());
            //APTScan.AssignDensity(AllAirports);
            //APTScan.AssignFeatures(AllAirports);

            //APTScan.Dump(AllAirports.Values.ToList());
            //APTScan.CreateReport(AllAirports.Values.ToList());
#endif
        }

        //internal void CalculateRelevancy()
        //{
        //    foreach (Airport apt in AllAirports)
        //    {
        //        apt.CalculateRelevancy();
        //    }
        //}

        public void LoadDefaultSet()
        {
            /*
            lock(LiteDbService.DBApt)
            {
                var DBCollection = LiteDbService.DBApt.Database.GetCollection("airports");

                foreach(var entry in DBCollection.FindAll())
                {
                    Airport airport = new Airport();
                    airport.ICAO = entry["_id"];
                    airport.Name = entry["Name"];
                    airport.Location = new GeoLoc((double)entry["Lon"], (double)entry["Lat"]);
                    airport.Elevation = Convert.ToSingle((double)entry["Elevation"]);
                    airport.Country = entry["Country"];
                    airport.CountryName = entry["CountryName"];
                    airport.State = entry["State"];
                    airport.City = entry["City"];
                    airport.IsClosed = entry["IsClosed"];
                    airport.IsMilitary = entry["IsMilitary"];
                    airport.Density = Convert.ToUInt32((int)entry["Density"]);
                    airport.Relief = Convert.ToInt16((int)entry["Relief"]);
                    airport.Radius = Convert.ToSingle((double)entry["Radius"]);

                    foreach (string Rwy in ((string)entry["Runways"]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        airport.Runways.Add(new Airport.Runway(Rwy));
                    }

                    ushort i1 = 0;
                    foreach (string Pkg in ((string)entry["Parkings"]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        airport.Parkings.Add(new Airport.Parking(Pkg, i1));
                        i1++;
                    }

                    lock (AllAirports)
                    {
                        AllAirports.Add(airport.ICAO.ToUpper(), airport);
                    }

                    lock (AirportsByChar)
                    {
                        char ch1 = airport.ICAO[0];
                        char ch2 = airport.ICAO.Length > 1 ? airport.ICAO[1] : ' ';
                        char ch3 = airport.ICAO.Length > 2 ? airport.ICAO[2] : ' ';
                        char ch4 = airport.ICAO.Length > 3 ? airport.ICAO[3] : ' ';

                        if (!AirportsByChar.ContainsKey(ch1))
                        {
                            AirportsByChar.Add(ch1, new Dictionary<char, Dictionary<char, List<Airport>>>());
                        }
                        if (!AirportsByChar[ch1].ContainsKey(ch2))
                        {
                            AirportsByChar[ch1].Add(ch2, new Dictionary<char, List<Airport>>());
                        }
                        if (!AirportsByChar[ch1][ch2].ContainsKey(ch3))
                        {
                            AirportsByChar[ch1][ch2].Add(ch3, new List<Airport>());
                        }
                        AirportsByChar[ch1][ch2][ch3].Add(airport);
                    }

                    lock (AirportsByTile)
                    {
                        string Tile = airport.GetTile();
                        if (!AirportsByTile.ContainsKey(Tile))
                        {
                            AirportsByTile.Add(Tile, new List<Airport>());
                        }

                        AirportsByTile[Tile].Add(airport);
                    }

                }

            }
            */

            
            string AssetsCSV = App.ReadResourceFile("TSP_Transponder.Models.Airports.DefaultSet.apt_dump.csv");
            string[] t = AssetsCSV.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> ColumnsDescr = new List<string>();

            int i = 0;
            foreach(string Line in t)
            {
                if (i == 0)
                {
                    ColumnsDescr = t[0].Split('\t').ToList();
                }
                else
                {
                    int o = 0;
                    string[] AirportStruct = Line.Split('\t');
                    Airport Airport = new Airport()
                    {
                        Line = 1,
                        ICAO = AirportStruct[o++],
                        Name = AirportStruct[o++].Trim('\"'),
                        Location = new GeoLoc(Convert.ToDouble(AirportStruct[o++]), Convert.ToDouble(AirportStruct[o++])),
                        Elevation = Convert.ToSingle(AirportStruct[o++]),
                        Country = AirportStruct[o++],
                        CountryName = AirportStruct[o++],
                        State = AirportStruct[o++],
                        City = AirportStruct[o++],
                        IsClosed = Convert.ToBoolean(Convert.ToInt32(AirportStruct[o++])),
                        IsMilitary = Convert.ToBoolean(Convert.ToInt32(AirportStruct[o++])),
                        Density = Convert.ToUInt32(AirportStruct[o++]),
                        Relief = Convert.ToInt16(AirportStruct[o++]),
                        Radius = Convert.ToSingle(AirportStruct[o++]),
                    };
                                        
                    foreach (string Rwy in ((string)AirportStruct[o++]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        Airport.Runways.Add(new Airport.Runway(Rwy));
                    }

                    ushort i1 = 0;
                    foreach (string Pkg in ((string)AirportStruct[o++]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    {
                        Airport.Parkings.Add(new Airport.Parking(Pkg, i1));
                        i1++;
                    }

                    //AirportTaxiwayNodes.Add((string)AirportStruct[o++]);
                    //AirportTaxiwayPaths.Add((string)AirportStruct[o++]);

                    //foreach (string Pth in ((string)AirportStruct[o++]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    //{
                    //    Airport.TaxiPaths.Add(new Airport.TaxiwayPath(Pth));
                    //}

                    //i1 = 0;
                    //foreach (string nde in ((string)AirportStruct[o++]).Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    //{
                    //    Airport.TaxiNodes.Add(new Airport.TaxiwayNode(nde, i1));
                    //    i1++;
                    //}

                    lock (AllAirports)
                    {
                        AllAirports.Add(Airport.ICAO.ToUpper(), Airport);
                    }

                    lock(AirportsByChar)
                    {
                        char ch1 = Airport.ICAO[0];
                        char ch2 = Airport.ICAO.Length > 1 ? Airport.ICAO[1] : ' ';
                        char ch3 = Airport.ICAO.Length > 2 ? Airport.ICAO[2] : ' ';
                        char ch4 = Airport.ICAO.Length > 3 ? Airport.ICAO[3] : ' ';
                        
                        if (!AirportsByChar.ContainsKey(ch1))
                        {
                            AirportsByChar.Add(ch1, new Dictionary<char, Dictionary<char, List<Airport>>>());
                        }
                        if (!AirportsByChar[ch1].ContainsKey(ch2))
                        {
                            AirportsByChar[ch1].Add(ch2, new Dictionary<char, List<Airport>>());
                        }
                        if (!AirportsByChar[ch1][ch2].ContainsKey(ch3))
                        {
                            AirportsByChar[ch1][ch2].Add(ch3, new List<Airport>());
                        }
                        AirportsByChar[ch1][ch2][ch3].Add(Airport);
                    }

                    lock (AirportsByTile)
                    {
                        string Tile = Airport.GetTile();
                        if (!AirportsByTile.ContainsKey(Tile))
                        {
                            AirportsByTile.Add(Tile, new List<Airport>());
                        }

                        AirportsByTile[Tile].Add(Airport);
                    }
                    //Airport.GenerateImage();
                }

                i++;
            }

            /*
            string[] AirportsStruct = App.ReadResourceFile("TSP_Transponder.Models.Airports.DefaultSet.Lookup.txt").Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach(string AirportStr in AirportsStruct)
            {
                string[] AirportStruct = AirportStr.Split('\t');
                string[] LocationStruct = AirportStruct[4].Split(',');

                Airport Airport = new Airport()
                {
                    ICAO = AirportStruct[0],
                    Location = new Point(Convert.ToDouble(LocationStruct[0]), Convert.ToDouble(LocationStruct[1])),
                    Elevation = Convert.ToSingle(AirportStruct[5]),
                    Name = AirportStruct[6],
                    City = AirportStruct[7],
                    State = AirportStruct[8],
                    Country = AirportStruct[9],
                };

                lock (AllAirports)
                {
                    AllAirports.Add(Airport);
                }

                lock (AirportsByTile)
                {
                    string Tile = Airport.GetTile();
                    if (!AirportsByTile.ContainsKey(Tile))
                    {
                        AirportsByTile.Add(Tile, new List<Airport>());
                    }

                    AirportsByTile[Tile].Add(Airport);
                }

            }
            */

            //var test = AllAirports.OrderByDescending(x => x.Relief).ToList();

        }

        public void GatherAndSendTiles(SocketClient Socket, ArrayList matrix, string verb, Dictionary<string, dynamic> meta, bool IsDetailed)
        {
            int QueueCount = 0;
            Dictionary<string, Dictionary<string, Dictionary<string, dynamic>>> ReturnedTiles = new Dictionary<string, Dictionary<string, Dictionary<string, dynamic>>>();
            ReturnedTiles.Add(Sim.NameStandard, new Dictionary<string, Dictionary<string, dynamic>>());
            foreach (string Geo in matrix)
            {
                if (!ReturnedTiles[Sim.NameStandard].ContainsKey(Geo))
                {
                    ReturnedTiles[Sim.NameStandard].Add(Geo, new Dictionary<string, dynamic>());
                }

                List<Airport> GeoAirports = GetAirportsByTile(Geo);
                foreach (Airport Airport in GeoAirports)
                {
                    if (!ReturnedTiles[Sim.NameStandard][Geo].ContainsKey(Airport.ICAO))
                    {
                        ReturnedTiles[Sim.NameStandard][Geo].Add(Airport.ICAO, Airport.Serialize(null));
                        QueueCount++;
                    }
                }

                if (QueueCount > 200)
                {
                    if (Socket.IsConnected)
                    {
                        Socket.SendMessage(verb, App.JSSerializer.Serialize(ReturnedTiles), meta);
                        ReturnedTiles[Sim.NameStandard].Clear();
                        QueueCount = 0;
                        Thread.Sleep(200);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            meta["callbackType"] = 0;
            Socket.SendMessage(verb, App.JSSerializer.Serialize(ReturnedTiles), meta);
        }

        public void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "from-icaos":
                    {
                        //ReturnedTiles.Add(Sim.NameStandard, new Dictionary<string, dynamic>());
                        //
                        //List<Dictionary<string, dynamic>> Result = new List<Dictionary<string, dynamic>>();
                        //string[] ICAOs = ((string)payload_struct["icaos"]).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        //foreach(string ICAO in ICAOs)
                        //{
                        //    Airport APT = GetByICAO(ICAO);
                        //    GatherAndSendTiles(Socket, new ArrayList(1) { APT.GetTile() }, "airports:from-icaos", (Dictionary<string, dynamic>)structure["meta"], true);
                        //}

                        List<Dictionary<string, dynamic>> ReturnedAirports = new List<Dictionary<string, dynamic>>();
                        string[] ICAOs = ((string)payload_struct["icaos"]).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach(string ICAO in ICAOs)
                        {
                            Airport APT = GetByICAO(ICAO);
                            if(APT != null)
                                ReturnedAirports.Add(APT.Serialize(new Dictionary<string, dynamic>()
                                {
                                    { "_all", true },
                                }));
                        }


                        Socket.SendMessage("response", App.JSSerializer.Serialize(ReturnedAirports), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
                case "custom-from-tile":
                    {
                        GatherAndSendTiles(Socket, (ArrayList)payload_struct["matrix"], "airports:custom-from-tile", (Dictionary<string, dynamic>)structure["meta"], false);
                        break;
                    }
                case "from-coords":
                    {
                        List<Dictionary<string, dynamic>> ReturnedTiles = new List<Dictionary<string, dynamic>>();

                        double Detailed = (double)payload_struct["detail"];
                        double Radius = -0.5 + ((1 - Easings.Interpolate(Detailed / 23, Easings.EasingFunctions.CircularEaseOut)) * 3);
                        int LonMin = (int)Math.Floor((double)payload_struct["coords"]["nw"][0]);
                        int LonMax = (int)Math.Ceiling((double)payload_struct["coords"]["se"][0]);
                        int LatMin = (int)Math.Floor((double)payload_struct["coords"]["se"][1]);
                        int LatMax = (int)Math.Ceiling((double)payload_struct["coords"]["nw"][1]);

                        int Count = 0;

                        //if(Radius < 0.8)
                            //Radius = 0;

                        int LonAt = LonMin;
                        while(LonAt < LonMax)
                        {
                            int LatAt = LatMin;
                            while (LatAt < LatMax)
                            {
                                var outlist = new List<Dictionary<string, dynamic>>();
                                var lonRead = LonAt;
                                var latRead = LatAt;

                                if (lonRead > 180)
                                    lonRead -= 360;

                                if (latRead > 90)
                                    latRead -= 180;
                                
                                string geo = lonRead.ToString() + "_" + latRead.ToString();
                                
                                foreach (var airport in GetAirportsByTile(geo))
                                {
                                    if(Radius < airport.Radius)
                                    {
                                        if (Detailed > 10)
                                        {
                                            outlist.Add(airport.Serialize(new Dictionary<string, dynamic>()
                                            {
                                                { "_all", true }
                                            }));
                                        }
                                        else if (Detailed > 7)
                                        {
                                            outlist.Add(airport.Serialize(new Dictionary<string, dynamic>()
                                            {
                                                { "icao", true },
                                                { "name", true },
                                                { "location", true },
                                                { "wx", true },
                                                { "runways", true }
                                            }));
                                        }
                                        else
                                        {
                                            outlist.Add(airport.Serialize(new Dictionary<string, dynamic>()
                                            {
                                                { "icao", true },
                                                { "name", true },
                                                { "location", true },
                                            }));
                                        }
                                        Count++;
                                    }
                                }

                                if(outlist.Count > 0)
                                {
                                    ReturnedTiles.Add(new Dictionary<string, dynamic>()
                                    {
                                        { "name", geo },
                                        { "list", outlist }
                                    });
                                }

                                LatAt++;
                            }
                            LonAt++;
                        }


                        //Console.WriteLine("Returned " + Count);
                        Socket.SendMessage("response", App.JSSerializer.Serialize(ReturnedTiles), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }

        public List<Airport> Query(string SearchQuery)
        {
            Stopwatch SearchDuration = new Stopwatch();
            SearchDuration.Start();
            string LCQuery = SearchQuery.ToLower();
            List<Airport> Results = new List<Airport>();

            // Search ICAO
            Results = Results.Concat(AllAirports.Values.Where(x => x.ICAO.ToLower().Contains(LCQuery))).ToList();

            // Search Name
            Results = Results.Concat(AllAirports.Values.Where(x => x.Name.ToLower().Contains(LCQuery))).ToList();

            // Search City
            Results = Results.Concat(AllAirports.Values.Where(x => x.City.ToLower().Contains(LCQuery))).ToList();

            SearchDuration.Stop();

            Console.WriteLine("Results for '" + SearchQuery + "' in " + SearchDuration.ElapsedMilliseconds + "ms");
            return Results;
        }

        public Airport GetByICAO(string icao, GeoLoc location = null)
        {
            if(icao == null)
            {
                return null;
            }

            if(icao.Length < 3)
            {
                throw new Exception(icao + " does not exist");
            }

            string lcICAO = icao.ToUpper();
            if (AllAirports.ContainsKey(lcICAO))
            {
                return AllAirports[lcICAO];
            }

            if (location != null)
            {
                var closest = GetAirportByRange(location, 2).FirstOrDefault();
                if (closest.Value != null)
                {
                    return closest.Value;
                }
            }
            throw new Exception(icao + " does not exist");

            /*
            try
            {
                Airport result = null;
                string lcICAO = icao.ToUpper();
                char ch1 = lcICAO[0];
                char ch2 = lcICAO.Length > 1 ? lcICAO[1] : ' ';
                char ch3 = lcICAO.Length > 2 ? lcICAO[2] : ' ';
                char ch4 = lcICAO.Length > 3 ? lcICAO[3] : ' ';
                result = AirportsByChar[ch1][ch2][ch3].Find(x => x.ICAO == lcICAO);
                
                if (result != null)
                {
                    return result;
                }
                else
                {
                    result = AllAirports.Find(x => x.ICAO == lcICAO);
                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        throw new Exception(icao + " does not exist");
                    }
                }
            }
            catch
            {
                throw new Exception(icao + " does not exist");
            }
            */


        }

        public List<KeyValuePair<double, Airport>> GetAirportByRange(GeoLoc location, float rangeKM)
        {
            GeoLoc W = Utils.MapOffsetPosition(location, rangeKM * 1000, 270);
            GeoLoc N = Utils.MapOffsetPosition(location, rangeKM * 1000, 0);
            GeoLoc E = Utils.MapOffsetPosition(location, rangeKM * 1000, 90);
            GeoLoc S = Utils.MapOffsetPosition(location, rangeKM * 1000, 180);
            
            List<Airport> AirportsInRange = AllAirports.Values.Where(x =>
            {
                if (x.Location.Lon > W.Lon)
                {
                    if (x.Location.Lat < N.Lat)
                    {
                        if (x.Location.Lon < E.Lon)
                        {
                            if (x.Location.Lat > S.Lat)
                            {
                                if(Utils.MapCalcDist(location, x.Location, Utils.DistanceUnit.Kilometers, true) < rangeKM)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }).ToList();
            
            // Find Distances
            List<KeyValuePair<double, Airport>> ResultsSorted = new List<KeyValuePair<double, Airport>>();
            foreach (Airport airport in AirportsInRange)
            {
                double aptDist = Utils.MapCalcDist(airport.Location, location, Utils.DistanceUnit.Kilometers, true);
                ResultsSorted.Add(new KeyValuePair<double, Airport>(aptDist, airport));
            }

            // Sort by Distance & 
            List<KeyValuePair<double, Airport>> ResultSortedDictionary = ResultsSorted.OrderBy(x => x.Key).ToList();

            // Filter distances to range
            List<KeyValuePair<double, Airport>> FinalDictionary = ResultSortedDictionary.FindAll(x => x.Key < rangeKM).ToList();

            // Substitute if none
            if (FinalDictionary.Count == 0)
            {
                FinalDictionary.Add(new KeyValuePair<double, Airport>(0, null));
            }

            // Send Results
            return FinalDictionary;
        }
        
        public List<Airport> GetAirportsByTile(string geo)
        {
            if (AirportsByTile.ContainsKey(geo))
            {
                return AirportsByTile[geo];
            }
            else
            {
                return new List<Airport>();
            }
        }

        public List<Airport> GetAirportsCopy()
        {
            lock(AllAirports)
            {
                return AllAirports.Values.ToList();
            }
        }

        public List<Airport> GetAirportsByTileFromLonLat(int lon, int lat)
        {
            string geo = lon.ToString() + "_" + lat.ToString();
            if (AirportsByTile.ContainsKey(geo))
            {
                return AirportsByTile[geo];
            }
            else
            {
                return new List<Airport>();
            }
        }
        
    }

    public enum CustomLevel
    {
        Default = 0,
        Correction = 1,
        Bulk = 2,
        CustomAirport = 3,
        ComplexAirport = 4
    }
}
