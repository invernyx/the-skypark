using System;
using System.Collections.Generic;
using System.Linq;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Payload : Compute_Base
    {
        //double TotalWeightHistory = 0;

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            //if(TotalWeightHistory != _TemporalNewBuffer.TOTAL_WEIGHT)
            //{
            //    TotalWeightHistory = _TemporalNewBuffer.TOTAL_WEIGHT;
            //    Session.AddEvent(new Event(Session)
            //    {
            //        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
            //        Type = EventType.Payload,
            //        TemporalRef = null,
            //        Params = new Dictionary<string, dynamic>()
            //        {
            //            { "Weight_Total", TotalWeightHistory },
            //        }
            //    }, false);
            //}

        }
    }
}
