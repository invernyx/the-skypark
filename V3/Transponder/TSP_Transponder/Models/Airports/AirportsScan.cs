using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;
using System.Windows;
using TSP_Transponder.Models.Topography;
using TSP_Transponder.Models.Topography.Utils;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.SimLibrary;
using System.Data.SQLite;

namespace TSP_Transponder.Models.Airports
{
    class AirportsScan
    {
        //private readonly string CacheVersion = "v2.6";
        private List<Airport> CSVAPTList = new List<Airport>();
        private Dictionary<string, Airport> CSVAPTDict = new Dictionary<string, Airport>();
        private Simulator Sim = null;

        internal string CacheDate = "";
        
        internal List<int> StatsValues = new List<int>()
        {
            0,
            0,
            0,
            0,
            0,
            0,
        };
        internal List<TextBlock> StatsFields = new List<TextBlock>();
        internal List<string> StatsParamsName = new List<string>()
        {
            "Airports",
            "Runways",
            "Parkings",
            "Default Apt",
            "Custom Apt",
            "Files",
        };

        internal AirportsScan(AirportsLib _APTLib, Simulator _Sim)
        {
            Sim = _Sim;

        }

        internal void AssignCountries(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Country Assignments");

            Parallel.ForEach(Apts, (newAirport) =>
            {
                Countries.Country c = Countries.GetCountry(newAirport.Location);
                newAirport.Country = c.Code;
                newAirport.CountryName = c.Name;
            });

            Console.WriteLine("Done with Country Assignments");
        }

        internal void AssignRadius(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Radius Assignments");
            
            foreach(Airport Apt in Apts)
            {
                List<GeoLoc> geoLocs = new List<GeoLoc>();

                foreach (Airport.Runway rw in Apt.Runways)
                {
                    geoLocs.Add(Utils.MapOffsetPosition(rw.Location.X, rw.Location.Y, ((rw.LengthFT * 0.3048) / 2), rw.Heading));
                    geoLocs.Add(Utils.MapOffsetPosition(rw.Location.X, rw.Location.Y, ((rw.LengthFT * 0.3048) / 2), rw.Heading + 180));

                    //double Rw1Dist = Utils.MapCalcDist(Apt.Location, Rw1, Utils.DistanceUnit.NauticalMiles);
                    //if (FarthestNode < Rw1Dist)
                    //{
                    //    FarthestNode = Rw1Dist;
                    //}

                    //double Rw2Dist = Utils.MapCalcDist(Apt.Location, Rw2, Utils.DistanceUnit.NauticalMiles);
                    //if (FarthestNode < Rw2Dist)
                    //{
                    //    FarthestNode = Rw2Dist;
                    //}
                }

                foreach (Airport.Parking pk in Apt.Parkings)
                {
                    geoLocs.Add(new GeoLoc(pk.Location.X, pk.Location.Y));
                    //double Pk1Dist = Utils.MapCalcDist(Apt.Location, new GeoLoc(pk.Location.X, pk.Location.Y), Utils.DistanceUnit.NauticalMiles);
                    //if (FarthestNode < Pk1Dist)
                    //{
                    //    FarthestNode = Pk1Dist;
                    //}
                }

                if(geoLocs.Count > 0)
                {
                    GeoLoc MinLoc = new GeoLoc(geoLocs.Min(x => x.Lon), geoLocs.Min(x => x.Lat));
                    GeoLoc MaxLoc = new GeoLoc(geoLocs.Max(x => x.Lon), geoLocs.Max(x => x.Lat));
                    GeoLoc AvgLoc = new GeoLoc(geoLocs.Average(x => x.Lon), geoLocs.Average(x => x.Lat));

                    double AptDist = Utils.MapCalcDist(MinLoc, MaxLoc, Utils.DistanceUnit.NauticalMiles, true);

                    Apt.Location = AvgLoc;
                    Apt.Radius = (float)Math.Round(Math.Max(0.1, AptDist), 5);
                }
                else
                {
                    Apt.Radius = 0.1f;
                }

            }

            Console.WriteLine("Done with Radius Assignments");
        }

