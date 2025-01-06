using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_g_end : action_base
    {
        [ObjValue("SuccessActions")]
        public List<action_base> SuccessActions = new List<action_base>();
        [ObjValue("FailActions")]
        public List<action_base> FailActions = new List<action_base>();

        public int LinkID = -1;
        public trigger_g_start Link = null;

        public trigger_g_end(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_g_end";

            LinkID = Params["Link"];

            if (Adv != null)
            {
                foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                {
                    if (action.Value.GetType() == typeof(trigger_g_start))
                    {
                        trigger_g_start TestAction = (trigger_g_start)action.Value;
                        if (TestAction.UID == LinkID)
                        {
                            Link = TestAction;
                            Link.Link = this;
                            break;
                        }
                    }
                }
            }
            
        }
        
        public override void Enter()
        {
            Link.Clear();

            if (!Link.HasExceeded)
            {
                foreach (action_base action in SuccessActions)
                {
                    action.Enter();
                }
            }
            else
            {
                foreach (action_base action in FailActions)
                {
                    action.Enter();
                }
            }

            Adventure.Save();
        }

    }
}
