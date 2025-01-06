using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.FlightPlans.Types;

namespace TSP_Transponder.Models.FlightPlans
{
    public class FlightPlansDirectory
    {
        public string Path = "";
        public string Signature = "";

        public FlightPlansDirectory(string _Path)
        {
            Path = _Path;
            CultureInfo.CurrentCulture = App.CI;
            
            foreach (PlanTypeBase Base in FlightPlans.Plans.PlanConverters)
            {
                try
                {
                    // Create a new FileSystemWatcher and set its properties.
                    FileSystemWatcher watcher = new FileSystemWatcher
                    {
                        Path = Path,
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                        Filter = "*" + Base.Ext
                    };

                    // Add event handlers.
                    watcher.Changed += new FileSystemEventHandler(OnChanged);
                    watcher.Created += new FileSystemEventHandler(OnChanged);
                    watcher.Deleted += new FileSystemEventHandler(OnDelete);
                    watcher.Renamed += new RenamedEventHandler(OnRenamed);

                    // Begin watching.
                    watcher.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {   
                    Console.WriteLine("Failed to read folder: " + Path + " because " + ex.Message);
                }
            }
            
        }

        public void CalculateSignature()
        {
            CultureInfo.CurrentCulture = App.CI;

            string OutputSignature = "0";
            string[] files = Directory.GetFiles(Path);
            foreach (string dir in files)
            {
                OutputSignature += Utils.TimeStamp(File.GetLastWriteTimeUtc(dir)) + ",";
            }
            
            StringBuilder sb = new StringBuilder();
            foreach (byte b in Utils.GetHash(OutputSignature))
            {
                sb.Append(b.ToString("X2"));
            }
            Signature = sb.ToString();
        }

        private void OnDelete(object sender, FileSystemEventArgs e)
        {
            CultureInfo.CurrentCulture = App.CI;

            try
            {
                //if (UserData.Get("privacy_remote_access") == "1")
                //{
                string path = Utils.CleanupPath(e.FullPath);
                string parent = Directory.GetParent(path).FullName;
                CalculateSignature();

                FlightPlan Plan = Plans.PlansList.Find(x => x.OriginDirectory == parent && x.File == e.Name);
                if(Plan != null)
                {
                    Plan.Delete();
                    Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
                    {
                        { "Hash", Plan.Hash },
                        { "Dir", parent },
                    };
                    APIBase.ClientCollection.SendMessage("flightplans:delete", App.JSSerializer.Serialize(rs), null, APIBase.ClientType.Skypad);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to delete flight plan file: " + ex.Message);
            }

        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            CultureInfo.CurrentCulture = App.CI;

            try
            {
                //if (UserData.Get("privacy_remote_access") == "1")
                //{
                string path = Utils.CleanupPath(e.FullPath);
                string parent = Directory.GetParent(path).FullName;
                CalculateSignature();

                string Ext = e.FullPath.Split('.').Last();
                FlightPlan Plan = Plans.PlanConverters.Find(x => x.Ext == '.' + Ext.ToLower()).ReadFile(path);

                if (Plan != null)
                {
                    lock (Plans.PlansList)
                    {
                        int PlanIndex = Plans.PlansList.FindIndex(x => x.OriginDirectory == parent && x.File == e.Name);
                        if (PlanIndex != -1)
                        {
                            int UID = Plans.PlansList[PlanIndex].UID;
                            Plans.PlansList.RemoveAt(PlanIndex);
                            Plans.PlansList.Insert(PlanIndex, Plan);
                            Plan.UID = UID;
                        }
                        else
                        {
                            Plans.PlansList.Add(Plan);
                        }
                    }

                    Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
                {
                    { "Hash", Plan.Hash },
                    { "Dir", parent },
                    { "Updated", Plan.ToDictionary(true) }
                };
                    APIBase.ClientCollection.SendMessage("flightplans:change", App.JSSerializer.Serialize(rs), null, APIBase.ClientType.Skypad);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get change on flight plan file: " + ex.Message);
            }

            
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            CultureInfo.CurrentCulture = App.CI;

            try
            {
                //if (UserData.Get("privacy_remote_access") == "1")
                //{
                string pathOld = Utils.CleanupPath(e.OldFullPath);
                string pathNew = Utils.CleanupPath(e.FullPath);
                string parent = Directory.GetParent(e.OldFullPath).FullName;
            
                FlightPlan Plan = Plans.PlansList.Find(x => x.OriginDirectory == parent && x.File == e.OldName);
                Plan.OriginDirectory = Directory.GetParent(e.FullPath).FullName;
                Plan.File = e.Name;
                string OldHash = Plan.Hash;
                Plan.CalculateHash();

                Dictionary<string, dynamic> rs = new Dictionary<string, dynamic>()
                {
                    { "Hash", OldHash },
                    { "Dir", parent },
                    { "Updated", Plan.ToDictionary(true) }
                };
                APIBase.ClientCollection.SendMessage("flightplans:rename", App.JSSerializer.Serialize(rs), null, APIBase.ClientType.Skypad);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to rename flight plan file: " + ex.Message);
            }

        }
    }
}
