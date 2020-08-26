using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class Polygon : List<GeoPoint>
    {
        public PolygonStyle Style { get; set; }

        public Polygon()
        {

        }

        public Polygon(PolygonStyle style)
        {
            Style = style;
        }
    }
}
