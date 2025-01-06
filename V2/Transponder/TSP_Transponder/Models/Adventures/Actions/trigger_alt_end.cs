using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_alt_end : action_base
    {
        public List<action_base> SuccessActions = new List<action_base>();
        public List<action_base> FailActions = new List<action_base>();

        public int LinkID = -1;
        public trigger_alt_start Link = null;
        public float CancelRange = 0;

        public trigger_alt_end(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_alt_end";

            if(Params["Link"] != null)
            {
                LinkID = Params["Link"];
            }

            CancelRange = Convert.ToSingle(Params["CancelRange"]);

            if (Adv != null)
            {
                foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                {
                    if (action.Value.GetType() == typeof(trigger_alt_start))
                    {
                        trigger_alt_start TestAction = (trigger_alt_start)action.Value;
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
                    action.Exit();
                }
            }
            else
            {
                foreach (action_base action in FailActions)
                {
                    action.Enter();
                    action.Exit();
                }
            }

            Adventure.Save();
        }

    }
}
