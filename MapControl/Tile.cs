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
    internal class Tile
    {
        /// <summary>
        /// X-index of the tile image
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y-index of the tile image
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Zoom level of the tile image
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Tile server name
        /// </summary>
        public string TileServer { get; }

        /// <summary>
        /// Tile image
        /// </summary>
        public Image Image { get; set; }

        /// <summary>
        /// Error message that should be displayed if tile does not exist by some reason (incorrect X/Y indices, zoom level, server unavailable etc.).
        /// </summary>
        public string ErrorMessage { get; set;  }

        /// <summary>
        /// Flag indicating image recently used (requested to be drawn on the map).
        /// </summary>
        public bool Used { get; set; }

        public Tile(int x, int y, int z, string tileServer)
        {
            X = x;
            Y = y;
            Z = z;
            TileServer = tileServer;
        }

        public Tile(Image image, int x, int y, int z, string tileServer)
        {
            Image = image;
            X = x;
            Y = y;
            Z = z;
            TileServer = tileServer;
        }
    }
}
