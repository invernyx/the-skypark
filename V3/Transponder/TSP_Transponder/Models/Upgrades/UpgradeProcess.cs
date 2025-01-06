using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Adventures.Actions;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.DataStore;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Transactor;

namespace TSP_Transponder.Models.PostProcessing
{
    public class UpgradeProcess
    {
        public static int VersionUpgrade()
        {
            #warning Ensure Upgrade versions
            Version previousVersion = null;
            Version currentVersion = null;

            try
            {
                try
                {
                    previousVersion = new Version(App.PreviousBuildNumber);
                }
                catch
                {
                    return 0;
                }

                currentVersion = new Version(App.BuildNumber);

                if (currentVersion.ToString() != previousVersion.ToString() || App.IsDev)
                {
                    // Flush Cache
                    if(previousVersion < new Version("2021.20.1.17"))
                    {
                        File.Delete(Path.Combine(App.AppDataDirectory, "2b99ead76463.dat"));
                        File.Delete(Path.Combine(App.AppDataDirectory, "1be7d115f709.dat"));
                    }

                    // Flush Audio
                    if (previousVersion < new Version("2021.23.3.22"))
                    {
                        string soundsFolder = Path.Combine(App.AppDataDirectory, "Sounds");
                        if(Directory.Exists(soundsFolder))
                        {
                            Directory.Delete(soundsFolder, true);
                        }
                    }

                    // Move to version folder
                    if (previousVersion < new Version("2021.43.5.0"))
                    {
                        foreach (var folder in new string[]
                        {
                            App.AppDataDirectory,
                            App.DocumentsDirectory
                        })
                        {
                            var rootDir = Directory.GetParent(folder);
                            var files = Directory.GetFiles(rootDir.FullName, "*", SearchOption.TopDirectoryOnly);
                            var dirs = Directory.GetDirectories(rootDir.FullName, "*", SearchOption.TopDirectoryOnly);

                            foreach (var file in files)
                            {
                                if(File.Exists(file))
                                {
                                    var fileClean = file.Replace(rootDir.FullName + '\\', "");
                                    var filePath = Path.Combine(folder, fileClean);

                                    if (File.Exists(filePath))
                                        File.Delete(filePath);

                                    File.Move(file, filePath);
                                }
                                
                            }

                            foreach (var dir in dirs.Where(x => !x.EndsWith("\\2")))
                            {
                                var dirClean = dir.Replace(rootDir.FullName + '\\', "");
                                var dirPath = Path.Combine(folder, dirClean);

                                Directory.Move(dir, dirPath);
                            }
                        }
                        return 2;
                    }

                    // Upgrade DB
                    if (previousVersion < new Version("2023.43.5.0"))
                    {
                        LiteDbService.Startup();

                        #region Invoices
                        IEnumerable<LiteDB.BsonDocument> all = null;
                        lock (LiteDbService.DB)
                        {
                            all = LiteDbService.DB.Database.GetCollection("invoices_user").FindAll();
                        }

                        foreach (var invoice in all.ToList())
                        {
                            #region Update Params
                            foreach (var fee in invoice["Fees"].AsArray)
                            {
                                if(fee.AsDocument["Params"].AsDocument != null)
                                {
                                    LiteDB.BsonDocument newStruct = new LiteDB.BsonDocument();
                                    foreach (var key in fee.AsDocument["Params"].AsDocument.Keys)
                                    {
                                        var caps = (from ch in key.ToArray() where Char.IsUpper(ch) select key.IndexOf(ch)).ToList();
                            
                                        if (caps.Count == 1)
                                        {
                                            string newKey = key.ToLower();
                                            newStruct.Add(newKey, fee.AsDocument["Params"].AsDocument[key]);
                                        }
                                        else if(caps.Count > 1)
                                        {
                                            switch (key)
                                            {
                                                case "DistanceKM": { newStruct.Add("distance", fee.AsDocument["Params"].AsDocument[key]); break; }
                                                default:
                                                    {
                                                        newStruct.Add(key, fee.AsDocument["Params"].AsDocument[key]);
                                                        break;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            newStruct.Add(key, fee.AsDocument["Params"].AsDocument[key]);
                                        }
                            
                                    }
                                    fee.AsDocument["Params"] = newStruct;
                            
                                    if(fee["Discounts"].AsArray != null)
                                    {
                                        foreach (var discount in fee["Discounts"].AsArray)
                                        {
                                            LiteDB.BsonDocument newStruct1 = new LiteDB.BsonDocument();
                                            foreach (var key in discount.AsDocument["Params"].AsDocument.Keys)
                                            {
                                                var caps = (from ch in key.ToArray() where Char.IsUpper(ch) select key.IndexOf(ch)).ToList();
                            
                                                if (caps.Count == 1)
                                                {
                                                    string newKey = key.ToLower();
                                                    newStruct1.Add(newKey, discount.AsDocument["Params"].AsDocument[key]);
                                                }
                                                else
                                                {
                                                    newStruct1.Add(key, discount.AsDocument["Params"].AsDocument[key]);
                                                }
                            
                                            }
                                            discount.AsDocument["Params"] = newStruct1;
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Ensure compatibility
                            if(!invoice.AsDocument.Keys.Contains("Refund"))
                            {
                                lock (LiteDbService.DB)
                                {
                                    var DBCollection = LiteDbService.DB.Database.GetCollection("invoices_user");
                                    DBCollection.Delete(invoice["_id"]);
                                }
                            }
                            else
                            {
                                lock (LiteDbService.DB)
                                {
                                    var DBCollection = LiteDbService.DB.Database.GetCollection("invoices_user");
                                    DBCollection.Upsert(invoice);
                                }
                            }
                            #endregion

                        }

                        Invoices.GetAll();
                        #endregion
                        
                        #region Fleet
                        lock (LiteDbService.DB)
                        {
                            all = LiteDbService.DB.Database.GetCollection("fleet").FindAll();
                        }

                        foreach (var aircraft in all.ToList())
                        {
                            #region Ensure compatibility
                            if (!aircraft.AsDocument.Keys.Contains("Model"))
                            {
                                lock (LiteDbService.DB)
                                {
                                    var DBCollection = LiteDbService.DB.Database.GetCollection("fleet");
                                    DBCollection.Delete(aircraft["_id"]);
                                }
                            }
                            else
                            {
                                lock (LiteDbService.DB)
                                {
                                    var DBCollection = LiteDbService.DB.Database.GetCollection("fleet");
                                    DBCollection.Upsert(aircraft);
                                }
                            }
                            #endregion
                        }
                        #endregion

                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to upgrade from " + previousVersion.ToString() + " to " + currentVersion.ToString() + " because " + ex.Message + " - " + ex.StackTrace);
                return 1;
            }
        }
        
        public static void TemplatesValidation()
        {
            if(App.IsDev)
            {
                #region Check Prospect
                // Validate for Prospect
                List<AdventureTemplate> withProspect = new List<AdventureTemplate>();
                lock (AdventuresBase.Templates)
                {
                    foreach (var template in AdventuresBase.Templates)
                    {
                        if (template.Tiers.Contains("prospect"))
                        {
                            bool warn = false;
                            bool valid = template.CheckValid();

                            // Skip Intro flight
                            if ((!template.FileName.ToLower().Contains("intro") && template.IsCustom) && valid)
                                warn = true;

                            if (warn)
                                withProspect.Add(template);
                        }
                    }
                }

                long id = 83475683745;
                NotificationService.RemoveFromID(id);

                if (withProspect.Count == 0)
                {
                    NotificationService.Add(new Notification()
                    {
                        UID = id,
                        Title = "Prospect has no custom templates",
                        Message = "1 template is required",
                        Type = NotificationType.Status,
                        IsTransponder = true,
                    });
                }
                else if (withProspect.Count > 1)
                {
                    NotificationService.Add(new Notification()
                    {
                        UID = id,
                        Title = "Prospect has " + withProspect.Count + " custom templates",
                        Message = string.Join(", ", withProspect.Select(x => x.FileName)),
                        Type = NotificationType.Status,
                        IsTransponder = true,
                    });
                }
                #endregion

                #region Check for Cargo Type 1
                lock (AdventuresBase.Templates)
                {
                    foreach (var template in AdventuresBase.Templates.FindAll(x => x.Activated))
                    {
                        var found = template.Actions.Find(x => x.GetType() == typeof(cargo_pickup) || x.GetType() == typeof(cargo_dropoff));

                        if (found != null)
                        {
                            NotificationService.Add(new Notification()
                            {
                                UID = 8734568456,
                                Title = template.FileName + " still has Cargo v1",
                                Message = "Please upgrade to v2",
                                Type = NotificationType.Status,
                                IsTransponder = true,
                            });
                            break;
                        }
                    }
                }
                #endregion

                #region Check Classifications
                lock (AdventuresBase.Templates)
                {
                    foreach (var template in AdventuresBase.Templates.Where(x => x.Activated))
                    {
                        if(template.Type.Contains("Tour"))
                        {
                            if (!template.Type.Contains("Experience"))
                            {
                                NotificationService.Add(new Notification()
                                {
                                    Title = "Template is missing a category",
                                    Message = template.FileName  + " is a Tour and should also be an Experience",
                                    Type = NotificationType.Status,
                                    IsTransponder = true,
                                });
                            }
                        }
                    }
                }
                #endregion
            }

        }

        public static bool TemplateUpgrade(string FileName, Dictionary<string, dynamic> source)
        {
            bool SaveIt = false;
            
            List<string> newImageURL = ((ArrayList)source["ImageURL"]).Cast<string>().ToList();
            List<string> newGalleryURL = ((ArrayList)source["GalleryURL"]).Cast<string>().ToList();

            #region ImageURL
            int index = 0;
            foreach (string img in source["ImageURL"])
            {
                if (img != string.Empty && img[0] != '%')
                {
                    Uri Uri = new Uri(img);

                    switch (Uri.Host)
                    {
                        case "storage.googleapis.com":
                            {
                                string[] imgSpl = img.Split('/');
                                string[] imgSplExt = imgSpl.Last().Split('.');

                                if (img.Contains(APIBase.CDNImagesHost))
                                {
                                    newImageURL[index] = "%imagecdn%" + img.Replace(APIBase.CDNImagesHost, "");
                                    SaveIt = true;
                                }
                                break;
                            }
                    }
                }
            }
            #endregion

            #region GalleryURL
            index = 0;
            foreach (string img in source["GalleryURL"])
            {
                if (img != string.Empty && img[0] != '%')
                {
                    Uri Uri = new Uri(img);

                    switch (Uri.Host)
                    {
                        case "storage.googleapis.com":
                            {
                                string[] imgSpl = img.Split('/');
                                string[] imgSplExt = imgSpl.Last().Split('.');

                                if (img.Contains(APIBase.CDNImagesHost))
                                {
                                    newGalleryURL[index] = "%imagecdn%" + img.Replace(APIBase.CDNImagesHost, "");
                                    SaveIt = true;
                                }
                                break;
                            }
                    }
                }
            }
            #endregion

            if (SaveIt)
            {
                source["ImageURL"] = new ArrayList(newImageURL);
                source["GalleryURL"] = new ArrayList(newGalleryURL);
            }

            return SaveIt;
        }
        
    }
}
