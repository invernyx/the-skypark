namespace Orbx.DataManager.Core.Common
{
    /// <summary>
    /// Represents a point on Earth.
    /// </summary>
    public class GeoLocation
    {
        /// <summary>
        /// The latitude of this point.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude of this point.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Initializes a new point with a provided latitude and longitude.
        /// </summary>
        /// <param name="lat">The latitude of this point.</param>
        /// <param name="lon">The longitude of this point.</param>
        public GeoLocation(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        /// <summary>
        /// Creates a new <see cref="GeoLocation"/> based on DWORDs from the BGL file.
        /// </summary>
        /// <param name="latitude">The latitude dword.</param>
        /// <param name="longitude">The longitude dword.</param>
        /// <returns></returns>
        public static GeoLocation FromDwords(uint latitude, uint longitude)
        {
            return new GeoLocation(90.0 - (double)latitude * 3.3527612686157227E-07, (double)longitude * 4.4703483581542969E-07 - 180.0);
        }

        /// <summary>
        /// Gets a string representation of this location.
        /// </summary>
        public override string ToString()
            => $"({Latitude}, {Longitude})";
    }
}
