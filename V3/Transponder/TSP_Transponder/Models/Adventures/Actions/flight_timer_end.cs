using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Utilities;
using static TSP_Transponder.Models.Connectors.SimConnection;

namespace TSP_Transponder.Models.Adventures.Actions
{
    class flight_timer_end : action_base
    {
        public int LinkID = -1;
        public flight_timer_start Link = null;
        
        public flight_timer_end(Adventure Adv, AdventureTemplate Template, Situation Sit, int ID, Dictionary<string, dynamic> Params, action_base Parent) : base(Adv, Template, Sit, ID, Params, Parent)
        {
            ActionName = "flight_timer_end";
            
            LinkID = Params["Link"];

            if (Adv != null)
            {
                foreach (KeyValuePair<int, action_base> action in Adventure.ActionsIndex)
                {
                    if (action.Value.GetType() == typeof(flight_timer_start))
                    {
                        flight_timer_start TestAction = (flight_timer_start)action.Value;
                        if (TestAction.UID == LinkID)
                        {
                            Link = TestAction;
                            Link.Link = this;
                            break;
                        }
                    }
                }
            }
        }

        public override void Enter()
        {          
            Link.Clear();
            Adventure.Save();
        }
    }
}
