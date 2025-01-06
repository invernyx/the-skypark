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
        public double SustainedGAVG = 0;
        public double SmoothedGMin = 0;
        public double SmoothedGMax = 0;

        List<double> SmoothedAccelsX = new List<double>();
        public double SustainedAccelsXAVG = 0;
        public double SmoothedAccelsXMin = 0;
        public double SmoothedAccelsXMax = 0;

        List<double> SmoothedAccelsY = new List<double>();
        public double SustainedAccelsYAVG = 0;
        public double SmoothedAccelsYMin = 0;
        public double SmoothedAccelsYMax = 0;

        List<double> SmoothedAccelsZ = new List<double>();
        public double SustainedAccelsZAVG = 0;
        public double SmoothedAccelsZMin = 0;
        public double SmoothedAccelsZMax = 0;

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

            if(_TemporalNewBuffer.IS_SLEW_ACTIVE)
            {
                return;
            }

            var td = (_TemporalNewBuffer.ABS_TIME - _TemporalLast.ABS_TIME);

            SmoothedAccelsX.Add(((_TemporalNewBuffer.VELOCITY_BODY_X - _TemporalLast.VELOCITY_BODY_X) / td) / 9.81);
            while (SmoothedAccelsX.Count > 10)
                SmoothedAccelsX.RemoveAt(0);

            SmoothedAccelsXMin = SmoothedAccelsX.Min();
            SmoothedAccelsXMax = SmoothedAccelsX.Max();
            SustainedAccelsXAVG = SmoothedAccelsX.Average();

            SmoothedAccelsY.Add(((_TemporalNewBuffer.VELOCITY_BODY_Y - _TemporalLast.VELOCITY_BODY_Y) / td) / 9.81);
            while (SmoothedAccelsY.Count > 10)
                SmoothedAccelsY.RemoveAt(0);

            SmoothedAccelsYMin = SmoothedAccelsY.Min();
            SmoothedAccelsYMax = SmoothedAccelsY.Max();
            SustainedAccelsYAVG = SmoothedAccelsY.Average();

            SmoothedAccelsZ.Add(((_TemporalNewBuffer.VELOCITY_BODY_Z - _TemporalLast.VELOCITY_BODY_Z) / td) / 9.81);
            while (SmoothedAccelsZ.Count > 10)
                SmoothedAccelsZ.RemoveAt(0);

            SmoothedAccelsZMin = SmoothedAccelsZ.Min();
            SmoothedAccelsZMax = SmoothedAccelsZ.Max();
            SustainedAccelsZAVG = SmoothedAccelsZ.Average();

            SmoothedG.Add(_TemporalNewBuffer.G_FORCE);
            while(SmoothedG.Count > 10)
                SmoothedG.RemoveAt(0);

            SmoothedGMin = SmoothedG.Min();
            SmoothedGMax = SmoothedG.Max();
            SustainedGAVG = SmoothedG.Average();
            
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