        internal void AssignDensity(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Density Assignments");
            
            Parallel.ForEach(Apts, (Apt) =>
            {
                // Find Distances
                List<KeyValuePair<double, Airport>> Results = new List<KeyValuePair<double, Airport>>();

                Parallel.ForEach(Apts, (Apt1) =>
                {
                    if(Apt1 != Apt)
                    {
                        double aptDist = Utils.MapCalcDist(Apt.Location, Apt1.Location, Utils.DistanceUnit.Kilometers, true);
                        if(aptDist < 4000)
                        {
                            lock (Results)
                            {
                                double DistFactor = Math.Round(Math.Pow((1 / (aptDist / 3000)), 3), 5);
                                Results.Add(new KeyValuePair<double, Airport>(DistFactor, Apt1));
                            }
                        }
                    }
                });

                double Density = 0;
                //Results = Results.OrderByDescending(x => x.Key).ToList();
                foreach (var entry in Results)
                {
                    Density += entry.Key;
                }
                
                //List<Airport> radiused = RouteGenerator.FilterAirportsDistance(Apts, Apt.Location, 0, 1300);
                Apt.Density = (uint)Density;
            });

            //var test = Apts.OrderByDescending(x => x.Density);

            // Density
            Console.WriteLine("Done with Density Assignments");
        }

