using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_Transponder.Models.Airports;
using TSP_Transponder.Models.API;

namespace TSP_Transponder.Models.Scenr
{
    class MapImages
    {
        private static string AirportImgDir = Path.Combine(App.AppDataDirectory, "Airports");
        public static List<Airport> ToDo = null;

        public static void Command(SocketClient Socket, string[] StructSplit, Dictionary<string, dynamic> structure)
        {
#if DEBUG
            Dictionary<string, dynamic> payload_struct = App.JSSerializer.Deserialize<Dictionary<string, dynamic>>(structure["payload"]);
            switch (StructSplit[2])
            {
                case "start":
                    {
                        if (ToDo == null)
                        {
                            ToDo = new List<Airport>(SimLibrary.SimList[0].AirportsLib.GetAirportsCopy()).OrderByDescending(x =>
                            {
                                int cumul = 0;
                                foreach (Airport.Runway d in x.Runways)
                                {
                                    cumul += d.LengthFT * d.WidthMeters;
                                }
                                return cumul;
                            }).ToList();

                            lock (ToDo)
                            {
                                if (Directory.Exists(AirportImgDir))
                                {
                                    var files = Directory.GetFiles(AirportImgDir);
                                    foreach (var file in files)
                                    {
                                        FileInfo fi = new FileInfo(file);
                                        string icao = fi.Name.Replace(".jpg", "");
                                        Airport found = ToDo.Find(x => x.ICAO == icao);
                                        if (found != null)
                                        {
                                            if (ToDo.Contains(found))
                                            {
                                                ToDo.Remove(found);
                                            }
                                        }
                                    }
                                }
                            }

                        }

                        lock (ToDo)
                        {
                            if (ToDo.Count > 0)
                            {
                                Socket.SendMessage("scenr:mapimages:getnext", App.JSSerializer.Serialize(ToDo[0].Serialize(null)), (Dictionary<string, dynamic>)structure["meta"]);
                                ToDo.RemoveAt(0);
                            }
                        }
                        break;
                    }
                case "getnext":
                    {
                        lock (ToDo)
                        {
                            if (ToDo.Count > 0)
                            {
                                Socket.SendMessage("scenr:mapimages:getnext", App.JSSerializer.Serialize(ToDo[0].Serialize(null)), (Dictionary<string, dynamic>)structure["meta"]);
                                ToDo.RemoveAt(0);
                            }
                        }

                        Image ImageFile;
                        string SavePhotoPath = Path.Combine(AirportImgDir, payload_struct["icao"]);
                        byte[] bytes = Convert.FromBase64String(((string)payload_struct["data"]).Replace("data:image/png;base64,", ""));

                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            ImageFile = Image.FromStream(ms);
                        }

                        if (!Directory.Exists(AirportImgDir))
                            Directory.CreateDirectory(AirportImgDir);
                        
                        //using (Bitmap ImageBitmap = new Bitmap(Utils.ResizeImage(ImageFile, 300)))
                        //{
                        //    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        //    myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 60L);
                        //    ImageBitmap.Save(SavePhotoPath + "_thumb.jpg", GetEncoder(ImageFormat.Jpeg), myEncoderParameters);
                        //};

                        using (Bitmap ImageBitmap = new Bitmap(ImageFile))
                        {
                            EncoderParameters myEncoderParameters = new EncoderParameters(1);
                            myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 70L);
                            ImageBitmap.Save(SavePhotoPath + ".jpg", GetEncoder(ImageFormat.Jpeg), myEncoderParameters);
                        };

                        break;
                    }
            }
#endif
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
