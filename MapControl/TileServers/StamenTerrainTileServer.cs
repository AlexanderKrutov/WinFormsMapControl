using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class StamenTerrainTileServer : WebTileServer
    {
        private Random _Random = new Random();
        private string[] _TileServers = new[] { "a", "b", "c", "d" };

        public override string Name => "Stamen Terrain";

        public override string AttributionText => "<a href='http://maps.stamen.com/'>Map tiles</a> by <a href='http://stamen.com'>Stamen Design</a>, under <a href='http://creativecommons.org/licenses/by/3.0'>CC BY 3.0</a>. Data © <a href='http://www.openstreetmap.org/copyright'>OpenStreetMap contributors</a>.";

        public override int MaxZoomLevel => 17;

        public override string UserAgent => null;

        public override Uri GetTileUri(int x, int y, int z)
        {
            string server = _TileServers[_Random.Next(_TileServers.Length)];
            return new Uri($"http://{server}.tile.stamen.com/terrain/{z}/{x}/{y}.png");
        }
    }
}
