using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Attributes.ObjAttr;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_g_start : action_base
    {
        [ObjValue("EnterActions")]
        public List<action_base> EnterActions = new List<action_base>();
        [ObjValue("ExitActions")]
        public List<action_base> ExitActions = new List<action_base>();

        public int LinkID = -1;
        public trigger_g_end Link = null;
        public bool HasExceeded = false;

        public float Min = 0;
        public float Max = 0;

        private bool Exceeds = false;
        private bool Exceeded = false;

        public trigger_g_start(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_g_start";

            Min = Convert.ToSingle(Params["Min"]);
            Max = Convert.ToSingle(Params["Max"]);

            Limit = new Limit(ID, Adv)
            {
                Visible = true,
                Label = (string)Params["Label"],
                Params = new dynamic[] { Min, Max },
                Type = "g_trigger",
            };

            if (Adv != null)
            {
                if (Link == null)
                {
                    foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                    {
                        if (action.Value.GetType() == typeof(trigger_g_end))
                        {
                            trigger_g_end TestAction = (trigger_g_end)action.Value;
                            if (TestAction.LinkID == UID)
                            {
                                Link = TestAction;
                                Link.Link = this;
                                break;
                            }
                        }
                    }
                }
            }
            
        }

        public override void Enter()
        {
            lock (Adventure.ActionsWatch)
            {
                if (!Adventure.ActionsWatch.Contains(this))
                {
                    Adventure.ActionsWatch.Add(this);
                }
            }
        }

        public override void Process()
        {
            Limit.Enabled = true;

            if (Exceeds) // Currently Exceeding
            {
                if (Exceeded != Exceeds) // Entering Excess
                {
                    Exceeded = true;
                    HasExceeded = true;
                    foreach (action_base action in ExitActions)
                    {
                        action.Enter();
                    }

                    Adventure.Save();
                }

                foreach (action_base action in ExitActions) // Process Exceed actions
                {
                    action.Process();
                }
            }
            else // Not Exceeding
            {
                if (Exceeded != Exceeds) // Exiting Excess
                {
                    Exceeded = false;
                    foreach (action_base action in EnterActions)
                    {
                        action.Enter();
                    }

                    Adventure.Save();
                }

                foreach (action_base action in EnterActions) // Process actions
                {
                    action.Process();
                }
            }
        }

        public override void ProcessLive(TemporalData LastData, TemporalData NewData)
        {
            bool newExceeds = false;
            if (NewData.G_FORCE > Max)
            {
                newExceeds = true;
            }
            
            if (NewData.G_FORCE < Min)
            {
                newExceeds = true;
            }

            if(Exceeds != newExceeds)
            {
                Exceeds = newExceeds;
                Process();
            }
        }

        public override void Clear()
        {
            lock (Adventure.ActionsWatch)
            {
                Adventure.ActionsWatch.Remove(this);
            }
            Limit.Enabled = false;
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            HasExceeded = State.ContainsKey("HasExceeded") ? State["HasExceeded"] : false;

            if (State.ContainsKey("LiveActionWatch"))
            {
                if (State["LiveActionWatch"])
                {
                    lock (Adventure.ActionsWatch)
                    {
                        if (!Adventure.ActionsWatch.Contains(this))
                        {
                            Adventure.ActionsWatch.Add(this);
                        }
                    }
                }
            }
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "HasExceeded", HasExceeded },
            };

            if (Adventure.ActionsWatch.Contains(this))
            {
                ns.Add("LiveActionWatch", true);
            }

            return ns;
        }
    }
}
