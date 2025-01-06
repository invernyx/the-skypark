using DiscordRPC;
using DiscordRPC.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.EventBus;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder
{
    public class RichPresence
    {
        public static DiscordRpcClient DiscordRichPresence = null;

        public static void Startup()
        {
            DiscordRichPresence = new DiscordRpcClient("635656780130811904");// new Discord.Discord(635656780130811904, (UInt64)Discord.CreateFlags.Default);

            //Set the logger
            DiscordRichPresence.Logger = new ConsoleLogger() { Level = LogLevel.None };

            //Subscribe to events
            DiscordRichPresence.OnReady += (sender, e) =>
            {
                Console.WriteLine("Discord is ready {0}", e.User.Username);
                Update();
            };

            //DiscordRichPresence.OnPresenceUpdate += (sender, e) =>
            //{
                //Console.WriteLine("Discord Presence has been updated {0}", e.Presence);
            //};

            DiscordRichPresence.OnError += (sender, e) =>
            {
                Console.WriteLine("Discord failed to update Presence {0}", e.Message);
            };


            //Connect to the RPC
            DiscordRichPresence.Initialize();
        }
        
        public static void Update()
        {
            bool Publish = false;
            string Details = "";
            string State = "";
            bool NearestInState = false;
            string Asset = "tsp_white";
            DateTime? Start = null;
            DateTime? End = null;

            if(IsLoaded && AdventuresBase.PersistenceRestored)
            {
                if (ActiveSim != null && LastTemporalData.ABS_TIME > 0 && UserData.Get("discord_presence") == "1")
                {
                    List<Adventure> ActiveAdventure = null;
                    lock (AdventuresBase.AllContracts)
                    {
                        ActiveAdventure = AdventuresBase.AllContracts.FindAll(x => x.State == Adventure.AState.Active && x.IsMonitored);
                    }

                    if (ActiveAdventure.Count > 0)
                    {
                        if (ActiveAdventure[0].Template.Name.Length > 0)
                        {
                            Details = ActiveAdventure[0].Template.Name + (ActiveAdventure[0].RouteCode.Length > 0 ? " (" + ActiveAdventure[0].RouteCode + ")" : "");
                        }
                        else
                        {
                            Details = ActiveAdventure[0].Route + (ActiveAdventure[0].RouteCode.Length > 0 ? " (" + ActiveAdventure[0].RouteCode + ")" : "");
                        }

                        NearestInState = true;

                        if (ActiveAdventure[0].Template.Company.Count > 0)
                        {
                            Asset = "logo_" + ActiveAdventure[0].Template.Company[0].Code;
                        }
                    }
                    else
                    {
                        if (EventManager.NearestAirport != null)
                        {
                            Details = "Free Flight near " + EventManager.NearestAirport.ICAO;
                        }
                        else
                        {
                            Details = "Free Flight";
                        }
                    }

                    if (EventManager.Active != null)
                    {
                        if (EventManager.Active.Compute_Touchdown.LastTakeoffData != null)
                        {
                            Start = EventManager.Active.Compute_Touchdown.LastTakeoffData.SYS_ZULU_TIME;
                        }

                        if (LastTemporalData.SIM_ON_GROUND)
                        {
                            State = "On the ground";
                        }
                        else
                        {
                            if (LastTemporalData.PLANE_ALTITUDE > 10000)
                            {
                                State = "Stable at FL" + Utils.FormatNumber(Math.Round(LastTemporalData.PLANE_ALTITUDE / 100)) + "";
                            }
                            else
                            {
                                State = "Stable at " + Utils.FormatNumber(Math.Round(LastTemporalData.PLANE_ALTITUDE)) + "ft";
                            }
                        }

                        if (LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED < 5)
                        {
                            if (LastTemporalData.SIM_ON_GROUND)
                            {
                                State = "Waiting";
                            }
                            else
                            {
                                State = "Hovering";
                            }
                        }
                        else if (LastTemporalData.SIM_ON_GROUND)
                        {
                            if (EventManager.Active.Compute_Touchdown.LandingData != null)
                            {
                                State = "Landing";
                            }
                            else
                            {
                                State = "Taxiing";
                            }
                        }

                        if (LastTemporalData.VERTICAL_SPEED > 300)
                        {
                            if (LastTemporalData.PLANE_ALTITUDE > 10000)
                            {
                                State = "▲ Climbing FL" + Math.Round(LastTemporalData.PLANE_ALTITUDE / 100) + "";
                            }
                            else
                            {
                                State = "▲ Climbing " + Utils.FormatNumber(Math.Round(LastTemporalData.PLANE_ALTITUDE)) + "ft";
                            }
                        }
                        else if (LastTemporalData.VERTICAL_SPEED < -300)
                        {
                            if (LastTemporalData.PLANE_ALTITUDE > 10000)
                            {
                                State = "▼ Descending FL" + Math.Round(LastTemporalData.PLANE_ALTITUDE / 100) + "";
                            }
                            else
                            {
                                State = "▼ Descending " + Utils.FormatNumber(Math.Round(LastTemporalData.PLANE_ALTITUDE)) + "ft";
                            }
                        }

                        if (Math.Abs(EventManager.Active.Compute_Cruise.LastStableAlt - LastTemporalData.PLANE_ALTITUDE) < 2000 && LastTemporalData.PLANE_ALT_ABOVE_GROUND > 2000)
                        {
                            if (EventManager.Active.Compute_Cruise.LastStableAlt < 10000)
                            {
                                State = "Cruising at " + EventManager.Active.Compute_Cruise.LastStableAlt + "ft";
                            }
                            else
                            {
                                State = "Cruising at FL" + Utils.FormatNumber(Math.Round((double)EventManager.Active.Compute_Cruise.LastStableAlt / 100));
                            }
                        }

                        if (!LastTemporalData.GENERAL_ENG_COMBUSTION)
                        {
                            if (LastTemporalData.EXIT)
                            {
                                State = "No Parking in the red zone";
                            }
                            else if (LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED > 30 && !LastTemporalData.SIM_ON_GROUND)
                            {
                                State = "I guess we're gliding";
                            }
                            else
                            {
                                State = "Just chillin'";
                            }
                        }

                        if (EventManager.Active.Compute_Touchdown.LastLandingData != null)
                        {
                            if (EventManager.Active.Compute_Touchdown.LastLandingData.SYS_TIME > DateTime.Now.AddMinutes(-2))
                            {
                                State = "Landed (" + Utils.FormatNumber(EventManager.Active.Compute_Touchdown.LastLandingAccel) + "Gs)";
                            }
                        }

                        if (EventManager.Active.Compute_AircraftCrash.LastCrash != null)
                        {
                            if (EventManager.Active.Compute_AircraftCrash.LastCrash.SYS_TIME > DateTime.Now.AddMinutes(-1))
                            {
                                State = "Aircraft went boom";
                            }
                        }

                        if (NearestInState)
                        {
                            if (EventManager.NearestAirport != null)
                            {
                                if (LastTemporalData.SIM_ON_GROUND && EventManager.NearestAirport.Radius < Utils.MapCalcDist(LastTemporalData.PLANE_LOCATION, EventManager.NearestAirport.Location, Utils.DistanceUnit.NauticalMiles, true))
                                {
                                    State += " at " + EventManager.NearestAirport.ICAO;
                                }
                                else
                                {
                                    State += " near " + EventManager.NearestAirport.ICAO;
                                }
                            }
                        }

                    }

                    Publish = true;
                }
                else if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parallel 42", "DEV.txt")))
                {
                    List<Airport> APTs = SimLibrary.SimList[0].AirportsLib.GetAirportsCopy();
                    Airport APT = APTs[(int)Math.Round((double)Utils.GetRandom(APTs.Count))];

                    Asset = "logo_orbit";
                    Details = "Relocating pilots";
                    State = "Near " + (APT.City != string.Empty ? APT.City + ", " : "") + APT.CountryName;
                    Publish = true;
                }
                else
                {
                    DiscordRichPresence.ClearPresence();
                }

                if (Publish)
                {
                    DiscordRPC.RichPresence rp = new DiscordRPC.RichPresence()
                    {
                        Details = LimitString(Details),
                        State = LimitString(State),
                    };

                    if (Asset != string.Empty)
                    {
                        rp.Assets = new Assets()
                        {
                            LargeImageKey = Asset,
                        };
                    }

                    if (Start != null)
                    {
                        rp.Timestamps = new Timestamps((DateTime)Start, End);
                    }

                    DiscordRichPresence.SetPresence(rp);
                }
            }
            else
            {
                DiscordRichPresence.ClearPresence();
            }

        }
        
        private static string LimitString(string message)
        {
            const int MaxLogMessageLength = 128;
            int n = Encoding.Unicode.GetByteCount(message);

            if (n > MaxLogMessageLength)
            {
                message = message.Substring(0, MaxLogMessageLength / 2); // Most UTF16 chars are 2 bytes.

                while (Encoding.Unicode.GetByteCount(message) > MaxLogMessageLength)
                    message = message.Substring(0, message.Length - 1);
            }

            return message;
        }

    }
}
