using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Collections.Generic;

namespace TSP_Transponder.Models.Updater
{
    class UpdateInstance
    {
        private static string UpdaterCode = "TSP";
        private static string UpdaterTemp = "196f6f05-e58e-49b4-b896-c233ae36ca5b";
        private static string UpdaterVersionURL = "https://cdn.invernyx.com/skypark/1CE04F/leg-content/v1/%channel%/v.txt";

        private static Version ProdChannelV = null;

        internal static bool HasUpdate = false;
        internal static bool ApplyOnQuit = false;
        internal static List<string> ProductIDs = new List<string>();
        internal static void SetProduct(string ID)
        {
            if (App.OwnsProduct(ID))
            {
                lock (ProductIDs)
                {
                    ProductIDs.Add(ID);
                }
            }

        }

        internal static bool AllowUpdate()
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NOUPDATE.txt")))
            {
#if DEBUG
                //#warning Update system is enabled
                //return false;
#else
                return true;
#endif
            }

            return false;
        }

        internal static string SetChannel(string channel)
        {
            HasUpdate = false;
            channel = CheckChannelMerge(channel);
            Check();
            return channel;
        }

        internal static string CheckChannelMerge(string channel)
        {
            if (channel == "rc")
            {
                try
                {
                    Version RCchannelLimitV = new Version(UserData.Get("rcv"));

                    if (ProdChannelV == null)
                    {
                        DateTime DT = DateTime.UtcNow;
                        string VURL = UpdaterVersionURL + "?t=" + DT.DayOfYear.ToString() + DT.Hour.ToString() + Math.Floor(DT.Minute / 20f).ToString();

                        using (var wb = new Utilities.ExtWebClient())
                        {
                            ProdChannelV = new Version(wb.DownloadString(VURL.Replace("%channel%", "prod")).Split('\n')[0]);
                        }
                    }

                    if (RCchannelLimitV.Major <= ProdChannelV.Major && RCchannelLimitV.Minor < ProdChannelV.Minor)
                    {
                        return "prod";
                    }
                }
                catch
                {
                }
            }

            return channel;
        }

        internal static void CheckReady(Action<int> Completed = null)
        {
            List<string> ArgsList = new List<string>();
            ArgsList.Add("update");
            ArgsList.Add("isready");
            ArgsList.Add("silent");

            ArgsList.Add("\"adventures|null|" + Path.Combine(App.AppDataDirectory).TrimEnd('\\') + "\"");
            ArgsList.Add("\"transponder|null|" + Path.Combine(UserData.Get("install").TrimEnd('\\'), "Transponder") + "\"");
            ArgsList.Add("\"skypad|null|" + Path.Combine(UserData.Get("install").TrimEnd('\\'), "Skypad") + "\"");
            ArgsList.Add("\"skyos|null|" + Path.Combine(UserData.Get("install").TrimEnd('\\'), "Skypad") + "\"");
            ArgsList.Add("\"topo|null|" + Path.Combine(UserData.Get("install").TrimEnd('\\'), "Transponder") + "\"");

            if (AllowUpdate())
            {
                if (App.IsSetup)
                {
                    BootUpdater(string.Join(" ", ArgsList), true, true, true, true, Completed);
                }
                else
                {
                    BootUpdater(string.Join(" ", ArgsList), false, false, true, false, Completed);
                }
            }
            else
            {
                Console.WriteLine("Hypothetical Update argument: " + string.Join(" ", ArgsList));
                Check(Completed);
            }
        }

