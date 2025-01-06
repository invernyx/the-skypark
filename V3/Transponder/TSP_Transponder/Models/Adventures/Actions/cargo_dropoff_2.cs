using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Payload;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class cargo_dropoff_2 : action_base
    {
        [ObjValue("UnloadedActions")]
        public List<action_base> UnloadedActions = new List<action_base>();
        [ObjValue("ForgotActions")]
        public List<action_base> ForgotActions = new List<action_base>();
        [ObjValue("EndActions")]
        public List<action_base> EndActions = new List<action_base>();

        public cargo_dropoff_2(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "cargo_dropoff_2";
            Completed = false;
        }
        
        public void SetComplete(bool state)
        {
            Completed = state;
            Deliver();
        }

        public void Deliver()
        {
            foreach (action_base action in UnloadedActions)
            {
                action.Enter();
            }
        }

        public override void ImportState(Dictionary<string, dynamic> state)
        {
            if (Adventure != null)
            {
                if (state.ContainsKey("Completed"))
                {
                    Completed = true;
                }
            }
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>();

            if (Completed == true)
            {
                ns.Add("Completed", true);
            }

            return ns;
        }

        public override Dictionary<string, dynamic> ToListedActions()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "id", UID },
                { "description", "Unload" },
                { "action", "Dropoff" },
                { "action_type", ActionName },
                { "completed", Completed },
            };

            return rt;
        }
    }
}
