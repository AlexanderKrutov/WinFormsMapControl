using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents point on the map
    /// </summary>
    public class Marker
    {
        public GeoPoint Point { get; set; }
        public string Label { get; set; }
        public Brush LabelBrush { get; set; }
        public Brush MarkerBrush { get; set; }
        public float MarkerWidth { get; set; }
        public Font Font { get; set; }

        public Marker(GeoPoint point, string label = null, Brush labelBrush = null, Brush markerBrush = null, Font font = null, float markerWidth = 3) 
        {
            Point = point;
            Label = label;
            LabelBrush = labelBrush;
            MarkerBrush = markerBrush;
            MarkerWidth = markerWidth;
            Font = font;
        }
    }
}
