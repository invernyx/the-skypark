using TSP_Installer.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

namespace TSP_Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> ArgsList = null;
        private int? ExpitCode = null;
        internal bool ExitReady = false;
        private bool OnBoardReady = false;
        private Grid CurrentPanel = null;
        private short Step = 0;

        public MainWindow(List<string> Args)
        {
            InitializeComponent();
            if(!Args.Contains("silent")) {
                Show();
                EnableBlur();
            }
            ArgsList = Args;

            #region Log Timer
            DispatcherTimer Logtimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            Logtimer.Tick += (o, e1) =>
            {
                App.ConsoleContent.WriteFile();
            };
            Logtimer.IsEnabled = true;
            #endregion

            #region Init Fields
            InstallLocationField.Text = App.InstallDir;
            #endregion

            if(ArgsList.Contains("uninstall"))
            {
                StepGoTo(steps_panel_uninstall, 0);
            }
            else if(App.IsInstalled)
            {
                OnBoardReady = true;
                StepGoTo(steps_panel_onboard, (short)(steps_panel_onboard.Children.Count - 1));
            }
            else
            {
                App.IsInitialInstall = true;
                StepGoTo(steps_panel_onboard, 0);
            }
        }

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

        public void StepGoTo(Grid g, short Index)
        {
            Dispatcher.Invoke(() =>
            {
                List<Grid> lg = new List<Grid>()
                {
                    steps_panel_uninstall,
                    steps_panel_onboard,
                    steps_panel_failed
                };

                foreach (var gr in lg)
                {
                    int At = 0;
                    Step = Index;
                    foreach (FrameworkElement El in gr.Children)
                    {
                        if (At == Index && gr == g)
                        {
                            El.Visibility = Visibility.Visible;
                            CurrentPanel = g;
                        }
                        else
                        {
                            El.Visibility = Visibility.Hidden;
                        }
                        At++;
                    }
                };
            });
               
        }
        
        public void UpdateStatus(string State)
        {
            if (IsVisible)
            {
                Dispatcher.Invoke(() =>
                {
                    if (State != string.Empty)
                    {
                        StatusText.Visibility = Visibility.Visible;
                        StatusText.Text = State;
                    }
                    else
                    {
                        StatusText.Visibility = Visibility.Collapsed;
                        StatusText.Text = String.Empty;
                    }
                });
            }
        }
        
        public void UpdateDownloadUI(Updater Updtr, float Progress, DownloadStages Stage)
        {
            TextBlock Status = Updtr.StateText;
            Grid El = Updtr.ProgressUIBase;
            Border Outline = Updtr.ProgressUIOutline;
            Border FillChecking = Updtr.ProgressUIFillChecking;
            Border FillDownload = Updtr.ProgressUIFillDownload;
            Border FillUnpacking = Updtr.ProgressUIFillUnpacking;
            Border FillInstalling = Updtr.ProgressUIFillInstalling;

            if (IsVisible)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (!ProgressStack.Children.Contains(El))
                        {
                            UpdateStatus(string.Empty);
                            ProgressStack.Children.Add(El);
                        }
                        
                        if (Progress > -1)
                        {
                            double DoneOpacity = 1;
                            double FutureOpacity = 0.6;
                            double WidthChecking = ((Grid)FillChecking.Parent).ColumnDefinitions[0].ActualWidth;
                            double WidthDownload = ((Grid)FillDownload.Parent).ColumnDefinitions[1].ActualWidth;
                            double WidthUnpacking = ((Grid)FillUnpacking.Parent).ColumnDefinitions[2].ActualWidth;
                            double WidthInstalling = ((Grid)FillInstalling.Parent).ColumnDefinitions[3].ActualWidth;
                            
                            if (Outline.ActualWidth > 0)
                            {
                                switch (Stage)
                                {
                                    case DownloadStages.Checking:
                                        {
                                            if (Status.Uid != "0")
                                            {
                                                Status.Uid = "0";
                                                Status.Foreground = FillChecking.Background;
                                            }

                                            Status.Text = "Comparing files... " + (Progress * 100).ToString("0.000") + "%";

                                            FillChecking.Opacity = FutureOpacity;
                                            FillDownload.Opacity = FutureOpacity;
                                            FillUnpacking.Opacity = FutureOpacity;
                                            FillInstalling.Opacity = FutureOpacity;

                                            FillChecking.Width = Progress * WidthChecking;
                                            FillDownload.Width = 0;
                                            FillUnpacking.Width = 0;
                                            FillInstalling.Width = 0;
                                            break;
                                        }
                                    case DownloadStages.Downloading:
                                        {
                                            if (Status.Uid != "1")
                                            {
                                                Status.Uid = "1";
                                                Status.Foreground = FillDownload.Background;
                                            }

                                            Status.Text = "Downloading... " + (Progress * 100).ToString("0.000") + "%";

                                            FillChecking.Opacity = DoneOpacity;
                                            FillUnpacking.Opacity = FutureOpacity;
                                            FillInstalling.Opacity = FutureOpacity;

                                            FillChecking.Width = WidthChecking;
                                            FillUnpacking.Width = 0;
                                            FillInstalling.Width = 0;

                                            if (Progress == 1)
                                            {
                                                Status.Text = "Validating...";
                                                Status.Foreground = FillUnpacking.Background;

                                                FillDownload.Opacity = DoneOpacity;
                                                FillDownload.Width = WidthDownload;
                                            }
                                            else
                                            {
                                                FillDownload.Opacity = FutureOpacity;
                                                FillDownload.Width = Progress * WidthDownload;
                                            }

                                            break;
                                        }
                                    case DownloadStages.Unpacking:
                                        {
                                            if (Status.Uid != "2")
                                            {
                                                Status.Uid = "2";
                                                Status.Foreground = FillUnpacking.Background;
                                            }

                                            Status.Text = "Extracting... " + (Progress * 100).ToString("0.000") + "%";

                                            FillChecking.Opacity = DoneOpacity;
                                            FillDownload.Opacity = DoneOpacity;

                                            FillChecking.Width = WidthChecking;
                                            FillDownload.Width = WidthDownload;

                                            if (Progress == 1)
                                            {
                                                FillUnpacking.Opacity = DoneOpacity;
                                                FillInstalling.Opacity = FutureOpacity;
                                                
                                                FillUnpacking.Width = WidthUnpacking;

                                                if (ArgsList.Contains("silent"))
                                                {
                                                    Status.Text = "Downloaded.";
                                                    Status.Foreground = FillInstalling.Background;
                                                    FillInstalling.Width = Progress * WidthInstalling;
                                                }
                                                else
                                                {
                                                    Status.Text = "Waiting to install...";
                                                    FillInstalling.Width = 0;
                                                }
                                                Status.Foreground = FillInstalling.Background;
                                            }
                                            else
                                            {
                                                FillUnpacking.Opacity = FutureOpacity;
                                                FillInstalling.Opacity = FutureOpacity;

                                                FillUnpacking.Width = Progress * WidthUnpacking;
                                                FillInstalling.Width = 0;
                                            }
                                            break;
                                        }
                                    case DownloadStages.Installing:
                                        {
                                            if (Status.Uid != "3")
                                            {
                                                Status.Uid = "3";
                                                Status.Foreground = FillInstalling.Background;
                                            }

                                            Status.Text = "Installing... " + (Progress * 100).ToString("0.000") + "%";

                                            if (Progress == 1)
                                            {
                                                Status.Text = "Up to date.";
                                            }

                                            FillChecking.Opacity = DoneOpacity;
                                            FillDownload.Opacity = DoneOpacity;
                                            FillUnpacking.Opacity = DoneOpacity;
                                            FillChecking.Width = WidthChecking;
                                            FillDownload.Width = WidthDownload;
                                            FillUnpacking.Width = WidthUnpacking;

                                            if (Progress == 1)
                                            {
                                                FillInstalling.Opacity = DoneOpacity;
                                                FillInstalling.Width = WidthInstalling;
                                            }
                                            else
                                            {
                                                FillInstalling.Opacity = FutureOpacity;
                                                FillInstalling.Width = Progress * WidthInstalling;
                                            }
                                            break;
                                        }
                                    case DownloadStages.Failed:
                                        {
                                            if (Status.Uid != "4")
                                            {
                                                Status.Uid = "4";
                                                Status.Text = Updtr.FailedReason;
                                                Status.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#CC5555");
                                            }
                                            
                                            break;
                                        }
                                }
                            }

                        }
                        else
                        {

                        }
                    }
                    catch
                    {

                    }
                });
            }
        }

        public void Exit(int Code)
        {
            if(ExitReady && OnBoardReady)
            {
                App.SaveConfig();

                if (Code != 28)
                {
                    if (ArgsList.Contains("install"))
                    {
                        App.ExcessCleanup();
                    }

                    if (!ArgsList.Contains("update"))
                    {
                        // Set Transponder Restart
                        switch (Code)
                        {
                            case 0:
                            case 61:
                                {
                                    ArgsList.Add("restart|" + Path.Combine(App.InstallDir, "Transponder", "Launcher.exe") + (!App.IsInstalled ? "&initialize" : ""));
                                    break;
                                }
                        }
                    }

                    if(App.IsInitialInstall)
                    {
                        App.CreateShortcuts(App.Config["install"]);
                    }

                    // Restart Apps
                    string Restart = ArgsList.Find(x => x.StartsWith("restart|"));
                    if (Restart != null)
                    {
                        Console.WriteLine(Environment.NewLine + "**** RESTART ****");

                        string[] RestartProcs = Restart.Split('|')[1].Split('?');
                        foreach (string Proc in RestartProcs)
                        {
                            Console.WriteLine("Restarting app " + Proc);
                            try
                            {
                                string[] Args = Proc.Split('&');
                                ProcessStartInfo PSI = new ProcessStartInfo(Args[0]);
                                PSI.Arguments = "updated " + (Args.Length > 1 ? Args[1] : "");
                                PSI.Verb = "runas";
                                
                                Process.Start(PSI);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to restart app " + Proc + " because " + ex.Message);
                            }
                        }
                    }
                    Thread.Sleep(3000);
                }

                Console.WriteLine(Environment.NewLine + "**** EXIT " + Code + " ****");

                //Thread et1 = new Thread(() =>
                //{
                //    Thread.Sleep(5000);
                //    Process.GetCurrentProcess().Kill();
                //});
                //et1.IsBackground = true;
                //et1.Start();

                //ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;
                //foreach (ProcessThread thread in currentThreads)
                //{
                //    //Console.WriteLine(thread.ThreadState.ToString());
                //    if(thread.ThreadState == System.Diagnostics.ThreadState.Wait)
                //    {
                //        Console.WriteLine(thread.WaitReason.ToString());
                //    }
                //}

                foreach (Updater u in App.Updaters)
                {
                    u.Cleanup();
                }

                Thread et = new Thread(() =>
                {
                    Console.WriteLine("Exiting with code " + Code);
                    
                    //try
                    //{
                    App.ConsoleContent.WriteFile(0);
                        App.ConsoleContent.Disp();

                        System.Windows.Forms.Application.ExitThread();
                    //}
                    //catch
                    //{
                    //}

                    //if (Code != 0)
                    //{
                    //    Thread.Sleep(2000);
                    //}

                    //Thread.Sleep(200);

                    //this.Dispatcher.Invoke(() =>
                    //{
                        //this.Exit(Code);
                        //this.Close();
                        //Process.GetCurrentProcess().Kill();
                        //Application.Current.MainWindow.Close();
                        //Application.Current.Shutdown(Code);
                        Environment.Exit(Code);

                    //});
                });
                et.IsBackground = true;
                et.Name = "Shutdown Thread";
                et.Start();
            }
            else
            {
                //switch (Code)
                //{
                //    case 0:
                //    {
                //        UpdateStatus("All good! 👍");
                //        break;
                //    }
                //}

                ExpitCode = Code;
            }
        }
        
        internal void Window_Drag(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch
            {

            }
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Uid)
            {
                case "WindowClose":
                    {
                        this.Close();
                        break;
                    }
                case "WindowMinimize":
                    {
                        this.WindowState = WindowState.Minimized;
                        break;
                    }
                case "OpenSupport":
                    {
                        try
                        {
                            Process.Start("https://support.tfdidesign.com");
                        }
                        catch
                        {
                        }
                        break;
                    }
                case "BrowseInstall":
                    {
                        CommonOpenFileDialog cofd = new CommonOpenFileDialog();
                        cofd.Title = "A The Skypark folder will be created in this folder";
                        cofd.IsFolderPicker = true;
                        cofd.DefaultDirectory = App.InstallDir;

                        if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                        {
                            App.InstallDir = Path.Combine(cofd.FileName, "The Skypark");
                            InstallLocationField.Text = App.InstallDir;
                        }
                        else
                        {
                        };
                        break;
                    }
                case "LaunchTransponder":
                    {
                        ExitReady = true;
                        ArgsList.Add("restart|" + Path.Combine(App.InstallDir, "Transponder", "Launcher.exe") + "&initialize");
                        Exit(ExpitCode != null ? (int)ExpitCode : 0);
                        break;
                    }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Close();
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox btn = (TextBox)sender;
            switch (btn.Uid)
            {
                case "BrowseInstall":
                    {
                        App.InstallDir = InstallLocationField.Text;
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
                        if(UriSpl.Length > 1)
                        {
                            switch (UriSpl[1])
                            {
                                case "DoneLocation":
                                    {
                                        App.LocationSet = true;
                                        InstallLocationField.IsEnabled = false;
                                        InstallLocationBtn.IsEnabled = false;
                                        App.Config["install"] = App.InstallDir;

                                        foreach (Updater U in App.Updaters)
                                        {
                                            switch (U.Module.ID)
                                            {
                                                case "transponder":
                                                case "topo":
                                                    {
                                                        U.SetInstall(true);
                                                        U.SetInstallDir(Path.Combine(App.InstallDir, "Transponder"));
                                                        break;
                                                    }
                                                case "skyos":
                                                case "skypad":
                                                    {
                                                        U.SetInstall(true);
                                                        U.SetInstallDir(Path.Combine(App.InstallDir, "Skypad"));
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        U.SetInstall(true);
                                                        break;
                                                    }
                                            }
                                        }
                                        break;
                                    }
                                case "LastOnBoard":
                                    {
                                        OnBoardReady = true;
                                        if(ExitReady)
                                        {
                                            if(ExpitCode != 28)
                                            {
                                                Exit((int)ExpitCode);
                                            }
                                            else
                                            {
                                                last_card_close_btn.Visibility = Visibility.Visible;
                                                last_card_support_btn.Visibility = Visibility.Visible;
                                            }
                                        }
                                        break;
                                    }
                                case "Uninstall":
                                    {
                                        App.Uninstall();
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "-":
                    {
                        OnBoardReady = false;
                        Step--;
                        break;
                    }
                default:
                    {
                        Step = Convert.ToInt16(btn.Uid);
                        break;
                    }
            }
            StepGoTo(CurrentPanel, Step);
        }

        public enum DownloadStages
        {
            Checking,
            Downloading,
            Unpacking,
            Installing,
            Failed
        }
    }
}
