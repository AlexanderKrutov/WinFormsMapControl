using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class MarkerStyle
    {
        public Brush MarkerBrush { get; set; }
        public float MarkerWidth { get; set; }
        public Brush LabelBrush { get; set; }
        public Font LabelFont { get; set; }

        public MarkerStyle(Brush markerBrush, float markerWidth, Brush labelBrush, Font labelFont) 
        {
            LabelBrush = labelBrush;
            MarkerBrush = markerBrush;
            MarkerWidth = markerWidth;
            LabelFont = labelFont;
        }
    }
}
