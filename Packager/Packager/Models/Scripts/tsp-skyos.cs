using System.IO;
using System.Windows.Media;

namespace Packager.Models.Scripts
{
    class tsp_skyos : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_skyos";
            string ManifestsDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "manifests");
            string GitDir = Path.Combine(App.BasePath, "tsp-skyos-" + App.MW.TargetVersion, "skyos_" + App.MW.TargetVersion);
            string OutputSkyOSDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "skyos");
            
            App.ResetDir(Id, OutputSkyOSDir);

            //\src\sys\components\changelog
            if(App.MW.NewVersion)
            {
                App.Exec(Id, App.BuildNumberMaker, "changelog \"" + Path.Combine(GitDir, "src", "sys", "components", "changelog", "changes.json") + "\"");
                App.CopyFile(Id, Path.Combine(GitDir, "src", "sys", "components", "changelog", "changes.json"), Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "changes.json"));
            }

            if (App.CMD(Id, GitDir, "npm run electron:build") == 0)
            {
                App.CreateDir(Id, Path.Combine(OutputSkyOSDir, "resources"));
                App.CopyFile(Id, Path.Combine(GitDir, "dist_electron", "win-unpacked", "resources", "app.asar"), Path.Combine(OutputSkyOSDir, "resources", "app.asar"));
                
                App.CreateDir(Id, ManifestsDir);
                App.Exec(Id, App.ManifestMakerPath, "\"" + OutputSkyOSDir + "\" \"" + ManifestsDir + "\" \"skyos.txt\"");

            };


            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
