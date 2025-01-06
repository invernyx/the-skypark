using System;
using System.Collections.Generic;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Aircraft : Compute_Base
    {
        private string AircraftName = "";

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if(SimConnection.Aircraft != null)
            {
                if (AircraftName != SimConnection.Aircraft.Name)
                {
                    AircraftName = SimConnection.Aircraft.Name;

                    Session.AddEvent(new Event(Session)
                    {
                        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                        Type = EventType.AircraftChange,
                        TemporalRef = null,
                        Params = new Dictionary<string, dynamic>()
                        {
                            { "Aircraft", SimConnection.Aircraft.Serialize(null) },
                        }
                    }, true);
                }
            }
            else
            {
                AircraftName = string.Empty;
            }
        }
    }
}
