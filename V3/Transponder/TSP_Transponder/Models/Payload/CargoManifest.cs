using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Aircraft.Cabin;
using TSP_Transponder.Models.Aircraft.Cabin.Features;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Payload.Assets;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Payload
{
    public class CargoManifest
    {
        public Adventure Adventure = null;
        public List<CargoGroup> Groups = null;
        public List<PayloadDestination> Destinations = new List<PayloadDestination>();
        public Situation Situation = null;
        public action_base Action = null;

        [StateSerializerField("UID")]
        [ClassSerializerField("id")]
        public long UID = 0;
        
        internal CargoAsset Definition = null;

        [StateSerializerField("Total")]
        [ClassSerializerField("total")]
        public int TotalQualtity = 0;

        [StateSerializerField("TotalPercent")]
        [ClassSerializerField("total_percent")]
        public float TotalPercent = 0;

        [StateSerializerField("Dropoffs")]
        public Dictionary<int, ushort> Dropoffs = null;
        [StateSerializerField("DropoffPoints")]
        public Dictionary<int, ushort> DropoffPoints = null;

        public CargoManifest(Adventure Adventure, Situation Situation, action_base Action, Dictionary<string, dynamic> config)
        {
            this.Adventure = Adventure;
            this.Situation = Situation;
            this.Action = Action;
            UID = Convert.ToInt64(config["UID"]);
        }

        public bool PreConfigure(Action<List<Dictionary<string, dynamic>>> objection_callback)
        {
            if (SimConnection.Aircraft != null)
            {
                Dropoffs = new Dictionary<int, ushort>();
                int SeatsCount = SimConnection.Aircraft.Cabin.Features.FindAll(x => x.Type == AircraftCabinFeatureType.Cargo).Count;
                int UsedSeats = Math.Max(DropoffPoints.Count, (int)Math.Floor(SeatsCount * (TotalPercent / 100)));
                int Total = 0;

                int SeatsLeft = UsedSeats;
                int TotalPoints = DropoffPoints.Sum(x => x.Value);

                if (UsedSeats > 0)
                {
                    int TotalDropoffs = DropoffPoints.Count;
                    SeatsLeft -= TotalDropoffs;
                    float SeatPerPoint = (float)SeatsLeft / TotalPoints;
                    foreach (var dropoff in DropoffPoints)
                    {
                        int DropoffCount = 1 + (int)Math.Floor(SeatPerPoint * dropoff.Value);
                        Total += DropoffCount;
                        Dropoffs.Add(dropoff.Key, (ushort)DropoffCount);
                    }
                }

                if (Total > SeatsCount || (SeatsCount == 0 && TotalPoints > 0))
                {
                    objection_callback(new List<Dictionary<string, dynamic>>()
                    {
                        { new Dictionary<string, dynamic>()
                            {
                                { "cargo_missing", null }
                            }
                        }
                    });
                    return false;
                }
            }
            else
            {
                objection_callback(new List<Dictionary<string, dynamic>>()
                {
                    { new Dictionary<string, dynamic>()
                        {
                            { "aircraft_none", null }
                        }
                    }
                });
                return false;
            }

            return true;
        }
        
        public void Configure(AircraftCabin Cabin)
        {
            if (Groups == null)
            {
                Groups = new List<CargoGroup>();
            }

            if(DropoffPoints != null)
            {
                int TotalPoints = DropoffPoints.Sum(x => x.Value);
                foreach (var doKey in DropoffPoints.Keys)
                {
                    if (DropoffPoints[doKey] > 0)
                    {
                        cargo_dropoff_2 dropoff = (cargo_dropoff_2)Adventure.Actions.Find(x => x.UID == doKey);
                        PayloadDestination destination = Destinations.Find(x => x.Situation == dropoff.Situation);
                        if (destination == null)
                        {
                            destination = new PayloadDestination(Adventure, dropoff);
                            Destinations.Add(destination);
                        }

                        short destination_index = (short)Destinations.IndexOf(destination);
                        CargoGroup group = Groups.Find(x => x.DestinationIndex == destination_index);

                        if (group == null)
                        {
                            group = new CargoGroup(this) { DestinationIndex = destination_index };
                            group.SetLocation(new GeoPosition(Situation.Location));
                            group.CountPercent = (float)DropoffPoints[doKey] / TotalPoints * 100;
                            Groups.Add(group);
                        }
                        else if (Adventure.State == Adventure.AState.Saved || Adventure.State == Adventure.AState.Active)
                        {
                            group.Count = Dropoffs[doKey];
                        }
                    }
                }
            }

            if (Adventure.State == Adventure.AState.Saved || Adventure.State == Adventure.AState.Active)
            {
                foreach (var group in Groups)
                {
                    group.CreateCargoItem(Cabin);
                    TotalQualtity += group.Count;
                }
                Adventure.ScheduleManifestsBroadcast = true;
                Adventure.ScheduleManifestsStateBroadcast = true;
            }
        }

        public void Ending()
        {
            foreach(var group in Groups)
            {
                group.Items = new List<Cargo.CargoItem>();
            }
        }

        public int Process()
        {
            List<CargoGroup> changes = new List<CargoGroup>();

            if (Groups != null)
            {
                foreach (var group in Groups)
                {
                    group.Process((changed) =>
                    {
                        if (changed)
                            changes.Add(group);
                    });
                }

                if (changes.Count > 0)
                {
                    Adventure.ScheduleManifestsStateBroadcast = true;
                }
            }

            return changes.Count;
        }

        public double GetLoadedWeight()
        {
            double cumulative = 0;
            if (SimConnection.Aircraft != null)
            {
                foreach (var group in Groups)
                {
                    if (group.TransitionTo != null || group.LoadedOn != null)
                    {
                        if ((group.TransitionTo != null ? group.TransitionTo.ID == SimConnection.Aircraft.ID : false) || (group.LoadedOn != null ? group.LoadedOn.ID == SimConnection.Aircraft.ID : false))
                        {
                            cumulative += group.Items.Where(x => x.State.Boarded).Sum(x => x.Group.Manifest.Definition.WeightKG);
                        }
                    }
                }
            }
            return cumulative;
        }

        public List<AircraftInstance> GetAircraft()
        {
            if (Groups != null)
            {
                return Groups.Select(x => x.LoadedOn).ToList();
            }
            else
            {
                return new List<AircraftInstance>();
            }
        }


        public void ImportState(Dictionary<string, dynamic> state)
        {
            if (state.ContainsKey("CargoGUID"))
            {
                Definition = CargoAssetsLibrary.CargoItems.Find(x => x.GUID == (string)state["CargoGUID"]);
            }
            if (Definition == null)
            {
                Definition = CargoAssetsLibrary.CargoItems.First();
            }

            #region Config
            if (state.ContainsKey("PickupConfig"))
            {
                TotalPercent = Convert.ToSingle(state["PickupConfig"]["Percent"]);
            }

            if (state.ContainsKey("DropoffConfig"))
            {
                DropoffPoints = new Dictionary<int, ushort>();
                foreach (var dropoff in state["DropoffConfig"])
                {
                    DropoffPoints.Add((int)dropoff["DropoffID"], Convert.ToUInt16(dropoff["Points"]));
                }
            }
            #endregion

            #region Restore bars
            var ss = new StateSerializer<CargoManifest>(this, state);

            ss.Set("UID");
            ss.Set("DropoffPoints", (v) => ((Dictionary<string, dynamic>)v).ToDictionary(p => Convert.ToInt32(p.Key), p => (ushort)p.Value));
            ss.Set("Dropoffs", (v) => v != null ? ((Dictionary<string, dynamic>)v).ToDictionary(p => Convert.ToInt32(p.Key), p => (ushort)p.Value) : null);
            ss.Set("Total");
            ss.Set("TotalPercent");
            #endregion

            #region Groups
            if (state.ContainsKey("Groups"))
            {
                Groups = new List<CargoGroup>();
                if (state["Groups"] != null)
                {
                    //if (Adventure.State != Adventure.AState.Succeeded && Adventure.State != Adventure.AState.Failed)
                    //{
                    foreach (var doKey in DropoffPoints.Keys)
                    {
                        cargo_dropoff_2 dropoff = (cargo_dropoff_2)Adventure.Actions.Find(x => x.UID == doKey);
                        PayloadDestination destination = Destinations.Find(x => x.Situation == dropoff.Situation);

                        if (destination == null)
                        {
                            destination = new PayloadDestination(Adventure, dropoff);
                            Destinations.Add(destination);
                        }
                    }

                    foreach (var cluster in state["Groups"])
                    {
                        CargoGroup cc = new CargoGroup(this)
                        {
                            GUID = new Guid(cluster["GUID"])
                        };
                        cc.ImportState((Dictionary<string, dynamic>)cluster);
                        Groups.Add(cc);
                    }
                    //}
                }
            }
            else
            {
                Configure(null);
            }
            #endregion

        }

        public Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<CargoManifest>(this);

            ss.Get("CargoGUID", (v) => Definition.GUID);
            ss.Get("UID");
            ss.Get("Total");
            ss.Get("TotalPercent");
            ss.Get("Groups", (v) => Groups != null ? Groups.Select(x => x.ExportState()).ToList() : null);
            ss.Get("Destinations", (v) => Destinations.Select(x => x.ExportState()).ToList());
            ss.Get("Dropoffs", (v) => Dropoffs != null ? Dropoffs.ToDictionary(p => Convert.ToString(p.Key), p => (int)p.Value) : null);
            ss.Get("DropoffPoints", (v) => DropoffPoints != null ? DropoffPoints.ToDictionary(p => Convert.ToString(p.Key), p => (int)p.Value) : null);

            return ss.Get();
        }


        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<CargoManifest> cs = new ClassSerializer<CargoManifest>(this, fields);
            cs.Generate(typeof(CargoManifest), fields);

            cs.Get("cargo_guid", fields, (f) => Definition.GUID);
            cs.Get("name", fields, (f) => Definition.Name);
            cs.Get("total_weight", fields, (f) => Definition.WeightKG * TotalQualtity);
            cs.Get("weight", fields, (f) => Definition.WeightKG);
            cs.Get("destinations", fields, (f) => Destinations.Select(x => x.Serialize(f)));
            cs.Get("origin", fields, (f) => new Dictionary<string, dynamic>()
            {
                { "action_id", Action.UID },
                { "airport", Situation.Airport != null ? Situation.Airport.Serialize(f) : null },
                { "location", Situation.Location.ToList(5) },
                { "location_name", Situation.LocationName != string.Empty ? Situation.LocationName : Situation.Template.SituationLabels[Situation.Index] }
            });
            cs.Get("groups", fields, (f) => Groups != null ? Groups.Select(x => x.Serialize(new Dictionary<string, dynamic>()
            {
                { "guid", true },
                { "destination_index", true },
                { "count_percent", true },
            })) : null);

            var result = cs.Get();
            return result;
        }

        public Dictionary<string, dynamic> SerializeState(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<CargoManifest> cs = new ClassSerializer<CargoManifest>(this, fields);
            cs.Generate(typeof(CargoManifest), fields);

            cs.Get("groups", fields, (f) => Groups != null ? Groups.Select(x => x.SerializeState(new Dictionary<string, dynamic>()
            {
                { "guid", true },
                { "loaded_on", true },
                { "transition_to", true },
                { "loadable", true },
                { "transferable", true },
                { "unloadable", true },
                { "deliverable", true },
                { "delivered", true },
                { "boarded", true },
                { "location", true },
                { "count", true },
                //{ "families", true },
                { "nearest_airport", new Dictionary<string, dynamic>()
                    {
                        { "icao", true },
                        { "name", true },
                        { "country_name", true },
                        { "country", true },
                    }
                },
            })) : null);

            var result = cs.Get();
            return result;
        }
    }
}
