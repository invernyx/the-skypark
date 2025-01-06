using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Messaging;
using TSP_Transponder.Models.WorldManager;
using TSP_Transponder.Models.Payload.Assets;
using TSP_Transponder.Models.WorldManager.Events;
using static TSP_Transponder.Models.Adventures.AdventuresBase;
using static TSP_Transponder.Models.Connectors.SimConnection;
using System.Threading;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class cargo_pickup: action_base
    {
        /*
        {
            "Model": "%RANDOM%",
            "Transport": "TSP_van_white",
            "LoadedActions": [{
 	            "UID": 73693913,
 	            "Action": "audio_speech_play",
 	            "Params": {
 		            "Delay": 0,
 		            "Character": "LARRY",
 		            "Message": "LOADED",
 		            "File": "Any"
 	            }
            }],
            "ForgotActions": []
        }
        */
        
        public CargoAsset Cargo = null;
        public string Model = "";
        public string Transport = "";

        [ObjValue("LoadedActions")]
        public List<action_base> LoadedActions = new List<action_base>();
        [ObjValue("ForgotActions")]
        public List<action_base> ForgotActions = new List<action_base>();
        [ObjValue("EndActions")]
        public List<action_base> EndActions = new List<action_base>();

        public cargo_dropoff Link = null;
        public Scene_Obj Item = null;
        private bool Reminded = false;
        private bool MeetsConditions = false;
        private bool HasEnteredRange = false;
        internal bool LoadedSoundPlayed = false;
        internal bool ItemDelivered = false;
        internal bool ItemBrought = false;
        internal bool ItemLoaded = false;
        internal DateTime? ItemLoadingEnd = null;
        internal DateTime? ItemUnloadingEnd = null;
        internal bool IsWithinCargoRange = false;
        private long LastCargoRangeCheck = 0;

        private DeliveryEvent DE = null;

        public cargo_pickup(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "cargo_pickup";
            Completed = false;
            
            if (Adv != null)
            {
                if(Params.ContainsKey("Cargo"))
                {
                    Cargo = CargoAssetsLibrary.CargoItems.Find(x => x.GUID == (string)Params["Cargo"]);
                }

                if(Cargo == null)
                {
                    Cargo = CargoAssetsLibrary.CargoItems.First();
                }

                if (!Params.ContainsKey("ContentName"))
                {
                    Params.Add("ContentName", "");
                    Params.Add("ContentValue", 0);
                    Params.Add("ContentHazard", 0);
                    Params.Add("ContentGMin", 0);
                    Params.Add("ContentGMax", 0);
                }

                if (Cargo != null)
                {
                    Model = (string)Params["Model"];
                    Transport = Cargo.TransportModels[Utils.GetRandom(Cargo.TransportModels.Count - 1)];
                    Params["ContentName"] = Cargo.Name;
                    Params["ContentValue"] = Cargo.Value;
                    Params["ContentHazard"] = Cargo.Hazard;
                    Params["ContentGMin"] = Cargo.GMin;
                    Params["ContentGMax"] = Cargo.GMax;
                    Adv.TotalValue += Cargo.Value;
                }
                else
                {
                    Console.WriteLine("Unable to find Cargo item for " + Params["Model"]);
                    Model = "wood_box_1";
                    Transport = "Skypark Truck Flatbed";
                    Params["ContentName"] = "Unspecified Cargo";
                    Params["ContentValue"] = 150;
                    Params["ContentHazard"] = 0;
                    Params["ContentGMin"] = -10;
                    Params["ContentGMax"] = 10;
                    Adv.TotalValue += 100;
                }

                #region Set Cargo Requirements on Action
                LvlMin = Cargo.XPScoreMin;
                LvlMax = Cargo.XPScoreMax;
                KarmaMin = Cargo.KarmaMin;
                KarmaMax = Cargo.KarmaMax;
                KarmaGain = Cargo.KarmaAdjust;
                #endregion

                #region Link
                if (Link == null)
                {
                    foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                    {
                        if (action.Value.GetType() == typeof(cargo_dropoff))
                        {
                            cargo_dropoff TestAction = (cargo_dropoff)action.Value;
                            if (TestAction.LinkID == UID)
                            {
                                Link = TestAction;
                                Link.Link = this;
                                break;
                            }
                        }
                    }
                }
                #endregion

                #region Lodeable Item
                Item = new Scene_Obj(Adventure.SocketID, "_welcome", null, SceneObjType.Dynamic)
                {
                    File = Model,
                    SpawnEffect = new Scene_Fx("_welcome", null)
                    {
                        UID = Utils.GetRandom(8000, 30000),
                        File = "tsp_cargonado",
                        Duration = 2,
                    },
                    DeSpawnEffect = new Scene_Fx("_welcome", null)
                    {
                        UID = Utils.GetRandom(8000, 30000),
                        File = "tsp_cargovanish",
                        Duration = 2,
                    }
                };
                DE = new DeliveryEvent(Adventure.SocketID, Item, Transport, () =>
                {
                    IsWithinCargoRange = true;
                    ItemBrought = true;
                    Load(); // TO BE REMOVED WHEN WE HAVE TRUCKS
                });
                #endregion
                
                #region Interactions
                Interactions.Add(new Interaction()
                {
                    Verb = "load",
                    UID = UID,
                    Label = "Load",
                    Style = "load",
                    Description = "",
                    Triggered = (Inter, data) =>
                    {
                        Load();
                    }
                });
                Interactions.Add(new Interaction()
                {
                    Verb = "loading",
                    UID = UID,
                    Label = "Loading...",
                    Style = "loading",
                    Description = "",
                    Enabled = false,
                    Data = ""
                });

                Interactions.Add(new Interaction()
                {
                    Verb = "unload",
                    UID = UID,
                    Label = "Unload",
                    Style = "unload",
                    Essential = false,
                    Description = "",
                    Triggered = (Inter, data) =>
                    {
                        Unload();
                        Adventure.ScheduleInteractionBroadcast = true;
                    }
                });
                Interactions.Add(new Interaction()
                {
                    Verb = "unloading",
                    UID = UID,
                    Label = "Unloading...",
                    Style = "unloading",
                    Description = "",
                    Enabled = false,
                    Data = ""
                });

                Interactions.Add(new Interaction()
                {
                    Verb = "bring",
                    UID = UID,
                    Label = "Load", // TO BE CHANGED WHEN WE HAVE TRUCKS 
                    Style = "bring",
                    Description = "",
                    Triggered = (Inter, data) =>
                    {
                        DE.Start();
                        Adventure.ScheduleInteractionBroadcast = true;
                    }
                });
                #endregion
            }
        }

        public override void UpdateInteractions(bool Broadcast)
        {
            Interaction Inter_Bring = Interactions.Find(x => x.Verb == "bring");
            Interaction Inter_Load = Interactions.Find(x => x.Verb == "load");
            Interaction Inter_Loading = Interactions.Find(x => x.Verb == "loading");
            //Interaction Inter_Unload = Interactions.Find(x => x.Verb == "unload");
            Interaction Inter_Unloading = Interactions.Find(x => x.Verb == "unloading");

            string Path = "to ???";
            if (Link != null)
            {
                Path = "❯ " + Link.Situation.ICAO;
            }

            Inter_Bring.Description = Path;
            Inter_Load.Description = Path;
            //Inter_Unload.Description = Path;

            Inter_Bring.Visible = false;
            Inter_Load.Visible = false;
            Inter_Loading.Visible = false;
            //Inter_Unload.Visible = false;
            Inter_Unloading.Visible = false;

            if (Adventure.IsMonitored)
            {
                if (ItemLoaded)
                {
                    //Inter_Unload.Visible = true;
                    return;
                }
                
                if (IsWithinCargoRange)
                {
                    if (!ItemDelivered)
                    {
                        
                        if (ItemLoaded || ItemLoadingEnd != null)
                        {
                            //Inter_Unload.Visible = true;
                            if (ItemLoadingEnd != null)
                            {
                                Inter_Loading.Visible = true;
                                Inter_Loading.Expire = ItemLoadingEnd;
                            }
                        }
                        else
                        {
                            //if (ItemBrought)
                            //{
                                Inter_Load.Visible = true;
                            //}
                        }
                    }
                }
                else
                {
                    if (!ItemBrought)
                    {
                        if (MeetsConditions)
                        {
                            Inter_Bring.Visible = true;
                            Inter_Bring.Enabled = !DE.IsRunning;
                            if (!Inter_Bring.Enabled)
                            {
                                Inter_Bring.Label = "Coming...";
                                Inter_Bring.Style = "coming";
                            }
                        }
                    }
                }
                


                //if (Situation.InRange)
                //{
                //    if (!ItemBrought)
                //    {
                //        Inter_Bring.Visible = true;
                //        Inter_Bring.Enabled = !DE.IsRunning;
                //    }
                //    else
                //    {
                //        if (!ItemLoaded)
                //        {
                //            Inter_Load.Visible = true;
                //        }
                //        else
                //        {
                //            Inter_Unload.Visible = true;
                //        }
                //    }
                //}
                //else if(Link.Situation.InRange)
                //{
                //    if (ItemLoaded)
                //    {
                //        Inter_Unload.Visible = true;
                //    }
                //}
            }

        }

        public void Load()
        {
            int loadDuration = 21000;

            if (ItemLoadingEnd == null && !ItemLoaded && !ItemDelivered && !LastTemporalData.IS_SLEW_ACTIVE)
            {
                RevalidateLoadmaster();

                ItemLoadingEnd = DateTime.UtcNow + TimeSpan.FromMilliseconds(loadDuration);
                Adventure.ScheduleInteractionBroadcast = true;
                
                Thread loadWait = new Thread(() =>
                {
                    Console.WriteLine("Loading Cargo " + Cargo.Name + " for " + (loadDuration / 1000) + "s (" + Adventure.ID + ")");
                    GeoLoc start_location = TemporalLast.PLANE_LOCATION.Copy();
                    AircraftInstance Aircraft = Connectors.SimConnection.Aircraft;

                    var loadSound = Audio.AudioFramework.GetEffect("cargomove", true);

                    #region Escape load
                    if (loadDuration > 1000)
                    {
                        int waited = 0;
                        while (waited < loadDuration)
                        {
                            Thread.Sleep(loadDuration - waited > 1000 ? 1000 : loadDuration - waited);
                            waited += 1000;

                            if (App.MW.IsShuttingDown || !Adventure.IsMonitored)
                            {
                                ItemLoadingEnd = null;
                                IsWithinCargoRange = false;
                                Adventure.ScheduleInteractionBroadcast = true;
                                loadSound.Stop();
                                return;
                            }

                            if(Utils.MapCalcDist(start_location, TemporalLast.PLANE_LOCATION, Utils.DistanceUnit.Meters) > 3)
                            {
                                ItemLoadingEnd = null;
                                IsWithinCargoRange = false;
                                Adventure.ScheduleInteractionBroadcast = true;
                                loadSound.Stop();
                                return;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(loadDuration);
                    }
                    #endregion

                    #region Attach
                    bool CanAttach = false;
                    //if (Aircraft.CargoMounts.Count > 0)
                    //{
                    //    foreach(AircraftMountingPoint Mount in Aircraft.CargoMounts)
                    //    {
                    //        if (!Mount.Occupied)
                    //        {
                    //            CanAttach = true;
                    //            Mount.Occupied = true;
                    //            Item.AttachTo(0, Mount);
                    //            break;
                    //        }
                    //    }
                    //}

                    if (!CanAttach)
                    {
                        Item.IsEnabled = false;
                    }
                    #endregion

                    ItemLoaded = true;
                    ItemLoadingEnd = null;

                    if (Link != null)
                    {
                        Link.Completed = false;
                    }
                    Completed = true;
                    foreach (action_base action in LoadedActions)
                    {
                        action.Enter();
                    }

                    #region Sound
                    // Check if we already played the load sound elsewhere
                    if (Situation.ChildActions.Find(x =>
                    {
                        if (x.GetType() == typeof(cargo_pickup))
                        {
                            cargo_pickup existing = (cargo_pickup)x;
                            if (existing.LoadedSoundPlayed)
                                return true;
                        }
                        return false;
                    }) != null)
                    {
                        if (Utils.GetRandom(5) == 3)
                        {
                            if (LoadedActions.Find(x => x.GetType() == typeof(audio_speech_play) || x.GetType() == typeof(audio_effect_play)) == null)
                            {
                                LoadedSoundPlayed = true;
                                Audio.AudioFramework.GetSpeech("characters", "larry/loaded", null, (member, route, msg) =>
                                {
                                    Chat.SendFromHandleIdent("larry", new Message(Adventure)
                                    {
                                        Content = msg,
                                        AudioPath = "characters:" + route,
                                        ContentType = Message.MessageType.Audio
                                    });

                                    Adventure.ScheduleMemosBroadcast = true;
                                    Adventure.Save();

                                    //Adventure.Memos.EnsureMember("larry", "Larry");
                                    //Adventure.Memos.Add(new Chat.Message()
                                    //{
                                    //    Type = Chat.Message.MessageType.Audio,
                                    //    Param = "characters:" + route,
                                    //    Member = member,
                                    //    Content = msg,
                                    //});
                                    //Adventure.ScheduleMemosBroadcast = true;
                                    //Adventure.Save();
                                });
                            }
                        }
                    }

                    loadSound.Stop();
                    //Audio.AudioFramework.GetEffect("cargochime", true);
                    #endregion
                    
                    Console.WriteLine("Loaded Cargo " + Cargo.Name + " for " + Adventure.Route + " (" + Adventure.ID + ")");
                    Adventure.Save();

                    Adventure.ScheduleInteractionBroadcast = true;
                });
                loadWait.IsBackground = true;
                loadWait.Start();
                
            }
        }

        public void Unload()
        {
            int loadDuration = 21000;

            if (ItemUnloadingEnd == null && ItemLoaded && !LastTemporalData.IS_SLEW_ACTIVE)
            {
                RevalidateLoadmaster();

                ItemUnloadingEnd = DateTime.UtcNow + TimeSpan.FromMilliseconds(loadDuration);
                Adventure.ScheduleInteractionBroadcast = true;

                Thread loadWait = new Thread(() =>
                {
                    Console.WriteLine("Unloading Cargo " + Cargo.Name + " for " + (loadDuration / 1000) + "s (" + Adventure.ID + ")");
                    GeoLoc start_location = TemporalLast.PLANE_LOCATION.Copy();
                    AircraftInstance Aircraft = Connectors.SimConnection.Aircraft;

                    var loadSound = Audio.AudioFramework.GetEffect("cargomove", true);

                    #region Escape load
                    if (loadDuration > 1000)
                    {
                        int waited = 0;
                        while (waited < loadDuration)
                        {
                            Thread.Sleep(loadDuration - waited > 1000 ? 1000 : loadDuration - waited);
                            waited += 1000;

                            if (App.MW.IsShuttingDown || !Adventure.IsMonitored)
                            {
                                ItemUnloadingEnd = null;
                                Adventure.ScheduleInteractionBroadcast = true;
                                loadSound.Stop();
                                return;
                            }

                            if (Utils.MapCalcDist(start_location, TemporalLast.PLANE_LOCATION, Utils.DistanceUnit.Meters) > 3)
                            {
                                ItemUnloadingEnd = null;
                                Adventure.ScheduleInteractionBroadcast = true;
                                loadSound.Stop();
                                return;
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(loadDuration);
                    }
                    #endregion

                    #region Unload location
                    int Hdg = Utils.GetRandom(0, 360);
                    GeoPosition UnloadLocation = new GeoPosition(Utils.MapOffsetPosition(
                        Utils.MapOffsetPosition(LastTemporalData.PLANE_LOCATION, Connectors.SimConnection.Aircraft.LocationFront.Z, LastTemporalData.PLANE_HEADING_DEGREES),
                        (float)(((Connectors.SimConnection.Aircraft.Wingspan * 0.5) + 2)),
                        (float)(LastTemporalData.PLANE_HEADING_DEGREES - 90)), LastTemporalData.PLANE_ALTITUDE - LastTemporalData.PLANE_ALT_ABOVE_GROUND);
                    UnloadLocation = new GeoPosition(World.FindSafeLocation(new List<Scene_Obj>() { Item }, new GeoLoc(UnloadLocation), 3, (float)LastTemporalData.PLANE_HEADING_DEGREES), UnloadLocation.Alt, Hdg);
                    if (LastTemporalData.PLANE_ALT_ABOVE_GROUND < 50 || LastTemporalData.SIM_ON_GROUND)
                    {
                        UnloadLocation.Alt = LastTemporalData.PLANE_ALTITUDE - LastTemporalData.PLANE_ALT_ABOVE_GROUND;
                    }
                    else
                    {
                        UnloadLocation.Alt = LastTemporalData.PLANE_ALTITUDE;
                    }
                    #endregion

                    #region Dropoff
                    Item.Relocate(UnloadLocation);
                    if (Item.AttachedTo != null)
                    {
                        Item.Detach();
                    }
                    else
                    {
                        Item.IsEnabled = true;
                    }
                    #endregion

                    ItemLoaded = false;
                    LastCargoRangeCheck = 0;
                    foreach (action_base action in LoadedActions)
                    {
                        action.Exit();
                    }
                    if (Link != null)
                    {
                        if (Link.Situation.InRange)
                        {
                            Link.Unload();
                        }
                        else if (Situation.InRange)
                        {
                            Completed = false;
                        }
                    }

                    #region Sound
                    Audio.AudioFramework.GetEffect("cargounload", true);
                    loadSound.Stop();
                    #endregion

                    Console.WriteLine("Unloaded Cargo " + Cargo.Name + " for " + Adventure.Route + " (" + Adventure.ID + ")");
                    Adventure.Save();
                    Adventure.ScheduleInteractionBroadcast = true;
                });
                loadWait.IsBackground = true;
                loadWait.Start();
            }
        }


        public override void Pausing()
        {
            if(!ItemLoaded)
            {
                ItemBrought = false;
            }
        }

        public override void Enter()
        {
            if (Situation.Index <= Adventure.SituationAt)
            {
                MeetsConditions = true;
            }

            lock (Adventure.ActionsWatch)
            {
                if (!Adventure.ActionsWatch.Contains(this))
                {
                    Adventure.ActionsWatch.Add(this);
                }
            }
            
            Adventure.ScheduleInteractionBroadcast = true;
        }

        public override void Exit()
        {
            MeetsConditions = false;
            HasEnteredRange = false;
            Adventure.ScheduleInteractionBroadcast = true;

            if(!ItemLoaded && !Reminded && !ItemDelivered)
            {
                if (ForgotActions.Find(x => x.GetType() == typeof(audio_speech_play) || x.GetType() == typeof(audio_effect_play)) == null)
                {
                    Reminded = true;

                    if (Template.CompanyStr.Contains("clearsky"))
                    {
                        Audio.AudioFramework.GetSpeech("characters", "brigit/load", null, (member, route, msg) =>
                        {
                            Chat.SendFromHandleIdent("brigit", new Message(Adventure)
                            {
                                Content = msg,
                                AudioPath = "characters:" + route,
                                ContentType = Message.MessageType.Call
                            });
                            
                            Adventure.ScheduleMemosBroadcast = true;
                            Adventure.Save();
                        });
                    }
                    else
                    {
                        Audio.AudioFramework.GetSpeech("characters", "pablo/load_forgot", null, (member, route, msg) =>
                        {
                            Chat.SendFromHandleIdent("pablo", new Message(Adventure)
                            {
                                Content = msg,
                                AudioPath = "characters:" + route,
                                ContentType = Message.MessageType.Call,
                            });

                            Adventure.ScheduleMemosBroadcast = true;
                            Adventure.Save();
                        });
                    }
                        
                }
            }

        }

        public override void Process()
        {
            if (ItemBrought && !HasEnteredRange && MeetsConditions)
            {
                HasEnteredRange = true;
                foreach (action_base action in EndActions)
                {
                    action.Enter();
                }
            }

            if (ItemLoaded)
            {
                foreach (action_base action in LoadedActions)
                {
                    action.Process();
                }
            }

        }
        
        public override void ProcessLive(TemporalData LastData, TemporalData NewData)
        {
            if (!ItemLoaded)
            {
                if(App.Timer.ElapsedMilliseconds > LastCargoRangeCheck + 3000)
                {
                    LastCargoRangeCheck = App.Timer.ElapsedMilliseconds;

                    if (ItemBrought)
                    {
                        //float Dist = (float)Utils.MapCalcDist(new GeoLoc(Item.Location), LastTemporalData.PLANE_LOCATION, Utils.DistanceUnit.Meters, true);
                        //var est = Math.Round(Dist, 5);
                        if (Situation.InRange) //// if (Dist < Connectors.SimConnection.Aircraft.WingspanMeters * 5) ///// && Math.Abs(LastTemporalData.PLANE_ALTITUDE - Item.Location.Alt) < 50)
                        {
                            if (!IsWithinCargoRange)
                            {
                                IsWithinCargoRange = true;
                                Adventure.ScheduleInteractionBroadcast = true;
                            }
                        }
                        //else if (IsWithinCargoRange)
                        //{
                        //    IsWithinCargoRange = false;
                        //    Adventure.ScheduleInteractionBroadcast = true;
                        //}
                    }
                }
            }

        }

        public override void Clear()
        {
            if (DE != null)
            {
                DE.Destroy();
            }
            
            Item.IsEnabled = false;

            lock (Adventure.ActionsWatch)
            {
                if (Adventure.ActionsWatch.Contains(this))
                {
                    Adventure.ActionsWatch.Remove(this);
                }
            }

            Adventure.Save();
        }

        public override void ChangedAircraft(AircraftInstance old_aircraft, AircraftInstance new_aircraft)
        {
            Item.AttachedTo = null;
        }


        public override void ImportState(Dictionary<string, dynamic> State)
        {
            if (State.ContainsKey("Completed"))
            {
                Completed = State["Completed"];
            }

            if (State.ContainsKey("ItemLoaded"))
            {
                ItemLoaded = State["ItemLoaded"];
            }

            if (State.ContainsKey("ItemDelivered"))
            {
                ItemDelivered = State["ItemDelivered"];
            }

            if (State.ContainsKey("ItemBrought"))
            {
                ItemBrought = State["ItemBrought"];
                if (ItemBrought)
                {
                    Item.Relocate(new GeoPosition(new GeoLoc(Convert.ToDouble(State["Item.Location"][0]), Convert.ToDouble(State["Item.Location"][1])), Convert.ToDouble(State["Item.Location"][2]), Convert.ToDouble(State["Item.Location"][3])));

                    if (!ItemLoaded && !Adventure.Cleanedup)
                    {
                        Item.IsEnabled = true;
                    }
                }
            }
            
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
                { "Cargo", Cargo.GUID },
                { "Model", Model },
            };

            if (Adventure.ActionsWatch.Contains(this))
            {
                ns.Add("LiveActionWatch", true);
            }

            if (Completed == true)
            {
                ns.Add("Completed", true);
            }

            if (ItemLoaded)
            {
                ns.Add("ItemLoaded", true);
            }

            if (ItemDelivered)
            {
                ns.Add("ItemDelivered", true);
            }

            if (ItemBrought)
            {
                ns.Add("ItemBrought", true);
            }

            if(Item.Location != null)
            {
                ns.Add("Item.Location", new List<double>() { Item.Location.Lon, Item.Location.Lat, Item.Location.Alt, Item.Location.Hdg });
            }

            return ns;
        }


        public override Dictionary<string, dynamic> ToListedActions()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "id", UID },
                { "description", Cargo.Name },
                { "action", "Pickup" },
                { "action_type", ActionName },
                { "completed", Completed },
            };

            return rt;
        }

        public override Dictionary<string, dynamic> ComputeForTempate(Route route, Dictionary<string, dynamic> Params)
        {
            // Find cargo from tag
            List<CargoAsset> PossibleItems = null;
            if (CargoAssetsLibrary.CargoItemsByTag.ContainsKey(Params["Model"]))
            {
                PossibleItems = CargoAssetsLibrary.CargoItemsByTag[Params["Model"]];
            }
            else
            {
                PossibleItems = CargoAssetsLibrary.CargoItemsByTag["any"];
            }
            List<CargoAsset> VotingItems = new List<CargoAsset>();

            // Change value of picked cargo based on filters
            //if(Filters != null)
            //{
            //    if(PossibleItems.Count > 5)
            //    {
            //        double PossibleBracket = 1;
            //        float MaxValue = PossibleItems.Max(x => x.Value);
            //        float MinValue = PossibleItems.Min(x => x.Value);
            //
            //        if (Filters["icao_from"] != string.Empty)
            //        {
            //            PossibleBracket /= 5;
            //        }
            //
            //        if (Filters["icao_to"] != string.Empty)
            //        {
            //            PossibleBracket /= 5;
            //        }
            //
            //        double EffectiveValue = MinValue + ((MaxValue - MinValue) * PossibleBracket);
            //        PossibleItems = PossibleItems.FindAll(x => x.Value < EffectiveValue);
            //    }
            //}

            // Create probability
            foreach(CargoAsset Item in PossibleItems)
            {
                // Populate Voting List
                int i = 0;
                while (i < Item.Frequency)
                {
                    VotingItems.Add(Item);
                    i++;
                }
            }
            
            // Pick a winner
            CargoAsset WinningCargo = VotingItems[Utils.GetRandom(VotingItems.Count - 1)];

            // Define Cargo
            Params["Cargo"] = WinningCargo.GUID;
            Params["Model"] = WinningCargo.Models[Utils.GetRandom(WinningCargo.Models.Count)];
            Params["Transport"] = WinningCargo.TransportModels[Utils.GetRandom(WinningCargo.TransportModels.Count - 1)];
            return Params;
        }
    }
}
