using System.Windows.Forms.Maps.Elements;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents class for  <see cref="MapControl"/> map element events.
    /// </summary>
    public class MapControlElementEventArgs
    {
        /// <summary>
        /// <see cref="IElement"/> instance which has been clicked.
        /// </summary>
        public IElement Element { get; internal set; }
    }
}
