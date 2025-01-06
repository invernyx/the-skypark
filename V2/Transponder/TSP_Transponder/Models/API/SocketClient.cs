using System;
using System.Collections.Generic;
using System.Windows.Controls;
using static TSP_Transponder.App;
using TSP_Transponder.Models.Adventures;
using static TSP_Transponder.Models.API.APIBase;
using static TSP_Transponder.Models.API.WSLocal;
using static TSP_Transponder.Models.API.WSRemote;
using TSP_Transponder.Models.Payload.Assets;
using TSP_Transponder.Models.FlightPlans;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Scenr;

namespace TSP_Transponder.Models.API
{
    public class SocketClient
    {
        public bool IsLocal = false;
        public LocalSocketClient LocalClient = null;
        public bool IsConnected = false;
        public string ID = "";
        public string Device = "";
        public double ConnectTime = 0;
        public double LastSeen = 0;
        public double LastSent = 0;
        public string AirportsCache = "";
        public Dictionary<string, string> FlightPlansCache = new Dictionary<string, string>();
        public Label Indicator = null;
        public ClientType Type = ClientType.Unknown;

        public SocketClient(string id, string device)
        {
            ID = id;
            Device = device;
            ConnectTime = Utils.TimeStamp(DateTime.UtcNow);
            LastSeen = Timer.ElapsedMilliseconds;
        }

        public void SendMessage(string verb, string message, Dictionary<string, dynamic> meta = null)
        {
            try
            {
                LastSeen = App.Timer.ElapsedMilliseconds;
                if (IsLocal)
                {
                    LocalClient.SendMessage(verb, message, meta);
                }
                else if (APIBase.ClientCollection.WSRemote.IsConnected)
                {
                    APIBase.ClientCollection.WSRemote.Send(verb, message, meta, SendMode.User, ID);
                }
            }
            catch
            {
                Console.WriteLine("Failed to send message to WS");
            }
        }

        public void SendReady()
        {
            if (APIBase.ClientCollection.DataReady)
            {
                switch (Type)
                {
                    case ClientType.Skypad:
                        {
                            SimConnection.WSConnected(this);
                            Bank.Bank.WSConnected(this);
                            Progress.Progress.WSConnected(this);
                            LocationHistory.WSConnected(this);
                            Notifications.NotificationService.WSConnected(this);
                            break;
                        }
                }
            }
            SendState();
        }

        public void SendState()
        {
            if (APIBase.ClientCollection.DataReady)
            {
                switch (Type)
                {
                    case ClientType.Skypad:
                        {
                            Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
                            {
                                { "ga", UserData.Get("ga") },
                                { "set", UserData.GetAll() },
                                { "persistence", Adventures.AdventuresBase.PersistenceRestored },
                            };

                            if (IsDev)
                            {
                                rs.Add("dev", "true");
                            }

                            if (IsBeta)
                            {
                                rs.Add("beta", "true");
                            }
                            
                           SendMessage("transponder:state", JSSerializer.Serialize(rs));
                            break;
                        }
                }
            }
        }

        public void Sent()
        {
            LastSent = App.Timer.ElapsedMilliseconds;
        }
        
        public void Disconnected()
        {
            IsConnected = false;
        }

        public void CommonReceive(Dictionary<string, dynamic> structure)
        {
            Console.WriteLine("WS in: " + structure["name"]);

            IsConnected = true;
            string[] StructSplit = structure["name"].Split(':');
            switch (StructSplit[0])
            {
                case "connect":
                    {
                        #region connect
                        switch (StructSplit[1])
                        {
                            case "ping":
                                {
                                    //Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
                                    //if (payload_struct != null)
                                    //{
                                    //    API.ClientCollection.Add(payload_struct["account"], payload_struct["name"], true);
                                    //}
                                    break;
                                }
                            case "confirm":
                                {
                                    Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
                                    if (payload_struct != null)
                                    {
                                        SendReady();
                                    }
                                    break;
                                }
                        }
                        break;
                        #endregion
                    }
                case "adventure":
                    {
                        #region adventure
                        Adventure.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "adventures":
                    {
                        #region adventures
                        Adventures.AdventuresBase.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "locationhistory":
                    {
                        #region location
                        LocationHistory.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "progress":
                    {
                        #region progress
                        Progress.Progress.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "fleet":
                    {
                        #region fleet
                        FleetService.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "bank":
                    {
                        #region bank
                        Bank.Bank.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "notification":
                    {
                        #region notification
                        Notifications.NotificationService.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "flyr":
                    {
                        #region flyr
                        Flyr.Flyr.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "eventbus":
                    {
                        #region eventbus
                        EventBus.EventManager.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "flightplans":
                    {
                        #region flightplans
                        Plans.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "airports":
                    {
                        #region airports
                        SimLibrary.SimList[0].AirportsLib.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "scenr":
                    {
                        #region Scenr Tool
                        switch (StructSplit[1])
                        {
                            case "cargotags":
                                {
                                    AssetsLibrary.Command(this, StructSplit, structure);
                                    break;
                                }
                            case "adventuretemplate":
                                {
                                    AdventureTemplate.Command(this, StructSplit, structure);
                                    break;
                                }
                            case "mapimages":
                                {
                                    MapImages.Command(this, StructSplit, structure);
                                    break;
                                }
                        }
                        break;
                        #endregion
                    }
                case "weather":
                    {
                        #region weather
                        Weather.AviationWeather_gov.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "topography":
                    {
                        #region topography
                        Topography.Topo.Command(this, StructSplit, structure);
                        break;
                        #endregion
                    }
                case "transponder":
                    {
                        #region transponder
                        Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
                        switch (StructSplit[1])
                        {
                            case "set":
                                {
                                    try
                                    {
                                        UserData.Set((string)payload_struct["param"], (string)payload_struct["value"], true, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Failed to change setting from WS: " + ex.Message);
                                    }
                                    MW.UpdatePreferences();
                                    break;
                                }
                            case "resetlife":
                                {
                                    AwaitsLifeReset = true;
                                    if(!RestartAdmin())
                                    {
                                        MW.Shutdown();
                                    }
                                    break;
                                }
                        }
                        break;
                        #endregion
                    }
                case "speech":
                    {
                        Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
                        switch (StructSplit[1])
                        {
                            case "play":
                                {
                                    try
                                    {
                                        string[] paramSpl = ((string)payload_struct["param"]).Split(':');
                                        Audio.AudioFramework.Stop();
                                        Audio.AudioFramework.GetSpeech(paramSpl[0], paramSpl[1].ToLower(), true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Failed to play audio: " + ex.Message);
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }

            LastSeen = App.Timer.ElapsedMilliseconds;
        }
    }
}
