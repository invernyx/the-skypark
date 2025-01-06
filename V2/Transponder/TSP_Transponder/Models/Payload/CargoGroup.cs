using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Payload
{
    public class CargoGroup
    {
        public CargoManifest Manifest = null;
        public Guid? GUID = null;
        public AircraftInstance LoadedOn = null;
        private bool _Loadable = false;
        public bool Loadable {
            get
            {
                if(Manifest.Adventure.IsMonitored && Manifest.Adventure.State == Adventures.Adventure.AState.Active)
                {
                    return _Loadable;
                }

                return false;
            }
            set
            {
                _Loadable = value;
            }
        }
        private bool _Unloadable = false;
        public bool Unloadable
        {
            get
            {
                if (Manifest.Adventure.IsMonitored && Manifest.Adventure.State == Adventures.Adventure.AState.Active)
                {
                    return _Unloadable;
                }

                return false;
            }
            set
            {
                _Unloadable = value;
            }
        }
        public GeoPosition Location = null;
        public Airport NearestAirport = null;
        public int? DestinationIndex = null;
        public List<CargoUnit> Units = new List<CargoUnit>();

        public CargoGroup(CargoManifest manifest, Dictionary<string, dynamic> config)
        {
            Manifest = manifest;
            GUID = new Guid(config["GUID"]);
        }

        public CargoGroup(CargoManifest manifest)
        {
            Manifest = manifest;
            GUID = Guid.NewGuid();
        }

        public void SetLocation(GeoPosition loc)
        {
            Location = loc;
            NearestAirport = Location != null ? SimLibrary.SimList[0].AirportsLib.GetAirportByRange(new GeoLoc(Location), 3).FirstOrDefault().Value : null;
        }

        public bool CanLoad()
        {
            if (LoadedOn == null && LastTemporalData.PLANE_ALT_ABOVE_GROUND < 200 && LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED < 2)
            {
                double triggerRangeKilometers = 5;
                if (NearestAirport != null)
                {
                    triggerRangeKilometers = NearestAirport.Radius * 1852 / 1000;
                }
                double distance = Utils.MapCalcDist(new GeoLoc(Location.Lon, Location.Lat), LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.Kilometers);
                return distance < triggerRangeKilometers;
            }

            return false;
        }

        public bool CanUnload()
        {
            if(LoadedOn != null)
            {
                if(LoadedOn.ID == SimConnection.Aircraft.ID)
                {
                    foreach(var destination in Manifest.Destinations)
                    {
                        double triggerRangeKilometers = 5;
                        if (destination.Airport != null)
                        {
                            triggerRangeKilometers = destination.Airport.Radius * 1852 / 1000;
                        }
                        double distance = Utils.MapCalcDist(destination.Location, LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.Kilometers);
                        if(distance < triggerRangeKilometers)
                        {
                            return true;
                        }
                    }

                }

            }

            return false;
        }

        public void Load()
        {
            LoadedOn = SimConnection.Aircraft;
            Location = null;
            NearestAirport = null;
        }

        public void Unload()
        {

        }
        
        public void ImportState(Dictionary<string, dynamic> state)
        {
            GUID = new Guid(state["GUID"]);
            LoadedOn = FleetService.GetAircraft((string)state["LoadedOn"]);
            SetLocation(state["Location"] != null ? new GeoPosition((double)state["Location"][0], (double)state["Location"][1], (double)state["Location"][2], (double)state["Location"][3]) : null);
            DestinationIndex = state["DestinationIndex"];

            foreach (var unit in state["Units"])
            {
                CargoUnit ncu = new CargoUnit(Manifest, unit);
                ncu.ImportState(unit);
                Units.Add(ncu);
            }
        }

        public Dictionary<string, dynamic> ExportListing()
        {
            return new Dictionary<string, dynamic>()
            {
                { "GUID", GUID.ToString() },
                { "LoadedOn", LoadedOn },
                { "Loadable", Loadable },
                { "Unloadable", Unloadable },
                { "Location", Location != null ? new double[] { Location.Lon, Location.Lat, Location.Alt, Location.Hdg } : null },
                { "Health", Units.Count > 0 ? Units.Average(x => x.Health) : 0 },
                { "NearestAirport", NearestAirport != null ? NearestAirport.ToSummary(true) : null },
                { "Units", Units.Select(x => x.ExportState()).ToList() }
            };
        }

        public Dictionary<string, dynamic> ExportState()
        {
            return new Dictionary<string, dynamic>()
            {
                { "GUID", GUID.ToString() },
                { "LoadedOn", LoadedOn != null ? LoadedOn.Name : null },
                { "Location", Location != null ? new double[] { Location.Lon, Location.Lat, Location.Alt, Location.Hdg } : null },
                { "Health", Units.Count > 0 ? Units.Average(x => x.Health) : 0 },
                { "NearestAirport", NearestAirport != null ? NearestAirport.ToSummary(true) : null },
                { "DestinationIndex", DestinationIndex },
                { "Units", Units.Select(x => x.ExportState()).ToList() }
            };
        }
    }
}
