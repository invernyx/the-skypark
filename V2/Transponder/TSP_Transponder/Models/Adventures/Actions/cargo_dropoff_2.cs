using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Payload;

namespace TSP_Transponder.Models.Adventures.Actions
{
    
    class cargo_dropoff_2 : action_base
    {
        public cargo_dropoff_2(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "cargo_dropoff_2";
            Completed = false;
        }


        public override void Process()
        {
            var pickups = Adventure.Actions.FindAll(x => x.GetType() == typeof(cargo_pickup_2));

            foreach(cargo_pickup_2 pickup in pickups)
            {
                lock (pickup.CargoManifests)
                {
                    List<CargoGroup> changes = new List<CargoGroup>();

                    foreach (var manifest in pickup.CargoManifests)
                    {
                        foreach (var group in manifest.Groups)
                        {
                            // Unloadable
                            if (group.CanUnload())
                            {
                                if (!group.Unloadable)
                                {
                                    group.Unloadable = true;
                                    changes.Add(group);
                                }
                            }
                            else
                            {
                                if (group.Unloadable)
                                {
                                    group.Unloadable = false;
                                    changes.Add(group);
                                }
                            }
                        }

                        if (changes.Count > 0)
                        {
                            Adventure.ScheduleManifestsBroadcast = true;
                        }
                    }

                    if (changes.Count > 0)
                    {
                        UpdateInteractions(true);
                        Adventure.ScheduleInteractionBroadcast = true;
                    }
                }
            }
        }

        public override void Enter()
        {
        }
        
        public override void Exit()
        {
        }

        public override void UpdateInteractions(bool Broadcast)
        {
        }
        
        public override void ImportState(Dictionary<string, dynamic> State)
        {
        }

        public override Dictionary<string, dynamic> ExportState()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>();
            
            return ns;
        }

        public override Dictionary<string, dynamic> ToListedActions()
        {
            Dictionary<string, dynamic> rt = new Dictionary<string, dynamic>()
            {
                { "UID", UID },
                { "Description", "Unload" },
                { "Action", "Dropoff" },
                { "ActionType", ActionName },
                { "Completed", Completed },
            };

            return rt;
        }
    }
}
