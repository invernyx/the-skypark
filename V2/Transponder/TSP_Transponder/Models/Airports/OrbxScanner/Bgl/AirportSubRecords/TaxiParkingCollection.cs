using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.Collections.Generic;
using System.IO;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class TaxiParkingCollection : SubRecord, IHasData
    {
        public List<TaxiParking> Parking { get; set; }

        public TaxiParkingCollection(BinaryReader reader, int identifier)
            : base(reader, identifier, true)
        {
            Parking = new List<TaxiParking>();
        }

        public void GetData(BinaryReader reader)
        {
            var parkingCount = reader.ReadUInt16();

            for (int i = 0; i < parkingCount; i++)
            {
                var main = reader.ReadUInt32();
                var airlineCodeCount = (main >> 24) & 0b11111111;

                var radius = reader.ReadSingle();
                var heading = reader.ReadSingle();
                var tee1 = reader.ReadSingle();
                var tee2 = reader.ReadSingle();
                var tee3 = reader.ReadSingle();
                var tee4 = reader.ReadSingle();
                var longitude = reader.ReadUInt32();
                var latitude = reader.ReadUInt32();

                for (int y = 0; y < airlineCodeCount; y++)
                {
                    // read airline code
                    reader.ReadUInt32();
                }

                Parking.Add(new TaxiParking
                {
                    Number = (int)((main >> 12) & 0b111111111111),
                    Type = (ParkingType)((main >> 8) & 0b1111),
                    Pushback = (PushbackType)((main >> 6) & 0b11),
                    Name = (ParkingName)(main & 0b111111),
                    Radius = radius,
                    Heading = heading,
                    TeeOffsets = new List<double>
                    {
                        tee1,
                        tee2,
                        tee3,
                        tee4
                    },
                    Location = GeoLocation.FromDwords(latitude, longitude)
                });
            }
        }
    }
}
