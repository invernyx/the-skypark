using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace Packager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static MainWindow MW = null;
        internal static bool IsAdmin = false;


        internal static string AssemblyPath = @"";
        internal static string BasePath = @"";
        internal static string ManifestsDir = @"";
        internal static string PublishPath = @"";
        internal static string PublishScriptDEV = @"";
        internal static string PublishScriptTEST = @"";
        internal static string PublishScriptBETA = @"";
        internal static string PublishScriptRC = @"";
        internal static string PublishScriptPROD = @"";
        internal static string ToolsPath = @"F:\Documents\Clients\Parallel 42\The Skypark\Dist\Tools";
        internal static string ManifestMakerPath = @"F:\Documents\Clients\Parallel 42\Git\p42-manifest-maker\bin\Debug\ManifestMaker.exe";
        internal static string MSBuildPath = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe";
        internal static string BuildNumberMaker = @"F:\Documents\Clients\Parallel 42\Git\p42-buildnumber\BuildNumber\bin\Debug\BuildNumber.exe";
        internal static string SHA1 = "C88AD16B642DD3250BF72A248DB7139EA35B798C";

        void App_Startup(object sender, StartupEventArgs e)
        {
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
                ProcessStartInfo startInfo = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);
                startInfo.Verb = "runas";
                Process.Start(startInfo);
                Current.Shutdown();
                return;
            }
            #endregion

            #region Find Assembly Path
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            AssemblyPath = Path.GetDirectoryName(path);
            #endregion

            MW = new MainWindow();
            MW.Show();

            ReadSettings();

            #region Create Folders
            List<string> Folders = new List<string>()
            {
                Path.Combine(BasePath, "Dist", "Output"),
                Path.Combine(BasePath, "Dist", "Zip"),
            };

            foreach (string p in Folders)
            {
                try
                {
                    Directory.CreateDirectory(p);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            #endregion
        }

        internal static void ReadSettings()
        {
            MW.Log("settings", "");
            MW.Log("settings", "");
            MW.Log("settings", "Reading Settings", Color.FromArgb(0xff, 0x11, 0x11, 0x22), true);

            #region Read Settings
            try
            {
                string IniPath = Path.Combine(AssemblyPath, "settings.ini");
                if (!File.Exists(IniPath))
                {
                    using (StreamWriter f = new StreamWriter(IniPath, false))
                    {
                        f.WriteLine("buildnumbermakerpath=" + BuildNumberMaker);
                        f.WriteLine("manifestmakerpath=" + ManifestMakerPath);
                        f.WriteLine("msbuildpath=" + MSBuildPath);
                        f.WriteLine("basepath=" + AssemblyPath);
                        f.WriteLine("publishpath=");
                        f.WriteLine("publishscript_dev=");
                        f.WriteLine("publishscript_test=");
                        f.WriteLine("publishscript_beta=");
                        f.WriteLine("publishscript_rc=");
                        f.WriteLine("publishscript_prod=");
                    }
                }

                StreamReader file = new StreamReader(IniPath);
                string line = "";
                int counter = 0;
                while ((line = file.ReadLine()) != null)
                {
                    if (line != string.Empty && !line.StartsWith("//"))
                    {
                        int index = line.IndexOf('=');
                        string first = line.Substring(0, index);
                        string second = line.Substring(index + 1);

                        switch (first.ToLower())
                        {
                            case "manifestmakerpath":
                                {
                                    ManifestMakerPath = second;
                                    break;
                                }
                            case "msbuildpath":
                                {
                                    MSBuildPath = second;
                                    break;
                                }
                            case "basepath":
                                {
                                    BasePath = second;
                                    ManifestsDir = Path.Combine(BasePath, "Dist", "Output", "manifests");
                                    break;
                                }
                            case "publishpath":
                                {
                                    PublishPath = second;
                                    break;
                                }
                            case "publishscript_dev":
                                {
                                    PublishScriptDEV = second;
                                    break;
                                }
                            case "publishscript_test":
                                {
                                    PublishScriptTEST = second;
                                    break;
                                }
                            case "publishscript_beta":
                                {
                                    PublishScriptBETA = second;
                                    break;
                                }
                            case "publishscript_rc":
                                {
                                    PublishScriptRC = second;
                                    break;
                                }
                            case "publishscript_prod":
                                {
                                    PublishScriptPROD = second;
                                    break;
                                }
                        }
                    }
                    MW.Log("settings", line);
                    counter++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid Settings " + ex.Message + Environment.NewLine + ex.StackTrace);
                Environment.Exit(0);
            }
            #endregion

            MW.Log("settings", "Ready", Color.FromRgb(0, 255,0), true);
            MW.Log("settings", "");
            MW.Log("settings", "");
        }

        internal static void CreateDir(string id, string Path)
        {
            Directory.CreateDirectory(Path);
        }

        internal static bool ResetDir(string id, string Path)
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Move(Path, Path + "_del");
                    Directory.Delete(Path + "_del", true);
                }
                Directory.CreateDirectory(Path);
                return true;
            }
            catch (Exception ex)
            {
                MW.Log(id, "Failed to Reset Dir: " + ex.Message);
                return false;
            }

        }

        internal static int CMD(string id, string Path, string Command)
        {

            MW.Log(id, "");
            MW.Log(id, "New CMD", null, true);
            MW.Log(id, Path);
            MW.Log(id, Command);
            MW.Log(id, "");
            MW.Log(id, "Starting up", Color.FromRgb(0, 0, 255), true);

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.WorkingDirectory = Path;
                p.StartInfo.Arguments = "/c " + Command;
                p.Start();
                p.WaitForExit();

                MW.Log(id, "Exited with code " + p.ExitCode, p.ExitCode == 0 ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0), true);

                if (p.ExitCode != 0)
                {
                    MW.LockPack = true;
                }

                return p.ExitCode;
            }
            catch (Exception ex)
            {
                MW.Log(id, ex.Message + Environment.NewLine + Path + ": " + Command);
                return -1;
            }
        }

        internal static int Exec(string id, string Path, string Args)
        {
            MW.Log(id, "");
            MW.Log(id, "New Process", null, true);
            MW.Log(id, Path);
            MW.Log(id, Args);
            MW.Log(id, "");
            MW.Log(id, "Starting up", Color.FromRgb(0, 0, 255), true);

            try
            {
                Process p = new Process();
                p.StartInfo.FileName = Path;
                p.StartInfo.Arguments = Args;
                p.StartInfo.Verb = "runas";
                p.Start();
                p.WaitForExit();

                MW.Log(id, "Exited with code " + p.ExitCode, p.ExitCode == 0 ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0), true);

                if(p.ExitCode != 0)
                {
                    MW.LockPack = true;
                }

                return p.ExitCode;
            }
            catch (Exception ex)
            {
                MW.Log(id, ex.Message + Environment.NewLine + Path + ": " + Args);
                return -1;
            }
        }

        internal static int ExecWithConsole(string id, string Path, string Args)
        {
            MW.Log(id, "");
            MW.Log(id, "New Process With Console", null, true);
            MW.Log(id, Path);
            MW.Log(id, Args);
            MW.Log(id, "");
            MW.Log(id, "Starting up", Color.FromRgb(0, 0, 255), true);
            MW.Log(id, "");

            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = Path;
                p.StartInfo.Arguments = Args;
                p.StartInfo.Verb = "runas";
                p.Start();

                string standard_output;
                while ((standard_output = p.StandardOutput.ReadLine()) != null)
                {
                    MW.Log(id, standard_output);
                }

                p.WaitForExit();

                MW.Log(id, "Exited with code " + p.ExitCode, p.ExitCode == 0 ? Color.FromRgb(0, 255, 0) : Color.FromRgb(255, 0, 0), true);

                if (p.ExitCode != 0)
                {
                    MW.LockPack = true;
                }

                return p.ExitCode;
            }
            catch (Exception ex)
            {
                MW.Log(id, ex.Message + Environment.NewLine + Path + ": " + Args);
                return -1;
            }
        }

        internal static void CopyFile(string id, string Origin, string Dest)
        {
            try
            {
                File.Copy(Origin, Dest, true);
                MW.Log(id, "Copied " + Dest);
            }
            catch
            {
                MW.Log(id, "Failed to copy " + Dest);
            }
        }

        internal static void CopyDir(string id, string Origin, string Dest)
        {
            try
            {
                DirectoryInfo OriginDirInfo = new DirectoryInfo(Origin);
                foreach (string OldPath in Directory.GetFiles(Origin, "*.*", SearchOption.AllDirectories))
                {
                    string RelativeNewPath = OldPath.Replace(OriginDirInfo.FullName + "\\", "");
                    string AbsoluteNewPath = Path.Combine(Dest, RelativeNewPath);
                    DirectoryInfo Parent = Directory.GetParent(AbsoluteNewPath);
                    if (!Directory.Exists(Parent.FullName))
                    {
                        Directory.CreateDirectory(Parent.FullName);
                    }
                    File.Copy(OldPath, AbsoluteNewPath, true);
                    MW.Log(id, "Copied " + RelativeNewPath.Replace(Dest, ""));
                }
            }
            catch
            {
                MW.Log(id, "Failed to copy " + Dest);
            }
}

        internal static void DeleteFile(string id, string Path)
        {
            try
            {
                File.Delete(Path);
            }
            catch
            {
                MW.Log(id, "Failed to delete " + Path);
            }
        }
    }
}
