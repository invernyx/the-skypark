using System;
using System.Collections.Generic;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Messages
    {
        internal static Event GetFPSMessage(EventsSession Session, double _Timecode, TemporalData _TemporalNewBuffer)
        {
            return new Event(Session)
            {
                Timecode = Convert.ToInt32(_Timecode),
                Type = EventType.FPS,
                Params = new Dictionary<string, dynamic>(){
                    { "FPS", Math.Round(SimFrameAvg, 4) },
                    { "Lon", Math.Round(_TemporalNewBuffer.PLANE_LOCATION.Lon, 6) },
                    { "Lat", Math.Round(_TemporalNewBuffer.PLANE_LOCATION.Lat, 6) },
                    { "Alt", Math.Round(_TemporalNewBuffer.PLANE_ALTITUDE) },
                    { "SimTimeOffset", _TemporalNewBuffer.SIM_ZULU_OFFSET },
                    { "SimTimeZulu", Utils.TimeStamp(_TemporalNewBuffer.SIM_ZULU_TIME) },
                    { "SecondsInDay", Math.Round((double)_TemporalNewBuffer.ABS_TIME) },
                }
            };
        }
        
    }
}
