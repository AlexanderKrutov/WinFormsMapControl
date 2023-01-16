using System.Collections.Generic;

namespace System.Windows.Forms.Maps.Layers
{
    /// <summary>
    /// Represents a layer grouping other layers together.
    /// </summary>
    public class LayerGroup : Layer
    {
        /// <summary>
        /// List of layers
        /// </summary>
        public List<Layer> Layers { get; set; } = new List<Layer>();

        /// <summary>
        /// Creates a layer group with level 0
        /// </summary>
        public LayerGroup(): base(0)
        {
        }

        /// <summary>
        /// Creates a layer group with specified level
        /// </summary>
        /// <param name="level"></param>
        public LayerGroup(int level) : base(level)
        {
        }

        /// <summary>
        /// Adds a layer into layer group.
        /// </summary>
        /// <param name="layer">Layer to add into group.</param>
        public void AddLayer(Layer layer)
        {
            layer.LayerPropertyChanged += HandleLayerPropertyChanged;
            
            Layers.Add(layer);
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }

        /// <summary>
        /// Cleas all subsequent layers.
        /// </summary>
        public void Clear()
        {
            Layers.Clear();
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles <see cref="LayerPropertyChanged" /> event from subsequent layers.
        /// </summary>
        /// <param name="sender">The layer which raised the event.</param>
        /// <param name="args">Event arguments.</param>
        private void HandleLayerPropertyChanged(object sender, EventArgs args)
        {
            RaiseLayerPropertyChangedEvent((Layer)sender, args);
        }
    }
}
