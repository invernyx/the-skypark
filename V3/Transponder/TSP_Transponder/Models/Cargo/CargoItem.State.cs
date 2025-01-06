using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Aircraft.Cabin;
using TSP_Transponder.Models.Aircraft.Cabin.Features;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Cargo
{
    public class CargoItemState
    {
        public CargoItem Host = null;

        [ClassSerializerField("boarded")]
        [StateSerializerField("Boarded")]
        public bool Boarded { get; set; } = false;

        [ClassSerializerField("x")]
        [StateSerializerField("X")]
        public ushort X { get; set; } = 0;

        [ClassSerializerField("y")]
        [StateSerializerField("Y")]
        public ushort Y { get; set; } = 0;

        [ClassSerializerField("z")]
        [StateSerializerField("Z")]
        public ushort Z { get; set; } = 0;

        [ClassSerializerField("assigned_pod")]
        [StateSerializerField("AssignedPod")]
        public string AssignedPodGUID { get; set; } = null;
        public AircraftCabinFeature AssignedPod { get; set; } = null;

        public AircraftCabin Cabin = null;

        public AircraftCabinFeature CurrentlyAt = null;

        private ushort NewX = 0;
        private ushort NewY = 0;
        private ushort NewZ = 0;

        public CargoItemState(CargoItem Host)
        {
            this.Host = Host;
        }

        public void Init()
        {
            Boarded = Host.Group == null ? true : (Host.Group.TransitionTo == null ? false : Boarded);
            
        }

        public void CabinRefresh(AircraftCabin cabin)
        {
            Cabin = cabin;

            // Validate that the seat feature exists and hasn't been taken
            if (AssignedPodGUID != null)
            {
                if (Cabin.ValidatePodAssign(AssignedPodGUID, Host.GUID))
                {
                    var PodFeature = Cabin.Features.Find(x => x.GUID == AssignedPodGUID);
                    AssignedPod = PodFeature;
                    if (PodFeature == null)
                    {
                        AssignedPodGUID = null;
                    }
                }
            }

            // If we had a door assigned, change that
            if (AssignedPod != null)
            {
                if (AssignedPod.Type != AircraftCabinFeatureType.Seat)
                {
                    var NewPodFeature = Cabin.GetPodAssign(Host);
                    AssignPod(NewPodFeature);
                }
            }

            // Check if we're out of bounds
            if (X > cabin.Levels[Z][0] + cabin.Levels[Z][2] || Y > cabin.Levels[Z][1] + cabin.Levels[Z][3] || Z > cabin.Levels.Count)
            {
                ResetPosition();
            }
            
            if (Boarded)
            {
                CurrentlyAt = cabin.Features.Find(f => f.X == X && f.Y == Y && f.Z == Z);

                if (CurrentlyAt == null)
                {
                    ResetPosition();
                }
            }

        }

        private void ResetPosition()
        {

        }

        private void AssignPod(AircraftCabinFeature NewSeatFeature)
        {
            if (NewSeatFeature != null)
            {
                AssignedPod = NewSeatFeature;
                AssignedPodGUID = NewSeatFeature.GUID;


            }
        }

        public bool ProcessState()
        {
            var changed = false;



            return changed;
        }

        public void ProcessCargo(List<CargoItem> changes, List<CargoItem> added)
        {
            if (Cabin == null)
                return;

            bool new_boarded = Host.Group != null ? (Host.Group.TransitionTo != null) : true;
            bool changed = false;
            bool add = false;

            if (new_boarded != Boarded)
            {
                if (new_boarded)
                {
                    add = true;

                    // Assign a new pod
                    if (AssignedPod == null || AssignedPodGUID == null)
                    {
                        AssignPod(Cabin.GetPodAssign(Host));
                    }

                    if (AssignedPod != null)
                    {
                        Boarded = true;
                        NewX = AssignedPod.X;
                        NewY = AssignedPod.Y;
                        NewZ = AssignedPod.Z;
                        CurrentlyAt = AssignedPod;
                        AdventuresBase.SchedulePayloadUpdate = true;
                    }
                }
                else
                {
                    AssignedPodGUID = null;
                    AssignedPod = null;
                    Boarded = false;
                    AdventuresBase.SchedulePayloadUpdate = true;
                }
            }

            if (CurrentlyAt == null)
            {
                CurrentlyAt = Cabin.Features.Find(f => f.X == X && f.Y == Y && f.Z == Z);
                changed = true;
            }

            // Process actions
            if (ProcessState())
            {
                changed = true;
            }

            // Check if the position changed
            if (X != NewX || Y != NewY || Z != NewZ)
            {
                X = NewX;
                Y = NewY;
                Z = NewZ;
                changed = true;
            }

            // Add changes to the tracking list
            if (changed)
                changes.Add(Host);

            if (add)
                added.Add(Host);
        }


        public Dictionary<string, dynamic> SerializeState(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<CargoItemState> cs = new ClassSerializer<CargoItemState>(this, fields);
            cs.Generate(typeof(CargoItemState), fields);
            
            var result = cs.Get();
            return result;
        }

        public CargoItemState ImportState(Dictionary<string, dynamic> state)
        {
            var ss = new StateSerializer<CargoItemState>(this, state);
            
            ss.Set("Boarded");
            
            ss.Set("X");
            ss.Set("Y");
            ss.Set("Z");
            ss.Set("AssignedPod");

            NewX = X;
            NewY = Y;
            NewZ = Z;

            return this;
        }

        public Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<CargoItemState>(this);
            
            ss.Get("Boarded");
            
            ss.Get("X");
            ss.Get("Y");
            ss.Get("Z");
            ss.Get("AssignedPod");

            return ss.Get();
        }
    }
}