        internal static void Check(Action<int> Completed = null)
        {
            HasUpdate = false;
            string Channel = UserData.Get("channel");
            string NewChannel = CheckChannelMerge(Channel);

            //if (Channel != NewChannel)
            //{
            //    Channel = SetChannel(NewChannel);
            //}

            if (File.Exists(Path.Combine(App.AppDataDirectory, "BETA.txt")))
            {
                Channel = "beta";
            }

            if (File.Exists(Path.Combine(App.AppDataDirectory, "TEST.txt")))
            {
                FileInfo fi = new FileInfo(Path.Combine(App.AppDataDirectory, "TEST.txt"));
                if(fi.LastWriteTimeUtc.AddDays(15) > DateTime.UtcNow)
                {
                    Channel = "test";
                }
                else
                {
                    try
                    {
                        File.Delete(Path.Combine(App.AppDataDirectory, "TEST.txt"));
                    }
                    catch
                    {
                    }
                }
            }

            if (App.IsDev)
            {
                Channel = "dev";
            }

            List<string> ArgsList = new List<string>();
            ArgsList.Add("update");
            ArgsList.Add("download");
            ArgsList.Add("silent");

            ArgsList.Add("\"adventures|" + Channel + "|" + Path.Combine(App.AppDataDirectory).TrimEnd('\\') + "\"");
            ArgsList.Add("\"transponder|" + Channel + "|" + Path.Combine(UserData.Get("install"), "Transponder").TrimEnd('\\') + "\"");
            ArgsList.Add("\"skypad|" + Channel + "|" + Path.Combine(UserData.Get("install"), "Skypad").TrimEnd('\\') + "\"");
            ArgsList.Add("\"skyos|" + Channel + "|" + Path.Combine(UserData.Get("install"), "Skypad").TrimEnd('\\') + "\"");
            ArgsList.Add("\"topo|" + Channel + "|" + Path.Combine(UserData.Get("install"), "Transponder").TrimEnd('\\') + "\"");

            if (AllowUpdate())
            {
                if (App.IsSetup)
                {
                    BootUpdater(string.Join(" ", ArgsList), false, true, false, true, Completed);
                }
                else
                {
                    BootUpdater(string.Join(" ", ArgsList), false, false, false, false, Completed);
                }
            }
            else
            {
                Console.WriteLine("Hypothetical Update argument: " + string.Join(" ", ArgsList));
                Completed?.Invoke(0);
            }

        }

        internal static void Apply(bool Relaunch, bool Visible)
        {
            List<string> ArgsList = new List<string>();
            ArgsList.Add("update");
            ArgsList.Add("install");

            ArgsList.Add("\"adventures|null|" + Path.Combine(App.AppDataDirectory).TrimEnd('\\') + "\"");
            ArgsList.Add("\"transponder|null|" + Path.Combine(UserData.Get("install"), "Transponder").TrimEnd('\\') + "\"");
            ArgsList.Add("\"skypad|null|" + Path.Combine(UserData.Get("install"), "Skypad").TrimEnd('\\') + "\"");
            ArgsList.Add("\"skyos|null|" + Path.Combine(UserData.Get("install"), "Skypad").TrimEnd('\\') + "\"");
            ArgsList.Add("\"topo|null|" + Path.Combine(UserData.Get("install"), "Transponder").TrimEnd('\\') + "\"");

            ArgsList.Add("\"waitfor|transponder:Transponder?skypad:Skypad\"");
            if (Relaunch)
            {
                ArgsList.Add("\"restart|" + Path.Combine(UserData.Get("install"), "Transponder") + "\\Transponder.exe\"");
            }
            if (Visible)
            {
                ArgsList.Add("visible");
            }

            if (AllowUpdate())
            {
                Console.WriteLine("Booting Updater...");
                BootUpdater(string.Join(" ", ArgsList), true, false, false, false);
            }
            else
            {
                Console.WriteLine("Hypothetical Update argument: " + string.Join(" ", ArgsList));
            }
        }

