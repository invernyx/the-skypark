using LiteDB;
using NAudio.SoundFont;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using TSP_Transponder.Models.Airports;
using static TSP_Transponder.Models.Adventures.RouteGeneration.RouteGenerator;
using static TSP_Transponder.Utilities.Countries;

namespace TSP_Transponder.Models.Adventures.RouteGenerationV2
{
    class RouteGenSituation
    {
        internal uint UID = 0;
        internal ushort Index = 0;
        internal ushort ProcessOnLoopIndex = 0;
        internal RouteSituationType SituationType = RouteSituationType.Any;

        internal List<string> SituationTypeParams = null;
        internal Dictionary<string, dynamic> SituationStructure = null;
        internal RouteGen Generator = null;
        internal RouteGenRoute Route = null;
        internal RouteGenSituationFilter Filter = null;
        internal List<Airport> PossibleAirports = null;

        internal RouteGenSituation Previous = null;
        internal RouteGenSituation Next = null;

        internal GeoLoc Location = null;
        internal string LocationName = null;
        internal Airport Airport = null;

        public RouteGenSituation(RouteGen Generator, RouteGenRoute Route, ushort Index, Dictionary<string, dynamic> SituationStructure)
        {
            this.UID = Convert.ToUInt32(SituationStructure["UID"]);
            this.Index = Index;
            this.Generator = Generator;
            this.Route = Route;
            this.SituationStructure = SituationStructure;

            this.PossibleAirports = Generator.PossibleAirports;

            switch ((string)SituationStructure["SituationType"])
            {
                case "Any": SituationType = RouteSituationType.Any; break;
                case "Country": SituationType = RouteSituationType.Country; break;
                case "ICAO": SituationType = RouteSituationType.ICAO; break;
                case "Geo": SituationType = RouteSituationType.Geo; break;
                case "Situation": SituationType = RouteSituationType.Situation; break;
                case "Location": SituationType = RouteSituationType.Location; break;
            }

            switch(SituationType)
            {
                case RouteSituationType.ICAO:
                case RouteSituationType.Geo:
                case RouteSituationType.Situation:
                    {
                        ProcessOnLoopIndex = 0;
                        break;
                    }
                case RouteSituationType.Location:
                    {
                        ProcessOnLoopIndex = 1;
                        break;
                    }
                case RouteSituationType.Any:
                case RouteSituationType.Country:
                    {
                        ProcessOnLoopIndex = 2;
                        break;
                    }
            }

            if (SituationStructure.ContainsKey("SituationTypeParams"))
                SituationTypeParams = ((ArrayList)SituationStructure["SituationTypeParams"]).Cast<string>().ToList();
        }

        public void Init()
        {
            if (Route != null)
            {
                Previous = Index > 0 ? Route.Situations[Index - 1] : null;
                Next = Index < Route.Situations.Count - 1 ? Route.Situations[Index + 1] : null;
            }
            else
            {
                Filter = new RouteGenSituationFilter(this, Generator, SituationStructure);
                if(Filter.SituationType == RouteSituationType.Any || Filter.SituationType == RouteSituationType.Country)
                {
                    var airports = PossibleAirports.AsEnumerable();
                    airports = Filter.FilterAirportsMeta(airports);
                    airports = Filter.FilterAirportsBounds(airports);
                    this.PossibleAirports = airports.ToList();
                }
            }
        }

        public void Find(int Pass)
        {
            if(ProcessOnLoopIndex == Pass)
            {
                // Process the initial pass based on type
                switch (SituationType)
                {
                    case RouteSituationType.ICAO: ProcessTypeICAO(); break;
                    case RouteSituationType.Geo: ProcessTypeGeo(); break;
                    case RouteSituationType.Location: ProcessTypeLocation(); break;
                    case RouteSituationType.Any: ProcessTypeAny(); break;
                    case RouteSituationType.Country: ProcessTypeCountry(); break;
                }
            }

            // Process the initial pass based on type
            switch (SituationType)
            {
                case RouteSituationType.Situation: ProcessTypeSituation(); break;
            }
        }

        private void FinalizeLocation()
        {
            // Reduce possible airports on neighboring situations
            if (Previous != null)
            {
                if (Previous.Location == null)
                {
                    switch (Previous.SituationType)
                    {
                        case RouteSituationType.Any:
                        case RouteSituationType.Country:
                            {
                                // Find possible iarports on the previous situation if null
                                Previous.PossibleAirports = Previous.Filter.FilterAirportsDistance(Previous.PossibleAirports, Location, (int)Previous.Filter.DistToNextMinKM, (int)Previous.Filter.DistToNextMaxKM).ToList();

                                // If what we've just set has zero results, let's kill it.
                                if (Previous.PossibleAirports.Count == 0) { Route.IsImpossible = true; return; }
                                break;
                            }
                    }
                }
            }

            if (Next != null)
            {
                if (Next.Location == null)
                {
                    switch (Next.SituationType)
                    {
                        case RouteSituationType.Any:
                        case RouteSituationType.Country:
                            {
                                // Find possible iarports on the previous situation if null
                                Next.PossibleAirports = Next.Filter.FilterAirportsDistance(Next.PossibleAirports, Location, (int)Filter.DistToNextMinKM, (int)Filter.DistToNextMaxKM).ToList();

                                // If what we've just set has zero results, let's kill it.
                                if (Next.PossibleAirports.Count == 0) { Route.IsImpossible = true; return; }
                                break;
                            }
                    }
                }
            }
        }

