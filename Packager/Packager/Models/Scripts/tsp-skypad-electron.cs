using System.IO;
using System.Windows.Media;

namespace Packager.Models.Scripts
{
    class tsp_skypad_electron : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_skypad_electron";
            string ManifestsDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "manifests");
            string GitDir = Path.Combine(App.BasePath, "tsp-skyos-" + App.MW.TargetVersion, "skyos_" + App.MW.TargetVersion);
            string OutputSkypadDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "skypad");
            
            App.ResetDir(Id, OutputSkypadDir);
            
            App.CopyDir(Id, Path.Combine(GitDir, "dist_electron", "win-unpacked"), OutputSkypadDir);
            App.DeleteFile(Id, Path.Combine(OutputSkypadDir, "resources", "app.asar"));
            
            App.Exec(Id, Path.Combine(App.ToolsPath, "DigiCertUtil.exe"), @"sign /noInput /sha1 " + App.SHA1 + " \"" + Path.Combine(OutputSkypadDir, "Skypad.exe") + "\"");

            App.CreateDir(Id, ManifestsDir);
            App.Exec(Id, App.ManifestMakerPath, "\"" + OutputSkypadDir + "\" \"" + ManifestsDir + "\" \"skypad.txt\"");
            
            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
