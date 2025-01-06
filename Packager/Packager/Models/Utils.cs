using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packager.Models
{
    class Utils
    {
        public static Image DownloadImageFromUrl(string imageUrl)
        {
            Image image = null;

            try
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(imageUrl);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;

                System.Net.WebResponse webResponse = webRequest.GetResponse();
                Stream stream = webResponse.GetResponseStream();
                image = Image.FromStream(stream);

                webResponse.Close();
            }
            catch
            {
                return null;
            }

            return image;
        }


        public static Image ResizeImage(Image imgToResize, int width)
        {
            if (imgToResize.Width > width)
            {
                int newWidth = width;
                int newHeight = (int)Math.Round(((double)newWidth / (double)imgToResize.Width) * imgToResize.Height);

                return new Bitmap(imgToResize, newWidth, newHeight);
            }

            return new Bitmap(imgToResize);
        }

    }
}
