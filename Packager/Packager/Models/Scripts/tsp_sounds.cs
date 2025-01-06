using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Packager.Models.Scripts
{ 
    class tsp_sounds : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_sounds";
            string GitDir = Path.Combine(App.BasePath, "tsp-skyos-" + App.MW.TargetVersion, "skyos_" + App.MW.TargetVersion);
            string AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\v"+ App.MW.TargetVersion + @"\");

            if (App.MW.TargetVersion == "2")
            {
                AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\" + App.MW.TargetVersion + @"\");
            }

            App.ExecWithConsole(Id, "cmd", "gcloud config configurations activate p42cdn");

            #region Publish Sounds
            string ChangelogImagesPath = Path.Combine(AppdataDir, "Sounds_Export");
            if (Directory.Exists(ChangelogImagesPath))
            {
                App.ExecWithConsole(Id, "cmd", "gsutil -m rsync -r \"" + ChangelogImagesPath + "\" " + App.CGNPath + "/common/sounds");
            }
            #endregion
                        
            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
