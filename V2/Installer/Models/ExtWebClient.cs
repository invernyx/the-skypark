using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace TSP_Installer.Models
{
    class ExtWebClient : WebClient
    {
        public NameValueCollection PostParam { get; set; }

        public int Timeout { get; set; }

        public ExtWebClient()
        {
            Timeout = 600000;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.Timeout = Timeout;

            // Set the auto decompression
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            if (request != null && PostParam != null && PostParam.Count > 0)
            {
                StringBuilder postBuilder = new StringBuilder();
                request.Method = "POST";
                //build the post string

                for (int i = 0; i < PostParam.Count; i++)
                {
                    postBuilder.AppendFormat("{0}={1}", Uri.EscapeDataString(PostParam.GetKey(i)),
                                             Uri.EscapeDataString(PostParam.Get(i)));
                    if (i < PostParam.Count - 1)
                    {
                        postBuilder.Append("&");
                    }
                }
                byte[] postBytes = Encoding.ASCII.GetBytes(postBuilder.ToString());
                request.ContentLength = postBytes.Length;
                request.ContentType = "application/x-www-form-urlencoded";

                using(var stream = request.GetRequestStream())
                {
                    stream.Write(postBytes, 0, postBytes.Length);
                    stream.Close();
                }
            }

            return request;
        }
    }
}
