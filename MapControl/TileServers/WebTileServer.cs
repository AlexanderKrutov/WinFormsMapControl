using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public abstract class WebTileServer : ITileServer
    {
        private Thread _Worker = null;

        private ConcurrentBag<CachedImage> _DowloadPool = new ConcurrentBag<CachedImage>();

        private EventWaitHandle _WorkerWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        private bool _IsDisposed = false;

        private int _ZoomLevel;

        internal string CacheFolder { get; set; }

        public abstract Uri GetTileUri(int x, int y, int z);

        public abstract string Name { get; }

        public abstract string AttributionText { get; }

        public virtual int MinZoomLevel => 0;

        public virtual int MaxZoomLevel => 19;

        public event Action InvalidateRequired;

        public WebTileServer()
        {
            ServicePointManager.ServerCertificateValidationCallback = new Net.Security.RemoteCertificateValidationCallback(AcceptAllCertificates);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public Image GetTile(int x, int y, int z)
        {
            _ZoomLevel = z;

            string localPath = Path.Combine(CacheFolder, $"{z}", $"{x}", $"{y}.png");

            if (File.Exists(localPath) && new FileInfo(localPath).Length > 0)
            {
                return Image.FromFile(localPath);                
            }
            else
            {
                // request to download image
                DownloadImage(x, y, z);

                // return empty image because it's not downloaded yet
                return null;
            }
        }

        private void DownloadImage(int x, int y, int z)
        {
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
                catch (Exception ex)
                {
                    Trace.TraceError($"Unable to download tile image. Reason: {ex.Message}");
                }
            };
        }

        private bool AcceptAllCertificates(object sender, Security.Cryptography.X509Certificates.X509Certificate certification, Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Dispose()
        {
            _IsDisposed = true;
            _WorkerWaitHandle.Set();
        }
    }
}
