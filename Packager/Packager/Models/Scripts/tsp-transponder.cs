using System.IO;
using System.Windows.Media;

namespace Packager.Models.Scripts
{
    class tsp_transponder : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_transponder";
            string ManifestsDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "manifests");
            string GitDir = Path.Combine(App.BasePath, "tsp-transponder-" + App.MW.TargetVersion);
            string OutputDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "transponder");

            App.ResetDir(Id, OutputDir);

            if (App.MW.NewVersion)
            {
                if (App.Exec(Id, App.BuildNumberMaker, "save \"" + Path.Combine(GitDir, "TSP_Transponder", "Resources", "BuildNumber.txt") + "\"") == 0)
                {
                    App.CopyFile(Id, Path.Combine(GitDir, "TSP_Transponder", "Resources", "BuildNumber.txt"), Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "v.txt"));
                }
            }

            //App.CopyFile(Path.Combine(App.BasePath, "tsp-installer", "BOSS", "Final", "The Skypark Installer.exe"), Path.Combine(GitDir, "TSP_Transponder", "Models", "Updater", "The Skypark Updater.exe"));

            if (App.Exec(Id, App.MSBuildPath, "\"" + Path.Combine(GitDir, "Release.proj") + "\"") == 0)
            {
                App.CopyFile(Id, Path.Combine(GitDir, "BOSS", "Final", "TSP_Transponder.exe"), Path.Combine(OutputDir, "Transponder.exe"));
                App.CopyFile(Id, Path.Combine(GitDir, "TSP_Launcher", "bin", "Release", "TSP_Launcher.exe"), Path.Combine(OutputDir, "Launcher.exe"));
                App.CopyFile(Id, Path.Combine(App.BasePath, "tsp-installer", "BOSS", "Final", "The Skypark Installer.exe"), Path.Combine(OutputDir, "Updater.exe"));

                App.ExecWithConsole(Id, Path.Combine(App.ToolsPath, "DigiCertUtil.exe"), @"sign /noInput /sha1 " + App.SHA1 + " \"" + Path.Combine(OutputDir, "Transponder.exe") + "\"");
                App.ExecWithConsole(Id, Path.Combine(App.ToolsPath, "DigiCertUtil.exe"), @"sign /noInput /sha1 " + App.SHA1 + " \"" + Path.Combine(OutputDir, "Launcher.exe") + "\"");
                App.ExecWithConsole(Id, Path.Combine(App.ToolsPath, "DigiCertUtil.exe"), @"sign /noInput /sha1 " + App.SHA1 + " \"" + Path.Combine(OutputDir, "Updater.exe") + "\"");

                App.CreateDir(Id, ManifestsDir);
                App.ExecWithConsole(Id, App.ManifestMakerPath, "\"" + OutputDir + "\" \"" + ManifestsDir + "\" \"transponder.txt\"");
            };

            
            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
