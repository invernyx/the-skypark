using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Utilities;

namespace TSP_Transponder.Models.Adventures
{
    public class Situation
    {
        public Adventure Adventure = null;
        public AdventureTemplate Template = null;
        [ClassSerializerField("id")]
        public int UID = 0;
        [ClassSerializerField("index")]
        private int _Index = -1;
        public int Index
        {
            get
            {
                if(_Index == -1)
                {
                    if (Adventure != null)
                    {
                        return Adventure.Situations.IndexOf(this);
                    }
                    else
                    {
                        return Template.Situations.IndexOf(this);
                    }
                }
                else
                {
                    return _Index;
                }
            }
            set
            {
                _Index = value;
            }
        }
        public RouteSituationType SituationType;
        public Airport Airport = null;
        public GeoLoc Location = null;
        public string LocationName = "";
        public string ICAO = "";
        public float DistToNext = 0;
        public float TriggerRange = 0;
        public float Height = 0;
        public bool Visible = true;
        public Dictionary<string, dynamic> InitialStructure = null;
        public List<action_base> Actions = new List<action_base>();
        public List<action_base> ChildActions = new List<action_base>();

        public List<action_base> CompleteActions = new List<action_base>();

        private bool _InRange = false;
        public bool InRange
        {
            get
            {
                return _InRange;
            }
            set
            {
                _InRange = value;
                Adventure.SchedulePathBroadcast = true;
            }
        }

        private bool _Visited = false;
        public bool Visited {
            get
            {
                return _Visited;
            }
            set
            {
                _Visited = value;
                Adventure.SchedulePathBroadcast = true;
            }
        }

