using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using static WindowsFormsApp1.MapControl;

namespace WindowsFormsApp1
{
    public interface ITileServer : IDisposable
    {
        string CacheFolder { get; }
        void RequestImage(int x, int y, int z);
        event Action InvalidateRequired;
        void SetZoomLevel(int z);
        string Name { get; }
        string CopyrightLink { get; }
    }

    public abstract class WebTileServer : ITileServer
    {
        protected ConcurrentBag<CachedImage> _DowloadPool = new ConcurrentBag<CachedImage>();

        private Thread _Worker = null;

        private bool _IsDisposed = false;

        private EventWaitHandle _WorkerWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        public string CacheFolder { get; protected set; }

        protected abstract Uri GetTileUri(int x, int y, int z);

        public abstract string Name { get; }

        public abstract string CopyrightLink { get; }

        protected int _ZoomLevel;

        public event Action InvalidateRequired;

        public WebTileServer()
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertificates);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private bool AcceptAllCertificates(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void RequestImage(int x, int y, int z)
        {
            // Intialize worker
            if (_Worker == null)
            {
                _Worker = new Thread(new ThreadStart(DownloadImages));
                _Worker.Start();
            }

            // check that image is already in download pool
            if (!_DowloadPool.Any(c => c.X == x && c.Y == y && c.Z == z))
            {
                // add image request to pool
                _DowloadPool.Add(new CachedImage() { X = x, Y = y, Z = z });

                // resume worker thread
                _WorkerWaitHandle.Set();
            }
        }

        private void DownloadImages()
        {
            while (!_IsDisposed)
            {
                try
                {
                    if (_DowloadPool.TryTake(out CachedImage cached))
                    {
                        // ignore pooled items with zoom level different than current
                        if (cached.Z != _ZoomLevel) continue;

                        string localDir = Path.Combine(CacheFolder, $"{cached.Z}", $"{cached.X}", $"{cached.Y}.png");

                        Uri uri = GetTileUri(cached.X, cached.Y, cached.Z);

                        Directory.CreateDirectory(Path.GetDirectoryName(localDir));

                        // First download the image to our memory.
                        var request = (HttpWebRequest)WebRequest.Create(uri);
                        request.UserAgent = "MapControl 1.0 contact mapcontrol@mapcontrol.io";

                        MemoryStream buffer = new MemoryStream();
                        using (var response = request.GetResponse())
                        {
                            Stream stream = response.GetResponseStream();
                            Image image = Image.FromStream(stream);

                            try
                            {
                                image.Save(localDir);
                            }
                            catch { }
                            stream.Close();
                        }

                        InvalidateRequired?.Invoke();
                    }
                    else
                    {
                        _WorkerWaitHandle.WaitOne();
                    }
                }
                catch (WebException we)
                {

                }
                catch (NotSupportedException nse) // Problem creating the bitmap (messed up download?)
                {

                }
                finally
                {
                    //Thread.Sleep(1000);
                }
            };
        }

        public void Dispose()
        {
            _IsDisposed = true;
            _WorkerWaitHandle.Set();
        }

        public void SetZoomLevel(int z)
        {
            _ZoomLevel = z;
        }
    }

    public class EmbeddedTileServer : ITileServer
    {
        public string CacheFolder => null;

        public event Action InvalidateRequired;

        public string Name => "Offline map";

        public string CopyrightLink => null;

        public void Dispose()
        {

        }

        public void RequestImage(int x, int y, int z)
        {

        }

        public void SetZoomLevel(int z)
        {

        }
    }

    public class OpenStreetMapTileServer : WebTileServer
    {
        private Random _Random = new Random();
        private string[] _TileServers = new[] { "a", "b", "c" };

        public override string Name => "OpenStreetMap";

        public override string CopyrightLink => "© <a href='https://www.openstreetmap.org/copyright'>OpenStreetMap</a> contributors.";

        public OpenStreetMapTileServer()
        {
            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl", "OpenStreetMap");
        }

        protected override Uri GetTileUri(int x, int y, int z)
        {
            string server = _TileServers[_Random.Next(_TileServers.Length)];
            return new Uri($"https://{server}.tile.openstreetmap.org/{z}/{x}/{y}.png");
        }
    }

    public class OpenTopoMapServer : WebTileServer
    {
        private Random _Random = new Random();
        private string[] _TileServers = new[] { "a", "b", "c" };

        public override string Name => "OpenTopoMap";

        public override string CopyrightLink => "Map data: © <a href='https://openstreetmap.org/copyright'>OpenStreetMap</a> contributors, <a href='http://viewfinderpanoramas.org'>SRTM</a> | map style: © <a href='https://opentopomap.org'>OpenTopoMap</a> (<a href='https://creativecommons.org/licenses/by-sa/3.0/'>CC-BY-SA</a>)";

        public OpenTopoMapServer()
        {
            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl", "OpenTopoMap");
        }

        protected override Uri GetTileUri(int x, int y, int z)
        {
            string server = _TileServers[_Random.Next(_TileServers.Length)];
            return new Uri($"https://{server}.tile.opentopomap.org/{z}/{x}/{y}.png");
        }
    }

    public class StamenTerrainTileServer : WebTileServer
    {        
        private Random _Random = new Random();
        private string[] _TileServers = new[] { "a", "b", "c", "d" };

        public override string Name => "Stamen Terrain";

        public override string CopyrightLink => "<a href='http://maps.stamen.com/'>Map tiles</a> by <a href='http://stamen.com'>Stamen Design</a>, under <a href='http://creativecommons.org/licenses/by/3.0'>CC BY 3.0</a>. Data © <a href='http://www.openstreetmap.org/copyright'>OpenStreetMap contributors</a>.";

        public StamenTerrainTileServer()
        {
            CacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MapControl", "StamenTerrain");
        }

        protected override Uri GetTileUri(int x, int y, int z)
        {
            string server = _TileServers[_Random.Next(_TileServers.Length)];
            return new Uri($"http://{server}.tile.stamen.com/terrain/{z}/{x}/{y}.png");
        }
    }
}