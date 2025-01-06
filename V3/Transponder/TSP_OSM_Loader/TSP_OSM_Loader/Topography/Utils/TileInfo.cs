using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_OSM_Loader.Topography.Utils
{
    /// <summary>
    /// Information about one tile
    /// </summary>
    public class TileInfo
    {
        public string FileName { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public double WestEastPixelResolution { get; set; }
        public double NorthSoutPixelResolution { get; set; }

        [JsonIgnore]
        public double LastX
        {
            get
            {
                return X + (WestEastPixelResolution * Width);
            }
        }

        [JsonIgnore]
        public double LastY
        {
            get
            {
                return Y + (NorthSoutPixelResolution * Height);
            }
        }

        public bool Contains(PointElevation point)
        {
            //return (point.X >= X && point.X < LastX) && (point.Y >= Y && point.Y < LastY);

            var insideX = point.X >= X && point.X < LastX;
            var insideY = point.Y <= Y && point.Y > LastY;

            return insideX && insideY;
        }

        public override string ToString()
        {
            return $"({X},{Y}) ({LastX}, {LastY})";
        }
    }
}
