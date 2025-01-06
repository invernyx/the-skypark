using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using TSP_Transponder.Models.EventBus;
using TSP_Transponder.Models.WeatherModel;
using static TSP_Transponder.App;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Aircraft;
using System.Globalization;
using static TSP_Transponder.Attributes.EnumAttr;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.WorldManager;

namespace TSP_Transponder.Models.Connectors
{
    public class SimConnection
    {
        internal static Process ActiveSimProc = null;
        internal static uint ActiveSimVersionMajor = 0;
        internal static uint ActiveSimVersionMinor = 0;
        internal static SimLibrary.Simulator ActiveSim = null;
        internal static bool PreviousSessionCrashed = false;
        internal static List<IntPtr> ActiveSimWindows = new List<IntPtr>();
        internal static TemporalData LastTemporalData = new TemporalData();
        internal static WeatherData LatestWeatherData = null;
        internal static IntPtr SimulationWindow = IntPtr.Zero;
        internal static bool SimHasSound = true;
        internal static bool SimHasFocus = false;
        internal static bool IsRunning = false;
        internal static bool IsPaused = false;
        internal static bool IsFrozen = false;
        internal static double SimFrameAvg = 0;
        internal static AircraftInstance Aircraft = null;
        internal static bool LightboxState = false;
        internal static List<float> APIResponseTime = new List<float>();
        internal static IntPtr current_focus = IntPtr.Zero;
        internal static Label ConnectionIndicator = null;
        internal static double LastSent = 0;
        internal static double LastReceived = 0;
        internal static CAMERA_MODE CameraMode = CAMERA_MODE.VC;
        internal static ConnectorInstance_Base ConnectedInstance = null;
        internal static List<ConnectorInstance_Base> Connectors = new List<ConnectorInstance_Base>();

        private static bool _IsLoaded = false;
        internal static bool IsLoaded {
            get
            {
                return _IsLoaded;
            }
            set
            {
                if(_IsLoaded != value)
                {
                    _IsLoaded = value;
                    if(ActiveSim != null)
                    {
                        ActiveSim.Connector.SimLoaded(value);
                    }
                }
            }
        }


        private static IntRect ClientRect = new IntRect();
        private static IntRect winRect = new IntRect();
        private static WinEventDelegate windowGotFocusDel = null;
        //internal static IntPtr m_hook_focus = IntPtr.Zero;
        internal static IntPtr m_hook_move = IntPtr.Zero;

        //private static ManagementEventWatcher proc_watch_stop;

        internal enum CAMERA_MODE
        {
            [EnumValue("Outside")]
            Outside,
            [EnumValue("Panel")]
            Panel,
            [EnumValue("VC")]
            VC,
            [EnumValue("Isometric")]
            Isometric,
        }

        internal static void Startup(MainWindow MW)
        {
            foreach (ConnectorInstance_Base Connector in Connectors)
            {
                Connector.Startup(MW);
            }

            #region Window Resize/Focus
            windowGotFocusDel = new WinEventDelegate(WindowGotFocus);
            //m_hook_focus = SetWinEventHook(0x0003, 0x0003, IntPtr.Zero, windowGotFocusDel, 0, 0, 0);

            //windowResizedDel = new WinEventDelegate(WindowResized);
            //m_hook_move = SetWinEventHook(0x800B, 0x800B, IntPtr.Zero, windowResizedDel, 0, 0, 0);
            #endregion
            
            //proc_watch_stop = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            //proc_watch_stop.EventArrived += new EventArrivedEventHandler(StopWatch_EventArrived);
            //proc_watch_stop.Start();
        }

        internal static void Connect()
        {
            if(ConnectedInstance == null)
            {
                Thread scThread = new Thread(() =>
                {
                    List<Type> Tryied = new List<Type>();
                    foreach (ConnectorInstance_Base Connector in Connectors)
                    {
                        if (!Tryied.Contains(Connector.GetType()))
                        {
                            Tryied.Add(Connector.GetType());
                            Connector.Connect();
                        }
                    }
                })
                {
                    IsBackground = true
                };
                scThread.CurrentCulture = CultureInfo.CurrentCulture;
                scThread.Start();
            }
        }

        internal static void Connected(ConnectorInstance_Base Instance)
        {
            ConnectedInstance = Instance;
            //ActiveSim.Addons.ScanScenery();

            //if (!PreviousSessionCrashed)
            //{
            //    Audio.AudioFramework.GetSpeech("BRIGIT", "WELCOME");
            //}

            PreviousSessionCrashed = false;
            MW.CreateSimConnectionIndicator("indicator_sim");
            MW.SetConnectedSimIndicator(ActiveSim.Platform + (ActiveSim.MajorVersion != 0 ? " v" + ActiveSim.MajorVersion : ""));
            //if (UserData.Get("launch_skypad") == "1")
            //{
            //    MW.LaunchSkypad();
            //}

            if (!APIBase.WSRemote.IsAlive)
            {
                APIBase.WSRemote.Connect();
            }

            BroadcastStatus();

            World.Connect();
            RichPresence.Update();
        }

