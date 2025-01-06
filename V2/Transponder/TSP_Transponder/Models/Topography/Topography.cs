using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Topography.Utils;

namespace TSP_Transponder.Models.Topography
{
    class Topo
    {
        private static string TopoDataFolder = Path.Combine(Path.GetDirectoryName(App.ThisApp.Location), "Topo");
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
#if DEBUG
                    TopoDataFolder = @"C:\Program Files\Parallel 42\The Skypark\Transponder\Topo";
#endif

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

        internal static double GetElevation(GeoLoc Loc, TileDataSet Dataset)
        {
            if(Math.Abs(Loc.Lon) < 180 && Math.Abs(Loc.Lat) < 90)
            {
                try
                {
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

            }
            return 0;
        }

        internal static List<PointElevation> GetElevationAlongPath(GeoLoc Loc1, GeoLoc Loc2, int Count, TileDataSet Dataset = null)
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
    }
}
