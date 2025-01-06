using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_alt_start : action_base
    {
        [ObjValue("EnterActions")]
        public List<action_base> WithinActions = new List<action_base>();
        [ObjValue("ExitActions")]
        public List<action_base> OutsideActions = new List<action_base>();


        public int LinkID = -1;
        public trigger_alt_end Link = null;
        public bool HasExceeded = false;
        public bool IsWithinCancelRange = false;

        private float Min = 0;
        private float Max = 0;
        private ERelation Relation = ERelation.AGL;

        private bool Exceeds = false;
        private bool? Exceeded = null;

        public trigger_alt_start(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_alt_start";
                
            Min = Convert.ToSingle(Params["Min"]);
            Max = Convert.ToSingle(Params["Max"]);

            if (Params.ContainsKey("ShowLimit"))
            {
                if(Params["ShowLimit"] == true)
                {
                    Limit = new Limit(ID, Adv)
                    {
                        Visible = true,
                        Label = (string)Params["Label"],
                        Params = new dynamic[] { Min, Max, Convert.ToString(Params["Relation"]) },
                        Type = "alt_trigger"
                    };
                }

            };


            switch (Convert.ToString(Params["Relation"]))
            {
                case "AGL": Relation = ERelation.AGL; break;
                case "ASL": Relation = ERelation.ASL; break;
            }

            if(Adv != null)
            {
                if (Link == null)
                {
                    foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                    {
                        if (action.Value.GetType() == typeof(trigger_alt_end))
                        {
                            trigger_alt_end TestAction = (trigger_alt_end)action.Value;
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
            base.Enter();

            if (IsWithinCancelRange)
                return;

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
            base.Exit();

            Exceeded = null;
        }
        
        public override void Process()
        {
            if (!Entered || IsWithinCancelRange || !SimConnection.IsLoaded || (AdventuresBase.TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND > 100 && AdventuresBase.TemporalNewBuffer.SIM_ON_GROUND))
                return;

            if (Limit != null)
                Limit.Enabled = true;

            double alt = AdventuresBase.TemporalNewBuffer.PLANE_ALTITUDE;
            if(Relation == ERelation.AGL)
            {
                alt = AdventuresBase.TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND;
                if(Min == 0 && alt < 0)
                {
                    alt = 0;
                }
            }

            bool newExceeds = false;
            if (alt > Max)
            {
                newExceeds = true;
            }

            if (alt < Min)
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

            if(Link != null)
            {
                if (Link.CancelRange > 0) // Within Cancel range... End tracking
                {
                    double DistToEnd = Utils.MapCalcDist(Situation.Location, AdventuresBase.TemporalNewBuffer.PLANE_LOCATION, Utils.DistanceUnit.NauticalMiles);
                    if (DistToEnd < Link.CancelRange)
                    {
                        Link.Enter();
                        Clear();
                        Adventure.Save();
                    }
                }
            }
        }

        public override void Clear()
        {
            IsWithinCancelRange = true;
            lock (Adventure.ActionsWatch)
                Adventure.ActionsWatch.Remove(this);

            if(Limit != null)
                Limit.Enabled = false;
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            HasExceeded = State.ContainsKey("HasExceeded") ? State["HasExceeded"] : false;
            IsWithinCancelRange = State.ContainsKey("IsWithinCancelRange") ? State["IsWithinCancelRange"] : false;

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
                { "IsDone", IsWithinCancelRange }
            };

            if (Adventure.ActionsWatch.Contains(this))
            {
                ns.Add("LiveActionWatch", true);
            }

            return ns;
        }

        private enum ERelation
        {
            AGL,
            ASL
        }
    }
}
