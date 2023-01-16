using System.Drawing;
using System.Windows.Forms.Maps.Elements;

namespace System.Windows.Forms
{
    /// <summary>
    /// Provides data for <see cref="MapControl.DrawEllipse"/> event.
    /// </summary>
    public class DrawEllipseEventArgs : MapControlDrawEventArgs
    {
        /// <summary>
        /// <see cref="System.Windows.Forms.Ellipse"/> instance to be drawn.
        /// </summary>
        public Ellipse Ellipse { get; internal set; }

        /// <summary>
        /// Coordinates of the marker on the map.
        /// </summary>
        public PointF Point { get; internal set; }

        /// <summary>
        /// Creates new instance of <see cref="DrawEllipseEventArgs"/>.
        /// </summary>
        internal DrawEllipseEventArgs() { }
    }
}
