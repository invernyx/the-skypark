using System;
using System.Collections.Generic;
using System.Linq;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Position : Compute_Base
    {
        private TemporalData LastEntry = new TemporalData();
        private List<Event> PosList = null;
        //private List<Event> SamePositionEvents = new List<Event>();
        private Event BackTwo = null;
        private Event BackOne = null;
        private double BackTwoBearing = 0;
        private double BackOneBearing = 0;
        private double BackTwoFPM = 0;
        private double BackOneFPM = 0;
        private double CourseDif = 0;
        private double HeadingDif = 0;
        private double AirspeedDif = 0;
        private double FPMDif = 0;
        private double Distance = 0;
        private double Narrowing = 0;
        
        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (_TemporalNewBuffer.IS_SLEW_ACTIVE)
            {
                return;
            }

            if(_TemporalLast.PLANE_LOCATION.Lat != 0 && _TemporalLast.PLANE_LOCATION.Lon != 0)
            {
                if(_TemporalNewBuffer.PLANE_LOCATION.Lon != _TemporalLast.PLANE_LOCATION.Lon && _TemporalNewBuffer.PLANE_LOCATION.Lat != _TemporalLast.PLANE_LOCATION.Lat)
                {
                    double newDist = Utils.MapCalcDist(_TemporalNewBuffer.PLANE_LOCATION.Lat, _TemporalNewBuffer.PLANE_LOCATION.Lon, _TemporalLast.PLANE_LOCATION.Lat, _TemporalLast.PLANE_LOCATION.Lon, Utils.DistanceUnit.Kilometers);
                    if (!Double.IsNaN(newDist))
                    {
                        Session.TravelDistance += newDist;
                    }
                }
            }

            // If position changed, push
            if (Math.Abs(_TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED - LastEntry.SURFACE_RELATIVE_GROUND_SPEED) > 3 || Math.Abs(_TemporalNewBuffer.PLANE_LOCATION.Lon - LastEntry.PLANE_LOCATION.Lon) > 0.0001 || Math.Abs(_TemporalNewBuffer.PLANE_LOCATION.Lat - LastEntry.PLANE_LOCATION.Lat) > 0.0001 || Math.Abs(_TemporalNewBuffer.PLANE_ALTITUDE - LastEntry.PLANE_ALTITUDE) > 1 || (Math.Abs(_TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED) > 1 && !Session.Started))
            {
                //SamePositionEvents.Clear();

                if (Math.Abs(_TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED) > 3)
                {
                    Session.Start();
                }

                if (Session.Started)
                {
                    // Set Current Tile
                    WorldManager.World.UpdateAircraftTiles(_TemporalNewBuffer.PLANE_LOCATION, (int)_TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND, (int)_TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED, (int)_TemporalNewBuffer.PLANE_COURSE);

                    // Remember last entry
                    LastEntry = _TemporalNewBuffer.Copy();

                    int FPM = 0;
                    lock (TemporalBuffer3s)
                    {
                        FPM = TemporalBuffer3s.Count > 3 ? (int)Math.Round(TemporalBuffer3s.Select(x => x.VERTICAL_SPEED).Average()) : 0;
                    }

                    Airport nearest = null;
                    var nearest_range = SimLibrary.SimList[0].AirportsLib.GetAirportByRange(LastTemporalData.PLANE_LOCATION, 3).FirstOrDefault();
                    if (nearest_range.Value != null)
                    {
                        nearest = nearest_range.Value;
                    }

                    Event newEvent = new Event(Session)
                    {
                        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                        Type = EventType.Position,
                        TemporalRef = LastEntry,
                        Params = new Dictionary<string, dynamic>(){
                            //{ "Platform", ActiveSim.Index },
                            //{ "VMajor", ActiveSimVersionMajor },
                            //{ "VMinor", ActiveSimVersionMinor },
                            { "Lon", Math.Round(_TemporalNewBuffer.PLANE_LOCATION.Lon, 6) },
                            { "Lat", Math.Round(_TemporalNewBuffer.PLANE_LOCATION.Lat, 6) },
                            { "Alt", Math.Round(_TemporalNewBuffer.PLANE_ALTITUDE) },
                            { "GAlt", Math.Round(_TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND) },
                            { "HDG", Math.Round(Utils.Normalize360(_TemporalNewBuffer.PLANE_HEADING_DEGREES)) },
                            { "GS", Math.Round(_TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED) },
                            { "CRS", Math.Round(_TemporalNewBuffer.PLANE_COURSE) },
                            { "MagVar", Math.Round(_TemporalNewBuffer.PLANE_MAGVAR) },
                            { "TurnRate", LastTemporalData.PLANE_TURNRATE },
                            { "FPM", FPM },
                            { "NearestAirport", nearest != null ? nearest.ICAO : null },
                            { "SimTimeOffset", LastTemporalData.SIM_ZULU_OFFSET },
                            { "SimTimeZulu", Utils.TimeStamp(LastTemporalData.SIM_ZULU_TIME) },
                            { "SecondsInDay", Math.Round((double)_TemporalNewBuffer.ABS_TIME) },
                        }
                    };

                    if (Session.EventDic[EventType.Position].Count < 2)
                    {
                        Session.AddEvent(newEvent, true);
                    }
                    else
                    {
                        PosList = Session.EventDic[EventType.Position];

                        BackTwo = PosList[PosList.Count - 2];
                        BackOne = PosList[PosList.Count - 1];
                        BackTwoBearing = Utils.MapCalcBearing(BackTwo.TemporalRef.PLANE_LOCATION.Lat, BackTwo.TemporalRef.PLANE_LOCATION.Lon, _TemporalNewBuffer.PLANE_LOCATION.Lat, _TemporalNewBuffer.PLANE_LOCATION.Lon);
                        BackOneBearing = _TemporalNewBuffer.PLANE_COURSE;
                        BackTwoFPM = (BackTwo.TemporalRef.PLANE_ALTITUDE - _TemporalNewBuffer.PLANE_ALTITUDE) / ((BackTwo.TemporalRef.ABS_TIME - _TemporalNewBuffer.ABS_TIME) / 60);
                        BackOneFPM = (BackOne.TemporalRef.PLANE_ALTITUDE - _TemporalNewBuffer.PLANE_ALTITUDE) / ((BackOne.TemporalRef.ABS_TIME - _TemporalNewBuffer.ABS_TIME) / 60);
                        CourseDif = BackOneBearing - BackTwoBearing;
                        HeadingDif = _TemporalNewBuffer.PLANE_HEADING_DEGREES - BackTwo.TemporalRef.PLANE_HEADING_DEGREES;
                        AirspeedDif = _TemporalNewBuffer.AIRSPEED_TRUE - BackTwo.TemporalRef.AIRSPEED_TRUE;

                        FPMDif = BackTwoFPM - BackOneFPM;
                        Distance = Utils.MapCalcDist(BackTwo.TemporalRef.PLANE_LOCATION.Lat, BackTwo.TemporalRef.PLANE_LOCATION.Lon, _TemporalNewBuffer.PLANE_LOCATION.Lat, _TemporalNewBuffer.PLANE_LOCATION.Lon, Utils.DistanceUnit.Kilometers);
                        Narrowing = 1 + Distance;

                        if (Math.Abs(CourseDif) < 1 / Narrowing && Math.Abs(FPMDif) < 150 / Narrowing && Math.Abs(HeadingDif) < 2 && Math.Abs(HeadingDif) < 50)
                        {
                            //Console.WriteLine("---> Updating last Position (" + Math.Round(CourseDif, 2) + "/" + (1 / Narrowing) + "deg, " + Math.Round(FPMDif, 2) + "/" + (150 / Narrowing) + "fpm)");
                            PosList[PosList.Count - 1] = newEvent;

                            Session.UpdateEvent(newEvent, false);
                            
                            if (SimHasFocus)
                            {
                                Session.AddEvent(Messages.GetFPSMessage(Session, Session.Timer.ElapsedMilliseconds, _TemporalNewBuffer), false);
                            }

                            Session.BroadcastEvents();

                        }
                        else
                        {
                            if (SimHasFocus)
                            {
                                newEvent.Params.Add("FPS", Math.Round(SimFrameAvg, 4));
                            }

                            Session.AddEvent(newEvent, true); 
                        }
                    }

                    SimConnection.Aircraft.Process();
                    
                }
            }

            //else if(SimHasFocus)
            //{
            //    Event newTempEvent = new Event(Session)
            //    {
            //        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
            //        Params = new Dictionary<string, dynamic>(){
            //            { "FPS", Math.Round(SimFrameAvg, 4) },
            //        }
            //    };
            //
            //    List<int> AverageFromSamePosition = new List<int>();
            //    lock (SamePositionEvents)
            //    {
            //        SamePositionEvents.Add(newTempEvent);
            //        if(SamePositionEvents.Count > 10)
            //        {
            //            SamePositionEvents.RemoveAt(0);
            //        }
            //
            //        foreach(Event ev in SamePositionEvents)
            //        {
            //            if (ev.Params.ContainsKey("FPS"))
            //            {
            //                AverageFromSamePosition.Add((int)ev.Params["FPS"]);
            //            }
            //        }
            //    }
            //
            //    Event newEvent = new Event(Session)
            //    {
            //        Timecode = newTempEvent.Timecode,
            //        Type = EventType.FPS,
            //        Params = new Dictionary<string, dynamic>(){
            //            { "FPS", Math.Round(SimFrameAvg, 4) },
            //            { "Time", Utils.TimeStamp(_TemporalNewBuffer.SIM_ZULU_TIME) },
            //            { "Local", Utils.TimeStamp(_TemporalNewBuffer.SIM_LOCAL_TIME) },
            //            { "SecondsInDay", Math.Round(_TemporalNewBuffer.SIM_RUNTIME) },
            //        }
            //    };
            //
            //    bool isKeyframe = false;
            //    if(Session.EventDic[EventType.FPS].Count > 1)
            //    {
            //        if(_TemporalNewBuffer.SIM_RUNTIME - Session.EventDic[EventType.FPS][Session.EventDic[EventType.FPS].Count - 2].Params["SecondsInDay"] > 60)
            //        {
            //            isKeyframe = true;
            //        }
            //    }
            //    else
            //    {
            //        isKeyframe = true;
            //    }
            //    
            //    if (AverageFromSamePosition.Count == 1 || isKeyframe)
            //    {
            //        Session.AddEvent(newEvent);
            //    }
            //    else
            //    {
            //        if(Session.EventDic[EventType.FPS].Last().Params["FPS"] != newEvent.Params["FPS"])
            //        {
            //            Session.SwapLastEvent(newEvent);
            //        }
            //    }
            //
            //}

        }
        
    }
}
