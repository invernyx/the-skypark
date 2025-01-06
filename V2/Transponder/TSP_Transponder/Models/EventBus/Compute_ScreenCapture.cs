using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_ScreenCapture : Compute_Base
    {
        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (!_TemporalNewBuffer.IS_SLEW_ACTIVE)
            {
                if(Session.HasTakeoff && Session.AirTime.ElapsedMilliseconds > 600000 && Session.PhotoCount == 0 && _TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND < 20 && _TemporalNewBuffer.VERTICAL_SPEED < -200)
                {
                    //Session.ImageCapture(true);
                }
            }
        }
    }
}
