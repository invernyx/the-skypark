using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class TaxiPoint
    {
        public TaxiwayPointType Type { get; set; }
        public bool IsForward { get; set; }
        public GeoLocation Location { get; set; }

        public override string ToString()
        {
            return $"TP {Type} {Location} fwd {IsForward}";
        }
    }
}