        private static void BootUpdater(string args, bool quitafter, bool applyafter, bool checkafter, bool blocking, Action<int> Completed = null)
        {
            // Extract the auto updater
            string UpdaterDir = Path.Combine(Path.GetTempPath(), UpdaterTemp);
            string UpdaterName = "Immersive_Updater_" + UpdaterCode;
            string UpdaterPath = Path.Combine(UpdaterDir, UpdaterName + "_0.exe");

            int Extract_Try = 1;
            Action Extract = null;
            Extract = () =>
            {
                UpdaterName = "Immersive_Updater_" + UpdaterCode + "_" + Extract_Try;
                UpdaterPath = Path.Combine(UpdaterDir, UpdaterName + ".exe");
                try
                {
                    try
                    {
                        foreach (var process in Process.GetProcessesByName(UpdaterName))
                        {
                            process.Kill();
                        }
                        Thread.Sleep(300);
                    }
                    catch
                    {
                    }

                    if (!Directory.Exists(UpdaterDir))
                    {
                        Directory.CreateDirectory(UpdaterDir);
                    }

                    if (File.Exists(UpdaterPath))
                    {
                        File.Delete(UpdaterPath);
                    }
                    
                    using (var resource = App.ThisApp.GetManifestResourceStream("TSP_Transponder.Models.Updater.The Skypark Installer.exe"))
                    {
                        using (var file = new FileStream(UpdaterPath, FileMode.Create, FileAccess.Write))
                        {
                            resource.CopyTo(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Extract_Try < 5)
                    {
                        Extract_Try++;
                        Console.WriteLine("Failed to replace existing Updater. (Try " + Extract_Try + ") " + ex.Message);
                        Extract();
                    }
                    else
                    {
                        MessageBox.Show(UpdaterName + " failed to update itself. \n\nTry updating it manually in OrbxCentral under the My Products section.", UpdaterName + "failed to update.");
                    }
                }
            };

            Action Go = () =>
            {
                try
                {
                    Extract();

                    Console.WriteLine("Immersive Updater Booting: " + args + " / " + quitafter + ", " + applyafter + ", " + checkafter + ", " + blocking);

                    ProcessStartInfo IMMUPI = new ProcessStartInfo();
                    IMMUPI.FileName = UpdaterPath;
                    IMMUPI.UseShellExecute = false;
                    IMMUPI.Arguments = args;
                    Process IMMU = Process.Start(IMMUPI); // Launch it 

                    Console.WriteLine("Immersive Updater Started");


                    if (quitafter)
                    {
                        Console.WriteLine("Updater started, Shutting down");

                        if (App.MW != null)
                        {
                            ApplyOnQuit = false;
                            App.MW.Shutdown();
                        }
                        else
                        {
                            Console.WriteLine("Goodbye!");
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Waiting for Updater to shutdown...");
                        IMMU.WaitForExit(5000);

                        Stopwatch UpdaterTimer = new Stopwatch();
                        UpdaterTimer.Start();
                        while (!IMMU.HasExited)
                        {
                            Thread.Sleep(2000);
                            Console.WriteLine("Waiting for Updater to shutdown...");
                            if (UpdaterTimer.ElapsedMilliseconds > 300000)
                            {
                                Console.WriteLine("Done waiting for Updater to shutdown. Sopping monitor.");
                                break;
                            }
                        }

                        //try
                        //{
                        //    IMMU.Kill();
                        //}
                        //catch
                        //{
                        //
                        //}

                        if(IMMU.HasExited)
                        {
                            Console.WriteLine("Immersive Updater returned code " + IMMU.ExitCode);

                            bool ShowUpdate = false;
                            switch (IMMU.ExitCode)
                            {
                                case 0: // No Update
                                    {
                                        if (checkafter)
                                        {
                                            Check(Completed);
                                        }
                                        else
                                        {
                                            Completed?.Invoke(IMMU.ExitCode);
                                        }
                                        break;
                                    }
                                case 61: // Has Update
                                    {
                                        if (applyafter)
                                        {
                                            if (App.IsSetup)
                                            {
                                                Apply(false, true);
                                            }
                                            else
                                            {
                                                Apply(true, true);
                                            }
                                        }
                                        else
                                        {
                                            ShowUpdate = true;
                                            ApplyOnQuit = true;
                                        }

                                        Completed?.Invoke(IMMU.ExitCode);
                                        break;
                                    }
                                case 28: // Update Failed
                                    {
                                        break;
                                    }
                            }

                            HasUpdate = ShowUpdate;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Immersive Updater FAILED: " + e.Message);

                    if (quitafter)
                    {
                        if (App.MW != null)
                        {
                            App.MW.Shutdown();
                        }
                        else
                        {
                            Console.WriteLine("Goodbye!");
                            Environment.Exit(0);
                        }
                    }

                    //MessageBox.Show("Immersion Manager wasn't able to update itself.\rThis is often caused by a security software or multiple instances of Immersion Manager running.\rTry rebooting your computer and try again.\r" + e.Message, "Unable to launch the Auto-Updater", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //DateTime AppNow = DateTime.UtcNow;
                //DateTime AppExpire = Convert.ToDateTime(Properties.Resources.ResourceManager.GetString("BuildDate").Trim());
                //if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parallel 42", "DEV.txt")))
                //{
                //    if (AppNow > AppExpire.AddDays(80)) // Expiration
                //    {
                //        MessageBox.Show("Expire in " + (AppNow - AppExpire.AddDays(120)).TotalDays );
                //        return;
                //    }
                //}
                //if (AppNow > AppExpire.AddDays(120)) // Expiration
                //{
                //    Check((code) =>
                //    {
                //        if(code == 61)
                //        {
                //            Apply(true, true);
                //        }
                //        else
                //        {
                //            Environment.Exit(0);
                //        }
                //    });
                //    return;
                //}

            };

            if (!blocking)
            {
                Thread UpdateThread = new Thread(() => { Go(); });
                UpdateThread.Start();
                UpdateThread.IsBackground = true;
            }
            else
            {
                Go();
            }
        }

    }
}
