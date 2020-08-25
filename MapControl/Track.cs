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
        public Pen Pen { get; set; }
    
        public Track(Pen pen)
        {
            Pen = pen;
        }
    }
}
