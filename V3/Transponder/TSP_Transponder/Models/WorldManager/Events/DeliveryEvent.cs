using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TSP_Transponder.Models.PathFinding;
using static TSP_Transponder.Models.PathFinding.PathFinding;
using static TSP_Transponder.Models.Connectors.SimConnection;
using System.Globalization;
using TSP_Transponder.Models.Airports;

namespace TSP_Transponder.Models.WorldManager.Events
{
    
    class DeliveryEvent
    {
        public bool IsRunning = false;
        private string SocketID = "";
        private bool Destroyed = false;
        private Airport Airport = null;
        private List<List<Node>> NodesClusters = null;
        private SearchMap SearchMap = null;
        private List<Scene_Obj> Objects = new List<Scene_Obj>();
        private List<Scene_Fx> Effects = new List<Scene_Fx>();
        private Action DoneAction = null;
        private Scene_Obj DeliveryVeh = null;
        private Scene_Obj DeliveryObj = null;

        private string VehicleModel = "";

        public DeliveryEvent(string _SocketID, Scene_Obj Obj, string Vehicle, Action Done)
        {
            SearchMap = new SearchMap();
            SocketID = _SocketID;
            DeliveryObj = Obj;
            VehicleModel = Vehicle;

            DoneAction = Done;
        }

        public void Start()
        {
            //CreateInstance("TSP_box", SceneObjType.Dynamic, null, new GeoPosition(Utils.MapOffsetPosition(
            //    Connectors.Connector.LastTemporalData.PLANE_LOCATION,
            //    (float)((Connectors.Connector.Aircraft.Wingspan * 0.5) / 1000),
            //    (float)(Connectors.Connector.LastTemporalData.PLANE_HEADING_DEGREES - 45)), 0, Connectors.Connector.LastTemporalData.PLANE_HEADING_DEGREES));
            //
            //GeoPosition friendLoc = new GeoPosition(Utils.MapOffsetPosition(
            //    Connectors.Connector.LastTemporalData.PLANE_LOCATION,
            //    (float)((Connectors.Connector.Aircraft.Wingspan * 1.1) / 1000),
            //    (float)(Connectors.Connector.LastTemporalData.PLANE_HEADING_DEGREES + 60)), 0, Connectors.Connector.LastTemporalData.PLANE_HEADING_DEGREES);
            //CreateInstance("C208B Grand Caravan EX N229TB", SceneObjType.Dynamic, null, friendLoc);
            //CreateInstance("TSP_box", SceneObjType.Dynamic, null, new GeoPosition(Utils.MapOffsetPosition(
            //    new GeoLoc(friendLoc.Lon, friendLoc.Lat),
            //    (float)((Connectors.Connector.Aircraft.Wingspan * 0.5) / 1000),
            //    (float)(Connectors.Connector.LastTemporalData.PLANE_HEADING_DEGREES - 45)), 0, Connectors.Connector.LastTemporalData.PLANE_HEADING_DEGREES));

            IsRunning = true;


            #region Find Airport and make Search Map
            // Find closest Airport
            Airport = Connectors.SimConnection.ActiveSim.AirportsLib.GetAirportByRange(Connectors.SimConnection.LastTemporalData.PLANE_LOCATION, 10)[0].Value;
            if (Airport == null)
            {
                Airport = new Airport();
            }

            // Add Nodes to SearchMap
            SearchMap.Nodes = World.AirportToPathfindNodes(Airport);

            // Make Clusters
            NodesClusters = World.GetNodeClustersOnAirport(SearchMap, Airport, true);
            #endregion

            SetState(Steps.ToAircraft);

        }

        public void Destroy()
        {
            Destroyed = true;
            foreach (Scene_Obj Obj in Objects)
            {
                if(Obj.Movement != null)
                {
                    Obj.Movement.Stop();
                }
                Obj.IsEnabled = false;
            }
            IsRunning = false;
        }

