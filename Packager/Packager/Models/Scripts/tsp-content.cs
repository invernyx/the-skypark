using System.IO;
using System.Windows.Media;

namespace Packager.Models.Scripts
{
    class tsp_content : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_content";
            string ManifestsDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "manifests");
            string GitDir = Path.Combine(App.BasePath, "tsp-content");
            string OutputDir = Path.Combine(App.BasePath, "Dist", App.MW.TargetVersion, "Output", "content");
            string ScenrModelsPath = Path.Combine(App.BasePath, "tsp-secret-lair", "src", "assets", "scenr", "models");
            string SkyosModelsPath = Path.Combine(App.BasePath, "tsp-skyos", "assets", "items");

            App.ResetDir(Id, OutputDir);
            App.CopyDir(Id, Path.Combine(GitDir, "content"), OutputDir);

            App.ResetDir(Id, ScenrModelsPath);
            App.ResetDir(Id, SkyosModelsPath);
            string[] Models = Directory.GetDirectories(Path.Combine(OutputDir, "SimObjects"));
            foreach(string Model in Models)
            {
                string[] ImageFiles = Directory.GetFiles(Model, "*.png", SearchOption.TopDirectoryOnly);
                foreach (string ImageFile in ImageFiles)
                {
                    FileInfo FI = new FileInfo(ImageFile);
                    if (Model.EndsWith("_loadable"))
                    {
                        App.CopyFile(Id, ImageFile, Path.Combine(SkyosModelsPath, FI.Name));
                    }
                    App.CopyFile(Id, ImageFile, Path.Combine(ScenrModelsPath, FI.Name));
                }
            }

            App.CreateDir(Id, ManifestsDir);
            App.Exec(Id, App.ManifestMakerPath, "\"" + OutputDir + "\" \"" + ManifestsDir + "\" \"content.txt\"");

            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
