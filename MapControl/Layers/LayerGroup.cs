using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
