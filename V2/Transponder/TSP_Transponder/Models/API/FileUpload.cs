using System;
using System.IO;
using System.Net.Http;

namespace TSP_Transponder.Models.API
{
    public class FileUpload
    {
        private static async void PostAs(string actionUrl, string paramString, byte[] paramFileBytes)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent("Upload----"))
                    {
                        content.Add(new StreamContent(new MemoryStream(paramFileBytes)), "file", "transponder_log.txt");

                        using (var message = await client.PostAsync(actionUrl + "?" + paramString, content))
                        {
                            var input = await message.Content.ReadAsStringAsync();
                            Console.WriteLine(input);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send log files " + ex.Message);
            }
        }

        internal static void Upload(string actionUrl, string paramString, byte[] paramFileBytes)
        {
            PostAs(actionUrl, paramString, paramFileBytes);
        }
    }
}
