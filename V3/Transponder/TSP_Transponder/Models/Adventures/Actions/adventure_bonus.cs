using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class adventure_bonus : action_base
    {
        public bool Triggered = false;
        public string Label = "";
        public string Company = "";

        public adventure_bonus(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "adventure_bonus";

            Label = Params.ContainsKey("Label") ? ((string)Params["Label"]).TrimEnd('.') : "";
            Company = Params.ContainsKey("Company") ? ((string)Params["Company"]).TrimEnd('.') : "";
        }

        public override void Enter()
        {
            Triggered = true;

            Adventure.ScheduleInvoiceBroadcast = true;
            
            Adventure.Save();
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            Triggered = State.ContainsKey("Triggered") ? State["Triggered"] : false;
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Triggered", Triggered },
            };
            return ns;
        }

    }
}
