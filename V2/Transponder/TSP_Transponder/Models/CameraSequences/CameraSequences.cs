using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Connectors;
using static TSP_Transponder.Utils;

namespace TSP_Transponder.Models.CameraSequences
{

    internal static class SequenceProcess
    {
        internal static Stopwatch Timer = new Stopwatch();
        private static List<Sequence> Sequences = new List<Sequence>();
        private static int SequenceIndex = 0;

        private static Sequence ActiveSequence = null;
        private static long SequenceTime = 0;
        private static CameraPosition SequencePos = null;

        private static Sequence UpcomingSequence = null;
        private static long SequenceUpcomingTime = 0;
        private static CameraPosition SequenceUpcomingPos = null;

        internal static void Create(CameraSequence type, List<Sequence> _Sequences)
        {            
            Sequences = _Sequences;
            ActiveSequence = Sequences[0];
            UpcomingSequence = null;
            SequenceIndex = 0;
            SequenceTime = 0;
            Timer.Reset();

            if(ActiveSequence.EventStart != null)
            {
                ActiveSequence.EventStart.DynamicInvoke(ActiveSequence, _Sequences);
            }

            //Sequences[SequenceIndex + 1].Start = ActiveSequence.End;
            Sequence prev = null;
            foreach (Sequence seq in _Sequences)
            {
                if(prev != null)
                {
                    if(seq.Start == null)
                    {
                        seq.Start = prev.End;
                    }
                }
                seq.Init();
                prev = seq;
            }

        }
        
        internal static CameraPosition Process()
        {
            double CurTime = Timer.ElapsedMilliseconds;
            while (CurTime - SequenceTime > ActiveSequence.Duration)
            {
                if (Sequences.Count <= SequenceIndex + 1)
                {
                    return null;
                }

                if (ActiveSequence.EventEnd != null)
                {
                    ActiveSequence.EventEnd.DynamicInvoke(ActiveSequence, Sequences);
                }

                UpcomingSequence = null;
                SequenceTime = (long)(CurTime - ActiveSequence.OverlapNext);
                ActiveSequence = Sequences[SequenceIndex + 1];
                SequenceUpcomingTime = 0;
                SequenceIndex++;

                if (UpcomingSequence == null)
                {
                    if (ActiveSequence.EventStart != null)
                    {
                        ActiveSequence.EventStart.DynamicInvoke(ActiveSequence, Sequences);
                    }
                }
            }

            if (CurTime - SequenceTime > ActiveSequence.Duration - ActiveSequence.OverlapNext)
            {
                if (Sequences.Count > SequenceIndex + 1)
                {
                    SequenceUpcomingTime = (long)(CurTime - ActiveSequence.OverlapNext);
                    UpcomingSequence = Sequences[SequenceIndex + 1];

                    if (UpcomingSequence.EventStart != null)
                    {
                        UpcomingSequence.EventStart.DynamicInvoke(UpcomingSequence, Sequences);
                    }
                }
            }

            double overlapPercent = 0;
            double overlapUpcoming = 0;
            double percent = (1 / ActiveSequence.Duration) * (CurTime - SequenceTime);

            if (UpcomingSequence != null)
            {
                overlapPercent = (1 / ActiveSequence.OverlapNext) * (CurTime - SequenceTime - ActiveSequence.Duration + ActiveSequence.OverlapNext);
                overlapUpcoming = ((1 / UpcomingSequence.Duration) * ActiveSequence.OverlapNext) * overlapPercent;
                overlapPercent = Easings.Interpolate(overlapPercent, Easings.EasingFunctions.CubicEaseInOut);
            }
            
            SequencePos = ActiveSequence.Calculate(percent);
            if (UpcomingSequence != null)
            {
                SequenceUpcomingPos = UpcomingSequence.Calculate(overlapUpcoming);
                return new CameraPosition()
                {
                    X = (SequencePos.X * (1 - overlapPercent)) + (SequenceUpcomingPos.X * overlapPercent),
                    Y = (SequencePos.Y * (1 - overlapPercent)) + (SequenceUpcomingPos.Y * overlapPercent),
                    Z = (SequencePos.Z * (1 - overlapPercent)) + (SequenceUpcomingPos.Z * overlapPercent),
                    P = (SequencePos.P * (1 - overlapPercent)) + (SequenceUpcomingPos.P * overlapPercent),
                    B = (SequencePos.B * (1 - overlapPercent)) + (SequenceUpcomingPos.B * overlapPercent),
                    H = (SequencePos.H * (1 - overlapPercent)) + (SequenceUpcomingPos.H * overlapPercent),
                    F = (SequencePos.F * (1 - overlapPercent)) + (SequenceUpcomingPos.F * overlapPercent),
                };
            }
            else
            {
                return SequencePos;
            }
            
        }
    }

    internal class Sequence
    {
        internal float Duration = 1000f;
        internal float OverlapNext = 0f;
        internal CameraPosition Start = null;
        internal CameraPosition End = new CameraPosition();
        internal Axis X = new Axis();
        internal Axis Y = new Axis();
        internal Axis Z = new Axis();
        internal Axis P = new Axis();
        internal Axis B = new Axis();
        internal Axis H = new Axis();
        internal Axis F = new Axis();
        internal Easings.EasingFunctions Easing = Easings.EasingFunctions.Linear;
        internal Action<Sequence, List<Sequence>> EventStart = null;
        internal Action<Sequence, List<Sequence>> EventEnd = null;

