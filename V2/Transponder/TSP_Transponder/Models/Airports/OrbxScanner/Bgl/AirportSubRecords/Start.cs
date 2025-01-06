using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class Start : SubRecord, IHasData
    {
        public string Runway { get; set; }
        public GeoLocation Location { get; set; }
        public double Altitude { get; set; }
        public double Heading { get; set; }
        public StartType Type { get; set; }

        public Start(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        { }

        public void GetData(BinaryReader reader)
        {
            var cardinalPoints = new string[]{
                "N",
                "NE",
                "E",
                "SE",
                "S",
                "SW",
                "W",
                "NW"
            };
            var designators = new string[]
            {
                "",
                "L",
                "R",
                "C",
                "W",
                "A",
                "B"
            };

            var runwayNumber = reader.ReadByte();
            var designatorType = reader.ReadByte();
            var designator = designatorType & 0b1111;

            Type = (StartType)((designatorType & 0b11110000) >> 4);

            Runway = (runwayNumber < 37 ? runwayNumber.ToString() : cardinalPoints[runwayNumber - 37]) + designators[designator];

            var longitude = reader.ReadUInt32();
            var latitude = reader.ReadUInt32();
            Location = GeoLocation.FromDwords(latitude, longitude);

            Altitude = reader.ReadUInt32() / 1000.0;
            Heading = reader.ReadSingle();
        }

        public override string ToString()
        {
            return $"{Type} {Runway} {Location} {Altitude} {Heading}";
        }
    }
}
