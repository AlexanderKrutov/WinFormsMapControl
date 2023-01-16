using System.Collections.Generic;
using System.Windows.Forms.Maps.Elements;

namespace System.Windows.Forms.Maps.Layers
{
    /// <summary>
    /// Represents a collection of polygons grouped together into a layer.
    /// </summary>
    public class PolygonLayer : Layer
    { 
        /// <summary>
        /// List of polygons
        /// </summary>
        public List<Polygon> Polygons { get; set; } = new List<Polygon>();

        /// <summary>
        /// Creates a polygon layer with specified level
        /// </summary>
        /// <param name="level"></param>
        public PolygonLayer(int level) : base(level)
        {
        }

        public void AddPolygon(Polygon polygon)
        {
            Polygons.Add(polygon);
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }

        public void Clear()
        {
            Polygons.Clear();
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }
    }
}
