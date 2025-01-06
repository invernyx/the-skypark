using System;
using System.Collections.Generic;
using System.Text;

namespace TSP_OSM_Loader
{
    public class GeoLoc
    {
        public double Lon = 0;
        public double Lat = 0;

        public GeoLoc(double lon, double lat)
        {
            this.Lon = lon;
            this.Lat = lat;
        }

        public override string ToString()
        {
            return Lon + ", " + Lat;
        }

        public string ToString(int decimals)
        {
            return Math.Round(Lon, decimals) + ", " + Math.Round(Lat, decimals);
        }

        public List<double> ToList(int lecimals = 15)
        {
            return new List<double>() { Math.Round(Lon, lecimals), Math.Round(Lat, lecimals) };
        }

        public GeoLoc Copy()
        {
            GeoLoc copy = (GeoLoc)this.MemberwiseClone();
            return copy;
        }
    }
}
