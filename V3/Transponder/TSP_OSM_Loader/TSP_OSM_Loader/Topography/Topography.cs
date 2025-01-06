using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TSP_OSM_Loader.Topography.Utils;

namespace TSP_OSM_Loader.Topography
{
    class Topo
    {
        internal static string TopoDataFolder = @"C:\Program Files\Parallel 42\The Skypark\Transponder\Topo";
        private static MapData MapData = null;

        internal static TileDataSet GetNewDataset()
        {
            return new TileDataSet(Path.Combine(TopoDataFolder, "meta.json"));
        }

        internal static void Startup()
        {
            if(MapData == null)
            {
                try
                {
                    GdalConfiguration.ConfigureGdal();
                    MapData = new MapData(TopoDataFolder);
                    MapData.BuildMetaFile();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to init Gdal: " + ex.Message + " - " + ex.StackTrace);
                }
            }
        }

        internal static double GetRelief(GeoLoc location, TileDataSet Dataset)
        {
            List<int> Rads = new List<int>() { 5, 10, 15 }; // 111 KM per unit

            // Base Altitude
            int Step = 20;
            int BaseAlt = (int)GetElevation(location, Dataset);

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

                    GeoLoc OffsetLocation = new GeoLoc(location.Lon + x, location.Lat + y);
                    int Elev = (int)(GetElevation(OffsetLocation, Dataset) - BaseAlt);
                    Samples.Add((int)Math.Round((float)Elev / m));
                }
                Angle = 0;
                m *= 2;
            }

            return Math.Abs(Samples.Average() * 0.6);
        }

        internal static double GetElevation(GeoLoc Loc, TileDataSet Dataset)
        {
            try
            {
                if (Loc.Lon < -180 || Loc.Lon > 180 || Loc.Lat < -90 || Loc.Lat > 90) return 0;

                if (Directory.Exists(TopoDataFolder))
                {
                    // Read the meta file.
                    return Dataset.GetElevationForPoint(new PointElevation { Lat = Loc.Lat, Lon = Loc.Lon }).Elevation;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to init Gdal: " + ex.Message + " - " + ex.StackTrace);
            }
            return 0;
        }

        internal static List<PointElevation> GetElevationAlongPath(GeoLoc Loc1, GeoLoc Loc2, int Count, TileDataSet Dataset)
        {
            List<PointElevation> Output = new List<PointElevation>();
            try
            {
                if (Directory.Exists(TopoDataFolder))
                {
                    var fp = new FlightPath();
                    var points = fp.GetPath(new PointElevation { Lat = Loc1.Lat, Lon = Loc1.Lon }, new PointElevation { Lat = Loc2.Lat, Lon = Loc2.Lon }, Count);

                    if(Dataset == null)
                    {
                        // Read the meta file.
                        using (var tds = new TileDataSet(Path.Combine(TopoDataFolder, "meta.json")))
                        {
                            var pointsWithElevation = tds.GetElevationForPoints(points);
                            foreach (var point in pointsWithElevation)
                            {
                                Output.Add(point);
                            }
                        }
                    }
                    else
                    {
                        var pointsWithElevation = Dataset.GetElevationForPoints(points);
                        foreach (var point in pointsWithElevation)
                        {
                            Output.Add(point);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to init Gdal: " + ex.Message + " - " + ex.StackTrace);
            }

            if(Output.Count == 0)
            {
                Output.Add(new PointElevation(0));
                Output.Add(new PointElevation(0));
            }
            
            return Output;
        }

        /*
        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "getForPath":
                    {
                        List<List<int>> ReturnData = new List<List<int>>();
                        try
                        {
                            foreach(var path in payload_struct["paths"])
                            {
                                List<int> ReturnPathData = new List<int>();

                                GeoLoc Pos1 = new GeoLoc((double)path[0][0], (double)path[0][1]);
                                GeoLoc Pos2 = new GeoLoc((double)path[1][0], (double)path[1][1]);
                                double Dist = TSP_Transponder.Utils.MapCalcDist(Pos1, Pos2, TSP_Transponder.Utils.DistanceUnit.NauticalMiles);
                                int Count = (int)(payload_struct.ContainsKey("resolution") ? Dist * payload_struct["resolution"] : 1);

                                List<PointElevation> Pts = GetElevationAlongPath(Pos1, Pos2, Count < 100 ? 100 : Count);
                                foreach (var Pt in Pts)
                                {
                                    ReturnPathData.Add((int)Pt.Elevation);
                                }

                                ReturnData.Add(ReturnPathData);
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to load Topography: " + ex.Message);
                        }
                        Socket.SendMessage("topography:getForPath", App.JSSerializer.Serialize(ReturnData), (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }
        */
    }
}
