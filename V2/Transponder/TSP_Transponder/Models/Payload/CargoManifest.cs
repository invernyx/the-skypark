using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Payload.Assets;

namespace TSP_Transponder.Models.Payload
{
    public class CargoManifest
    {
        public Adventure Adventure = null;
        public long UID = 0;
        public List<CargoGroup> Groups = null;
        internal Cargo Definition = null;
        public List<CargoDestination> Destinations = new List<CargoDestination>();
        public int TotalQualtity = 0;
        public Dictionary<int, int> Dropoffs = new Dictionary<int, int>();

        public CargoManifest(Adventure Adventure, Dictionary<string, dynamic> config)
        {
            this.Adventure = Adventure;
            UID = Convert.ToInt64(config["UID"]);
            Groups = new List<CargoGroup>();
        }

        public void Startup(cargo_pickup_2 pickup)
        {
            Groups = new List<CargoGroup>();
            foreach (var doKey in Dropoffs.Keys)
            {
                cargo_dropoff_2 dropoff = (cargo_dropoff_2)Adventure.Actions.Find(x => x.UID == doKey);
                CargoDestination destination = Destinations.Find(x => x.Airport == dropoff.Situation.Airport);
                if (destination == null)
                {
                    destination = new CargoDestination()
                    {
                        Airport = dropoff.Situation.Airport,
                        Location = dropoff.Situation.Location
                    };
                    Destinations.Add(destination);
                }

                int destination_index = Destinations.IndexOf(destination);

                CargoGroup group = Groups.Find(x => x.DestinationIndex == destination_index);
                if(group == null)
                    group = new CargoGroup(this) { DestinationIndex = destination_index };

                int destCount = 0;
                while (destCount < Dropoffs[doKey])
                {
                    group.Units.Add(new CargoUnit(this));
                    destCount++;
                }

            }


            /*
            Groups = new List<CargoGroup>();
            CargoGroup cc = new CargoGroup(this);
            cc.SetLocation(new GeoPosition(pickup.Situation.Location));
            Groups.Add(cc);

            foreach (var doKey in Dropoffs.Keys)
            {
                cargo_dropoff_2 dropoff = (cargo_dropoff_2)Adventure.Actions.Find(x => x.UID == doKey);
                CargoDestination destination = Destinations.Find(x => x.Airport == dropoff.Situation.Airport);

                if(destination == null)
                {
                    destination = new CargoDestination()
                    {
                        Airport = dropoff.Situation.Airport,
                        Location = dropoff.Situation.Location
                    };
                    Destinations.Add(destination);
                }

                int destCount = 0;
                while (destCount < Dropoffs[doKey])
                {
                    cc.Units.Add(new CargoUnit(this));
                    destCount++;
                }
            }
            */
            }

        public void Cleanup()
        {
            Groups = null;
        }

        public void ImportState(Dictionary<string, dynamic> state)
        {
        }

        
        public List<AircraftInstance> GetAircraft()
        {
            if(Groups != null)
            {
                return Groups.Select(x => x.LoadedOn).ToList();
            }
            else
            {
                return new List<AircraftInstance>();
            }
        }

        public Dictionary<string, dynamic> ExportListing()
        {
            return new Dictionary<string, dynamic>()
            {
                { "UID", (int)UID },
                { "Total", TotalQualtity },
                { "Name", Definition.Name },
                { "WeightKG", Definition.WeightKG },
                { "Groups", Groups != null ? Groups.Select(x => x.ExportListing()) : null },
            };
        }

        public Dictionary<string, dynamic> ExportState()
        {
            return new Dictionary<string, dynamic>()
            {
                { "UID", (int)UID },
            };
        }
    }
}
