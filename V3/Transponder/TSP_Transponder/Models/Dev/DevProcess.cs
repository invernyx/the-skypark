using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using LiteDB;
using TSP_Transponder.Models.API;
using static TSP_Transponder.App;
using TSP_Transponder.Models.Adventures;
using TSP_Transponder.Models.Notifications;
using TSP_Transponder.Models.Adventures.RouteGeneration;

namespace TSP_Transponder.Models.Dev
{
    public class DevProcess
    {
#if DEBUG
        public static void Startup_Pre()
        {

        }

        public static void Startup_Post()
        {
            RouteReader.Startup();
        }

        public static bool TemplateValidation(string FileName, Dictionary<string, dynamic> source)
        {
            string SaveDir = Path.Combine(AppDataDirectory, "Images", "Adventures");
            if (!Directory.Exists(SaveDir))
                Directory.CreateDirectory(SaveDir);
            bool SaveIt = false;

            // Add the "CompleteActions" entry to every Situations
            //foreach (var situation in source["Situations"])
            //{
            //    if (!situation.ContainsKey("CompleteActions"))
            //    {
            //        situation.Add("CompleteActions", new ArrayList());
            //        SaveIt = true;
            //    }
            //
            //}

            // Add set AircraftRecommendation
            //if (!source.ContainsKey("AircraftRecommendation"))
            //{
            //    source.Add("AircraftRecommendation", null);
            //    SaveIt = true;
            //}

            // Check & Remove Prospect
            //if (source["Tiers"].Contains("prospect"))
            //{
            //    if ((source["Name"] != string.Empty))
            //    {
            //        MessageBox.Show("Removed Prospect from " + FileName + " (Name)", "The Skypark Transponder");
            //        source["Tiers"].Remove("prospect");
            //        SaveIt = true;
            //    }
            //
            //    if (source["Company"].Contains("coyote") || source["Company"].Contains("skyparktravel"))
            //    {
            //        MessageBox.Show("Removed Prospect from " + FileName + " (Coyote/Travel)", "The Skypark Transponder");
            //        source["Tiers"].Remove("prospect");
            //        SaveIt = true;
            //    }
            //}

            //#region Update template Modified Date to today
            //source["ModifiedOn"] = DateTime.UtcNow.ToString("O");
            //SaveIt = true;
            //#endregion

            /*
            #region Update cargo version on simple templates
            bool is_complex = false;
            Dictionary<string, dynamic> pickup_action = null;
            Dictionary<string, dynamic> dropoff_action = null;
            foreach (var action in source["Actions"])
            {
                switch (action["Action"])
                {
                    case "cargo_pickup":
                        {
                            if (pickup_action == null)
                            {
                                pickup_action = action;
                            }
                            else
                            {
                                is_complex = true;
                            }
                            break;
                        }
                    case "cargo_dropoff":
                        {
                            if (dropoff_action == null)
                            {
                                dropoff_action = action;
                            }
                            else
                            {
                                is_complex = true;
                            }
                            break;
                        }
                }
            }

            var pickup_counts = ((ArrayList)source["Actions"]).Cast<Dictionary<string, dynamic>>().ToList().FindAll(x => ((string)x["Action"]) == "cargo_pickup").Count;

            if (!is_complex && pickup_action != null && dropoff_action != null)
            {
                int pickup_index = source["Actions"].IndexOf(pickup_action);
                int dropoff_index = source["Actions"].IndexOf(dropoff_action);

                var newPickup = new Dictionary<string, dynamic>()
                {
                    { "UID", pickup_action["UID"] },
                    { "Action", "cargo_pickup_2" },
                    { "Params", new Dictionary<string, dynamic>() {
                        { "Manifests", new List<Dictionary<string, dynamic>>() {
                            new Dictionary<string, dynamic>()
                            {
                                { "UID", Convert.ToInt32(pickup_action["UID"]) + 1 },
                                { "Tag", pickup_action["Params"]["Model"] },
                                { "MinPercent", 60f / pickup_counts },
                                { "MaxPercent", 100f / pickup_counts },
                                { "AutoBracket", "recommended_aircraft_min" },
                            }
                        }},
                        { "LoadedActions", pickup_action["Params"]["LoadedActions"] },
                        { "ForgotActions", pickup_action["Params"]["ForgotActions"] },
                        { "EndActions", pickup_action["Params"]["EndActions"] },
                    }}
                };

                source["Actions"].Remove(pickup_action);
                source["Actions"].Insert(pickup_index, newPickup);

                var newDropoff = new Dictionary<string, dynamic>()
                {
                    { "UID", dropoff_action["UID"] },
                    { "Action", "cargo_dropoff_2" },
                    { "Params", new Dictionary<string, dynamic>() {
                        { "Manifests", new List<Dictionary<string, dynamic>>() {
                            new Dictionary<string, dynamic>()
                            {
                                { "UID", pickup_action["UID"] },
                                { "Manifests", new List<Dictionary<string, dynamic>>() {
                                    new Dictionary<string, dynamic>()
                                    {
                                        { "ID", Convert.ToInt32(pickup_action["UID"]) + 1 },
                                        { "MinDeliveryRatio", 1 },
                                        { "MaxDeliveryRatio", 100 }
                                    }
                                }}
                            }
                        }},
                        { "UnloadedActions", dropoff_action["Params"]["UnloadedActions"] },
                        { "ForgotActions", dropoff_action["Params"]["ForgotActions"] },
                        { "EndActions", dropoff_action["Params"]["EndActions"] },
                    }}
                };

                source["Actions"].Remove(dropoff_action);
                source["Actions"].Insert(dropoff_index, newDropoff);

                SaveIt = true;
            }
            #endregion
            */

            if (SaveIt)
            {
                string SavePath = Path.Combine(DocumentsDirectory, "Adventures");

                if (!Directory.Exists(SavePath))
                    Directory.CreateDirectory(SavePath);

                using (StreamWriter writer = new StreamWriter(Path.Combine(SavePath, FileName + ".p42adv"), false))
                    writer.WriteLine(JSSerializer.Serialize(source));
            }

            return SaveIt;
        }
        
