using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class OpenTopoMapServer : WebTileServer
    {
        private Random _Random = new Random();
        private string[] _TileServers = new[] { "a", "b", "c" };

        public override string Name => "OpenTopoMap";

        public override string AttributionText => "Map data: © <a href='https://openstreetmap.org/copyright'>OpenStreetMap</a> contributors, <a href='http://viewfinderpanoramas.org'>SRTM</a> | map style: © <a href='https://opentopomap.org'>OpenTopoMap</a> (<a href='https://creativecommons.org/licenses/by-sa/3.0/'>CC-BY-SA</a>)";

        public override int MinZoomLevel => 1;

        public override int MaxZoomLevel => 17;

        public override string UserAgent => null;

        public override Uri GetTileUri(int x, int y, int z)
        {
            string server = _TileServers[_Random.Next(_TileServers.Length)];
            return new Uri($"https://{server}.tile.opentopomap.org/{z}/{x}/{y}.png");
        }
    }
}
