using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Models.PathFinding.PathFinding;
using TSP_Transponder.Models.PathFinding;
using static TSP_Transponder.Models.Connectors.SimConnection;
using System.Threading;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using TSP_Transponder.Models.Aircraft;
using static TSP_Transponder.Models.Airports.AirportsLib;
using System.Globalization;
using TSP_Transponder.Models.Airports;

namespace TSP_Transponder.Models.WorldManager
{
    internal class World
    {
        internal static List<TileInstance> AircraftTiles = new List<TileInstance>();
        internal static TilesLibrary TilesLib = new TilesLibrary();

        internal static List<Scene> Scenes = new List<Scene>();

        internal static List<Attached_Fx> AFXs = new List<Attached_Fx>();
        internal static List<Attached_Fx> UnconfirmedAFXs = new List<Attached_Fx>();
        internal static List<Attached_Fx> TrashAFXs = new List<Attached_Fx>();

        internal static List<Scene_Fx> FXs = new List<Scene_Fx>();
        internal static List<Scene_Fx> UnconfirmedFXs = new List<Scene_Fx>();
        internal static List<Scene_Fx> TrashFXs = new List<Scene_Fx>();

        internal static List<Scene_Obj> OBJs = new List<Scene_Obj>();
        internal static List<Scene_Obj> UnconfirmedOBJs = new List<Scene_Obj>();
        internal static List<Scene_Obj> TrashOBJs = new List<Scene_Obj>();

        internal static bool IsOffline = true;

        internal static object WorldBusy = new object();

        internal static void ConfirmID(long uid, uint SimID)
        {
            Scene_Obj UnconfirmedOBJ = null;
            lock (UnconfirmedOBJs)
            {
                UnconfirmedOBJ = UnconfirmedOBJs.Find(x => x.UID == uid);
                if (UnconfirmedOBJ != null)
                {
                    UnconfirmedOBJ.ConfirmID(SimID);
                    UnconfirmedOBJs.Remove(UnconfirmedOBJ);
                    Console.WriteLine("++++++++ Confirmed object ID " + uid + " as SimID " + SimID + " / " + UnconfirmedOBJ.File);

                }
            }

            Scene_Obj PendingDeletionOBJ = null;
            lock (OBJs)
            {
                lock (TrashOBJs)
                {
                    PendingDeletionOBJ = TrashOBJs.Find(x => x.UID == uid);
                    if (PendingDeletionOBJ != null)
                    {
                        Console.WriteLine("//////// Removing Pending object ID " + uid + " from Layer " + PendingDeletionOBJ.Layer);
                        lock (TrashOBJs)
                        {
                            TrashOBJs.Remove(PendingDeletionOBJ);
                        }
                        PendingDeletionOBJ.Deactivate();
                    }
                }
            }

            if(UnconfirmedOBJ != null && PendingDeletionOBJ == null)
            {
                Thread MonitorThread = new Thread(() =>
                {
                    Scene_Obj Monitored = null;
                    lock (OBJs)
                    {
                        Monitored = OBJs.Find(x => x.SimID == SimID);
                    }
                    if (Monitored != null)
                    {
                        while (Monitored.SimID > 0 && !App.MW.IsShuttingDown && ConnectedInstance != null)
                        {
                            ConnectedInstance.MonitorAI(Monitored.SimID);
                            Thread.Sleep(2000);
                        }
                    }
                });
                MonitorThread.IsBackground = true;
                MonitorThread.CurrentCulture = CultureInfo.CurrentCulture;
                MonitorThread.Start();
            }

            lock (UnconfirmedFXs)
            {
                Scene_Fx UnconfirmedFX = UnconfirmedFXs.Find(x => x.UID == uid);
                if (UnconfirmedFX != null)
                {
                    UnconfirmedFX.ConfirmID(SimID);
                    UnconfirmedFXs.Remove(UnconfirmedFX);
                    Console.WriteLine("++++++++ Confirmed effect ID " + uid + " as SimID " + SimID + " / " + UnconfirmedFX.File);
                }

                lock (TrashFXs)
                {
                    Scene_Fx PendingDeletionFX = TrashFXs.Find(x => x.UID == uid);
                    if (PendingDeletionFX != null)
                    {
                        ConnectedInstance.DestroyEffect(UnconfirmedFX.SimID);
                    }
                }
            }

            lock (UnconfirmedAFXs)
            {
                Attached_Fx UnconfirmedAFX = UnconfirmedAFXs.Find(x => x.UID == uid);
                if (UnconfirmedAFX != null)
                {
                    UnconfirmedAFX.ConfirmID(SimID);
                    UnconfirmedAFXs.Remove(UnconfirmedAFX);
                    Console.WriteLine("++++++++ Confirmed effect ID " + uid + " as SimID " + SimID + " / " + UnconfirmedAFX.File);
                }

                lock (TrashFXs)
                {
                    Attached_Fx PendingDeletionAFX = TrashAFXs.Find(x => x.UID == uid);
                    if (PendingDeletionAFX != null)
                    {
                        ConnectedInstance.DestroyEffect(UnconfirmedAFX.SimID);
                    }
                }
            }


        }

