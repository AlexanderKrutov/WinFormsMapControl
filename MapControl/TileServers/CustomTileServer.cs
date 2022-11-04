using System.Text;

namespace System.Windows.Forms
{
    /// <summary>
    /// Represents a custom wrapper for custom tile servers.
    /// </summary>
    public class CustomTileServer : WebTileServer
    {

        /// <summary>
        /// Gets displayable name of the Tile server.
        /// </summary>
        public override string Name => "CustomTileServer";

        /// <summary>
        /// Gets attribution text.
        /// </summary>
        public override string AttributionText => "© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> Contributors";

        /// <summary>
        /// User-Agent string used to dowload tile images from the tile server.
        /// </summary>
        /// <remarks>
        /// OpenStreetMap requires valid HTTP User-Agent identifying application.
        /// Faking app's User-Agent may get you blocked.
        /// </remarks>
        public override string UserAgent { get; set; }

        /// <summary>
        /// Represents the server address to load tiles from with placeholder for z, x and y.
        /// </summary>
        /// <remarks>
        /// Server address needs to be formatted in http[s]://[IP-Address]/.../{z}/{x}/{y}.[Format]
        /// </remarks>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Gets tile URI by X and Y indices of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-index of the tile.</param>
        /// <param name="y">Y-index of the tile.</param>
        /// <param name="z">Zoom level.</param>
        /// <returns><see cref="Uri"/> instance.</returns>
        public override Uri GetTileUri(int x, int y, int z)
        {
            string serverAddress = this.ServerAddress;

            serverAddress = serverAddress.Replace("{z}", Convert.ToString(z));
            serverAddress = serverAddress.Replace("{x}", Convert.ToString(x));
            serverAddress = serverAddress.Replace("{y}", Convert.ToString(y));

            return new Uri(serverAddress);
        }

        /// <summary>
        /// Constructs a new instance of custom tile server.
        /// </summary>
        /// <param name="serverAddress">The server's address where to load tiles from in ZXY format</param>
        public CustomTileServer(string serverAddress)
        {
            this.ServerAddress = serverAddress;
        }

        /// <summary>
        /// Constructs a new instance of custom tile server.
        /// </summary>
        /// <param name="serverAddress">The server's address where to load tiles from in ZXY format</param>
        public CustomTileServer(string serverAddress, string username, string password)
        {
            this.ServerAddress = serverAddress;

            string authorizationString = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            this.Authorization = string.Format("Basic {0}", authorizationString);
        }
    }
}
