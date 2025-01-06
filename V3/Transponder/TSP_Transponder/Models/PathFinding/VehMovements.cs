using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.PathFinding
{
    
    public class VehMovement
    {
        #region Options
        public double MaxAccel {
            get {
                return MaxAccelCycle * TimerFPS;
            }
            set {
                MaxAccelCycle = value / TimerFPS;
            }
        }
        public double MaxDecel
        {
            get
            {
                return MaxDecelCycle * TimerFPS;
            }
            set
            {
                MaxDecelCycle = value / TimerFPS;
            }
        }
        public double MaxTurnDate
        {
            get
            {
                return MaxTurnDateCycle * TimerFPS;
            }
            set
            {
                MaxTurnDateCycle = value / TimerFPS;
            }
        }
        #endregion
        
        public List<Waypoint> Waypoints = new List<Waypoint>();
        public Action DoneAction = null;
        public bool Running = false;

        private Action<MovementState> LoopAction = null;
        private double MoveSpeed = 0;
        private double Heading = 0;
        private double SteerAngle = 0;
        private double SteerAngleToDestPrevious = 0;
        private GeoLoc PreviousPosition = new GeoLoc(0, 0);
        private GeoLoc CarrotPosition = new GeoLoc(0, 0);
        private GeoPosition VehPosition = new GeoPosition(0, 0);
        private MovementState OutputState = new MovementState()
        {
             Position = new GeoPosition(0, 0)
        };
        private Waypoint WPActive = null;

        private double MaxAccelCycle = 0;  // m/second/s
        private double MaxDecelCycle = 0;  // m/second/s
        private double MaxTurnDateCycle = 0;  // Deg/second

        private int TimerFPS = 60;
        private double TimerMS = 0;
        #region Timers

        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimeProc proc, int user, int mode);

        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        private delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        [StructLayout(LayoutKind.Sequential)]
        private struct TimerCaps
        {
            public int periodMin;
            public int periodMax;
        }

        private TimeProc callback;
        private int camTimerID = 0;

        private void SetTick(bool status)
        {
            if (status)
            {
                TimerCaps caps = new TimerCaps();
                timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
                int period = (int)Math.Round(1000.0/TimerFPS);
                int resolution = 0;
                int mode = 1; // 0 for periodic, 1 for single event
                callback = new TimeProc(MoveTick);
                camTimerID = timeSetEvent(period, resolution, callback, 0, mode);
            }
            else
            {
                timeKillEvent(camTimerID);
            }
        }
        #endregion

        public VehMovement()
        {
            TimerMS = 1000/TimerFPS;
        }

        public void AddWP(Waypoint Wps)
        {
            Waypoints.Add(Wps);
        }

        public void Stop()
        {
            if (Running)
            {
                Running = false;
                SetTick(false);
                DoneAction?.Invoke();
                Console.WriteLine("-------- Done with Vehicle Movements");
            }
        }

        public void Hold()
        {

        }

        public void Resume()
        {

        }
        
        public void Move(Action<MovementState> SendCommand)
        {
            Running = true;
            LoopAction = SendCommand;

            #region Calculate Distances
            double TotalDistance = 0;
            Waypoint Next = null;
            foreach (Waypoint wp in Waypoints)
            {
                Next = FindNextWP(wp);
                if (Next != null)
                {
                    double WPDist = Utils.MapCalcDist(wp.Location.Lat, wp.Location.Lon, Next.Location.Lat, Next.Location.Lon, Utils.DistanceUnit.Meters);
                    wp.TravelDistance = WPDist;
                    wp.StartDistance = TotalDistance;
                    TotalDistance += WPDist;
                }
                else
                {
                    wp.StartDistance = TotalDistance;
                    wp.Speed = 20;
                }
                
            }
            #endregion
            
            WPActive = Waypoints.First();
            Next = FindNextWP(WPActive);
            Heading = Utils.MapCalcBearing(WPActive.Location.Lat, WPActive.Location.Lon, Next.Location.Lat, Next.Location.Lon);
            PreviousPosition.Lon = WPActive.Location.Lon;
            PreviousPosition.Lat = WPActive.Location.Lat;
            SetTick(true);
        }

        public Waypoint FindPreviousWP(Waypoint wp)
        {
            int ind = Waypoints.IndexOf(wp) - 1;
            if(ind > 0)
            {
                return Waypoints[ind];
            }
            else
            {
                return null;
            }
        }

        public Waypoint FindNextWP(Waypoint wp)
        {
            int ind = Waypoints.IndexOf(wp) + 1;
            if (ind < Waypoints.Count)
            {
                return Waypoints[ind];
            }
            else
            {
                return null;
            }
        }
        
        private void MoveTick(int id, int msg, int user, int param1, int param2)
        {
            Action Proc = null;
            Proc = () =>
            {
                lock (Waypoints)
                {
                    // Set Percentage
                    #region Waypoint State Calculations
                    Waypoint WPNext = FindNextWP(WPActive);
                    Waypoint WPPrevious = FindPreviousWP(WPActive);
                    double SpeedRestraint = 1;

                    //Console.WriteLine("Sin: " + Math.Sin(Math.PI * (Utils.MapCalcBearing(VehPosition.Lat, VehPosition.Lon, WPNext.Location.Lat, WPNext.Location.Lon) / 180)));
                    if(WPNext != null)
                    {
                        double hdg = Utils.MapCompareBearings(Heading, Utils.MapCalcBearing(VehPosition.Lat, VehPosition.Lon, WPNext.Location.Lat, WPNext.Location.Lon));
                        SpeedRestraint = Utils.Limiter(0.02, 1, (Math.Cos(Math.PI * ((hdg) / 180))));
                    }

                    double NewPcnt = WPActive.Percent + (((1 / WPActive.TravelDistance) * ((MoveSpeed * 0.277778) / TimerFPS)) * SpeedRestraint);

                    if (WPActive.Percent > 1)
                    {
                        if (WPActive == Waypoints.Last())
                        {
                            WPActive.Percent = 1;
                            if(Utils.MapCalcDist(VehPosition.Lat, VehPosition.Lon, WPActive.Location.Lat, WPActive.Location.Lon, Utils.DistanceUnit.Meters) < 2)
                            {
                                WPActive.Speed = 0;
                                if (MoveSpeed < 0.1)
                                {
                                    Stop();
                                }
                            }
                        }
                        else
                        {
                            WPActive = FindNextWP(WPActive);
                            WPActive.Percent = NewPcnt - 1;
                            Proc();
                            return;
                        }
                    }
                    else
                    {
                        WPActive.Percent = NewPcnt;
                    }

                    double TraveledDistance = (WPActive.TravelDistance * WPActive.Percent) + WPActive.StartDistance;
                    #endregion

                    #region Speed Calculations
                    List<Waypoint> WaypointsAhead = Waypoints.FindAll(x => x.StartDistance > TraveledDistance && x.StartDistance < TraveledDistance + (MoveSpeed));
                    
                    double SpeedRestriction = WPActive.Speed;
                    foreach(Waypoint wp in WaypointsAhead)
                    {
                        if (SpeedRestriction > wp.Speed)
                        {
                            SpeedRestriction = wp.Speed;
                        }
                    }

                    double SpeedDif = (SpeedRestriction - MoveSpeed);
                    if(SpeedDif < 0)
                    {
                        MoveSpeed += (SpeedRestriction - MoveSpeed) * (0.3 / TimerMS); // Decelerate
                    }
                    else
                    {
                        MoveSpeed += (SpeedRestriction - MoveSpeed) * (0.05 / TimerMS); // Accelerate
                    }
                    #endregion

                    #region Find Target Position
                    if(WPNext != null)
                    {
                        CarrotPosition.Lon = WPActive.Location.Lon + ((WPNext.Location.Lon - WPActive.Location.Lon) * (WPActive.Percent));
                        CarrotPosition.Lat = WPActive.Location.Lat + ((WPNext.Location.Lat - WPActive.Location.Lat) * (WPActive.Percent));

                        CarrotPosition.Lon = PreviousPosition.Lon + (CarrotPosition.Lon - PreviousPosition.Lon);
                        CarrotPosition.Lat = PreviousPosition.Lat + (CarrotPosition.Lat - PreviousPosition.Lat);
                    }
                    
                    double TargetDistance = Utils.MapCalcDist(CarrotPosition.Lat, CarrotPosition.Lon, PreviousPosition.Lat, PreviousPosition.Lon, Utils.DistanceUnit.Kilometers);
                    double FrameTravelDistance = TargetDistance * ((MoveSpeed * 0.00002) * TimerMS);
                    #endregion

                    #region Find Steer Angle
                    double SteerAngleToDest = Utils.Limiter(-40, 40, Utils.MapCompareBearings(Heading, Utils.MapCalcBearing(VehPosition.Lat, VehPosition.Lon, CarrotPosition.Lat, CarrotPosition.Lon)));
                    double SteerAngleDif = ((SteerAngleToDest - SteerAngleToDestPrevious) * TimerMS);
                    double SteerSpeedLimit = (1 + (MoveSpeed * 0.0001)) * TimerMS;
                    
                    SteerAngle += (SteerAngleDif * SteerSpeedLimit);
                    
                    SteerAngleToDestPrevious = SteerAngleToDest;
                    Heading += SteerAngle * ((FrameTravelDistance / 20) * TimerMS);
                    #endregion

                    #region Fine Vehicle Final position
                    GeoLoc NewPosition = Utils.MapOffsetPosition(PreviousPosition.Lon, PreviousPosition.Lat, FrameTravelDistance * 1000, Heading);
                    VehPosition.Lon = PreviousPosition.Lon + ((PreviousPosition.Lon - NewPosition.Lon));
                    VehPosition.Lat = PreviousPosition.Lat + ((PreviousPosition.Lat - NewPosition.Lat));
                    #endregion

                    #region Send Final Position
                    OutputState.Position.Lon = VehPosition.Lon;
                    OutputState.Position.Lat = VehPosition.Lat;
                    OutputState.Position.Alt = VehPosition.Alt;
                    OutputState.Position.Hdg = Utils.Normalize180(Heading);
                    OutputState.Steer = SteerAngle;
                    OutputState.Speed = MoveSpeed;
                    LoopAction(OutputState);
                    PreviousPosition = NewPosition;
                    #endregion
                }

            };
            Proc();

        }

        public enum WPType
        {
            Rolling,
            Stopped,
        }

        
        public class Waypoint
        {
            public WPType Type = WPType.Rolling;
            public GeoLoc Location = null;
            public double Speed = 5;
            public double StopDuration = 0;
            public double Percent = 0;
            public double TravelDistance = 0;
            public double StartDistance = 0;
            public GeoLoc ClosestOnSegment = null;
        }

        
        public class MovementState
        {
            public GeoPosition Position = null;
            public double Steer = 0;
            public double Speed = 0;
        }


    }
}