        private void FinalizeAirportChoice()
        {
            WeightedRandom<Airport> airport_choices = new WeightedRandom<Airport>();

            // If we don't have any airport to work right, kill it.
            if(PossibleAirports.Count == 0) { Route.IsImpossible = true; return; }

            // Add all possible airports to the calculator
            foreach(var airport in PossibleAirports)
            {
                KeyValuePair<Airport, uint[]> result = new KeyValuePair<Airport, uint[]>();

                // Get the airport in the Airport Uses
                lock (Generator.AirportUses)
                    if(Generator.AirportUses.ContainsKey(airport)) 
                        result = new KeyValuePair<Airport, uint[]>(airport, Generator.AirportUses[airport]);

                var apt_rating = 1.0;

                if (Previous != null)
                {
                    if (Previous.Location != null)
                    {
                        var hdg = Route.GetAvgHdg();
                        if (hdg != null)
                        {
                            var bearing = Utils.MapCalcBearing(Previous.Location, airport.Location);
                            var comp = Math.Abs(Utils.MapCompareBearings(bearing, (double)hdg));

                            var hdg_factor = 1 + (comp / 70);
                            var hdg_divider = Math.Pow(hdg_factor, PossibleAirports.Count);
                            apt_rating /= hdg_divider;

                            //var hdg_factor = 1 - Math.Min(comp / 180, 0.9999);
                            //apt_rating *= hdg_factor;
                        }
                    }
                }

                if (Next != null)
                {
                    if (Next.Location != null)
                    {
                        var hdg = Route.GetAvgHdg();
                        if (hdg != null)
                        {
                            var bearing = Utils.MapCalcBearing(airport.Location, Next.Location);
                            var comp = Math.Abs(Utils.MapCompareBearings(bearing, (double)hdg));

                            var hdg_factor = 1 + (comp / 70);
                            var hdg_divider = Math.Pow(hdg_factor, PossibleAirports.Count);
                            apt_rating /= hdg_divider;

                            //var hdg_factor = 1 - Math.Min(comp / 180, 0.9999);
                            //apt_rating *= hdg_factor;
                        }
                    }
                }

                if (result.Key != null)
                {
                    var factor_approved = (result.Value[0] + 1f) / (result.Value[1] + 1f);
                    var factor_uses = Math.Pow((result.Value[0] + 1f), 2);
                    apt_rating *= factor_approved / factor_uses;
                }

                //if (apt_rating > 0.00001)
                    airport_choices.AddEntry(airport, apt_rating * 10000);
            }

            // If we don't have any choices, kill it.
            if(airport_choices.Count == 0) { Route.IsImpossible = true; return; }

            Airport = airport_choices.GetRandom();
            Location = Airport.Location;

            FinalizeLocation();
        }


        private void ProcessTypeAny()
        {
            if(Airport == null)
            {
                // Basic filtering
                IEnumerable<Airport> airports = PossibleAirports;

                bool HasNeighbor = false;
                bool ShouldPass = false;

                // Find airports within range of the previous Situation if set
                if (Previous != null ? Previous.Location != null : false)
                {
                    airports = Filter.FilterAirportsDistance(airports, Previous.Location, (int)Previous.Filter.DistToNextMinKM, (int)Previous.Filter.DistToNextMaxKM);
                    HasNeighbor = true;
                }
                // If we're not the seed and not on edge, move to next pass
                else if (Index > 0 && Route.RouteSeedIndex != Index)
                    ShouldPass = true;

                // Find airports within range of the next Situation if set
                if (Next != null ? Next.Location != null : false)
                {
                    airports = Filter.FilterAirportsDistance(airports, Next.Location, (int)Filter.DistToNextMinKM, (int)Filter.DistToNextMaxKM);
                    HasNeighbor = true;
                }
                // If we're not the seed and not on edge, move to next pass
                else if (Index < Route.Situations.Count - 1 && Route.RouteSeedIndex != Index)
                    ShouldPass = true;

                // Skip to next loop
                if (!HasNeighbor && ShouldPass) { ProcessOnLoopIndex++; return; }

                PossibleAirports = airports.ToList();
                FinalizeAirportChoice();
            }
        }

        private void ProcessTypeCountry()
        {
            ProcessTypeAny();
        }

