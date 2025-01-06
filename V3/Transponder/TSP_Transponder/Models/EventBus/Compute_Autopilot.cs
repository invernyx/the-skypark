using System;
using System.Collections.Generic;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Autopilot : Compute_Base
    {

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (!Session.Started)
            {
                return;
            }

            bool Send = false;
            if (_TemporalNewBuffer.AP_ON != _TemporalLast.AP_ON)
            {
                Send = true;
            }
            if (_TemporalNewBuffer.AP_HDG_ON != _TemporalLast.AP_HDG_ON)
            {
                Send = true;
            }
            if (_TemporalNewBuffer.AP_HDG != _TemporalLast.AP_HDG)
            {
                Send = true;
            }

            // Check AP change Master
            if (Send)
            {
                Session.AddEvent(new Event(Session)
                {
                    Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                    Type = EventType.Autopilot,
                    Params = new Dictionary<string, dynamic>()
                    {
                        { "On", _TemporalNewBuffer.AP_ON },
                        { "HdgOn", _TemporalNewBuffer.AP_HDG_ON },
                        { "Hdg", _TemporalNewBuffer.AP_HDG },
                    }
                }, false);
            }


        }
    }
}
