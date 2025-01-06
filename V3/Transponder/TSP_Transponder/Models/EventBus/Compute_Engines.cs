using System;
using System.Collections.Generic;
using TSP_Transponder.Models.WorldManager.Events;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Engines : Compute_Base
    {
        private bool HasChanged = false;
        private bool EngRunning = false;
        private Dictionary<string, dynamic> Change = new Dictionary<string, dynamic>();

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (_TemporalNewBuffer.IS_SLEW_ACTIVE)
            {
                return;
            }

            // If Engine changed, push
            if (_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_1 != _TemporalLast.GENERAL_ENG_COMBUSTION_1)
            {
                HasChanged = true;
                if(Connectors.SimConnection.Aircraft.EngineCount == 1)
                {
                    Change.Add("0", Convert.ToInt16(_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_1));
                }
                else
                {
                    Change.Add("1", Convert.ToInt16(_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_1));
                }
            }

            if (_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_2 != _TemporalLast.GENERAL_ENG_COMBUSTION_2)
            {
                HasChanged = true;
                Change.Add("2", Convert.ToInt16(_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_2));
            }

            if (_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_3 != _TemporalLast.GENERAL_ENG_COMBUSTION_3)
            {
                HasChanged = true;
                Change.Add("3", Convert.ToInt16(_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_3));
            }

            if (_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_4 != _TemporalLast.GENERAL_ENG_COMBUSTION_4)
            {
                HasChanged = true;
                Change.Add("4", Convert.ToInt16(_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_4));
            }

            EngRunning = (_TemporalNewBuffer.GENERAL_ENG_COMBUSTION_1 || _TemporalNewBuffer.GENERAL_ENG_COMBUSTION_2 || _TemporalNewBuffer.GENERAL_ENG_COMBUSTION_3 || _TemporalNewBuffer.GENERAL_ENG_COMBUSTION_4);

            if (Session.Timer.ElapsedMilliseconds == 0 && EngRunning)
            {
                Session.Start();
            }

            if (HasChanged)
            {
                if (EngRunning)
                {
                    Session.Start();
                }

                HasChanged = false;
                Session.AddEvent(new Event(Session)
                {
                    Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                    Type = EventType.Engines,
                    Params = Change,
                }, true);
                Change = new Dictionary<string, dynamic>();

            }
            
            //if (!EngRunning && Session.Started && Session.HasTakeoff)
            //{
            //    if (Math.Abs(_TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED) < 1 && Session.Compute_Touchdown.GetBufferState() && _TemporalNewBuffer.SIM_ON_GROUND)
            //    {
            //        Reset();
            //    }
            //}
        }
    }
}
