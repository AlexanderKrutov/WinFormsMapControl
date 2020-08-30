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
        /// <summary>
        /// Pen to draw marker outline
        /// </summary>
        public Pen MarkerPen { get; set; }
        public Brush MarkerBrush { get; set; }
        public float MarkerWidth { get; set; }
        public Brush LabelBrush { get; set; }
        public Font LabelFont { get; set; }
        public StringFormat LabelFormat { get; set; }

        public MarkerStyle(Pen markerPen, Brush markerBrush, float markerWidth, Brush labelBrush, Font labelFont, StringFormat labelFormat) 
        {
            MarkerPen = markerPen;
            LabelBrush = labelBrush;
            MarkerBrush = markerBrush;
            MarkerWidth = markerWidth;
            LabelFont = labelFont;
            LabelFormat = labelFormat;
        }

        public static MarkerStyle Default = new MarkerStyle(null, Brushes.Red, 3, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
    }
}
