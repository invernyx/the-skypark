using System;
using System.Linq;
using System.Collections.Generic;
using TSP_Transponder.Utilities;
using TSP_Transponder.Attributes;
using System.Drawing;
using TSP_Transponder.Models.API;

namespace TSP_Transponder.Models.Aircraft.Cabin.Features
{
    public class AircraftCabinFeatureUtil : AircraftCabinFeature
    {
        private int EnterDelay = Utils.GetRandom(3, 8);
        private int ExitDelay = Utils.GetRandom(5, 15);

        public AircraftCabinFeatureUtil() : base()
        {
            Type = AircraftCabinFeatureType.Util;

        }

        public override void Process()
        {

        }

        public override void Interact(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure, Dictionary<string, dynamic> payload_struct)
        {
            base.Interact(Socket, StructSplit, structure, payload_struct);
        }

        public override Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<AircraftCabinFeatureUtil> cs = new ClassSerializer<AircraftCabinFeatureUtil>(this, fields);
            cs.Generate(typeof(AircraftCabinFeatureUtil), fields);

            cs.Get("type", fields, (f) => EnumAttr.GetDescription(Type));
            cs.Get("layer", fields, (f) => EnumAttr.GetDescription(Layer));
            cs.Get("orientation", fields, (f) => EnumAttr.GetDescription(Orientation));

            return cs.Get();
        }

        public override AircraftCabinFeature Deserialize(Dictionary<string, dynamic> state)
        {
            base.Deserialize(state);

            ConfigSerializer<AircraftCabinFeatureUtil> cs = new ConfigSerializer<AircraftCabinFeatureUtil>(this, state);

            return this;
        }

        public override AircraftCabinFeature ImportState(Dictionary<string, dynamic> state)
        {
            base.ImportState(state);

            var ss = new StateSerializer<AircraftCabinFeatureUtil>(this, state);

            switch(SubType)
            {
                case "lavatories":
                    {
                        ExitDelay = 30;
                        break;
                    }
            }
            
            return this;
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<AircraftCabinFeatureUtil>(this);

            return ss.Get().Union(base.ExportState()).ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
