using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Utilities
{
    
    class P42StopWatch
    {
        private string _name = "";
        private bool _isRunning = false;
        private double _runtime = 0;
        private double _startTime = 0;
        private double _offset = 0;
        private double _playbackSpeedOffset = 0;
        private Stopwatch _mainStopwatch = new Stopwatch();
        private double nanosecondsMath = Stopwatch.Frequency / 1000;

        public P42StopWatch(string name = "")
        {
            _name = name;
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public double StartTime
        {
            get
            {
                return _startTime;
            }
        }

        public double Offset
        {
            get
            {
                return _offset;
            }
        }

        public double ElapsedTicks
        {
            get
            {
                if (!_isRunning)
                {
                    return _runtime;
                }
                else
                {
                    _runtime = (_mainStopwatch.ElapsedTicks - _startTime) + (_mainStopwatch.ElapsedTicks * _playbackSpeedOffset);
                    return _runtime;
                }
            }
        }

        private void SetElapsed()
        {
            _runtime = ((_mainStopwatch.ElapsedTicks - _startTime) / nanosecondsMath) + (_mainStopwatch.ElapsedTicks * _playbackSpeedOffset / nanosecondsMath);
        }

        public double ElapsedMilliseconds
        {
            get
            {
                if (!_isRunning)
                {
                    return _runtime;
                }
                else
                {
                    SetElapsed();
                    return _runtime;
                }
            }
        }

        public double PlaybackSpeed
        {
            get
            {
                return _playbackSpeedOffset + 1;
            }
            set
            {
               // Console.WriteLine("Set Playback Speed " + _name + " to " + value);
                Set(_runtime, !_isRunning);
                _playbackSpeedOffset = value - 1;
            }
        }

        public void Start()
        {
            if (!_isRunning)
            {
                //Console.WriteLine("Start Timer " + _name);
                _mainStopwatch.Start();
                _isRunning = true;
                SetElapsed();
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                //Console.WriteLine("Stop Timer " + _name);
                _isRunning = false;
                _mainStopwatch.Stop();
            }
            SetElapsed();
        }

        public void Reset()
        {
            //Console.WriteLine("Reset Timer " + _name);
            _isRunning = false;
            _mainStopwatch.Reset();
            _startTime = 0;
            _runtime = 0;
        }

        public void Restart()
        {
            //Console.WriteLine("Restart Timer " + _name);
            _isRunning = true;
            _mainStopwatch.Restart();
            _startTime = 0;
            _runtime = 0;
        }

        public void Set(double time, bool pause = false)
        {
            _startTime = -time * nanosecondsMath;
            _runtime = _startTime;

            if (pause)
            {
                _isRunning = false;
                _mainStopwatch.Reset();
            }
            else
            {
                _isRunning = true;
                _mainStopwatch.Restart();
            }

            SetElapsed();

        }
    }
}