        private void ProcessTypeICAO()
        {
            if (Airport == null)
            {
                string[] ICAOs = ((string)SituationStructure["ICAO"]).Split(" ,;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (ICAOs.Length > 1)
                {
                    PossibleAirports = new List<Airport>();
                    foreach (var ICAO in ICAOs)
                    {
                        PossibleAirports.Add(SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO));
                    }
                    FinalizeAirportChoice();
                }
                else
                {
                    var apt = SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAOs[0]);
                    if (apt != null)
                    {
                        Airport = apt;
                        Location = apt.Location;
                        Generator.GenSeedIndex = Index;
                        FinalizeLocation();
                    }
                    else
                    {
                        Route.IsImpossible = true;
                        return;
                    }
                }
            }
        }

        private void ProcessTypeGeo()
        {
            if (Location == null)
            {
                Location = new GeoLoc(Convert.ToDouble(SituationStructure["Lon"]), Convert.ToDouble(SituationStructure["Lat"]));
                Generator.GenSeedIndex = Index;
                FinalizeLocation();
            }
        }

        private void ProcessTypeSituation()
        {
            if(Location == null)
            {
                var source_situation = Route.Situations.Find(x => x.UID == Convert.ToInt32(SituationTypeParams[0]));

                // Situations where we know we're not going to find anything, Go to next pass
                if (source_situation.Location == null) { ProcessOnLoopIndex++; return; }

                Airport = source_situation.Airport;
                Location = source_situation.Location.Copy();
                LocationName = source_situation.LocationName;
                FinalizeLocation();
            }
        }

        private void ProcessTypeLocation()
        {
            // Situations where we know we're not going to find anything, Go to next pass
            switch (SituationTypeParams[1])
            {
                case "nearest_prev":
                    {
                        if (Previous.Location == null) { ProcessOnLoopIndex++; return; }
                        break;
                    }
                case "nearest_next":
                    {
                        if (Next.Location == null) { ProcessOnLoopIndex++; return; }
                        break;
                    }
            }


            var filter = new Dictionary<RouteGenSituation, List<float>>();
            if (Previous != null) { filter.Add(Previous, new List<float>() { Previous.Filter.DistToNextMinKM * 1000, Previous.Filter.DistToNextMaxKM * 1000 }); }
            if (Next != null) { filter.Add(Next, new List<float>() { Filter.DistToNextMinKM * 1000, Filter.DistToNextMaxKM * 1000 }); }
            var locations = Filter.FilterLocations(Route.RouteSeedIndex, SituationTypeParams[0], filter);


            IOrderedEnumerable<BsonDocument> ordered_location = null;
            switch (SituationTypeParams[1])
            {
                case "highest":
                    {
                        ordered_location = locations.OrderByDescending(x => x["alt"]);
                        break;
                    }
                case "lowest":
                    {
                        ordered_location = locations.OrderBy(x => x["alt"]);
                        break;
                    }
                case "largest":
                    {
                        ordered_location = locations.OrderByDescending(x => x["area"]);
                        break;
                    }
                case "nearest_prev":
                    {
                        ordered_location = locations.OrderBy(x => Utils.MapCalcDistFloat((float)(x["lat"].AsDouble), (float)(x["lon"].AsDouble), (float)Previous.Location.Lat, (float)Previous.Location.Lon));
                        break;
                    }
                case "nearest_next":
                    {
                        ordered_location = locations.OrderBy(x => Utils.MapCalcDistFloat((float)(x["lat"].AsDouble), (float)(x["lon"].AsDouble), (float)Next.Location.Lat, (float)Next.Location.Lon));
                        break;
                    }
                default:
                    {
                        ordered_location = locations.OrderBy(x => Utils.GetRandom(100));
                        break;
                    }
            }

            var chosen_location = ordered_location.FirstOrDefault();

            if(chosen_location != null)
            {
                var l = BsonMapper.Global.Deserialize<Dictionary<string, dynamic>>(chosen_location);

                Location = new GeoLoc((double)l["lon"], (double)l["lat"]);
                LocationName = l["attrs"].ContainsKey("name:en") ? l["attrs"]["name:en"] : (l["attrs"].ContainsKey("name") ? l["attrs"]["name"] : null);
                FinalizeLocation();
            }
            else
            {
                Route.IsImpossible = true;
                return;
            }
        }
        
        public RouteGenSituation Copy()
        {
            return (RouteGenSituation)this.MemberwiseClone();
        }

        public string ToHash()
        {
            if (Airport != null)
            {
                return Airport.ICAO.ToString();
            }
            else if (Location != null)
            {
                return Location.ToString(3);
            }
            else
            {
                return "UA";
            }
        }

        public override string ToString()
        {
            if(Airport != null)
            {
                return Airport.ICAO.ToString();
            } 
            else if(Location != null)
            {
                return Location.ToString(3);
            }
            else
            {
                return "Unassigned";
            }
        }

    }
}
