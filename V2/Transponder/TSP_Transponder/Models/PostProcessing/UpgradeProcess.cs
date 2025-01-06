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
using TSP_Transponder.Models.Notifications;

namespace TSP_Transponder.Models.PostProcessing
{
    public class UpgradeProcess
    {
        public static int VersionUpgrade()
        {
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
                    previousVersion = new Version("0.0.0");
                }

                currentVersion = new Version(App.BuildNumber);

                if (currentVersion.ToString() != previousVersion.ToString())
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

                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to upgrade from " + previousVersion.ToString() + " to " + currentVersion.ToString() + " because " + ex.Message + " - " + ex.StackTrace);
                return 1;
            }
        }
        
        public static void TemplatesValidation(bool IsPersisted)
        {
            if(App.IsDev)
            {
                #region Check for Cargo Type 2
                lock (AdventuresBase.Templates)
                {
                    foreach (var template in AdventuresBase.Templates.FindAll(x => x.Activated))
                    {
                        var found = template.Actions.Find(x => x.GetType() == typeof(cargo_pickup_2) || x.GetType() == typeof(cargo_dropoff_2));

                        if (found != null)
                        {
                            NotificationService.Add(new Notification()
                            {
                                Title = template.FileName + " has Cargo v2",
                                Message = "Please downgrade to v1",
                                Type = NotificationType.Status,
                                IsTransponder = true,
                            });
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

            int index = 0;
            foreach (string img in source["ImageURL"])
            {
                if (img != string.Empty && img[0] != '%')
                {
                    Uri Uri = new Uri(img);

                    switch (Uri.Host)
                    {
                        case "cdn.invernyx.com":
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


            index = 0;
            foreach (string img in source["GalleryURL"])
            {
                if (img != string.Empty && img[0] != '%')
                {
                    Uri Uri = new Uri(img);

                    switch (Uri.Host)
                    {
                        case "cdn.invernyx.com":
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




            if (SaveIt)
            {
                source["ImageURL"] = new ArrayList(newImageURL);
                source["GalleryURL"] = new ArrayList(newGalleryURL);
            }

            return SaveIt;
        }
        
    }
}
