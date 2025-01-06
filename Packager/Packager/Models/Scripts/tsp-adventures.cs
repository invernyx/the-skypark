using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Windows.Media;

namespace Packager.Models.Scripts
{ 
    class tsp_adventures : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_adventures";
            string GitDir = Path.Combine(App.BasePath, "tsp-transponder-" + App.MW.TargetVersion);
            string ManifestsDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "adventures_manifests");
            string OutputDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "adventures");
            string AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\v"+ App.MW.TargetVersion + @"\");
            string ChangelogPath = Path.Combine(App.BasePath, "tsp-skyos-" + App.MW.TargetVersion, "skyos_" + App.MW.TargetVersion, "src", "sys", "components", "changelog", "changes_adv.json");

            if(App.MW.TargetVersion == "2")
            {
                AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\" + App.MW.TargetVersion + @"\");
            }

            App.ResetDir(Id, OutputDir);

            if(!Directory.Exists(AppdataDir))
            {
                App.MW.Log(Id, "Adventures don't exist in " + AppdataDir, Color.FromRgb(255, 0, 0), true);
            }
            else
            {
                if (App.MW.NewVersion)
                {
                    App.Exec(Id, App.BuildNumberMaker, "changelog \"" + ChangelogPath + "\"");
                }
                
                App.CopyDir(Id, Path.Combine(AppdataDir, "Persistence_Debug"), Path.Combine(OutputDir, "Persistence"));
                App.CopyFile(Id, Path.Combine(AppdataDir, "748b154629c5_debug.dat"), Path.Combine(OutputDir, "748b154629c5.dat"));
                
                App.CopyFile(Id, ChangelogPath, Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "changes_adv.json"));
                
                string Pack = Path.Combine(GitDir, "TSP_Transponder", "Models", "Adventures", "DefaultPack", "Adventures.zip");
                if(File.Exists(Pack))
                {
                    File.Delete(Pack);
                }
                ZipFile.CreateFromDirectory(OutputDir, Pack, CompressionLevel.Optimal, false);

                App.CreateDir(Id, ManifestsDir);
                App.Exec(Id, App.ManifestMakerPath, "\"" + OutputDir + "\" \"" + ManifestsDir + "\" \"adventures.txt\"");
            }
            
            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
