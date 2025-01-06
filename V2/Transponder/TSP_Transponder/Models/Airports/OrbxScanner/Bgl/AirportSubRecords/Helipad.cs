using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class Helipad : SubRecord, IHasData
    {
        public Surface Surface { get; set; }
        public HelipadType Type { get; set; }
        public bool Transparent { get; set; }
        public bool Closed { get; set; }

        public GeoLocation Location { get; set; }
        public double Altitude { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Heading { get; set; }

        public Helipad(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
        }

        public void GetData(BinaryReader reader)
        {
            Surface = (Surface)reader.ReadByte();

            var bitMask = reader.ReadByte();

            Type = (HelipadType)(bitMask & 0b1111);
            Transparent = (bitMask & 0b10000) > 0;
            Closed = (bitMask & 0b100000) > 0;

            reader.ReadUInt32(); // unused

            var lon = reader.ReadUInt32();
            var lat = reader.ReadUInt32();
            Location = GeoLocation.FromDwords(lat, lon);

            Altitude = reader.ReadUInt32() / 1000.0;
            Length = reader.ReadSingle();
            Width = reader.ReadSingle();
            Heading = reader.ReadSingle();
        }
    }
}
