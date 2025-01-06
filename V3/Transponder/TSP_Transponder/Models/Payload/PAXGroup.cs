using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Aircraft.Cabin;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.WorldManager;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Payload
{
    public class PAXGroup
    {

        [StateSerializerField("GUID")]
        public Guid? GUID = null;
        [StateSerializerField("LoadedOn")]
        public AircraftInstance LoadedOn = null;
        public AircraftInstance TransitionTo = null;
        public Action CompletedAction = null;
        [StateSerializerField("DestinationIndex")]
        public short DestinationIndex = -1;
        [StateSerializerField("Location")]
        public GeoPosition Location = null;
        [StateSerializerField("TakenCharge")]
        public bool TakenCharge = false;
        [StateSerializerField("Delivered")]
        public bool Delivered = false;
        
        public PAXManifest Manifest = null;
        public Airport NearestAirport = null;
        [StateSerializerField("Families")]
        public List<string> Families = new List<string>();
        [StateSerializerField("Count")]
        public ushort Count = 0;
        [StateSerializerField("CountPercent")]
        public float CountPercent = 0;
        
        public bool Loadable = false;
        public bool Unloadable = false;
        public bool Deliverable = false;
        public bool Transferable = false;

        public PAXGroup(PAXManifest manifest, Dictionary<string, dynamic> config)
        {
            Manifest = manifest;
            GUID = new Guid(config["GUID"]);
        }

        public PAXGroup(PAXManifest manifest)
        {
            Manifest = manifest;
            GUID = Guid.NewGuid();
        }


        private void Init()
        {
        }

        public void ChangedAircraft(AircraftInstance old_aircraft, AircraftInstance new_aircraft)
        {
            if (LoadedOn == old_aircraft)
            {
                SetLocation(new GeoPosition(LastTemporalData.PLANE_LOCATION, LastTemporalData.PLANE_ALT_ABOVE_GROUND, LastTemporalData.PLANE_HEADING_DEGREES));
            }
        }

        public void SetLocation(GeoPosition loc)
        {
            Location = loc;
            NearestAirport = Location != null ? SimLibrary.SimList[0].AirportsLib.GetAirportByRange(new GeoLoc(Location), 3).FirstOrDefault().Value : null;
        }
        
        public void UpdateLocation()
        {
            if(LoadedOn != null)
            {
                if(LoadedOn.ID == SimConnection.Aircraft.ID)
                {
                    Location = new GeoPosition(LastTemporalData.PLANE_LOCATION, LastTemporalData.PLANE_ALT_ABOVE_GROUND, LastTemporalData.PLANE_HEADING_DEGREES);
                    NearestAirport = Location != null ? SimLibrary.SimList[0].AirportsLib.GetAirportByRange(new GeoLoc(Location), 3).FirstOrDefault().Value : null;
                    
                }
            }
        }



        public void Process(Action<bool> interaction_changed)
        {
            UpdateLocation();

            bool transition_update = LoadedOn != TransitionTo;
            bool can_load = CanLoadChanged();
            bool can_deliver = CanDeliverChanged();
            bool can_unload = CanUnloadChanged();
            bool can_transfer = CanTransferChanged();
            interaction_changed(can_load || can_deliver || can_unload || can_transfer);
        }

        public void Transfer()
        {
            TransitionTo = SimConnection.Aircraft;
            Transferable = false;
            NearestAirport = null;
            AdventuresBase.SchedulePayloadUpdate = true;
        }

        public bool CanTransfer()
        {
            CanTransferChanged();
            return Transferable;
        }

        public bool CanTransferChanged()
        {
            bool new_can_transfer = false;
            if (Manifest.Adventure.IsMonitored
                && Manifest.Adventure.State == Adventures.Adventure.AState.Active
                && (TransitionTo != null ? TransitionTo.ID != SimConnection.Aircraft.ID : false)
                && !Delivered
                && !LastTemporalData.IS_SLEW_ACTIVE
                && LastTemporalData.PLANE_ALT_ABOVE_GROUND < 200
                && LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED < 2)
            {
                var test = LastTemporalData;
                double triggerRangeKilometers = 5;
                double distance = Utils.MapCalcDist(new GeoLoc(Location.Lon, Location.Lat), LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.Kilometers);
                new_can_transfer = distance < triggerRangeKilometers;
            }

            bool changed = new_can_transfer != Transferable;
            Transferable = new_can_transfer;
            return changed;
        }


        public void Load()
        {
            TransitionTo = SimConnection.Aircraft;
            NearestAirport = null;
            TakenCharge = true;
                        
            AdventuresBase.SchedulePayloadUpdate = true;
        }

        public bool CanLoad()
        {
            CanLoadChanged();
            return Loadable;
        }

        public bool CanLoadChanged()
        {
            bool new_can_load = false;
            if (Manifest.Adventure.IsMonitored
                //&& (Manifest.Adventure.Template.StrictOrder ? Manifest.Situation.Index == Manifest.Adventure.SituationAt : true)
                && Manifest.Adventure.State == Adventures.Adventure.AState.Active
                && LoadedOn == TransitionTo
                && LoadedOn == null
                && (TakenCharge ? true : Manifest.Action.Within)
                && !Delivered
                && !LastTemporalData.IS_SLEW_ACTIVE
                && LastTemporalData.PLANE_ALT_ABOVE_GROUND < 200 
                && LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED < 2)
            {
                double triggerRangeKilometers = 0.2;
                if (NearestAirport != null)
                {
                    triggerRangeKilometers = NearestAirport.Radius * 1852 / 1000;
                }
                double distance = Utils.MapCalcDist(new GeoLoc(Location.Lon, Location.Lat), LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.Kilometers);
                new_can_load = distance < triggerRangeKilometers;
            }

            bool changed = new_can_load != Loadable;
            Loadable = new_can_load;
            return changed;
        }


        public void Unload()
        {
            SetLocation(new GeoPosition(LastTemporalData.PLANE_LOCATION));
            TransitionTo = null;
            AdventuresBase.SchedulePayloadUpdate = true;

        }

        public bool CanUnload()
        {
            CanUnloadChanged();
            return Unloadable;
        }

        public bool CanUnloadChanged()
        {
            bool new_can_unload = false;
            if (Manifest.Adventure.IsMonitored 
                && Manifest.Adventure.State == Adventures.Adventure.AState.Active
                && LoadedOn == TransitionTo
                && LoadedOn != null
                && TransitionTo != null
                && !LastTemporalData.IS_SLEW_ACTIVE
                && LastTemporalData.PLANE_ALT_ABOVE_GROUND < 200 
                && LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED < 2)
            {
                if (LoadedOn.ID == SimConnection.Aircraft.ID)
                {
                    new_can_unload = true;
                }
            }

            bool changed = new_can_unload != Unloadable;
            Unloadable = new_can_unload;
            
            return changed;
        }
        

        public bool CanDeliver()
        {
            CanDeliverChanged();
            return Deliverable;
        }

        public bool CanDeliverChanged()
        {
            bool new_can_deliver = false;
            if (Manifest.Adventure.IsMonitored
                //&& (Manifest.Adventure.Template.StrictOrder ? Manifest.Situation.Index == Manifest.Adventure.SituationAt : true)
                && Manifest.Adventure.State == Adventures.Adventure.AState.Active
                && Manifest.Destinations[(int)DestinationIndex].Action.Within
                && LoadedOn != null
                && !LastTemporalData.IS_SLEW_ACTIVE
                && LastTemporalData.PLANE_ALT_ABOVE_GROUND < 200
                && LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED < 2)
            {
                if (LoadedOn.ID == SimConnection.Aircraft.ID)
                {
                    var destination = Manifest.Destinations[(int)DestinationIndex];
                    double triggerRangeKilometers = destination.Situation.TriggerRange * 1852 / 1000;

                    if (destination.Situation.Airport != null)
                    {
                        triggerRangeKilometers = destination.Situation.Airport.Radius * 1852 / 1000;
                    }
                    double distance = Utils.MapCalcDist(destination.Situation.Location, LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.Kilometers);
                    if (distance < triggerRangeKilometers)
                    {
                        new_can_deliver = true;
                    }


                }
            }

            bool changed = new_can_deliver != Deliverable;
            Deliverable = new_can_deliver;
            return changed;
        }


        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<PAXGroup> cs = new ClassSerializer<PAXGroup>(this, fields);
            cs.Generate(typeof(PAXGroup), fields);

            cs.Get("guid", fields, (f) => GUID.ToString());
            cs.Get("destination_index", fields, (f) => DestinationIndex);
            cs.Get("count_percent", fields, (f) => CountPercent);
            cs.Get("families", fields, (f) => new List<string>() );

            var result = cs.Get();
            return result;
        }

        public Dictionary<string, dynamic> SerializeState(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<PAXGroup> cs = new ClassSerializer<PAXGroup>(this, fields);
            cs.Generate(typeof(PAXGroup), fields);

            cs.Get("guid", fields, (f) => GUID.ToString());
            cs.Get("loaded_on", fields, (f) => LoadedOn != null ? LoadedOn.Serialize(f) : null);
            cs.Get("transition_to", fields, (f) => TransitionTo != null ? TransitionTo.Serialize(f) : null);
            cs.Get("loadable", fields, (f) => Loadable);
            cs.Get("transferable", fields, (f) => Transferable);
            cs.Get("unloadable", fields, (f) => Unloadable);
            cs.Get("deliverable", fields, (f) => Deliverable);
            cs.Get("delivered", fields, (f) => Delivered);
            //cs.Get("boarded", fields, (f) => Families.Sum(x => x.Humans.Sum(x1 => x1.State.Boarded ? 1 : 0)) - 1 );
            cs.Get("location", fields, (f) => Location != null ? new double[] { Location.Lon, Location.Lat, Location.Alt, Location.Hdg } : null);
            cs.Get("families", fields, (f) => new List<string>());
            cs.Get("count", fields, (f) => Count);
            cs.Get("nearest_airport", fields, (f) => NearestAirport != null ? NearestAirport.Serialize(f) : null);

            var result = cs.Get();
            return result;
        }


        public void ImportState(Dictionary<string, dynamic> state)
        {
            var ss = new StateSerializer<PAXGroup>(this, state);

            ss.Set("GUID", (v) => new Guid(state["GUID"]));
            ss.Set("LoadedOn", (v) => FleetService.GetAircraft((string)state["LoadedOn"]));
            ss.Set("Delivered");
            ss.Set("TakenCharge");
            ss.Set("DestinationIndex"); 
            ss.Set("Count");
            ss.Set("CountPercent");
            //ss.Set("Families");

            TransitionTo = LoadedOn;
            SetLocation(state["Location"] != null ? new GeoPosition((double)state["Location"][0], (double)state["Location"][1], (double)state["Location"][2], (double)state["Location"][3]) : null);
            Init();
        }

        public Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<PAXGroup>(this);
            ss.Get("GUID", (v) => GUID.ToString());
            ss.Get("LoadedOn", (v) => LoadedOn != null ? LoadedOn.Name : null);
            ss.Get("Location", (v) => Location != null ? new double[] { Location.Lon, Location.Lat, Location.Alt, Location.Hdg } : null);
            ss.Get("Delivered");
            ss.Get("TakenCharge");
            ss.Get("DestinationIndex");
            ss.Get("Count");
            ss.Get("CountPercent");
            //ss.Get("Families", (v) => new List<string>());

            return ss.Get();
        }
    }
}