        public static bool TemplateDownloadImages(string FileName, Dictionary<string, dynamic> source)
        {
            string SaveDir = Path.Combine(AppDataDirectory, "Images", "Adventures");
            if (!Directory.Exists(SaveDir))
            {
                Directory.CreateDirectory(SaveDir);
            }

            bool SaveIt = false;
            foreach (var entry in new Dictionary<string, ArrayList>()
            {
                { "ImageURL", ((ArrayList)source["ImageURL"]) },
                { "GalleryURL", ((ArrayList)source["GalleryURL"]) },
            })
            {
                List<string> newImageURL = entry.Value.Cast<string>().ToList();
                int index = 0;
                foreach (string img in source[entry.Key])
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
                                    }
                                    SaveIt = true;
                                    break;
                                }
                            case "i.imgur.com":
                                {
                                    string[] imgSpl = img.Split('/');
                                    string[] imgSplExt = imgSpl.Last().Split('.');
                                    string ImgName = imgSplExt[0] + ".jpg";
                                    string SavePath = Path.Combine(SaveDir, ImgName);

                                    if (!File.Exists(SavePath))
                                    {
                                        System.Drawing.Image imgObj = Utils.DownloadImageFromUrl(img);
                                        imgObj = Utils.ResizeImage(imgObj, 1920);

                                        using (System.Drawing.Bitmap ImageBitmap = new System.Drawing.Bitmap(imgObj))
                                        {
                                            try
                                            {
                                                if (!File.Exists(SavePath))
                                                {
                                                    ImageBitmap.Save(SavePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                                }
                                            }
                                            catch
                                            {
                                            }
                                        };
                                    }

                                    newImageURL[index] = "%imagecdn%/adventures/" + ImgName;
                                    SaveIt = true;
                                    break;
                                }
                            case "images.unsplash.com":
                                {
                                    // "https://images.unsplash.com/photo-1558489580-faa74691fdc5?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1920&q=60"

                                    string[] imgSpl = img.Split('/');
                                    string[] imgSplExt = imgSpl.Last().Split('?');
                                    string ImgName = imgSplExt[0].Replace("photo-", "") + ".jpg";
                                    string SavePath = Path.Combine(SaveDir, ImgName);

                                    if (!File.Exists(SavePath))
                                    {
                                        System.Drawing.Image imgObj = Utils.DownloadImageFromUrl(img);
                                        imgObj = Utils.ResizeImage(imgObj, 1920);

                                        using (System.Drawing.Bitmap ImageBitmap = new System.Drawing.Bitmap(imgObj))
                                        {
                                            ImageBitmap.Save(SavePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        };
                                    }

                                    newImageURL[index] = "%imagecdn%/adventures/" + ImgName;
                                    SaveIt = true;
                                    break;
                                }
                            default:
                                {

                                    break;
                                }
                        }
                    }

                    index++;

                }

                source[entry.Key] = new ArrayList(newImageURL);
            }
            
            if (SaveIt)
            {
                string SavePath = Path.Combine(DocumentsDirectory, "Adventures");

                if (!Directory.Exists(SavePath))
                {
                    Directory.CreateDirectory(SavePath);
                }

                using (StreamWriter writer = new StreamWriter(Path.Combine(SavePath, FileName + ".p42adv"), false))
                {
                    writer.WriteLine(JSSerializer.Serialize(source));
                }
            }

            return SaveIt;
        }
#endif
    }
}
