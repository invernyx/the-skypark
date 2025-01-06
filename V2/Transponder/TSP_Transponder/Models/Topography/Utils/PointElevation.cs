using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_Transponder.Models.Topography.Utils
{
    /// <summary>
    /// World coordinates in Lat Lon and elevation in meters
    /// </summary>
    public class PointElevation
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Elevation { get; set; }

        public double X { get { return Lon; } }
        public double Y { get { return Lat; } }
        public double Z { get { return Elevation; } }

        public PointElevation()
        {
        }

        public PointElevation(double Elevation)
        {
            this.Elevation = Elevation;
        }

        public override string ToString()
        {
            return Elevation.ToString();
        }
    }
}
