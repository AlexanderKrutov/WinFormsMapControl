using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms.Maps.Layers
{
    /// <summary>
    /// Represents an abstract base class for layers of each type.
    /// </summary>
    public abstract class Layer
    {
        /// <summary>
        /// Backing field for <see cref="Level"/> property.
        /// </summary>
        private int _Level = 0;
        
        /// <summary>
        /// Level of the layer, decides whether a layer is displayed above / below another one
        /// </summary>
        public int Level 
        { 
            get => _Level; 
            set
            {
                _Level = value;
                LayerPropertyChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Backing field for <see cref="Visible"/> property.
        /// </summary>
        private bool _Visible = true;

        /// <summary>
        /// Enables visibility of an layer
        /// </summary>
        public bool Visible 
        { 
            get => _Visible; 
            set
            {
                _Visible = value;
                LayerPropertyChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raised when <see cref="Layer"/> property value is changed.
        /// </summary>
        public event EventHandler<EventArgs> LayerPropertyChanged;

        /// <summary>
        /// Creates a layer with specified level
        /// </summary>
        /// <param name="level"></param>
        public Layer(int level)
        {
            Level = level;
            Visible = true;
        }

        /// <summary>
        /// Raises <see cref="LayerPropertyChanged"/> in base class.
        /// </summary>
        /// <param name="sender">Sender object which raised the event.</param>
        /// <param name="args">Event arguments.</param>
        protected void RaiseLayerPropertyChangedEvent(object sender, EventArgs args)
        {
            LayerPropertyChanged?.Invoke(sender, args);
        }
    }
}
