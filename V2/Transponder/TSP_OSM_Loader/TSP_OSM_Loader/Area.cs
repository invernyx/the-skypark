using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace TSP_OSM_Loader
{
    class Area
    {
        const double EARTH_RADIUS = 6371009;

        static double ToRadians(double input)
        {
            return input / 180.0 * Math.PI;
        }

        public static double ComputeSignedArea(List<List<float>> path)
        {
            return ComputeSignedArea(path, EARTH_RADIUS);
        }

        static double ComputeSignedArea(List<List<float>> path, double radius)
        {
            int size = path.Count;
            if (size < 3) { return 0; }
            double total = 0;
            var prev = path[size - 1];
            double prevTanLat = Math.Tan((Math.PI / 2 - ToRadians(prev[1])) / 2);
            double prevLng = ToRadians(prev[0]);

            foreach (var point in path)
            {
                double tanLat = Math.Tan((Math.PI / 2 - ToRadians(point[1])) / 2);
                double lng = ToRadians(point[0]);
                total += PolarTriangleArea(tanLat, lng, prevTanLat, prevLng);
                prevTanLat = tanLat;
                prevLng = lng;
            }
            return Math.Abs(total * (radius * radius));
        }

        static double PolarTriangleArea(double tan1, double lng1, double tan2, double lng2)
        {
            double deltaLng = lng1 - lng2;
            double t = tan1 * tan2;
            return 2 * Math.Atan2(t * Math.Sin(deltaLng), 1 + t * Math.Cos(deltaLng));
        }
    }
}
