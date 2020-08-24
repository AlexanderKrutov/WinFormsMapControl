using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class OfflineTileServer : ITileServer
    {
        public event Action InvalidateRequired;

        public string Name => "Offline map";

        public string AttributionText => "© <a href='https://www.maptiler.com/copyright/'>MapTiler</a> © <a href='https://www.openstreetmap.org/copyright\'>OpenStreetMap</a> contributors";

        public int MinZoomLevel => 0;

        public int MaxZoomLevel => 5;

        public void Dispose()
        {

        }

        public Image GetTile(int x, int y, int z)
        {
            return new Bitmap(Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream($"MapControl.OfflineMaps._{z}._{x}.{(1 << z) - y - 1}.jpg"));
        }
    }
}
