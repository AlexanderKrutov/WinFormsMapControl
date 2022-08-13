using System.Collections.Generic;
using System.Windows.Forms.Maps.Common;

namespace System.Windows.Forms.Maps.Elements
{
    /// <summary>
    ///  Represents filled closed area on the map.
    /// </summary>
    public class Polygon : List<GeoPoint>, IElement
    {
        /// <summary>
        /// Gets or sets polygon style.
        /// </summary>
        public PolygonStyle Style { get; set; } = PolygonStyle.Default;


        /// <summary>
        /// Creates new polygon without assigned style
        /// </summary>
        public Polygon()
        {

        }

        /// <summary>
        /// Creates new polygon with specified style.
        /// </summary>
        /// <param name="style"></param>
        public Polygon(PolygonStyle style)
        {
            Style = style;
        }
    }
}
