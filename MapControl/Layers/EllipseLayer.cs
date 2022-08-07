using System.Collections.Generic;
using System.Windows.Forms.Maps.Elements;

namespace System.Windows.Forms.Maps.Layers
{
    /// <summary>
    /// Represents a collection of ellipses grouped together into a layer.
    /// </summary>
    public class EllipseLayer : Layer
    {
        /// <summary>
        /// List of ellipses
        /// </summary>
        public List<Ellipse> Ellipses { get; set; } = new List<Ellipse>();
        
        /// <summary>
        /// Creates an ellipse layer with specified level
        /// </summary>
        /// <param name="level"></param>
        public EllipseLayer(int level) : base(level)
        {
        }

        public void AddEllipse(Ellipse ellipse)
        {
            Ellipses.Add(ellipse);
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }

        public void Clear()
        {
            Ellipses.Clear();
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }
    }
}
