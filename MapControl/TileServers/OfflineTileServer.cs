using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Tile server for offline maps.
    /// Uses tiles from <see href="https://www.maptiler.com/copyright/">MapTiler</see> embedded.
    /// </summary>
    public class OfflineTileServer : ITileServer
    {
        /// <summary>
        /// Displayable name of the tile server
        /// </summary>
        public string Name => "Offline map";

        /// <summary>
        /// Attribution text that will be displayed in bottom-right corner of the map.
        /// </summary>
        public string AttributionText => "© <a href='https://www.maptiler.com/copyright/'>MapTiler</a> © <a href='https://www.openstreetmap.org/copyright\'>OpenStreetMap</a> contributors";

        /// <summary>
        /// Gets minimal zoom level allowed for the tile server
        /// </summary>
        public int MinZoomLevel => 0;

        /// <summary>
        /// Gets maximal zoom level allowed for the tile server
        /// </summary>
        public int MaxZoomLevel => 5;

        /// <summary>
        /// Gets tile image by X and Y coordinates of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-coordinate of the tile.</param>
        /// <param name="y">Y-coordinate of the tile.</param>
        /// <param name="z">Zoom level</param>
        /// <returns></returns>
        public Image GetTile(int x, int y, int z)
        {
            Stream stream = Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream($"MapControl.OfflineMaps._{z}._{x}.{(1 << z) - y - 1}.jpg");
            if (stream != null)
            {
                return new Bitmap(stream);
            }
            else
            {
                throw new Exception("Tile image does not exist.");
            }
        }
    }
}
