using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace TSP_OSM_Loader
{
    internal class TestGeojson
    {
        public static void LoadGeojson()
        {

            Action<string> imageProcessor = (imageb64) =>
            {
                string base64String = imageb64.Replace("data:image/png;base64,", "");
                byte[] imageBytes = Convert.FromBase64String(base64String);

                SKBitmap bitmap;
                using (var ms = new MemoryStream(imageBytes))
                {
                    bitmap = SKBitmap.Decode(ms);
                }

                int p1_x = (int)(bitmap.Width * 0.4);
                int p1_y = (int)(bitmap.Height * 0.4);

                int p2_x = (int)(bitmap.Width * 0.5);
                int p2_y = (int)(bitmap.Height * 0.5);

                int p3_x = (int)(bitmap.Width * 0.6);
                int p3_y = (int)(bitmap.Height * 0.6);


                SKColor c11 = bitmap.GetPixel(p1_x, p1_y);
                SKColor c12 = bitmap.GetPixel(p1_x, p2_y);
                SKColor c13 = bitmap.GetPixel(p1_x, p3_y);
                SKColor c21 = bitmap.GetPixel(p2_x, p1_y);
                SKColor c22 = bitmap.GetPixel(p2_x, p2_y);
                SKColor c23 = bitmap.GetPixel(p2_x, p3_y);
                SKColor c31 = bitmap.GetPixel(p3_x, p1_y);
                SKColor c32 = bitmap.GetPixel(p3_x, p2_y);
                SKColor c33 = bitmap.GetPixel(p3_x, p3_y);

                List<double> a = new List<double>()
                {
                    Utils.ColorDifferencePercentage(255, 255, 255, c11.Red, c11.Green, c11.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c12.Red, c12.Green, c12.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c13.Red, c13.Green, c13.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c21.Red, c21.Green, c21.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c22.Red, c22.Green, c22.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c23.Red, c23.Green, c23.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c31.Red, c31.Green, c31.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c32.Red, c32.Green, c32.Blue),
                    Utils.ColorDifferencePercentage(255, 255, 255, c33.Red, c33.Green, c33.Blue),
                };

                var average = a.Average();

            };



            // load export.geojson from the local file as a text file and deserialize json
            string json = System.IO.File.ReadAllText(@"export.geojson");

            // convert Geojson to dictionary
            var geojson = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            var features = (Newtonsoft.Json.Linq.JArray)geojson["features"];

            foreach (var feature in features)
            {
                //if ((string)feature["id"] != @"way/5875683") continue;

                var geometry = (Newtonsoft.Json.Linq.JObject)feature["geometry"];
                var coordinates = (Newtonsoft.Json.Linq.JArray)geometry["coordinates"];

                var type = (string)geometry["type"];

                if (type == "Polygon")
                {
                    var polygon = (Newtonsoft.Json.Linq.JArray)coordinates[0];

                    var path = new List<List<double>>();

                    foreach (var point in polygon)
                    {
                        var lat = (double)point[1];
                        var lon = (double)point[0];

                        path.Add(new List<double>() { lat, lon });
                    }

                    //var area = Area.ComputeSignedArea(path);

                    var fa = new float[1][][];
                    fa[0] = PolyLabel.ConvertPolygonToFloatArray(path);
                    var cn = PolyLabel.GetPolyLabel(fa, 0.00001f);

                    WSMapImage.GetImage(new Job()
                    {
                        Id = cn[0] + "_" + cn[1],
                        Latitude = cn[0],
                        Longitude = cn[1],
                        Callback = imageProcessor,
                    });

                    Console.WriteLine(feature["properties"].ToString() + "(" + feature["id"] + "): " + cn[0] + "," + cn[1]);

                }
                else if (type == "MultiPolygon")
                {
                    foreach (var polygon in coordinates)
                    {
                        var path = new List<List<double>>();

                        foreach (var point in polygon[0])
                        {
                            var lat = (double)point[1];
                            var lon = (double)point[0];

                            path.Add(new List<double>() { lat, lon });
                        }

                        //var area = Area.ComputeSignedArea(path);

                        var fa = new float[1][][];
                        fa[0] = PolyLabel.ConvertPolygonToFloatArray(path);
                        var cn = PolyLabel.GetPolyLabel(fa, 0.00001f);

                        WSMapImage.GetImage(new Job()
                        {
                            Id = cn[0] + "_" + cn[1],
                            Latitude = cn[0],
                            Longitude = cn[1],
                            Callback = imageProcessor,
                        });

                        Console.WriteLine(feature["properties"].ToString() + "(" + feature["id"] + "): " + cn[0] + "," + cn[1]);

                    }
                }
            }   

        }
    }
}
