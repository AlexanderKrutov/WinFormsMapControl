using System.Collections.Generic;
using System.Windows.Forms.Maps.Common;

namespace System.Windows.Forms.Maps.Elements
{
    /// <summary>
    /// Represents track (collection of connected points).
    /// </summary>
    public class Track : List<GeoPoint>, IElement
    {
        /// <summary>
        /// Style to draw the track
        /// </summary>
        public TrackStyle Style { get; set; } = TrackStyle.Default;

        /// <summary>
        /// Custom data associated with the marker
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Creates a track with specified style
        /// </summary>
        /// <param name="style"></param>
        public Track(TrackStyle style)
        {
            Style = style;
        }
    }
}
