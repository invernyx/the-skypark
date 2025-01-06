using System.Collections.Generic;
using TSP_Transponder.Models.Airports;
using static TSP_Transponder.Models.Airports.AirportsLib;

namespace TSP_Transponder.Models.Adventures
{
    public class CreationParams
    {
        public Dictionary<string, dynamic> Filters = null;
        public List<Airport> IncludeICAO = new List<Airport>();
        public List<Airport> ExcludeICAO = new List<Airport>();
        public List<Airport> DepICAO = new List<Airport>();
        public List<Airport> ArrICAO = new List<Airport>();
        public bool StatusUpdate = false;
        public int Limit = -1;

    }
}
