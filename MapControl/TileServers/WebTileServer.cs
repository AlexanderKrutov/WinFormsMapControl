using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace System.Windows.Forms
{
    /// <summary>
    /// Base class for all web tile servers
    /// </summary>
    public abstract class WebTileServer : ITileServer
    {
        /// <summary>
        /// Worker thread for downloading images from the server
        /// </summary>
        private Thread _Worker = null;

        /// <summary>
        /// Pool of images to be downloaded
        /// </summary>
        private ConcurrentBag<TileImage> _DowloadPool = new ConcurrentBag<TileImage>();

        /// <summary>
        /// Event handle to stop/resume downloading
        /// </summary>
        private EventWaitHandle _WorkerWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Flag indicating object is disposed
        /// </summary>
        private bool _IsDisposed = false;

        /// <summary>
        /// Last requested zoom level
        /// </summary>
        private int _ZoomLevel;

        /// <summary>
        /// Reference to parent map control
        /// </summary>
        internal IMapControl Map { get; set; }

        /// <summary>
        /// Gets tile URI by X and Y coordinates of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-coordinate of the tile.</param>
        /// <param name="y">Y-coordinate of the tile.</param>
        /// <param name="z">Zoom level</param>
        /// <returns></returns>
        public abstract Uri GetTileUri(int x, int y, int z);

        /// <summary>
        /// User-Agent string used to dowload tile images from the tile server.
        /// </summary>
        /// <remarks>
        /// Some web tile servers (for example OpenStreetMap) require valid HTTP User-Agent identifying application.
        /// Faking app's User-Agent may get you blocked.
        /// </remarks>
        public abstract string UserAgent { get; }

        /// <summary>
        /// Tile expiration period.
        /// </summary>
        /// <remarks>
        /// Different tile servers have various tile usage policies, so do not set small values here to prevent loading same tiles from the server frequently.
        /// For example, for OpenStretMap tile expiration period should not be smaller than 7 days: <see href="https://operations.osmfoundation.org/policies/tiles/"/>
        /// </remarks>
        public virtual TimeSpan TileExpirationPeriod => TimeSpan.FromDays(30);

        /// <summary>
        /// Displayable name of the tile server, i.e. human-readable map name, for example, "Open Street Map".
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Attribution text that will be displayed in bottom-right corner of the map.
        /// Can be null (no attribution text) or can contain html links for navigating with default system web browser.
        /// </summary>
        /// <example>© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors</example>
        public abstract string AttributionText { get; }

        /// <summary>
        /// Gets minimal zoom level allowed for the tile server
        /// </summary>
        public virtual int MinZoomLevel => 0;

        /// <summary>
        /// Gets maximal zoom level allowed for the tile server
        /// </summary>
        public virtual int MaxZoomLevel => 19;

        /// <summary>
        /// Creates new instance of WebTileServer
        /// </summary>
        public WebTileServer()
        {
            ServicePointManager.ServerCertificateValidationCallback = new Net.Security.RemoteCertificateValidationCallback(AcceptAllCertificates);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// Gets tile image by X and Y coordinates of the tile and zoom level Z.
        /// </summary>
        /// <param name="x">X-coordinate of the tile.</param>
        /// <param name="y">Y-coordinate of the tile.</param>
        /// <param name="z">Zoom level</param>
        /// <returns></returns>
        public Image GetTile(int x, int y, int z)
        {
            _ZoomLevel = z;

            // Intialize worker
            if (_Worker == null)
            {
                _Worker = new Thread(new ThreadStart(DownloadImages));
                _Worker.IsBackground = true;
                _Worker.Start();
            }

            // check that image is already in download pool
            if (!_DowloadPool.Any(c => c.X == x && c.Y == y && c.Z == z))
            {
                // add image request to pool
                _DowloadPool.Add(new TileImage() { X = x, Y = y, Z = z });

                // resume worker thread
                _WorkerWaitHandle.Set();
            }

            // return empty image because it's not downloaded yet
            return null;
        }
       
        /// <summary>
        /// Background worker function. 
        /// Downloads images if pool is not empty, than stops the exucution until pool gets new image request.
        /// Breaks execution on disposing.
        /// </summary>
        private void DownloadImages()
        {
            while (!_IsDisposed)
            {
                if (_DowloadPool.TryTake(out TileImage tile))
                {
                    try
                    {
                        // ignore pooled items with zoom level different than current
                        if (tile.Z != _ZoomLevel) continue;

                        // local path to the cached tile image
                        string localPath = Path.Combine(Map?.CacheFolder, GetType().Name, $"{tile.Z}", $"{tile.X}", $"{tile.Y}.png");

                        // if no such file or if it's expired, download it
                        if (!File.Exists(localPath) || new FileInfo(localPath).LastWriteTime + TileExpirationPeriod < DateTime.Now)
                        {
                            Uri uri = GetTileUri(tile.X, tile.Y, tile.Z);

                            // Create directories and subdirectories if needed
                            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                            var request = (HttpWebRequest)WebRequest.Create(uri);
                            request.UserAgent = UserAgent;

                            MemoryStream buffer = new MemoryStream();
                            using (var response = request.GetResponse())
                            using (Stream stream = response.GetResponseStream())
                            {
                                tile.Image = Image.FromStream(stream);
                                tile.Image.Save(localPath);
                                Debug.WriteLine($"Downloaded tile {localPath}");
                            }

                            Map?.OnTileReady(tile);
                        }
                    }
                    catch (Exception ex)
                    {
                        tile.Message = ex.Message;
                        Map?.OnTileReady(tile);

                        //Debug.Fail($"Unable to download tile image. Reason: {ex.Message}");

                        if (ex is WebException wex)
                        {
                            // If no response from the server, wait for a while 
                            // in order to prevent freezes
                            if (wex.Response == null)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
                else
                {
                    _WorkerWaitHandle.WaitOne();
                }
                
            };
        }

        /// <summary>
        /// Function to handle accepting HTTPs certificates 
        /// </summary>
        private bool AcceptAllCertificates(object sender, Security.Cryptography.X509Certificates.X509Certificate certification, Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            _IsDisposed = true;
            _WorkerWaitHandle.Set();
        }
    }
}
