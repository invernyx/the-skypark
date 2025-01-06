//#define TESTLIC
#if TESTLIC
#warning TESTLIC is active on Ui.App.xaml.cs. This might prevent some products from working in DEBUG
#endif
using IPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Web.Script.Serialization;
using System.Reflection;
using Microsoft.Win32;
using System.Globalization;
using TSP_Transponder.Models;
using TSP_Transponder.Models.Audio;
using TSP_Transponder.Models.Connectors;
using TSP_Transponder.Utilities;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Payload.Assets;
using TSP_Transponder.Models.Progress;
using TSP_Transponder.Models.FlightPlans;
using TSP_Transponder.Models.Topography;
using TSP_Transponder.Models.EventBus;
using TSP_Transponder.Models.Bank;
using TSP_Transponder.Models.Updater;
using static TSP_Transponder.Models.SimLibrary;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.Transactor;
using TSP_Transponder.Models.Notifications;
using System.Text;
using TSP_Transponder.Models.Messaging;
using TSP_Transponder.Models.Aircraft;
using TSP_Transponder.Models.Dev;

namespace TSP_Transponder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static List<string> Args = new List<string>();
        internal static CultureInfo CI = null;
        internal static Assembly ThisApp = Assembly.GetExecutingAssembly();
        internal static string ThisAssemblyDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        internal static Process CurrentProc = Process.GetCurrentProcess();
        internal static int SystemSeed = -1;
        internal static bool AwaitsLifeReset = false;
        internal static bool IsDev = false;
        internal static bool IsBeta = false;
        internal static bool IsDead = false;
        internal static bool IsAdmin = false;
        internal static bool IsSetup = false;
        internal static bool RegenRoutes = false;
        internal static bool RefusedAdminRestart = false;
        internal static Stopwatch Timer = new Stopwatch();
        internal static UriHandler URIH = null;
        internal static MainWindow MW = null;
        internal static string LastURI = "";
        internal static string BuildNumber = "";
        internal static string PreviousBuildNumber = "";
        internal static string BuildChannel = "";
        internal static DateTime BuildTime = DateTime.UtcNow;
        internal static DateTime PreviousBuildTime = DateTime.UtcNow;
        internal static NamedPipesServer NS = null;
        internal static JavaScriptSerializer JSSerializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        internal static string DocumentsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Parallel 42\The Skypark\v3");
        internal static string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark\v3");
        internal static string LogPath = Path.Combine(AppDataDirectory, "Transponder_Console_" + Utils.TimeStamp(DateTime.UtcNow) + ".txt");
        internal static Thread PrimaryThread = null;
        internal static ConsoleProcessor ConsoleOut = null;
        internal static CancellationTokenSource ThreadCancel = new CancellationTokenSource();

        internal static bool dev_avail = false;
        internal static bool staging_avail = false;
        internal static bool prod_avail = false;

        internal static void TryCaptureLogFile()
        {
            try
            {
                ConsoleOut = new ConsoleProcessor(LogPath);
                Console.SetOut(ConsoleOut);
                Console.SetError(ConsoleOut);
            }
            catch
            {
            }
        }

        // Startup application
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        void App_Startup(object sender, StartupEventArgs e)
        {
            InitBuildNumber();
            CreateAppdataFolder();
            GetSystemSeed();
            Console.WriteLine("Build Number: " + BuildNumber);

            #region Set unified culture
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
            //CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";
            //CultureInfo.CurrentCulture.NumberFormat.NumberNegativePattern = 1;
            //CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            //CultureInfo.CurrentCulture.NumberFormat.CurrencyNegativePattern = 1;
            //CultureInfo.CurrentCulture.NumberFormat.PercentDecimalSeparator = ".";
            //CultureInfo.CurrentCulture.NumberFormat.PercentNegativePattern = 1;
            CI = CultureInfo.CurrentCulture;
            CultureInfo.DefaultThreadCurrentCulture = CI;
            #endregion

            #region Set Process Priority
            CurrentProc.PriorityClass = ProcessPriorityClass.Idle;
            #endregion
            
            #region Register URI Handler
            URIH = new UriHandler();
            URIH.Connect("TSPTransponder");
            #endregion

            #region Gather Command Line Args
            Args = Environment.GetCommandLineArgs().ToList();
            #endregion

            #region Check Admin
            var wp = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (wp.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("Running as Admin");
                IsAdmin = true;
            }
            else
            {
                Console.WriteLine("NOT Running as Admin");
                // Restart program and run as admin
                ProcessStartInfo startInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName, string.Join(" ", Args));
                startInfo.Verb = "runas";
                Process.Start(startInfo);
                Current.Shutdown();
                return;
            }
            #endregion

            #region Check for another instance
            // Check if another instance is running
            if (URIH.IsConnected())
            {
                Process[] OtherProcess = Process.GetProcessesByName("Transponder");
                foreach (Process proc in OtherProcess)
                {
                    try
                    {
                        if (proc.Id != CurrentProc.Id && !proc.HasExited)
                        {
                            URIH.Send("Restore");
                            URIH.Send(string.Join(" ", Args));
                            Current.Shutdown();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to check Admin on process. " + ex.Message);
                    }
                }

                // We pass the URI to the running instance
                //URIH.Send(string.Join(" ", args));

                // Check if we received an URI that we must pass to the other instance
                //if (!args.Contains("silent"))
                //{
                //    // We pass the OnFocus message to the running instance
                //    try
                //    {
                //        URIH.Send("Restore");
                //    }
                //    catch
                //    {
                //        Console.WriteLine("Failed to wake up the other instance");
                //    }
                //}
            }


            if (!URIH.IsConnected())
            {
                // We must register this instance as running
                URIH.Register("TSPTransponder");

                // Check if we received an URI
                if (LastURI != "")
                {
                    // a URI was passed, handle it locally
                    URIH.Receive(LastURI);
                }
            }
            #endregion

            #region Set Exceptions, Assembly Resolve and start Log
            if(Directory.Exists(AppDataDirectory))
            {
                List<string> ConsoleFiles = Directory.GetFiles(AppDataDirectory, "Transponder_Console_*").OrderByDescending(x => x).ToList();
                int c = 0;
                foreach (string file in ConsoleFiles)
                {
                    try
                    {
                        FileInfo FI = new FileInfo(file);
                        if (c > 10 || FI.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-5))
                        {
                            File.Delete(file);
                        }
                    }
                    catch
                    {
                    }
                    c++;
                }
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleAppException);
            TryCaptureLogFile();
            #endregion
            
            #region Check if we need to On-Board
            if (Args.Contains("initialize"))
            {
                IsSetup = true;
            }
            #endregion

            #region Get Orbx
            //https://www.binaryhexconverter.com/hex-to-ascii-text-converter
            string orbxConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Orbx", "Central", "central.json");
            if (File.Exists(orbxConfigPath))
            {
                try
                {
                    var content = JSSerializer.Deserialize<Dictionary<string, dynamic>>(File.ReadAllText(orbxConfigPath));
                    Console.WriteLine(BitConverter.ToString(Encoding.Default.GetBytes(content["auth"]["email"])).Replace("-", ""));
                }
                catch
                {
                    Console.WriteLine("Failed to read Orbx Config");
                }
            }
            #endregion

            #region Is Dev/Beta
            CheckChannel();
            #endregion

            #region Check if we are regen routes
            if (Directory.Exists(Path.Combine(AppDataDirectory, "Persistence")))
            {
                var fCount = Directory.GetFiles(Path.Combine(AppDataDirectory, "Persistence")).Length;
                if (fCount == 0)
                {
                    RegenRoutes = true;
                }
            }
            #endregion

            #region Gegister URI
            RegisterURIScheme();
            #endregion
            
            #region Setup Primary Thread
            PrimaryThread = new Thread(() =>
            {
                CultureInfo.CurrentCulture = CI;

                #region Set Temp folder
                string TempDir = Path.Combine(Path.GetTempPath(), "e2cef799-1d96-477d-96c9-321e5376714e");
                if(!Directory.Exists(TempDir))
                {
                    Directory.CreateDirectory(TempDir);
                }
                if (Directory.Exists(Path.Combine(AppDataDirectory, "Temp")))
                {
                    Directory.Delete(Path.Combine(AppDataDirectory, "Temp"), true);
                }
                #endregion

                #region Deploy DLLs
                List<string> ListDeploy = ThisApp.GetManifestResourceNames().Where(x => x.StartsWith("TSP_Transponder.Deploy.")).ToList();
                foreach (string dll in ListDeploy)
                {
                    using (var resource = ThisApp.GetManifestResourceStream(dll))
                    {
                        try
                        {
                            using (var file = new FileStream(Path.Combine(TempDir, dll.Replace("TSP_Transponder.Deploy.", "")), FileMode.Create, FileAccess.Write))
                            {
                                resource.CopyTo(file);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Failed to deploy " + dll);
                            MessageBox.Show("Another instance of Transponder is already running.");
                            Environment.Exit(0);
                        }
                    }
                }

                List<string> List = ThisApp.GetManifestResourceNames().Where(x => x.StartsWith("TSP_Transponder.Assembly.")).ToList();
                foreach (string dll in List)
                {
                    EmbeddedAssembly.Load(dll, dll.Replace("TSP_Transponder.Assembly.", ""));
                }
                #endregion

                #region Set license
                UpdateInstance.SetProduct("TSP");
                #endregion
                
                #region Start EventBus
                Models.EventBus.EventManager.ReadSession(0);
                #endregion

                #region Start MainWindow
                MW = new MainWindow(Args);
                SimLibrary.Startup();
                #endregion

                #region Load User Data
                if (!UserData.Startup(MW))
                {
                    MW.Shutdown();
                    return;
                };
                GoogleAnalyticscs.Startup(MW);
                #endregion

                #region Check Updates
                if (!IsSetup)
                {
                    MW.Dispatcher.Invoke(() =>
                    {
                        MW.page_launcher_checkupdate_button.IsEnabled = false;
                    });

                    UpdateInstance.CheckReady((code) =>
                    {
                        MW.Dispatcher.Invoke(() =>
                        {
                            MW.page_launcher_checkupdate_button.IsEnabled = true;
                        });

                        if (code == 0 || code == 28)
                        {
                            //MW.Dispatcher.Invoke(() =>
                            //{
                                //    MW.CheckUpdateButton.IsEnabled = true;
                                //    MW.CheckUpdateButton.Content = MW.CheckUpdateButton.Tag;
                            //});
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
                }
                #endregion
                
                #region Create Named Pipe Server to IPC
                try
                {
                    NS = new NamedPipesServer()
                    {
                        Response = (param) =>
                        {
                            switch (param)
                            {
                                case "hello":
                                    {
                                        NS.Send("Restore");
                                        break;
                                    }
                            }
                        }
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to create Named Pipe: " + ex.Message);
                }
                #endregion

                #region Set Version Variables
                switch(CheckLastVersion())
                {
                    case 0:
                        {
                            break;
                        }
                    case 1:
                        {
                            Dispatcher.BeginInvoke((Action)(() =>
                            {
                                Shutdown();
                            }));
                            return;
                        }
                    case 2:
                        {
                            RestartAdmin(string.Join(" ", Args));
                            Environment.Exit(0);
                            return;
                        }
                }
                #endregion

#if DEBUG
                DevProcess.Startup_Pre();
#endif

                #region Startup Modules
                UpdateMechanism();
                LiteDbService.Startup();
                Contacts.Startup();

                foreach (Simulator sim in SimList)
                {
                    sim.Startup();
                }

                Plans.Startup();
                LocationHistory.Startup();
                Progress.Startup();
                Bank.Startup();
                FleetService.Startup();

                APIBase.FindHost();
                APIBase.Startup(MW);

                Progress.RestoreLegacyPersistence(false);
                Bank.RestoreLegacyPersistence(false);
                Models.EventBus.EventManager.RestorePersistence();

                AudioFramework.Startup();
                CargoAssetsLibrary.Init();
                
#if DEBUG
                DevProcess.Startup_Post();
#endif

                MW.UpdatePreferences();
                MW.RepositionWindow();

                Timer.Start();

                SimConnection.Startup(MW);
                #endregion
                
                #region Version
                if (IsDev)
                {
                    BuildChannel = "D";
                }
                else if (IsBeta)
                {
                    BuildChannel = "B";
                }
                else if (File.Exists(Path.Combine(AppDataDirectory, "TEST.txt")))
                {
                    BuildChannel = "TEST CHANNEL";
                }
                else
                {
                    string channel = UserData.Get("channel");
                    switch (channel)
                    {
                        case "rc":
                            {
                                BuildChannel = "RC";
                                break;
                            }
                        case "prod":
                            {
                                BuildChannel = "P";
                                break;
                            }
                    }
                }
                #endregion

                #region Init UI
                if (IsSetup)
                {
                    MW.SwitchPage(MW.pages_collection, MW.page_onboard, MW.CurrentPage);
                    MW.StepGoTo(0);
                }
                else
                {
                    #region Login
                    //if (UserData.Get("token") != string.Empty)
                    //{
                    //MW.AuthFromToken();
                    //}
                    //else
                    //{
                    //MW.Logout();
                    //}
                    MW.GoodToGo = true;
                    MW.SwitchPage(MW.pages_collection, MW.page_launcher, MW.CurrentPage);
                    #endregion
                }
                #endregion
                
                MW.appVersion.Text = BuildNumber + ' ' + BuildChannel;
                Console.WriteLine("Build: " + MW.appVersion.Text);
                System.Windows.Threading.Dispatcher.Run();
            });
            PrimaryThread.IsBackground = true;
            PrimaryThread.SetApartmentState(ApartmentState.STA);
            PrimaryThread.Start();
            #endregion

            #region Wait for MW to initialize
            while (MW == null)
            {
                Thread.Sleep(100);
            }
            #endregion

            if (IsDead)
            {
                return;
            }

            #region Startup modules
            Timezones.Startup();
            Countries.Startup();
            Topo.Startup();
            RichPresence.Startup();
            Controller.Startup();
            MW.Set_Timer_State(true);
            MW.SetupTrayIcon();
            #endregion

        }
        
        private static void GetSystemSeed()
        {
            string HID = "Unknown";
            
            try
            {
                HID = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Cryptography", "MachineGuid", null).ToString();

                SystemSeed = 0;
            }
            catch
            {
            }

        }

        private static void UpdateMechanism()
        {
            string AppdataPersist = Path.Combine(AppDataDirectory, "Persistence");
            if(PreviousBuildTime < new DateTime(637440870664895538))
            {
                if(Directory.Exists(AppdataPersist)) {
                    Directory.Delete(AppdataPersist, true);
                }
            }
        }

        private static int CheckLastVersion()
        {
            try
            {
                PreviousBuildNumber = UserData.Get("llb");
                PreviousBuildTime = new DateTime(Convert.ToInt64(UserData.Get("llv")));

                UserData.Set("llb", BuildNumber);
                UserData.Set("llv", Convert.ToString(BuildTime.Ticks), true);

                int VersionReturn = Models.PostProcessing.UpgradeProcess.VersionUpgrade();
                if (VersionReturn == 0 || VersionReturn == 2)
                {
                    return VersionReturn;
                }
                else
                {
                    MessageBox.Show("Please reinstall The Skypark", "The Skypark Updater");
                    return VersionReturn;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                MessageBox.Show("Please reinstall The Skypark", "The Skypark Updater");
                return 1;
            }
        }

        internal static void CheckChannel()
        {
            #region Is Dev
            IsDev = File.Exists(Path.Combine(AppDataDirectory, "DEV.txt")) && File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parallel 42", "DEV.txt"));
            if(IsDev)
            {
                string OriginalDocumentsFolder = DocumentsDirectory;
                string OriginalAppdataFolder = AppDataDirectory;
                DocumentsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Parallel 42\The Skypark DEV\v3");
                AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\v3");

                if (!Directory.Exists(DocumentsDirectory) && Directory.Exists(OriginalDocumentsFolder))
                {
                    Utils.CopyDir(OriginalDocumentsFolder, DocumentsDirectory);
                }

                if (!Directory.Exists(AppDataDirectory) && Directory.Exists(OriginalAppdataFolder))
                {
                    Utils.CopyDir(OriginalAppdataFolder, AppDataDirectory);
                }

            }
            #endregion

            #region Is Beta
            IsBeta = File.Exists(Path.Combine(AppDataDirectory, "BETA.txt"));
            #endregion
        }

        internal static void CreateAppdataFolder()
        {
            if (!Directory.Exists(AppDataDirectory))
            {
                Directory.CreateDirectory(AppDataDirectory);
            }

            if (!Directory.Exists(DocumentsDirectory))
            {
                Directory.CreateDirectory(DocumentsDirectory);
            }
        }
        
        internal static string InitBuildNumber()
        {
            string BN = TSP_Transponder.Properties.Resources.ResourceManager.GetString("BuildNumber").Trim();
            string[] BNL = BN.Split('\n');
            BuildNumber = BNL[0].Trim();
            BuildTime = DateTime.Parse(BNL[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return BN;
        }

        internal static string GetBuildDate()
        {
            return TSP_Transponder.Properties.Resources.ResourceManager.GetString("BuildDate").Trim();
        }

        internal static bool RestartAdmin(string arg = "")
        {
            try
            {
                DirectoryInfo SelfDir = new FileInfo( CurrentProc.MainModule.FileName).Directory;
                string LauncherPath = Path.Combine(SelfDir.FullName, "Launcher.exe");

                if(File.Exists(LauncherPath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(LauncherPath, "")
                    {
                        Verb = "runas"
                    };
                    startInfo.Arguments = arg;
                    Process.Start(startInfo);
                    return true;
                }
                else
                {
                    MessageBox.Show("The Skypark Transponder was unable to restart as Admin. Please launch the Transponder manually as admin.", "The Skypark Transponder");
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("The Skypark Transponder requires Admin permissions to work properly.", "The Skypark Transponder");
                RefusedAdminRestart = true;
                return false;
            }
        }
        
        private static void KillProcess(string proc)
        {
            foreach (var process in Process.GetProcessesByName(proc))
            {
                try
                {
                    process.Kill();
                }
                catch
                {

                }
            }
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            
            if (!ExceptionHandler(e.Exception.Message))
            {
                if (!IsDead)
                {
                    IsDead = true;
                    CreateCrashLog(e.Exception);

                }
            }

        }

        private static void HandleAppException(object sender, UnhandledExceptionEventArgs args)
        {
            if (!IsDead)
            {
                IsDead = true;
                CreateCrashLog((Exception)args.ExceptionObject);

            }
        }
        
        private static bool ExceptionHandler(string exMsg)
        {
            return false;
        }
        
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        private static void CreateCrashLog(Exception e)
        {
            string RunningSim = "No Running Sim";
            if (SimConnection.ActiveSim != null)
            {
                RunningSim = SimConnection.ActiveSim.ToString();
            }

            LiteDbService.DisposeAll();

            DateTime time = DateTime.UtcNow;
            using (StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\TSP_Transponder_CrashReport_" + Utils.TimeStamp(DateTime.UtcNow) + ".txt", true))
            {
                writer.WriteLine("-- REPORT START --------------------------------------------------------------------" + Environment.NewLine);

                //writer.WriteLine("User:             " + UserData.AccountDetails["account_handle"]);
                //writer.WriteLine("Install Location: " + UserData.Get("install"));
                writer.WriteLine("Time (UTC):       " + time);
                writer.WriteLine("Launch (s):       " + Timer.ElapsedMilliseconds / 1000);
                writer.WriteLine("Sim:              " + RunningSim);
                writer.WriteLine("Build:            " + BuildNumber);
                //writer.WriteLine("Channel:          " + UserData.Get("channel"));
                writer.WriteLine("Memory:           " + String.Format("{0:##.##}", CurrentProc.PrivateMemorySize64 / 1073741824.0) + " GB");
                writer.WriteLine("Simulators: ");
                try
                {
                    foreach (var sim in SimList)
                    {
                        if (sim.IsInstalled)
                        {
                            writer.WriteLine('\t' + sim.ToString() + " \"" + sim.InstallDirectory + "\"");
                        }
                    }
                }
                catch
                {

                }
                writer.WriteLine(Environment.NewLine + "Exception: " + Environment.NewLine + e.Message + Environment.NewLine);
                writer.WriteLine("StackTrace: " + Environment.NewLine + e.StackTrace + Environment.NewLine + Environment.NewLine);
                try
                {

                    writer.WriteLine("Console: " + ConsoleOut.Get());
                }
                catch (Exception ex)
                {
                    writer.WriteLine("Failed to get Console: " + ex.Message);
                }
                writer.WriteLine(Environment.NewLine + "-- REPORT END ----------------------------------------------------------------------");
            }

            System.Windows.Forms.DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("There was an issue with the Skypark Transponder. (Build " + App.BuildNumber + ") \n\nA Crash Report has been generated on your desktop, please send it to us via our Ticket system. \n\nDo you want to restart the Transponder?", "Restart the Skypark Transponder?", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                RestartAdmin(Environment.CommandLine);
            }
            else if (dialogResult == System.Windows.Forms.DialogResult.No)
            {
                Environment.Exit(1);
            }
        }

        internal static string ReadResourceFile(string filename)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            using (var stream = thisAssembly.GetManifestResourceStream(filename))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static bool EztractResourceZip(string filename, string directory)
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(filename))
                {
                    using (ZipArchive archive = new ZipArchive(stream))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string extractDirectoryPath = Path.GetDirectoryName(Path.Combine(directory, entry.FullName));

                            if (entry.Name != string.Empty)
                            {
                                string extractFullPath = Path.Combine(extractDirectoryPath, entry.Name);
                                Directory.CreateDirectory(extractDirectoryPath);
                                entry.ExtractToFile(extractFullPath, true);
                            }
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        internal static bool OwnsProduct(string ProductID)
        {
            bool Passed = false;
#if !DEBUG || TESTLIC
            switch (ProductID)
            {
                case "TSP":
                    {
                        Passed = OwnedPackages.Contains(LicenseApplication.TheSkypark);
                        break;
                    }
            }
#else
            Passed = true;
#endif
            return Passed;
        }
        
        // Register URI Scheme
        internal static bool RegisterURIScheme()
        {
            // Add scheme to registry
            try
            {
                // Convert path from / to \
                Assembly ASS = Assembly.GetExecutingAssembly();
                string correctPath = ASS.Location.Replace(@"/", @"\");

                // Opening the registry key
                RegistryKey rk = Registry.ClassesRoot;
                RegistryKey key = rk.CreateSubKey("skypark");
                key.SetValue("", "The Skypark Protocol");
                key.SetValue("URL Protocol", "");
                key.CreateSubKey("DefaultIcon").SetValue("", correctPath + ",0");
                key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + correctPath + "\" \"%1\"");
                key.Close();
            }
            catch
            {
                Console.WriteLine("Unable to register the URI Scheme FSFXCP to registry");
                return false;
            }
            return true;
        }
        #region Externs
        public static class ProcessHelper
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            private static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr hObject);

            private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
            private const int TOKEN_ASSIGN_PRIMARY = 0x1;
            private const int TOKEN_DUPLICATE = 0x2;
            private const int TOKEN_IMPERSONATE = 0x4;
            private const int TOKEN_QUERY = 0x8;
            private const int TOKEN_QUERY_SOURCE = 0x10;
            private const int TOKEN_ADJUST_GROUPS = 0x40;
            private const int TOKEN_ADJUST_PRIVILEGES = 0x20;
            private const int TOKEN_ADJUST_SESSIONID = 0x100;
            private const int TOKEN_ADJUST_DEFAULT = 0x80;
            private const int TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_SESSIONID | TOKEN_ADJUST_DEFAULT);

            public static bool IsProcessOwnerAdmin(Process proc)
            {
                IntPtr ph = IntPtr.Zero;

                OpenProcessToken(proc.Handle, TOKEN_ALL_ACCESS, out ph);

                WindowsIdentity iden = new WindowsIdentity(ph);

                bool result = false;

                foreach (IdentityReference role in iden.Groups)
                {
                    if (role.IsValidTargetType(typeof(SecurityIdentifier)))
                    {
                        SecurityIdentifier sid = role as SecurityIdentifier;

                        if (sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) || sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid))
                        {
                            result = true;
                            break;
                        }
                    }
                }

                CloseHandle(ph);

                return result;
            }
        }
        #endregion
        
    }
}