        internal void AssignFeatures(List<Airport> Apts)
        {
            Console.WriteLine("Starting with Relief Assignments");

            Topo.Startup();

            Func<List<PointElevation>, float, float, double> GetAmp = (List<PointElevation> Pos, float Dist, float Base) =>
            {
                return (Pos.Max(x => x.Elevation - Base) - Pos.Min(x => x.Elevation - Base)) / (Dist * 0.2f);
            };


            int Write = 0;
            int Count = 0;
            List<int> Rads = new List<int>() { 5, 10, 15 }; // 111 KM per unit
            Parallel.ForEach(Apts, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 100,
            },(apt) =>
            {
                apt.Relief = 0;

                // Base Altitude
                int Step = 20;
                TileDataSet tds = Topo.GetNewDataset();
                int BaseAlt = (int)Topo.GetElevation(apt.Location, tds);

                // Get radii
                List<int> Samples = new List<int>();
                double Angle = 0;
                int m = 1;
                foreach (var Rad in Rads)
                {
                    while (Angle < 360)
                    {
                        double y = (1f / 111) * Rad * Math.Cos(2 * Math.PI * Angle / 360);
                        double x = (1f / 111) * Rad * Math.Sin(2 * Math.PI * Angle / 360);
                        Angle += Step;
                
                        GeoLoc OffsetLocation = new GeoLoc(apt.Location.Lon + x, apt.Location.Lat + y);

                        if (OffsetLocation.Lon > 180) { OffsetLocation.Lon = 179.999; }
                        if (OffsetLocation.Lon < -180) { OffsetLocation.Lon = -179.999; }

                        if (OffsetLocation.Lat > 90) { OffsetLocation.Lat = 89.999; }
                        if (OffsetLocation.Lat < -90) { OffsetLocation.Lat = -89.999; }

                        int Elev = (int)(Topo.GetElevation(OffsetLocation, tds) - BaseAlt);
                        Samples.Add((int)Math.Round((float)Elev / m));
                    }
                    Angle = 0;
                    m *= 2;
                }

                //int sampleCount = 10;
                float RunwayLine = 0;

                foreach(var Rwy in apt.Runways)
                {
                    float thr = Rwy.LengthFT * 3.28084f * 0.5f;
                    float thr1 = thr * 1.5f;
                    float thr2 = thr * 2.5f;

                    //List<PointElevation> track = Topo.GetElevationAlongPath(Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr * 10, Rwy.Heading), Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr * 10, Rwy.Heading - 180), sampleCount * 2);
                    //List<double> leveledTrack = new List<double>();
                    //double trackFirst = track.GetRange(0, sampleCount).Average(x => x.Elevation - BaseAlt);
                    //double trackLast = track.GetRange(sampleCount, sampleCount).Average(x => x.Elevation - BaseAlt);
                    //double slope = trackFirst - trackLast;
                    //int prog = 1;
                    //foreach(var pt in track)
                    //{
                    //    double slopepcnt = 1f / track.Count * prog;
                    //    double rebase = pt.Elevation - BaseAlt - trackFirst;
                    //    double leveled = rebase + (slope * slopepcnt);
                    //    leveledTrack.Add(leveled > 0 ? leveled : leveled * 0.2);
                    //    
                    //    prog++;
                    //}
                    //double variance = 0;
                    //foreach(double elev in leveledTrack)
                    //{
                    //    variance += Math.Abs(elev);
                    //}
                    //
                    //RunwayLine += Convert.ToSingle(variance / apt.Runways.Count);
                    GeoLoc PrimRwy = Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr, Rwy.Heading);
                    GeoLoc SecRwy = Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr, Rwy.Heading - 180);

                    float PrimB = (float)Topo.GetElevation(PrimRwy, tds);
                    float PrimL = (float)GetAmp(Topo.GetElevationAlongPath(PrimRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading - 3), 5, tds), thr1, PrimB);
                    float PrimC = (float)GetAmp(Topo.GetElevationAlongPath(PrimRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr2, Rwy.Heading + 0), 5, tds), thr2, PrimB);
                    float PrimR = (float)GetAmp(Topo.GetElevationAlongPath(PrimRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading + 3), 5, tds), thr1, PrimB);

                    float SecB = (float)Topo.GetElevation(SecRwy, tds);
                    float SecL = (float)GetAmp(Topo.GetElevationAlongPath(SecRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading - 180 - 3), 5, tds), thr1, SecB);
                    float SecC = (float)GetAmp(Topo.GetElevationAlongPath(SecRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr2, Rwy.Heading - 180 + 0), 5, tds), thr2, SecB);
                    float SecR = (float)GetAmp(Topo.GetElevationAlongPath(SecRwy, Utils.MapOffsetPosition(Rwy.Location.X, Rwy.Location.Y, thr1, Rwy.Heading - 180 + 3), 5, tds), thr1, SecB);

                    float RwySum = 0;
                    RwySum += Math.Abs(PrimB - SecB);

                    RwySum += Math.Abs(PrimL) / apt.Runways.Count;
                    RwySum += Math.Abs(PrimC) / apt.Runways.Count;
                    RwySum += Math.Abs(PrimR) / apt.Runways.Count;
                    
                    RwySum += Math.Abs(SecL) / apt.Runways.Count;
                    RwySum += Math.Abs(SecC) / apt.Runways.Count;
                    RwySum += Math.Abs(SecR) / apt.Runways.Count;
                    
                    RwySum /= ((float)(Rwy.WidthMeters * 0.3048) + Rwy.LengthFT) * 0.001f;
                    
                    RunwayLine += RwySum;
                    
                }


                // Set on airport
                apt.Relief += (short)Math.Abs(Samples.Average() * 0.6); //(short)Math.Round(Convert.ToInt16(Samples.Max() - Samples.Min()) * 0.01f);
                apt.Relief += Convert.ToInt16(RunwayLine);
                
                Count++;
                Write++;
                if (Write > 1000)
                {
                    Write = 0;
                    Console.WriteLine("Progress: " + ((1f / Apts.Count) * Count) * 100);
                }
            });

            //var test = Apts.OrderByDescending(x => x.Relief).ToList();
            //int v = 0;
            //while(v < 100)
            //{
            //    Console.WriteLine(test[v].ICAO + " - " + test[v].Relief + " - " + test[v].Location.Lat + "," + test[v].Location.Lon);
            //    v++;
            //}

            Console.WriteLine("Done with Relief Assignments");
        }


        internal void ScanLittleNavMap()
        {
#if DEBUG
            string filePath = @"C:\Users\keven\AppData\Roaming\ABarthel\little_navmap_db\little_navmap_msfs.sqlite";

            File.Copy(filePath, filePath + "_001", true);
            
            //SQLITE_OPEN_FULLMUTEX
            SQLiteConnection sqlite_conn_0 = new SQLiteConnection(@"Data Source=" + filePath + ";");

            try
            {
                sqlite_conn_0.Open();

                #region Indices
                Action SetIndices = () =>
                {
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM sqlite_master where type='index'", sqlite_conn_0);
                    List<string> tables = new List<string>();
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(1));
                    }

                    if(!tables.Contains("taxi_path_idx"))
                    {
                        Console.WriteLine("Creating Index: taxi_path_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX taxi_path_idx ON taxi_path(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("apron_idx"))
                    {
                        Console.WriteLine("Creating Index: apron_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX apron_idx ON apron(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("start_idx"))
                    {
                        Console.WriteLine("Creating Index: start_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX start_idx ON start(start_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("runway_end_idx"))
                    {
                        Console.WriteLine("Creating Index: runway_end_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX runway_end_idx ON runway_end(runway_end_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("runway_idx"))
                    {
                        Console.WriteLine("Creating Index: runway_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX runway_idx ON runway(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                    if (!tables.Contains("parking_idx"))
                    {
                        Console.WriteLine("Creating Index: parking_idx");
                        using (var command = sqlite_conn_0.CreateCommand())
                        {
                            command.CommandText = "CREATE INDEX parking_idx ON parking(airport_id)";
                            command.ExecuteNonQuery();
                        }
                    }

                };
                SetIndices();
                #endregion
                
                #region TaxiwayPaths
                Func<int, Action<List<Airport.TaxiwayPath>, List<Airport.TaxiwayNode>>, Task> GetTaxiways = async (int AptID, Action<List<Airport.TaxiwayPath>, List<Airport.TaxiwayNode>> Callback) =>
                {
                    List<Airport.TaxiwayNode> newNodes = new List<Airport.TaxiwayNode>();
                    List<Airport.TaxiwayPath> newTaxiways = new List<Airport.TaxiwayPath>();

                    string Name = "";
                    double Width = 0;
                    Point Start = new Point();
                    Point End = new Point();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM taxi_path WHERE airport_id=" + AptID, sqlite_conn_0);
                    while (await reader.ReadAsync())
                    {
                        Name = reader.GetString(reader.GetOrdinal("name"));
                        Width = reader.GetDouble(reader.GetOrdinal("width"));
                        Start.X = reader.GetDouble(reader.GetOrdinal("start_lonx"));
                        Start.Y = reader.GetDouble(reader.GetOrdinal("start_laty"));
                        End.X = reader.GetDouble(reader.GetOrdinal("end_lonx"));
                        End.Y = reader.GetDouble(reader.GetOrdinal("end_laty"));

                        Airport.TaxiwayNode existingStart = newNodes.Find(x => x.Location.Lon == Start.X && x.Location.Lat == Start.Y);
                        if(existingStart == null)
                        {
                            existingStart = new Airport.TaxiwayNode(Convert.ToUInt16(newNodes.Count))
                            {
                                Location = new GeoLoc(Start.X, Start.Y)
                            };
                            newNodes.Add(existingStart);
                        }

                        Airport.TaxiwayNode existingEnd = newNodes.Find(x => x.Location.Lon == End.X && x.Location.Lat == End.Y);
                        if (existingEnd == null)
                        {
                            existingEnd = new Airport.TaxiwayNode(Convert.ToUInt16(newNodes.Count))
                            {
                                Location = new GeoLoc(Start.X, Start.Y)
                            };
                            newNodes.Add(existingEnd);
                        }

                        newTaxiways.Add(new Airport.TaxiwayPath()
                        {
                             Name = Name.Replace(',', ' ').Replace(';', ' ').Replace('\t', ' '),
                             Start = existingStart.ID,
                             End = existingEnd.ID,
                             Width = Convert.ToInt32(Width)
                        });
                    }

                    Callback(newTaxiways, newNodes);

                };
                #endregion

                #region Apron
                Func<int, Action<List<Airport.ApronGeometry>>, Task> GetApron = async (int AptID, Action<List<Airport.ApronGeometry>> Callback) =>
                {
                    List<Airport.ApronGeometry> geometryList = new List<Airport.ApronGeometry>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM apron WHERE airport_id=" + AptID, sqlite_conn_0);

                    while (await reader.ReadAsync())
                    {
                        Airport.ApronGeometry geometry = new Airport.ApronGeometry();
                        byte[] bytes = (byte[])reader.GetValue(reader.GetOrdinal("vertices"));
                        Array.Reverse(bytes, 0, bytes.Length);

                        int offset = 0;
                        while(offset + 4 < bytes.Length)
                        {
                            var lat = BitConverter.ToSingle(bytes, offset);
                            var lon = BitConverter.ToSingle(bytes, offset + 4);
                            geometry.Geometry.Add(new Point(lat, lon));
                            offset += 8;
                        }

                        geometryList.Add(geometry);
                    }

                    Callback(geometryList);
                };
                #endregion

                #region GetStart
                Func<int, string> GetStart = (StartID) =>
                {
                    string Name = "";
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM start WHERE start_id=" + StartID, sqlite_conn_0);
                    while (reader.Read())
                    {
                        Name = reader.GetString(reader.GetOrdinal("runway_name"));
                    }

                    return Name;
                };
                #endregion

                #region GetRunwayEnd
                Action<int, Action<string, bool, bool>> GetRunwayEnd = (EndID, Callback) =>
                {
                    string Name = "";
                    bool Closed = false;
                    bool ILS = false;
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM runway_end WHERE runway_end_id=" + EndID, sqlite_conn_0);
                    while (reader.Read())
                    {
                        Name = reader.GetString(reader.GetOrdinal("name"));
                        Closed = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("has_closed_markings")));
                        ILS = reader.GetValue(reader.GetOrdinal("ils_ident")).GetType() != typeof(DBNull);
                    }

                    Callback(Name, Closed, ILS);
                };
                #endregion

                #region GetRunways
                Func<int, Action<List<Airport.Runway>>, Task> GetRunways = async (int AptID, Action<List<Airport.Runway>> Callback) =>
                {
                    List<Airport.Runway> newRunways = new List<Airport.Runway>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM runway WHERE airport_id=" + AptID, sqlite_conn_0);

                    while (await reader.ReadAsync())
                    {
                        Airport.Runway Runway = new Airport.Runway();

                        string sfc = reader.GetString(reader.GetOrdinal("surface"));
                        Thread NT = new Thread(() =>
                        {
                            switch (sfc)
                            {
                                case "C": { Runway.Surface = Connectors.SimConnection.Surface.Concrete; break; }
                                case "G": { Runway.Surface = Connectors.SimConnection.Surface.Grass; break; }
                                case "W": { Runway.Surface = Connectors.SimConnection.Surface.Water; break; }
                                case "A": { Runway.Surface = Connectors.SimConnection.Surface.Asphalt; break; }
                                case "CE": { Runway.Surface = Connectors.SimConnection.Surface.Concrete; break; }
                                case "CL": { Runway.Surface = Connectors.SimConnection.Surface.Clay; break; }
                                case "SN": { Runway.Surface = Connectors.SimConnection.Surface.Snow; break; }
                                case "I": { Runway.Surface = Connectors.SimConnection.Surface.Ice; break; }
                                case "D": { Runway.Surface = Connectors.SimConnection.Surface.Dirt; break; }
                                case "R": { Runway.Surface = Connectors.SimConnection.Surface.Coral; break; }
                                case "GR": { Runway.Surface = Connectors.SimConnection.Surface.Gravel; break; }
                                case "OT": { Runway.Surface = Connectors.SimConnection.Surface.OilTreated; break; }
                                case "SM": { Runway.Surface = Connectors.SimConnection.Surface.SteelMats; break; }
                                case "B": { Runway.Surface = Connectors.SimConnection.Surface.Bituminous; break; }
                                case "BR": { Runway.Surface = Connectors.SimConnection.Surface.Brick; break; }
                                case "M": { Runway.Surface = Connectors.SimConnection.Surface.Macadam; break; }
                                case "P": { Runway.Surface = Connectors.SimConnection.Surface.Planks; break; }
                                case "S": { Runway.Surface = Connectors.SimConnection.Surface.Sand; break; }
                                case "SH": { Runway.Surface = Connectors.SimConnection.Surface.Shale; break; }
                                case "T": { Runway.Surface = Connectors.SimConnection.Surface.Tarmac; break; }
                                case "TR": { Runway.Surface = Connectors.SimConnection.Surface.Unknown; break; }
                            }
                        });
                        NT.CurrentCulture = CultureInfo.CurrentCulture;
                        NT.Start();
                        
                        GetRunwayEnd(reader.GetInt32(reader.GetOrdinal("primary_end_id")), (Name, Closed, ILS) =>
                        {
                            Runway.PrimaryClosed = Closed;
                            Runway.PrimaryILS = ILS;
                            Runway.Primary = Name;
                        });

                        GetRunwayEnd(reader.GetInt32(reader.GetOrdinal("secondary_end_id")), (Name, Closed, ILS) =>
                        {
                            Runway.SecondaryClosed = Closed;
                            Runway.SecondaryILS = ILS;
                            Runway.Secondary = Name;
                        });

                        if(Runway.SecondaryClosed && Runway.PrimaryClosed)
                        {
                            continue;
                        }
                        
                        Runway.AltitudeFeet = (short)reader.GetDouble(reader.GetOrdinal("altitude"));
                        Runway.Location = new Point(reader.GetDouble(reader.GetOrdinal("lonx")), reader.GetDouble(reader.GetOrdinal("laty")));
                        Runway.LengthFT = (ushort)reader.GetDouble(reader.GetOrdinal("length"));
                        Runway.WidthMeters = (ushort)reader.GetDouble(reader.GetOrdinal("width"));
                        Runway.Heading = reader.GetDouble(reader.GetOrdinal("heading"));

                        var edge_light = reader.GetValue(reader.GetOrdinal("edge_light"));
                        if(edge_light.GetType() != typeof(DBNull))
                        {
                            switch(edge_light)
                            {
                                case "L":
                                    {
                                        Runway.EdgeLight = 1;
                                        break;
                                    }
                                case "M":
                                    {
                                        Runway.EdgeLight = 2;
                                        break;
                                    }
                                case "H":
                                    {
                                        Runway.EdgeLight = 3;
                                        break;
                                    }
                            }
                        }

                        var center_light = reader.GetValue(reader.GetOrdinal("center_light"));
                        if (center_light.GetType() != typeof(DBNull))
                        {
                            switch (center_light)
                            {
                                case "L":
                                    {
                                        Runway.CenterLight = 1;
                                        break;
                                    }
                                case "M":
                                    {
                                        Runway.CenterLight = 2;
                                        break;
                                    }
                                case "H":
                                    {
                                        Runway.CenterLight = 3;
                                        break;
                                    }
                            }
                        }
                        
                        newRunways.Add(Runway);
                    }

                    Callback(newRunways);
                };
                #endregion

                #region GetParkings
                Func<int, Action<List<Airport.Parking>>, Task> GetParkings = async (int AptID, Action<List<Airport.Parking>> Callback) =>
                {
                    List<Airport.Parking> newParkings = new List<Airport.Parking>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM parking WHERE airport_id=" + AptID, sqlite_conn_0);

                    while (await reader.ReadAsync())
                    {
                        Airport.Parking Parking = new Airport.Parking((ushort)reader.GetInt32(reader.GetOrdinal("parking_id")));

                        Parking.Type = Airport.ParkingType.RampGAMedium;
                        Parking.Diameter = (float)reader.GetDouble(reader.GetOrdinal("radius")) * 2;
                        Parking.Number = (ushort)reader.GetInt32(reader.GetOrdinal("number"));
                        Parking.Location = new Point(reader.GetDouble(reader.GetOrdinal("lonx")), reader.GetDouble(reader.GetOrdinal("laty")));
                        Parking.Heading = (int)Math.Round(reader.GetDouble(reader.GetOrdinal("heading")));

                        newParkings.Add(Parking);
                    }

                    Callback(newParkings);
                };
                #endregion


                #region GetAirports
                Func<Action<List<Airport>>, Task> GetAirports = async (Callback) =>
                {
                    List<Airport> newAirports = new List<Airport>();
                    SQLiteDataReader reader = QuerySQLite("SELECT * FROM airport", sqlite_conn_0);

                    int i1 = 0;
                    foreach (string Name in reader.GetValues())
                    {
                        Console.WriteLine(i1 + ": " + Name);
                        i1++;
                    }

                    int ct = 0;
                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            int Index = 0;
                            Airport newAirport = new Airport();

                            Index = reader.GetOrdinal("ident");
                            if (!reader.IsDBNull(Index)) { newAirport.ICAO = reader.GetString(Index); }

                            Index = reader.GetOrdinal("name");
                            if (!reader.IsDBNull(Index)) { newAirport.Name = reader.GetString(Index).Replace("	", " "); }

                            Index = reader.GetOrdinal("city");
                            if (!reader.IsDBNull(Index)) { newAirport.City = reader.GetString(Index); }

                            Index = reader.GetOrdinal("state");
                            if (!reader.IsDBNull(Index)) { newAirport.State = reader.GetString(Index); }

                            newAirport.IsClosed = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("is_closed")));
                            newAirport.IsMilitary = Convert.ToBoolean(reader.GetInt32(reader.GetOrdinal("is_military")));
                            newAirport.Elevation = reader.GetInt32(reader.GetOrdinal("altitude"));
                            newAirport.Location = new GeoLoc(reader.GetDouble(reader.GetOrdinal("lonx")), reader.GetDouble(reader.GetOrdinal("laty")));
                        
                            await GetRunways(reader.GetInt32(0), (r) =>
                            {
                                newAirport.Runways = r;
                            });
                            await GetParkings(reader.GetInt32(0), (p) =>
                            {
                                newAirport.Parkings = p;
                            });
                            await GetApron(reader.GetInt32(0), (p) =>
                            {
                                newAirport.Aprons = p;
                            });
                            await GetTaxiways(reader.GetInt32(0), (paths, nodes) =>
                            {
                                newAirport.TaxiPaths = paths;
                                newAirport.TaxiNodes = nodes;
                            });

                            lock (newAirports)
                            {
                                newAirports.Add(newAirport);
                                ct++;
                                if (ct >= 500)
                                {
                                    Console.WriteLine("Apt Count " + newAirports.Count);
                                    ct = 0;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                    List<Airport> FoundUnknowns = newAirports.FindAll(x => x.Name == "Unknown Airport");
                    foreach (Airport Apt in FoundUnknowns)
                    {
                        Console.WriteLine("XXXXXXXXX " + Apt.ICAO + " / " + Apt.Name);
                    }

                    AssignCountries(newAirports);
                    AssignRadius(newAirports);
                    AssignDensity(newAirports);
                    AssignFeatures(newAirports);
                    Callback(newAirports);
                };
                #endregion
                
                GetAirports((simAirports) =>
                {
                    var newList = simAirports.FindAll(x => x.ICAO != "GLOB" && x.Runways.Count > 0);
                    Dump(newList);
                    CreateReport(newList);
                });
                Thread.Sleep(1000);
                //AssignCountries(simAirports);

                sqlite_conn_0.Close();
                App.MW.Shutdown();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed SQL scan: " + ex.Message);
            }
#endif
        }
        
#if DEBUG
        private SQLiteDataReader QuerySQLite(string Query, SQLiteConnection Connection)
        {
            SQLiteCommand sqlite_cmd = Connection.CreateCommand();
            sqlite_cmd.CommandText = Query; // Get all Root tables
            SQLiteDataReader sqlite_datareader = sqlite_cmd.ExecuteReader();
            var schema = sqlite_datareader.GetValues();

            return sqlite_datareader;
        }
#endif

        internal void CreateReport(List<Airport> Apts)
        {
#if DEBUG
            var AptsPrev = SimList[0].AirportsLib.GetAirportsCopy();
            string pat_dump_file = Path.Combine(App.AppDataDirectory, "Debug", "apt_report.csv");
            if (File.Exists(pat_dump_file))
                File.Delete(pat_dump_file);

            using (StreamWriter sw = File.CreateText(pat_dump_file))
            {
                sw.WriteLine("ICAO, Change");

                List<Airport> proc = new List<Airport>(AptsPrev);
                foreach(Airport apt in Apts)
                {
                    Airport aptPrev = AptsPrev.Find(x => x.ICAO == apt.ICAO);
                    if (aptPrev == null)
                    {
                        sw.WriteLine(apt.ICAO + ",Added");
                        continue;
                    }
                    else
                    {
                        proc.Remove(aptPrev);
                        List<string> change = new List<string>();

                        if (apt.IsMilitary != aptPrev.IsMilitary)
                        {
                            change.Add(apt.IsMilitary ? "IsMilitary" : "NotMilitary");
                        }

                        if (apt.IsClosed != aptPrev.IsClosed)
                        {
                            change.Add(apt.IsMilitary ? "IsClosed" : "NotClosed");
                        }

                        if (apt.Location.ToString() != aptPrev.Location.ToString())
                        {
                            change.Add("Location");
                        }

                        if (apt.Country != aptPrev.Country)
                        {
                            change.Add("Country");
                        }

                        if (apt.City != aptPrev.City)
                        {
                            change.Add("City");
                        }

                        if (apt.Name != aptPrev.Name)
                        {
                            change.Add("Name");
                        }

                        if (apt.TaxiParkingsToString() != aptPrev.TaxiParkingsToString())
                        {
                            change.Add("Parking");
                        }

                        if (apt.RunwaysToString() != aptPrev.RunwaysToString())
                        {
                            change.Add("Runway");
                        }

                        if (change.Count > 0)
                        {
                            sw.WriteLine(apt.ICAO + "," + string.Join(",", change));
                        }
                    }                     
                }

                foreach(Airport apt in proc)
                {
                    sw.WriteLine(apt.ICAO + ",Removed");
                }

            }
            Console.WriteLine("------------------------- APT Report Done");
#endif
        }

        internal void Dump(List<Airport> Apts)
        {
#if DEBUG
            #region Compile Airports for Database
            string apt_dump_file = Path.Combine(App.AppDataDirectory, "Debug", "apt_dump.csv");
            if (File.Exists(apt_dump_file))
            {
                File.Delete(apt_dump_file);
            }
            using (StreamWriter sw = File.CreateText(apt_dump_file))
            {
                sw.WriteLine("ICAO, Name, Lon, Lat, Elevation, Country, CountryName, Province, City, IsClosed, IsMilitary, Density, Relief, Runways, Taxis");

                int aptDumpCount = 0;
                foreach (Airport apt in Apts)
                {
                    aptDumpCount++;
                    sw.WriteLine(string.Join("\t", new List<string>()
                    {
                        apt.ICAO, // 0
                        "\"" + apt.Name + "\"", // 1
                        apt.Location.Lon.ToString(), // 2
                        apt.Location.Lat.ToString(), // 3
                        apt.Elevation.ToString(), // 4
                        apt.Country, // 5
                        apt.CountryName, // 6
                        apt.State, // 7
                        apt.City, // 8
                        Convert.ToInt32(apt.IsClosed).ToString(), // 9
                        Convert.ToInt32(apt.IsMilitary).ToString(), // 10
                        apt.Density.ToString(), // 11
                        apt.Relief.ToString(), // 12
                        apt.Radius.ToString(), // 13
                        apt.RunwaysToString(), // 14
                        apt.TaxiParkingsToString(), // 15
                    }));
                        
                    /*
                    lock (LiteDbService.DBApt)
                    {
                        var DBCollection = LiteDbService.DBApt.Database.GetCollection("airports");

                        var LastLocationBson = new BsonDocument();
                        LastLocationBson["TaxiwayPaths"] = apt.TaxiwayPathsToString();
                        LastLocationBson["TaxiwayNodes"] = apt.TaxiwayNodesToString();
                        LastLocationBson["Aprons"] = apt.ApronsToString();

                        DBCollection.Upsert(apt.ICAO, LastLocationBson);
                    }
                    */

                    //lock (LiteDbService.DBApt)
                    //{
                    //    var DBCollection = LiteDbService.DBApt.Database.GetCollection("airports");
                    //    DBCollection.EnsureIndex("_id", true);
                    //}
                }
                Console.WriteLine("------------------------- APT Dump Count: " + aptDumpCount);
            }
            #endregion

            #region Compile Airports for Stripr
            var apt_stripr_dump_file = Path.Combine(App.AppDataDirectory, "Debug", "apt_stripr.txt");
            var apt_stripr_dump_countries_file = Path.Combine(App.AppDataDirectory, "Debug", "apt_stripr_countries.txt");
            if (File.Exists(apt_stripr_dump_file)) File.Delete(apt_stripr_dump_file);
            if (File.Exists(apt_stripr_dump_countries_file)) File.Delete(apt_stripr_dump_countries_file);

            Dictionary<string, string> apt_countries = new Dictionary<string, string>();
            using (StreamWriter sw = File.CreateText(apt_stripr_dump_file))
            {
                int aptDumpCount = 0;
                foreach (Airport apt in Apts.Where(x => 
                    !x.IsClosed &&
                    x.Parkings.Count > 0 &&
                    x.Radius > 0.1 &&
                    x.Runways.Find(x1 =>  
                        x1.Surface != Connectors.SimConnection.Surface.Unknown
                    ) != null
                ))
                {
                    if(!apt_countries.ContainsKey(apt.Country))
                        apt_countries.Add(apt.Country, apt.CountryName);

                    aptDumpCount++;
                    sw.WriteLine(string.Join("\t", new List<string>()
                    {
                        apt.ICAO, // 0
                        apt.Name, // 1
                        apt.Location.Lon.ToString(), // 2
                        apt.Location.Lat.ToString(), // 3
                        apt.Elevation.ToString(), // 4
                        apt.Country, // 5
                        apt.CountryName, // 6
                        apt.State, // 7
                        apt.City, // 8
                        apt.Density.ToString(), // 9
                        apt.Relief.ToString(), // 10
                        apt.Radius.ToString(), // 11
                    }));
                }
            }
            using (StreamWriter sw = File.CreateText(apt_stripr_dump_countries_file))
            {
                foreach (var kvp in apt_countries.OrderBy(x => x.Value))
                {
                    sw.WriteLine(kvp.Key + "\t" + kvp.Value);
                }
            }

            #endregion
#endif
            }

        internal static SortedDictionary<int, List<string>> AddFileToLayers(int layer, SortedDictionary<int, List<string>> dict, string path)
        {
            if (!dict.ContainsKey(layer))
            {
                dict.Add(layer, new List<string>());
            }
            dict[layer].Add(path);
            return dict;
        }

        internal static SortedDictionary<int, List<string>> AddFilesToLayers(int layer, SortedDictionary<int, List<string>> dict, string[] paths)
        {
            if (!dict.ContainsKey(layer))
            {
                dict.Add(layer, new List<string>());
            }

            dict[layer].AddRange(paths);
            return dict;
        }

        private Dictionary<int, List<string>> AddToNewCache(int state, string line, Dictionary<int, List<string>> list)
        {
            if (!list.ContainsKey(state))
            {
                list.Add(state, new List<string>());
            }

            list[state].Add(line);
            return list;
        }
    }
}
