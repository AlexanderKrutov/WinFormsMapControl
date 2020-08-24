using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    internal class CachedImage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public Image Image { get; set; }
        public bool Used { get; set; }
    }
}
