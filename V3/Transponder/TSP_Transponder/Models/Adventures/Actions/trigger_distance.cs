using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Attributes.ObjAttr;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class trigger_distance : action_base
    {
        [ObjValue("EnterActions")]
        public List<action_base> EnterActions = new List<action_base>();
        [ObjValue("ExitActions")]
        public List<action_base> ExitActions = new List<action_base>();

        public string Distance = ""; 
        public bool EnterTriggered = false;
        public bool ExitTriggered = false;
        public float TargetDistance = -1;

        public trigger_distance(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "trigger_distance";
            Distance = (string)Parameters["Distance"];
            
        }

        public override void Starting()
        {
            switch (Distance[0])
            {
                case 'n':
                    {
                        switch (Distance[1])
                        {
                            case '*':
                                {
                                    float num = Convert.ToSingle(Distance.Substring(2, Distance.Length - 2));
                                    if (Situation.Index < Adventure.Situations.Count - 1)
                                    {
                                        TargetDistance = Situation.DistToNext * num;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 'p':
                    {
                        switch (Distance[1])
                        {
                            case '*':
                                {
                                    float num = Convert.ToSingle(Distance.Substring(2, Distance.Length - 2));
                                    if (Situation.Index > 0)
                                    {
                                        Situation PreviousSit = Adventure.Situations[Situation.Index - 1];
                                        TargetDistance = PreviousSit.DistToNext * num;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        TargetDistance = Convert.ToSingle(Distance);
                        break;
                    }
            }
        }

        public override void Process()
        {
            //if(TargetDistance == -1)
            //{
            //    Starting();
            //}

            if(!EnterTriggered || !ExitTriggered)
            {
                double DistToSit = Utils.MapCalcDist(AdventuresBase.TemporalNewBuffer.PLANE_LOCATION, Situation.Location, Utils.DistanceUnit.NauticalMiles, true);

                if (!EnterTriggered)
                {
                    if (DistToSit < TargetDistance)
                    {
                        EnterTriggered = true;
                        foreach (action_base action in EnterActions)
                        {
                            action.Enter();
                        }
                        Adventure.Save();
                    }
                }

                if (!ExitTriggered)
                {
                    if (DistToSit > TargetDistance)
                    {
                        ExitTriggered = true;
                        foreach (action_base action in ExitActions)
                        {
                            action.Enter();
                        }
                        Adventure.Save();
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
        }


        public override void ImportState(Dictionary<string, dynamic> State)
        {
            EnterTriggered = State.ContainsKey("EnterTriggered") ? State["EnterTriggered"] : false;
            ExitTriggered = State.ContainsKey("ExitTriggered") ? State["ExitTriggered"] : false;
            TargetDistance = State.ContainsKey("TargetDistance") ? Convert.ToSingle(State["TargetDistance"]) : -1;
            
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
                { "EnterTriggered", EnterTriggered },
                { "ExitTriggered", ExitTriggered },
                { "TargetDistance", TargetDistance },
            };
            
            if (Adventure.ActionsWatch.Contains(this))
            {
                ns.Add("LiveActionWatch", true);
            }

            return ns;
        }
    }
}
