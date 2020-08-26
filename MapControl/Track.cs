using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class Track : List<GeoPoint>
    {
        public TrackStyle Style { get; set; }
        
        public Track(TrackStyle style)
        {
            Style = style;
        }
    }
}