        public void SetState(Steps Step)
        {

            switch (Step)
            {
                case Steps.ToAircraft:
                    {
                        Thread StepThread = new Thread(() =>
                        {
                            bool Instant = false;
                            Random rnd = new Random();
                            GeoLoc AircraftLocation = LastTemporalData.PLANE_LOCATION;

                            if(LastTemporalData.SURFACE_TYPE != Surface.Water)
                            {
                                if (NodesClusters.Count > 0)
                                {
                                    // Parking Nodes Sorted by distance
                                    Dictionary<Node, double> SortedParkings = PathFindingHelpers.SortNodesByDistance(NodesClusters[0].FindAll(x => x.Type == NodeType.Parking), AircraftLocation);
                                    if (SortedParkings.Count > 0)
                                    {
                                        #region Find Start/End
                                        Node StartParking = SortedParkings.LastOrDefault(x => x.Value < 1000).Key;
                                        if (StartParking == null)
                                        {
                                            StartParking = SortedParkings.First().Key;
                                        }


                                        // Nodes Sorted by distance
                                        Dictionary<Node, double> SortedCluster = PathFindingHelpers.SortNodesByDistance(NodesClusters[0], AircraftLocation);
                                        List<Node> Exclusion = SortedCluster.Keys.Where(x => Utils.MapCalcDist(x.Location, AircraftLocation) > Connectors.SimConnection.Aircraft.Wingspan || x.Type == NodeType.Parking).ToList();
                                        foreach (Node nd in Exclusion)
                                        {
                                            SortedCluster.Remove(nd);
                                        }

                                        // Find closest Segment
                                        KeyValuePair<List<Node>, GeoLoc>? ClosestSegment = PathFindingHelpers.GetClosestSegment(SortedCluster.Keys.ToList(), AircraftLocation, Connectors.SimConnection.Aircraft.Wingspan);

                                        // Start/End Point
                                        SearchMap.StartNode = StartParking;

                                        double DistanceToClosestSegment = 0;
                                        if (ClosestSegment != null)
                                        {
                                            DistanceToClosestSegment = Utils.MapCalcDist(((KeyValuePair<List<Node>, GeoLoc>)ClosestSegment).Value, AircraftLocation, Utils.DistanceUnit.Meters);
                                        }

                                        if (ClosestSegment != null && DistanceToClosestSegment < Utils.MapCalcDist(SortedCluster.Keys.FirstOrDefault().Location, AircraftLocation, Utils.DistanceUnit.Meters) && DistanceToClosestSegment > Connectors.SimConnection.Aircraft.Wingspan)
                                        {
                                            Node nn = new Node()
                                            {
                                                Id = (ushort)SearchMap.Nodes.Count,
                                                Location = ((KeyValuePair<List<Node>, GeoLoc>)ClosestSegment).Value,
                                                Type = NodeType.Path,
                                                Cluster = 0,
                                            };

                                            foreach (Node nd in ((KeyValuePair<List<Node>, GeoLoc>)ClosestSegment).Key)
                                            {
                                                double dist = Utils.MapCalcDist(nd.Location, nn.Location, Utils.DistanceUnit.Meters);
                                                nn.Connections.Add(new Connection()
                                                {
                                                    ConnectedNode = nd,
                                                    Cost = 0,
                                                    Length = dist,
                                                });

                                                nd.Connections.Add(new Connection()
                                                {
                                                    ConnectedNode = nn,
                                                    Cost = 0,
                                                    Length = dist,
                                                });
                                            }

                                            SearchMap.EndNode = nn;
                                        }
                                        else
                                        {
                                            SearchMap.EndNode = SortedCluster.Keys.Where(x => Utils.MapCalcDist(x.Location, Connectors.SimConnection.LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.Meters) > Connectors.SimConnection.Aircraft.Wingspan).FirstOrDefault();
                                        }
                                        #endregion

                                        #region Find Path
                                        // Query the shortest path
                                        Query QueryResponse = new Query(SearchMap);
                                        QueryResponse.Process();

                                        // Extract Start / End Nodes
                                        Node TestObj_start = QueryResponse.Map.ShortestPath.First();
                                        Node TestObj_end = QueryResponse.Map.ShortestPath.Last();
                                        #endregion

                                        // Delivery Truck
                                        VehMovement VEH1_Move = new VehMovement();

                                        #region Done Action
                                        VEH1_Move.DoneAction = () =>
                                        {
                                            //Thread.Sleep(3000);
                                            if (!Destroyed)
                                            {
                                                int Hdg = Utils.GetRandom(0, 359);
                                                GeoPosition BoxLocation = new GeoPosition(Utils.MapOffsetPosition(
                                                    new GeoLoc(DeliveryVeh.Location.Lon, DeliveryVeh.Location.Lat), 3,
                                                    Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES + 90), DeliveryVeh.Data.PLANE_ALTITUDE - DeliveryVeh.Data.PLANE_ALT_ABOVE_GROUND, Hdg);

                                                BoxLocation = new GeoPosition(World.FindSafeLocation(new List<Scene_Obj>()
                                        {
                                            DeliveryObj,
                                            DeliveryVeh
                                        }, new GeoLoc(BoxLocation), 3, (float)Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES), BoxLocation.Alt, BoxLocation.Hdg);

                                                DeliveryObj.Relocate(BoxLocation);

                                                SetState(Steps.DropCargo);
                                            }
                                        };
                                        #endregion

                                        #region Waypoints
                                        Node FT1_PreviousNode = null;
                                        foreach (Node node in QueryResponse.Map.ShortestPath)
                                        {
                                            GeoLoc wpLoc = node.Location;

                                            #region Offset node for right driving
                                            if (FT1_PreviousNode != null)
                                            {
                                                wpLoc = Utils.MapOffsetPosition(
                                                    node.Location,
                                                    Utils.GetRandom(2500, 3500) / 1000,
                                                    (float)(Utils.MapCalcBearing(node.Location, FT1_PreviousNode.Location) - (Utils.GetRandom(4500, 7500) / 1000)));
                                            }
                                            #endregion

                                            #region High Speed segments
                                            if (VEH1_Move.Waypoints.Count > 0)
                                            {
                                                if (Utils.MapCalcDist(node.Location, FT1_PreviousNode.Location, Utils.DistanceUnit.Meters) > 60)
                                                {
                                                    VEH1_Move.Waypoints.Last().Speed = 120;
                                                }
                                            }
                                            #endregion

                                            VEH1_Move.AddWP(new VehMovement.Waypoint()
                                            {
                                                Location = wpLoc,
                                                Speed = 40,
                                            });

                                            FT1_PreviousNode = node;
                                        }
                                        #endregion

                                        #region Terminal Guidance
                                        double HeadingFromEndpoint = Utils.MapCompareBearings(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES, Utils.MapCalcBearing(Connectors.SimConnection.LastTemporalData.PLANE_LOCATION, VEH1_Move.Waypoints.LastOrDefault().Location));

                                        switch (HeadingFromEndpoint)
                                        {
                                            case double n0 when (n0 > 0 && n0 <= 90): // Front Right
                                                {
                                                    VEH1_Move.AddWP(new VehMovement.Waypoint()
                                                    {
                                                        Location = Utils.MapOffsetPosition(
                                                            Connectors.SimConnection.LastTemporalData.PLANE_LOCATION,
                                                            (float)((Connectors.SimConnection.Aircraft.Wingspan * 2)),
                                                            (float)(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES)),
                                                        Speed = 15,
                                                    });
                                                    VEH1_Move.AddWP(new VehMovement.Waypoint()
                                                    {
                                                        Location = Utils.MapOffsetPosition(
                                                            Connectors.SimConnection.LastTemporalData.PLANE_LOCATION,
                                                            (float)((Connectors.SimConnection.Aircraft.Wingspan * 2)),
                                                            (float)(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES - 30)),
                                                        Speed = 15,
                                                    });
                                                    break;
                                                }
                                            case double n0 when (n0 > -90 && n0 <= 0): // Front Left
                                                {
                                                    VEH1_Move.AddWP(new VehMovement.Waypoint()
                                                    {
                                                        Location = Utils.MapOffsetPosition(
                                                            Connectors.SimConnection.LastTemporalData.PLANE_LOCATION,
                                                            (float)((Connectors.SimConnection.Aircraft.Wingspan * 2)),
                                                            (float)(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES - 30)),
                                                        Speed = 15,
                                                    });
                                                    break;
                                                }
                                            case double n0 when (n0 > -180 && n0 <= -90): // Back Left
                                            case double n1 when (n1 > 130 && n1 <= 180): // Back Right
                                                {
                                                    VEH1_Move.AddWP(new VehMovement.Waypoint()
                                                    {
                                                        Location = Utils.MapOffsetPosition(
                                                            Connectors.SimConnection.LastTemporalData.PLANE_LOCATION,
                                                            (float)((Connectors.SimConnection.Aircraft.Wingspan * 1.1)),
                                                            (float)(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES - 145)),
                                                        Speed = 15,
                                                    });
                                                    break;
                                                }
                                            case double n0 when (n0 > 90 && n0 <= 130): // Back Right
                                                {
                                                    VEH1_Move.AddWP(new VehMovement.Waypoint()
                                                    {
                                                        Location = Utils.MapOffsetPosition(
                                                            Connectors.SimConnection.LastTemporalData.PLANE_LOCATION,
                                                            (float)((Connectors.SimConnection.Aircraft.Wingspan * 1.1)),
                                                            (float)(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES - 180)),
                                                        Speed = 15,
                                                    });

                                                    VEH1_Move.AddWP(new VehMovement.Waypoint()
                                                    {
                                                        Location = Utils.MapOffsetPosition(
                                                            Connectors.SimConnection.LastTemporalData.PLANE_LOCATION,
                                                            (float)((Connectors.SimConnection.Aircraft.Wingspan * 1.1)),
                                                            (float)(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES - 145)),
                                                        Speed = 15,
                                                    });

                                                    break;
                                                }
                                        }


                                        VEH1_Move.AddWP(new VehMovement.Waypoint()
                                        {
                                            Location = Utils.MapOffsetPosition(
                                                Utils.MapOffsetPosition(Connectors.SimConnection.LastTemporalData.PLANE_LOCATION, Connectors.SimConnection.Aircraft.LocationFront.Z, Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES),
                                                (float)(((Connectors.SimConnection.Aircraft.Wingspan * 0.5) + 2)),
                                                (float)(Connectors.SimConnection.LastTemporalData.PLANE_HEADING_DEGREES - 90)),
                                            Speed = 15,
                                        });
                                        #endregion

                                        DeliveryVeh = new Scene_Obj(SocketID, "_welcome", new GeoPosition(TestObj_start.Location), SceneObjType.Dynamic)
                                        {
                                            File = VehicleModel,
                                            Movement = VEH1_Move
                                        };

                                        CreateObjectInstance(DeliveryVeh, 0);
                                    }
                                    else
                                    {
                                        Instant = true;
                                    }
                                }
                                else
                                {
                                    Instant = true;
                                }
                            }
                            else
                            {
                                Instant = true;
                            }

                            if (Instant)
                            {
                                int Hdg = Utils.GetRandom(0, 359);

                                var temp = LastTemporalData.PLANE_LOCATION;

                                GeoPosition UnloadLocation = new GeoPosition(Utils.MapOffsetPosition(
                                    Utils.MapOffsetPosition(LastTemporalData.PLANE_LOCATION, Connectors.SimConnection.Aircraft.LocationFront.Z, LastTemporalData.PLANE_HEADING_DEGREES),
                                    (float)(((Connectors.SimConnection.Aircraft.Wingspan * 0.5) + 2)),
                                    (float)(LastTemporalData.PLANE_HEADING_DEGREES - 90)), LastTemporalData.PLANE_ALTITUDE - LastTemporalData.PLANE_ALT_ABOVE_GROUND);

                                //UnloadLocation = new GeoPosition(World.FindSafeLocation(new List<Scene_Obj>(), new GeoLoc(UnloadLocation), 3, (float)LastTemporalData.PLANE_HEADING_DEGREES), UnloadLocation.Alt, Hdg);
                                DeliveryObj.Relocate(UnloadLocation);
                                SetState(Steps.DropCargo);
                            }
                        })
                        {
                            IsBackground = true
                        };
                        StepThread.CurrentCulture = CultureInfo.CurrentCulture;
                        StepThread.Start();
                        break;
                    }
                case Steps.DropCargo:
                    {
                        CreateObjectInstance(DeliveryObj, 0);

                        DoneAction();
                        IsRunning = false;

                        if (DeliveryVeh != null)
                        {
                            Thread StepThread = new Thread(() =>
                            {
                                Thread.Sleep(10000);
                                DeliveryVeh.IsEnabled = false;
                            });
                            StepThread.IsBackground = true;
                            StepThread.CurrentCulture = CultureInfo.CurrentCulture;
                            StepThread.Start();
                        }
                        break;
                    }
            }
        }
        
        private void CreateObjectInstance(Scene_Obj Obj, int delay = 0)
        {
            Thread ObjThread = new Thread(() => {
                lock (Objects)
                {
                    Objects.Add(Obj);
                }
                Thread.Sleep(delay);
                Obj.IsEnabled = true;
            });
            ObjThread.IsBackground = true;
            ObjThread.CurrentCulture = CultureInfo.CurrentCulture;
            ObjThread.Start();
        }
        
        public enum Steps
        {
            ToAircraft,
            ToFollow,
            ToPark,
            DropCargo
        }

    }
}