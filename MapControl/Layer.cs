using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class Layer
    {
        public ITileServer TileServer { get; set; }

        public uint ZIndex { get; set; }

        internal Point Offset = new Point();

        public float Opacity { get; set; } = 1;
    }
}
