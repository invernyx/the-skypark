#define TestUpdate
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Audio;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Models.EventBus;
using static TSP_Transponder.Models.SimLibrary;
using static TSP_Transponder.Models.API.APIBase;
using TSP_Transponder.Models.Transactor;
using TSP_Transponder.Models.Progress;
using TSP_Transponder.Models.Weather;
using TSP_Transponder.Models.Bank;
using TSP_Transponder.Models.Updater;
using System.Windows.Media.Imaging;
using System.Resources;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.DataStore;

namespace TSP_Transponder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
	public partial class MainWindow : Window
    {
        internal IntPtr Handle = IntPtr.Zero;
        internal bool GoodToGo = false;
        private short Step = 0;
        private bool InitialStartup = false;
        private int DevClick = 0;
        internal bool IsShuttingDown = false;
        internal bool IsAuthenticated = true;
        CultureInfo ci = new CultureInfo("en-us");
        RegistryKey Reg_Read = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Parallel 42\\The Skypark");
        RegistryKey Reg_Write = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Parallel 42\\The Skypark");
        public Grid CurrentPage = null;
        private Dictionary<string, Storyboard> Storyboards = new Dictionary<string, Storyboard>();
        public System.Windows.Forms.NotifyIcon TrayIcon = new System.Windows.Forms.NotifyIcon();
        private long ValidationLoopLast = -20000;
        private long ValidationLoopDelay = 20000;
        //private long GetWeatherLoopLast = -900000;
        //private long GetWeatherLoopDelay = 900000;
        private long UpdateCheckLast = 0;
        private double UpdateCheckDelay = 43200000; // Every 12 hours (43200000)
        private long UIUpdateLoopLast = 0;
        private long UIUpdateLoopDelay = 100;
        //private long SceneryScanUpdateLoopLast = 0;
        //private long SceneryScanUpdateLoopDelay = 50;
        private bool WiggleArmed = false;
        private bool WiggleActive = false;
        private double WiggleMultiplier = 0;

        #region Timers
        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimeProc proc, int user, int mode);

        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        delegate void TimeProc(int id, int msg, int user, int param1, int param2);

        [StructLayout(LayoutKind.Sequential)]
        public struct TimerCaps
        {
            public int periodMin;
            public int periodMax;
        }

        private TimeProc callback;
        public int camTimerID = 0;
        #endregion

        //static object test;
        public MainWindow(List<string> args)
        {
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;
            this.MouseEnter += MainWindow_MouseEnter;
            InitializeComponent();
            this.Top = -999999;
            this.Left = -999999;
            Handle = new WindowInteropHelper(this).Handle;
            pages_collection.MaxHeight = 600;
        }

        #region Reposition Window
        internal void RepositionWindow()
        {
            Console.WriteLine("Setting Window Position");
            this.WindowState = WindowState.Minimized;
            this.Show();
            this.Hide();
            double window_pos_x = 200;
            double window_pos_y = 200;
            try
            {
                if (Reg_Read != null)
                {
                    if (Reg_Read.GetValue("transponder_window_pos_x") != null)
                    {
                        window_pos_x = Convert.ToDouble(Reg_Read.GetValue("transponder_window_pos_x"));
                    }

                    if (Reg_Read.GetValue("transponder_window_pos_y") != null)
                    {
                        window_pos_y = Convert.ToDouble(Reg_Read.GetValue("transponder_window_pos_y"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to set Window Position: " + ex.Message);
            }

            this.Left = window_pos_x;
            this.Top = window_pos_y;
            if (!App.Args.Contains("silent")) //UserData.Get("token") == string.Empty
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
            }


            double m_window_pos_x = Left;
            double m_window_pos_y = Top;
            double m_default_width = Width;
            double m_default_height = Height;

            Point winLoc = new Point(m_window_pos_x, m_window_pos_y);
            Size winSize = new Size(m_default_width, m_default_height);
            Rect winrect = new Rect(winLoc, winSize);
            System.Windows.Forms.Screen ActiveScreen = WindowAllVisible(winrect);

            if (ActiveScreen != null && m_window_pos_x + m_window_pos_y != 0)
            {
                float ratio = 1;
                //uint dpi = GetDpi(System.Windows.Forms.Screen.PrimaryScreen, DpiType.Effective);
                //if (dpi > 96) ratio = Convert.ToSingle((decimal)dpi / 96M);
                this.Left = m_window_pos_x * ratio;
                this.Top = m_window_pos_y * ratio;
            }
            else
            {
                Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top + 50;
                Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left + 50;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            Console.WriteLine("Window Position Set to " + this.Left + ", " + this.Top);
        }
        #endregion

        #region TRANSLUCENT WINDOW
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            var accentStructSize = Marshal.SizeOf(accent);
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        #endregion

        #region Page Switcher
        public void SwitchPage(Grid collection, Grid newPage, Grid currentPage)
        {
            if (newPage != currentPage)
            {
                foreach (Grid page in collection.Children)
                {
                    if (page != newPage)
                    {
                        page.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        page.Visibility = Visibility.Visible;
                    }
                }
            }

            if(newPage.Name == "page_settings")
            {
                string setting = UserData.Get("audio_output");
                page_settings__audio_devices__list.Items.Clear();
                for (int n = -1; n < WaveOut.DeviceCount; n++)
                {
                    try
                    {
                        WaveOutCapabilities caps = WaveOut.GetCapabilities(n);
                        ComboBoxItem cbi = new ComboBoxItem();
                        string DName = caps.ProductName;
                        if (DName == "Microsoft Sound Mapper")
                        {
                            DName = "Default";
                        }
                        cbi.Content = DName;
                        cbi.Uid = caps.ProductName;
                        page_settings__audio_devices__list.Items.Add(cbi);
                        if (setting == caps.ProductName || setting == string.Empty)
                        {
                            cbi.IsSelected = true;
                            setting = caps.ProductName;
                            UserData.Set("audio_output", caps.ProductName);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Failed to get audio device: " + ex.Message);
                    }
                }
                
            }


        }
        #endregion

        #region Timers
        public void Set_Timer_State(bool status)
        {
            if (status)
            {
                Console.WriteLine("Starting Timer");
                TimerCaps caps = new TimerCaps();
                timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
                int period = 18;
                int resolution = 0;
                int mode = 1; // 0 for periodic, 1 for single event
                callback = new TimeProc(Timer_Tick);
                camTimerID = timeSetEvent(period, resolution, callback, 0, mode);
                Console.WriteLine("Started Timer");
            }
            else
            {
                Console.WriteLine("Stopping Timer");
                timeKillEvent(camTimerID);
                Console.WriteLine("Stopped Timer");
            }
        }
        private void Timer_Tick(int id, int msg, int user, int param1, int param2)
        {
            if (App.IsDead || IsShuttingDown)
            {
                return;
            }
            
            #region Initial
            if (!InitialStartup)
            {
                InitialStartup = true;
            }
            #endregion

            #region Get Weather
            //if (GetWeatherLoopLast + GetWeatherLoopDelay < App.Timer.ElapsedMilliseconds && !IsUpdating && !IsClosing && HasSimInstalled)
            //{
            //    GetWeatherLoopLast = App.Timer.ElapsedMilliseconds;
            //    AviationWeather_gov.GetWeatherData(() => {});
            //}
            #endregion

            #region Clients Validation loop
            if (ValidationLoopLast + ValidationLoopDelay < App.Timer.ElapsedMilliseconds && !IsShuttingDown && UserData.IsLoaded && HasSimInstalled && GoodToGo)
            {
                ValidationLoopLast = App.Timer.ElapsedMilliseconds;

                //if (!IsAuthenticated)
                //{
                //    AuthFromToken();
                //}

                if (APIBase.ClientCollection != null)
                {
                    APIBase.ClientCollection.ValidateClients();
                }

                if (IsAuthenticated)
                {
                    SimConnection.Connect();

                    if (!APIBase.WSRemote.IsAlive)
                    {
                        APIBase.WSRemote.Connect();
                    }

                    if (!APIBase.WSLocal.IsConnected)
                    {
                        APIBase.WSLocal.Connect();
                    }

                    Models.EventBus.EventManager.Active.CheckQueue();
                }
                

            }
            #endregion

            #region Check for Updates
            if ((UpdateCheckLast + UpdateCheckDelay < App.Timer.ElapsedMilliseconds) && SimConnection.ConnectedInstance == null && !IsShuttingDown)
            {
                UpdateCheckLast = App.Timer.ElapsedMilliseconds;
                
                //Console.WriteLine("Ping for update");
                //bool Force = HeldUpdate;
                //HeldUpdate = false;
                //Task.Run(() => APIBase.AuthAsync((code) =>
                //{
                //    switch (code)
                //    {
                //        case HttpStatusCode.OK:
                //            {
                //                CheckUpdate(HeldUpdateNotice);
                //                break;
                //            }
                //        default:
                //            {
                //                Console.WriteLine("Failed pre-update check");
                //                if (Force)
                //                {
                //                    CheckUpdate(HeldUpdateNotice);
                //                }
                //                UpdateCheckLast = Convert.ToInt64(App.Timer.ElapsedMilliseconds - UpdateCheckDelay + 120000);
                //                break;
                //            }
                //    }
                //    
                //    HeldUpdateNotice = false;
                //}));
            }
            #endregion

            #region UI Updates
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (UIUpdateLoopLast + UIUpdateLoopDelay < App.Timer.ElapsedMilliseconds && !IsShuttingDown && UserData.IsLoaded)
                    {
                        UIUpdateLoopLast = App.Timer.ElapsedMilliseconds;

                        if(WindowState == WindowState.Normal)
                        {
                            UpdateUINotifications();

                            lock (APIBase.ClientCollection.ConnectedClients)
                            {
                                foreach (SocketClient client in APIBase.ClientCollection.GetClients())
                                {
                                    if (client.Indicator != null)
                                    {
                                        Dispatcher.Invoke(new Action(() =>
                                        {
                                            double nw = App.Timer.ElapsedMilliseconds;
                                            if (client.LastSent + 100 > nw)
                                            {
                                                client.Indicator.Tag = "2";
                                            }
                                            else if (client.LastSeen + 100 > nw)
                                            {
                                                client.Indicator.Tag = "1";
                                            }
                                            else
                                            {
                                                client.Indicator.Tag = "0";
                                            }
                                        }));
                                    }
                                }
                            }

                            Label sim_ci = SimConnection.ConnectionIndicator;
                            if (sim_ci != null)
                            {

                                Dispatcher.Invoke(new Action(() =>
                                {
                                    double nw = App.Timer.ElapsedMilliseconds;
                                    if (SimConnection.LastSent + 100 > nw)
                                    {
                                        sim_ci.Tag = "2";
                                    }
                                    else if (SimConnection.LastReceived + 100 > nw)
                                    {
                                        sim_ci.Tag = "1";
                                    }
                                    else
                                    {
                                        sim_ci.Tag = "0";
                                    }
                                }));
                            }

                            Label api_ci = APIBase.ConnectionIndicator;
                            if (api_ci != null)
                            {
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    double nw = App.Timer.ElapsedMilliseconds;
                                    if (APIBase.WSRemote.LastSent + 100 > nw)
                                    {
                                        api_ci.Tag = "2";
                                    }
                                    else if (APIBase.WSRemote.LastReceived + 100 > nw)
                                    {
                                        api_ci.Tag = "1";
                                    }
                                    else
                                    {
                                        api_ci.Tag = "0";
                                    }
                                }));
                            }
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to run UI " + ex.Message);
            }
            #endregion

            #region Ingest Loop
            if(Models.EventBus.EventManager.LastLoop + 1000 < App.Timer.ElapsedMilliseconds)
            {
                Models.EventBus.EventManager.Ingest(SimConnection.LastTemporalData, Models.EventBus.EventManager.Active);
            }
            #endregion
        }
        #endregion

        #region Actions
        public void Login()
        {
            //page_login_submit_btn.IsEnabled = false;
            //page_login_email_field.IsEnabled = false;
            //page_login_password_field.IsEnabled = false;
            //
            //string em = page_login_email_field.Text;
            //string pw = page_login_password_field.Password;
            //
            //if (em.Trim() == string.Empty)
            //{
            //    AnimHighlight("page_login_email_highlight", page_login_email_highlight, 1, 0, 0, 5000);
            //    AnimHighlight("page_login_password_highlight", page_login_password_highlight, 1, 0, 0, 5000);
            //}
            //
            //Dispatcher.Invoke(new Action(() =>
            //{
            //    Task.Run(() => HTTP.LoginAsync(em, pw, (result) =>
            //    {
            //        Dispatcher.Invoke(new Action(() =>
            //        {
            //            if (result["status"] == "success")
            //            {
            //                UserData.Save();
            //                AuthFromToken();
            //                SwitchPage(pages_collection, page_launcher, CurrentPage);
            //            }
            //            else
            //            {
            //                switch (result["descr"])
            //                {
            //                    case "client_credentials_invalid":
            //                        {
            //                            MessageBox.Show("Credentials Invalid");
            //                            break;
            //                        }
            //                        default:
            //                        {
            //                            MessageBox.Show("Login failed due to an unknown error");
            //                            break;
            //                        }
            //                }
            //                page_login_submit_btn.IsEnabled = true;
            //                page_login_email_field.IsEnabled = true;
            //                page_login_password_field.IsEnabled = true;
            //                SwitchPage(pages_collection, page_login, CurrentPage);
            //            }
            //        }));
            //        return 0;
            //    }));
            //}));
        }

        public void Logout()
        {
            //page_login_submit_btn.IsEnabled = true;
            //page_login_email_field.IsEnabled = true;
            //page_login_password_field.IsEnabled = true;

            if (APIBase.WSRemote != null)
            {
                string json = App.JSSerializer.Serialize(new Dictionary<string, string>() {
                    { "close_host", UserData.Get("token") },
                });
                //API.WSRemote.Send(json);
                APIBase.WSRemote.Disconnect();
                APIBase.WSLocal.Disconnect();
            }

            UserData.Set("token", "", true);


            //SwitchPage(pages_collection, page_login, CurrentPage);
            //Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    this.Hide();
            //}));
            //CheckUpdate(false, true);

            //this.Show();
            //this.WindowState = WindowState.Normal;
            //this.Activate();
        }

        public void AuthFromToken()
        {
            //Console.WriteLine("---> Auth");
            //Task.Run(() => APIBase.AuthAsync((code) =>
            //{
            //    Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        try
            //        {
            //            page_login_submit_btn.IsEnabled = true;
            //            page_login_email_field.IsEnabled = true;
            //            page_login_password_field.IsEnabled = true;
            //
            //            switch (code)
            //            {
            //                case HttpStatusCode.OK:
            //                    {
            //                        if (GoodToGo)
            //                        {
            //                            SwitchPage(pages_collection, page_launcher, CurrentPage);
            //                        }
            //                        else
            //                        {
            //                            SwitchPage(pages_collection, page_onboard, CurrentPage);
            //                        }
            //                        App.MW.IsAuthenticated = true;
            //
            //
            //                        if (UserData.staging_available)
            //                        {
            //                            install_beta_chk.Visibility = Visibility.Visible;
            //                        }
            //                        else
            //                        {
            //                            install_beta_chk.Visibility = Visibility.Collapsed;
            //                        }
            //
            //                        if (UserData.Get("channel") == "dev" && UserData.dev_available)
            //                        {
            //                            page_settings__extras.Visibility = Visibility.Visible;
            //                        }
            //                        else
            //                        {
            //                            ((StackPanel)page_settings__extras.Parent).Children.Remove(page_settings__extras);
            //                        }
            //                        break;
            //                    }
            //                default:
            //                    {
            //                        Console.WriteLine("---> Failed to connect using Auth Token (" + code.ToString() + ")");
            //                        IsUpdating = false;
            //                        //HeldUpdate = true;
            //
            //                        App.MW.IsAuthenticated = false;
            //                        //UserData.Get("token") = "";
            //                        //UserData.Save();
            //                        //Logout();
            //                        break;
            //                    }
            //            }
            //
            //            #region Clear Log files
            //            string[] ConsoleFiles = Directory.GetFiles(App.AppDataDirectory, "Transponder_Console_*");
            //            foreach (string file in ConsoleFiles)
            //            {
            //                if (file != App.LogPath)
            //                {
            //                    File.Delete(file);
            //                }
            //            }
            //            #endregion
            //            
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine("Failed to Authenticate: " + ex.Message);
            //        }
            //    }));
            //}));
        }
        
        public void LaunchSkypad()
        {
            Process[] sp_proc = Process.GetProcessesByName("Skypad");
            if (sp_proc.Length > 0)
            {
                //uint procId = 0;
                //GetWindowThreadProcessId(GetForegroundWindow(), out procId);

                //if (Process.GetProcessById((int)procId).MainWindowHandle == sp_proc[0].MainWindowHandle)
                //{
                    //ShowWindow(SimConnection.SimulationWindow, 1);
                    //SetForegroundWindow(SimConnection.SimulationWindow);
                //}
                //else if (SimConnection.SimHasFocus)
                //{
                    //ShowWindow(sp_proc[0].MainWindowHandle, 1);
                    //SetForegroundWindow(sp_proc[0].MainWindowHandle);
                //}
            }
            else
            {
                Thread StartSP = new Thread(() =>
                {
                    try
                    {
                        Process Skypad = new Process();
                        Skypad.StartInfo.FileName = Path.Combine(UserData.Get("install"), "Skypad/Skypad.exe");
                        //Skypad.StartInfo.Arguments = UserData.Get("token");
                        Skypad.Start();

                        //Thread.Sleep(2000);
                        //SetForegroundWindow(Skypad.MainWindowHandle);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to start Skypad: " + ex.Message);
                    }
                });
                StartSP.CurrentCulture = CultureInfo.CurrentCulture;
                StartSP.Start();
            }
        }

        public void ToggleSkypad()
        {
            List<Process> sp_proc = Process.GetProcessesByName("Skypad").ToList();
            sp_proc.AddRange(Process.GetProcessesByName("Skypad_Native").ToList());
            if (sp_proc.Count > 0)
            {
                uint procId = 0;
                GetWindowThreadProcessId(GetForegroundWindow(), out procId);

                Thread ShowThread = new Thread(() =>
                {

                    if (Process.GetProcessById((int)procId).MainWindowHandle == sp_proc[0].MainWindowHandle)
                    {
                        Console.WriteLine("Order to Pipe to Minimize Skypad");
                        App.NS.Send("Minimize");
                        //App.SkypadHandler.Send("Minimize");
                        //handler.HandleUri("Minimize");
                        //SwitchToThisWindow(Connector.SimulationWindow, true);
                        //SetActiveWindow(Connector.SimulationWindow);
                        //SetWindowPos(sp_proc[0].MainWindowHandle, Connector.SimulationWindow, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                        //ShowWindow(sp_proc[0].MainWindowHandle, 6);
                        //SetForegroundWindow(SimConnection.SimulationWindow);
                    }
                    else if (SimConnection.SimHasFocus)
                    {
                        Console.WriteLine("Order to Pipe to Restore Skypad");
                        App.NS.Send("Restore");
                        //App.SkypadHandler.Send("Restore");
                        //handler.HandleUri("Restore");
                        //SwitchToThisWindow(sp_proc[0].MainWindowHandle, true);
                        //SetWindowPos(Connector.SimulationWindow, sp_proc[0].MainWindowHandle, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                        //SetActiveWindow(sp_proc[0].MainWindowHandle);
                        //SetForegroundWindow(sp_proc[0].MainWindowHandle);
                    }
                });
                ShowThread.CurrentCulture = CultureInfo.CurrentCulture;
                ShowThread.Start();
            }
            else
            {
                LaunchSkypad();
            }

        }

        private void AskAppShutdown()
        {
            Shutdown();
        }

        internal void Shutdown(bool Restart = false)
        {
            Console.WriteLine("Confirming Shutdown");
            if(!IsShuttingDown)
            {
                IsShuttingDown = true;
                
                if (Restart)
                {
                    APIBase.ClientCollection.SendMessage("transponder:restart", null);
                }

                //SimConnection.UnhookWinEvent(SimConnection.m_hook_focus);
                SimConnection.UnhookWinEvent(SimConnection.m_hook_move);

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Hide();
                }));

                //Controller.Dispose();
                TrayIcon.Visible = false;
                TrayIcon.Dispose();

                if(!App.ThreadCancel.IsCancellationRequested)
                {
                    App.ThreadCancel.Cancel();
                   // App.ThreadCancel.Dispose();
                }
                
                LiteDbService.DisposeAll();

                if (APIBase.WSRemote != null)
                {
                    APIBase.WSRemote.Disconnect();
                }
                if (APIBase.WSLocal != null)
                {
                    APIBase.WSLocal.Disconnect();
                }
                Set_Timer_State(false);
                if (SimConnection.ConnectedInstance != null)
                {
                    SimConnection.ConnectedInstance.Disconnect();
                }

#if !DEBUG
                if (UpdateInstance.ApplyOnQuit)
                {
                    Console.WriteLine("Applying update, delaying shutdown");
                    UpdateInstance.Apply(false, false);
                    return;
                }
#endif
            }


            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Restart)
                {
                    App.RestartAdmin("silent");
                }
                try
                {
                    Application.Current.Shutdown();
                }
                catch
                {
                    Environment.Exit(0);
                }
            });

            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    //this.Close();
            //    //this.Dispatcher.InvokeShutdown();
            //
            //    try
            //    {
            //        //System.Windows.Threading.Dispatcher.ExitAllFrames();
            //        this.Dispatcher.BeginInvoke(new Action(() =>
            //        {
            //            Application.Current.Shutdown();
            //        }));
            //    }
            //    catch
            //    {
            //        Environment.Exit(0);
            //    }
            //}));

        }
        
        internal void ResetValidateLoop()
        {
            ValidationLoopLast -= ValidationLoopDelay;
        }

        internal void UpdatePreferences()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    if (UserData.Get("install") != string.Empty)
                    {
                        page_location_field.Text = UserData.Get("install");
                    }
                    else
                    {
                        Logout();
                        return;
                    }
                    
                    if (UserData.Get("channel") != "dev")
                    {
                        install_beta_chk.IsChecked = UserData.Get("channel") == "rc";
                    }
                    else
                    {
                        install_beta_chk.IsChecked = true;
                        install_beta_chk.IsEnabled = false;
                    }
                    
                    float setting_volume = 0.75f;
                    if(!float.TryParse(UserData.Get("audio_voices"), out setting_volume))
                    {
                        UserData.Set("audio_voices", "0.75");
                    }
                    page_settings__audio_voices__sld.Value = Convert.ToSingle(UserData.Get("audio_voices"));
                    page_onboard__audio_voices__sld.Value = Convert.ToSingle(UserData.Get("audio_voices"));

                    float setting_effects = 0.75f;
                    if (!float.TryParse(UserData.Get("audio_effects"), out setting_effects))
                    {
                        UserData.Set("audio_effects", "0.75");
                    }
                    page_settings__audio_effects__sld.Value = Convert.ToSingle(UserData.Get("audio_effects"));
                    page_onboard__audio_effects__sld.Value = Convert.ToSingle(UserData.Get("audio_effects"));

                    //if (UserData.Get("privacy_airport_data") != string.Empty)
                    //{
                    //    page_settings__privacy_airport_data_chk.IsChecked = Convert.ToBoolean(Convert.ToInt16(UserData.Get("privacy_airport_data")));
                    //    page_settings__privacy_remote_access_chk.IsChecked = Convert.ToBoolean(Convert.ToInt16(UserData.Get("privacy_remote_access")));
                    //}
                    //else
                    //{
                    //    UserData.Set("privacy_airport_data", "1");
                    //    UserData.Set("privacy_remote_access", "1");
                    //    page_settings__privacy_airport_data_chk.IsChecked = true;
                    //    page_settings__privacy_remote_access_chk.IsChecked = true;
                    //}


                    //if (UserData.Get("launch_skypad") != string.Empty)
                    //{
                    //    page_settings__launch_skypad__chk.IsChecked = Convert.ToBoolean(Convert.ToInt16(UserData.Get("launch_skypad")));
                    //}
                    //else
                    //{
                    //    UserData.Set("launch_skypad", "1");
                    //    page_settings__launch_skypad__chk.IsChecked = true;
                    //}

                }
                catch
                {
                    UserData.Set("channel", "prod");
                    UserData.Set("audio_voices", "0.75");
                    UserData.Set("audio_effects", "0.75");
                    UserData.Set("launch_skypad", "1");
                    UserData.Set("privacy_airport_data", "1");
                    UserData.Set("privacy_remote_access", "1");
                    //page_settings__privacy_airport_data_chk.IsChecked = true;
                    //page_settings__privacy_remote_access_chk.IsChecked = true;
                    //page_settings__launch_skypad__chk.IsChecked = true;

                    SwitchPage(pages_collection, page_settings, CurrentPage);
                }
            }));
        }

        internal void ProcessHandler(string uri)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch(uri)
                {
                    case "Restore":
                        {
                            Restore();
                            break;
                        }
                    case "Exit":
                        {
                            IsShuttingDown = true;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                
            }));
        }
        
        internal void Restore()
        {
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                }));
            });
        }

        public void SetupTrayIcon()
        {
            Func<string, System.Windows.Forms.MenuItem> mk = (name) =>
            {
                TrayIcon.ContextMenu.MenuItems.Add(name);
                return TrayIcon.ContextMenu.MenuItems[TrayIcon.ContextMenu.MenuItems.Count - 1];
            };

            // Initializing the notifyIcon menu
            TrayIcon.Visible = true;
            TrayIcon.Text = "The Skypark Transponder";
            TrayIcon.Icon = Properties.Resources.output;
            TrayIcon.ContextMenu = new System.Windows.Forms.ContextMenu();

            var title = mk("The Skypark Transponder");
            title.Enabled = false;

            TrayIcon.ContextMenu.MenuItems.Add("-");
            

            mk("Open the Transponder").Click += new EventHandler(
            (sendItem, args1) =>
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                }));
            });

            mk("Open the Skypad").Click += new EventHandler(
            (sendItem, args1) =>
            {
                LaunchSkypad();
            });

            TrayIcon.ContextMenu.MenuItems.Add("-");
            
            mk("Quit").Click += new EventHandler(
            (sendItem, args1) =>
            {
                AskAppShutdown();
            });
            
            EventHandler OpenEvent = new EventHandler(
            (sendItem, args1) =>
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    this.Activate();

                    if (WiggleArmed)
                    {
                        if (!WiggleActive)
                        {
                            WiggleMultiplier += 20;
                            long StartTime = App.Timer.ElapsedMilliseconds;
                            Point Location = new Point(App.MW.Left, App.MW.Top);
                            Random Rand = new Random();
                            Thread wt = new Thread(() =>
                            {
                                double easeOut = WiggleMultiplier;
                                while (App.Timer.ElapsedMilliseconds - StartTime < 2000)
                                {
                                    int range = Convert.ToInt32(Math.Round(10.0 * (double)easeOut));
                                    easeOut *= 0.95;
                                    Dispatcher.Invoke(() =>
                                    {
                                        App.MW.Left = Location.X + Rand.Next(0, range) - (range / 2);
                                        App.MW.Top = Location.Y + Rand.Next(0, range) - (range / 2);
                                    });
                                    Thread.Sleep(10);
                                }
                                Dispatcher.Invoke(() =>
                                {
                                    App.MW.Left = Location.X;
                                    App.MW.Top = Location.Y;
                                });
                                Thread.Sleep(1000);
                                WiggleActive = false;
                            });
                            wt.CurrentCulture = CultureInfo.CurrentCulture;
                            wt.Start();
                        }
                    }
                    else
                    {
                        WiggleArmed = true;
                    }

                }));
            });
            
            TrayIcon.DoubleClick += OpenEvent;
        }
        
        public void StepGoTo(short Index)
        {
            Step = Index;
            int At = 0;
            foreach (FrameworkElement El in page_onboard.Children)
            {
                if (At == Index)
                {
                    El.Visibility = Visibility.Visible;

                    switch (El.Name)
                    {
                        case "page_onboard_unboxing":
                            {
                                AnimateImageSequence(new string[] { "Unboxing" }, page_onboard_unboxing_img, 120, 1, 30, () =>
                                {
                                    page_onboard_unboxing_titles.Visibility = Visibility.Visible;
                                    page_onboard_unboxing_buttons.Visibility = Visibility.Visible;
                                });
                                break;
                            }
                    }
                }
                else
                {
                    El.Visibility = Visibility.Hidden;
                }
                At++;
            }
        }


        public void ShowError(string code, string message)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                SwitchPage(pages_collection, page_error, CurrentPage);
                page_error_code.Text = code + Environment.NewLine + message;
                page_error_try_btn.IsEnabled = true;
                Restore();
            }));
        }

        public void PopulateUISceneryLists()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                page_scenery_list.Children.Clear();

                foreach (Simulator sim in SimList)
                {
                    Grid mainGd = new Grid();
                    
                    // Main Stack
                    StackPanel sp = new StackPanel();
                    mainGd.Children.Add(sp);

                    // Title
                    sp.Children.Add(new TextBlock()
                    {
                        Text = sim.Platform + " v" + sim.MajorVersion,
                        Margin = new Thickness(10),
                        TextAlignment = TextAlignment.Center,
                        FontSize = 28,
                    });

                    // Progress Bar
                    //sim.AirportsLib.APTScan.SceneryScanProgress = new Border()
                    //{
                    //    VerticalAlignment = VerticalAlignment.Stretch,
                    //    HorizontalAlignment = HorizontalAlignment.Stretch,
                    //    Margin = new Thickness(1),
                    //    Background = new SolidColorBrush(Color.FromArgb(0x11, 0x00, 0x00, 0x00)),
                    //    CornerRadius = new CornerRadius(3),
                    //};

                    //sim.AirportsLib.APTScan.SceneryScanProgressOutline = new Border()
                    //{
                    //    VerticalAlignment = VerticalAlignment.Stretch,
                    //    HorizontalAlignment = HorizontalAlignment.Stretch,
                    //    BorderThickness = new Thickness(1),
                    //    BorderBrush = new SolidColorBrush(Color.FromArgb(0x33, 0x00, 0x00, 0x00)),
                    //    CornerRadius = new CornerRadius(4),
                    //    Child = sim.AirportsLib.APTScan.SceneryScanProgress,
                    //};
                    //mainGd.Children.Add(sim.AirportsLib.APTScan.SceneryScanProgressOutline);


                    // Param Grid
                    Grid pg = new Grid()
                    {
                        Margin = new Thickness(0, 0, 0, 20),
                    };
                    pg.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                    });
                    pg.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                    });
                    sp.Children.Add(pg);

                    int i = 0;
                    while (i < 2)
                    {
                        StackPanel pgsp = new StackPanel();
                        pg.Children.Add(pgsp);
                        pgsp.SetValue(Grid.ColumnProperty, i);
                        i++;
                    }

                    /*
                    // Params
                    i = 0;
                    foreach (string ParamName in sim.AirportsLib.APTScan.StatsParamsName)
                    {
                        Grid gd = new Grid();
                        gd.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = new GridLength(1, GridUnitType.Star),
                        });
                        gd.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = new GridLength(1, GridUnitType.Star),
                        });

                        switch (i)
                        {
                            case 0:
                            case 1:
                            case 2:
                                {
                                    ((StackPanel)pg.Children[1]).Children.Add(gd);
                                    break;
                                }
                            default:
                                {
                                    ((StackPanel)pg.Children[0]).Children.Add(gd);
                                    break;
                                }
                        }

                        TextBlock val = new TextBlock()
                        {
                            Text = "0",
                            Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00)),
                            FontWeight = FontWeights.Bold,
                            TextAlignment = TextAlignment.Right,
                            Margin = new Thickness(0, 0, 10, 0),
                        };
                        val.SetValue(Grid.ColumnProperty, 0);
                        sim.AirportsLib.APTScan.StatsFields.Add(val);
                        sim.AirportsLib.APTScan.StatsValues.Add(0);
                        gd.Children.Add(val);

                        TextBlock param = new TextBlock()
                        {
                            Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00)),
                            Text = ParamName,
                        };
                        param.SetValue(Grid.ColumnProperty, 1);
                        gd.Children.Add(param);
                        i++;
                    };
                    */

                    // Add to the UI
                    page_scenery_list.Children.Add(mainGd);
                }
            }));
        }
        
        public void UpdateSceneryProcess(Simulator sim, double proc, string status)
        {
            /*
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    bool UpdateCounts = false;
                    int i = 0;
                    if (proc == 0)
                    {
                        page_launcher_scenery_status.Content = status;
                        sim.AirportsLib.APTScan.SceneryScanProgress.Width = 6;
                        sim.AirportsLib.APTScan.SceneryScanProgress.HorizontalAlignment = HorizontalAlignment.Left;
                        UpdateCounts = true;
                    }
                    else if (proc == -1)
                    {
                        page_launcher_scenery_status.Content = "Ready";
                        sim.AirportsLib.APTScan.SceneryScanProgress.Width = double.NaN;
                        sim.AirportsLib.APTScan.SceneryScanProgress.HorizontalAlignment = HorizontalAlignment.Stretch;
                        UpdateCounts = true;
                    }
                    else if (proc == -2)
                    {
                        page_launcher_scenery_status.Content = status;
                    }
                    else if (proc == -3)
                    {
                        page_launcher_scenery_status.Content = status;
                    }
                    else if (SceneryScanUpdateLoopLast + SceneryScanUpdateLoopDelay < App.Timer.ElapsedMilliseconds)
                    {
                        SceneryScanUpdateLoopLast = App.Timer.ElapsedMilliseconds;
                        page_launcher_scenery_status.Content = status + " " + proc.ToString("F2", ci) + "%";
                        UpdateCounts = true;

                        double newWidth = 6;
                        if (proc > 0)
                        {
                            double proposedWidth = (Convert.ToDouble(proc) / 100.0) * (sim.AirportsLib.APTScan.SceneryScanProgressOutline.ActualWidth - 4);
                            if (proposedWidth > 6)
                            {
                                newWidth = proposedWidth;
                                sim.AirportsLib.APTScan.SceneryScanProgress.Width = newWidth;
                            }
                        }
                    }

                    if (UpdateCounts)
                    {
                        foreach (TextBlock tb in sim.AirportsLib.APTScan.StatsFields)
                        {
                            sim.AirportsLib.APTScan.StatsFields[i].Text = Utils.FormatNumber(sim.AirportsLib.APTScan.StatsValues[i]);
                            i++;
                        }
                    }

                }));
            }
            catch
            {

            }
            */
        }
        
        public void SetConnectedSimIndicator(string sim)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                page_launcher_connected.Text = sim;
            }));
        }

        public void SetSpeechUI(bool state, string name = "")
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (state)
                {
                    speech_active.Visibility = Visibility.Visible;
                    speech_active_name.Text = name.ToUpper();
                }
                else
                {
                    speech_active.Visibility = Visibility.Hidden;
                }
            }));
        }

        public void PopulateControlAssignment(string name, string keys)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                if (keys == string.Empty)
                {
                    keys = "No Assignment";
                }

                Button btn = (Button)LogicalTreeHelper.FindLogicalNode(page_settings, "page_settings__" + name + "__btn");
                if (btn != null)
                {
                    btn.Content = keys;
                }
            }));
        }

        public uint GetDpi(System.Windows.Forms.Screen screen, DpiType dpiType)
        {
            var pnt = new System.Drawing.Point(screen.Bounds.Left + 1, screen.Bounds.Top + 1);
            var mon = MonitorFromPoint(pnt, 2);

            try
            {
                GetDpiForMonitor(mon, dpiType, out uint dpiX, out uint dpiY);
                return dpiX;
            }
            catch
            {
                return 1;
            }
        }

        public void CreateLTEConnectionIndicator(string style, SocketClient cd)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Label ind = new Label()
                {
                    Style = FindResource(style) as Style,
                    ToolTip = "Connection established with a remote Skypad",
                };

                cd.Indicator = ind;
                clients_indicators_lte.Children.Add(ind);
                ind.Tag = "0";

            }));
        }

        public void CreateWifiConnectionIndicator(string style, SocketClient cd)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Label ind = new Label()
                {
                    Style = FindResource(style) as Style,
                    ToolTip = "Connection established with a local Skypad",
                };

                cd.Indicator = ind;
                clients_indicators_lte.Children.Add(ind);
                ind.Tag = "0";

            }));
        }

        public void CreateSimConnectionIndicator(string style)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Label ind = new Label()
                {
                    Style = FindResource(style) as Style,
                    ToolTip = "Connection established with the simulator (" + SimConnection.ActiveSim.Platform + " v" + SimConnection.ActiveSim.MajorVersion + ")",
                };

                SimConnection.ConnectionIndicator = ind;
                clients_indicators_sim.Children.Insert(0, ind);
                ind.Tag = "0";

            }));
        }

        public void CreateAPIConnectionIndicator(string style)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Label ind = new Label()
                {
                    Style = FindResource(style) as Style,
                    ToolTip = "Connected to The Skypark infrastructure",
                };

                APIBase.ConnectionIndicator = ind;
                clients_indicators_remote.Children.Insert(0, ind);
                ind.Tag = "0";

            }));
        }

        public void DestroyConnectionIndicator(Label ind)
        {
            try
            {
                if(ind != null)
                {
                    StackPanel parent = ((StackPanel)ind.Parent);
                    if (parent != null)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            parent.Children.Remove(ind);
                        }));
                    }
                }
            }
            catch
            {

            }
        }
        #endregion

        #region Notifications
        public void UpdateUINotifications()
        {
            List<Notification> n = null;
            lock (NotificationService.Actives)
            {
                n = new List<Notification>(NotificationService.Actives);
            }


            Dispatcher.Invoke(() =>
            {
                lock (page_launcher_notifList.Children)
                {
                    List<Border> ToRemove = new List<Border>();
                    foreach (var border in page_launcher_notifList.Children)
                    {
                        ToRemove.Add((Border)border);
                    }

                    foreach (var notif in n)
                    {
                        lock (page_launcher_notifList.Children)
                        {
                            if (notif.NotificationListObj != null)
                            {
                                ToRemove.Remove(notif.NotificationListObj);
                                if (!page_launcher_notifList.Children.Contains(notif.NotificationListObj))
                                {
                                    page_launcher_notifList.Children.Insert(0, notif.NotificationListObj);
                                }
                                else if(notif.Changed)
                                {
                                    notif.Changed = false;

                                    int Index = page_launcher_notifList.Children.IndexOf(notif.NotificationListObj);
                                    page_launcher_notifList.Children.Remove(notif.NotificationListObj);

                                    Border newElement = notif.GetNotifListElement(true);
                                    page_launcher_notifList.Children.Insert(Index, newElement);
                                }
                            }
                            else
                            {
                                page_launcher_notifList.Children.Insert(0, notif.GetNotifListElement());
                            }
                        }
                    }

                    foreach(var border in ToRemove)
                    {
                        page_launcher_notifList.Children.Remove(border);
                    }

                    SetUINotificationState();
                }
            });
        }

        public void SetUINotificationState()
        {
            Dispatcher.Invoke(() =>
            {
                if (page_launcher_notifList.Children.Count == 0)
                {
                    page_launcher_notifNone.Visibility = Visibility.Visible;
                    page_launcher_notifHas.Visibility = Visibility.Collapsed;
                }
                else
                {
                    page_launcher_notifNone.Visibility = Visibility.Collapsed;
                    page_launcher_notifHas.Visibility = Visibility.Visible;
                }
            });
        }
        #endregion

        #region Events
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string[] command = btn.Uid.Split(';');
            switch (command[0])
            {
                case "Settings":
                    {
                        switch (command[1])
                        {
                            case "Controls_Assign":
                                {
                                    if ((string)btn.Tag == "0")
                                    {
                                        Controller.MonitorDoneFunc?.Invoke();

                                        Controller.MonitorDoneFunc = () =>
                                        {
                                            this.Dispatcher.BeginInvoke((Action)delegate
                                            {
                                                Dispatcher.Invoke((Action)(() =>
                                                {
                                                    Controller.MonitorDoneFunc = null;
                                                    Controller.MonitorFunc = null;
                                                    btn.Background = new SolidColorBrush(Colors.Transparent);
                                                    btn.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));
                                                    btn.Tag = "0";
                                                    Controller.AddAssignement(Controller.LastCombo, command[2]);
                                                }));
                                            });
                                        };

                                        Controller.MonitorFunc = (keys) =>
                                        {
                                            this.Dispatcher.BeginInvoke((Action)delegate
                                            {
                                                Dispatcher.Invoke((Action)(() =>
                                                {
                                                    btn.Content = Controller.GetKeyName(keys);
                                                }));
                                            });
                                        };

                                        Controller.LastCombo.Clear();
                                        btn.Content = "Press keys";
                                        btn.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xAA, 0x00, 0x00));
                                        btn.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                                        btn.Tag = "1";
                                    }
                                    else
                                    {
                                        Controller.MonitorDoneFunc();
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case "Login_Submit":
                    {
                        Login();
                        break;
                    }
                case "Reconnect":
                    {
                        btn.IsEnabled = false;
                        page_error_code.Text = "Reconnecting..." + Environment.NewLine;
                        //AuthFromToken();
                        break;
                    }
                case "Login":
                    {
                        SwitchPage(pages_collection, page_login, CurrentPage);
                        break;
                    }
                case "Logout":
                    {
                        Logout();
                        break;
                    }
                case "Privacy":
                    {
                        SwitchPage(pages_collection, page_settings, CurrentPage);
                        break;
                    }
                case "Scenery":
                    {
                        SwitchPage(pages_collection, page_scenery, CurrentPage);
                        break;
                    }
                case "ScenerySave":
                    {
                        SwitchPage(pages_collection, page_launcher, CurrentPage);
                        break;
                    }
                case "PrivacySave":
                    {
                        UserData.Save();
                        ValidationLoopLast = -20000;
                        SwitchPage(pages_collection, page_launcher, CurrentPage);
                        break;
                    }
                case "ManageInstall":
                    {
                        SwitchPage(pages_collection, page_install, CurrentPage);
                        break;
                    }
                case "CheckUpdate":
                    {
                        page_launcher_checkupdate_button.IsEnabled = false;
                        App.CheckChannel();

                        UpdateInstance.Check((code) =>
                        {
                            if (code == 0 || code == 28)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    page_launcher_checkupdate_button.IsEnabled = true;
                                });
                            }
                            else if (code == 61)
                            {
                                NotificationService.Add(new Notification()
                                {
                                    UID = 61,
                                    Title = "A new update is available",
                                    Message = "Do you want to install it now?",
                                    Type = NotificationType.Status,
                                    IsTransponder = true,
                                    Act = () =>
                                    {
                                        UpdateInstance.Apply(true, true);
                                        UpdateInstance.ApplyOnQuit = false;
                                    }
                                });
                            }
                        });

                        //install_update_btn.IsEnabled = false;
                        //if (SimConnection.ConnectedInstance != null)
                        //{
                        //    MessageBox.Show("The Skypark will be updated when the simulator is shutdown", "The Skypark Update");
                        //}
                        break;
                    }
                case "Uninstall":
                    {
                        install_uninstall_btn.IsEnabled = false;
                        MessageBoxResult result = MessageBox.Show("Do you want to uninstall The Skypark?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            if (SimConnection.ConnectedInstance != null)
                            {
                                MessageBox.Show("The Skypark will be uninstalled when the simulator is shutdown", "The Skypark Uninstall");
                            }
                            UpdateCheckLast = Convert.ToInt64(App.Timer.ElapsedMilliseconds - UpdateCheckDelay + 3000);
                            SwitchPage(pages_collection, page_unloading, CurrentPage);
                        }
                        else
                        {
                            install_uninstall_btn.IsEnabled = true;
                        }

                        break;
                    }
                case "CancelUninstall":
                    {
                        install_uninstall_btn.IsEnabled = true;
                        SwitchPage(pages_collection, page_launcher, CurrentPage);
                        break;
                    }
                case "InstallSave":
                    {
                        UserData.Save();
                        SwitchPage(pages_collection, page_launcher, CurrentPage);
                        break;
                    }
                case "OpenSkypad":
                    {
                        LaunchSkypad();

                        this.Hide();
                        this.WindowState = WindowState.Minimized;
                        break;
                    }
                case "OpenBrowser":
                    {
                        System.Diagnostics.Process.Start(SkyOSHost);

                        this.Hide();
                        this.WindowState = WindowState.Minimized;
                        break;
                    }
                case "Support":
                    {
                        System.Diagnostics.Process.Start("https://portal.theskypark.com/kb/");
                        break;
                    }
                case "Tutorial":
                    {
                        System.Diagnostics.Process.Start("https://www.youtube.com/channel/UCVjQE1ScvHNb4j0eFLMMOnw/");
                        break;
                    }
                case "SelfTest":
                    {
                        SocketClient OverlayClient = APIBase.ClientCollection.ConnectedClients.Find(x => x.Type == ClientType.Overlay);
                        //APIBase.ClientCollection.SendMessage(App.JSSerializer.Serialize(new Dictionary<string, dynamic>() { { "test", "no-audio" } }), ClientType.All);
                        if (OverlayClient == null && SimConnection.ActiveSim != null)
                        {
                            MessageBox.Show("Self Test Failed: No In-Sim Overlay Connected.", "Self Test Failed");
                        }
                        break;
                    }
                case "SelfTestAudio":
                    {
                        SocketClient OverlayClient = APIBase.ClientCollection.ConnectedClients.Find(x => x.Type == ClientType.Overlay);
                        //APIBase.ClientCollection.SendMessage(App.JSSerializer.Serialize(new Dictionary<string, dynamic>() { { "test", "audio" } }), ClientType.All);
                        AudioFramework.GetEffect("test", true);
                        if (OverlayClient == null && SimConnection.ActiveSim != null)
                        {
                            MessageBox.Show("Self Test Failed: No In-Sim Overlay Connected.", "Self Test Failed");
                        }
                        break;
                    }
                case "WindowClose":
                    {
                        this.WindowState = WindowState.Minimized;
                        this.Hide();
                        break;
                    }
                case "WindowMinimize":
                    {
                        break;
                    }
            }
        }

        private void Button_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Button btn = (Button)sender;
            string[] command = btn.Uid.Split(';');
            switch (command[0])
            {
                case "Settings":
                    {
                        switch (command[1])
                        {
                            case "Controls_Assign":
                                {
                                    if (Controller.MonitorDoneFunc != null)
                                    {
                                        Controller.MonitorDoneFunc();
                                        return;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            if (chk.IsKeyboardFocused)
            {
                string[] command = chk.Uid.Split(';');
                switch (command[0])
                {
                    case "privacy_remote_access":
                    case "privacy_airport_data":
                        {
                            UserData.Set(chk.Uid, Convert.ToString(Convert.ToInt16(chk.IsChecked)), true);
                            break;
                        }
                    case "install_Beta":
                        {
                            break;
                        }
                    case "Settings":
                        {
                            switch (command[1])
                            {
                                case "General":
                                    {
                                        UserData.Set(command[2], Convert.ToString(Convert.ToInt16(chk.IsChecked)), true);
                                        break;
                                    }
                                case "Extra":
                                    {
                                        switch (command[2])
                                        {
                                            case "1":
                                                {
                                                    if ((bool)chk.IsChecked)
                                                    {
                                                        Models.EventBus.EventManager.Flux60 = 0;
                                                        Models.EventBus.EventManager.Flux1 = 0;
                                                        Models.EventBus.EventManager.Flux5s = 0;
                                                    }
                                                    else
                                                    {
                                                        Models.EventBus.EventManager.Flux60 = 1000 / 30;
                                                        Models.EventBus.EventManager.Flux1 = 1000 / 1;
                                                        Models.EventBus.EventManager.Flux5s = 3000;
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtb = (TextBox)sender;
            switch (txtb.Uid)
            {
                case "placeholder_email":
                    {
                        if (txtb.Text.Trim().Length == 0)
                        {
                            page_login_email_placeholder.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            page_login_email_placeholder.Visibility = Visibility.Hidden;
                        }
                        break;
                    }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox txtb = (TextBox)sender;
            if (txtb.IsEnabled)
            {
                switch (txtb.Uid)
                {
                    case "placeholder_email":
                        {
                            if (e.Key == Key.Enter || e.Key == Key.Return)
                            {
                                Login();
                            }
                            break;
                        }
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox txtb = (PasswordBox)sender;
            switch (txtb.Uid)
            {
                case "placeholder_password":
                    {
                        if (txtb.Password.Trim().Length == 0)
                        {
                            page_login_password_placeholder.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            page_login_password_placeholder.Visibility = Visibility.Hidden;
                        }
                        break;
                    }
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            PasswordBox txtb = (PasswordBox)sender;
            if (txtb.IsEnabled)
            {
                switch (txtb.Uid)
                {
                    case "placeholder_password":
                        {
                            if (e.Key == Key.Enter || e.Key == Key.Return)
                            {
                                Login();
                            }
                            break;
                        }
                }
            }
        }
        
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider sld = (Slider)sender;
            string[] command = sld.Uid.Split(';');
            switch (command[0])
            {
                case "Settings":
                    {
                        switch (command[1])
                        {
                            case "Audio_Voices":
                                {
                                    UserData.Set("audio_voices", Math.Round(sld.Value, 3).ToString());
                                    AudioFramework.SetSpeechVolume((float)sld.Value);
                                    break;
                                }
                            case "Audio_Effects":
                                {
                                    UserData.Set("audio_effects", Math.Round(sld.Value, 3).ToString());
                                    AudioFramework.SetEffectVolume((float)sld.Value);
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Slider sld = (Slider)sender;
            string[] command = sld.Uid.Split(';');
            switch (command[0])
            {
                case "Settings":
                    {
                        switch (command[1])
                        {
                            case "Audio_Voices":
                                {
                                    AudioFramework.GetSpeech("characters", "brigit/welcome_big_day", true, true, true);
                                    break;
                                }
                            case "Audio_Effects":
                                {
                                    AudioFramework.GetEffect("completed", true, 0, true, true);
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Slider sld = (Slider)sender;
            string[] command = sld.Uid.Split(';');
            switch (command[0])
            {
                case "Settings":
                    {
                        switch (command[1])
                        {
                            case "Audio_Voices":
                                {
                                    AudioFramework.Stop();
                                    break;
                                }
                            case "Audio_Effects":
                                {
                                    AudioFramework.Stop();
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        internal void Window_Drag(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
                double x = this.Left;
                double y = this.Top;

                Reg_Write.SetValue("transponder_window_pos_x", Convert.ToDouble(this.Left));
                Reg_Write.SetValue("transponder_window_pos_y", Convert.ToDouble(this.Top));

                this.Left = x;
                this.Top = y;
            }
            catch
            {

            }
        }

        public static DateTime GetLinkerTimestampUtc(Assembly assembly)
        {
            var location = assembly.Location;
            return GetLinkerTimestampUtc(location);
        }

        public static DateTime GetLinkerTimestampUtc(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var bytes = new byte[2048];

            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                file.Read(bytes, 0, bytes.Length);
            }

            var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(secondsSince1970);
        }

        private void AppVersion_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DevClick++;
            if (DevClick == 7)
            {
                if (UserData.Get("channel") != "2")
                {
                    MessageBox.Show("Cleared to land 24R, Caution Dev turbulence.", "Uncharted territories...");
                    if (SimConnection.ConnectedInstance != null)
                    {
                        MessageBox.Show("Close your simulator to update...", "But...");
                    }
                    UserData.Save();
                }
                else
                {
                    MessageBox.Show("You are out of the storm, Sky Clear", "Charted territories...");
                    if (SimConnection.ConnectedInstance != null)
                    {
                        MessageBox.Show("Close your simulator to update...", "But...");
                    }
                    UserData.Save();
                }

                DevClick = 0;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Console.WriteLine("-- Last Entry");
            //e.Cancel = true;
            //AskAppShutdown();
        }

        private void MainWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            WiggleArmed = false;
            WiggleMultiplier = 0;
        }
        
        private void ComboBox_Selected(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbb = (ComboBox)sender;
            string[] command = cbb.Uid.Split(';');
            switch (command[0])
            {
                case "Settings":
                    {
                        ComboBoxItem sel = ((ComboBoxItem)cbb.SelectedItem);
                        if(sel != null)
                        {
                            switch (command[1])
                            {
                                case "Audio_Output":
                                    {
                                        string no = sel.Uid.ToString();
                                        if (UserData.Get("audio_output") != no)
                                        {
                                            UserData.Set("audio_output", no);
                                            //AudioFramework.GetEffect("throat", true);
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
            }
        }

        private void StepClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string[] UriSpl = btn.Uid.Split('_');
            switch (UriSpl[0])
            {
                case "+":
                    {
                        Step++;
                        if (UriSpl.Length > 1)
                        {
                            switch (UriSpl[1])
                            {
                                case "Illicit":
                                    {
                                        UserData.Set("illicit", UriSpl[2], true);

                                        break;
                                    }
                                case "LastOnBoard":
                                    {
                                        GoodToGo = true;
                                        ValidationLoopLast = -20000;
                                        UpdatePreferences();
                                        SwitchPage(pages_collection, page_launcher, CurrentPage);
                                        LaunchSkypad();

                                        this.Hide();
                                        this.WindowState = WindowState.Minimized;
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "-":
                    {
                        Step--;
                        break;
                    }
                default:
                    {
                        Step = Convert.ToInt16(btn.Uid);
                        break;
                    }
            }
            StepGoTo(Step);
        }

        internal System.Windows.Forms.Screen WindowAllVisible(Rect windowRectangle)
        {
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                float ratio = 1;
                //uint dpi = GetDpi(screen, DpiType.Effective);
                //if (dpi > 96) ratio = Convert.ToSingle((decimal)dpi / 96M);

                if (((windowRectangle.Top + windowRectangle.Height - 50) * ratio) > screen.WorkingArea.Top) // More than the top of the screen
                {
                    if (((windowRectangle.Top + 50) * ratio) < screen.WorkingArea.Top + screen.WorkingArea.Height) // Less than the bottom of the screen
                    {
                        if (((windowRectangle.Left + windowRectangle.Width - 50) * ratio) > screen.WorkingArea.Left) // More than the left of the screen
                        {
                            if (((windowRectangle.Left + 50) * ratio) < screen.WorkingArea.Left + screen.WorkingArea.Width) // Less than the right of the screen
                            {
                                return screen;
                            }
                        }
                    }
                }

            }
            return null;
        }
        #endregion

        #region Animations
        public void AnimHighlight(string elemName, DependencyObject elementObj, double opacity0, double opacity1, int delay, int duration, Storyboard storyboard = null)
        {
            FrameworkElement element = (FrameworkElement)elementObj;

            Storyboard sb;
            if (storyboard == null)
            {
                if (Storyboards.ContainsKey(elemName))
                {
                    sb = Storyboards[elemName];
                    sb.Stop();
                }
                else
                {
                    sb = new Storyboard();
                }
            }
            else
            {
                sb = storyboard;
                sb.Stop();
            }

            // Set easing
            BackEase ssAnimEasing = new BackEase
            {
                Amplitude = 0.1,
                EasingMode = EasingMode.EaseIn
            };

            // Opacity
            DoubleAnimation opacityAnimation = new DoubleAnimation() { From = opacity0, To = opacity1 };
            opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            opacityAnimation.BeginTime = TimeSpan.FromMilliseconds(delay);
            Storyboard.SetTarget(opacityAnimation, element);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));

            // Set storyboards and animate!
            sb.Children.Add(opacityAnimation);

            sb.Completed += (s, v) =>
            {
                element.Opacity = opacity1;

                sb.Stop();
                sb.Children.Clear();
            };

            sb.Begin();
        }

        public void AnimateImageSequence(string[] imagesPath, Image TargetImage, int last, int skip, int fps, Action Callback)
        {
            Task.Factory.StartNew(() =>
            {
                int i = 0;
                while(i <= last)
                {
                    Uri u = new Uri(@"pack://application:,,,/" + App.ThisApp.GetName().Name + ";component/" + "Resources/" + string.Join("/", imagesPath) + "/" + i.ToString("0000") + ".jpg", UriKind.Absolute);

                    Dispatcher.Invoke(() =>
                    {
                        TargetImage.Source = new BitmapImage(u);
                    });

                    //Task.Delay(1000 / fps);
                    Thread.Sleep(1000 / fps);
                    i += skip;
                }

                Dispatcher.Invoke(() =>
                {
                    Callback();
                });
            }, App.ThreadCancel.Token);
            
            //ResourceSet rsrcSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, true);

            //foreach (var entry in rsrcSet)
            //{
            //}

            //var resourceManager = new ResourceManager(p, Assembly.GetExecutingAssembly());
            //var resources = resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            //foreach (var res in resources)
            //{
            //    System.Console.WriteLine(res);
            //}

            //foreach (string ImagePath in App.ThisApp.GetManifestResourceNames().Where(x => x.StartsWith(p)).ToList())
            //{
            //    string t = ImagePath.Replace(p + ".", "");
            //    string u = @"pack://application:,,,/" + string.Join("/", imagesPath) + "/" + t;
            //    TargetImage.Source = new BitmapImage(new Uri(u));
            //}
        }
        #endregion

        #region Externs
        //private const int SW_SHOWNORMAL = 1;
        //private const int SW_SHOWMINIMIZED = 2;
        //private const int SW_SHOWMAXIMIZED = 3;
        //[DllImport("user32.dll")]
        //private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        //[DllImport("user32.dll")]
        //private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        //[DllImport("user32.dll")]
        //private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        //[DllImport("user32.dll")]
        //private static extern bool SetForegroundWindow(IntPtr hWnd);

        //[DllImport("user32.dll", SetLastError = true)]
        //static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);

        public enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2,
        }

        [DllImport("User32.dll")]
        public static extern IntPtr MonitorFromPoint([In]System.Drawing.Point pt, [In]uint dwFlags);

        [DllImport("Shcore.dll")]
        public static extern IntPtr GetDpiForMonitor([In]IntPtr hmonitor, [In]DpiType dpiType, [Out]out uint dpiX, [Out]out uint dpiY);

        //[DllImport("user32.dll")]
        //static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        //static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        //static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        //static readonly IntPtr HWND_TOP = new IntPtr(0);
        //const UInt32 SWP_NOSIZE = 0x0001;
        //const UInt32 SWP_NOMOVE = 0x0002;
        //const UInt32 SWP_SHOWWINDOW = 0x0040;

        #endregion

    }
}
