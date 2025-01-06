using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Fuel : Compute_Base
    {
        TemporalData StartFuel = null;
        double BurnRate = 0;
        double Burned = 0;
        int WarnLevel = 0;
        List<double> History = new List<double>();

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (!Session.Started)
            {
                return;
            }

            // Fuel burned result
            if(_TemporalNewBuffer.GENERAL_ENG_COMBUSTION != _TemporalLast.GENERAL_ENG_COMBUSTION)
            {
                if (!_TemporalNewBuffer.GENERAL_ENG_COMBUSTION && Burned > 5)
                {
                    Session.AddEvent(new Event(Session)
                    {
                        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                        Type = EventType.Fuel,
                        Params = new Dictionary<string, dynamic>()
                        {
                            { "WarningLevel", 0 },
                            { "LitersBurned", Math.Round(Burned) }
                        }
                    }, false);
                }
            }

            // Fuel Added
            if(_TemporalLast.FUEL_QUANTITY != 0)
            {
                double diff = _TemporalNewBuffer.FUEL_QUANTITY - _TemporalLast.FUEL_QUANTITY;
                if (diff > 2)
                {
                    Session.AddEvent(new Event(Session)
                    {
                        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                        Type = EventType.Fuel,
                        Params = new Dictionary<string, dynamic>()
                        {
                            { "WarningLevel", -1 },
                            { "LitersAdded", Math.Round(diff) },
                            { "LitersRemaining", Math.Round(_TemporalNewBuffer.FUEL_QUANTITY) },
                            { "LitersBurned", Math.Round(Burned) }
                        }
                    }, false);
                }
                else
                {
                    Burned -= diff;
                }
            }

            if(StartFuel == null)
            {
                StartFuel = _TemporalNewBuffer.Copy();
                return;
            }

            double BurnDif = -(_TemporalNewBuffer.FUEL_QUANTITY - _TemporalLast.FUEL_QUANTITY);
            if(BurnDif > 0)
            {
                Burned += BurnDif;
            }

            BurnRate = BurnDif / (_TemporalNewBuffer.ABS_TIME - _TemporalLast.ABS_TIME);
            double Autonomy = _TemporalNewBuffer.FUEL_QUANTITY / BurnRate;

            if (double.IsNaN(Autonomy))
            {
                Autonomy = 999999;
            }

            // Keep 10 in history
            History.Add(Autonomy);
            while(History.Count > 10)
            {
                History.RemoveAt(0);
            }

            double BurnRateAverage = Autonomy - History.Average();

            if (_TemporalNewBuffer.VERTICAL_SPEED > 2000 || Math.Abs(BurnRateAverage) > 20 || History.Count < 10 || double.IsInfinity(Autonomy))
            {
                return;
            }
            
            bool TriggerWarning = false;
            if (Autonomy < 5 && WarnLevel <= 2)
            {
                WarnLevel = 3;
                TriggerWarning = true;
            }
            else if (Autonomy < 900 && WarnLevel <= 1)
            {
                WarnLevel = 2;
                TriggerWarning = true;
            }
            else if (Autonomy < 1800 && WarnLevel <= 0)
            {
                WarnLevel = 1;
                TriggerWarning = true;
            }

            // Low Fuel warning
            if (TriggerWarning)
            {
                Session.AddEvent(new Event(Session)
                {
                    Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                    Type = EventType.Fuel,
                    Params = new Dictionary<string, dynamic>()
                    {
                        { "WarningLevel", WarnLevel },
                        { "LitersRemaining", Math.Round(_TemporalNewBuffer.FUEL_QUANTITY) },
                        { "DurationRemaining", Math.Round(Autonomy) },
                        { "LitersBurned", Math.Round(Burned) }
                    }
                }, false);
            }
            
        }
    }
}
