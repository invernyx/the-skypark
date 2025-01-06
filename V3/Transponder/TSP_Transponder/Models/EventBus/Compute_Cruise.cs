using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Cruise : Compute_Base
    {
        internal int LastStableAlt = 0;
        private const int LimitHistory = 10;
        List<TemporalData> History = new List<TemporalData>();
        List<double> HistoryTC = new List<double>();

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if(_TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND > 1000 && Session.Started)
            {
                History.Add(_TemporalNewBuffer.Copy());
                HistoryTC.Add(Session.Timer.ElapsedMilliseconds);

                while (History.Count > LimitHistory)
                {
                    History.RemoveAt(0);
                    HistoryTC.RemoveAt(0);
                    
                    int Max = (int)History.Max(x => x.INDICATED_ALTITUDE);
                    int Min = (int)History.Min(x => x.INDICATED_ALTITUDE);

                    if (Max - Min < 50)
                    {
                        int NewCruise = (int)Math.Round(History.Average(x => x.INDICATED_ALTITUDE) / 500) * 500;

                        if (LastStableAlt != NewCruise)
                        {
                            Session.AddEvent(new Event(Session)
                            {
                                Timecode = HistoryTC[0],
                                Type = EventType.CruiseChange,
                                TemporalRef = null,
                                Params = new Dictionary<string, dynamic>()
                                {
                                    { "Altitude", NewCruise },
                                    { "From", LastStableAlt },
                                    { "AGL", _TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND },
                                }
                            }, false);

                            LastStableAlt = NewCruise;
                        }
                    }
                }

            }
        }
    }
}
