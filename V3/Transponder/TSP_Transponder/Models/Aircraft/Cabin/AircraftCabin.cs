using LiteDB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TSP_Transponder.Attributes;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Aircraft.Cabin.Features;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Cargo;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.EventBus;
using TSP_Transponder.Models.Payload;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Aircraft.Cabin
{
    public class AircraftCabin
    {
        public AircraftInstance Aircraft = null;

        [ClassSerializerField("name")]
        [StateSerializerField("Name")]
        public string Name { get; set; } = "";

        [ClassSerializerField("title")]
        [StateSerializerField("Title")]
        public string Title { get; set; } = "";

        [ClassSerializerField("livery")]
        [StateSerializerField("Livery")]
        public string Livery { get; set; } = "";

        [ClassSerializerField("levels")]
        [StateSerializerField("Levels")]
        public List<List<int>> Levels { get; set; } = new List<List<int>>()
        {
            new List<int>() { 3, 5, 0, 0 }
        };

        [ClassSerializerField("seatbelts_behavior")]
        public ushort SeatbeltsBehavior { get; set; } = 2;
        [ClassSerializerField("seatbelts_state")]
        public bool SeatbeltsState = false;
        public DateTime SeatbeltsLastChanged = DateTime.UtcNow;
        public DateTime? SeatbeltsFirstTurbulence = null;
        public DateTime? SeatbeltsLastTurbulence = null;

        [StateSerializerField("Features")]
        public List<AircraftCabinFeature> Features { get; set; } = new List<AircraftCabinFeature>();
        
        public bool ForceRefresh = false;
        public List<AircraftCabinPath> Paths = null;

        public long CabinCycle = 0;
        public int PathsInThisLoop = 0;
        private Thread ProcessThread = null;

        public volatile List<CargoItem> Cargos = new List<CargoItem>();
        public List<CargoItem> CargosRemoved = new List<CargoItem>();
        public List<CargoItem> CargosAdded = new List<CargoItem>();

        public AircraftCabin()
        {

        }

        public AircraftCabin(AircraftInstance aircraft)
        {
            Aircraft = aircraft;
            Livery = aircraft.LastLivery;

            switch(aircraft.Size)
            {
                case 0: // Heli
                    {
                        Levels[0][0] = 2;
                        Levels[0][1] = 1;
                        Levels[0][2] = 0;
                        Levels[0][3] = 0;
                        break;
                    }
                case 1: // Prop
                    {
                        /*
                        X = 3;
                        Y = 4;

                        Features.Add(new AircraftCabinFeatureDoor()
                        {
                            X = 0,
                            Y = 0,
                        });

                        Features.Add(new AircraftCabinFeatureDoor()
                        {
                            X = 3,
                            Y = 0,
                        });

                        Features.Add(new AircraftCabinFeatureSeat()
                        {
                            SubType = "pilot",
                            X = 3,
                            Y = 0,
                        });

                        Features.Add(new AircraftCabinFeatureSeat()
                        {
                            SubType = "copilot",
                            X = 3,
                            Y = 0,
                        });
                        */
                        break;
                    }
                case 2: // Turbo
                    {
                        //X = 3;
                        //Y = 4;
                        break;
                    }
                case 3: // Jet
                    {
                        //X = 3;
                        //Y = 4;
                        break;
                    }
                case 4: // Narrow
                    {
                        //X = 3;
                        //Y = 4;
                        break;
                    }
                case 5: // Wide
                    {
                        //X = 3;
                        //Y = 4;
                        break;
                    }
            }
            
        }
        
        public void Init(AircraftInstance aircraft)
        {
            Aircraft = aircraft;
        }
    
        public void StartProcess()
        {
            ProcessThread = new Thread(() =>
            {
                List<CargoItem> CargosChanged = new List<CargoItem>();
                List<Adventure> AdventuresLive = new List<Adventure>();

                CabinRefresh();
                
                while (ProcessThread != null && !App.MW.IsShuttingDown)
                {
                    PathsInThisLoop = 0;
                    Thread.Sleep(1000);
                    CargosChanged.Clear();

                    // Get Adventures list
                    AdventuresBase.GetAllLive((Adv) =>
                    {
                        if(!AdventuresLive.Contains(Adv) && Adv.IsMonitored)
                        {
                            AdventuresLive.Add(Adv);
                            Adv.CabinRefresh();
                        }
                    });

                    // Go through all Adventures
                    List<Adventure> ToRemove = new List<Adventure>();
                    foreach (var Adv in AdventuresLive)
                    {
                        // Check what needs to be removed
                        if(!Adv.IsMonitored)
                        {
                            ToRemove.Add(Adv);
                            continue;
                        }

                        // Process all changes
                        //foreach(pax_pickup_2 pickup in Adv.Actions.FindAll(x => typeof(pax_pickup_2) == x.GetType()))
                        //{
                        //    pickup.ProcessCabinChanges(Changed, this);
                        //}

                    }

                    // Remove what needs to be removed
                    foreach(var Adv in ToRemove)
                    {
                        lock (Cargos)
                        {
                            // Remove related cargo
                            foreach (var cargo in Cargos.FindAll(x => x.Adventure != null ? x.Adventure == Adv : false))
                            {
                                CargosRemoved.Add(cargo);
                                Cargos.Remove(cargo);
                            }
                        }

                        AdventuresLive.Remove(Adv);
                        ForceRefresh = true;
                    }

                    CheckCabinStates();

                    lock (Cargos)
                    {
                        foreach (var cargo in Cargos)
                        {
                            cargo.State.ProcessCargo(CargosChanged, CargosAdded);
                        }
                    }

                    if (ForceRefresh || CargosAdded.Count > 0)
                    {
                        ForceRefresh = false;
                        APIBase.ClientCollection.SendMessage("cabin:update", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                        {
                            { "livery", Livery },
                            { "cargos_removed", CargosRemoved.Select(x => x.GUID + "_" + (x.Group != null ? x.Group.Manifest.UID : 0)) },
                            { "cargos", Cargos.Select(x => x.Serialize(new Dictionary<string, dynamic>()
                                {
                                    { "guid", true },
                                    { "action_id_origin", true },
                                    { "action_id_destination", true },
                                }))
                            },
                            { "cargos_state", Cargos.Select(x => x.SerializeState(new Dictionary<string, dynamic>()
                                {
                                    { "guid", true },
                                    { "state", true },
                                }))
                            },
                            { "humans_removed", new List<string>() },
                            { "humans", new List<string>() },
                            { "humans_state", new List<string>() }
                        }), null, APIBase.ClientType.Skypad);
                    }
                    else
                    {
                        if (CargosChanged.Count > 0)
                        {
                            APIBase.ClientCollection.SendMessage("cabin:update", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                            {
                                { "livery", Livery },
                                { "cargos_removed", CargosRemoved.Select(x => x.GUID + "_" + (x.Group != null ? x.Group.Manifest.UID : 0)) },
                                { "cargos_state", CargosChanged.Select(x => x.SerializeState(new Dictionary<string, dynamic>()
                                    {
                                        { "guid", true },
                                        { "state", true },
                                    }))
                                },
                                { "humans_removed", new List<string>() },
                                { "humans_state", new List<string>() }
                            }), null, APIBase.ClientType.Skypad);
                        }
                    }
                    
                    CargosRemoved.Clear();
                    CargosChanged.Clear();
                    CargosAdded.Clear();

                    CabinCycle++;
                }

            });
            ProcessThread.IsBackground = true;
            ProcessThread.Start();
        }

        public void EndProcess()
        {
            ProcessThread = null;

            lock (Cargos)
            {
                Cargos.Clear();
            }
        }

        public void TriggerEvent(string guid, string ev, Dictionary<string, dynamic> data)
        {
            switch (ev)
            {
                case "seatbelts_behavior":
                    {
                        SeatbeltsBehavior = Convert.ToUInt16(data["state"]);
                        BroadcastPartial(new Dictionary<string, dynamic>() { { "seatbelts_behavior", true }, { "seatbelts_state", true } });
                        break;
                    }

            }
        }

        public void CabinRefresh()
        {
            // Create pathways
            Paths = new List<AircraftCabinPath>();
            ushort z_at = 0;

            while (z_at < Levels.Count)
            {
                int x_at = Levels[z_at][2];
                int y_at = Levels[z_at][3];
            
                while (x_at < Levels[z_at][0] + Levels[z_at][2])
                {
                    y_at = 0;
                    while (y_at < Levels[z_at][1] + Levels[z_at][3])
                    {
                        var existing_feature = Features.Find(f => f.X == x_at && f.Y == y_at && f.Z == z_at && f.Layer == AircraftCabinFeatureLayer.Floor);
                        if (existing_feature == null)
                        {
                            var new_path = new AircraftCabinPath()
                            {
                                X = (ushort)x_at,
                                Y = (ushort)y_at,
                                Z = z_at
                            };
                            Paths.Add(new_path);
                        }
                        y_at++;
                    }
                    x_at++;
                }
                z_at++;
            }


            // Predetermin feature access points
            foreach(var feature in Features)
            {
                feature.CabinRefresh(this);

                var y_padd = 0;
                switch(feature.Type)
                {
                    case AircraftCabinFeatureType.Seat:
                        {
                            switch(feature.SubType){
                                case "jumpseat":
                                    {
                                        y_padd = 0;
                                        break;
                                    }
                                default:
                                    {
                                        y_padd = 4;
                                        break;
                                    }
                            }
                            break;
                        }
                    case AircraftCabinFeatureType.Util: y_padd = 0; break;
                    default: y_padd = 1; break;
                }

                double distance = double.MaxValue;
                List<AircraftCabinPath> paths = new List<AircraftCabinPath>();
                var filtered_paths = Paths.Where(p => p.Z == feature.Z);
                
                foreach (var path in filtered_paths)
                {
                    double y_padding = path.Y != feature.Y ? y_padd : 0;
                    double dist = Math.Ceiling(Utils.GetDistance(path.X, path.Y, feature.X, feature.Y)) + y_padding;

                    switch (feature.Orientation)
                    {
                        case AircraftCabinFeatureOrientation.Left:
                            {
                                if(path.X < feature.X)
                                    dist -= 3;
                                break;
                            }
                        case AircraftCabinFeatureOrientation.Right:
                            {
                                if (path.X > feature.X)
                                    dist -= 3;
                                break;
                            }
                        case AircraftCabinFeatureOrientation.Up:
                            {
                                if (path.Y < feature.Y)
                                    dist -= 3;
                                break;
                            }
                        case AircraftCabinFeatureOrientation.Down:
                            {
                                if (path.Y > feature.Y)
                                    dist -= 3;
                                break;
                            }
                    }

                    if (dist < distance)
                    {
                        distance = dist;
                        paths.Clear();
                        paths.Add(path);
                    }
                    else if(dist == distance)
                    {
                        paths.Add(path);
                    }
                }
                
                feature.AccessIsPath = distance <= 1;
                feature.AccessPaths = paths;
            }
            
            lock (Cargos)
            {
                foreach (var cargo in new List<CargoItem>(Cargos))
                {
                    cargo.CabinRefresh(this);
                }
            }

            ForceRefresh = true;
        }

        private void CheckCabinStates()
        {
            var session = EventManager.Active;

            if(SeatbeltsBehavior != 2)
            {
                var nv = SeatbeltsBehavior == 1 ? true : false;
                if(SeatbeltsState != nv)
                {
                    SeatbeltsState = nv;
                    BroadcastPartial(new Dictionary<string, dynamic>() { { "seatbelts_behavior", true }, { "seatbelts_state", true } });
                }
            }

            var turb_g = session.Compute_GForces.SustainedGAVG;
            var turb_x = session.Compute_GForces.SustainedAccelsXAVG;
            var turb_y = session.Compute_GForces.SustainedAccelsYAVG;
            var turb_z = session.Compute_GForces.SustainedAccelsZAVG;

            var g_diff = Math.Abs(turb_x) + Math.Abs(turb_y) + Math.Abs(turb_z);

            //Console.WriteLine("SG " + Math.Round(turb_x, 3).ToString("N2") + " - " + Math.Round(turb_y, 3).ToString("N2") + " - " + Math.Round(turb_z, 3).ToString("N2") + " - " + Math.Round(turb_g, 3).ToString("N2"));

            // Check G-Forces and save timestamps
            if (g_diff > 0.2)
            {
                if (SeatbeltsFirstTurbulence == null)
                {
                    SeatbeltsFirstTurbulence = DateTime.UtcNow;
                    SeatbeltsLastTurbulence = DateTime.UtcNow;
                }
                else
                {
                    SeatbeltsLastTurbulence = DateTime.UtcNow;
                }
            }

            if (SimConnection.LastTemporalData.PLANE_ALT_ABOVE_GROUND < 10000)
            {
                if (SeatbeltsBehavior == 2)
                {
                    if (!SimConnection.LastTemporalData.GENERAL_ENG_COMBUSTION && SimConnection.LastTemporalData.SIM_ON_GROUND && SimConnection.LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED < 2)
                    {
                        if (SeatbeltsState)
                        {
                            SeatbeltsState = false;
                            BroadcastPartial(new Dictionary<string, dynamic>() { { "seatbelts_behavior", true }, { "seatbelts_state", true } });
                        }
                    }
                    else
                    {
                        if (!SeatbeltsState)
                        {
                            SeatbeltsState = true;
                            BroadcastPartial(new Dictionary<string, dynamic>() { { "seatbelts_behavior", true }, { "seatbelts_state", true } });
                        }
                    }
                }

                // If the last turbulence happened more then 30 seconds ago
                if (SeatbeltsLastTurbulence != null)
                {
                    if ((DateTime.UtcNow - SeatbeltsLastTurbulence.Value).TotalSeconds > 30)
                    {
                        SeatbeltsLastTurbulence = null;
                        SeatbeltsFirstTurbulence = null;
                    }
                }

            }
            else
            {
                // If the last turbulence happened more then 30 seconds ago
                if (SeatbeltsLastTurbulence != null)
                {
                    if ((DateTime.UtcNow - SeatbeltsLastTurbulence.Value).TotalSeconds > 30)
                    {
                        SeatbeltsLastTurbulence = null;
                        SeatbeltsFirstTurbulence = null;

                        if (SeatbeltsBehavior == 2)
                        {
                            if (SeatbeltsState)
                            {

                                SeatbeltsState = false;
                                BroadcastPartial(new Dictionary<string, dynamic>() { { "seatbelts_behavior", true }, { "seatbelts_state", true } });
                            }
                        }

                    }
                }
                
                if(SeatbeltsBehavior == 2)
                {
                    // Check how long we've been in turbulence and trigger seatbelts
                    if (SeatbeltsLastTurbulence != null && SeatbeltsFirstTurbulence != null)
                    {
                        var turb_span = (SeatbeltsLastTurbulence - SeatbeltsFirstTurbulence).Value;
                        if (turb_span.TotalSeconds > 5)
                        {
                            if (!SeatbeltsState)
                            {
                                SeatbeltsState = true;
                                BroadcastPartial(new Dictionary<string, dynamic>() { { "seatbelts_behavior", true }, { "seatbelts_state", true } });
                            }
                        }
                    }
                    else
                    {
                        if (SeatbeltsState)
                        {
                            SeatbeltsState = false;
                            BroadcastPartial(new Dictionary<string, dynamic>() { { "seatbelts_behavior", true }, { "seatbelts_state", true } });
                        }
                    }
                }
                
            }

        }

        public int GetAvailableSeats()
        {
            return Features.FindAll(x => x.Type == AircraftCabinFeatureType.Seat && x.SubType != "pilot" && x.SubType != "copilot" && x.SubType != "jumpseat").Count;
        }

        public int GetAvailablePods()
        {
            return Features.FindAll(x => x.Type == AircraftCabinFeatureType.Cargo).Count;
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            AircraftCabin Cabin = null;

            if (SimConnection.Aircraft != null)
                Cabin = SimConnection.Aircraft.Cabin;

            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "get-all":
                    {
                        if(Cabin != null)
                        {
                            lock (Cabin.Cargos)
                            {
                                Socket.SendMessage("response", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                                {
                                    { "livery", Cabin.Livery },
                                    { "humans", new List<string>() },
                                    { "humans_state", new List<string>() },
                                    { "cargos", Cabin.Cargos.Select(x => x.Serialize(new Dictionary<string, dynamic>()
                                        {
                                            { "guid", true },
                                            { "action_id_origin", true },
                                            { "action_id_destination", true },
                                        }))
                                    },
                                    { "cargos_state", Cabin.Cargos.Select(x => x.SerializeState(new Dictionary<string, dynamic>()
                                        {
                                            { "guid", true },
                                            { "state", true },
                                        }))
                                    }
                                }), (Dictionary<string, dynamic>)structure["meta"]);
                            }
                        }
                        else
                        {
                            Socket.SendMessage("response", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
                            {
                                { "livery", null },
                                { "humans", new List<object>() },
                                { "humans_state", new List<object>() },
                                { "cargos", new List<object>() },
                                { "cargos_state", new List<object>() }
                            }), (Dictionary<string, dynamic>)structure["meta"]);
                        }
                        
                        break;
                    }
            }
        }

        private bool CheckDuplicateFeatures(int x, int y, int z, AircraftCabinFeatureType type)
        {
            var features = Features.FindAll(f => f.X == x && f.Y == y && f.Z == z && f.Type == type);
            return features.Count > 0;
        }

        public bool ValidatePodAssign(string pod_guid, string cargo_guid)
        {
            if (Features.Find(x => x.GUID == pod_guid && x.Type == AircraftCabinFeatureType.Cargo) != null)
            {
                var CargosAssigned = Cargos.Find(x => x.GUID != cargo_guid && x.State.AssignedPodGUID == pod_guid);
                return CargosAssigned == null;
            }
            else
            {
                return false;
            }
        }

        public AircraftCabinFeature GetPodAssign(CargoItem cargo)
        {
            AircraftCabinFeature Assigned = null;

            var random_pods = Features.Where(x => x.Type == AircraftCabinFeatureType.Cargo);
            var random_pods_l = random_pods.Where(x => Cargos.Find(x1 => x1.State.AssignedPod != null ? (x1.State.AssignedPod.GUID == x.GUID) : false) == null).ToList();

            if(random_pods_l.Count > 0)
            {
                Assigned = random_pods_l[Utils.GetRandom(random_pods_l.Count - 1)];
            }

            return Assigned;
        }

        private void RevalidateFeatures()
        {
            // Delete out of Level 
            var features_delete = Features.FindAll(f => f.Z >= Levels.Count);
            foreach (var feature in features_delete)
            {
                Features.Remove(feature);
            }


            // Reposition Doors
            var features_doors_fix = Features.FindAll(f => f.Type == AircraftCabinFeatureType.Door && !(f.X == 0 || f.X == Levels[f.Z][0] - 1 ));
            foreach (var feature in features_doors_fix)
            {
                ushort newX = (ushort)(Levels[feature.Z][0] - 1);
                if (!CheckDuplicateFeatures(newX, feature.Y, feature.Z, feature.Type))
                {
                    feature.X = newX;
                }
                else
                {
                    Features.Remove(feature);
                }
            }
            
            // Remove out of bounds
            var features_beyond_bounds = Features.FindAll(f => f.X >= Levels[f.Z][0] + Levels[f.Z][2] || f.X < Levels[f.Z][2] || f.Y >= Levels[f.Z][1] + Levels[f.Z][3] || f.Y < Levels[f.Z][3] || f.Z >= Levels.Count);
            foreach(var feature in features_beyond_bounds)
            {
                Features.Remove(feature);
            }
        }

        private AircraftCabinFeature AddFeature(Dictionary<string, dynamic> feature)
        {
            AircraftCabinFeature new_feature = null;
            switch (feature["type"])
            {
                case "door": { new_feature = new AircraftCabinFeatureDoor().Deserialize(feature); break; }
                case "seat": { new_feature = new AircraftCabinFeatureSeat().Deserialize(feature); break; }
                case "util": { new_feature = new AircraftCabinFeatureUtil().Deserialize(feature); break; }
                case "stairs": { new_feature = new AircraftCabinFeatureStairs().Deserialize(feature); break; }
                case "cargo": { new_feature = new AircraftCabinFeatureCargo().Deserialize(feature); break; }
            }

            if (new_feature != null)
            {
                var in_cell = Features.FindAll(f => f.X == new_feature.X && f.Y == new_feature.Y && f.Z == new_feature.Z);

                foreach (var found_feature in in_cell.Where(f =>
                {
                    switch(f.Layer)
                    {
                        case AircraftCabinFeatureLayer.Wall:
                            {
                                if(new_feature.Layer == AircraftCabinFeatureLayer.Wall)
                                {
                                    if(new_feature.Orientation == f.Orientation)
                                    {
                                        return true;
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                if(f.Layer == new_feature.Layer)
                                {
                                    return true;
                                }
                                break;
                            }
                    }

                    return false;
                }))
                {
                    Features.Remove(found_feature);
                }
                Features.Add(new_feature);
            }

            return new_feature;
        }

        private void UpdateFeature(Dictionary<string, dynamic> feature)
        {
        }

        internal void Interact(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure, Dictionary<string, dynamic> payload_struct)
        {
            bool send_response = true;
            switch (payload_struct["verb"])
            {
                case "AddFeatures":
                    {
                        var new_features = new List<AircraftCabinFeature>();
                        foreach (Dictionary<string, dynamic> feature in payload_struct["features"])
                            new_features.Add(AddFeature(feature));

                        send_response = false;
                        Socket.SendMessage("response", App.JSSerializer.Serialize(new RequestState()
                        {
                            Status = RequestState.STATUS.SUCCESS,
                            ReferenceID = Convert.ToInt64(payload_struct["id"]),
                            Data = new_features.Select(x => x.GUID),
                        }.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
                        CabinRefresh();
                        break;
                    }
                case "RemoveCells":
                    {
                        foreach (var cell in payload_struct["cells"])
                        {
                            var to_remove = Features.FindAll(f => f.X == cell[0] && f.Y == cell[1] && f.Z == cell[2]);
                            var orient = (AircraftCabinFeatureOrientation)EnumAttr.GetEnum(typeof(AircraftCabinFeatureOrientation), payload_struct["orientation"]);

                            foreach(var feature in to_remove.FindAll(x => x.Layer != AircraftCabinFeatureLayer.Wall))
                            {
                                Features.Remove(feature);
                            }

                            foreach (var feature in to_remove.FindAll(x => x.Layer == AircraftCabinFeatureLayer.Wall && x.Orientation == orient))
                            {
                                Features.Remove(feature);
                            }
                        }
                        CabinRefresh();
                        break;
                    }
                case "AddFeature":
                    {
                        send_response = false;
                        Socket.SendMessage("response", App.JSSerializer.Serialize(new RequestState()
                        {
                            Status = RequestState.STATUS.SUCCESS,
                            ReferenceID = Convert.ToInt64(payload_struct["id"]),
                            Data = AddFeature((Dictionary<string, dynamic>)payload_struct["feature"]).GUID,
                        }.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);
                        CabinRefresh();
                        break;
                    }
                case "RemoveFeature":
                    {
                        var found = Features.Find(x => x.GUID == (string)payload_struct["guid"]);
                        Features.Remove(found);
                        CabinRefresh();
                        break;
                    }
                case "RemoveFeatures":
                    {
                        foreach (string guid in payload_struct["guids"])
                        {
                            var found = Features.Find(x => x.GUID == guid);
                            Features.Remove(found);
                        }
                        CabinRefresh();
                        break;
                    }
                case "UpdateFeatures":
                    {
                        foreach (Dictionary<string, dynamic> featureChanges in payload_struct["features"])
                        {
                            var feature = Features.Find(x => x.GUID == featureChanges["guid"]);
                            feature.Interact(Socket, StructSplit, structure, new Dictionary<string, dynamic>() { { "id", Convert.ToInt64(payload_struct["id"]) }, { "data", featureChanges } } );
                        }
                        CabinRefresh();
                        break;
                    }
                case "Update":
                    {
                        Name = (string)payload_struct["name"];

                        try
                        {
                            var new_levels = new List<List<int>>();
                            foreach (ArrayList level in (ArrayList)payload_struct["levels"])
                            {
                                new_levels.Add(new List<int>()
                                {
                                    (int)level[0], //x
                                    (int)level[1], //y
                                    level.Count > 2 ? (int)level[2] : 0, //ox
                                    level.Count > 3 ? (int)level[3] : 0, //oy
                                });
                            }
                            Levels = new_levels;

                        }
                        catch
                        {

                        }

                        CabinRefresh();
                        break;
                    }
                case "TriggerEvent":
                    {
                        TriggerEvent(payload_struct.ContainsKey("guid") ? (string)payload_struct["guid"] : null, (string)payload_struct["event"], (Dictionary<string, dynamic>)payload_struct["data"]);
                        break;
                    }
                case "Refresh":
                    {
                        RevalidateFeatures();
                        BroadcastPartial(null);
                        CabinRefresh();
                        break;
                    }
            }

            if (send_response)
                Socket.SendMessage("response", App.JSSerializer.Serialize(new RequestState()
                {
                    Status = RequestState.STATUS.SUCCESS,
                    ReferenceID = Convert.ToInt64(payload_struct["id"]),
                }.ToDictionary()), (Dictionary<string, dynamic>)structure["meta"]);

            BroadcastPartial(null);
        }

        public void BroadcastPartial(Dictionary<string, dynamic> fields)
        {
            APIBase.ClientCollection.SendMessage("fleet:cabin", App.JSSerializer.Serialize(new Dictionary<string, dynamic>()
            {
                { "livery", Livery },
                { "cabin", Serialize(fields) }
            }), null, APIBase.ClientType.Skypad);
        }


        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<AircraftCabin> cs = new ClassSerializer<AircraftCabin>(this, fields);
            cs.Generate(typeof(AircraftCabin), fields);

            cs.Get("features", fields, (f) => Features.Select(x => x.Serialize(f)));

            return cs.Get();
        }


        public AircraftCabin ImportState(Dictionary<string, dynamic> state)
        {
            var ss = new StateSerializer<AircraftCabin>(this, state);

            ss.Set("Name");
            ss.Set("Title");
            ss.Set("Livery");
            ss.Set("Levels", (v) =>
            {
                List<List<int>> NewLevels = new List<List<int>>();
                foreach (var level in v)
                {
                    NewLevels.Add(((ArrayList)level).Cast<int>().ToList());
                }
                return NewLevels;
            });

            ss.Set("Features", (v) =>
            {
                List<AircraftCabinFeature> ncs = new List<AircraftCabinFeature>();
                foreach (var x in v)
                {
                    switch(x["Type"])
                    {
                        case "door": { ncs.Add(new AircraftCabinFeatureDoor().ImportState((Dictionary<string, dynamic>)x)); break; }
                        case "seat": { ncs.Add(new AircraftCabinFeatureSeat().ImportState((Dictionary<string, dynamic>)x)); break; }
                        case "stairs": { ncs.Add(new AircraftCabinFeatureStairs().ImportState((Dictionary<string, dynamic>)x)); break; }
                        case "util": { ncs.Add(new AircraftCabinFeatureUtil().ImportState((Dictionary<string, dynamic>)x)); break; }
                        case "cargo": { ncs.Add(new AircraftCabinFeatureCargo().ImportState((Dictionary<string, dynamic>)x)); break; }
                    }
                }

                foreach(var feature in ncs)
                {
                    feature.CabinRefresh(this);
                }

                return ncs;
            });

            return this;
        }

        public Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<AircraftCabin>(this);

            ss.Get("Name");
            ss.Get("Title");
            ss.Get("Livery");
            ss.Get("Levels");

            ss.Get("Features", (v) => Features.Select(x => x.ExportState()));

            return ss.Get();
        }
    }
}
