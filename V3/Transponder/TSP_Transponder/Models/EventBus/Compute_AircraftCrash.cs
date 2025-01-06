using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using TSP_Transponder.Models.WorldManager;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_AircraftCrash : Compute_Base
    {
        private Scene_Fx Burn = null;
        internal TemporalData LastCrash = null;

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (!Session.Started || _TemporalNewBuffer.IS_SLEW_ACTIVE || _TemporalLast.IS_SLEW_ACTIVE)
            {
                return;
            }

            if(Burn != null && _TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND > 50)
            {
                Burn.Remove();
                Burn = null;
            }

            double td = _TemporalNewBuffer.ABS_TIME - _TemporalLast.ABS_TIME;
            double difSpd = ((_TemporalNewBuffer.AIRSPEED_TRUE - _TemporalLast.AIRSPEED_TRUE) / td);
            double difX = (_TemporalNewBuffer.VELOCITY_BODY_X - _TemporalLast.VELOCITY_BODY_X) / td;
            double difY = (_TemporalNewBuffer.VELOCITY_BODY_Y - _TemporalLast.VELOCITY_BODY_Y) / td;
            double difZ = (_TemporalNewBuffer.VELOCITY_BODY_Z - _TemporalLast.VELOCITY_BODY_Z) / td;
            
            if ((Math.Abs(difY) > 50 || Math.Abs(difX) > 50 || Math.Abs(difZ) > 50) && difSpd < -15 && _TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND < 30)
            {
                LastCrash = _TemporalNewBuffer.Copy();
                Attached_Fx Explosion = new Attached_Fx(new Point3D(0, 0, 0))
                {
                    File = "tsp_explode_medium",
                    UID = Utils.GetRandom(8000, 30000),
                    Duration = 2,
                };
                Explosion.Create();
                
                if (Burn != null)
                {
                    Burn.Remove();
                    Burn = null;
                }
                
                Burn = new Scene_Fx("_user", new GeoPosition(_TemporalLast.PLANE_LOCATION, _TemporalLast.PLANE_ALTITUDE + 1))
                {
                    File = "tsp_burn_medium",
                    UID = Utils.GetRandom(8000, 30000),
                };
                Burn.Create();

                Session.AddEvent(new Event(Session)
                {
                    Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                    Type = EventType.AircraftCrash,
                    TemporalRef = null,
                }, false);

                GoogleAnalyticscs.TrackEvent("Aircraft", "Crash", Connectors.SimConnection.Aircraft.Name, (int)Math.Round(Session.TravelDistance));
            }


        }
    }
}
