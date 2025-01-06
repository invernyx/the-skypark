using System;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Globalization;

namespace TSP_Transponder.Models.API
{
    public class FileRequest
    {
        bool Active = false;
        Action<float, long, long> ProgressMethod = null;
        Action CompletedMethod = null;
        Action<Exception> ErrorMethod = null;
        string URL = "";
        string SavePath = "";
        Stopwatch Timeout = new Stopwatch();

        public FileRequest(Action<float, long, long> ProgressMethod, Action CompletedMethod, Action<Exception> ErrorMethod, string URL, string SavePath, bool ClearDest = false)
        {
            this.ProgressMethod = ProgressMethod;
            this.CompletedMethod = CompletedMethod;
            this.ErrorMethod = ErrorMethod;
            this.URL = URL;
            this.SavePath = SavePath;

            FileInfo fInfo = new FileInfo(SavePath);
            string dirName = fInfo.Directory.FullName;

            if(ClearDest)
            {
                try
                {
                    if (Directory.Exists(dirName))
                    {
                        Directory.Delete(dirName, true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to delete dir " + dirName + " because " + ex.Message);
                }
            }

            if(!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            Active = true;
            Timeout.Start();
            using (WebClient wc = new WebClient())
            {
                Console.WriteLine("Downloading " + this.URL);
                wc.DownloadProgressChanged += DownloadProgressChanged;
                wc.DownloadFileCompleted += DownloadFileCompleted;
                wc.DownloadFileAsync(new System.Uri(this.URL), this.SavePath);

                Thread TimeoutCheck = new Thread(() => {
                    while (Active)
                    {
                        if (Timeout.ElapsedMilliseconds < 30000 && !App.MW.IsShuttingDown)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Console.WriteLine("Timeout of Download");
                            Active = false;
                            wc.CancelAsync();
                        }
                    }
                });
                TimeoutCheck.IsBackground = true;
                TimeoutCheck.CurrentCulture = CultureInfo.CurrentCulture;
                TimeoutCheck.Start();
            }

        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Active = false;
            Timeout.Reset();
            if (e.Error != null || e.Cancelled)
            {
                Console.WriteLine("Failed to download " + URL + " because " + e.Error.Message);
                try
                {
                    if (File.Exists(SavePath))
                    {
                        File.Delete(SavePath);
                    }
                }
                catch
                {
                }
                ErrorMethod?.Invoke(e.Error);
            }
            else
            {
                CompletedMethod?.Invoke();
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //Console.WriteLine("Downloading " + URL + " - " + (100.0f / e.TotalBytesToReceive) * e.BytesReceived);
            Timeout.Restart();
            ProgressMethod?.Invoke((100.0f / e.TotalBytesToReceive) * e.BytesReceived, e.TotalBytesToReceive, e.BytesReceived);
        }
    }
}
