using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TSP_Installer.Models
{
    public class Updater
    {
        string UpdateDir = "";
        static List<int> ExitCodes = new List<int>();
        static List<int> DownloadCodes = new List<int>();

        int ExitCodeIndex = -1;
        int DownloadCodeIndex = -1;

        internal UpdateInfo Product = null;
        internal UpdateModule Module = null;
        internal string FailedReason = "Failed for an unknown reason.";

        bool CleanupExcess = false;
        bool CleanupExcessExcludeRoot = true;
        bool DoInstall = false;
        string InstallDir = "";
        string ManifestDir = "";
        string Channel = "";
        string DownloadURL = "";
        string ManifestsURL = "";

        long DownloadSize = 0;

        public Grid ProgressUIBase = null;
        public TextBlock StateText = null;
        public Border ProgressUIOutline = null;
        public Border ProgressUIFillDownload = null;
        public Border ProgressUIFillUnpacking = null;
        public Border ProgressUIFillInstalling = null;
        public Border ProgressUIFillChecking = null;

        internal bool CheckDownloadDir()
        {
            // Delete old directory
            string OldDir = Path.Combine(Path.GetTempPath(), "1dc97850-6e57-4333-af1d-e1f4b151e9d8");
            try
            {
                if (Directory.Exists(OldDir))
                {
                    Directory.Delete(OldDir, true);
                }
            }
            catch
            {
            }

            DriveInfo[] DI = DriveInfo.GetDrives();

            foreach (var drive in DI)
            {
                try
                {
                    string tempPath = Path.Combine(drive.Name, ".TSPUpdater_3_" + Module.ID);
                    if (Directory.Exists(tempPath))
                    {
                        UpdateDir = tempPath;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to check existing files in " + drive.Name + " : " + ex.Message);
                }
            }

            return false;
        }

        internal bool SetDownloadDir()
        {
            try
            {
                Func<DriveInfo, bool> Do = (drive) =>
                {
                    try
                    {
                        string tempPath = Path.Combine(drive.Name, ".TSPUpdater_3_" + Module.ID);
                        if (!Directory.Exists(tempPath))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(tempPath);
                            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                        }
                        File.WriteAllText(Path.Combine(tempPath, "test"), "");
                        UpdateDir = tempPath;
                        Console.WriteLine("Download directory for " + Module.Name + ": " + UpdateDir);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to write to " + drive.Name + " : " + ex.Message);
                    }
                    return false;
                };

                Func<DriveInfo, bool> CheckExist = (drive) =>
                {
                    try
                    {
                        string tempPath = Path.Combine(drive.Name, ".TSPUpdater_3_" + Module.ID);
                        if (Directory.Exists(tempPath))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to check existing files in " + drive.Name + " : " + ex.Message);
                    }
                    return false;
                };

                DriveInfo Existing = null;
                DriveInfo[] DI = DriveInfo.GetDrives();

                foreach (var drive in DI)
                {
                    if (CheckExist(drive))
                    {
                        Existing = drive;
                        break;
                    }
                }

                if(Existing == null)
                {
                    string SysPath = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
                    DriveInfo SysDrive = DI.Where(x => x.Name == SysPath).FirstOrDefault();

                    if (Do(SysDrive))
                    {
                        return true;
                    }

                    foreach (var drive in DI.Where(x => x.DriveType == DriveType.Fixed && x.IsReady && x.TotalFreeSpace > DownloadSize && x != SysDrive).OrderByDescending(x => x.TotalFreeSpace))
                    {
                        if (Do(drive))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (Do(Existing))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read drives: " + ex.Message);
                FailedReason = "Failed to read computer drive.";
            }

            return false;
        }

        internal bool Download(bool force)
        {
            try
            {
                bool GetUpdate = force;
                int ManifestExitCode = 0;

                ManifestExitCode = CompareManifest(Module.ID, InstallDir);

                if(ManifestExitCode == 28)
                {
                    Exit(28);
                    return false;
                }

                if (ManifestExitCode == 61)
                {
                    GetUpdate = true;
                }

                if(GetUpdate)
                {
                    if (!SetDownloadDir())
                    {
                        lock (DownloadCodes)
                        {
                            DownloadCodes[DownloadCodeIndex] = 28;
                        }
                        Exit(28);
                        return false;
                    }

                    App.MW.UpdateDownloadUI(this, 0, MainWindow.DownloadStages.Downloading);

                    DateTime Date = DateTime.UtcNow;
                    string FileHashName = String.Format("{0:X}", Module.ID.GetHashCode());
                    string DownloadPath = Path.Combine(UpdateDir, FileHashName);

                    try
                    {
                        System.Net.WebClient client = new System.Net.WebClient();
                        client.OpenRead(new Uri(DownloadURL + "?t=" + Date.DayOfYear + Date.Hour + Date.Minute));
                        long bytes_total = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);
                        DownloadSize += bytes_total;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to get file size for " + DownloadURL + ": " + ex.Message);
                    }
                    
                    string TempDir = Path.Combine(UpdateDir, "Extract_" + FileHashName);

                    if (File.Exists(DownloadPath)) { File.Delete(DownloadPath); }
                    if (Directory.Exists(TempDir)) { Directory.Delete(TempDir, true); }
                    if (!Directory.Exists(TempDir)) { Directory.CreateDirectory(TempDir); }


                    Console.WriteLine("Downloading " + Module.Name + " (" + Module.ID + ")...");

                    using (var wb = new ExtWebClient())
                    {
                        wb.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                        {
                            float Progress = (e.BytesReceived + 0f / e.TotalBytesToReceive) / e.TotalBytesToReceive;
                            App.MW.UpdateDownloadUI(this, Progress, MainWindow.DownloadStages.Downloading); 
                        };

                        wb.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                        {
                            App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Downloading);
                            if (e.Error == null)
                            {
                                try
                                {
                                    Thread.Sleep(100);
                                    int FileCount = 0;
                                    
                                    using (FileStream sr = new FileStream(DownloadPath, FileMode.Open))
                                    {
                                        using (ZipArchive Archive = new ZipArchive(sr))
                                        {
                                            int i = 0;
                                            long Size = 0;
                                            List<long> Sizes = new List<long>();
                                            long ProcessedSize = 0;
                                            List<ZipArchiveEntry> Entries = Archive.Entries.ToList().FindAll(x => x.Name != string.Empty);
                                            FileCount = Archive.Entries.Count;
                                            foreach (ZipArchiveEntry Entry in Archive.Entries)
                                            {
                                                long SizeF = Entry.Length;
                                                Sizes.Add(SizeF);
                                                Size += SizeF;
                                            }
                                            if (!Module.SkipValidate)
                                            {
                                                foreach (ZipArchiveEntry Entry in Archive.Entries)
                                                {
                                                    ProcessedSize += Sizes[i];
                                                    float Progress = ((float)ProcessedSize / Size);
                                                    App.MW.UpdateDownloadUI(this, Progress, MainWindow.DownloadStages.Unpacking);
                                                    if (Entry.Name != "")
                                                    {
                                                        string ExtractDirectoryPath = Path.GetDirectoryName(Path.Combine(TempDir, Entry.FullName));
                                                        string extractFullPath = Path.Combine(ExtractDirectoryPath, Entry.Name);
                                                        Directory.CreateDirectory(ExtractDirectoryPath);
                                                        Entry.ExtractToFile(extractFullPath, true);
                                                    }
                                                    i++;
                                                }
                                                Console.WriteLine("Downloaded & Verified " + Module.Name + " (" + Module.ID + ")");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Downloaded " + Module.Name + " (" + Module.ID + ")");
                                            }
                                        }
                                    }

                                    if(FileCount > 0)
                                    {
                                        if (!Module.SkipValidate)
                                        {
                                            App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Unpacking);
                                        }
                                        else
                                        {
                                            App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Downloading);
                                        }
                                    }
                                    else
                                    {
                                        App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);
                                    }

                                    if (!Module.SkipValidate)
                                    {
                                        File.Delete(DownloadPath);
                                    }

                                    lock (DownloadCodes)
                                    {
                                        DownloadCodes[DownloadCodeIndex] = 0;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("ERROR --- Downloaded " + Module.Name + " (" + Module.ID + ")" + " is not a valid file (" + DownloadURL + ") " + ex.Message);

#if !DEBUG
                                    if(File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Parallel 42", "DEV.txt")))
                                    {
                                        if (MessageBoxResult.OK == MessageBox.Show(Module.Name + "\r" + Module.ID + "\r\r" + DownloadURL + "\r\r" + ex.Message + "\r\rOK to copy URL to clipboard", "Immersive Updater Error", MessageBoxButton.OKCancel))
                                        {
                                            Clipboard.SetText(DownloadURL);
                                        };
                                    }
#endif

                                    lock (DownloadCodes)
                                    {
                                        DownloadCodes[DownloadCodeIndex] = 28;
                                    }

                                    try
                                    {
                                        if (File.Exists(DownloadPath))
                                        {
                                            File.Delete(DownloadPath);
                                        }
                                        if (Directory.Exists(TempDir))
                                        {
                                            Directory.Delete(TempDir, true);
                                        }
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine("Failed to delete update files " + TempDir + " - " + ex1.Message);
                                    }
                                    FailedReason = "The downloaded file is corrupt.";
                                    Exit(28);
                                }

                                Thread CheckDone = new Thread(() =>
                                {
                                    Thread.Sleep(100);
                                    int Count = 0;
                                    lock (DownloadCodes)
                                    {
                                        Count = DownloadCodes.FindAll(x => x == -1).Count;
                                    }

                                    while (DownloadCodes.FindAll(x => x == -1).Count > 0)
                                    {
                                        lock (DownloadCodes)
                                        {
                                            Count = DownloadCodes.FindAll(x => x == -1).Count;
                                        }
                                        Thread.Sleep(300);
                                    }

                                    if (DoInstall)
                                    {
                                        lock (DownloadCodes)
                                        {
                                            if (DownloadCodes.FindAll(x => x == 0).Count == DownloadCodes.Count)
                                            {
                                                Install();
                                            }
                                            else
                                            {
                                                if (Directory.Exists(TempDir))
                                                {
                                                    Directory.Delete(TempDir, true);
                                                }
                                                Exit(61);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Exit(61);
                                    }
                                });
                                CheckDone.IsBackground = true;
                                CheckDone.Start();
                            }
                            else
                            {
                                lock (DownloadCodes)
                                {
                                    DownloadCodes[DownloadCodeIndex] = 28;
                                }

                                Console.WriteLine("Failed to download update " + DownloadURL + " - " + e.Error.Message);
                                try
                                {
                                    if (File.Exists(DownloadPath))
                                    {
                                        File.Delete(DownloadPath);
                                    }
                                    if (Directory.Exists(TempDir))
                                    {
                                        Directory.Delete(TempDir, true);
                                    }
                                }
                                catch (Exception ex1)
                                {
                                    Console.WriteLine("Failed to delete update files " + TempDir + " - " + ex1.Message);
                                }

                                FailedReason = "Failed to download the new files.";
                                App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Failed);
                                Exit(28);
                            }

                        };

                        wb.DownloadFileAsync(new Uri(DownloadURL + "?t=" + Date.DayOfYear + Date.Hour + Date.Minute), DownloadPath);
                    }
                    
                }
                else
                {
                    App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);

                    lock (DownloadCodes)
                    {
                        DownloadCodes[DownloadCodeIndex] = ManifestExitCode;
                    }
                    Exit(ManifestExitCode);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to update " + DownloadURL + " - " + ex.Message + Environment.NewLine + ex.StackTrace);

                lock (DownloadCodes)
                {
                    DownloadCodes[DownloadCodeIndex] = 28;
                }
                Exit(28);
                FailedReason = "Failed to download the new files.";
                return false;
            }
        }

        internal void Install()
        {
            if (!CheckDownloadDir())
            {
                App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);
                Exit(0);
            }

            string FileHashName = string.Format("{0:X}", Module.ID.GetHashCode());
            string TempDl = Path.Combine(UpdateDir, FileHashName);
            string TempDir = Path.Combine(UpdateDir, "Extract_" + FileHashName);

            if (Directory.Exists(TempDir))
            {
                App.MW.UpdateDownloadUI(this, 0, MainWindow.DownloadStages.Installing);
                Thread ExtractThread = new Thread(() =>
                {
                    try
                    {
                        if(!Module.SkipValidate)
                        {
                            if (File.Exists(TempDl))
                            {
                                File.Delete(TempDl);
                            }
                        }

                        Console.WriteLine("");
                        Console.WriteLine("Installing " + Module.Name + " (" + Module.ID + ")" + " in " + InstallDir);
                        
                        if (!Module.SkipValidate)
                        {
                            if (Directory.Exists(TempDir))
                            {
                                var sourcePath = TempDir.TrimEnd('\\', ' ');
                                var targetPath = InstallDir.TrimEnd('\\', ' ');
                                var folders = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories).GroupBy(s => Path.GetDirectoryName(s)).ToList();
                                var filesCount = 0;
                                foreach (var folder in folders)
                                {
                                    foreach (var file in folder)
                                    {
                                        filesCount++;
                                    }
                                }
                                int i = 0;
                                long Size = 0;
                                long SizeProcessed = 0;
                                if (folders.Count > 0)
                                {
                                    foreach (var folder in folders)
                                    {
                                        foreach (var file in folder)
                                        {
                                            FileInfo FI = new FileInfo(file);
                                            long SizeF = FI.Length;
                                            Size += SizeF;
                                        }
                                    }

                                    foreach (var folder in folders)
                                    {
                                        var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                                        Directory.CreateDirectory(targetFolder);
                                        foreach (var file in folder)
                                        {
                                            FileInfo FI = new FileInfo(file);
                                            SizeProcessed += FI.Length;

                                            float Progress = ((float)SizeProcessed / Size);
                                            App.MW.UpdateDownloadUI(this, Progress, MainWindow.DownloadStages.Installing);

                                            var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                                            if (File.Exists(targetFile))
                                            {
                                                File.Delete(targetFile);
                                            }
                                            File.Move(file, targetFile);
                                            Console.WriteLine("Installed " + Module.Name + " (" + Module.ID + ")" + " in " + targetFile);
                                            i++;
                                        }
                                    }
                                }
                                else
                                {
                                    App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);
                                }

                                try
                                {
                                    if (Directory.Exists(TempDir))
                                    {
                                        Directory.Delete(TempDir, true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Failed to delete " + TempDir + " - " + ex.Message);
                                }

                                Exit(61);
                            }
                            else
                            {
                                Exit(0);
                            }

                            App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);
                        }
                        else
                        {
                            int FileCount = 0;
                            string DownloadPath = Path.Combine(UpdateDir, FileHashName);
                            string targetPath = InstallDir.TrimEnd('\\', ' ');
                            using (FileStream sr = new FileStream(DownloadPath, FileMode.Open))
                            {
                                using (ZipArchive Archive = new ZipArchive(sr))
                                {
                                    int i = 0;
                                    long Size = 0;
                                    List<long> Sizes = new List<long>();
                                    long ProcessedSize = 0;
                                    List<ZipArchiveEntry> Entries = Archive.Entries.ToList().FindAll(x => x.Name != string.Empty);
                                    FileCount = Archive.Entries.Count;
                                    foreach (ZipArchiveEntry Entry in Archive.Entries)
                                    {
                                        long SizeF = Entry.Length;
                                        Sizes.Add(SizeF);
                                        Size += SizeF;
                                    }
                                    foreach (ZipArchiveEntry Entry in Archive.Entries)
                                    {
                                        ProcessedSize += Sizes[i];
                                        float Progress = ((float)ProcessedSize / Size);
                                        App.MW.UpdateDownloadUI(this, Progress, MainWindow.DownloadStages.Unpacking);
                                        if (Entry.Name != "")
                                        {
                                            string ExtractDirectoryPath = Path.GetDirectoryName(Path.Combine(targetPath, Entry.FullName));
                                            string extractFullPath = Path.Combine(ExtractDirectoryPath, Entry.Name);
                                            Directory.CreateDirectory(ExtractDirectoryPath);
                                            Entry.ExtractToFile(extractFullPath, true);
                                        }
                                        i++;
                                    }
                                }
                            }
                            File.Delete(DownloadPath);
                            Console.WriteLine("Installed " + Module.Name + " (" + Module.ID + ")" + " in " + targetPath);
                            App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);
                            Exit(61);
                        }
                        
                        DeleteUpdateDir();
                    }
                    catch (Exception ex)
                    {
                        App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Failed);
                        Console.WriteLine("Failed to install " + Module.Name + " in " + InstallDir + " because " + ex.Message);
                        Exit(28);
                        FailedReason = "Failed to install the new files.";
                    }

                });
                ExtractThread.Name = "Extract Thread " + Module.Name;
                ExtractThread.IsBackground = true;
                ExtractThread.Start();
            }
            else
            {
                App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);
                Exit(0);
            }

        }

        internal void IsReady()
        {
            Console.WriteLine(Environment.NewLine + "**** ISREADY ****");
            try
            {
                if (!CheckDownloadDir())
                {
                    Console.WriteLine("Couldn't find download files for " + Module.Name);
                    Exit(0);
                    return;
                }

                Console.WriteLine("Checking if an update is ready for " + Module.Name + " (" + Module.ID + ")" + "...");
                string FileHashName = String.Format("{0:X}", Module.ID.GetHashCode());
                string TempDir = Path.Combine(UpdateDir, "Extract_" + FileHashName);
                if (Directory.Exists(TempDir))
                {
                    string[] f = Directory.GetFiles(TempDir);
                    if(f.Length > 0)
                    {
                        Console.WriteLine("...Yep!");
                        Exit(61);
                    }
                    else
                    {
                        Directory.Delete(TempDir, true);
                        Console.WriteLine("...Was empty!");
                        Exit(0);
                    }
                }
                else
                {
                    Console.WriteLine("...Nope!");
                    Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to check if update is ready for " + Module.Name + " (" + Module.ID + ")" + " because " + ex.Message);
                FailedReason = "Failed to check for downloaded updates.";
                Exit(28);
            }
        }

        internal void Exit(int Code)
        {
            if(Code == 28)
            {
                App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Failed);
            }

            int ExitCode = 0; // No printers were found (No update)
            lock (ExitCodes)
            {
                ExitCodes[ExitCodeIndex] = Code;
                if (!ExitCodes.Contains(-1))
                {
                    if (ExitCodes.Contains(61))
                    {
                        ExitCode = 61; // The printer queue is full (Something has updated)
                    }

                    if (ExitCodes.Contains(28))
                    {
                        ExitCode = 28; // The printer is out of paper (One or more updates have failed)

                        App.GenerateLogFile(Path.Combine(Product.ConsoleDir, "TSP_Installer_Failed_" + DateTime.UtcNow.Ticks + ".txt"), new Exception("Update Failed"));
                    }

                    // Cleanup
                    if (ExitCode != 28 || App.ArgsList.Contains("silent"))
                    {
                        App.MW.ExitReady = true;
                        App.MW.Exit(ExitCode);
                    }
                    else
                    {
                        App.MW.Dispatcher.Invoke(() =>
                        {
                            App.MW.last_card_close_btn.Visibility = Visibility.Visible;
                            App.MW.last_card_support_btn.Visibility = Visibility.Visible;
                        });
                    }
                }
            }
        }

        internal void Cleanup()
        {
            DeleteManifests();
        }

        internal void SetProduct(UpdateInfo Product)
        {
            this.Product = Product;

        }

        internal void SetInstall(bool State)
        {
            DoInstall = State;
            Console.WriteLine(Module.Name + " Install is now set to " + DoInstall);
            lock (DownloadCodes)
            {
                if (DownloadCodes.FindAll(x => x == 61 || x == 0).Count == DownloadCodes.Count)
                {
                    Install();
                }
            }
        }

        internal void SetModule(UpdateModule Module)
        {
            this.Module = Module;
        }
        
        internal void SetExcessCleanup(bool Cleanup, bool Root)
        {
            CleanupExcess = Cleanup;
            CleanupExcessExcludeRoot = Root;
        }

        internal void SetChannel(string Name, string VersionDownload)
        {
            Channel = Name;
            DownloadURL = Module.DownloadURL.Replace("%channel%", Name).Replace("%version%", VersionDownload);
            ManifestsURL = Product.ManifestsURL.Replace("%channel%", Name).Replace("%version%", VersionDownload);
        }
        
        internal void SetManifestDir(string Dir)
        {
            ManifestDir = Path.Combine(new FileInfo(App.ThisApp.Location).DirectoryName, Dir);
        }
        
        internal void SetInstallDir(string Dir)
        {
            if(Dir != string.Empty)
            {
                InstallDir = new DirectoryInfo(Dir).FullName;
                Console.WriteLine("Set install dir for " + Module.Name + " to " + InstallDir);
            }
        }

        private void DeleteUpdateDir()
        {
            try
            {
                if (UpdateDir != string.Empty)
                {
                    if(Directory.Exists(UpdateDir))
                    {
                        Directory.Delete(UpdateDir, true);
                    }
                }
            }
            catch
            {
            }
        }

        private int GetManifest()
        {
            try
            {
                if (!Product.ManifestData.ContainsKey(Channel))
                {
                    Console.WriteLine(Environment.NewLine + "**** MANIFESTS ****");
                    Console.WriteLine("Manifest URL " + ManifestsURL);
                    Console.WriteLine("Getting manifests ...");

                    byte[] DownloadData;
                    DateTime Date = DateTime.UtcNow;
                    using (var wb = new ExtWebClient() { CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore) })
                    {
                        DownloadData = wb.DownloadData(ManifestsURL);
                    }

                    Console.WriteLine("... Manifests received");
                    Product.ManifestData.Add(Channel, DownloadData);
                }
                else
                {
                    Console.WriteLine("Already have manifests " + ManifestsURL);
                }

                if (!Directory.Exists(ManifestDir))
                {
                    using (ZipArchive Archive = new ZipArchive(new MemoryStream(Product.ManifestData[Channel])))
                    {
                        foreach (ZipArchiveEntry Entry in Archive.Entries)
                        {
                            string ExtractDirectoryPath = Path.GetDirectoryName(Path.Combine(ManifestDir, Entry.FullName));

                            if (Entry.Name != string.Empty)
                            {
                                string extractFullPath = Path.Combine(ExtractDirectoryPath, Entry.Name);
                                Directory.CreateDirectory(ExtractDirectoryPath);
                                Entry.ExtractToFile(extractFullPath, true);
                            }
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get manifests " + ManifestsURL + " / " + ex.Message + Environment.NewLine + ex.StackTrace);
                FailedReason = "Failed to check for updates.";
                App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Failed);
                return 28;
            }

        }

        internal void DeleteManifests()
        {
            try
            {
                if(ManifestDir != string.Empty)
                {
                    if (Directory.Exists(ManifestDir))
                    {
                        Console.WriteLine("Cleaning manifests ...");
                        Directory.Delete(ManifestDir, true);
                        Console.WriteLine("... Manifests cleaned");
                    }
                }
            }
            catch
            {
                Console.WriteLine("... Failed to clean manifests in " + ManifestDir);
            }
        }

        private int CompareManifest(string ManifestName, string DirPath)
        {
            if (!Product.ManifestData.ContainsKey(Channel))
            {
                int GMC = GetManifest();
                if (GMC != 0)
                {
                    return GMC;
                };
                Console.WriteLine(Environment.NewLine + "**** CHECK ****");
            }
            
            try
            {
                int ReturnCode = 0;
                string ManifestPath = Path.Combine(ManifestDir, ManifestName + ".txt");
                if (File.Exists(ManifestPath))
                {
                    string[] Lines = File.ReadAllLines(ManifestPath);
                    

                    int i = 0;
                    foreach (string Line in Lines)
                    {
                        string[] FileDetails = Line.Split('\t');
                        string AbsPath = Path.Combine(DirPath, FileDetails[0]);

                        DownloadSize += Convert.ToInt64(FileDetails[1]);

                        try
                        {
                            DirectoryInfo DI = Directory.GetParent(AbsPath);
                            if (CleanupExcess)
                            {
                                if (CleanupExcessExcludeRoot && DI.FullName == InstallDir)
                                {
                                }
                                else
                                {
                                    lock (App.ExcessCleanupIndex)
                                    {
                                        if (!App.ExcessCleanupIndex.ContainsKey(DI.FullName))
                                        {
                                            App.ExcessCleanupIndex.Add(DI.FullName, new List<string>());
                                        }
                                    }
                                }
                            }
                            
                            lock (App.ExcessCleanupIndex)
                            {
                                if (App.ExcessCleanupIndex.ContainsKey(DI.FullName))
                                {
                                    App.ExcessCleanupIndex[DI.FullName].Add(new FileInfo(AbsPath).FullName.Replace(DI.FullName, "").TrimStart('\\'));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to Track file for Cleanup Index " + AbsPath + " / " + ex.Message + Environment.NewLine + ex.StackTrace);
                        }

                        if (ReturnCode != 0) // No need to compare additional files, we know we have to update
                        {
                            continue;
                        }

                        float Progress = ((float)i / Lines.Length);
                        App.MW.UpdateDownloadUI(this, Progress, MainWindow.DownloadStages.Checking);
                        i++;

                        if (File.Exists(AbsPath))
                        {
                            try
                            {
                                long Size = new FileInfo(AbsPath).Length;
                                string MD5Hash = "";
                                try
                                {
                                    if (Size < 3e+8 || App.Rnd.Next(6) == 1)
                                    {
                                        MD5Hash = CalculateMD5(AbsPath);
                                    }
                                }
                                catch
                                {
                                }
                                
                                if (MD5Hash != string.Empty ? MD5Hash != FileDetails[2] : false)
                                {
                                    Console.WriteLine("MD5 DIF -> " + FileDetails[0] + " - " + MD5Hash + " LOCAL vs " + FileDetails[2] + " REMOTE");
                                    ReturnCode = 61;
                                    App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Checking);
                                    continue;
                                }

                                if (Convert.ToString(Size) != FileDetails[1])
                                {
                                    Console.WriteLine("SIZE DIF -> " + FileDetails[0]);
                                    ReturnCode = 61;
                                    App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Checking);
                                    continue;
                                }
                                
                                Console.WriteLine("OK -> " + FileDetails[0]);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("IN USE -> " + FileDetails[0]);
                                if (App.ArgsList.Contains("install"))
                                {
                                    Console.WriteLine(ManifestName + " update is doomed: " + ex.Message);
                                    ReturnCode = 28;
                                    App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Checking);
                                }
                                continue;
                            }
                        }
                        else
                        {
                            Console.WriteLine("MISSING -> " + FileDetails[0]);
                            ReturnCode = 61;
                            App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Checking);
                            continue;
                        }

                    }
                }
                else
                {
                    Console.WriteLine("NO MANIFEST -> " + ManifestName);
                    App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Checking);
                }

                if(ReturnCode == 0)
                {
                    App.MW.UpdateDownloadUI(this, 1, MainWindow.DownloadStages.Installing);
                }

                return ReturnCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to update " + ManifestName + " / " + ex.Message + Environment.NewLine + ex.StackTrace);
                App.GenerateLogFile(Path.Combine(Product.ConsoleDir,"TSP_Installer_Failed_" + DateTime.UtcNow.Ticks + ".txt"), ex);
                return 28;
            }
        }

        private static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public void MakeProgressEl()
        {
            SolidColorBrush White = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFF");


            ProgressUIBase = new Grid()
            {
                Margin = new Thickness(5, 0, 5, 5),
            };
            
            StackPanel ProgressStack = new StackPanel();
            ProgressUIBase.Children.Add(ProgressStack);

            Grid HeaderGrid = new Grid()
            {
                Margin = new Thickness(0, 0, 0, 2)
            };
            ProgressStack.Children.Add(HeaderGrid);

            TextBlock Description = new TextBlock()
            {
                Text = Module.Name,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };
            HeaderGrid.Children.Add(Description);


            StateText = new TextBlock()
            {
                Text = "Starting...",
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#7777FF"),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                FontWeight = FontWeights.Bold,
                FontSize = 12
            };
            HeaderGrid.Children.Add(StateText);



            ProgressUIOutline = new Border()
            {
                //BorderThickness = new Thickness(1),
                //BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#22000000"),
                //CornerRadius = new CornerRadius(6),
                VerticalAlignment = VerticalAlignment.Center,
                SnapsToDevicePixels = true,
            };
            ProgressStack.Children.Add(ProgressUIOutline);

            Grid ProgressUIFrameGrid = new Grid();
            ProgressUIFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star), });
            ProgressUIFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Star), });
            ProgressUIFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star), });
            ProgressUIFrameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star), });
            ProgressUIOutline.Child = ProgressUIFrameGrid;

            ProgressUIFillChecking = new Border()
            {
                Height = 10,
                Width = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#55cc55"),
                //CornerRadius = new CornerRadius(5),
                //BorderBrush = White,
                //BorderThickness = new Thickness(1),
            };
            Grid.SetColumn(ProgressUIFillChecking, 0);
            ProgressUIFrameGrid.Children.Add(ProgressUIFillChecking);

            ProgressUIFillDownload = new Border()
            {
                Height = 10,
                Width = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#55cc55"),
                //CornerRadius = new CornerRadius(5),
                //BorderBrush = White,
                //BorderThickness = new Thickness(1),
            };
            Grid.SetColumn(ProgressUIFillDownload, 1);
            ProgressUIFrameGrid.Children.Add(ProgressUIFillDownload);
            
            ProgressUIFillUnpacking = new Border()
            {
                Height = 10,
                Width = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#55cc55"),
                //CornerRadius = new CornerRadius(5),
                //BorderBrush = White,
                //BorderThickness = new Thickness(1),
            };
            Grid.SetColumn(ProgressUIFillUnpacking, 2);
            ProgressUIFrameGrid.Children.Add(ProgressUIFillUnpacking);

            ProgressUIFillInstalling = new Border()
            {
                Height = 10,
                Width = 0,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#55cc55"),
                //CornerRadius = new CornerRadius(5),
                //BorderBrush = White,
                //BorderThickness = new Thickness(1),
            };
            Grid.SetColumn(ProgressUIFillInstalling, 3);
            ProgressUIFrameGrid.Children.Add(ProgressUIFillInstalling);



            Border ProgressUIFrameOutline = new Border()
            {
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#44000000"),
                BorderThickness = new Thickness(1),
            };
            Grid.SetColumnSpan(ProgressUIFrameOutline, 4);
            ProgressUIFrameGrid.Children.Add(ProgressUIFrameOutline);

            App.MW.UpdateDownloadUI(this, 0, MainWindow.DownloadStages.Checking);
            
        }
        
        internal Updater()
        {
            ExitCodeIndex = ExitCodes.Count;
            ExitCodes.Add(-1);
            DownloadCodeIndex = DownloadCodes.Count;
            DownloadCodes.Add(-1);
        }
    }
}
