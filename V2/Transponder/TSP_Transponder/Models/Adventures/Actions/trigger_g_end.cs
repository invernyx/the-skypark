using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_g_end : action_base
    {
        public List<action_base> SuccessActions = new List<action_base>();
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

            foreach (int ActionID in Params["SuccessActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    SuccessActions.Add(ActionObj);
                }
            }

            foreach (int ActionID in Params["FailActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    FailActions.Add(ActionObj);
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
