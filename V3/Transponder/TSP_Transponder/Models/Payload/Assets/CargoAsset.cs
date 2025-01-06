using System;
using System.Collections.Generic;

namespace TSP_Transponder.Models.Payload.Assets
{
    
    class CargoAsset
    {
        internal string Name = "";
        internal string GUID = "";
        internal List<string> Models = new List<string>();
        internal List<string> TransportModels = new List<string>();
        internal List<string> Tags = new List<string>();
        internal bool CanInsure = false;
        internal uint Frequency = 0;
        internal uint WeightKG = 42;
        internal uint Hazard = 0;
        internal float Value = 0;
        internal float GMin = 0;
        internal float GMax = 0;
        internal float DamagePerGPerS = 0;
        internal float KarmaAdjust = 0;
        internal float KarmaMin = 0;
        internal float KarmaMax = 0;
        internal float FlightHoursMin = 0;
        internal float FlightHoursMax = 0;
        internal float XPScoreMin = 0;
        internal float XPScoreMax = 0;

        public override string ToString()
        {
            return Name + " - " + String.Join(", ", Models.ToArray());
        }

    }
}
