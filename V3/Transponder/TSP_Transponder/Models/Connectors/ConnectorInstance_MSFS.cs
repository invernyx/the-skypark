using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
using System.Text.RegularExpressions;
using Microsoft.FlightSimulator.SimConnect;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.EventBus;
using TSP_Transponder.Models.WeatherModel;
using TSP_Transponder.Models.WorldManager;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Aircraft;
using static TSP_Transponder.Utils;
using static TSP_Transponder.Models.SimLibrary;
using static TSP_Transponder.Models.PathFinding.VehMovement;
using static TSP_Transponder.Models.WeatherModel.WeatherData;
using static TSP_Transponder.Models.Connectors.SimConnection;
using TSP_Transponder.Models;

namespace TSP_Transponder
{
    public class ConnectorInstance_MSFS : ConnectorInstance_Base
    {
        #region SimConnect Variables

        
        private SimConnect SimConnectModule;
        private enum GROUPID
        {
            FLAG = 2000000000,
        };
        private enum DATA_REQUESTS
        {
            Version,
            Sim_Frame_Data,
            Sim_1Sec_Data,
            Sim_10Sec_Data,
            Sim_CG_Data,
            Sim_Slow_AI_Data,
            AI_Release,
            Aircraft_Loaded,
            Aircraft_Init,
            Flight_Loaded,
            GetCameraPos,
            Environment_Wx,
            Sim_Version,
            Payload_1 = 1001,
            Payload_2 = 1002,
            Payload_3 = 1003,
            Payload_4 = 1004,
            Payload_5 = 1005,
            Payload_6 = 1006,
            Payload_7 = 1007,
            Payload_8 = 1008,
            Payload_9 = 1009,
            Payload_10 = 1010,
            Payload_11 = 1011,
            Payload_12 = 1012,
            Payload_13 = 1013,
            Payload_14 = 1014,
            Payload_15 = 1015,
            Payload_16 = 1016,
            Payload_17 = 1017,
            Payload_18 = 1018,
            Payload_19 = 1019,
            Payload_20 = 1020,
            Payload_21 = 1021,
            Payload_22 = 1022,
            Payload_23 = 1023,
            Payload_24 = 1024,
            Payload_25 = 1025,
            Payload_26 = 1026,
            Payload_27 = 1027,
            Payload_28 = 1028,
            Payload_29 = 1029,
            Payload_30 = 1030,
            Payload_31 = 1031,
            Payload_32 = 1032,
            Payload_33 = 1033,
            Payload_34 = 1034,
            Payload_35 = 1035,
            Payload_36 = 1036,
            Payload_37 = 1037,
            Payload_38 = 1038,
            Payload_39 = 1039,
            Payload_40 = 1040,
            Payload_41 = 1041,
            Payload_42 = 1042,
            Payload_43 = 1043,
            Payload_44 = 1044,
            Payload_45 = 1045,
            Payload_46 = 1046,
            Payload_47 = 1047,
            Payload_48 = 1048,
            Payload_49 = 1049,
            Payload_50 = 1050,
            Payload_51 = 1051,
            Payload_52 = 1052,
            Payload_53 = 1053,
            Payload_54 = 1054,
            Payload_55 = 1055,
            Payload_56 = 1056,
            Payload_57 = 1057,
            Payload_58 = 1058,
            Payload_59 = 1059,
            Payload_60 = 1060,
            Payload_61 = 1061,
            Payload_62 = 1062,
            Payload_63 = 1063,
            Payload_64 = 1064,
            Payload_65 = 1065,
            Payload_66 = 1066,
            Payload_67 = 1067,
            Payload_68 = 1068,
            Payload_69 = 1069,
            Payload_70 = 1070,
            Payload_71 = 1071,
            Payload_72 = 1072,
            Payload_73 = 1073,
            Payload_74 = 1074,
            Payload_75 = 1075,
            Payload_76 = 1076,
            Payload_77 = 1077,
            Payload_78 = 1078,
            Payload_79 = 1079,
            Payload_80 = 1080,
            Payload_81 = 1081,
            Payload_82 = 1082,
            Payload_83 = 1083,
            Payload_84 = 1084,
            Payload_85 = 1085,
            Payload_86 = 1086,
            Payload_87 = 1087,
            Payload_88 = 1088,
            Payload_89 = 1089,
            Payload_90 = 1090,
            Payload_91 = 1091,
            Payload_92 = 1092,
            Payload_93 = 1093,
            Payload_94 = 1094,
            Payload_95 = 1095,
            Payload_96 = 1096,
            Payload_97 = 1097,
            Payload_98 = 1098,
            Payload_99 = 1099,
            Payload_100 = 1100,
            Payload_Calibration_1 = 1101,
            Payload_Calibration_2 = 1102,
            Payload_Calibration_3 = 1103,
            Payload_Calibration_4 = 1104,
            Payload_Calibration_5 = 1105,
            Payload_Calibration_6 = 1106,
            Payload_Calibration_7 = 1107,
            Payload_Calibration_8 = 1108,
            Payload_Calibration_9 = 1109,
            Payload_Calibration_10 = 1110,
            Payload_Calibration_11 = 1111,
            Payload_Calibration_12 = 1112,
            Payload_Calibration_13 = 1113,
            Payload_Calibration_14 = 1114,
            Payload_Calibration_15 = 1115,
            Payload_Calibration_16 = 1116,
            Payload_Calibration_17 = 1117,
            Payload_Calibration_18 = 1118,
            Payload_Calibration_19 = 1119,
            Payload_Calibration_20 = 1120,
            Payload_Calibration_21 = 1121,
            Payload_Calibration_22 = 1122,
            Payload_Calibration_23 = 1123,
            Payload_Calibration_24 = 1124,
            Payload_Calibration_25 = 1125,
            Payload_Calibration_26 = 1126,
            Payload_Calibration_27 = 1127,
            Payload_Calibration_28 = 1128,
            Payload_Calibration_29 = 1129,
            Payload_Calibration_30 = 1130,
            Payload_Calibration_31 = 1131,
            Payload_Calibration_32 = 1132,
            Payload_Calibration_33 = 1133,
            Payload_Calibration_34 = 1134,
            Payload_Calibration_35 = 1135,
            Payload_Calibration_36 = 1136,
            Payload_Calibration_37 = 1137,
            Payload_Calibration_38 = 1138,
            Payload_Calibration_39 = 1139,
            Payload_Calibration_40 = 1140,
            Payload_Calibration_41 = 1141,
            Payload_Calibration_42 = 1142,
            Payload_Calibration_43 = 1143,
            Payload_Calibration_44 = 1144,
            Payload_Calibration_45 = 1145,
            Payload_Calibration_46 = 1146,
            Payload_Calibration_47 = 1147,
            Payload_Calibration_48 = 1148,
            Payload_Calibration_49 = 1149,
            Payload_Calibration_50 = 1150,
            Payload_Calibration_51 = 1151,
            Payload_Calibration_52 = 1152,
            Payload_Calibration_53 = 1153,
            Payload_Calibration_54 = 1154,
            Payload_Calibration_55 = 1155,
            Payload_Calibration_56 = 1156,
            Payload_Calibration_57 = 1157,
            Payload_Calibration_58 = 1158,
            Payload_Calibration_59 = 1159,
            Payload_Calibration_60 = 1160,
            Payload_Calibration_61 = 1161,
            Payload_Calibration_62 = 1162,
            Payload_Calibration_63 = 1163,
            Payload_Calibration_64 = 1164,
            Payload_Calibration_65 = 1165,
            Payload_Calibration_66 = 1166,
            Payload_Calibration_67 = 1167,
            Payload_Calibration_68 = 1168,
            Payload_Calibration_69 = 1169,
            Payload_Calibration_70 = 1170,
            Payload_Calibration_71 = 1171,
            Payload_Calibration_72 = 1172,
            Payload_Calibration_73 = 1173,
            Payload_Calibration_74 = 1174,
            Payload_Calibration_75 = 1175,
            Payload_Calibration_76 = 1176,
            Payload_Calibration_77 = 1177,
            Payload_Calibration_78 = 1178,
            Payload_Calibration_79 = 1179,
            Payload_Calibration_80 = 1180,
            Payload_Calibration_81 = 1181,
            Payload_Calibration_82 = 1182,
            Payload_Calibration_83 = 1183,
            Payload_Calibration_84 = 1184,
            Payload_Calibration_85 = 1185,
            Payload_Calibration_86 = 1186,
            Payload_Calibration_87 = 1187,
            Payload_Calibration_88 = 1188,
            Payload_Calibration_89 = 1189,
            Payload_Calibration_90 = 1190,
            Payload_Calibration_91 = 1191,
            Payload_Calibration_92 = 1192,
            Payload_Calibration_93 = 1193,
            Payload_Calibration_94 = 1194,
            Payload_Calibration_95 = 1195,
            Payload_Calibration_96 = 1196,
            Payload_Calibration_97 = 1197,
            Payload_Calibration_98 = 1198,
            Payload_Calibration_99 = 1199,
            Payload_Calibration_100 = 1200,
        };
        private enum OBJECTS_REQUESTS
        {
            Delete,
            Lightbox,
            Scenr,
        };
        private enum EVENT_ID
        {
            Sim_State,
            Sim_Paused,
            Sim_Loaded,
            Sim_Sound,
            Menu_1,
            CameraMode,
            Frame,
            PositionChange,
            EVENT_FREEZE_ALT,
            EVENT_FREEZE_ATT,
            EVENT_FREEZE_POS,
            EVENT_SIM_PAUSED_SET,
            EVENT_SIM_SLEW_SET,
            ShowMessage,
        }
        private enum DEFINITIONS
        {
            LoopFrameStruct,
            Loop1SecStruct,
            Loop10SecStruct,
            AircraftStruct,
            AircraftCGStruct,
            SlowLoopAIStruct,
            VehMovementsWP,
            VehMovementsLoop,
            Payload_1 = 1001,
            Payload_2 = 1002,
            Payload_3 = 1003,
            Payload_4 = 1004,
            Payload_5 = 1005,
            Payload_6 = 1006,
            Payload_7 = 1007,
            Payload_8 = 1008,
            Payload_9 = 1009,
            Payload_10 = 1010,
            Payload_11 = 1011,
            Payload_12 = 1012,
            Payload_13 = 1013,
            Payload_14 = 1014,
            Payload_15 = 1015,
            Payload_16 = 1016,
            Payload_17 = 1017,
            Payload_18 = 1018,
            Payload_19 = 1019,
            Payload_20 = 1020,
            Payload_21 = 1021,
            Payload_22 = 1022,
            Payload_23 = 1023,
            Payload_24 = 1024,
            Payload_25 = 1025,
            Payload_26 = 1026,
            Payload_27 = 1027,
            Payload_28 = 1028,
            Payload_29 = 1029,
            Payload_30 = 1030,
            Payload_31 = 1031,
            Payload_32 = 1032,
            Payload_33 = 1033,
            Payload_34 = 1034,
            Payload_35 = 1035,
            Payload_36 = 1036,
            Payload_37 = 1037,
            Payload_38 = 1038,
            Payload_39 = 1039,
            Payload_40 = 1040,
            Payload_41 = 1041,
            Payload_42 = 1042,
            Payload_43 = 1043,
            Payload_44 = 1044,
            Payload_45 = 1045,
            Payload_46 = 1046,
            Payload_47 = 1047,
            Payload_48 = 1048,
            Payload_49 = 1049,
            Payload_50 = 1050,
            Payload_51 = 1051,
            Payload_52 = 1052,
            Payload_53 = 1053,
            Payload_54 = 1054,
            Payload_55 = 1055,
            Payload_56 = 1056,
            Payload_57 = 1057,
            Payload_58 = 1058,
            Payload_59 = 1059,
            Payload_60 = 1060,
            Payload_61 = 1061,
            Payload_62 = 1062,
            Payload_63 = 1063,
            Payload_64 = 1064,
            Payload_65 = 1065,
            Payload_66 = 1066,
            Payload_67 = 1067,
            Payload_68 = 1068,
            Payload_69 = 1069,
            Payload_70 = 1070,
            Payload_71 = 1071,
            Payload_72 = 1072,
            Payload_73 = 1073,
            Payload_74 = 1074,
            Payload_75 = 1075,
            Payload_76 = 1076,
            Payload_77 = 1077,
            Payload_78 = 1078,
            Payload_79 = 1079,
            Payload_80 = 1080,
            Payload_81 = 1081,
            Payload_82 = 1082,
            Payload_83 = 1083,
            Payload_84 = 1084,
            Payload_85 = 1085,
            Payload_86 = 1086,
            Payload_87 = 1087,
            Payload_88 = 1088,
            Payload_89 = 1089,
            Payload_90 = 1090,
            Payload_91 = 1091,
            Payload_92 = 1092,
            Payload_93 = 1093,
            Payload_94 = 1094,
            Payload_95 = 1095,
            Payload_96 = 1096,
            Payload_97 = 1097,
            Payload_98 = 1098,
            Payload_99 = 1099,
            Payload_100 = 1100,
        }
        private enum GROUP_ID
        {
            SET_TIME,
            SET_POS,
            ID_PRIORITY_STANDARD = 1900000000
        };
        private enum NOTIFICATION_GROUPS
        {
            GROUP_MENU,
            GROUP_KNEEBOARD,
        }

