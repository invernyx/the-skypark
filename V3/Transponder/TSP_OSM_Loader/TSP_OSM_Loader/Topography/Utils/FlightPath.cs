using GeographicLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_OSM_Loader.Topography.Utils
{
    /// <summary>
    /// FlightPath has methods for working with Geodesic paths.
    /// </summary>
    /// <remarks>
    /// See https://en.wikipedia.org/wiki/Geodesic
    /// </remarks>
    public class FlightPath
    {
        private Geodesic geod;

        public FlightPath()
        {
            geod = new GeographicLib.Geodesic(Constants.WGS84_a, Constants.WGS84_f);
        }

        /// <summary>
        /// GetPath returns points along a geodesic line between two points.
        /// </summary>
        /// <param name="startPoint"><see cref="PointElevation"/> containg start location</param>
        /// <param name="endPoint"><see cref="PointElevation"/> containg end location</param>
        /// <param name="numOfSamples">How many samples will be returned between start and end.</param>
        /// <returns></returns>
        public IEnumerable<PointElevation> GetPath(PointElevation startPoint, PointElevation endPoint, int numOfSamples)
        {
            double s12, azi1, azi2;
            var geodInverse = geod.Inverse(startPoint.Lat, startPoint.Lon, endPoint.Lat, endPoint.Lon);
            s12 = geodInverse.s12;
            azi1 = geodInverse.azi1;
            azi2 = geodInverse.azi2;

            var line = new GeodesicLine(geod, startPoint.Lat, startPoint.Lon, azi1);

            var pos = line.Position(0);
            yield return new PointElevation { Lat = pos.lat2, Lon = pos.lon2 };

            //Iterate the points in between
            var deltaPos = s12 / numOfSamples;
            for (var t = 2; t < numOfSamples; ++t)
            {
                pos = line.Position(deltaPos * t);
                yield return new PointElevation { Lat = pos.lat2, Lon = pos.lon2 };
            }

            pos = line.Position(s12);
            yield return new PointElevation { Lat = pos.lat2, Lon = pos.lon2 };
        }
    }
}
