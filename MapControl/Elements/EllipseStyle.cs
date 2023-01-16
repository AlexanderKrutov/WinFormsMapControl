using System.Drawing;

namespace System.Windows.Forms.Maps.Elements
{
    /// <summary>
    /// Defines visual style of the <see cref="Ellipse"/>.
    /// </summary>
    public class EllipseStyle
    {
        /// <summary>
        /// Pen to draw ellipse outline.
        /// </summary>
        public Pen EllipsePen { get; set; }

        /// <summary>
        /// Brush to fill ellipse interior.
        /// </summary>
        public Brush EllipseBrush { get; set; }

        /// <summary>
        /// Width of the ellipse circle in specified unit.
        /// </summary>
        public float EllipseWidth { get; set; }

        /// <summary>
        /// Height of the ellipse circle in specified unit.
        /// </summary>
        public float EllipseHeight { get; set; }

        /// <summary>
        /// Unit for width and height. Default is PIXELS.
        /// </summary>
        public Unit EllipseUnit { get; set; } = Unit.PIXELS;

        /// <summary>
        /// Creates new ellipse style.
        /// </summary>        
        public EllipseStyle() : this(Default.EllipseWidth, Default.EllipseHeight, Default.EllipseBrush, Default.EllipsePen, Default.EllipseUnit) { }

        /// <summary>
        /// Creates new ellipse style.
        /// </summary>
        /// <param name="ellipseCircleWidth">Width of the ellipse circle.</param>
        public EllipseStyle(float ellipseCircleWidth) : this(ellipseCircleWidth, ellipseCircleWidth, Default.EllipseBrush, Default.EllipsePen, Default.EllipseUnit) { }

        /// <summary>
        /// Creates new ellipse style.
        /// </summary>
        /// <param name="ellipseWidth">Width of the ellipse circle.</param>
        /// <param name="ellipseHeight">Height of the ellipse circle.</param>
        public EllipseStyle(float ellipseWidth, float ellipseHeight) : this(ellipseWidth, ellipseHeight, Default.EllipseBrush, Default.EllipsePen, Default.EllipseUnit) { }

        /// <summary>
        /// Creates new ellipse style.
        /// </summary>
        /// <param name="ellipseWidth">Width of the ellipse circle.</param>
        /// <param name="ellipseHeight">Height of the ellipse circle.</param>
        /// <param name="ellipseBrush">Brush to fill ellipse interior.</param>
        public EllipseStyle(float ellipseWidth, float ellipseHeight, Brush ellipseBrush) : this(ellipseWidth, ellipseHeight, ellipseBrush, Default.EllipsePen, Default.EllipseUnit) { }

        /// <summary>
        /// Creates new ellipse style.
        /// </summary>
        /// <param name="ellipseWidth">Width of the ellipse circle.</param>
        /// <param name="ellipseHeight">Height of the ellipse circle.</param>
        /// <param name="ellipseBrush">Brush to fill ellipse interior.</param>
        /// <param name="ellipsePen">Pen to draw ellipse outline.</param>
        /// <param name="ellipseUnit">Unit to measure ellipse outline.</param>
        public EllipseStyle(float ellipseWidth, float ellipseHeight, Brush ellipseBrush, Pen ellipsePen, Unit ellipseUnit)
        {
            EllipsePen = ellipsePen;
            EllipseBrush = ellipseBrush;
            EllipseWidth = ellipseWidth;
            EllipseHeight = ellipseHeight;
            EllipseUnit = ellipseUnit;
        }

        /// <summary>
        /// Default ellipse style.
        /// </summary>
        public static EllipseStyle Default = new EllipseStyle(20, 20, Brushes.Red, null, Unit.PIXELS);

        /// <summary>
        /// Unit enumeration defining units for width and height.
        /// </summary>
        public enum Unit
        {
            PIXELS,
            METERS,
            YARDS
        }
    }
}
