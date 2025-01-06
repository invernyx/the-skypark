using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Distance;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TSP_OSM_Loader.Datasets
{
    public class WorldCities
    { 
        public static List<WorldCity> Cities = new List<WorldCity>();
        public static DensityCalculator DC = null;

        public static void Startup()
        {
            Console.WriteLine("Loading Cities");
            var i = 0;
            foreach (var city in Program.ReadResourceFile("TSP_OSM_Loader.Datasets.worldcities.csv").Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (i == 0)
                {
                    i++;
                    continue;
                }

                var cityo = new WorldCity(city);
                //if(cityo.Population > 4000)
                //{
                    Cities.Add(cityo);
                //}
                i++;
            }

            DC = new DensityCalculator(Cities);
            Console.WriteLine("Cities Loaded");
        }

        public static double GetDensity(GeoLoc Loc)
        {
            List<KeyValuePair<double, WorldCity>> Results = new List<KeyValuePair<double, WorldCity>>();

            Parallel.ForEach(Cities, (City) =>
            {
                double dist = Utils.MapCalcDist(City.Lat, City.Lon, (float)Loc.Lat, (float)Loc.Lon, Utils.DistanceUnit.Kilometers);
                if (dist < 200)
                {
                    lock (Results)
                    {
                        double DistFactor = Math.Round(Math.Pow((1 / (dist / 3000)), 3), 5);
                        Results.Add(new KeyValuePair<double, WorldCity>(DistFactor, City));
                    }
                }
            });

            return Results.Sum(x => x.Key);
        }
    }

    public class WorldCity
    {
        public float Lon = 0;
        public float Lat = 0;
        public int Population = 0;

        public WorldCity(string source)
        {
            string pattern = "\t";//",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
            string[] cols = Regex.Split(source, pattern);
            var col_i = 0;
            foreach(var col in cols)
            {
                var dp = col; // col.Substring(1, col.Length - 2);

                switch (col_i)
                {
                    case 4:
                        {
                            Lat = Convert.ToSingle(dp);
                            break;
                        }
                    case 5:
                        {
                            Lon = Convert.ToSingle(dp);
                            break;
                        }
                    case 14:
                        {
                            Population = dp.Length > 0 ? Convert.ToInt32(Convert.ToSingle(dp)) : 0;
                            break;
                        }
                }
                col_i++;
            }
        }
    }


    public class DensityCalculator
    {
        private readonly List<WorldCity> cities;
        private readonly STRtree<WorldCity> spatialIndex;
        private readonly Dictionary<(double, double), double> memo;

        public DensityCalculator(List<WorldCity> cities)
        {
            this.cities = cities;
            spatialIndex = new STRtree<WorldCity>();
            foreach (var city in cities)
            {
                spatialIndex.Insert(new Envelope(city.Lon, city.Lon, city.Lat, city.Lat), city);
            }
            memo = new Dictionary<(double, double), double>();
        }

        public double CalculateDensityAt(double lat, double lon)
        {
            double radiusKm = 70;
            double sigma = 0.5 * radiusKm;

            lock (memo)
            {
                if (memo.TryGetValue((lat, lon), out var density)) { return density; }

                var envelope = new Envelope(lon - radiusKm / 111.0, lon + radiusKm / 111.0, lat - radiusKm / 111.0, lat + radiusKm / 111.0);
                var cities_in_envelope = spatialIndex.Query(envelope);
                var total_population = 0;
                var kernel_sum = 0.0;
                foreach (var city in cities_in_envelope)
                {
                    var distance_km = Distance(city.Lat, city.Lon, lat, lon);
                    if (distance_km <= radiusKm)
                    {
                        double weight = Math.Exp(-distance_km * distance_km / (2 * sigma * sigma)); // Gaussian kernel function
                        kernel_sum += weight;
                        total_population += (int)Math.Round(city.Population * weight);
                    }
                }
                density = total_population; //total_population > 0 ? (total_population / (Math.PI * radiusKm * radiusKm * kernel_sum)) : 0;
                memo[(lat, lon)] = density;
                return density;
            }
        }

        private static double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            double dLat = (lat2 - lat1) * (Math.PI / 180);
            double dLon = (lon2 - lon1) * (Math.PI / 180);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = R * c;
            return d;
        }
    }
}
