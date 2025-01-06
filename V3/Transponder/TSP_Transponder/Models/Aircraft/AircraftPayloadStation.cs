using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Aircraft
{
    public class AircraftPayloadStation
    {
        [BsonField("Name")]
        public string Name { get; set; } = "";

        [BsonField("Type")]
        public string Type { get; set; } = "default";

        [BsonField("X")]
        public float X { get; set; } = 0;

        [BsonField("Y")]
        public float Y { get; set; } = 0;

        [BsonField("Z")]
        public float Z { get; set; } = 0;
        
        public float Load = 0;

        public AircraftPayloadStation()
        {

        }

        public AircraftPayloadStation(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        internal Dictionary<string, dynamic> ToDictionary()
        {
            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
            {
                { "Name", Name },
                { "Type", Type },
                { "Load", Load },
                { "X", X },
                { "Y", Y },
                { "Z", Z },
            };

            return rs;
        }
    }
}