        private bool ConnectConfirmed = false;
        private long ConnectAttemptLast = 0;
        private long ConnectAttemptDelay = 30000;
        private object ParallelBusy = new object();
        
        private string InstallDir = "";
        private const int WM_USER_SIMCONNECT = 0x0402;
        private const string DEFAULT_SIMCONNECT_STATUS = "";
        private bool TryingConnect = false;
        private HwndSource HandleSource;
        private double LastAPIResponse = 0;
        private IntPtr Handle;
        private Stopwatch SimRuntime = new Stopwatch();
        private double LastSimFrame = 0;
        private List<double> FrameRates = new List<double>();
        private GeoLoc PreMenuPosition = new GeoLoc(0, 0);
        private List<KeyValuePair<float, string>> MessageQueue = new List<KeyValuePair<float, string>>();

        List<double> CRSDeltas = new List<double>();

        private string AircraftName = "";
        private string AircraftDirectory = "";
        private string AircraftDirectoryFull = "";

        private uint PayloadStationRemaining = 0;
        private uint PayloadStationsCount = 0;
        private DateTime? PayloadStationsInitialized = null;
        //private bool PayloadStationCalibrating = false;
        //private int PayloadStationCalibrationIndex = -1;
        //private uint PayloadStationCalibrationRemaining = 0;
        //private double PayloadStationCalibrationOffset = 0;
        //private List<float> PayloadStationCalibrationMemory = null;
        //private List<AircraftPayloadStation> PayloadStationCalibrationResults = null;
        //private AircraftInstance PayloadStationCalibrationAircraft = null;
        //private Action<List<AircraftPayloadStation>> PayloadStationCalibrationDone = null;

        public override bool IsConnected
        {
            get
            {
                try
                {
                    return SimConnectModule != null && ConnectConfirmed;
                }
                catch
                {
                    Disconnect();
                    return false;
                }
            }
        }
        #endregion


        public override void Startup(MainWindow _Window)
        {
            SimConnectModule = null;
            MW = _Window;

            Handle = new WindowInteropHelper(_Window).Handle;
            HandleSource = HwndSource.FromHwnd(Handle);
            ConnectAttemptLast = -ConnectAttemptDelay;
        }