        internal void Init()
        {
            X.Init(this);
            Y.Init(this);
            Z.Init(this);
            P.Init(this);
            B.Init(this);
            H.Init(this);
            F.Init(this);
        }

        internal CameraPosition Calculate(double percent)
        {
            CameraPosition newCamera = new CameraPosition()
            {
                X = X.Calculate(Start.X, End.X, percent),
                Y = Y.Calculate(Start.Y, End.Y, percent),
                Z = Z.Calculate(Start.Z, End.Z, percent),
                P = P.Calculate(Start.P, End.P, percent),
                B = B.Calculate(Start.B, End.B, percent),
                H = H.Calculate(Start.H, End.H, percent),
                F = F.Calculate(Start.F, End.F, percent),
            };

            //Console.WriteLine(newCamera.ToString());
            return newCamera;
        }

        internal class Axis
        {
            internal Easings.EasingFunctions Easing = Easings.EasingFunctions.None;
            private Sequence Sequence = null;

            internal void Init(Sequence _Sequence)
            {
                Sequence = _Sequence;
                if(Easing == Easings.EasingFunctions.None)
                {
                    Easing = Sequence.Easing;
                }
            }

            internal double Calculate(double start, double end, double percent)
            {
                double range = (end - start);
                double easedpercent = Easings.Interpolate(percent, Easing);
                return (range * easedpercent) + start;
            }

        }

    }

    internal static class SequenceLib
    {
        internal static List<Sequence> GetSequence(CameraSequence type)
        {
            switch (type)
            {
                default: return new List<Sequence>();
                case CameraSequence.Introduction:
                    {
                        AircraftInstance acf = SimConnection.Aircraft;
                        double ZSize = Math.Abs(acf.LocationFront.Z) + Math.Abs(acf.LocationTail.Z);
                        double ZCenter = (acf.LocationTail.Z + acf.LocationFront.Z) / 2;

                        return new List<Sequence>()
                        {
                            new Sequence() // Top down arrival
                            {
                                Duration = 8000,
                                Easing = Easings.EasingFunctions.QuadraticEaseOut,
                                EventStart = (seq, seqs) =>
                                {

                                },
                                EventEnd = (seq, seqs) =>
                                {

                                },
                                Start = new CameraPosition() {
                                    X = 0,
                                    Y = (acf.WingspanMeters * 1),
                                    Z = (acf.LocationFront.Z * 2) + 15,
                                    P = -89.9,
                                    H = -180,
                                },
                                End = new CameraPosition() {
                                    X = 0,
                                    Y = (acf.WingspanMeters * 1),
                                    Z = (acf.LocationFront.Z * 1),
                                    P = -89.9,
                                    H = -180,
                                },
                            },

                            new Sequence() // Engine
                            {
                                Duration = 4000,
                                Start = new CameraPosition() {
                                    X = (acf.EngineLocation1.X) - (acf.WingspanMeters * 0.05) - 1,
                                    Y = (acf.EngineLocation1.Y) + 3,
                                    Z = (acf.EngineLocation1.Z) + (acf.WingspanMeters * 0.2) + 1,
                                    P = -25,
                                    H = 150,
                                },
                                End = new CameraPosition() {
                                    X = (acf.EngineLocation1.X) - 1,
                                    Y = (acf.EngineLocation1.Y) + 2.5,
                                    Z = (acf.EngineLocation1.Z) + (acf.WingspanMeters * 0.2) + 2,
                                    P = -20,
                                    H = 170,
                                },
                            },

                            new Sequence() // Tail swoop
                            {
                                Duration = 8000,
                                Easing = Easings.EasingFunctions.SineEaseIn,
                                OverlapNext = 5000,
                                X = new Sequence.Axis() { Easing = Easings.EasingFunctions.Linear },
                                B = new Sequence.Axis() { Easing = Easings.EasingFunctions.Linear },
                                Start = new CameraPosition(){
                                    X = (acf.LocationTail.Y * 1.5) + 1,
                                    Y = (-SimConnection.LastTemporalData.PLANE_PITCH_DEGREES * acf.LocationTail.Z * 0.01) + (acf.LocationTail.Y * 0.9),
                                    Z = (acf.LocationTail.Z * 0.9),
                                    P = -10,
                                    B = -10,
                                    H = -80,
                                },
                                End = new CameraPosition() {
                                    X = (acf.WingspanMeters * 1.5),
                                    Y = (acf.WingspanMeters * 0.3),
                                    P = -10,
                                    B = 0,
                                    H = -80,
                                },
                            },
                            new Sequence()
                            {
                                Duration = 8000,
                                Easing = Easings.EasingFunctions.CubicEaseOut,
                                EventEnd = (seq, seqs) =>
                                {

                                },
                                End = new CameraPosition() {
                                    X = (ZSize * 0.5) + 2,
                                    Y = (ZSize * 0.3) + 1 + (ZCenter * 0.3),
                                    Z = (ZSize * 0.7) + 2 + (ZCenter * 0.5),
                                    P = -15,
                                    H = -140
                                },
                            },
                        };
                    }
            }
        }
    }

    internal class CameraPosition
    {
        internal double X;
        internal double Y;
        internal double Z;
        internal double P;
        internal double B;
        internal double H;
        internal double F;

        public override string ToString()
        {
            return "X" + X + " Y" + Y + " Z" + Z + " P" + P + " B" + B + " H" + H + " F" + F;
        }
    }

    internal enum CameraSequence
    {
        Introduction,
        JobComplete,
    }

}
