using TSP_Installer.Models;
using TSP_Installer.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using System.Security.Principal;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Web.Script.Serialization;
using Microsoft.Win32;
using static TSP_Installer.LicensingManager;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace TSP_Installer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static MainWindow MW = null;
        internal static List<string> ArgsList = null;
        internal static DateTime DT = DateTime.UtcNow;
        internal static string CacheTime = DT.DayOfYear.ToString() + DT.Hour.ToString() + DT.Minute.ToString() + DT.Second.ToString();
        internal static Assembly ThisApp = System.Reflection.Assembly.GetExecutingAssembly();
        internal static List<Updater> Updaters = new List<Updater>();
        internal static Random Rnd = new Random();
        internal static ControlWriter ConsoleContent = null;
        internal static JavaScriptSerializer JSSerializer = new JavaScriptSerializer() { MaxJsonLength = int.MaxValue };
        internal static string ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark\v3\config.json");
        internal static string AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark\v3\");
        internal static Dictionary<string, string> Config = new Dictionary<string, string>() { { "install", "" } };
        internal static string DefaultInstallDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Parallel 42\The Skypark\v3\";
        internal static Guid UninstallGuid = new Guid(new byte[] { 65, 23, 2, 8, 21, 78, 45, 26, 13, 1, 9, 18, 33, 0, 31, 55 });
        internal static string InstallDir = "";
        internal static bool IsInitialInstall = false;
        internal static bool IsInstalled = false;
        internal static string Channel = "prod";
        internal static bool IsDev = false;
        internal static bool LocationSet = false;
        internal static string VersionChannel = "3";
        internal static string VersionDownload = "3";

        #region Products
        List<UpdateInfo> Products = new List<UpdateInfo>()
        {
            new UpdateInfo()
            {
                Name = "Skypark",
                //Lic = LicenseApplication.Skypark,
                ManifestsURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/%channel%/manifests.zip?t=" + CacheTime,
                Modules = new List<UpdateModule>()
                {
                    new UpdateModule()
                    {
                        Name = "Transponder",
                        ID = "transponder",
                        CanDropConsole = true,
                        DownloadURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/%channel%/transponder.zip?t=" + CacheTime,
                        //CleanupExcess = true,
                        //CleanupExcessExcludeRoot = true,
                    },
                    new UpdateModule()
                    {
                        Name = "Skypad",
                        ID = "skypad",
                        CanDropConsole = true,
                        DownloadURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/%channel%/skypad.zip?t=" + CacheTime,
                        CleanupExcess = true,
                        //CleanupExcessExcludeRoot = true,
                    },
                    new UpdateModule()
                    {
                        Name = "SkyOS",
                        ID = "skyos",
                        CanDropConsole = true,
                        DownloadURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/%channel%/skyos.zip?t=" + CacheTime,
                        //CleanupExcess = true,
                        //CleanupExcessExcludeRoot = true,
                    },
                    new UpdateModule()
                    {
                        Name = "Topography",
                        ID = "topo",
                        CanDropConsole = true,
                        SkipValidate = true,
                        DownloadURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/common/topo.zip?t=" + CacheTime,
                        //CleanupExcess = true,
                        //CleanupExcessExcludeRoot = true,
                    },
                    //new UpdateModule()
                    //{
                    //    Name = "Content",
                    //    ID = "content",
                    //    CanDropConsole = true,
                    //    DownloadURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/%channel%/content.zip?t=" + CacheTime,
                    //    CleanupExcess = true,
                    //    CleanupExcessExcludeRoot = true,
                    //},
                },
            },
            new UpdateInfo()
            {
                Name = "Skypark Adventures",
                //Lic = LicenseApplication.Skypark,
                ManifestsURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/%channel%/adventures_manifests.zip?t=" + CacheTime,
                Modules = new List<UpdateModule>()
                {
                    new UpdateModule()
                    {
                        Name = "Adventures",
                        ID = "adventures",
                        CanDropConsole = true,
                        ManifestSubDirName = "Manifests_Adv",
                        DownloadURL = "https://storage.googleapis.com/gilfoyle/the-skypark/%version%/%channel%/adventures.zip?t=" + CacheTime,
                        //CleanupExcessExcludeRoot = true,
                    },
                },
            },
        };
        #endregion

        static internal Dictionary<string, List<string>> ExcessCleanupIndex = new Dictionary<string, List<string>>();

        void App_Startup(object sender, StartupEventArgs e)
        {
            ArgsList = e.Args.ToList();

            IsDev = File.Exists(Path.Combine(AppDataDirectory, "DEV.txt")) && File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parallel 42", "DEV.txt"));

            #region Redirect ConsoleWriter
            ConsoleContent = new ControlWriter("");
#if DEBUG
            //Console.SetOut(ConsoleContent);
#else
            Console.SetOut(ConsoleContent);
#endif
            #endregion

            #region Check Admin
            try
            {
                var wp = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                if (!wp.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    // Restart program and run as admin
                    var exeName = Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                    {
                        Verb = "runas",
                    };
                    for (int i = 0; i < e.Args.Length; i++) // Loop through array
                    {
                        startInfo.Arguments += '"' + e.Args[i] + '"' + " ";
                    }
                    Console.WriteLine("App is being relaunched as Admin...");
                    Process.Start(startInfo);
                    Current.Shutdown();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("The //42 Update requires to be launched as Admin");
                Current.Shutdown();
                return;
            }
            #endregion

            #region Read Settings
            if (File.Exists(ConfigPath))
            {
                try
                {
                    using (StreamReader r = new StreamReader(new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        var json = r.ReadToEnd();
                        Config = JSSerializer.Deserialize<Dictionary<string, string>>(json);
                        if(Config.ContainsKey("install"))
                        {
                            if(Directory.Exists(Config["install"]))
                            {
                                IsInstalled = true;
                                InstallDir = Config["install"];
                            }
                        }
                        else
                        {
                            Config.Add("install", "");
                        }
                    }
                }
                catch
                {

                }
            }
            #endregion

            #region Uninstall check
            if(InstallDir == string.Empty && ArgsList.Contains("uninstall"))
            {
                ArgsList.Remove("uninstall");
            }

            if (ArgsList.Contains("uninstall"))
            {
                if(Config["install"] != string.Empty)
                {
                    DirectoryInfo DI = new DirectoryInfo(Config["install"]);
                    if (ThisApp.Location.StartsWith(DI.FullName))
                    {
                        string uninstPath = Path.Combine(Path.GetTempPath(), "0a555242-77ca-4bf5-b939-a2366ae347d2");
                        string exePath = Path.Combine(uninstPath, "The Skypark Uninstaller.exe");
                        if (!Directory.Exists(uninstPath))
                        {
                            Directory.CreateDirectory(uninstPath);
                        }
                        try
                        {
                            if (File.Exists(exePath))
                            {
                                File.Delete(exePath);
                            }
                            File.Copy(ThisApp.Location, exePath);
                        }
                        catch
                        {
                        }
                        ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
                        {
                            Verb = "runas",
                        };
                        for (int i = 0; i < e.Args.Length; i++) // Loop through array
                        {
                            startInfo.Arguments += '"' + e.Args[i] + '"' + " ";
                        }
                        Process.Start(startInfo);
                        Current.Shutdown();
                        return;
                    }
                }
                else
                {
                    Current.Shutdown();
                    return;
                }
            }
            #endregion

            #region Set Channel
            if (Config.ContainsKey("channel"))
            {
                Channel = Config["channel"];
            }
            if (ArgsList.Contains("beta"))
            {
                Channel = "beta";
            }
            if (ArgsList.Contains("rc"))
            {
                Channel = "rc";
            }
            if (ArgsList.Contains("dev") || IsDev)
            {
                Channel = "dev";
            }
            if (ArgsList.Contains("test"))
            {
                Channel = "test";
            }
            #endregion

            #region Set Install Arguments
            if (!ArgsList.Contains("update"))
            {
                if (InstallDir == "") { InstallDir = DefaultInstallDir; }
                ArgsList.Add("download");

                ArgsList.Add(@"adventures|" + Channel + "|" + Path.Combine(AppDataDirectory).TrimEnd('\\'));
                ArgsList.Add(@"transponder|" + Channel + "|" + Path.Combine(InstallDir, "Transponder").TrimEnd('\\'));
                ArgsList.Add(@"skypad|" + Channel + "|" + Path.Combine(InstallDir, "Skypad").TrimEnd('\\'));
                ArgsList.Add(@"skyos|" + Channel + "|" + Path.Combine(InstallDir, "Skypad").TrimEnd('\\'));
                ArgsList.Add(@"topo|" + Channel + "|" + Path.Combine(InstallDir, "Transponder").TrimEnd('\\'));

                if (!ArgsList.Contains("silent"))
                {
                    ArgsList.Add(@"waitfor|transponder:Transponder");
                    ArgsList.Add(@"waitfor|skypad:Skypad");
                    if (IsInstalled)
                    {
                        ArgsList.Add("install");
                    }
                    else
                    {
                        ArgsList.Add("force");
                    }
                }
            }

            /*
            download
            install
            force
            "imm_manager|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_p10|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_p10_tex_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p10_tex_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p11|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_p11_tex_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p11_tex_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p12|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_p12_tex_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p12_tex_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p13|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_p13_tex_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p13_tex_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p14|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_p14_tex_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p14_tex_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p15|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_p15_tex_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_p15_tex_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_pSFX|dev|C:\Program Files\Parallel 42\Immersion Manager"
            "imm_pSFX_tex_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_pSFX_tex_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_textures_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_simobjects_p3dv4|dev|B:\FS\P3D4\Parallel 42\Immersion Manager\Live\SimObjects"
            "imm_textures_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\Effects\Texture"
            "imm_simobjects_p3dv5|dev|B:\FS\P3D5\Parallel 42\Immersion Manager\Live\SimObjects"
            "waitfor|immersion manager:Immersion Manager"
            */
            #endregion

            // W7 fix for downloads
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.DefaultConnectionLimit = 9999;

            List<string> List = ThisApp.GetManifestResourceNames().Where(x => x.StartsWith("TSP_Installer.Assembly.")).ToList();
            foreach (string dll in List)
            {
                Console.WriteLine("Loading Assembly " + dll);
                EmbeddedAssembly.Load(dll, dll.Replace("TSP_Installer.Assembly.", ""));
            }

            //

            // Registering for UnhandledException
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(AppDomain_UnhandledException);

            // Registering for AssemblyResolve
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            // Sets the CurrentCulture property to U.S. English.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            MW = new MainWindow(ArgsList);

            if(!ArgsList.Contains("uninstall"))
            {
                Thread DlThread = new Thread(() =>
                {
                    // Wait for apps to exit
                    string WaitFor = ArgsList.Find(x => x.StartsWith("waitfor|"));
                    if (WaitFor != null)
                    {
                        string[] WaitForProcs = WaitFor.Split('|')[1].Split('?');
                        foreach (string Proc in WaitForProcs)
                        {
                            string[] ProcParams = Proc.Split(':');
                            Process[] ps = Process.GetProcessesByName(ProcParams[0]);
                            foreach (Process p in ps)
                            {
                                if (!p.HasExited)
                                {
                                    Console.WriteLine("Waiting for " + ProcParams[1] + " shutdown.");
                                    MW.UpdateStatus("Waiting for " + ProcParams[1] + " shutdown.");
                                    p.WaitForExit();
                                }
                            }
                        }
                    }

                    if (ArgsList.Contains("download"))
                    {
                        MW.UpdateStatus("Looking for Updates...");
                    }

                    List<LicenseApplication> Owned = GetValidApps();

                    // Start the updaters
                    bool HasValid = false;
                    foreach (UpdateInfo Product in Products)
                    {
                        if (Product.Lic != LicenseApplication.None && !Owned.Contains(Product.Lic))
                        {
                            continue;
                        }

                        foreach (UpdateModule Module in Product.Modules)
                        {
                            try
                            {
                                if (Module.Lic != LicenseApplication.None && !Owned.Contains(Module.Lic))
                                {
                                    continue;
                                }

                                HasValid = true;
                                string InstallPathArg = ArgsList.Find(x => x.StartsWith(Module.ID + "|"));
                                string InstallPath = Module.InstallPath;
                                string InstallChannel = "prod";

                                if (InstallPathArg != null)
                                {
                                    string[] InstallPathArgs = InstallPathArg.Trim('"').Split('|');
                                    InstallChannel = InstallPathArgs[1];
                                    if (InstallPathArgs.Length > 2)
                                    {
                                        InstallPath = InstallPathArgs[2];
                                    }
                                    if (Module.CanDropConsole)
                                    {
                                        Product.ConsoleDir = Path.Combine(InstallPath, "Updater Logs");
                                    }
                                }
                                else
                                {
                                    continue;
                                }

                                if (Product.Lic != LicenseApplication.None && !Owned.Contains(Product.Lic))
                                {
                                    Console.WriteLine("Missing Lic for " + Product.Name);
                                    Console.WriteLine(Owned.Count + " Installed Licenses");

                                    Updaters.Clear();
                                    MW.Exit(77);
                                    return;
                                }

                                Updater U = new Updater();
                                U.SetProduct(Product);
                                U.SetModule(Module);
                                U.SetInstallDir(InstallPath);
                                U.SetInstall(ArgsList.Contains("install"));
                                U.SetManifestDir(Module.ManifestSubDirName);
                                U.SetChannel(InstallChannel, "v" + VersionDownload);
                                U.SetExcessCleanup(Module.CleanupExcess, Module.CleanupExcessExcludeRoot);
                                Updaters.Add(U);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Failed to create Update for " + Module.Name + " (" + Module.ID + ")" + " - " + ex.Message);
                            }
                        }
                    }

                    if(!HasValid)
                    {
                        MW.StepGoTo(MW.steps_panel_failed, 0);
                    }


                    // Execute!
                    try
                    {
                        foreach(Updater U in Updaters)
                        {
                            MW.Dispatcher.Invoke(() =>
                            {
                                U.MakeProgressEl();
                            }, DispatcherPriority.ContextIdle);
                        }

                        Parallel.ForEach(Products, (U0) =>
                        {
                            foreach (Updater U in Updaters.FindAll(x => x.Product == U0).ToList())
                            {
                                if (ArgsList.Contains("download"))
                                {
                                    U.Download(ArgsList.Contains("force"));
                                }
                                else if (ArgsList.Contains("isready"))
                                {
                                    U.IsReady();
                                }
                                else if (ArgsList.Contains("install"))
                                {
                                    U.Install();
                                }
                            }
                        });
                    }
                    catch
                    {
                    }
                });

                DlThread.IsBackground = true;
                DlThread.Start();
            }
        }


        private static void RegisterProgram(string location)
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                if (parent == null)
                {
                    throw new Exception("Uninstall registry key not found.");
                }
                try
                {
                    RegistryKey key = null;

                    try
                    {
                        //long InstallSize = Directory.GetFiles(location, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));

                        string guidText = UninstallGuid.ToString("B");
                        key = parent.OpenSubKey(guidText, true) ??
                                parent.CreateSubKey(guidText);

                        if (key == null)
                        {
                            throw new Exception(String.Format("Unable to create uninstaller"));
                        }

                        key.SetValue("DisplayName", "The Skypark");
                        key.SetValue("ApplicationVersion", "");
                        key.SetValue("DisplayVersion", "");
                        key.SetValue("InstallLocation", location);
                        key.SetValue("Publisher", "Parallel 42 LLC");
                        key.SetValue("DisplayIcon", Path.Combine(location, @"Transponder\Transponder.exe"));
                        key.SetValue("URLInfoAbout", "https://parallel42.com/");
                        key.SetValue("Contact", "https://help.parallel42.com/");
                        key.SetValue("InstallDate", DateTime.UtcNow.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", Path.Combine(location, @"Transponder\Updater.exe") + " uninstall");
                        key.SetValue("QuietUninstallString", Path.Combine(location, @"Transponder\Updater.exe") + " uninstall silent");
                        key.SetValue("ModifyPath", Path.Combine(location, @"Transponder\Transponder.exe") + " modify");
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to register application: " + ex.Message);
                }
            }
        }

        private static void UnregisterProgram(string location)
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                if (parent == null)
                {
                    throw new Exception("Uninstall registry key not found.");
                }
                try
                {
                    string guidText = UninstallGuid.ToString("B");
                    parent.DeleteSubKeyTree(guidText);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to Unregister application: " + ex.Message);
                }
            }
        }

        internal static void CreateShortcuts(string location)
        {
            List<KeyValuePair<string, string>> Shortcuts = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Parallel 42", "The Skypark", "The Skypark Transponder.lnk"), Path.Combine(location, @"Transponder\Transponder.exe")),
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Parallel 42", "The Skypark", "The Skypark Skypad.lnk"), Path.Combine(location, @"Skypad\Skypad.exe")),
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"The Skypark Transponder.lnk"), Path.Combine(location, @"Transponder\Transponder.exe")),
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"The Skypark Skypad.lnk"), Path.Combine(location, @"Skypad\Skypad.exe")),
            };

            foreach (var p in Shortcuts)
            {
                if (!File.Exists(p.Key))
                {
                    try
                    {
                        if (File.Exists(p.Value))
                        {
                            FileInfo FI = new FileInfo(p.Key);
                            if (!Directory.Exists(FI.Directory.FullName))
                            {
                                Directory.CreateDirectory(FI.Directory.FullName);
                            }

                            IShellLink link = (IShellLink)new ShellLink();

                            // setup shortcut information
                            link.SetDescription("");
                            link.SetPath(p.Value);
                            //link.SetArguments("background silent");

                            // save it
                            IPersistFile file = (IPersistFile)link;
                            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                            file.Save(p.Key, false);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        internal static void RemoveShortcuts(string location)
        {
            List<KeyValuePair<string, string>>  Shortcuts = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Parallel 42", "The Skypark", "The Skypark Transponder.lnk"), Path.Combine(location, @"Transponder\Transponder.exe")),
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "Parallel 42", "The Skypark", "The Skypark Skypad.lnk"), Path.Combine(location, @"Skypad\Skypad.exe")),
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"The Skypark Transponder.lnk"), Path.Combine(location, @"Transponder\Transponder.exe")),
                new KeyValuePair<string, string>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), @"The Skypark Skypad.lnk"), Path.Combine(location, @"Skypad\Skypad.exe")),
            };
            foreach (var p in Shortcuts)
            {
                try
                {
                    if (File.Exists(p.Key))
                    {
                        File.Delete(p.Key);
                    }
                }
                catch
                {

                }
            }
        }

        internal static void Uninstall()
        {
            UnregisterProgram(Config["install"]);
            RemoveShortcuts(Config["install"]);

            MW.Dispatcher.Invoke(() =>
            {
                MW.uninstall_close_btn.IsEnabled = false;
            });

            try
            {
                Thread ut = new Thread(() =>
                {
                    Process[] ProcTransponder = Process.GetProcessesByName("Transponder");
                    Process[] ProcSkypad = Process.GetProcessesByName("Skypad");

                    foreach (Process p in ProcTransponder)
                    {
                        if (!p.HasExited)
                        {
                            MW.Dispatcher.Invoke(() =>
                            {
                                MW.uninstall_title_txtb.Text = "Before we go";
                                MW.uninstall_state_txtb.Text = "Please close the Transponder.";
                            });
                            p.WaitForExit();
                        }
                    }

                    foreach (Process p in ProcSkypad)
                    {
                        if (!p.HasExited)
                        {
                            MW.Dispatcher.Invoke(() =>
                            {
                                MW.uninstall_title_txtb.Text = "Before we go";
                                MW.uninstall_state_txtb.Text = "Please close the Skypad.";
                            });
                            p.WaitForExit();
                        }
                    }

                    MW.Dispatcher.Invoke(() =>
                    {
                        MW.uninstall_title_txtb.Text = "Uninstalling...";
                        MW.uninstall_state_txtb.Text = "Making sure everything is uninstalled.";
                    });

                    try
                    {
                        if (Directory.Exists(Config["install"]))
                        {
                            string InstallPath = Path.Combine(Config["install"]);

                            if (Directory.Exists(InstallPath))
                            {
                                Directory.Delete(InstallPath, true);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to delete " + AppDataDirectory + " - " + ex.Message);
                    }

                    #region Files
                    foreach (string f in new List<string>()
                    {
                        Path.Combine(AppDataDirectory, "config.json"),
                    })
                    {
                        try
                        {
                            if (File.Exists(f))
                            {
                                File.Delete(f);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to delete in " + AppDataDirectory + " - " + ex.Message);
                        }
                    }
                    #endregion

                    #region Directories
                    foreach (string Dir in new List<string>()
                    {
                        Path.Combine(AppDataDirectory, "Persistence"),
                        Path.Combine(AppDataDirectory, "Skypad"),
                        Path.Combine(AppDataDirectory, "Sounds"),
                        Path.Combine(AppDataDirectory, "Plans"),
                        Path.Combine(AppDataDirectory, "Routes"),
                    })
                    {
                        try
                        {
                            if (Directory.Exists(Dir))
                            {
                                Directory.Delete(Dir, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to delete in " + AppDataDirectory + " - " + ex.Message);
                        }
                    }
                    #endregion

                    #region Orbx
                    string OrbxVersionFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Orbx\Central\Integrations\Parallel 42\version_p42-the-skypark.txt";
                    try
                    {
                        if (File.Exists(OrbxVersionFile))
                        {
                            File.Delete(OrbxVersionFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to delete " + OrbxVersionFile + " - " + ex.Message);
                    }
                    #endregion

                    MW.Dispatcher.Invoke(() =>
                    {
                        MW.uninstall_title_txtb.Text = "Done";
                        MW.uninstall_state_txtb.Text = "We hope to see you again soon!";
                        MW.uninstall_close_btn.IsEnabled = true;
                    });

                });
                ut.Start();
            }
            catch
            {

            }

        }

        internal static void SaveConfig()
        {
            RegisterProgram(Config["install"]);
            string json = JSSerializer.Serialize(Config);
            CreateAppdataFolder();
            File.WriteAllText(ConfigPath, json);

        }

        internal static void CreateAppdataFolder()
        {
            if (!Directory.Exists(AppDataDirectory))
            {
                Directory.CreateDirectory(AppDataDirectory);
            }
        }

        static internal void ExcessCleanup()
        {
            Console.WriteLine(Environment.NewLine + "**** CLEANUP EXCESS ****");

            foreach(var Dir in ExcessCleanupIndex)
            {
                string[] DIrListing = Directory.GetFiles(Dir.Key);
                foreach(var F in DIrListing)
                {
                    FileInfo FI = new FileInfo(F);
                    if (Dir.Value.Contains(FI.Name))
                    {

                    }
                    else
                    {
                        try
                        {
                            File.Delete(FI.FullName);
                            Console.WriteLine("DELETED -> " + FI.FullName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("FAILED DELETE: " + FI.FullName + ": " + ex.Message);
                        }
                    }

                }
            }

        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        internal static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        internal static void Close()
        {

        }

        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "TSP_Installer_Crash_" + DateTime.UtcNow.Ticks + ".txt");
            e.Handled = true;

            GenerateLogFile(LogPath, e.Exception);

            MessageBox.Show("It looks like something unexpected occured.\rA log file has been saved in " + LogPath + ". \r\rPlease open a ticket at https://help.parallel42.com \r\rSorry for the inconvenience.", "Immersive Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Environment.Exit(1);
        }

        public static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "TSP_Installer_Crash_" + DateTime.UtcNow.Ticks + ".txt");
            Exception e = (Exception)args.ExceptionObject;

            GenerateLogFile(LogPath, e);

            MessageBox.Show("It looks like something unexpected occured.\rA log file has been saved in " + LogPath + ". \r\rPlease open a ticket at https://help.parallel42.com \r\rSorry for the inconvenience.", "Immersive Updater", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Environment.Exit(1);
        }


        public static void GenerateLogFile(string path, Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            if (e.InnerException != null)
            {
                Console.WriteLine(e.InnerException.Message);
                Console.WriteLine(e.InnerException.StackTrace);
            }

            ConsoleContent.WriteFile(0);
            ConsoleContent.Disp();

            // Init Auto-Reporting

            string os = "Unknown";
            string dotNetVersion = "Unknown";
            string runTime = Convert.ToString((DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds);
            try
            {
                // Retreive Windows Version
                try
                {
                    var osName = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>() select x.GetPropertyValue("Caption")).FirstOrDefault();
                    if (osName != null)
                    {
                        os = osName.ToString();
                    }
                }
                catch
                {
                    os = "Failed to get";
                }

                // Retreive the Dot Net Framework version
                using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
                {
                    int releaseKey = Convert.ToInt32(ndpKey.GetValue("Release"));
                    if (true)
                    {
                        dotNetVersion = CheckFor45DotVersion(releaseKey);
                    }
                }
            }
            catch
            {

            }

            DirectoryInfo DI = Directory.GetParent(path);
            if (!DI.Exists)
            {
                Directory.CreateDirectory(DI.FullName);
            }

            //List<FileInfo> FIs = new List<FileInfo>();
            //foreach(string F in Directory.GetFiles(DI.FullName))
            //{
            //    FIs.Add(new FileInfo(F));
            //}
            //FIs = FIs.OrderBy(x => x.LastWriteTimeUtc).ToList();
            //
            //while (FIs.Count >= 20)
            //{
            //    try
            //    {
            //        File.Delete(FIs[0].FullName);
            //    }
            //    catch
            //    {
            //    }
            //    FIs.RemoveAt(0);
            //}

            try
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine("--" + Convert.ToString(FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion) + "-------------- START OF LOG SECTION ----------------------" + Environment.NewLine);

                    writer.WriteLine("-----------------------------------------------------------------");
                    writer.WriteLine("-------- To avoid spambots, do not share this Log Online --------");
                    writer.WriteLine("-----------------------------------------------------------------");
                    writer.WriteLine("------ Please open a ticket at https://help.parallel42.com ------");
                    writer.WriteLine("-----------------------------------------------------------------" + Environment.NewLine);
                    writer.WriteLine("Time (UTC):       " + DateTime.UtcNow);
                    writer.WriteLine("Launch (s):       " + runTime);
                    writer.WriteLine("OS:               " + os);
                    writer.WriteLine("Version:          " + Convert.ToString(FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion));
                    writer.WriteLine(".NET Framework:   " + dotNetVersion);
                    writer.WriteLine("Memory:           " + String.Format("{0:##.##}", Process.GetCurrentProcess().PrivateMemorySize64 / 1073741824.0) + " GB" + Environment.NewLine);
                    writer.WriteLine(Environment.NewLine + "Exception " + e.HResult.ToString() + ": " + Environment.NewLine + e.Message + Environment.NewLine);
                    writer.WriteLine("StackTrace: " + Environment.NewLine + e.StackTrace + Environment.NewLine + Environment.NewLine);
                    writer.WriteLine("Console: " + File.ReadAllText(ConsoleContent.FilePath));
                    writer.WriteLine("---------------------- END OF LOG SECTION -----------------------");
                    writer.Flush();
                    writer.Close();
                }
            }
            catch
            {
            }

        }

        public static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 528040)
            {
                return "4.8 or later";
            }
            if (releaseKey >= 461808)
            {
                return "4.7.2";
            }
            if (releaseKey >= 461308)
            {
                return "4.7.1";
            }
            if (releaseKey >= 460798)
            {
                return "4.7";
            }
            if (releaseKey >= 394802)
            {
                return "4.6.2";
            }
            if (releaseKey >= 394254)
            {
                return "4.6.1";
            }
            if (releaseKey >= 393295)
            {
                return "4.6";
            }
            if ((releaseKey >= 379893))
            {
                return "4.5.2";
            }
            if ((releaseKey >= 378675))
            {
                return "4.5.1";
            }
            if ((releaseKey >= 378389))
            {
                return "4.5";
            }
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No version detected";
        }

        public class ControlWriter : TextWriter
        {
            StreamWriter Writer = null;

            public string FilePath = AppDomain.CurrentDomain.BaseDirectory + @"/TSP_Installer_Log.txt";
            List<string> Buffer = new List<string>();
            int BufferSize = 2;

            public ControlWriter(string FilePath)
            {
                try
                {
                    if(FilePath != string.Empty)
                    {
                        this.FilePath = FilePath;
                    }

                    File.Delete(this.FilePath);

                    Writer = new StreamWriter(this.FilePath, true);

                    Buffer.Add("****************** Do not post this file online. Send this file via a ticket at https://help.parallel42.com ******************\n");
                    Buffer.Add("Version:     " + Convert.ToString(FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).FileVersion) + "\n");
                    Buffer.Add("Time (UTC):  " + DateTime.UtcNow + "\n\n");
                }
                catch
                {

                }
            }

            public void Disp()
            {
                try
                {
                    Writer.Flush();
                    Writer.Close();
                }
                catch
                {

                }
            }

            public void WriteFile(int LeaveLine = 5, bool OnDesktop = false)
            {
                try
                {
                    if (Buffer.Count > BufferSize || LeaveLine != 0)
                    {
                        try
                        {
                            if (LeaveLine == 0)
                            {
                                Buffer.Add("Compiling Console...\r");
                            }

                            while (Buffer.Count > LeaveLine)
                            {
                                string line = Buffer[0];
                                //Debug.Write(line);

                                int searchOffset = 1;
                                while (Buffer.Count >= 2)
                                {
                                    if (line == Buffer[1])
                                    {
                                        Buffer.RemoveAt(0);
                                        searchOffset++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                if (searchOffset > 1)
                                {
                                    string newstr = line.Replace('\r', ' ') + "(" + searchOffset + "x)";
                                    Writer.WriteLine(newstr);
                                }
                                else
                                {
                                    Writer.Write(line);
                                }
                                Buffer.RemoveAt(0);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to write log: " + ex.Message);
                        }
                    }

                    if (OnDesktop)
                    {
                        if (File.Exists(FilePath))
                        {
                            string newReport = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "/TSP_Installer_Log.txt";
                            try
                            {
                                if (File.Exists(newReport))
                                {
                                    File.Delete(newReport);
                                }
                                File.Copy(FilePath, newReport);
                            }
                            catch
                            {
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to write log: " + ex.Message);
                }
            }

            string CNewLine = "";
            public override void Write(char value)
            {
                if (value == '\n')
                {
                    Buffer.Add(CNewLine);
                    CNewLine = "";
                }
                else
                {
                    CNewLine += value;
                }
            }

            public override void Write(string value)
            {
                Buffer.Add(value);
            }

            public override Encoding Encoding
            {
                get { return Encoding.ASCII; }
            }
        }
    }


    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink
    {
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }


}
