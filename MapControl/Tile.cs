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
    public class Tile
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
        /// Tile image
        /// </summary>
        public Image Image { get; }

        /// <summary>
        /// Error message that should be displayed if tile does not exist by some reason (incorrect X/Y indices, zoom level, server unavailable etc.).
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Flag indicating image recently used (requested to be drawn on the map).
        /// </summary>
        internal bool Used { get; set; }

        internal Tile(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Tile(Image image, int x, int y, int z)
        {
            Image = image;
            X = x;
            Y = y;
            Z = z;
        }

        public Tile(string errorMessage, int x, int y, int z)
        {
            ErrorMessage = errorMessage;
            X = x;
            Y = y;
            Z = z;
        }
    }
}
