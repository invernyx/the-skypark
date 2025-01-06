﻿using System;
using System.Linq;
using System.Collections.Generic;
using TSP_Transponder.Utilities;
using TSP_Transponder.Attributes;
using TSP_Transponder.Models.API;

namespace TSP_Transponder.Models.Aircraft.Cabin.Features
{
    public class AircraftCabinFeatureDoor : AircraftCabinFeature
    {
        public AircraftCabinFeatureDoor() : base()
        {
            Type = AircraftCabinFeatureType.Door;

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
            ClassSerializer<AircraftCabinFeatureDoor> cs = new ClassSerializer<AircraftCabinFeatureDoor>(this, fields);
            cs.Generate(typeof(AircraftCabinFeatureDoor), fields);

            cs.Get("type", fields, (f) => EnumAttr.GetDescription(Type));
            cs.Get("layer", fields, (f) => EnumAttr.GetDescription(Layer));
            cs.Get("orientation", fields, (f) => EnumAttr.GetDescription(Orientation));

            return cs.Get();
        }

        public override AircraftCabinFeature Deserialize(Dictionary<string, dynamic> state)
        {
            base.Deserialize(state);

            ConfigSerializer<AircraftCabinFeatureDoor> cs = new ConfigSerializer<AircraftCabinFeatureDoor>(this, state);
            
            return this;
        }

        public override AircraftCabinFeature ImportState(Dictionary<string, dynamic> state)
        {
            base.ImportState(state);

            var ss = new StateSerializer<AircraftCabinFeatureDoor>(this, state);

            return this;
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<AircraftCabinFeatureDoor>(this);

            return ss.Get().Union(base.ExportState()).ToDictionary(k => k.Key, v => v.Value);
        }

    }
}
