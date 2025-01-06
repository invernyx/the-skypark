using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class TaxiPointCollection : SubRecord, IHasData
    {
        public List<TaxiPoint> Points { get; set; }

        public TaxiPointCollection(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            Points = new List<TaxiPoint>();
        }

        public void GetData(BinaryReader reader)
        {
            var pointCount = reader.ReadUInt16();

            for (int i = 0; i < pointCount; i++)
            {
                var type = (TaxiwayPointType)reader.ReadByte();
                var forward = reader.ReadByte() == 1;
                var discard = reader.ReadUInt16();
                var longitude = reader.ReadUInt32();
                var latitude = reader.ReadUInt32();

                Points.Add(new TaxiPoint
                {
                    Type = type,
                    IsForward = forward,
                    Location = GeoLocation.FromDwords(latitude, longitude)
                });
            }
        }
    }
}