        public bool _Done = false;
        public bool Done {
            get
            {
                if (Visited)
                {
                    if (!_Done)
                    {
                        bool new_done = ChildActions.Find(x => x.Completed == false) == null;
                        if(new_done)
                        {
                            foreach (var action in CompleteActions)
                                action.Enter();
                        }
                        return new_done;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
            
        private bool RefreshInteractions = false;
        
        public Situation(int Index, AdventureTemplate Template, Adventure Adventure = null, Situation Situation = null)
        {
            this.Index = Index;
            this.Template = Template;
            this.Adventure = Adventure;

            if (Situation != null)
            {
                UID = Situation.UID;
                Height = Situation.Height;
                Visible = Situation.Visible;
                SituationType = Situation.SituationType;
                TriggerRange = Situation.TriggerRange;
                ICAO = Situation.ICAO;
                if (Situation.Location != null)
                {
                    Location = Situation.Location.Copy();
                }

                foreach (action_base Action in Situation.CompleteActions)
                {
                    CompleteActions.Add(AdventuresBase.CreateAction(Adventure, Template, this, Action.UID, null));
                }
            }


        }
        
        public void Process()
        {
            bool NewInRange = Utils.MapCalcDist(AdventuresBase.TemporalNewBuffer.PLANE_LOCATION, Location, Utils.DistanceUnit.NauticalMiles) < TriggerRange + 0.25 && (Height != 0 ? AdventuresBase.TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND < Height : AdventuresBase.TemporalNewBuffer.SIM_ON_GROUND && AdventuresBase.TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED < 50);
            switch (Adventure.State)
            {
                case Adventure.AState.Active:
                    {
                        #region Monitored
                        if (Adventure.IsMonitored)
                        {
                            if (Index - Adventure.SituationAt > 0)
                            {
                                if (InRange)
                                {
                                    InRange = false;
                                    foreach (action_base action in Actions)
                                    {
                                        action.Exit();
                                        RefreshInteractions = true;
                                    }

                                    if (RefreshInteractions)
                                    {
                                        RefreshInteractions = false;
                                        Adventure.ScheduleInteractionBroadcast = true;
                                    }
                                }
                                return;
                            }

                            if (Done)
                                foreach (var action in CompleteActions)
                                    action.Process();
                        }
                        #endregion
                        
                        if (Adventure.IsMonitored)
                        {
                            #region Process Active Adventure
                            if (NewInRange)
                            {
                                if (!InRange || (InRange != Visited))
                                {
                                    InRange = true;
                                    if (!Visited)
                                    {
                                        Visited = true;
                                        Adventure.SchedulePathBroadcast = true;
                                        Adventure.Save();

                                        if (Airport != null)
                                        {
                                            bool Welcome = true;
                                            if (Airport.LastWelcome != null)
                                            {
                                                if (((DateTime)Airport.LastWelcome).AddMinutes(15) > DateTime.UtcNow)
                                                {
                                                    Welcome = false;
                                                }
                                            }

                                            if (Welcome)
                                            {
                                                Airport.LastWelcome = DateTime.UtcNow;
                                                Connectors.SimConnection.ConnectedInstance.SendMessage("Welcome to " + Airport.ICAO + ", " + Airport.Name + ".", 10);
                                            }

                                            List<string> ToDo = new List<string>();
                                            foreach (var Action in ChildActions)
                                            {
                                                var ActionStruct = Action.ToListedActions();
                                                if (ActionStruct != null)
                                                {
                                                    ToDo.Add(ActionStruct["action"] + " " + ActionStruct["description"]);
                                                }
                                            }

                                            if (Index == Adventure.Situations.Count - 1)
                                            {
                                                if (ToDo.Count > 0)
                                                {
                                                    Connectors.SimConnection.ConnectedInstance.SendMessage(string.Join(", ", ToDo) + " to complete.", 10);
                                                }
                                            }
                                            else
                                            {
                                                if (ToDo.Count > 0)
                                                {
                                                    Connectors.SimConnection.ConnectedInstance.SendMessage("Next up: " + string.Join(", ", ToDo) + ".", 10);
                                                }
                                                else
                                                {
                                                    Connectors.SimConnection.ConnectedInstance.SendMessage("We're done here, let's go to the next location!", 10);
                                                }
                                            }

                                            int Count = EventBus.EventManager.CountAirportVisit(Airport.ICAO);
                                            GoogleAnalyticscs.TrackEvent("Situation Location", "Country", Airport.Country);
                                            GoogleAnalyticscs.TrackEvent("Situation Location", "Airport", Airport.ICAO, Count);
                                            if (!Template.IsCustom)
                                            {
                                                GoogleAnalyticscs.TrackEvent("Situation Location", "Unbiased Country", Airport.Country);
                                                GoogleAnalyticscs.TrackEvent("Situation Location", "Unbiased Airport", Airport.ICAO, Count);
                                            }
                                        }

                                    }

                                    foreach (action_base action in ChildActions)
                                    {
                                        action.EnterSit();
                                    }

                                    foreach (action_base action in Actions)
                                    {
                                        action.Enter();
                                    }
                                    
                                    LocationHistory.UpdateLastLocation(Airport, Location);
                                    
                                    RefreshInteractions = true;
                                    Adventure.ScheduleStateBroadcast = true;
                                }

                                foreach (action_base action in ChildActions)
                                {
                                    action.ProcessSit();
                                }

                                foreach (action_base action in Actions)
                                {
                                    action.Process();
                                }
                            }
                            else if (InRange)
                            {
                                InRange = false;
                                if (Visited)
                                {
                                    foreach (action_base action in Actions)
                                    {
                                        action.Exit();
                                    }

                                    foreach (action_base action in ChildActions)
                                    {
                                        action.ExitSit();
                                    }

                                    RefreshInteractions = true;
                                }

                            }

                            if (RefreshInteractions)
                            {
                                RefreshInteractions = false;
                                Adventure.ScheduleInteractionBroadcast = true;
                            }
                            #endregion
                        }
                        else if (Adventure.LastLocationGeo == null && Index == 0)
                        {
                            if (NewInRange)
                            {
                                Adventure.LastLocationGeo = new GeoLoc(AdventuresBase.TemporalNewBuffer.PLANE_LOCATION.Lon, AdventuresBase.TemporalNewBuffer.PLANE_LOCATION.Lat);
                                Adventure.CheckMonitorState(true);
                                Adventure.ScheduleStateBroadcast = true;

                                if (SimConnection.Aircraft != null)
                                    SimConnection.Aircraft.Process();
                            }
                        }
                        else
                        {
                            if (NewInRange)
                            {
                                if (!InRange)
                                {
                                    InRange = true;

                                    foreach (action_base action in Actions)
                                    {
                                        action.EnterPaused();
                                    }
                                }

                                foreach (action_base action in Actions)
                                {
                                    action.ProcessPaused();
                                }
                            }
                            else if (InRange)
                            {
                                InRange = false;

                                foreach (action_base action in Actions)
                                {
                                    action.ExitPaused();
                                }
                            }
                        }
                        break;
                    }
                case Adventure.AState.Saved:
                    {
                        if (NewInRange)
                        {
                            if (!InRange)
                            {
                                InRange = true;
                            }
                        }
                        else if (InRange)
                        {
                            InRange = false;
                        }
                        break;
                    }
                default:
                    {
                        InRange = false;
                        break;
                    }
            }
        }

        public void ResetStates()
        {
            if(_InRange)
            {
                _InRange = false;
            }
        }


        public void ImportState(Dictionary<string, dynamic> State)
        {
            ICAO = State.ContainsKey("ICAO") ? State["ICAO"] : "";

            if(State.ContainsKey("Location"))
            {
                Location = new GeoLoc(Convert.ToDouble(State["Location"][0]), Convert.ToDouble(State["Location"][1]));
            }

            if (State.ContainsKey("LocationName"))
            {
                LocationName = (string)State["LocationName"];
            }
            
            if (State.ContainsKey("Visited"))
            {
                _Visited = State["Visited"];
            }
        }


        public Dictionary<string, dynamic> GetExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>();

            if (Visited)
            {
                ns.Add("Visited", true);
            }

            if (ICAO != string.Empty)
            {
                ns.Add("ICAO", ICAO);
            }

            ns.Add("Location", Location.ToList());
            
            return ns;
        }

        public Dictionary<string, dynamic> Export()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "TriggerRange", TriggerRange },
                { "Height", Height },
                { "Visible", Visible },
                { "Actions", new List<int>() },
            };

            foreach (action_base acn in Actions)
            {
                ns["Actions"].Add(acn.UID);
            }

            return ns;
        }
        
