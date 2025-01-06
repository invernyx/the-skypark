using LiteDB;
using System;
using System.Collections.Generic;
using System.Drawing;
using TSP_Transponder.Attributes;
using TSP_Transponder.Models.API;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Attributes.EnumAttr;

namespace TSP_Transponder.Models.Aircraft.Cabin.Features
{
    public class AircraftCabinFeature
    {
        private string _GUID = null;
        [ConfigSerializerField("guid")]
        [ClassSerializerField("guid")]
        [StateSerializerField("GUID")]
        public string GUID {
            get
            {
                if(_GUID == null)
                {
                    _GUID = X + ":" + Y + ":" + Z + ":" + GetDescription(Type);
                }

                return _GUID;
            }
        }

        [ConfigSerializerField("type")]
        [StateSerializerField("Type")]
        public AircraftCabinFeatureType Type { get; set; } = AircraftCabinFeatureType.Door;

        [ConfigSerializerField("orientation")]
        [StateSerializerField("Orientation")]
        public AircraftCabinFeatureOrientation Orientation { get; set; } = AircraftCabinFeatureOrientation.Up;

        [ConfigSerializerField("layer")]
        [StateSerializerField("Layer")]
        public AircraftCabinFeatureLayer Layer { get; set; } = AircraftCabinFeatureLayer.Floor;

        [ConfigSerializerField("sub_type")]
        [ClassSerializerField("sub_type")]
        [StateSerializerField("SubType")]
        public string SubType { get; set; } = "";
        
        [ConfigSerializerField("x")]
        [ClassSerializerField("x")]
        [StateSerializerField("X")]
        public ushort X { get; set; } = 0;

        [ConfigSerializerField("y")]
        [ClassSerializerField("y")]
        [StateSerializerField("Y")]
        public ushort Y { get; set; } = 0;

        [ConfigSerializerField("z")]
        [ClassSerializerField("z")]
        [StateSerializerField("Z")]
        public ushort Z { get; set; } = 0;
        
        [ConfigSerializerField("uses")]
        [ClassSerializerField("uses")]
        [StateSerializerField("Uses")]
        public uint Uses = 0;

        public AircraftCabin Cabin = null;
        public List<AircraftCabinPath> AccessPaths = null;
        public bool AccessIsPath = false;

        public AircraftCabinFeature()
        {
        }

        public virtual void CabinRefresh(AircraftCabin cabin)
        {
            Cabin = cabin;
        }

        public virtual void Process()
        {

        }

        public virtual void Interact(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure, Dictionary<string, dynamic> payload_struct)
        {
            Dictionary<string, dynamic> Data = ((Dictionary<string, dynamic>)payload_struct["data"]);

            X = Convert.ToUInt16(Data["x"]);
            Y = Convert.ToUInt16(Data["y"]);
            Z = Convert.ToUInt16(Data["z"]);
            Orientation = Data.ContainsKey("orientation") ? (AircraftCabinFeatureOrientation)GetEnum(typeof(AircraftCabinFeatureOrientation), (string)Data["orientation"]) : AircraftCabinFeatureOrientation.Up;
            Cabin.ForceRefresh = true;
        }

        public virtual Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            return null;
        }

        public virtual AircraftCabinFeature Deserialize(Dictionary<string, dynamic> state)
        {
            var ss = new ConfigSerializer<AircraftCabinFeature>(this, state);
            
            ss.Set("orientation", (v) => (AircraftCabinFeatureOrientation)GetEnum(typeof(AircraftCabinFeatureOrientation), (string)v));
            ss.Set("type", (v) => (AircraftCabinFeatureType)GetEnum(typeof(AircraftCabinFeatureType), (string)v));
            ss.Set("layer", (v) => (AircraftCabinFeatureLayer)GetEnum(typeof(AircraftCabinFeatureLayer), (string)v));
            ss.Set("sub_type");
            ss.Set("x");
            ss.Set("y");
            ss.Set("z");

            return this;
        }


        public virtual AircraftCabinFeature ImportState(Dictionary<string, dynamic> state)
        {
            var ss = new StateSerializer<AircraftCabinFeature>(this, state);
            
            ss.Set("Orientation", (v) => (AircraftCabinFeatureOrientation)GetEnum(typeof(AircraftCabinFeatureOrientation), (string)v));
            ss.Set("Type", (v) => (AircraftCabinFeatureType)GetEnum(typeof(AircraftCabinFeatureType), (string)v));
            ss.Set("Layer", (v) => (AircraftCabinFeatureLayer)GetEnum(typeof(AircraftCabinFeatureLayer), (string)v));
            ss.Set("SubType");
            ss.Set("X");
            ss.Set("Y");
            ss.Set("Z");

            return this;
        }

        public virtual Dictionary<string, dynamic> ExportState()
        {
            var ss = new StateSerializer<AircraftCabinFeature>(this);
            
            ss.Get("Orientation", (v) => GetDescription(Orientation));
            ss.Get("Type", (v) => GetDescription(Type));
            ss.Get("Layer", (v) => GetDescription(Layer));
            ss.Get("SubType");
            ss.Get("X");
            ss.Get("Y");
            ss.Get("Z");

            return ss.Get();
        }
    }

    public enum AircraftCabinFeatureOrientation
    {
        [EnumValue("up")]
        Up = 0,
        [EnumValue("right")]
        Right = 1,
        [EnumValue("bottom")]
        Down = 2,
        [EnumValue("left")]
        Left = 3
    }

    public enum AircraftCabinFeatureType
    {
        [EnumValue("door")]
        Door = 0,
        [EnumValue("seat")]
        Seat = 1,
        [EnumValue("path")]
        Path = 2,
        [EnumValue("util")]
        Util = 3,
        [EnumValue("stairs")]
        Stairs = 4,
        [EnumValue("cargo")]
        Cargo = 5
    }

    public enum AircraftCabinFeatureLayer
    {
        [EnumValue("floor")]
        Floor = 0,
        [EnumValue("wall")]
        Wall = 1,
        [EnumValue("ceiling")]
        Ceiling = 2,
    }
}
