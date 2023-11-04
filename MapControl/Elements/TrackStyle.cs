using System.Drawing;

namespace System.Windows.Forms.Maps.Elements
{
    /// <summary>
    /// Defines visual style of the <see cref="Track"/>.
    /// </summary>
    public class TrackStyle
    {
        /// <summary>
        /// Pen used to draw track path.
        /// </summary>
        public Pen Pen { get; set; }

        /// <summary>
        /// Indicator for drawing the direction indicator.
        /// </summary>
        public Pen DirectionIndicatorPen { get; set; }

        /// <summary>
        /// Minimum required zoom level vor showing up an direction indicator.
        /// </summary>
        public int DirectionIndicatorMinimumZoomLevel { get; set; }

        /// <summary>
        /// Creates new <see cref="TrackStyle"/>.
        /// </summary>
        public TrackStyle()
        {

        }

        /// <summary>
        /// Creates new <see cref="TrackStyle"/>.
        /// </summary>
        /// <param name="pen"> Pen used to draw track path.</param>
        public TrackStyle(Pen pen) 
        {
            Pen = pen;
            DirectionIndicatorPen = null;
            DirectionIndicatorMinimumZoomLevel = 12;
        }

        /// <summary>
        /// Creates new <see cref="TrackStyle"/> with a direction indicator drawn by the second pen passed.
        /// </summary>
        /// <param name="pen">Pen used to draw track path.</param>
        /// <param name="directionIndicatorPen">Pen used to draw the direction indicator.</param>
        public TrackStyle(Pen pen, Pen directionIndicatorPen)
        {
            Pen = pen;
            DirectionIndicatorPen = directionIndicatorPen;
            DirectionIndicatorMinimumZoomLevel = 12;
        }

        /// <summary>
        /// Creates new <see cref="TrackStyle"/> with a direction indicator drawn by the second pen passed.
        /// </summary>
        /// <param name="pen">Pen used to draw track path.</param>
        /// <param name="directionIndicatorPen">Pen used to draw the direction indicator.</param>
        /// <param name="directionIndicatorMinimumZoomLevel">Minimum zoom level to show up a direction indicator.</param>
        public TrackStyle(Pen pen, Pen directionIndicatorPen, int directionIndicatorMinimumZoomLevel)
        {
            Pen = pen;
            DirectionIndicatorPen = directionIndicatorPen;
            DirectionIndicatorMinimumZoomLevel = directionIndicatorMinimumZoomLevel;
        }

        /// <summary>
        /// Default track style.
        /// </summary>
        public static TrackStyle Default = new TrackStyle(new Pen(Color.Blue) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });
    }
}
