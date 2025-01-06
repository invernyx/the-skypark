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
    class tsp_images : ActionBase
    {
        internal override void Process()
        {
            string Id = "tsp_images";
            string GitDir = Path.Combine(App.BasePath, "tsp-skyos-" + App.MW.TargetVersion, "skyos_" + App.MW.TargetVersion);
            string AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\v"+ App.MW.TargetVersion + @"\");

            if (App.MW.TargetVersion == "2")
            {
                AppdataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Parallel 42\The Skypark DEV\" + App.MW.TargetVersion + @"\");
            }

            #region Get Changelog images
            #region Convert
            Func<string, string> Convert = (img) =>
            {

                string NewURL = img;
                string SaveDir = Path.Combine(AppdataDir, "Images", "Changelog");
                if (!Directory.Exists(SaveDir))
                {
                    Directory.CreateDirectory(SaveDir);
                }

                Uri Uri = new Uri(img);

                switch (Uri.Host)
                {
                    case "i.imgur.com":
                        {
                            App.MW.Log(Id, "Converting " + img);
                            string[] imgSpl = img.Split('/');
                            string[] imgSplExt = imgSpl.Last().Split('.');
                            string FileName = imgSplExt[0] + ".jpg";
                            string SavePath = Path.Combine(SaveDir, FileName);

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
                                            App.MW.Log(Id, "Saving new image " + SavePath);
                                            ImageBitmap.Save(SavePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                };
                            }

                            NewURL = "https://storage.googleapis.com/gilfoyle/the-skypark/v1/common/images/changelog/" + FileName;
                            break;
                        }
                    case "images.unsplash.com":
                        {
                            App.MW.Log(Id, "Converting " + img);

                            // "https://images.unsplash.com/photo-1558489580-faa74691fdc5?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=1920&q=60"

                            string[] imgSpl = img.Split('/');
                            string[] imgSplExt = imgSpl.Last().Split('?');
                            string FileName = imgSplExt[0].Replace("photo-", "") + ".jpg";
                            string SavePath = Path.Combine(SaveDir, FileName);

                            if (!File.Exists(SavePath))
                            {
                                System.Drawing.Image imgObj = Utils.DownloadImageFromUrl(img);
                                imgObj = Utils.ResizeImage(imgObj, 1920);

                                using (System.Drawing.Bitmap ImageBitmap = new System.Drawing.Bitmap(imgObj))
                                {
                                    App.MW.Log(Id, "Saving new image " + SavePath);
                                    ImageBitmap.Save(SavePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                };
                            }

                            NewURL = "https://storage.googleapis.com/gilfoyle/the-skypark/v1/common/images/changelog/" + FileName;
                            break;
                        }
                    default:
                        {

                            break;
                        }
                }

                return NewURL;
            };
            #endregion

            List<string> Changelogpaths = new List<string>() {
                Path.Combine(GitDir, "src", "sys", "components", "changelog", "changes.json"),
                Path.Combine(GitDir, "src", "sys", "components", "changelog", "changes_adv.json"),
            };

            foreach(string path in Changelogpaths)
            {
                string ChangeLogContentJson = string.Join(" ", File.ReadAllLines(path));
                var LogContent = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(ChangeLogContentJson);

                foreach (var Entry in LogContent["entries"])
                {
                    if (Entry.ContainsKey("image"))
                    {
                        Entry["image"] = Convert(Entry["image"]);
                    }

                    if (Entry.ContainsKey("changes"))
                    {
                        foreach (KeyValuePair<string, dynamic> ChangeType in Entry["changes"])
                        {
                            foreach (var Change in ChangeType.Value)
                            {
                                if (Change.ContainsKey("image"))
                                {
                                    Change["image"] = Convert(Change["image"]);
                                }
                            }
                        }
                    }

                }

                StringBuilder sb = new StringBuilder(256);
                StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

                var jsonSerializer = JsonSerializer.CreateDefault();
                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.IndentChar = ' ';
                    jsonWriter.Indentation = 4;

                    jsonSerializer.Serialize(jsonWriter, LogContent, typeof(Dictionary<string, dynamic>));
                }

                File.Delete(path);
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
                {
                    StreamWriter write = new StreamWriter(fs);
                    write.WriteLine(sw.ToString());
                    write.Flush();
                    write.Close();
                    fs.Close();
                }
            }

            #endregion
            
            App.ExecWithConsole(Id, "cmd", "gcloud config configurations activate p42cdn");

            #region Publish Changelog Images
            string ChangelogImagesPath = Path.Combine(AppdataDir, "Images", "Changelog");
            if (Directory.Exists(ChangelogImagesPath))
            {
                App.ExecWithConsole(Id, "cmd", "gsutil -m rsync -r \"" + ChangelogImagesPath + "\" " + App.CGNPath + "/common/images/changelog");
            }
            #endregion

            #region Publish Adventure Images
            string AdventureImagesPath = Path.Combine(AppdataDir, "Images", "Adventures");
            if (Directory.Exists(AdventureImagesPath))
            {
                App.ExecWithConsole(Id, "cmd", "gsutil -m rsync -r \"" + AdventureImagesPath + "\" " + App.CGNPath + "/common/images/adventures");
            }
            #endregion

            #region Publish Avatar Images
            string AvatarImagesPath = Path.Combine(AppdataDir, "Images", "Avatars");
            if (Directory.Exists(AvatarImagesPath))
            {
                App.ExecWithConsole(Id, "cmd", "gsutil -m rsync -r \"" + AvatarImagesPath + "\" " + App.CGNPath + "/common/images/avatars");
            }
            #endregion

            App.MW.Log(Id, "DONE", Color.FromRgb(0, 255, 0), true);
        }
        
    }
}
