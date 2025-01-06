using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    class ApronCollection : SubRecord, IHasData
    {
        public Apron Apron { get; set; }

        public ApronCollection(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
        }

        public void GetData(BinaryReader reader)
        {
            var surface = reader.ReadByte();
            var vertexCount = reader.ReadUInt16();

            Apron = new Apron
            {
                Surface = (Surface)surface
            };

            for (int i = 0; i < vertexCount; i++)
            {
                var lon = reader.ReadUInt32();
                var lat = reader.ReadUInt32();

                Apron.Vertices.Add(GeoLocation.FromDwords(lat, lon));
            }

            Skip();

            var secondPos = reader.BaseStream.Position;

            var secondId = reader.ReadUInt16();
            if (secondId != 0x0030)
                throw new System.IO.FileFormatException("Can't read second apron entry");

            reader.BaseStream.Seek(secondPos + reader.ReadUInt32(), SeekOrigin.Begin);
        }
    }
}
