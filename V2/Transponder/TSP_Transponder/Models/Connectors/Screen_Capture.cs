using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using TSP_Transponder.Models.API;
using TSP_Transponder.Models.Audio;
using static TSP_Transponder.Models.EventBus.EventManager;

namespace TSP_Transponder.Models.Connectors
{
    class Screen_Capture
    {
        internal FileSystemWatcher watcher = null;

        private EventsSession Session = null;
        private Action<HttpStatusCode, long, string, string, Image> _Callback = null;
        private long _Timecode = 0;
        private string _FileName = "";
        private bool _Stealth = false;

        public Screen_Capture(EventsSession Blog, bool Stealth, Action<HttpStatusCode, long, string, string, Image> Callback)
        {
            if(Blog.ID == -1)
            {
                return;
            }

            try
            {
                _Callback = Callback;
                Session = Blog;
                _FileName = App.Timer.ElapsedMilliseconds.ToString();

                if (!_Stealth)
                {
                    AudioFramework.GetEffect("shutter", true);
                }
                
                string DirPath = Path.Combine(App.AppDataDirectory, "Temp");
                if (!Directory.Exists(DirPath))
                {
                    Directory.CreateDirectory(DirPath);
                }

                StartWatch(DirPath);

            }
            catch
            {

            }
        }

        internal void StartWatch(string path)
        {
            watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = _FileName + ".png"
            };
            watcher.Created += new FileSystemEventHandler(ImageReady);
            watcher.EnableRaisingEvents = true;
        }
        
        private async void ImageReady(object sender, FileSystemEventArgs e)
        {
            watcher.Dispose();
            watcher = null;

            string path = Utils.CleanupPath(e.FullPath);

            _Timecode = App.Timer.ElapsedMilliseconds;
            await SendAsync(_Timecode, path, Session.ID);

        }

        private Image TryGetImage(string FilePath)
        {
            Image ImageFile = null;
            try
            {
                using (FileStream loadStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    ImageFile = Image.FromStream(loadStream);
                }
            }
            catch
            {
                Console.WriteLine("Trying to acquire image");
                Thread.Sleep(300);
                return TryGetImage(FilePath);
            }
            return ImageFile;
        }

        private async Task SendAsync(long Timecode, string FilePath, long BlogID)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string JPGFilePath = FilePath.Replace(".png", ".jpg");
                    Image ImageFile = TryGetImage(FilePath);

                    #region Save to User files
                    using (Bitmap ImageBitmap = new Bitmap(ImageFile))
                    {
                        string TSPPhotoDir = Path.Combine(Environment.GetFolderPath(
                            Environment.SpecialFolder.MyPictures),
                            "The Skypark",
                            SimConnection.ActiveSim.Name,
                            DateTime.UtcNow.Year.ToString(),
                            DateTime.UtcNow.Month.ToString(),
                            SimConnection.Aircraft.Name);

                        if (!Directory.Exists(TSPPhotoDir))
                        {
                            Directory.CreateDirectory(TSPPhotoDir);
                        }

                        string TSPPhotoPath = Path.Combine(TSPPhotoDir, SimConnection.Aircraft.LastLivery + "__" + DateTime.UtcNow.Year.ToString() + "-" + DateTime.UtcNow.Month.ToString() + "-" + DateTime.UtcNow.Day.ToString() + "__" + DateTime.UtcNow.Hour.ToString() + "-" + DateTime.UtcNow.Minute.ToString() + "-" + DateTime.UtcNow.Second.ToString() + "-" + DateTime.UtcNow.Millisecond.ToString());
                        ImageBitmap.Save(TSPPhotoPath + ".png");
                        ImageBitmap.Save(JPGFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    };
                    #endregion

                    //string ImageBase64 = Convert.ToBase64String(ImageToByteArray(ResizeImage(ImageFile, new Size(800, 600))));
                    //byte[] ImageBytes = ImageToByteArray(ResizeImage(ImageFile, new Size(1920, 1080)));

                    #region Send to Server
                    using (HttpClient httpClient = new HttpClient())
                    {
                        MultipartFormDataContent form = new MultipartFormDataContent();
                        byte[] ImageBytes = System.IO.File.ReadAllBytes(JPGFilePath);

                        form.Add(new StringContent(UserData.Get("token")), "guid");
                        form.Add(new StringContent(Convert.ToString(Timecode)), "timecode");
                        form.Add(new ByteArrayContent(ImageBytes, 0, ImageBytes.Length), "image", "1.jpg");
                        HttpResponseMessage response = await httpClient.PostAsync(APIBase.CDNImagesHost + "/blogs/" + BlogID + "/images", form);

                        string sd = response.Content.ReadAsStringAsync().Result;

                        if (response.IsSuccessStatusCode)
                        {
                            _Callback?.Invoke(response.StatusCode, _Timecode, sd, "", ImageFile);
                        }
                        else
                        {
                            _Callback?.Invoke(response.StatusCode, 0, sd, "", null);
                        }
                    };
                    #endregion
                }
                else
                {
                    _Callback?.Invoke(0, 0, "", "", null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send Screen Capture: " + ex.Message);
            }

        }

    }
}
