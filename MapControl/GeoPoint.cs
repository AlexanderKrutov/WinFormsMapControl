using System.Globalization;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents point on the Earth surface with geographical coordinates
    /// </summary>
    public struct GeoPoint
    {
        /// <summary>
        /// Longitude of the point, in degrees, from 0 to ±180, positive East, negative West. 0 is a point on prime meridian.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Latitude of the point, in degrees, from +90 (North pole) to -90 (South Pole). 0 is a point on equator.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="GeoPoint"/> and initializes it with longitude and latitude values.
        /// </summary>
        /// <param name="longitude">Longitude of the point, in degrees, from 0 to ±180, positive East, negative West. 0 is a point on prime meridian.</param>
        /// <param name="latitude">Latitude of the point, in degrees, from +90 (North pole) to -90 (South Pole). 0 is a point on equator.</param>
        public GeoPoint(float longitude, float latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{DegreeToString(Longitude, "W", "E")}, {DegreeToString(Latitude, "S", "N")}";
        }

        private static string DegreeToString(double coordinate, string negativeSym, string positiveSym)
        {
            string sym = coordinate < 0d ? negativeSym : positiveSym;
            coordinate = Math.Abs(coordinate);
            double d = Math.Floor(coordinate);
            coordinate -= d;
            coordinate *= 60;
            double m = Math.Floor(coordinate);
            coordinate -= m;
            coordinate *= 60;
            double s = coordinate;
            string dd = d.ToString();
            string mm = m.ToString().PadLeft(2, '0');
            string ss = s.ToString("00.00", CultureInfo.InvariantCulture);
            return $"{dd}° {mm}' {ss}\" {sym}";
        }
    }
}
