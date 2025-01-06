using System;
using System.Collections.Generic;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Integrity : Compute_Base
    {
        private GeoLoc PrePosition = new GeoLoc(0,0);
        private double CombinedDistance = 0;
        private double Duration = 0;

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if(!IsPaused && IsRunning && !_TemporalNewBuffer.IS_SLEW_ACTIVE)
            {
                if (!_TemporalNewBuffer.SIM_ON_GROUND)
                {
                    Session.AirTime.Start();
                }

                if (_TemporalNewBuffer.GENERAL_ENG_COMBUSTION && Session.Started)
                {
                    Session.BlockTime.Start();
                }
            }
            else
            {
                Session.AirTime.Stop();
                Session.BlockTime.Stop();
            }

            if (_TemporalLast.IS_SLEW_ACTIVE != _TemporalNewBuffer.IS_SLEW_ACTIVE && Session.Started)
            {
                if (_TemporalNewBuffer.IS_SLEW_ACTIVE)
                {
                    PrePosition.Lon = _TemporalNewBuffer.PLANE_LOCATION.Lon;
                    PrePosition.Lat = _TemporalNewBuffer.PLANE_LOCATION.Lat;
                    Duration = App.Timer.ElapsedMilliseconds / 1000;
                }
                else
                {
                    double dist = Utils.MapCalcDist(_TemporalNewBuffer.PLANE_LOCATION.Lat, _TemporalNewBuffer.PLANE_LOCATION.Lon, PrePosition.Lat, PrePosition.Lon, Utils.DistanceUnit.Kilometers);
                    CombinedDistance += dist;

                    if (dist > 0.1 || CombinedDistance > 20)
                    {
                        Session.AddEvent(new Event(Session)
                        {
                            Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                            Type = EventType.Slew,
                            TemporalRef = null,
                            Params = new Dictionary<string, dynamic>()
                            {
                                { "Distance", Math.Round(CombinedDistance, 2) },
                                { "Duration", Math.Round(Duration) },
                            }
                        }, false);
                    }
                    
                    if (CombinedDistance > 28 && !App.IsDev)
                    {
                        EventManager.ResetSession();
                    }

                }
            }


            //if (_TemporalLast.IS_SLEW_ACTIVE != _TemporalNewBuffer.IS_SLEW_ACTIVE)
            //{
            //    if (!_TemporalNewBuffer.IS_SLEW_ACTIVE)
            //    {
            //        Scene_Fx nfx = new Scene_Fx("_welcome", new GeoPosition(_TemporalNewBuffer.PLANE_LOCATION, _TemporalNewBuffer.PLANE_ALTITUDE))
            //        {
            //            UID = App.rnd.Next(8000, 30000),
            //            File = "tsp_cargonado",
            //        };
            //        nfx.Create();
            //    }
            //}
            
        }
    }
}
