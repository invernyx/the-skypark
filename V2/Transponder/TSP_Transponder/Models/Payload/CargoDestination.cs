using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Airports;

namespace TSP_Transponder.Models.Payload
{
    public class CargoDestination
    {
        public GeoLoc Location = null;
        public Airport Airport = null;

        public CargoDestination(Dictionary<string, dynamic> config)
        {
        }

        public CargoDestination()
        {
        }

        public void ImportState(Dictionary<string, dynamic> state)
        {
            Location = new GeoLoc((double)state["Location"][0], (double)state["Location"][1]);
            Airport = SimLibrary.SimList[0].AirportsLib.GetByICAO(state["Airport"], Location);
        }

        public Dictionary<string, dynamic> ExportState()
        {
            return new Dictionary<string, dynamic>()
            {
                { "Location", new double[] { Location.Lon, Location.Lat } },
                { "Airport", Airport != null ? Airport.ToSummary(false) : null }
            };
        }

    }
}
