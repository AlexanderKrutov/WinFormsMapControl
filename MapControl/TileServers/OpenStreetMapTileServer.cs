using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public class OpenStreetMapTileServer : WebTileServer
    {
        private readonly Random random = new Random();
        private readonly string[] tileServers = new[] { "a", "b", "c" };

        public override string Name => "OpenStreetMap";

        public override string AttributionText => "© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors";

        public override Uri GetTileUri(int x, int y, int z)
        {
            string server = tileServers[random.Next(tileServers.Length)];
            return new Uri($"https://{server}.tile.openstreetmap.org/{z}/{x}/{y}.png");
        }
    }
}
