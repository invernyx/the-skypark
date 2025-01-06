using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_GForces : Compute_Base
    {
        List<double> SmoothedG = new List<double>();
        double HeldTime = 0;
        double SustainedG = 0;
        double SustainedGMin = 0;
        double SustainedGMax = 0;

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (!Session.Started)
            {
                return;
            }

            SmoothedG.Add(_TemporalLast.G_FORCE);
            while(SmoothedG.Count > 10)
            {
                SmoothedG.RemoveAt(0);
            }

            double SustainedGAVG = SmoothedG.Average();
            
            if (SustainedGAVG > SustainedGMin && SustainedGAVG < SustainedGMax)
            {
                if (HeldTime == 0)
                {
                    HeldTime = _TemporalNewBuffer.ABS_TIME;
                }
                // Console.WriteLine(Math.Round(SustainedGAVG).ToString());
            }
            else
            {
                double HeldFor = _TemporalNewBuffer.ABS_TIME - HeldTime;
                if (HeldFor > 5 && HeldTime != 0 && Math.Round(SustainedG) >= 2)
                {
                    Console.WriteLine("Held " + SustainedG + " G for " + HeldFor + "s");
                }
                SustainedG = Math.Round(SustainedGAVG);
                SustainedGMin = SustainedG - 0.5;
                SustainedGMax = SustainedG + 0.5;
                HeldTime = 0;
            }

        }
    }
}