        internal static void UpdateObjectData(uint SimID, TemporalData td)
        {
            Scene_Obj obj = OBJs.Find(x => x.SimID == SimID);
            if(obj != null)
            {
                obj.UpdateTracking(td);
            }
        }
        
        internal static void UpdateAircraftTiles(GeoLoc Location, int AltitudeAGL, int Speed, int Course)
        {
            if (AltitudeAGL > 15000)
            {
                AircraftTiles.Clear();
                return;
            }

            List<Rect> Bounds = new List<Rect>();
            List<TileInstance> Tiles = new List<TileInstance>();

            short bound_w = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, 20000, 270).Lon + 180));
            short bound_n = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, 20000, 0).Lat + 90));
            short bound_e = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, 20000, 90).Lon + 180));
            short bound_s = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, 20000, 180).Lat + 90));

            List<short> lon_enum = new List<short>();
            short lon_seek = bound_w;
            while (lon_seek <= bound_e)
            {
                lon_enum.Add(lon_seek);
                lon_seek++;
            }

            List<short> lat_enum = new List<short>();
            short lat_seek = bound_s;
            while (lat_seek <= bound_n)
            {
                lat_enum.Add(lat_seek);
                lat_seek++;
            }

            foreach(short lat in lat_enum)
            {
                foreach (short lon in lon_enum)
                {
                    short new_lon = lon;
                    short new_lat = lat;

                    if (new_lon < 0)
                    {
                        new_lon += 360;
                    }

                    if (new_lat < 0)
                    {
                        new_lat += 180;
                    }

                    TileInstance tl = TilesLib.GetTile("_user", new GeoLoc(new_lon - 180, new_lat - 90));
                    if (!Tiles.Contains(tl))
                    {
                        Tiles.Add(tl);
                    }
                }
            }

            lock (AircraftTiles)
            {
                List<TileInstance> ToRemove = TilesLib.GetLayer("_user");

                foreach (TileInstance tile in Tiles)
                {
                    if (ToRemove.Contains(tile))
                    {
                        ToRemove.Remove(tile);
                    }
                }

                foreach (TileInstance tile in ToRemove)
                {
                    tile.Clear();
                    tile.Remove();
                }
                
                AircraftTiles = Tiles;


            }
            
        }
        
        internal static List<KeyValuePair<double, Scene_Obj>> GetClosestObjects(GeoLoc Location)
        {
            List<KeyValuePair<double, Scene_Obj>> Sorted = new List<KeyValuePair<double, Scene_Obj>>();
            lock (OBJs)
            {
                foreach(Scene_Obj OBJ in OBJs)
                {
                    if(OBJ.IsEnabled && OBJ.CullingVisible)
                    {
                        double Dist = Utils.MapCalcDist(Location, new GeoLoc(OBJ.Location), Utils.DistanceUnit.Meters);
                        Sorted.Add(new KeyValuePair<double, Scene_Obj>(Dist, OBJ));
                    }
                }
            }
            Sorted = Sorted.OrderBy(x => x.Key).ToList();
            return Sorted;
        }

        internal static GeoLoc FindSafeLocation(List<Scene_Obj> Exclude, GeoLoc Location, float DistanceMeters, float Bearing)
        {
            GeoLoc ResultGeo = Location.Copy();
            if(OBJs.Count > 0)
            {
                int iter = 0;
                double ClosestDist = 0;
                while (ClosestDist < DistanceMeters && iter < 100)
                {
                    List<KeyValuePair<double, Scene_Obj>> Sorted = new List<KeyValuePair<double, Scene_Obj>>();
                    lock (OBJs)
                    {
                        foreach (Scene_Obj OBJ in OBJs)
                        {
                            if (OBJ.IsEnabled && OBJ.CullingVisible && !Exclude.Contains(OBJ))
                            {
                                double Dist = Utils.MapCalcDist(ResultGeo, new GeoLoc(OBJ.Location), Utils.DistanceUnit.Meters);
                                Sorted.Add(new KeyValuePair<double, Scene_Obj>(Dist, OBJ));
                            }
                        }
                    }

                    if (Sorted.Count > 0)
                    {
                        Sorted = Sorted.OrderBy(x => x.Key).ToList();
                        ClosestDist = Sorted[0].Key;
                        ResultGeo = Utils.MapOffsetPosition(ResultGeo, DistanceMeters - ClosestDist, Bearing);
                    }
                    iter++;
                }
            }
            return ResultGeo;
        }

        public static List<List<Node>> GetNodeClustersOnAirport(SearchMap Map, Airport Airport, bool OnlyIfRunway = true)
        {
            List<Node> NodesPool = new List<Node>(Map.Nodes);
            List<List<Node>> NodesClusters = new List<List<Node>>();
            Node CurrentNode = null;
            int ClusterID = 0;
            while (NodesPool.Count > 0)
            {
                bool HasRunways = false;
                List<Node> Cluster = new List<Node>();
                CurrentNode = NodesPool[0];
                Cluster.Add(CurrentNode);

                Action<Node> Recurse = null;
                Recurse = delegate (Node node)
                {
                    int CurrentConnection = 0;
                    while (node.Connections.Count - CurrentConnection > 0)
                    {
                        Node ConnectedNode = node.Connections[CurrentConnection].ConnectedNode;
                        if (NodesPool.Contains(ConnectedNode))
                        {
                            bool HasDirectRunway = false;

                            if (ConnectedNode.Type == NodeType.Path)
                            {
                                Airport.TaxiwayPath ConnectingPath = Airport.TaxiPaths.Find(x => x.Start == ConnectedNode.Id || x.End == ConnectedNode.Id);
                                if (ConnectingPath != null && ConnectingPath.Type == Airport.TaxiwayPathType.Runway)
                                {
                                    HasRunways = true;
                                    HasDirectRunway = true;
                                }
                            }
                            else
                            {

                            }

                            ConnectedNode.Cluster = ClusterID;
                            if (!Cluster.Contains(ConnectedNode))
                            {
                                Cluster.Add(ConnectedNode);
                            }
                            NodesPool.Remove(ConnectedNode);
                            Recurse(ConnectedNode);

                            // Increate cost when linked to runway
                            if (HasDirectRunway)
                            {
                                Action<Node, uint> RunwayPropagate = null;
                                RunwayPropagate = delegate (Node n, uint from)
                                {
                                    if(n.Connections.Count <= 2)
                                    {
                                        foreach (Connection c in n.Connections)
                                        {
                                            if(c.ConnectedNode.Id != from)
                                            {
                                                if(double.IsNaN(c.Cost))
                                                {
                                                    c.Cost = 0;
                                                }
                                                c.Cost += 2;
                                                RunwayPropagate(c.ConnectedNode, n.Id);
                                            }
                                        }
                                    }
                                };
                                RunwayPropagate(ConnectedNode, node.Id);
                                
                            }
                        }

                        CurrentConnection++;
                    }
                };

                Recurse(CurrentNode);

                if (HasRunways || !OnlyIfRunway)
                {
                    NodesClusters.Add(Cluster);
                    ClusterID++;
                }

                if(NodesPool.Count > 0)
                {
                    NodesPool.RemoveAt(0);
                }
            }

            NodesClusters.Sort((a, b) => a.Count - b.Count);

            return NodesClusters;
        }

        public static List<Node> AirportToPathfindNodes(Airport Airport)
        {
            List<Node> Nodes = new List<Node>();

            // Loop all Taxi Nodes
            foreach (Airport.TaxiwayNode TaxiNode in Airport.TaxiNodes)
            {
                Nodes.Add(new Node()
                {
                    Id = TaxiNode.ID,
                    Type = NodeType.Path,
                    Location = TaxiNode.Location,
                    MinCostToStart = null,
                });
            }

            // Loop all Parking Nodes
            foreach (Airport.Parking Parking in Airport.Parkings)
            {
                Nodes.Add(new Node()
                {
                    Id = Parking.ID,
                    Type = NodeType.Parking,
                    Location = new GeoLoc(Parking.Location.X, Parking.Location.Y),
                    MinCostToStart = null,
                });
            }


            // Link paths to Nodes
            foreach (Airport.TaxiwayPath Path in Airport.TaxiPaths)
            {
                double CostMx = 0;

                switch (Path.Type)
                {
                    case Airport.TaxiwayPathType.Runway:
                    case Airport.TaxiwayPathType.Parking:
                        {
                            CostMx = 10;
                            break;
                        }
                }

                Node SearchNodeStart = null;
                Node SearchNodeEnd = null;

                switch (Path.Type)
                {
                    case Airport.TaxiwayPathType.Parking:
                        {
                            Airport.TaxiwayNode StartNode = Airport.TaxiNodes.Find(x => x.ID == Path.Start);
                            Airport.Parking EndNode = Airport.Parkings.Find(x => x.ID == Path.End);

                            if (StartNode != null && EndNode != null)
                            {
                                SearchNodeStart = Nodes.Find(x => x.Id == StartNode.ID && x.Type == NodeType.Path);
                                SearchNodeEnd = Nodes.Find(x => x.Id == EndNode.ID && x.Type == NodeType.Parking);

                            }
                            break;
                        }
                    default:
                        {
                            Airport.TaxiwayNode StartNode = Airport.TaxiNodes.Find(x => x.ID == Path.Start);
                            Airport.TaxiwayNode EndNode = Airport.TaxiNodes.Find(x => x.ID == Path.End);

                            if (StartNode != null && EndNode != null)
                            {
                                SearchNodeStart = Nodes.Find(x => x.Id == StartNode.ID && x.Type == NodeType.Path);
                                SearchNodeEnd = Nodes.Find(x => x.Id == EndNode.ID && x.Type == NodeType.Path);
                                
                            }
                            break;
                        }
                }


                if (SearchNodeStart != null && SearchNodeEnd != null)
                {
                    double dist = Utils.MapCalcDist(SearchNodeStart.Location, SearchNodeEnd.Location);

                    SearchNodeStart.Connections.Add(new Connection()
                    {
                        ConnectedNode = SearchNodeEnd,
                        Length = dist,
                        Cost = CostMx,
                    });

                    SearchNodeEnd.Connections.Add(new Connection()
                    {
                        ConnectedNode = SearchNodeStart,
                        Length = dist,
                        Cost = CostMx,
                    });
                }
            }



            /*
            // Link paths to Nodes
            foreach (Airport.TaxiwayNode Node in Airport.TaxiNodes)
            {
                List<Connection> Connections = new List<Connection>();
                foreach (Airport.TaxiwayPath Path in Airport.TaxiPaths)
                {
                    double CostMx = 0;

                    switch (Path.Type)
                    {
                        case Airport.TaxiwayPathType.Runway:
                        case Airport.TaxiwayPathType.Parking:
                            {
                                CostMx = 10;
                                break;
                            }
                    }

                    switch (Path.Type)
                    {
                        case Airport.TaxiwayPathType.Parking:
                            {
                                break;
                            }
                        default:
                            {

                                if (Path.Start == Node.ID)
                                {
                                    double dist = Utils.MapCalcDist(Node.Location.Y, Node.Location.X, Nodes[Node.ID].Location.Y, Nodes[Node.ID].Location.X);
                                    Connections.Add(new Connection()
                                    {
                                        ConnectedNode = Nodes[Path.End],
                                        Length = dist,
                                        Cost = CostMx,
                                    });
                                }

                                if (Path.End == Node.ID)
                                {
                                    double dist = Utils.MapCalcDist(Node.Location.Y, Node.Location.X, Nodes[Node.ID].Location.Y, Nodes[Node.ID].Location.X);
                                    Connections.Add(new Connection()
                                    {
                                        ConnectedNode = Nodes[Path.Start],
                                        Length = dist,
                                        Cost = CostMx,
                                    });
                                }
                                break;
                            }
                    }
                }

                Nodes[Node.ID].Connections = Connections;
            }
            */

            return Nodes;
        }

        internal static void Dispose()
        {
            TilesLib.Clear();
            lock (UnconfirmedOBJs)
            {
                UnconfirmedOBJs.Clear();
            }
            lock (TrashOBJs)
            {
                TrashOBJs.Clear();
            }
        }
    }

    internal class TilesLibrary
    {
        private Dictionary<string, List<TileInstance>> Layers = new Dictionary<string, List<TileInstance>>();
        
        internal List<TileInstance> Add(string layer)
        {
            lock (Layers)
            {
                if (!Layers.ContainsKey(layer))
                {
                    Layers.Add(layer, new List<TileInstance>());
                }
                return Layers[layer];
            }

        }

        internal void Remove(string layer)
        {
            lock (Layers)
            {
                if (Layers.ContainsKey(layer))
                {
                    lock (Layers[layer])
                    {
                        foreach (TileInstance tile in Layers[layer])
                        {
                            tile.Clear();
                        }
                    }
                }
            }
        }
        
        internal void Clear()
        {
            lock (Layers)
            {
                foreach (KeyValuePair<string, List<TileInstance>> layer in Layers)
                {
                    lock (layer.Value)
                    {
                        layer.Value.Clear();
                    }
                }
                Layers.Clear();
            }
        }

        internal List<TileInstance> GetLayer(string Layer)
        {
            lock (Layers)
            {
                if (Layers.ContainsKey(Layer))
                {
                    return new List<TileInstance>(Layers[Layer]);
                }
            }

            return new List<TileInstance>();
        }

        internal void ClearTileOnAllLayers(TileInstance Tile)
        {
            try
            {
                lock (Layers)
                {
                    Console.WriteLine("-------- Removing all Tiles in " + Tile.Name);

                    List<TileInstance> ToRemove = new List<TileInstance>();
                    foreach (KeyValuePair<string, List<TileInstance>> layer in Layers)
                    {
                        if (Tile.LayerName != layer.Key)
                        {
                            lock (layer.Value)
                            {
                                foreach (TileInstance layerTile in layer.Value)
                                {
                                    if (layerTile.Name == Tile.Name)
                                    {
                                        ToRemove.Add(layerTile);
                                    }
                                }
                            }
                        }
                    }

                    foreach (TileInstance tl in ToRemove)
                    {
                        tl.Remove();
                    }
                }
            }
            catch
            {

            }
        }

        internal TileInstance GetTile(string Layer, GeoLoc Location)
        {
            List<TileInstance> Tiles = Add(Layer);
            Rect Bounds = GetGeoTile(Location);
            string TileName = Bounds.X + "_" + Bounds.Y;
            TileInstance NewTile = Tiles.Find(x => x.Name == TileName);

            if(NewTile == null)
            {
                NewTile = new TileInstance(Layer, Tiles, Bounds);
                Tiles.Add(NewTile);
            }

            return NewTile;
        }
        
        internal static List<string> GetTileNameFromRadius(GeoLoc Location, double DistanceMeters)
        {
            List<Rect> Bounds = new List<Rect>();
            List<string> Tiles = new List<string>();

            short bound_w = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, DistanceMeters, 270).Lon + 180));
            short bound_n = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, DistanceMeters, 0).Lat + 90));
            short bound_e = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, DistanceMeters, 90).Lon + 180));
            short bound_s = Convert.ToInt16(Math.Floor(Utils.MapOffsetPosition(Location.Lon, Location.Lat, DistanceMeters, 180).Lat + 90));

            List<short> lon_enum = new List<short>();
            short lon_seek = bound_w;
            while (lon_seek <= bound_e)
            {
                lon_enum.Add(lon_seek);
                lon_seek++;
            }

            List<short> lat_enum = new List<short>();
            short lat_seek = bound_s;
            while (lat_seek <= bound_n)
            {
                lat_enum.Add(lat_seek);
                lat_seek++;
            }

            foreach (short lat in lat_enum)
            {
                foreach (short lon in lon_enum)
                {
                    short new_lon = lon;
                    short new_lat = lat;

                    if (new_lon < 0)
                    {
                        new_lon += 360;
                    }

                    if (new_lat < 0)
                    {
                        new_lat += 180;
                    }

                    string tl = (new_lon - 180) + "_" + (new_lat - 90);
                    if (!Tiles.Contains(tl))
                    {
                        Tiles.Add(tl);
                    }
                }
            }

            return Tiles;
            
        }
        
        internal Rect GetGeoTile(GeoLoc geo) // Tiles Logic
        {
            short lon = Convert.ToInt16(Math.Floor(geo.Lon + 180));
            short lat = Convert.ToInt16(Math.Floor(geo.Lat + 90));
            int width = 1;
            int height = 1;

            if (lat > 165) // North Cap
            {
                lon = 0;
                lat = 180;
                width = 360;
                height = 15;
            }
            else if (lat < 30) // South Cap
            {
                lon = 0;
                lat = 30;
                width = 1;
                height = 30;
            }

            return new Rect(lon - 180, lat - 90, width, height);
        }
        
    }

    
    internal class TileInstance
    {
        internal string Name = "";
        internal string LayerName = "";
        internal List<TileInstance> TilesList = null;
        internal Rect Bounds = new Rect();
        internal List<Scene_Obj> OBJs = new List<Scene_Obj>();
        internal List<Scene_Fx> FXs = new List<Scene_Fx>();

        internal TileInstance(string _Layer, List<TileInstance> _TilesList, Rect tileBounds)
        {
            TilesList = _TilesList;
            LayerName = _Layer;
            Bounds = (Rect)tileBounds;
            Name = Bounds.X + "_" + Bounds.Y;
            
            switch (LayerName)
            {
                case "_user":
                    {
                        Console.WriteLine("++++++++ Joined Tile " + Name + " in " + LayerName);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("++++++++ Created Tile " + Name + " in " + LayerName);
                        break;
                    }
            }

        }

        internal void Clear()
        {
            lock (OBJs)
            {
                foreach (Scene_Obj obj in OBJs)
                {
                    obj.Deactivate();
                }
                OBJs.Clear();
            }

            lock (FXs)
            {
                foreach (Scene_Fx fx in FXs)
                {
                    fx.Remove();
                }
                FXs.Clear();
            }
        }
        
        internal void Remove()
        {
            Clear();
            switch (LayerName)
            {
                case "_user":
                    {
                        Console.WriteLine("++++++++ Left Tile " + Name + " in " + LayerName);
                        World.TilesLib.ClearTileOnAllLayers(this);
                        break;
                    }
                default:
                    {
                        Console.WriteLine("++++++++ Cleared Tile " + Name + " in " + LayerName);
                        break;
                    }
            }

            TilesList.Remove(this);
        }

        internal void Remove(Scene_Obj obj)
        {
            lock (OBJs)
            {
                OBJs.Remove(obj);
            }
        }

        internal void Remove(Scene_Fx fx)
        {
            lock (FXs)
            {
                FXs.Remove(fx);
            }
        }

        internal void Add(Scene_Obj obj)
        {
            lock (OBJs)
            {
                OBJs.Add(obj);
            }
        }

        internal void Add(Scene_Fx fx)
        {
            lock (FXs)
            {
                FXs.Add(fx);
            }
        }
    }

    
    internal class Attached_Fx
    {
        internal long UID = 0;
        internal uint SimID = 0;
        internal string File = "";
        internal int Duration = -1;
        internal Point3D Location = new Point3D(0, 0, 0);

        internal Attached_Fx(Point3D _Location)
        {
            Location = _Location;
        }

        internal void ConfirmID(uint id)
        {
            SimID = id;
        }

        internal void Create()
        {
            if (SimID == 0)
            {
                lock (World.AFXs)
                {
                    World.AFXs.Add(this);
                }
                
                Send();
            }
        }

        internal void Remove()
        {
            if (SimID > 0)
            {
                Console.WriteLine("-------- Removing Effect " + File);
                lock (World.AFXs)
                {
                    World.AFXs.Remove(this);
                }

                ConnectedInstance.DestroyEffect(SimID);
                SimID = 0;
            }
            else
            {
                lock (World.TrashAFXs)
                {
                    World.TrashAFXs.Add(this);
                }
            }
        }

        private bool Send()
        {
            try
            {
                // Lock and add the new object to the tile
                lock (World.UnconfirmedFXs)
                {
                    // Check if the Object is already waiting for the sim
                    if (World.UnconfirmedFXs.Find(x => x.UID == UID) != null)
                    {
                        Console.WriteLine("++++++++ Trying to resend effect on unconfirmed ID " + UID);
                        return false;
                    }

                    World.UnconfirmedAFXs.Add(this);

                    // Send to Sim
                    Console.WriteLine("++++++++ Sending effect ID " + UID);
                    ConnectedInstance.CreateAttachedEffect(UID, File, Location, Duration);
                }

                if (Duration > -1)
                {
                    Thread RemoveCheck = new Thread(() =>
                    {
                        Thread.Sleep(Duration * 1000);
                        Remove();
                    });
                    RemoveCheck.IsBackground = true;
                    RemoveCheck.CurrentCulture = CultureInfo.CurrentCulture;
                    RemoveCheck.Start();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("++++++++ Unable to inject effect: " + ex.Message);
                return false;
            }
        }
    }

    
    internal class Scene_Fx
    {
        internal long UID = 0;
        internal uint SimID = 0;
        internal string File = "";
        internal string Layer = "";
        internal int Duration = -1;
        internal GeoPosition Location = new GeoPosition(0, 0);
        internal TileInstance Tile = null;

        internal Scene_Fx(string _Layer, GeoPosition _Location)
        {
            Layer = _Layer;
            if(_Location != null)
            {
                Relocate(_Location);
            }
        }

        internal void ConfirmID(uint id)
        {
            SimID = id;
        }

        internal void Relocate(GeoPosition NewLocation)
        {
            Location = NewLocation.Copy();
            Tile = World.TilesLib.GetTile(Layer, new GeoLoc(Location.Lon, Location.Lat));
        }

        internal void Create()
        {
            if (SimID == 0 && ConnectedInstance != null)
            {
                lock (World.FXs)
                {
                    World.FXs.Add(this);
                }

                Tile.Add(this);
                Send();
            }
        }

        internal void Remove()
        {
            if (SimID > 0)
            {
                Console.WriteLine("-------- Removing Effect " + File + " from Layer " + Layer);
                lock (World.FXs)
                {
                    World.FXs.Remove(this);
                }

                ConnectedInstance.DestroyEffect(SimID);
                SimID = 0;
            }
            else
            {
                lock (World.TrashFXs)
                {
                    World.TrashFXs.Add(this);
                }
            }
        }

        private bool Send()
        {
            try
            {
                // Lock and add the new object to the tile
                lock (World.UnconfirmedFXs)
                {
                    // Check if the Object is already waiting for the sim
                    if (World.UnconfirmedFXs.Find(x => x.UID == UID) != null)
                    {
                        Console.WriteLine("++++++++ Trying to resend effect on unconfirmed ID " + UID);
                        return false;
                    }

                    World.UnconfirmedFXs.Add(this);

                    // Send to Sim
                    Console.WriteLine("++++++++ Sending effect ID " + UID);
                    ConnectedInstance.CreateEffect(UID, File, Location, Duration);
                }

                if(Duration > -1)
                {
                    Thread RemoveCheck = new Thread(() =>
                    {
                        Thread.Sleep(Duration * 1000);
                        Remove();
                    });
                    RemoveCheck.IsBackground = true;
                    RemoveCheck.CurrentCulture = CultureInfo.CurrentCulture;
                    RemoveCheck.Start();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("++++++++ Unable to inject effect: " + ex.Message);
                return false;
            }
        }
    }

    
    internal class Scene_Obj
    {
        private bool _CullingVisible = false;
        internal bool CullingVisible {
            get
            {
                return _CullingVisible;
            }
            set
            {
                _CullingVisible = value;
                if (_CullingVisible)
                {
                    Send();
                    if (SpawnEffect != null)
                    {
                        SpawnEffect.Create();
                    }
                }
                else
                {
                    Cullout();
                }
            }
        }
        private bool _IsEnabled = false;
        internal bool IsEnabled {
            get
            {
                return _IsEnabled;
            }
            set
            {
                if (value)
                {
                    if (!_IsEnabled)
                    {
                        Activate();
                    }
                }
                else
                {
                    if (_IsEnabled)
                    {
                        Deactivate();
                    }
                }
            }
        }
        internal long UID = 0;
        internal string Name = "";
        internal uint SimID = 0;
        internal uint? AttachedTo = null;
        internal string SocketID = "";
        internal string Type = "";
        internal string File = "";
        internal string Layer = "";
        internal float Health = 100;
        internal float CullingDist = 1;
        internal SceneObjType ObjType = SceneObjType.Static;
        internal Scene_Fx SpawnEffect = null;
        internal Scene_Fx DeSpawnEffect = null;
        internal GeoPosition Location = null;
        internal VehMovement Movement = null;

        internal AircraftMountingPoint Mount = null;
        internal TileInstance Tile = null;
        internal TemporalData Data = null;
        private Stopwatch MonitorThreadTimer = new Stopwatch();
        private CancellationTokenSource MonitorThreadCancel = new CancellationTokenSource();

        internal Scene_Obj(string SocketID, string _Layer, GeoPosition _Location, SceneObjType _ObjType)
        {
            UID = Utils.GetRandom(8000, int.MaxValue);
            ObjType = _ObjType;
            Layer = _Layer;
            if(_Location != null)
            {
                Relocate(_Location, true);
            }
        }

        internal void Move()
        {
            if(Movement == null) { return; }

            Movement.Move((Position) => {
                if (Movement.Running)
                {
                    Location = Position.Position;
                    ConnectedInstance.MoveSimObject(SimID, Position);
                }
            });
        }

        internal void ConfirmID(uint id)
        {
            SimID = id;
            
            if (Movement != null)
            {
                Move();
            }
        }

        private bool IsInRange()
        {
            double Dist = Utils.MapCalcDist(new GeoLoc(Location), LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.NauticalMiles);
            if (Dist < CullingDist)
            {
                return true;
            }
            return false;
        }

        internal void Relocate(GeoPosition NewLocation, bool Passive = false)
        {
            Location = NewLocation.Copy();

            TileInstance TI = World.TilesLib.GetTile(Layer, new GeoLoc(Location.Lon, Location.Lat));
            if (TI != Tile)
            {
                if (Tile != null)
                {
                    lock (Tile)
                    {
                        Tile.Remove(this);
                    }
                }
                Tile = TI;
                lock (Tile)
                {
                    Tile.Add(this);
                }
            }

            if (!Passive)
            {
                if (SimID > 0)
                {
                    if (IsInRange())
                    {
                        ConnectedInstance.MoveSimObject(SimID, new VehMovement.MovementState()
                        {
                            Position = Location,
                        });
                    }
                }
            }

            if (SpawnEffect != null)
            {
                SpawnEffect.Relocate(Location);
            }

            if (DeSpawnEffect != null)
            {
                DeSpawnEffect.Relocate(Location);
            }
        }

        internal void AttachTo(uint uid, AircraftMountingPoint mount)
        {
            AttachedTo = uid;
            Mount = mount;
            ConnectedInstance.AttachSimObject(SimID, uid, mount);
            Tile.Remove(this);
            Tile = null;
        }

        internal void Detach()
        {
            if(AttachedTo != null)
            {
                Mount.Occupied = false;
                Mount = null;
                Relocate(Location, true);
                ConnectedInstance.ReleaseSimObject(SimID, (uint)AttachedTo);
            }
        }

        internal void Activate()
        {
            if (SimID == 0 && !_IsEnabled)
            {
                _IsEnabled = true;

                lock (World.TrashOBJs)
                {
                    if (World.TrashOBJs.Contains(this))
                    {
                        World.TrashOBJs.Remove(this);
                    }
                }

                lock (World.OBJs)
                {
                    World.OBJs.Add(this);
                }

                lock (Tile)
                {
                    Tile.Add(this);
                }
            }
            
            if (MonitorThreadTimer.IsRunning)
            {
                MonitorThreadTimer.Stop();
                if(!MonitorThreadCancel.IsCancellationRequested)
                {
                    MonitorThreadCancel.Cancel();
                    MonitorThreadCancel.Dispose();
                }
            }

            MonitorThreadCancel = new CancellationTokenSource();

            Task.Factory.StartNew(async () => {

                MonitorThreadTimer.Restart();

                while (!App.MW.IsShuttingDown)
                {
                    if (_IsEnabled)
                    {
                        if (IsInRange())
                        {
                            if (!CullingVisible && SimID == 0)
                            {
                                CullingVisible = true;
                                await Task.Delay(Utils.GetRandom(World.OBJs.Count * 10));
                                continue;
                            }
                        }
                        else
                        {
                            if (CullingVisible && SimID > 0)
                            {
                                CullingVisible = false;
                            }
                        }
                    }
                    await Task.Delay(1000 + ((World.OBJs.Count - 1) * 10));
                }
                
            }, MonitorThreadCancel.Token);
            
        }

        internal void Cullout()
        {
            if(ConnectedInstance != null)
            {
                if (SimID > 0)
                {
                    uint id = SimID;
                    SimID = 0;
                    ConnectedInstance.DestroySimObject(id);
                }
                else
                {
                    lock (World.TrashOBJs)
                    {
                        if (!World.TrashOBJs.Contains(this))
                        {
                            World.TrashOBJs.Add(this);
                        }
                    }
                }
            }
        }

        internal void Deactivate()
        {
            _IsEnabled = false;
            CullingVisible = false;

            if (MonitorThreadTimer.IsRunning)
            {
                MonitorThreadTimer.Stop();
                MonitorThreadCancel.Cancel(false);
            }

            if (SimID > 0)
            {
                //Console.WriteLine("-------- Removing Simobject " + File + " with ID " + UID + " from Layer " + Layer);
                lock (World.OBJs)
                {
                    World.OBJs.Remove(this);
                }

                if (DeSpawnEffect != null)
                {
                    DeSpawnEffect.Create();
                }
            }
        }
        
        private bool Send()
        {
            try
            {
                // Lock and add the new object to the tile
                lock (World.UnconfirmedOBJs)
                {
                    // Check if the Object is already waiting for the sim
                    if (World.UnconfirmedOBJs.Find(x => x.UID == UID) != null)
                    {
                        Console.WriteLine("++++++++ Trying to resend object on unconfirmed ID " + UID);
                        return false;
                    }

                    World.UnconfirmedOBJs.Add(this);

                    // Send to Sim
                    Console.WriteLine("++++++++ Sending object " + File + " (ID " + UID + ")");
                    ConnectedInstance.CreateSimObject(UID, Utils.GetHash1(File), Location, ObjType);
                }


                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("++++++++ Unable to inject Simobject: " + ex.Message);
                return false;
            }
        }

        public void UpdateTracking(TemporalData td)
        {
            Data = td;

            if(Data.PLANE_LOCATION.Lon != Location.Lon || Data.PLANE_LOCATION.Lat != Location.Lat || Data.PLANE_ALTITUDE != Location.Alt)
            {
                Relocate(new GeoPosition(Data.PLANE_LOCATION, Data.PLANE_ALTITUDE, Data.PLANE_HEADING_DEGREES), true);
            }

        }
        
    }
    
    public enum SceneObjType
    {
        Dynamic,
        Static
    }
}
