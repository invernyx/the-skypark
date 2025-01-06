using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class flight_timer_start : action_base
    {
        public List<action_base> LandedInActions = new List<action_base>();
        public List<action_base> LandedOutActions = new List<action_base>();

        public List<action_base> ExceedMinActions = new List<action_base>();
        public List<action_base> ExceedMaxActions = new List<action_base>();

        public int LinkID = -1;
        public flight_timer_end Link = null;
        public bool Triggered = false;
        public bool HasExceeded = false;

        public float MinDuration = 0;
        public float MaxDuration = 0;
        public P42StopWatch SW = new P42StopWatch();

        private bool Within = false;

        public flight_timer_start(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "flight_timmer";
            //Completed = false;

            MinDuration = Convert.ToSingle(Params["MinDuration"]);
            MaxDuration = Convert.ToSingle(Params["MaxDuration"]);

            Limit = new Limit(ID, Adv)
            {
                Visible = true,
                Label = Params["Label"] != string.Empty ? Params["Label"] : "",
                Type = "Flight Timer"
            };

            if (Adv != null)
            {
                if (Link == null)
                {
                    foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                    {
                        if (action.Value.GetType() == typeof(flight_timer_end))
                        {
                            flight_timer_end TestAction = (flight_timer_end)action.Value;
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

            foreach (int ActionID in Params["LandedInActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    LandedInActions.Add(ActionObj);
                }
            }

            foreach (int ActionID in Params["LandedOutActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    LandedOutActions.Add(ActionObj);
                }
            }

            foreach (int ActionID in Params["ExceedMinActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    ExceedMinActions.Add(ActionObj);
                }
            }

            foreach (int ActionID in Params["ExceedMaxActions"])
            {
                action_base ActionObj = AdventuresBase.CreateAction(Adventure, Template, Situation, ActionID, this);
                if (ActionObj != null)
                {
                    ExceedMaxActions.Add(ActionObj);
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
            Triggered = true;
            Limit.Enabled = true;
            Adventure.Save();
        }
        
        public override void Process()
        {
            double Ellapsed = SW.ElapsedMilliseconds / 1000 / 60;
            if (Ellapsed > MinDuration && Ellapsed < MaxDuration)
            {
                if (!Within)
                {
                    Within = true;
                    foreach (action_base action in ExceedMinActions)
                    {
                        action.Enter();
                    }
                }
                foreach (action_base action in ExceedMinActions)
                {
                    action.Process();
                }
            }
            else
            {
                if (Within)
                {
                    Within = false;
                    foreach (action_base action in ExceedMinActions)
                    {
                        action.Exit();
                    }
                    foreach (action_base action in ExceedMaxActions)
                    {
                        action.Enter();
                    }
                }
                if (Ellapsed > MaxDuration)
                {
                    foreach (action_base action in ExceedMaxActions)
                    {
                        action.Process();
                        Clear();
                    }
                }
            }
        }

        public override void LiveProcess(TemporalData LastData, TemporalData NewData)
        {
            if(!NewData.SIM_ON_GROUND)
            {
                if (!SW.IsRunning)
                {
                    SW.Start();
                }
            }
            else
            {
                if (SW.IsRunning)
                {
                    SW.Stop();
                }
            }
        }


        public override void Clear()
        {
            lock (Adventure.ActionsWatch)
            {
                Adventure.ActionsWatch.Remove(this);
            }
            Triggered = false;
            //Completed = true;
            Limit.Enabled = false;
        }


        public override void ImportState(Dictionary<string, dynamic> State)
        {
            HasExceeded = State.ContainsKey("HasExceeded") ? State["HasExceeded"] : false;
            Triggered = State.ContainsKey("Triggered") ? State["Triggered"] : false;
            //Completed = State.ContainsKey("Completed") ? State["Completed"] : false;
            SW.Set(State.ContainsKey("Elapsed") ? (double)State["Elapsed"] : 0, true);

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
                { "Triggered", Triggered },
                { "Elapsed", SW.ElapsedMilliseconds },
            };

            //if (Completed == true)
            //{
            //    ns.Add("Completed", true);
            //}

            if (Adventure.ActionsWatch.Contains(this))
            {
                ns.Add("LiveActionWatch", true);
            }

            return ns;
        }
    }
}
