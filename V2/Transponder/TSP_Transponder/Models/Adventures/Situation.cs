using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.DataStore;

namespace TSP_Transponder.Models.Adventures
{
    public class Situation
    {
        public Adventure Adventure = null;
        public AdventureTemplate Template = null;
        public int UID = 0;
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
        public Airport Airport = null;
        public GeoLoc Location = null;
        public string ICAO = "";
        public float DistToNext = 0;
        public float TriggerRange = 0;
        public float Height = 0;
        public bool Visible = true;
        public Dictionary<string, dynamic> InitialStructure = null;
        public List<action_base> Actions = new List<action_base>();
        public List<action_base> ChildActions = new List<action_base>();

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

        public bool Done {
            get
            {
                if (Visited)
                {
                    return ChildActions.Find(x => x.Completed == false) == null;
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
                TriggerRange = Situation.TriggerRange;
                ICAO = Situation.ICAO;
                if (Situation.Location != null)
                {
                    Location = Situation.Location.Copy();
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
                        if (Adventure.IsMonitored)
                        {
                            if (Index - Adventure.SituationAt > 0)
                            {
                                if (InRange)
                                {
                                    Console.WriteLine("Situation for " + Adventure.Route + " at " + Index + " is beyond range");
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
                        }
                        
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
                                                    ToDo.Add(ActionStruct["Action"] + " " + ActionStruct["Description"]);
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
                            }
                        }
                        else
                        {
                            if (NewInRange)
                            {
                                if (!InRange)
                                {
                                    InRange = true;
                                    Console.WriteLine("Situation for " + Adventure.Route + " at " + Index + " is now in range");

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
                                Console.WriteLine("Situation for " + Adventure.Route + " at " + Index + " is out of range");
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
                        break;
                    }
            }
        }


        public void ImportState(Dictionary<string, dynamic> State)
        {
            ICAO = State.ContainsKey("ICAO") ? State["ICAO"] : "";

            if(State.ContainsKey("Location"))
            {
                Location = new GeoLoc(Convert.ToDouble(State["Location"][0]), Convert.ToDouble(State["Location"][1]));
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

        public Dictionary<string, dynamic> ToListing(bool detailed)
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "UID", UID },
                { "Location", Location.ToList(4) },
            };

            if (detailed)
            {
                ns.Add("Height", Height);
                ns.Add("Visible", Visible);
            }

            if (Index < Adventure.Situations.Count - 1)
            {
                ns.Add("DistToNext", Adventure.Situations.Count > 2 ? Math.Round(Utils.MapCalcDist(Location, Adventure.Situations[Index + 1].Location, Utils.DistanceUnit.NauticalMiles, true)) : Math.Round(Adventure.DistanceNM));
            }

            if (ICAO != string.Empty)
            {
                Airport APT = null;
                try
                {
                    APT = SimLibrary.SimList[0].AirportsLib.GetByICAO(ICAO, Location);
                    ns.Add("ICAO", ICAO);
                    ns.Add("Airport", APT.ToSummary(true));
                }
                catch
                {
                    ns.Add("TriggerRange", 3);
                    ns.Add("Height", 300);
                    ns.Add("Label", "Unknown Airport (" + ICAO + ")");
                    Adventure.WavePenalties = true;
                    //APT = new Airport() { ICAO = ICAO, Location = Location };
                }

            }
            else
            {
                ns.Add("TriggerRange", TriggerRange);
                ns.Add("Height", Height);
                ns.Add("Label", Template.SituationLabels[Index]);
            }

            return ns;

        }


        public override string ToString()
        {
            return Location.ToString() + " (" + ICAO + ")";
        }
    }
    
}