        internal static void Disconnected()
        {
            AdventuresBase.Disconnect();

            ConnectedInstance = null;
            RefusedAdminRestart = false;
            MW.SetConnectedSimIndicator("No Simulator");
            MW.DestroyConnectionIndicator(ConnectionIndicator);
            
            EventBus.EventManager.ResetSession();
            FleetService.Disconnect(Aircraft);

            ActiveSimWindows.Clear();
            LightboxState = false;
            LastTemporalData = new TemporalData();
            SimulationWindow = IntPtr.Zero;
            APIResponseTime.Clear();
            Aircraft = null;
            ActiveSimProc = null;
            ActiveSim = null;

            BroadcastStatus();

            World.Disconnect();
            RichPresence.Update();
        }

        public static void BroadcastStatus()
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "State", ActiveSim != null && ConnectedInstance != null },
            };

            if (ActiveSim != null && ConnectedInstance != null)
            {
                ns.Add("Name", ActiveSim.Name);
                ns.Add("Version", ActiveSim.MajorVersion);
            }

            APIBase.ClientCollection.SendMessage("sim:status", JSSerializer.Serialize(ns), null, APIBase.ClientType.All);
        }

        public static void WSConnected(SocketClient Socket)
        {
            Dictionary<string, dynamic> ns = new Dictionary<string, dynamic>()
            {
                { "State", ActiveSim != null && ConnectedInstance != null },
            };
            
            if(ActiveSim != null && ConnectedInstance != null)
            {
                ns.Add("Name", ActiveSim.Name);
                ns.Add("Version", ActiveSim.MajorVersion);
            }
            
            if (Socket != null)
            {
                Socket.SendMessage("sim:status", JSSerializer.Serialize(ns));
            }
            else
            {
                APIBase.ClientCollection.SendMessage("sim:status", JSSerializer.Serialize(ns));
            }
        }

        internal static void ProcessPauseEvent()
        {
            if (IsPaused || !IsRunning && !LastTemporalData.IS_SLEW_ACTIVE)
            {
                EventBus.EventManager.Active.Timer.Stop();
                EventBus.EventManager.Active.AirTime.Stop();
                EventBus.EventManager.Active.BlockTime.Stop();
            }
            else
            {
                EventBus.EventManager.Active.Timer.Start();
            }
        }

        internal static void GetSimIsFocused()
        {
            IntPtr NewFocus = GetForegroundWindow();
            if(NewFocus != current_focus)
            {
                if (ActiveSimWindows.Contains(NewFocus))
                {
                    Console.WriteLine("Sim Got Focus (" + NewFocus + ")");
                    SimHasFocus = true;
                }
                current_focus = NewFocus;
            }
        }

        internal static void GetSimWindow()
        {
            if(ActiveSimProc != null)
            {
                if (SimulationWindow == IntPtr.Zero)
                {
                    Console.WriteLine("Getting Sim Window Info");
                    WINDOWINFO info;
                    StringBuilder message;
                    List<IntPtr> handles = EnumerateProcessWindowHandles(ActiveSimProc.Id).ToList();

                    if (Monitor.TryEnter(ActiveSimWindows, 1000))
                    {
                        bool refresh = false;
                        foreach (IntPtr wi in handles)
                        {
                            if (!ActiveSimWindows.Contains(wi))
                            {
                                refresh = true;
                            }
                        }

                        if (refresh)
                        {
                            ActiveSimWindows.Clear();
                            foreach (IntPtr handle in handles)
                            {
                                info = new WINDOWINFO();
                                info.cbSize = (uint)Marshal.SizeOf(info);
                                GetWindowInfo(handle, ref info);

                                message = new StringBuilder(1000);
                                SendMessage(handle, WM_GETTEXT, message.Capacity, message);
                                ActiveSimWindows.Add(handle);
                                string title = Convert.ToString(message);

                                Console.WriteLine(title + " /// " + info.dwExStyle);

                                if(title.StartsWith("Microsoft Flight Simulator"))
                                {
                                    SimulationWindow = handle;
                                    //break;
                                }

                                switch (info.dwExStyle)
                                {
                                    case 537135360:
                                    case 537135104:
                                        {
                                            SimulationWindow = handle;
                                            break;
                                        }
                                    case 536872960:
                                        {
                                            Console.WriteLine("Loading Dialog");
                                            break;
                                        }
                                }
                                
                            }
                        }

                        Monitor.Exit(ActiveSimWindows);
                    }
                }
            }
        }
        
        internal static Rect CalculateSimWindowInnerCoords()
        {
            System.Drawing.Point p = new System.Drawing.Point(0, 0);
            ClientToScreen(SimulationWindow, ref p);
            GetClientRect(SimulationWindow, ref ClientRect);
            GetWindowRect(SimulationWindow, ref winRect);

            return new Rect(p.X, p.Y, ClientRect.Right, ClientRect.Bottom);
        }

        internal static Rect CalculateSimWindowOutterCoords()
        {
            System.Drawing.Point p = new System.Drawing.Point(0, 0);
            ClientToScreen(SimulationWindow, ref p);
            GetClientRect(SimulationWindow, ref ClientRect);
            GetWindowRect(SimulationWindow, ref winRect);

            return new Rect(winRect.Left, winRect.Top, winRect.Right - winRect.Left, winRect.Bottom - winRect.Top);
        }
        
        private static void WindowGotFocus(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (IsDead)
            {
                return;
            }

            if (ActiveSimWindows.Contains(hwnd))
            {
                SimHasFocus = true;
                Console.WriteLine("Sim Got Focus (" + hwnd + ")");
            }
            else
            {
                SimHasFocus = false;
                if (ActiveSimWindows.Contains(current_focus))
                {
                    Console.WriteLine("Sim had Focus");
                }
            }

            current_focus = hwnd;
        }

        private static void WindowResized(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if(hwnd != IntPtr.Zero)
            {
            }
        }

        //private static void StopWatch_EventArrived(object sender, EventArrivedEventArgs e)
        //{
        //    if (ActiveSim != null)
        //    {
        //        string procName = ((string)e.NewEvent.Properties["ProcessName"].Value).ToLower().Replace(".exe", "").Replace(".", "");
        //        if (ActiveSim.Exe.Replace(".exe", "").Replace(".", "") == procName)
        //        {
        //
        //        }
        //    }
        //}
                
        public class TemporalData
        {
            public double FRAME_DIST = 0;
            public double ENGINE_TYPE = 0;
            public double EMPTY_WEIGHT_KG = 0;
            public double AIRSPEED_TRUE = 0;
            public double SURFACE_RELATIVE_GROUND_SPEED = 0;
            public double PLANE_PITCH_DEGREES = 0;
            public double PLANE_BANK_DEGREES = 0;
            public double VERTICAL_SPEED = 0;
            public GeoLoc PLANE_LOCATION = new GeoLoc(0,0);
            public double PLANE_ALTITUDE = 0;
            public double INDICATED_ALTITUDE = 0;
            public double PLANE_ALT_ABOVE_GROUND = 0;
            public double PLANE_HEADING_DEGREES = 0;
            public double PLANE_MAGVAR = 0;
            public double PLANE_COURSE = 0;
            public double PLANE_TURNRATE = 0;
            public double INCIDENCE_ALPHA = 0;
            public double INCIDENCE_BETA = 0;
            public double ACCELERATION_BODY_X = 0;
            public double ACCELERATION_BODY_Y = 0;
            public double ACCELERATION_BODY_Z = 0;
            public double VELOCITY_BODY_X = 0;
            public double VELOCITY_BODY_Y = 0;
            public double VELOCITY_BODY_Z = 0;
            public double G_FORCE = 0;
            public bool GENERAL_ENG_COMBUSTION = false;
            public bool GENERAL_ENG_COMBUSTION_1 = false;
            public bool GENERAL_ENG_COMBUSTION_2 = false;
            public bool GENERAL_ENG_COMBUSTION_3 = false;
            public bool GENERAL_ENG_COMBUSTION_4 = false;
            public double AIRCRAFT_WIND_X = 0;
            public double AIRCRAFT_WIND_Z = 0;
            public double AMBIENT_WIND_DIRECTION = 0;
            public double AMBIENT_WIND_VELOCITY = 0;
            public double FUEL_TOTAL_CAPACITY_LITERS = 0;
            public double CG_AFT_LIMIT = 0;
            public double CG_FWD_LIMIT = 0;
            public double CG_PERCENT = 0;
            public double CG_PERCENT_LATERAL = 0;
            public bool STALL_WARNING = false;
            public bool OVERSPEED_WARNING = false;
            public double LIGHT_ON_STATES = 0;
            public bool AUTOPILOT_MASTER = false;
            public double GEAR_CENTER_POSITION = 0;
            public double GEAR_LEFT_POSITION = 0;
            public double GEAR_RIGHT_POSITION = 0;
            public double GEAR_TAIL_POSITION = 0;
            public double GEAR_AUX_POSITION = 0;
            public double CENTER_WHEEL_RPM = 0;
            public double LEFT_WHEEL_RPM = 0;
            public double RIGHT_WHEEL_RPM = 0;
            public double AUX_WHEEL_RPM = 0;
            public bool SIM_ON_GROUND = false;
            public long AMBIENT_VISIBILITY = 0;
            public int AMBIENT_PRECIP_STATE = -1;
            public int AMBIENT_PRECIP_RATE = -1;
            public double BAROMETER_PRESSURE = 0;
            public bool AMBIENT_IN_CLOUD = false;
            public double AMBIENT_TEMPERATURE = 0;
            public int SURFACE_CONDITION = -1;
            public bool IS_SLEW_ACTIVE = false;
            public DateTime SIM_LOCAL_TIME = new DateTime();
            public DateTime SIM_ZULU_TIME = new DateTime();
            public double SIM_ZULU_OFFSET = 0;
            public DateTime SYS_TIME = new DateTime();
            public DateTime SYS_ZULU_TIME = new DateTime();
            public double APP_RUNTIME = 0;
            public double ABS_TIME = 0;
            public double FUEL_QUANTITY = 0;
            public double TOTAL_WEIGHT = 0;
            public bool EXIT = false;
            public double EXIT_0 = 0;
            public double EXIT_1 = 0;
            public double EXIT_2 = 0;
            public double EXIT_3 = 0;
            public double EXIT_4 = 0;
            public List<float> PAYLOAD = new List<float>();

            public int CAMERA_STATE = -1;
            public Surface SURFACE_TYPE = Surface.Unknown;

            public bool AP_ON = false;
            public bool AP_HDG_ON = false;
            public double AP_HDG = 0;

            public TemporalData Copy()
            {
                TemporalData copy = (TemporalData)this.MemberwiseClone();
                copy.PLANE_LOCATION = new GeoLoc(copy.PLANE_LOCATION.Lon, copy.PLANE_LOCATION.Lat);
                copy.SIM_LOCAL_TIME = new DateTime(copy.SIM_LOCAL_TIME.Ticks);
                copy.SIM_ZULU_TIME = new DateTime(copy.SIM_ZULU_TIME.Ticks);
                copy.SYS_TIME = new DateTime(copy.SYS_TIME.Ticks);
                copy.SYS_ZULU_TIME = new DateTime(copy.SYS_ZULU_TIME.Ticks);
                copy.PAYLOAD = new List<float>(copy.PAYLOAD);
                return copy;
            }

            public void SetPayload(int Station, float WeightKG)
            {
                if (Aircraft != null)
                {
                    if (Station < Aircraft.PayloadStationCount)
                    {
                        lock (PAYLOAD)
                        {
                            while (PAYLOAD.Count <= Station)
                            {
                                PAYLOAD.Add(0);
                            }

                            PAYLOAD[Station] = WeightKG;
                            if(Aircraft.PayloadStations != null)
                            {
                                Aircraft.PayloadStations[Station].Load = WeightKG;
                            }
                        }
                    }
                }

            }
        }
        
        public enum Surface
        {
            [EnumValue("Concrete")]
            Concrete,
            [EnumValue("Grass")]
            Grass,
            [EnumValue("Water")]
            Water,
            [EnumValue("Asphalt")]
            Asphalt,
            [EnumValue("Clay")]
            Clay,
            [EnumValue("Snow")]
            Snow,
            [EnumValue("Ice")]
            Ice,
            [EnumValue("Dirt")]
            Dirt,
            [EnumValue("Coral")]
            Coral,
            [EnumValue("Gravel")]
            Gravel,
            [EnumValue("OilTreated")]
            OilTreated,
            [EnumValue("SteelMats")]
            SteelMats,
            [EnumValue("Bituminous")]
            Bituminous,
            [EnumValue("Brick")]
            Brick,
            [EnumValue("Macadam")]
            Macadam,
            [EnumValue("Planks")]
            Planks,
            [EnumValue("Sand")]
            Sand,
            [EnumValue("Shale")]
            Shale,
            [EnumValue("Tarmac")]
            Tarmac,
            [EnumValue("Unknown")]
            Unknown,
        }

        #region Externs
        private const uint WM_GETTEXT = 0x000D;
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWINFO
        {
            public uint cbSize;
            public Rect rcWindow;
            public Rect rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler) : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

        }

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        private static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();
            try
            {
                foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
                    EnumThreadWindows(thread.Id,
                        (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }
            catch
            {
                Disconnected();
            }
            return handles;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref IntRect lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hwnd, ref IntRect rectangle);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        //[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        //public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        #endregion
    }
}
