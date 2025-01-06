using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class adventure_milestone : action_base
    {

        [ObjValue("SuccessActions")]
        public List<action_base> SuccessActions = new List<action_base>();

        public bool InSit = false;
        public string Label = "";

        public adventure_milestone(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "adventure_milestone";
            Completed = false;

            Label = Params.ContainsKey("Label") ? ((string)Params["Label"]).TrimEnd('.') : "";
            
            Interactions.Add(new Interaction()
            {
                Verb = "notice",
                UID = UID,
                Label = "",
                Style = "notice",
                Essential = true,
                Description = "",
            });
        }

        public override void Enter()
        {
            Completed = true;
            foreach (action_base action in SuccessActions)
            {
                action.Enter();
            }
            Adventure.Save();
        }

        public override void EnterSit()
        {
            if (Label != "")
            {
                Adventure.ScheduleInteractionBroadcast = true;
            }
        }

        public override void ProcessSit()
        {
        }

        public override void ExitSit()
        {
            if (Label != "")
            {
                Adventure.ScheduleInteractionBroadcast = true;
            }
        }

        public override void UpdateInteractions(bool Broadcast)
        {
            Interaction Inter_Fly = Interactions.Find(x => x.Verb == "notice");
            Inter_Fly.Visible = false;

            if (Adventure.IsMonitored)
            {
                if (Situation.InRange)
                {
                    if (Label != "")
                    {
                        Inter_Fly.Visible = true;
                    }
                }
            }

        }

        public override Dictionary<string, dynamic> ToListedActions()
        {
            if (Label != "")
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
            else
            {
                return null;
            }

        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            if(State.ContainsKey("Completed"))
            {
                Completed = (bool)State["Completed"];
            }
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>();

            if(Completed == true)
            {
                ns.Add("Completed", true);
            }

            return ns;
        }
    }
}
