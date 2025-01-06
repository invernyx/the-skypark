using System.Collections.Generic;
using static TSP_Installer.LicensingManager;

namespace TSP_Installer.Models
{
    class UpdateInfo
    {
        internal string Name = "";
        internal LicenseApplication Lic = LicenseApplication.None;
        internal string ManifestsURL = "";
        internal string ConsoleDir = "";
        internal Dictionary<string, byte[]> ManifestData = new Dictionary<string, byte[]>();
        internal List<UpdateModule> Modules = new List<UpdateModule>();
        
    }

    class UpdateModule
    {
        internal string Name = "";
        internal string ID = "";
        internal LicenseApplication Lic = LicenseApplication.None;
        internal string DownloadURL = "";
        internal string ManifestSubDirName = "Manifests";
        internal string InstallPath = "";
        internal bool SkipValidate = false;
        internal bool CanDropConsole = false;
        internal bool CleanupExcess = false;
        internal bool CleanupExcessExcludeRoot = false;
    }
}
