using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo.GDAL;

namespace TSP_OSM_Loader.Topography.Utils
{
    /// <summary>
    /// TileDataSet contains methods for working with the whole dataset.
    /// </summary>
    public class TileDataSet : IDisposable
    {
        private string dataPath = string.Empty;
        private List<TileInfo> tileInfos = new List<TileInfo>();
        private Dictionary<TileInfo, Band> bands = new Dictionary<TileInfo, Band>();

        public TileDataSet(string metadata)
        {
            if (File.Exists(metadata))
            {
                dataPath = Path.GetDirectoryName(metadata);
                var deserialized = (IEnumerable<TileInfo>)JsonConvert.DeserializeObject(File.ReadAllText(metadata), typeof(TileInfo[]));
                tileInfos.AddRange(deserialized);
            }
        }

        /// <summary>
        /// For each point, find and set the elevation
        /// </summary>
        /// <param name="points"></param>
        public IEnumerable<PointElevation> GetElevationForPoints(IEnumerable<PointElevation> points)
        {
            foreach (var point in points)
            {
                var tileInfoForPoint = GetTileForCoordinate(point);

                // Make sure the band is loaded from the relevant tif file
                if (!bands.ContainsKey(tileInfoForPoint))
                {
                    bands.Add(tileInfoForPoint, LoadBand(tileInfoForPoint));
                }
                
                // Get the elevation in meters from the band
                point.Elevation = GetElevation(tileInfoForPoint, bands[tileInfoForPoint], point);

                yield return point;
            }
        }

        public PointElevation GetElevationForPoint(PointElevation point)
        {
            var tileInfoForPoint = GetTileForCoordinate(point);

            // Make sure the band is loaded from the relevant tif file
            if (!bands.ContainsKey(tileInfoForPoint))
            {
                bands.Add(tileInfoForPoint, LoadBand(tileInfoForPoint));
            }

            // Get the elevation in meters from the band
            point.Elevation = GetElevation(tileInfoForPoint, bands[tileInfoForPoint], point);

            return point;
        }

        public double GetElevation(TileInfo tileInfo, Band band, PointElevation point)
        {
            double[] buffer = new double[1];

            //var x = (tileInfo.X + point.X) / tileInfo.WestEastPixelResolution;
            var x = (point.X - tileInfo.X) / tileInfo.WestEastPixelResolution;
            var y = (tileInfo.Y - point.Y) / tileInfo.NorthSoutPixelResolution * -1;
            band.ReadRaster((int)x, (int)y, 1, 1, buffer, 1, 1, 0, 0);

            return buffer[0];
        }

        private TileInfo GetTileForCoordinate(PointElevation point)
        {
            return tileInfos.Where(t => t.Contains(point)).FirstOrDefault();
        }

        /// <summary>
        /// Load the one and only Band. Code need to be changed if the 
        /// tif files has more bands.
        /// </summary>
        /// <param name="tileInfo"></param>
        /// <returns></returns>
        private Band LoadBand(TileInfo tileInfo)
        {
            var ds = Gdal.Open(Path.Combine(dataPath, tileInfo.FileName), Access.GA_ReadOnly);

            var drv = ds.GetDriver();
            if (drv == null)
            {
                Console.WriteLine("Can't get driver.");
                return null;
            }

            return ds.GetRasterBand(1);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var band in bands.Values)
                        band.Dispose();
                }

                disposedValue = true;
            }
        }
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
