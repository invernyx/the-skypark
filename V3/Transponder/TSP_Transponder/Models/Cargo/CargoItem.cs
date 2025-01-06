using System;
using System.Collections.Generic;
using TSP_Transponder.Attributes;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Aircraft.Cabin;
using TSP_Transponder.Models.Payload;
using TSP_Transponder.Models.WorldManager;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Cargo
{
    public class CargoItem
    {
        public Adventure Adventure = null;
        
        public string GUID { get; set; } = "";
        
        [ClassSerializerField("action_id_origin")]
        public int ActionIdOrigin { get; set; } = -1;
        
        [ClassSerializerField("action_id_destination")]
        public int ActionIdDestination { get; set; } = -1;

        [StateSerializerField("State")]
        public CargoItemState State = null;

        public AircraftCabin Cabin = null;
        public CargoGroup Group = null;
        public Scene_Obj Item = null;

        public CargoItem()
        {
            GUID = Guid.NewGuid().ToString();
            State = new CargoItemState(this);
        }


        public void Init(string SocketID, Adventure adventure, CargoGroup group)
        {
            Group = group;

            if (group != null)
            {
                Adventure = group.Manifest.Adventure;
                ActionIdOrigin = Group.Manifest.Action.UID;
                ActionIdDestination = Group.Manifest.Destinations[Group.DestinationIndex].Action.UID;
            }

            State.Init();
            
            string pallet_obj = "SM_Generic_Crates_None";
            double weight = Group.Manifest.Definition.WeightKG;

            if (weight > 900)
            {
                pallet_obj = "SM_Generic_Crates_Large";
            }
            else if (weight > 300)
            {
                pallet_obj = "SM_Generic_Crates_Medium";
            }
            else
            {
                pallet_obj = "SM_Generic_Crates_Small";
            }

            Item = new Scene_Obj(Adventure.SocketID, "_welcome", null, SceneObjType.Dynamic)
            {
                File = pallet_obj
            };
        }

        public void ChangedAircraft(AircraftInstance old_aircraft, AircraftInstance new_aircraft)
        {
            ValidateCargoState(true);
        }
        
        public void ValidateCargoState(bool reset)
        {
            if (Cabin != null)
            {
                if (Group != null ? (Cabin.Aircraft == Group.TransitionTo) : true)
                {
                    lock (Cabin.Cargos)
                    {
                        if (!Cabin.Cargos.Contains(this))
                        {
                            Cabin.Cargos.Add(this);
                        }
                        if (reset)
                        {
                            Reset();
                        }
                    }
                }
                else
                {
                    lock (Cabin.Cargos)
                    {
                        if (Cabin.Cargos.Contains(this))
                        {
                            if (reset)
                            {
                                Reset();
                            }
                            Cabin.Cargos.Remove(this);
                        }
                    }
                }
            }
        }


        public void Reset()
        {
            State.Boarded = false;

            AdventuresBase.SchedulePayloadUpdate = true;
        }

        public void CabinRefresh(AircraftCabin cabin)
        {
            Cabin = cabin;
            State.CabinRefresh(cabin);
            ValidateCargoState(false);
        }
        

        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<CargoItem> cs = new ClassSerializer<CargoItem>(this, fields);
            cs.Generate(typeof(CargoItem), fields);

            cs.Get("guid", fields, (f) => GUID + "_" + (Group != null ? Group.Manifest.UID : 0));

            var result = cs.Get();
            return result;
        }

        public Dictionary<string, dynamic> SerializeState(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<CargoItem> cs = new ClassSerializer<CargoItem>(this, fields);
            cs.Generate(typeof(CargoItem), fields);

            cs.Get("guid", fields, (f) => GUID + "_" + (Group != null ? Group.Manifest.UID : 0));
            cs.Get("state", fields, (f) => State.SerializeState(f));

            var result = cs.Get();
            return result;
        }

        public CargoItem ImportState(Dictionary<string, dynamic> state)
        {
            var ss = new StateSerializer<CargoItem>(this, state);

            ss.Set("GUID");
            ss.Set("Boarded");
            ss.Set("X");
            ss.Set("Y");
            ss.Set("Z");

            return this;
        }

        public Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<CargoItem>(this);

            ss.Get("GUID");
            ss.Get("Boarded");
            ss.Get("X");
            ss.Get("Y");
            ss.Get("Z");

            return ss.Get();
        }
    }
}
