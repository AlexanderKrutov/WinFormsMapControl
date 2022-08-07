﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}