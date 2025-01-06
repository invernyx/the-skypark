using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_engine_stop : action_base
    {
        public List<action_base> EnterActions = new List<action_base>();

        public bool Triggered = false;
        public bool LastState = false;
        
        public trigger_engine_stop(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            Completed = false;
            ActionName = "trigger_engine_stop";

            foreach (int ActionID in Params["EnterActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    EnterActions.Add(ActionObj);
                }
            }
        }

        public override void Process()
        {
            if (LastState != AdventuresBase.TemporalNewBuffer.GENERAL_ENG_COMBUSTION)
            {
                if (!AdventuresBase.TemporalNewBuffer.GENERAL_ENG_COMBUSTION)
                {
                    if (!(bool)Completed || !Triggered)
                    {
                        Completed = true;
                        Triggered = true;
                        foreach (action_base action in EnterActions)
                        {
                            action.Enter();
                        }
                        Adventure.Save();
                    }
                }
                else
                {
                    foreach (action_base action in EnterActions) // Process actions
                    {
                        action.Exit();
                    }
                }
            }

            if (!AdventuresBase.TemporalNewBuffer.GENERAL_ENG_COMBUSTION)
            {
                foreach (action_base action in EnterActions) // Process actions
                {
                    action.Process();
                }
            }
            
            LastState = AdventuresBase.TemporalNewBuffer.GENERAL_ENG_COMBUSTION;
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            Completed = State.ContainsKey("Completed") ? State["Completed"] : false;
            
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Completed", Completed },
            };

            return ns;
        }
    }
}
