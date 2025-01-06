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
    class tsp_airports : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_airports";
            string GitDir = Path.Combine(App.BasePath, "tsp-skyos-" + App.MW.TargetVersion, "skyos_" + App.MW.TargetVersion);
            string AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\v"+ App.MW.TargetVersion + @"\");

            if (App.MW.TargetVersion == "2")
            {
                AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\" + App.MW.TargetVersion + @"\");
            }

            App.ExecWithConsole(Id, "cmd", "gcloud config configurations activate p42cdn");
            
            #region Publish Avatar Images
            string AirportImagesPath = Path.Combine(AppdataDir, "Airports");
            if (Directory.Exists(AirportImagesPath))
            {
                App.ExecWithConsole(Id, "cmd", "gsutil -m rsync -r \"" + AirportImagesPath + "\" " + App.CGNPath + "/common/images/airports");
            }
            #endregion

            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
