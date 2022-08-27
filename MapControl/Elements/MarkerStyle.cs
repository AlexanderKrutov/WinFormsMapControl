using System.Drawing;

namespace System.Windows.Forms.Maps.Elements
{
    /// <summary>
    /// Defines visual style of the <see cref="Marker"/>.
    /// </summary>
    public class MarkerStyle
    {
        /// <summary>
        /// Pen to draw marker outline.
        /// </summary>
        public Pen MarkerPen { get; set; }

        /// <summary>
        /// Brush to fill marker interior.
        /// </summary>
        public Brush MarkerBrush { get; set; }

        /// <summary>
        /// Image to draw the marker alternatively.
        /// </summary>
        public Image MarkerImage { get; set; }

        /// <summary>
        /// Width of the marker circle, in pixels.
        /// </summary>
        public float MarkerWidth { get; set; }
        
        /// <summary>
        /// Brush to draw marker label.
        /// </summary>
        public Brush LabelBrush { get; set; }

        /// <summary>
        /// Font used to draw marker label.
        /// </summary>
        public Font LabelFont { get; set; }

        /// <summary>
        /// String format used to draw marker label.
        /// </summary>
        public StringFormat LabelFormat { get; set; }

        /// <summary>
        /// Creates new marker style.
        /// </summary>        
        public MarkerStyle() : this(Default.MarkerImage, Default.MarkerWidth, Default.MarkerBrush, Default.MarkerPen, Default.LabelBrush, Default.LabelFont, Default.LabelFormat) { }

        /// <summary>
        /// Creates new marker style.
        /// </summary>
        /// <param name="markerWidth">Width of the marker circle, in pixels.</param>
        public MarkerStyle(float markerWidth) : this(Default.MarkerImage, markerWidth, Default.MarkerBrush, Default.MarkerPen, Default.LabelBrush, Default.LabelFont, Default.LabelFormat) { }

        /// <summary>
        /// Creates new marker style.
        /// </summary>
        /// <param name="markerWidth">Width of the marker circle, in pixels.</param>
        /// /// <param name="markerBrush">Brush to fill marker interior.</param>
        public MarkerStyle(float markerWidth, Brush markerBrush) : this(Default.MarkerImage, markerWidth, markerBrush, Default.MarkerPen, Default.LabelBrush, Default.LabelFont, Default.LabelFormat) { }

        /// <summary>
        /// Creates a new marker style from image.
        /// </summary>
        /// <param name="markerImage">Image to be drawn as marker.</param>
        public MarkerStyle(Image markerImage) : this(markerImage, markerImage.Width, Default.MarkerBrush, Default.MarkerPen, Default.LabelBrush, Default.LabelFont, Default.LabelFormat) { }

        /// <summary>
        /// Creates new marker style.
        /// </summary>
        /// <param name="markerWidth">Width of the marker circle, in pixels.</param>
        /// <param name="markerBrush">Brush to fill marker interior.</param>
        /// <param name="markerPen">Pen to draw marker outline.</param>
        /// <param name="labelBrush">Brush to draw marker label.</param>
        /// <param name="labelFont">Font used to draw marker label.</param>
        /// <param name="labelFormat">String format used to draw marker label.</param>
        public MarkerStyle(Image markerImage, float markerWidth, Brush markerBrush, Pen markerPen, Brush labelBrush, Font labelFont, StringFormat labelFormat)
        {
            MarkerImage = markerImage;
            MarkerPen = markerPen;
            LabelBrush = labelBrush;
            MarkerBrush = markerBrush;
            MarkerWidth = markerWidth;
            LabelFont = labelFont;
            LabelFormat = labelFormat;
        }

        /// <summary>
        /// Default marker style.
        /// </summary>
        public static MarkerStyle Default = new MarkerStyle(null, 3, Brushes.Red, null, Brushes.Black, SystemFonts.DefaultFont, StringFormat.GenericDefault);
    }
}
