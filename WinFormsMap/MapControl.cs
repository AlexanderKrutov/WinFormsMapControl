using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    [DesignerCategory("code")]
    public partial class MapControl : Control
    {
        /// <summary>
        /// Tile size, in pixels
        /// </summary>
        private const int TILE_SIZE = 256;

        /// <summary>
        /// First tile offset
        /// </summary>
        private Point _Offset = new Point();

        /// <summary>
        /// Map zoom level backing field
        /// </summary>
        private int _ZoomLevel = 0;

        /// <summary>
        /// Map zoom level
        /// </summary>
        public int ZoomLevel
        {
            get => _ZoomLevel;
            set
            {
                if (value < 0 || value > MaxZoomLevel)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(ZoomLevel)} property.");

                _ZoomLevel = value;
                TileServer?.SetZoomLevel(value);
                Invalidate();
            }
        }

        /// <summary>
        /// Maximal zoom level backing field
        /// </summary>
        private int _MaxZoomLevel = 18;

        /// <summary>
        /// Maximal zoom level
        /// </summary>
        public int MaxZoomLevel
        {
            get => _MaxZoomLevel;
            set
            {
                if (value < 0 || value > 18)
                    throw new ArgumentException($"{value} is an incorrect value for {nameof(MaxZoomLevel)} property.");

                _MaxZoomLevel = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets size of map in tiles
        /// </summary>
        public int FullMapSizeInTiles => 1 << ZoomLevel;

        /// <summary>
        /// Gets maps size in pixels
        /// </summary>
        public long FullMapSizeInPixels => FullMapSizeInTiles * TILE_SIZE;

        private bool _MouseCaptured = false;

        private Point _LastMouse = new Point();
      
        private ConcurrentBag<CachedImage> _Cache = new ConcurrentBag<CachedImage>();

        private ITileServer _TileServer;

        public ITileServer TileServer
        {
            get => _TileServer;
            set
            {
                if (_TileServer != null)
                {
                    _TileServer.InvalidateRequired -= Invalidate;
                }

                _TileServer = value;
                _LinkLabel.Links.Clear();
                _LinkLabel.Visible = false;

                if (value != null)
                {
                    _TileServer.InvalidateRequired += Invalidate;
                    ClearCache();
                    _TileServer.SetZoomLevel(ZoomLevel);

                    if (_TileServer.CopyrightLink != null)
                    {
                        ExtractCopyrightLinks(_TileServer.CopyrightLink);
                        _LinkLabel.Visible = true;
                        OnSizeChanged(new EventArgs());
                    }
                }
            }
        }

        private LinkLabel _LinkLabel;

        public double CenterLon
        {
            get
            {
                float x = ArrangeTileNumber(-(_Offset.X - Width / 2) / TILE_SIZE);
                float y = -(_Offset.Y - Height / 2) / TILE_SIZE;
                return TileToWorldPos(x, y).X;
            }
            set
            {
                var center = WorldToTilePos(value, CenterLat);
                _Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                _Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                Invalidate();
            }
        }

        private void ExtractCopyrightLinks(string copyright)
        {
            StringBuilder copyrightText = new StringBuilder();
            int lastIndex = 0;

            MatchCollection m1 = Regex.Matches(copyright, @"(<a.*?>.*?</a>)", RegexOptions.Singleline);

            foreach (Match m2 in m1)
            {
                LinkLabel.Link link = new LinkLabel.Link();

                copyrightText.Append(copyright.Substring(lastIndex, m2.Index - lastIndex));

                string fullLinkWithTags = m2.Groups[1].Value;

                Match m3 = Regex.Match(fullLinkWithTags, @"href=[\""'](.*?)[\""']", RegexOptions.Singleline);
                if (m3.Success)
                {
                    link.LinkData = m3.Groups[1].Value;
                    link.Start = copyrightText.Length;
                }

                string innerText = Regex.Replace(fullLinkWithTags, @"\s*<.*?>\s*", "", RegexOptions.Singleline);

                copyrightText.Append(innerText);
                link.Length = innerText.Length;
                lastIndex = m2.Index + m2.Length;
                _LinkLabel.Links.Add(link);
            }

            if (lastIndex <= copyright.Length - 1)
            {
                copyrightText.Append(copyright.Substring(lastIndex));
            }

            _LinkLabel.Text = copyrightText.ToString();
        }


        public double CenterLat
        {
            get
            {
                float x = ArrangeTileNumber(-(_Offset.X - Width / 2) / TILE_SIZE);
                float y = -(_Offset.Y - Height / 2) / TILE_SIZE;
                return TileToWorldPos(x, y).Y;
            }
            set
            {
                var center = WorldToTilePos(CenterLon, value);
                _Offset.X = -(int)(center.X * TILE_SIZE) + Width / 2;
                _Offset.Y = -(int)(center.Y * TILE_SIZE) + Height / 2;
                Invalidate();
            }
        }

        public double MouseLat
        {
            get
            {
                float x = ArrangeTileNumber(-(float)(_Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return TileToWorldPos(x, y).Y;
            }
        }

        public double MouseLon
        {
            get
            {
                float x = ArrangeTileNumber(-(float)(_Offset.X - _LastMouse.X) / TILE_SIZE);
                float y = -(float)(_Offset.Y - _LastMouse.Y) / TILE_SIZE;
                return TileToWorldPos(x, y).X;
            }
        }

        public MapControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            Cursor = Cursors.Cross;

            _LinkLabel = new LinkLabel() { Text = "", BackColor = Color.FromArgb(100, Color.White) };
            _LinkLabel.AutoSize = true;
            _LinkLabel.ForeColor = Color.Black;
            
            _LinkLabel.Margin = new Padding(2);
            _LinkLabel.LinkClicked += _LinkLabel_LinkClicked;

            Controls.Add(_LinkLabel);
        }

        private void _LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            _LinkLabel.Left = Width - _LinkLabel.Width;
            _LinkLabel.Top = Height - _LinkLabel.Height;

            base.OnSizeChanged(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _MouseCaptured = true;
                _LastMouse.X = e.X;
                _LastMouse.Y = e.Y;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _MouseCaptured = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_MouseCaptured)
            {
                _Offset.X += (e.X - _LastMouse.X);
                _Offset.Y += (e.Y - _LastMouse.Y);

                Invalidate();
            }

            _LastMouse.X = e.X;
            _LastMouse.Y = e.Y;

            base.OnMouseMove(e);
        }

        private float ArrangeTileNumber(float n)
        {
            int size = FullMapSizeInTiles;
            return (n %= size) >= 0 ? n : (n + size);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (!DesignMode)
            {
                // indices of first visible tile
                int fromX = (int)Math.Floor(-(float)_Offset.X / TILE_SIZE);
                int fromY = (int)Math.Floor(-(float)_Offset.Y / TILE_SIZE);

                // count of visible tiles (vertically and horizontally)
                int tilesByWidth = (int)Math.Ceiling((float)Width / TILE_SIZE);
                int tilesByHeight = (int)Math.Ceiling((float)Height / TILE_SIZE);

                int toX = fromX + tilesByWidth;
                int toY = fromY + tilesByHeight;

                foreach (var c in _Cache)
                {
                    c.Used = false;
                }

                for (int x = fromX; x <= toX; x++)
                {
                    for (int y = fromY; y <= toY; y++)
                    {
                        int x_ = (int)ArrangeTileNumber(x);
                        if (y >= 0 && y < FullMapSizeInTiles)
                        {
                            Image tile = TryGetTile(x_, y, ZoomLevel);
                            if (tile != null)
                            {
                                DrawTile(pe.Graphics, x, y, tile);
                            }
                        }
                    }
                }

                // Dispose images that were not used 
                _Cache.Where(c => !c.Used).ToList().ForEach(c => c.Image.Dispose());

                // Update cache, leave only used images
                _Cache = new ConcurrentBag<CachedImage>(_Cache.Where(c => c.Used));
            }

            base.OnPaint(pe);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int z = ZoomLevel;

            if (e.Delta > 0)
                z = ZoomLevel + 1;
            else if (e.Delta < 0)
                z = ZoomLevel - 1;

            if (z < 0) z = 0;
            if (z > MaxZoomLevel) z = MaxZoomLevel;

            if (z != ZoomLevel)
            {
                double factor = Math.Pow(2, z - ZoomLevel);
                _Offset.X = (int)((_Offset.X - e.X) * factor) + e.X;
                _Offset.Y = (int)((_Offset.Y - e.Y) * factor) + e.Y;
                ZoomLevel = z;
                Invalidate();
            }

            base.OnMouseWheel(e);
        }

        private void DrawTile(Graphics g, int x, int y, Image image)
        {
            Point p = new Point();
            p.X = _Offset.X + x * TILE_SIZE;
            p.Y = _Offset.Y + y * TILE_SIZE;
            g.DrawImageUnscaled(image, p);
        }

        private Image TryGetTile(int x, int y, int z)
        {
            try
            {
                CachedImage cached = _Cache.FirstOrDefault(c => c.X == x && c.Y == y && c.Z == z);
                if (cached != null)
                {
                    cached.Used = true;
                    return cached.Image;
                }

                string localPath = Path.Combine(TileServer.CacheFolder, $"{z}", $"{x}", $"{y}.png");

                if (File.Exists(localPath) && new FileInfo(localPath).Length > 0)
                {
                    cached = new CachedImage() { X = x, Y = y, Z = z, Image = Image.FromFile(localPath) };
                    _Cache.Add(cached);
                    return cached.Image;
                }
                else
                {
                    TileServer?.RequestImage(x, y, z);
                   
                    // return empty image because it's not downloaded yet
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets projection of geographical coordinates onto the map
        /// </summary>
        /// <param name="lon">Longitude, in degrees, positive East</param>
        /// <param name="lat">Latitude, in degrees</param>
        /// <returns></returns>
        public PointF GetProjection(double lon, double lat)
        {
            var p = WorldToTilePos(lon, lat);
            return new PointF(p.X * TILE_SIZE + _Offset.X, p.Y * TILE_SIZE + _Offset.Y);
        }

        public PointF WorldToTilePos(double lon, double lat)
        {
            PointF p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << ZoomLevel));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << ZoomLevel));

            return p;
        }

        public PointF TileToWorldPos(double tile_x, double tile_y)
        {
            PointF p = new Point();
            double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, ZoomLevel));

            p.X = (float)((tile_x / Math.Pow(2.0, ZoomLevel) * 360.0) - 180.0);
            p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return p;
        }

        public void ClearCache()
        {
            _Cache = new ConcurrentBag<CachedImage>();
            if (TileServer != null)
            {
                if (Directory.Exists(TileServer.CacheFolder))
                {
                    Directory.Delete(TileServer.CacheFolder, true);
                }
            }
            Invalidate();
        }

        public class CachedImage
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public Image Image { get; set; }
            public bool Used { get; set; }
        }

        
    }
}
