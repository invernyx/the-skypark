using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    abstract public class Compute_Base
    {
        public virtual void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {

        }
    }
}
