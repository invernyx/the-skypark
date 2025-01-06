using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Payload;
using TSP_Transponder.Models.Payload.Assets;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    public class cargo_pickup_2: action_base
    {
        public List<CargoManifest> CargoManifests = null;

        Interaction Interactions_LoadRange = null;
        Interaction Interactions_UnloadRange = null;
        
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
                    CargoManifests.Add(new CargoManifest(Adv, config));
                }

                #region Interactions
                Interactions_LoadRange = new Interaction()
                {
                    Verb = "load_range",
                    UID = UID,
                    Label = "Load",
                    Style = "load",
                    Description = "",
                    Triggered = (Inter, data) =>
                    {
                        Load();
                        Adventure.ScheduleInteractionBroadcast = true;
                    }
                };
                Interactions.Add(Interactions_LoadRange);

                Interactions_UnloadRange = new Interaction()
                {
                    Verb = "unload_range",
                    UID = UID,
                    Label = "Unload",
                    Style = "unload",
                    Description = "",
                    Triggered = (Inter, data) =>
                    {
                        Unload();
                        Adventure.ScheduleInteractionBroadcast = true;
                    }
                };
                Interactions.Add(Interactions_UnloadRange);
                #endregion

            }
        }

        public override void UpdateInteractions(bool broadcast)
        {
            Interactions_LoadRange.Visible = false;
            Interactions_UnloadRange.Visible = false;

            if (Adventure.IsMonitored)
            {
                foreach (var manifest in CargoManifests)
                {
                    if(manifest.Groups.Find(x => x.CanLoad()) != null)
                    {
                        Interactions_LoadRange.Visible = true;
                    }

                    if (manifest.Groups.Find(x => x.CanUnload()) != null)
                    {
                        Interactions_UnloadRange.Visible = true;
                    }
                }
            }
        }
        
        public void Load(long? manifestID = null, Guid? groupGUID = null, List<Guid> unitGUID = null)
        {
            if(manifestID != null)
            {
                CargoManifest found = CargoManifests.Find(x => x.UID == manifestID);
                if (found != null)
                {
                    if (groupGUID != null)
                    {
                        CargoGroup group = found.Groups.Find(x => x.GUID.ToString() == ((Guid)groupGUID).ToString());
                        if (group != null)
                        {
                            group.Load();
                            Adventure.ScheduleManifestsBroadcast = true;
                        }
                    }
                }
            }

            var unloaded = CargoManifests.Find(x => x.Groups.Find(x1 => x1.LoadedOn == null) != null);
            if(unloaded == null)
            {
                Completed = true;
            }

        }

        public void Unload(long? manifestID = null, Guid? groupGUID = null, List<Guid> unitGUID = null)
        {
            if (manifestID != null)
            {
                CargoManifest found = CargoManifests.Find(x => x.UID == manifestID);
                if (found != null)
                {
                    if (groupGUID != null)
                    {
                        CargoGroup group = found.Groups.Find(x => x.GUID.ToString() == ((Guid)groupGUID).ToString());
                        if (group != null)
                        {
                            group.Unload();
                            Adventure.ScheduleManifestsBroadcast = true;
                        }
                    }
                }
            }

            var unloaded = CargoManifests.Find(x => x.Groups.Find(x1 => x1.LoadedOn == null) != null);
            if (unloaded == null)
            {
                Completed = true;
            }
        }

        public override void Interact(Dictionary<string, dynamic> interaction)
        {
            switch ((string)interaction["Verb"])
            {
                case "load":
                    {
                        long manifest = Convert.ToInt64(interaction["Data"]["Manifest"]);
                        string group = interaction["Data"]["Group"];

                        Load(manifest, group != null ? new Guid(group) : (Guid?)null);
                        break;
                    }
                case "unload":
                    {
                        long manifest = Convert.ToInt64(interaction["Data"]["Manifest"]);
                        string group = interaction["Data"]["Group"];

                        Unload(manifest, group != null ? new Guid(group) : (Guid?)null);
                        break;
                    }
            }
        }

        public override void Starting()
        {
            foreach(var manifest in CargoManifests)
            {
                manifest.Startup(this);
            }
        }

        public override void Ending()
        {
            foreach (var manifest in CargoManifests)
            {
                manifest.Cleanup();
            }
        }


        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void Process()
        {
            lock (CargoManifests)
            {
                List<CargoGroup> changes = new List<CargoGroup>();

                foreach(var manifest in CargoManifests)
                {
                    foreach(var group in manifest.Groups)
                    {
                        // Loadable
                        if (group.CanLoad())
                        {
                            if (!group.Loadable)
                            {
                                group.Loadable = true;
                                changes.Add(group);
                            }
                        }
                        else
                        {
                            if (group.Loadable)
                            {
                                group.Loadable = false;
                                changes.Add(group);
                            }
                        }
                    }

                    if (changes.Count > 0)
                    {
                        Adventure.ScheduleManifestsBroadcast = true;
                    }
                }

                if(changes.Count > 0)
                {
                    UpdateInteractions(true);
                    Adventure.ScheduleInteractionBroadcast = true;
                }
            }
        }
        
        public override void LiveProcess(TemporalData LastData, TemporalData NewData)
        {

        }

        public override void Clear()
        {

        }

        public override void ChangedAircraft()
        {

        }


        public override void ImportState(Dictionary<string, dynamic> state)
        {
            if (Adventure != null)
            {
                foreach (Dictionary<string, dynamic> param in state["Manifests"])
                {
                    CargoManifest existingManifest = CargoManifests.Find(x => x.UID == (long)param["UID"]);
                    existingManifest.ImportState(param);
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


        public override Dictionary<string, dynamic> ToListedActions()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "UID", UID },
                { "Description", "TBD" },
                { "Action", "Pickup" },
                { "ActionType", ActionName },
                { "Completed", Completed },
            };

            return rt;
        }

        public override Dictionary<string, dynamic> ComputeForTempate(Route route, Dictionary<string, dynamic> config)
        {
            // Find cargo from tag
            List<Cargo> possibleItems = null;

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
                Cargo winningCargo = null;
                float minWeightKG = manifest["MinWeight"];
                float maxWeightKG = manifest["MaxWeight"];
                string tag = (string)manifest["Tag"];
                int dropoffCount = 0;

                // Extract dropoff manifests about this pickup manifest
                var filteredDropoffManifests = dropoffManifestsForThisPickup.Select(x =>
                {
                    var key = x.Key;
                    Dictionary<string, dynamic> val = ((ArrayList)x.Value["Manifests"]).Cast<Dictionary<string, dynamic>>().ToList().Find(x1 => x1["ID"] == manifest["UID"]); //((List<Dictionary<string, dynamic>>)x.Value["Manifests"]).Find(x1 => x1["ID"] == manifest["UID"]);
                    return new KeyValuePair<int, Dictionary<string, dynamic>>(key, val);
                }).ToList();

                // Make it count
                dropoffCount = filteredDropoffManifests.FindAll(x => x.Value["MinDeliveryRatio"] > 0 && x.Value["MaxDeliveryRatio"] > 0).Count;

                // Find all with tag
                possibleItems = AssetsLibrary.CargoItemsByTag.ContainsKey(tag) ? AssetsLibrary.CargoItemsByTag[tag] : AssetsLibrary.CargoItemsByTag["any"];
                
                // Filter to possible cargo weights
                possibleItems = possibleItems.FindAll(x => maxWeightKG / x.WeightKG >= 1 && x.WeightKG * dropoffCount < maxWeightKG);
                
                // Find the right cargo item
                if (possibleItems.Count > 0)
                {
                    // Build random library
                    WeightedRandom<Cargo> targetCounts = new WeightedRandom<Cargo>();
                    foreach (Cargo c in possibleItems)
                    {
                        targetCounts.AddEntry(c, c.Frequency);
                    }

                    // Pick a winner
                    winningCargo = targetCounts.GetRandom();
                }
                else
                {
                    winningCargo = AssetsLibrary.CargoItemsByTag["any"].First();
                    if (App.IsDev)
                    {
                        NotificationService.Add(new Notification()
                        {
                            Title = "Failed to find cargo item for " + tag + " in " + Template.Name,
                            Message = "Nothing in the weight bracket " + minWeightKG + " and " + maxWeightKG + "KG",
                            Type = NotificationType.Status,
                        });
                    }
                }

                int maxRouteWeight = 0;// (int)Math.Round(minWeightKG + Utils.GetRandom(maxWeightKG - minWeightKG));
                int index = 0;
                foreach(var recommendedAcf in route.RecommendedAircraft)
                {
                    if(recommendedAcf > 10)
                    {
                        switch (index)
                        {
                            case 0: { maxRouteWeight = 400; break; } // Heli
                            case 1: { maxRouteWeight = 600; break; } // Piston
                            case 2: { maxRouteWeight = 1000; break; } // Turbo
                            case 3: { maxRouteWeight = 4000; break; } // Jet
                            case 4: { maxRouteWeight = 25000; break; } // Narrow
                            case 5: { maxRouteWeight = 60000; break; } // Heavy
                        }
                        if(maxRouteWeight > 0) { break; }
                    }
                    index++;
                }

                // Restrict weights
                maxWeightKG = Math.Min(maxRouteWeight, maxWeightKG);

                // Find our target weight
                int targetWeight = (int)Math.Round(minWeightKG + Utils.GetRandom(maxWeightKG - minWeightKG));
                int targetUnits = (int)Math.Ceiling((double)targetWeight / winningCargo.WeightKG);

                // Create the setting for this dropoff
                List<Dictionary<string, dynamic>> dropoffsSet = new List<Dictionary<string, dynamic>>();
                
                manifest.Add("Total", targetUnits);
                manifest.Add("DropoffConfig", dropoffsSet);

                // Assign quantities
                double totalRatio = 0;
                foreach(var dropoff in filteredDropoffManifests)
                {
                    double ratio = Math.Round(Utils.GetRandom((double)dropoff.Value["MinDeliveryRatio"], (double)dropoff.Value["MaxDeliveryRatio"]));
                    totalRatio += ratio;
                    
                    dropoffsSet.Add(new Dictionary<string, dynamic>()
                    {
                        { "Ratio", ratio },
                        { "DropoffID", dropoff.Key }
                    });
                }
                
                // distribute payload based on ratios
                int aboveFloorUnits = targetUnits - dropoffCount;
                int unitsLeft = targetUnits;
                double ratioOfRatio = aboveFloorUnits * (1 / totalRatio);
                int index1 = 0;
                foreach (var dropoff in filteredDropoffManifests)
                {
                    double ratio = dropoffsSet[index1]["Ratio"];
                    int proposedDropoffUnits = 0;

                    if (ratio > 0)
                    {
                        proposedDropoffUnits = 1 + (int)Math.Floor(ratioOfRatio * ratio);
                        if (index1 == filteredDropoffManifests.Count - 1)
                        {
                            if (unitsLeft > 0)
                            {
                                proposedDropoffUnits = unitsLeft;
                            }
                        }
                        dropoffsSet[index1]["Units"] = proposedDropoffUnits;
                    }
                    else
                    {
                        dropoffsSet[index1]["Units"] = 0;
                    }
                    index1++;
                    unitsLeft -= proposedDropoffUnits;
                }


                
                // Define Cargo
                manifest.Add("CargoGUID", winningCargo.GUID.ToString());
                manifest.Add("Model", winningCargo.Models[Utils.GetRandom(winningCargo.Models.Count)]);
            }
            
            return config;
        }
        
    }
}
