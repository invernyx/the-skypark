using System.IO;
using System.Windows.Media;

namespace Packager.Models.Scripts
{
    class tsp_topo : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_topo";
            string ManifestsDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "manifests");
            string GitDir = Path.Combine(App.BasePath, "tsp-transponder-" + App.MW.TargetVersion);
            string OutputDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "topo");

            App.ResetDir(Id, OutputDir);
            App.CopyDir(Id, Path.Combine(GitDir, "TSP_Transponder", "bin", "x64", "Debug", "gdal"), Path.Combine(OutputDir, "gdal"));
            App.CopyDir(Id, Path.Combine(GitDir, "TSP_Transponder", "bin", "x64", "Debug", "Topo"), Path.Combine(OutputDir, "Topo"));
            App.DeleteFile(Id, Path.Combine(OutputDir, "Topo", "meta.json"));

            App.CreateDir(Id, ManifestsDir);
            App.Exec(Id, App.ManifestMakerPath, "\"" + OutputDir + "\" \"" + ManifestsDir + "\" \"topo.txt\"");
            
            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
