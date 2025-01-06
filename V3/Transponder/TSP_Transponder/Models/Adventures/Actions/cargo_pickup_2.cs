using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.Messaging;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Payload;
using TSP_Transponder.Models.Payload.Assets;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Attributes.ObjAttr;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    public class cargo_pickup_2: action_base
    {
        public List<CargoManifest> CargoManifests = null;

        [ObjValue("LoadedActions")]
        public List<action_base> LoadedActions = new List<action_base>();
        [ObjValue("ForgotActions")]
        public List<action_base> ForgotActions = new List<action_base>();
        [ObjValue("EndActions")]
        public List<action_base> EndActions = new List<action_base>();
        
        private bool Reminded = false;
        private bool ItemLoaded
        {
            get{
                return CargoManifests.Find(x => x.Groups.Find(x1 => x1.LoadedOn == null) == null) == null;
            }
        }

        public cargo_pickup_2(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "cargo_pickup_2";

            Completed = false;
            Order = 1;

            if (Adv != null)
            {
                CargoManifests = new List<CargoManifest>();
                foreach (Dictionary<string, dynamic> config in Params["Manifests"])
                {
                    var manifest = new CargoManifest(Adv, Situation, this, config);
                    CargoManifests.Add(manifest);
                }
            }
        }


        public void Load(long? manifest_id = null, Guid? group_guid = null, List<Guid> unit_guid = null)
        {
            List<CargoGroup> groups = new List<CargoGroup>();

            if (manifest_id != null)
            {
                CargoManifest found = CargoManifests.Find(x => x.UID == manifest_id);
                if (found != null)
                {
                    if (group_guid != null)
                    {
                        CargoGroup group = found.Groups.Find(x => x.GUID.ToString() == ((Guid)group_guid).ToString());
                        if (group != null)
                        {
                            groups.Add(group);
                            group.Load();

                            if (CargoManifests.Find(x => x.Groups.Find(x1 => !x1.TakenCharge) != null) == null)
                            {
                                group.CompletedAction = () =>
                                {
                                    Completed = true;
                                    foreach (action_base action in LoadedActions)
                                    {
                                        action.Enter();
                                    }
                                };
                            }

                            Adventure.ScheduleManifestsStateBroadcast = true;
                            Adventure.ScheduleInteractionBroadcast = true;

                            Adventure.Save();
                        }
                    }
                }
            }


        }

        public void Transfer(long? manifest_id = null, Guid? group_guid = null, List<Guid> unit_guid = null)
        {
            if (manifest_id != null)
            {
                CargoManifest found = CargoManifests.Find(x => x.UID == manifest_id);
                if (found != null)
                {
                    if (group_guid != null)
                    {
                        CargoGroup group = found.Groups.Find(x => x.GUID.ToString() == ((Guid)group_guid).ToString());
                        if (group != null)
                        {
                            group.Transfer();
                            Adventure.ScheduleManifestsStateBroadcast = true;
                            Adventure.ScheduleInteractionBroadcast = true;
                        }
                    }
                }
            }

            Adventure.Save();
        }

        public void Unload(long? manifest_id = null, Guid? group_guid = null, List<Guid> unit_guid = null)
        {
            if (manifest_id != null)
            {
                CargoManifest found = CargoManifests.Find(x => x.UID == manifest_id);
                if (found != null)
                {
                    if (group_guid != null)
                    {
                        CargoGroup group = found.Groups.Find(x => x.GUID.ToString() == ((Guid)group_guid).ToString());
                        if (group != null)
                        {
                            if (group.CanUnload())
                            {
                                var dropoff_keys = found.Dropoffs.Keys.ToList();
                                cargo_dropoff_2 dropoff = (cargo_dropoff_2)found.Destinations[(int)group.DestinationIndex].Action;

                                group.Unload();

                                group.CompletedAction = () =>
                                {

                                    var remaining_all = 0;
                                    var all_pickups = Adventure.Actions.FindAll(x => x.GetType() == typeof(cargo_pickup_2)).ToList();
                                    PayloadDestination dest = found.Destinations[(int)group.DestinationIndex];
                                    foreach (var pickup in all_pickups)
                                    {
                                        cargo_pickup_2 pickup_cast = (cargo_pickup_2)pickup;

                                        var remaining_current = pickup_cast.CargoManifests.FindAll(x =>
                                        {
                                            return x.Groups.FindAll(x1 => x.Destinations[(int)x1.DestinationIndex].Action.UID == dest.Action.UID && !x1.Delivered).Count > 0;
                                        });
                                        remaining_all += remaining_current.Count;
                                    }

                                    if (remaining_all == 0)
                                        dropoff.SetComplete(true);
                                };


                            }

                        }
                        Adventure.ScheduleInteractionBroadcast = true;
                        Adventure.ScheduleManifestsStateBroadcast = true;
                    }
                }
            }

            Adventure.Save();
        }


        public override void Interact(Dictionary<string, dynamic> interaction)
        {
            switch ((string)interaction["verb"])
            {
                case "load_range":
                    {
                        //long manifest = Convert.ToInt64(interaction["Data"]["Manifest"]);
                        //string group = interaction["Data"]["Group"];

                        //Load(manifest, group != null ? new Guid(group) : (Guid?)null);
                        break;
                    }
                case "transfer":
                    {
                        long manifest = Convert.ToInt64(interaction["data"]["manifest"]);
                        string group = interaction["data"]["group"];

                        Transfer(manifest, group != null ? new Guid(group) : (Guid?)null);
                        break;
                    }
                case "load":
                    {
                        long manifest = Convert.ToInt64(interaction["data"]["manifest"]);
                        string group = interaction["data"]["group"];

                        Load(manifest, group != null ? new Guid(group) : (Guid?)null);
                        break;
                    }
                case "unload":
                    {
                        long manifest = Convert.ToInt64(interaction["data"]["manifest"]);
                        string group = interaction["data"]["group"];

                        Unload(manifest, group != null ? new Guid(group) : (Guid?)null);
                        break;
                    }
            }
        }

        public override bool PreStart(Action<List<Dictionary<string, dynamic>>> objection_callback)
        {
            bool objections = false;

            foreach (var manifest in CargoManifests)
            {
                if (!manifest.PreConfigure((objection) =>
                {
                    objection_callback(objection);
                }))
                {
                    objections = true;
                    break;
                }
            }

            return objections;
        }


        public override void Starting()
        {
            foreach (var manifest in CargoManifests)
            {
                manifest.Configure(SimConnection.Aircraft.Cabin);
            }
        }

        public override void Ending()
        {
            foreach (var manifest in CargoManifests)
            {
                manifest.Ending();
            }
        }


        public override void Pausing()
        {
            ProcessAdv();
        }
        
        public override void ProcessAdv()
        {
            lock (CargoManifests)
            {
                int changes = 0;

                foreach (var manifest in CargoManifests)
                {
                    changes += manifest.Process();
                }

                if (changes > 0)
                {
                    UpdateInteractions(true);
                    Adventure.ScheduleInteractionBroadcast = true;
                }
            }
        }

        public override void Process()
        {
            if (ItemLoaded)
            {
                foreach (action_base action in LoadedActions)
                {
                    action.Process();
                }
            }

        }

        public override void Clear()
        {

        }

        public override void ChangedAircraft(AircraftInstance old_aircraft, AircraftInstance new_aircraft)
        {
            if (old_aircraft != new_aircraft && old_aircraft != null)
            {
                lock (CargoManifests)
                {
                    foreach (var manifest in CargoManifests)
                    {
                        foreach (var group in manifest.Groups)
                        {
                            group.ChangedAircraft(old_aircraft, new_aircraft);
                        }
                    }
                }
            }
        }

        public override void Enter()
        {
            base.Enter();
            
            lock (Adventure.ActionsWatch)
            {
                if (!Adventure.ActionsWatch.Contains(this))
                {
                    Adventure.ActionsWatch.Add(this);
                }
            }

            if (ItemLoaded)
            {
                foreach (action_base action in LoadedActions)
                {
                    action.Process();
                }
            }

            Adventure.ScheduleInteractionBroadcast = true;
        }

        public override void Exit()
        {
            base.Exit();
            
            Adventure.ScheduleInteractionBroadcast = true;

            if (!ItemLoaded && !Reminded && !(bool)Completed)
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

            if (ItemLoaded)
            {
                foreach (action_base action in LoadedActions)
                {
                    action.Exit();
                }
            }

        }



        public override void ImportState(Dictionary<string, dynamic> state)
        {
            if (Adventure != null)
            {
                foreach (Dictionary<string, dynamic> param in state["Manifests"])
                {
                    CargoManifest existingManifest = CargoManifests.Find(x => x.UID == (long)param["UID"]);
                    existingManifest.ImportState(param);
                    Adventure.TotalValue += existingManifest.Definition.Value * existingManifest.TotalQualtity;
                }

                if (state.ContainsKey("Completed"))
                {
                    Completed = true;
                }
            }
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "Manifests", CargoManifests.Select(x => x.ExportState()).ToList() },
            };

            if (Completed == true)
            {
                ns.Add("Completed", true);
            }

            return ns;
        }


        public override Dictionary<string, dynamic> Serialize(Dictionary<string, dynamic> fields)
        {
            ClassSerializer<cargo_pickup_2> cs = new ClassSerializer<cargo_pickup_2>(this, fields);
            cs.Generate(typeof(Adventure), fields);

            cs.Get("manifest", fields, (f) => CargoManifests.Select(x => x.Serialize(f)) );

            var result = cs.Get();
            return result;
        }

        public override Dictionary<string, dynamic> ToListedActions()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "id", UID },
                { "description", "TBD" },
                { "action", "Pickup" },
                { "action_type", ActionName },
                { "completed", Completed },
            };

            return rt;
        }

        public override Dictionary<string, dynamic> ComputeForTempate(Route route, Dictionary<string, dynamic> config)
        {
            // Find all dropoffs and pickups
            List<cargo_pickup_2> pickups = Template.Actions.FindAll(x => typeof(cargo_pickup_2) == x.GetType()).Cast<cargo_pickup_2>().ToList();
            List<cargo_dropoff_2> dropoffs = Template.Actions.FindAll(x => typeof(cargo_dropoff_2) == x.GetType()).Cast<cargo_dropoff_2>().ToList();
            float weightDivider = pickups.Sum(x => x.Parameters["Manifests"].Count);

            // Get all dropoffs for this manifest
            List<KeyValuePair<int, Dictionary<string, dynamic>>> dropoffManifestsForThisPickup = new List<KeyValuePair<int, Dictionary<string, dynamic>>>();
            foreach (var dropoff in dropoffs)
            {
                var dropoffManifests = ((ArrayList)dropoff.Parameters["Manifests"]).Cast<Dictionary<string, dynamic>>().ToList();
                var existingManifest = dropoffManifests.Find(x => x["UID"] == UID);
                if (existingManifest != null)
                {
                    dropoffManifestsForThisPickup.Add(new KeyValuePair<int, Dictionary<string, dynamic>>(dropoff.UID, existingManifest));
                }
            }

            // Go through all Manifests for this pickup action
            foreach (Dictionary<string, dynamic> manifest in config["Manifests"])
            {
                // Extract dropoff manifests about this pickup manifest
                var filteredDropoffManifests = dropoffManifestsForThisPickup.Select(x =>
                {
                    var key = x.Key;
                    Dictionary<string, dynamic> val = ((ArrayList)x.Value["Manifests"]).Cast<Dictionary<string, dynamic>>().ToList().Find(x1 => x1["ID"] == manifest["UID"]); //((List<Dictionary<string, dynamic>>)x.Value["Manifests"]).Find(x1 => x1["ID"] == manifest["UID"]);
                    return new KeyValuePair<int, Dictionary<string, dynamic>>(key, val);
                }).ToList();

                // Create the setting for dropoffs
                List<Dictionary<string, dynamic>> dropoffsSet = new List<Dictionary<string, dynamic>>();
                Dictionary<string, dynamic> pickupsSet = new Dictionary<string, dynamic>();

                // Assign delivery points
                float totalDeliveryPoints = 0;
                foreach (var dropoff in filteredDropoffManifests)
                {
                    int points = Convert.ToInt32(Math.Ceiling(Utils.GetRandom((double)dropoff.Value["MinDeliveryRatio"], (double)dropoff.Value["MaxDeliveryRatio"])));
                    totalDeliveryPoints += points;

                    dropoffsSet.Add(new Dictionary<string, dynamic>()
                    {
                        { "Points", points },
                        { "DropoffID", dropoff.Key }
                    });
                }

                // Assign pickup percentages
                float minPercent = manifest["MinPercent"];
                float maxPercent = manifest["MaxPercent"];
                pickupsSet.Add("Percent", Utils.GetRandom(minPercent, maxPercent));
                
                // Save Totals
                manifest.Add("DropoffConfig", dropoffsSet);
                manifest.Add("PickupConfig", pickupsSet);
                
            }


            // Go through all Manifests for this pickup action
            foreach (Dictionary<string, dynamic> manifest in config["Manifests"])
            {
                CargoAsset winningCargo = null;
                string tag = (string)manifest["Tag"];
                
                // Find all with tag
                var possibleItems = CargoAssetsLibrary.CargoItemsByTag.ContainsKey(tag) ? CargoAssetsLibrary.CargoItemsByTag[tag] : CargoAssetsLibrary.CargoItemsByTag["any"];
                
                // Find the right cargo item
                if (possibleItems.Count > 0)
                {
                    // Build random library
                    WeightedRandom<CargoAsset> targetCounts = new WeightedRandom<CargoAsset>();
                    foreach (CargoAsset c in possibleItems)
                    {
                        targetCounts.AddEntry(c, c.Frequency);
                    }

                    // Pick a winner
                    winningCargo = targetCounts.GetRandom();
                }
                else
                {
                    winningCargo = CargoAssetsLibrary.CargoItemsByTag["any"].First();
                    if (App.IsDev)
                    {
                        NotificationService.Add(new Notification()
                        {
                            Title = "Failed to find cargo item for " + tag + " in " + Template.Name,
                            Message = "Nothing possible cargo.",
                            Type = NotificationType.Status,
                        });
                    }
                }

                // Define Cargo
                manifest.Add("CargoGUID", winningCargo.GUID.ToString());
                manifest.Add("Model", winningCargo.Models[Utils.GetRandom(winningCargo.Models.Count)]);
            }


            return config;
        }

    }
}
