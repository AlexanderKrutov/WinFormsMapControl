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
        /// Level of the layer, decides whether a layer is displayed above / below another one
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Enables visibility of an layer
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Creates a layer with specified level
        /// </summary>
        /// <param name="level"></param>
        public Layer(int level)
        {
            Level = level;
            Visible = true;
        }
    }
}
