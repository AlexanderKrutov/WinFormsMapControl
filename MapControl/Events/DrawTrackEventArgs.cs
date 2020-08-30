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
    /// Provides data for <see cref="MapControl.DrawTrack"/> event.
    /// </summary>
    public class DrawTrackEventArgs : MapControlDrawEventArgs
    {
        /// <summary>
        /// <see cref="System.Windows.Forms.Track"/> instance to be drawn.
        /// </summary>
        public Track Track { get; internal set; }

        /// <summary>
        /// Array of points defining track polyline.
        /// </summary>
        public PointF[] Points { get; internal set; }

        /// <summary>
        /// Creates new instance of <see cref="DrawTrackEventArgs"/>.
        /// </summary>
        internal DrawTrackEventArgs() { }
    }
}
