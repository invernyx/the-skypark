using System.Collections.Generic;
using System.IO;
using System.Linq;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.FlightPlans.Types;

namespace TSP_Transponder.Models.FlightPlans
{
    public static class Plans
    {
        public static List<PlanTypeBase> PlanConverters = new List<PlanTypeBase>();
        public static List<FlightPlansDirectory> PlanDirectories = new List<FlightPlansDirectory>();
        public static List<FlightPlan> PlansList = new List<FlightPlan>();

        public static void Startup()
        {
            PlanConverters.Add(new PlanTypePLN());

            foreach (SimLibrary.Simulator Sim in SimLibrary.SimList)
            {
                foreach(string PlansFolder in Sim.FlightPlansFolders)
                {
                    try
                    {
                        if (Directory.Exists(PlansFolder))
                        {
                            PlanDirectories.Add(new FlightPlansDirectory(PlansFolder));
                        }
                    }
                    catch
                    {
                    }
                }
            }

            foreach(PlanTypeBase PlanType in PlanConverters)
            {
                foreach(FlightPlansDirectory PlanPath in PlanDirectories)
                {
                    var test = PlanType.ReadDirectory(PlanPath.Path);
                    if (test != null)
                    {
                        PlansList.AddRange(test);
                    }
                }
            }

            SortPlans();
        }

        public static void SortPlans()
        {
            PlansList = PlansList.OrderByDescending(x => x.LastModified).ToList();
        }

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[1])
            {
                case "query-from-id":
                    {
                        List<FlightPlan> plans = new List<FlightPlan>(PlansList);
                        AdventuresBase.GetAllLive((adv) =>
                        {
                            plans.AddRange(adv.LegsFlightPlans);
                        });

                        FlightPlan found = plans.Find(x => x.UID == payload_struct["id"]);
                        Socket.SendMessage("response", found != null ? App.JSSerializer.Serialize(found.Serialize((Dictionary<string, dynamic>)payload_struct["fields"])) : null, (Dictionary<string, dynamic>)structure["meta"]);
                        break;
                    }
            }
        }
    }
}
