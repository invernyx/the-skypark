using Newtonsoft.Json;
using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSP_OSM_Loader.Topography.Utils;

namespace TSP_OSM_Loader.Topography
{
    /// <summary>
    /// MapData will create a metafile for the datasets in the given data folder.
    /// </summary>
    public class MapData
    {
        string dataFolder = string.Empty;

        public MapData(string dataFolder)
        {
            this.dataFolder = dataFolder;
        }

        /// <summary>
        /// Build a file containing information about all the the tiles in the dataset.
        /// </summary>
        /// <remarks>
        /// A file called 'meta.json' will be created in the data folder.
        /// The file contains a list of TileInfo for each tile.
        /// </remarks>
        public void BuildMetaFile()
        {
            var tiles = new List<TileInfo>();

            foreach (var file in Directory.GetFiles(dataFolder, "*.tif"))
            {
                var tileInfo = GetFileCoverage(file);
                tileInfo.FileName = Path.GetFileName(file);

                tiles.Add(tileInfo);
            }

            //Serialize tiles to disk
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var str = JsonConvert.SerializeObject(tiles.ToArray(), jsonSerializerSettings);
            File.WriteAllText(Path.Combine(dataFolder, "meta.json"), str);
        }

        /// <summary>
        /// Get coverage from a tif tile. Assuming that north is always up.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public TileInfo GetFileCoverage(string path)
        {
            var tileInfo = new TileInfo();

            //geoTransformArr[0] /* top left x */
            //geoTransformArr[1] /* w-e pixel resolution */
            //geoTransformArr[2] /* rotation, 0 if image is "north up" */
            //geoTransformArr[3] /* top left y */
            //geoTransformArr[4] /* rotation, 0 if image is "north up" */
            //geoTransformArr[5] /* n-s pixel resolution */

            using (var ds = Gdal.Open(path, Access.GA_ReadOnly))
            {
                var geoTransformArr = new double[6];
                ds.GetGeoTransform(geoTransformArr);

                tileInfo.X = geoTransformArr[0];
                tileInfo.Y = geoTransformArr[3];
                tileInfo.WestEastPixelResolution = geoTransformArr[1];
                tileInfo.NorthSoutPixelResolution = geoTransformArr[5];

                //Get driver
                var drv = ds.GetDriver();
                if (drv == null)
                {
                    Console.WriteLine("Can't get driver.");
                    System.Environment.Exit(-1);
                }

                // Data has only one band
                using (var band = ds.GetRasterBand(1))
                {
                    tileInfo.Width = band.XSize;
                    tileInfo.Height = band.YSize;
                }
            }

            return tileInfo;
        }
    }
}
