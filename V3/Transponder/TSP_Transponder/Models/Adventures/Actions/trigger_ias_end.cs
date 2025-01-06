﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_ias_end : action_base
    {
        [ObjValue("SuccessActions")]
        public List<action_base> SuccessActions = new List<action_base>();
        [ObjValue("FailActions")]
        public List<action_base> FailActions = new List<action_base>();

        public int LinkID = -1;
        public trigger_ias_start Link = null;
        public float CancelRange = 0;

        public trigger_ias_end(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_ias_end";

            LinkID = Params["Link"];
            CancelRange = Convert.ToSingle(Params["CancelRange"]);

            if (Adv != null)
            {
                foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                {
                    if (action.Value.GetType() == typeof(trigger_ias_start))
                    {
                        trigger_ias_start TestAction = (trigger_ias_start)action.Value;
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