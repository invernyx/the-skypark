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

namespace TSP_Transponder.Models.PostProcessing
{

    public class DevProcess
    {
#if DEBUG
        public static void TemplateValidation(string FileName, Dictionary<string, dynamic> source)
        {
            string SaveDir = Path.Combine(AppDataDirectory, "Images", "Adventures");
            if (!Directory.Exists(SaveDir))
                Directory.CreateDirectory(SaveDir);
            bool SaveIt = false;

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
        }
        
        public static void TemplateDownloadImages(string FileName, Dictionary<string, dynamic> source)
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
                            case "cdn.invernyx.com":
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
        }
#endif
    }
}
