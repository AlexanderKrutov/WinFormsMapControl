using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Used to store tile image in memory 
    /// </summary>
    internal class TileImage
    {
        /// <summary>
        /// X-index of the tile image
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y-index of the tile image
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Zoom level of the tile image
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Tile image
        /// </summary>
        public Image Image { get; set; }

        /// <summary>
        /// Flag indicating image recently used (requested to be drawn on the map).
        /// </summary>
        public bool Used { get; set; }

        /// <summary>
        /// Optional message that should be displayed if tile image is empty.
        /// </summary>
        public string Message { get; set; }
    }
}
