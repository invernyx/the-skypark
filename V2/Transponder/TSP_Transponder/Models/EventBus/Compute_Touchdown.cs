using System;
using System.Collections.Generic;
using System.Diagnostics;
using static TSP_Transponder.Models.Connectors.SimConnection;
using static TSP_Transponder.Models.Airports.AirportsLib;
using static TSP_Transponder.Models.EventBus.EventManager;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.DataStore;
using System.Linq;
using LiteDB;

namespace TSP_Transponder.Models.EventBus
{
    class Compute_Touchdown : Compute_Base
    {
        private readonly float TakeoffClearAlt = 50;
        private readonly float LandingClearCrs = 30;
        private readonly int AccelBufferDuration = 40;
        private float MaxYAccel = 0;
        private float MaxFPM = 0;
        private int BounceCount = 0;
        private double AccelBufferStart = 0;
        private float LandingBearing = 0;
        private float LandingLatestBearing = 0;
        private bool LandingCaptureExpired = false;
        internal TemporalData LandingData = null;
        private TemporalData TakeoffData = null;

        internal TemporalData LastLandingData = null;
        internal double LastLandingAccel = 0;

        internal TemporalData LastTakeoffData = null;

        public override void Compute(long _TimeNow, TemporalData _TemporalLast, TemporalData _TemporalNewBuffer, EventsSession Session)
        {
            if (_TemporalNewBuffer.IS_SLEW_ACTIVE || Session.Timer.ElapsedMilliseconds < 1000)
            {
                return;
            }

            // Check Expire
            if (AccelBufferStart != 0)
            {
                if (_TemporalNewBuffer.ABS_TIME > AccelBufferStart + AccelBufferDuration)
                {
                    Console.WriteLine("Landing Data Expired");
                    if (_TemporalNewBuffer.SIM_ON_GROUND)
                    {
                        LandingCaptureExpired = true;
                    }
                    else
                    {
                        AccelBufferStart = 0;
                        LandingCaptureExpired = false;
                    }
                }
            }

            // Buffer landing rate
            if (AccelBufferStart != 0)
            {
                LandingLatestBearing = (float)Utils.MapCalcBearing(_TemporalLast.PLANE_LOCATION.Lat, _TemporalLast.PLANE_LOCATION.Lon, _TemporalNewBuffer.PLANE_LOCATION.Lat, _TemporalNewBuffer.PLANE_LOCATION.Lon);
                
                if (!LandingCaptureExpired
                    && _TemporalNewBuffer.SURFACE_RELATIVE_GROUND_SPEED > 0.1
                    && Math.Abs(Utils.MapCompareBearings(LandingBearing, LandingLatestBearing)) < LandingClearCrs)
                {
                    if (_TemporalNewBuffer.SIM_ON_GROUND)
                    {
                        // Listen for higher accel values
                        if (MaxYAccel < _TemporalLast.ACCELERATION_BODY_Y)
                        {
                            MaxYAccel = (float)_TemporalLast.ACCELERATION_BODY_Y;
                        }

                        // FPM
                        if (MaxFPM > _TemporalLast.VERTICAL_SPEED)
                        {
                            MaxFPM = (float)_TemporalLast.VERTICAL_SPEED;
                        }
                        

                    }
                }
                else
                {
                    if (LandingData != null && TakeoffData == null)
                    {
                        if (_TemporalNewBuffer.SIM_ON_GROUND)
                        {
                            // Landed and Clear
                            AccelBufferStart = 0;
                            string LandingDist = Convert.ToString(Math.Round(Utils.MapCalcDist(LandingData.PLANE_LOCATION.Lat, LandingData.PLANE_LOCATION.Lon, _TemporalLast.PLANE_LOCATION.Lat, _TemporalLast.PLANE_LOCATION.Lon, Utils.DistanceUnit.Meters), 2));
                            if (LandingCaptureExpired)
                            {
                                LandingDist = "Unknown";
                            }

                            Airport nearAirport = Session.Sim.AirportsLib.GetAirportByRange(LandingData.PLANE_LOCATION, 10)[0].Value;
                            
                            if(nearAirport != null)
                            {
                                lock (Session.Airports)
                                {
                                    if (Session.Airports.Find(x => x.ICAO == nearAirport.ICAO) == null)
                                    {
                                        Session.Airports.Add(nearAirport);
                                    }
                                }

                                if (nearAirport.ICAO != string.Empty)
                                {
                                    VisitAirport(nearAirport.ICAO);
                                }
                            }


                            LastLandingAccel = Math.Round(1 + MaxYAccel, 3);
                            LastTakeoffData = null;
                            LastLandingData = LandingData.Copy();
                            Session.AddEvent(new Event(Session)
                            {
                                Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                                Type = EventType.Landing,
                                TemporalRef = LandingData,
                                Params = new Dictionary<string, dynamic>()
                                {
                                    { "AirportName", nearAirport != null ? nearAirport.Name : null },
                                    { "AirportICAO", nearAirport != null ? nearAirport.ICAO : null },
                                    { "Bounce", BounceCount },
                                    { "LandingDist", LandingDist },
                                    { "AirTime", Math.Round((double)Session.AirTime.ElapsedMilliseconds / 1000) },
                                    { "Lon", Math.Round(LandingData.PLANE_LOCATION.Lon, 6) },
                                    { "Lat", Math.Round(LandingData.PLANE_LOCATION.Lat, 6) },
                                    { "Alt", Math.Round(LandingData.PLANE_ALTITUDE) },
                                    { "HDG", Math.Round(LandingData.PLANE_HEADING_DEGREES) },
                                    { "CRS", Math.Round(LandingData.PLANE_COURSE) },
                                    { "Rate", LastLandingAccel },
                                    { "FPM", Math.Round(MaxFPM) },
                                    { "Pitch", Math.Round(LandingData.PLANE_PITCH_DEGREES, 3) },
                                    { "Bank", Math.Round(LandingData.PLANE_BANK_DEGREES, 3) },
                                    { "Beta", Math.Round(LandingData.INCIDENCE_BETA, 3) },
                                    { "Sfc", LandingData.SURFACE_TYPE },
                                    { "SfcCond", LandingData.SURFACE_CONDITION },
                                    { "Alpha", Math.Round(LandingData.INCIDENCE_ALPHA, 3) },
                                    { "TAS", Math.Round(LandingData.AIRSPEED_TRUE) },
                                    { "GS", Math.Round(LandingData.SURFACE_RELATIVE_GROUND_SPEED, 3) },
                                    { "WindHdg", Math.Round(LandingData.AMBIENT_WIND_DIRECTION) },
                                    { "WindSpeed", Math.Round(LandingData.AMBIENT_WIND_VELOCITY) },
                                    { "SimTimeOffset", LandingData.SIM_ZULU_OFFSET },
                                    { "SimTimeZulu", Utils.TimeStamp(LandingData.SIM_ZULU_TIME) },
                                    { "FPS", Math.Round(SimFrameAvg, 4) },
                                }
                            }, false);
                            Console.WriteLine("Added latest Landing");
                            GoogleAnalyticscs.TrackEvent("Aircraft", "Landing", Connectors.SimConnection.Aircraft.Name, (int)Math.Round(Session.TravelDistance));

                            bool UpdateLocation = false;
                            Adventures.AdventuresBase.GetAllLive((Adv) => {
                                if (Adv.IsMonitored)
                                {
                                    UpdateLocation = true;
                                }
                            });

                            // Update last location in DB
                            if (UpdateLocation)
                            {
                                LocationHistory.UpdateLastLocation(nearAirport, LandingData.PLANE_LOCATION);
                            }
                            
                            if (Session.AirTime.ElapsedMilliseconds > 300000 && Session.Airports.Count > 1 && nearAirport.ICAO == "KMKC")
                            {
                                Progress.Progress.UnlockReliability();
                            }
                        }
                        else
                        {
                            // Cancel... Back in the air
                            Console.WriteLine("No Landing if you are back in the air...");
                        }

                        AccelBufferStart = 0;
                        LandingData = null;
                    }
                    else
                    {
                        // Cancel... There is takeoff data
                        Console.WriteLine("No Landing if there is no Cleared Takeoff...");
                        AccelBufferStart = 0;
                        LandingData = null;
                    }
                }
                 
            }

            // Check changes
            if (_TemporalNewBuffer.SIM_ON_GROUND != _TemporalLast.SIM_ON_GROUND)
            {
                if (_TemporalNewBuffer.SIM_ON_GROUND)
                {
                    Session.AirTime.Stop();
                    if (TakeoffData != null || LandingData != null)
                    {
                        // Bounce
                        double now = _TemporalNewBuffer.ABS_TIME;
                        double dif = now - AccelBufferStart;
                        AccelBufferStart = now;

                        if (dif > 0.5)
                        {
                            BounceCount++;
                            Console.WriteLine("Bounce Detected");
                        }
                    }
                    else
                    {
                        // Touchdown
                        AccelBufferStart = _TemporalNewBuffer.ABS_TIME;
                        LandingData = _TemporalNewBuffer.Copy();
                        BounceCount = 0;
                        LandingBearing = (float)Utils.MapCalcBearing(_TemporalLast.PLANE_LOCATION.Lat, _TemporalLast.PLANE_LOCATION.Lon, LandingData.PLANE_LOCATION.Lat, LandingData.PLANE_LOCATION.Lon);
                        Console.WriteLine("Touchdown Detected");
                    }
                }
                else
                {
                    Session.AirTime.Start();
                    if (LandingData == null)
                    {
                        // Takeoff
                        TakeoffData = _TemporalNewBuffer.Copy();
                        Console.WriteLine("Takeoff Detected");
                    }
                }
            }
            else if (TakeoffData != null)
            {
                // Takeoff & Cleared
                if (_TemporalNewBuffer.PLANE_ALT_ABOVE_GROUND > TakeoffClearAlt)
                {
                    Console.WriteLine("Confirmed Takeoff");
                    Airport nearAirport = Session.Sim.AirportsLib.GetAirportByRange(TakeoffData.PLANE_LOCATION, 10)[0].Value;

                    if (nearAirport == null)
                    {
                        nearAirport = new Airport();
                    }

                    lock(Session.Airports)
                    {
                        if (Session.Airports.Find(x => x.ICAO == nearAirport.ICAO) == null)
                        {
                            Session.Airports.Add(nearAirport);
                        }
                    }

                    if (nearAirport.ICAO != string.Empty)
                    {
                        VisitAirport(nearAirport.ICAO);
                    }

                    LastLandingData = null;
                    LastTakeoffData = TakeoffData.Copy();
                    Session.AddEvent(new Event(Session)
                    {
                        Timecode = Convert.ToInt32(Session.Timer.ElapsedMilliseconds),
                        Type = EventType.Takeoff,
                        TemporalRef = TakeoffData,
                        Params = new Dictionary<string, dynamic>(){
                            { "AirportName", nearAirport.Name },
                            { "AirportICAO", nearAirport.ICAO },
                            { "Bounce", BounceCount },
                            { "Lon", Math.Round(TakeoffData.PLANE_LOCATION.Lon, 6) },
                            { "Lat", Math.Round(TakeoffData.PLANE_LOCATION.Lat, 6) },
                            { "Alt", Math.Round(TakeoffData.PLANE_ALTITUDE) },
                            { "HDG", Math.Round(TakeoffData.PLANE_HEADING_DEGREES) },
                            { "CRS", Math.Round(TakeoffData.PLANE_COURSE) },
                            { "Pitch", Math.Round(TakeoffData.PLANE_PITCH_DEGREES, 3) },
                            { "Bank", Math.Round(TakeoffData.PLANE_BANK_DEGREES, 3) },
                            { "Beta", Math.Round(TakeoffData.INCIDENCE_BETA, 3) },
                            { "Sfc", TakeoffData.SURFACE_TYPE },
                            { "SfcCond", TakeoffData.SURFACE_CONDITION },
                            { "Alpha", Math.Round(TakeoffData.INCIDENCE_ALPHA, 3) },
                            { "TAS", Math.Round(TakeoffData.AIRSPEED_TRUE, 3) },
                            { "GS", Math.Round(TakeoffData.SURFACE_RELATIVE_GROUND_SPEED, 3) },
                            { "WindHdg", Math.Round(TakeoffData.AMBIENT_WIND_DIRECTION) },
                            { "WindSpeed", Math.Round(TakeoffData.AMBIENT_WIND_VELOCITY) },
                            { "SimTimeOffset", TakeoffData.SIM_ZULU_OFFSET },
                            { "SimTimeZulu", Utils.TimeStamp(TakeoffData.SIM_ZULU_TIME) },
                            { "FPS", Math.Round(SimFrameAvg, 4) },
                        }
                    }, false);
                    Console.WriteLine("Added latest Takeoff");
                    GoogleAnalyticscs.TrackEvent("Aircraft", "Takeoff", Connectors.SimConnection.Aircraft.Name, (int)Math.Round(Session.TravelDistance));

                    MaxYAccel = 0;
                    MaxFPM = 0;
                    LandingData = null;
                    TakeoffData = null;
                }
            }
        }

        public bool GetBufferState()
        {
            return AccelBufferStart == 0;
        }
    }
}
