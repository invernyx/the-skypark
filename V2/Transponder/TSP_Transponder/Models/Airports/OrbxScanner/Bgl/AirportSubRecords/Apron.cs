using Orbx.DataManager.Core.Common;
using Orbx.DataManager.Core.Esp;
using System.Collections.Generic;

namespace Orbx.DataManager.Core.Bgl.AirportSubRecords
{
    public class Apron
    {
        public Surface Surface { get; set; }
        public List<GeoLocation> Vertices { get; set; }

        public Apron()
        {
            Vertices = new List<GeoLocation>();
        }
    }
}
