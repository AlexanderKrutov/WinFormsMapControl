using System.Collections.Concurrent;

namespace System.Windows.Forms
{
    /// <summary>
    /// Internal interface used for referencing <see cref="MapControl"/> instance in <see cref="WebTileServer"/>
    /// </summary>
    internal interface IMapControl
    {
        /// <summary>
        /// Called when tile is ready to be displayed
        /// </summary>
        /// <param name="tile">Tile to be displayed</param>
        void OnTileReady(TileImage tile);

        /// <summary>
        /// Path to tile cache folder
        /// </summary>
        string CacheFolder { get; set; }
    }
}