using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Attributes.ObjAttr;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_button : action_base
    {
        [ObjValue("EnterActions")]
        public List<action_base> EnterActions = new List<action_base>();
        public bool InRange = false;
        public string Label = "";
        
        public trigger_button(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            Completed = false;
            ActionName = "trigger_button";
            
            Label = Parameters["Label"];
            
            Interactions.Add(new Interaction()
            {
                Verb = "act",
                UID = UID,
                Label = Params["Button"],
                Style = "",
                Description = "",
                Essential = true,
                Triggered = (Inter, data) =>
                {
                    Completed = true;
                    foreach (action_base action in EnterActions)
                    {
                        action.Enter();
                    }
                    Adventure.ScheduleInteractionBroadcast = true;
                }
            });
        }

        public override void Enter()
        {
            InRange = true;
            if(Completed == true)
            {
                foreach (action_base action in EnterActions)
                {
                    action.Enter();
                }
                Adventure.ScheduleInteractionBroadcast = true;
            }
        }

        public override void Exit()
        {
            InRange = false;
        }

        public override void Process()
        {
        }

        public override void UpdateInteractions(bool Broadcast)
        {
            Interaction Inter_Act = Interactions.Find(x => x.Verb == "act");
            Inter_Act.Visible = Completed == false && InRange && Adventure.IsMonitored;
            
        }

        public override Dictionary<string, dynamic> ToListedActions()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "id", UID },
                { "description", Label },
                { "action", "" },
                { "action_type", ActionName },
                { "completed", Completed },
            };
            return rt;

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
