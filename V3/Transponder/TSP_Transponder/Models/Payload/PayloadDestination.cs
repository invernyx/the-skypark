using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Payload
{
    public class PayloadDestination
    {
        public Adventure Adventure = null;
        public action_base Action = null;
        public Situation Situation = null;
        public string LocationName = null;

        public PayloadDestination(Adventure Adventure, action_base Action, Dictionary<string, dynamic> config)
        {
            this.Adventure = Adventure;
            this.Action = Action;
            this.Situation = Action.Situation;
        }

        public PayloadDestination(Adventure Adventure, action_base Action)
        {
            this.Adventure = Adventure;
            this.Action = Action;
            this.Situation = Action.Situation;
        }

        public Dictionary<string, dynamic> ExportState()
        {
            return new Dictionary<string, dynamic>()
            {
                { "Location", new double[] { Situation.Location.Lon, Situation.Location.Lat } },
                { "Airport", Situation.Airport != null ? Situation.Airport.Serialize(null) : null },
                { "LocationName", LocationName }
            };
        }
        
        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<PayloadDestination> cs = new ClassSerializer<PayloadDestination>(this, fields);
            cs.Generate(typeof(PayloadDestination), fields);

            cs.Get("action_id", fields, (f) => Action.UID);
            cs.Get("location", fields, (f) => Situation.Location.ToList());
            cs.Get("airport", fields, (f) => Situation.Airport != null ? Situation.Airport.Serialize(f) : null);
            cs.Get("location_name", fields, (f) => Situation.LocationName != string.Empty ? Situation.LocationName : Situation.Template.SituationLabels[Situation.Index]);
            
            var result = cs.Get();
            return result;
        }

    }
}
