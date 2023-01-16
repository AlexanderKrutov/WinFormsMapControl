using System.Windows.Forms.Maps.Elements;
using System.Windows.Forms.Maps.Layers;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents class for  <see cref="MapControl"/> map element events.
    /// </summary>
    public class MapControlElementEventArgs
    {
        /// <summary>
        /// Cursor X position.
        /// </summary>
        public int X { get; internal set; }

        /// <summary>
        /// Cursor Y position.
        /// </summary>
        public int Y { get; internal set; }
        
        /// <summary>
        /// <see cref="Layer" /> insance containing the element.
        /// </summary>
        public Layer Layer { get; internal set; }
        
        /// <summary>
        /// <see cref="IElement"/> instance which has been clicked.
        /// </summary>
        public IElement Element { get; internal set; }
    }
}
