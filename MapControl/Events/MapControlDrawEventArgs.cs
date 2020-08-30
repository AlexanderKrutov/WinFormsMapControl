using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents base class for all <see cref="MapControl"/> drawing events.
    /// </summary>
    public abstract class MapControlDrawEventArgs : HandledEventArgs
    {
        /// <summary>
        /// <see cref="System.Drawing.Graphics"/> instance to draw on.
        /// </summary>
        public Graphics Graphics { get; internal set; }
    }
}