        public override void Connect()
        {
            if (TryingConnect)
            {
                return;
            }
            TryingConnect = true;
        
            if (SimConnectModule == null)
            {
                if (ConnectedInstance != null)
                {
                    return;
                }

                try
                {
                    HandleSource.AddHook(HandleSimConnectEvents);
                    SimConnectModule = new SimConnect("The Skypark", Handle, WM_USER_SIMCONNECT, null, 0);
                    
                    LastSent = App.Timer.ElapsedMilliseconds;
                    foreach (Simulator sim in SimList)
                    {
                        if (sim.Connector.GetType() == typeof(ConnectorInstance_MSFS))
                        {
                            Process[] processes = Process.GetProcessesByName(sim.Exe.Replace(".exe", ""));
                            foreach (Process proc in processes)
                            {
                                if (proc.MainWindowTitle != string.Empty && proc.Id != 0 && proc.MainWindowHandle != IntPtr.Zero && !proc.HasExited)
                                {
                                    var versionInfo = FileVersionInfo.GetVersionInfo(proc.MainModule.FileName);
                                    int version = versionInfo.FileMajorPart;

                                    //if (sim.MajorVersion == version)
                                    //{
                                        ActiveSimProc = proc;
                                        ActiveSim = sim;
                                        SetupEventHandlers();
                                        Connected(this);
                                        ConnectConfirmed = true;
                                        Console.WriteLine("Connected to MSFS SimConnect");
                                        return;
                                    //}
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("MSFS SimConnect not ready: " + ex.Message + " / " + ex.StackTrace);
                }

                TryingConnect = false;
            }
            else
            {
                if (IsConnected)
                {
                    try
                    {
                        LastSent = App.Timer.ElapsedMilliseconds;
                        TryingConnect = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Previous Session Crashed " + ex.Message + Environment.NewLine + ex.StackTrace);
                        PreviousSessionCrashed = true;
                        Disconnect();
                    }
                }
                else
                {
                    Console.WriteLine("Error connecting to SimConnect");
                    Disconnect();
                    TryingConnect = false;
                }
            }
            
        }

        public override void Disconnect()
        {
            Console.WriteLine("Disconnect SimConnect");
            if (SimConnectModule != null)
            {
                try
                {
                    if (HandleSource != null)
                    {
                        HandleSource.RemoveHook(HandleSimConnectEvents);
                    }

                    SimConnectModule.Dispose();
                }
                catch
                {
                }
                
                SimConnectModule = null;
                Disconnected();

                SimRuntime.Stop();
                //ConnectConfirmed = false;
                IsLoaded = false;
                ConnectAttemptLast = 0;
                PayloadStationsInitialized = DateTime.UtcNow;
                IsRunning = false;
                IsPaused = false;
                TryingConnect = false;
                AircraftDirectory = "";

                AdventuresBase.ChangeAircraft(Aircraft, null);

                Console.WriteLine("SimConnect Closed");
            }
        }
        
        public override void SetPause(bool state)
        {
            LastSent = App.Timer.ElapsedMilliseconds;
            if (state)
            {
                SimConnectModule.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SIM_PAUSED_SET, 1, GROUP_ID.SET_POS, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
            }
            else
            {
                SimConnectModule.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SIM_PAUSED_SET, 0, GROUP_ID.SET_POS, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
            }
        }

        public override void MoveSimObject(uint id, MovementState State)
        {
            if (App.MW.IsShuttingDown)
            {
                return;
            }

            VehMovementState Init = new VehMovementState()
            {
                Latitude = State.Position.Lat,
                Longitude = State.Position.Lon,
                Heading = State.Position.Hdg,
            };

            try
            {
                lock (ParallelBusy)
                {
                    SimConnectModule.SetDataOnSimObject(DEFINITIONS.VehMovementsLoop, id, SIMCONNECT_DATA_SET_FLAG.DEFAULT, Init);
                }
            }
            catch
            {
            }
        }
        
        public override void CreateSimObject(long uid, string simobject, GeoPosition Loc, SceneObjType Type)
        {

            SIMCONNECT_DATA_INITPOSITION Init;
            Init.Altitude = Loc.Alt;
            Init.Latitude = Loc.Lat;
            Init.Longitude = Loc.Lon;
            Init.Pitch = 0;
            Init.Bank = 0;
            Init.Heading = Loc.Hdg;
            Init.Airspeed = 0;

            if (Loc.Alt != 0)
            {
                Init.OnGround = 0;
            }
            else
            {
                Init.Altitude = 100000;
                Init.OnGround = 1;
            }

            switch (Type)
            {
                case SceneObjType.Dynamic:
                    {
                        try
                        {
                            lock (ParallelBusy)
                            {
                                SimConnectModule.AICreateSimulatedObject(simobject, Init, (OBJECTS_REQUESTS)uid);
                            }
                        }
                        catch
                        {
                        }
                        break;
                    }
                case SceneObjType.Static:
                    {

                        try
                        {
                            lock (ParallelBusy)
                            {
                                SimConnectModule.AICreateNonATCAircraft(simobject, "idk", Init, (OBJECTS_REQUESTS)uid);
                            }
                        }
                        catch
                        {
                        }
                        break;
                    }
            }

        }

        public override void DestroySimObject(uint simID)
        {
            try
            {
                lock (ParallelBusy)
                {
                    SimConnectModule.AIRemoveObject(simID, OBJECTS_REQUESTS.Delete);
                }
            }
            catch
            {
            }
        }

        public override void SendMessage(string msg, float duration, bool force = false)
        {
            if(UserData.Get("sim_tips") == "0" && !force)
            {
                return;
            }

            lock(MessageQueue)
            {
                MessageQueue.Add(new KeyValuePair<float, string>(duration, msg));
                if (MessageQueue.Count > 1)
                {
                    return;
                }
            }

            Task.Factory.StartNew(() => {
                CultureInfo.CurrentCulture = App.CI;

                while (MessageQueue.Count > 0)
                {
                    if(SimConnectModule != null)
                    {
                        Console.WriteLine("Sending Message to sim: " + MessageQueue[0].Value);
                        SimConnectModule.Text(SIMCONNECT_TEXT_TYPE.SCROLL_BLACK, MessageQueue[0].Key, EVENT_ID.ShowMessage, MessageQueue[0].Value);
                        Thread.Sleep((int)(MessageQueue[0].Key * 1000));
                    }
                    lock(MessageQueue)
                    {
                        MessageQueue.RemoveAt(0);
                    }
                }
            }, App.ThreadCancel.Token);
        }

        public override void SendFlightPlan(string path)
        {
            SimConnectModule.FlightPlanLoad(path);
        }

        public override void MonitorAI(uint SimID)
        {
            //try
            //{
            //    if (IsConnected)
            //    {
            //        // Get Data at every 10 seconds
            //        lock (ParallelBusy)
            //        {
            //            SimConnectModule.RequestDataOnSimObject(
            //                DATA_REQUESTS.Sim_Slow_AI_Data,
            //                DEFINITIONS.SlowLoopAIStruct,
            //                SimID,
            //                SIMCONNECT_PERIOD.ONCE,
            //                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
            //                0,
            //                1,
            //                0
            //            );
            //        }
            //    }
            //}
            //catch
            //{
            //    Disconnect();
            //}

        }
        

        public override void ReadPackageConfig(Simulator Simulator, string DirectoryName, string DirectoryFull, Action<string, string, string, string> Callback)
        {
            foreach(string Dir in Simulator.ConfigsFolders)
            {
                try
                {
                    string optFilePath = Path.Combine(Dir, "UserCfg.opt");
                    if (File.Exists(optFilePath))
                    {
                        string installDir = GetInstallDir(optFilePath);
                        InstallDir = installDir;

                        List<string> Manifests = new List<string>(); //Directory.GetFiles(p, "aircraft.cfg", SearchOption.AllDirectories).ToList();


                        // Community
                        foreach (var package in Directory.GetDirectories(Path.Combine(installDir, "Community")))
                        {
                            string simobjects_path = Path.Combine(package, "SimObjects");
                            if (Directory.Exists(simobjects_path))
                            {
                                Manifests.AddRange(Directory.GetFiles(simobjects_path, "aircraft.cfg", SearchOption.AllDirectories).ToList());
                            }
                        }

                        // Officials
                        foreach (var package_group in Directory.GetDirectories(Path.Combine(installDir, "Official")))
                        {
                            foreach (var package in Directory.GetDirectories(package_group))
                            {
                                string simobjects_path = Path.Combine(package, "SimObjects");
                                if (Directory.Exists(simobjects_path))
                                {
                                    Manifests.AddRange(Directory.GetFiles(simobjects_path, "aircraft.cfg", SearchOption.AllDirectories).ToList());
                                }
                            }
                        }
                        
                        
                        //Manifests.AddRange(Directory.GetFiles(p, "aircraft.loc", SearchOption.AllDirectories).ToList());
                        List<string> AircraftCFGs = Manifests.FindAll(x => x.ToLower().Contains("\\" + DirectoryName.ToLower() + "\\")).ToList();

                        if (AircraftCFGs.Count > 0)
                        {
                            foreach (string AircraftCFG in AircraftCFGs)
                            {
                                string clearRefDir = DirectoryFull.Replace(DirectoryFull.Split('\\').Last(), "").ToLower().TrimEnd('\\');
                                FileInfo fi = new FileInfo(AircraftCFG);
                                
                                string packagePath = fi.Directory.FullName.ToLower().Replace(clearRefDir, "");
                                string manifestPath = Path.Combine(packagePath, "manifest.json");

                                if(File.Exists(manifestPath))
                                {
                                    // manifest.json
                                    string manifestJSON = File.ReadAllText(manifestPath);
                                    Dictionary<string, dynamic> manifestDict = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(manifestJSON);

                                    string Manufacturer = null;
                                    string Creator = null;
                                    string Model = null;
                                    if (manifestDict.ContainsKey("manufacturer"))
                                    {
                                        Manufacturer = manifestDict["manufacturer"];
                                    }

                                    if (manifestDict.ContainsKey("creator"))
                                    {
                                        Creator = manifestDict["creator"];
                                    }

                                    if (manifestDict.ContainsKey("title"))
                                    {
                                        Model = manifestDict["title"];
                                    }

                                    Callback(Manufacturer, Creator, Model, packagePath);
                                    return;
                                }
                            }
                        }

                        //DecompileCFG(ReadCFG(Aircraft.ConfigFilePath));
                    }

                    Callback(null, null, null, null);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to read Aircraft Config: " + ex.Message + " - " + ex.StackTrace);
                    Callback(null, null, null, null);
                }
            }
        }

        public override Bitmap CaptureImage(ImageFormat format, string savePath = null)
        {
            SetForegroundWindow(ActiveSimProc.MainWindowHandle);
            Thread.Sleep(100);
            Rect rect = CalculateSimWindowInnerCoords();
            Bitmap bmp = GetScreenshotFromWindow(rect);

            if (savePath != null)
            {
                bmp.Save(savePath, format);
            }

            return bmp;
        }

        public override Bitmap GetAircraftBitmap(AircraftInstance aircraft, string save_path)
        {
            string save_dir = Path.Combine(App.AppDataDirectory, "Fleet");

            if(!File.Exists(save_path))
            {
                string cfg_path = Path.Combine(aircraft.DirectoryPackage, aircraft.DirectoryFull);
                Console.WriteLine("Getting thumbnail for " + cfg_path);

                if(File.Exists(cfg_path))
                {
                    List<string> thumb_path = Directory.GetFiles(new FileInfo(cfg_path).Directory.FullName, "thumbnail.jpg", SearchOption.AllDirectories).ToList();

                    if (thumb_path.Count > 0)
                    {
                        Bitmap ImageBitmap = new Bitmap(ResizeImage(Image.FromFile(thumb_path.First()), 600));
                        if (!Directory.Exists(save_dir))
                            Directory.CreateDirectory(save_dir);

                        ImageBitmap.Save(save_path, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return ImageBitmap;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new Bitmap(Image.FromFile(save_path));
            }
            
        }


        public override void GetCG()
        {
            /*
            SimConnectModule.RequestDataOnSimObject(
                DATA_REQUESTS.Sim_CG_Data,
                DEFINITIONS.AircraftCGStruct,
                SimConnect.SIMCONNECT_OBJECT_ID_USER,
                SIMCONNECT_PERIOD.ONCE,
                SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                0,
                1,
                0
            );
            */
        }

        public override void CalibratePayloadStations(AircraftInstance Acf, Action<List<AircraftPayloadStation>> Done)
        {
            /*
            if(!PayloadStationCalibrating)
            {
                if (App.IsDev)
                {
                    PayloadStationCalibrating = true;
                    PayloadStationCalibrationMemory = new List<float>(LastTemporalData.PAYLOAD);
                    PayloadStationCalibrationResults = new List<AircraftPayloadStation>();
                    PayloadStationCalibrationIndex = -1;
                    PayloadStationCalibrationAircraft = Acf;
                    PayloadStationCalibrationDone = Done;

                    int i = 0;
                    while (i < PayloadStationCalibrationAircraft.PayloadStationCount)
                    {
                        SendPayload(i, 0);
                        PayloadStationCalibrationResults.Add(new AircraftPayloadStation());
                        i++;
                    }

                    GetCG();
                }
            }
            */
        }

        public override void SendPayload(int Station, float WeightKG)
        {
            if(IsConnected)
            {
                PayloadStruct existingPayload = new PayloadStruct()
                {
                    STATION = WeightKG,
                };

                SimConnectModule.SetDataOnSimObject((DEFINITIONS)(1001 + Station), SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, existingPayload);
            }
        }


        private void SetupEventHandlers()
        {
            try
            {
                LastSent = App.Timer.ElapsedMilliseconds;
                SimConnectModule.OnRecvOpen += Simconnect_OnRecvOpen;
                SimConnectModule.OnRecvQuit += Simconnect_OnRecvQuit;
                SimConnectModule.OnRecvException += Simconnect_OnRecvException;
                SimConnectModule.OnRecvEvent += Simconnect_OnRecvEvent;
                SimConnectModule.OnRecvEventFrame += SimConnect_OnRecvEventFrame;
                SimConnectModule.OnRecvSystemState += SimConnect_OnRecvSystemState;
                SimConnectModule.OnRecvWeatherObservation += Simconnect_OnRecvWeatherObservation;
                SimConnectModule.OnRecvSimobjectData += Simconnect_OnRecvSimobjectData;
                SimConnectModule.OnRecvAssignedObjectId += SimConnect_OnRecvAssignedObjectId;

                // Static Data
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "TITLE", null, SIMCONNECT_DATATYPE.STRING256, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "CATEGORY", null, SIMCONNECT_DATATYPE.STRING256, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "WING SPAN", "METERS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "STRUCT ENGINE POSITION:1", "FEET", SIMCONNECT_DATATYPE.XYZ, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "STRUCT ENGINE POSITION:2", "FEET", SIMCONNECT_DATATYPE.XYZ, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "STRUCT ENGINE POSITION:3", "FEET", SIMCONNECT_DATATYPE.XYZ, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "STRUCT ENGINE POSITION:4", "FEET", SIMCONNECT_DATATYPE.XYZ, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "FUEL TOTAL QUANTITY WEIGHT", "KILOGRAMS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "FUEL TOTAL CAPACITY", "LITERS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "EMPTY WEIGHT", "KILOGRAMS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "MAX GROSS WEIGHT", "KILOGRAMS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "ENGINE TYPE", "MASK", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "NUMBER OF ENGINES", "NUMBER", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftStruct, "PAYLOAD STATION COUNT", "NUMBER", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);

                // AI Aircraft Tracking
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "AIRSPEED TRUE", "KNOTS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "SURFACE RELATIVE GROUND SPEED", "KNOTS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "PLANE BANK DEGREES", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "PLANE PITCH DEGREES", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "VERTICAL SPEED", "FEET PER MINUTE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "PLANE LATITUDE", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "PLANE LONGITUDE", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "PLANE ALTITUDE", "FEET", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "INDICATED ALTITUDE", "FEET", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "PLANE ALT ABOVE GROUND", "FEET", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.SlowLoopAIStruct, "PLANE HEADING DEGREES TRUE", "DEGREE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

                //SimObject Movements Setter
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.VehMovementsLoop, "PLANE LATITUDE", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.VehMovementsLoop, "PLANE LONGITUDE", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.VehMovementsLoop, "PLANE HEADING DEGREES TRUE", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.VehMovementsWP, "AI WAYPOINT LIST", "number", SIMCONNECT_DATATYPE.WAYPOINT, 0.0f, SimConnect.SIMCONNECT_UNUSED);


                // 10sec Refresh Loop
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop10SecStruct, "FUEL TOTAL CAPACITY", "LITERS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);

                // 1sec Aircraft loop
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GENERAL ENG COMBUSTION:1", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GENERAL ENG COMBUSTION:2", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GENERAL ENG COMBUSTION:3", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GENERAL ENG COMBUSTION:4", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AIRCRAFT WIND X", "KNOTS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AIRCRAFT WIND Z", "KNOTS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AMBIENT WIND DIRECTION", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AMBIENT WIND VELOCITY", "KNOTS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "LIGHT ON STATES", "ENUM", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                //SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AUTOPILOT MASTER", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GEAR CENTER POSITION", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GEAR LEFT POSITION", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GEAR RIGHT POSITION", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GEAR TAIL POSITION", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "GEAR AUX POSITION", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "CENTER WHEEL RPM", "RPM", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "LEFT WHEEL RPM", "RPM", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "RIGHT WHEEL RPM", "RPM", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AUX WHEEL RPM", "RPM", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "FUEL TOTAL QUANTITY", "GALLONS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "TOTAL WEIGHT", "KILOGRAMS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "EXIT OPEN:0", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "EXIT OPEN:1", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "EXIT OPEN:2", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "EXIT OPEN:3", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "EXIT OPEN:4", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AUTOPILOT MASTER", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AUTOPILOT HEADING LOCK", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.Loop1SecStruct, "AUTOPILOT HEADING LOCK DIR", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                

                // Frame Aircraft loop
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "AIRSPEED TRUE", "KNOTS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "SURFACE RELATIVE GROUND SPEED", "KNOTS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "PLANE BANK DEGREES", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "PLANE PITCH DEGREES", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "VERTICAL SPEED", "FEET PER MINUTE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "PLANE LATITUDE", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "PLANE LONGITUDE", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "PLANE ALTITUDE", "FEET", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "INDICATED ALTITUDE", "FEET", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "PLANE ALT ABOVE GROUND", "FEET", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "PLANE HEADING DEGREES TRUE", "DEGREE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "MAGVAR", "DEGREE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "INCIDENCE ALPHA", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "INCIDENCE BETA", "DEGREES", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "ACCELERATION BODY X", "GFORCE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "ACCELERATION BODY Y", "GFORCE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "ACCELERATION BODY Z", "GFORCE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "VELOCITY BODY X", "METERS PER SECOND", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "VELOCITY BODY Y", "METERS PER SECOND", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "VELOCITY BODY Z", "METERS PER SECOND", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "G FORCE", "GFORCE", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "TIME ZONE OFFSET", "SECONDS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "ABSOLUTE TIME", "SECONDS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "CAMERA STATE", "ENUM", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "SURFACE TYPE", "ENUM", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "STALL WARNING", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "OVERSPEED WARNING", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "SIM ON GROUND", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "AMBIENT IN CLOUD", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.LoopFrameStruct, "IS SLEW ACTIVE", "BOOL", SIMCONNECT_DATATYPE.INT32, 0f, SimConnect.SIMCONNECT_UNUSED);

                // CG
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftCGStruct, "CG AFT LIMIT", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftCGStruct, "CG FWD LIMIT", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftCGStruct, "CG PERCENT", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
                SimConnectModule.AddToDataDefinition(DEFINITIONS.AircraftCGStruct, "CG PERCENT LATERAL", "PERCENT", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);


                // Structs
                SimConnectModule.RegisterDataDefineStruct<AircraftStruct>(DEFINITIONS.AircraftStruct);
                SimConnectModule.RegisterDataDefineStruct<Loop1SecStruct>(DEFINITIONS.Loop1SecStruct);
                SimConnectModule.RegisterDataDefineStruct<Loop10SecStruct>(DEFINITIONS.Loop10SecStruct);
                SimConnectModule.RegisterDataDefineStruct<LoopFrameStruct>(DEFINITIONS.LoopFrameStruct);
                SimConnectModule.RegisterDataDefineStruct<CGStruct>(DEFINITIONS.AircraftCGStruct);

                int i = 0;
                while(i < 100)
                {
                    i++;
                    SimConnectModule.RegisterDataDefineStruct<PayloadStruct>((DEFINITIONS)(1000 + i));
                }

                //SimConnectModule.RegisterDataDefineStruct<Loop10SecAIStruct>(DEFINITIONS.SlowLoopAIStruct);
                //SimConnectModule.RegisterDataDefineStruct<VehMovementState>(DEFINITIONS.VehMovementsLoop);

                SimConnectModule.MapClientEventToSimEvent(EVENT_ID.EVENT_FREEZE_ALT, "FREEZE_ALTITUDE_SET");
                SimConnectModule.MapClientEventToSimEvent(EVENT_ID.EVENT_SIM_PAUSED_SET, "PAUSE_SET");
                SimConnectModule.MapClientEventToSimEvent(EVENT_ID.EVENT_SIM_SLEW_SET, "SLEW_SET");
                SimConnectModule.MapClientEventToSimEvent(EVENT_ID.EVENT_FREEZE_ATT, "FREEZE_ATTITUDE_SET");
                SimConnectModule.MapClientEventToSimEvent(EVENT_ID.EVENT_FREEZE_POS, "FREEZE_LATITUDE_LONGITUDE_SET");

                SimConnectModule.AddClientEventToNotificationGroup(GROUP_ID.SET_POS, EVENT_ID.EVENT_FREEZE_ALT, false);
                SimConnectModule.AddClientEventToNotificationGroup(GROUP_ID.SET_POS, EVENT_ID.EVENT_FREEZE_ATT, false);
                SimConnectModule.AddClientEventToNotificationGroup(GROUP_ID.SET_POS, EVENT_ID.EVENT_FREEZE_POS, false);
                SimConnectModule.AddClientEventToNotificationGroup(GROUP_ID.SET_POS, EVENT_ID.EVENT_SIM_PAUSED_SET, false);
                SimConnectModule.AddClientEventToNotificationGroup(GROUP_ID.SET_POS, EVENT_ID.EVENT_SIM_SLEW_SET, false);

                SimConnectModule.SubscribeToSystemEvent(EVENT_ID.Sim_State, "Sim");
                SimConnectModule.SubscribeToSystemEvent(EVENT_ID.Sim_Paused, "Pause");
                SimConnectModule.SubscribeToSystemEvent(EVENT_ID.Sim_Sound, "Sound");
                SimConnectModule.SubscribeToSystemEvent(EVENT_ID.CameraMode, "View");
                SimConnectModule.SubscribeToSystemEvent(EVENT_ID.PositionChange, "PositionChanged");
                
                // Get Data at every Frame
                SimConnectModule.RequestDataOnSimObject(
                    DATA_REQUESTS.Sim_Frame_Data,
                    DEFINITIONS.LoopFrameStruct,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.VISUAL_FRAME,
                    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                    0,
                    3,
                    0
                );

                // Get Data at every 1 second
                SimConnectModule.RequestDataOnSimObject(
                    DATA_REQUESTS.Sim_1Sec_Data,
                    DEFINITIONS.Loop1SecStruct,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.SECOND,
                    SIMCONNECT_DATA_REQUEST_FLAG.CHANGED,
                    0,
                    1,
                    0
                );

                // Get Data at every 10 seconds
                SimConnectModule.RequestDataOnSimObject(
                    DATA_REQUESTS.Sim_10Sec_Data,
                    DEFINITIONS.Loop10SecStruct,
                    SimConnect.SIMCONNECT_OBJECT_ID_USER,
                    SIMCONNECT_PERIOD.SECOND,
                    SIMCONNECT_DATA_REQUEST_FLAG.CHANGED,
                    0,
                    10,
                    0
                );
            }
            catch (COMException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        

        private void SimConnect_OnRecvAssignedObjectId(SimConnect sender, SIMCONNECT_RECV_ASSIGNED_OBJECT_ID data)
        {
            LastReceived = App.Timer.ElapsedMilliseconds;

            switch ((OBJECTS_REQUESTS)data.dwRequestID)
            {
                case OBJECTS_REQUESTS.Delete:
                    {
                        break;
                    }
                default:
                    {
                        World.ConfirmID(data.dwRequestID, data.dwObjectID);
                        // SimConnectModule.AIReleaseControl(data.dwObjectID, DATA_REQUESTS.AI_Release);

                        // Get Data about that new Object
                        //SimConnectModule.RequestDataOnSimObject(
                        //    DATA_REQUESTS.Sim_Slow_AI_Data,
                        //    DEFINITIONS.SlowLoopAIStruct,
                        //    data.dwObjectID,
                        //    SIMCONNECT_PERIOD.SECOND,
                        //    SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                        //    0,
                        //    1,
                        //    0
                        //);

                        //SimConnectModule.TransmitClientEvent(data.dwObjectID, EVENT_ID.EVENT_SIM_SLEW_SET, 1, GROUPID.FLAG, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);

                        break;
                    }
            }

            /*
            switch ((OBJECTS_REQUESTS)data.dwRequestID)
            {
                case OBJECTS_REQUESTS.Lightbox:
                    {
                        //if(Lightbox_ID == 0)
                        //{
                        //    Lightbox_ID = Convert.ToUInt32(data.dwObjectID);
                        //    SIMCONNECT_DATA_XYZ pos1 = new SIMCONNECT_DATA_XYZ()
                        //    {
                        //        x = 0,
                        //        y = 0,
                        //        z = 0,
                        //    };
                        //    SIMCONNECT_DATA_XYZ pos2 = new SIMCONNECT_DATA_XYZ()
                        //    {
                        //        x = 0,
                        //        y = 20,
                        //        z = 0,
                        //    };
                        //    SIMCONNECT_DATA_PBH pbh1 = new SIMCONNECT_DATA_PBH()
                        //    {
                        //        Bank = 0,
                        //        Heading = 0,
                        //        Pitch = 0,
                        //    };
                        //
                        //
                        //    SimConnectModule.AttachSimObjectToSimObject(0, pos1, pbh1, Lightbox_ID, pos2, pbh1);
                        //
                        //    SimConnectModule.SetDataOnSimObject(DEFINITIONS.Struct_Rec_Playback, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_DATA_SET_FLAG.DEFAULT, new PlaybackStruct()
                        //    {
                        //        Altitude = SimData.SimConnect.Altitude + 230,
                        //    });
                        //
                        //    //SimConnectModule.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SIM_SLEW_SET, 1, GROUP_ID.SET_POS, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                        //    SimConnectModule.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_SIM_PAUSED_SET, 1, GROUP_ID.SET_POS, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                        //
                        //    SimConnectModule.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_FREEZE_ALT, 1, GROUP_ID.SET_POS, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                        //    SimConnectModule.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_FREEZE_ATT, 1, GROUP_ID.SET_POS, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                        //    SimConnectModule.TransmitClientEvent(SimConnect.SIMCONNECT_OBJECT_ID_USER, EVENT_ID.EVENT_FREEZE_POS, 1, GROUP_ID.SET_POS, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                        //
                        //}
                        //else
                        //{
                        //    Lightbox_ID = 0;
                        //}
                        break;
                    }
            }
            */
        }

        private void Simconnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            LastReceived = App.Timer.ElapsedMilliseconds;
            switch ((DATA_REQUESTS)data.dwRequestID)
            {
                case DATA_REQUESTS.Sim_Frame_Data:
                    {
                        LastAPIResponse = App.Timer.ElapsedMilliseconds;
                        if (Aircraft != null)
                        {
                            SimData.LoopFrame = (LoopFrameStruct)data.dwData[0];

                            if(SimData.LoopFrame.ABS_TIME != LastTemporalData.ABS_TIME)
                            {
                                if(!IsPaused && IsRunning)
                                {
                                    if(LastTemporalData.PLANE_LOCATION.Lat != SimData.LoopFrame.PLANE_LATITUDE && LastTemporalData.PLANE_LOCATION.Lon != SimData.LoopFrame.PLANE_LONGITUDE)
                                    {
                                        LastTemporalData.PLANE_COURSE = MapCalcBearing(LastTemporalData.PLANE_LOCATION.Lat, LastTemporalData.PLANE_LOCATION.Lon, SimData.LoopFrame.PLANE_LATITUDE, SimData.LoopFrame.PLANE_LONGITUDE);

                                        if (Models.EventBus.EventManager.TemporalBuffer3s.Count > 2)
                                        {
                                            TemporalData Prev = null;
                                            lock (Models.EventBus.EventManager.TemporalBuffer3s)
                                            {
                                                foreach (TemporalData TD in Models.EventBus.EventManager.TemporalBuffer3s)
                                                {
                                                    if (Prev != null)
                                                    {
                                                        CRSDeltas.Add((MapCompareBearings(Prev.PLANE_COURSE, TD.PLANE_COURSE) / (TD.SYS_TIME - Prev.SYS_TIME).TotalMilliseconds) * 1000);
                                                    }
                                                    Prev = TD;
                                                }
                                            }
                                            double Avg = CRSDeltas.Average();
                                            LastTemporalData.PLANE_TURNRATE = double.IsNaN(Avg) ? 0 : double.IsInfinity(Avg) ? 0 : Avg;
                                            CRSDeltas.Clear();
                                        }
                                    }
                                    else
                                    {
                                        LastTemporalData.PLANE_COURSE = SimData.LoopFrame.PLANE_HEADING_DEGREES;
                                    }

                                }

                                LastTemporalData.FRAME_DIST = MapCalcDist(LastTemporalData.PLANE_LOCATION, new GeoLoc(SimData.LoopFrame.PLANE_LONGITUDE, SimData.LoopFrame.PLANE_LATITUDE), DistanceUnit.Kilometers, true);
                                LastTemporalData.AIRSPEED_TRUE = SimData.LoopFrame.AIRSPEED_TRUE;
                                LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED = SimData.LoopFrame.SURFACE_RELATIVE_GROUND_SPEED;
                                LastTemporalData.PLANE_BANK_DEGREES = SimData.LoopFrame.PLANE_BANK_DEGREES;
                                LastTemporalData.PLANE_PITCH_DEGREES = SimData.LoopFrame.PLANE_PITCH_DEGREES;
                                LastTemporalData.VERTICAL_SPEED = SimData.LoopFrame.VERTICAL_SPEED;
                                LastTemporalData.PLANE_LOCATION.Lon = SimData.LoopFrame.PLANE_LONGITUDE;
                                LastTemporalData.PLANE_LOCATION.Lat = SimData.LoopFrame.PLANE_LATITUDE;
                                LastTemporalData.PLANE_ALTITUDE = SimData.LoopFrame.PLANE_ALTITUDE;
                                LastTemporalData.INDICATED_ALTITUDE = SimData.LoopFrame.INDICATED_ALTITUDE;
                                LastTemporalData.PLANE_ALT_ABOVE_GROUND = SimData.LoopFrame.PLANE_ALT_ABOVE_GROUND;
                                LastTemporalData.PLANE_HEADING_DEGREES = SimData.LoopFrame.PLANE_HEADING_DEGREES;
                                LastTemporalData.PLANE_MAGVAR = SimData.LoopFrame.PLANE_MAGVAR;
                                LastTemporalData.INCIDENCE_ALPHA = SimData.LoopFrame.INCIDENCE_ALPHA;
                                LastTemporalData.INCIDENCE_BETA = SimData.LoopFrame.INCIDENCE_BETA;
                                LastTemporalData.ACCELERATION_BODY_X = SimData.LoopFrame.ACCELERATION_BODY_X;
                                LastTemporalData.ACCELERATION_BODY_Y = SimData.LoopFrame.ACCELERATION_BODY_Y;
                                LastTemporalData.ACCELERATION_BODY_Z = SimData.LoopFrame.ACCELERATION_BODY_Z;
                                LastTemporalData.VELOCITY_BODY_X = SimData.LoopFrame.VELOCITY_BODY_X;
                                LastTemporalData.VELOCITY_BODY_Y = SimData.LoopFrame.VELOCITY_BODY_Y;
                                LastTemporalData.VELOCITY_BODY_Z = SimData.LoopFrame.VELOCITY_BODY_Z;
                                LastTemporalData.G_FORCE = SimData.LoopFrame.G_FORCE;
                                LastTemporalData.SIM_LOCAL_TIME = new DateTime((long)(SimData.LoopFrame.ABS_TIME - SimData.LoopFrame.TIME_ZONE_OFFSET) * 10000000, DateTimeKind.Utc);
                                LastTemporalData.SIM_ZULU_TIME = new DateTime((long)(SimData.LoopFrame.ABS_TIME) * 10000000, DateTimeKind.Utc);
                                LastTemporalData.SIM_ZULU_OFFSET = SimData.LoopFrame.TIME_ZONE_OFFSET;
                                LastTemporalData.ABS_TIME = SimData.LoopFrame.ABS_TIME;
                                LastTemporalData.APP_RUNTIME = App.Timer.ElapsedMilliseconds;
                                LastTemporalData.SYS_TIME = new DateTime(DateTime.Now.Ticks);
                                LastTemporalData.SYS_ZULU_TIME = new DateTime(DateTime.UtcNow.Ticks);
                                LastTemporalData.CAMERA_STATE = Convert.ToInt32(SimData.LoopFrame.CAMERA_STATE);
                                LastTemporalData.SURFACE_TYPE = ConvertSurface(Convert.ToInt32(SimData.LoopFrame.SURFACE_TYPE));

                                LastTemporalData.STALL_WARNING = Convert.ToBoolean(SimData.LoopFrame.STALL_WARNING);
                                LastTemporalData.OVERSPEED_WARNING = Convert.ToBoolean(SimData.LoopFrame.OVERSPEED_WARNING);
                                LastTemporalData.SIM_ON_GROUND = Convert.ToBoolean(SimData.LoopFrame.SIM_ON_GROUND);
                                LastTemporalData.AMBIENT_IN_CLOUD = Convert.ToBoolean(SimData.LoopFrame.AMBIENT_IN_CLOUD);
                                LastTemporalData.IS_SLEW_ACTIVE = Convert.ToBoolean(SimData.LoopFrame.IS_SLEW_ACTIVE);
                            }

                            IsLoaded = ((Math.Round(LastTemporalData.PLANE_LOCATION.Lat, 3) != 0 && Math.Round(LastTemporalData.PLANE_LOCATION.Lon, 3) != 0) 
                                || Math.Round(LastTemporalData.SURFACE_RELATIVE_GROUND_SPEED) > 70) 
                                && LastTemporalData.CAMERA_STATE != 11
                                && LastTemporalData.CAMERA_STATE != 15 
                                && LastTemporalData.CAMERA_STATE != 12 
                                && LastTemporalData.CAMERA_STATE != 18;
                            
                            if (IsLoaded && PayloadStationsInitialized != null)
                            {
                                if((DateTime.UtcNow - ((DateTime)PayloadStationsInitialized)).TotalSeconds > 5)
                                {
                                    EnablePayloadStations(Aircraft);
                                    AdventuresBase.SchedulePayloadUpdate = true;
                                }
                            }

                            Models.EventBus.EventManager.Ingest(LastTemporalData, Models.EventBus.EventManager.Active);
                            AdventuresBase.ProcessInstantData(LastTemporalData);
                            AdventuresBase.ProcessBroadcasts();
                        }

                        break;
                    }
                case DATA_REQUESTS.Sim_10Sec_Data:
                    {
                        SimData.Loop10Sec = (Loop10SecStruct)data.dwData[0];
                        LastTemporalData.FUEL_TOTAL_CAPACITY_LITERS = SimData.Loop10Sec.FUEL_TOTAL_QUANTITY;
                       
                        break;
                    }
                case DATA_REQUESTS.Sim_CG_Data:
                    {
                        SimData.CG = (CGStruct)data.dwData[0];
                        LastTemporalData.CG_AFT_LIMIT = SimData.CG.CG_AFT_LIMIT;
                        LastTemporalData.CG_FWD_LIMIT = SimData.CG.CG_FWD_LIMIT;
                        LastTemporalData.CG_PERCENT = SimData.CG.CG_PERCENT;
                        
                        PayloadCalibrationCGConfirmed();
                        break;
                    }
                case DATA_REQUESTS.Sim_1Sec_Data:
                    {
                        SimData.Loop1Sec = (Loop1SecStruct)data.dwData[0];

                        LastTemporalData.GENERAL_ENG_COMBUSTION_1 = Convert.ToBoolean(SimData.Loop1Sec.GENERAL_ENG_COMBUSTION_1);
                        LastTemporalData.GENERAL_ENG_COMBUSTION_2 = Convert.ToBoolean(SimData.Loop1Sec.GENERAL_ENG_COMBUSTION_2);
                        LastTemporalData.GENERAL_ENG_COMBUSTION_3 = Convert.ToBoolean(SimData.Loop1Sec.GENERAL_ENG_COMBUSTION_3);
                        LastTemporalData.GENERAL_ENG_COMBUSTION_4 = Convert.ToBoolean(SimData.Loop1Sec.GENERAL_ENG_COMBUSTION_4);
                        LastTemporalData.GENERAL_ENG_COMBUSTION = LastTemporalData.GENERAL_ENG_COMBUSTION_1 || LastTemporalData.GENERAL_ENG_COMBUSTION_2 || LastTemporalData.GENERAL_ENG_COMBUSTION_3 || LastTemporalData.GENERAL_ENG_COMBUSTION_4;
                        LastTemporalData.AIRCRAFT_WIND_X = SimData.Loop1Sec.AIRCRAFT_WIND_X;
                        LastTemporalData.AIRCRAFT_WIND_Z = SimData.Loop1Sec.AIRCRAFT_WIND_Z;
                        LastTemporalData.AMBIENT_WIND_DIRECTION = SimData.Loop1Sec.AMBIENT_WIND_DIRECTION;
                        LastTemporalData.AMBIENT_WIND_VELOCITY = SimData.Loop1Sec.AMBIENT_WIND_VELOCITY;
                        LastTemporalData.LIGHT_ON_STATES = SimData.Loop1Sec.LIGHT_ON_STATES;
                        //LastTemporalData.AUTOPILOT_MASTER = Convert.ToBoolean(SimData.Loop1Sec.AUTOPILOT_MASTER);
                        LastTemporalData.GEAR_CENTER_POSITION = SimData.Loop1Sec.GEAR_CENTER_POSITION;
                        LastTemporalData.GEAR_LEFT_POSITION = SimData.Loop1Sec.GEAR_LEFT_POSITION;
                        LastTemporalData.GEAR_RIGHT_POSITION = SimData.Loop1Sec.GEAR_RIGHT_POSITION;
                        LastTemporalData.GEAR_TAIL_POSITION = SimData.Loop1Sec.GEAR_TAIL_POSITION;
                        LastTemporalData.GEAR_AUX_POSITION = SimData.Loop1Sec.GEAR_AUX_POSITION;
                        LastTemporalData.CENTER_WHEEL_RPM = SimData.Loop1Sec.CENTER_WHEEL_RPM;
                        LastTemporalData.LEFT_WHEEL_RPM = SimData.Loop1Sec.LEFT_WHEEL_RPM;
                        LastTemporalData.RIGHT_WHEEL_RPM = SimData.Loop1Sec.RIGHT_WHEEL_RPM;
                        LastTemporalData.AUX_WHEEL_RPM = SimData.Loop1Sec.AUX_WHEEL_RPM;
                        LastTemporalData.FUEL_QUANTITY = SimData.Loop1Sec.FUEL_QUANTITY;
                        LastTemporalData.TOTAL_WEIGHT = SimData.Loop1Sec.TOTAL_WEIGHT;
                        LastTemporalData.EXIT_0 = Convert.ToInt32(SimData.Loop1Sec.EXIT_0);
                        LastTemporalData.EXIT_1 = Convert.ToInt32(SimData.Loop1Sec.EXIT_1);
                        LastTemporalData.EXIT_2 = Convert.ToInt32(SimData.Loop1Sec.EXIT_2);
                        LastTemporalData.EXIT_3 = Convert.ToInt32(SimData.Loop1Sec.EXIT_3);
                        LastTemporalData.EXIT_4 = Convert.ToInt32(SimData.Loop1Sec.EXIT_4);
                        LastTemporalData.EXIT = (LastTemporalData.EXIT_0 > 0 || LastTemporalData.EXIT_1 > 0 || LastTemporalData.EXIT_2 > 0 || LastTemporalData.EXIT_3 > 0);

                        LastTemporalData.AP_ON = Convert.ToBoolean(SimData.Loop1Sec.AP_ON);
                        LastTemporalData.AP_HDG_ON = Convert.ToBoolean(SimData.Loop1Sec.AP_HDG_ON);
                        LastTemporalData.AP_HDG = SimData.Loop1Sec.AP_HDG;

                        break;
                    }
                case DATA_REQUESTS.Sim_Slow_AI_Data:
                    {
                        Loop10SecAIStruct SlowAILoop = (Loop10SecAIStruct)data.dwData[0];

                        TemporalData TD = new TemporalData();
                        TD.AIRSPEED_TRUE = SlowAILoop.AIRSPEED_TRUE;
                        TD.SURFACE_RELATIVE_GROUND_SPEED = SlowAILoop.SURFACE_RELATIVE_GROUND_SPEED;
                        TD.PLANE_BANK_DEGREES = SlowAILoop.PLANE_BANK_DEGREES;
                        TD.PLANE_PITCH_DEGREES = SlowAILoop.PLANE_PITCH_DEGREES;
                        TD.VERTICAL_SPEED = SlowAILoop.VERTICAL_SPEED;
                        TD.PLANE_LOCATION.Lon = SlowAILoop.PLANE_LONGITUDE;
                        TD.PLANE_LOCATION.Lat = SlowAILoop.PLANE_LATITUDE;
                        TD.PLANE_ALTITUDE = SlowAILoop.PLANE_ALTITUDE;
                        TD.INDICATED_ALTITUDE = SlowAILoop.INDICATED_ALTITUDE;
                        TD.PLANE_ALT_ABOVE_GROUND = SlowAILoop.PLANE_ALT_ABOVE_GROUND;
                        TD.PLANE_HEADING_DEGREES = SlowAILoop.PLANE_MAGVAR;
                        TD.APP_RUNTIME = App.Timer.ElapsedMilliseconds;
                        TD.SYS_TIME = new DateTime(DateTime.Now.Ticks);
                        TD.SYS_ZULU_TIME = new DateTime(DateTime.UtcNow.Ticks);
                        TD.PLANE_COURSE = MapCalcBearing(TD.PLANE_LOCATION.Lat, TD.PLANE_LOCATION.Lon, SlowAILoop.PLANE_LATITUDE, SlowAILoop.PLANE_LONGITUDE);

                        World.UpdateObjectData(data.dwObjectID, TD);
                        break;
                    }
                case DATA_REQUESTS.Aircraft_Init:
                    {
                        SimData.AircraftData = (AircraftStruct)data.dwData[0];
                        AircraftInstance Previous = null;

                        if(Aircraft != null)
                        {
                            if (SimData.AircraftData.TITLE == Aircraft.LastLivery)
                            {
                                return;
                            }
                            Previous = Aircraft;
                        }

                        DisablePayloadStations();
                        
                        string ImagePath = "";

                        Aircraft = FleetService.CreateAircraft(
                            ActiveSim, 
                            AircraftName, 
                            AircraftDirectory, 
                            AircraftDirectoryFull,
                            ImagePath,
                            SimData.AircraftData.TITLE,
                            SimData.AircraftData.TYPE,
                            SimData.AircraftData.EMPTY_WEIGHT_KG,
                            SimData.AircraftData.MAX_GROSS_WEIGHT_KG,
                            SimData.AircraftData.WING_SPAN_METERS, 
                            SimData.AircraftData.ENGINE_COUNT,
                            (uint)SimData.AircraftData.PAYLOAD_STATION_COUNT
                        );
                        
                        Models.EventBus.EventManager.ResetSession();

                        Console.WriteLine("New Aircraft loaded: " + AircraftName + " / " + SimData.AircraftData.TITLE);

                        FleetService.ChangedAircraft(Previous, Aircraft);
                        AdventuresBase.ChangeAircraft(Previous, Aircraft);

                        break;
                    }
                default:
                    {
                        try
                        {
                            if (data.dwRequestID > 1000 && data.dwRequestID <= 1200) // Loop
                            {
                                int PayloadIndex = (int)data.dwRequestID - (data.dwRequestID > 1100 ? 1101 : 1001);
                                SimData.Payload = (PayloadStruct)data.dwData[0];

                                LastTemporalData.SetPayload(PayloadIndex, (float)SimData.Payload.STATION);

                                /*
                                if (data.dwRequestID > 1100 && data.dwRequestID <= 1200) // Calibration
                                {
                                    if (PayloadStationCalibrationRemaining == 0)
                                    {
                                        GetCG();
                                    }
                                    else
                                    {
                                        PayloadStationCalibrationRemaining--;
                                    }
                                }
                                else if(PayloadStationRemaining > 0)// Normal fetch
                                {
                                    PayloadStationRemaining--;
                                    if (PayloadStationRemaining == 0)
                                    {
                                        PayloadConfirmed();
                                    }
                                }
                                */

                            }
                        }
                        catch
                        {
                            PayloadStationRemaining = 0;
                            //PayloadStationCalibrationRemaining = 0;
                        }

                        break;
                    }

            }
        }
        
        private void Simconnect_OnRecvWeatherObservation(SimConnect sender, SIMCONNECT_RECV_WEATHER_OBSERVATION data)
        {
            SimConnection.LastReceived = App.Timer.ElapsedMilliseconds;
            SimData.Metar = data.szMetar;
            InjestMetar(SimData.Metar);
        }

        private void SimConnect_OnRecvSystemState(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            LastReceived = App.Timer.ElapsedMilliseconds;
            switch ((DATA_REQUESTS)data.dwRequestID)
            {
                case DATA_REQUESTS.Aircraft_Loaded:
                    {
                        if (data.szString == string.Empty)
                        {
                            Console.WriteLine("Sim Aircraft does not have a title.");
                            return;
                        }

                        Console.WriteLine("Sim Aircraft Loaded: '" + data.szString + "'. Getting more details.");

                        // Split path from air file to get the aircraft folder name
                        string[] pathArray = data.szString.Split('\\');

                        string aircraftDirectory = Path.GetDirectoryName(data.szString);
                        string aircraftName = pathArray[pathArray.Length - 2];
                        
                        LastSent = App.Timer.ElapsedMilliseconds;
                        SimConnectModule.RequestDataOnSimObject(
                            DATA_REQUESTS.Aircraft_Init,
                            DEFINITIONS.AircraftStruct,
                            SimConnect.SIMCONNECT_OBJECT_ID_USER,
                            SIMCONNECT_PERIOD.ONCE,
                            SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                            0,
                            0,
                            0
                        );

                        if (Aircraft != null)
                        {
                            if (Aircraft.Name == aircraftName)
                            {
                                return;
                            }
                        }

                        AircraftDirectoryFull = data.szString;
                        AircraftDirectory = pathArray[pathArray.Length - 2];
                        AircraftName = aircraftName;
                        
                        LastSent = App.Timer.ElapsedMilliseconds;
                        SimConnectModule.RequestSystemState(DATA_REQUESTS.Flight_Loaded, "FlightLoaded");
                        break;
                    }
                case DATA_REQUESTS.Flight_Loaded:
                    {
                        Console.WriteLine("Flight Loaded");

                        GetSimWindow();
                        //SimConnection.GameOverlay.ResetOverlayZ();

                        //SetPause(false);
                        //Connector.SetLightbox(false);
                        //if(SimData.FastLoop.SIM_ON_GROUND == 1)
                        //{
                        //    Connector.SetLightbox(true, CameraSequence.Introduction);
                        //}
                        break;
                    }
            }
        }

        private void Simconnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_EVENT recEvent)
        {
            //eventCount++;
            //Console.WriteLine(eventCount);

            LastReceived = App.Timer.ElapsedMilliseconds;
            switch ((EVENT_ID)recEvent.uEventID)
            {
                case EVENT_ID.Menu_1:
                    {
                        MW.ToggleSkypad();
                        break;
                    }
                case EVENT_ID.Sim_State:
                    {
                        //0 = Sim Stopped 1 = Sim Runnning
                        if (Convert.ToBoolean(recEvent.dwData))
                        {
                            Console.WriteLine("Simulation Started");
                            IsRunning = true;
                            SimRuntime.Restart();
                            LastSent = App.Timer.ElapsedMilliseconds;
                            SimConnectModule.RequestSystemState(DATA_REQUESTS.Aircraft_Loaded, "AircraftLoaded");

                            double dist = MapCalcDist(LastTemporalData.PLANE_LOCATION.Lat, LastTemporalData.PLANE_LOCATION.Lon, PreMenuPosition.Lat, PreMenuPosition.Lon, Utils.DistanceUnit.Kilometers);
                            if (dist > 11)
                            {
                                Models.EventBus.EventManager.ResetSession();
                            }

                            GetSimWindow();
                            GetSimIsFocused();
                        }
                        else
                        {
                            IsRunning = false;
                            Console.WriteLine("Simulation Stopped");
                            APIResponseTime.Clear();

                        }

                        PreMenuPosition.Lon = LastTemporalData.PLANE_LOCATION.Lon;
                        PreMenuPosition.Lat = LastTemporalData.PLANE_LOCATION.Lat;
                        ProcessPauseEvent();
                        break;
                    }
                case EVENT_ID.Sim_Paused:
                    {
                        //0 = Sim Unpaused 1 = Sim Paused
                        if (Convert.ToBoolean(recEvent.dwData))
                        {
                            IsPaused = true;
                            Console.WriteLine("Simulation Paused");
                        }
                        else
                        {
                            IsPaused = false;
                            Console.WriteLine("Simulation Unpaused");
                            //Connector.SetLightbox(false);
                        }
                        ProcessPauseEvent();

                        break;
                    }
                case EVENT_ID.Sim_Sound:
                    {
                        if (Convert.ToBoolean(recEvent.dwData))
                        {
                            SimHasSound = true;
                            Console.WriteLine("Sound On");
                        }
                        //else
                        //{
                        //    SimHasSound = false;
                        //    Console.WriteLine("Sound Off");
                        //}
                        break;
                    }
                case EVENT_ID.CameraMode:
                    {
                        CameraMode = (CAMERA_MODE)recEvent.dwData;
                        break;
                    }
                case EVENT_ID.PositionChange:
                    {
                        SimConnection.SimFrameAvg = 0;
                        LastSimFrame = 0;
                        lock (FrameRates)
                        {
                            Models.EventBus.EventManager.ResetSession();
                            FrameRates.Clear();
                        }
                        break;
                    }
            }
        }

        private void SimConnect_OnRecvEventFrame(SimConnect sender, SIMCONNECT_RECV_EVENT_FRAME data)
        {
            LastReceived = App.Timer.ElapsedMilliseconds;
            switch ((EVENT_ID)data.uEventID)
            {
                case EVENT_ID.Frame:
                    {
                        if (SimHasFocus || FrameRates.Count < 10)
                        {
                            double fr = App.Timer.ElapsedMilliseconds - LastSimFrame;
                            double fps = 1000 / fr;
                            if (fps > 0 && !double.IsInfinity(fps))
                            {
                                FrameRates.Add(fps);
                                if (FrameRates.Count > 10)
                                {
                                    FrameRates.RemoveAt(0);
                                }
                            }

                            SimFrameAvg = FrameRates.Average();
                        }
                        LastSimFrame = App.Timer.ElapsedMilliseconds;
                        break;
                    }
            }
        }

        private void Simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            SimConnection.LastReceived = App.Timer.ElapsedMilliseconds;
        }

        private void Simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            SimConnection.LastReceived = App.Timer.ElapsedMilliseconds;
            Console.WriteLine("MSFS has exited");
            Disconnect();
        }

        private void Simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SimConnection.LastReceived = App.Timer.ElapsedMilliseconds;
            Console.WriteLine("Exception received: " + App.JSSerializer.Serialize(data));

            World.ConfirmID(data.dwID, null);

        }



        private void RecoverFromError()
        {
            Disconnect();
            Connect();
        }

        private IntPtr HandleSimConnectEvents(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool isHandled)
        {
            isHandled = false;
            switch (message)
            {
                case WM_USER_SIMCONNECT:
                    {
                        if (SimConnectModule != null)
                        {
                            try
                            {
                                if (message == WM_USER_SIMCONNECT)
                                {
                                    SimConnectModule.ReceiveMessage();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Recover from Error: " + ex.Message + ": " + ex.StackTrace);
                                Thread.Sleep(3000);
                                RecoverFromError();
                            }
            
                            isHandled = true;
                        }
                    }
                    break;
            
                default:
                break;
            }

            return IntPtr.Zero;
        }
        
        private void InjestMetar(string metar)
        {
            // Examples
            //????&A0 212126Z 00000KT&D0NG 100KM&B-450&D5450 CLR 17/10 Q1018 
            //"????&A0 222221Z 07303KT&D462NG 100KM&B-457&D30936 1ST010&ST000FNVN000N -40/-52 Q1019 "
            //KSEA&A131 000000Z 00000KT&D985NG 100KM&B-581&D3048 2CU053&CU000FNMN-19N 15/05 Q1013 @@@ 65 15 270 20 | 196 15 270 25 | 
            //????&A0 222335Z 26910KT&D458NG 59KM&B-600&D1600 100KM&B1001&D23999 1CU030&CU000FNVN000N 13/07 Q1020
            //????&A0 221658Z 22805KT&D900LM 227V229 32KM&B-429&D6000 2CU043&CU001FMMN000N 8CI295&CI001FMLN000N 25/25 Q0989
            //????&A0 221658Z 22805G152KT&D900LM 227V229 32KM&B0&D5571 2CU043&CU001FMMN000N 8CI295&CI001FMLN000N 25/25 Q0989
            //????&A0 221658Z 04612G15KT&D985NG 036V056 32KM&B-429&D2000 +SN 7ST033&ST001FNHS000L -5/-15 Q1001 

            try
            {
                WeatherData NewWeather = new WeatherData();
                string[] metarSplit = metar.Split(' ');

                foreach (string segment in metarSplit)
                {
                    #region Cloud
                    MatchCollection CloudMatches = new Regex(@"(\d)([A-Z]{2})(\d{3})((\&)([A-Z]{2})(\d{3})(\S{1,9}))?").Matches(segment);
                    foreach (Match CloudMatch in CloudMatches)
                    {
                        string MatchStr = CloudMatch.ToString();

                        WeatherData.Cloud_Types Type = WeatherData.Cloud_Types.None;
                        switch (MatchStr.Substring(1, 2))
                        {
                            case "CI": Type = WeatherData.Cloud_Types.CI; break;
                            case "CS": Type = WeatherData.Cloud_Types.CS; break;
                            case "CC": Type = WeatherData.Cloud_Types.CC; break;
                            case "AS": Type = WeatherData.Cloud_Types.AS; break;
                            case "AC": Type = WeatherData.Cloud_Types.AC; break;
                            case "SC": Type = WeatherData.Cloud_Types.SC; break;
                            case "NS": Type = WeatherData.Cloud_Types.NS; break;
                            case "ST": Type = WeatherData.Cloud_Types.ST; break;
                            case "CU": Type = WeatherData.Cloud_Types.CU; break;
                            case "CB": Type = WeatherData.Cloud_Types.CB; break;
                        }

                        WeatherData.CloudLayer NewLayer = new WeatherData.CloudLayer()
                        {
                            Base = Convert.ToInt32(MatchStr.Substring(3, 3)) * 100,
                            //Coverage = Convert.ToInt16(MatchStr.Substring(0, 1)),
                            Type = Type,
                        };

                        NewWeather.Clouds.Add(NewLayer);
                    }
                    #endregion

                    #region Temperatures
                    MatchCollection TemperatureMatches = new Regex(@"(-)?(\d{1,2})\/(-)?(\d{1,2})").Matches(segment);
                    foreach (Match TemperatureMatche in TemperatureMatches)
                    {
                        string MatchStr = TemperatureMatche.ToString();
                        string[] TempSpl = MatchStr.Split('/');

                        NewWeather.Temperature = Convert.ToInt16(TempSpl[0]);
                        NewWeather.DewPoint = Convert.ToInt16(TempSpl[1]);
                    }
                    #endregion

                    #region Precipitation   
                    foreach (Precipitation_Types Type in (Precipitation_Types[])Enum.GetValues(typeof(Precipitation_Types)))
                    {
                        MatchCollection mc = new Regex(@"(-|\+)?(\w{2})?(\w{2})?" + Type + @"(\w{2})?(\w{2})?\s").Matches(metar);
                        if (mc.Count > 0)
                        {
                            string precip = mc[0].ToString().Trim();
                            short Rate = 0;

                            if (precip.Contains("TS"))
                            {
                                NewWeather.Thunderstorm = true;
                            }

                            switch (precip[0])
                            {
                                case '+':
                                    {
                                        Rate = 3;
                                        break;
                                    }
                                case '-':
                                    {
                                        Rate = 1;
                                        break;
                                    }
                                default:
                                    {
                                        Rate = 2;
                                        break;
                                    }
                            }

                            NewWeather.Precipitation = Type;
                            NewWeather.Precipitation_Rate = Rate;
                            break;
                        }
                    }
                    #endregion

                    #region Visibility
                    NewWeather.VisibilitySM = Convert.ToInt16(LastTemporalData.AMBIENT_VISIBILITY);
                    #endregion

                    #region Wind
                    MatchCollection WindMatches = new Regex(@"(\d{3})(\d{2})(G(\d{1,3}))?KT").Matches(segment);
                    foreach (Match WindMatche in WindMatches)
                    {
                        string MatchStr = WindMatche.ToString();
                        string[] Sects = MatchStr.Split('G');


                        string Heading = Sects[0].Substring(0, 3);
                        string Speed = Sects[0].Substring(3).Replace("KT", "");
                        string Gust = Speed;
                        if (MatchStr.Length > 7)
                        {
                            Gust = Sects[1].Replace("KT", "");
                        }

                        NewWeather.WindSpeed = Convert.ToInt16(Speed);
                        NewWeather.WindHeading = Convert.ToInt16(Heading);
                        NewWeather.WindGust = Convert.ToInt16(Gust);
                    }
                    #endregion

                    #region Altimeter
                    MatchCollection AltimeterMatches = new Regex(@"Q\d{4}").Matches(segment);
                    foreach (Match AltimeterMatche in AltimeterMatches)
                    {
                        string MatchStr = AltimeterMatche.ToString();
                        string[] Sects = MatchStr.Split('Q');

                        string AltimeterStr = Sects[1];

                        NewWeather.Altimeter = Convert.ToSingle(AltimeterStr);
                    }
                    #endregion

                }

                SimConnection.LatestWeatherData = NewWeather;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to process METAR: " + ex.Message + " on " + metar);
            }
        }

        private string GetInstallDir(string path)
        {
            try
            {
                List<string> OptFile = File.ReadAllLines(path).ToList();
                string Line = OptFile.Find(x => x.StartsWith("InstalledPackagesPath"));
                Line = Line.Replace("InstalledPackagesPath ", "").Trim('"');
                if (Directory.Exists(Line))
                {
                    return Line;
                }
            }
            catch
            {

            }

            return null;
        }
        

        private void EnablePayloadStations(AircraftInstance Acf)
        {
            PayloadStationsInitialized = null;
            PayloadStationsCount = Acf.PayloadStationCount;
            
            int i = 0;
            while (i < Acf.PayloadStationCount)
            {
                i++;
                SimConnectModule.AddToDataDefinition((DEFINITIONS)(1000 + i), "PAYLOAD STATION WEIGHT:" + i, "KILOGRAMS", SIMCONNECT_DATATYPE.FLOAT64, 0f, SimConnect.SIMCONNECT_UNUSED);
            }

            PayloadStationRemaining = Aircraft.PayloadStationCount;
            //i = 0;
            //while (i < Aircraft.PayloadStationCount)
            //{
            //    i++;
            //    SimConnectModule.RequestDataOnSimObject(
            //        (DATA_REQUESTS)(1000 + i),
            //        (DEFINITIONS)(1000 + i),
            //        SimConnect.SIMCONNECT_OBJECT_ID_USER,
            //        SIMCONNECT_PERIOD.SECOND,
            //        SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
            //        0,
            //        1,
            //        0
            //    );
            //}
            
        }

        private void DisablePayloadStations()
        {
            if (PayloadStationsInitialized == null)
            {
                int i = 0;
                while (i < PayloadStationsCount)
                {
                    i++;
                    SimConnectModule.ClearDataDefinition((DEFINITIONS)(1000 + i));
                }
                PayloadStationsCount = 0;
                PayloadStationsInitialized = DateTime.UtcNow;
            }
        }
        
        private void GetCalibratedPayloadStations()
        {
            /*
            if (PayloadStationCalibrationRemaining == 0)
            {
                PayloadStationCalibrationRemaining = Aircraft.PayloadStationCount;
                int i = 0;
                while (i < Aircraft.PayloadStationCount)
                {
                    i++;
                    SimConnectModule.RequestDataOnSimObject(
                        (DATA_REQUESTS)(1100 + i),
                        (DEFINITIONS)(1000 + i),
                        SimConnect.SIMCONNECT_OBJECT_ID_USER,
                        SIMCONNECT_PERIOD.ONCE,
                        SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                        0,
                        0,
                        0
                    );
                }
            }
            */
        }


        private void PayloadConfirmed()
        {
            Aircraft.Load();
        }

        private void PayloadCalibrationCGConfirmed()
        {
            /*
            // Set initial offset, Save CG info
            if(PayloadStationCalibrationIndex >= 0)
            {
                PayloadStationCalibrationResults[PayloadStationCalibrationIndex] = new AircraftPayloadStation((float)SimData.CG.CG_PERCENT_LATERAL, 0, (float)(PayloadStationCalibrationOffset + SimData.CG.CG_PERCENT) * -1);
            }
            else
            {
                PayloadStationCalibrationOffset = -SimData.CG.CG_PERCENT;
            }

            if (PayloadStationCalibrationIndex < PayloadStationCalibrationAircraft.PayloadStationCount - 1)
            {
                PayloadStationCalibrationIndex++;

                // Insert calibration load
                int i = 0;
                while(i < PayloadStationCalibrationAircraft.PayloadStationCount)
                {
                    SendPayload(i, i == PayloadStationCalibrationIndex ? 100 : 0);
                    i++;
                }
                Thread.Sleep(300);
                GetCalibratedPayloadStations();
            }
            else
            {
                PayloadStationCalibrating = false;

                // Restore initial payload
                int i = 0;
                while (i < PayloadStationCalibrationAircraft.PayloadStationCount)
                {
                    SendPayload(i, PayloadStationCalibrationMemory[i]);
                    i++;
                }

                PayloadStationCalibrationDone?.Invoke(PayloadStationCalibrationResults);
            }
            */
        }
        


        private Surface ConvertSurface(int surface)
        {
            /*
            0 = Concrete
            1 = Grass
            2 = Water
            3 = Grass_bumpy
            4 = Asphalt
            5 = Short_grass
            6 = Long_grass
            7 = Hard_turf
            8 = Snow
            9 = Ice
            10 = Urban
            11 = Forest
            12 = Dirt
            13 = Coral
            14 = Gravel
            15 = Oil_treated
            16 = Steel_mats
            17 = Bituminus
            18 = Brick
            19 = Macadam
            20 = Planks
            21 = Sand
            22 = Shale
            23 = Tarmac
            24 = Wright_flyer_track
            */

            switch (surface)
            {
                case 0: return Surface.Concrete;
                case 1: return Surface.Grass;
                case 2: return Surface.Water;
                case 3: return Surface.Grass;
                case 4: return Surface.Asphalt;
                case 5: return Surface.Grass;
                case 6: return Surface.Grass;
                case 7: return Surface.Grass;
                case 8: return Surface.Snow;
                case 9: return Surface.Ice;
                case 10: return Surface.Unknown;
                case 11: return Surface.Unknown;
                case 12: return Surface.Dirt;
                case 13: return Surface.Coral;
                case 14: return Surface.Gravel;
                case 15: return Surface.OilTreated;
                case 16: return Surface.SteelMats;
                case 17: return Surface.Bituminous;
                case 18: return Surface.Brick;
                case 19: return Surface.Macadam;
                case 20: return Surface.Planks;
                case 21: return Surface.Sand;
                case 22: return Surface.Shale;
                case 23: return Surface.Tarmac;
                default: return Surface.Unknown;
            }
        }

        /*
        private void SetLightbox(bool state, CameraSequence type = CameraSequence.Introduction)
        {
            if (LightboxState != state)
            {
                LightboxState = state;
                if (state)
                {
                    ConnectorInstance_P3D.Lightbox.Create(type);
                }
                else
                {
                    ConnectorInstance_P3D.Lightbox.Destroy();
                }
            }
        }

        private class Lightbox
        {
            private void Create(CameraSequence type)
            {
                Console.WriteLine("Creating Lightbox");
                SimConnection.LastSent = App.Timer.ElapsedMilliseconds;
                SimConnectModule.CloseView("TSP Lightbox");

                Rect size = SimConnection.CalculateSimWindowInnerCoords();
                if (size.Right > 0 && size.Bottom > 0)
                {
                    SimConnectModule.UnsubscribeFromSystemEvent(EVENT_ID.CameraMode);
                    SimConnectModule.OpenView("TSP Lightbox", "TSP_Lightbox");
                    SimConnectModule.SetCameraWindowSize("TSP_Lightbox", (uint)(size.Right), (uint)size.Bottom);
                    SimConnectModule.SetCameraWindowPosition("TSP_Lightbox", 0, 0);
                    SimConnection.SetGameOverlay(true);
                    SequenceProcess.Create(type, SequenceLib.GetSequence(CameraSequence.Introduction));
                    SetCameraTimer(true);

                }
            }

            private void Process()
            {
                CameraPosition SequenceFinalPos = SequenceProcess.Process();

                if (SequenceFinalPos != null && SimConnectModule != null)
                {

                    double cos = Math.Cos(Math.PI * ((SimConnection.LastTemporalData.PLANE_HEADING_DEGREES) / 180));
                    double sin = Math.Sin(Math.PI * ((SimConnection.LastTemporalData.PLANE_HEADING_DEGREES) / 180));

                    double x = (SequenceFinalPos.X * cos) + (SequenceFinalPos.Z * sin);
                    double z = (SequenceFinalPos.Z * cos) - (SequenceFinalPos.X * sin);
                    double h = Normalize180(SequenceFinalPos.H + (SimConnection.LastTemporalData.PLANE_HEADING_DEGREES));

                    try
                    {
                        SimConnection.LastSent = App.Timer.ElapsedMilliseconds;
                        SimConnectModule.CameraSetRelative6DofByName("TSP_Lightbox", (float)x, (float)SequenceFinalPos.Y, (float)z, (float)-SequenceFinalPos.P, (float)SequenceFinalPos.B, (float)h);
                    }
                    catch
                    {
                        Disconnect();
                        Destroy();
                    }
                }
                else
                {
                    Destroy();
                }
            }

            internal static void Destroy()
            {
                //Connector.SetGameOverlay(false);
                SimConnection.LightboxState = false;
                SetCameraTimer(false);
                if (SimConnectModule != null)
                {
                    SimConnection.LastSent = App.Timer.ElapsedMilliseconds;
                    SimConnectModule.CloseView("TSP Lightbox");
                    SimConnectModule.SubscribeToSystemEvent(EVENT_ID.CameraMode, "View");
                    SetPause(false);
                }
            }
        }
            */

        private class SimData
        {
            internal static string Metar = "";
            internal static LoopFrameStruct LoopFrame = new LoopFrameStruct();
            internal static Loop1SecStruct Loop1Sec = new Loop1SecStruct();
            internal static PayloadStruct Payload = new PayloadStruct();
            internal static Loop10SecStruct Loop10Sec = new Loop10SecStruct();
            internal static CGStruct CG = new CGStruct();
            internal static AircraftStruct AircraftData = new AircraftStruct();
        }

        #region Structs
#pragma warning disable 0649

        private struct AircraftStruct
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string TITLE;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string TYPE;
            public double WING_SPAN_METERS;
            public DATA_XYZ STRUCT_ENGINE_POSITION_1;
            public DATA_XYZ STRUCT_ENGINE_POSITION_2;
            public DATA_XYZ STRUCT_ENGINE_POSITION_3;
            public DATA_XYZ STRUCT_ENGINE_POSITION_4;
            public double FUEL_TOTAL_QUANTITY_KG;
            public double FUEL_TOTAL_CAPACITY_LITERS;
            public double EMPTY_WEIGHT_KG;
            public double MAX_GROSS_WEIGHT_KG;
            public double ENGINE_TYPE;
            public int ENGINE_COUNT;
            public int PAYLOAD_STATION_COUNT;
        }


        private struct Loop1SecStruct
        {
            public int GENERAL_ENG_COMBUSTION_1;
            public int GENERAL_ENG_COMBUSTION_2;
            public int GENERAL_ENG_COMBUSTION_3;
            public int GENERAL_ENG_COMBUSTION_4;
            public double AIRCRAFT_WIND_X;
            public double AIRCRAFT_WIND_Z;
            public double AMBIENT_WIND_DIRECTION;
            public double AMBIENT_WIND_VELOCITY;
            public double LIGHT_ON_STATES;
            //public int AUTOPILOT_MASTER;
            public double GEAR_CENTER_POSITION;
            public double GEAR_LEFT_POSITION;
            public double GEAR_RIGHT_POSITION;
            public double GEAR_TAIL_POSITION;
            public double GEAR_AUX_POSITION;
            public double CENTER_WHEEL_RPM;
            public double LEFT_WHEEL_RPM;
            public double RIGHT_WHEEL_RPM;
            public double AUX_WHEEL_RPM;
            public double FUEL_QUANTITY;
            public double TOTAL_WEIGHT;
            public double EXIT_0;
            public double EXIT_1;
            public double EXIT_2;
            public double EXIT_3;
            public double EXIT_4;
            public int AP_ON;
            public int AP_HDG_ON;
            public double AP_HDG;
        };

        private struct LoopFrameStruct
        {
            public double AIRSPEED_TRUE;
            public double SURFACE_RELATIVE_GROUND_SPEED;
            public double PLANE_BANK_DEGREES;
            public double PLANE_PITCH_DEGREES;
            public double VERTICAL_SPEED;
            public double PLANE_LATITUDE;
            public double PLANE_LONGITUDE;
            public double PLANE_ALTITUDE;
            public double INDICATED_ALTITUDE;
            public double PLANE_ALT_ABOVE_GROUND;
            public double PLANE_HEADING_DEGREES;
            public double PLANE_MAGVAR;
            public double INCIDENCE_ALPHA;
            public double INCIDENCE_BETA;
            public double ACCELERATION_BODY_X;
            public double ACCELERATION_BODY_Y;
            public double ACCELERATION_BODY_Z;
            public double VELOCITY_BODY_X;
            public double VELOCITY_BODY_Y;
            public double VELOCITY_BODY_Z;
            public double G_FORCE;
            public double TIME_ZONE_OFFSET;
            public double ABS_TIME;
            public double CAMERA_STATE;
            public double SURFACE_TYPE;
            public int STALL_WARNING;
            public int OVERSPEED_WARNING;
            public int SIM_ON_GROUND;
            public int AMBIENT_IN_CLOUD;
            public int IS_SLEW_ACTIVE;
        };

        private struct Loop10SecStruct
        {
            public double FUEL_TOTAL_QUANTITY;
        };

        private struct CGStruct
        {
            public double CG_AFT_LIMIT;
            public double CG_FWD_LIMIT;
            public double CG_PERCENT;
            public double CG_PERCENT_LATERAL;
        };

        private struct Loop10SecAIStruct
        {
            public double AIRSPEED_TRUE;
            public double SURFACE_RELATIVE_GROUND_SPEED;
            public double PLANE_BANK_DEGREES;
            public double PLANE_PITCH_DEGREES;
            public double VERTICAL_SPEED;
            public double PLANE_LATITUDE;
            public double PLANE_LONGITUDE;
            public double PLANE_ALTITUDE;
            public double INDICATED_ALTITUDE;
            public double PLANE_ALT_ABOVE_GROUND;
            public double PLANE_MAGVAR;
            public double MAGVAR;
        };

        private struct PayloadStruct
        {
            public double STATION;
        };


        private struct VehMovementState
        {
            public double Latitude;
            public double Longitude;
            //public double Altitude;
            public double Heading;
            //public double Steer;
            //public double Speed;
        }

        private struct DATA_XYZ
        {
            public double x;
            public double y;
            public double z;
        }

#pragma warning restore 0649
        #endregion


        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
