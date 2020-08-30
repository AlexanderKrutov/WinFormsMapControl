using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class TrackStyle
    {
        public Pen Pen { get; set; }

        public TrackStyle()
        {

        }

        public TrackStyle(Pen pen) 
        {
            Pen = pen;
        }

        public static TrackStyle Default = new TrackStyle(new Pen(Color.Blue) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash });
    }
}
