using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public struct GeoPoint
    {
        public float Longitude { get; set; }
        public float Latitude { get; set; }

        public GeoPoint(float longitude, float latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
}
