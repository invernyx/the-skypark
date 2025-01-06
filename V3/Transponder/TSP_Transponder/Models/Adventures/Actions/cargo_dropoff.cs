using System;
using System.Collections.Generic;
using static TSP_Transponder.Attributes.ObjAttr;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class cargo_dropoff : action_base
    {
        /*
        {
 			"Link": 61653354,
 			"Transport": "TSP_van_white",
 			"UnloadedActions": [],
 			"ForgotActions": []
 		}
        */

        public bool Allow = false;
        public int LinkID = -1;
        public cargo_pickup Link = null;
        public string Transport = "";

        [ObjValue("UnloadedActions")]
        public List<action_base> UnloadedActions = new List<action_base>();
        [ObjValue("ForgotActions")]
        public List<action_base> ForgotActions = new List<action_base>();
        [ObjValue("EndActions")]
        public List<action_base> EndActions = new List<action_base>();

        public cargo_dropoff(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "cargo_dropoff";
            Completed = false;

            LinkID = Params["Link"];
            Transport = Params["Transport"];
            
            if(Adventure != null)
            {
                foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                {
                    if (action.Value.GetType() == typeof(cargo_pickup))
                    {
                        cargo_pickup TestAction = (cargo_pickup)action.Value;
                        if (TestAction.UID == LinkID)
                        {
                            Link = TestAction;
                            Link.Link = this;
                            break;
                        }
                    }
                }
            }

            #region Interactions
            Interactions.Add(new Interaction()
            {
                Verb = "deliver",
                UID = UID,
                Label = "Deliver",
                Style = "deliver",
                Description = "",
                Triggered = (Inter, data) =>
                {
                    Link.Unload();
                    Adventure.ScheduleInteractionBroadcast = true;
                }
            });
            Interactions.Add(new Interaction()
            {
                Verb = "delivering",
                UID = UID,
                Label = "Delivering...",
                Style = "delivering",
                Description = "",
                Enabled = false,
            });

            Interactions.Add(new Interaction()
            {
                Verb = "delivered",
                Enabled = false,
                Essential = false,
                UID = UID,
                Label = "Delivered",
                Style = "delivered",
                Description = ""
            });
            #endregion
        }

        public void Unload()
        {
            if(Link.Item != null)
            {
                lock (Adventure.ActionsWatch)
                {
                    Adventure.ActionsWatch.Remove(Link);
                }

                Link.ItemDelivered = true;
                Completed = true;

                foreach (action_base action in UnloadedActions)
                {
                    action.Enter();
                }

                Console.WriteLine("Delivered Cargo " + Link.Cargo.Name + " for " + Adventure.Route + " (" + Adventure.ID + ")");
                Adventure.Save();
            }
        }

        public override void Enter()
        {
            Allow = true;
            Adventure.ScheduleInteractionBroadcast = true;
        }

        public override void Exit()
        {
            Allow = false;
            Adventure.ScheduleInteractionBroadcast = true;
        }

        public override void Process()
        {
            if (Math.Abs(LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED) < 2)
            {
                if (!Allow)
                {
                    Allow = true;
                    Adventure.ScheduleInteractionBroadcast = true;
                }
            }
            else
            {
                if (Allow)
                {
                    Allow = false;
                    Adventure.ScheduleInteractionBroadcast = true;
                }
            }
        }

        public override void UpdateInteractions(bool Broadcast)
        {
            Interaction Inter_Deliver = Interactions.Find(x => x.Verb == "deliver");
            Interaction Inter_Delivering = Interactions.Find(x => x.Verb == "delivering");
            Interaction Inter_Delivered = Interactions.Find(x => x.Verb == "delivered");

            Inter_Deliver.Visible = false;
            Inter_Delivering.Visible = false;
            Inter_Delivered.Visible = false;

            if (Adventure.IsMonitored)
            {
                if (Situation.InRange && !Link.ItemDelivered && Link.ItemLoaded && Adventure.SituationAt == Situation.Index && Allow)
                {
                    if(Link.ItemUnloadingEnd != null)
                    {
                        Inter_Delivering.Visible = true;
                        Inter_Delivering.Expire = Link.ItemUnloadingEnd;
                    }
                    else
                    {
                        Inter_Deliver.Visible = true;
                    }
                    return;
                }

                if (Link.ItemDelivered && Adventure.State == Adventure.AState.Active)
                {
                    Inter_Delivered.Visible = true;
                }
            }
        }

        public override void ImportState(Dictionary<string, dynamic> State)
        {
            if (State.ContainsKey("Completed"))
            {
                Completed = State["Completed"];
            }
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>();

            if (Completed == true)
            {
                ns.Add("Completed", true);
            }

            return ns;
        }

        public override Dictionary<string, dynamic> ToListedActions()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "id", UID },
                { "description", Link.Cargo.Name },
                { "action", "Dropoff" },
                { "action_type", ActionName },
                { "completed", Completed },
            };

            return rt;
        }
    }
}
