using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_gs_start : action_base
    {
        public List<action_base> WithinActions = new List<action_base>();
        public List<action_base> OutsideActions = new List<action_base>();

        public int LinkID = -1;
        public trigger_gs_end Link = null;
        public bool HasExceeded = false;
        public bool IsDone = false;

        public float Min = 0;
        public float Max = 0;

        private bool Exceeds = false;
        private bool? Exceeded = null;

        public trigger_gs_start(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_gs_start";

            Min = Convert.ToSingle(Params["Min"]);
            Max = Convert.ToSingle(Params["Max"]);

            Limit = new Limit(ID, Adv)
            {
                Visible = true,
                Label = Params["Label"] != string.Empty ? Params["Label"] : "Stay between " + Min + " and " + Max + "kts",
                Type = "Speed Limits"
            };

            if (Adv != null)
            {
                if (Link == null)
                {
                    foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                    {
                        if (action.Value.GetType() == typeof(trigger_gs_end))
                        {
                            trigger_gs_end TestAction = (trigger_gs_end)action.Value;
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

            foreach (int ActionID in Params["EnterActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    WithinActions.Add(ActionObj);
                }
            }

            foreach (int ActionID in Params["ExitActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    OutsideActions.Add(ActionObj);
                }
            }
        }

        public override void Enter()
        {
            if (IsDone)
            {
                return;
            }

            lock (Adventure.ActionsWatch)
            {
                if (!Adventure.ActionsWatch.Contains(this))
                {
                    Adventure.ActionsWatch.Add(this);
                }
            }
        }

        public override void Exit()
        {
            Exceeded = null;
        }

        public override void Process()
        {
            if (IsDone)
            {
                return;
            }

            Limit.Enabled = true;

            bool newExceeds = false;
            if (Math.Abs(AdventuresBase.TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED) > Max)
            {
                newExceeds = true;
            }

            if (Math.Abs(AdventuresBase.TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED) < Min)
            {
                newExceeds = true;
            }
            Exceeds = newExceeds;

            if (Exceeds) // Currently Exceeding
            {
                if (Exceeded != Exceeds) // Entering Excess
                {
                    if (Exceeded == false)
                    {
                        foreach (action_base action in WithinActions)
                        {
                            action.Exit();
                        }
                    }
                    Exceeded = true;
                    HasExceeded = true;
                    foreach (action_base action in OutsideActions)
                    {
                        action.Enter();
                    }

                    Adventure.Save();
                }

                foreach (action_base action in OutsideActions) // Process Exceed actions
                {
                    action.Process();
                }
            }
            else // Not Exceeding
            {
                if (Exceeded != Exceeds) // Exiting Excess
                {
                    if (Exceeded == true)
                    {
                        foreach (action_base action in OutsideActions)
                        {
                            action.Exit();
                        }
                    }
                    Exceeded = false;
                    foreach (action_base action in WithinActions)
                    {
                        action.Enter();
                    }

                    Adventure.Save();
                }

                foreach (action_base action in WithinActions) // Process actions
                {
                    action.Process();
                }
            }

            if (Link != null)
            {
                if (Link.CancelRange > 0) // Within Cancel range... End tracking
                {
                    double DistToEnd = Utils.MapCalcDist(Situation.Location, AdventuresBase.TemporalNewBuffer.PLANE_LOCATION, Utils.DistanceUnit.NauticalMiles);
                    if (DistToEnd < Link.CancelRange)
                    {
                        Link.Enter();
                        IsDone = true;
                    }
                }
            }
        }

        public override void Clear()
        {
            lock (Adventure.ActionsWatch)
            {
                Adventure.ActionsWatch.Remove(this);
            }
            IsDone = true;
            Limit.Enabled = false;
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            HasExceeded = State.ContainsKey("HasExceeded") ? State["HasExceeded"] : false;
            IsDone = State.ContainsKey("IsDone") ? State["IsDone"] : false;

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
                { "IsDone", IsDone }
            };

            if (Adventure.ActionsWatch.Contains(this))
            {
                ns.Add("LiveActionWatch", true);
            }

            return ns;
        }
    }
}
