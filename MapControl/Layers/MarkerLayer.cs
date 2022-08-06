using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms.Maps.Layers
{
    /// <summary>
    /// Represents a collection of markers grouped together into a layer.
    /// </summary>
    public class MarkerLayer : Layer
    {
        /// <summary>
        /// List of markers
        /// </summary>
        public List<Marker> Markers { get; set; } = new List<Marker>();

        /// <summary>
        /// Creates a marker layer with specified level
        /// </summary>
        /// <param name="level"></param>
        public MarkerLayer(int level): base(level)
        {
        }
    }
}
