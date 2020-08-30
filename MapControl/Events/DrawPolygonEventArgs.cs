using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    /// <summary>
    /// Provides data for <see cref="MapControl.DrawPolygon"/> event.
    /// </summary>
    public class DrawPolygonEventArgs : MapControlDrawEventArgs
    {
        /// <summary>
        /// <see cref="System.Windows.Forms.Polygon"/> instance to be drawn.
        /// </summary>
        public Polygon Polygon { get; internal set; }

        /// <summary>
        /// <see cref="GraphicsPath"/> instance describing polygon interior. 
        /// </summary>
        public GraphicsPath Path { get; internal set; }

        /// <summary>
        /// Creates new instance of <see cref="DrawPolygonEventArgs"/>.
        /// </summary>
        internal DrawPolygonEventArgs() { }
    }
}