        public Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<Situation> cs = new ClassSerializer<Situation>(this, fields);
            cs.Generate(typeof(Situation), fields);

            cs.Get("location", fields, (f) => Location.ToList(4));
            cs.Get("dist_to_next", fields, (f) => Index < Adventure.Situations.Count - 1 ? Adventure.Situations.Count > 2 ? Math.Round(Utils.MapCalcDist(Location, Adventure.Situations[Index + 1].Location, Utils.DistanceUnit.NauticalMiles, true)) : Math.Round(Adventure.DistanceNM) : 0);

            if (ICAO != string.Empty)
            {
                //Airport APT = null;
                if (Airport != null)
                {
                    //APT = SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO, Location);
                    cs.Get("airport", fields, (f) => Airport.Serialize(f));
                    cs.Get("trigger_range", fields, (f) => TriggerRange);
                    cs.Get("height", fields, (f) => Height);
                    cs.Get("icao", fields, (f) => ICAO);
                }
                else
                {
                    cs.Get("trigger_range", fields, (f) => 3);
                    cs.Get("height", fields, (f) => 300);
                    cs.Get("label", fields, (f) => "Unknown Airport (" + ICAO + ")");
                    Adventure.WavePenalties = true;
                }
            }
            else
            {
                cs.Get("trigger_range", fields, (f) => TriggerRange);
                cs.Get("height", fields, (f) => Height);
                cs.Get("label", fields, (f) => LocationName != string.Empty ? LocationName : Template.SituationLabels[Index]);
            }

            var result = cs.Get();
            return result;
        }

        public override string ToString()
        {
            return Location.ToString() + " (" + ICAO + ")";
        }
    }
    
}
