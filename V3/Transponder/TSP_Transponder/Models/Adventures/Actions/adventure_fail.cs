using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class adventure_fail : action_base
    {

        [ObjValue("Actions")]
        public List<action_base> Actions = new List<action_base>();

        private string Reason = "";

        public adventure_fail(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "adventure_fail";

            Reason = Parameters.ContainsKey("Reason") ? (string)Parameters["Reason"] : "";
        }

        public override void Enter()
        {
            Adventure.Fail(Reason);
        }

    }
}
