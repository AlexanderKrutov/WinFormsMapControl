using System.Collections.Generic;
using System.Windows.Forms.Maps.Elements;

namespace System.Windows.Forms.Maps.Layers
{
    /// <summary>
    /// Represents a collection of tracks grouped together into a layer.
    /// </summary>
    public class TrackLayer : Layer
    {
        /// <summary>
        /// List of tracks
        /// </summary>
        public List<Track> Tracks { get; set; } = new List<Track>();

        /// <summary>
        /// Creates a track layer with specified level
        /// </summary>
        /// <param name="level"></param>
        public TrackLayer(int level) : base(level)
        {
        }

        public void AddTrack(Track track)
        {
            Tracks.Add(track);
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }

        public void Clear()
        {
            Tracks.Clear();
            RaiseLayerPropertyChangedEvent(this, EventArgs.Empty);
        }
    }
}
