using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Taxi : Compute_Base
    {
        TemporalData TaxiStart = null;
        TemporalData TaxiLast = null;
        internal double TaxiDistance = 0;
        internal Stopwatch TaxiDuration = new Stopwatch();

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (!_TemporalNewBuffer.IS_SLEW_ACTIVE)
            {

                if (_TemporalNewBuffer.SIM_ON_GROUND && _TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED < 60)
                {
                    if (Session.Started)
                    {
                        if (TaxiStart == null)
                        {
                            Session.AddEvent(new Event(Session)
                            {
                                Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                                Type = EventType.Taxi,
                                TemporalRef = null,
                                Params = new Dictionary<string, dynamic>()
                                {
                                    { "State", 1 },
                                }
                            }, false);

                            TaxiStart = _TemporalNewBuffer;
                            TaxiDistance = 0;
                            TaxiDuration.Restart();
                        }
                        else
                        {
                            double d = Utils.MapCalcDist(_TemporalNewBuffer.PLANE_LOCATION.Lat, _TemporalNewBuffer.PLANE_LOCATION.Lon, _TemporalLast.PLANE_LOCATION.Lat, _TemporalLast.PLANE_LOCATION.Lon, Utils.DistanceUnit.Kilometers);
                            if (!double.IsNaN(d))
                            {
                                TaxiDistance += d;
                            }
                        }
                    }

                    TaxiLast = _TemporalNewBuffer.Copy();
                }
                else if (TaxiStart != null)
                {
                    double diff = Utils.TimeStamp(_TemporalNewBuffer.SYS_ZULU_TIME) - Utils.TimeStamp(TaxiLast.SYS_ZULU_TIME);

                    bool EngRunning = false;
                    if (_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_1 
                        || _TemporalNewBuffer.GENERAL_ENG_COMBUSTION_2 
                        || _TemporalNewBuffer.GENERAL_ENG_COMBUSTION_3 
                        || _TemporalNewBuffer.GENERAL_ENG_COMBUSTION_4)
                    {
                        EngRunning = true;
                    }

                    if (diff > 800 || _TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND > 15 || !EngRunning)
                    {
                        if(TaxiDistance > 0.2 && TaxiDuration.ElapsedMilliseconds > 10000)
                        {
                            Session.AddEvent(new Event(Session)
                            {
                                Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                                Type = EventType.Taxi,
                                TemporalRef = null,
                                Params = new Dictionary<string, dynamic>()
                                {
                                    { "State", 0 },
                                    { "Distance", TaxiDistance },
                                    { "Duration", TaxiDuration.ElapsedMilliseconds },
                                }
                            }, false);

                        }
                        TaxiStart = null;
                        TaxiDuration.Reset();
                        TaxiDistance = 0;
                    }
                }
            }
        }
    }
}
