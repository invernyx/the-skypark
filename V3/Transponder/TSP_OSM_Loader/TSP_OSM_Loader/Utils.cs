using System;
using System.Collections.Generic;
using System.Text;

namespace TSP_OSM_Loader
{
    public class Utils
    {
        private static Random Rnd = new Random();

        public enum DistanceUnit
        {
            Kilometers,
            Meters,
            Feet,
            NauticalMiles,
            Miles
        }


        public static double GetDirectionInDegrees(double startX, double startY, double endX, double endY)
        {
            double deltaX = endX - startX;
            double deltaY = endY - startY;
            double radians = Math.Atan2(deltaY, deltaX);
            double degrees = radians * (180 / Math.PI);

            // Convert negative angles to positive equivalent
            if (degrees < 0)
            {
                degrees += 360;
            }

            return degrees;
        }

        public static double MapCalcDist(double lat1, double lon1, double lat2, double lon2, DistanceUnit unit = DistanceUnit.Kilometers)
        {
            double dist = 0;
            double rlat1 = 0;
            double rlat2 = 0;
            double theta = 0;
            double rtheta = 0;

            rlat1 = Math.PI * lat1 / 180;
            rlat2 = Math.PI * lat2 / 180;
            theta = lon1 - lon2;
            rtheta = Math.PI * theta / 180;
            dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515f;

            if (double.IsNaN(dist))
            {
                return 0;
            }

            switch (unit)
            {
                case DistanceUnit.Kilometers: //Kilometers -> default
                    return dist * 1.609344f;
                case DistanceUnit.Meters: //Meters
                    return (dist * 1.609344f) * 1000;
                case DistanceUnit.Feet: //Feet
                    return (dist * 5280);
                case DistanceUnit.NauticalMiles: //Nautical Miles 
                    return dist * 0.8684f;
                case DistanceUnit.Miles: //Miles
                    return dist;
            }

            return dist;
        }

        public static double ColorDifferencePercentage(int r1, int g1, int b1, int r2, int g2, int b2)
        {
            // Calculate Euclidean distance between the two RGB values
            double distance = Math.Sqrt(Math.Pow(r2 - r1, 2) + Math.Pow(g2 - g1, 2) + Math.Pow(b2 - b1, 2));

            // Normalize distance by dividing by maximum possible distance
            double maxDistance = Math.Sqrt(Math.Pow(255, 2) * 3); // Maximum distance between two RGB values
            double percentage = distance / maxDistance * 100;

            return percentage;
        }

        public static int GetRandom()
        {
            lock (Rnd)
            {
                return Rnd.Next();
            }
        }

        public static double GetRandomDouble()
        {
            lock (Rnd)
            {
                return Rnd.NextDouble();
            }
        }
        public static int GetRandom(int max)
        {
            lock (Rnd)
            {
                return Rnd.Next(max);
            }
        }

        public static double GetRandomDouble(int max)
        {
            lock (Rnd)
            {
                return Rnd.NextDouble() * max;
            }
        }
    }
}
