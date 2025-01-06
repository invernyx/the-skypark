using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.Collections.Generic;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class TaxiParking
    {
        public int Number { get; set; }
        public ParkingType Type { get; set; }
        public PushbackType Pushback { get; set; }
        public ParkingName Name { get; set; }
        public double Radius { get; set; }
        public double Heading { get; set; }
        public List<double> TeeOffsets { get; set; }
        public GeoLocation Location { get; set; }
        public List<string> AirlineCodes { get; set; }

        public override string ToString()
        {
            return $"{Type} {Name} {Number}";
        }
    }
}
