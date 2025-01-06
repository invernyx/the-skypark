using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.FlightPlans.Types
{
    abstract public class PlanTypeBase
    {
        public string Ext = "";

        public virtual FlightPlan ReadFile(string path)
        {
            return null;
        }

        public virtual FlightPlan ReadContent(List<string> content)
        {
            return null;
        }

        public virtual List<FlightPlan> ReadDirectory(string path)
        {
            return null;
        }
    }
}
