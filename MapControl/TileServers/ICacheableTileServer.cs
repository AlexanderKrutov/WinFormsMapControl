using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Provides the functionality of a Tile Server implementations
    /// </summary>
    public interface ICacheableTileServer : ITileServer
    {
        /// <summary>
        /// Gets tile validity period. 
        /// Tile will be requested again from the tile server 
        /// if tile's image file from the file system cache older than that value. 
        /// </summary>
        TimeSpan TileExpirationPeriod { get; }
    }
}
