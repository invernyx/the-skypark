using System;
using System.Linq;
using System.Collections.Generic;
using TSP_Transponder.Utilities;
using TSP_Transponder.Attributes;
using System.Drawing;
using LiteDB;
using TSP_Transponder.Models.API;

namespace TSP_Transponder.Models.Aircraft.Cabin.Features
{
    public class AircraftCabinFeatureStairs : AircraftCabinFeature
    {
        public AircraftCabinFeatureStairs() : base()
        {
            Type = AircraftCabinFeatureType.Stairs;
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
            ClassSerializer<AircraftCabinFeatureStairs> cs = new ClassSerializer<AircraftCabinFeatureStairs>(this, fields);
            cs.Generate(typeof(AircraftCabinFeatureStairs), fields);

            cs.Get("type", fields, (f) => EnumAttr.GetDescription(Type));
            cs.Get("layer", fields, (f) => EnumAttr.GetDescription(Layer));
            cs.Get("orientation", fields, (f) => EnumAttr.GetDescription(Orientation));

            return cs.Get();
        }

        public override AircraftCabinFeature Deserialize(Dictionary<string, dynamic> state)
        {
            base.Deserialize(state);

            ConfigSerializer<AircraftCabinFeatureStairs> cs = new ConfigSerializer<AircraftCabinFeatureStairs>(this, state);

            //cs.Set("Floors");

            return this;
        }

        public override AircraftCabinFeature ImportState(Dictionary<string, dynamic> state)
        {
            base.ImportState(state);

            var ss = new StateSerializer<AircraftCabinFeatureStairs>(this, state);
            
            //ss.Set("Floors");

            return this;
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<AircraftCabinFeatureStairs>(this);

            //ss.Get("Floors");

            return ss.Get().Union(base.ExportState()).ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
